using Inu.Language;

namespace Inu.Assembler.Sm8521;

internal class Tokenizer : Inu.Assembler.Tokenizer
{
    public Tokenizer()
    {
        ReservedWord.AddWords(Keyword.Words);
    }

    protected override Token GetToken(char c, SourcePosition position)
    {
        var upper = char.ToUpper(c);
        if (upper == 'R') {
            var nextChar1 = NextChar();
            if (char.ToUpper(nextChar1) == 'R') {
                var nextChar2 = NextChar();
                if (char.IsDigit(nextChar2)) {
                    return new ReservedWord(position, Keyword.RR);
                }
                ReturnChar(nextChar1);
            }
            if (char.IsDigit(nextChar1)) {
                return new ReservedWord(position, upper);
            }

            if (char.IsAsciiLetter(nextChar1)) {
                ReturnChar(c);
            }
            else {
                return new ReservedWord(position, 'R');
            }
        }
        return base.GetToken(c, position);
    }

    protected override bool IsIdentifierHead(char c)
    {
        return c != '@' && base.IsIdentifierHead(c);
    }
}