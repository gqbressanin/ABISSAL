using System.Collections.Generic;
using System.IO;
using Abissal.World;
using UnityEditor;
using UnityEngine;

namespace Abissal.EditorTools
{
    /// <summary>
    /// Pinta as rochas pixel a pixel a partir do campo do mapa e monta a colisão.
    /// Use o menu ABISSAL > Reconstruir terreno. Isso substitui o objeto "Terreno" da cena.
    /// </summary>
    public static class TerrainBuilder
    {
        private const string MapFolder = "Assets/_Abissal/Art/Sprites/Terrain/Map/";
        private const string LitMaterialPath = "Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Lit-Default.mat";
        private const string TerrainLayer = "Terrain";

        private const int PixelsPerUnit = 16;
        private const int FieldStep = 4;
        private const int ChunkSize = 1024;
        private const int NoiseSize = 512;
        private const int CoverReach = 44;

        // Limites das zonas em metros de mundo: 300 m e 700 m de profundidade.
        private const float MiddleZoneY = -60f;
        private const float DeepZoneY = -140f;

        private static readonly int[,] Bayer =
        {
            { 0, 8, 2, 10 },
            { 12, 4, 14, 6 },
            { 3, 11, 1, 9 },
            { 15, 7, 13, 5 }
        };

        private sealed class Palette
        {
            public Color32[] Ramp;
            public Color32 Moss;
            public Color32 MossLight;

            public Palette(string[] ramp, string moss, string mossLight)
            {
                Ramp = new Color32[ramp.Length];

                for (int i = 0; i < ramp.Length; i++)
                {
                    Ramp[i] = PixelCanvas.Hex(ramp[i]);
                }

                Moss = PixelCanvas.Hex(moss);
                MossLight = PixelCanvas.Hex(mossLight);
            }
        }

        // Do interior mais escuro até a borda iluminada.
        private static readonly Palette[] Palettes =
        {
            new Palette(new[] { "16222f", "1e3042", "2a4258", "3b5a72", "5a8090" }, "3f8f76", "74cf97"),
            new Palette(new[] { "1a1630", "262046", "39335f", "4f4780", "7268a8" }, "4f68b0", "8aaaf0"),
            new Palette(new[] { "15112a", "221c40", "332b58", "473d78", "6a5aa0" }, "6e42b8", "d195ff")
        };

        private static int width;
        private static int height;
        private static float[] noise;

        [MenuItem("ABISSAL/Reconstruir terreno")]
        public static void Rebuild()
        {
            Prepare();

            float[,] openField = LevelMapGenerator.SampleField(FieldStep / (float)PixelsPerUnit, true);
            float[,] closedField = LevelMapGenerator.SampleField(FieldStep / (float)PixelsPerUnit, false);

            bool[] openRock = BuildRockMask(openField);
            bool[] closedRock = BuildRockMask(closedField);
            int[] openDepth = MeasureDepth(openRock);
            int[] closedDepth = MeasureDepth(closedRock);

            var pixels = new Color32[width * height];
            Paint(pixels, openRock, openDepth, openField);
            AddSurfaceDetails(pixels, openRock);

            Directory.CreateDirectory(MapFolder);
            RectInt coverArea = SaveCover(openRock, closedRock, closedDepth, closedField);
            List<string> chunks = SaveChunks(pixels);
            AssetDatabase.Refresh();

            InstallInScene(chunks, coverArea);
        }

        private static void Prepare()
        {
            width = Mathf.RoundToInt((LevelMapGenerator.MapRight - LevelMapGenerator.MapLeft) * PixelsPerUnit);
            height = Mathf.RoundToInt((LevelMapGenerator.MapTop - LevelMapGenerator.MapBottom) * PixelsPerUnit);
            noise = BuildNoise(new System.Random(2024));
        }

        /// <summary>Ruído suave repetível, somando camadas de tamanhos diferentes.</summary>
        private static float[] BuildNoise(System.Random random)
        {
            var result = new float[NoiseSize * NoiseSize];

            for (int cell = 8; cell <= 64; cell *= 2)
            {
                int lattice = NoiseSize / cell;
                var values = new float[lattice * lattice];

                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = (float)random.NextDouble();
                }

                for (int y = 0; y < NoiseSize; y++)
                {
                    for (int x = 0; x < NoiseSize; x++)
                    {
                        float gx = x / (float)cell;
                        float gy = y / (float)cell;
                        int x0 = (int)gx;
                        int y0 = (int)gy;
                        float tx = Mathf.SmoothStep(0f, 1f, gx - x0);
                        float ty = Mathf.SmoothStep(0f, 1f, gy - y0);

                        float bottom = Mathf.Lerp(values[y0 % lattice * lattice + x0 % lattice], values[y0 % lattice * lattice + (x0 + 1) % lattice], tx);
                        float top = Mathf.Lerp(values[(y0 + 1) % lattice * lattice + x0 % lattice], values[(y0 + 1) % lattice * lattice + (x0 + 1) % lattice], tx);
                        result[y * NoiseSize + x] += Mathf.Lerp(bottom, top, ty) * (cell / 120f);
                    }
                }
            }

