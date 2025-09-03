namespace Inu.Assembler.Hd6301;

internal class Assembler(int version) : Mc6801.Assembler(new Tokenizer(version), version)
{
    static Assembler() 
    {
        ImpliedElements.Add(Keyword.XGDX, 0x18);
    }
}