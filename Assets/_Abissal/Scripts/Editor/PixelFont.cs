using System.Collections.Generic;

namespace Abissal.EditorTools
{
    /// <summary>Letras em pixel (5x7) usadas para gerar o título do jogo como imagem.</summary>
    public static class PixelFont
    {
        public const int GlyphWidth = 5;
        public const int GlyphHeight = 7;

        private const int AccentRows = 2;

        private static readonly string[] Blank = { ".....", ".....", ".....", ".....", ".....", ".....", "....." };
        private static readonly string[] Acute = { "...#.", "..#.." };
        private static readonly string[] Grave = { ".#...", "..#.." };
        private static readonly string[] Circumflex = { "..#..", ".#.#." };
        private static readonly string[] Tilde = { ".##.#", "#..#." };

        private static readonly Dictionary<char, string[]> Glyphs = new Dictionary<char, string[]>
        {
            { 'A', new[] { ".###.", "#...#", "#...#", "#####", "#...#", "#...#", "#...#" } },
            { 'B', new[] { "####.", "#...#", "#...#", "####.", "#...#", "#...#", "####." } },
            { 'C', new[] { ".###.", "#...#", "#....", "#....", "#....", "#...#", ".###." } },
            { 'D', new[] { "####.", "#...#", "#...#", "#...#", "#...#", "#...#", "####." } },
            { 'E', new[] { "#####", "#....", "#....", "####.", "#....", "#....", "#####" } },
            { 'T', new[] { "#####", "..#..", "..#..", "..#..", "..#..", "..#..", "..#.." } },
            { 'V', new[] { "#...#", "#...#", "#...#", "#...#", "#...#", ".#.#.", "..#.." } },
            { 'X', new[] { "#...#", "#...#", ".#.#.", "..#..", ".#.#.", "#...#", "#...#" } },
            { '-', new[] { ".....", ".....", ".....", "#####", ".....", ".....", "....." } },
            { '.', new[] { ".....", ".....", ".....", ".....", ".....", ".##..", ".##.." } },
            { ',', new[] { ".....", ".....", ".....", ".....", ".##..", ".##..", "..#.." } },
            { ':', new[] { ".....", ".##..", ".##..", ".....", ".##..", ".##..", "....." } },
            { '/', new[] { "....#", "....#", "...#.", "..#..", ".#...", "#....", "#...." } },
            { '%', new[] { "##..#", "##.#.", "...#.", "..#..", ".#...", ".#.##", "#..##" } },
            { '!', new[] { "..#..", "..#..", "..#..", "..#..", "..#..", ".....", "..#.." } },
            { '0', new[] { ".###.", "#...#", "#..##", "#.#.#", "##..#", "#...#", ".###." } },
            { '1', new[] { "..#..", ".##..", "..#..", "..#..", "..#..", "..#..", ".###." } },
            { '2', new[] { ".###.", "#...#", "....#", "...#.", "..#..", ".#...", "#####" } },
            { '3', new[] { "####.", "....#", "....#", ".###.", "....#", "....#", "####." } },
            { '4', new[] { "...#.", "..##.", ".#.#.", "#..#.", "#####", "...#.", "...#." } },
            { '5', new[] { "#####", "#....", "####.", "....#", "....#", "#...#", ".###." } },
            { '6', new[] { ".###.", "#....", "#....", "####.", "#...#", "#...#", ".###." } },
            { '7', new[] { "#####", "....#", "...#.", "..#..", ".#...", ".#...", ".#..." } },
            { '8', new[] { ".###.", "#...#", "#...#", ".###.", "#...#", "#...#", ".###." } },
            { '9', new[] { ".###.", "#...#", "#...#", ".####", "....#", "....#", ".###." } },
            { 'J', new[] { "..###", "...#.", "...#.", "...#.", "...#.", "#..#.", ".##.." } },
            { 'K', new[] { "#...#", "#..#.", "#.#..", "##...", "#.#..", "#..#.", "#...#" } },
            { 'Q', new[] { ".###.", "#...#", "#...#", "#...#", "#.#.#", "#..#.", ".##.#" } },
            { 'W', new[] { "#...#", "#...#", "#...#", "#.#.#", "#.#.#", "##.##", "#...#" } },
            { 'Y', new[] { "#...#", "#...#", ".#.#.", "..#..", "..#..", "..#..", "..#.." } },
            { 'Z', new[] { "#####", "....#", "...#.", "..#..", ".#...", "#....", "#####" } },
            { 'F', new[] { "#####", "#....", "#....", "####.", "#....", "#....", "#...." } },
            { 'G', new[] { ".###.", "#...#", "#....", "#.###", "#...#", "#...#", ".###." } },
            { 'H', new[] { "#...#", "#...#", "#...#", "#####", "#...#", "#...#", "#...#" } },
            { 'I', new[] { "#####", "..#..", "..#..", "..#..", "..#..", "..#..", "#####" } },
            { 'L', new[] { "#....", "#....", "#....", "#....", "#....", "#....", "#####" } },
            { 'M', new[] { "#...#", "##.##", "#.#.#", "#.#.#", "#...#", "#...#", "#...#" } },
            { 'N', new[] { "#...#", "##..#", "##..#", "#.#.#", "#..##", "#..##", "#...#" } },
            { 'O', new[] { ".###.", "#...#", "#...#", "#...#", "#...#", "#...#", ".###." } },
            { 'P', new[] { "####.", "#...#", "#...#", "####.", "#....", "#....", "#...." } },
            { 'R', new[] { "####.", "#...#", "#...#", "####.", "#.#..", "#..#.", "#...#" } },
            { 'S', new[] { ".####", "#....", "#....", ".###.", "....#", "....#", "####." } },
            { 'U', new[] { "#...#", "#...#", "#...#", "#...#", "#...#", "#...#", ".###." } },
            // O Ç tem duas linhas a mais, que descem abaixo da base das outras letras.
            { 'Ç', new[] { ".###.", "#...#", "#....", "#....", "#....", "#...#", ".###.", "..#..", ".##.." } }
        };

