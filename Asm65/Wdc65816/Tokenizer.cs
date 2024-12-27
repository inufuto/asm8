using Inu.Language;

namespace Inu.Assembler.Wdc65816;

internal class Tokenizer : Mos6502.Tokenizer
{
    public Tokenizer()
    {
        ReservedWord.AddWords(Keyword.Words);
    }
}