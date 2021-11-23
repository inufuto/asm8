using Inu.Language;

namespace Inu.Assembler.MuCom87
{
    internal class Tokenizer : Inu.Assembler.Tokenizer
    {
        public Tokenizer()
        {
            ReservedWord.AddWords(Keyword.Words);
        }
    }
}
