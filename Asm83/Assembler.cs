using Inu.Language;
using System.Diagnostics;

namespace Inu.Assembler.Sm83;

internal class Assembler(int version) : LittleEndianAssembler(new Tokenizer(version))
{
    private void ShowOutOfRange(SourcePosition position)
    {
        ShowError(position, "Out of range.");
    }

    private void ShowMustBeConstant(SourcePosition position)
    {
        ShowError(position, "Must be constant.");
    }


    private static class ByteRegister
    {
        public const int B = 0;
        public const int C = 1;
        public const int D = 2;
        public const int E = 3;
        public const int H = 4;
        public const int L = 5;
        public const int A = 7;
        public const int M = 6;
    }

    private static readonly Dictionary<int, int> ByteRegisters = new()
    {
        {Keyword.A,ByteRegister.A},
        {Keyword.B,ByteRegister.B},
        {Keyword.C,ByteRegister.C},
        {Keyword.D,ByteRegister.D},
        {Keyword.E,ByteRegister.E},
        {Keyword.H,ByteRegister.H},
        {Keyword.L,ByteRegister.L},
    };

    private int? ParseByteRegister()
    {
        if (LastToken is not ReservedWord reservedWord) return null;
        if (ByteRegisters.TryGetValue(reservedWord.Id, out var value)) {
            NextToken();
            return value;
        }
        return null;
    }

    private static class WordRegister
    {
        public const int BC = 0;
        public const int DE = 1;
        public const int HL = 2;
        public const int SP = 3;
    }

    private static readonly int[] WordRegisters = {
        Keyword.BC, Keyword.DE, Keyword.HL, Keyword.SP
    };

    private int? ParseWordRegister()
    {
        if (LastToken is not ReservedWord reservedWord) return null;
        for (var i = 0; i < WordRegisters.Length; ++i) {
            if (WordRegisters[i] == reservedWord.Id) {
                NextToken();
                return i;
            }
        }
        return null;
    }

    private static class PointerRegister
    {
        public const int BC = 0;
        public const int DE = 1;
        public const int HLIncrement = 2;
        public const int HLDecrement = 3;
    }

    private int? ParsePointerRegister()
    {
        if (LastToken is not ReservedWord reservedWord) return null;
        switch (reservedWord.Id) {
            case Keyword.BC:
                NextToken();
                return 0;
            case Keyword.DE:
                NextToken();
                return 1;
            case Keyword.HL:
                NextToken();
                if (LastToken.IsReservedWord('+')) {
                    NextToken();
                    return 2;
                }
                if (LastToken.IsReservedWord('-')) {
                    NextToken();
                    return 3;
                }
                ShowError(LastToken.Position, "Missing + or -.");
                ReturnToken(reservedWord);
                break;
        }
        return null;
    }

    private static readonly int[] Conditions =
    [
        Keyword.NZ, Keyword.Z, Keyword.NC, Keyword.C
    ];

    private int? ParseCondition()
    {
        if (LastToken is not ReservedWord reservedWord) return null;
        for (var i = 0; i < Conditions.Length; ++i) {
            if (Conditions[i] == reservedWord.Id) {
                NextToken();
                return i;
            }
        }
        return null;
    }

    private int? ParseRestartAddress()
    {
        var addressToken = LastToken;
        var address = Expression();
        if (address != null) {
            if (!address.IsConst()) {
                ShowMustBeConstant(addressToken.Position);
            }
            var value = address.Value / 8;
            if (value is < 0 or >= 8) {
                ShowOutOfRange(addressToken.Position);
            }
            return value;
        }
        return null;
    }

    private static readonly int[] StackableRegisters =
    [
        Keyword.BC, Keyword.DE, Keyword.HL, Keyword.AF
    ];

    private int? ParseStackableRegister()
    {
        if (LastToken is not ReservedWord reservedWord) return null;
        for (var i = 0; i < StackableRegisters.Length; ++i) {
            if (StackableRegisters[i] == reservedWord.Id) {
                NextToken();
                return i;
            }
        }
        return null;
    }



