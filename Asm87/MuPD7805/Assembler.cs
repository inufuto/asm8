using Inu.Language;

namespace Inu.Assembler.MuCom87.MuPD7805
{
    internal class Assembler : MuCom87.Assembler
    {
        protected override bool Instruction(ReservedWord reservedWord)
        {
            switch (reservedWord.Id) {
                case Keyword.XRI:
                    ByteOperationImmediateA(0x16);
                    return true;
                case Keyword.ADINC:
                    ByteOperationImmediateA(0x26);
                    return true;
                case Keyword.SUINB:
                    ByteOperationImmediateA(0x36);
                    return true;
                case Keyword.ADI:
                    ByteOperationImmediateA(0x46);
                    return true;
                case Keyword.ACI:
                    ByteOperationImmediateA(0x56);
                    return true;
                case Keyword.SUI:
                    ByteOperationImmediateA(0x66);
                    return true;
                case Keyword.SBI:
                    ByteOperationImmediateA(0x76);
                    return true;
                case Keyword.GTI:
                    ByteOperationImmediateA(0x27);
                    return true;
                case Keyword.LTI:
                    ByteOperationImmediateA(0x37);
                    return true;
                case Keyword.NEI:
                    ByteOperationImmediateA(0x67);
                    return true;
                case Keyword.EQI:
                    ByteOperationImmediateA(0x77);
                    return true;
            }
            return base.Instruction(reservedWord);
        }

        protected override bool SkipInstruction(int id)
        {
            switch (id) {
                case Keyword.ADINC:
                    ByteOperationImmediateA(0x26);
                    return true;
                case Keyword.SUINB:
                    ByteOperationImmediateA(0x36);
                    return true;
                case Keyword.GTI:
                    ByteOperationImmediateA(0x27);
                    return true;
                case Keyword.LTI:
                    ByteOperationImmediateA(0x37);
                    return true;
                case Keyword.NEI:
                    ByteOperationImmediateA(0x67);
                    return true;
                case Keyword.EQI:
                    ByteOperationImmediateA(0x77);
                    return true;
            }
            return base.SkipInstruction(id);
        }

        private void ByteOperationImmediateA(int code)
        {
            var leftToken = NextToken();
            if (LastToken.IsReservedWord(Keyword.A)) {
                NextToken();
                AcceptReservedWord(',');
                WriteByte(code);
                WriteByteExpression();
                return;
            }
            ShowInvalidRegister(leftToken);
        }
    }
}
