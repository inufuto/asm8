using Inu.Language;

namespace Inu.Assembler.Hd6301
{
    internal class Tokenizer : Mc6801.Tokenizer
    {
        public Tokenizer()
        {
            ReservedWord.AddWords(Keyword.Words);
        }
    }
}
