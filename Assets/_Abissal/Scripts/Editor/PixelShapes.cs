using System;
using UnityEngine;

namespace Abissal.EditorTools
{
    /// <summary>Formas básicas com sombra, usadas para desenhar o mergulhador e as criaturas.</summary>
    public static class PixelShapes
    {
        /// <summary>Trio de cores de uma peça: base, luz e sombra.</summary>
        public static Color32[] Shades(string baseHex, string lightHex, string darkHex)
        {
            return new[] { PixelCanvas.Hex(baseHex), PixelCanvas.Hex(lightHex), PixelCanvas.Hex(darkHex) };
        }

        /// <summary>Desenha uma peça em uma camada separada, contorna e cola no resultado.</summary>
        public static void Layer(PixelCanvas target, Color32 outline, Action<PixelCanvas> draw)
        {
            var part = new PixelCanvas(target.Width, target.Height);
            draw(part);
            part.Outline(outline);
            target.Stamp(part);
        }

        /// <summary>Cápsula com luz na parte de cima e sombra na de baixo.</summary>
        public static void FillCapsule(PixelCanvas canvas, Vector2 from, Vector2 to, float radius, Color32[] shades)
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
        public static void FillSphere(PixelCanvas canvas, float centerX, float centerY, float radius, Color32[] shades)
        {
            FillEllipse(canvas, centerX, centerY, radius, radius, shades);
        }

        public static void FillEllipse(PixelCanvas canvas, float centerX, float centerY, float radiusX, float radiusY, Color32[] shades)
        {
            for (int y = 0; y < canvas.Height; y++)
            {
                for (int x = 0; x < canvas.Width; x++)
                {
                    float dx = (x - centerX) / radiusX;
                    float dy = (y - centerY) / radiusY;

                    if (dx * dx + dy * dy > 1f)
                    {
                        continue;
                    }

                    // Luz e sombra seguem a borda, o que mantém o formato arredondado.
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

        public static void FillDisc(PixelCanvas canvas, float centerX, float centerY, float radius, Color32 color)
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

        /// <summary>Preenche um polígono, usado principalmente para nadadeiras.</summary>
        public static void FillPolygon(PixelCanvas canvas, Color32 color, params Vector2[] points)
        {
            for (int y = 0; y < canvas.Height; y++)
            {
                for (int x = 0; x < canvas.Width; x++)
                {
                    if (IsInsidePolygon(x, y, points))
                    {
                        canvas.Set(x, y, color);
                    }
                }
            }
        }

        /// <summary>
        /// Ondula o desenho na vertical: quanto mais perto da cauda (esquerda), maior o movimento.
        /// É o que faz o corpo dos peixes parecer flexível ao nadar.
        /// </summary>
        public static PixelCanvas Wave(PixelCanvas source, float amplitude, float phase, float length)
        {
            var result = new PixelCanvas(source.Width, source.Height);

            for (int x = 0; x < source.Width; x++)
            {
                float tail = Mathf.Clamp01(1f - x / length);
                int shift = Mathf.RoundToInt(Mathf.Sin(phase + x * 0.22f) * amplitude * tail * tail);

                for (int y = 0; y < source.Height; y++)
                {
                    Color32 pixel = source.Get(x, y);

                    if (pixel.a != 0)
                    {
                        result.Set(x, y + shift, pixel);
                    }
                }
            }

            return result;
        }

        private static bool IsInsidePolygon(float x, float y, Vector2[] points)
        {
            bool inside = false;

            for (int i = 0, j = points.Length - 1; i < points.Length; j = i++)
            {
                bool crosses = (points[i].y > y) != (points[j].y > y);

                if (crosses && x < (points[j].x - points[i].x) * (y - points[i].y) / (points[j].y - points[i].y) + points[i].x)
                {
                    inside = !inside;
                }
            }

            return inside;
        }
    }
}
