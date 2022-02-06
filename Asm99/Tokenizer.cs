using Inu.Language;

namespace Inu.Assembler.Tms99
{
    internal class Tokenizer : Inu.Assembler.Tokenizer
    {
        private const char HexValueHead = '>';
        public Tokenizer()
        {
            ReservedWord.AddWords(Keyword.Words);
        }

        protected override bool IsIdentifierHead(char c)
        {
            return c != '@' && base.IsIdentifierHead(c);
        }

        protected override bool IsHexValueHead(char c)
        {
            return c == HexValueHead || base.IsHexValueHead(c);
        }
    }
}