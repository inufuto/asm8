using Inu.Language;
using System.Diagnostics;

namespace Inu.Assembler.Sm8521;

internal class Assembler(int version) : BigEndianAssembler(new Tokenizer(version))
{
    private const int Zero = 6;
    private const int Always = 8;
    private const int NotZero = 14;

    private void ShowInvalidRegister(Token token, int index)
    {
        ShowError(token.Position, "Invalid register: " + token + index);
    }

    private Address ParseExpressionNotNull()
    {
        var token = LastToken;
        var value = Expression();
        if (value == null) {
            ShowSyntaxError(token);
            return new Address(0);
        }
        return value;
    }


    private int? ParseConstantExpression()
    {
        var token = LastToken;
        var value = Expression();
        if (value == null) {
            return null;
        }
        if (!value.IsConst()) {
            ShowError(token.Position, "Must be constant: " + token);
            return 0;
        }
        return value.Value;
    }

    private int ParseConstantExpressionNotNull()
    {
        var token = LastToken;
        var value = ParseConstantExpression();
        if (value == null) {
            ShowSyntaxError(token);
            return 0;
        }
        return value.Value;
    }


    private int? ParseRegister()
    {
        if (!LastToken.IsReservedWord('R')) return null;
        NextToken();
        return ParseRegisterIndexNotNull();
    }

    private int ParseRegisterNotNull()
    {
        var register = ParseRegister();
        if (register == null) {
            ShowSyntaxError();
            return 1;
        }
        return register.Value;
    }

    private int ParseRegisterIndexNotNull()
    {
        var token = LastToken;
        var register = ParseConstantExpressionNotNull();
        if (!IsInRegisterRange(register)) {
            ShowOutOfRange(token, register);
            return 1;
        }
        return register;
    }

    private static bool IsInRegisterRange(int register)
    {
        return register is >= 0 and <= 7;
    }


    private int? ParseRegisterPair()
    {
        var token = LastToken;
        if (!LastToken.IsReservedWord(Keyword.RR)) return null;
        NextToken();
        var registerPairIndex = ParseRegisterPairIndexNotNull();
        //if ((registerPairIndex & 1) != 0) {
        //    ShowInvalidRegister(token, registerPairIndex);
        //}
        return registerPairIndex;
    }

    private int ParseRegisterPairNotNull()
    {
        var register = ParseRegisterPair();
        if (register == null) {
            ShowSyntaxError();
            return 1;
        }
        return register.Value;
    }

    private int ParseRegisterPairIndexNotNull()
    {
        var token = LastToken;
        var register = ParseConstantExpressionNotNull();
        if (!IsInRegisterPairRange(register)) {
            ShowOutOfRange(token, register);
            return 2;
        }
        if (register >= 8) {
            register = (register & 7) | 1;
        }
        return register;
    }

    private static bool IsInRegisterPairRange(int register)
    {
        return register is >= 0 and <= 15;
    }


    private Address? ParseRegisterFile()
    {
        if (!LastToken.IsReservedWord('R')) return null;
        NextToken();
        return ParseRegisterFileIndexNotNull();
    }

    private Address ParseRegisterFileNotNull()
    {
        var registerFile = ParseRegisterFile();
        if (registerFile == null) {
            ShowSyntaxError();
            return new Address(1);
        }
        return registerFile;
    }

    private Address ParseRegisterFileIndexNotNull()
    {
        var token = LastToken;
        var registerFile = ParseExpressionNotNull();
        if (registerFile.IsConst() && !IsInRegisterFileRange(registerFile.Value)) {
            ShowOutOfRange(token, registerFile.Value);
            return new Address(1);

        }
        return registerFile;
    }

    private Address? ParseRegisterPairFile()
    {
        if (!LastToken.IsReservedWord(Keyword.RR)) return null;
        var token = LastToken;
        NextToken();
        var registerPairFileIndex = ParseRegisterPairFileIndexNotNull();
        if (registerPairFileIndex.IsConst() && (registerPairFileIndex.Value & 1) != 0) {
            ShowInvalidRegister(token, registerPairFileIndex.Value);
        }
        return registerPairFileIndex;
    }

    private Address ParseRegisterPairFileNotNull()
    {
        var registerFile = ParseRegisterPairFile();
        if (registerFile == null) {
            ShowSyntaxError();
            return new Address(2);
        }
        return registerFile;
    }

    private Address ParseRegisterPairFileIndexNotNull()
    {
        var token = LastToken;
        var registerFile = ParseExpressionNotNull();
        if (registerFile.IsConst() && !IsInRegisterFileRange(registerFile.Value)) {
            ShowOutOfRange(token, registerFile.Value);
            return new Address(2);

        }
        return registerFile;
    }

    private static bool IsInRegisterFileRange(int registerValue)
    {
        return registerValue is >= 0 and < 0x100;
    }


    private int ParseBitIndexNotNull()
    {
        var token = LastToken;
        var bit = ParseConstantExpressionNotNull();
        if (!IsInBitRange(bit)) {
            ShowOutOfRange(token, bit);
            return 0;
        }
        return bit;
    }

    private static bool IsInBitRange(int bit)
    {
        return (bit is >= 0 and < 8);
    }

    private static readonly Dictionary<int, int> Conditions = new()
    {
        {Keyword.F,0},
        {Keyword.LT,1},
        {Keyword.LE,2},
        {Keyword.ULE,3},
        {Keyword.OV,4},
        {Keyword.MI,5},
        {Keyword.EQ,6},
        {Keyword.Z,6},
        {Keyword.ULT,7},
        {Keyword.C,7},
        {Keyword.T,8},
        {Keyword.GE,9},
        {Keyword.GT,10},
        {Keyword.UGT,11},
        {Keyword.NOV,12},
        {Keyword.PL,13},
        {Keyword.NE,14},
        {Keyword.NZ,14},
        {Keyword.UGE,15},
        {Keyword.NC,15},
    };
    private int? ParseCondition()
    {
        if (LastToken is not ReservedWord reservedWord) return null;
        if (!Conditions.TryGetValue(reservedWord.Id, out var value)) return null;
        NextToken();
        return value;
    }

    private int InvertCondition(int condition)
    {
        return condition ^ 0b1000;
    }

    private static readonly int[] PortNames = new[]
    {
        Keyword.IE0, Keyword.IE1, Keyword.IR0, Keyword.IR1, Keyword.P0, Keyword.P1, Keyword.P2, Keyword.P3
    };

    private int? ParsePortName()
    {
        for (var i = 0; i < PortNames.Length; ++i) {
            if (!LastToken.IsReservedWord(PortNames[i])) continue;
            NextToken();
            return i;
        }
        return null;
    }


    private void BitSetReset(int memoryOp, int registerOp)
    {
        {
            var registerToken = LastToken;
            var registerFile = ParseRegisterFile();
            if (registerFile != null) {
                AcceptReservedWord(',');
                var bit = ParseBitIndexNotNull();
                WriteByte(registerOp | bit);
                WriteByte(registerToken, registerFile);
                return;
            }
        }
        {
            var addressToken = LastToken;
            var address = Expression();
            if (address != null) {
                if (LastToken.IsReservedWord('(')) {
                    var registerToken = NextToken();
                    var register = ParseRegisterNotNull();
                    if (register == 0) {
                        ShowInvalidRegister(registerToken, register);
                    }
                    AcceptReservedWord(')');
                    AcceptReservedWord(',');
                    var bit = ParseBitIndexNotNull();
                    WriteByte(memoryOp);
                    WriteByte(register << 3 | bit);
                    WriteByte(addressToken, address);
                }
                else {
                    AcceptReservedWord(',');
                    var bit = ParseBitIndexNotNull();
                    WriteByte(memoryOp);
                    WriteByte(bit);
                    WriteByte(addressToken, address);
                }
            }
            else {
                ShowSyntaxError(addressToken);
            }
        }

    }

