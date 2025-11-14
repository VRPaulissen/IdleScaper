using System.Collections.Generic;
using IdleScaper.World;
using UnityEngine;

namespace IdleScaper.Areas.Core
{
    /// <summary>
    /// Generates connected, organic area shapes on a small grid.
    /// </summary>
    public static class AreaShapeGenerator
    {
        /// <summary>
        /// Generates a connected boolean mask for an area.
        /// true = tile exists; false = empty.
        /// Tuned for small maps (~50x50).
        /// </summary>
        public static bool[,] GenerateShape(int width, int height, int targetTiles, int seed)
        {
            width = Mathf.Clamp(width, 5, 50);
            height = Mathf.Clamp(height, 5, 50);
            targetTiles = Mathf.Clamp(targetTiles, 5, width * height);

            var rnd = new System.Random(seed);
            var map = new bool[width, height];

            // Start near center (slight random offset for variation).
            var startX = width / 2 + rnd.Next(-2, 3);
            var startY = height / 2 + rnd.Next(-2, 3);
            startX = Mathf.Clamp(startX, 1, width - 2);
            startY = Mathf.Clamp(startY, 1, height - 2);

            map[startX, startY] = true;
            var placed = 1;

            var frontier = new List<Vector2Int>();
            var inFrontier = new bool[width, height];

            AddNeighborsToFrontier(startX, startY);

            // --- Region growing with "pick best of N" heuristic for blobby shapes ---
            const int samplesPerStep = 4;

            while (placed < targetTiles && frontier.Count > 0)
            {
                // Sample a few frontier cells, pick the one with highest score.
                Vector2Int best = frontier[0];
                float bestScore = float.NegativeInfinity;

                for (int s = 0; s < samplesPerStep; s++)
                {
                    if (frontier.Count == 0) break;
                    var idx = rnd.Next(frontier.Count);
                    var candidate = frontier[idx];

                    var score = ScoreCandidate(map, candidate.x, candidate.y, width, height, seed);

                    if (score > bestScore)
                    {
                        bestScore = score;
                        best = candidate;
                    }
                }

                // Remove chosen from frontier.
                frontier.Remove(best);
                inFrontier[best.x, best.y] = false;

                if (map[best.x, best.y])
                    continue;

                // Must stay connected.
                var neighborCount = CountFilledNeighbors4(map, best.x, best.y);
                if (neighborCount == 0)
                    continue;

                map[best.x, best.y] = true;
                placed++;

                AddNeighborsToFrontier(best.x, best.y);
            }

            // Smooth noisy pixels / thin tendrils.
            Smooth(map, passes: 2);

            // Ensure single connected blob (remove tiny islands if any).
            EnsureSingleComponent(map);

            return map;

            // ---- local helpers ----

            void AddNeighborsToFrontier(int x, int y)
            {
                TryAdd(x + 1, y);
                TryAdd(x - 1, y);
                TryAdd(x, y + 1);
                TryAdd(x, y - 1);
            }

            void TryAdd(int x, int y)
            {
                if (x < 0 || y < 0 || x >= width || y >= height)
                    return;
                if (map[x, y]) return;
                if (inFrontier[x, y]) return;

                inFrontier[x, y] = true;
                frontier.Add(new Vector2Int(x, y));
            }
        }

        /// <summary>
        /// Picks an entrance tile along the bottom edge of the shape, or first found fallback.
        /// </summary>
        public static GridPosition FindEntrance(bool[,] map)
        {
            var width = map.GetLength(0);
            var height = map.GetLength(1);
            const int y = 0;

            for (var x = 0; x < width; x++)
            {
                if (map[x, y])
                    return new GridPosition(x, y);
            }

            for (var yy = 0; yy < height; yy++)
            for (var xx = 0; xx < width; xx++)
            {
                if (map[xx, yy])
                    return new GridPosition(xx, yy);
            }

            return new GridPosition(0, 0);
        }

