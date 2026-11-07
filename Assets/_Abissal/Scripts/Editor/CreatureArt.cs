using System;
using UnityEngine;
using static Abissal.EditorTools.PixelShapes;

namespace Abissal.EditorTools
{
    /// <summary>
    /// Desenha todos os animais do jogo em camadas.
    /// Regra visual: animais vermelhos, pontudos ou com dentes são perigosos.
    /// </summary>
    public static class CreatureArt
    {
        private const int Frames = 4;

        private static readonly Color32 Ink = PixelCanvas.Hex("101826");
        private static readonly Color32 White = PixelCanvas.Hex("ffffff");

        public static void Generate(string root)
        {
            string path = root + "Creatures/";

            for (int frame = 0; frame < Frames; frame++)
            {
                float phase = frame / (float)Frames * Mathf.PI * 2f;

                DrawClownFish(phase).Save(path + $"fish_orange_{frame}.png");
                DrawBandFish(phase).Save(path + $"fish_yellow_{frame}.png");
                DrawTang(phase).Save(path + $"fish_blue_{frame}.png");
                DrawSardine(phase).Save(path + $"fish_silver_{frame}.png");
                DrawLanternFish(phase).Save(path + $"fish_glow_{frame}.png");

                DrawShark(phase, false).Save(path + $"bigfish_{frame}.png");
                DrawShark(phase, true).Save(path + $"bigfish_attack_{frame}.png");
                DrawAngler(phase).Save(path + $"angler_{frame}.png");

                DrawJellyfish(phase, "d67cf0", "ffd9ff", "a24fd0", "e7a7ff", "6e3aa0").Save(path + $"jellyfish_{frame}.png");
                DrawJellyfish(phase, "4fd8ff", "e0fbff", "2a9fd8", "9debff", "1d6f9e").Save(path + $"jellyfish_deep_{frame}.png");

                DrawOctopus(phase).Save(path + $"octopus_{frame}.png");
                DrawTurtle(phase).Save(path + $"turtle_{frame}.png");
                DrawManta(phase).Save(path + $"manta_{frame}.png");
                DrawSeahorse(phase).Save(path + $"seahorse_{frame}.png");
                DrawCrab(phase).Save(path + $"crab_{frame}.png");
                DrawPufferfish(phase).Save(path + $"pufferfish_{frame}.png");
                DrawSquid(phase).Save(path + $"squid_{frame}.png");
            }
        }

