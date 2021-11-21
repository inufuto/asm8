using System;
using System.Diagnostics;
using Inu.Language;

namespace Inu.Assembler.Mos6502
{
    internal class Assembler : LittleEndianAssembler
    {
        public Assembler() : base(new Tokenizer()) { }

        enum AddressingMode
        {
            Immediate, ZeroPage, ZeroPageX, ZeroPageY, Absolute, AbsoluteX, AbsoluteY, Indirect, IndirectX, IndirectY
        }

        private void ShowInvalidAddressingMode(Token token)
        {
            ShowError(token.Position, "Invalid addressing mode.");
        }


        private Address? OperandWithOffset(out int? registerId)
        {
            registerId = null;
            var value = Expression();
            if (value == null)
                return null;
            if (!LastToken.IsReservedWord(','))
                return value;
            var token = NextToken();
            if (!token.IsReservedWord(Keyword.X) && !token.IsReservedWord(Keyword.Y))
                return null;
            var reservedWord = token as ReservedWord;
            Debug.Assert(reservedWord != null);
            registerId = reservedWord.Id;
            NextToken();
            return value;

        }

        private Address? Operand(out AddressingMode? addressingMode)
        {
            addressingMode = null;
            Address? value;
            if (LastToken.IsReservedWord('#')) {
                NextToken();
                value = Expression();
                addressingMode = AddressingMode.Immediate;
                return value;
            }

            Address? Indirect(char c, out AddressingMode? mode)
            {
                NextToken();
                if (LastToken.IsReservedWord('<')) {
                    // zero page
                    NextToken();
                }
                value = Expression();
                if (LastToken.IsReservedWord(c)) {
                    NextToken();
                    if (LastToken.IsReservedWord(',')) {
                        NextToken();
                        AcceptReservedWord(Keyword.Y);
                        mode = AddressingMode.IndirectY;
                        return value;
                    }
                    mode = AddressingMode.Indirect;
                    return value;
                }
                AcceptReservedWord(',');
                AcceptReservedWord(Keyword.X);
                mode = AddressingMode.IndirectX;
                AcceptReservedWord(c);
                return value;
            }

            if (LastToken.IsReservedWord('(')) {
                return Indirect(')', out addressingMode);
            }
            if (LastToken.IsReservedWord('[')) {
                return Indirect(']', out addressingMode);
            }

            var zeroPage = false;
            if (LastToken.IsReservedWord('<')) {
                // zero page
                zeroPage = true;
                NextToken();
            }

            var absolute = false;
            if (LastToken.IsReservedWord('>')) {
                // absolute
                absolute = true;
                NextToken();
            }

            value = OperandWithOffset(out var registerId);
            if (value == null)
                return null;
            if (zeroPage || (value.IsByte() && !absolute)) {
                addressingMode = registerId switch
                {
                    Keyword.X => AddressingMode.ZeroPageX,
                    Keyword.Y => AddressingMode.ZeroPageY,
                    _ => AddressingMode.ZeroPage
                };
            }
            else {
                addressingMode = registerId switch
                {
                    Keyword.X => AddressingMode.AbsoluteX,
                    Keyword.Y => AddressingMode.AbsoluteY,
                    _ => AddressingMode.Absolute
                };
            }
            return value;
        }


        private bool EightModeInstruction()
        {
            byte code;
            var store = false;
            var reservedWord = LastToken as ReservedWord;
            Debug.Assert(reservedWord != null);
            switch (reservedWord.Id) {
                case Keyword.Lda:
                    code = 0xa1;
                    break;
                case Keyword.Sta:
                    code = 0x81;
                    store = true;
                    break;
                case Keyword.Adc:
                    code = 0x61;
                    break;
                case Keyword.Sbc:
                    code = 0xe1;
                    break;
                case Keyword.Cmp:
                    code = 0xc1;
                    break;
                case Keyword.And:
                    code = 0x21;
                    break;
                case Keyword.Eor:
                    code = 0x41;
                    break;
                case Keyword.Ora:
                    code = 0x01;
                    break;
                default:
                    return false;
            }


            Token token = NextToken();
            var value = Operand(out var addressingMode);
            if (value == null) {
                ShowSyntaxError(token);
                return true;
            }

            if (!store && addressingMode is AddressingMode.Immediate && !store) {
                WriteByte(code | 0x09);
                WriteByte(token, value);
            }
            else {
                switch (addressingMode) {
                    case AddressingMode.ZeroPage:
                        WriteByte(code | 0x05);
                        WriteByte(token, value);
                        break;
                    case AddressingMode.ZeroPageX:
                        WriteByte(code | 0x15);
                        WriteByte(token, value);
                        break;
                    case AddressingMode.Absolute:
                        WriteByte(code | 0x0d);
                        WriteWord(token, value);
                        break;
                    case AddressingMode.AbsoluteX:
                        WriteByte(code | 0x1d);
                        WriteWord(token, value);
                        break;
                    case AddressingMode.AbsoluteY:
                        WriteByte(code | 0x19);
                        WriteWord(token, value);
                        break;
                    case AddressingMode.IndirectX:
                        WriteByte(code | 0x01);
                        WriteByte(token, value);
                        break;
                    case AddressingMode.IndirectY:
                        WriteByte(code | 0x11);
                        WriteByte(token, value);
                        break;
                    default:
                        ShowInvalidAddressingMode(token);
                        break;
                }
            }
            return true;
        }


