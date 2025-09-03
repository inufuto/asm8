using Inu.Language;

namespace Inu.Assembler.Wdc65c02;

internal class Tokenizer : Mos6502.Tokenizer
{
    public Tokenizer(int version) : base(version)
    {
        ReservedWord.AddWords(Keyword.Words);
    }
}