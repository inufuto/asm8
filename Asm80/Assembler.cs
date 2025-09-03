using Inu.Language;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Inu.Assembler.Z80;

internal class Assembler(int version) : Inu.Assembler.LittleEndianAssembler(new Tokenizer(version))
{
    private int? ByteExpression()
    {
        Debug.Assert(LastToken != null);
        var token = LastToken;
        var value = Expression();
        if (value == null) { return null; }
        if (!value.IsConst()) {
            ShowAddressUsageError(token);
        }
        return value.Value;
    }

    private static readonly Dictionary<int, int[]> InstructionsWithoutOperand = new Dictionary<int, int[]>
    {
        {Keyword.LdI, [0b11101101, 0b10100000] },
        {Keyword.LdIr, [0b11101101, 0b10110000] },
        {Keyword.LdD, [0b11101101, 0b10101000] },
        {Keyword.LdDr, [0b11101101, 0b10111000] },
        {Keyword.Exx, [0b11011001] },
        {Keyword.RlcA, [0b00000111] },
        {Keyword.Rla, [0b00010111] },
        {Keyword.RrcA, [0b00001111] },
        {Keyword.Rra, [0b00011111] },
        {Keyword.Cpl, [0b00101111] },
        {Keyword.Neg, [0b11101101, 0b01000100] },
        {Keyword.Ccf, [0b00111111] },
        {Keyword.Scf, [0b00110111] },
        {Keyword.Cpi, [0b11101101, 0b10100001] },
        {Keyword.CpiR, [0b11101101, 0b10110001] },
        {Keyword.Cpd, [0b11101101, 0b10101001] },
        {Keyword.CpdR, [0b11101101, 0b10111001] },
        {Keyword.RetI, [0b11101101, 0b01001101] },
        {Keyword.RetN, [0b11101101, 0b01000101] },
        {Keyword.Nop, [0b00000000] },
        {Keyword.Halt, [0b01110110] },
        {Keyword.Di, [0b11110011] },
        {Keyword.Ei, [0b11111011] },
        {Keyword.Ini, [0b11101101, 0b10100010] },
        {Keyword.IniR, [0b11101101, 0b10110010] },
        {Keyword.Ind, [0b11101101, 0b10101010] },
        {Keyword.IndR, [0b11101101, 0b10111010] },
        {Keyword.OutI, [0b11101101, 0b10100011] },
        {Keyword.OutIr, [0b11101101, 0b10110011] },
        {Keyword.OtIr, [0b11101101, 0b10110011] },
        {Keyword.OutD, [0b11101101, 0b10101011] },
        {Keyword.OutDr, [0b11101101, 0b10111011] },
        {Keyword.OtDr, [0b11101101, 0b10111011] },
        {Keyword.Daa, [0b00100111] },
        {Keyword.Rld, [0b11101101, 0b01101111] },
        {Keyword.Rrd, [0b11101101, 0b01100111] },
    };
    private bool InstructionWithoutOperand()
    {
        var reservedWord = LastToken as ReservedWord;
        Debug.Assert(reservedWord != null);
        if (!InstructionsWithoutOperand.TryGetValue(reservedWord.Id, out var codes)) { return false; }
        NextToken();
        foreach (var code in codes) {
            WriteByte(code);
        }
        return true;
    }

    private static readonly int[] SingleRegisters = [Keyword.B, Keyword.C, Keyword.D, Keyword.E, Keyword.H, Keyword.L, 0, Keyword.A
    ];
    private int? SingleRegister()
    {
        var token = LastToken;
        if (token is not ReservedWord reservedWord)
            return null;
        var code = 0;
        foreach (var r in SingleRegisters) {
            if (r == reservedWord.Id) {
                NextToken();
                return code;
            }
            ++code;
        }
        return null;

    }

    private int OffsetForIndex()
    {
        var token = NextToken();
        var offset = ByteExpression() ?? 0;
        if (offset is < -128 or > 127) {
            ShowOutOfRange(token, offset);
        }
        return offset;
    }
    private bool IndexedAddress(out int registerId, out Address address)
    {
        registerId = 0;

        var token = LastToken;
        if (token is ReservedWord reservedWord) {
            switch (registerId = reservedWord.Id) {
                case Keyword.Hl:
                case Keyword.De:
                case Keyword.Bc: {
                        NextToken();
                        address = Address.Default;
                        return true;
                    }
                case Keyword.Ix:
                case Keyword.Iy:
                    var offset = OffsetForIndex();
                    address = new Address(AddressType.Const, offset);
                    return true;
            }

            address = Address.Default;
            return false;
        }

        address = Address.Default;
        return false;
    }
    private bool MemoryAddress(out int registerId, out Address address)
    {
        registerId = 0;

        var token = LastToken;
        if (!token.IsReservedWord('(')) {
            address = Address.Default;
            return false;
        }
        NextToken();
        if (!IndexedAddress(out registerId, out address)) {
            var expression = Expression();
            if ((expression) != null) {
                address = expression;
                registerId = 0;
            }
            else {
                ShowSyntaxError();
            }
        }
        AcceptReservedWord(')');
        return true;
    }

    private static readonly Dictionary<int, int> IndexRegisterCodes = new()
    {
        { Keyword.Ix, 0b11011101},
        { Keyword.Iy, 0b11111101},
    };
    private int? IndexRegister()
    {
        var leftToken = LastToken;
        if (leftToken is not ReservedWord reservedWord)
            return null;
        switch (reservedWord.Id) {
            case Keyword.Ix:
            case Keyword.Iy:
                NextToken();
                return IndexRegisterCodes[reservedWord.Id];
        }
        return null;
    }