    private void Load()
    {
        if (LastToken.IsReservedWord('(')) {
            NextToken();
            {
                var wordRegisterToken = LastToken;
                var wordRegister = ParseWordRegister();
                if (wordRegister != null) {
                    if (wordRegister == WordRegister.BC) {
                        ViaPointer(PointerRegister.BC);
                        return;
                    }
                    if (wordRegister == WordRegister.DE) {
                        ViaPointer(PointerRegister.DE);
                        return;
                    }
                    if (wordRegister == WordRegister.HL) {
                        if (LastToken.IsReservedWord('+')) {
                            NextToken();
                            ViaPointer(PointerRegister.HLIncrement);
                            return;
                        }

                        if (LastToken.IsReservedWord('-')) {
                            NextToken();
                            ViaPointer(PointerRegister.HLDecrement);
                            return;
                        }

                        AcceptReservedWord(')');
                        AcceptReservedWord(',');
                        {
                            var byteRegister = ParseByteRegister();
                            if (byteRegister != null) {
                                // ld (hl), r8
                                WriteByte(0b01000000 | ByteRegister.M << 3 | byteRegister.Value);
                                return;
                            }
                        }
                        {
                            var valueToken = LastToken;
                            var value = Expression();
                            if (value != null) {
                                // ld (hl), imm8
                                WriteByte(0b00000110 | ByteRegister.M << 3);
                                WriteByte(valueToken, value);
                                return;
                            }
                        }
                    }
                    else {
                        ShowInvalidRegister(wordRegisterToken);
                        ViaPointer(0);
                    }

                    return;

                    void ViaPointer(int pointerRegister)
                    {
                        AcceptReservedWord(')');
                        AcceptReservedWord(',');
                        var byteRegisterToken = LastToken;
                        var byteRegister = ParseByteRegister();
                        if (byteRegister != null) {
                            if (byteRegister.Value != ByteRegister.A) {
                                ShowInvalidRegister(byteRegisterToken);
                            }
                        }
                        // ld [r16mem], a
                        WriteByte(0b00000010 | pointerRegister << 4);
                    }
                }
            }
            {
                var addressToken = LastToken;
                var address = Expression();
                if (address != null) {
                    AcceptReservedWord(')');
                    AcceptReservedWord(',');
                    {
                        var wordRegisterToken = LastToken;
                        var wordRegister = ParseWordRegister();
                        if (wordRegister != null) {
                            if (wordRegister.Value != WordRegister.SP) {
                                ShowInvalidRegister(wordRegisterToken);
                            }
                            // ld [imm16], sp
                            WriteByte(0b00001000);
                            WriteWord(addressToken, address);
                            return;
                        }
                    }
                    {
                        var byteRegisterToken = LastToken;
                        var byteRegister = ParseByteRegister();
                        if (byteRegister != null) {
                            if (byteRegister.Value != ByteRegister.A) {
                                ShowInvalidRegister(byteRegisterToken);
                            }
                            // ld (imm16), a
                            WriteByte(0b11101010);
                            WriteWord(addressToken, address);
                            return;
                        }
                    }
                }
            }
        }
        {
            var leftRegisterToken = LastToken;
            var leftRegister = ParseByteRegister();
            if (leftRegister != null) {
                AcceptReservedWord(',');
                if (LastToken.IsReservedWord('(')) {
                    NextToken();
                    {
                        var wordRegisterToken = LastToken;
                        var wordRegister = ParseWordRegister();
                        if (wordRegister != null) {
                            switch (wordRegister) {
                                case WordRegister.BC:
                                    ViaPointer(PointerRegister.BC);
                                    return;
                                case WordRegister.DE:
                                    ViaPointer(PointerRegister.DE);
                                    return;
                                case WordRegister.HL:
                                    if (LastToken.IsReservedWord('+')) {
                                        NextToken();
                                        ViaPointer(PointerRegister.HLIncrement);
                                        return;
                                    }
                                    if (LastToken.IsReservedWord('-')) {
                                        NextToken();
                                        ViaPointer(PointerRegister.HLIncrement);
                                        return;
                                    }
                                    AcceptReservedWord(')');
                                    // ld r8, (hl)
                                    WriteByte(0b01000000 | leftRegister.Value << 3 | ByteRegister.M);
                                    return;
                                default:
                                    ShowInvalidRegister(wordRegisterToken);
                                    ViaPointer(0);
                                    break;
                            }
                            void ViaPointer(int pointerRegister)
                            {
                                AcceptReservedWord(')');
                                if (leftRegister.Value != ByteRegister.A) {
                                    ShowInvalidRegister(leftRegisterToken);
                                }
                                // ld a, (r16mem)
                                WriteByte(0b00001010 | pointerRegister << 4);
                            }
                        }
                    }
                    {
                        var addressToken = LastToken;
                        var address = Expression();
                        if (address != null) {
                            AcceptReservedWord(')');
                            // ld a, (imm16)
                            WriteByte(0b11111010);
                            WriteWord(addressToken, address);
                            return;
                        }
                    }
                }
                {
                    var rightRegister = ParseByteRegister();
                    if (rightRegister != null) {
                        // ld r8, r8
                        WriteByte(0b01000000 | leftRegister.Value << 3 | rightRegister.Value);
                        return;
                    }
                }
                {
                    var valueToken = LastToken;
                    var value = Expression();
                    if (value != null) {
                        // ld r8, imm8
                        WriteByte(0b00000110 | leftRegister.Value << 3);
                        WriteByte(valueToken, value);
                        return;
                    }
                }
            }
        }
        {
            var leftRegisterToken = LastToken;
            var leftRegister = ParseWordRegister();
            if (leftRegister != null) {
                AcceptReservedWord(',');
                if (LastToken.IsReservedWord('(') && leftRegister.Value != WordRegister.SP) {
                    ShowSyntaxError(LastToken);
                }
                {
                    var valueToken = LastToken;
                    var value = Expression();
                    if (value != null) {
                        // ld r16, imm16
                        WriteByte(0b00000001 | leftRegister.Value << 4);
                        WriteWord(valueToken, value);
                        return;
                    }
                }
                {
                    var rightRegisterToken = LastToken;
                    var rightRegister = ParseWordRegister();
                    if (rightRegister != null) {
                        switch (leftRegister.Value) {
                            case WordRegister.HL: {
                                    if (rightRegister.Value == WordRegister.SP) {
                                        if (LastToken.IsReservedWord('+')) {
                                            var valueToken = NextToken();
                                            var value = Expression();
                                            if (value != null) {
                                                // ld hl, sp + imm8	
                                                WriteByte(0b11111000);
                                                WriteByte(valueToken, value);
                                                return;
                                            }
                                        }
                                        else {
                                            // ld hl, sp
                                            WriteByte(0b11111000);
                                            WriteByte(0);
                                            return;
                                        }
                                    }
                                    else {
                                        ShowInvalidRegister(rightRegisterToken);
                                    }
                                    break;
                                }
                            case WordRegister.SP: {
                                    if (rightRegister.Value == WordRegister.HL) {
                                        // ld sp, hl
                                        WriteByte(0b11111001);
                                        return;
                                    }
                                    ShowInvalidRegister(rightRegisterToken);
                                    break;
                                }
                            default:
                                ShowInvalidRegister(leftRegisterToken);
                                break;
                        }
                    }
                }
            }
        }
        ShowSyntaxError(LastToken);
    }



