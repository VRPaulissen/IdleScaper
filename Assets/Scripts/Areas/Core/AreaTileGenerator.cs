using System;
using System.Collections.Generic;
using IdleScaper.Areas.Core;
using IdleScaper.World;
using UnityEngine;

namespace IdleScaper.Areas
{
    /// <summary>
    /// Generates biome-aware tile types for a given area shape.
    /// Emphasizes:
    /// - Continuous water/rock rim
    /// - Vertical depth gradient (clean Y bands)
    /// - Noise-based flowing patches instead of circles
    /// </summary>
    public static class AreaTileGenerator
    {
        public static TileType[,] GenerateTiles(Biome biome, bool[,] shape, int seed)
        {
            var width = shape.GetLength(0);
            var height = shape.GetLength(1);

            var map = new TileType[width, height];
            var rnd = new System.Random(seed);

            // 1. Base fill.
            var baseType = GetBaseTile(biome);
            FillBase(map, shape, baseType);

            // 2. Water/rock rim around boundary.
            ApplyContinuousEdgeRim(map, shape, biome, rnd);

            // 3. Interior decoration uses height + noise to create natural flows.
            switch (biome)
            {
                case Biome.Forest:
                    DecorateForest(map, shape, seed);
                    break;

                case Biome.Mountain:
                    DecorateMountain(map, shape, seed);
                    break;

                case Biome.Lake:
                    DecorateLake(map, shape, seed);
                    break;

                case Biome.Desert:
                    DecorateDesert(map, shape, seed);
                    break;

                case Biome.Cave:
                    DecorateCave(map, shape, seed);
                    break;
            }

            return map;
        }

        // --- Base ---

        private static TileType GetBaseTile(Biome biome)
        {
            return biome switch
            {
                Biome.Forest => TileType.Grass,
                Biome.Mountain => TileType.Rock,
                Biome.Lake => TileType.Grass,
                Biome.Desert => TileType.Sand,
                Biome.Cave => TileType.Rock,
                _ => TileType.Grass
            };
        }

        private static void FillBase(TileType[,] map, bool[,] shape, TileType type)
        {
            var w = map.GetLength(0);
            var h = map.GetLength(1);

            for (var x = 0; x < w; x++)
            for (var y = 0; y < h; y++)
            {
                if (shape[x, y])
                    map[x, y] = type;
            }
        }

        // --- Rim generation (mostly same, tuned for nicer segments) ---

        private static void ApplyContinuousEdgeRim(
            TileType[,] map,
            bool[,] shape,
            Biome biome,
            System.Random rnd)
        {
            var w = map.GetLength(0);
            var h = map.GetLength(1);

            var edgeCells = new List<(int x, int y, float angle)>();

            // Center of mass.
            float sumX = 0, sumY = 0;
            var count = 0;

            for (var x = 0; x < w; x++)
            for (var y = 0; y < h; y++)
            {
                if (!shape[x, y]) continue;
                sumX += x;
                sumY += y;
                count++;
            }

            if (count == 0)
                return;

            var cx = sumX / count;
            var cy = sumY / count;

            // Collect boundary cells.
            for (var x = 0; x < w; x++)
            for (var y = 0; y < h; y++)
            {
                if (shape[x, y] && IsBoundary(shape, x, y))
                {
                    var angle = Mathf.Atan2(y - cy, x - cx);
                    edgeCells.Add((x, y, angle));
                }
            }

            if (edgeCells.Count == 0)
                return;

            edgeCells.Sort((a, b) => a.angle.CompareTo(b.angle));

            GetRimParams(biome,
                out var waterBias,
                out var rockBias,
                out var minSegment,
                out var maxSegment,
                out var switchChance);

            var total = waterBias + rockBias;
            if (total <= 0f)
            {
                waterBias = rockBias = 0.5f;
                total = 1f;
            }

            waterBias /= total;
            rockBias /= total;

            var currentType = (rnd.NextDouble() < waterBias)
                ? TileType.Water
                : TileType.Rock;

            var segmentLen = 0;

            for (var i = 0; i < edgeCells.Count; i++)
            {
                var (x, y, _) = edgeCells[i];
                map[x, y] = currentType;
                segmentLen++;

                var mustSwitch = segmentLen >= maxSegment;
                var canSwitch = segmentLen >= minSegment;

                if (mustSwitch || (canSwitch && rnd.NextDouble() < switchChance))
                {
                    var roll = rnd.NextDouble();
                    var next = (roll < waterBias) ? TileType.Water : TileType.Rock;
                    if (next == currentType)
                        next = currentType == TileType.Water ? TileType.Rock : TileType.Water;

                    currentType = next;
                    segmentLen = 0;
                }
            }
        }

