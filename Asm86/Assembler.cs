using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Inu.Language;

namespace Inu.Assembler.I8086
{
    internal class Assembler : LittleEndianAssembler
    {
        private int? byteCount = null;
        private readonly IList<byte> instructionBytes = new List<byte>();
        private byte? segmentOverride;

        public Assembler() : base(new Tokenizer()) { }

        public int? ByteCount
        {
            get => byteCount;
            set
            {
                if (byteCount != null && value != null && value != byteCount) {
                    ShowTypeMismatch(LastToken);
                }
                byteCount = value;
            }
        }
        protected override void WriteByte(int value)
        {
            instructionBytes.Add((byte)value);
        }

        protected override Address CurrentAddress
        {
            get
            {
                var address = base.CurrentAddress;
                return new Address(address.Type, address.Value + instructionBytes.Count);
            }
        }

        private void ShowTypeMismatch(Token token)
        {
            ShowError(token.Position, "Type mismatch.");
        }

        private void ShowUnknownType(Token token)
        {
            ShowError(token.Position, "Ambiguous type.");
        }

        private static bool IsSignedByte(Address value)
        {
            return value.IsConst() && value.Value >= -128 && value.Value <= 127;
        }

        private void ParseByteCount()
        {
            if (LastToken.IsReservedWord(Keyword.BYTE)) {
                NextToken();
                ByteCount = 1;
            }
            else if (LastToken.IsReservedWord(Keyword.WORD)) {
                NextToken();
                ByteCount = 2;
            }
            else {
                return;
            }
            AcceptReservedWord(Keyword.PTR);
        }

        private void ParseSegmentOverride()
        {
            var register = SegmentRegisterCode(LastToken);
            if (register == null) return;
            NextToken();
            ParseSegmentOverride(register.Value);
        }

        private void ParseSegmentOverride(int register)
        {
            segmentOverride = (byte)(0b00100110 | (register << 3));
            AcceptReservedWord(':');
        }


        private static readonly IList<int> ByteRegisterCodes = new List<int>()
        {
            Keyword.AL, Keyword.CL, Keyword.DL, Keyword.BL, Keyword.AH, Keyword.CH, Keyword.DH, Keyword.BH
        };

        private static int? ByteRegisterCode(Token token)
        {
            if (!(token is ReservedWord reservedWord)) return null;
            for (var code = 0; code < ByteRegisterCodes.Count; ++code) {
                if (reservedWord.Id == ByteRegisterCodes[code]) {
                    return code;
                }
            }
            return null;
        }

        private static readonly IList<int> WordRegisterCodes = new List<int>()
        {
            Keyword.AX,Keyword.CX,Keyword.DX,Keyword.BX,Keyword.SP,Keyword.BP,Keyword.SI,Keyword.DI
        };
        private static int? WordRegisterCode(Token token)
        {
            if (!(token is ReservedWord reservedWord)) return null;
            for (var code = 0; code < WordRegisterCodes.Count; ++code) {
                if (reservedWord.Id == WordRegisterCodes[code]) {
                    return code;
                }
            }
            return null;
        }

        private static readonly IList<int> SegmentRegisterCodes = new List<int>()
        {
            Keyword.ES,Keyword.CS,Keyword.SS,Inu.Assembler.Keyword.Ds
        };
        private static int? SegmentRegisterCode(Token token)
        {
            if (!(token is ReservedWord reservedWord)) return null;
            for (var code = 0; code < SegmentRegisterCodes.Count; ++code) {
                if (reservedWord.Id == SegmentRegisterCodes[code]) {
                    return code;
                }
            }
            return null;
        }


        private bool LeftRightRegister(int code, int leftRegister, Func<Token, int?> func)
        {
            if (!LastToken.IsReservedWord(',')) return false;
            var rightToken = NextToken();
            ParseByteCount();
            var rightRegister = func(rightToken);
            if (rightRegister == null) return false;
            NextToken();
            WriteByte(code);
            WriteByte(0b11000000 | (leftRegister << 3) | rightRegister.Value);
            return true;
        }

        private bool LeftRightByteRegister(int code, int leftRegister)
        {
            return LeftRightRegister(code, leftRegister, ByteRegisterCode);
        }

        private bool LeftRightWordRegister(int code, int leftRegister)
        {
            return LeftRightRegister(code, leftRegister, WordRegisterCode);
        }


        private bool MemoryOperand(out int? rm, out Token? valueToken, out Address? value)
        {
            rm = null;
            valueToken = null;
            value = null;
            var offsetRequired = false;
            ParseSegmentOverride();
            if (LastToken.IsReservedWord(Keyword.BX) || LastToken.IsReservedWord(Keyword.BP)) {
                var pointer1 = LastToken;
                NextToken();
                if (LastToken.IsReservedWord('+')) {
                    NextToken();
                    if (LastToken.IsReservedWord(Keyword.SI) || LastToken.IsReservedWord(Keyword.DI)) {
                        var pointer2 = LastToken;
                        NextToken();
                        if (pointer1.IsReservedWord(Keyword.BX)) {
                            if (pointer2.IsReservedWord(Keyword.SI)) {
                                rm = 0b000;
                            }
                            else if (pointer2.IsReservedWord(Keyword.DI)) {
                                rm = 0b001;
                            }
                        }
                        else if (pointer1.IsReservedWord(Keyword.BP)) {
                            if (pointer2.IsReservedWord(Keyword.SI)) {
                                rm = 0b010;
                            }
                            else if (pointer2.IsReservedWord(Keyword.DI)) {
                                rm = 0b011;
                            }
                        }
                    }
                    else {
                        offsetRequired = true;
                    }
                }
                if (rm == null) {
                    if (pointer1.IsReservedWord(Keyword.BP)) {
                        rm = 0b110;
                    }
                    else if (pointer1.IsReservedWord(Keyword.BX)) {
                        rm = 0b111;
                    }
                }
            }
            else {
                if (LastToken.IsReservedWord(Keyword.SI) || LastToken.IsReservedWord(Keyword.DI)) {
                    var pointer = LastToken;
                    NextToken();
                    if (pointer.IsReservedWord(Keyword.SI)) {
                        rm = 0b100;
                    }
                    else if (pointer.IsReservedWord(Keyword.DI)) {
                        rm = 0b101;
                    }
                }
            }
            if (rm != null) {
                if (!offsetRequired) {
                    if (LastToken.IsReservedWord('+')) {
                        offsetRequired = true;
                        NextToken();
                    }
                    else if (LastToken.IsReservedWord('-')) {
                        offsetRequired = true;
                    }
                }
                if (offsetRequired) {
                    valueToken = LastToken;
                    value = Expression();
                }
                AcceptReservedWord(']');
                return true;
            }
            // direct
            valueToken = LastToken;
            value = Expression();
            if (value == null) return false;
            AcceptReservedWord(']');
            return true;
        }

