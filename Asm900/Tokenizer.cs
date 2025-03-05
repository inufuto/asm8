using Inu.Language;

namespace Inu.Assembler.Tlcs900;

internal class Tokenizer : Inu.Assembler.Tokenizer
{
    public Tokenizer()
    {
        ReservedWord.AddWords(Keyword.Words);
    }

    protected override bool IsIdentifierElement(char c)
    {
        return base.IsIdentifierElement(c) || c == '\'';
    }
}