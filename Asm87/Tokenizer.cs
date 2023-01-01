using Inu.Language;

namespace Inu.Assembler.MuCom87
{
    public class Tokenizer : Inu.Assembler.Tokenizer
    {
        public Tokenizer()
        {
            ReservedWord.AddWords(Keyword.Words);
        }
    }
}
