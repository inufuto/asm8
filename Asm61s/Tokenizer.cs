using Inu.Language;

namespace Inu.Assembler.SC61860;

internal class Tokenizer : Inu.Assembler.Tokenizer
{
    public Tokenizer()
    {
        ReservedWord.AddWords(Keyword.Words);
    }
}