        /// <summary>O principal perigo: silhueta de tubarão, dorso escuro, barriga clara e cicatrizes.</summary>
        private static PixelCanvas DrawShark(float phase, bool isAttacking)
        {
            const int width = 74;
            const int height = 38;
            const float center = 17f;

            Color32 back = PixelCanvas.Hex("3f5a70");
            Color32 flank = PixelCanvas.Hex("5f8096");
            Color32 ridge = PixelCanvas.Hex("2f465a");
            Color32 belly = PixelCanvas.Hex("dbe8ec");
            Color32 bellyShade = PixelCanvas.Hex("a9c0ca");
            Color32 fin = PixelCanvas.Hex("35506a");
            Color32 finLight = PixelCanvas.Hex("56788f");
            Color32 finDark = PixelCanvas.Hex("283c4e");
            Color32 gill = PixelCanvas.Hex("223444");
            Color32 scar = PixelCanvas.Hex("9ab4c2");

            var sprite = new PixelCanvas(width, height);

            // Peças de trás: nadadeiras do lado oposto do corpo.
            Layer(sprite, Ink, part => FillPolygon(part, finDark,
                new Vector2(45f, center - 6f), new Vector2(50f, center - 7f), new Vector2(43f, center - 15f), new Vector2(39f, center - 15f)));
            Layer(sprite, Ink, part => FillPolygon(part, finDark,
                new Vector2(24f, center - 5f), new Vector2(29f, center - 6f), new Vector2(22f, center - 11f), new Vector2(20f, center - 10f)));

            // Cauda com o lóbulo de cima bem maior, típica dos tubarões.
            Layer(sprite, Ink, part =>
            {
                FillPolygon(part, fin, new Vector2(10f, center + 1f), new Vector2(0f, center + 18f), new Vector2(4f, center + 19f), new Vector2(13f, center + 7f), new Vector2(16f, center + 2f));
                FillPolygon(part, finDark, new Vector2(9f, center + 1f), new Vector2(1f, center - 10f), new Vector2(4f, center - 10f), new Vector2(15f, center - 1f));
                FillPolygon(part, finLight, new Vector2(2f, center + 17f), new Vector2(4f, center + 18f), new Vector2(12f, center + 7f), new Vector2(10f, center + 7f));
            });

            // Barbatana dorsal alta, a parte mais reconhecível do animal.
            Layer(sprite, Ink, part =>
            {
                FillPolygon(part, fin, new Vector2(30f, center + 8f), new Vector2(31f, center + 18f), new Vector2(34f, center + 20f), new Vector2(44f, center + 9f));
                FillPolygon(part, finLight, new Vector2(33f, center + 18f), new Vector2(35f, center + 19f), new Vector2(43f, center + 10f), new Vector2(40f, center + 9f));
            });

            Layer(sprite, Ink, part => FillPolygon(part, fin,
                new Vector2(17f, center + 4f), new Vector2(17f, center + 8f), new Vector2(19f, center + 9f), new Vector2(23f, center + 5f)));

            Layer(sprite, Ink, part =>
            {
                for (int x = 11; x <= 72; x++)
                {
                    float u = (x - 11f) / 61f;
                    float thickness = Mathf.Pow(Mathf.Sin(Mathf.PI * Mathf.Pow(u, 0.72f)), 0.8f) * 10.5f + 1f;
                    float lift = u * u * 2.5f;
                    float top = center + thickness + lift;
                    float bottom = center - thickness * 0.8f + lift;

                    for (int y = Mathf.RoundToInt(bottom); y <= Mathf.RoundToInt(top); y++)
                    {
                        float height01 = (y - bottom) / Mathf.Max(top - bottom, 1f);
                        float wavyEdge = 0.4f + Mathf.Sin(x * 0.45f) * 0.03f;
                        Color32 color;

                        if (height01 < wavyEdge * 0.45f)
                        {
                            color = bellyShade;
                        }
                        else if (height01 < wavyEdge)
                        {
                            color = belly;
                        }
                        else if (height01 < wavyEdge + 0.2f)
                        {
                            color = (x + y) % 2 == 0 && height01 < wavyEdge + 0.08f ? belly : flank;
                        }
                        else
                        {
                            color = height01 > 0.9f ? ridge : back;
                        }

                        part.Set(x, y, color);
                    }
                }

                // Guelras.
                for (int i = 0; i < 4; i++)
                {
                    float gillX = 50f + i * 2.2f;
                    part.DrawLine(gillX, center - 2f, gillX + 1f, center + 5f, gill);
                }

                // Cicatrizes de brigas antigas.
                part.DrawLine(30f, center + 6f, 35f, center + 3f, scar);
                part.DrawLine(32f, center + 7f, 36f, center + 5f, scar);
                part.DrawLine(40f, center + 2f, 43f, center + 4f, scar);

                DrawSharkHead(part, center, isAttacking, gill);
            });

            // Nadadeira peitoral por cima do corpo.
            Layer(sprite, Ink, part =>
            {
                FillPolygon(part, fin, new Vector2(47f, center - 3f), new Vector2(53f, center - 5f), new Vector2(44f, center - 16f), new Vector2(39f, center - 15f));
                FillPolygon(part, finLight, new Vector2(47f, center - 4f), new Vector2(50f, center - 5f), new Vector2(43f, center - 14f), new Vector2(41f, center - 14f));
            });

            return Wave(sprite, 2.6f, phase, 46f);
        }

        private static void DrawSharkHead(PixelCanvas part, float center, bool isAttacking, Color32 line)
        {
            Color32 mouth = PixelCanvas.Hex("7a1f2a");
            Color32 mouthDeep = PixelCanvas.Hex("3e0f18");
            Color32 eye = isAttacking ? PixelCanvas.Hex("ff3b30") : PixelCanvas.Hex("0c1218");

            if (isAttacking)
            {
                // Boca aberta: a mandíbula desce e mostra as fileiras de dentes.
                FillPolygon(part, mouth, new Vector2(72f, center + 1f), new Vector2(55f, center - 2f), new Vector2(53f, center - 7f), new Vector2(64f, center - 10f), new Vector2(70f, center - 7f));
                FillPolygon(part, mouthDeep, new Vector2(62f, center - 1f), new Vector2(55f, center - 2f), new Vector2(54f, center - 6f), new Vector2(61f, center - 7f));

                for (int x = 57; x <= 70; x += 2)
                {
                    int topTooth = Mathf.RoundToInt(center - 1f + (x - 55) * 0.16f);
                    part.Set(x, topTooth, White);
                    part.Set(x, topTooth - 1, White);
                    part.Set(x, topTooth - 2, PixelCanvas.Hex("cfd8dc"));
                }

                for (int x = 56; x <= 67; x += 2)
                {
                    int lowTooth = Mathf.RoundToInt(center - 8f - (x - 56) * 0.1f);
                    part.Set(x, lowTooth, White);
                    part.Set(x, lowTooth + 1, White);
                }
            }
            else
            {
                // Boca fechada: linha com a ponta dos dentes aparecendo.
                part.DrawLine(69f, center - 1f, 55f, center - 4f, line);

                for (int x = 57; x <= 67; x += 3)
                {
                    int y = Mathf.RoundToInt(center - 4f + (x - 55) * 0.21f);
                    part.Set(x, y - 1, White);
                }
            }

            part.FillRect(61, Mathf.RoundToInt(center) + 4, 3, 3, eye);
            part.Set(61, Mathf.RoundToInt(center) + 6, White);
            part.DrawLine(59f, center + 8f, 65f, center + 7f, line);
        }