        private bool FiveModeInstruction()
        {
            byte code;

            var reservedWord = LastToken as ReservedWord;
            Debug.Assert(reservedWord != null);
            switch (reservedWord.Id) {
                case Keyword.Asl:
                    code = 0x02;
                    break;
                case Keyword.Lsr:
                    code = 0x42;
                    break;
                case Keyword.Rol:
                    code = 0x22;
                    break;
                case Keyword.Ror:
                    code = 0x62;
                    break;
                default:
                    return false;
            }


            Token token = NextToken();
            if (token.IsReservedWord(Keyword.A)) {
                NextToken();
                WriteByte(code | 0x0a);
                return true;
            }

            var value = Operand(out var addressingMode);
            if (value == null) {
                ShowSyntaxError(token);
                return true;
            }

            switch (addressingMode) {
                case AddressingMode.ZeroPage:
                    WriteByte(code | 0x06);
                    WriteByte(token, value);
                    break;
                case AddressingMode.ZeroPageX:
                    WriteByte(code | 0x16);
                    WriteByte(token, value);
                    break;
                case AddressingMode.Absolute:
                    WriteByte(code | 0x0e);
                    WriteWord(token, value);
                    break;
                case AddressingMode.AbsoluteX:
                    WriteByte(code | 0x1e);
                    WriteWord(token, value);
                    break;
                default:
                    ShowInvalidAddressingMode(token);
                    break;
            }
            return true;
        }

        private bool ImpliedInstruction()
        {
            var reservedWord = LastToken as ReservedWord;
            Debug.Assert(reservedWord != null);
            byte code;
            switch (reservedWord.Id) {
                case Keyword.Tax:
                    code = 0xaa;
                    break;
                case Keyword.Tay:
                    code = 0xa8;
                    break;
                case Keyword.Tsx:
                    code = 0xba;
                    break;
                case Keyword.Txa:
                    code = 0x8a;
                    break;
                case Keyword.Txs:
                    code = 0x9a;
                    break;
                case Keyword.Tya:
                    code = 0x98;
                    break;
                case Keyword.Dex:
                    code = 0xca;
                    break;
                case Keyword.Dey:
                    code = 0x88;
                    break;
                case Keyword.Inx:
                    code = 0xe8;
                    break;
                case Keyword.Iny:
                    code = 0xc8;
                    break;
                case Keyword.PhA:
                    code = 0x48;
                    break;
                case Keyword.Php:
                    code = 0x08;
                    break;
                case Keyword.Pla:
                    code = 0x68;
                    break;
                case Keyword.Plp:
                    code = 0x28;
                    break;
                case Keyword.Rts:
                    code = 0x60;
                    break;
                case Keyword.Rti:
                    code = 0x40;
                    break;
                case Keyword.Clc:
                    code = 0x18;
                    break;
                case Keyword.Cld:
                    code = 0xd8;
                    break;
                case Keyword.Cli:
                    code = 0x58;
                    break;
                case Keyword.Clv:
                    code = 0xb8;
                    break;
                case Keyword.Sec:
                    code = 0x38;
                    break;
                case Keyword.Sed:
                    code = 0xf8;
                    break;
                case Keyword.Sei:
                    code = 0x78;
                    break;
                case Keyword.Brk:
                    code = 0x00;
                    break;
                case Keyword.Nop:
                    code = 0xea;
                    break;
                default:
                    return false;
            }
            NextToken();
            WriteByte(code);
            return true;
        }