    private void BitMove()
    {
        if (LastToken.IsReservedWord(Keyword.BF)) {
            NextToken();
            AcceptReservedWord(',');
            var registerToken = LastToken;
            var registerFile = ParseRegisterFileNotNull();
            AcceptReservedWord(',');
            var bit = ParseBitIndexNotNull();
            WriteByte(0b01001110);
            WriteByte(bit);
            WriteByte(registerToken, registerFile);
        }
        else {
            var registerToken = LastToken;
            var registerFile = ParseRegisterFileNotNull();
            AcceptReservedWord(',');
            var bit = ParseBitIndexNotNull();
            AcceptReservedWord(',');
            AcceptReservedWord(Keyword.BF);
            WriteByte(0b01001110);
            WriteByte(0b01000000 | bit);
            WriteByte(registerToken, registerFile);
        }
    }

    private void OperateRegisterOrIndirect(int registerOp, int indirectOp1, int indirectOp2)
    {
        if (LastToken.IsReservedWord('@')) {
            NextToken();
            var register = ParseRegisterNotNull();
            WriteByte(indirectOp1);
            WriteByte(indirectOp2 | register << 3);
        }
        else {
            var registerToken = LastToken;
            var registerFile = ParseRegisterFileNotNull();
            WriteByte(registerOp);
            WriteByte(registerToken, registerFile);
        }
    }