        private static PixelCanvas DrawAngler(float phase)
        {
            const int width = 46;
            const int height = 36;
            const float centerX = 25f;
            const float centerY = 14f;
            float sway = Mathf.Sin(phase) * 1.5f;

            Color32[] skin = Shades("3a3560", "5a5490", "26223f");
            Color32 fin = PixelCanvas.Hex("2c2850");
            var sprite = new PixelCanvas(width, height);

            Layer(sprite, Ink, part => FillPolygon(part, fin,
                new Vector2(13f, centerY), new Vector2(1f, centerY + 8f), new Vector2(4f, centerY), new Vector2(1f, centerY - 8f)));

            Layer(sprite, Ink, part =>
            {
                FillEllipse(part, centerX, centerY, 14f, 12.5f, skin);

                // Verrugas e manchas na pele.
                foreach (Vector2 spot in new[] { new Vector2(17f, 19f), new Vector2(22f, 23f), new Vector2(15f, 11f), new Vector2(20f, 7f), new Vector2(27f, 23f) })
                {
                    part.Set((int)spot.x, (int)spot.y, skin[1]);
                    part.Set((int)spot.x + 1, (int)spot.y, skin[2]);
                }

                // Boca enorme e cheia de dentes.
                FillPolygon(part, PixelCanvas.Hex("2a0d18"), new Vector2(40f, 15f), new Vector2(24f, 11f), new Vector2(25f, 5f), new Vector2(38f, 4f));

                for (int x = 26; x <= 38; x += 3)
                {
                    int upper = Mathf.RoundToInt(11f + (x - 24) * 0.27f);
                    part.DrawLine(x, upper, x + 1f, upper - 3f, PixelCanvas.Hex("f4fbff"));
                    part.DrawLine(x + 1f, 4f, x, 7f, PixelCanvas.Hex("f4fbff"));
                }

                part.FillRect(33, 19, 4, 4, PixelCanvas.Hex("ffe45c"));
                part.FillRect(34, 19, 2, 3, Ink);
                part.Set(34, 21, White);
            });

            Layer(sprite, Ink, part => FillPolygon(part, fin,
                new Vector2(19f, 12f), new Vector2(11f, 7f), new Vector2(13f, 15f)));

            var lureBase = new Vector2(27f, 26f);
            var lureMid = new Vector2(33f, 32f);
            var lureEnd = new Vector2(39f + sway, 29f + sway * 0.4f);
            sprite.DrawLine(lureBase.x, lureBase.y, lureMid.x, lureMid.y, PixelCanvas.Hex("7a6fb0"));
            sprite.DrawLine(lureMid.x, lureMid.y, lureEnd.x, lureEnd.y, PixelCanvas.Hex("7a6fb0"));
            FillDisc(sprite, lureEnd.x, lureEnd.y, 2.6f, PixelCanvas.Hex("9dffea"));
            sprite.Set(Mathf.RoundToInt(lureEnd.x) - 1, Mathf.RoundToInt(lureEnd.y) + 1, White);
            return Wave(sprite, 1.4f, phase, 18f);
        }