        private void LoadXInstruction(Token token, Address value, AddressingMode addressingMode)
        {
            switch (addressingMode) {
                case AddressingMode.Immediate:
                    WriteByte(0xa2);
                    WriteByte(token, value);
                    break;
                case AddressingMode.ZeroPage:
                    WriteByte(0xa6);
                    WriteByte(token, value);
                    break;
                case AddressingMode.ZeroPageY:
                    WriteByte(0xb6);
                    WriteByte(token, value);
                    break;
                case AddressingMode.Absolute:
                    WriteByte(0xae);
                    WriteWord(token, value);
                    break;
                case AddressingMode.AbsoluteY:
                    WriteByte(0xbe);
                    WriteWord(token, value);
                    break;
                default:
                    ShowInvalidAddressingMode(token);
                    break;
            }
        }

        private void LoadYInstruction(Token token, Address value, AddressingMode addressingMode)
        {
            switch (addressingMode) {
                case AddressingMode.Immediate:
                    WriteByte(0xa0);
                    WriteByte(token, value);
                    break;
                case AddressingMode.ZeroPage:
                    WriteByte(0xa4);
                    WriteByte(token, value);
                    break;
                case AddressingMode.ZeroPageX:
                    WriteByte(0xb4);
                    WriteByte(token, value);
                    break;
                case AddressingMode.Absolute:
                    WriteByte(0xac);
                    WriteWord(token, value);
                    break;
                case AddressingMode.AbsoluteX:
                    WriteByte(0xbc);
                    WriteWord(token, value);
                    break;
                default:
                    ShowInvalidAddressingMode(token);
                    break;
            }
        }


        private void StoreXInstruction(Token token, Address value, AddressingMode addressingMode)
        {
            switch (addressingMode) {
                case AddressingMode.ZeroPage:
                    WriteByte(0x86);
                    WriteByte(token, value);
                    break;
                case AddressingMode.ZeroPageY:
                    WriteByte(0x96);
                    WriteByte(token, value);
                    break;
                case AddressingMode.Absolute:
                    WriteByte(0x8e);
                    WriteWord(token, value);
                    break;
                default:
                    ShowInvalidAddressingMode(token);
                    break;
            }
        }

        private void StoreYInstruction(Token token, Address value, AddressingMode addressingMode)
        {
            switch (addressingMode) {
                case AddressingMode.ZeroPage:
                    WriteByte(0x84);
                    WriteByte(token, value);
                    break;
                case AddressingMode.ZeroPageX:
                    WriteByte(0x94);
                    WriteByte(token, value);
                    break;
                case AddressingMode.Absolute:
                    WriteByte(0x8c);
                    WriteWord(token, value);
                    break;
                default:
                    ShowInvalidAddressingMode(token);
                    break;
            }
        }

        private void BitInstruction(Token token, Address value, AddressingMode addressingMode)
        {
            switch (addressingMode) {
                case AddressingMode.ZeroPage:
                    WriteByte(0x24);
                    WriteByte(token, value);
                    break;
                case AddressingMode.Absolute:
                    WriteByte(0x2c);
                    WriteWord(token, value);
                    break;
                default:
                    ShowInvalidAddressingMode(token);
                    break;
            }
        }

        private void CompareXInstruction(Token token, Address value, AddressingMode addressingMode)
        {
            switch (addressingMode) {
                case AddressingMode.Immediate:
                    WriteByte(0xe0);
                    WriteByte(token, value);
                    break;
                case AddressingMode.ZeroPage:
                    WriteByte(0xe4);
                    WriteByte(token, value);
                    break;
                case AddressingMode.Absolute:
                    WriteByte(0xec);
                    WriteWord(token, value);
                    break;
                default:
                    ShowInvalidAddressingMode(token);
                    break;
            }
        }

        private void CompareYInstruction(Token token, Address value, AddressingMode addressingMode)
        {
            switch (addressingMode) {
                case AddressingMode.Immediate:
                    WriteByte(0xc0);
                    WriteByte(token, value);
                    break;
                case AddressingMode.ZeroPage:
                    WriteByte(0xc4);
                    WriteByte(token, value);
                    break;
                case AddressingMode.Absolute:
                    WriteByte(0xcc);
                    WriteWord(token, value);
                    break;
                default:
                    ShowInvalidAddressingMode(token);
                    break;
            }
        }