        private static void GetRimParams(
            Biome biome,
            out float waterBias,
            out float rockBias,
            out int minSegment,
            out int maxSegment,
            out float switchChance)
        {
            // Defaults: mixed.
            waterBias = 0.5f;
            rockBias = 0.5f;
            minSegment = 3;
            maxSegment = 8;
            switchChance = 0.22f;

            switch (biome)
            {
                case Biome.Forest:
                    // Rooty rock & creek feel
                    waterBias = 0.28f;
                    rockBias = 0.72f;
                    minSegment = 4;
                    maxSegment = 8;
                    switchChance = 0.18f;
                    break;

                case Biome.Mountain:
                    // Almost all rock ring
                    waterBias = 0.05f;
                    rockBias = 0.95f;
                    minSegment = 5;
                    maxSegment = 10;
                    switchChance = 0.14f;
                    break;

                case Biome.Lake:
                    // Strong watery shore, some rocky spots
                    waterBias = 0.88f;
                    rockBias = 0.12f;
                    minSegment = 5;
                    maxSegment = 11;
                    switchChance = 0.18f;
                    break;

                case Biome.Desert:
                    // Rocky edges, rare water pockets
                    waterBias = 0.06f;
                    rockBias = 0.94f;
                    minSegment = 4;
                    maxSegment = 9;
                    switchChance = 0.2f;
                    break;

                case Biome.Cave:
                    // Damp stone ring
                    waterBias = 0.18f;
                    rockBias = 0.82f;
                    minSegment = 3;
                    maxSegment = 7;
                    switchChance = 0.16f;
                    break;
            }
        }

        private static bool IsBoundary(bool[,] shape, int x, int y)
        {
            var w = shape.GetLength(0);
            var h = shape.GetLength(1);

            if (!shape[x, y])
                return false;

            if (x == 0 || !shape[x - 1, y]) return true;
            if (x == w - 1 || !shape[x + 1, y]) return true;
            if (y == 0 || !shape[x, y - 1]) return true;
            if (y == h - 1 || !shape[x, y + 1]) return true;

            return false;
        }

        private static bool IsRim(TileType[,] map, bool[,] shape, int x, int y)
        {
            // Rim cells are boundary cells already painted as Water/Rock by ApplyContinuousEdgeRim.
            if (!shape[x, y]) return false;
            var t = map[x, y];
            return t == TileType.Water || t == TileType.Rock;
        }

        // --- Shared helpers for "depth" & noise ---

        private static float Depth01(int y, int height, float exponent = 1.2f)
        {
            if (height <= 1) return 0.5f;
            var t = (float)y / (height - 1); // 0 = bottom, 1 = top
            // Adjust curve to control perceived depth gradient.
            return Mathf.Pow(t, exponent);
        }

        private static float Noise(int x, int y, int seed, float scale)
        {
            return Mathf.PerlinNoise(
                (x + seed * 17) * scale,
                (y + seed * 31) * scale);
        }

        // --- Biome-specific flows ---

        private static void DecorateForest(TileType[,] map, bool[,] shape, int seed)
        {
            var w = map.GetLength(0);
            var h = map.GetLength(1);

            // Low frequency = big blobs.
            const float macroScale = 0.05f;
            // Higher frequency = edge breakup inside those blobs.
            const float microScale = 0.17f;

            for (var x = 0; x < w; x++)
            for (var y = 0; y < h; y++)
            {
                if (!shape[x, y]) continue;
                if (IsRim(map, shape, x, y)) continue;

                var depth = Depth01(y, h, 1.05f); // 0 = bottom, 1 = top
                var macro = Noise(x, y, seed + 100, macroScale); // 0..1 big regions
                var micro = Noise(x, y, seed + 211, microScale); // 0..1 detail

                TileType tile;

                // --- Region selection ---
                // Use macro as a hard region splitter.
                // 0.00 - 0.28 : wet / muddy zone
                // 0.28 - 0.62 : dirt / clearing zone
                // 0.62 - 1.00 : tall grass / lush zone

                if (depth < 0.2f || macro < 0.28f)
                {
                    if (micro < 0.22f)
                    {
                        tile = TileType.Water; // small ponds/streams
                    }
                    else if (micro < 0.65f)
                    {
                        tile = TileType.Dirt; // muddy ground
                    }
                    else
                    {
                        tile = TileType.Grass; // lush near-water grass
                    }
                }
                else if (macro < 0.62f)
                {
                    if (micro < 0.18f)
                    {
                        tile = TileType.Grass; // patches of grass in clearings
                    }
                    else if (micro < 0.9f)
                    {
                        tile = TileType.Dirt; // dominant: visible paths & open spots
                    }
                    else
                    {
                        tile = TileType.HighGrass; // odd bushy tuft
                    }
                }
                else
                {
                    if (micro < 0.25f)
                    {
                        tile = TileType.Grass; // softer transitions
                    }
                    else
                    {
                        tile = TileType.HighGrass; // dense vegetation
                    }
                }

                map[x, y] = tile;
            }

            // Smooth it so it becomes groups instead of TV static.
            SmoothPatches(map, shape, 2);
        }

