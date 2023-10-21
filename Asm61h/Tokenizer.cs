using Inu.Language;

namespace Inu.Assembler.HD61700
{
    internal class Tokenizer : Inu.Assembler.Tokenizer
    {
        private const char HexValueHead = '&';
        public Tokenizer()
        {
            ReservedWord.AddWords(Keyword.Words);
        }

        protected override bool IsHexValueHead(char c)
        {
            return c == HexValueHead;
        }

        protected override int ReadNumericValue()
        {
            if (LastChar == HexValueHead) {
                var c = NextChar();
                if (c is 'H' or 'h') {
                    NextChar();
                    return ReadHexValue();
                }
                ReturnChar('&');
            }
            return ReadDecValue();
        }
    }
}
