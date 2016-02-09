using System;

namespace CodicExtension.Model
{
    public class LetterCase
    {
        public string Id { get; }
        public string Name { get; }
        public string ShortName { get; }

        private static LetterCase[] ENTRIES = {
            new LetterCase("pascal", "PascalCase", "Aa"),
            new LetterCase("camel", "camelCase", "aA"),
            new LetterCase("lower underscore", "snake_case (小文字)", "a_a"),
            new LetterCase("upper underscore", "SNAKE_CASE (大文字)", "A_A"),
            new LetterCase("hyphen", "ハイフネーション", "a-a"),
            new LetterCase("", "変換なし", "a a"),
        };

        public static LetterCase[] Enumulate()
        {
            return ENTRIES;
        }

        public static LetterCase ValueOf(string id, LetterCase default_)
        {
            for (int i = 0; i < ENTRIES.Length; i++)
            {
                if (ENTRIES[i].Id.Equals(id))
                {
                    return ENTRIES[i];
                }
            }
            return default_;
        }

        private LetterCase(string id, string name, string shortName)
        {
            this.Id = id;
            this.Name = name;
            this.ShortName = shortName;
        }
    }
}
