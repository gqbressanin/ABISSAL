using System;
using UnityEngine;

namespace Abissal.EditorTools
{
    /// <summary>
    /// Desenha o mergulhador com escafandro clássico em camadas: cada parte recebe
    /// sombra e contorno próprios antes de ser colada, o que separa visualmente as peças.
    /// </summary>
    public static class DiverArt
    {
        private const int SwimFrames = 4;

        private static readonly Color32 Ink = PixelCanvas.Hex("1a1420");

        private static readonly Color32[] Suit = Shades("e8a820", "fbd65a", "b87818");
        private static readonly Color32[] FarSuit = Shades("b87818", "d09428", "8a5812");
        private static readonly Color32[] Brass = Shades("d99a2a", "ffe58a", "9a6118");
        private static readonly Color32[] Copper = Shades("b0502a", "e08a50", "7a3018");
        private static readonly Color32[] Rubber = Shades("d8582a", "f58a50", "a23a1c");
        private static readonly Color32[] FarRubber = Shades("a23a1c", "c2502a", "7a2a14");
        private static readonly Color32[] Metal = Shades("6a6a7a", "a2a2b4", "42424e");
        private static readonly Color32[] Patch = Shades("7a7030", "9a9048", "565020");

        private static readonly Color32 Leather = PixelCanvas.Hex("5a3a20");
        private static readonly Color32 Rope = PixelCanvas.Hex("c8a060");
        private static readonly Color32 RopeDark = PixelCanvas.Hex("7a5a2c");
        private static readonly Color32 Glass = PixelCanvas.Hex("9fdde8");
        private static readonly Color32 GlassDark = PixelCanvas.Hex("4f9fb4");
        private static readonly Color32 Shine = PixelCanvas.Hex("ffffff");

        public static void Generate(string root)
        {
            for (int frame = 0; frame < SwimFrames; frame++)
            {
                DrawSwimming(frame / (float)SwimFrames * Mathf.PI * 2f).Save(root + $"Characters/diver_swim_{frame}.png");
            }

            DrawStanding().Save(root + "Characters/diver_stand.png");
        }

        /// <summary>Nadando de lado, com a cabeça para a direita.</summary>
        private static PixelCanvas DrawSwimming(float phase)
        {
            const int width = 46;
            const int height = 28;
            const float centerY = 13f;
            const float headY = centerY + 1.5f;

            // O deslocamento evita que dois quadros tenham exatamente a mesma pose.
            float kick = Mathf.Sin(phase + Mathf.PI * 0.25f);
            float stroke = Mathf.Cos(phase + Mathf.PI * 0.25f);
            var result = new PixelCanvas(width, height);

            Stamp(result, part => DrawLeg(part, 18f, centerY - 1f, -kick, FarSuit, FarRubber));
            Stamp(result, part => DrawArm(part, 23f, centerY - 2f, -stroke, FarSuit, FarRubber));
            Stamp(result, part => DrawTank(part, centerY + 6.5f));
            Stamp(result, part => DrawTorso(part, centerY));
            Stamp(result, part => DrawLeg(part, 18f, centerY + 1f, kick, Suit, Rubber));
            Stamp(result, part => DrawAirHose(part, centerY));
            Stamp(result, part => DrawHelmet(part, 36f, headY));
            Stamp(result, part => DrawArm(part, 24f, centerY - 2f, stroke, Suit, Rubber));
            return result;
        }

        private static void DrawLeg(PixelCanvas canvas, float hipX, float hipY, float swing, Color32[] suit, Color32[] rubber)
        {
            var knee = new Vector2(hipX - 7f, hipY + swing * 1.5f);
            var ankle = new Vector2(hipX - 13f, hipY + swing * 3.5f);
            var toe = new Vector2(ankle.x - 3f, ankle.y + swing * 0.8f);

            FillCapsule(canvas, new Vector2(hipX, hipY), knee, 2.3f, suit);
            FillCapsule(canvas, knee + new Vector2(1f, 0f), knee - new Vector2(1f, 0f), 2.1f, Patch);
            FillCapsule(canvas, knee, ankle, 1.9f, suit);
            FillCapsule(canvas, ankle, toe, 2f, rubber);
        }

        private static void DrawArm(PixelCanvas canvas, float shoulderX, float shoulderY, float stroke, Color32[] suit, Color32[] rubber)
        {
            var shoulder = new Vector2(shoulderX, shoulderY);
            var elbow = new Vector2(shoulderX + 3f + stroke, shoulderY - 4f);
            var wrist = new Vector2(shoulderX + 8f + stroke * 2f, shoulderY - 5f - stroke * 0.5f);

            FillCapsule(canvas, shoulder, elbow, 1.9f, suit);
            FillCapsule(canvas, elbow, wrist, 1.6f, suit);
            FillCapsule(canvas, wrist, wrist + new Vector2(1.5f, 0f), 1.9f, rubber);
        }

