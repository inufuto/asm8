using Inu.Language;

namespace Inu.Assembler.Mc6801;

internal class Tokenizer : Mc6800.Tokenizer
{
    public Tokenizer(int version) : base(version)
    {
        ReservedWord.AddWords(Keyword.Words);
    }
}