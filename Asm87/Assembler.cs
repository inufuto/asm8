using System.Collections.Generic;
using System.Diagnostics;
using Inu.Language;

namespace Inu.Assembler.MuCom87
{
    public abstract class Assembler : LittleEndianAssembler
    {
        protected Assembler() : base(new Tokenizer()) { }

        protected Assembler(Tokenizer tokenizer) : base(tokenizer) { }

        protected void WriteByteExpression()
        {
            var token = LastToken;
            var value = Expression();
            if (value == null) {
                ShowSyntaxError(token);
                value = new Address(0);
            }
            var high = value.High();
            if (value.Part == AddressPart.HighByte && high != null) {
                WriteByte(token, high);
                return;
            }
            var low = value.Low();
            if (low != null) {
                WriteByte(token, low);
            }
            else {
                ShowAddressUsageError(token);
            }
        }

        private void WriteWordExpression()
        {
            var token = LastToken;
            var value = Expression();
            if (value == null) {
                ShowSyntaxError(token);
                value = new Address(0);
            }
            var low = value.Low();
            if (low != null) {
                WriteWord(token, value);
            }
            else {
                ShowAddressUsageError(token);
            }
        }

        protected void InstructionWithoutOperand(int code)
        {
            NextToken();
            WriteByte(code);
        }

        protected void InstructionWithoutOperand(int code1, int code2)
        {
            NextToken();
            WriteByte(code1);
            WriteByte(code2);
        }

        private void InstructionWithByte(int code)
        {
            InstructionWithoutOperand(code);
            WriteByteExpression();
        }

        protected void InstructionWithByte(int code1, int code2)
        {
            WriteByte(code1);
            InstructionWithByte(code2);
        }

        private void InstructionWith2Bytes(int code)
        {
            InstructionWithoutOperand(code);
            WriteByteExpression();
            AcceptReservedWord(',');
            WriteByteExpression();
        }

        private void InstructionWithWord(int code)
        {
            InstructionWithoutOperand(code);
            WriteWordExpression();
        }

        private void InstructionWithWord(int code1, int code2)
        {
            InstructionWithoutOperand(code1, code2);
            WriteWordExpression();
        }


        private static int? Register(Token token)
        {
            if (!(token is ReservedWord reservedWord)) return null;
            var registers = new[]
                { Keyword.V, Keyword.A, Keyword.B, Keyword.C, Keyword.D, Keyword.E, Keyword.H, Keyword.L };
            for (var i = 0; i < registers.Length; ++i) {
                if (registers[i] != reservedWord.Id) continue;
                return i;
            }
            return null;
        }

        protected int? Register()
        {
            var register = Register(LastToken);
            if (register == null) return null;
            NextToken();
            return register;
        }

        private void InstructionWithRegisterByte(int code)
        {
            NextToken();
            var register = Register();
            if (register == null) {
                ShowSyntaxError(LastToken);
                register = 0;
            }
            AcceptReservedWord(',');
            WriteByte(code | register.Value);
            WriteByteExpression();
        }

        private static int? RegisterPair(Token token)
        {
            if (!(token is ReservedWord reservedWord)) return null;
            var registers = new[] { Keyword.B, Keyword.D, Keyword.H };
            for (var i = 0; i < registers.Length; ++i) {
                if (registers[i] == reservedWord.Id) {
                    return i + 1;
                }
            }
            return null;
        }

        protected int? RegisterPair()
        {
            var register = RegisterPair(LastToken);
            if (register != null) {
                NextToken();
            }
            return register;
        }

        private void InstructionWithRegisterPair(int code)
        {
            NextToken();
            var register = RegisterPair();
            if (register == null) {
                ShowSyntaxError(LastToken);
                register = 0;
            }
            WriteByte(code | register.Value);
        }


        private static int? RegisterPairVa(Token token)
        {
            if (!(token is ReservedWord reservedWord)) return null;
            var registers = new[] { Keyword.V, Keyword.B, Keyword.D, Keyword.H };
            for (var i = 0; i < registers.Length; ++i) {
                if (registers[i] == reservedWord.Id) {
                    return i;
                }
            }
            return null;
        }

        private int? RegisterPairVa()
        {
            var register = RegisterPairVa(LastToken);
            if (register != null) {
                NextToken();
            }
            return register;
        }

