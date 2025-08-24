using System;
using Inu.Language;
using System.Collections.Generic;

namespace Inu.Assembler.Wdc65816;

internal class Assembler(int version) : Mos6502.Assembler(new Tokenizer(version),version)
{
    private enum RegisterMode
    {
        Byte, Word
    }

    private RegisterMode memoryMode = RegisterMode.Word;
    private RegisterMode indexMode = RegisterMode.Word;

    protected override bool Directive(ReservedWord reservedWord)
    {
        switch (reservedWord.Id) {
            case Keyword.A16:
                memoryMode = RegisterMode.Word;
                break;
            case Keyword.A8:
                memoryMode = RegisterMode.Byte;
                break;
            case Keyword.I16:
                indexMode = RegisterMode.Word;
                break;
            case Keyword.I8:
                indexMode = RegisterMode.Byte;
                break;
            default:
                return base.Directive(reservedWord);
        }
        NextToken();
        return true;
    }

    private static readonly Dictionary<int, Action<Assembler>> InstructionActions = new Dictionary<int, Action<Assembler>>()
    {
        { Keyword.STZ, a=>a.Stz()},
        { Keyword.PEA, a=>a.Pea()},
        { Keyword.PEI, a=>a.Pei()},
        { Keyword.PER, a=>a.PerBrl(0x62)},
        { Mos6502.Keyword.Inc, a=>a.IncDec(0x1a,0xe6)},
        { Mos6502.Keyword.Dec, a=>a.IncDec(0x3a,0xc6)},
        { Keyword.TRB, a=>a.TrbTsb(0x14)},
        { Keyword.TSB, a=>a.TrbTsb(0x04)},
        { Mos6502.Keyword.Cpx, a=>a.CpxCpy(0xe0)},
        { Mos6502.Keyword.Cpy, a=>a.CpxCpy(0xc0)},
        { Keyword.SEP, a=>a.SepRep(0xe2)},
        { Keyword.REP, a=>a.SepRep(0xc2)},
        {Keyword.BLT, a=>a.BranchInstruction(0x90)},
        {Keyword.BGE, a=>a.BranchInstruction(0xb0)},
        { Keyword.BRA, a=>a.Bra()},
        { Keyword.BRL, a=>a.PerBrl(0x82)},
        { Keyword.JML, a=>a.Jml()},
        { Keyword.JSL, a=>a.Jsl()},
        { Keyword.MVP, a=>a.Move(0x44)},
        { Keyword.MVN, a=>a.Move(0x54)},
    };

    private void Move(int code)
    {
        const string mustBeConstant = "Must be constant.";

        var token1 = LastToken;
        var value1 = Expression();
        if (value1 == null) {
            ShowSyntaxError(token1);
            value1 = Address.Default;
        }
        if (!value1.IsConst()) {
            ShowError(token1.Position, mustBeConstant);
        }
        AcceptReservedWord(',');
        var token2 = LastToken;
        var value2 = Expression();
        if (value2 == null) {
            ShowSyntaxError(token2);
            value2 = Address.Default;
        }
        if (!value2.IsConst()) {
            ShowError(token2.Position, mustBeConstant);
        }
        WriteByte(code);
        WriteByte(token1, value1);
        WriteByte(token2, value2);
    }

    protected override bool Instruction()
    {
        if (LastToken is not ReservedWord reservedWord)
            return false;
        if (InstructionActions.TryGetValue(reservedWord.Id, out var action)) {
            NextToken();
            action(this);
            return true;
        }
        return base.Instruction();
    }


    private void Jsl()
    {
        var token = LastToken;
        var value = Operand(out var addressingMode);
        if (value == null) {
            ShowSyntaxError(token);
            value = Address.Default;
        }
        switch (addressingMode) {
            case AddressingMode.Absolute when IsLong(value):
                WriteByte(0x22);
                WriteTripleBytes(value.Value);
                break;
            default:
                ShowInvalidAddressingMode(token);
                break;
        }
    }

