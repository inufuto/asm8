using Inu.Language;

namespace Inu.Assembler.I8080
{
    internal class Tokenizer : Inu.Assembler.Tokenizer
    {
        public Tokenizer()
        {
            ReservedWord.AddWords(Keyword.Words);
        }
    }
}
