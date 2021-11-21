using Inu.Language;

namespace Inu.Assembler.Z80
{
    internal class Tokenizer : Inu.Assembler.Tokenizer
    {
        public Tokenizer()
        {
            ReservedWord.AddWords(Keyword.Words);
        }
    }
}
