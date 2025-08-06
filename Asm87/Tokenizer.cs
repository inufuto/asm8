using Inu.Language;

namespace Inu.Assembler.MuCom87;

public class Tokenizer : Inu.Assembler.Tokenizer
{
    public Tokenizer(int version) : base(version)
    {
        ReservedWord.AddWords(Keyword.Words);
    }
}