        private bool FromMemory(int code, int register)
        {
            if (!MemoryOperand(out var rm, out var valueToken, out var value)) return false;
            if (rm != null) {
                if (value != null) {
                    if (IsSignedByte(value)) {
                        // mod != 01
                        WriteByte(code);
                        WriteByte(0b01000000 | (register << 3) | rm.Value);
                        Debug.Assert(valueToken != null);
                        WriteByte(valueToken, value);
                        return true;
                    }

                    // mod != 10
                    WriteByte(code);
                    WriteByte(0b10000000 | (register << 3) | rm.Value);
                    Debug.Assert(valueToken != null);
                    WriteWord(valueToken, value);
                    return true;
                }
                // mod == 00
                WriteByte(code);
                WriteByte((register << 3) | rm.Value);
                return true;
            }
            if (value == null) return false;
            WriteByte(code);
            WriteByte(0b00000110 | (register << 3));
            Debug.Assert(valueToken != null);
            WriteWord(valueToken, value);
            return true;
        }


        private bool FromRegisterOrMemory(int code, int leftRegister, Func<Token, int?> func)
        {
            {
                var rightRegister = func(LastToken);
                if (rightRegister != null) {
                    NextToken();
                    WriteByte(code);
                    WriteByte(0b11000000 | (leftRegister << 3) | rightRegister.Value);
                    return true;
                }
            }
            ParseSegmentOverride();
            if (!LastToken.IsReservedWord('[')) return false;
            NextToken();
            if (ByteCount == null) {
                ShowUnknownType(LastToken);
            }
            if (ByteCount == 2) code |= 1;
            return FromMemory(code, leftRegister);
        }

        public delegate bool ParseRightOperand(Token token, out int? register, out Address? value, out int code, out bool shortenable);
        private bool ToMemory(ParseRightOperand func)
        {
            if (!MemoryOperand(out var rm, out var leftToken, out var leftValue)) return false;
            AcceptReservedWord(',');
            ParseByteCount();
            var rightToken = LastToken;
            if (!func(rightToken, out var rightRegister, out var rightValue, out var code, out var shortenable)) return false;
            Action writeImmediateValue;
            int register;
            if (rightValue != null) {
                register = rightRegister ?? 0;
                if (ByteCount == null) {
                    ShowUnknownType(rightToken);
                }
                if (ByteCount is 2) {
                    code |= 1;
                }
                if (ByteCount is 1 || (shortenable && IsSignedByte(rightValue))) {
                    writeImmediateValue = () => { WriteByte(rightToken, rightValue); };
                }
                else {
                    writeImmediateValue = () => { WriteWord(rightToken, rightValue); };
                }
            }
            else if (rightRegister != null) {
                register = rightRegister.Value;
                writeImmediateValue = () => { };
            }
            else {
                return false;
            }
            if (rm != null) {
                if (leftValue != null && (leftValue.Value != 0 || rm == 0b110)) {
                    if (IsSignedByte(leftValue)) {
                        // mod != 01
                        WriteByte(code);
                        WriteByte(0b01000000 | (register << 3) | rm.Value);
                        Debug.Assert(leftToken != null);
                        WriteByte(leftToken, leftValue);
                        writeImmediateValue();
                        return true;
                    }

                    // mod != 10
                    WriteByte(code);
                    WriteByte(0b10000000 | (register << 3) | rm.Value);
                    Debug.Assert(leftToken != null);
                    WriteWord(leftToken, leftValue);
                    writeImmediateValue();
                    return true;
                }
                // mod == 00
                WriteByte(code);
                WriteByte((register << 3) | rm.Value);
                writeImmediateValue();
                return true;
            }
            if (leftValue == null) return false;
            //AcceptReservedWord(']');
            WriteByte(code);
            WriteByte(0b00000110 | (register << 3));
            Debug.Assert(leftToken != null);
            WriteWord(leftToken, leftValue);
            writeImmediateValue();
            return true;
        }

        private bool InherentInstruction(params int[] codes)
        {
            foreach (var code in codes) {
                WriteByte(code);
            }
            return true;
        }

