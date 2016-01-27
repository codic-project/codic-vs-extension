using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace CodicExtension.Model
{
    public class Translation
    {
        public string TranslatedText { get; set; }
        public List<Word> Words { get; set; }
    }
}