    private void Jml()
    {
        var token = LastToken;
        var value = Operand(out var addressingMode);
        if (value == null) {
            ShowSyntaxError(token);
            value = Address.Default;
        }
        switch (addressingMode) {
            case AddressingMode.Absolute when IsLong(value):
                WriteByte(0x5c);
                WriteTripleBytes(value.Value);
                break;
            case AddressingMode.IndirectLong:
                WriteByte(0xdc);
                WriteWord(token, value);
                break;
            default:
                ShowInvalidAddressingMode(token);
                break;
        }
    }

    private void Bra()
    {
        if (RelativeOffset(out var address, out var offset)) {
            WriteByte(0x80);
            WriteByte(offset);
            return;
        }
        WriteByte(0x4c);    // JMP
        WriteWord(LastToken, address);
    }

    private void SepRep(int code)
    {
        var token = LastToken;
        var value = Operand(out var addressingMode);
        if (value == null) {
            ShowSyntaxError(token);
            value = Address.Default;
        }
        switch (addressingMode) {
            case AddressingMode.Immediate:
                WriteByte(code);
                WriteByte(token, value);
                break;
            default:
                ShowInvalidAddressingMode(token);
                break;
        }
    }

    private void CpxCpy(int code)
    {
        var token = LastToken;
        var value = Operand(out var addressingMode);
        if (value == null) {
            ShowSyntaxError(token);
            value = Address.Default;
        }
        switch (addressingMode) {
            case AddressingMode.Immediate:
                WriteByte(code);
                if (indexMode == RegisterMode.Word) {
                    WriteWord(token, value);
                }
                else {
                    WriteByte(token, value);
                }
                break;
            case AddressingMode.ZeroPage:
                WriteByte(code | 0x04);
                WriteByte(token, value);
                break;
            case AddressingMode.Absolute:
                WriteByte(code | 0x0c);
                WriteWord(token, value);
                break;
            default:
                ShowInvalidAddressingMode(token);
                break;
        }
    }

    private void TrbTsb(int code)
    {
        var token = LastToken;
        var value = Operand(out var addressingMode);
        if (value == null) {
            ShowSyntaxError(token);
            value = Address.Default;
        }
        switch (addressingMode) {
            case AddressingMode.ZeroPage:
                WriteByte(code);
                WriteByte(token, value);
                break;
            case AddressingMode.Absolute:
                WriteByte(code | 0x08);
                WriteWord(token, value);
                break;
            default:
                ShowInvalidAddressingMode(token);
                break;
        }
    }

    private void IncDec(int codeA, int codeB)
    {
        if (LastToken.IsReservedWord(Mos6502.Keyword.A)) {
            NextToken();
            WriteByte(codeA);
            return;
        }
        var token = LastToken;
        var value = Operand(out var addressingMode);
        if (value == null) {
            ShowSyntaxError(token);
            value = Address.Default;
        }
        switch (addressingMode) {
            case AddressingMode.ZeroPage:
                WriteByte(codeB);
                WriteByte(token, value);
                break;
            case AddressingMode.ZeroPageX:
                WriteByte(codeB | 0x10);
                WriteByte(token, value);
                break;
            case AddressingMode.Absolute:
                WriteByte(codeB | 0x08);
                WriteWord(token, value);
                break;
            case AddressingMode.AbsoluteX:
                WriteByte(codeB | 0x18);
                WriteWord(token, value);
                break;
            default:
                ShowInvalidAddressingMode(token);
                break;
        }
    }

    private void PerBrl(int code)
    {
        var token = LastToken;
        if (!RelativeOffset(3, o => true, out var address, out var offset)) {
            offset = 0;
        }
        WriteByte(code);
        WriteWord(offset);
    }

    private void Pei()
    {
        var token = LastToken;
        var value = Operand(out var addressingMode);
        if (value == null) {
            ShowSyntaxError(token);
            value = Address.Default;
        }

        if (addressingMode == AddressingMode.Indirect) {
            WriteByte(0xd4);
            WriteByte(token, value);
        }
        else {
            ShowInvalidAddressingMode(token);
        }
    }


    private void Pea()
    {
        var token = LastToken;
        var value = Operand(out var addressingMode);
        if (value == null) {
            ShowSyntaxError(token);
            value = Address.Default;
        }
        switch (addressingMode) {
            case AddressingMode.Absolute:
            case AddressingMode.Immediate:
                WriteByte(0xf4);
                WriteWord(token, value);
                break;
            default:
                ShowInvalidAddressingMode(token);
                break;
        }
    }

