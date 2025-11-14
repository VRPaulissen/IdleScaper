using System.Collections.Generic;
using UnityEngine;

namespace IdleScaper.World
{
/// <summary>
    /// Provides pathfinding on the grid.
    /// </summary>
    public static class GridPathfinder
    {
        private class Node
        {
            public GridPosition Pos;
            public Node Parent;
            public int G;
            public int H;
            public int F => G + H;
        }

        /// <summary>
        /// Finds a path from start to goal. Returns null if no path.
        /// </summary>
        public static List<GridPosition> FindPath(GridPosition start, GridPosition goal)
        {
            var open = new List<Node>();
            var closed = new HashSet<GridPosition>();

            var startNode = new Node { Pos = start, G = 0, H = Heuristic(start, goal) };
            open.Add(startNode);

            while (open.Count > 0)
            {
                // pick node with lowest F
                var current = open[0];
                for (var i = 1; i < open.Count; i++)
                {
                    if (open[i].F < current.F)
                        current = open[i];
                }

                if (current.Pos == goal)
                    return Reconstruct(current);

                open.Remove(current);
                closed.Add(current.Pos);

                foreach (var neighborPos in GridManager.GetNeighbors(current.Pos))
                {
                    if (closed.Contains(neighborPos))
                        continue;

                    if (!GridManager.IsWalkable(neighborPos))
                        continue;

                    var tentativeG = current.G + 1;

                    var existing = open.Find(n => n.Pos == neighborPos);
                    if (existing == null)
                    {
                        existing = new Node
                        {
                            Pos = neighborPos,
                            Parent = current,
                            G = tentativeG,
                            H = Heuristic(neighborPos, goal)
                        };
                        open.Add(existing);
                    }
                    else if (tentativeG < existing.G)
                    {
                        existing.G = tentativeG;
                        existing.Parent = current;
                    }
                }
            }

            return null;
        }

        private static int Heuristic(GridPosition a, GridPosition b)
        {
            // Manhattan distance for 4-way grid.
            return Mathf.Abs(a.X - b.X) + Mathf.Abs(a.Y - b.Y);
        }

        private static List<GridPosition> Reconstruct(Node node)
        {
            var path = new List<GridPosition>();
            var current = node;
            while (current != null)
            {
                path.Add(current.Pos);
                current = current.Parent;
            }
            path.Reverse();
            return path;
        }
    }
}