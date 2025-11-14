using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI Line Renderer with:
/// - Miter joins (no gaps)
/// - Optional polygon fill
/// - For use on a Canvas
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class UILineRenderer : Graphic
{
    [Header("Shape")]
    [SerializeField] private List<Vector2> points = new();
    [SerializeField] private bool loop = true;

    [Header("Outline")]
    [SerializeField] private float thickness = 8f;
    [SerializeField, Tooltip("Clamp how long the corner spikes can get on very sharp angles.")]
    private float miterLimit = 2f;

    [Header("Fill")]
    [SerializeField] private bool fill = true;
    [SerializeField] private Color fillColor = new Color(1, 1, 1, 0.1f);

    public List<Vector2> Points
    {
        get => points;
        set { points = value; SetVerticesDirty(); }
    }

    public bool Loop
    {
        get => loop;
        set { loop = value; SetVerticesDirty(); }
    }

    public float Thickness
    {
        get => thickness;
        set { thickness = Mathf.Max(0f, value); SetVerticesDirty(); }
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        SetVerticesDirty();
    }
#endif

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        if (points == null || points.Count < 2)
            return;

        if (fill)
            GenerateFill(vh);

        GenerateOutline(vh);
    }

    // --------------------------------------------------------------
    // 1) FILL (triangulate polygon)
    // --------------------------------------------------------------
    private void GenerateFill(VertexHelper vh)
    {
        if (!loop || points.Count < 3)
            return;

        var tris = Triangulate(points);
        if (tris == null || tris.Count < 3)
            return;

        int startIndex = vh.currentVertCount;

        // Add vertices for polygon
        for (int i = 0; i < points.Count; i++)
        {
            var v = UIVertex.simpleVert;
            v.color = fillColor;
            v.position = points[i];
            vh.AddVert(v);
        }

        // Add triangles
        for (int i = 0; i < tris.Count; i += 3)
        {
            vh.AddTriangle(
                startIndex + tris[i],
                startIndex + tris[i + 1],
                startIndex + tris[i + 2]
            );
        }
    }

    // --------------------------------------------------------------
    // 2) OUTLINE (smooth miter-joined polyline)
    // --------------------------------------------------------------
    private void GenerateOutline(VertexHelper vh)
    {
        int count = points.Count;
        if (count < 2 || thickness <= 0f)
            return;

        float half = thickness * 0.5f;
        bool closed = loop;

        // Two vertices per point (left/right edge of strip)
        var strip = new Vector2[count * 2];

        for (int i = 0; i < count; i++)
        {
            Vector2 curr = points[i];

            int prevIndex = i - 1;
            if (prevIndex < 0) prevIndex = closed ? count - 1 : 0;

            int nextIndex = i + 1;
            if (nextIndex >= count) nextIndex = closed ? 0 : count - 1;

            Vector2 prev = points[prevIndex];
            Vector2 next = points[nextIndex];

            Vector2 dirPrev = (curr - prev);
            if (dirPrev.sqrMagnitude < 1e-4f)
                dirPrev = (next - curr);
            dirPrev.Normalize();

            Vector2 dirNext = (next - curr);
            if (dirNext.sqrMagnitude < 1e-4f)
                dirNext = dirPrev;
            dirNext.Normalize();

            Vector2 nPrev = new Vector2(-dirPrev.y, dirPrev.x);
            Vector2 nNext = new Vector2(-dirNext.y, dirNext.x);

            Vector2 miter = nPrev + nNext;
            if (miter.sqrMagnitude < 1e-4f)
                miter = nNext;
            miter.Normalize();

            float dot = Mathf.Max(Vector2.Dot(miter, nPrev), 0.0001f);
            float miterLength = half / dot;

            float maxMiter = half * miterLimit;
            if (miterLength > maxMiter)
                miterLength = maxMiter;

            Vector2 offset = miter * miterLength;

            strip[i * 2]     = curr + offset;
            strip[i * 2 + 1] = curr - offset;
        }

        int baseVert = vh.currentVertCount;

        // Add outline vertices
        for (int i = 0; i < strip.Length; i++)
        {
            var v = UIVertex.simpleVert;
            v.color = color;
            v.position = strip[i];
            vh.AddVert(v);
        }

        // Add triangles along the strip
        int segmentCount = closed ? count : count - 1;
        for (int i = 0; i < segmentCount; i++)
        {
            int i0 = baseVert + (i * 2);
            int i1 = baseVert + (i * 2 + 1);
            int i2 = baseVert + (((i + 1) % count) * 2);
            int i3 = baseVert + (((i + 1) % count) * 2 + 1);

            vh.AddTriangle(i0, i2, i3);
            vh.AddTriangle(i3, i1, i0);
        }
    }

    // --------------------------------------------------------------
    // 3) Ear-clipping polygon triangulation
    //    (works for convex & simple concave polygons)
    // --------------------------------------------------------------

    private static List<int> Triangulate(IList<Vector2> pts)
    {
        int n = pts.Count;
        if (n < 3)
            return null;

        var indices = new List<int>();

        // Build a list of vertex indices with correct winding
        int[] V = new int[n];
        if (PolygonArea(pts) > 0)
        {
            for (int v = 0; v < n; v++)
                V[v] = v;
        }
        else
        {
            // Reverse for clockwise polygons
            for (int v = 0; v < n; v++)
                V[v] = (n - 1) - v;
        }

        int nv = n;
        int count = 2 * nv;    // Safety counter

        for (int m = 0, v = nv - 1; nv > 2;)
        {
            if ((count--) <= 0)
                break; // likely not simple polygon

            int u = v;
            if (u >= nv) u = 0;
            v = u + 1;
            if (v >= nv) v = 0;
            int w = v + 1;
            if (w >= nv) w = 0;

            if (Snip(pts, u, v, w, nv, V))
            {
                int a = V[u];
                int b = V[v];
                int c = V[w];

                indices.Add(a);
                indices.Add(b);
                indices.Add(c);

                // Remove v from the list
                for (int s = v, t = v + 1; t < nv; s++, t++)
                    V[s] = V[t];

                nv--;
                count = 2 * nv;
            }
        }

        return indices;
    }

    private static float PolygonArea(IList<Vector2> pts)
    {
        int n = pts.Count;
        float A = 0f;
        for (int p = n - 1, q = 0; q < n; p = q++)
        {
            Vector2 pVal = pts[p];
            Vector2 qVal = pts[q];
            A += pVal.x * qVal.y - qVal.x * pVal.y;
        }
        return A * 0.5f;
    }

    private static bool Snip(IList<Vector2> pts, int u, int v, int w, int nv, int[] V)
    {
        const float EPSILON = 1e-6f;

        Vector2 A = pts[V[u]];
        Vector2 B = pts[V[v]];
        Vector2 C = pts[V[w]];

        // Is triangle ABC convex?
        if (((B.x - A.x) * (C.y - A.y) - (B.y - A.y) * (C.x - A.x)) <= EPSILON)
            return false;

        // Check if any other point lies inside this triangle
        for (int p = 0; p < nv; p++)
        {
            if (p == u || p == v || p == w) continue;

            Vector2 P = pts[V[p]];
            if (PointInTriangle(P, A, B, C))
                return false;
        }

        return true;
    }

    private static bool PointInTriangle(Vector2 P, Vector2 A, Vector2 B, Vector2 C)
    {
        // Barycentric technique
        float v0x = C.x - A.x;
        float v0y = C.y - A.y;
        float v1x = B.x - A.x;
        float v1y = B.y - A.y;
        float v2x = P.x - A.x;
        float v2y = P.y - A.y;

        float dot00 = v0x * v0x + v0y * v0y;
        float dot01 = v0x * v1x + v0y * v1y;
        float dot02 = v0x * v2x + v0y * v2y;
        float dot11 = v1x * v1x + v1y * v1y;
        float dot12 = v1x * v2x + v1y * v2y;

        float invDenom = 1f / (dot00 * dot11 - dot01 * dot01);
        float u = (dot11 * dot02 - dot01 * dot12) * invDenom;
        float v = (dot00 * dot12 - dot01 * dot02) * invDenom;

        return (u >= 0) && (v >= 0) && (u + v < 1);
    }
}
