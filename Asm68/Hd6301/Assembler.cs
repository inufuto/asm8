namespace Inu.Assembler.Hd6301
{

    internal class Assembler : Mc6801.Assembler
    {
        static Assembler()
        {
            ImpliedElements.Add(Keyword.XGDX, 0x18);
        }

        public Assembler() : base(new Tokenizer())
        {
        }
    }
}