    private void Stz()
    {
        var token = LastToken;
        var value = Operand(out var addressingMode);
        if (value == null) {
            ShowSyntaxError(token);
            value = Address.Default;
        }
        switch (addressingMode) {
            case AddressingMode.ZeroPage:
                WriteByte(0x64);
                WriteByte(token, value);
                break;
            case AddressingMode.ZeroPageX:
                WriteByte(0x74);
                WriteByte(token, value);
                break;
            case AddressingMode.Absolute:
                WriteByte(0x9c);
                WriteWord(token, value);
                break;
            case AddressingMode.AbsoluteX:
                WriteByte(0x9e);
                WriteWord(token, value);
                break;
            default:
                ShowInvalidAddressingMode(token);
                break;
        }
    }

    protected override bool IsOffsetRegister(Token token)
    {
        return base.IsOffsetRegister(token) || token.IsReservedWord(Keyword.S);
    }

    protected override Address? Operand(out AddressingMode? addressingMode)
    {
        if (LastToken.IsReservedWord('(')) {
            NextToken();
            if (LastToken.IsReservedWord('<')) {
                // direct page
                NextToken();
            }
            var value = Expression();
            if (LastToken.IsReservedWord(')')) {
                NextToken();
                if (LastToken.IsReservedWord(',')) {
                    NextToken();
                    if (LastToken.IsReservedWord(Mos6502.Keyword.Y)) {
                        NextToken();
                        addressingMode = AddressingMode.IndirectY;
                        return value;
                    }
                }
                addressingMode = AddressingMode.Indirect;
                return value;
            }
            AcceptReservedWord(',');
            if (LastToken.IsReservedWord(Mos6502.Keyword.X)) {
                NextToken();
                addressingMode = AddressingMode.IndirectX;
                AcceptReservedWord(')');
                return value;
            }
            if (LastToken.IsReservedWord(Keyword.S)) {
                NextToken();
                AcceptReservedWord(')');
                AcceptReservedWord(',');
                AcceptReservedWord(Mos6502.Keyword.Y);
                addressingMode = AddressingMode.StackRelativeIndirectY;
                return value;
            }
            AcceptReservedWord(')');
            addressingMode = AddressingMode.Indirect;
            return value;
        }
        if (LastToken.IsReservedWord('[')) {
            NextToken();
            if (LastToken.IsReservedWord('<')) {
                // direct page
                NextToken();
            }
            var value = Expression();
            AcceptReservedWord(']');
            if (LastToken.IsReservedWord(',')) {
                NextToken();
                AcceptReservedWord(Mos6502.Keyword.Y);
                addressingMode = AddressingMode.IndirectLongY;
            }
            else {
                addressingMode = AddressingMode.IndirectLong;
            }
            return value;
        }
        return base.Operand(out addressingMode);
    }

    protected override AddressingMode ToAddressingMode(int? registerId)
    {
        return registerId is Keyword.S ? AddressingMode.StackRelative : base.ToAddressingMode(registerId);
    }

    protected override void ImmediateInstruction(Token token, byte code, Address value)
    {
        if (memoryMode == RegisterMode.Word) {
            WriteByte(code | 0x09);
            WriteWord(token, value);
            return;
        }
        base.ImmediateInstruction(token, code, value);
    }

    protected override void EightModeInstruction(AddressingMode? addressingMode, byte code, Token token, Address value)
    {
        switch (addressingMode) {
            case AddressingMode.Indirect:
                WriteByte((code & 0xfe) | 0x12);
                WriteByte(token, value);
                break;
            case AddressingMode.Absolute when IsLong(value):
                WriteByte(code | 0x0f);
                WriteTripleBytes(value.Value);
                break;
            case AddressingMode.AbsoluteX when IsLong(value):
                WriteByte(code | 0x1f);
                WriteTripleBytes(value.Value);
                break;
            case AddressingMode.IndirectLong:
                WriteByte(code | 0x07);
                WriteByte(token, value);
                break;
            case AddressingMode.IndirectLongY:
                WriteByte(code | 0x17);
                WriteByte(token, value);
                break;
            case AddressingMode.StackRelative:
                WriteByte(code | 0x03);
                WriteByte(token, value);
                break;
            case AddressingMode.StackRelativeIndirectY:
                WriteByte(code | 0x13);
                WriteByte(token, value);
                break;
            default:
                base.EightModeInstruction(addressingMode, code, token, value);
                break;
        }
    }

