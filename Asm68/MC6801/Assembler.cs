using Inu.Language;
using System.Collections.Generic;

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
            ImpliedElements.Add(Keyword.SLP, 0x1a);

            BranchElements.Add(Keyword.BRN, 0x21);
        }

        public Assembler() : this(new Tokenizer())
        { }

        protected Assembler(Inu.Assembler.Tokenizer tokenizer) : base(tokenizer)
        { }

        protected override bool Instruction()
        {
            if (MemoryOperation()) return true;
            return base.Instruction();
        }

        private static readonly Dictionary<int, int> OperationCodes = new Dictionary<int, int>()
        {
            {Keyword.AIM, 0x71},
            {Keyword.OIM, 0x72},
            {Keyword.EIM, 0x75},
            {Keyword.TIM, 0x7b},
        };
        private bool MemoryOperation()
        {
            if (!(LastToken is ReservedWord reservedWord) ||
                !OperationCodes.TryGetValue(reservedWord.Id, out var instruction)) return false;
            NextToken();
            AcceptReservedWord('#');
            var valueToken = LastToken;
            var value = Expression();
            if (value == null) {
                ShowSyntaxError(valueToken);
                return true;
            }
            AcceptReservedWord(',');
            var addressToken = LastToken;
            if (addressToken.IsReservedWord('<') || addressToken.IsReservedWord('>')) {
                //ignore
                addressToken = NextToken();
            }
            var address = Expression();
            if (address == null) {
                ShowSyntaxError(addressToken);
                return true;
            }
            if (LastToken.IsReservedWord(',')) {
                NextToken();
                AcceptReservedWord(Mc6800.Keyword.X);
                WriteByte(instruction - 0x10);
                WriteByte(valueToken, value);
                WriteByte(addressToken, address);
                return true;
            }
            WriteByte(instruction);
            WriteByte(valueToken, value);
            WriteByte(addressToken, address);
            return true;
        }
    }
}
