using Inu.Language;
using System.Collections.Generic;

namespace Inu.Assembler.MuCom87.MuPD7800
{
    internal class Assembler : MuCom87.Assembler
    {
        public Assembler() : base(new Tokenizer()) { }
        protected override bool Instruction(ReservedWord reservedWord)
        {
            switch (reservedWord.Id) {
                case MuCom87.Keyword.XRI:
                    ByteOperationImmediate(0x16, 0x64, 0x10);
                    return true;
                case MuCom87.Keyword.ADINC:
                    ByteOperationImmediate(0x26, 0x64, 0x20);
                    return true;
                case MuCom87.Keyword.SUINB:
                    ByteOperationImmediate(0x36, 0x64, 0x30);
                    return true;
                case MuCom87.Keyword.ADI:
                    ByteOperationImmediate(0x46, 0x64, 0x40);
                    return true;
                case MuCom87.Keyword.ACI:
                    ByteOperationImmediate(0x56, 0x64, 0x50);
                    return true;
                case MuCom87.Keyword.SUI:
                    ByteOperationImmediate(0x66, 0x64, 0x60);
                    return true;
                case MuCom87.Keyword.SBI:
                    ByteOperationImmediate(0x76, 0x64, 0x70);
                    return true;
                case MuCom87.Keyword.GTI:
                    ByteOperationImmediate(0x27, 0x64, 0x28);
                    return true;
                case MuCom87.Keyword.LTI:
                    ByteOperationImmediate(0x37, 0x64, 0x38);
                    return true;
                case MuCom87.Keyword.NEI:
                    ByteOperationImmediate(0x67, 0x64, 0x68);
                    return true;
                case MuCom87.Keyword.EQI:
                    ByteOperationImmediate(0x77, 0x64, 0x78);
                    return true;
                case Keyword.XRAW:
                    InstructionWithByte(0x74, 0x90);
                    return true;
                case Keyword.ADDW:
                    InstructionWithByte(0x74, 0xc0);
                    return true;
                case Keyword.ADCW:
                    InstructionWithByte(0x74, 0xd0);
                    return true;
                case Keyword.SUBW:
                    InstructionWithByte(0x74, 0xe0);
                    return true;
                case Keyword.SBBW:
                    InstructionWithByte(0x74, 0xf0);
                    return true;
                case Keyword.ANAW:
                    InstructionWithByte(0x74, 0x88);
                    return true;
                case Keyword.ORAW:
                    InstructionWithByte(0x74, 0x98);
                    return true;
                case Keyword.TABLE:
                    InstructionWithoutOperand(0x21);
                    return true;
                case Keyword.SHCL:
                    InstructionWithoutOperand(0x48, 0x36);
                    return true;
                case Keyword.SHCR:
                    InstructionWithoutOperand(0x48, 0x37);
                    return true;
                case Keyword.CALB:
                    InstructionWithoutOperand(0x63);
                    return true;
                case Keyword.RCL:
                    InstructionWithoutOperand(0x48, 0x32);
                    return true;
                case Keyword.RCR:
                    InstructionWithoutOperand(0x48, 0x33);
                    return true;
                case Keyword.PEN:
                    InstructionWithoutOperand(0x48, 0x2c);
                    return true;
                case Keyword.MVIW:
                    MoveImmediateWorking();
                    return true;
                case Keyword.MVIX:
                    MoveImmediateIndex();
                    return true;
                case Keyword.HLT:
                    InstructionWithoutOperand(0x01);
                    return true;
                case Keyword.EXX:
                    InstructionWithoutOperand(0b00010001);
                    return true;
                case Keyword.EX:
                    InstructionWithoutOperand(0b00010000);
                    return true;
                case Keyword.BLOCK:
                    InstructionWithoutOperand(0b00110001);
                    return true;
                case Keyword.SHAL:
                    InstructionWithoutOperand(0x48, 0x34);
                    return true;
                case Keyword.SHAR:
                    InstructionWithoutOperand(0x48, 0x35);
                    return true;
            }
            return base.Instruction(reservedWord);
        }