        private bool MoveInstruction()
        {
            ParseByteCount();
            var leftToken = LastToken;
            {
                // to byte register
                var leftRegister = ByteRegisterCode(leftToken);
                if (leftRegister != null) {
                    byteCount = 1;
                    NextToken();
                    {
                        // register
                        if (LeftRightByteRegister(0b10001010, leftRegister.Value)) return true;
                    }
                    {
                        // immediate
                        var token = LastToken;
                        var value = Expression();
                        if (value != null) {
                            WriteByte(0b10110000 | leftRegister.Value);
                            WriteByte(token, value);
                            return true;
                        }
                    }
                    ParseSegmentOverride();
                    if (LastToken.IsReservedWord('[')) {
                        NextToken();
                        ParseSegmentOverride();
                        {
                            var token = LastToken;
                            var value = Expression();
                            if (value != null) {
                                AcceptReservedWord(']');
                                if (leftToken.IsReservedWord(Keyword.AL)) {
                                    WriteByte(0b10100000);
                                    WriteWord(token, value);
                                }
                                else {
                                    WriteByte(0b10001010);
                                    WriteByte(0b00000110 | (leftRegister.Value << 3));
                                    WriteWord(token, value);
                                }
                                return true;
                            }
                        }
                        if (FromMemory(0b10001010, leftRegister.Value)) return true;
                    }
                }
            }
            {
                // to word register
                var leftRegister = WordRegisterCode(leftToken);
                if (leftRegister != null) {
                    byteCount = 2;
                    NextToken();
                    {
                        // register
                        if (LeftRightWordRegister(0b10001011, leftRegister.Value)) return true;
                    }
                    {
                        // immediate
                        var token = LastToken;
                        var value = Expression();
                        if (value != null) {
                            WriteByte(0b10111000 | leftRegister.Value);
                            WriteWord(token, value);
                            return true;
                        }
                    }
                    {
                        var rightRegister = SegmentRegisterCode(LastToken);
                        if (rightRegister != null) {
                            NextToken();
                            WriteByte(0b10001100);
                            WriteByte(0b11000000 | (rightRegister.Value << 3) | leftRegister.Value);
                            return true;
                        }
                    }
                    ParseSegmentOverride();
                    if (LastToken.IsReservedWord('[')) {
                        NextToken();
                        ParseSegmentOverride();
                        {
                            var token = LastToken;
                            var value = Expression();
                            if (value != null) {
                                AcceptReservedWord(']');
                                if (leftToken.IsReservedWord(Keyword.AX)) {
                                    WriteByte(0b10100001);
                                    WriteWord(token, value);
                                    return true;
                                }
                                WriteByte(0b10001011);
                                WriteByte(0b00000110 | (leftRegister.Value << 3));
                                WriteWord(token, value);
                                return true;
                            }
                        }
                        if (FromMemory(0b10001011, leftRegister.Value)) return true;
                    }
                }
            }
            {
                // to segment register
                var leftRegister = SegmentRegisterCode(leftToken);
                if (leftRegister != null) {
                    NextToken();
                    {
                        // register
                        if (LeftRightWordRegister(0b10001110, leftRegister.Value)) return true;
                    }
                    if (LastToken.IsReservedWord(':')) {
                        ParseSegmentOverride(leftRegister.Value);
                        goto mem;
                    }
                    if (LastToken.IsReservedWord('[')) {
                        NextToken();
                        ParseSegmentOverride();
                        if (FromMemory(0b10001110, leftRegister.Value)) return true;
                    }
                }
            }
        mem:
            ParseSegmentOverride();
            if (LastToken.IsReservedWord('[')) {
                NextToken();
                ParseSegmentOverride();
                {
                    var valueToken = LastToken;
                    var value = Expression();
                    if (value != null) {
                        AcceptReservedWord(']');
                        AcceptReservedWord(',');
                        ParseByteCount();
                        var rightToken = LastToken;
                        {
                            if (rightToken.IsReservedWord(Keyword.AL)) {
                                NextToken();
                                WriteByte(0b10100010);
                                WriteWord(valueToken, value);
                                return true;
                            }
                            if (rightToken.IsReservedWord(Keyword.AX)) {
                                NextToken();
                                WriteByte(0b10100011);
                                WriteWord(valueToken, value);
                                return true;
                            }
                        }
                        {
                            var rightRegister = ByteRegisterCode(LastToken);
                            if (rightRegister != null) {
                                NextToken();
                                WriteByte(0b10001000);
                                WriteByte(0b00000110 | (rightRegister.Value << 3));
                                WriteWord(valueToken, value);
                                return true;
                            }
                        }
                        {
                            var rightRegister = WordRegisterCode(LastToken);
                            if (rightRegister != null) {
                                NextToken();
                                WriteByte(0b10001001);
                                WriteByte(0b00000110 | (rightRegister.Value << 3));
                                WriteWord(valueToken, value);
                                return true;
                            }
                        }
                        {
                            var rightValue = Expression();
                            if (rightValue != null) {
                                switch (ByteCount) {
                                    case null:
                                        ShowUnknownType(rightToken);
                                        return false;
                                    case 1:
                                        WriteByte(0b11000110);
                                        WriteByte(0b00000110);
                                        WriteWord(valueToken, value);
                                        WriteByte(rightToken, rightValue);
                                        return true;
                                    default:
                                        WriteByte(0b11000111);
                                        WriteByte(0b00000110);
                                        WriteWord(valueToken, value);
                                        WriteWord(rightToken, rightValue);
                                        return true;
                                }
                            }
                        }
                    }
                }
                if (ToMemory((Token token, out int? register, out Address? value, out int code, out bool shortenable) =>
                {
                    code = 0;
                    shortenable = true;
                    value = null;
                    register = ByteRegisterCode(token);
                    if (register != null) {
                        NextToken();
                        code = 0b10001000;
                        return true;
                    }
                    register = WordRegisterCode(token);
                    if (register != null) {
                        NextToken();
                        code = 0b10001001;
                        return true;
                    }
                    register = SegmentRegisterCode(token);
                    if (register != null) {
                        NextToken();
                        code = 0b10001100;
                        return true;
                    }
                    value = Expression();
                    if (value != null) {
                        shortenable = false;
                        code = 0b11000110;
                        return true;
                    }
                    return false;
                })) return true;
            }
            return false;
        }

        private bool PushPop(int code1, int code2, int code3)
        {
            {
                var register = WordRegisterCode(LastToken);
                if (register != null) {
                    NextToken();
                    WriteByte(code2 | register.Value);
                    return true;
                }
            }
            {
                var register = SegmentRegisterCode(LastToken);
                if (register != null) {
                    NextToken();
                    WriteByte(code3 | (register.Value << 3));
                    return true;
                }
            }
            ParseSegmentOverride();
            if (LastToken.IsReservedWord('[')) {
                NextToken();
                ParseSegmentOverride();
                if (FromMemory(code1, 0b110)) return true;
            }
            return false;
        }

        private bool ExchangeInstruction()
        {
            {
                var leftRegister = ByteRegisterCode(LastToken);
                if (leftRegister != null) {
                    NextToken();
                    AcceptReservedWord(',');
                    ParseByteCount();
                    var rightRegister = ByteRegisterCode(LastToken);
                    if (rightRegister != null) {
                        NextToken();
                        WriteByte(0b10000110);
                        WriteByte(0b11000000 | (leftRegister.Value << 3) | rightRegister.Value);
                        return true;
                    }
                    ParseSegmentOverride();
                    if (LastToken.IsReservedWord('[')) {
                        NextToken();
                        ParseSegmentOverride();
                        if (FromMemory(0b10000110, leftRegister.Value)) return true;
                    }
                    return false;
                }
            }
            {
                var leftRegister = WordRegisterCode(LastToken);
                if (leftRegister != null) {
                    NextToken();
                    AcceptReservedWord(',');
                    ParseByteCount();
                    var rightRegister = WordRegisterCode(LastToken);
                    if (rightRegister != null) {
                        NextToken();
                        if (leftRegister == 0) {
                            WriteByte(0b10010000 | rightRegister.Value);
                            return true;
                        }
                        if (rightRegister == 0) {
                            WriteByte(0b10010000 | leftRegister.Value);
                            return true;
                        }
                        WriteByte(0b10000111);
                        WriteByte(0b11000000 | (leftRegister.Value << 3) | rightRegister.Value);
                        return true;
                    }
                    ParseSegmentOverride();
                    if (LastToken.IsReservedWord('[')) {
                        NextToken();
                        ParseSegmentOverride();
                        if (FromMemory(0b10000111, leftRegister.Value)) return true;
                    }
                    return false;
                }
            }
            return false;
        }

        private bool InputInstruction(int code1, int code2)
        {
            NextToken();
            AcceptReservedWord(',');
            if (LastToken.IsReservedWord(Keyword.DX)) {
                NextToken();
                WriteByte(code2);
                return true;
            }
            {
                var valueToken = LastToken;
                var value = Expression();
                if (value == null) return false;
                WriteByte(code1);
                WriteByte(valueToken, value);
                return true;
            }
        }
        private bool InputInstruction()
        {
            if (LastToken.IsReservedWord(Keyword.AL)) {
                if (InputInstruction(0b11100100, 0b11101100)) return true;
            }
            if (LastToken.IsReservedWord(Keyword.AX)) {
                if (InputInstruction(0b11100101, 0b11101101)) return true;
            }
            return false;
        }