    private void Move()
    {
        if (LastToken.IsReservedWord('@')) {
            NextToken();
            {
                var destinationRegister = ParseRegister();
                if (destinationRegister != null) {
                    AcceptReservedWord(',');
                    var sourceRegister = ParseRegister();
                    if (sourceRegister != null) {
                        // Register Indirect, Register
                        WriteByte(0b00101001);
                        WriteByte(sourceRegister.Value << 3 | destinationRegister.Value);
                        return;
                    }
                    {
                        var valueToken = LastToken;
                        var value = Expression();
                        if (value != null) {
                            // Register Indirect, Immediate
                            WriteByte(0b01011011);
                            WriteByte(destinationRegister.Value);
                            WriteByte(valueToken, value);
                            return;
                        }
                    }
                }
            }
            {
                var destinationRegister = ParseRegisterPair();
                if (destinationRegister != null) {
                    AcceptReservedWord(',');
                    var sourceRegister = ParseRegister();
                    if (sourceRegister != null) {
                        // Register Pair Indirect, Register
                        WriteByte(0b00111001);
                        WriteByte(sourceRegister.Value << 3 | destinationRegister.Value);
                        return;
                    }
                }
            }
            {
                var addressToken = LastToken;
                var value = Expression();
                if (value != null) {
                    AcceptReservedWord(',');
                    var sourceRegister = ParseRegister();
                    if (sourceRegister != null) {
                        // Direct Indirect, Register
                        WriteByte(0b00111001);
                        WriteByte(0b10000000 | sourceRegister.Value << 3);
                        WriteWord(addressToken, value);
                        return;
                    }
                }
            }
        }
        else if (LastToken.IsReservedWord('(')) {
            NextToken();
            {
                var destinationRegister = ParseRegister();
                if (destinationRegister != null) {
                    AcceptReservedWord(')');
                    AcceptReservedWord('+');
                    AcceptReservedWord(',');
                    {
                        var sourceRegister = ParseRegister();
                        if (sourceRegister != null) {
                            // Register Indirect Auto Increment , Register
                            WriteByte(0b00101001);
                            WriteByte(0b01000000 | sourceRegister.Value << 3 | destinationRegister.Value);
                            return;
                        }
                    }
                    {
                        var valueToken = LastToken;
                        var value = Expression();
                        if (value != null) {
                            // Register Indirect Auto Increment , Immediate
                            WriteByte(0b01011011);
                            WriteByte(0b01000000 | destinationRegister.Value);
                            WriteByte(valueToken, value);
                            return;
                        }
                    }
                }
            }
            {
                var destinationRegister = ParseRegisterPair();
                if (destinationRegister != null) {
                    AcceptReservedWord(')');
                    AcceptReservedWord('+');
                    AcceptReservedWord(',');
                    {
                        var sourceRegister = ParseRegister();
                        if (sourceRegister != null) {
                            // Register Pair Indirect Auto Increment , Register
                            WriteByte(0b00111001);
                            WriteByte(0b01000000 | sourceRegister.Value << 3 | destinationRegister.Value);
                            return;
                        }
                    }
                }
            }
        }
        else if (LastToken.IsReservedWord('-')) {
            var sign = LastToken;
            NextToken();
            if (LastToken.IsReservedWord('(')) {
                NextToken();
                {
                    var destinationRegister = ParseRegister();
                    if (destinationRegister != null) {
                        AcceptReservedWord(')');
                        AcceptReservedWord(',');
                        {
                            var sourceRegister = ParseRegister();
                            if (sourceRegister != null) {
                                // Register Indirect Auto Decrement , Register
                                WriteByte(0b00101001);
                                WriteByte(0b11000000 | sourceRegister.Value << 3 | destinationRegister.Value);
                                return;
                            }
                        }
                        {
                            var valueToken = LastToken;
                            var value = Expression();
                            if (value != null) {
                                // Register Indirect Auto Decrement , Immediate
                                WriteByte(0b01011011);
                                WriteByte(0b11000000 | destinationRegister.Value);
                                WriteByte(valueToken, value);
                                return;
                            }
                        }
                    }
                }
                {
                    var destinationRegister = ParseRegisterPair();
                    if (destinationRegister != null) {
                        AcceptReservedWord(')');
                        AcceptReservedWord(',');
                        {
                            var sourceRegister = ParseRegister();
                            if (sourceRegister != null) {
                                // Register Pair Indirect Auto Decrement , Register
                                WriteByte(0b00111001);
                                WriteByte(0b11000000 | sourceRegister.Value << 3 | destinationRegister.Value);
                                return;
                            }
                        }
                    }
                }
            }
            else {
                ReturnToken(sign);
            }
        }
        else if (LastToken.IsReservedWord(Keyword.PS0)) {
            NextToken();
            AcceptReservedWord(',');
            var valueToken = LastToken;
            var value = Expression();
            if (value == null) {
                ShowSyntaxError(valueToken);
                value = new Address(0);
            }
            // PS0, Immediate
            WriteByte(0b00101110);
            WriteByte(valueToken, value);
            return;
        }
        {
            var port = ParsePortName();
            if (port != null) {
                AcceptReservedWord(',');
                var valueToken = LastToken;
                var value = Expression();
                if (value == null) {
                    ShowSyntaxError(valueToken);
                    value = new Address(0);
                }
                // , Immediate
                WriteByte(0b11001000 | port.Value);
                WriteByte(valueToken, value);
                return;

            }
        }
        {
            var destinationRegisterToken = LastToken;
            var destinationRegisterFile = ParseRegisterFile();
            if (destinationRegisterFile != null) {
                AcceptReservedWord(',');
                if (destinationRegisterFile.IsConst() && IsInRegisterRange(destinationRegisterFile.Value)) {
                    var destinationRegister = destinationRegisterFile.Value;
                    if (LastToken.IsReservedWord('@')) {
                        NextToken();
                        {
                            var sourceRegister = ParseRegister();
                            if (sourceRegister != null) {
                                //	Register, Register Indirect
                                WriteByte(0b00101000);
                                WriteByte(destinationRegister << 3 | sourceRegister.Value);
                                return;
                            }
                        }
                        {
                            var sourceRegister = ParseRegisterPair();
                            if (sourceRegister != null) {
                                // Register, Register Pair Indirect
                                WriteByte(0b00111000);
                                WriteByte(destinationRegister << 3 | sourceRegister.Value);
                                return;
                            }
                        }
                        {
                            var sourceAddressToken = LastToken;
                            var sourceAddress = Expression();
                            if (sourceAddress != null) {
                                // 	Register, Direct Indirect
                                WriteByte(0b00111000);
                                WriteByte(0b10000000 | destinationRegister << 3);
                                WriteWord(sourceAddressToken, sourceAddress);
                                return;
                            }
                        }
                    }
                    else if (LastToken.IsReservedWord('(')) {
                        NextToken();
                        {
                            var sourceRegister = ParseRegister();
                            if (sourceRegister != null) {
                                AcceptReservedWord(')');
                                AcceptReservedWord('+');
                                // 	Register, Register Indirect Auto Increment
                                WriteByte(0b00101000);
                                WriteByte(0b01000000 | destinationRegister << 3 | sourceRegister.Value);
                                return;
                            }
                        }
                        {
                            var sourceRegister = ParseRegisterPair();
                            if (sourceRegister != null) {
                                AcceptReservedWord(')');
                                AcceptReservedWord('+');
                                // 	Register, Register Pair Indirect Auto Increment
                                WriteByte(0b00111000);
                                WriteByte(0b01000000 | destinationRegister << 3 | sourceRegister.Value);
                                return;
                            }
                        }
                    }
                    else if (LastToken.IsReservedWord('-')) {
                        var sign = LastToken;
                        NextToken();
                        if (LastToken.IsReservedWord('(')) {
                            NextToken();
                            {
                                var sourceRegister = ParseRegister();
                                if (sourceRegister != null) {
                                    AcceptReservedWord(')');
                                    // Register, Register Indirect Auto Decrement
                                    WriteByte(0b00101000);
                                    WriteByte(0b11000000 | destinationRegister << 3 | sourceRegister.Value);
                                    return;
                                }
                            }
                            {
                                var sourceRegister = ParseRegisterPair();
                                if (sourceRegister != null) {
                                    AcceptReservedWord(')');
                                    // Register, Register Pair Indirect Auto Decrement
                                    WriteByte(0b00111000);
                                    WriteByte(0b11000000 | destinationRegister << 3 | sourceRegister.Value);
                                    return;
                                }
                            }
                        }
                        else {
                            ReturnToken(sign);
                        }
                    }
                    {
                        var sourceAddressToken = LastToken;
                        var sourceAddress = Expression();
                        if (sourceAddress != null) {
                            if (LastToken.IsReservedWord('(')) {
                                {
                                    var sourceRegisterToken = NextToken();
                                    var sourceRegister = ParseRegister();
                                    if (sourceRegister != null) {
                                        if (sourceRegister.Value == 0) {
                                            ShowInvalidRegister(sourceRegisterToken, sourceRegister.Value);
                                        }
                                        AcceptReservedWord(')');
                                        // 	Register, Register Index
                                        WriteByte(0b00101000);
                                        WriteByte(0b10000000 | destinationRegister << 3 | sourceRegister.Value);
                                        WriteByte(sourceAddressToken, sourceAddress);
                                        return;
                                    }
                                }
                                {
                                    var sourceRegisterToken = LastToken;
                                    var sourceRegister = ParseRegisterPair();
                                    if (sourceRegister != null) {
                                        if (sourceRegister == 0) {
                                            ShowInvalidRegister(sourceRegisterToken, sourceRegister.Value);
                                        }
                                        AcceptReservedWord(')');
                                        // Register, Register Pair Index
                                        WriteByte(0b00111000);
                                        WriteByte(0b10000000 | destinationRegister << 3 | sourceRegister.Value);
                                        WriteWord(sourceAddressToken, sourceAddress);
                                        return;
                                    }
                                }
                            }
                            {
                                // Register, Immediate
                                WriteByte(0b11000000 | destinationRegister);
                                WriteByte(sourceAddressToken, sourceAddress);
                                return;
                            }
                        }
                    }
                    {
                        var port = ParsePortName();
                        if (port != null) {
                            // Register, Register File
                            WriteByte(0b10110000 | destinationRegister);
                            WriteByte(0x10 + port.Value);
                            return;
                        }
                    }
                }
                {
                    var sourceRegisterToken = LastToken;
                    var sourceRegisterFile = ParseRegisterFile();
                    if (sourceRegisterFile != null) {
                        if (destinationRegisterFile.IsConst() && IsInRegisterRange(destinationRegisterFile.Value)) {
                            // Register, Register File
                            WriteByte(0b10110000 | destinationRegisterFile.Value);
                            WriteByte(sourceRegisterToken, sourceRegisterFile);
                            return;
                        }

                        if (sourceRegisterFile.IsConst() && IsInRegisterRange(sourceRegisterFile.Value)) {
                            // Register File, Register
                            WriteByte(0b10111000 | sourceRegisterFile.Value);
                            WriteByte(destinationRegisterToken, destinationRegisterFile);
                            return;
                        }
                        // Register File, Register File
                        WriteByte(0b01001000);
                        WriteByte(sourceRegisterToken, sourceRegisterFile);
                        WriteByte(destinationRegisterToken, destinationRegisterFile);
                        return;
                    }
                }
                {
                    var valueToken = LastToken;
                    var value = Expression();
                    if (value != null) {
                        // Register File, Immediate
                        WriteByte(0b01011000);
                        WriteByte(valueToken, value);
                        WriteByte(destinationRegisterToken, destinationRegisterFile);
                        return;
                    }
                }
            }
        }
        {
            var destinationAddressToken = LastToken;
            var destinationAddress = Expression();
            if (destinationAddress != null) {
                if (LastToken.IsReservedWord('(')) {
                    var destinationRegisterToken = NextToken();
                    {
                        var destinationRegister = ParseRegister();
                        if (destinationRegister != null) {
                            if (destinationRegister == 0) {
                                ShowInvalidRegister(destinationRegisterToken, destinationRegister.Value);
                            }
                            AcceptReservedWord(')');
                            AcceptReservedWord(',');
                            {
                                var sourceRegister = ParseRegister();
                                if (sourceRegister != null) {
                                    // Register Index, Register
                                    WriteByte(0b00101001);
                                    WriteByte(0b10000000 | sourceRegister.Value << 3 | destinationRegister.Value);
                                    WriteByte(destinationAddressToken, destinationAddress);
                                    return;
                                }
                            }
                            {
                                var valueToken = LastToken;
                                var value = Expression();
                                if (value != null) {
                                    // Register Index, Immediate
                                    WriteByte(0b01011011);
                                    WriteByte(0b10000000 | destinationRegister.Value);
                                    WriteByte(destinationAddressToken, destinationAddress);
                                    WriteByte(valueToken, value);
                                    return;
                                }
                            }
                        }
                    }
                    {
                        var destinationRegister = ParseRegisterPair();
                        if (destinationRegister != null) {
                            if (destinationRegister == 0) {
                                ShowInvalidRegister(destinationRegisterToken, destinationRegister.Value);
                            }
                            AcceptReservedWord(')');
                            AcceptReservedWord(',');
                            var sourceRegister = ParseRegister();
                            if (sourceRegister != null) {
                                // Register Pair Index, Register
                                WriteByte(0b00111001);
                                WriteByte(0b10000000 | sourceRegister.Value << 3 | destinationRegister.Value);
                                WriteWord(destinationAddressToken, destinationAddress);
                                return;
                            }
                        }
                    }
                }
            }
        }
        ShowSyntaxError(LastToken);
    }

    private void MoveUnderMask()
    {
        var destinationRegisterToken = LastToken;
        var destinationRegisterFile = ParseRegisterFileNotNull();
        AcceptReservedWord(',');
        var value1Token = LastToken;
        var value1 = Expression();
        if (value1 != null) {
            AcceptReservedWord(',');
            {
                var sourceRegisterToken = LastToken;
                var sourceRegisterFile = ParseRegisterFile();
                if (sourceRegisterFile != null) {
                    // Register File, Immediate, Register File
                    WriteByte(0b01011110);
                    WriteByte(destinationRegisterToken, destinationRegisterFile);
                    WriteByte(value1Token, value1);
                    WriteByte(sourceRegisterToken, sourceRegisterFile);
                    return;
                }
            }
            {
                var value2Token = LastToken;
                var value2 = Expression();
                if (value2 != null) {
                    // Register File, Immediate, Immediate
                    WriteByte(0b01011111);
                    WriteByte(destinationRegisterToken, destinationRegisterFile);
                    WriteByte(value1Token, value1);
                    WriteByte(value2Token, value2);
                    return;
                }
            }
        }
        ShowSyntaxError(LastToken);
    }

