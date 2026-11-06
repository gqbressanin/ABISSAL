using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Abissal.UI
{
    /// <summary>Escreve uma linha de texto na interface usando as letras em pixel do jogo.</summary>
    [RequireComponent(typeof(RectTransform))]
    public class PixelText : MonoBehaviour
    {
        public enum Alignment
        {
            Left,
            Center,
            Right
        }

        [SerializeField] private PixelFontData font;
        [SerializeField] private string text;
        [SerializeField, Min(1)] private int pixelScale = 4;
        [SerializeField] private Color color = Color.white;
        [SerializeField] private Alignment alignment = Alignment.Center;

        private readonly List<Image> letters = new List<Image>();
        private string renderedText;

        public Color Color
        {
            get => color;
            set
            {
                color = value;

                foreach (Image letter in letters)
                {
                    letter.color = value;
                }
            }
        }

        private void Awake()
        {
            SetText(text);
        }

        public void SetText(string value)
        {
            if (value == renderedText)
            {
                return;
            }

            renderedText = value;
            text = value;
            string upperText = (value ?? string.Empty).ToUpperInvariant();

            EnsureLetterCount(upperText.Length);

            float advance = font.Advance * pixelScale;
            float start = StartPosition(upperText.Length * advance);

            for (int i = 0; i < letters.Count; i++)
            {
                Sprite glyph = i < upperText.Length ? font.GetGlyph(upperText[i]) : null;
                Image letter = letters[i];
                letter.enabled = glyph != null;

                if (glyph != null)
                {
                    letter.sprite = glyph;
                    letter.rectTransform.sizeDelta = glyph.rect.size * pixelScale;
                    letter.rectTransform.anchoredPosition = new Vector2(start + i * advance, 0f);
                }
            }
        }

        private float StartPosition(float totalWidth)
        {
            switch (alignment)
            {
                case Alignment.Center:
                    return -totalWidth * 0.5f;
                case Alignment.Right:
                    return -totalWidth;
                default:
                    return 0f;
            }
        }

        private void EnsureLetterCount(int count)
        {
            while (letters.Count < count)
            {
                var letterObject = new GameObject("Letra", typeof(RectTransform), typeof(Image));
                letterObject.transform.SetParent(transform, false);

                var image = letterObject.GetComponent<Image>();
                image.raycastTarget = false;
                image.color = color;

                RectTransform rect = image.rectTransform;
                rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0f, 0.5f);
                letters.Add(image);
            }
        }
    }
}