    private void IncrementDecrement(int wordOpCode, int byteOpCode)
    {
        if (LastToken.IsReservedWord('(')) {
            var registerToken = NextToken();
            var register = ParseWordRegister();
            if (register != null) {
                if (register.Value != WordRegister.HL) {
                    ShowInvalidRegister(registerToken);
                }
                AcceptReservedWord(')');
                // (hl)
                WriteByte(byteOpCode | ByteRegister.M << 3);
                return;
            }
        }
        {
            var register = ParseByteRegister();
            if (register != null) {
                // r8
                WriteByte(byteOpCode | register.Value << 3);
                return;
            }
        }
        {
            var register = ParseWordRegister();
            if (register != null) {
                // r16
                WriteByte(wordOpCode | register.Value << 4);
                return;
            }
        }
        ShowSyntaxError(LastToken);
    }

    private void Add()
    {
        {
            var leftRegisterToken = LastToken;
            var leftRegister = ParseWordRegister();
            if (leftRegister != null) {
                AcceptReservedWord(',');
                switch (leftRegister.Value) {
                    case WordRegister.HL: {
                            var rightRegister = ParseWordRegister();
                            if (rightRegister != null) {
                                // add hl, r16
                                WriteByte(0b00001001 | rightRegister.Value << 4);
                                return;
                            }
                            break;
                        }
                    case WordRegister.SP: {
                            var valueToken = LastToken;
                            var value = Expression();
                            if (value != null) {
                                // add sp, imm8
                                WriteByte(0b11101000);
                                WriteByte(valueToken, value);
                                return;
                            }
                            break;
                        }
                    default:
                        ShowInvalidRegister(leftRegisterToken);
                        break;
                }
            }
        }
        {
            var leftRegisterToken = LastToken;
            var leftRegister = ParseByteRegister();
            if (leftRegister != null) {
                if (leftRegister.Value != ByteRegister.A) {
                    ShowInvalidRegister(leftRegisterToken);
                }
                AcceptReservedWord(',');
                if (LastToken.IsReservedWord('(')) {
                    var wordRegisterToken = NextToken();
                    var wordRegister = ParseWordRegister();
                    if (wordRegister != null) {
                        if (wordRegister.Value != WordRegister.HL) {
                            ShowInvalidRegister(wordRegisterToken);
                        }
                        AcceptReservedWord(')');
                        // a, (hl)
                        WriteByte(0b10000000 | ByteRegister.M);
                        return;
                    }
                }
                {
                    var rightRegister = ParseByteRegister();
                    if (rightRegister != null) {
                        // a, r8
                        WriteByte(0b10000000 | rightRegister.Value);
                        return;
                    }
                }
                {
                    var valueToken = LastToken;
                    var value = Expression();
                    if (value != null) {
                        // a, imm8
                        WriteByte(0b11000110);
                        WriteByte(valueToken, value);
                        return;
                    }
                }
            }
        }
        ShowSyntaxError(LastToken);
    }