    private void MoveWord()
    {
        if (LastToken.IsReservedWord('@')) {
            NextToken();
            {
                var destinationRegister = ParseRegisterPair();
                if (destinationRegister != null) {
                    AcceptReservedWord(',');
                    var sourceRegister = ParseRegisterPairNotNull();
                    // Register Pair Indirect, Register Pair
                    WriteByte(0b00111011);
                    WriteByte(sourceRegister << 3 | destinationRegister.Value);
                    return;
                }
            }
            {
                var addressToken = LastToken;
                var address = Expression();
                if (address != null) {
                    AcceptReservedWord(',');
                    var sourceRegister = ParseRegisterPairNotNull();
                    // Direct Indirect, Register Pair
                    WriteByte(0b00111011);
                    WriteByte(0b10000000 | sourceRegister << 3);
                    WriteWord(addressToken, address);
                    return;
                }
            }
        }
        else if (LastToken.IsReservedWord('(')) {
            NextToken();
            var destinationRegister = ParseRegisterPairNotNull();
            AcceptReservedWord(')');
            AcceptReservedWord('+');
            AcceptReservedWord(',');
            var sourceRegister = ParseRegisterPairNotNull();
            // Register Pair Indirect Auto Increment , Register Pair
            WriteByte(0b00111011);
            WriteByte(0b01000000 | sourceRegister << 3 | destinationRegister);
            return;
        }
        else if (LastToken.IsReservedWord('-')) {
            var sign = LastToken;
            NextToken();
            if (LastToken.IsReservedWord('(')) {
                NextToken();
                var destinationRegister = ParseRegisterPairNotNull();
                AcceptReservedWord(')');
                AcceptReservedWord(',');
                var sourceRegister = ParseRegisterPairNotNull();
                // Register Pair Indirect Auto Decrement , Register Pair
                WriteByte(0b00111011);
                WriteByte(0b11000000 | sourceRegister << 3 | destinationRegister);
                return;
            }
            ReturnToken(sign);
        }
        {
            var destinationRegisterToken = LastToken;
            var destinationRegisterFile = ParseRegisterPairFile();
            if (destinationRegisterFile != null) {
                if (destinationRegisterFile.IsConst() && IsInRegisterPairRange(destinationRegisterFile.Value)) {
                    var destinationRegister = destinationRegisterFile.Value;
                    if (destinationRegister >= 8) {
                        destinationRegister = (destinationRegister & 7) | 1;
                    }
                    AcceptReservedWord(',');
                    if (LastToken.IsReservedWord('@')) {
                        NextToken();
                        {
                            var sourceRegister = ParseRegisterPair();
                            if (sourceRegister != null) {
                                // Register Pair, Register Pair Indirect
                                WriteByte(0b00111010);
                                WriteByte(destinationRegister << 3 | sourceRegister.Value);
                                return;
                            }
                        }
                        {
                            var addressToken = LastToken;
                            var address = Expression();
                            if (address != null) {
                                // Register Pair, Direct Indirect
                                WriteByte(0b00111010);
                                WriteByte(0b10000000 | destinationRegister << 3);
                                WriteWord(addressToken, address);
                                return;
                            }
                        }
                    }
                    else if (LastToken.IsReservedWord('(')) {
                        NextToken();
                        var sourceRegister = ParseRegisterPair();
                        if (sourceRegister != null) {
                            AcceptReservedWord(')');
                            AcceptReservedWord('+');
                            // Register Pair, Register Pair Indirect Auto Increment
                            WriteByte(0b00111010);
                            WriteByte(0b01000000 | destinationRegister << 3 | sourceRegister.Value);
                            return;
                        }

                    }
                    else if (LastToken.IsReservedWord('-')) {
                        var sign = LastToken;
                        NextToken();
                        if (LastToken.IsReservedWord('(')) {
                            NextToken();
                            var sourceRegister = ParseRegisterPairNotNull();
                            AcceptReservedWord(')');
                            // Register Pair, Register Pair Indirect Auto Decrement
                            WriteByte(0b00111010);
                            WriteByte(0b11000000 | destinationRegister << 3 | sourceRegister);
                            return;
                        }
                        ReturnToken(sign);
                    }
                    {
                        var sourceRegisterToken = LastToken;
                        var sourceRegisterFile = ParseRegisterPairFile();
                        if (sourceRegisterFile != null) {
                            if (sourceRegisterFile.IsConst() && IsInRegisterPairRange(sourceRegisterFile.Value)) {
                                var sourceRegister = sourceRegisterFile.Value;
                                if (sourceRegister >= 8) {
                                    sourceRegister = (sourceRegister & 7) | 1;
                                }
                                // Register Pair, Register Pair
                                WriteByte(0b00111100);
                                WriteByte(destinationRegister << 3 | sourceRegister);
                                return;
                            }
                            // Register Pair File, Register Pair File
                            WriteByte(0b01001010);
                            WriteByte(sourceRegisterToken, sourceRegisterFile);
                            WriteByte(destinationRegisterToken, destinationRegisterFile);
                            return;
                        }
                    }
                    {
                        var addressToken = LastToken;
                        var address = Expression();
                        if (address != null) {
                            if (LastToken.IsReservedWord('(')) {
                                var sourceRegisterToken = NextToken();
                                var sourceRegister = ParseRegisterPairNotNull();
                                if (sourceRegister == 0) {
                                    ShowInvalidRegister(sourceRegisterToken, sourceRegister);
                                }

                                AcceptReservedWord(')');
                                // Register Pair, Register Pair Index
                                WriteByte(0b00111010);
                                WriteByte(0b10000000 | destinationRegister << 3 | sourceRegister);
                                WriteWord(addressToken, address);
                                return;
                            }
                            // Register Pair, Immediate Long
                            WriteByte(0b01111000 | destinationRegister);
                            WriteWord(addressToken, address);
                            return;
                        }
                    }
                }
                else {
                    AcceptReservedWord(',');
                    {
                        var sourceRegisterToken = LastToken;
                        var sourceRegisterFile = ParseRegisterPairFile();
                        if (sourceRegisterFile != null) {
                            // Register Pair File, Register Pair File
                            WriteByte(0b01001010);
                            WriteByte(sourceRegisterToken, sourceRegisterFile);
                            WriteByte(destinationRegisterToken, destinationRegisterFile);
                            return;
                        }
                    }
                    {
                        var valueToken = LastToken;
                        var value = Expression();
                        if (value != null) {
                            // Register Pair File, Immediate Long
                            WriteByte(0b01001011);
                            WriteByte(destinationRegisterToken, destinationRegisterFile);
                            WriteWord(valueToken, value);
                            return;
                        }
                    }
                }
            }
        }
        {
            var addressToken = LastToken;
            var address = Expression();
            if (address != null) {
                AcceptReservedWord('(');
                var destinationToken = LastToken;
                var destinationRegister = ParseRegisterPairNotNull();
                if (destinationRegister == 0) {
                    ShowInvalidRegister(destinationToken, destinationRegister);
                }
                AcceptReservedWord(')');
                AcceptReservedWord(',');
                var sourceRegister = ParseRegisterPairNotNull();
                // Register Pair Index, Register Pair
                WriteByte(0b00111011);
                WriteByte(0b10000000 | sourceRegister << 3 | destinationRegister);
                WriteWord(addressToken, address);
                return;
            }
        }
        ShowSyntaxError(LastToken);
    }