    /**
     * "LD (?),r".
     */
    private bool LoadToMemoryInstruction(Token leftToken, int indexId, Address address)
    {
        var rightToken = LastToken;
        var rightRegister = SingleRegister();
        if (rightRegister == null) { return false; }
        switch (indexId) {
            case Keyword.Hl:
                //	LD (HL),r	
                WriteByte(0b01110000 | rightRegister.Value);
                break;
            case Keyword.Ix:
            case Keyword.Iy:
                //	LD (IX or IY+d),r
                WriteByte(IndexRegisterCodes[indexId]);
                WriteByte(0b01110000 | rightRegister.Value);
                WriteByte(address.Value);
                break;
            case Keyword.Bc:
                if (!rightToken.IsReservedWord(Keyword.A)) { ShowInvalidRegister(rightToken); }
                //	LD (BC),A
                WriteByte(0b00000010);
                break;
            case Keyword.De:
                if (!rightToken.IsReservedWord(Keyword.A)) { ShowInvalidRegister(rightToken); }
                //	LD (DE),A
                WriteByte(0b00010010);
                break;
            default:
                if (!rightToken.IsReservedWord(Keyword.A)) { ShowInvalidRegister(rightToken); }
                //	LD (nn),A
                WriteByte(0b00110010);
                WriteWord(leftToken, address);
                break;
        }
        return true;
    }

