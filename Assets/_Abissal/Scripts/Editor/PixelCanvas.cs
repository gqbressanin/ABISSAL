using System.IO;
using UnityEngine;

namespace Abissal.EditorTools
{
    /// <summary>Tela de pixels usada para desenhar os sprites. A origem (0,0) fica no canto inferior esquerdo.</summary>
    public class PixelCanvas
    {
        private readonly Color32[] pixels;

        public int Width { get; }
        public int Height { get; }

        public PixelCanvas(int width, int height)
        {
            Width = width;
            Height = height;
            pixels = new Color32[width * height];
        }

        public static Color32 Hex(string hex, byte alpha = 255)
        {
            ColorUtility.TryParseHtmlString("#" + hex, out Color color);
            Color32 result = color;
            result.a = alpha;
            return result;
        }

        public bool Contains(int x, int y) => x >= 0 && y >= 0 && x < Width && y < Height;

        public Color32 Get(int x, int y) => Contains(x, y) ? pixels[y * Width + x] : default;

        public void Set(int x, int y, Color32 color)
        {
            if (Contains(x, y))
            {
                pixels[y * Width + x] = color;
            }
        }

        public void FillRect(int x, int y, int width, int height, Color32 color)
        {
            for (int py = y; py < y + height; py++)
            {
                for (int px = x; px < x + width; px++)
                {
                    Set(px, py, color);
                }
            }
        }

        public void DrawLine(float x0, float y0, float x1, float y1, Color32 color)
        {
            int steps = Mathf.CeilToInt(Mathf.Max(Mathf.Abs(x1 - x0), Mathf.Abs(y1 - y0))) + 1;

            for (int i = 0; i <= steps; i++)
            {
                float t = i / (float)steps;
                Set(Mathf.RoundToInt(Mathf.Lerp(x0, x1, t)), Mathf.RoundToInt(Mathf.Lerp(y0, y1, t)), color);
            }
        }

        /// <summary>Desenha linhas de texto em que cada caractere é uma cor. A primeira linha fica no topo.</summary>
        public void DrawRows(int x, int topY, string[] rows, System.Func<char, Color32?> palette)
        {
            for (int row = 0; row < rows.Length; row++)
            {
                for (int column = 0; column < rows[row].Length; column++)
                {
                    Color32? color = palette(rows[row][column]);

                    if (color.HasValue)
                    {
                        Set(x + column, topY - row, color.Value);
                    }
                }
            }
        }

        /// <summary>Pinta de contorno todo pixel vazio encostado em um pixel preenchido.</summary>
        public void Outline(Color32 color)
        {
            var source = (Color32[])pixels.Clone();

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    if (source[y * Width + x].a != 0)
                    {
                        continue;
                    }

                    if (IsFilled(source, x - 1, y) || IsFilled(source, x + 1, y) ||
                        IsFilled(source, x, y - 1) || IsFilled(source, x, y + 1))
                    {
                        Set(x, y, color);
                    }
                }
            }
        }

        public void Save(string assetPath)
        {
            var texture = new Texture2D(Width, Height, TextureFormat.RGBA32, false);
            texture.SetPixels32(pixels);
            texture.Apply();

            Directory.CreateDirectory(Path.GetDirectoryName(assetPath));
            File.WriteAllBytes(assetPath, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
        }

        private bool IsFilled(Color32[] source, int x, int y)
        {
            return Contains(x, y) && source[y * Width + x].a != 0;
        }
    }
}
