using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace CodicExtension.Model
{
    public class Word
    {
        public string TranslatedText { get; set; }
        public bool Successful { get; set; }
        public List<string> Candidates { get; set; }
    }
}