    private static readonly int[] RegisterPairs = [Keyword.Bc, Keyword.De, Keyword.Hl, Keyword.Sp];
    private int? RegisterPair()
    {
        var token = LastToken;
        if (token is not ReservedWord reservedWord)
            return null;
        var code = 0;
        foreach (var r in RegisterPairs) {
            if (r == reservedWord.Id) {
                NextToken();
                return code;
            }
            ++code;
        }
        return null;
    }
    /**
     * "LD (nn),rp IX or IY".
     */
    private bool LoadRegisterPairToMemoryInstruction(Token leftToken, int indexId, Address address)
    {
        var rightToken = LastToken;
        var registerCode = RegisterPair();
        if (registerCode != null) {
            if (rightToken.IsReservedWord(Keyword.Hl)) {
                // LD (nn),HL
                WriteByte(0b00100010);
            }
            else {
                // LD (nn),rp
                WriteByte(0b11101101);
                WriteByte(0b01000011 | (registerCode.Value << 4));
            }
        }
        else if ((registerCode = IndexRegister()) != null) {
            // LD (nn),IX or IY
            WriteByte(registerCode.Value);
            WriteByte(0b00100010);
        }
        else {
            return false;
        }
        WriteWord(leftToken, address);
        return true;
    }
    private bool LoadConstantByteToMemoryInstruction(Token leftToken, int indexId, Address address)
    {
        var value = ByteExpression();
        if (value == null) { return false; }
        const int instruction = 0b00110110;
        switch (indexId) {
            case Keyword.Hl:
                //	LD (HL),n
                WriteByte(instruction);
                WriteByte(value.Value);
                break;
            case Keyword.Ix:
            case Keyword.Iy:
                //	LD (IX or IY+d),n
                WriteByte(IndexRegisterCodes[indexId]);
                WriteByte(instruction);
                WriteByte(address.Value);
                WriteByte(value.Value);
                break;
            default:
                ;
                ShowInvalidRegister(leftToken);
                break;
        }
        return true;
    }
    /**
     * "LD (?),?".
     */
    private bool LoadToMemoryInstruction()
    {
        var leftToken = LastToken;
        if (!MemoryAddress(out var indexId, out var address)) { return false; }
        AcceptReservedWord(',');
        Debug.Assert(address != null);
        return
            LoadToMemoryInstruction(leftToken, indexId, address) ||
            LoadRegisterPairToMemoryInstruction(leftToken, indexId, address) ||
            LoadConstantByteToMemoryInstruction(leftToken, indexId, address);
    }
    /**
     * "LD r,(?)".
     */
    private bool LoadRegisterFromMemoryInstruction(Token leftToken, int leftRegister)
    {
        if (!MemoryAddress(out var indexId, out var address)) { return false; }
        Debug.Assert(address != null);
        switch (indexId) {
            case Keyword.Hl:
                //	LD r,(HL)
                WriteByte(0b01000110 | (leftRegister << 3));
                break;
            case Keyword.Ix:
            case Keyword.Iy:
                //	LD r,(IX or IY+d)
                WriteByte(IndexRegisterCodes[indexId]);
                WriteByte(0b01000110 | (leftRegister << 3));
                WriteByte(address.Value);
                break;
            case Keyword.Bc:
                if (!leftToken.IsReservedWord(Keyword.A)) { ShowInvalidRegister(leftToken); }
                //	LD A,(BC)
                WriteByte(0b00001010);
                break;
            case Keyword.De:
                if (!leftToken.IsReservedWord(Keyword.A)) { ShowInvalidRegister(leftToken); }
                //	LD A,(DE)
                WriteByte(0b00011010);
                break;
            default:
                if (!leftToken.IsReservedWord(Keyword.A)) { ShowInvalidRegister(leftToken); }
                //	LD A,(nn)
                WriteByte(0b00111010);
                WriteWord(leftToken, address);
                break;
        }
        return true;
    }
    /**
     * "LD r1,r2".
     */
    private bool LoadRegisterFromRegisterInstruction(int leftRegister)
    {
        var rightRegister = SingleRegister();
        if (rightRegister == null) { return false; }
        //	LD r1,r2
        var opeCode = 0b01000000 | (leftRegister << 3) | rightRegister.Value;
        WriteByte(opeCode);
        return true;
    }
    /**
     * "LD r1,n".
     */
    private bool LoadRegisterFromConstantInstruction(int leftRegister)
    {
        var value = Expression();
        if (value == null) { return false; }
        //	LD r,n
        WriteByte(0b00000110 | (leftRegister << 3));
        WriteByte(LastToken, value);
        return true;
    }
    /**
     * "LD A,I or R".
     */
    private bool LoadRegisterFromSpecialRegisterInstruction(Token leftToken)
    {
        var rightToken = LastToken;
        if (rightToken is not ReservedWord reservedWord)
            return false;
        switch (reservedWord.Id) {
            case Keyword.I:
                if (!leftToken.IsReservedWord(Keyword.A)) {
                    ShowInvalidRegister(leftToken);
                }

                //	LD A,I
                WriteByte(0b11101101);
                WriteByte(0b01010111);
                NextToken();
                break;
            case Keyword.R:
                if (!leftToken.IsReservedWord(Keyword.A)) {
                    ShowInvalidRegister(leftToken);
                }

                //	LD A,R
                WriteByte(0b11101101);
                WriteByte(0b01011111);
                NextToken();
                break;
            default:
                return false;
        }

        return true;

    }
    /**
     * "LD r,?".
     */
    private bool LoadRegisterInstruction()
    {
        var leftToken = LastToken;
        var leftRegister = SingleRegister();
        if (leftRegister == null) { return false; }
        AcceptReservedWord(',');

        return
            LoadRegisterFromMemoryInstruction(leftToken, leftRegister.Value) ||
            LoadRegisterFromRegisterInstruction(leftRegister.Value) ||
            LoadRegisterFromConstantInstruction(leftRegister.Value) ||
            LoadRegisterFromSpecialRegisterInstruction(leftToken);
    }
    /**
     * "LD SP,?".
     */
    private bool LoadStackPointer(Token leftRegister)
    {
        if (!leftRegister.IsReservedWord(Keyword.Sp)) { return false; }

        var rightToken = LastToken;
        if (rightToken.Type == TokenType.ReservedWord) {
            if (rightToken.IsReservedWord(Keyword.Hl)) {
                NextToken();
                //	LD SP,HL
                WriteByte(0b11111001);
            }
            else {
                var registerCode = IndexRegister();
                if (registerCode != null) {
                    //	LD SP,IX or IY
                    WriteByte(registerCode.Value);
                    WriteByte(0b11111001);
                }
                else {
                    ShowSyntaxError(rightToken);
                }
            }
            return true;
        }
        var value = Expression();
        if (value != null && !value.Parenthesized) {
            //	LD SP,nn
            WriteByte(0b00110001);
            WriteWord(rightToken, value);
            return true;
        }
        ShowSyntaxError(rightToken);
        return true;
    }
    /**
     * "LD rp,?".
     */
    private bool LoadRegisterPairInstruction()
    {
        var leftToken = LastToken;
        var leftRegister = RegisterPair();
        if (leftRegister == null) { return false; }
        AcceptReservedWord(',');
        if (LoadStackPointer(leftToken)) { return true; }
        var rightToken = LastToken;
        var value = Expression();
        if (value == null) { return false; }
        if (value.Parenthesized) {
            if (leftToken.IsReservedWord(Keyword.Hl)) {
                // LD HL,(nn)
                WriteByte(0b00101010);
                WriteWord(rightToken, value);
            }
            else {
                //	LD rp,(nn)
                WriteByte(0b11101101);
                WriteByte(0b01001011 | (leftRegister.Value << 4));
                WriteWord(rightToken, value);
            }
        }
        else {
            //	LD rp,nn
            WriteByte(0b00000001 | (leftRegister.Value << 4));
            WriteWord(rightToken, value);
        }
        return true;
    }
    /**
     * "LD IX or IY,?".
     */
    private bool LoadIndexRegisterInstruction()
    {
        var leftRegister = IndexRegister();
        if (leftRegister == null) { return false; }
        AcceptReservedWord(',');
        var rightToken = LastToken;
        var value = Expression();
        if (value == null)
            return true;
        if (value.Parenthesized) {
            // LD IX or IY,(nn)
            WriteByte(leftRegister.Value);
            WriteByte(0b00101010);
            WriteWord(rightToken, value);
        }
        else {
            //	LD IX or IY,nn
            WriteByte(leftRegister.Value);
            WriteByte(0b00100001);
            WriteWord(rightToken, value);
        }
        return true;
    }
    /**
     * "LD I or R,A".
     */
    private static readonly Dictionary<int, int> SpecialRegisterCodes = new Dictionary<int, int>{
        {Keyword.I, 0b01000111},
        {Keyword.R, 0b01001111},
    };
    private bool LoadSpecialRegisterFromRegister(Token leftToken)
    {
        if (leftToken is not ReservedWord reservedWord)
            return false;
        if (!SpecialRegisterCodes.TryGetValue(reservedWord.Id, out int instruction)) {
            return false;
        }

        NextToken();
        var rightToken = AcceptReservedWord(',');
        if (!rightToken.IsReservedWord(Keyword.A)) {
            ShowSyntaxError();
        }

        NextToken();
        //	LD I or R,A
        WriteByte(0b11101101);
        WriteByte(instruction);
        return true;

    }
    /**
     * "LD ?,?".
     */
    private void LoadInstruction()
    {
        var leftToken = NextToken();
        if (
            LoadToMemoryInstruction() ||
            LoadRegisterInstruction() ||
            LoadRegisterPairInstruction() ||
            LoadIndexRegisterInstruction() ||
            LoadSpecialRegisterFromRegister(leftToken)) {
            return;
        }
        ShowSyntaxError();
    }
    /**
     * "EX ?,?".
     */
    private void ExchangeInstruction()
    {
        var leftToken = NextToken();
        var leftRegisterCode = RegisterPair();
        if (leftRegisterCode != null) {
            if (!leftToken.IsReservedWord(Keyword.De)) { ShowInvalidRegister(leftToken); }
            var rightToken = AcceptReservedWord(',');
            var rightRegisterCode = RegisterPair();
            if (rightRegisterCode != null) {
                if (!rightToken.IsReservedWord(Keyword.Hl)) { ShowInvalidRegister(rightToken); }
                // EX DE,HL
                WriteByte(0b11101011);
            }
            else {
                ShowSyntaxError();
            }
        }
        else if (leftToken.IsReservedWord(Keyword.Af)) {
            NextToken();
            var rightToken = AcceptReservedWord(',');
            if (!rightToken.IsReservedWord(Keyword.AfX)) { ShowSyntaxError(); }
            NextToken();
            //	EX AF,AF'
            WriteByte(0b00001000);
        }
        else if (leftToken.IsReservedWord('(')) {
            leftToken = NextToken();
            leftRegisterCode = RegisterPair();
            if (leftRegisterCode == null)
                return;
            if (!leftToken.IsReservedWord(Keyword.Sp)) { ShowInvalidRegister(leftToken); }
            AcceptReservedWord(')');
            var rightToken = AcceptReservedWord(',');
            var rightRegisterCode = RegisterPair();
            if (rightRegisterCode != null) {
                if (!rightToken.IsReservedWord(Keyword.Hl)) { ShowInvalidRegister(rightToken); }
                //	EX (SP),HL
                WriteByte(0b11100011);
                return;
            }
            rightRegisterCode = IndexRegister();
            if (rightRegisterCode == null)
                return;
            //	EX (SP),HL IX or IY
            WriteByte(rightRegisterCode.Value);
            WriteByte(0b11100011);
        }
        else {
            ShowSyntaxError();
        }
    }
    /**
     * "PUSH ?".
     */
    private void PushInstruction()
    {
        var operand = NextToken();
        {
            if (operand.IsReservedWord(Keyword.Af)) {
                NextToken();
                //	PUSH AF
                WriteByte(0b11110101);
                return;
            }
        }
        {
            var registerCode = RegisterPair();
            if (registerCode != null) {
                if (operand.IsReservedWord(Keyword.Sp)) { ShowInvalidRegister(operand); }
                //	PUSH rp
                WriteByte(0b11000101 | (registerCode.Value << 4));
                return;
            }
        }
        {
            var registerCode = IndexRegister();
            if (registerCode != null) {
                //	PUSH IX or IY
                WriteByte(registerCode.Value);
                WriteByte(0b11100101);
                return;
            }
        }
        ShowSyntaxError();
    }
    /**
     * "POP ?".
     */
    private void PopInstruction()
    {
        var operand = NextToken();
        {
            if (operand.IsReservedWord(Keyword.Af)) {
                NextToken();
                //	POP AF
                WriteByte(0b11110001);
                return;
            }
        }
        {
            var registerCode = RegisterPair();
            if (registerCode != null) {
                if (operand.IsReservedWord(Keyword.Sp)) { ShowInvalidRegister(operand); }
                //	POP rp
                WriteByte(0b11000001 | (registerCode.Value << 4));
                return;
            }
        }
        {
            var registerCode = IndexRegister();
            if (registerCode != null) {
                //	POP IX or IY
                WriteByte(registerCode.Value);
                WriteByte(0b11100001);
                return;
            }
        }
        ShowSyntaxError();
    }
    /**
     * "RL? RR? SL? SR?".
     */
    private static readonly Dictionary<int, int> RotateOrShiftInstructions = new()
    {
        {Keyword.Rlc, 0b00000000},
        {Keyword.Rl, 0b00010000},
        {Keyword.Rrc, 0b00001000},
        {Keyword.Rr, 0b00011000},
        {Keyword.Sla, 0b00100000},
        {Keyword.Sra, 0b00101000},
        {Keyword.Srl, 0b00111000},
    };
    private bool RotateOrShiftInstruction()
    {
        var reservedWord = LastToken as ReservedWord;
        Debug.Assert(reservedWord != null);
        if (!RotateOrShiftInstructions.TryGetValue(reservedWord.Id, out var instruction)) { return false; }

        NextToken();
        if (LastToken.IsReservedWord('(')) {
            var operand = NextToken();
            if (IndexedAddress(out var registerId, out var value)) {
                switch (registerId) {
                    case Keyword.Hl:
                        WriteByte(0b11001011);
                        WriteByte(instruction | 0b110);
                        break;
                    case Keyword.Ix:
                    case Keyword.Iy:
                        WriteByte(IndexRegisterCodes[registerId]);
                        WriteByte(0b11001011);
                        WriteByte(value.Value);
                        WriteByte(instruction | 0b110);
                        break;
                    default:
                        if (registerId != 0) {
                            ShowInvalidRegister(operand);
                        }
                        else {
                            ShowSyntaxError();
                        }
                        break;
                }
            }
            AcceptReservedWord(')');
            return true;
        }
        var registerCode = SingleRegister();
        if (registerCode != null) {
            WriteByte(0b11001011);
            WriteByte(instruction | registerCode.Value);
            return true;
        }
        ShowSyntaxError();
        return true;
    }
    private bool ConstByteOperationInstruction(int instruction)
    {
        var value = Expression();
        if (value == null)
            return false;
        WriteByte(instruction);
        WriteByte(LastToken, value);
        return true;
    }
    private bool ByteOperationInstruction(int instruction, int shiftCount = 0)
    {
        if (LastToken.IsReservedWord('(')) {
            var operand = NextToken();
            if (IndexedAddress(out var registerId, out var value)) {
                switch (registerId) {
                    case Keyword.Hl:
                        WriteByte(instruction | (0b110 << shiftCount));
                        break;
                    case Keyword.Ix:
                    case Keyword.Iy:
                        WriteByte(IndexRegisterCodes[registerId]);
                        WriteByte(instruction | (0b110 << shiftCount));
                        WriteByte(value.Value);
                        break;
                    default:
                        if (registerId != 0) {
                            ShowInvalidRegister(operand);
                        }
                        else {
                            ShowSyntaxError();
                        }
                        break;
                }
            }
            AcceptReservedWord(')');
            return true;
        }
        var registerCode = SingleRegister();
        if (registerCode == null)
            return false;
        WriteByte(instruction | (registerCode.Value << shiftCount));
        return true;
    }
    /**
     * "SUB,AND,OR,XOR or CP".
     */
    private static readonly Dictionary<int, int[]> ByteOperationInstructions = new()
    {
        {Keyword.Sub, [0b10010000, 0b11010110] },
        {Inu.Assembler.Keyword.And, [0b10100000,0b11100110] },
        {Inu.Assembler.Keyword.Or, [0b10110000,0b11110110] },
        {Inu.Assembler.Keyword.Xor, [0b10101000, 0b11101110] },
        {Keyword.Cp, [0b10111000, 0b11111110] },
    };
    private bool ByteOperationInstruction()
    {
        var reservedWord = LastToken as ReservedWord;
        Debug.Assert(reservedWord != null);
        if (!ByteOperationInstructions.TryGetValue(reservedWord.Id, out var instructions)) { return false; }

        NextToken();
        if (ByteOperationInstruction(instructions[0], 0)) { return true; }
        if (ConstByteOperationInstruction(instructions[1])) { return true; }
        ShowSyntaxError();
        return true;
    }
    /**
     * "ADD".
     */
    private void AddInstruction()
    {
        var leftOperand = NextToken();
        if (leftOperand.IsReservedWord(Keyword.A)) {
            NextToken();
            AcceptReservedWord(',');
            // ADD A,?
            if (ByteOperationInstruction(0b10000000, 0)) { return; }
            if (ConstByteOperationInstruction(0b11000110)) { return; }
        }
        else {
            var leftRegisterCode = RegisterPair();
            if (leftRegisterCode != null) {
                if (!leftOperand.IsReservedWord(Keyword.Hl)) { ShowInvalidRegister(leftOperand); }
                AcceptReservedWord(',');
                var rightRegisterCode = RegisterPair();
                if (rightRegisterCode != null) {
                    //	ADD HL, rp
                    WriteByte(0b00001001 | (rightRegisterCode.Value << 4));
                    return;
                }
            }
            leftRegisterCode = IndexRegister();
            if (leftRegisterCode != null) {
                AcceptReservedWord(',');
                var rightRegisterCode = RegisterPair();
                if (rightRegisterCode != null) {
                    //	ADD IX or IY, rp
                    WriteByte(leftRegisterCode.Value);
                    WriteByte(0b00001001 | (rightRegisterCode.Value << 4));
                    return;
                }
            }
        }
        ShowSyntaxError();
    }
    /**
     * "ADC or SBC".
     */
    private static readonly Dictionary<int, int[]> AddOrSubtractWithCarryInstructions = new()
    {
        {Keyword.Adc, [0b10001000,0b11001110, 0b01001010] },
        {Keyword.Sbc, [0b10011000,0b11011110, 0b01000010] },
    };
    private bool AddOrSubtractWithCarryInstruction()
    {
        var reservedWord = LastToken as ReservedWord;
        Debug.Assert(reservedWord != null);
        if (!AddOrSubtractWithCarryInstructions.TryGetValue(reservedWord.Id, out var instructions)) { return false; }

        var leftOperand = NextToken();
        if (leftOperand.IsReservedWord(Keyword.A)) {
            NextToken();
            AcceptReservedWord(',');
            // ??? A,?
            if (ByteOperationInstruction(instructions[0], 0)) { return true; }
            if (ConstByteOperationInstruction(instructions[1])) { return true; }
        }
        else {
            var leftRegisterCode = RegisterPair();
            if (leftRegisterCode != null) {
                if (!leftOperand.IsReservedWord(Keyword.Hl)) { ShowInvalidRegister(leftOperand); }
                AcceptReservedWord(',');
                var rightRegisterCode = RegisterPair();
                if (rightRegisterCode != null) {
                    //	??? HL, rp
                    WriteByte(0b11101101);
                    WriteByte(instructions[2] | (rightRegisterCode.Value << 4));
                    return true;
                }
            }
        }
        ShowSyntaxError();
        return true;
    }
    /**
     * "INC or DEC".
     */
    private static readonly Dictionary<int, int[]> InclementOrDecrementInstructions = new()
    {
        {Keyword.Inc, [0b00000100, 0b00000011] },
        {Keyword.Dec, [0b00000101, 0b00001011] },
    };
    private bool InclementOrDecrementInstruction()
    {
        var reservedWord = LastToken as ReservedWord;
        Debug.Assert(reservedWord != null);
        if (!InclementOrDecrementInstructions.TryGetValue(reservedWord.Id, out var instructions)) { return false; }

        var operand = NextToken();
        if (ByteOperationInstruction(instructions[0], 3)) { return true; }

        var registerCode = RegisterPair();
        if (registerCode != null) {
            // INC or DEC rp
            WriteByte(instructions[1] | (registerCode.Value << 4));
        }
        else {
            registerCode = IndexRegister();
            if (registerCode != null) {
                // INC or DEC IX or IY
                WriteByte(registerCode.Value);
                WriteByte(instructions[1] | (0b10 << 4));
            }
            else {
                ShowSyntaxError();
            }
        }
        return true;
    }
    /**
     * "BIT SET RES".
     */
    private static readonly Dictionary<int, int> BitOperationInstructions = new()
    {
        {Keyword.Bit, 0b01000000},
        {Keyword.Set, 0b11000000},
        {Keyword.Res, 0b10000000},
    };
    private bool BitOperationInstruction()
    {
        var reservedWord = LastToken as ReservedWord;
        Debug.Assert(reservedWord != null);
        if (!BitOperationInstructions.TryGetValue(reservedWord.Id, out var instruction)) { return false; }

        var leftOperand = NextToken();
        var bitNumber = ByteExpression();
        if (bitNumber != null) {
            if (bitNumber is < 0 or >= 8) {
                ShowOutOfRange(leftOperand, bitNumber.Value);
            }

            AcceptReservedWord(',');
            var registerCode = SingleRegister();
            if (registerCode != null) {
                //	??? b,r
                WriteByte(0b11001011);
                WriteByte(instruction | (bitNumber.Value << 3) | registerCode.Value);
                return true;
            }

            if (MemoryAddress(out var registerId, out var offset)) {
                switch (registerId) {
                    case Keyword.Hl:
                        //	??? b,(HL)
                        WriteByte(0b11001011);
                        WriteByte(instruction | (bitNumber.Value << 3) | 0b110);
                        return true;
                    case Keyword.Ix:
                    case Keyword.Iy:
                        //	??? b,(IX or IY+d)
                        WriteByte(IndexRegisterCodes[registerId]);
                        WriteByte(0b11001011);
                        WriteByte(offset.Value);
                        WriteByte(instruction | (bitNumber.Value << 3) | 0b110);
                        return true;
                }
            }
        }
        else {
            ShowSyntaxError();
        }

        ShowSyntaxError();
        return true;
    }
    private static readonly int[] ConditionCodes = [Keyword.Nz, Keyword.Z, Keyword.Nc, Keyword.C, Keyword.Po, Keyword.Pe, Keyword.P, Keyword.M];
    private static int? ConditionCode(Token token)
    {
        if (token is not ReservedWord reservedWord)
            return null;
        var code = 0;
        foreach (var r in ConditionCodes) {
            if (r == reservedWord.Id) {
                return code;
            }
            ++code;
        }
        return null;
    }