        private static void DrawTank(PixelCanvas canvas, float y)
        {
            FillCapsule(canvas, new Vector2(15f, y), new Vector2(27f, y), 3.3f, Metal);
            canvas.FillRect(18, Mathf.RoundToInt(y) - 3, 1, 7, Leather);
            canvas.FillRect(24, Mathf.RoundToInt(y) - 3, 1, 7, Leather);
            canvas.FillRect(30, Mathf.RoundToInt(y) - 1, 2, 3, Metal[2]);
            canvas.FillRect(31, Mathf.RoundToInt(y) + 2, 1, 2, Ink);
            canvas.FillRect(29, Mathf.RoundToInt(y) + 3, 5, 1, Ink);
        }

        private static void DrawTorso(PixelCanvas canvas, float y)
        {
            FillCapsule(canvas, new Vector2(18f, y), new Vector2(27f, y), 4.2f, Suit);

            int center = Mathf.RoundToInt(y);
            canvas.FillRect(17, center - 4, 2, 9, Leather);
            canvas.FillRect(23, center - 4, 1, 9, Leather);
            canvas.FillRect(17, center - 1, 2, 3, Brass[1]);
            canvas.Set(23, center, Brass[1]);
        }

        private static void DrawAirHose(PixelCanvas canvas, float y)
        {
            float top = y + 10f;

            for (int i = 0; i <= 10; i++)
            {
                float t = i / 10f;
                int x = Mathf.RoundToInt(Mathf.Lerp(26f, 34f, t));
                int py = Mathf.RoundToInt(top + Mathf.Sin(t * Mathf.PI) * 3f - t * 3f);
                canvas.Set(x, py, i % 2 == 0 ? Rope : RopeDark);
            }
        }

        private static void DrawHelmet(PixelCanvas canvas, float x, float y)
        {
            FillCapsule(canvas, new Vector2(x - 7f, y - 5f), new Vector2(x - 7f, y + 5f), 1.6f, Brass);
            FillSphere(canvas, x, y, 7.8f, Brass);

            // Parafusos da gola e válvula no topo do capacete.
            for (int offset = -4; offset <= 4; offset += 4)
            {
                canvas.Set(Mathf.RoundToInt(x - 7f), Mathf.RoundToInt(y) + offset, Brass[2]);
            }

            canvas.FillRect(Mathf.RoundToInt(x) - 2, Mathf.RoundToInt(y) + 8, 4, 2, Brass[2]);
            canvas.FillRect(Mathf.RoundToInt(x) - 1, Mathf.RoundToInt(y) + 10, 2, 1, Ink);

            DrawPorthole(canvas, x + 2f, y, 4.4f);
        }

        /// <summary>Visor redondo: aro de cobre, vidro com reflexo e uma pequena grade.</summary>
        private static void DrawPorthole(PixelCanvas canvas, float x, float y, float radius)
        {
            FillSphere(canvas, x, y, radius, Copper);
            FillDisc(canvas, x, y, radius - 1.3f, Glass);

            int px = Mathf.RoundToInt(x);
            int py = Mathf.RoundToInt(y);

            canvas.FillRect(px - 2, py - 2, 3, 1, GlassDark);
            canvas.FillRect(px + 1, py - 1, 1, 2, GlassDark);
            canvas.FillRect(px - 1, py + 1, 1, 1, Shine);
            canvas.Set(px - 2, py, Shine);
        }

        /// <summary>De frente, em pé no barco antes do mergulho.</summary>
        private static PixelCanvas DrawStanding()
        {
            var result = new PixelCanvas(26, 34);

            Stamp(result, DrawStandingTank);
            Stamp(result, part => DrawStandingLeg(part, 9));
            Stamp(result, part => DrawStandingLeg(part, 14));
            Stamp(result, DrawStandingBody);
            Stamp(result, part => DrawStandingArm(part, 5f));
            Stamp(result, part => DrawStandingArm(part, 19f));
            Stamp(result, DrawStandingHelmet);
            return result;
        }

        private static void DrawStandingTank(PixelCanvas canvas)
        {
            FillCapsule(canvas, new Vector2(21f, 15f), new Vector2(21f, 25f), 2.6f, Metal);
            canvas.FillRect(20, 28, 3, 2, Ink);

            for (int i = 0; i <= 8; i++)
            {
                float t = i / 8f;
                int x = Mathf.RoundToInt(Mathf.Lerp(20f, 16f, t));
                int y = Mathf.RoundToInt(23f + Mathf.Sin(t * Mathf.PI) * 2f - t * 2f);
                canvas.Set(x, y, i % 2 == 0 ? Rope : RopeDark);
            }
        }

        private static void DrawStandingLeg(PixelCanvas canvas, int x)
        {
            canvas.FillRect(x, 4, 4, 9, Suit[0]);
            canvas.FillRect(x + 3, 4, 1, 9, Suit[2]);
            canvas.FillRect(x, 4, 1, 9, Suit[1]);
            canvas.FillRect(x, 7, 4, 2, Patch[0]);
            canvas.FillRect(x - 1, 1, 5, 3, Rubber[0]);
            canvas.FillRect(x - 1, 3, 5, 1, Rubber[1]);
            canvas.FillRect(x - 1, 0, 5, 1, Rubber[2]);
        }

