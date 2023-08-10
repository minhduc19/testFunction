using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirstFunction
{
    public class WordPair
    {
        public WordPair(string _word, string _furigana)
        {
            word = _word;
            if (_word == _furigana) { furigana = ""; }
            if (_word != _furigana) { furigana = _furigana; }
        }
        public string word { get; set; }
        public string furigana { get; set; }
    }
}