    private void JumpRelative()
    {
        {
            var condition = ParseCondition();
            if (condition != null) {
                AcceptReservedWord(',');
                var addressToken = LastToken;
                var address = Expression();
                if (address != null) {
                    var conditionBits = condition.Value;
                    Jump(conditionBits, addressToken, address);
                    return;
                }
            }
        }
        {
            var addressToken = LastToken;
            var address = Expression();
            if (address != null) {
                JumpAlways(addressToken, address);
                return;
            }
        }
        ShowSyntaxError(LastToken);
    }

    private void JumpAlways(Token addressToken, Address address)
    {
        if (RelativeOffset(addressToken, address, 2, out var offset)) {
            // jr imm8
            WriteByte(0b00011000);
            WriteByte(offset);
            return;
        }
        // jp imm16
        WriteByte(0b11000011);
        WriteWord(addressToken, address);
        return;
    }

    private void Jump(int conditionBits, Token addressToken, Address address)
    {
        if (RelativeOffset(addressToken, address, 2, out var offset)) {
            // jr cond, imm8
            WriteByte(0b00100000 | conditionBits << 3);
            WriteByte(offset);
            return;
        }
        // jp cond, imm16
        WriteByte(0b11000010 | conditionBits << 3);
        WriteWord(addressToken, address);
    }

    private void OperateArithmetic(int registerOpCode, int immediateOpCode)
    {
        {
            var leftRegisterToken = LastToken;
            var leftRegister = ParseByteRegister();
            if (leftRegister != null) {
                if (leftRegister.Value != ByteRegister.A) {
                    ShowInvalidRegister(leftRegisterToken);
                }
                AcceptReservedWord(',');
                if (LastToken.IsReservedWord('(')) {
                    var wordRegisterToken = NextToken();
                    var wordRegister = ParseWordRegister();
                    if (wordRegister != null) {
                        if (wordRegister.Value != WordRegister.HL) {
                            ShowInvalidRegister(wordRegisterToken);
                        }
                        AcceptReservedWord(')');
                        // a, (hl)
                        WriteByte(registerOpCode | ByteRegister.M);
                        return;
                    }
                }
                {
                    var rightRegisterToken = LastToken;
                    var rightRegister = ParseByteRegister();
                    if (rightRegister != null) {
                        // a, r8
                        WriteByte(registerOpCode | rightRegister.Value);
                        return;
                    }
                }
                {
                    var valueToken = LastToken;
                    var value = Expression();
                    if (value != null) {
                        // a, imm8
                        WriteByte(immediateOpCode);
                        WriteByte(valueToken, value);
                        return;
                    }
                }
            }
        }
        ShowSyntaxError(LastToken);
    }

    private void Return()
    {
        {
            var condition = ParseCondition();
            if (condition != null) {
                // ret cond
                WriteByte(0b11000000 | condition.Value << 3);
                return;
            }
        }
        // ret
        WriteByte(0b11001001);
    }