    private static readonly Dictionary<int, int> ImpliedInstructionCodes = new()
    {
        {Keyword.TXY,0x9b},
        {Keyword.TYX,0xbb},
        {Keyword.TAD,0x5b},
        {Keyword.TCD,0x5b},
        {Keyword.TDA,0x7b},
        {Keyword.TDC,0x7b},
        {Keyword.TAS,0x1b},
        {Keyword.TCS,0x1b},
        {Keyword.TSA,0x3b},
        {Keyword.TSC,0x3b},
        {Keyword.XBA,0xeb},
        {Keyword.SWA,0xeb},
        {Keyword.PHX, 0xda},
        {Keyword.PHY, 0x5a},
        {Keyword.PLX, 0xfa},
        {Keyword.PLY, 0x7a},
        {Keyword.PHB, 0x8b},
        {Keyword.PHD, 0x0b},
        {Keyword.PHK, 0x4b},
        {Keyword.PLB, 0xab},
        {Keyword.PLD, 0x2b},
        {Keyword.XCE, 0xfb},
        {Keyword.RTL, 0x6b},
        {Keyword.STP, 0xdb},
        {Keyword.COP, 0x02},
        {Keyword.WAI, 0xcb},
        {Keyword.WDM, 0x42},
    };
    protected override bool ImpliedInstruction(int id)
    {
        if (!ImpliedInstructionCodes.TryGetValue(id, out var code)) return base.ImpliedInstruction(id);
        NextToken();
        WriteByte(code);
        return true;
    }

    protected override void LoadXInstruction(Token token, Address value, AddressingMode addressingMode)
    {
        if (addressingMode == AddressingMode.Immediate && indexMode == RegisterMode.Word) {
            WriteByte(0xa2);
            WriteWord(token, value);
            return;
        }
        base.LoadXInstruction(token, value, addressingMode);
    }

    protected override void LoadYInstruction(Token token, Address value, AddressingMode addressingMode)
    {
        if (addressingMode == AddressingMode.Immediate && indexMode == RegisterMode.Word) {
            WriteByte(0xa0);
            WriteWord(token, value);
            return;
        }
        base.LoadYInstruction(token, value, addressingMode);
    }

    protected override void BitInstruction(Token token, Address value, AddressingMode addressingMode)
    {
        switch (addressingMode) {
            case AddressingMode.Immediate:
                WriteByte(0x89);
                if (memoryMode == RegisterMode.Word) {
                    WriteWord(token, value);
                }
                else {
                    WriteByte(token, value);
                }
                break;
            case AddressingMode.ZeroPageX:
                WriteByte(0x34);
                WriteByte(token, value);
                break;
            case AddressingMode.AbsoluteX:
                WriteByte(0x3c);
                WriteWord(token, value);
                break;
            default:
                base.BitInstruction(token, value, addressingMode);
                break;
        }
    }

    protected override void JumpInstruction(Token token, Address value, AddressingMode addressingMode)
    {
        if (addressingMode == AddressingMode.IndirectX) {
            WriteByte(0x7c);
            WriteWord(token, value);
        }
        else {
            base.JumpInstruction(token, value, addressingMode);
        }
    }

    protected override void JumpSubroutineInstruction(Token token, Address value, AddressingMode addressingMode)
    {
        if (addressingMode == AddressingMode.IndirectX) {
            WriteByte(0xfc);
            WriteWord(token, value);
        }
        else {
            base.JumpSubroutineInstruction(token, value, addressingMode);
        }
    }

    private static bool IsLong(Address value) => value.IsConst() && (value.Value & 0xff0000) != 0;

    private void WriteTripleBytes(int value)
    {
        WriteWord(value);
        WriteByte(value >> 16);
    }
}