        private static void DecorateMountain(TileType[,] map, bool[,] shape, int seed)
        {
            var w = map.GetLength(0);
            var h = map.GetLength(1);

            for (var x = 0; x < w; x++)
            for (var y = 0; y < h; y++)
            {
                if (!shape[x, y]) continue;
                if (IsRim(map, shape, x, y)) continue;

                var depth = Depth01(y, h, 1.0f);
                var n = Noise(x, y, seed, 0.16f);

                // Higher rows → more rock / cliffs.
                if (depth > 0.7f || (n > 0.65f && depth > 0.5f))
                {
                    map[x, y] = TileType.Rock;
                }
                // Mid slopes: mix of dirt & rock.
                else if (depth > 0.4f && n < 0.55f)
                {
                    map[x, y] = TileType.Dirt;
                }
            }
        }

        private static void DecorateLake(TileType[,] map, bool[,] shape, int seed)
        {
            var w = map.GetLength(0);
            var h = map.GetLength(1);

            // Large noisy central water mass with dirt/grass shores.
            var cx = w / 2f;
            var cy = h / 2f;

            var maxR = Mathf.Min(w, h) * 0.45f;
            var minR = maxR * 0.35f;

            for (var x = 0; x < w; x++)
            for (var y = 0; y < h; y++)
            {
                if (!shape[x, y]) continue;
                if (IsRim(map, shape, x, y)) continue;

                var dx = x - cx;
                var dy = y - cy;
                var dist = Mathf.Sqrt(dx * dx + dy * dy);
                var n = Noise(x, y, seed, 0.18f);

                if (dist + n * 2f < minR)
                {
                    map[x, y] = TileType.Water;
                }
                else if (dist + n * 2f < maxR)
                {
                    map[x, y] = TileType.Dirt;
                }
                else
                {
                    map[x, y] = TileType.Grass;
                }
            }
        }

        private static void DecorateDesert(TileType[,] map, bool[,] shape, int seed)
        {
            var w = map.GetLength(0);
            var h = map.GetLength(1);

            for (var x = 0; x < w; x++)
            for (var y = 0; y < h; y++)
            {
                if (!shape[x, y]) continue;
                if (IsRim(map, shape, x, y)) continue;

                var depth = Depth01(y, h, 0.9f);
                var n = Noise(x, y, seed, 0.11f);

                // Rocky ridges/plateaus.
                if (n > 0.75f || depth > 0.8f)
                {
                    map[x, y] = TileType.Rock;
                }
                // Occasional oasis "flows" in lower/deeper band.
                else if (depth < 0.35f && n < 0.16f)
                {
                    map[x, y] = TileType.Water;
                }
                else
                {
                    map[x, y] = TileType.Sand;
                }
            }
        }

        private static void DecorateCave(TileType[,] map, bool[,] shape, int seed)
        {
            var w = map.GetLength(0);
            var h = map.GetLength(1);

            for (var x = 0; x < w; x++)
            for (var y = 0; y < h; y++)
            {
                if (!shape[x, y]) continue;
                if (IsRim(map, shape, x, y)) continue;

                var depth = Depth01(y, h, 1.0f);
                var n = Noise(x, y, seed, 0.17f);

                // Dark mud patches in low/deep zones.
                if (depth < 0.3f && n < 0.35f)
                {
                    map[x, y] = TileType.Mud;
                }
                // Rocky veins / pillars.
                else if (n > 0.7f || depth > 0.85f)
                {
                    map[x, y] = TileType.Rock;
                }
            }
        }

        private static void SmoothPatches(TileType[,] map, bool[,] shape, int iterations)
        {
            var w = map.GetLength(0);
            var h = map.GetLength(1);

            var smoothable = new HashSet<TileType>
            {
                TileType.Grass,
                TileType.HighGrass,
                TileType.Dirt,
                TileType.Water
            };

            for (var it = 0; it < iterations; it++)
            {
                var copy = (TileType[,])map.Clone();

                for (var x = 0; x < w; x++)
                for (var y = 0; y < h; y++)
                {
                    if (!shape[x, y]) continue;
                    if (IsRim(copy, shape, x, y)) continue;

                    var current = copy[x, y];
                    if (!smoothable.Contains(current)) continue;

                    int grass = 0, highGrass = 0, dirt = 0, water = 0;

                    for (int nx = x - 1; nx <= x + 1; nx++)
                    for (int ny = y - 1; ny <= y + 1; ny++)
                    {
                        if (nx == x && ny == y) continue;
                        if (nx < 0 || ny < 0 || nx >= w || ny >= h) continue;
                        if (!shape[nx, ny]) continue;
                        if (IsRim(copy, shape, nx, ny)) continue;

                        switch (copy[nx, ny])
                        {
                            case TileType.Grass:      grass++; break;
                            case TileType.HighGrass:  highGrass++; break;
                            case TileType.Dirt:       dirt++; break;
                            case TileType.Water:      water++; break;
                        }
                    }

                    TileType best = current;
                    var bestCount = 0;

                    void Consider(TileType t, int c)
                    {
                        if (c > bestCount)
                        {
                            bestCount = c;
                            best = t;
                        }
                    }

                    Consider(TileType.Grass, grass);
                    Consider(TileType.HighGrass, highGrass);
                    Consider(TileType.Dirt, dirt);
                    Consider(TileType.Water, water);

                    map[x, y] = best;
                }
            }
        }

    }
}