        private bool OutputInstruction()
        {
            if (LastToken.IsReservedWord(Keyword.DX)) {
                NextToken();
                AcceptReservedWord(',');
                if (LastToken.IsReservedWord(Keyword.AL)) {
                    NextToken();
                    WriteByte(0b11101110);
                    return true;
                }
                if (LastToken.IsReservedWord(Keyword.AX)) {
                    NextToken();
                    WriteByte(0b11101111);
                    return true;
                }
                return false;
            }
            {
                var valueToken = LastToken;
                var value = Expression();
                if (value != null) {
                    AcceptReservedWord(',');
                    if (LastToken.IsReservedWord(Keyword.AL)) {
                        NextToken();
                        WriteByte(0b11100110);
                        WriteByte(valueToken, value);
                        return true;
                    }
                    if (LastToken.IsReservedWord(Keyword.AX)) {
                        NextToken();
                        WriteByte(0b11100111);
                        WriteByte(valueToken, value);
                        return true;
                    }
                    return false;
                }
            }
            return false;
        }

        private bool LoadPointerInstruction(int code)
        {
            ByteCount = 2;
            var leftRegister = WordRegisterCode(LastToken);
            if (leftRegister != null) {
                NextToken();
                AcceptReservedWord(',');
                ParseByteCount();
                if (FromRegisterOrMemory(code, leftRegister.Value, WordRegisterCode)) return true;
            }
            return false;
        }


        private bool BinomialInstruction(int code1, int reverseBit, int code2, int code3, int code4, bool shortenable)
        {
            ParseByteCount();
            var leftToken = LastToken;
            {
                var leftRegister = ByteRegisterCode(LastToken);
                if (leftRegister != null) {
                    NextToken();
                    ByteCount = 1;
                    AcceptReservedWord(',');
                    ParseByteCount();
                    if (leftToken.IsReservedWord(Keyword.AL)) {
                        var valueToken = LastToken;
                        var value = Expression();
                        if (value != null) {
                            WriteByte(code4);
                            WriteByte(valueToken, value);
                            return true;
                        }
                    }
                    {
                        var valueToken = LastToken;
                        var value = Expression();
                        if (value != null) {
                            WriteByte(code2);
                            WriteByte(code3 | 0b11000000 | leftRegister.Value);
                            WriteByte(valueToken, value);
                            return true;
                        }
                    }
                    return FromRegisterOrMemory(code1 | reverseBit, leftRegister.Value, ByteRegisterCode);
                }
            }
            {
                var leftRegister = WordRegisterCode(LastToken);
                if (leftRegister != null) {
                    NextToken();
                    ByteCount = 2;
                    AcceptReservedWord(',');
                    ParseByteCount();
                    if (leftToken.IsReservedWord(Keyword.AX)) {
                        var valueToken = LastToken;
                        var value = Expression();
                        if (value != null) {
                            WriteByte(code4 | 1);
                            WriteWord(valueToken, value);
                            return true;
                        }
                    }

                    {
                        var valueToken = LastToken;
                        var value = Expression();
                        if (value != null) {
                            if (IsSignedByte(value)) {
                                WriteByte(code2 | 0b11);
                                WriteByte(code3 | 0b11000000 | leftRegister.Value);
                                WriteByte(valueToken, value);
                                return true;
                            }
                            WriteByte(code2 | 0b01);
                            WriteByte(code3 | 0b11000000 | leftRegister.Value);
                            WriteWord(valueToken, value);
                            return true;
                        }
                    }
                    return FromRegisterOrMemory(code1 | reverseBit | 0b1, leftRegister.Value, WordRegisterCode);
                }
            }
            ParseSegmentOverride();
            if (!LastToken.IsReservedWord('[')) return false;
            {
                NextToken();
                ParseSegmentOverride();
                ToMemory((Token token, out int? register, out Address? value, out int code, out bool s) =>
                {
                    code = 0;
                    s = shortenable;
                    value = null;
                    register = ByteRegisterCode(token);
                    if (register != null) {
                        NextToken();
                        code = code1;
                        return true;
                    }
                    register = WordRegisterCode(token);
                    if (register != null) {
                        NextToken();
                        code = code1 | 1;
                        return true;
                    }
                    value = Expression();
                    if (value == null) return false;
                    code = code2;
                    if (ByteCount is 2) {
                        if (shortenable) {
                            if (IsSignedByte(value)) {
                                code |= 0b11;
                            }
                            else {
                                code |= 0b01;
                            }
                        }
                        else {
                            code |= 0b1;
                        }
                    }
                    register = code3 >> 3;
                    return true;
                });
                return true;
            }
        }

        private bool BinomialInstruction(int code1, int reverseBit, int code2, int code3, bool shortenable)
        {
            return BinomialInstruction(code1, reverseBit, 0b10000000, code2, code3, shortenable);
        }


        private bool MonomialInstruction(int code1, int code2)
        {
            ParseByteCount();
            return
                FromRegisterOrMemory(code1, code2 >> 3, ByteRegisterCode) ||
                FromRegisterOrMemory(code1 | 1, code2 >> 3, WordRegisterCode);
        }

        private bool IncrementOrDecrement(int code1, int code2)
        {
            {
                var register = WordRegisterCode(LastToken);
                if (register != null) {
                    NextToken();
                    WriteByte(code2 | register.Value);
                    return true;
                }
            }
            return MonomialInstruction(0b11111110, code1);
        }

