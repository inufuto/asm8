using Inu.Language;

namespace Inu.Assembler.MuCom87.MuPD7800
{
    internal class Tokenizer : MuCom87.Tokenizer
    {
        public Tokenizer()
        {
            ReservedWord.AddWords(Keyword.Words);
        }
    }
}