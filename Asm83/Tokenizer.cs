using Inu.Language;

namespace Inu.Assembler.Sm83;

internal class Tokenizer : Inu.Assembler.Tokenizer
{
    public Tokenizer()
    {
        ReservedWord.AddWords(Keyword.Words);
    }
}