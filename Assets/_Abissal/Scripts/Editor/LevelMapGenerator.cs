using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Abissal.EditorTools
{
    /// <summary>
    /// Desenha o oceano no Tilemap: começa com rocha sólida e "escava" salões e túneis.
    /// ATENÇÃO: gerar de novo apaga pinturas feitas à mão no Tilemap "Rochas".
    /// </summary>
    public static class LevelMapGenerator
    {
        private const string TilesPath = "Assets/_Abissal/Art/Tiles/";
        private const string SpritesPath = "Assets/_Abissal/Art/Sprites/Terrain/";

        private const int Left = -63;
        private const int Right = 63;
        private const int Bottom = -208;
        private const int Top = 0;
        private const float DropCenterX = 6f;

        // Profundidades (em tiles) em que a rocha muda de zona: 300 m e 700 m.
        private const int MiddleZoneY = -60;
        private const int DeepZoneY = -140;

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
            new Vector4(34f, -120f, 8f, 5f),     // Fenda escondida
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
            (new Vector2(8f, -122f), new Vector2(27f, -120f), 2.2f),     // Passagem da fenda escondida
            (new Vector2(-20f, -132f), new Vector2(-8f, -148f), 5f),     // Do crepúsculo ao abismo
            (new Vector2(-40f, -130f), new Vector2(-24f, -156f), 3f),    // Rota alternativa oeste
            (new Vector2(-22f, -156f), new Vector2(-41f, -151f), 2.5f),  // Entrada da grota
            (new Vector2(38f, -163f), new Vector2(46f, -175f), 3f),      // Entrada do desvio profundo
            (new Vector2(6f, -168f), new Vector2(0f, -184f), 5f)         // Descida à câmara final
        };

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

        public static void Generate(Tilemap tilemap)
        {
            TileBase[][] tiles =
            {
                new TileBase[] { GetTile("rock_top"), GetTile("rock_fill_0"), GetTile("rock_fill_1") },
                new TileBase[] { GetTile("rock_mid_top"), GetTile("rock_mid_fill_0"), GetTile("rock_mid_fill_1") },
                new TileBase[] { GetTile("rock_deep_top"), GetTile("rock_deep_fill_0"), GetTile("rock_deep_fill_1") }
            };

            tilemap.ClearAllTiles();

            for (int x = Left; x <= Right; x++)
            {
                for (int y = Bottom; y <= Top; y++)
                {
                    if (IsWater(x, y))
                    {
                        continue;
                    }

                    TileBase[] zone = tiles[ZoneAt(x, y)];
                    int variant = IsWater(x, y + 1) ? 0 : (x * 7 + y * 13) % 3 == 0 ? 2 : 1;
                    tilemap.SetTile(new Vector3Int(x, y, 0), zone[variant]);
                }
            }
        }

        /// <summary>Recria os colisores, porque o colisor composto não acompanha mudanças feitas por código.</summary>
        public static void RebuildColliders(GameObject rocks)
        {
            Object.DestroyImmediate(rocks.GetComponent<CompositeCollider2D>());
            Object.DestroyImmediate(rocks.GetComponent<TilemapCollider2D>());
            rocks.GetComponent<Tilemap>().RefreshAllTiles();

            var tileCollider = rocks.AddComponent<TilemapCollider2D>();
            tileCollider.ProcessTilemapChanges();

            var composite = rocks.AddComponent<CompositeCollider2D>();
            rocks.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
            composite.geometryType = CompositeCollider2D.GeometryType.Outlines;
            tileCollider.usedByComposite = true;
            tileCollider.ProcessTilemapChanges();
            composite.GenerateGeometry();
        }

        /// <summary>Cobre a fenda escondida com rocha falsa, sem colisão, para ela parecer parede.</summary>
        public static void PaintHiddenCover(Tilemap cover, RectInt area)
        {
            TileBase rock = GetTile("rock_mid_fill_0");
            cover.ClearAllTiles();

            for (int x = area.xMin; x < area.xMax; x++)
            {
                for (int y = area.yMin; y < area.yMax; y++)
                {
                    if (IsWater(x, y))
                    {
                        cover.SetTile(new Vector3Int(x, y, 0), rock);
                    }
                }
            }
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

        private static float GroundHeight(int x)
        {
            float reef = -12.6f + Mathf.Sin(x * 0.35f) * 0.9f + Mathf.Sin(x * 0.13f + 1f) * 0.8f;
            float distance = Mathf.Abs(x - DropCenterX);
            return Mathf.Lerp(-23f, reef, Mathf.Clamp01((distance - 3f) / 3f));
        }

        private static int ZoneAt(int x, int y)
        {
            // Pequena variação para a troca de rocha não formar uma linha reta.
            int jitter = Mathf.RoundToInt(Mathf.Sin(x * 0.4f) * 2f + Mathf.Sin(x * 1.3f + y) * 2f);

            if (y + jitter > MiddleZoneY)
            {
                return 0;
            }

            return y + jitter > DeepZoneY ? 1 : 2;
        }

        public static bool IsWater(int x, int y)
        {
            if (y > Mathf.RoundToInt(GroundHeight(x)))
            {
                return true;
            }

            var point = new Vector2(x + 0.5f, y + 0.5f);
            float wobble = Mathf.Sin(x * 0.37f + y * 0.21f) * 0.6f + Mathf.Sin(x * 0.13f - y * 0.29f) * 0.6f;

            foreach (Vector4 hall in Halls)
            {
                var offset = new Vector2((point.x - hall.x) / hall.z, (point.y - hall.y) / hall.w);
                float smallestRadius = Mathf.Min(hall.z, hall.w);

                if ((offset.magnitude - 1f) * smallestRadius < wobble * EdgeRoughness(smallestRadius))
                {
                    return true;
                }
            }

            foreach ((Vector2 from, Vector2 to, float radius) in Tunnels)
            {
                Vector2 segment = to - from;
                float along = Mathf.Clamp01(Vector2.Dot(point - from, segment) / segment.sqrMagnitude);
                float distance = Vector2.Distance(point, from + segment * along);

                if (distance - radius < wobble * EdgeRoughness(radius))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>Lugares estreitos recebem menos irregularidade, para não fechar a passagem.</summary>
        private static float EdgeRoughness(float radius)
        {
            return Mathf.Clamp01(radius * 0.14f);
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
            bool inside = cell.x >= Left && cell.x < Right && cell.y >= Bottom && cell.y < -1;
            return inside
                && IsWater(cell.x, cell.y) && IsWater(cell.x + 1, cell.y)
                && IsWater(cell.x, cell.y + 1) && IsWater(cell.x + 1, cell.y + 1);
        }

        private static Tile GetTile(string spriteName)
        {
            string path = TilesPath + spriteName + ".asset";
            var tile = AssetDatabase.LoadAssetAtPath<Tile>(path);

            if (tile == null)
            {
                tile = ScriptableObject.CreateInstance<Tile>();
                AssetDatabase.CreateAsset(tile, path);
            }

            tile.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(SpritesPath + spriteName + ".png");

            // A colisão usa a célula inteira, então não depende do desenho do sprite.
            tile.colliderType = Tile.ColliderType.Grid;
            EditorUtility.SetDirty(tile);
            return tile;
        }
    }
}
