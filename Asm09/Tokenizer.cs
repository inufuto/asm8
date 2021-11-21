using Inu.Language;

namespace Inu.Assembler.Mc6809
{
    internal class Tokenizer : Inu.Assembler.Tokenizer
    {

        public Tokenizer()
        {
            ReservedWord.AddWords(Keyword.Words);
        }
    }
}
