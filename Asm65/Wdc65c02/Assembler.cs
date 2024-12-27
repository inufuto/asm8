using System;
using System.Collections.Generic;
using System.Diagnostics;
using Inu.Language;

namespace Inu.Assembler.Wdc65c02;

internal class Assembler() : Mos6502.Assembler(new Tokenizer())
{
    protected override bool Instruction()
    {
        if (LastToken is not ReservedWord reservedWord)
            return false;
        if (Bra(reservedWord.Id))
            return true;
        if (Stz(reservedWord.Id))
            return true;
        if (TrbTsb(reservedWord.Id))
            return true;
        if (BbrBbs(reservedWord.Id))
            return true;
        if (RmbSmb(reservedWord.Id))
            return true;
        if (IncDec(reservedWord.Id))
            return true;
        return base.Instruction();
    }

    private bool Bra(int id)
    {
        if (id != Keyword.BRA) return false;
        NextToken();

        if (RelativeOffset(out var address, out var offset)) {
            WriteByte(0x80);
            WriteByte(offset);
            return true;
        }
        WriteByte(0x4c);    // JMP
        WriteWord(LastToken, address);
        return true;
    }


    private static readonly Dictionary<int, int> ImpliedInstructionCodes = new()
    {
        {Keyword.PHX, 0xda},
        {Keyword.PHY, 0x5a},
        {Keyword.PLX, 0xfa},
        {Keyword.PLY, 0x7a},
        {Keyword.STP, 0xdb},
        {Keyword.WAI, 0xcb},
        {Keyword.INA, 0x1a},
        {Keyword.DEA, 0x3a},
    };

    protected override bool ImpliedInstruction(int id)
    {
        if (!ImpliedInstructionCodes.TryGetValue(id, out var code)) return base.ImpliedInstruction(id);
        NextToken();
        WriteByte(code);
        return true;
    }