        /// <summary>Base dos peixes pequenos. As cores de cada ponto vêm da função "pattern".</summary>
        private static PixelCanvas DrawReefFish(
            float phase,
            int width,
            int height,
            float radiusX,
            float radiusY,
            string finHex,
            string finDarkHex,
            Func<float, float, Color32> pattern,
            float waveAmount)
        {
            Color32 finColor = PixelCanvas.Hex(finHex);
            Color32 finDark = PixelCanvas.Hex(finDarkHex);
            float centerX = width - radiusX - 2f;
            float centerY = height * 0.5f - 0.5f;
            float tailRoot = centerX - radiusX + 1.5f;
            var sprite = new PixelCanvas(width, height);

            Layer(sprite, Ink, part =>
            {
                FillPolygon(part, finColor,
                    new Vector2(tailRoot, centerY + 1f), new Vector2(1f, centerY + radiusY), new Vector2(3f, centerY), new Vector2(1f, centerY - radiusY), new Vector2(tailRoot, centerY - 1f));
                FillPolygon(part, finColor,
                    new Vector2(centerX - radiusX * 0.5f, centerY + radiusY * 0.8f), new Vector2(centerX - radiusX * 0.2f, centerY + radiusY + 2f), new Vector2(centerX + radiusX * 0.35f, centerY + radiusY * 0.8f));
                FillPolygon(part, finDark,
                    new Vector2(centerX - radiusX * 0.4f, centerY - radiusY * 0.8f), new Vector2(centerX - radiusX * 0.2f, centerY - radiusY - 2f), new Vector2(centerX + radiusX * 0.2f, centerY - radiusY * 0.8f));
            });

            Layer(sprite, Ink, part =>
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        float u = (x - centerX) / radiusX;
                        float v = (y - centerY) / radiusY;

                        if (u * u + v * v <= 1f)
                        {
                            part.Set(x, y, pattern(u, v));
                        }
                    }
                }