    private int? ConditionCode()
    {
        var conditionCode = ConditionCode(LastToken);
        if (conditionCode != null) {
            NextToken();
        }
        return conditionCode;
    }
    /**
     * "JP".
     */
    private void JumpInstruction()
    {
        var operand = NextToken();

        if (operand.IsReservedWord('(')) {
            operand = NextToken();
            var registerCode = RegisterPair();
            if (registerCode != null) {
                if (!operand.IsReservedWord(Keyword.Hl)) { ShowInvalidRegister(operand); }
                //	JP (HL)
                WriteByte(0b11101001);
            }
            else if ((registerCode = IndexRegister()) != null) {
                //	JP (IX or IY)
                WriteByte(registerCode.Value);
                WriteByte(0b11101001);
            }
            AcceptReservedWord(')');
            return;
        }
        var conditionCode = ConditionCode();
        if (conditionCode != null) {
            operand = AcceptReservedWord(',');
            var address = Expression();
            if (address == null) {
                ShowSyntaxError();
                address = new Address(0);
            }
            // JP cc,nn
            WriteByte(0b11000010 | (conditionCode.Value << 3));
            WriteWord(operand, address);
            return;
        }
        {
            var address = Expression();
            if (address != null) {
                //	JP nn
                WriteByte(0b11000011);
                WriteWord(operand, address);
                return;
            }
        }
        ShowSyntaxError();
    }
    private static readonly Dictionary<int, int> JumpRelativeInstructionCodes = new()
    {
        {Keyword.C,  0b00111000},
        {Keyword.Nc, 0b00110000},
        {Keyword.Z,  0b00101000},
        {Keyword.Nz, 0b00100000},
    };
    private static int? JumpRelativeInstructionCode(Token token)
    {
        if (token is not ReservedWord reservedWord)
            return null;
        if (!JumpRelativeInstructionCodes.TryGetValue(reservedWord.Id, out var instruction)) {
            return null;
        }
        return instruction;
    }