        protected override bool SkipInstruction(int id)
        {
            switch (id) {
                case MuCom87.Keyword.ADINC:
                    ByteOperationImmediate(0x26, 0x64, 0x20);
                    return true;
                case MuCom87.Keyword.SUINB:
                    ByteOperationImmediate(0x36, 0x64, 0x30);
                    return true;
                case MuCom87.Keyword.GTI:
                    ByteOperationImmediate(0x27, 0x64, 0x28);
                    return true;
                case MuCom87.Keyword.LTI:
                    ByteOperationImmediate(0x37, 0x64, 0x38);
                    return true;
                case MuCom87.Keyword.NEI:
                    ByteOperationImmediate(0x67, 0x64, 0x68);
                    return true;
                case MuCom87.Keyword.EQI:
                    ByteOperationImmediate(0x77, 0x64, 0x78);
                    return true;
                case Keyword.ONA:
                    ByteOperationA(0x60, 0xC8);
                    return true;
                case Keyword.OFFA:
                    ByteOperationA(0x60, 0xd8);
                    return true;
                case Keyword.ADDNCW:
                    InstructionWithByte(0x74, 0xa0);
                    return true;
                case Keyword.SUBNBW:
                    InstructionWithByte(0x74, 0xb0);
                    return true;
                case Keyword.GTAW:
                    InstructionWithByte(0x74, 0xa8);
                    return true;
                case Keyword.LTAW:
                    InstructionWithByte(0x74, 0xb8);
                    return true;
                case Keyword.ONAW:
                    InstructionWithByte(0x74, 0xc8);
                    return true;
                case Keyword.OFFAW:
                    InstructionWithByte(0x74, 0xd8);
                    return true;
                case Keyword.NEAW:
                    InstructionWithByte(0x74, 0xe8);
                    return true;
                case Keyword.EQAW:
                    InstructionWithByte(0x74, 0xf8);
                    return true;
                case Keyword.BIT:
                    BitTest();
                    return true;
                case Keyword.SKC:
                    InstructionWithoutOperand(0x48, 0x0a);
                    return true;
                case Keyword.SKZ:
                    InstructionWithoutOperand(0x48, 0x0c);
                    return true;
                case Keyword.SKIT:
                    InstructionWithInterrupt(0x48, 0x00);
                    return true;
            }
            return base.SkipInstruction(id);
        }

        private void BitTest()
        {
            var leftToken = NextToken();
            var bit = Expression();
            if (bit == null || bit.Type != AddressType.Const) {
                bit = Address.Default;
            }
            if (bit.Value < 0 || bit.Value > 7) {
                ShowOutOfRange(leftToken, bit.Value);
            }
            WriteByte(0x58 | bit.Value & 7);
            AcceptReservedWord(',');
            WriteByteExpression();
        }
        private void ByteOperationA(int code1, int code2)
        {
            var leftToken = NextToken();
            if (LastToken.IsReservedWord(Keyword.A)) {
                NextToken();
                AcceptReservedWord(',');
                var rightRegister = Register();
                if (rightRegister != null) {
                    if (LastToken.IsReservedWord(Keyword.V) || LastToken.IsReservedWord(Keyword.A)) {
                        ShowInvalidRegister(leftToken);
                    }
                    WriteByte(code1);
                    WriteByte(code2 | rightRegister.Value);
                    return;
                }
            }
            if (Register() != null) {
                AcceptReservedWord(',');
                Register();
                ShowInvalidRegister(leftToken);
                return;
            }
            ShowSyntaxError(LastToken);
        }
        private void MoveImmediateWorking()
        {
            NextToken();
            WriteByte(0b01110001);
            WriteByteExpression();
            AcceptReservedWord(',');
            WriteByteExpression();
        }
        private void MoveImmediateIndex()
        {


            Dictionary<int, int> codes = new Dictionary<int, int>()
            {
                {Keyword.B,0x49},
                {Keyword.D,0x4a},
                {Keyword.H,0x4b},
            };
            var leftToken = NextToken();
            var register = RegisterPair();
            if (register == null) {
                ShowSyntaxError(LastToken);
                register = 0;
            }
            AcceptReservedWord(',');
            WriteByte(0x48 | register.Value);
            WriteByteExpression();
        }
    }
}