        private bool ShiftRotateInstruction(int code1, int code2, Func<Token, int?> func)
        {
            int ConstOrVariable(int code)
            {
                if (!LastToken.IsReservedWord(',')) return code;
                NextToken();
                if (LastToken.IsReservedWord(Keyword.CL)) {
                    NextToken();
                    return code | 0b10;
                }
                {
                    var valueToken = LastToken;
                    var value = Expression();
                    if (value == null) return code;
                    if (!value.IsConst() || value.Value != 1) {
                        ShowError(valueToken.Position, "Must be CL or 1: " + valueToken);
                    }
                    NextToken();
                }
                return code;
            }

            {
                var register = func(LastToken);
                if (register != null) {
                    NextToken();
                    WriteByte(ConstOrVariable(code1));
                    WriteByte(0b11000000 | code2 | register.Value);
                    return true;
                }
            }
            ParseByteCount();
            ParseSegmentOverride();
            if (!LastToken.IsReservedWord('[')) return false;
            {
                NextToken();
                if (!MemoryOperand(out var rm, out var valueToken, out var value)) return false;
                WriteByte(ConstOrVariable(code1));
                if (rm != null) {
                    if (value != null) {
                        if (IsSignedByte(value)) {
                            // mod != 01
                            WriteByte(0b01000000 | code2 | rm.Value);
                            Debug.Assert(valueToken != null);
                            WriteByte(valueToken, value);
                            return true;
                        }

                        // mod != 10
                        WriteByte(0b10000000 | code2 | rm.Value);
                        Debug.Assert(valueToken != null);
                        WriteWord(valueToken, value);
                        return true;
                    }

                    // mod == 00
                    WriteByte(code2 | rm.Value);
                    return true;
                }

                if (value == null) return false;
                WriteByte(0b00000110 | code2);
                Debug.Assert(valueToken != null);
                WriteWord(valueToken, value);
                return true;
            }
        }
        private bool ShiftRotateInstruction(int code)
        {
            return
                ShiftRotateInstruction(0b11010000, code, ByteRegisterCode) ||
                ShiftRotateInstruction(0b11010001, code, WordRegisterCode);
        }


        private bool RepeatInstruction()
        {
            IDictionary<int, Func<Assembler, bool>> actions = new Dictionary<int, Func<Assembler, bool>>()
            {
                { Keyword.MOVSB,a=>a.InherentInstruction(0b10100100)},
                { Keyword.MOVSW,a=>a.InherentInstruction(0b10100101)},
                { Keyword.LODSB,a=>a.InherentInstruction(0b10101100)},
                { Keyword.LODSW,a=>a.InherentInstruction(0b10101101)},
                { Keyword.STOSB,a=>a.InherentInstruction(0b10101100)},
                { Keyword.STOSW,a=>a.InherentInstruction(0b10101101)},
            };
            if (!(LastToken is ReservedWord reservedWord)) return false;
            if (!actions.TryGetValue(reservedWord.Id, out var func)) return false;
            NextToken();
            WriteByte(0b11110010);
            return func(this);
        }

        private bool RepeatConditionInstruction(int bit)
        {
            IDictionary<int, Func<Assembler, bool>> actions = new Dictionary<int, Func<Assembler, bool>>()
            {
                { Keyword.CMPSB,a=>a.InherentInstruction(0b10100110)},
                { Keyword.CMPSW,a=>a.InherentInstruction(0b10100111)},
                { Keyword.SCASB,a=>a.InherentInstruction(0b10101110)},
                { Keyword.SCASW,a=>a.InherentInstruction(0b10101111)},
            };
            if (!(LastToken is ReservedWord reservedWord)) return false;
            if (!actions.TryGetValue(reservedWord.Id, out var func)) return false;
            NextToken();
            WriteByte(0b11110010 | bit);
            return func(this);
        }

        protected override bool IsRelativeOffsetInRange(int offset)
        {
            return offset >= -32768 && offset <= 32767;
        }


        private bool IndirectJump(int nearCode, int farCode)
        {
            {
                var rightRegister = WordRegisterCode(LastToken);
                if (rightRegister != null) {
                    NextToken();
                    WriteByte(0b11111111);
                    WriteByte(0b11000000 | nearCode | rightRegister.Value);
                    return true;
                }
            }
            int code2;
            if (LastToken.IsReservedWord(Keyword.WORD)) {
                NextToken();
                code2 = nearCode;
            }
            else if (LastToken.IsReservedWord(Keyword.FAR)) {
                NextToken();
                code2 = farCode;
            }
            else {
                ShowTypeMismatch(LastToken);
                return false;
            }
            AcceptReservedWord(Keyword.PTR);
            ParseSegmentOverride();
            if (!LastToken.IsReservedWord('[')) return false;
            NextToken();
            return FromMemory(0b11111111, code2 >> 3);
        }

        private bool CallInstruction()
        {
            var operandToken = LastToken;
            var operand = Expression();
            if (operand == null) return IndirectJump(0b00010000, 0b00011000);
            if (operand.IsConst()) {
                // far
                WriteByte(0b10011010);
                WriteWord(operandToken, operand);
                var highWord = (operand.Value & 0xf0000) >> 4;
                WriteByte(highWord);
                WriteByte(highWord >> 8);
                return true;
            }
            // near
            WriteByte(0b11101000);
            if (operand.Type != AddressType.External) {
                if (!RelativeOffset(operandToken, operand, out var offset)) {
                    offset = 0;
                }
                //else {
                //    --offset;
                //}
                WriteByte(offset);
                WriteByte(offset >> 8);
            }
            else {
                WriteWord(operandToken, operand.RelativeValue);
            }
            return true;
        }

        private bool JumpInstruction(Token operandToken, Address operand)
        {
            // near
            if (operand.Type != AddressType.External) {
                if (!RelativeOffset(operandToken, operand, out var offset)) {
                    offset = 0;
                }
                else {
                    if (IsRelativeOffsetInByte(offset)) {
                        WriteByte(0b11101011);
                        WriteByte(offset);
                        return true;
                    }
                    --offset;
                }
                WriteByte(0b11101001);
                WriteByte(offset);
                WriteByte(offset >> 8);
            }
            else {
                WriteByte(0b11101001);
                WriteWord(operandToken, operand.RelativeValue);
            }
            return true;
        }

        private bool JumpInstruction()
        {
            var operandToken = LastToken;
            var operand = Expression();
            if (operand == null) return IndirectJump(0b00100000, 0b00101000);
            if (!operand.IsConst()) {
                return JumpInstruction(operandToken, operand);
            }

            int highWord;
            if (LastToken.IsReservedWord(':')) {
                highWord = operand.Value;
                NextToken();
                operandToken = LastToken;
                operand = Expression();
                if (operand == null) {
                    return false;
                }
            }
            else {
                highWord = (operand.Value & 0xf0000) >> 4;
            }
            // far
            WriteByte(0b11101010);
            Debug.Assert(operand != null);
            WriteWord(operandToken, operand);
            WriteByte(highWord);
            WriteByte(highWord >> 8);
            return true;
        }



        private bool ReturnInstruction(int code1, int code2)
        {
            var operandToken = LastToken;
            var operand = Expression();
            if (operand != null) {
                WriteByte(code2);
                WriteWord(operandToken, operand);
                return true;
            }
            WriteByte(code1);
            return true;
        }

        private void JumpRelativeInstruction(int code, Token operandToken, Address operand)
        {
            if (RelativeOffset(operandToken, operand, out var offset) && IsRelativeOffsetInByte(offset)) {
                WriteByte(code);
                WriteByte(offset);
                return;
            }

            WriteByte(code ^ 1);
            WriteByte(3);
            offset -= 3;

            WriteByte(0b11101001);
            WriteByte(offset);
            WriteByte(offset >> 8);
        }