            return result;
        }

        private static float SampleNoise(int x, int y)
        {
            return noise[((y % NoiseSize + NoiseSize) % NoiseSize) * NoiseSize + (x % NoiseSize + NoiseSize) % NoiseSize];
        }

        private static float SampleField(float[,] field, int x, int y)
        {
            float gx = x / (float)FieldStep;
            float gy = y / (float)FieldStep;
            int x0 = (int)gx;
            int y0 = (int)gy;
            float tx = gx - x0;
            float ty = gy - y0;

            float bottom = Mathf.Lerp(field[x0, y0], field[x0 + 1, y0], tx);
            float top = Mathf.Lerp(field[x0, y0 + 1], field[x0 + 1, y0 + 1], tx);
            return Mathf.Lerp(bottom, top, ty);
        }

        private static bool[] BuildRockMask(float[,] field)
        {
            var mask = new bool[width * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Pequeno ruído para a borda parecer pedra lascada, e não uma curva perfeita.
                    float roughness = (SampleNoise(x * 3, y * 3) - 0.5f) * 0.22f;
                    mask[y * width + x] = SampleField(field, x, y) + roughness <= 0f;
                }
            }

            return mask;
        }

        /// <summary>Calcula a distância de cada pixel de rocha até a água mais próxima.</summary>
        private static int[] MeasureDepth(bool[] rock)
        {
            const int straight = 3;
            const int diagonal = 4;
            const int far = 60000;
            var depth = new int[rock.Length];

            for (int i = 0; i < depth.Length; i++)
            {
                depth[i] = rock[i] ? far : 0;
            }

            for (int y = 1; y < height; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    int i = y * width + x;
                    int best = Mathf.Min(depth[i], depth[i - 1] + straight);
                    best = Mathf.Min(best, depth[i - width] + straight);
                    best = Mathf.Min(best, depth[i - width - 1] + diagonal);
                    depth[i] = Mathf.Min(best, depth[i - width + 1] + diagonal);
                }
            }

            for (int y = height - 2; y >= 0; y--)
            {
                for (int x = width - 2; x >= 1; x--)
                {
                    int i = y * width + x;
                    int best = Mathf.Min(depth[i], depth[i + 1] + straight);
                    best = Mathf.Min(best, depth[i + width] + straight);
                    best = Mathf.Min(best, depth[i + width + 1] + diagonal);
                    depth[i] = Mathf.Min(best, depth[i + width - 1] + diagonal);
                }
            }

            return depth;
        }

        private static void Paint(Color32[] pixels, bool[] rock, int[] depth, float[,] field)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int i = y * width + x;

                    if (rock[i])
                    {
                        pixels[i] = ShadeRock(x, y, depth[i] / 3f, field);
                    }
                }
            }
        }

        private static Color32 ShadeRock(int x, int y, float edgeDistance, float[,] field)
        {
            Palette palette = Palettes[ZoneAt(x, y)];
            float wander = (SampleNoise(x, y) - 0.5f) * 11f;
            float depth = edgeDistance + wander;
            float dither = (Bayer[y & 3, x & 3] + 0.5f) / 16f - 0.5f;

            float upward = 0f;

            if (edgeDistance < 12f)
            {
                upward = Mathf.Clamp(
                    (SampleField(field, x, Mathf.Min(y + 8, height - 1)) - SampleField(field, x, Mathf.Max(y - 8, 0))) * 1.2f,
                    -1f,
                    1f);

                float mossReach = 2f + SampleNoise(x * 2, y) * 7f;

                if (upward > 0.25f && edgeDistance <= mossReach)
                {
                    return edgeDistance <= 1.5f ? palette.MossLight : palette.Moss;
                }

                if (upward < -0.25f && edgeDistance <= 2f)
                {
                    return palette.Ramp[0];
                }
            }

            float level = depth < 2.5f ? 4f : depth < 7f ? 3f : depth < 17f ? 2f : depth < 40f ? 1f : 0f;
            level += dither * 0.9f;

            // Veios finos e camadas de sedimento, alongados na horizontal.
            float layers = SampleNoise(x, y * 4);
            bool isSeam = Mathf.Repeat(layers * 9f, 1f) < 0.07f;
            bool isCrack = Mathf.Abs(SampleNoise(x * 2 + 170, y * 2 + 90) - 0.5f) < 0.006f;

            if (isCrack)
            {
                level -= 2f;
            }
            else if (isSeam)
            {
                level -= 1f;
            }

            return palette.Ramp[Mathf.Clamp(Mathf.RoundToInt(level), 0, palette.Ramp.Length - 1)];
        }

        private static int ZoneAt(int x, int y)
        {
            float worldY = LevelMapGenerator.MapBottom + y / (float)PixelsPerUnit;
            float jitter = (SampleNoise(x, y) - 0.5f) * 14f + ((Bayer[y & 3, x & 3] + 0.5f) / 16f - 0.5f) * 1.5f;
            float position = worldY + jitter;

            if (position > MiddleZoneY)
            {
                return 0;
            }

            return position > DeepZoneY ? 1 : 2;
        }

        /// <summary>Tufos sobre as saliências e estalactites nos tetos. Não têm colisão.</summary>
        private static void AddSurfaceDetails(Color32[] pixels, bool[] rock)
        {
            for (int x = 1; x < width - 1; x++)
            {
                for (int y = 1; y < height - 12; y++)
                {
                    int i = y * width + x;
                    int hash = Hash(x, y);

                    if (rock[i] && !rock[i + width] && !rock[i + width * 2])
                    {
                        AddTuft(pixels, rock, x, y, hash);
                    }
                    else if (!rock[i] && rock[i + width] && hash % 23 == 0)
                    {
                        AddStalactite(pixels, rock, x, y, hash);
                    }
                }
            }
        }

        private static void AddTuft(Color32[] pixels, bool[] rock, int x, int y, int hash)
        {
            if (hash % 5 != 0)
            {
                return;
            }

            Palette palette = Palettes[ZoneAt(x, y)];
            int length = 2 + hash % 5;
            int lean = hash % 3 - 1;

            for (int i = 1; i <= length; i++)
            {
                int px = x + lean * i / 3;
                int index = (y + i) * width + px;

                if (px > 0 && px < width && index < pixels.Length && !rock[index])
                {
                    pixels[index] = i == length ? palette.MossLight : palette.Moss;
                }
            }
        }

        private static void AddStalactite(Color32[] pixels, bool[] rock, int x, int y, int hash)
        {
            Palette palette = Palettes[ZoneAt(x, y)];
            int length = 4 + hash % 12;

            for (int i = 0; i < length; i++)
            {
                int half = (length - i) / 6;

                for (int offset = -half; offset <= half; offset++)
                {
                    int index = (y - i) * width + x + offset;

                    if (y - i > 0 && x + offset > 0 && x + offset < width && !rock[index])
                    {
                        pixels[index] = palette.Ramp[offset == -half ? 3 : offset == half ? 1 : 2];
                    }
                }
            }
        }

        private static int Hash(int x, int y)
        {
            unchecked
            {
                int value = x * 73856093 ^ y * 19349663;
                value ^= value >> 13;
                return (value * 1274126177) & int.MaxValue;
            }
        }

        /// <summary>
        /// A cobertura da fenda repete a rocha como se não houvesse passagem. Ela vai um pouco
        /// além da fenda para encostar em rocha escura, o que esconde a emenda.
        /// </summary>
        private static RectInt SaveCover(bool[] openRock, bool[] closedRock, int[] closedDepth, float[,] closedField)
        {
            var hidden = new bool[width * height];

            for (int i = 0; i < hidden.Length; i++)
            {
                hidden[i] = closedRock[i] && !openRock[i];
            }

            int[] distanceToFissure = MeasureReach(hidden);
            int minX = width;
            int minY = height;
            int maxX = 0;
            int maxY = 0;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int i = y * width + x;

                    if (closedRock[i] && distanceToFissure[i] <= CoverReach * 3)
                    {
                        minX = Mathf.Min(minX, x);
                        minY = Mathf.Min(minY, y);
                        maxX = Mathf.Max(maxX, x);
                        maxY = Mathf.Max(maxY, y);
                    }
                }
            }

            var area = new RectInt(minX, minY, maxX - minX + 1, maxY - minY + 1);
            var cover = new PixelCanvas(area.width, area.height);

            for (int y = 0; y < area.height; y++)
            {
                for (int x = 0; x < area.width; x++)
                {
                    int px = area.x + x;
                    int py = area.y + y;
                    int i = py * width + px;

                    if (closedRock[i] && distanceToFissure[i] <= CoverReach * 3)
                    {
                        cover.Set(x, y, ShadeRock(px, py, closedDepth[i] / 3f, closedField));
                    }
                }
            }

            cover.Save(MapFolder + "cover.png");
            return area;
        }

        private static int[] MeasureReach(bool[] sources)
        {
            var inverted = new bool[sources.Length];

            for (int i = 0; i < sources.Length; i++)
            {
                inverted[i] = !sources[i];
            }

            return MeasureDepth(inverted);
        }

        private static List<string> SaveChunks(Color32[] pixels)
        {
            var saved = new List<string>();

            for (int startY = 0; startY < height; startY += ChunkSize)
            {
                for (int startX = 0; startX < width; startX += ChunkSize)
                {
                    int chunkWidth = Mathf.Min(ChunkSize, width - startX);
                    int chunkHeight = Mathf.Min(ChunkSize, height - startY);
                    var canvas = new PixelCanvas(chunkWidth, chunkHeight);
                    bool hasPixels = false;

                    for (int y = 0; y < chunkHeight; y++)
                    {
                        for (int x = 0; x < chunkWidth; x++)
                        {
                            Color32 color = pixels[(startY + y) * width + startX + x];

                            if (color.a != 0)
                            {
                                canvas.Set(x, y, color);
                                hasPixels = true;
                            }
                        }
                    }

                    if (hasPixels)
                    {
                        string path = MapFolder + $"map_{startX / ChunkSize}_{startY / ChunkSize}.png";
                        canvas.Save(path);
                        saved.Add(path);
                    }
                }
            }

            return saved;
        }

        private static Vector3 PixelToWorld(float x, float y)
        {
            return new Vector3(
                LevelMapGenerator.MapLeft + x / PixelsPerUnit,
                LevelMapGenerator.MapBottom + y / PixelsPerUnit,
                0f);
        }

        private static void InstallInScene(List<string> chunks, RectInt coverArea)
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(LitMaterialPath);
            GameObject previous = GameObject.Find("Terreno");

            if (previous != null)
            {
                Object.DestroyImmediate(previous);
            }

            Transform root = new GameObject("Terreno").transform;

            foreach (string path in chunks)
            {
                string[] parts = Path.GetFileNameWithoutExtension(path).Split('_');
                int column = int.Parse(parts[1]);
                int row = int.Parse(parts[2]);
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);

                Vector3 position = PixelToWorld(
                    column * ChunkSize + sprite.rect.width * 0.5f,
                    row * ChunkSize + sprite.rect.height * 0.5f);

                CreateSprite($"Rocha_{column}_{row}", root, sprite, position, "Scenery", 10, material);
            }

            var coverSprite = AssetDatabase.LoadAssetAtPath<Sprite>(MapFolder + "cover.png");
            Vector3 coverPosition = PixelToWorld(coverArea.x + coverArea.width * 0.5f, coverArea.y + coverArea.height * 0.5f);
            SpriteRenderer cover = CreateSprite("Cobertura_Fenda", root, coverSprite, coverPosition, "Foreground", 5, material);

            var reveal = Object.FindObjectOfType<HiddenAreaReveal>();
            var serialized = new SerializedObject(reveal);
            serialized.FindProperty("coverLayer").objectReferenceValue = cover;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            BuildCollision(root);
        }

        private static SpriteRenderer CreateSprite(string name, Transform parent, Sprite sprite, Vector3 position, string sortingLayer, int order, Material material)
        {
            var spriteObject = new GameObject(name);
            spriteObject.transform.SetParent(parent, false);
            spriteObject.transform.position = position;

            var renderer = spriteObject.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sharedMaterial = material;
            renderer.sortingLayerName = sortingLayer;
            renderer.sortingOrder = order;
            return renderer;
        }

        private static void BuildCollision(Transform root)
        {
            var collision = new GameObject("Colisao") { layer = LayerMask.NameToLayer(TerrainLayer) };
            collision.transform.SetParent(root, false);

            foreach (Vector2[] contour in LevelMapGenerator.BuildContours())
            {
                collision.AddComponent<EdgeCollider2D>().points = contour;
            }
        }
    }
}
