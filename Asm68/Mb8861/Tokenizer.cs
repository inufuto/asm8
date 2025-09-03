using Inu.Language;
using System;

namespace Inu.Assembler.Mb8861
{
    internal class Tokenizer : Mc6800.Tokenizer
    {
        public Tokenizer(int version) : base(version)
        {
            ReservedWord.AddWords(Keyword.Words);
        }
    }
}
