namespace Inu.Assembler.Mc6801
{
    internal class Assembler : Mc6800.Assembler
    {
        static Assembler()
        {
            FourModeElements.Add(Keyword.ADDD, new FourModeElement(0xC3, true, true));
            FourModeElements.Add(Keyword.LDD, new FourModeElement(0xCC, true, true));
            FourModeElements.Add(Keyword.STD, new FourModeElement(0xCD, false, true));
            FourModeElements.Add(Keyword.SUBD, new FourModeElement(0x83, true, true));

            ImpliedElements.Add(Keyword.MUL, 0x3D);
            ImpliedElements.Add(Keyword.ASLD, 0x05);
            ImpliedElements.Add(Keyword.LSRD, 0x04);
            ImpliedElements.Add(Keyword.ABX, 0x3a);
            ImpliedElements.Add(Keyword.PSHX, 0x3c);
            ImpliedElements.Add(Keyword.PULX, 0x38);
            ImpliedElements.Add(Keyword.XGDX, 0x18);
            ImpliedElements.Add(Keyword.SLP, 0x1a);

            BranchElements.Add(Keyword.BRN, 0x21);
        }

        public Assembler() : base(new Tokenizer())
        {
        }
    }
}