    private void Jump()
    {
        {
            var condition = ParseCondition();
            if (condition != null) {
                AcceptReservedWord(',');
                var addressToken = LastToken;
                var address = Expression();
                if (address != null) {
                    // jp cond, imm16	
                    WriteByte(0b11000010 | condition.Value << 3);
                    WriteWord(addressToken, address);
                    return;
                }
            }
        }
        {
            var registerToken = LastToken;
            var register = ParseWordRegister();
            if (register != null) {
                if (register.Value != WordRegister.HL) {
                    ShowInvalidRegister(registerToken);
                }
                // jp hl
                WriteByte(0b11101001);
                return;
            }
        }
        {
            var addressToken = LastToken;
            var address = Expression();
            if (address != null) {
                // jp imm16
                WriteByte(0b11000011);
                WriteWord(addressToken, address);
                return;
            }
        }
        ShowSyntaxError(LastToken);
    }

    private void Call()
    {
        {
            var condition = ParseCondition();
            if (condition != null) {
                AcceptReservedWord(',');
                var addressToken = LastToken;
                var address = Expression();
                if (address != null) {
                    // call cond, imm16
                    WriteByte(0b11000100 | condition.Value << 3);
                    WriteWord(addressToken, address);
                    return;
                }
            }
        }
        {
            var addressToken = LastToken;
            var address = Expression();
            if (address != null) {
                // call imm16
                WriteByte(0b11001101);
                WriteWord(addressToken, address);
                return;
            }
        }
        ShowSyntaxError(LastToken);
    }

    private void Restart()
    {
        {
            var value = ParseRestartAddress();
            if (value != null) {
                // rst tgt3
                WriteByte(0b11000111 | value.Value << 3);
                return;
            }
        }
        ShowSyntaxError(LastToken);
    }

    private void PushPop(int opCode)
    {
        {
            var register = ParseStackableRegister();
            if (register != null) {
                // r16stk
                WriteByte(opCode | register.Value << 4);
                return;
            }
        }
        ShowSyntaxError(LastToken);
    }

    private void LoadHigh()
    {
        if (LastToken.IsReservedWord('(')) {
            NextToken();
            {
                var leftRegisterToken = LastToken;
                var leftRegister = ParseByteRegister();
                if (leftRegister != null) {
                    if (leftRegister.Value != ByteRegister.C) {
                        ShowInvalidRegister(leftRegisterToken);
                    }
                    AcceptReservedWord(')');
                    AcceptReservedWord(',');
                    var rightRegisterToken = LastToken;
                    var rightRegister = ParseByteRegister();
                    if (rightRegister != null) {
                        if (rightRegister.Value != ByteRegister.A) {
                            ShowInvalidRegister(rightRegisterToken);
                        }
                        // ldh (c), a
                        WriteByte(0b11100010);
                        return;
                    }
                }
            }
            {
                var addressToken = LastToken;
                var address = Expression();
                if (address != null) {
                    AcceptReservedWord(')');
                    AcceptReservedWord(',');
                    var rightRegisterToken = LastToken;
                    var rightRegister = ParseByteRegister();
                    if (rightRegister != null) {
                        if (rightRegister.Value != ByteRegister.A) {
                            ShowInvalidRegister(rightRegisterToken);
                        }

                        // ldh (imm8), a
                        WriteByte(0b11100000);
                        WriteByte(addressToken, address);
                        return;
                    }
                }
            }
        }
        {
            var leftRegisterToken = LastToken;
            var leftRegister = ParseByteRegister();
            if (leftRegister != null) {
                if (leftRegister.Value != ByteRegister.A) {
                    ShowInvalidRegister(leftRegisterToken);
                }

                AcceptReservedWord(',');
                AcceptReservedWord('(');
                {
                    var rightRegisterToken = LastToken;
                    var rightRegister = ParseByteRegister();
                    if (rightRegister != null) {
                        AcceptReservedWord(')');
                        if (rightRegister.Value != ByteRegister.C) {
                            ShowInvalidRegister(rightRegisterToken);
                        }

                        // ldh a, (c)
                        WriteByte(0b11110010);
                        return;
                    }
                }
                {
                    var addressToken = LastToken;
                    var address = Expression();
                    if (address != null) {
                        AcceptReservedWord(')');
                        // ldh a, (imm8)
                        WriteByte(0b11110000);
                        WriteByte(addressToken, address);
                        return;
                    }
                }
            }
        }
        ShowSyntaxError(LastToken);
    }