        private static void DrawStandingBody(PixelCanvas canvas)
        {
            canvas.FillRect(7, 11, 12, 11, Suit[0]);
            canvas.FillRect(7, 11, 2, 11, Suit[1]);
            canvas.FillRect(17, 11, 2, 11, Suit[2]);

            canvas.DrawLine(8f, 21f, 13f, 14f, Leather);
            canvas.DrawLine(18f, 21f, 13f, 14f, Leather);
            canvas.FillRect(7, 12, 12, 2, Leather);
            canvas.FillRect(12, 12, 3, 3, Brass[1]);
            canvas.FillRect(13, 13, 1, 1, Brass[2]);

            canvas.FillRect(8, 21, 10, 2, Brass[2]);
            canvas.FillRect(8, 22, 10, 1, Brass[0]);
        }

        private static void DrawStandingArm(PixelCanvas canvas, float x)
        {
            FillCapsule(canvas, new Vector2(x, 20f), new Vector2(x, 13f), 2.1f, Suit);
            FillCapsule(canvas, new Vector2(x, 11f), new Vector2(x, 10f), 2.1f, Rubber);
        }

        private static void DrawStandingHelmet(PixelCanvas canvas)
        {
            FillSphere(canvas, 13f, 26f, 7.2f, Brass);
            canvas.FillRect(12, 33, 3, 1, Brass[2]);
            DrawPorthole(canvas, 13f, 26f, 4.4f);
            canvas.Set(5, 26, Brass[2]);
            canvas.Set(20, 26, Brass[2]);
        }

        private static Color32[] Shades(string baseHex, string lightHex, string darkHex)
        {
            return new[] { PixelCanvas.Hex(baseHex), PixelCanvas.Hex(lightHex), PixelCanvas.Hex(darkHex) };
        }

        /// <summary>Desenha uma peça em uma camada separada, contorna e cola no resultado.</summary>
        private static void Stamp(PixelCanvas target, Action<PixelCanvas> draw)
        {
            var part = new PixelCanvas(target.Width, target.Height);
            draw(part);
            part.Outline(Ink);
            target.Stamp(part);
        }

        /// <summary>Cápsula com luz na parte de cima e sombra na de baixo.</summary>
        private static void FillCapsule(PixelCanvas canvas, Vector2 from, Vector2 to, float radius, Color32[] shades)
        {
            Vector2 segment = to - from;
            float lengthSquared = Mathf.Max(segment.sqrMagnitude, 0.0001f);

            for (int y = 0; y < canvas.Height; y++)
            {
                for (int x = 0; x < canvas.Width; x++)
                {
                    var point = new Vector2(x, y);
                    Vector2 closest = from + segment * Mathf.Clamp01(Vector2.Dot(point - from, segment) / lengthSquared);
                    float distance = Vector2.Distance(point, closest);

                    if (distance <= radius)
                    {
                        float height = (point.y - closest.y) / radius;
                        canvas.Set(x, y, height > 0.45f ? shades[1] : height < -0.45f ? shades[2] : shades[0]);
                    }
                }
            }
        }

        /// <summary>Círculo com aparência de esfera, iluminado de cima para baixo.</summary>
        private static void FillSphere(PixelCanvas canvas, float centerX, float centerY, float radius, Color32[] shades)
        {
            for (int y = 0; y < canvas.Height; y++)
            {
                for (int x = 0; x < canvas.Width; x++)
                {
                    float dx = (x - centerX) / radius;
                    float dy = (y - centerY) / radius;

                    if (dx * dx + dy * dy > 1f)
                    {
                        continue;
                    }

                    // Luz e sombra seguem a borda do círculo, o que mantém o formato arredondado.
                    float highlightX = dx + 0.3f;
                    float highlightY = dy - 0.45f;
                    float shadowX = dx - 0.1f;
                    float shadowY = dy + 0.2f;
                    bool isLit = highlightX * highlightX + highlightY * highlightY < 0.2f;
                    bool isShadow = shadowX * shadowX + shadowY * shadowY > 0.72f && dy < 0.1f;

                    canvas.Set(x, y, isLit ? shades[1] : isShadow ? shades[2] : shades[0]);
                }
            }
        }

        private static void FillDisc(PixelCanvas canvas, float centerX, float centerY, float radius, Color32 color)
        {
            for (int y = 0; y < canvas.Height; y++)
            {
                for (int x = 0; x < canvas.Width; x++)
                {
                    float dx = x - centerX;
                    float dy = y - centerY;

                    if (dx * dx + dy * dy <= radius * radius)
                    {
                        canvas.Set(x, y, color);
                    }
                }
            }
        }
    }
}
