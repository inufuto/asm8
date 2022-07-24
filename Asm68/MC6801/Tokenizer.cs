using Inu.Language;

namespace Inu.Assembler.Mc6801
{
    internal class Tokenizer : Mc6800.Tokenizer
    {
        public Tokenizer()
        {
            ReservedWord.AddWords(Keyword.Words);
        }
    }
}