        private static float ScoreCandidate(bool[,] map, int x, int y, int w, int h, int seed)
        {
            // More neighbors = chunkier = better.
            int n4 = CountFilledNeighbors4(map, x, y);
            int n8 = CountFilledNeighbors8(map, x, y);

            // Center bias: prefer tiles not too close to borders.
            float nx = (x + 0.5f) / w;
            float ny = (y + 0.5f) / h;
            float borderDist = Mathf.Min(nx, 1f - nx) + Mathf.Min(ny, 1f - ny); // 0..1-ish

            // Noise adds irregularity.
            float noise = Mathf.PerlinNoise(
                (x + seed * 0.37f) * 0.21f,
                (y + seed * 0.73f) * 0.21f);

            // Tune weights to taste.
            return
                n4 * 3f +
                n8 * 0.5f +
                borderDist * 1.5f +
                (noise - 0.5f) * 1.0f;
        }

        private static int CountFilledNeighbors4(bool[,] map, int x, int y)
        {
            var w = map.GetLength(0);
            var h = map.GetLength(1);
            var c = 0;

            if (x > 0 && map[x - 1, y]) c++;
            if (x < w - 1 && map[x + 1, y]) c++;
            if (y > 0 && map[x, y - 1]) c++;
            if (y < h - 1 && map[x, y + 1]) c++;
            return c;
        }

        private static int CountFilledNeighbors8(bool[,] map, int x, int y)
        {
            var w = map.GetLength(0);
            var h = map.GetLength(1);
            var c = 0;

            for (int ix = -1; ix <= 1; ix++)
            for (int iy = -1; iy <= 1; iy++)
            {
                if (ix == 0 && iy == 0) continue;
                int xx = x + ix;
                int yy = y + iy;
                if (xx < 0 || yy < 0 || xx >= w || yy >= h) continue;
                if (map[xx, yy]) c++;
            }

            return c;
        }

        private static void Smooth(bool[,] map, int passes)
        {
            var w = map.GetLength(0);
            var h = map.GetLength(1);

            for (int p = 0; p < passes; p++)
            {
                var copy = (bool[,])map.Clone();

                for (int x = 0; x < w; x++)
                for (int y = 0; y < h; y++)
                {
                    int n = CountFilledNeighbors8(copy, x, y);

                    if (copy[x, y])
                    {
                        // Kill very isolated pixels.
                        if (n <= 1)
                            map[x, y] = false;
                    }
                    else
                    {
                        // Fill small gaps inside dense areas.
                        if (n >= 6)
                            map[x, y] = true;
                    }
                }
            }
        }

        private static void EnsureSingleComponent(bool[,] map)
        {
            var w = map.GetLength(0);
            var h = map.GetLength(1);

            // Find first filled as seed.
            var found = false;
            var seed = Vector2Int.zero;

            for (int x = 0; x < w && !found; x++)
            for (int y = 0; y < h && !found; y++)
            {
                if (map[x, y])
                {
                    seed = new Vector2Int(x, y);
                    found = true;
                }
            }

            if (!found) return;

            var visited = new bool[w, h];
            var q = new Queue<Vector2Int>();
            q.Enqueue(seed);
            visited[seed.x, seed.y] = true;

            while (q.Count > 0)
            {
                var c = q.Dequeue();
                var dirs = new[]
                {
                    new Vector2Int(1,0),
                    new Vector2Int(-1,0),
                    new Vector2Int(0,1),
                    new Vector2Int(0,-1),
                };

                foreach (var d in dirs)
                {
                    int nx = c.x + d.x;
                    int ny = c.y + d.y;
                    if (nx < 0 || ny < 0 || nx >= w || ny >= h)
                        continue;
                    if (!map[nx, ny] || visited[nx, ny])
                        continue;

                    visited[nx, ny] = true;
                    q.Enqueue(new Vector2Int(nx, ny));
                }
            }

            // Clear all unvisited (small islands).
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
            {
                if (map[x, y] && !visited[x, y])
                    map[x, y] = false;
            }
        }
    }
}