        private void InstructionWithRegisterPairVa(int code1, int code2)
        {
            NextToken();
            var register = RegisterPairVa();
            if (register == null) {
                ShowSyntaxError(LastToken);
                register = 0;
            }
            WriteByte(code1);
            WriteByte(code2 | register.Value << 4);
        }

        private static int? RegisterPairSp(Token token)
        {
            if (!(token is ReservedWord reservedWord)) return null;
            var registers = new[] { Keyword.SP, Keyword.B, Keyword.D, Keyword.H };
            for (var i = 0; i < registers.Length; ++i) {
                if (registers[i] == reservedWord.Id) {
                    return i;
                }
            }
            return null;
        }

        private int? RegisterPairSp()
        {
            var register = RegisterPairSp(LastToken);
            if (register != null) {
                NextToken();
            }
            return register;
        }
        private void InstructionWithRegisterPairSp(int code)
        {
            NextToken();
            var register = RegisterPairSp();
            if (register == null) {
                ShowSyntaxError(LastToken);
                register = 0;
            }
            WriteByte(code | register.Value << 4);
        }

        private void InstructionWithRegisterPairSpWord(int code)
        {
            InstructionWithRegisterPairSp(code);
            AcceptReservedWord(',');
            WriteWordExpression();
        }

        private int? SpecialRegister()
        {
            if (!(LastToken is ReservedWord reservedWord)) return null;
            var registers = new[]
                { Keyword.PA, Keyword.PB, Keyword.PC, Keyword.MK, Keyword.MB , Keyword.MC, Keyword.TM0, Keyword.TM1,Keyword.S };
            for (var i = 0; i < registers.Length; ++i) {
                if (registers[i] != reservedWord.Id) continue;
                NextToken();
                return i;
            }
            return null;
        }

        private int? Interrupt()
        {
            if (!(LastToken is ReservedWord reservedWord)) return null;
            var interrupts = new[]
                { Keyword.F0, Keyword.FT, Keyword.F1, Keyword.F2, Keyword.FS };
            for (var i = 0; i < interrupts.Length; ++i) {
                if (interrupts[i] != reservedWord.Id) continue;
                NextToken();
                return i;
            }
            return null;
        }

        protected void InstructionWithInterrupt(int code1, int code2)
        {
            NextToken();
            var interrupt = Interrupt();
            if (interrupt == null) {
                ShowSyntaxError(LastToken);
                interrupt = 0;
            }
            WriteByte(code1);
            WriteByte(code2 | interrupt.Value);
        }


        private bool MoveFromMemory(int register)
        {
            var rightToken = LastToken;
            var address = Expression();
            if (address == null) return false;
            WriteByte(0b01110000);
            WriteByte(0b01101000 | register);
            WriteWord(rightToken, address);
            return true;
        }

        private void Move()
        {
            var leftToken = NextToken();
            if (LastToken.IsReservedWord(Keyword.A)) {
                NextToken();
                AcceptReservedWord(',');
                var rightRegister = Register();
                if (rightRegister != null) {
                    //NextToken();
                    if (LastToken.IsReservedWord(Keyword.V) || LastToken.IsReservedWord(Keyword.A)) {
                        ShowInvalidRegister(leftToken);
                    }

                    WriteByte(0b00001000 | rightRegister.Value);
                    return;
                }
                var specialRegister = SpecialRegister();
                if (specialRegister != null) {
                    WriteByte(0b01001100);
                    WriteByte(0b11000000 | specialRegister.Value);
                    return;
                }
                var leftRegister = Register(leftToken);
                if (leftRegister != null && MoveFromMemory(leftRegister.Value)) {
                    return;
                }
                ShowSyntaxError(LastToken);
            }
            {
                var leftRegister = Register();
                if (leftRegister != null) {
                    var rightToken = AcceptReservedWord(',');
                    if (LastToken.IsReservedWord(Keyword.A)) {
                        NextToken();
                        if (LastToken.IsReservedWord(Keyword.V) || LastToken.IsReservedWord(Keyword.A)) {
                            ShowInvalidRegister(leftToken);
                        }
                        WriteByte(0b00011000 | leftRegister.Value);
                        return;
                    }
                    if (MoveFromMemory(leftRegister.Value)) {
                        return;
                    }
                    ShowSyntaxError(LastToken);
                }
                var specialRegister = SpecialRegister();
                if (specialRegister != null) {
                    AcceptReservedWord(',');
                    if (LastToken.IsReservedWord(Keyword.A)) {
                        NextToken();
                        WriteByte(0b01001101);
                        WriteByte(0b11000000 | specialRegister.Value);
                        return;
                    }
                }
            }
            {
                var address = Expression();
                if (address != null) {
                    AcceptReservedWord(',');
                    var register = Register();
                    if (register != null) {
                        NextToken();
                        WriteByte(0b01110000);
                        WriteByte(0b01111000 | register.Value);
                        WriteWord(leftToken, address);
                        return;
                    }
                }
            }
            ShowSyntaxError(LastToken);
        }