    private int? JumpRelativeInstructionCode()
    {
        var instruction = JumpRelativeInstructionCode(LastToken);
        if (instruction == null) { return null; }
        NextToken();
        return instruction;
    }
    /**
     * "JR".
     */
    private void JumpRelativeInstruction()
    {
        var operand = NextToken();

        var condition = operand;
        var instruction = JumpRelativeInstructionCode();
        if (instruction != null) {
            operand = AcceptReservedWord(',');
        }
        else {
            // JR e
            instruction = 0b00011000;
        }

        if (RelativeOffset(out var address, out var offset)) {
            // JR
            WriteByte(instruction.Value);
            WriteByte(offset);
        }
        else {
            Debug.Assert(address != null);
            var conditionCode = ConditionCode(condition);
            if (conditionCode != null) {
                // JP cc,nn
                WriteByte(0b11000010 | (conditionCode.Value << 3));
            }
            else {
                //	JP nn
                WriteByte(0b11000011);
            }
            WriteWord(operand, address);
        }
    }
    /**
     * "DJNZ".
     */
    private void DecrementJumpRelativeInstruction()
    {
        var operand = NextToken();
        if (RelativeOffset(out var address, out var offset)) {
            // DJNZ e
            WriteByte(0b00010000);
            WriteByte(offset);
        }
        else {
            Debug.Assert(address != null);
            // DEC B
            WriteByte(0b00000101);
            // JP NZ, nn
            WriteByte(0b11000010);
            WriteWord(operand, address);
        }
    }
    private void ConditionalJump(Address address)
    {
        var condition = LastToken;
        var instruction = JumpRelativeInstructionCode(LastToken);
        if (instruction != null) {
            if (!address.IsUndefined()) {
                int offset = RelativeOffset(address);
                if (IsRelativeOffsetInRange(offset)) {
                    NextToken();
                    // JR cc, else
                    WriteByte(instruction.Value);
                    WriteByte(offset);
                    return;
                }
            }
        }
        var conditionCode = ConditionCode(condition);
        if (conditionCode == null)
            return;
        NextToken();
        // JP cc,else
        WriteByte(0b11000010 | (conditionCode.Value << 3));
        WriteWord(LastToken, address);
    }
    private void NegatedConditionalJump(Address address)
    {
        var condition = LastToken;
        int? instruction;
        if (!address.IsUndefined() && (instruction = JumpRelativeInstructionCode(LastToken)) != null) {
            instruction ^= 0b00001000;  // negate condition
            var offset = RelativeOffset(address);
            if (IsRelativeOffsetInRange(offset)) {
                NextToken();
                // JR !cc, else
                WriteByte(instruction.Value);
                WriteByte(offset);
                return;
            }
        }
        var conditionCode = ConditionCode(condition);
        if (conditionCode == null)
            return;
        NextToken();
        conditionCode ^= 1; // negate condition
        // JP !cc,else
        WriteByte(0b11000010 | (conditionCode.Value << 3));
        WriteWord(LastToken, address);
    }
    private void UnconditionalJump(Address address)
    {
        if (!address.IsUndefined()) {
            var offset = RelativeOffset(address);
            if (IsRelativeOffsetInRange(offset)) {
                // JR endif
                WriteByte(0b00011000);
                WriteByte(offset);
                return;
            }
        }
        WriteByte(0b11000011);
        WriteWord(LastToken, address);
    }
    private void StartIf(IfBlock block)
    {
        var address = SymbolAddress(block.ElseId);
        NegatedConditionalJump(address);
    }
    private void IfStatement()
    {
        NextToken();
        var block = NewIfBlock();
        StartIf(block);
    }
    private void ElseStatement()
    {
        if (LastBlock() is IfBlock block) {
            if (block.ElseId <= 0) {
                ShowError(LastToken.Position, "Multiple ELSE statement.");
            }

            var address = SymbolAddress(block.EndId);
            UnconditionalJump(address);
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
        if (LastBlock() is IfBlock block)
        {
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
        if (LastBlock() is not WhileBlock block) {
            ShowNoStatementError(LastToken, "WHILE");
            NextToken();
            return;
        }

        var repeatAddress = SymbolAddress(block.RepeatId);
        var instruction = JumpRelativeInstructionCode(LastToken);
        var next = instruction != null ? 0 : 1;
        if (repeatAddress.Type == CurrentSegment.Type && (RelativeOffset(repeatAddress)) <= next) {
            var address = SymbolAddress(block.BeginId);
            ConditionalJump(address);
            block.EraseEndId();
        }
        else {
            var address = SymbolAddress(block.EndId);
            NegatedConditionalJump(address);
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
                UnconditionalJump(address);
                DefineSymbol(block.EndId, CurrentAddress);
            }
            EndBlock();
        }
        NextToken();
    }
    private void WNzStatement()
    {
        if (LastBlock() is not WhileBlock block) {
            ShowNoStatementError(LastToken, "WHILE");
        }
        else {
            if (block.EndId <= 0) {
                ShowError(LastToken.Position, "WHILE and WNZ cannot be used in the same syntax.");
            }
            var address = SymbolAddress(block.BeginId);
            EndBlock();
            if (!address.IsUndefined()) {
                var offset = RelativeOffset(address);
                if (IsRelativeOffsetInRange(offset)) {
                    // DJNZ e
                    WriteByte(0b00010000);
                    WriteByte(offset);
                    goto exit;
                }
            }
            // DEC B
            WriteByte(0b00000101);
            // JP NZ, nn
            WriteByte(0b11000010);
            WriteWord(LastToken, address);
        }
    exit:
        NextToken();
    }
    /**
     * "CALL".
     */
    private void CallInstruction()
    {
        var operand = NextToken();

        var conditionCode = ConditionCode();
        if (conditionCode != null) {
            operand = AcceptReservedWord(',');
            var address = Expression();
            if (address != null) {
                // CALL cc,nn
                WriteByte(0b11000100 | (conditionCode.Value << 3));
                WriteWord(operand, address);
            }
            else {
                ShowSyntaxError();
            }
        }
        else {
            var address = Expression();
            if (address != null) {
                //	CALL nn
                WriteByte(0b11001101);
                WriteWord(operand, address);
            }
            else {
                ShowSyntaxError();
            }
        }
    }
    /**
     * "RET".
     */
    private void ReturnInstruction()
    {
        NextToken();
        var conditionCode = ConditionCode();
        if (conditionCode != null) {
            // RET cc
            WriteByte(0b11000000 | (conditionCode.Value << 3));
        }
        else {
            //	RET
            WriteByte(0b11001001);
        }
    }
    /**
     * "RST".
     */
    private void RestartInstruction()
    {
        var operand = NextToken();
        var value = ByteExpression();
        if (value != null) {
            if ((value & 0b11000111) != 0) {
                ShowOutOfRange(operand, value.Value);
            }

            value &= 0b00111000;
            // RST p
            WriteByte(0b11000111 | value.Value);
        }
        else {
            ShowSyntaxError();
        }
    }
    /**
     * "IM".
     */
    private static readonly int[] InterruptModeInstructions = [0b01000110, 0b01010110, 0b01011110];
    private void InterruptModeInstruction()
    {
        var operand = NextToken();
        var value = ByteExpression();
        if (value != null) {
            if (value < 0 || value >= InterruptModeInstructions.Length) { ShowOutOfRange(operand, value.Value); }
            //	IM ?
            WriteByte(0b11101101);
            WriteByte(InterruptModeInstructions[value.Value]);
        }
        else { ShowSyntaxError(); }
    }
    /**
     * "IN".
     */
    private void InputInstruction()
    {
        var leftOperand = NextToken();
        var leftRegisterCode = SingleRegister();
        if (leftRegisterCode != null) {
            AcceptReservedWord(',');
            var rightOperand = AcceptReservedWord('(');
            var rightRegisterCode = SingleRegister();
            if (rightRegisterCode != null) {
                if (!rightOperand.IsReservedWord(Keyword.C)) {
                    ShowInvalidRegister(rightOperand);
                }

                //	IN r,(C)
                WriteByte(0b11101101);
                WriteByte(0b01000000 | (leftRegisterCode.Value << 3));
            }
            else {
                if (!leftOperand.IsReservedWord(Keyword.A)) {
                    ShowInvalidRegister(leftOperand);
                }

                var value = ByteExpression();
                if (value != null) {
                    //	IN a,(n)
                    WriteByte(0b11011011);
                    WriteByte(value.Value);
                }
                else {
                    ShowSyntaxError();
                }
            }

            AcceptReservedWord(')');
        }
        else {
            ShowSyntaxError();
        }
    }
    /**
     * "OUT".
     */
    private void OutputInstruction()
    {
        NextToken();
        var leftOperand = AcceptReservedWord('(');

        var leftRegisterCode = SingleRegister();
        if (leftRegisterCode != null) {
            if (!leftOperand.IsReservedWord(Keyword.C)) { ShowInvalidRegister(leftOperand); }
            AcceptReservedWord(')');
            AcceptReservedWord(',');
            var rightRegisterCode = SingleRegister();
            if (rightRegisterCode != null) {
                //	OUT (C),r
                WriteByte(0b11101101);
                WriteByte(0b01000001 | (rightRegisterCode.Value << 3));
            }
            else {
                ShowSyntaxError();
            }
        }
        else {
            var value = ByteExpression();
            if (value != null) {
                AcceptReservedWord(')');
                var rightOperand = AcceptReservedWord(',');
                var rightRegisterCode = SingleRegister();
                if (rightRegisterCode != null) {
                    if (rightOperand.IsReservedWord(Keyword.A)) {
                        //	OUT (n),a
                        WriteByte(0b11010011);
                        WriteByte(value.Value);
                    }
                    else {
                        ShowInvalidRegister(rightOperand);
                    }
                }
                else {
                    ShowSyntaxError();
                }
            }
            else {
                ShowSyntaxError();
            }
        }
    }

    private static readonly Dictionary<int, Action<Assembler>> Actions = new()
    {
        {Keyword.Ld, a => { a.LoadInstruction(); }    },
        {Keyword.Ex, a => { a.ExchangeInstruction(); } },
        {Keyword.Push, a => { a.PushInstruction(); }},
        {Keyword.Pop, a => { a.PopInstruction(); }},
        {Keyword.Add, a => { a.AddInstruction(); }},
        {Keyword.Jp, a => { a.JumpInstruction(); }},
        {Keyword.Jr, a => { a.JumpRelativeInstruction(); }},
        {Keyword.DjNz, a => { a.DecrementJumpRelativeInstruction(); }},
        {Keyword.Call, a => { a.CallInstruction(); }},
        {Keyword.Ret, a => { a.ReturnInstruction(); }},
        {Keyword.Rst, a => { a.RestartInstruction(); }},
        {Keyword.Im, a => { a.InterruptModeInstruction(); }},
        {Keyword.In, a => { a.InputInstruction(); }},
        {Keyword.Out,a => { a.OutputInstruction(); }},
        {Inu.Assembler.Keyword.If, a => { a.IfStatement(); }},
        {Inu.Assembler.Keyword.Else, a => { a.ElseStatement(); }},
        {Inu.Assembler.Keyword.EndIf,a => { a.EndIfStatement(); }},
        {Inu.Assembler.Keyword.ElseIf, a => { a.ElseIfStatement(); }},
        {Inu.Assembler.Keyword.Do, a => { a.DoStatement(); }},
        {Inu.Assembler.Keyword.While, a => { a.WhileStatement(); }},
        {Inu.Assembler.Keyword.WEnd, a=>{a.WEndStatement(); }},
        {Keyword.DWNz, a=>{a.WNzStatement(); }},
    };
    protected override bool Instruction()
    {
        if (InstructionWithoutOperand()) { return true; }
        if (RotateOrShiftInstruction()) { return true; }
        if (AddOrSubtractWithCarryInstruction()) { return true; }
        if (ByteOperationInstruction()) { return true; }
        if (InclementOrDecrementInstruction()) { return true; }
        if (BitOperationInstruction()) { return true; }

        var reservedWord = LastToken as ReservedWord;
        Debug.Assert(reservedWord != null);
        if (!Actions.TryGetValue(reservedWord.Id, out var action)) { return false; }
        action(this);
        return true;
    }
}