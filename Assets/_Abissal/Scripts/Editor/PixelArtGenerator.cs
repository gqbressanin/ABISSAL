using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Abissal.EditorTools
{
    /// <summary>
    /// Gera a arte-base original do jogo em arquivos PNG, pelo menu ABISSAL > Gerar arte-base.
    /// Os PNGs podem ser substituídos depois por artes melhores, mantendo o mesmo nome.
    /// </summary>
    public static class PixelArtGenerator
    {
        private const string Root = "Assets/_Abissal/Art/Sprites/";

        private static readonly int[,] Bayer =
        {
            { 0, 8, 2, 10 },
            { 12, 4, 14, 6 },
            { 3, 11, 1, 9 },
            { 15, 7, 13, 5 }
        };

        private static readonly Color32 Ink = PixelCanvas.Hex("0d1b2a");

        [MenuItem("ABISSAL/Gerar arte-base")]
        public static void GenerateAll()
        {
            GenerateEnvironment();
            GenerateTerrain();
            GeneratePlants();
            CreatureArt.Generate(Root);
            GenerateHazards();
            GenerateSamples();
            DiverArt.Generate(Root);
            GenerateBoat();
            GenerateEffects();
            GenerateTitle();
            GenerateInterface();
            GenerateGlyphs();
            AssetDatabase.Refresh();
            BuildFontAsset();
        }

        /// <summary>Cada letra vira um PNG branco com contorno, que a interface pode colorir.</summary>
        private static void GenerateGlyphs()
        {
            const int width = PixelFont.GlyphWidth + 2;
            const int height = PixelFont.GlyphHeight + 6;
            const int baseTop = height - 4;
            Color32 white = PixelCanvas.Hex("ffffff");

            foreach (char character in PixelFont.Characters)
            {
                var canvas = new PixelCanvas(width, height);
                int top = baseTop + PixelFont.AccentHeight(character);
                canvas.DrawRows(1, top, PixelFont.GetGlyph(character), c => c == '#' ? white : (Color32?)null);
                canvas.Outline(Ink);
                canvas.Save(Root + $"UI/Glyphs/glyph_{(int)character:X4}.png");
            }
        }

        private static void BuildFontAsset()
        {
            const string fontPath = "Assets/_Abissal/Data/PixelFontData.asset";
            var font = AssetDatabase.LoadAssetAtPath<Abissal.UI.PixelFontData>(fontPath);

            if (font == null)
            {
                font = ScriptableObject.CreateInstance<Abissal.UI.PixelFontData>();
                AssetDatabase.CreateAsset(font, fontPath);
            }

            var characters = new List<char>(PixelFont.Characters);
            var sprites = new Sprite[characters.Count];

            for (int i = 0; i < characters.Count; i++)
            {
                sprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>(Root + $"UI/Glyphs/glyph_{(int)characters[i]:X4}.png");
            }

            font.Configure(new string(characters.ToArray()), sprites, PixelFont.GlyphWidth + 1);
            EditorUtility.SetDirty(font);
            AssetDatabase.SaveAssets();
        }

        private static void GenerateEnvironment()
        {
            DrawGradient(480, 270, new[] { "45cfc9", "22a6be", "187ea6", "0f5a82" }).Save(Root + "Environment/water_gradient.png");
            DrawGradient(64, 64, new[] { "8fd6ea", "b8ecec", "e4f8e4" }).Save(Root + "Environment/sky.png");

            for (int frame = 0; frame < 3; frame++)
            {
                DrawSurface(frame / 3f).Save(Root + $"Environment/surface_{frame}.png");
            }

            DrawRidge(960, 104, 1.3f, "1a7198", "2486a8").Save(Root + "Environment/ridge_far.png");
            DrawRidge(960, 80, 4.1f, "12567c", "1b6a8e").Save(Root + "Environment/ridge_near.png");
            DrawLightRay().Save(Root + "Environment/light_ray.png");
        }

        private static PixelCanvas DrawGradient(int width, int height, string[] hexStops)
        {
            var canvas = new PixelCanvas(width, height);

            for (int y = 0; y < height; y++)
            {
                float position = (1f - y / (float)(height - 1)) * (hexStops.Length - 1);
                int index = Mathf.Min(Mathf.FloorToInt(position), hexStops.Length - 2);
                float blend = position - index;

                for (int x = 0; x < width; x++)
                {
                    float threshold = (Bayer[y % 4, x % 4] + 0.5f) / 16f;
                    canvas.Set(x, y, PixelCanvas.Hex(hexStops[blend > threshold ? index + 1 : index]));
                }
            }

            return canvas;
        }

        private static PixelCanvas DrawSurface(float phase)
        {
            const int width = 64;
            const int height = 8;
            var canvas = new PixelCanvas(width, height);
            Color32 foam = PixelCanvas.Hex("eafff8");
            Color32 water = PixelCanvas.Hex("7fe6da");

            for (int x = 0; x < width; x++)
            {
                float wave = Mathf.Sin((x / (float)width + phase) * Mathf.PI * 4f);
                int top = 4 + Mathf.RoundToInt(wave * 1.4f);
                canvas.FillRect(x, 0, 1, top, water);
                canvas.Set(x, top, foam);
            }

            return canvas;
        }

        private static PixelCanvas DrawRidge(int width, int height, float seed, string bodyHex, string edgeHex)
        {
            var canvas = new PixelCanvas(width, height);
            Color32 body = PixelCanvas.Hex(bodyHex);
            Color32 edge = PixelCanvas.Hex(edgeHex);
            int fadeHeight = height / 3;

            for (int x = 0; x < width; x++)
            {
                float shape = 0.5f
                    + 0.24f * Mathf.Sin(x * 0.012f + seed)
                    + 0.14f * Mathf.Sin(x * 0.031f + seed * 2f)
                    + 0.05f * Mathf.Sin(x * 0.097f + seed * 3f);
                int top = Mathf.Clamp(Mathf.RoundToInt(shape * height), 1, height - 1);

                canvas.FillRect(x, 0, 1, top, body);
                canvas.Set(x, top, edge);

                // A base some aos poucos, para o relevo não terminar em uma linha reta.
                for (int y = 0; y < fadeHeight; y++)
                {
                    if (y / (float)fadeHeight < (Bayer[y % 4, x % 4] + 0.5f) / 16f)
                    {
                        canvas.Set(x, y, default);
                    }
                }
            }

            return canvas;
        }

        private static PixelCanvas DrawLightRay()
        {
            const int width = 56;
            const int height = 176;
            var canvas = new PixelCanvas(width, height);

            for (int y = 0; y < height; y++)
            {
                float depth = 1f - y / (float)height;
                float centerX = 14f + depth * 24f;
                float halfWidth = 6f + depth * 8f;
                float strength = Mathf.Sin(Mathf.Min(depth * 5f, 1f) * Mathf.PI * 0.5f) * (1f - depth);

                for (int x = 0; x < width; x++)
                {
                    float falloff = 1f - Mathf.Abs(x - centerX) / halfWidth;
                    float alpha = falloff * strength;

                    if (alpha > (Bayer[y % 4, x % 4] + 0.5f) / 16f)
                    {
                        canvas.Set(x, y, PixelCanvas.Hex("d8fff2", 46));
                    }
                }
            }

            return canvas;
        }

        private static void GenerateTerrain()
        {
            string[] shallow = { "233347", "2c425a", "1a2839", "3f8f76", "74cf97" };
            string[] middle = { "39335f", "4a4278", "2b2649", "4f68b0", "8aaaf0" };
            string[] deep = { "3d3566", "4e447f", "2c264d", "6e42b8", "d195ff" };

            SaveRockSet("rock", shallow, 11);
            SaveRockSet("rock_mid", middle, 21);
            SaveRockSet("rock_deep", deep, 41);
        }

        private static void SaveRockSet(string prefix, string[] palette, int seed)
        {
            DrawRockTile(seed, palette, false).Save(Root + $"Terrain/{prefix}_fill_0.png");
            DrawRockTile(seed + 1, palette, false).Save(Root + $"Terrain/{prefix}_fill_1.png");
            DrawRockTile(seed + 2, palette, true).Save(Root + $"Terrain/{prefix}_top.png");
        }

        /// <summary>Paleta: base, claro, escuro, cobertura e borda da cobertura.</summary>
        private static PixelCanvas DrawRockTile(int seed, string[] palette, bool hasCover)
        {
            var random = new System.Random(seed);
            var canvas = new PixelCanvas(16, 16);
            canvas.FillRect(0, 0, 16, 16, PixelCanvas.Hex(palette[0]));

            for (int i = 0; i < 26; i++)
            {
                string hex = palette[i % 2 == 0 ? 1 : 2];
                canvas.FillRect(random.Next(16), random.Next(16), 1 + random.Next(3), 1 + random.Next(2), PixelCanvas.Hex(hex));
            }

            if (hasCover)
            {
                for (int x = 0; x < 16; x++)
                {
                    int depth = 2 + random.Next(3);
                    canvas.FillRect(x, 16 - depth, 1, depth, PixelCanvas.Hex(palette[3]));
                    canvas.Set(x, 15, PixelCanvas.Hex(palette[4]));
                }
            }

            return canvas;
        }

        private static void GeneratePlants()
        {
            for (int frame = 0; frame < 4; frame++)
            {
                float phase = frame / 4f * Mathf.PI * 2f;
                DrawKelp(phase).Save(Root + $"Plants/kelp_{frame}.png");
                DrawSeagrass(phase).Save(Root + $"Plants/seagrass_{frame}.png");
            }

            for (int frame = 0; frame < 3; frame++)
            {
                DrawAnemone(frame / 3f * Mathf.PI * 2f).Save(Root + $"Plants/anemone_{frame}.png");
            }

            DrawFanCoral().Save(Root + "Plants/coral_fan.png");
            DrawTubeCoral().Save(Root + "Plants/coral_tube.png");
            DrawGlowMushroom().Save(Root + "Plants/glow_mushroom.png");
        }

        private static PixelCanvas DrawAnemone(float phase)
        {
            var canvas = new PixelCanvas(22, 18);
            canvas.FillRect(6, 0, 10, 5, PixelCanvas.Hex("6b3f8f"));

            for (int tentacle = 0; tentacle < 7; tentacle++)
            {
                float baseX = 5f + tentacle * 2f;
                float lean = (tentacle - 3) * 1.1f;

                for (int y = 0; y < 11; y++)
                {
                    float t = y / 10f;
                    int x = Mathf.RoundToInt(baseX + lean * t + Mathf.Sin(phase + tentacle + t * 3f) * t * 1.6f);
                    canvas.Set(x, 4 + y, PixelCanvas.Hex(y > 8 ? "ffe08a" : "b86bd6"));
                }
            }

            canvas.Outline(PixelCanvas.Hex("3a2150"));
            return canvas;
        }

        private static PixelCanvas DrawGlowMushroom()
        {
            var canvas = new PixelCanvas(16, 16);
            canvas.FillRect(7, 0, 2, 8, PixelCanvas.Hex("a8fff0"));

            for (int y = 0; y < 6; y++)
            {
                int half = 7 - y;
                canvas.FillRect(8 - half, 8 + y, half * 2, 1, PixelCanvas.Hex(y < 2 ? "2fd8b8" : "49f0c8"));
            }

            canvas.FillRect(4, 10, 2, 1, PixelCanvas.Hex("e6fffa"));
            canvas.FillRect(9, 11, 2, 1, PixelCanvas.Hex("e6fffa"));
            return canvas;
        }

        private static PixelCanvas DrawKelp(float phase)
        {
            const int width = 20;
            const int height = 72;
            var canvas = new PixelCanvas(width, height);
            Color32 stem = PixelCanvas.Hex("23875f");
            Color32 leaf = PixelCanvas.Hex("3fb876");
            Color32 tip = PixelCanvas.Hex("7fdc8c");

            for (int y = 0; y < height - 2; y++)
            {
                float growth = y / (float)height;
                int x = Mathf.RoundToInt(10f + Mathf.Sin(y * 0.16f + phase) * (1f + growth * 4f));
                canvas.FillRect(x, y, 2, 1, stem);

                if (y > 4 && y % 7 == 0)
                {
                    int side = y % 14 == 0 ? 1 : -1;
                    int startX = side > 0 ? x + 2 : x - 1;

                    for (int length = 0; length < 4; length++)
                    {
                        canvas.Set(startX + side * length, y + length / 2, length == 3 ? tip : leaf);
                        canvas.Set(startX + side * length, y + length / 2 + 1, leaf);
                    }
                }
            }

            return canvas;
        }

        private static PixelCanvas DrawSeagrass(float phase)
        {
            var canvas = new PixelCanvas(20, 16);
            int[] heights = { 10, 14, 9, 13, 11 };

            for (int blade = 0; blade < heights.Length; blade++)
            {
                for (int y = 0; y < heights[blade]; y++)
                {
                    float bend = Mathf.Sin(phase + blade + y * 0.3f) * (y / 7f);
                    string hex = y > heights[blade] - 3 ? "8fdf88" : "3aa873";
                    canvas.Set(2 + blade * 4 + Mathf.RoundToInt(bend), y, PixelCanvas.Hex(hex));
                }
            }

            return canvas;
        }

        private static PixelCanvas DrawFanCoral()
        {
            var canvas = new PixelCanvas(28, 26);
            var random = new System.Random(7);
            DrawBranch(canvas, random, 14f, 1f, 90f, 7f, 4);
            canvas.Outline(PixelCanvas.Hex("6d1f47"));
            return canvas;
        }

        private static void DrawBranch(PixelCanvas canvas, System.Random random, float x, float y, float angle, float length, int depth)
        {
            float radians = angle * Mathf.Deg2Rad;
            float endX = x + Mathf.Cos(radians) * length;
            float endY = y + Mathf.Sin(radians) * length;
            canvas.DrawLine(x, y, endX, endY, PixelCanvas.Hex(depth > 1 ? "ef6a8f" : "ffb0bd"));

            if (depth == 0)
            {
                return;
            }

            float spread = 22f + (float)random.NextDouble() * 18f;
            DrawBranch(canvas, random, endX, endY, angle - spread, length * 0.78f, depth - 1);
            DrawBranch(canvas, random, endX, endY, angle + spread, length * 0.78f, depth - 1);
        }

        private static PixelCanvas DrawTubeCoral()
        {
            var canvas = new PixelCanvas(24, 20);
            int[,] tubes = { { 3, 9 }, { 8, 15 }, { 13, 12 }, { 18, 7 } };

            for (int i = 0; i < tubes.GetLength(0); i++)
            {
                int x = tubes[i, 0];
                int height = tubes[i, 1];
                canvas.FillRect(x, 0, 4, height, PixelCanvas.Hex("f09a3e"));
                canvas.FillRect(x, 0, 1, height, PixelCanvas.Hex("c86f2c"));
                canvas.FillRect(x, height - 1, 4, 2, PixelCanvas.Hex("ffd27a"));
                canvas.FillRect(x + 1, height, 2, 1, PixelCanvas.Hex("7c3f24"));
            }

            canvas.Outline(PixelCanvas.Hex("54281c"));
            return canvas;
        }

        private static void GenerateSamples()
        {
            string[,] liquids =
            {
                { "water", "6fd8ff" },
                { "algae", "5fdc7a" },
                { "coral", "ff8fa8" },
                { "organism", "c58cff" },
                { "plankton", "8affea" },
                { "sediment", "e0b070" }
            };

            for (int i = 0; i < liquids.GetLength(0); i++)
            {
                DrawSampleVial(liquids[i, 1]).Save(Root + $"Samples/sample_{liquids[i, 0]}.png");
            }

            DrawCrystal().Save(Root + "Props/crystal.png");
            DrawAltar().Save(Root + "Props/altar.png");

            for (int frame = 0; frame < 4; frame++)
            {
                DrawMainSample(frame / 4f * Mathf.PI * 2f).Save(Root + $"Samples/sample_main_{frame}.png");
            }
        }

        /// <summary>Cápsula grande e dourada, bem diferente dos frascos das amostras secundárias.</summary>
        private static PixelCanvas DrawMainSample(float phase)
        {
            const int width = 28;
            const int height = 38;
            var canvas = new PixelCanvas(width, height);
            const float centerX = 13.5f;
            const float centerY = 19f;
            float coreRadius = 6.5f + Mathf.Sin(phase) * 1.2f;

            for (int y = 7; y < height - 7; y++)
            {
                for (int x = 5; x < width - 5; x++)
                {
                    float dx = (x - centerX) / 9f;
                    float dy = (y - centerY) / 13f;
                    float distance = Mathf.Sqrt((x - centerX) * (x - centerX) + (y - centerY) * (y - centerY));

                    if (dx * dx + dy * dy > 1f)
                    {
                        continue;
                    }

                    string hex = distance < coreRadius * 0.5f ? "ffffff"
                        : distance < coreRadius ? "8ffff0"
                        : "1f8fa8";
                    canvas.Set(x, y, PixelCanvas.Hex(hex, (byte)(distance < coreRadius ? 255 : 170)));
                }
            }

            canvas.FillRect(6, 3, 16, 4, PixelCanvas.Hex("f4b92f"));
            canvas.FillRect(8, 1, 12, 2, PixelCanvas.Hex("c9701f"));
            canvas.FillRect(6, height - 7, 16, 4, PixelCanvas.Hex("f4b92f"));
            canvas.FillRect(8, height - 3, 12, 2, PixelCanvas.Hex("ffe08a"));
            canvas.Outline(Ink);
            return canvas;
        }

        private static PixelCanvas DrawAltar()
        {
            var canvas = new PixelCanvas(60, 18);

            for (int y = 0; y < 16; y++)
            {
                int inset = y / 2;
                canvas.FillRect(inset, y, 60 - inset * 2, 1, PixelCanvas.Hex(y % 4 == 0 ? "4e447f" : "3d3566"));
            }

            canvas.FillRect(8, 15, 44, 1, PixelCanvas.Hex("f4b92f"));

            for (int x = 12; x < 50; x += 6)
            {
                canvas.FillRect(x, 6, 2, 4, PixelCanvas.Hex("8ffff0"));
            }

            canvas.Outline(PixelCanvas.Hex("1a1224"));
            return canvas;
        }

        private static PixelCanvas DrawSampleVial(string liquidHex)
        {
            var canvas = new PixelCanvas(12, 18);
            canvas.FillRect(3, 2, 6, 11, PixelCanvas.Hex("e6fbff", 90));
            canvas.FillRect(3, 2, 6, 7, PixelCanvas.Hex(liquidHex));
            canvas.FillRect(4, 3, 1, 5, PixelCanvas.Hex("ffffff", 200));
            canvas.FillRect(3, 13, 6, 2, PixelCanvas.Hex("c98a4a"));
            canvas.Outline(PixelCanvas.Hex("0d1b2a"));
            return canvas;
        }

        private static PixelCanvas DrawCrystal()
        {
            var canvas = new PixelCanvas(16, 18);
            int[,] shards = { { 5, 9, 2 }, { 8, 15, 3 }, { 12, 7, 2 } };

            for (int shard = 0; shard < shards.GetLength(0); shard++)
            {
                int center = shards[shard, 0];
                int height = shards[shard, 1];
                int half = shards[shard, 2];

                for (int y = 0; y < height; y++)
                {
                    int width = y > height - 4 ? Mathf.Max(0, half * (height - y) / 4) : half;
                    canvas.FillRect(center - width, y, width + 1, 1, PixelCanvas.Hex("ffffff"));
                    canvas.FillRect(center + 1, y, width, 1, PixelCanvas.Hex("c8c8e8"));
                }
            }

            return canvas;
        }

        private static void GenerateHazards()
        {
            DrawSpikes().Save(Root + "Props/spikes.png");
            DrawVent().Save(Root + "Props/vent.png");
        }

        /// <summary>Rocha afiada com pontas vermelhas, para não ser confundida com a rocha comum.</summary>
        private static PixelCanvas DrawSpikes()
        {
            var canvas = new PixelCanvas(26, 20);
            int[,] peaks = { { 6, 12, 5 }, { 13, 19, 6 }, { 20, 14, 5 } };

            for (int peak = 0; peak < peaks.GetLength(0); peak++)
            {
                int center = peaks[peak, 0];
                int height = peaks[peak, 1];
                int baseWidth = peaks[peak, 2];

                for (int y = 0; y < height; y++)
                {
                    int half = Mathf.RoundToInt(baseWidth * (1f - y / (float)height));
                    string hex = y > height - 7 ? "ff5a3c" : y > height - 10 ? "b83a3a" : "5a4150";
                    canvas.FillRect(center - half, y, half * 2 + 1, 1, PixelCanvas.Hex(hex));
                }
            }

            canvas.Outline(PixelCanvas.Hex("2a1020"));
            return canvas;
        }

        private static PixelCanvas DrawVent()
        {
            var canvas = new PixelCanvas(30, 12);

            for (int y = 0; y < 10; y++)
            {
                int inset = y * 11 / 10;
                canvas.FillRect(inset, y, 30 - inset * 2, 1, PixelCanvas.Hex(y % 3 == 0 ? "4a3b52" : "3a2e45"));
            }

            canvas.FillRect(11, 9, 8, 2, PixelCanvas.Hex("ff8a3c"));
            canvas.FillRect(13, 10, 4, 1, PixelCanvas.Hex("ffe08a"));
            canvas.Outline(PixelCanvas.Hex("1a1224"));
            return canvas;
        }

        private static void GenerateBoat()
        {
            const int width = 84;
            const int height = 38;
            var canvas = new PixelCanvas(width, height);

            for (int y = 0; y < 13; y++)
            {
                int left = 8 - y / 3;
                int right = width - 3 - (12 - y) * 2 / 3 - y / 6;
                string hex = y < 5 ? "c9472b" : y == 5 ? "f1f5f4" : y < 11 ? "dfe8e8" : "f4b92f";
                canvas.FillRect(left, y, right - left, 1, PixelCanvas.Hex(hex));
            }

            canvas.FillRect(42, 13, 26, 13, PixelCanvas.Hex("f1f5f4"));
            canvas.FillRect(42, 13, 26, 2, PixelCanvas.Hex("c7d4d6"));
            canvas.FillRect(40, 26, 30, 2, PixelCanvas.Hex("c9472b"));

            for (int window = 0; window < 3; window++)
            {
                canvas.FillRect(46 + window * 7, 18, 4, 4, PixelCanvas.Hex("2a6e92"));
                canvas.Set(46 + window * 7, 21, PixelCanvas.Hex("a8ecff"));
            }

            canvas.FillRect(54, 28, 2, 9, PixelCanvas.Hex("54636c"));
            canvas.FillRect(50, 33, 10, 1, PixelCanvas.Hex("54636c"));
            canvas.DrawLine(18, 13, 25, 30, PixelCanvas.Hex("54636c"));
            canvas.DrawLine(32, 13, 25, 30, PixelCanvas.Hex("54636c"));
            canvas.DrawLine(25, 30, 18, 25, PixelCanvas.Hex("7a8a92"));
            canvas.Outline(Ink);
            canvas.Save(Root + "Props/boat.png");
        }

        private static void GenerateEffects()
        {
            var bubble = new PixelCanvas(8, 8);
            Color32 ring = PixelCanvas.Hex("d8fbff", 230);
            bubble.FillRect(2, 0, 4, 1, ring);
            bubble.FillRect(2, 7, 4, 1, ring);
            bubble.FillRect(0, 2, 1, 4, ring);
            bubble.FillRect(7, 2, 1, 4, ring);
            bubble.Set(1, 1, ring);
            bubble.Set(6, 1, ring);
            bubble.Set(1, 6, ring);
            bubble.Set(6, 6, ring);
            bubble.FillRect(1, 2, 6, 4, PixelCanvas.Hex("9fe8f2", 70));
            bubble.FillRect(2, 1, 4, 6, PixelCanvas.Hex("9fe8f2", 70));
            bubble.FillRect(2, 5, 2, 1, PixelCanvas.Hex("ffffff"));
            bubble.Save(Root + "Effects/bubble.png");

            var speck = new PixelCanvas(2, 2);
            speck.FillRect(0, 0, 2, 2, PixelCanvas.Hex("e9fff0", 200));
            speck.Save(Root + "Effects/speck.png");
        }

        private static void GenerateTitle()
        {
            const string title = "ABISSAL";
            const int scale = 5;
            const int margin = 5;
            int glyphAdvance = (PixelFont.GlyphWidth + 1) * scale;
            var canvas = new PixelCanvas(title.Length * glyphAdvance - scale + margin * 2, PixelFont.GlyphHeight * scale + margin * 2 + 3);

            DrawText(canvas, title, margin, margin + 3, scale, _ => PixelCanvas.Hex("082a3e"), 0, -3);
            DrawText(canvas, title, margin, margin + 3, scale, TitleColor, 0, 0);
            canvas.Outline(Ink);
            canvas.Outline(PixelCanvas.Hex("0d3a52"));
            canvas.Save(Root + "UI/title_abissal.png");

            DrawLabel("PRESSIONE ESPAÇO PARA MERGULHAR", "f2fffb", 1).Save(Root + "UI/prompt_dive.png");
        }

        private static void GenerateInterface()
        {
            DrawLabel("PAUSADO", "f2fffb", 3).Save(Root + "UI/pause_title.png");
            DrawLabel("PRESSIONE ESC PARA CONTINUAR", "f2fffb", 1).Save(Root + "UI/pause_prompt.png");
            DrawLabel("ESPAÇO PARA COLETAR", "ffe08a", 1).Save(Root + "UI/prompt_collect.png");
            DrawLabel("A AMOSTRA PRINCIPAL AINDA NÃO FOI COLETADA", "ff9a7a", 1).Save(Root + "UI/objective_missing.png");
            DrawLabel("MISSÃO CONCLUÍDA", "ffe08a", 2).Save(Root + "UI/victory_title.png");
            DrawLabel("PRESSIONE ESPAÇO PARA JOGAR NOVAMENTE", "f2fffb", 1).Save(Root + "UI/victory_prompt.png");
            DrawLabel("ENCONTRE A AMOSTRA PRINCIPAL NO FUNDO DO OCEANO", "f2fffb", 1).Save(Root + "UI/objective_start.png");
            DrawLabel("AMOSTRA COLETADA - RETORNE À SUPERFÍCIE", "ffe08a", 1).Save(Root + "UI/objective_return.png");
            DrawLabel("GAME OVER", "ff7a66", 3).Save(Root + "UI/gameover_title.png");
            DrawLabel("SEM OXIGÊNIO", "f2fffb", 1).Save(Root + "UI/gameover_oxygen.png");
            DrawLabel("VIDAS ESGOTADAS", "f2fffb", 1).Save(Root + "UI/gameover_lives.png");
            DrawLabel("PRESSIONE ESPAÇO PARA TENTAR NOVAMENTE", "f2fffb", 1).Save(Root + "UI/gameover_prompt.png");

            var frame = new PixelCanvas(84, 10);
            frame.FillRect(0, 0, 84, 10, Ink);
            frame.FillRect(1, 1, 82, 8, PixelCanvas.Hex("b8ecec"));
            frame.FillRect(2, 2, 80, 6, PixelCanvas.Hex("12354a"));
            frame.Save(Root + "UI/oxygen_frame.png");

            var fill = new PixelCanvas(80, 6);
            fill.FillRect(0, 0, 80, 6, PixelCanvas.Hex("ffffff"));
            fill.FillRect(0, 0, 80, 2, PixelCanvas.Hex("c8c8c8"));
            fill.Save(Root + "UI/oxygen_fill.png");

            string[] heart =
            {
                ".##.##.",
                "#######",
                "#######",
                ".#####.",
                "..###..",
                "...#..."
            };

            DrawHeart(heart, "ff5a64").Save(Root + "UI/heart_full.png");
            DrawHeart(heart, "2c3e50").Save(Root + "UI/heart_empty.png");
        }

        private static PixelCanvas DrawHeart(string[] rows, string colorHex)
        {
            var canvas = new PixelCanvas(9, 8);
            Color32 color = PixelCanvas.Hex(colorHex);
            canvas.DrawRows(1, 6, rows, c => c == '#' ? color : (Color32?)null);
            canvas.Outline(Ink);
            return canvas;
        }

        private static PixelCanvas DrawLabel(string text, string colorHex, int scale)
        {
            int advance = (PixelFont.GlyphWidth + 1) * scale;
            var canvas = new PixelCanvas(text.Length * advance + 4, (PixelFont.GlyphHeight + 5) * scale + 4);
            Color32 color = PixelCanvas.Hex(colorHex);
            DrawText(canvas, text, 2, 2 * scale + 2, scale, _ => color, 0, 0);
            canvas.Outline(Ink);
            return canvas;
        }

        private static Color32 TitleColor(float height)
        {
            if (height > 0.62f)
            {
                return PixelCanvas.Hex("e7fffb");
            }

            return PixelCanvas.Hex(height > 0.3f ? "7be6dc" : "2fb0c0");
        }

        private static void DrawText(PixelCanvas canvas, string text, int x, int y, int scale, System.Func<float, Color32> colorAtHeight, int offsetX, int offsetY)
        {
            for (int index = 0; index < text.Length; index++)
            {
                string[] rows = PixelFont.GetGlyph(text[index]);

                for (int row = 0; row < rows.Length; row++)
                {
                    for (int column = 0; column < rows[row].Length; column++)
                    {
                        if (rows[row][column] != '#')
                        {
                            continue;
                        }

                        int baseRow = PixelFont.GlyphHeight - 1 + PixelFont.AccentHeight(text[index]) - row;
                        int px = x + offsetX + (index * (PixelFont.GlyphWidth + 1) + column) * scale;
                        int py = y + offsetY + baseRow * scale;

                        for (int sy = 0; sy < scale; sy++)
                        {
                            float height = (baseRow + (sy + 0.5f) / scale) / PixelFont.GlyphHeight;
                            canvas.FillRect(px, py + sy, scale, 1, colorAtHeight(height));
                        }
                    }
                }
            }
        }
    }
}