        private void DecrementInstruction(Token token, Address value, AddressingMode addressingMode)
        {
            switch (addressingMode) {
                case AddressingMode.ZeroPage:
                    WriteByte(0xc6);
                    WriteByte(token, value);
                    break;
                case AddressingMode.ZeroPageX:
                    WriteByte(0xd6);
                    WriteByte(token, value);
                    break;
                case AddressingMode.Absolute:
                    WriteByte(0xce);
                    WriteWord(token, value);
                    break;
                case AddressingMode.AbsoluteX:
                    WriteByte(0xde);
                    WriteWord(token, value);
                    break;
                default:
                    ShowInvalidAddressingMode(token);
                    break;
            }
        }

        private void IncrementInstruction(Token token, Address value, AddressingMode addressingMode)
        {
            switch (addressingMode) {
                case AddressingMode.ZeroPage:
                    WriteByte(0xe6);
                    WriteByte(token, value);
                    break;
                case AddressingMode.ZeroPageX:
                    WriteByte(0xf6);
                    WriteByte(token, value);
                    break;
                case AddressingMode.Absolute:
                    WriteByte(0xee);
                    WriteWord(token, value);
                    break;
                case AddressingMode.AbsoluteX:
                    WriteByte(0xfe);
                    WriteWord(token, value);
                    break;
                default:
                    ShowInvalidAddressingMode(token);
                    break;
            }
        }

        private void JumpInstruction(Token token, Address value, AddressingMode addressingMode)
        {
            switch (addressingMode) {
                case AddressingMode.Absolute:
                    WriteByte(0x4c);
                    WriteWord(token, value);
                    break;
                case AddressingMode.Indirect:
                    WriteByte(0x6c);
                    WriteWord(token, value);
                    break;
                default:
                    ShowInvalidAddressingMode(token);
                    break;
            }
        }

        private void JumpSubroutineInstruction(Token token, Address value, AddressingMode addressingMode)
        {
            switch (addressingMode) {
                case AddressingMode.Absolute:
                    WriteByte(0x20);
                    WriteWord(token, value);
                    break;
                default:
                    ShowInvalidAddressingMode(token);
                    break;
            }
        }

        private bool InstructionWithOperand()
        {
            var reservedWord = LastToken as ReservedWord;
            Debug.Assert(reservedWord != null);
            Action<Token, Address, AddressingMode>? action = reservedWord.Id switch
            {
                Keyword.Ldx => LoadXInstruction,
                Keyword.Ldy => LoadYInstruction,
                Keyword.Stx => StoreXInstruction,
                Keyword.Sty => StoreYInstruction,
                Keyword.Bit => BitInstruction,
                Keyword.Cpx => CompareXInstruction,
                Keyword.Cpy => CompareYInstruction,
                Keyword.Dec => DecrementInstruction,
                Keyword.Inc => IncrementInstruction,
                Keyword.Jmp => JumpInstruction,
                Keyword.Jsr => JumpSubroutineInstruction,
                _ => null
            };
            if (action == null)
                return false;
            Token token = NextToken();
            var value = Operand(out var addressingMode);
            if (value != null) {
                Debug.Assert(addressingMode != null);
                action(token, value, addressingMode.Value);
            }
            else {
                ShowSyntaxError(token);
            }

            return true;
        }


        private bool BranchInstruction()
        {
            var reservedWord = LastToken as ReservedWord;
            Debug.Assert(reservedWord != null);

            byte code;
            switch (reservedWord.Id) {
                case Keyword.Bcc:
                    code = 0x90;
                    break;
                case Keyword.Bcs:
                    code = 0xb0;
                    break;
                case Keyword.Beq:
                    code = 0xf0;
                    break;
                case Keyword.Bne:
                    code = 0xd0;
                    break;
                case Keyword.Bmi:
                    code = 0x30;
                    break;
                case Keyword.Bpl:
                    code = 0x10;
                    break;
                case Keyword.Bvc:
                    code = 0x50;
                    break;
                case Keyword.Bvs:
                    code = 0x70;
                    break;
                default:
                    return false;
            }
            NextToken();

            if (RelativeOffset(out var address, out var offset)) {
                WriteByte(code);
                WriteByte(offset);
                return true;
            }
            WriteByte(code ^ 0x20);
            WriteByte(3);
            WriteByte(0x4c);    // JMP
            WriteWord(LastToken, address);
            return true;
        }

