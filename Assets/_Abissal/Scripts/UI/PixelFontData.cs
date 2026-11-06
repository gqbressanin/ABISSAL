using System.Collections.Generic;
using UnityEngine;

namespace Abissal.UI
{
    /// <summary>Lista de letras em pixel do jogo. O arquivo é gerado pelo menu ABISSAL > Gerar arte-base.</summary>
    public class PixelFontData : ScriptableObject
    {
        [SerializeField] private string characters;
        [SerializeField] private Sprite[] glyphs;
        [SerializeField, Min(1)] private int advance = 6;

        private Dictionary<char, Sprite> lookup;

        /// <summary>Distância, em pixels da arte, entre o início de uma letra e o da próxima.</summary>
        public int Advance => advance;

        public void Configure(string newCharacters, Sprite[] newGlyphs, int newAdvance)
        {
            characters = newCharacters;
            glyphs = newGlyphs;
            advance = newAdvance;
            lookup = null;
        }

        /// <summary>Retorna a letra, ou null para espaços e caracteres inexistentes.</summary>
        public Sprite GetGlyph(char character)
        {
            if (lookup == null)
            {
                lookup = new Dictionary<char, Sprite>();

                for (int i = 0; i < glyphs.Length; i++)
                {
                    lookup[characters[i]] = glyphs[i];
                }
            }

            return lookup.TryGetValue(character, out Sprite glyph) ? glyph : null;
        }
    }
}
