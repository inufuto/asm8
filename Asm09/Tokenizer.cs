﻿using Inu.Language;

namespace Inu.Assembler.Mc6809;

internal class Tokenizer : Inu.Assembler.Tokenizer
{
    public Tokenizer(int version) : base(version)
    {
        ReservedWord.AddWords(Keyword.Words);
    }
}