    private void Shift(int opCode)
    {
        if (LastToken.IsReservedWord('(')) {
            var registerToken = NextToken();
            var register = ParseWordRegister();
            if (register != null) {
                if (register.Value != WordRegister.HL) {
                    ShowInvalidRegister(registerToken);
                }
                AcceptReservedWord(')');
                // (hl)
                WriteByte(0b11001011);
                WriteByte(opCode | ByteRegister.M);
                return;
            }
        }
        {
            var register = ParseByteRegister();
            if (register != null) {
                // r8
                WriteByte(0b11001011);
                WriteByte(opCode | register.Value);
                return;
            }
        }
        ShowSyntaxError(LastToken);
    }

    private void OperateBit(int opCode)
    {
        {
            var valueToken = LastToken;
            var value = Expression();
            if (value != null) {
                if (value.IsConst()) {
                    if (value.Value is < 0 or > 7) {
                        ShowOutOfRange(valueToken.Position);
                    }
                }
                else {
                    ShowMustBeConstant(valueToken.Position);
                }
                AcceptReservedWord(',');
                if (LastToken.IsReservedWord('(')) {
                    var registerToken = NextToken();
                    var register = ParseWordRegister();
                    if (register != null) {
                        if (register.Value != WordRegister.HL) {
                            ShowInvalidRegister(registerToken);
                        }
                        AcceptReservedWord(')');
                        // b3, (hl)
                        WriteByte(0b11001011);
                        WriteByte(opCode | value.Value << 3 | ByteRegister.M);
                        return;
                    }
                }
                {
                    var register = ParseByteRegister();
                    if (register != null) {
                        //  b3, r8
                        WriteByte(0b11001011);
                        WriteByte(opCode | value.Value << 3 | register.Value);
                        return;
                    }
                }

            }
        }
        ShowSyntaxError(LastToken);
    }

    private static int InvertCondition(int conditionBits) => conditionBits ^ 1;

    private void ConditionalJump(Address address, bool inverted)
    {
        {
            var condition = ParseCondition();
            if (condition != null) {
                var conditionBits = condition.Value;
                if (inverted) {
                    conditionBits = InvertCondition(conditionBits);
                }
                Jump(conditionBits, LastToken, address);
                return;
            }
        }
        ShowSyntaxError(LastToken);
    }


    private void StartIf(IfBlock block)
    {
        var address = SymbolAddress(block.ElseId);
        ConditionalJump(address, true);
    }


    private void IfStatement()
    {
        var block = NewIfBlock();
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
    }

    private void UnconditionalJump(Address address)
    {
        JumpAlways(LastToken, address);
    }

    private void ElseStatement()
    {
        if (LastBlock() is not IfBlock block) {
            ShowNoStatementError(LastToken, "IF");
        }
        else {
            if (block.ElseId <= 0) {
                ShowError(LastToken.Position, "Multiple ELSE statement.");
            }
            var address = SymbolAddress(block.EndId);
            UnconditionalJump(address);
            DefineSymbol(block.ConsumeElse(), CurrentAddress);
        }
    }

    private void ElseIfStatement()
    {
        ElseStatement();
        if (LastBlock() is not IfBlock block) { return; }
        Debug.Assert(block.ElseId == Block.InvalidId);
        block.ElseId = AutoLabel();
        StartIf(block);
    }

    private void DoStatement()
    {
        var block = NewWhileBlock();
        DefineSymbol(block.BeginId, CurrentAddress);
    }

    private void WhileStatement()
    {
        if (LastBlock() is not WhileBlock block) {
            ShowNoStatementError(LastToken, "WHILE");
            NextToken();
            return;
        }

        var repeatAddress = SymbolAddress(block.RepeatId);
        int offset;
        if (repeatAddress.Type == CurrentSegment.Type && (offset = RelativeOffset(repeatAddress)) <= 0 && offset >= -2) {
            var address = SymbolAddress(block.BeginId);
            ConditionalJump(address, false);
            block.EraseEndId();
        }
        else {
            var address = SymbolAddress(block.EndId);
            ConditionalJump(address, true);
        }
    }

    private void WEndStatement()
    {
        if (LastBlock() is not WhileBlock block) {
            ShowNoStatementError(LastToken, "WHILE");
        }
        else {
            if (block.EndId > 0) {
                DefineSymbol(block.RepeatId, CurrentAddress);
                var address = SymbolAddress(block.BeginId);
                UnconditionalJump(address);
                DefineSymbol(block.EndId, CurrentAddress);
            }
            EndBlock();
        }
    }