        private bool JumpRelativeInstruction(int code)
        {
            var operandToken = LastToken;
            var operand = Expression();
            if (operand == null) return false;
            if (!operand.IsConst()) {
                JumpRelativeInstruction(code, operandToken, operand);
                return true;
            }
            WriteByte(code ^ 1);
            WriteByte(5);
            // far
            WriteByte(0b11101010);
            WriteWord(operandToken, operand);
            var highWord = (operand.Value & 0xf0000) >> 4;
            WriteByte(highWord);
            WriteByte(highWord >> 8);
            return true;
        }

        private bool LoopInstruction(Token operandToken, Address operand, int code)
        {
            if (RelativeOffset(operandToken, operand, out var offset) && IsRelativeOffsetInByte(offset)) {
                WriteByte(code);
                WriteByte(offset);
                return true;
            }

            WriteByte(code);
            WriteByte(2);

            // JMP relative byte
            WriteByte(0b11101011);
            WriteByte(3);

            // JMP relative word
            offset -= 5;
            WriteByte(0b11101001);
            WriteByte(offset);
            WriteByte(offset >> 8);
            return true;
        }

        private bool LoopInstruction(int code)
        {
            var operandToken = LastToken;
            var operand = Expression();
            if (operand == null) return false;
            if (!operand.IsConst()) {
                return LoopInstruction(operandToken, operand, code);
            }
            WriteByte(code);
            WriteByte(2);

            WriteByte(0b11101011);
            WriteByte(5);

            // far
            WriteByte(0b11101010);
            WriteWord(operandToken, operand);
            var highWord = (operand.Value & 0xf0000) >> 4;
            WriteByte(highWord);
            WriteByte(highWord >> 8);
            return true;
        }



        private bool InterruptInstruction()
        {
            var operandToken = LastToken;
            var operand = Expression();
            if (operand == null) return false;
            if (operand.IsConst() && operand.Value == 3) {
                WriteByte(0b11001100);
                return true;
            }
            WriteByte(0b11001101);
            WriteByte(operandToken, operand);
            return true;
        }


        private static readonly IDictionary<int, int> ConditionCodes = new Dictionary<int, int>
        {
            { Keyword.E,0b01110100},
            { Keyword.Z,0b01110100},
            { Keyword.L,0b01111100},
            { Keyword.NGE,0b01111100},
            { Keyword.LE,0b01111110},
            { Keyword.NG,0b01111110},
            { Keyword.B,0b01110010},
            { Keyword.C,0b01110010},
            { Keyword.NAE,0b01110010},
            { Keyword.BE,0b01110110},
            { Keyword.NA,0b01110110},
            { Keyword.P,0b01111010},
            { Keyword.PE,0b01111010},
            { Keyword.O,0b01110000},
            { Keyword.S,0b01111000},
            { Keyword.NE,0b01110101},
            { Keyword.NZ,0b01110101},
            { Keyword.NL,0b01111101},
            { Keyword.GE,0b01111101},
            { Keyword.NLE,0b01111111},
            { Keyword.G,0b01111111},
            { Keyword.NB,0b01110011},
            { Keyword.NC,0b01110011},
            { Keyword.AE,0b01110011},
            { Keyword.NBE,0b01110111},
            { Keyword.A,0b01110111},
            { Keyword.NP,0b01111011},
            { Keyword.PO,0b01111011},
            { Keyword.NO,0b01110001},
            { Keyword.NS,0b01111001},
        };

        private bool NegatedConditionalJump(Address address)
        {
            var condition = LastToken;
            if (condition is ReservedWord reservedWord && ConditionCodes.TryGetValue(reservedWord.Id, out var code)) {
                NextToken();
                JumpRelativeInstruction(code ^ 1, condition, address);
                return true;
            }
            return false;
        }

        private bool StartIf(IfBlock block)
        {
            var address = SymbolAddress(block.ElseId);
            return NegatedConditionalJump(address);
        }

        private bool IfStatement()
        {
            var block = NewIfBlock();
            return StartIf(block);
        }

        private bool ElseStatement()
        {
            if (LastBlock() is IfBlock block) {
                if (block.ElseId <= 0) {
                    ShowError(LastToken.Position, "Multiple ELSE statement.");
                }
                var address = SymbolAddress(block.EndId);
                JumpInstruction(LastToken, address);
                DefineSymbol(block.ConsumeElse(), CurrentAddress);
            }
            else {
                ShowNoStatementError(LastToken, "IF");
            }
            return true;
        }

        private bool ElseIfStatement()
        {
            ElseStatement();
            if (!(LastBlock() is IfBlock block)) return true;
            Debug.Assert(block.ElseId == Block.InvalidId);
            block.ElseId = AutoLabel();
            StartIf(block);
            return true;
        }

        private bool EndIfStatement()
        {
            if (LastBlock() is IfBlock block) {
                DefineSymbol(block.ElseId <= 0 ? block.EndId : block.ConsumeElse(), CurrentAddress);
                EndBlock();
            }
            else {
                ShowNoStatementError(LastToken, "IF");
            }
            return true;
        }

        private bool DoStatement()
        {
            var block = NewWhileBlock();
            DefineSymbol(block.BeginId, CurrentAddress);
            return true;
        }

        private bool WhileStatement()
        {
            if (!(LastBlock() is WhileBlock block)) {
                ShowNoStatementError(LastToken, "WHILE");
                NextToken();
                return true;
            }
            var repeatAddress = SymbolAddress(block.RepeatId);
            if (repeatAddress.Type == CurrentSegment.Type && RelativeOffset(repeatAddress) <= 0) {
                var condition = LastToken;
                if (!(condition is ReservedWord reservedWord) ||
                    !ConditionCodes.TryGetValue(reservedWord.Id, out var code)) return false;
                NextToken();
                var address = SymbolAddress(block.BeginId);
                JumpRelativeInstruction(code, condition, address);
                block.EraseEndId();
                return true;
            }
            {
                var address = SymbolAddress(block.EndId);
                return NegatedConditionalJump(address);
            }
        }

        private bool WEndStatement()
        {
            if (!(LastBlock() is WhileBlock block)) {
                ShowNoStatementError(LastToken, "WHILE");
            }
            else {
                if (block.EndId > 0) {
                    DefineSymbol(block.RepeatId, CurrentAddress);
                    var address = SymbolAddress(block.BeginId);
                    if (!JumpInstruction(LastToken, address)) return false;
                    DefineSymbol(block.EndId, CurrentAddress);
                }
                EndBlock();
            }
            return true;
        }

