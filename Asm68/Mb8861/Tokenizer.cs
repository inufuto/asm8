using Inu.Language;

namespace Inu.Assembler.Mb8861
{
    internal class Tokenizer : Mc6800.Tokenizer
    {
        public Tokenizer()
        {
            ReservedWord.AddWords(Keyword.Words);
        }
    }
}