        private void ConditionalBranch(Address address, byte invertedBits)
        {
            Token token = LastToken;
            if (token is ReservedWord reservedWord) {
                byte code;
                switch (reservedWord.Id) {
                    case Keyword.Cc:
                        code = 0x90;
                        break;
                    case Keyword.Cs:
                        code = 0xb0;
                        break;
                    case Keyword.Eq:
                        code = 0xf0;
                        break;
                    case Keyword.Mi:
                        code = 0x30;
                        break;
                    case Keyword.Ne:
                        code = 0xd0;
                        break;
                    case Keyword.Pl:
                        code = 0x10;
                        break;
                    case Keyword.Vc:
                        code = 0x50;
                        break;
                    case Keyword.Vs:
                        code = 0x70;
                        break;
                    default:
                        ShowSyntaxError(token);
                        return;
                }

                if (!address.IsUndefined()) {
                    var offset = RelativeOffset(address);
                    if (IsRelativeOffsetInRange(offset)) {
                        NextToken();
                        // branch to else/endif
                        WriteByte(code ^ invertedBits);
                        WriteByte(offset);
                        return;
                    }
                }

                NextToken();
                WriteByte(code ^ (invertedBits ^ 0x20));
                WriteByte(3);
                WriteByte(0x4c); // JMP
                WriteWord(LastToken, address);
            }
            else {
                ShowSyntaxError(token);
                return;
            }
        }

        private void UnconditionalBranch(Address address)
        {
            WriteByte(0x4c);    //	JMP
            WriteWord(LastToken, address);
        }

        private void StartIf(IfBlock block)
        {
            Address address = SymbolAddress(block.ElseId);
            ConditionalBranch(address, 0x20);
        }

        private void IfStatement()
        {
            NextToken();
            IfBlock block = NewIfBlock();
            StartIf(block);
        }

        private void ElseStatement()
        {
            if (!(LastBlock() is IfBlock block)) {
                ShowNoStatementError(LastToken, "IF");
            }
            else {
                if (block.ElseId <= 0) {
                    ShowError(LastToken.Position, "Multiple ELSE statement.");
                }
                Address address = SymbolAddress(block.EndId);
                UnconditionalBranch(address);
                DefineSymbol(block.ConsumeElse(), CurrentAddress);
            }
            NextToken();
        }

        private void ElseIfStatement()
        {
            if (!(LastBlock() is IfBlock block)) {
                ShowNoStatementError(LastToken, "IF");
            }
            else {
                if (block.ElseId <= 0) {
                    DefineSymbol(block.EndId, CurrentAddress);
                }
                else {
                    DefineSymbol(block.ConsumeElse(), CurrentAddress);
                }
                EndBlock();
            }
            NextToken();
        }

        private void EndIfStatement()
        {
            if (!(LastBlock() is IfBlock block)) {
                ShowNoStatementError(LastToken, "IF");
            }
            else {
                if (block.ElseId <= 0) {
                    DefineSymbol(block.EndId, CurrentAddress);
                }
                else {
                    DefineSymbol(block.ConsumeElse(), CurrentAddress);
                }
                EndBlock();
            }
            NextToken();
        }

        private void DoStatement()
        {
            WhileBlock block = NewWhileBlock();
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

            Address repeatAddress = SymbolAddress(block.RepeatId);
            int offset;
            if (repeatAddress.Type == CurrentSegment.Type && (offset = RelativeOffset(repeatAddress)) <= 0 && offset >= -2) {
                Address address = SymbolAddress(block.BeginId);
                ConditionalBranch(address, 0x00);
                block.EraseEndId();
            }
            else {
                Address address = SymbolAddress(block.EndId);
                ConditionalBranch(address, 0x20);
            }
        }

        private void WEndStatement()
        {
            if (!(LastBlock() is WhileBlock block)) {
                ShowNoStatementError(LastToken, "WHILE");
            }
            else {
                if (block.EndId > 0) {
                    DefineSymbol(block.RepeatId, CurrentAddress);
                    Address address = SymbolAddress(block.BeginId);
                    UnconditionalBranch(address);
                    DefineSymbol(block.EndId, CurrentAddress);
                }
                EndBlock();
            }
            NextToken();
        }


        public override bool ZeroPageAvailable => true;

        protected override bool Instruction()
        {
            if (!(LastToken is ReservedWord reservedWord))
                return false;
            if (ImpliedInstruction())
                return true;
            if (EightModeInstruction())
                return true;
            if (FiveModeInstruction())
                return true;
            if (BranchInstruction())
                return true;
            if (InstructionWithOperand())
                return true;

            switch (reservedWord.Id) {
                case Keyword.If:
                    IfStatement();
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
            }

            return false;

        }

    }
}