        private bool WhileLoopStatement()
        {
            if (!(LastBlock() is WhileBlock block)) {
                ShowNoStatementError(LastToken, "WHILE");
                return true;
            }
            if (block.EndId <= 0) {
                ShowError(LastToken.Position, "WHILE and WLOOP cannot be used in the same syntax.");
            }

            int code;
            if (LastToken.IsReservedWord(Keyword.NZ) || LastToken.IsReservedWord(Keyword.NE)) {
                code = 0b11100001;
            }
            else if (LastToken.IsReservedWord(Keyword.Z) || LastToken.IsReservedWord(Keyword.E)) {
                code = 0b11100000;
            }
            else {
                code = 0b11100010;
            }
            var operandToken = LastToken;
            var address = SymbolAddress(block.BeginId);
            EndBlock();

            return LoopInstruction(operandToken, address, code);
        }

        private bool WhileDecStatement()
        {
            if (!(LastBlock() is WhileBlock block)) {
                ShowNoStatementError(LastToken, "WHILE");
                return true;
            }

            if (block.EndId <= 0) {
                ShowError(LastToken.Position, "WHILE and WCXZ cannot be used in the same syntax.");
            }
            var operandToken = LastToken;
            var address = SymbolAddress(block.BeginId);
            EndBlock();

            return LoopInstruction(operandToken, address, 0b11100011);
        }


        private void FlashInstructionBytes()
        {
            if (segmentOverride != null) {
                base.WriteByte(segmentOverride.Value);
                segmentOverride = null;
            }
            foreach (var b in instructionBytes) {
                base.WriteByte(b);
            }
            instructionBytes.Clear();
        }

        protected override Token SkipEndOfStatement()
        {
            FlashInstructionBytes();
            return base.SkipEndOfStatement();
        }