        private void PairRegisterIncrementDecrement(int code)
        {
            var registerToken = NextToken();
            var register = RegisterPair();
            if (register != null) {
                var operand = register.Value;
                if (LastToken.IsReservedWord('+')) {
                    NextToken();
                    if (registerToken.IsReservedWord(Keyword.B)) {
                        ShowInvalidRegister(registerToken);
                    }
                    operand += 2;
                }
                else if (LastToken.IsReservedWord('-')) {
                    NextToken();
                    if (registerToken.IsReservedWord(Keyword.B)) {
                        ShowInvalidRegister(registerToken);
                    }
                    operand += 4;
                }
                WriteByte(code | operand);
                return;
            }
            ShowSyntaxError(LastToken);
        }

        private void ByteOperation(int code1, int code2)
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
                    WriteByte(code2 | 0x80 | rightRegister.Value);
                    return;
                }
                ShowSyntaxError(LastToken);
            }
            {
                var leftRegister = Register();
                if (leftRegister != null) {
                    AcceptReservedWord(',');
                    if (LastToken.IsReservedWord(Keyword.A)) {
                        NextToken();
                        if (LastToken.IsReservedWord(Keyword.V) || LastToken.IsReservedWord(Keyword.A)) {
                            ShowInvalidRegister(leftToken);
                        }
                        WriteByte(code1);
                        WriteByte(code2 | leftRegister.Value);
                        return;
                    }
                }
                ShowSyntaxError(LastToken);
            }
        }

        private void ByteOperationX(int code1, int code2)
        {
            WriteByte(code1);
            PairRegisterIncrementDecrement(code2);
        }

        protected void ByteOperationImmediate(int codeA, int code1, int code2)
        {
            var leftToken = NextToken();
            if (LastToken.IsReservedWord(Keyword.A)) {
                NextToken();
                AcceptReservedWord(',');
                WriteByte(codeA);
                WriteByteExpression();
                return;
            }
            {
                var register = Register();
                if (register != null) {
                    AcceptReservedWord(',');
                    WriteByte(code1);
                    WriteByte(code2 | register.Value);
                    WriteByteExpression();
                    return;
                }
            }
            {
                var register = SpecialRegister();
                if (register != null) {
                    AcceptReservedWord(',');
                    if (register.Value >= 4) {
                        ShowInvalidRegister(leftToken);
                        register = 0;
                    }
                    WriteByte(code1);
                    WriteByte(code2 | 0x80 | register.Value);
                    WriteByteExpression();
                    return;
                }
            }
            ShowSyntaxError(leftToken);
        }

        private void IncrementDecrementRegister(int code)
        {
            var operand = NextToken();
            var register = Register();
            if (register != null) {
                if (!(register >= 1) || !(register <= 3)) {
                    ShowInvalidRegister(operand);
                    register = 0;
                }
                WriteByte(code | register.Value);
                return;
            }
            ShowSyntaxError();
        }


        private int? RelativeOffset(Address address, Token token)
        {
            switch (address.Type) {
                case AddressType.Undefined:
                    return null;
                case AddressType.Const:
                case AddressType.External:
                    ShowAddressUsageError(token);
                    return null;
                case AddressType.Code:
                case AddressType.Data:
                case AddressType.ZeroPage:
                    break;
            }
            if (address.Type == CurrentSegment.Type) {
                return address.Value - CurrentAddress.Value;
            }
            ShowAddressUsageError(token);
            return null;
        }

        private void RelativeJump(Token token, Address address)
        {
            var offset = RelativeOffset(address, token);
            if (offset != null) {
                offset -= 2;
                if (offset >= 0 && offset < 0x100) {
                    WriteByte(0x4e);
                    WriteByte(offset.Value);
                    return;
                }
                if (offset <= 0 && offset > -0x100) {
                    WriteByte(0x4f);
                    WriteByte(offset.Value);
                    return;
                }
            }
            WriteByte(0x54); // JMP
            WriteWord(token, address);
        }

        private void RelativeJump()
        {
            var operand = NextToken();
            var address = Expression();
            if (address == null) {
                ShowSyntaxError();
                address = Address.Default;
            }
            RelativeJump(operand, address);
        }

        private void RelativeJumpShort()
        {
            var operand = NextToken();
            var address = Expression();
            if (address == null) {
                ShowSyntaxError();
                address = Address.Default;
            }
            RelativeJumpShort(operand, address);
        }

        private void RelativeJumpShort(Token operand, Address address)
        {
            var offset = RelativeOffset(address, operand);
            if (offset != null) {
                offset -= 1;
                if (offset >= 0 && offset < 0x20) {
                    WriteByte(0xc0 + offset.Value);
                    return;
                }

                if (offset <= 0 && offset > -0x20) {
                    WriteByte(offset.Value);
                    return;
                }
            }

            RelativeJump(operand, address);
        }

        private int ConstantAddress()
        {
            var operand = LastToken;
            var address = Expression();
            if (address == null) {
                ShowSyntaxError();
                address = Address.Default;
            }
            if (address.Type == AddressType.Const) {
                return address.Value;
            }
            ShowAddressUsageError(operand);
            return 0;
        }

        private void CallConstant()
        {
            var operand = NextToken();
            var addressValue = ConstantAddress();
            if ((addressValue & 0x7ff | 0x800) != addressValue) {
                ShowOutOfRange(operand, addressValue);
            }
            WriteByte(0x78 | addressValue >> 8 & 0x07);
            WriteByte(addressValue & 0xff);
        }

        private void CallConstantShort()
        {
            var operand = NextToken();
            var addressValue = ConstantAddress();
            if (addressValue < 0x80 || addressValue > 0xff || (addressValue & 1) != 0) {
                ShowOutOfRange(operand, addressValue);
            }
            WriteByte(0x80 | addressValue >> 1 & 0x3f);
        }

        private void InOut(int code)
        {
            InstructionWithoutOperand(code);
            var token = LastToken;
            var value = Expression();
            if (value == null) {
                ShowSyntaxError(token);
                value = new Address(0);
            }
            var low = value.Low();
            int port;
            if (low == null) {
                ShowAddressUsageError(token);
                port = 0;
            }
            else {
                port = low.Value;
            }
            if (port < 0 || port > 0xbf) {
                ShowOutOfRange(token, port);
            }
            WriteByte(port);
        }

        private void ConditionalJump(int code1, int code2, Address address)
        {
            WriteByte(code1);
            WriteByte(code2);
            RelativeJumpShort(LastToken, address);
        }

        private void ConditionalJump(Address address)
        {
            if (LastToken is ReservedWord reservedWord) {
                if (SkipInstruction(reservedWord.Id)) {
                    RelativeJumpShort(LastToken, address);
                    return;
                }
            }
            ShowSyntaxError(LastToken);
            NextToken();
        }

        private void StartIf(IfBlock block)
        {
            var address = SymbolAddress(block.ElseId);
            ConditionalJump(address);
        }

        private void IfStatement()
        {
            NextToken();
            var block = NewIfBlock();
            StartIf(block);
        }

        private void IfStatement(int code1, int code2)
        {
            NextToken();
            var block = NewIfBlock();
            var address = SymbolAddress(block.ElseId);
            ConditionalJump(code1, code2, address);
        }

        private void ElseStatement()
        {
            if (LastBlock() is IfBlock block) {
                if (block.ElseId <= 0) {
                    ShowError(LastToken.Position, "Multiple ELSE statement.");
                }
                var address = SymbolAddress(block.EndId);
                RelativeJumpShort(LastToken, address);
                DefineSymbol(block.ConsumeElse(), CurrentAddress);
            }
            else {
                ShowNoStatementError(LastToken, "IF");
            }
            NextToken();
        }

        private void ElseIfStatement()
        {
            ElseStatement();
            if (!(LastBlock() is IfBlock block)) { return; }
            Debug.Assert(block.ElseId == Block.InvalidId);
            block.ElseId = AutoLabel();
            StartIf(block);
        }

        private void EndIfStatement()
        {
            if (LastBlock() is IfBlock block) {
                DefineSymbol(block.ElseId <= 0 ? block.EndId : block.ConsumeElse(), CurrentAddress);
                EndBlock();
            }
            else {
                ShowNoStatementError(LastToken, "IF");
            }
            NextToken();
        }

        private void DoStatement()
        {
            var block = NewWhileBlock();
            DefineSymbol(block.BeginId, CurrentAddress);
            NextToken();
        }
        private void WhileStatement()
        {
            NextToken();
            if (!(LastBlock() is WhileBlock block)) {
                ShowNoStatementError(LastToken, "WHILE");
                NextToken();
                return;
            }
            var address = SymbolAddress(block.EndId);
            ConditionalJump(address);
        }
        private void WEndStatement()
        {
            if (LastBlock() is WhileBlock block) {
                if (block.EndId > 0) {
                    DefineSymbol(block.RepeatId, CurrentAddress);
                    var address = SymbolAddress(block.BeginId);
                    RelativeJumpShort(LastToken, address);
                    DefineSymbol(block.EndId, CurrentAddress);
                }
                EndBlock();
            }
            else {
                ShowNoStatementError(LastToken, "WHILE");
            }
            NextToken();
        }

        private void RepeatStatement()
        {
            if (LastBlock() is WhileBlock block) {
                if (block.EndId > 0) {
                    DefineSymbol(block.RepeatId, CurrentAddress);
                    var address = SymbolAddress(block.BeginId);
                    RelativeJumpShort(LastToken, address);
                    DefineSymbol(block.EndId, CurrentAddress);
                }
                EndBlock();
            }
            else {
                ShowNoStatementError(LastToken, "WHILE");
            }
            NextToken();
        }

        protected virtual bool SkipInstruction(int id)
        {
            switch (id) {
                case Keyword.ADDNC:
                    ByteOperation(0x60, 0x20);
                    return true;
                case Keyword.ADDNCX:
                    ByteOperationX(0x70, 0xa0);
                    return true;
                case Keyword.SUBNB:
                    ByteOperation(0x60, 0x30);
                    return true;
                case Keyword.SUBNBX:
                    ByteOperationX(0x70, 0xb0);
                    return true;
                case Keyword.GTA:
                    ByteOperation(0x60, 0x28);
                    return true;
                case Keyword.GTAX:
                    ByteOperationX(0x70, 0xa8);
                    return true;
                case Keyword.LTA:
                    ByteOperation(0x60, 0x38);
                    return true;
                case Keyword.LTAX:
                    ByteOperationX(0x70, 0xb8);
                    return true;
                case Keyword.ONAX:
                    ByteOperationX(0x70, 0xc8);
                    return true;
                case Keyword.OFFAX:
                    ByteOperationX(0x70, 0xd8);
                    return true;
                case Keyword.NEA:
                    ByteOperation(0x60, 0x68);
                    return true;
                case Keyword.NEAX:
                    ByteOperationX(0x70, 0xe8);
                    return true;
                case Keyword.EQA:
                    ByteOperation(0x60, 0x78);
                    return true;
                case Keyword.EQAX:
                    ByteOperationX(0x70, 0xf8);
                    return true;
                case Keyword.ONI:
                    ByteOperationImmediate(0x47, 0x64, 0x48);
                    return true;
                case Keyword.OFFI:
                    ByteOperationImmediate(0x57, 0x64, 0x58);
                    return true;
                case Keyword.GTIW:
                    InstructionWith2Bytes(0x25);
                    return true;
                case Keyword.LTIW:
                    InstructionWith2Bytes(0x35);
                    return true;
                case Keyword.ONIW:
                    InstructionWith2Bytes(0x45);
                    return true;
                case Keyword.OFFIW:
                    InstructionWith2Bytes(0x55);
                    return true;
                case Keyword.NEIW:
                    InstructionWith2Bytes(0x65);
                    return true;
                case Keyword.EQIW:
                    InstructionWith2Bytes(0x75);
                    return true;
                case Keyword.INR:
                    IncrementDecrementRegister(0x40);
                    return true;
                case Keyword.DCR:
                    IncrementDecrementRegister(0x50);
                    return true;
                case Keyword.INRW:
                    InstructionWithByte(0x20);
                    return true;
                case Keyword.DCRW:
                    InstructionWithByte(0x30);
                    return true;
                case Keyword.SKNC:
                    InstructionWithoutOperand(0x48, 0x1a);
                    return true;
                case Keyword.SKNZ:
                    InstructionWithoutOperand(0x48, 0x1c);
                    return true;
                case Keyword.SKNIT:
                    InstructionWithInterrupt(0x48, 0x10);
                    return true;
            }
            return false;
        }


        public override bool ZeroPageAvailable => true;

        protected override bool Instruction()
        {
            var reservedWord = LastToken as ReservedWord;
            Debug.Assert(reservedWord != null);

            return Instruction(reservedWord);
        }

        protected virtual bool Instruction(ReservedWord reservedWord)
        {
            switch (reservedWord.Id) {
                case Keyword.MOV:
                    Move();
                    return true;
                case Keyword.MVI:
                    InstructionWithRegisterByte(0b01101000);
                    return true;
                case Keyword.STAW:
                    InstructionWithByte(0b00111000);
                    return true;
                case Keyword.LDAW:
                    InstructionWithByte(0b00101000);
                    return true;
                case Keyword.STAX:
                    PairRegisterIncrementDecrement(0b00111000);
                    return true;
                case Keyword.LDAX:
                    PairRegisterIncrementDecrement(0b00101000);
                    return true;
                case Keyword.SBCD:
                    InstructionWithWord(0x70, 0x1e);
                    return true;
                case Keyword.SDED:
                    InstructionWithWord(0x70, 0x2e);
                    return true;
                case Keyword.SHLD:
                    InstructionWithWord(0x70, 0x3e);
                    return true;
                case Keyword.SSPD:
                    InstructionWithWord(0x70, 0x0e);
                    return true;
                case Keyword.LBCD:
                    InstructionWithWord(0x70, 0x1f);
                    return true;
                case Keyword.LDED:
                    InstructionWithWord(0x70, 0x2f);
                    return true;
                case Keyword.LHLD:
                    InstructionWithWord(0x70, 0x3f);
                    return true;
                case Keyword.LSPD:
                    InstructionWithWord(0x70, 0x0f);
                    return true;
                case Keyword.PUSH:
                    InstructionWithRegisterPairVa(0x48, 0x0e);
                    return true;
                case Keyword.POP:
                    InstructionWithRegisterPairVa(0x48, 0x0f);
                    return true;
                case Keyword.LXI:
                    InstructionWithRegisterPairSpWord(0x04);
                    return true;
                case Keyword.ADD:
                    ByteOperation(0x60, 0x40);
                    return true;
                case Keyword.ADDX:
                    ByteOperationX(0x70, 0xc0);
                    return true;
                case Keyword.ADC:
                    ByteOperation(0x60, 0x50);
                    return true;
                case Keyword.ADCX:
                    ByteOperationX(0x70, 0xd0);
                    return true;
                case Keyword.SUB:
                    ByteOperation(0x60, 0x60);
                    return true;
                case Keyword.SUBX:
                    ByteOperationX(0x70, 0xe0);
                    return true;
                case Keyword.SBB:
                    ByteOperation(0x60, 0x70);
                    return true;
                case Keyword.SBBX:
                    ByteOperationX(0x70, 0xf0);
                    return true;
                case Keyword.ANA:
                    ByteOperation(0x60, 0x08);
                    return true;
                case Keyword.ANAX:
                    ByteOperationX(0x70, 0x88);
                    return true;
                case Keyword.ORA:
                    ByteOperation(0x60, 0x18);
                    return true;
                case Keyword.ORAX:
                    ByteOperationX(0x70, 0x98);
                    return true;
                case Keyword.XRA:
                    ByteOperation(0x60, 0x10);
                    return true;
                case Keyword.XRAX:
                    ByteOperationX(0x70, 0x90);
                    return true;
                case Keyword.ANI:
                    ByteOperationImmediate(0x07, 0x64, 0x08);
                    return true;
                case Keyword.ORI:
                    ByteOperationImmediate(0x17, 0x64, 0x18);
                    return true;
                case Keyword.ONI:
                    ByteOperationImmediate(0x47, 0x64, 0x48);
                    return true;
                case Keyword.OFFI:
                    ByteOperationImmediate(0x57, 0x64, 0x58);
                    return true;
                case Keyword.ANIW:
                    InstructionWith2Bytes(0x05);
                    return true;
                case Keyword.ORIW:
                    InstructionWith2Bytes(0x15);
                    return true;
                case Keyword.INX:
                    InstructionWithRegisterPairSp(0x02);
                    return true;
                case Keyword.DCX:
                    InstructionWithRegisterPairSp(0x03);
                    return true;
                case Keyword.DAA:
                    InstructionWithoutOperand(0x61);
                    return true;
                case Keyword.STC:
                    InstructionWithoutOperand(0x48, 0x2b);
                    return true;
                case Keyword.CLC:
                    InstructionWithoutOperand(0x48, 0x2a);
                    return true;
                case Keyword.RLD:
                    InstructionWithoutOperand(0x48, 0x38);
                    return true;
                case Keyword.RRD:
                    InstructionWithoutOperand(0x48, 0x39);
                    return true;
                case Keyword.RAL:
                    InstructionWithoutOperand(0x48, 0x30);
                    return true;
                case Keyword.RAR:
                    InstructionWithoutOperand(0x48, 0x31);
                    return true;
                case Keyword.JMP:
                    InstructionWithWord(0x54);
                    return true;
                case Keyword.JB:
                    InstructionWithoutOperand(0x73);
                    return true;
                case Keyword.JRE:
                    RelativeJump();
                    return true;
                case Keyword.JR:
                    RelativeJumpShort();
                    return true;
                case Keyword.CALL:
                    InstructionWithWord(0x44);
                    return true;
                case Keyword.CALF:
                    CallConstant();
                    return true;
                case Keyword.CALT:
                    CallConstantShort();
                    return true;
                case Keyword.RET:
                    InstructionWithoutOperand(0x08);
                    return true;
                case Keyword.RETS:
                    InstructionWithoutOperand(0x18);
                    return true;
                case Keyword.RETI:
                    InstructionWithoutOperand(0x62);
                    return true;
                case Keyword.NOP:
                    InstructionWithoutOperand(0x00);
                    return true;
                case Keyword.EI:
                    InstructionWithoutOperand(0x48, 0x20);
                    return true;
                case Keyword.DI:
                    InstructionWithoutOperand(0x48, 0x24);
                    return true;
                case Keyword.SIO:
                    InstructionWithoutOperand(0x09);
                    return true;
                case Keyword.STM:
                    InstructionWithoutOperand(0x19);
                    return true;
                case Keyword.IN:
                    InOut(0x4c);
                    return true;
                case Keyword.OUT:
                    InOut(0x4d);
                    return true;
                case Keyword.PEX:
                    InstructionWithoutOperand(0x48, 0x2d);
                    return true;
                case Keyword.PER:
                    InstructionWithoutOperand(0x48, 0x3c);
                    return true;
                //
                case Keyword.If:
                    IfStatement();
                    return true;
                case Keyword.IFC:
                    IfStatement(0x48, 0x0a);
                    return true;
                case Keyword.IFNC:
                    IfStatement(0x48, 0x1a);
                    return true;
                case Keyword.IFZ:
                    IfStatement(0x48, 0x0c);
                    return true;
                case Keyword.IFNZ:
                    IfStatement(0x48, 0x1c);
                    return true;
                case Keyword.Else:
                    ElseStatement();
                    return true;
                case Keyword.ElseIf:
                    ElseIfStatement();
                    return true;
                case Keyword.EndIf:
                    EndIfStatement();
                    return true;
                case Keyword.Do:
                    DoStatement();
                    return true;
                case Keyword.While:
                    WhileStatement();
                    return true;
                case Keyword.WEnd:
                    WEndStatement();
                    return true;
                case Keyword.REPEAT:
                    RepeatStatement();
                    return true;
                default:
                    return SkipInstruction(reservedWord.Id);
            }
        }
    }
}