    private void PushPop(int registerOp, int registerFileOp)
    {
        var registerToken = LastToken;
        var registerFile = ParseRegisterFileNotNull();
        if (registerFile.IsConst() && IsInRegisterRange(registerFile.Value)) {
            // Register
            WriteByte(0b00011011);
            WriteByte(registerOp | registerFile.Value << 3);
            return;
        }
        // Register File
        WriteByte(registerFileOp);
        WriteByte(registerToken, registerFile);
    }

    private void OperateRegisterPairFile(int op)
    {
        var registerPairToken = LastToken;
        var registerPairFile = ParseRegisterPairFileNotNull();
        WriteByte(op);
        WriteByte(registerPairToken, registerPairFile);
    }

    private void OperateByte(int op)
    {
        var destinationRegisterToken = LastToken;
        var destinationRegisterFile = ParseRegisterFileNotNull();
        AcceptReservedWord(',');
        if (destinationRegisterFile.IsConst() && IsInRegisterRange(destinationRegisterFile.Value)) {
            var destinationRegister = destinationRegisterFile.Value;
            if (LastToken.IsReservedWord('@')) {
                NextToken();
                {
                    var sourceRegister = ParseRegister();
                    if (sourceRegister != null) {
                        // Register, Register Indirect
                        WriteByte(0b00100000 | op);
                        WriteByte(destinationRegister << 3 | sourceRegister.Value);
                        return;
                    }
                }
                {
                    var sourceRegister = ParseRegisterPair();
                    if (sourceRegister != null) {
                        // Register, Register Pair Indirect
                        WriteByte(0b00110000 | op);
                        WriteByte(destinationRegister << 3 | sourceRegister.Value);
                        return;
                    }
                }
                {
                    var addressToken = LastToken;
                    var address = Expression();
                    if (address != null) {
                        // Register, Direct Indirect
                        WriteByte(0b00110000 | op);
                        WriteByte(0b10000000 | destinationRegister << 3);
                        WriteWord(addressToken, address);
                        return;
                    }
                }
            }
            else if (LastToken.IsReservedWord('(')) {
                NextToken();
                {
                    var sourceRegister = ParseRegister();
                    if (sourceRegister != null) {
                        AcceptReservedWord(')');
                        AcceptReservedWord('+');
                        // Register, Register Indirect Auto Increment
                        WriteByte(0b00100000 | op);
                        WriteByte(0b01000000 | destinationRegister << 3 | sourceRegister.Value);
                        return;
                    }
                }
                {
                    var sourceRegister = ParseRegisterPair();
                    if (sourceRegister != null) {
                        AcceptReservedWord(')');
                        AcceptReservedWord('+');
                        // Register, Register Pair Indirect Auto Increment
                        WriteByte(0b00110000 | op);
                        WriteByte(0b01000000 | destinationRegister << 3 | sourceRegister.Value);
                        return;
                    }
                }
            }
            else if (LastToken.IsReservedWord('-')) {
                var sign = LastToken;
                NextToken();
                if (LastToken.IsReservedWord('(')) {
                    NextToken();
                    {
                        var sourceRegister = ParseRegister();
                        if (sourceRegister != null) {
                            AcceptReservedWord(')');
                            // Register, Register Indirect Auto Decrement
                            WriteByte(0b00100000 | op);
                            WriteByte(0b11000000 | destinationRegister << 3 | sourceRegister.Value);
                            return;
                        }
                    }
                    {
                        var sourceRegister = ParseRegisterPair();
                        if (sourceRegister != null) {
                            AcceptReservedWord(')');
                            // Register, Register Pair Indirect Auto Decrement
                            WriteByte(0b00110000 | op);
                            WriteByte(0b11000000 | destinationRegister << 3 | sourceRegister.Value);
                            return;
                        }
                    }
                }
                else {
                    ReturnToken(sign);
                }
            }

            {
                var sourceRegisterToken = LastToken;
                var sourceRegisterFile = ParseRegisterFile();
                if (sourceRegisterFile != null) {
                    if (sourceRegisterFile.IsConst() && IsInRegisterRange(sourceRegisterFile.Value)) {
                        var sourceRegister = sourceRegisterFile.Value;
                        // Register, Register
                        WriteByte(0b00010000 | op);
                        WriteByte(destinationRegister << 3 | sourceRegister);
                        return;
                    }
                    // Register File, Register File
                    WriteByte(0b01000000 | op);
                    WriteByte(sourceRegisterToken, sourceRegisterFile);
                    WriteByte(destinationRegisterToken, destinationRegisterFile);
                    return;
                }
            }
            {
                var addressToken = LastToken;
                var address = Expression();
                if (address != null) {
                    if (LastToken.IsReservedWord('(')) {
                        var registerToken = NextToken();
                        {
                            var sourceRegister = ParseRegister();
                            if (sourceRegister != null) {
                                if (sourceRegister.Value == 0) {
                                    ShowInvalidRegister(registerToken, sourceRegister.Value);
                                }
                                AcceptReservedWord(')');
                                // Register, Register Index
                                WriteByte(0b00100000 | op);
                                WriteByte(0b10000000 | destinationRegister << 3 | sourceRegister.Value);
                                WriteByte(addressToken, address);
                                return;
                            }
                        }
                        {
                            var sourceRegister = ParseRegisterPair();
                            if (sourceRegister != null) {
                                if (sourceRegister.Value == 0) {
                                    ShowInvalidRegister(registerToken, sourceRegister.Value);
                                }
                                AcceptReservedWord(')');
                                // Register, Register Pair Index
                                WriteByte(0b00110000 | op);
                                WriteByte(0b10000000 | destinationRegister << 3 | sourceRegister.Value);
                                WriteWord(addressToken, address);
                                return;
                            }
                        }
                    }
                    {
                        // Register File, Immediate
                        WriteByte(0b01010000 | op);
                        WriteByte(addressToken, address);
                        WriteByte(destinationRegisterToken, destinationRegisterFile);
                        return;
                    }
                }
            }
        }
        else {
            {
                var sourceRegisterToken = LastToken;
                var sourceRegisterFile = ParseRegisterFile();
                if (sourceRegisterFile != null) {
                    // Register File, Register File
                    WriteByte(0b01000000 | op);
                    WriteByte(sourceRegisterToken, sourceRegisterFile);
                    WriteByte(destinationRegisterToken, destinationRegisterFile);
                    return;
                }
            }
            {
                var valueToken = LastToken;
                var value = Expression();
                if (value != null) {
                    // Register File, Immediate
                    WriteByte(0b01010000 | op);
                    WriteByte(valueToken, value);
                    WriteByte(destinationRegisterToken, destinationRegisterFile);
                    return;
                }
            }
        }
        ShowSyntaxError(LastToken);
    }
    private void OperateWord(int op)
    {
        var destinationRegisterToken = LastToken;
        var destinationRegisterFile = ParseRegisterPairFileNotNull();
        AcceptReservedWord(',');
        {
            var sourceRegisterToken = LastToken;
            var sourceRegisterFile = ParseRegisterPairFile();
            if (sourceRegisterFile != null) {
                // Register Pair File, Register Pair File
                WriteByte(0b01100000 | op);
                WriteByte(sourceRegisterToken, sourceRegisterFile);
                WriteByte(destinationRegisterToken, destinationRegisterFile);
                return;
            }
            {
                var valueToken = LastToken;
                var value = Expression();
                if (value != null) {
                    // Register Pair File, Immediate Long
                    WriteByte(0b01101000 | op);
                    WriteByte(destinationRegisterToken, destinationRegisterFile);
                    WriteWord(valueToken, value);
                    return;
                }
            }
        }
        ShowSyntaxError(LastToken);
    }

