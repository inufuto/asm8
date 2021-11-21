using Inu.Language;

namespace Inu.Assembler.Mos6502
{
    internal class Tokenizer : Inu.Assembler.Tokenizer
    {
        public Tokenizer()
        {
            ReservedWord.AddWords(Keyword.Words);
        }
    }
}