    private static readonly Dictionary<int, Action<Assembler>> Actions = new()
    {
        {Keyword.NOP,a=>a.WriteByte(0b00000000)},
        {Keyword.LD,a=>a.Load()},
        {Keyword.INC,a=>a.IncrementDecrement(0b00000011,0b0000100)},
        {Keyword.DEC,a=>a.IncrementDecrement(0b00001011,0b0000101)},
        {Keyword.ADD, a=>a.Add()},
        {Keyword.RLCA,a=>a.WriteByte(0b00000111)},
        {Keyword.RRCA,a=>a.WriteByte(0b00001111)},
        {Keyword.RLA,a=>a.WriteByte(0b00010111)},
        {Keyword.RRA,a=>a.WriteByte(0b00011111)},
        {Keyword.DAA,a=>a.WriteByte(0b00100111)},
        {Keyword.CPL,a=>a.WriteByte(0b00101111)},
        {Keyword.SCF,a=>a.WriteByte(0b00110111)},
        {Keyword.CCF,a=>a.WriteByte(0b00111111)},
        {Keyword.JR,a=>a.JumpRelative()},
        {Keyword.STOP,a=>a.WriteByte(0b00010000)},
        {Keyword.HALT,a=>a.WriteByte(0b01110110)},
        {Keyword.ADC,a=>a.OperateArithmetic(0b10001000, 0b11001110)},
        {Keyword.SUB,a=>a.OperateArithmetic(0b10010000, 0b11010110)},
        {Keyword.SBC,a=>a.OperateArithmetic(0b10011000, 0b11011110)},
        {Inu.Assembler.Keyword.And,a=>a.OperateArithmetic(0b10100000, 0b11100110)},
        {Inu.Assembler.Keyword.Xor,a=>a.OperateArithmetic(0b10101000, 0b11101110)},
        {Inu.Assembler.Keyword.Or,a=>a.OperateArithmetic(0b10110000, 0b11110110)},
        {Keyword.CP,a=>a.OperateArithmetic(0b10111000, 0b11111110)},
        {Keyword.RET,a=>a.Return()},
        {Keyword.RETI,a=>a.WriteByte(0b11011001)},
        {Keyword.JP,a=>a.Jump()},
        {Keyword.CALL,a=>a.Call()},
        {Keyword.RST,a=>a.Restart()},
        {Keyword.POP,a=>a.PushPop(0b11000001)},
        {Keyword.PUSH,a=>a.PushPop(0b11000101)},
        {Keyword.LDH,a=>a.LoadHigh()},
        {Keyword.DI,a=>a.WriteByte(0b11110011)},
        {Keyword.EI,a=>a.WriteByte(0b11111011)},
        {Keyword.RLC,a=>a.Shift(0b00000000)},
        {Keyword.RRC,a=>a.Shift(0b00001000)},
        {Keyword.RL,a=>a.Shift(0b00010000)},
        {Keyword.RR,a=>a.Shift(0b00011000)},
        {Keyword.SLA,a=>a.Shift(0b00100000)},
        {Keyword.SRA,a=>a.Shift(0b00101000)},
        {Keyword.SWAP,a=>a.Shift(0b00110000)},
        {Keyword.SRL,a=>a.Shift(0b00111000)},
        {Keyword.BIT,a=>a.OperateBit(0b01000000)},
        {Keyword.RES,a=>a.OperateBit(0b10000000)},
        {Keyword.SET,a=>a.OperateBit(0b11000000)},
        {Inu.Assembler.Keyword.If, a=>a.IfStatement()},
        {Inu.Assembler.Keyword.EndIf, a=>a.EndIfStatement()},
        {Inu.Assembler.Keyword.Else, a=>a.ElseStatement()},
        {Inu.Assembler.Keyword.ElseIf, a=>a.ElseIfStatement()},
        {Inu.Assembler.Keyword.Do, a=>a.DoStatement()},
        {Inu.Assembler.Keyword.While, a=>a.WhileStatement()},
        {Inu.Assembler.Keyword.WEnd, a=>a.WEndStatement()},
    };

    protected override bool Instruction()
    {
        var reservedWord = LastToken as ReservedWord;
        Debug.Assert(reservedWord != null);
        if (!Actions.TryGetValue(reservedWord.Id, out var action)) return false;
        NextToken();
        action(this);
        return true;
    }
}