        /// <summary>Letras acentuadas: a letra base inteira, com duas linhas de acento acima dela.</summary>
        private static readonly Dictionary<char, (char letter, string[] accent)> AccentedLetters =
            new Dictionary<char, (char, string[])>
            {
                { 'Á', ('A', Acute) },
                { 'À', ('A', Grave) },
                { 'Â', ('A', Circumflex) },
                { 'Ã', ('A', Tilde) },
                { 'É', ('E', Acute) },
                { 'Ê', ('E', Circumflex) },
                { 'Í', ('I', Acute) },
                { 'Ó', ('O', Acute) },
                { 'Ô', ('O', Circumflex) },
                { 'Õ', ('O', Tilde) },
                { 'Ú', ('U', Acute) }
            };

        /// <summary>Todos os caracteres disponíveis, usados para gerar as letras da interface.</summary>
        public static IEnumerable<char> Characters
        {
            get
            {
                foreach (char character in Glyphs.Keys)
                {
                    yield return character;
                }

                foreach (char character in AccentedLetters.Keys)
                {
                    yield return character;
                }
            }
        }

        /// <summary>Quantas linhas o caractere ocupa acima da altura normal das letras.</summary>
        public static int AccentHeight(char character)
        {
            return AccentedLetters.ContainsKey(character) ? AccentRows : 0;
        }

        public static string[] GetGlyph(char character)
        {
            if (AccentedLetters.TryGetValue(character, out (char letter, string[] accent) accented))
            {
                var rows = new List<string>(accented.accent);
                rows.AddRange(Glyphs[accented.letter]);
                return rows.ToArray();
            }

            return Glyphs.TryGetValue(character, out string[] glyph) ? glyph : Blank;
        }
    }
}
