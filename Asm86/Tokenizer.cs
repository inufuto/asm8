using Inu.Language;

namespace Inu.Assembler.I8086;

internal class Tokenizer : Inu.Assembler.Tokenizer
{
    public Tokenizer(int version) : base(version)
    {
        ReservedWord.AddWords(Keyword.Words);
    }
}