                int eyeX = Mathf.RoundToInt(centerX + radiusX * 0.5f);
                int eyeY = Mathf.RoundToInt(centerY + radiusY * 0.25f);
                part.FillRect(eyeX, eyeY, 2, 2, White);
                part.Set(eyeX + 1, eyeY, Ink);
                part.Set(eyeX + 1, eyeY + 1, Ink);
                part.Set(Mathf.RoundToInt(centerX + radiusX * 0.85f), Mathf.RoundToInt(centerY - radiusY * 0.2f), Ink);
            });

            if (radiusY > 4f)
            {
                Layer(sprite, Ink, part => FillPolygon(part, finDark,
                    new Vector2(centerX, centerY), new Vector2(centerX - 3f, centerY + Mathf.Sin(phase) * 1.5f - 2f), new Vector2(centerX + 1f, centerY - 2f)));
            }

            return Wave(sprite, waveAmount, phase, width * 0.55f);
        }

        private static PixelCanvas DrawClownFish(float phase)
        {
            Color32 orange = PixelCanvas.Hex("f27a2a");
            Color32 light = PixelCanvas.Hex("ffa95c");
            Color32 dark = PixelCanvas.Hex("c7541c");

            return DrawReefFish(phase, 20, 13, 7.5f, 4.5f, "f27a2a", "c7541c", (u, v) =>
            {
                bool isBand = Mathf.Abs(u + 0.35f) < 0.13f || Mathf.Abs(u - 0.4f) < 0.11f;

                if (isBand)
                {
                    return White;
                }

                return v > 0.45f ? light : v < -0.45f ? dark : orange;
            }, 1.2f);
        }

        private static PixelCanvas DrawBandFish(float phase)
        {
            Color32 yellow = PixelCanvas.Hex("ffd24d");
            Color32 light = PixelCanvas.Hex("fff0a6");
            Color32 dark = PixelCanvas.Hex("e0a428");
            Color32 band = PixelCanvas.Hex("2b3a55");

            return DrawReefFish(phase, 24, 20, 8.5f, 7f, "ffd24d", "e0a428", (u, v) =>
            {
                bool isBand = Mathf.Abs(u + 0.15f + v * 0.25f) < 0.12f || Mathf.Abs(u - 0.55f) < 0.08f;

                if (isBand)
                {
                    return band;
                }

                return v > 0.5f ? light : v < -0.5f ? dark : yellow;
            }, 1.2f);
        }

        private static PixelCanvas DrawTang(float phase)
        {
            Color32 blue = PixelCanvas.Hex("3d9de8");
            Color32 light = PixelCanvas.Hex("8fd3ff");
            Color32 dark = PixelCanvas.Hex("1f4f9a");

            return DrawReefFish(phase, 22, 13, 6.5f, 4f, "ffd24d", "e0a428", (u, v) =>
            {
                bool isMark = v > -0.1f && v < 0.45f && u < 0.45f && u > -0.8f && Mathf.Abs(v - 0.15f - u * 0.2f) < 0.2f;

                if (isMark)
                {
                    return dark;
                }

                return v > 0.5f ? light : v < -0.4f ? dark : blue;
            }, 1f);
        }

        private static PixelCanvas DrawSardine(float phase)
        {
            Color32 back = PixelCanvas.Hex("5f7f9c");
            Color32 side = PixelCanvas.Hex("b6c8d6");
            Color32 belly = PixelCanvas.Hex("eef5f8");

            return DrawReefFish(phase, 24, 11, 8f, 2.8f, "8ea6ba", "5f7f9c", (u, v) =>
            {
                if (v > 0.25f)
                {
                    return back;
                }

                bool isScale = Mathf.Abs(v) < 0.2f && Mathf.Repeat(u * 6f, 1f) < 0.3f;
                return isScale ? back : v < -0.35f ? belly : side;
            }, 1f);
        }

        private static PixelCanvas DrawLanternFish(float phase)
        {
            Color32 body = PixelCanvas.Hex("2fb8a8");
            Color32 dark = PixelCanvas.Hex("147a78");
            Color32 glow = PixelCanvas.Hex("c8fff2");

            return DrawReefFish(phase, 20, 11, 5.5f, 3f, "1f8f88", "147a78", (u, v) =>
            {
                bool isLight = v < -0.1f && v > -0.65f && Mathf.Repeat(u * 2.5f, 1f) < 0.4f;
                return isLight ? glow : v > 0.3f ? dark : body;
            }, 0.8f);
        }

        private static PixelCanvas DrawJellyfish(float phase, string bodyHex, string lightHex, string darkHex, string tentacleHex, string outlineHex)
        {
            const int width = 26;
            const int height = 34;
            const float centerX = 13f;
            const float bellBase = 20f;
            float pulse = Mathf.Sin(phase);
            float radiusX = 9f + pulse * 1.2f;
            float radiusY = 8f - pulse;
            var outline = PixelCanvas.Hex(outlineHex, 230);
            Color32 body = PixelCanvas.Hex(bodyHex, 225);
            Color32 light = PixelCanvas.Hex(lightHex, 235);
            Color32 dark = PixelCanvas.Hex(darkHex, 225);
            Color32 tentacle = PixelCanvas.Hex(tentacleHex, 205);
            var sprite = new PixelCanvas(width, height);

            for (int arm = -3; arm <= 3; arm++)
            {
                int length = 14 + Mathf.Abs(arm + 1) % 3 * 3;

                for (int i = 0; i < length; i++)
                {
                    float wiggle = Mathf.Sin(phase + i * 0.55f + arm) * (i / 8f);
                    int x = Mathf.RoundToInt(centerX + arm * radiusX * 0.27f * (1f - i / 48f) + wiggle);
                    sprite.Set(x, Mathf.RoundToInt(bellBase) - i, tentacle);

                    // Dois braços centrais mais grossos e franjados.
                    if (Mathf.Abs(arm) == 1 && i % 3 != 2)
                    {
                        sprite.Set(x + 1, Mathf.RoundToInt(bellBase) - i, light);
                    }
                }
            }

            Layer(sprite, outline, part =>
            {
                for (int y = Mathf.RoundToInt(bellBase); y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        float dx = (x - centerX) / radiusX;
                        float dy = (y - bellBase) / radiusY;

                        if (dx * dx + dy * dy > 1f)
                        {
                            continue;
                        }

                        bool isRib = Mathf.Repeat(dx * 2.5f + 0.5f, 1f) < 0.16f && dy < 0.8f;
                        bool isShine = dx < -0.15f && dy > 0.45f;
                        part.Set(x, y, isShine ? light : isRib ? dark : body);
                    }
                }

                // Órgão interno claro e bolinhas na borda.
                FillDisc(part, centerX, bellBase + radiusY * 0.45f, 2.5f, light);

                for (int x = Mathf.RoundToInt(centerX - radiusX) + 1; x < centerX + radiusX; x += 3)
                {
                    part.Set(x, Mathf.RoundToInt(bellBase), light);
                }
            });

            return sprite;
        }

        /// <summary>Perigoso: vermelho, com ventosas e tentáculos que se enrolam.</summary>
        private static PixelCanvas DrawOctopus(float phase)
        {
            const int width = 36;
            const int height = 36;
            Color32[] skin = Shades("c8402a", "f07a50", "8a2418");
            Color32[] farSkin = Shades("8a2418", "b03a26", "641a12");
            Color32 sucker = PixelCanvas.Hex("ffb89a");
            var sprite = new PixelCanvas(width, height);

            for (int arm = 0; arm < 6; arm++)
            {
                bool isFar = arm % 2 == 1;
                int index = arm;

                Layer(sprite, Ink, part =>
                {
                    float baseX = 18f + (index - 2.5f) * 3f;
                    float side = (index - 2.5f) / 2.5f;
                    Vector2 previous = new Vector2(baseX, 17f);

                    for (int segment = 1; segment <= 7; segment++)
                    {
                        float t = segment / 7f;
                        float curl = Mathf.Sin(phase + index * 1.7f + t * 3.5f) * 2.4f * t;
                        var next = new Vector2(
                            baseX + side * 10f * t + curl,
                            17f - 15f * t + Mathf.Abs(side) * 5f * t * t);

                        FillCapsule(part, previous, next, Mathf.Lerp(2.4f, 0.8f, t), isFar ? farSkin : skin);

                        if (!isFar && segment % 2 == 0)
                        {
                            part.Set(Mathf.RoundToInt(next.x), Mathf.RoundToInt(next.y) - 1, sucker);
                        }

                        previous = next;
                    }
                });
            }

            float breath = Mathf.Sin(phase) * 0.6f;

            Layer(sprite, Ink, part =>
            {
                FillEllipse(part, 18f, 24f + breath, 8.5f, 10f - breath, skin);

                foreach (Vector2 spot in new[] { new Vector2(14f, 29f), new Vector2(21f, 31f), new Vector2(23f, 26f), new Vector2(13f, 23f) })
                {
                    part.Set((int)spot.x, (int)(spot.y + breath), skin[2]);
                }

                part.FillRect(13, 19, 4, 3, PixelCanvas.Hex("ffe45c"));
                part.FillRect(20, 19, 4, 3, PixelCanvas.Hex("ffe45c"));
                part.FillRect(14, 20, 3, 1, Ink);
                part.FillRect(21, 20, 3, 1, Ink);
            });

            return sprite;
        }

        private static PixelCanvas DrawTurtle(float phase)
        {
            const int width = 40;
            const int height = 24;
            Color32[] shell = Shades("4f8f4a", "86c46a", "2f6236");
            Color32[] skin = Shades("9ab86a", "c4dc94", "6a8a48");
            Color32 scute = PixelCanvas.Hex("2a5230");
            float paddle = Mathf.Sin(phase);
            var sprite = new PixelCanvas(width, height);

            Layer(sprite, Ink, part => FillCapsule(part, new Vector2(14f, 8f), new Vector2(8f, 6f - paddle * 2f), 2f, skin));
            Layer(sprite, Ink, part => FillCapsule(part, new Vector2(26f, 8f), new Vector2(23f, 3f - paddle * 2f), 2.3f, skin));

            Layer(sprite, Ink, part =>
            {
                for (int y = 8; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        float dx = (x - 20f) / 14f;
                        float dy = (y - 9f) / 11f;

                        if (dx * dx + dy * dy <= 1f)
                        {
                            part.Set(x, y, dy > 0.7f ? shell[1] : dy < 0.15f ? shell[2] : shell[0]);
                        }
                    }
                }

                // Placas do casco.
                for (int x = 11; x <= 29; x += 6)
                {
                    part.DrawLine(x, 9f, x + 2f, 15f, scute);
                    part.DrawLine(x + 2f, 15f, x + 6f, 17f, scute);
                }

                part.DrawLine(8f, 12f, 32f, 12f, scute);
            });

            Layer(sprite, Ink, part =>
            {
                FillEllipse(part, 35f, 11f, 3.8f, 3f, skin);
                part.Set(36, 12, Ink);
            });

            Layer(sprite, Ink, part => FillCapsule(part, new Vector2(26f, 9f), new Vector2(29f, 3f + paddle * 2f), 2.3f, skin));
            return sprite;
        }

        /// <summary>Vista de cima, com as asas subindo e descendo e o dorso manchado.</summary>
        private static PixelCanvas DrawManta(float phase)
        {
            const int width = 56;
            const int height = 44;
            const float centerY = 21.5f;
            float wing = 19f + Mathf.Sin(phase) * 2.5f;
            float sweep = Mathf.Cos(phase) * 2f;
            Color32 back = PixelCanvas.Hex("2f4a7a");
            Color32 light = PixelCanvas.Hex("5a7fb0");
            Color32 dark = PixelCanvas.Hex("1f3358");
            Color32 pale = PixelCanvas.Hex("dbe6f0");
            var sprite = new PixelCanvas(width, height);

            sprite.DrawLine(18f, centerY, 2f, centerY + Mathf.Sin(phase) * 2f, dark);

            Layer(sprite, Ink, part =>
            {
                FillPolygon(part, back,
                    new Vector2(47f, centerY + 3f),
                    new Vector2(30f + sweep, centerY + wing),
                    new Vector2(25f + sweep, centerY + wing - 1f),
                    new Vector2(15f, centerY),
                    new Vector2(25f + sweep, centerY - wing + 1f),
                    new Vector2(30f + sweep, centerY - wing),
                    new Vector2(47f, centerY - 3f));

                FillPolygon(part, light,
                    new Vector2(44f, centerY + 3f),
                    new Vector2(31f + sweep, centerY + wing - 1f),
                    new Vector2(29f + sweep, centerY + wing - 3f),
                    new Vector2(38f, centerY + 3f));

                FillPolygon(part, dark,
                    new Vector2(44f, centerY - 3f),
                    new Vector2(31f + sweep, centerY - wing + 1f),
                    new Vector2(29f + sweep, centerY - wing + 3f),
                    new Vector2(38f, centerY - 3f));

                // Marcas claras no dorso e nos ombros.
                FillPolygon(part, pale, new Vector2(36f, centerY + 5f), new Vector2(31f, centerY + 9f), new Vector2(29f, centerY + 5f));
                FillPolygon(part, pale, new Vector2(36f, centerY - 5f), new Vector2(31f, centerY - 9f), new Vector2(29f, centerY - 5f));
                part.Set(25, Mathf.RoundToInt(centerY) + 2, pale);
                part.Set(22, Mathf.RoundToInt(centerY) - 1, pale);
                part.Set(27, Mathf.RoundToInt(centerY) - 3, pale);

                FillCapsule(part, new Vector2(38f, centerY), new Vector2(47f, centerY), 3.6f, new[] { back, light, dark });
            });

            // Lóbulos da cabeça, característicos da manta.
            Layer(sprite, Ink, part =>
            {
                FillCapsule(part, new Vector2(47f, centerY + 3f), new Vector2(52f, centerY + 4f), 1.1f, new[] { back, light, dark });
                FillCapsule(part, new Vector2(47f, centerY - 3f), new Vector2(52f, centerY - 4f), 1.1f, new[] { back, light, dark });
                part.Set(46, Mathf.RoundToInt(centerY) + 3, White);
                part.Set(46, Mathf.RoundToInt(centerY) - 3, White);
            });

            return sprite;
        }

        private static PixelCanvas DrawSeahorse(float phase)
        {
            const int width = 16;
            const int height = 26;
            Color32[] skin = Shades("f0a020", "ffd56a", "b86a14");
            Color32 fin = PixelCanvas.Hex("ffe3a0");
            float sway = Mathf.Sin(phase);
            var sprite = new PixelCanvas(width, height);

            Layer(sprite, Ink, part =>
            {
                FillCapsule(part, new Vector2(8f, 16f), new Vector2(7f, 9f), 3f, skin);
                FillCapsule(part, new Vector2(7f, 9f), new Vector2(9f, 5f), 2f, skin);
                FillCapsule(part, new Vector2(9f, 5f), new Vector2(7f, 2f), 1.1f, skin);
                FillSphere(part, 8.5f, 20f, 3.4f, skin);
                FillCapsule(part, new Vector2(10f, 20f), new Vector2(14f, 19f), 1f, skin);

                for (int y = 8; y <= 16; y += 2)
                {
                    part.Set(4, y, skin[2]);
                    part.DrawLine(6f, y, 10f, y, skin[2]);
                }

                part.FillRect(7, 23, 1, 2, skin[2]);
                part.FillRect(9, 20, 2, 2, White);
                part.Set(10, 20, Ink);
            });

            Layer(sprite, Ink, part => FillPolygon(part, fin,
                new Vector2(5f, 15f), new Vector2(2f + sway, 14f), new Vector2(1f + sway, 10f), new Vector2(5f, 11f)));

            return sprite;
        }

        private static PixelCanvas DrawCrab(float phase)
        {
            const int width = 26;
            const int height = 16;
            Color32[] shell = Shades("e06a30", "ff9a5c", "a8441c");
            float step = Mathf.Sin(phase) * 1.2f;
            var sprite = new PixelCanvas(width, height);

            for (int leg = 0; leg < 3; leg++)
            {
                int index = leg;

                Layer(sprite, Ink, part =>
                {
                    float offset = index % 2 == 0 ? step : -step;
                    FillCapsule(part, new Vector2(8f - index, 6f), new Vector2(3f - index, 1f + offset), 0.7f, shell);
                    FillCapsule(part, new Vector2(18f + index, 6f), new Vector2(23f + index, 1f - offset), 0.7f, shell);
                });
            }

            Layer(sprite, Ink, part =>
            {
                FillCapsule(part, new Vector2(5f, 8f), new Vector2(2f, 11f), 1.8f, shell);
                FillCapsule(part, new Vector2(21f, 8f), new Vector2(24f, 11f), 1.8f, shell);
                FillEllipse(part, 13f, 7f, 8f, 4.5f, shell);

                for (int x = 8; x <= 18; x += 5)
                {
                    part.Set(x, 8, shell[2]);
                }

                part.FillRect(10, 11, 1, 3, Ink);
                part.FillRect(15, 11, 1, 3, Ink);
                part.FillRect(9, 13, 3, 2, White);
                part.FillRect(14, 13, 3, 2, White);
                part.Set(10, 14, Ink);
                part.Set(15, 14, Ink);
            });

            return sprite;
        }

        /// <summary>Perigoso: redondo e cheio de espinhos, que crescem quando ele infla.</summary>
        private static PixelCanvas DrawPufferfish(float phase)
        {
            const int size = 30;
            const float center = 14.5f;
            Color32[] skin = Shades("d8b83a", "f8e88a", "9a8020");
            Color32 belly = PixelCanvas.Hex("fff6d0");
            Color32 spike = PixelCanvas.Hex("e8503a");
            float radius = 8.5f + Mathf.Sin(phase) * 1f;
            var sprite = new PixelCanvas(size, size);

            Layer(sprite, Ink, part =>
            {
                FillPolygon(part, skin[2], new Vector2(6f, center + 2f), new Vector2(1f, center + 5f), new Vector2(1f, center - 5f), new Vector2(6f, center - 2f));

                for (int angle = 0; angle < 360; angle += 30)
                {
                    float radians = angle * Mathf.Deg2Rad;
                    var direction = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
                    Vector2 root = new Vector2(center, center) + direction * radius;
                    part.DrawLine(root.x, root.y, root.x + direction.x * 3.5f, root.y + direction.y * 3.5f, spike);
                }
            });

            Layer(sprite, Ink, part =>
            {
                FillSphere(part, center, center, radius, skin);

                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        float dx = x - center;
                        float dy = y - center;

                        if (dx * dx + dy * dy < radius * radius * 0.8f && dy < -radius * 0.25f)
                        {
                            part.Set(x, y, belly);
                        }
                    }
                }

                part.FillRect(18, 16, 4, 4, White);
                part.FillRect(20, 16, 2, 3, Ink);
                part.DrawLine(21f, 12f, 24f, 11f, Ink);
                part.Set(13, 19, skin[2]);
                part.Set(10, 16, skin[2]);
                part.Set(15, 22, skin[2]);
            });

            return sprite;
        }

        private static PixelCanvas DrawSquid(float phase)
        {
            const int width = 40;
            const int height = 18;
            Color32[] skin = Shades("46c8e8", "b8f4ff", "1f7fa8");
            Color32 spot = PixelCanvas.Hex("e0ffff");
            var sprite = new PixelCanvas(width, height);

            Layer(sprite, Ink, part =>
            {
                for (int arm = -2; arm <= 2; arm++)
                {
                    Vector2 previous = new Vector2(14f, 9f + arm);

                    for (int i = 1; i <= 6; i++)
                    {
                        float wiggle = Mathf.Sin(phase + i * 0.9f + arm) * 0.9f;
                        var next = new Vector2(14f - i * 2.2f, 9f + arm * (1f + i * 0.25f) + wiggle);
                        part.DrawLine(previous.x, previous.y, next.x, next.y, skin[arm % 2 == 0 ? 0 : 2]);
                        previous = next;
                    }
                }
            });

            Layer(sprite, Ink, part =>
            {
                FillPolygon(part, skin[2], new Vector2(27f, 12f), new Vector2(34f, 17f), new Vector2(38f, 10f), new Vector2(30f, 11f));
                FillPolygon(part, skin[2], new Vector2(27f, 6f), new Vector2(34f, 1f), new Vector2(38f, 8f), new Vector2(30f, 7f));
                FillPolygon(part, skin[0], new Vector2(26f, 13f), new Vector2(39f, 9f), new Vector2(26f, 5f));
                FillCapsule(part, new Vector2(15f, 9f), new Vector2(27f, 9f), 4.3f, skin);

                for (int x = 19; x <= 31; x += 3)
                {
                    part.Set(x, 7 + x % 2 * 4, spot);
                }

                part.FillRect(15, 10, 3, 3, White);
                part.Set(15, 11, Ink);
                part.Set(16, 11, Ink);
            });

            return sprite;
        }
    }
}