    private void OperateBit(int op)
    {
        AcceptReservedWord(Keyword.BF);
        AcceptReservedWord(',');
        var registerToken = LastToken;
        var registerFile = ParseRegisterFileNotNull();
        AcceptReservedWord(',');
        var bit = ParseBitIndexNotNull();
        WriteByte(0b01001111);
        WriteByte(op | bit);
        WriteByte(registerToken, registerFile);
    }

    private void Compare()
    {
        if (LastToken.IsReservedWord('@')) {
            NextToken();
            var destinationRegister = ParseRegisterNotNull();
            AcceptReservedWord(',');
            var valueToken = LastToken;
            var value = Expression();
            if (value != null) {
                // Register Indirect, Immediate
                WriteByte(0b01011010);
                WriteByte(destinationRegister << 3);
                WriteByte(valueToken, value);
                return;
            }
        }
        else if (LastToken.IsReservedWord('(')) {
            NextToken();
            var destinationRegister = ParseRegisterNotNull();
            AcceptReservedWord(')');
            AcceptReservedWord('+');
            AcceptReservedWord(',');
            var valueToken = LastToken;
            var value = Expression();
            if (value != null) {
                // Register Indirect Auto Increment , Immediate
                WriteByte(0b01011010);
                WriteByte(0b01000000 | destinationRegister << 3);
                WriteByte(valueToken, value);
                return;
            }
        }
        else if (LastToken.IsReservedWord('-')) {
            var sign = LastToken;
            NextToken();
            if (LastToken.IsReservedWord('(')) {
                NextToken();
                var destinationRegister = ParseRegisterNotNull();
                AcceptReservedWord(')');
                AcceptReservedWord(',');
                var valueToken = LastToken;
                var value = Expression();
                if (value != null) {
                    // Register Indirect Auto Decrement , Immediate
                    WriteByte(0b01011010);
                    WriteByte(0b11000000 | destinationRegister << 3);
                    WriteByte(valueToken, value);
                    return;
                }
            }
            else {
                ReturnToken(sign);
            }
        }
        {
            var addressToken = LastToken;
            var address = Expression();
            if (address != null) {
                AcceptReservedWord('(');
                var destinationRegisterToken = LastToken;
                var destinationRegister = ParseRegisterNotNull();
                if (destinationRegister == 0) {
                    ShowInvalidRegister(destinationRegisterToken, destinationRegister);
                }
                AcceptReservedWord(')');
                AcceptReservedWord(',');
                var valueToken = LastToken;
                var value = Expression();
                if (value != null) {
                    // Register Index, Immediate
                    WriteByte(0b01011010);
                    WriteByte(0b10000000 | destinationRegister << 3);
                    WriteByte(addressToken, address);
                    WriteByte(valueToken, value);
                    return;
                }
            }
        }
        OperateByte(0);
    }

    private void OperateWordByte(int op)
    {
        var destinationRegisterToken = LastToken;
        var destinationRegisterFile = ParseRegisterPairFileNotNull();
        AcceptReservedWord(',');
        {
            var sourceRegisterToken = LastToken;
            var sourceRegisterFile = ParseRegisterPairFile();
            if (sourceRegisterFile != null) {
                // Register Pair File, Register Pair File
                WriteByte(op);
                WriteByte(sourceRegisterToken, sourceRegisterFile);
                WriteByte(destinationRegisterToken, destinationRegisterFile);
                return;
            }
        }
        {
            var valueToken = LastToken;
            var value = Expression();
            if (value != null) {
                // Register Pair File, Immediate
                WriteByte(op | 1);
                WriteByte(valueToken, value);
                WriteByte(destinationRegisterToken, destinationRegisterFile);
                return;
            }
        }
        ShowSyntaxError(LastToken);
    }

    private void BitTest()
    {
        var destinationRegisterToken = LastToken;
        var destinationRegisterFile = ParseRegisterFileNotNull();
        AcceptReservedWord(',');
        {
            var valueToken = LastToken;
            var value = Expression();
            if (value != null) {
                // Register Pair File, Immediate
                WriteByte(0b00101111);
                WriteByte(destinationRegisterToken, destinationRegisterFile);
                WriteByte(valueToken, value);
                return;
            }
        }
        ShowSyntaxError(LastToken);
    }

    private void Call()
    {
        if (LastToken.IsReservedWord('@')) {
            NextToken();
            {
                var register = ParseRegisterPair();
                if (register != null) {
                    // Register Pair Indirect
                    WriteByte(0b00111111);
                    WriteByte(register.Value);
                    return;
                }
            }
            {
                var addressToken = LastToken;
                var address = Expression();
                if (address != null) {
                    // Direct Indirect
                    WriteByte(0b00111111);
                    WriteByte(0b01000000);
                    WriteWord(addressToken, address);
                    return;
                }
            }
        }
        {
            var addressToken = LastToken;
            var address = Expression();
            if (address != null) {
                if (LastToken.IsReservedWord('(')) {
                    var registerToken = NextToken();
                    var register = ParseRegisterPairNotNull();
                    if (register == 0) {
                        ShowInvalidRegister(registerToken, register);
                    }
                    AcceptReservedWord(')');
                    // Index Indirect
                    WriteByte(0b00111111);
                    WriteByte(0b01000000 | register << 3);
                    WriteWord(addressToken, address);
                    return;
                }
                // Direct
                WriteByte(0b01001001);
                WriteWord(addressToken, address);
                return;
            }
        }
        ShowSyntaxError(LastToken);
    }
    private void CallShort()
    {
        var token = LastToken;
        var address = ParseConstantExpressionNotNull();
        if (address > 0x1000) {
            ShowOutOfRange(token, address);
            address &= 0xfff;
        }
        WriteByte(0b11100000 | address >> 8);
        WriteByte(address);
    }

    private void Jump()
    {
        if (LastToken.IsReservedWord('@')) {
            NextToken();
            {
                var register = ParseRegisterPair();
                if (register != null) {
                    // Register Pair Indirect
                    WriteByte(0b00111110);
                    WriteByte(register.Value);
                    return;
                }
            }
            {
                var addressToken = LastToken;
                var address = Expression();
                if (address != null) {
                    // Direct Indirect
                    WriteByte(0b00111110);
                    WriteByte(0b01000000);
                    WriteWord(addressToken, address);
                    return;
                }
            }
        }

        {
            var condition = ParseCondition();
            if (condition != null) {
                AcceptReservedWord(',');
                var addressToken = LastToken;
                var address = Expression();
                if (address != null) {
                    // Condition, Direct
                    Jump(condition.Value, addressToken, address);
                    return;
                }
            }
        }
        {
            var addressToken = LastToken;
            var address = Expression();
            if (address != null) {
                AcceptReservedWord('(');
                var registerToken = LastToken;
                var register = ParseRegisterPairNotNull();
                if (register == 0) {
                    ShowInvalidRegister(registerToken, register);
                }
                AcceptReservedWord(')');
                // Index Indirect
                WriteByte(0b00111110);
                WriteByte(0b01000000 | register << 3);
                WriteWord(addressToken, address);
                return;
            }
        }
        ShowSyntaxError(LastToken);
    }

    private void Jump(int condition, Token addressToken, Address address)
    {
        WriteByte(0b10010000 | condition);
        WriteWord(addressToken, address);
    }

    private void Branch()
    {
        var condition = ParseCondition();
        if (condition != null) {
            AcceptReservedWord(',');
            var addressToken = LastToken;
            var address = Expression();
            if (address != null) {
                if (RelativeOffset(addressToken, address, 2, out var offset)) {
                    // Condition, Relative
                    WriteByte(0b11010000 | condition.Value);
                    WriteByte(offset);
                    return;
                }
                Jump(condition.Value, addressToken, address);
                return;
            }
        }
        ShowSyntaxError(LastToken);
    }
    private void DecrementBranch()
    {
        var register = ParseRegisterNotNull();
        AcceptReservedWord(',');
        var addressToken = LastToken;
        var address = Expression();
        if (address != null) {
            if (RelativeOffset(addressToken, address, 2, out var offset)) {
                // Condition, Relative
                WriteByte(0b01110000 | register);
                WriteByte(offset);
                return;
            }
            WriteByte(0b00001001); // DEC
            WriteByte(register);
            WriteByte(0b11010000 | Zero); // BR Z,
            WriteByte(3);
            Jump(Always, addressToken, address);
            return;
        }
        ShowSyntaxError(LastToken);
    }