    private bool Stz(int id)
    {
        if (id != Keyword.STZ) return false;
        var token = NextToken();
        var value = Operand(out var addressingMode);
        if (value == null) {
            ShowSyntaxError(token);
            return true;
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
        return true;
    }

    private bool TrbTsb(int id)
    {
        int code;
        switch (id) {
            case Keyword.TRB:
                code = 0x10;
                break;
            case Keyword.TSB:
                code = 0x00;
                break;
            default:
                return false;
        }
        var token = NextToken();
        var value = Operand(out var addressingMode);
        if (value == null) {
            ShowSyntaxError(token);
            return true;
        }
        switch (addressingMode) {
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
        return true;
    }


    private static readonly Dictionary<int, int> BbrBbsCodes = new()
    {
        {Keyword.BBR0, 0x0f},
        {Keyword.BBR1, 0x1f},
        {Keyword.BBR2, 0x2f},
        {Keyword.BBR3, 0x3f},
        {Keyword.BBR4, 0x4f},
        {Keyword.BBR5, 0x5f},
        {Keyword.BBR6, 0x6f},
        {Keyword.BBR7, 0x7f},
        {Keyword.BBS0, 0x8f},
        {Keyword.BBS1, 0x9f},
        {Keyword.BBS2, 0xaf},
        {Keyword.BBS3, 0xbf},
        {Keyword.BBS4, 0xcf},
        {Keyword.BBS5, 0xdf},
        {Keyword.BBS6, 0xef},
        {Keyword.BBS7, 0xff},
    };
    private bool BbrBbs(int id)
    {
        const int instructionLength = 3;
        if (!BbrBbsCodes.TryGetValue(id, out var code)) return false;
        var token = NextToken();
        var value = ZeroPageAddress();
        if (value == null) {
            ShowSyntaxError(token);
            return true;
        }
        AcceptReservedWord(',');
        if (RelativeOffset(instructionLength, out var destination, out var offset)) {
            WriteByte(code);
            WriteByte(token, value);
            WriteByte(offset);
            return true;
        }
        WriteByte(code ^ 0x80);
        WriteByte(token, value);
        WriteByte(3);
        WriteByte(0x4c);    // JMP
        WriteWord(LastToken, destination);
        return true;
    }

    private Address? ZeroPageAddress()
    {
        var token = LastToken;
        var (zeroPage, absolute) = AddressSize();
        var value = Expression();
        if (value != null) {
            if (value.IsConst() && !zeroPage && (!value.IsByte() || absolute)) {
                ShowInvalidAddressingMode(token);
            }
        }
        return value;
    }

    private static readonly Dictionary<int, int> RmbSmbCodes = new()
    {
        {Keyword.RMB0, 0x07},
        {Keyword.RMB1, 0x17},
        {Keyword.RMB2, 0x27},
        {Keyword.RMB3, 0x37},
        {Keyword.RMB4, 0x47},
        {Keyword.RMB5, 0x57},
        {Keyword.RMB6, 0x67},
        {Keyword.RMB7, 0x77},
        {Keyword.SMB0, 0x87},
        {Keyword.SMB1, 0x97},
        {Keyword.SMB2, 0xa7},
        {Keyword.SMB3, 0xb7},
        {Keyword.SMB4, 0xc7},
        {Keyword.SMB5, 0xd7},
        {Keyword.SMB6, 0xe7},
        {Keyword.SMB7, 0xf7},
    };
    private bool RmbSmb(int id)
    {
        if (!RmbSmbCodes.TryGetValue(id, out var code)) return false;
        var token = NextToken();
        var value = ZeroPageAddress();
        if (value == null) {
            ShowSyntaxError(token);
            return true;
        }
        WriteByte(code);
        WriteByte(token, value);
        return true;
    }

    protected override void EightModeInstruction(AddressingMode? addressingMode, byte code, Token token, Address value)
    {
        if (addressingMode == AddressingMode.Indirect) {
            WriteByte((code & 0xfe) | 0x12);
            WriteByte(token, value);
        }
        else {
            base.EightModeInstruction(addressingMode, code, token, value);
        }
    }


    protected override void BitInstruction(Token token, Address value, AddressingMode addressingMode)
    {
        switch (addressingMode) {
            case AddressingMode.Immediate:
                WriteByte(0x89);
                WriteByte(token, value);
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

    private bool IncDec(int id)
    {
        int? code = null;
        Action<Token, Address, AddressingMode>? action = null;
        switch (id) {
            case Mos6502.Keyword.Inc:
                if (NextToken().IsReservedWord(Mos6502.Keyword.A)) {
                    NextToken();
                    code = 0x1a;
                }
                else {
                    action = IncrementInstruction;
                }
                break;
            case Mos6502.Keyword.Dec:
                if (NextToken().IsReservedWord(Mos6502.Keyword.A)) {
                    NextToken();
                    code = 0x3a;
                }
                else {
                    action = DecrementInstruction;
                }
                break;
            default:
                return false;
        }
        if (code != null) {
            WriteByte(code.Value);
        }
        else {
            var token = LastToken;
            var value = Operand(out var addressingMode);
            if (value != null) {
                Debug.Assert(addressingMode != null);
                Debug.Assert(action != null);
                action(token, value, addressingMode.Value);
            }
        }
        return true;
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

    private static readonly Dictionary<int, int> BrBsCodes = new()
    {
        {Keyword.BR0,0x0f},
        {Keyword.BR1,0x1f},
        {Keyword.BR2,0x2f},
        {Keyword.BR3,0x3f},
        {Keyword.BR4,0x4f},
        {Keyword.BR5,0x5f},
        {Keyword.BR6,0x6f},
        {Keyword.BR7,0x7f},
        {Keyword.BS0,0x8f},
        {Keyword.BS1,0x9f},
        {Keyword.BS2,0xaf},
        {Keyword.BS3,0xbf},
        {Keyword.BS4,0xcf},
        {Keyword.BS5,0xdf},
        {Keyword.BS6,0xef},
        {Keyword.BS7,0xff},
    };
    protected override void ConditionalBranch(Address address, byte invertedBits)
    {
        var token = LastToken;
        if (token is ReservedWord reservedWord) {
            if (BrBsCodes.TryGetValue(reservedWord.Id, out var code)) {
                var valueToken = NextToken();
                var valueAddress = ZeroPageAddress();
                if (valueAddress == null) {
                    ShowSyntaxError(token);
                    return;
                }
                invertedBits <<= 2;
                if (!address.IsUndefined()) {
                    const int instructionLength = 3;
                    var offset = RelativeOffset(address, instructionLength);
                    if (IsRelativeOffsetInRange(offset)) {
                        // branch to else/endif
                        WriteByte(code ^ invertedBits);
                        WriteByte(valueToken, valueAddress);
                        WriteByte(offset);
                        return;
                    }
                }
                WriteByte(code ^ (invertedBits ^ 0x80));
                WriteByte(valueToken, valueAddress);
                WriteByte(3);
                WriteByte(0x4c); // JMP
                WriteWord(LastToken, address);
                return;
            }
        }
        base.ConditionalBranch(address, invertedBits);
    }

    protected override void UnconditionalBranch(Address address)
    {
        if (!address.IsUndefined()) {
            var offset = RelativeOffset(address);
            if (IsRelativeOffsetInRange(offset)) {
                WriteByte(0x80);    //	BRA
                WriteByte(offset);
                return;
            }
        }
        base.UnconditionalBranch(address);
    }
}