        private static readonly IDictionary<int, Func<Assembler, bool>> Actions = new Dictionary<int, Func<Assembler, bool>>()
        {
            { Keyword.MOV, a=> a.MoveInstruction() },
            { Keyword.PUSH, a=> a.PushPop(0b11111111, 0b01010000, 0b00000110) },
            { Keyword.POP, a=> a.PushPop(0b10001111, 0b01011000, 0b00000111) },
            { Keyword.XCHG, a=> a.ExchangeInstruction() },
            { Keyword.IN, a=> a.InputInstruction() },
            { Keyword.OUT, a=> a.OutputInstruction() },
            { Keyword.XLAT, a=> a.InherentInstruction(0b11010111) },
            { Keyword.LEA, a=> a.LoadPointerInstruction(0b10001101) },
            { Keyword.LDS, a=> a.LoadPointerInstruction(0b11000101) },
            { Keyword.LES, a=> a.LoadPointerInstruction(0b11000100) },
            { Keyword.LAHF, a=> a.InherentInstruction(0b10011111) },
            { Keyword.SAHF, a=> a.InherentInstruction(0b10011110) },
            { Keyword.PUSHF, a=> a.InherentInstruction(0b10011100) },
            { Keyword.POPF, a=> a.InherentInstruction(0b10011101) },
            { Keyword.ADD, a=> a.BinomialInstruction(0b00000000, 0b10,0b00000000,0b00000100, true) },
            { Keyword.ADC, a=> a.BinomialInstruction(0b00010000, 0b10,0b00010000,0b00010100, true) },
            { Keyword.INC, a => a.IncrementOrDecrement(0b00000000, 0b01000000) },
            { Keyword.AAA, a=> a.InherentInstruction(0b00110111) },
            { Keyword.DAA, a=> a.InherentInstruction(0b00100111) },
            { Keyword.SUB, a=> a.BinomialInstruction(0b00101000, 0b10,0b00101000,0b00101100, true) },
            { Keyword.SBB, a=> a.BinomialInstruction(0b00011000, 0b10,0b00011000,0b00011100, true) },
            { Keyword.DEC, a => a.IncrementOrDecrement(0b00001000, 0b01001000) },
            { Keyword.NEG, a => a.MonomialInstruction(0b11110110,0b00011000) },
            { Keyword.CMP, a=> a.BinomialInstruction(0b00111000, 0b10,0b00111000,0b00111100, true) },
            { Keyword.AAS, a=> a.InherentInstruction(0b00111111) },
            { Keyword.DAS, a=> a.InherentInstruction(0b00101111) },
            { Keyword.MUL, a => a.MonomialInstruction(0b11110110,0b00100000) },
            { Keyword.IMUL, a => a.MonomialInstruction(0b11110110,0b00101000) },
            { Keyword.AAM, a=> a.InherentInstruction(0b11010100,0b00001010) },
            { Keyword.DIV, a => a.MonomialInstruction(0b11110110,0b00110000) },
            { Keyword.IDIV, a => a.MonomialInstruction(0b11110110,0b00111000) },
            { Keyword.AAD, a=> a.InherentInstruction(0b11010101,0b00001010) },
            { Keyword.CBW, a=> a.InherentInstruction(0b10011000) },
            { Keyword.CWD, a=> a.InherentInstruction(0b10011001) },
            { Inu.Assembler.Keyword.Not, a => a.MonomialInstruction(0b11110110,0b00010000) },
            { Inu.Assembler.Keyword.Shl, a => a.ShiftRotateInstruction(0b00100000) },
            { Keyword.SAL, a => a.ShiftRotateInstruction(0b00100000) },
            { Inu.Assembler.Keyword.Shr, a => a.ShiftRotateInstruction(0b00101000) },
            { Keyword.SAR, a => a.ShiftRotateInstruction(0b00111000) },
            { Keyword.ROL, a => a.ShiftRotateInstruction(0b00000000) },
            { Keyword.ROR, a => a.ShiftRotateInstruction(0b00001000) },
            { Keyword.RCL, a => a.ShiftRotateInstruction(0b00010000) },
            { Keyword.RCR, a => a.ShiftRotateInstruction(0b00011000) },
            { Inu.Assembler.Keyword.And, a=> a.BinomialInstruction(0b00100000, 0b10,0b00100000,0b00100100, false) },
            { Keyword.TEST, a=> a.BinomialInstruction(0b10000100, 0,0b11110110,0b00000000,0b10101000, false) },
            { Inu.Assembler.Keyword.Or, a=> a.BinomialInstruction(0b00001000, 0b10,0b00001000,0b00001100, false) },
            { Inu.Assembler.Keyword.Xor, a=> a.BinomialInstruction(0b00110000, 0b10,0b00110000,0b00110100, false) },
            { Keyword.MOVSB,a=>a.InherentInstruction(0b10100100)},
            { Keyword.MOVSW,a=>a.InherentInstruction(0b10100101)},
            { Keyword.CMPSB,a=>a.InherentInstruction(0b10100110)},
            { Keyword.CMPSW,a=>a.InherentInstruction(0b10100111)},
            { Keyword.SCASB,a=>a.InherentInstruction(0b10101110)},
            { Keyword.SCASW,a=>a.InherentInstruction(0b10101111)},
            { Keyword.LODSB,a=>a.InherentInstruction(0b10101100)},
            { Keyword.LODSW,a=>a.InherentInstruction(0b10101101)},
            { Keyword.STOSB,a=>a.InherentInstruction(0b10101100)},
            { Keyword.STOSW,a=>a.InherentInstruction(0b10101101)},
            { Keyword.REP,a=>a.RepeatInstruction()},
            { Keyword.REPE,a=>a.RepeatConditionInstruction(1)},
            { Keyword.REPNE,a=>a.RepeatConditionInstruction(0)},
            { Keyword.REPZ,a=>a.RepeatConditionInstruction(1)},
            { Keyword.REPNZ,a=>a.RepeatConditionInstruction(0)},
            { Keyword.CALL,a=>a.CallInstruction()},
            { Keyword.JMP,a=>a.JumpInstruction()},
            { Keyword.RET,a=>a.ReturnInstruction(0b11000011,0b11000010)},
            { Keyword.RETN,a=>a.ReturnInstruction(0b11000011,0b11000010)},
            { Keyword.RETF,a=>a.ReturnInstruction(0b11001011,0b11001010)},
            { Keyword.JE,a=>a.JumpRelativeInstruction(0b01110100)},
            { Keyword.JZ,a=>a.JumpRelativeInstruction(0b01110100)},
            { Keyword.JL,a=>a.JumpRelativeInstruction(0b01111100)},
            { Keyword.JNGE,a=>a.JumpRelativeInstruction(0b01111100)},
            { Keyword.JLE,a=>a.JumpRelativeInstruction(0b01111110)},
            { Keyword.JNG,a=>a.JumpRelativeInstruction(0b01111110)},
            { Keyword.JB,a=>a.JumpRelativeInstruction(0b01110010)},
            { Keyword.JNAE,a=>a.JumpRelativeInstruction(0b01110010)},
            { Keyword.JBE,a=>a.JumpRelativeInstruction(0b01110110)},
            { Keyword.JNA,a=>a.JumpRelativeInstruction(0b01110110)},
            { Keyword.JP,a=>a.JumpRelativeInstruction(0b01111010)},
            { Keyword.JPE,a=>a.JumpRelativeInstruction(0b01111010)},
            { Keyword.JO,a=>a.JumpRelativeInstruction(0b01110000)},
            { Keyword.JS,a=>a.JumpRelativeInstruction(0b01111000)},
            { Keyword.JNE,a=>a.JumpRelativeInstruction(0b01110101)},
            { Keyword.JNZ,a=>a.JumpRelativeInstruction(0b01110101)},
            { Keyword.JNL,a=>a.JumpRelativeInstruction(0b01111101)},
            { Keyword.JGE,a=>a.JumpRelativeInstruction(0b01111101)},
            { Keyword.JNLE,a=>a.JumpRelativeInstruction(0b01111111)},
            { Keyword.JG,a=>a.JumpRelativeInstruction(0b01111111)},
            { Keyword.JNB,a=>a.JumpRelativeInstruction(0b01110011)},
            { Keyword.JAE,a=>a.JumpRelativeInstruction(0b01110011)},
            { Keyword.JNBE,a=>a.JumpRelativeInstruction(0b01110111)},
            { Keyword.JA,a=>a.JumpRelativeInstruction(0b01110111)},
            { Keyword.JNP,a=>a.JumpRelativeInstruction(0b01111011)},
            { Keyword.JPO,a=>a.JumpRelativeInstruction(0b01111011)},
            { Keyword.JNO,a=>a.JumpRelativeInstruction(0b01110001)},
            { Keyword.JNS,a=>a.JumpRelativeInstruction(0b01111001)},
            { Keyword.LOOP,a=>a.LoopInstruction(0b11100010)},
            { Keyword.LOOPE,a=>a.LoopInstruction(0b11100001)},
            { Keyword.LOOPZ,a=>a.LoopInstruction(0b11100001)},
            { Keyword.LOOPNE,a=>a.LoopInstruction(0b11100000)},
            { Keyword.LOOPNZ,a=>a.LoopInstruction(0b11100000)},
            { Keyword.JCXZ,a=>a.LoopInstruction(0b11100011)},
            { Keyword.INT,a=>a.InterruptInstruction()},
            { Keyword.INTO,a=>a.InherentInstruction(0b11001110)},
            { Keyword.IRET,a=>a.InherentInstruction(0b11001111)},
            { Keyword.CLC,a=>a.InherentInstruction(0b11111000)},
            { Keyword.CMC,a=>a.InherentInstruction(0b11110101)},
            { Keyword.STC,a=>a.InherentInstruction(0b11111001)},
            { Keyword.CLD,a=>a.InherentInstruction(0b11111100)},
            { Keyword.STD,a=>a.InherentInstruction(0b11111101)},
            { Keyword.CLI,a=>a.InherentInstruction(0b11111010)},
            { Keyword.STI,a=>a.InherentInstruction(0b11111011)},
            { Keyword.HLT,a=>a.InherentInstruction(0b11110100)},
            { Keyword.WAIT,a=>a.InherentInstruction(0b10011011)},
            //{ Keyword.ESC, a => a.MonomialInstruction(0b11011000,0) },
            { Keyword.LOCK,a=>a.InherentInstruction(0b11110000)},
            //
            {Inu.Assembler.Keyword.If, a=> a.IfStatement() },
            {Inu.Assembler.Keyword.Else, a=> a.ElseStatement() },
            {Inu.Assembler.Keyword.EndIf,a=> a.EndIfStatement() },
            {Inu.Assembler.Keyword.ElseIf, a=> a.ElseIfStatement() },
            {Inu.Assembler.Keyword.Do, a=> a.DoStatement() },
            {Inu.Assembler.Keyword.While, a=> a.WhileStatement() },
            {Inu.Assembler.Keyword.WEnd, a=> a.WEndStatement() },
            {Keyword.WLOOP,a=>a.WhileLoopStatement() },
            {Keyword.WCXZ,a=>a.WhileDecStatement() },
        };

        protected override bool Instruction()
        {
            var reservedWord = LastToken as ReservedWord;
            Debug.Assert(reservedWord != null);
            if (!Actions.TryGetValue(reservedWord.Id, out var func)) return false;
            NextToken();
            ByteCount = null;
            if (!func(this)) {
                ShowSyntaxError(LastToken);
            }
            FlashInstructionBytes();
            return true;
        }
    }
}