    private void BranchOnBit(int registerOp, int memoryOp)
    {
        {
            var register = ParseRegisterFile();
            if (register != null) {
                AcceptReservedWord(',');
                var bit = ParseBitIndexNotNull();
                AcceptReservedWord(',');
                var addressToken = LastToken;
                var address = Expression();
                if (address != null) {
                    if (RelativeOffset(addressToken, address, 3, out var offset)) {
                        // Register File, Bit, Relative
                        WriteByte(registerOp | bit);
                        WriteByte(register.Value);
                        WriteByte(offset);
                        return;
                    }
                    WriteByte((registerOp | bit) ^ 0b1000);
                    WriteByte(register.Value);
                    WriteByte(3);
                    Jump(Always, addressToken, address);
                    return;
                }
            }
        }
        {
            var sourceAddressToken = LastToken;
            var sourceAddress = Expression();
            if (sourceAddress != null) {
                if (LastToken.IsReservedWord('(')) {
                    var registerToken = NextToken();
                    var register = ParseRegisterNotNull();
                    if (register == 0) {
                        ShowInvalidRegister(registerToken, register);
                    }
                    AcceptReservedWord(')');
                    AcceptReservedWord(',');
                    var bit = ParseBitIndexNotNull();
                    AcceptReservedWord(',');
                    var destinationAddressToken = LastToken;
                    var destinationAddress = Expression();
                    if (destinationAddress != null) {
                        if (RelativeOffset(destinationAddressToken, destinationAddress, 4, out var offset)) {
                            // Register Index, Bit, Relative [note]
                            WriteByte(memoryOp);
                            WriteByte(register << 3 | bit);
                            WriteByte(sourceAddressToken, sourceAddress);
                            WriteByte(offset);
                            return;
                        }
                        WriteByte(memoryOp ^ 0b1);
                        WriteByte(register << 3 | bit);
                        WriteByte(sourceAddressToken, sourceAddress);
                        WriteByte(3);
                        Jump(Always, destinationAddressToken, destinationAddress);
                        return;
                    }
                }
                else {
                    AcceptReservedWord(',');
                    var bit = ParseBitIndexNotNull();
                    AcceptReservedWord(',');
                    var destinationAddressToken = LastToken;
                    var destinationAddress = Expression();
                    if (destinationAddress != null) {
                        if (RelativeOffset(destinationAddressToken, destinationAddress, 4, out var offset)) {
                            // Direct Special Page, Bit, Relative
                            WriteByte(memoryOp);
                            WriteByte(bit);
                            WriteByte(sourceAddressToken, sourceAddress);
                            WriteByte(offset);
                            return;
                        }
                        WriteByte(memoryOp ^ 0b1);
                        WriteByte(bit);
                        WriteByte(sourceAddressToken, sourceAddress);
                        WriteByte(3);
                        Jump(Always, destinationAddressToken, destinationAddress);
                        return;
                    }
                }
            }
        }
        ShowSyntaxError(LastToken);
    }

    private int? ParseBitCondition()
    {
        if (LastToken.IsReservedWord(Keyword.BC)) {
            NextToken();
            return 0;
        }
        if (LastToken.IsReservedWord(Keyword.BS)) {
            NextToken();
            return 1;
        }
        return null;
    }

    private void ConditionalBranch(Address address, bool inverted)
    {
        {
            var condition = ParseBitCondition();
            if (condition != null) {
                AcceptReservedWord(',');
                var conditionBit = condition.Value;
                if (inverted) {
                    conditionBit ^= 1;
                }
                {
                    var register = ParseRegisterFile();
                    if (register != null) {
                        AcceptReservedWord(',');
                        var bit = ParseBitIndexNotNull();
                        if (!address.IsUndefined()) {
                            var offset = RelativeOffset(address, 3);
                            if (IsRelativeOffsetInRange(offset)) {
                                // Register File, Bit, Relative
                                WriteByte(0b10000000 | bit | conditionBit << 3);
                                WriteByte(register.Value);
                                WriteByte(offset);
                                return;
                            }
                        }
                        WriteByte(0b10000000 | bit | (conditionBit ^ 1) << 3);
                        WriteByte(register.Value);
                        WriteByte(3);
                        Jump(Always, LastToken, address);
                        return;
                    }
                }
                {
                    var sourceToken = LastToken;
                    var source = Expression();
                    if (source != null) {
                        if (LastToken.IsReservedWord('(')) {
                            var registerToken = NextToken();
                            var register = ParseRegisterNotNull();
                            if (register == 0) {
                                ShowInvalidRegister(registerToken, register);
                            }
                            AcceptReservedWord(')');
                            AcceptReservedWord(',');
                            var bit = ParseBitIndexNotNull();
                            if (!address.IsUndefined()) {
                                var offset = RelativeOffset(address, 4);
                                if (IsRelativeOffsetInRange(offset)) {
                                    // Register Index, Bit, Relative
                                    WriteByte(0b00101010 | conditionBit);
                                    WriteByte(register << 3 | bit);
                                    WriteByte(sourceToken, source);
                                    WriteByte(offset);
                                    return;
                                }
                            }
                            WriteByte(0b00101010 | conditionBit ^ 1);
                            WriteByte(register << 3 | bit);
                            WriteByte(sourceToken, source);
                            WriteByte(3);
                            Jump(Always, LastToken, address);
                            return;
                        }
                        if (LastToken.IsReservedWord(',')) {
                            NextToken();
                            var bit = ParseBitIndexNotNull();
                            if (!address.IsUndefined()) {
                                var offset = RelativeOffset(address, 4);
                                if (IsRelativeOffsetInRange(offset)) {
                                    // Direct Special Page, Bit, Relative
                                    WriteByte(0b00101010 | conditionBit);
                                    WriteByte(bit);
                                    WriteByte(sourceToken, source);
                                    WriteByte(offset);
                                    return;
                                }
                            }
                            WriteByte(0b00101010 | conditionBit ^ 1);
                            WriteByte(bit);
                            WriteByte(sourceToken, source);
                            WriteByte(3);
                            Jump(Always, LastToken, address);
                            return;
                        }
                    }
                }
            }
        }
        {
            var condition = ParseCondition();
            if (condition != null) {
                var conditionBits = condition.Value;
                if (inverted) {
                    conditionBits = InvertCondition(conditionBits);
                }
                if (!address.IsUndefined()) {
                    var offset = RelativeOffset(address, 2);
                    if (IsRelativeOffsetInRange(offset)) {
                        // Condition, Relative
                        WriteByte(0b11010000 | conditionBits);
                        WriteByte(offset);
                        return;
                    }
                }
                //WriteByte(0b11010000 | InvertCondition(conditionBits));
                //WriteByte(3);
                //Jump(Always, LastToken, address);
                Jump(conditionBits, LastToken, address);
                return;
            }
        }
        ShowSyntaxError(LastToken);
    }

    private void UnconditionalBranch(Address address)
    {
        if (!address.IsUndefined()) {
            var offset = RelativeOffset(address, 2);
            if (IsRelativeOffsetInRange(offset)) {
                WriteByte(0b11010000 | Always);
                WriteByte(offset);
                return;
            }
            Jump(Always, LastToken, address);
        }
    }

