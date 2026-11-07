using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Abissal.EditorTools
{
    /// <summary>
    /// Define o formato do oceano como um campo contínuo: valores positivos são água
    /// e negativos são rocha. A arte e a colisão são geradas a partir deste mesmo campo.
    /// </summary>
    public static class LevelMapGenerator
    {
        public const float MapLeft = -66f;
        public const float MapRight = 66f;
        public const float MapBottom = -208f;
        public const float MapTop = 2f;

        private const int CellLeft = -63;
        private const int CellRight = 63;
        private const int CellBottom = -208;
        private const float DropCenterX = 6f;
        private const float SearchStep = 0.1f;
        private const float ContourStep = 0.25f;
        private const float ContourTolerance = 0.05f;

        /// <summary>Salão oval. Valores: centro X, centro Y, raio X, raio Y.</summary>
        private static readonly Vector4[] Halls =
        {
            new Vector4(-8f, -42f, 34f, 16f),    // Bacia do recife
            new Vector4(32f, -38f, 20f, 12f),    // Bacia leste
            new Vector4(-49f, -30f, 7f, 5f),     // Gruta lateral do recife
            new Vector4(43f, -51f, 12f, 8f),     // Floresta de algas
            new Vector4(0f, -92f, 38f, 17f),     // Grande salão do crepúsculo
            new Vector4(51f, -90f, 7f, 6f),      // Caverna de duas entradas
            new Vector4(-20f, -122f, 30f, 13f),  // Salão inferior do crepúsculo
            new Vector4(10f, -158f, 36f, 13f),   // Salão abissal
            new Vector4(-48f, -150f, 8f, 6f),    // Grota bioluminescente
            new Vector4(48f, -180f, 8f, 6f),     // Desvio profundo
            new Vector4(0f, -192f, 18f, 8f)      // Câmara da amostra principal
        };

        /// <summary>Túnel reto. Valores: início, fim e raio.</summary>
        private static readonly (Vector2 from, Vector2 to, float radius)[] Tunnels =
        {
            (new Vector2(6f, -12f), new Vector2(6f, -32f), 5f),          // Descida principal
            (new Vector2(-38f, -12f), new Vector2(-32f, -33f), 3f),      // Segunda descida
            (new Vector2(14f, -40f), new Vector2(20f, -38f), 5f),        // Ligação entre as bacias
            (new Vector2(-38f, -38f), new Vector2(-46f, -31f), 2.5f),    // Entrada da gruta lateral
            (new Vector2(36f, -44f), new Vector2(40f, -48f), 4f),        // Entrada das algas
            (new Vector2(-6f, -55f), new Vector2(0f, -76f), 5f),         // Da zona iluminada ao crepúsculo
            (new Vector2(40f, -54f), new Vector2(30f, -80f), 3f),        // Rota alternativa leste
            (new Vector2(30f, -84f), new Vector2(46f, -87f), 2.5f),      // Caverna, entrada de cima
            (new Vector2(33f, -97f), new Vector2(44f, -103f), 2.5f),     // Caverna, entrada de baixo (início)
            (new Vector2(44f, -103f), new Vector2(52f, -96f), 2.5f),     // Caverna, entrada de baixo (fim)
            (new Vector2(-14f, -105f), new Vector2(-20f, -112f), 6f),    // Descida ao salão inferior
            (new Vector2(-20f, -132f), new Vector2(-8f, -148f), 5f),     // Do crepúsculo ao abismo
            (new Vector2(-40f, -130f), new Vector2(-24f, -156f), 3f),    // Rota alternativa oeste
            (new Vector2(-22f, -156f), new Vector2(-41f, -151f), 2.5f),  // Entrada da grota
            (new Vector2(38f, -163f), new Vector2(46f, -175f), 3f),      // Entrada do desvio profundo
            (new Vector2(6f, -168f), new Vector2(0f, -184f), 5f)         // Descida à câmara final
        };

        // A fenda fica separada porque, vista de fora, ela precisa parecer rocha maciça.
        private static readonly Vector4 HiddenHall = new Vector4(34f, -120f, 8f, 5f);
        private static readonly (Vector2 from, Vector2 to, float radius) HiddenTunnel =
            (new Vector2(8f, -122f), new Vector2(27f, -120f), 2.2f);

        /// <summary>Lugares importantes, usados na validação e no posicionamento das amostras.</summary>
        public static readonly (string name, Vector2Int cell)[] PointsOfInterest =
        {
            ("Gruta lateral", new Vector2Int(-49, -30)),
            ("Floresta de algas", new Vector2Int(44, -52)),
            ("Caverna de duas entradas", new Vector2Int(51, -90)),
            ("Fenda escondida", new Vector2Int(34, -120)),
            ("Grota bioluminescente", new Vector2Int(-48, -150)),
            ("Desvio profundo", new Vector2Int(48, -180)),
            ("Amostra principal", new Vector2Int(0, -196))
        };

        /// <summary>Positivo é água e negativo é rocha. O valor cresce com a distância até a parede.</summary>
        public static float Field(float x, float y, bool includeHidden = true)
        {
            float wobble = Mathf.Sin(x * 0.37f + y * 0.21f) * 0.6f
                + Mathf.Sin(x * 0.13f - y * 0.29f) * 0.6f
                + Mathf.Sin(x * 0.91f - y * 0.7f) * 0.28f
                + Mathf.Sin(x * 1.7f + y * 1.3f) * 0.13f;

            float best = y - GroundHeight(x);

            foreach (Vector4 hall in Halls)
            {
                best = Mathf.Max(best, HallValue(hall, x, y, wobble));
            }

            foreach ((Vector2 from, Vector2 to, float radius) in Tunnels)
            {
                best = Mathf.Max(best, TunnelValue(from, to, radius, x, y, wobble));
            }

            if (includeHidden)
            {
                best = Mathf.Max(best, HallValue(HiddenHall, x, y, wobble));
                best = Mathf.Max(best, TunnelValue(HiddenTunnel.from, HiddenTunnel.to, HiddenTunnel.radius, x, y, wobble));
            }

            return best;
        }

        public static bool IsWater(int x, int y)
        {
            return Field(x + 0.5f, y + 0.5f) > 0f;
        }

        /// <summary>Procura o chão abaixo do ponto. Retorna null se não houver dentro da busca.</summary>
        public static float? FindFloor(float x, float fromY, float maxSearch)
        {
            return FindSurface(x, fromY, maxSearch, -SearchStep);
        }

        /// <summary>Procura o teto acima do ponto. Retorna null se não houver dentro da busca.</summary>
        public static float? FindCeiling(float x, float fromY, float maxSearch)
        {
            return FindSurface(x, fromY, maxSearch, SearchStep);
        }

        /// <summary>Amostra o campo em uma grade, usada pela arte e pela colisão.</summary>
        public static float[,] SampleField(float step, bool includeHidden)
        {
            int columns = Mathf.CeilToInt((MapRight - MapLeft) / step) + 2;
            int rows = Mathf.CeilToInt((MapTop - MapBottom) / step) + 2;
            var grid = new float[columns, rows];

            for (int column = 0; column < columns; column++)
            {
                for (int row = 0; row < rows; row++)
                {
                    grid[column, row] = Field(MapLeft + column * step, MapBottom + row * step, includeHidden);
                }
            }

            return grid;
        }

        /// <summary>Traça os contornos entre água e rocha, simplificados, para virarem colisores.</summary>
        public static List<Vector2[]> BuildContours()
        {
            float[,] grid = SampleField(ContourStep, true);
            int columns = grid.GetLength(0);
            int rows = grid.GetLength(1);
            var links = new Dictionary<long, List<long>>();
            var points = new Dictionary<long, Vector2>();

            for (int column = 0; column < columns - 1; column++)
            {
                for (int row = 0; row < rows - 1; row++)
                {
                    AddCellSegments(grid, column, row, columns, links, points);
                }
            }

            var contours = new List<Vector2[]>();
            var visited = new HashSet<long>();

            foreach (long start in links.Keys)
            {
                if (visited.Contains(start))
                {
                    continue;
                }

                List<Vector2> chain = WalkContour(start, links, points, visited);

                if (chain.Count >= 4)
                {
                    contours.Add(Simplify(chain, ContourTolerance).ToArray());
                }
            }

            return contours;
        }

        /// <summary>Confere se cada ponto é alcançável e mede o caminho até ele.</summary>
        /// <param name="blockedCells">Células ocupadas por perigos fixos, tratadas como intransponíveis.</param>
        public static string Validate(HashSet<Vector2Int> blockedCells = null)
        {
            var start = new Vector2Int(5, -9);
            Dictionary<Vector2Int, float> distances = MeasureDistances(start, blockedCells ?? new HashSet<Vector2Int>());
            var report = new StringBuilder();
            report.AppendLine($"Celulas alcancaveis: {distances.Count}");

            foreach ((string name, Vector2Int cell) in PointsOfInterest)
            {
                string status = distances.TryGetValue(cell, out float distance)
                    ? $"OK, {distance:F0} tiles ({distance / 6f:F0}s a 6 u/s)"
                    : "INALCANCAVEL";
                report.AppendLine($"{name} {cell}: {status}");
            }

            return report.ToString();
        }

        private static float? FindSurface(float x, float fromY, float maxSearch, float step)
        {
            if (Field(x, fromY) <= 0f)
            {
                return null;
            }

            int steps = Mathf.CeilToInt(maxSearch / Mathf.Abs(step));

            for (int i = 1; i <= steps; i++)
            {
                float y = fromY + step * i;

                if (Field(x, y) <= 0f)
                {
                    return y;
                }
            }

            return null;
        }

        private static float GroundHeight(float x)
        {
            float reef = -12.6f
                + Mathf.Sin(x * 0.35f) * 0.9f
                + Mathf.Sin(x * 0.13f + 1f) * 0.8f
                + Mathf.Sin(x * 0.83f + 2f) * 0.3f;

            float distance = Mathf.Abs(x - DropCenterX);
            return Mathf.Lerp(-23f, reef, Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(3f, 7f, distance)));
        }

        private static float HallValue(Vector4 hall, float x, float y, float wobble)
        {
            float offsetX = (x - hall.x) / hall.z;
            float offsetY = (y - hall.y) / hall.w;
            float smallestRadius = Mathf.Min(hall.z, hall.w);
            float distance = Mathf.Sqrt(offsetX * offsetX + offsetY * offsetY);
            return wobble * EdgeRoughness(smallestRadius) - (distance - 1f) * smallestRadius;
        }

        private static float TunnelValue(Vector2 from, Vector2 to, float radius, float x, float y, float wobble)
        {
            var point = new Vector2(x, y);
            Vector2 segment = to - from;
            float along = Mathf.Clamp01(Vector2.Dot(point - from, segment) / segment.sqrMagnitude);
            float distance = Vector2.Distance(point, from + segment * along);
            return wobble * EdgeRoughness(radius) - (distance - radius);
        }

        /// <summary>Lugares estreitos recebem menos irregularidade, para não fechar a passagem.</summary>
        private static float EdgeRoughness(float radius)
        {
            return Mathf.Clamp01(radius * 0.14f);
        }

        /// <summary>Marching squares: cada quadrado da grade liga os lados por onde o contorno passa.</summary>
        private static void AddCellSegments(float[,] grid, int column, int row, int columns, Dictionary<long, List<long>> links, Dictionary<long, Vector2> points)
        {
            float a = grid[column, row];
            float b = grid[column + 1, row];
            float c = grid[column + 1, row + 1];
            float d = grid[column, row + 1];

            int index = (a <= 0f ? 1 : 0) | (b <= 0f ? 2 : 0) | (c <= 0f ? 4 : 0) | (d <= 0f ? 8 : 0);

            if (index == 0 || index == 15)
            {
                return;
            }

            long bottom = EdgeKey(column, row, 0, columns);
            long left = EdgeKey(column, row, 1, columns);
            long top = EdgeKey(column, row + 1, 0, columns);
            long right = EdgeKey(column + 1, row, 1, columns);

            if ((a <= 0f) != (b <= 0f))
            {
                points[bottom] = EdgePoint(column, row, column + 1, row, a, b);
            }

            if ((a <= 0f) != (d <= 0f))
            {
                points[left] = EdgePoint(column, row, column, row + 1, a, d);
            }

            if ((d <= 0f) != (c <= 0f))
            {
                points[top] = EdgePoint(column, row + 1, column + 1, row + 1, d, c);
            }

            if ((b <= 0f) != (c <= 0f))
            {
                points[right] = EdgePoint(column + 1, row, column + 1, row + 1, b, c);
            }

            switch (index)
            {
                case 1:
                case 14:
                    Link(links, left, bottom);
                    break;
                case 2:
                case 13:
                    Link(links, bottom, right);
                    break;
                case 3:
                case 12:
                    Link(links, left, right);
                    break;
                case 4:
                case 11:
                    Link(links, right, top);
                    break;
                case 6:
                case 9:
                    Link(links, bottom, top);
                    break;
                case 7:
                case 8:
                    Link(links, left, top);
                    break;
                case 5:
                    Link(links, left, top);
                    Link(links, bottom, right);
                    break;
                case 10:
                    Link(links, left, bottom);
                    Link(links, right, top);
                    break;
            }
        }

        private static long EdgeKey(int column, int row, int orientation, int columns)
        {
            return ((long)row * (columns + 1) + column) * 2 + orientation;
        }

        private static Vector2 EdgePoint(int columnA, int rowA, int columnB, int rowB, float valueA, float valueB)
        {
            float t = Mathf.Clamp01(valueA / (valueA - valueB));
            float column = Mathf.Lerp(columnA, columnB, t);
            float row = Mathf.Lerp(rowA, rowB, t);
            return new Vector2(MapLeft + column * ContourStep, MapBottom + row * ContourStep);
        }

        private static void Link(Dictionary<long, List<long>> links, long first, long second)
        {
            AddLink(links, first, second);
            AddLink(links, second, first);
        }

        private static void AddLink(Dictionary<long, List<long>> links, long from, long to)
        {
            if (!links.TryGetValue(from, out List<long> targets))
            {
                targets = new List<long>(2);
                links[from] = targets;
            }

            targets.Add(to);
        }

        private static List<Vector2> WalkContour(long start, Dictionary<long, List<long>> links, Dictionary<long, Vector2> points, HashSet<long> visited)
        {
            var chain = new List<Vector2>();
            long current = start;
            long previous = long.MinValue;

            while (true)
            {
                visited.Add(current);
                chain.Add(points[current]);

                long next = long.MinValue;

                foreach (long candidate in links[current])
                {
                    if (candidate != previous && !visited.Contains(candidate))
                    {
                        next = candidate;
                        break;
                    }
                }

                if (next == long.MinValue)
                {
                    break;
                }

                previous = current;
                current = next;
            }

            // Fecha o contorno quando o último ponto volta a encostar no primeiro.
            if (links[current].Contains(start))
            {
                chain.Add(chain[0]);
            }

            return chain;
        }

        /// <summary>Remove pontos quase alinhados (algoritmo de Douglas-Peucker).</summary>
        private static List<Vector2> Simplify(List<Vector2> points, float tolerance)
        {
            var keep = new bool[points.Count];
            keep[0] = true;
            keep[points.Count - 1] = true;

            var stack = new Stack<(int first, int last)>();
            int middle = points.Count / 2;
            keep[middle] = true;
            stack.Push((0, middle));
            stack.Push((middle, points.Count - 1));

            while (stack.Count > 0)
            {
                (int first, int last) = stack.Pop();
                int farthest = -1;
                float farthestDistance = tolerance;

                for (int i = first + 1; i < last; i++)
                {
                    float distance = DistanceToSegment(points[i], points[first], points[last]);

                    if (distance > farthestDistance)
                    {
                        farthest = i;
                        farthestDistance = distance;
                    }
                }

                if (farthest >= 0)
                {
                    keep[farthest] = true;
                    stack.Push((first, farthest));
                    stack.Push((farthest, last));
                }
            }

            var result = new List<Vector2>();

            for (int i = 0; i < points.Count; i++)
            {
                if (keep[i])
                {
                    result.Add(points[i]);
                }
            }

            return result;
        }

        private static float DistanceToSegment(Vector2 point, Vector2 from, Vector2 to)
        {
            Vector2 segment = to - from;
            float lengthSquared = segment.sqrMagnitude;

            if (lengthSquared < 0.00001f)
            {
                return Vector2.Distance(point, from);
            }

            float along = Mathf.Clamp01(Vector2.Dot(point - from, segment) / lengthSquared);
            return Vector2.Distance(point, from + segment * along);
        }

        /// <summary>
        /// Busca de caminho que só passa onde cabe o mergulhador (bloco de 2x2 livre).
        /// Usa uma fila simples: uma célula é reavaliada se aparecer um caminho menor.
        /// </summary>
        private static Dictionary<Vector2Int, float> MeasureDistances(Vector2Int start, HashSet<Vector2Int> blockedCells)
        {
            var distances = new Dictionary<Vector2Int, float> { [start] = 0f };
            var queue = new Queue<Vector2Int>();
            queue.Enqueue(start);

            while (queue.Count > 0)
            {
                Vector2Int cell = queue.Dequeue();
                float current = distances[cell];

                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        var next = new Vector2Int(cell.x + dx, cell.y + dy);

                        if ((dx == 0 && dy == 0) || blockedCells.Contains(next) || !FitsDiver(next))
                        {
                            continue;
                        }

                        float total = current + (dx != 0 && dy != 0 ? 1.414f : 1f);

                        if (!distances.TryGetValue(next, out float known) || total < known - 0.001f)
                        {
                            distances[next] = total;
                            queue.Enqueue(next);
                        }
                    }
                }
            }

            return distances;
        }

        private static bool FitsDiver(Vector2Int cell)
        {
            bool inside = cell.x >= CellLeft && cell.x < CellRight && cell.y >= CellBottom && cell.y < -1;
            return inside
                && IsWater(cell.x, cell.y) && IsWater(cell.x + 1, cell.y)
                && IsWater(cell.x, cell.y + 1) && IsWater(cell.x + 1, cell.y + 1);
        }
    }
}
