using Inu.Language;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Inu.Assembler.Tlcs900;

internal class Assembler() : LittleEndianAssembler(new Tokenizer())
{
    private int? ConstantExpression()
    {
        var token = LastToken;
        var address = Expression();
        if (address == null) return null;
        if (!address.IsConst()) {
            ShowAddressUsageError(token);
        }

        return address.Value;
    }

    private static bool IsByte(int value)
    {
        return value is >= 0 and <= 0xff;
    }

    private static bool IsSignedByte(int value)
    {
        return value is >= -0x80 and <= 0x7f;
    }

    private static bool IsWord(int value)
    {
        return value is >= 0 and <= 0xffff;
    }

    private bool IsWord(Address address)
    {
        return address.IsConst() && IsWord(address.Value);
    }


    private static bool IsSignedWord(int value)
    {
        return value is >= -0x8000 and <= 0x7fff;
    }

    enum OperandSize
    {
        Byte,
        Word,
        DoubleWord
    };

    private static int SizeBits2(OperandSize size)
    {
        return size switch
        {
            OperandSize.Byte => 0b00,
            OperandSize.Word => 0b01,
            OperandSize.DoubleWord => 0b10,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private int SizeBits3(OperandSize size)
    {
        return size switch
        {
            OperandSize.Byte => 0b010,
            OperandSize.Word => 0b011,
            OperandSize.DoubleWord => 0b100,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private void ShowSizeMismatch(Token token)
    {
        ShowError(token.Position, "Size mismatch: " + token);
    }

    private int? ReservedWordIndex(int[] array)
    {
        if (LastToken is not ReservedWord reservedWord) return null;
        var index = Array.IndexOf(array, reservedWord.Id);
        if (index < 0) return null;
        NextToken();
        return index;
    }

    private bool ParseReservedWord(int id)
    {
        if (!LastToken.IsReservedWord(id)) return false;
        NextToken();
        return true;
    }

    private const int WA = 0xe0;
    private const int A = 0xe0;
    private const int B = 0xe4;
    private const int XDE = 0xe8;
    private const int XHL = 0xec;
    private const int XIX = 0xf0;
    private const int XIY = 0xf4;

    private int? ParseByteRegister()
    {
        var index = ReservedWordIndex([
            Keyword.RA0, Keyword.RW0, Keyword.QA0, Keyword.QW0,
            Keyword.RC0, Keyword.RB0, Keyword.QC0, Keyword.QB0,
            Keyword.RE0, Keyword.RD0, Keyword.QE0, Keyword.QD0,
            Keyword.RL0, Keyword.RH0, Keyword.QL0, Keyword.QH0,
            Keyword.RA1, Keyword.RW1, Keyword.QA1, Keyword.QW1,
            Keyword.RC1, Keyword.RB1, Keyword.QC1, Keyword.QB1,
            Keyword.RE1, Keyword.RD1, Keyword.QE1, Keyword.QD1,
            Keyword.RL1, Keyword.RH1, Keyword.QL1, Keyword.QH1,
            Keyword.RA2, Keyword.RW2, Keyword.QA2, Keyword.QW2,
            Keyword.RC2, Keyword.RB2, Keyword.QC2, Keyword.QB2,
            Keyword.RE2, Keyword.RD2, Keyword.QE2, Keyword.QD2,
            Keyword.RL2, Keyword.RH2, Keyword.QL2, Keyword.QH2,
            Keyword.RA3, Keyword.RW3, Keyword.QA3, Keyword.QW3,
            Keyword.RC3, Keyword.RB3, Keyword.QC3, Keyword.QB3,
            Keyword.RE3, Keyword.RD3, Keyword.QE3, Keyword.QD3,
            Keyword.RL3, Keyword.RH3, Keyword.QL3, Keyword.QH3,
        ]);
        if (index != null) return index;

        index = ReservedWordIndex([
            Keyword.ADash, Keyword.WADash, Keyword.QADash, Keyword.QWDash,
            Keyword.CDash, Keyword.BDash, Keyword.QCDash, Keyword.QBDash,
            Keyword.EDash, Keyword.Ddash, Keyword.QEDash, Keyword.QDDash,
            Keyword.LDash, Keyword.Hdash, Keyword.QLDash, Keyword.QHDash,
            Keyword.A, Keyword.W, Keyword.QA, Keyword.QW,
            Keyword.C, Keyword.B, Keyword.QC, Keyword.QB,
            Keyword.E, Keyword.D, Keyword.QE, Keyword.QD,
            Keyword.L, Keyword.H, Keyword.QL, Keyword.QH,
            Keyword.IXL, Keyword.IXH, Keyword.QIXL, Keyword.QIXH,
            Keyword.IYL, Keyword.IYH, Keyword.QIYL, Keyword.QIYH,
            Keyword.IZL, Keyword.IZH, Keyword.QIZL, Keyword.QIZH,
            Keyword.SPL, Keyword.SPH, Keyword.QSPL, Keyword.QSPH,
        ]);
        return 0xd0 + index;
    }

    private int? ParseWordRegister()
    {
        var index = ReservedWordIndex([
            Keyword.RWA0, Keyword.QWA0, Keyword.RBC0, Keyword.QBC0,
            Keyword.RDE0, Keyword.QDE0, Keyword.RHL0, Keyword.QHL0,
            Keyword.RWA1, Keyword.QWA1, Keyword.RBC1, Keyword.QBC1,
            Keyword.RDE1, Keyword.QDE1, Keyword.RHL1, Keyword.QHL1,
            Keyword.RWA2, Keyword.QWA2, Keyword.RBC2, Keyword.QBC2,
            Keyword.RDE2, Keyword.QDE2, Keyword.RHL2, Keyword.QHL2,
            Keyword.RWA3, Keyword.QWA3, Keyword.RBC3, Keyword.QBC3,
            Keyword.RDE3, Keyword.QDE3, Keyword.RHL3, Keyword.QHL3,
        ]);
        if (index != null) return index * 2;

        index = ReservedWordIndex([
            Keyword.WADash, Keyword.QWADash, Keyword.BCDash, Keyword.QBCDash,
            Keyword.DEDash, Keyword.QDEDash, Keyword.HLDash, Keyword.QHLDash,
            Keyword.WA, Keyword.QWA, Keyword.BC, Keyword.QBC,
            Keyword.DE, Keyword.QDE, Keyword.HL, Keyword.QHL,
            Keyword.IX, Keyword.QIX, Keyword.IY, Keyword.QIY,
            Keyword.IZ, Keyword.QIZ, Keyword.SP, Keyword.QSP,
        ]);
        return 0xd0 + index * 2;
    }

    private int? ParseDoubleWordRegister()
    {
        var index = ReservedWordIndex([
            Keyword.XWA0,Keyword.XBC0,Keyword.XDE0,Keyword.XHL0,
            Keyword.XWA1,Keyword.XBC1,Keyword.XDE1,Keyword.XHL1,
            Keyword.XWA2,Keyword.XBC2,Keyword.XDE2,Keyword.XHL2,
            Keyword.XWA3,Keyword.XBC3,Keyword.XDE3,Keyword.XHL3,
        ]);
        if (index != null) return index * 4;

        index = ReservedWordIndex([
            Keyword.XWADash,Keyword.XBCDash,Keyword.XDEDash,Keyword.XHLDash,
            Keyword.XWA,Keyword.XBC,Keyword.XDE,Keyword.XHL,
            Keyword.XIX,Keyword.XIY,Keyword.XIZ,Keyword.XSP,
        ]);
        return 0xd0 + index * 4;
    }

    private int? ParseRegister(out OperandSize size)
    {
        size = OperandSize.Byte;
        var register = ParseByteRegister();
        if (register != null) {
            size = OperandSize.Byte;
            return register;
        }

        register = ParseWordRegister();
        if (register != null) {
            size = OperandSize.Word;
            return register;
        }

        register = ParseDoubleWordRegister();
        if (register != null) {
            size = OperandSize.DoubleWord;
            return register;
        }

        return null;
    }

    private static int? ToRegularRegister(int register, OperandSize size)
    {
        int[] array = size switch
        {
            OperandSize.Byte => [0xe1, 0xe0, 0xe5, 0xe4, 0xe9, 0xe8, 0xed, 0xec],
            OperandSize.Word or OperandSize.DoubleWord => [0xe0, 0xe4, 0xe8, 0xec, 0xf0, 0xf4, 0xf8, 0xfc],
            _ => throw new ArgumentOutOfRangeException()
        };
        var index = Array.IndexOf(array, register);
        if (index >= 0) return index;
        return null;
    }

    private int ToRegularRegisterSurely(Token token, int register, OperandSize size)
    {
        var regularRegister = ToRegularRegister(register, size);
        if (regularRegister != null) return regularRegister.Value;
        ShowInvalidRegister(token);
        return 0;
    }

    private void WriteRegisterCode(int code, int register, OperandSize size)
    {
        var regularRegister = ToRegularRegister(register, size);
        if (regularRegister != null) {
            WriteByte((code & 0xf0) | 0x08 | regularRegister.Value);
        }
        else {
            WriteByte((code & 0xf0) | 0x07);
            WriteByte(register);
        }
    }

    private void WriteRegisterCodeZ1(int code, int register, OperandSize size)
    {
        var sizeBits = size == OperandSize.Word ? 1 : 0;
        code |= (sizeBits << 4);
        WriteRegisterCode(code, register, size);
    }

    private void WriteRegisterCodeZ2(int code, int register, OperandSize size)
    {
        var sizeBits = SizeBits2(size);
        code |= (sizeBits << 4);
        WriteRegisterCode(code, register, size);
    }


    internal abstract class Addressing
    {
        public abstract void Write(Assembler a, int code, int sizeBits);
    }

    private class RegisterIndirectAddressing(int pointerRegister) : Addressing
    {
        public override void Write(Assembler a, int code, int sizeBits)
        {
            var pointerRegularRegister = ToRegularRegister(pointerRegister, OperandSize.DoubleWord);
            if (pointerRegularRegister != null) {
                // R32
                a.WriteByte(code | 0b00000000 | pointerRegularRegister.Value);
                return;
            }
            // r32
            a.WriteByte(code | 0b01000011);
            a.WriteByte(0xe0 | (pointerRegister & 0b11111100) | 0b00);
        }
    }

    private class RegisterIndirectPreDecrementAddressing(int pointerRegister) : Addressing
    {
        public override void Write(Assembler a, int code, int sizeBits)
        {
            a.WriteByte(code | 0b01000100);
            a.WriteByte(0xe0 | (pointerRegister & 0b11111100) | sizeBits);
        }
    }

    private class RegisterIndirectPostIncrementAddressing(int pointerRegister) : Addressing
    {
        public override void Write(Assembler a, int code, int sizeBits)
        {
            a.WriteByte(code | 0b01000101);
            a.WriteByte(0xe0 | (pointerRegister & 0b11111100) | sizeBits);
        }
    }

    internal class IndexAddressing(int pointerRegister, int offset) : Addressing
    {
        public override void Write(Assembler a, int code, int sizeBits)
        {
            var pointerRegularRegister = ToRegularRegister(pointerRegister, OperandSize.DoubleWord);
            if (IsSignedByte(offset) && pointerRegularRegister != null) {
                // R32+d8
                a.WriteByte(code | 0b00001000 | pointerRegularRegister.Value);
                a.WriteByte(offset);
            }
            else {
                // r32+d16
                a.WriteByte(code | 0b01000011);
                a.WriteByte(0xe0 | (pointerRegister & 0b11111100) | 0b01);
                a.WriteWord(offset);
            }
        }
    }

    private class RegisterIndexAddressing(int pointerRegister, Token token, int offsetRegister, OperandSize offsetSize) : Addressing
    {
        public override void Write(Assembler a, int code, int sizeBits)
        {
            var secondByte = 0b00000011;
            switch (offsetSize) {
                case OperandSize.Byte:
                    // r32+r8
                    secondByte = 0b00000011;
                    break;
                case OperandSize.Word:
                    // r32+r16
                    secondByte = 0b00000111;
                    break;
                case OperandSize.DoubleWord:
                default:
                    a.ShowInvalidRegister(token);
                    break;
            }
            a.WriteByte(code | 0b01000011);
            a.WriteByte(secondByte);
            a.WriteByte(pointerRegister);
            a.WriteByte(offsetRegister);
        }
    }

    private class AbsoluteAddressing(Token token, Address address) : Addressing
    {
        public readonly Token Token = token;
        public readonly Address Address = address;

        public override void Write(Assembler a, int code, int sizeBits)
        {
            if (Address.IsConst()) {
                var value = Address.Value;
                if (IsByte(value)) {
                    a.WriteByte(code | 0b01000000);
                    a.WriteByte(value);
                }
                else if (IsWord(value)) {
                    a.WriteByte(code | 0b01000001);
                    a.WriteWord(value);
                }
                else {
                    a.WriteByte(code | 0b01000010);
                    a.WriteTripleByte(value);
                }
            }
            else {
                a.WriteByte(code | 0b01000010);
                a.WriteTripleByte(Token, Address);
            }
        }
    }

    private Addressing? ParseAddressing()
    {
        if (ParseReservedWord('-')) {
            var pointerRegister = ParseDoubleWordRegister();
            if (pointerRegister != null) {
                return new RegisterIndirectPreDecrementAddressing(pointerRegister.Value);
            }
        }
        {
            var pointerRegister = ParseDoubleWordRegister();
            if (pointerRegister != null) {
                if (ParseReservedWord('+')) {
                    {
                        var token = LastToken;
                        var offsetRegister = ParseRegister(out var offsetSize);
                        if (offsetRegister != null) {
                            return new RegisterIndexAddressing(pointerRegister.Value, token, offsetRegister.Value, offsetSize);
                        }
                    }
                    {
                        var offset = ConstantExpression();
                        if (offset != null) {
                            return new IndexAddressing(pointerRegister.Value, offset.Value);
                        }
                    }
                    return new RegisterIndirectPostIncrementAddressing(pointerRegister.Value);
                }
                {
                    var offset = ConstantExpression();
                    if (offset != null) {
                        return new IndexAddressing(pointerRegister.Value, offset.Value);
                    }
                }
                return new RegisterIndirectAddressing(pointerRegister.Value);
            }
            {
                var token = LastToken;
                var address = Expression();
                if (address != null) {
                    return new AbsoluteAddressing(token, address);
                }
            }
        }
        return null;
    }

    private int? ParseControlRegister(out OperandSize size)
    {
        var index = ReservedWordIndex([
            Keyword.DMAS0,Keyword.DMAS1,Keyword.DMAS2,Keyword.DMAS3,Keyword.DMAS4,Keyword.DMAS5,Keyword.DMAS6,Keyword.DMAS7,
            Keyword.DMAD0,Keyword.DMAD1,Keyword.DMAD2,Keyword.DMAD3,Keyword.DMAD4,Keyword.DMAD5,Keyword.DMAD6,Keyword.DMAD7,
        ]);
        if (index >= 0) {
            size = OperandSize.DoubleWord;
            return index * 4 + 0x00;
        }

        index = ReservedWordIndex([
            Keyword.DMAC0,Keyword.DMAC1,Keyword.DMAC2,Keyword.DMAC3,Keyword.DMAC4,Keyword.DMAC5,Keyword.DMAC6,Keyword.DMAC7,
        ]);
        if (index > 0) {
            size = OperandSize.Word;
            return index * 4 + 0x40;
        }
        if (LastToken.IsReservedWord(Keyword.INTNEST)) {
            NextToken();
            size = OperandSize.Word;
            return 0x7c;
        }

        size = OperandSize.Byte;
        index = ReservedWordIndex([
            Keyword.DMAM0,Keyword.DMAM1,Keyword.DMAM2,Keyword.DMAM3,Keyword.DMAM4,Keyword.DMAM5,Keyword.DMAM6,Keyword.DMAM7,
        ]);
        if (index >= 0) {
            return index * 4 + 0x42;
        }
        return null;
    }


    private const int Always = 0b1000;

    private int? ParseConditionCode()
    {
        var dictionary = new Dictionary<int, int>()
        {
            {Keyword.F,0b0000},
            {Keyword.Z,0b0110},{Keyword.NZ,0b1110},
            {Keyword.C,0b0111},{Keyword.NC,0b1111},
            {Keyword.PL,0b1101},{Keyword.P,0b1101},{Keyword.MI,0b0101},{Keyword.M,0b0101},
            {Keyword.NE,0b1110},{Keyword.EQ,0b0110},
            {Keyword.OV,0b0100},{Keyword.NOV,0b1100},
            {Keyword.PE,0b0100},{Keyword.PO,0b1100},
            {Keyword.GE,0b1001},{Keyword.LT,0b0001},
            {Keyword.GT,0b1010},{Keyword.LE,0b0010},
            {Keyword.UGE,0b1111},{Keyword.ULT,0b0111},
            {Keyword.UGT,0b1011},{Keyword.ULE,0b0011},
        };
        if (LastToken is ReservedWord reservedWord && dictionary.TryGetValue(reservedWord.Id, out var code)) {
            NextToken();
            return code;
        }
        return null;
    }



    private void Load()
    {
        if (ParseReservedWord('(')) {
            {
                var destinationAddressing = ParseAddressing();
                if (destinationAddressing != null) {
                    AcceptReservedWord(')');
                    AcceptReservedWord(',');
                    {
                        var token = LastToken;
                        var sourceRegister = ParseRegister(out var sourceSize);
                        if (sourceRegister != null) {
                            var sourceRegularRegister = ToRegularRegisterSurely(token, sourceRegister.Value, sourceSize);
                            var sizeBits = SizeBits2(sourceSize);
                            // LD (mem),R
                            destinationAddressing.Write(this, 0b10110000, sizeBits);
                            WriteByte(0b01000000 | (sizeBits << 4) | sourceRegularRegister);
                            return;
                        }
                    }
                    if (ParseReservedWord('(')) {
                        var sourceAddressing = ParseAddressing();
                        if (sourceAddressing != null) {
                            AcceptReservedWord(')');
                            if (destinationAddressing is AbsoluteAddressing destinationAbsoluteAddressing) {
                                var destinationAddress = destinationAbsoluteAddressing.Address;
                                if (IsWord(destinationAddress)) {
                                    // LD (#16),(mem)
                                    sourceAddressing.Write(this, 0b10000000, 0);
                                    WriteByte(0b00011001);
                                    WriteWord(destinationAbsoluteAddressing.Token, destinationAbsoluteAddressing.Address);
                                    return;
                                }
                            }
                            if (sourceAddressing is AbsoluteAddressing sourceAbsoluteAddressing) {
                                var sourceAddress = sourceAbsoluteAddressing.Address;
                                if (IsWord(sourceAddress)) {
                                    // LD (mem),(#16)
                                    destinationAddressing.Write(this, 0b10110000, 0);
                                    WriteByte(0b00010100);
                                    WriteWord(sourceAbsoluteAddressing.Token, sourceAbsoluteAddressing.Address);
                                    return;
                                }
                            }
                        }
                    }
                    {
                        var valueToken = LastToken;
                        var value = Expression();
                        if (value != null) {
                            if (destinationAddressing is AbsoluteAddressing absoluteAddressing) {
                                var address = absoluteAddressing.Address;
                                if (address.IsConst()) {
                                    var constAddress = address.Value;
                                    if (IsByte(constAddress)) {
                                        // LD (#8),#
                                        WriteByte(0b00001000);
                                        WriteByte(constAddress);
                                        WriteByte(valueToken, value);
                                        return;
                                    }
                                }
                            }
                            // LD (mem),#
                            destinationAddressing.Write(this, 0b10110000, 0);
                            WriteByte(0b00000000);
                            WriteByte(valueToken, value);
                            return;
                        }
                    }
                }
            }
        }
        {
            var destinationToken = LastToken;
            var destinationRegister = ParseRegister(out var destinationSize);
            if (destinationRegister != null) {
                AcceptReservedWord(',');
                {
                    var token = LastToken;
                    var sourceRegister = ParseRegister(out var sourceSize);
                    if (sourceRegister != null) {
                        if (destinationSize != sourceSize) {
                            ShowSizeMismatch(token);
                        }
                        {
                            var destinationRegularRegister = ToRegularRegister(destinationRegister.Value, destinationSize);
                            if (destinationRegularRegister != null) {
                                // LD R,r
                                WriteRegisterCodeZ2(0b11001000, sourceRegister.Value, sourceSize);
                                WriteByte(0b10001000 | destinationRegularRegister.Value);
                                return;
                            }
                            {
                                var sourceRegularRegister = ToRegularRegisterSurely(token, sourceRegister.Value, sourceSize);
                                var sizeBits = SizeBits2(destinationSize);
                                // LD r,R
                                WriteByte(0b11000111 | (sizeBits << 4));
                                WriteByte(destinationRegister.Value);
                                WriteByte(0b10011000 | sourceRegularRegister);
                                return;
                            }
                        }
                    }
                }
                {
                    var token = LastToken;
                    var sourceRegister = ParseRegister(out var sourceSize);
                    if (sourceRegister != null) {
                        if (destinationSize != sourceSize) {
                            ShowSizeMismatch(token);
                        }
                        var sizeBits = SizeBits2(destinationSize);
                        // LD R,r
                        WriteByte(0b11000111 | (sizeBits << 4));
                        WriteByte(sourceRegister.Value);
                        WriteByte(0b10001000 | destinationRegister.Value);
                        return;
                    }
                }
                if (ParseReservedWord('(')) {
                    var destinationRegularRegister = ToRegularRegisterSurely(destinationToken, destinationRegister.Value, destinationSize);
                    var addressing = ParseAddressing();
                    if (addressing != null) {
                        AcceptReservedWord(')');
                        var sizeBits = SizeBits2(destinationSize);
                        // LD R,(mem)
                        addressing.Write(this, 0b10000000 | (sizeBits << 4), sizeBits);
                        WriteByte(0b00100000 | destinationRegularRegister);
                        return;
                    }
                }
                {
                    var token = LastToken;
                    var address = Expression();
                    if (address != null) {
                        if (address.IsConst()) {
                            var value = address.Value;
                            if (value is >= 0 and < 8) {
                                // LD r,#3
                                WriteRegisterCodeZ2(0b11001000, destinationRegister.Value, destinationSize);
                                WriteByte(0b10101000 | value);
                                return;
                            }
                        }
                        {
                            var destinationRegularRegister = ToRegularRegister(destinationRegister.Value, destinationSize);
                            if (destinationRegularRegister != null) {
                                var sizeBits = SizeBits3(destinationSize);
                                // LD R,#
                                WriteByte(0b0000000 | (sizeBits << 4) | destinationRegularRegister.Value);
                            }
                            else {
                                var sizeBits = SizeBits2(destinationSize);
                                // LD r,#
                                WriteByte(0b11000111 | (sizeBits << 4));
                                WriteByte(destinationRegister.Value);
                                WriteByte(0b00000011);
                            }
                            switch (destinationSize) {
                                case OperandSize.Byte:
                                    WriteByte(token, address);
                                    break;
                                case OperandSize.Word:
                                    WriteWord(token, address);
                                    break;
                                case OperandSize.DoubleWord:
                                    WriteDoubleWord(token, address);
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                            return;
                        }
                    }
                }
            }
        }
        ShowSyntaxError(LastToken);
    }


    private void LoadWord()
    {
        if (ParseReservedWord('(')) {
            {
                var destinationAddressing = ParseAddressing();
                if (destinationAddressing != null) {
                    AcceptReservedWord(')');
                    AcceptReservedWord(',');
                    if (ParseReservedWord('(')) {
                        var sourceAddressing = ParseAddressing();
                        if (sourceAddressing != null) {
                            AcceptReservedWord(')');
                            if (destinationAddressing is AbsoluteAddressing destinationAbsoluteAddressing) {
                                var destinationAddress = destinationAbsoluteAddressing.Address;
                                if (IsWord(destinationAddress)) {
                                    // LDW (#16),(mem)
                                    sourceAddressing.Write(this, 0b10010000, 1);
                                    WriteByte(0b00011001);
                                    WriteWord(destinationAbsoluteAddressing.Token, destinationAbsoluteAddressing.Address);
                                    return;
                                }
                            }
                            if (sourceAddressing is AbsoluteAddressing sourceAbsoluteAddressing) {
                                var sourceAddress = sourceAbsoluteAddressing.Address;
                                if (IsWord(sourceAddress)) {
                                    // LDW (mem),(#16)
                                    destinationAddressing.Write(this, 0b10110000, 1);
                                    WriteByte(0b00010110);
                                    WriteWord(sourceAbsoluteAddressing.Token, sourceAbsoluteAddressing.Address);
                                    return;
                                }
                            }
                        }
                    }
                    {
                        var valueToken = LastToken;
                        var value = Expression();
                        if (value != null) {
                            if (destinationAddressing is AbsoluteAddressing absoluteAddressing) {
                                var address = absoluteAddressing.Address;
                                if (address.IsConst()) {
                                    var constAddress = address.Value;
                                    if (IsByte(constAddress)) {
                                        // LDW (#8),#
                                        WriteByte(0b00001010);
                                        WriteByte(constAddress);
                                        WriteWord(valueToken, value);
                                        return;
                                    }
                                }
                            }
                            // LDW (mem),#
                            destinationAddressing.Write(this, 0b10110000, 1);
                            WriteByte(0b00000010);
                            WriteWord(valueToken, value);
                            return;
                        }
                    }
                }
            }
        }
        ShowSyntaxError(LastToken);
    }


    private void Push()
    {
        if (ParseReservedWord(Keyword.F)) {
            // PUSH F
            WriteByte(0b00011000);
            return;
        }
        if (ParseReservedWord(Keyword.A)) {
            // PUSH A
            WriteByte(0b00010100);
            return;
        }

        if (ParseReservedWord(Keyword.SR)) {
            // PUSH SR
            WriteByte(0b00000010);
            return;
        }
        {
            var register = ParseRegister(out var size);
            if (register != null) {
                switch (size) {
                    case OperandSize.Byte: {
                            var byteRegularRegister = ToRegularRegister(register.Value, size);
                            if (byteRegularRegister != null) {
                                var sizeBits = SizeBits2(size);
                                // push R8
                                WriteByte(0b11001000 | (sizeBits << 4) | byteRegularRegister.Value);
                                WriteByte(0b00000100);
                                return;
                            }
                            break;
                        }
                    case OperandSize.Word: {
                            var wordRegularRegister = ToRegularRegister(register.Value, size);
                            if (wordRegularRegister != null) {
                                // PUSH R16
                                WriteByte(0b00101000 | wordRegularRegister.Value);
                                return;
                            }
                            break;
                        }
                    case OperandSize.DoubleWord: {
                            var doubleWordRegularRegister = ToRegularRegister(register.Value, size);
                            if (doubleWordRegularRegister != null) {
                                // PUSH R32
                                WriteByte(0b00111000 | doubleWordRegularRegister.Value);
                                return;
                            }
                            break;
                        }
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                {
                    var sizeBits = SizeBits2(size);
                    // PUSH r
                    WriteByte(0b11000111 | (sizeBits << 4));
                    WriteByte(register.Value);
                    WriteByte(0b00000100);
                    return;
                }
            }
        }
        if (ParseReservedWord('(')) {
            var addressing = ParseAddressing();
            if (addressing != null) {
                AcceptReservedWord(')');
                // PUSH (mem)
                addressing.Write(this, 0b10000000, 0);
                WriteByte(0b00000100);
                return;
            }
        }
        {
            var token = LastToken;
            var value = Expression();
            if (value != null) {
                var constValue = 0;
                if (value.IsConst()) {
                    constValue = value.Value;
                }
                else {
                    ShowAddressUsageError(token);
                }
                // PUSH #8
                WriteByte(0b00001001);
                WriteByte(constValue);
                return;
            }
        }
        ShowSyntaxError(LastToken);
    }

    private void PushWord()
    {
        if (ParseReservedWord('(')) {
            var addressing = ParseAddressing();
            if (addressing != null) {
                AcceptReservedWord(')');
                // PUSHW (mem)
                addressing.Write(this, 0b10010000, 0);
                WriteByte(0b00000100);
                return;
            }
        }
        {
            var token = LastToken;
            var value = Expression();
            if (value != null) {
                // PUSHW #16
                WriteByte(0b00001011);
                WriteWord(token, value);
                return;
            }
        }
        ShowSyntaxError(LastToken);
    }

    private void Pop()
    {
        if (ParseReservedWord(Keyword.F)) {
            // POP F
            WriteByte(0b00011001);
            return;
        }
        if (ParseReservedWord(Keyword.A)) {
            // POP A
            WriteByte(0b00010101);
            return;
        }

        if (ParseReservedWord(Keyword.SR)) {
            WriteByte(0b00000011);
            return;
        }
        {
            var register = ParseRegister(out var size);
            if (register != null) {
                switch (size) {
                    case OperandSize.Byte: {
                            var byteRegularRegister = ToRegularRegister(register.Value, size);
                            if (byteRegularRegister != null) {
                                var sizeBits = SizeBits2(size);
                                // POP R8
                                WriteByte(0b11001000 | (sizeBits << 4) | byteRegularRegister.Value);
                                WriteByte(0b00000101);
                                return;
                            }
                            break;
                        }
                    case OperandSize.Word: {
                            var wordRegularRegister = ToRegularRegister(register.Value, size);
                            if (wordRegularRegister != null) {
                                // POP R16
                                WriteByte(0b01001000 | wordRegularRegister.Value);
                                return;
                            }
                            break;
                        }
                    case OperandSize.DoubleWord: {
                            var doubleWordRegularRegister = ToRegularRegister(register.Value, size);
                            if (doubleWordRegularRegister != null) {
                                // POP R32
                                WriteByte(0b01011000 | doubleWordRegularRegister.Value);
                                return;
                            }
                            break;
                        }
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                {
                    var sizeBits = SizeBits2(size);
                    // POP r
                    WriteByte(0b11000111 | (sizeBits << 4));
                    WriteByte(register.Value);
                    WriteByte(0b00000101);
                    return;
                }
            }
        }
        if (ParseReservedWord('(')) {
            var addressing = ParseAddressing();
            if (addressing != null) {
                AcceptReservedWord(')');
                // POP (mem)
                addressing.Write(this, 0b10110000, 0);
                WriteByte(0b00000100);
                return;
            }
        }
        ShowSyntaxError(LastToken);
    }

    private void PopWord()
    {
        if (ParseReservedWord('(')) {
            var addressing = ParseAddressing();
            if (addressing != null) {
                AcceptReservedWord(')');
                // POPW (mem)
                addressing.Write(this, 0b10110000, 0);
                WriteByte(0b00000110);
                return;
            }
        }
        ShowSyntaxError(LastToken);
    }

    private void LoadAddress()
    {
        {
            var token = LastToken;
            var register = ParseRegister(out var size);
            if (register != null) {
                var regularRegister = ToRegularRegisterSurely(token, register.Value, size);
                AcceptReservedWord(',');
                var addressing = ParseAddressing();
                if (addressing != null) {
                    if (size == OperandSize.DoubleWord) {
                        addressing.Write(this, 0b10110000, 0);
                        WriteByte(0b00110000 | regularRegister);
                        return;
                    }
                    if (size == OperandSize.Byte) {
                        ShowInvalidRegister(token);
                    }
                    addressing.Write(this, 0b10110000, 0);
                    WriteByte(0b00100000 | regularRegister);
                    return;
                }
            }
        }
        ShowSyntaxError(LastToken);
    }

    private void LoadAddressRelative()
    {
        {
            var token = LastToken;
            var register = ParseRegister(out var size);
            if (register != null) {
                var regularRegister = ToRegularRegisterSurely(token, register.Value, size);
                AcceptReservedWord(',');
                var address = Expression();
                if (address != null) {
                    if (!RelativeOffset(token, address, 4, IsSignedWord, out var offset)) {
                        offset = 0;
                    }
                    if (size == OperandSize.DoubleWord) {
                        // LDAR R32,
                        WriteByte(0b11110011);
                        WriteByte(0b00010011);
                        WriteWord(offset);
                        WriteByte(0b00110000 | regularRegister);
                        return;
                    }
                    if (size == OperandSize.Byte) {
                        ShowInvalidRegister(token);
                    }
                    // LDAR R16,
                    WriteByte(0b11110011);
                    WriteByte(0b00010011);
                    WriteWord(offset);
                    WriteByte(0b00100000 | regularRegister);
                    return;
                }
            }
        }
        ShowSyntaxError(LastToken);
    }

    private void Exchange()
    {
        if (ParseReservedWord(Keyword.F)) {
            AcceptReservedWord(',');
            AcceptReservedWord(Keyword.FDash);
            // EX F,F'
            WriteByte(0b00010110);
            return;
        }
        {
            var destinationToken = LastToken;
            var destinationRegister = ParseRegister(out var destinationSize);
            if (destinationRegister != null) {
                var destinationRegularRegister = ToRegularRegisterSurely(destinationToken, destinationRegister.Value, destinationSize);
                AcceptReservedWord(',');
                var sourceToken = LastToken;
                var sourceRegister = ParseRegister(out var sourceSize);
                if (sourceRegister != null) {
                    if (sourceSize == OperandSize.DoubleWord) {
                        ShowInvalidRegister(sourceToken);
                    }
                    else if (sourceSize != destinationSize) {
                        ShowSizeMismatch(sourceToken);
                    }
                    // EX R,r
                    WriteRegisterCodeZ2(0b11001000, sourceRegister.Value, sourceSize);
                    WriteByte(0b10111000 | destinationRegularRegister);
                    return;
                }
            }
        }
        if (ParseReservedWord('(')) {
            var addressing = ParseAddressing();
            if (addressing != null) {
                AcceptReservedWord(')');
                AcceptReservedWord(',');
                var token = LastToken;
                var register = ParseRegister(out var size);
                if (register != null) {
                    var regularRegister = ToRegularRegisterSurely(token, register.Value, size);
                    var sizeBits = SizeBits2(size);
                    // EX (mem),R
                    addressing.Write(this, 0b10000000 | (sizeBits << 4), 0);
                    WriteByte(0b00110000 | regularRegister);
                    return;
                }
            }
        }
        ShowSyntaxError(LastToken);
    }

    private void OperateRegisterZ1(int code, OperandSize[] sizes)
    {
        var token = LastToken;
        var register = ParseRegister(out var size);
        if (register != null) {
            if (Array.IndexOf(sizes, size) < 0) {
                ShowInvalidRegister(token);
                size = sizes[0];
            }
            // r
            WriteRegisterCodeZ1(0b11001000, register.Value, size);
            WriteByte(code);
            return;
        }
        ShowSyntaxError(LastToken);
    }

    private void LoadBlock(int code1, int code2, int sign)
    {
        if (LastToken.IsReservedWord('(')) {
            var register = ParseElement([XDE, XIX]);
            AcceptReservedWord(',');
            if (register == XDE) {
                code1 = code1 & 0b11111001 | 0b010;
                ParseElement([XHL]);
            }
            else {
                code1 = code1 & 0b11111001 | 0b100;
                ParseElement([XIY]);
            }
        }
        WriteByte(code1);
        WriteByte(code2);
        return;

        int ParseElement(int[] registers)
        {
            AcceptReservedWord('(');
            var token = LastToken;
            var register = ParseRegister(out var _);
            if (register != null) {
                AcceptReservedWord(sign);
                AcceptReservedWord(')');
                if (Array.IndexOf(registers, register.Value) >= 0) {
                    return register.Value;
                }
                ShowInvalidRegister(token);
            }
            ShowSyntaxError(token);
            return registers[0];
        }
    }

    private void CompareBlock(int code2, int sign)
    {
        var code1 = 0b10000000;
        var token1 = LastToken;
        var register1 = ParseRegister(out var size1);
        if (register1 != null) {
            switch (register1.Value) {
                case A when size1 == OperandSize.Byte:
                    break;
                case WA when size1 == OperandSize.Word:
                    code1 |= 0b00010000;
                    break;
                default:
                    ShowInvalidRegister(token1);
                    break;
            }
            if (LastToken.IsReservedWord('(')) {
                {
                }
            }
            AcceptReservedWord(',');
            AcceptReservedWord('(');
            {
                var token2 = LastToken;
                var register2 = ParseRegister(out var size);
                if (register2 != null) {
                    if (size != OperandSize.DoubleWord) {
                        ShowInvalidRegister(token2);
                    }
                    var registerRegister = ToRegularRegisterSurely(token2, register2.Value, size);
                    code1 |= registerRegister;
                }
                else {
                    ShowSyntaxError(token2);
                }
            }
            AcceptReservedWord(sign);
            AcceptReservedWord(')');
        }
        else {
            code1 |= 0b011; // XHL;
        }
        WriteByte(code1);
        WriteByte(code2);
    }

    private void Operate(int code)
    {
        {
            var destinationToken = LastToken;
            var destinationRegister = ParseRegister(out var destinationSize);
            if (destinationRegister != null) {
                AcceptReservedWord(',');
                {
                    var sourceToken = LastToken;
                    {
                        var sourceRegister = ParseRegister(out var sourceSize);
                        if (sourceRegister != null) {
                            if (sourceSize != destinationSize) {
                                ShowSizeMismatch(sourceToken);
                            }
                            var destinationRegularRegister = ToRegularRegisterSurely(destinationToken, destinationRegister.Value, destinationSize);
                            // R,r
                            WriteRegisterCodeZ2(0b11001000, sourceRegister.Value, sourceSize);
                            WriteByte(0b10000000 | (code << 4) | destinationRegularRegister);
                            return;
                        }
                    }
                    if (ParseReservedWord('(')) {
                        var addressing = ParseAddressing();
                        if (addressing != null) {
                            var destinationRegularRegister = ToRegularRegisterSurely(destinationToken, destinationRegister.Value, destinationSize);
                            AcceptReservedWord(')');
                            // R,(mem)
                            addressing.Write(this, 0b10000000, SizeBits2(destinationSize));
                            WriteByte(0b10000000 | (code << 4) | destinationRegularRegister);
                            return;
                        }
                    }
                    {
                        var source = Expression();
                        if (source != null) {
                            // r,#
                            WriteRegisterCodeZ2(0b11001000, destinationRegister.Value, destinationSize);
                            WriteByte(0b11001000 | code);
                            switch (destinationSize) {
                                case OperandSize.Byte: {
                                        var value = 0;
                                        if (source.IsConst()) {
                                            value = source.Value;
                                        }
                                        else {
                                            ShowAddressUsageError(sourceToken);
                                        }
                                        WriteByte(value);
                                        break;
                                    }
                                case OperandSize.Word:
                                    WriteWord(sourceToken, source);
                                    break;
                                case OperandSize.DoubleWord:
                                    WriteDoubleWord(sourceToken, source);
                                    break;
                            }
                            return;
                        }
                    }
                }
            }
        }
        if (ParseReservedWord('(')) {
            var addressing = ParseAddressing();
            if (addressing != null) {
                AcceptReservedWord(')');
                AcceptReservedWord(',');
                var destinationToken = LastToken;
                {
                    var register = ParseRegister(out var size);
                    if (register != null) {
                        var registerRegister = ToRegularRegisterSurely(destinationToken, register.Value, size);
                        // (mem),R
                        addressing.Write(this, 0b10000000, SizeBits2(size));
                        WriteByte(0b10001000 | (code << 4) | registerRegister);
                        return;
                    }
                }
                {
                    var destination = Expression();
                    if (destination != null) {
                        var value = 0;
                        if (destination.IsConst()) {
                            value = destination.Value;
                        }
                        else {
                            ShowAddressUsageError(destinationToken);
                        }

                        // (mem),#
                        addressing.Write(this, 0b10000000, 0);
                        WriteByte(0b00111000 | code);
                        WriteByte(value);
                        return;
                    }
                }
            }
        }
        ShowSyntaxError();
    }

    private void OperateWord(int code)
    {
        if (ParseReservedWord('(')) {
            var addressing = ParseAddressing();
            if (addressing != null) {
                AcceptReservedWord(')');
                AcceptReservedWord(',');
                var destinationToken = LastToken;
                {
                    var destination = Expression();
                    if (destination != null) {
                        // (mem),#
                        addressing.Write(this, 0b10010000, 1);
                        WriteByte(0b00111000 | code);
                        WriteWord(destinationToken, destination);
                        return;
                    }
                }
            }
        }
        ShowSyntaxError();
    }

    private void IncrementDecrement(int code)
    {
        {
            var token = LastToken;
            var count = ConstantExpression();
            if (count != null) {
                var countValue = count.Value;
                if (countValue is < 1 or > 8) {
                    ShowOutOfRange(token, countValue);
                }
                countValue &= 7;
                AcceptReservedWord(',');
                if (ParseReservedWord('(')) {
                    var addressing = ParseAddressing();
                    AcceptReservedWord(')');
                    if (addressing != null) {
                        // #,(mem)
                        addressing.Write(this, 0b10000000, 0);
                        WriteByte(code | countValue);
                        return;
                    }
                }
                {
                    var register = ParseRegister(out var size);
                    if (register != null) {
                        // #,r
                        WriteRegisterCodeZ2(0b11001000, register.Value, size);
                        WriteByte(code | countValue);
                        return;
                    }
                }
            }
        }
        ShowSyntaxError();
    }

    private void IncrementDecrementWord(int code)
    {
        {
            var token = LastToken;
            var count = ConstantExpression();
            if (count != null) {
                var countValue = count.Value;
                if (countValue is < 1 or > 8) {
                    ShowOutOfRange(token, countValue);
                }
                countValue &= 7;
                AcceptReservedWord(',');
                if (ParseReservedWord('(')) {
                    var addressing = ParseAddressing();
                    AcceptReservedWord(')');
                    if (addressing != null) {
                        // #,(mem)
                        addressing.Write(this, 0b10010000, 1);
                        WriteByte(code | countValue);
                        return;
                    }
                }
            }
        }
        ShowSyntaxError();
    }

    private void OperateRegisterZ2(int code, OperandSize[] sizes)
    {
        var token = LastToken;
        var register = ParseRegister(out var size);
        if (register != null) {
            if (Array.IndexOf(sizes, size) < 0) {
                ShowInvalidRegister(token);
                size = sizes[0];
            }
            // r
            WriteRegisterCodeZ2(0b11001000, register.Value, size);
            WriteByte(code);
            return;
        }
        ShowSyntaxError(LastToken);
    }

    private void MultiplyDivide(int code)
    {
        var destinationToken = LastToken;
        var destinationRegister = ParseRegister(out var destinationSize);
        if (destinationRegister != null) {
            if (destinationSize == OperandSize.Byte) {
                ShowInvalidRegister(destinationToken);
            }
            var sourceToken = LastToken;
            AcceptReservedWord(',');
            {
                var sourceRegister = ParseRegister(out var sourceSize);
                if (sourceRegister != null) {
                    if ((destinationSize != OperandSize.DoubleWord || sourceSize != OperandSize.Word) &&
                        (destinationSize != OperandSize.Word || sourceSize != OperandSize.Byte)) {
                        ShowSizeMismatch(sourceToken);
                    }
                    var regularRegister = RegularRegister(out var sizeBit);
                    // RR,r
                    WriteRegisterCode(0b11001000 | (sizeBit << 4), sourceRegister.Value, sourceSize);
                    WriteByte(0b01000000 | (code << 3) | regularRegister);
                    return;
                }
            }
            if (ParseReservedWord('(')) {
                var addressing = ParseAddressing();
                if (addressing != null) {
                    AcceptReservedWord(')');
                    var regularRegister = RegularRegister(out var sizeBit);
                    // RR,(mem)
                    addressing.Write(this, 0b10000000 | (sizeBit << 4), sizeBit);
                    WriteByte(0b01000000 | (code << 3) | regularRegister);
                    return;
                }
            }
            {
                var value = ConstantExpression();
                if (value != null) {
                    if (destinationSize == OperandSize.Word) {
                        // rr,# ; byte
                        WriteRegisterCode(0b11001000, destinationRegister.Value, OperandSize.Byte);
                        WriteByte(0b00001000 | code);
                        WriteByte(value.Value);
                        return;
                    }
                    // rr,# ; word
                    WriteRegisterCode(0b11011000, destinationRegister.Value, OperandSize.Word);
                    WriteByte(0b00001000 | code);
                    WriteWord(value.Value);
                    return;
                }
            }
        }
        ShowSyntaxError(LastToken);
        return;

        int RegularRegister(out int sizeBit)
        {
            var regularRegister = ToRegularRegisterSurely(destinationToken, destinationRegister.Value, destinationSize);
            sizeBit = 0;
            if (destinationSize == OperandSize.DoubleWord) {
                sizeBit = 1;
            }
            else {
                regularRegister = (regularRegister << 1) + 1;
            }

            return regularRegister;
        }
    }

    private void ModuloIncrementDecrement(int code)
    {
        var value = ConstantExpression();
        if (value != null) {
            AcceptReservedWord(',');
            var registerToken = LastToken;
            var register = ParseRegister(out var size);
            if (register != null) {
                if (size != OperandSize.Word) {
                    ShowSizeMismatch(registerToken);
                }
                // #,r
                WriteRegisterCode(0b11011000, register.Value, size);
                WriteByte(0b00111000 | code);
                WriteWord(value.Value);
                return;
            }
        }
        ShowSyntaxError(LastToken);
    }

    private void OperateCarryFlag(int code)
    {
        if (ParseReservedWord(Keyword.A)) {
            AcceptReservedWord(',');
            {
                var token = LastToken;
                var register = ParseRegister(out var size);
                if (register != null) {
                    if (size == OperandSize.DoubleWord) {
                        ShowInvalidRegister(token);
                    }
                    // A,r
                    WriteRegisterCodeZ1(0b11001000, register.Value, size);
                    WriteByte(0b00101000 | code);
                    return;
                }
            }
            if (ParseReservedWord('(')) {
                var addressing = ParseAddressing();
                if (addressing != null) {
                    AcceptReservedWord(')');
                    // A,(mem)
                    addressing.Write(this, 0b10110000, 0);
                    WriteByte(0b00101000 | code);
                    return;
                }
            }
        }

        {
            var value = ConstantExpression();
            if (value != null) {
                AcceptReservedWord(',');
                {
                    var token = LastToken;
                    var register = ParseRegister(out var size);
                    if (register != null) {
                        if (size == OperandSize.DoubleWord) {
                            ShowInvalidRegister(token);
                        }
                        // #,r
                        WriteRegisterCodeZ1(0b11001000, register.Value, size);
                        WriteByte(0b00100000 | code);
                        WriteByte(value.Value & 0x0f);
                        return;
                    }
                }
                if (ParseReservedWord('(')) {
                    var addressing = ParseAddressing();
                    if (addressing != null) {
                        AcceptReservedWord(')');
                        // A,(mem)
                        addressing.Write(this, 0b10110000, 0);
                        WriteByte(0b10000000 | (code << 3) | (value.Value & 7));
                        return;
                    }
                }
            }
        }
        ShowSyntaxError(LastToken);
    }

    private void OperateBit(int registerCode, int memoryCode)
    {
        var position = ConstantExpression();
        if (position != null) {
            AcceptReservedWord(',');
            var token = LastToken;
            {
                var register = ParseRegister(out var size);
                if (register != null) {
                    if (size == OperandSize.DoubleWord) {
                        ShowInvalidRegister(token);
                    }
                    // #,r
                    WriteRegisterCodeZ1(0b11001000, register.Value, size);
                    WriteByte(registerCode);
                    WriteByte(position.Value & 0x0f);
                    return;
                }
            }
            if (ParseReservedWord('(')) {
                var addressing = ParseAddressing();
                if (addressing != null) {
                    AcceptReservedWord(')');
                    // #,(mem)
                    addressing.Write(this, 0b10110000, 0);
                    WriteByte(memoryCode | (position.Value & 7));
                    return;
                }
            }
        }
        ShowSyntaxError();
    }

    private void BitSearch(int code)
    {
        if (ParseReservedWord(Keyword.A)) {
            AcceptReservedWord(',');
            {
                var token = LastToken;
                var register = ParseRegister(out var size);
                if (register != null) {
                    if (size != OperandSize.Word) {
                        ShowSizeMismatch(token);
                    }
                    // A,r
                    WriteRegisterCode(0b11011000, register.Value, 0);
                    WriteByte(code);
                    return;
                }
            }
        }
        ShowSyntaxError();
    }

    private void EnableInterrupt()
    {
        var token = LastToken;
        var value = ConstantExpression() ?? 0;

        if (value is not (>= 0 and <= 7)) {
            ShowOutOfRange(token, value);
        }
        WriteByte(0b00000110);
        WriteByte(value);
    }

    private void SoftwareInterrupt()
    {
        var token = LastToken;
        var value = ConstantExpression() ?? 0;

        if (value is not (>= 0 and <= 7)) {
            ShowOutOfRange(token, value);
        }
        WriteByte(0b11111000 | value);
    }

    private void LoadControlRegister()
    {
        {
            var controlRegister = ParseControlRegister(out var destinationSize);
            if (controlRegister != null) {
                AcceptReservedWord(',');
                var token = LastToken;
                var register = ParseRegister(out var sourceSize);
                if (register != null) {
                    if (sourceSize != destinationSize) {
                        ShowSizeMismatch(token);
                    }
                    // cr,r
                    WriteRegisterCodeZ2(0b11001000, register.Value, sourceSize);
                    WriteByte(0b00101110);
                    WriteByte(controlRegister.Value);
                    return;
                }
            }
        }
        {
            var register = ParseRegister(out var destinationSize);
            if (register != null) {
                AcceptReservedWord(',');
                var token = LastToken;
                var controlRegister = ParseControlRegister(out var sourceSize);
                if (controlRegister != null) {
                    if (sourceSize != destinationSize) {
                        ShowSizeMismatch(token);
                    }

                    // r,cr
                    WriteRegisterCodeZ2(0b11001000, register.Value, sourceSize);
                    WriteByte(0b00101111);
                    WriteByte(controlRegister.Value);
                    return;
                }
            }
        }
        ShowSyntaxError();
    }

    private void Link()
    {
        var token = LastToken;
        var register = ParseRegister(out var size);
        if (register != null) {
            if (size != OperandSize.DoubleWord) {
                ShowInvalidRegister(token);
            }
            AcceptReservedWord(',');
            var value = ConstantExpression();
            if (value != null) {
                // r,d16
                WriteRegisterCode(0b11101000, register.Value, size);
                WriteByte(0b00001100);
                WriteWord(value.Value);
                return;
            }
        }
        ShowSyntaxError();
    }

    private void LoadRegisterFilePointer()
    {
        var token = LastToken;
        var value = ConstantExpression();
        if (value != null) {
            if (value.Value is < 0 or > 3) {
                ShowOutOfRange(token, value.Value);
            }
            WriteByte(0b00010111);
            WriteByte(value.Value & 0b11);
            return;
        }
        ShowSyntaxError();
    }

    private void SetConditionCode()
    {
        var conditionCode = ParseConditionCode();
        if (conditionCode != null) {
            AcceptReservedWord(',');
        }
        else {
            conditionCode = Always;
        }

        var token = LastToken;
        var register = ParseRegister(out var size);
        if (register != null) {
            if (size == OperandSize.DoubleWord) {
                ShowInvalidRegister(token);
            }
            WriteRegisterCode(0b11001000, register.Value, size);
            WriteByte(0b01110000 | conditionCode.Value);
            return;
        }
        ShowSyntaxError();
    }

    private void Shift(int code)
    {
        if (ParseReservedWord(Keyword.A)) {
            AcceptReservedWord(',');
            var register = ParseRegister(out var size);
            if (register != null) {
                // A,r
                WriteRegisterCodeZ2(0b11001000, register.Value, size);
                WriteByte(0b11111000 | code);
                return;
            }
        }
        if (ParseReservedWord('(')) {
            var addressing = ParseAddressing();
            if (addressing != null) {
                AcceptReservedWord(')');
                // (mem)
                addressing.Write(this, 0b10000000, 0);
                WriteByte(0b01111000 | code);
                return;
            }
        }
        {
            var token = LastToken;
            var count = ConstantExpression();
            if (count != null) {
                if (count is < 0 or > 15) {
                    ShowOutOfRange(token, count.Value);
                }
                AcceptReservedWord(',');
                var register = ParseRegister(out var size);
                if (register != null) {
                    // #,r
                    WriteRegisterCodeZ2(0b11001000, register.Value, size);
                    WriteByte(0b11101000 | code);
                    WriteByte(count.Value & 0x0f);
                    return;
                }
            }
        }
        ShowSyntaxError();
    }

    private void ShiftWord(int code)
    {
        if (ParseReservedWord('(')) {
            var addressing = ParseAddressing();
            if (addressing != null) {
                AcceptReservedWord(')');
                // (mem)
                addressing.Write(this, 0b10010000, 1);
                WriteByte(0b01111000 | code);
                return;
            }
        }
        ShowSyntaxError();
    }

    private void RotateDigit(int code)
    {
        if (ParseReservedWord(Keyword.A)) {
            AcceptReservedWord(',');
        }
        if (ParseReservedWord('(')) {
            var addressing = ParseAddressing();
            if (addressing != null) {
                AcceptReservedWord(')');
                addressing.Write(this, 0b10000000, 0);
                WriteByte(0b00000110 | code);
                return;
            }
        }
        ShowSyntaxError();
    }

    private void Jump(int code)
    {
        {
            var token = LastToken;
            var address = Expression();
            if (address != null) {
                if (address.IsConst() && IsWord(address.Value)) {
                    // #16
                    WriteByte(code << 1);
                    WriteWord(token, address);
                    return;
                }
                // #24
                WriteByte((code << 1) | 1);
                WriteTripleByte(token, address);
                return;
            }
        }
        {
            var conditionCode = ParseConditionCode();
            if (conditionCode != null) {
                AcceptReservedWord(',');
            }
            else {
                conditionCode = Always;
            }
            var addressing = ParseAddressing();
            if (addressing != null) {
                // [cc,]mem
                addressing.Write(this, 0b10110000, 0);
                WriteByte((code << 4) | conditionCode.Value);
                return;
            }
        }
        ShowSyntaxError();
    }

    private void JumpRelative()
    {
        {
            var conditionCode = ParseConditionCode();
            if (conditionCode != null) {
                AcceptReservedWord(',');
            }
            else {
                conditionCode = Always;
            }

            var token = LastToken;
            var address = Expression();
            if (address != null) {
                JumpRelative(token, conditionCode.Value, address);
                return;
            }
        }
        ShowSyntaxError();
    }

    private void JumpRelative(Token token, int conditionCode, Address address)
    {
        if (RelativeOffset(token, address, 2, IsSignedByte, out var byteOffset)) {
            // jr condition,d8
            WriteByte(0b01100000 | conditionCode);
            WriteByte(byteOffset);
            return;
        }
        if (RelativeOffset(token, address, 3, IsSignedWord, out var wordOffset)) {
            // jr condition,d16
            WriteByte(0b01110000 | conditionCode);
            WriteWord(wordOffset);
            return;
        }
        if (conditionCode == Always) {
            if (address.IsConst() && IsWord(address.Value)) {
                // jp #16
                WriteByte(0b00011010);
                WriteWord(token, address);
                return;
            }
            // jp #24
            WriteByte(0b00011011);
            WriteTripleByte(token, address);
            return;
        }
        // jp condition,mem
        new AbsoluteAddressing(token, address).Write(this, 0b10110000, 0);
        WriteByte(0b11010000 | conditionCode);
    }

    private void CallRelative()
    {
        {
            var token = LastToken;
            var address = Expression();
            if (address != null) {
                RelativeOffset(token, address, 3, IsSignedWord, out var wordOffset);
                // call d16
                WriteByte(0b00011110);
                WriteWord(wordOffset);
                return;
            }
        }
        ShowSyntaxError();
    }

    private void DecrementJump()
    {
        {
            var registerToken = LastToken;
            var register = ParseRegister(out var size);
            if (register != null) {
                AcceptReservedWord(',');
            }
            else {
                register = B;
            }
            if (size == OperandSize.DoubleWord) {
                ShowSizeMismatch(registerToken);
            }

            var addressToken = LastToken;
            var address = Expression();
            if (address != null) {
                DecrementJump(register, size, addressToken, address);
                return;
            }
        }
        ShowSyntaxError();
    }

    private void DecrementJump([DisallowNull] int? register, OperandSize size, Token addressToken, Address address)
    {
        var length = ToRegularRegister(register.Value, size) != null ? 3 : 4;
        if (RelativeOffset(addressToken, address, length, IsSignedByte, out var offset)) {
            // djnz r,d8
            WriteRegisterCode(0b11001000, register.Value, size);
            WriteByte(0b00011100);
            WriteByte(offset);
            return;
        }
        // dec 1,r
        WriteRegisterCode(0b11001000, register.Value, size);
        WriteByte(0b01101001);
        if (RelativeOffset(addressToken, address, length, IsSignedWord, out offset)) {
            // jr nz,d16
            WriteByte(0b01111110);
            WriteWord(offset);
            return;
        }
        // jp nz,mem
        new AbsoluteAddressing(addressToken, address).Write(this, 0b10110000, 0);
        WriteByte(0b11011110);
        return;
    }

    private void Return()
    {
        var conditionCode = ParseConditionCode();
        if (conditionCode != null) {
            // cc
            WriteByte(0b10110000);
            WriteByte(0b11110000 | conditionCode.Value);
            return;
        }
        //
        WriteByte(0b00001110);
    }

    private void ReturnDeallocate()
    {
        var value = ConstantExpression();
        if (value != null) {
            // d16
            WriteByte(0b00001111);
            WriteWord(value.Value);
            return;
        }
        ShowSyntaxError();
    }

    private void ConditionalJump(Address address, int conditionBit)
    {
        var conditionCode = ParseConditionCode();
        if (conditionCode != null) {
            JumpRelative(LastToken, conditionCode.Value ^ conditionBit, address);
            return;
        }
        ShowSyntaxError();
    }


    private void StartIf(IfBlock block)
    {
        var address = SymbolAddress(block.ElseId);
        ConditionalJump(address, 0b1000);
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
            JumpRelative(LastToken, Always, address);
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
        if (repeatAddress.Type == CurrentSegment.Type && (RelativeOffset(repeatAddress)) <= 0) {
            var address = SymbolAddress(block.BeginId);
            ConditionalJump(address, 0b0000);
            block.EraseEndId();
        }
        else {
            var address = SymbolAddress(block.EndId);
            ConditionalJump(address, 0b1000);
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
                JumpRelative(LastToken, Always, address);
                DefineSymbol(block.EndId, CurrentAddress);
            }
            EndBlock();
        }
    }

    private void DwNzStatement()
    {
        {
            var registerToken = LastToken;
            var register = ParseRegister(out var size);
            if (register != null) {
                if (size == OperandSize.DoubleWord) {
                    ShowSizeMismatch(registerToken);
                }
                if (LastBlock() is not WhileBlock block) {
                    ShowNoStatementError(LastToken, "WHILE");
                }
                else {
                    if (block.EndId <= 0) {
                        ShowError(LastToken.Position, "WHILE and WNZ cannot be used in the same syntax.");
                    }
                    var addressToken = LastToken;
                    var address = SymbolAddress(block.BeginId);
                    EndBlock();
                    DecrementJump(register, size, addressToken, address);
                    return;
                }
            }
        }
        ShowSyntaxError();
    }


    private static readonly Dictionary<int, Action<Assembler>> Actions = new()
    {
        {Keyword.LD,a=>a.Load()},
        {Keyword.LDW,a=>a.LoadWord()},
        {Keyword.PUSH,a=>a.Push()},
        {Keyword.PUSHW,a=>a.PushWord()},
        {Keyword.POP,a=>a.Pop()},
        {Keyword.POPW,a=>a.PopWord()},
        {Keyword.LDA,a=>a.LoadAddress()},
        {Keyword.LDAR,a=>a.LoadAddressRelative()},
        {Keyword.EX,a=>a.Exchange()},
        {Keyword.MIRR,a=>a.OperateRegisterZ1(0b00010110, [OperandSize.Word])},
        {Keyword.LDI,a=>a.LoadBlock(0b10000011,0b00010000, '+')},
        {Keyword.LDIW,a=>a.LoadBlock(0b10010011,0b00010000, '+')},
        {Keyword.LDIR,a=>a.LoadBlock(0b10000011,0b00010001, '+')},
        {Keyword.LDIRW,a=>a.LoadBlock(0b10010011,0b00010001, '+')},
        {Keyword.LDD,a=>a.LoadBlock(0b10000011,0b00010010, '-')},
        {Keyword.LDDW,a=>a.LoadBlock(0b10010011,0b00010010, '-')},
        {Keyword.LDDR,a=>a.LoadBlock(0b10000011,0b00010011, '-')},
        {Keyword.LDDRW,a=>a.LoadBlock(0b10010011,0b00010011, '-')},
        {Keyword.CPI,a=>a.CompareBlock(0b00010100, '+')},
        {Keyword.CPIR,a=>a.CompareBlock(0b00010101, '+')},
        {Keyword.CPD,a=>a.CompareBlock(0b00010110, '-')},
        {Keyword.CPDR,a=>a.CompareBlock(0b00010111, '-')},
        {Keyword.ADD,a=>a.Operate(0b000)},
        {Keyword.ADDW,a=>a.OperateWord(0b000)},
        {Keyword.ADC,a=>a.Operate(0b001)},
        {Keyword.ADCW,a=>a.OperateWord(0b001)},
        {Keyword.SUB,a=>a.Operate(0b010)},
        {Keyword.SUBW,a=>a.OperateWord(0b010)},
        {Keyword.SBC,a=>a.Operate(0b011)},
        {Keyword.SBCW,a=>a.OperateWord(0b011)},
        {Keyword.CP,a=>a.Operate(0b111)},
        {Keyword.CPW,a=>a.OperateWord(0b111)},
        {Keyword.INC,a=>a.IncrementDecrement(0b01100000)},
        {Keyword.INCW,a=>a.IncrementDecrementWord(0b01100000)},
        {Keyword.DEC,a=>a.IncrementDecrement(0b01101000)},
        {Keyword.DECW,a=>a.IncrementDecrementWord(0b01101000)},
        {Keyword.NEG,a=>a.OperateRegisterZ1(0b00000111, [OperandSize.Byte,OperandSize.Word])},
        {Keyword.EXTZ,a=>a.OperateRegisterZ2(0b00010010, [OperandSize.Word,OperandSize.DoubleWord])},
        {Keyword.EXTS,a=>a.OperateRegisterZ2(0b00010011, [OperandSize.Word,OperandSize.DoubleWord])},
        {Keyword.DAA,a=>a.OperateRegisterZ1(0b00010000, [OperandSize.Byte])},
        {Keyword.PAA,a=>a.OperateRegisterZ2(0b00010100, [OperandSize.Word,OperandSize.DoubleWord])},
        {Keyword.MUL,a=>a.MultiplyDivide(0b00)},
        {Keyword.MULS,a=>a.MultiplyDivide(0b01)},
        {Keyword.DIV,a=>a.MultiplyDivide(0b10)},
        {Keyword.DIVS,a=>a.MultiplyDivide(0b11)},
        {Keyword.MULA,a=>a.OperateRegisterZ1(0b00011001, [OperandSize.Word])},
        {Keyword.MINC1,a=>a.ModuloIncrementDecrement(0b000)},
        {Keyword.MINC2,a=>a.ModuloIncrementDecrement(0b001)},
        {Keyword.MINC4,a=>a.ModuloIncrementDecrement(0b010)},
        {Keyword.MDEC1,a=>a.ModuloIncrementDecrement(0b100)},
        {Keyword.MDEC2,a=>a.ModuloIncrementDecrement(0b101)},
        {Keyword.MDEC4,a=>a.ModuloIncrementDecrement(0b110)},
        {Inu.Assembler.Keyword.And,a=>a.Operate(0b100)},
        {Keyword.ANDW,a=>a.OperateWord(0b100)},
        {Inu.Assembler.Keyword.Or,a=>a.Operate(0b110)},
        {Keyword.ORW,a=>a.OperateWord(0b110)},
        {Inu.Assembler.Keyword.Xor,a=>a.Operate(0b101)},
        {Keyword.XORW,a=>a.OperateWord(0b101)},
        {Keyword.CPL,a=>a.OperateRegisterZ1(0b00000110, [OperandSize.Byte,OperandSize.Word])},
        {Keyword.LDCF,a=>a.OperateCarryFlag(0b011)},
        {Keyword.STCF,a=>a.OperateCarryFlag(0b100)},
        {Keyword.ANDCF,a=>a.OperateCarryFlag(0b000)},
        {Keyword.ORCF,a=>a.OperateCarryFlag(0b001)},
        {Keyword.XORCF,a=>a.OperateCarryFlag(0b010)},
        {Keyword.RCF,a=>a.WriteByte(0b00010000)},
        {Keyword.SCF,a=>a.WriteByte(0b00010001)},
        {Keyword.CCF,a=>a.WriteByte(0b00010010)},
        {Keyword.ZCF,a=>a.WriteByte(0b00010011)},
        {Keyword.BIT,a=>a.OperateBit(0b00110011,0b11001000)},
        {Keyword.RES,a=>a.OperateBit(0b00110000,0b10110000)},
        {Keyword.SET,a=>a.OperateBit(0b00110001,0b10111000)},
        {Keyword.CHG,a=>a.OperateBit(0b00110010,0b11000000)},
        {Keyword.TSET,a=>a.OperateBit(0b00110100,0b10101000)},
        {Keyword.BS1B,a=>a.BitSearch(0b00001111)},
        {Keyword.BS1F,a=>a.BitSearch(0b00001110)},
        {Keyword.NOP,a=>a.WriteByte(0b00000000)},
        {Keyword.EI,a=>a.EnableInterrupt()},
        {Keyword.DI,a=>{a.WriteByte(0b0000110);a.WriteByte(0b00000111);}},
        {Keyword.SWI,a=>a.SoftwareInterrupt()},
        {Keyword.HALT,a=>a.WriteByte(0b00000101)},
        {Keyword.LDC,a=>a.LoadControlRegister()},
        {Keyword.LINK,a=>a.Link()},
        {Keyword.UNLK,a=>a.OperateRegisterZ2(0b00001101,[OperandSize.DoubleWord])},
        {Keyword.LDF,a=>a.LoadRegisterFilePointer()},
        {Keyword.INCF,a=>a.WriteByte(0b00001100)},
        {Keyword.DECF,a=>a.WriteByte(0b00001101)},
        {Keyword.SCC,a=>a.SetConditionCode()},
        {Keyword.RLC,a=>a.Shift(0b000)},
        {Keyword.RLCW,a=>a.ShiftWord(0b000)},
        {Keyword.RRC,a=>a.Shift(0b001)},
        {Keyword.RRCW,a=>a.ShiftWord(0b001)},
        {Keyword.RL,a=>a.Shift(0b010)},
        {Keyword.RLW,a=>a.ShiftWord(0b010)},
        {Keyword.RR,a=>a.Shift(0b011)},
        {Keyword.RRW,a=>a.ShiftWord(0b011)},
        {Keyword.SLA,a=>a.Shift(0b100)},
        {Keyword.SLAW,a=>a.ShiftWord(0b100)},
        {Keyword.SRA,a=>a.Shift(0b101)},
        {Keyword.SRAW,a=>a.ShiftWord(0b101)},
        {Keyword.SLL,a=>a.Shift(0b110)},
        {Keyword.SLLW,a=>a.ShiftWord(0b110)},
        {Keyword.SRL,a=>a.Shift(0b111)},
        {Keyword.SRLW,a=>a.ShiftWord(0b111)},
        {Keyword.RLD,a=>a.RotateDigit(0)},
        {Keyword.RRD,a=>a.RotateDigit(1)},
        {Keyword.JP,a=>a.Jump(0b1101)},
        {Keyword.CALL,a=>a.Jump(0b1110)},
        {Keyword.JR,a=>a.JumpRelative()},
        {Keyword.CALR, a=>a.CallRelative()},
        {Keyword.DJNZ,a=>a.DecrementJump()},
        {Keyword.RET,a=>a.Return()},
        {Keyword.RETD,a=>a.ReturnDeallocate()},
        {Keyword.RETI,a=>a.WriteByte(0b00000111)},
        //
        {Inu.Assembler.Keyword.If, a=>a.IfStatement()},
        {Inu.Assembler.Keyword.EndIf, a=>a.EndIfStatement()},
        {Inu.Assembler.Keyword.Else, a=>a.ElseStatement()},
        {Inu.Assembler.Keyword.ElseIf, a=>a.ElseIfStatement()},
        {Inu.Assembler.Keyword.Do, a=>a.DoStatement()},
        {Inu.Assembler.Keyword.While, a=>a.WhileStatement()},
        {Inu.Assembler.Keyword.WEnd, a=>a.WEndStatement()},
        {Keyword.DWNZ, (Assembler a)=>{a.DwNzStatement(); }},
    };



    public override AddressPart PointerAddressPart => AddressPart.DoubleWord;

    protected override bool Instruction()
    {
        var reservedWord = LastToken as ReservedWord;
        Debug.Assert(reservedWord != null);
        if (!Actions.TryGetValue(reservedWord.Id, out var action)) return false;
        NextToken();
        action(this);
        return true;
    }

    protected override Dictionary<int, Func<bool>> StorageDirectives
    {
        get
        {
            var storageDirectives = base.StorageDirectives;
            storageDirectives[Keyword.DEFD] = DoubleWordStorageOperand;
            storageDirectives[Keyword.DD] = DoubleWordStorageOperand;
            return storageDirectives;
        }
    }

    private bool DoubleWordStorageOperand()
    {
        var token = LastToken;
        var value = Expression();
        if (value == null) { return false; }
        WriteDoubleWord(token, value);
        return true;
    }
}