    private void StartIf(IfBlock block)
    {
        var address = SymbolAddress(block.ElseId);
        ConditionalBranch(address, true);
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
            UnconditionalBranch(address);
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
            ConditionalBranch(address, false);
            block.EraseEndId();
        }
        else {
            var address = SymbolAddress(block.EndId);
            ConditionalBranch(address, true);
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
                UnconditionalBranch(address);
                DefineSymbol(block.EndId, CurrentAddress);
            }
            EndBlock();
        }
    }

    private void DwNzStatement()
    {
        if (LastBlock() is not WhileBlock block) {
            ShowNoStatementError(LastToken, "WHILE");
        }
        else {
            var register = ParseRegisterNotNull();
            if (block.EndId <= 0) {
                ShowError(LastToken.Position, "WHILE and WNZ cannot be used in the same syntax.");
            }
            var address = SymbolAddress(block.BeginId);
            EndBlock();
            if (!address.IsUndefined()) {
                var offset = RelativeOffset(address, 2);
                if (IsRelativeOffsetInRange(offset)) {
                    // Register, Relative
                    WriteByte(0b01110000 | register);
                    WriteByte(offset);
                    return;
                }
            }
            WriteByte(0b00001001); // DEC
            WriteByte(register);
            WriteByte(0b11010000 | Zero); // BR Z,
            WriteByte(3);
            Jump(Always, LastToken, address);
            return;
        }
        ShowSyntaxError(LastToken);
    }


    private static readonly Dictionary<int, Action<Assembler>> Actions = new()
    {
        {Keyword.BCLR, a=>a.BitSetReset(0b00011100,0b10100000)},
        {Keyword.BMOV,a=>a.BitMove()},
        {Keyword.BSET, a=>a.BitSetReset(0b00011101,0b10101000)},
        {Keyword.CLR,a=>a.OperateRegisterOrIndirect(0b00000000,0b00011010, 0b000)},
        {Keyword.CLRC,a=>a.WriteByte(0b11111010)},
        {Keyword.MOV,a=>a.Move()},
        {Keyword.MOVM,a=>a.MoveUnderMask()},
        {Keyword.MOVW,a=>a.MoveWord()},
        {Keyword.POP, a=>a.OperateRegisterOrIndirect(0b00001111,0b00011011 ,0b111)},
        {Keyword.POPW, a=>a.OperateRegisterPairFile(0b00011111 )},
        {Keyword.PUSH, a=>a.OperateRegisterOrIndirect(0b00001110,0b00011011 ,0b110 )},
        {Keyword.PUSHW, a=>a.OperateRegisterPairFile(0b00011110)},
        {Keyword.SETC,a=>a.WriteByte(0b11111100)},
        {Keyword.SWAP,a=>a.OperateRegisterOrIndirect(0b00001101,0b00011011, 0b101)},
        {Keyword.ADC,a=>a.OperateByte(0b011)},
        {Keyword.ADCW,a=>a.OperateWord(0b011)},
        {Keyword.ADD,a=>a.OperateByte(0b001)},
        {Keyword.ADDW,a=>a.OperateWord(0b001)},
        {Keyword.BCMP,a=>a.OperateBit(0b00000000)},
        {Keyword.CMP,a=>a.Compare()},
        {Keyword.CMPW,a=>a.OperateWord(0b000)},
        {Keyword.DA,a=>a.OperateRegisterOrIndirect(0b00001100,0b00011011,0b100)},
        {Keyword.DEC,a=>a.OperateRegisterOrIndirect(0b00001001,0b00011011,0b001)},
        {Keyword.DECW, a=>a.OperateRegisterPairFile(0b00011001)},
        {Keyword.DIV,a=>a.OperateWordByte(0b01011100)},
        {Keyword.EXTS, a=>a.OperateRegisterPairFile(0b00101100)},
        {Keyword.INC,a=>a.OperateRegisterOrIndirect(0b00001000,0b00011011,0b000)},
        {Keyword.INCW, a=>a.OperateRegisterPairFile(0b00011000)},
        {Keyword.MULT,a=>a.OperateWordByte(0b01001100)},
        {Keyword.NEG,a=>a.OperateRegisterOrIndirect(0b00000001,0b00011010,0b001)},
        {Keyword.SBC,a=>a.OperateByte(0b100)},
        {Keyword.SBCW,a=>a.OperateWord(0b100)},
        {Keyword.SUB,a=>a.OperateByte(0b010)},
        {Keyword.SUBW,a=>a.OperateWord(0b010)},
        {Inu.Assembler.Keyword.And,a=>a.OperateByte(0b101)},
        {Keyword.ANDW,a=>a.OperateWord(0b101)},
        {Keyword.BAND,a=>a.OperateBit(0b01000000)},
        {Keyword.BOR,a=>a.OperateBit(0b10000000)},
        {Keyword.BTST,a=>a.BitTest()},
        {Keyword.BXOR,a=>a.OperateBit(0b11000000)},
        {Keyword.COM,a=>a.OperateRegisterOrIndirect(0b00000010,0b00011010,0b010)},
        {Keyword.COMC,a=>a.WriteByte(0b11111011)},
        {Inu.Assembler.Keyword.Or,a=>a.OperateByte(0b110)},
        {Keyword.ORW,a=>a.OperateWord(0b110)},
        {Keyword.RL,a=>a.OperateRegisterOrIndirect(0b00000100,0b00011010,0b100)},
        {Keyword.RLC,a=>a.OperateRegisterOrIndirect(0b00000110,0b00011010,0b110)},
        {Keyword.RR,a=>a.OperateRegisterOrIndirect(0b00000011,0b00011010,0b011)},
        {Keyword.RRC,a=>a.OperateRegisterOrIndirect(0b00000101,0b00011010,0b101)},
        {Keyword.SLL,a=>a.OperateRegisterOrIndirect(0b00001011,0b00011011,0b011)},
        {Keyword.SRA,a=>a.OperateRegisterOrIndirect(0b00001010,0b00011011,0b010)},
        {Keyword.SRL,a=>a.OperateRegisterOrIndirect(0b00000111,0b00011010,0b111)},
        {Inu.Assembler.Keyword.Xor,a=>a.OperateByte(0b111)},
        {Keyword.XORW,a=>a.OperateWord(0b111)},
        {Keyword.DI,a=>a.WriteByte(0b11111110)},
        {Keyword.EI,a=>a.WriteByte(0b11111101)},
        {Keyword.HALT,a=>a.WriteByte(0b11110001)},
        {Keyword.IRET,a=>a.WriteByte(0b11111001)},
        {Keyword.NOP,a=>a.WriteByte(0b11111111)},
        {Keyword.RET,a=>a.WriteByte(0b11111000)},
        {Keyword.STOP,a=>a.WriteByte(0b11110000)},
        {Keyword.CALL,a=>a.Call()},
        {Keyword.CALS,a=>a.CallShort()},
        {Keyword.JMP, a=>a.Jump()},
        {Keyword.BR, a=>a.Branch()},
        {Keyword.DBNZ,a=>a.DecrementBranch()},
        {Keyword.BBC,a=>a.BranchOnBit(0b10000000,0b00101010)},
        {Keyword.BBS,a=>a.BranchOnBit(0b10001000,0b00101011)},
        {Inu.Assembler.Keyword.If, a=>a.IfStatement()},
        {Inu.Assembler.Keyword.EndIf, a=>a.EndIfStatement()},
        {Inu.Assembler.Keyword.Else, a=>a.ElseStatement()},
        {Inu.Assembler.Keyword.ElseIf, a=>a.ElseIfStatement()},
        {Inu.Assembler.Keyword.Do, a=>a.DoStatement()},
        {Inu.Assembler.Keyword.While, a=>a.WhileStatement()},
        {Inu.Assembler.Keyword.WEnd, a=>a.WEndStatement()},
        {Keyword.DWNZ, (Assembler a)=>{a.DwNzStatement(); }},
    };

    protected override bool Instruction()
    {
        if (LastToken is not ReservedWord reservedWord) {
            return false;
        }
        if (Actions.TryGetValue(reservedWord.Id, out var action)) {
            NextToken();
            action(this);
            return true;
        }
        return false;
    }
}