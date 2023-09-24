using Inu.Language;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Inu.Assembler.HD61700;

internal class Assembler : Inu.Assembler.LittleEndianAssembler
{
    public const int MainRegisterCount = 32;



    public Assembler() : base(new Tokenizer()) { }


    private int? ParseMainRegister()
    {
        if (LastToken is NumericValue numericValue) {
            var index = numericValue.Value;
            if (index is >= 0 and < MainRegisterCount) {
                NextToken();
                return index;
            }
            ShowOutOfRange(LastToken, index);
        }
        return null;
    }

    private int? ParseMainRegisterWithHead()
    {
        if (LastToken.IsReservedWord('$')) {
            NextToken();
            return ParseMainRegister();
        }
        return null;
    }

    private static readonly Dictionary<int, int> IndexRegisterMap = new()
    {
        {Keyword.IX, 0x00},
        {Keyword.IZ, 0x01},
    };

    private int? ParseIndexRegister()
    {
        foreach (var i in IndexRegisterMap.Where(i => LastToken.IsReservedWord(i.Key))) {
            NextToken();
            return i.Value;
        }
        return null;
    }


    private int? ParseSign()
    {
        if (LastToken.IsReservedWord('+')) {
            NextToken();
            return 0x00;
        }
        if (LastToken.IsReservedWord('-')) {
            NextToken();
            return 0x80;
        }
        return null;
    }

    private static readonly Dictionary<int, int> SpecificRegisterMap = new()
    {
        {Keyword.SX, 0x00},
        {Keyword.SY, 0x20},
        {Keyword.SZ, 0x40},
    };

    private int? ParseSpecificRegister()
    {
        foreach (var i in SpecificRegisterMap.Where(i => LastToken.IsReservedWord(i.Key))) {
            NextToken();
            return i.Value;
        }
        return null;
    }

    private int? ParseSpecificRegisterWithHead()
    {
        if (LastToken.IsReservedWord('$')) {
            NextToken();
            return ParseSpecificRegister();
        }
        return null;
    }


    private static readonly Dictionary<int, int> StatusRegisterMap = new()
    {
        {Keyword.PE, 0x00},
        {Keyword.PD, 0x02},
        {Keyword.IB, 0x04},
        {Keyword.UA, 0x06},
        {Keyword.IA, 0x10},
        {Keyword.IE, 0x12},
        {Keyword.TM, 0x16},
    };

    private int? ParseStatusRegister()
    {
        foreach (var i in StatusRegisterMap.Where(i => LastToken.IsReservedWord(i.Key))) {
            NextToken();
            return i.Value;
        }
        return null;
    }

    private static readonly Dictionary<int, int> WordRegisterMap = new()
    {
        {Keyword.IX, 0x00},
        {Keyword.IY, 0x02},
        {Keyword.IZ, 0x04},
        {Keyword.US, 0x06},
        {Keyword.SS, 0x10},
        {Keyword.KY, 0x12},
    };

    private int? ParseWordRegister()
    {
        foreach (var i in WordRegisterMap.Where(i => LastToken.IsReservedWord(i.Key))) {
            NextToken();
            return i.Value;
        }
        return null;
    }

    private static readonly Dictionary<int, int> ConditionMap = new()
    {
        {Keyword.Z, 0x00},
        {Keyword.NC, 0x01},
        {Keyword.LZ, 0x02},
        {Keyword.UZ, 0x03},
        {Keyword.NZ, 0x04},
        {Keyword.C, 0x05},
        {Keyword.LNZ, 0x06},
        {Keyword.NLZ, 0x06},
    };

    private int? ParseCondition()
    {
        foreach (var i in ConditionMap.Where(i => LastToken.IsReservedWord(i.Key))) {
            NextToken();
            return i.Value;
        }
        return null;
    }


    private int? ParseJumpOption()
    {
        if (LastToken.IsReservedWord(',')) {
            NextToken();
            if (RelativeOffset(out _, out var relativeOffset)) {
                if (relativeOffset >= 0) {
                    relativeOffset &= 0x7f;
                }
                else {
                    relativeOffset = (-relativeOffset & 0x7f) | 0x80;
                }
                return relativeOffset;
            }
        }
        return null;
    }

    private bool RightRegister(int code, int leftRegister, Action? endOfSequence = null, bool multi = false)
    {
        if (LastToken.IsReservedWord('$')) {
            NextToken();
            var rightRegister = ParseMainRegister();
            if (rightRegister != null) {
                var rightOperand = rightRegister.Value;
                WriteByte(code);
                endOfSequence?.Invoke();
                if (multi) {
                    rightOperand |= ParseCount();
                }
                var relativeOffset = ParseJumpOption();
                var leftOperand = leftRegister | 0x60;
                if (relativeOffset != null)
                    leftOperand |= 0x80;
                WriteByte(leftOperand);
                WriteByte(rightOperand);
                if (relativeOffset != null) {
                    WriteByte(relativeOffset.Value);
                }
                return true;
            }
            var specificRegister = ParseSpecificRegister();
            if (specificRegister != null) {
                var leftOperand = leftRegister | specificRegister.Value;
                endOfSequence?.Invoke();
                var count = 0;
                int? relativeOffset;
                if (!multi) {
                    relativeOffset = ParseJumpOption();
                    WriteByte(code);
                }
                else {
                    WriteByte(code);
                    count = ParseCount();
                    relativeOffset = ParseJumpOption();
                }
                if (relativeOffset != null)
                    leftOperand |= 0x80;
                WriteByte(leftOperand);
                if (multi) {
                    WriteByte(count);
                }
                if (relativeOffset != null) {
                    WriteByte(relativeOffset.Value);
                }
                return true;
            }
        }
        return false;
    }

    private int ParseCount()
    {
        AcceptReservedWord(',');
        var token = LastToken;
        var count = Expression();
        if (count != null) {
            if (count.IsConst()) {
                return ((count.Value - 1) & 0x07) << 5;
            }
            ShowAddressUsageError(token);
        }
        return 0;
    }


    private bool SingleRegister(int opCode, int operandBit, bool multi = false)
    {
        var register = ParseMainRegisterWithHead();
        if (register != null) {
            var count = 0;
            int? relativeOffset = null;
            if (multi) {
                count = ParseCount();
            }
            else {
                relativeOffset = ParseJumpOption();
            }
            WriteByte(opCode);
            var operand = register.Value | operandBit;
            if (relativeOffset != null) operand |= 0x80;
            WriteByte(operand);
            if (multi) {
                WriteByte(count);
            }
            if (relativeOffset != null) {
                WriteByte(relativeOffset.Value);
            }
            return true;
        }
        return false;
    }

    private bool Indirect(int code, int leftRegister)
    {
        return RightRegister(code, leftRegister, () => AcceptReservedWord(')'));
    }

    private bool RightRegisterOffset(int code, int leftRegister, int sign, bool multi = false)
    {
        if (LastToken.IsReservedWord('$')) {
            NextToken();
            {
                var rightRegister = ParseMainRegister();
                if (rightRegister != null) {
                    var rightOperand = rightRegister.Value;
                    AcceptReservedWord(')');
                    if (multi) {
                        rightOperand |= ParseCount();
                    }
                    WriteByte(code);
                    var leftOperand = leftRegister | 0x60 | sign;
                    WriteByte(leftOperand);
                    WriteByte(rightOperand);
                    return true;
                }
            }
            {
                var specificRegister = ParseSpecificRegister();
                if (specificRegister != null) {
                    AcceptReservedWord(')');
                    WriteByte(code);
                    var leftOperand = leftRegister | specificRegister.Value | sign;
                    WriteByte(leftOperand);
                    if (multi) {
                        WriteByte(ParseCount());
                    }
                    return true;
                }
            }
        }
        return false;
    }

    private bool LeftOffset(int code, int sign)
    {
        if (LastToken.IsReservedWord('$')) {
            NextToken();
            {
                var offsetRegister = ParseMainRegister();
                if (offsetRegister != null) {
                    AcceptReservedWord(')');
                    AcceptReservedWord(',');
                    {
                        var rightRegister = ParseMainRegisterWithHead();
                        if (rightRegister != null) {
                            WriteByte(code);
                            WriteByte(sign | 0x60 | rightRegister.Value);
                            WriteByte(offsetRegister.Value);
                            return true;
                        }
                    }
                    ShowSyntaxError(LastToken);
                    return true;
                }
            }
            {
                var offsetRegister = ParseSpecificRegister();
                if (offsetRegister != null) {
                    AcceptReservedWord(')');
                    AcceptReservedWord(',');
                    {
                        var rightRegister = ParseMainRegisterWithHead();
                        if (rightRegister != null) {
                            WriteByte(code);
                            WriteByte(sign | offsetRegister.Value | rightRegister.Value);
                            return true;
                        }
                    }
                    ShowSyntaxError(LastToken);
                    return true;
                }
            }
        }
        return false;
    }

    private bool RightRegisterOffset(int code, int leftRegister, bool multi = false)
    {
        var indexRegister = ParseIndexRegister();
        if (indexRegister != null) {
            code |= indexRegister.Value;
            var sign = ParseSign();
            if (sign != null) {
                if (RightRegisterOffset(code, leftRegister, sign.Value, multi)) return true;
            }
        }
        return false;
    }


    private bool ImmediateOffset(int code, int leftRegister, int sign)
    {
        var immediateValue = Expression();
        if (immediateValue != null) {
            AcceptReservedWord(')');
            WriteByte(code);
            var leftOperand = leftRegister | sign;
            WriteByte(leftOperand);
            WriteByte(immediateValue.Value);
            return true;
        }
        return false;
    }

    private bool RightOffset(int code, int leftRegister)
    {
        var indexRegister = ParseIndexRegister();
        if (indexRegister != null) {
            code |= indexRegister.Value;
            var sign = ParseSign();
            if (sign != null) {
                if (RightRegisterOffset(code, leftRegister, sign.Value)) return true;
                if (ImmediateOffset(code | 0x40, leftRegister, sign.Value)) return true;
            }
        }
        return false;
    }

    private bool RightOffsetMulti(int code, int leftRegister)
    {
        var indexRegister = ParseIndexRegister();
        if (indexRegister != null) {
            code |= indexRegister.Value;
            var sign = ParseSign();
            if (sign != null) {
                if (RightRegisterOffset(code, leftRegister, sign.Value, true)) return true;
            }
        }
        return false;
    }



    private bool ImmediateByte(int code, int leftRegister, bool multi = false)
    {
        var valueToken = LastToken;
        var immediateValue = Expression();
        if (immediateValue != null) {
            WriteByte(code);
            var count = 0;
            if (multi) {
                count = ParseCount();
            }
            var relativeOffset = ParseJumpOption();
            var leftOperand = leftRegister;
            if (relativeOffset != null)
                leftOperand |= 0x80;
            WriteByte(leftOperand);
            if (multi) {
                var value = 0;
                if (immediateValue.IsConst()) {
                    value = immediateValue.Value & 0x1f;
                }
                else {
                    ShowAddressUsageError(valueToken);
                }
                WriteByte(count | value);
            }
            else {
                WriteByte(valueToken, immediateValue);
            }
            if (relativeOffset != null) {
                WriteByte(relativeOffset.Value);
            }
            return true;
        }
        return false;
    }

    private bool ImmediateWord(int code, int leftRegister)
    {
        var valueToken = LastToken;
        var immediateValue = Expression();
        if (immediateValue != null) {
            WriteByte(code);
            var leftOperand = leftRegister;
            WriteByte(leftOperand);
            WriteWord(valueToken, immediateValue);
            return true;
        }
        return false;
    }


    private bool StatusRight(int code, [DisallowNull] int? statusRegister)
    {
        var mainRegister = ParseMainRegisterWithHead();
        if (mainRegister != null) {
            code |= statusRegister.Value >> 4;
            var relativeOffset = ParseJumpOption();
            WriteByte(code);
            var operand = ((statusRegister.Value << 4) & 0xf0) | mainRegister.Value;
            if (relativeOffset != null) operand |= 0x80;
            WriteByte(operand);
            if (relativeOffset != null) {
                WriteByte(relativeOffset.Value);
            }

            return true;
        }

        return false;
    }

    private void OperateMainRegisterOrImmediate(int code)
    {
        {
            var register = ParseMainRegisterWithHead();
            if (register != null) {
                WriteByte(code | 0x80);
                WriteByte(register.Value);
                return;
            }
        }
        {
            var valueToken = LastToken;
            var immediateValue = Expression();
            if (immediateValue != null) {
                WriteByte(code);
                WriteByte(valueToken, immediateValue);
                return;
            }
        }
        ShowSyntaxError(LastToken);
    }

    private void ImmediateByteOnly(int code)
    {
        var valueToken = LastToken;
        var immediateValue = Expression();
        if (immediateValue != null) {
            WriteByte(code);
            WriteByte(valueToken, immediateValue);
            return;
        }
        ShowSyntaxError(LastToken);
    }


    private void LD()
    {
        var leftRegister = ParseMainRegisterWithHead();
        if (leftRegister != null) {
            AcceptReservedWord(',');
            if (RightRegister(0x02, leftRegister.Value)) return;
            if (LastToken.IsReservedWord('(')) {
                NextToken();
                if (Indirect(0x11, leftRegister.Value)) return;
                if (RightOffset(0x28, leftRegister.Value)) return;
            }
            if (ImmediateByte(0x42, leftRegister.Value)) return;
        }
        ShowSyntaxError(LastToken);
    }


    private void IncDecByte(int code)
    {
        var leftRegister = ParseMainRegisterWithHead();
        if (leftRegister == null) return;
        AcceptReservedWord(',');
        if (LastToken.IsReservedWord('(')) {
            NextToken();
            if (RightOffset(code, leftRegister.Value)) return;
        }
        ShowSyntaxError(LastToken);
    }

    private void OperateRegisterOrImmediate(int code, bool multi = false)
    {
        var leftRegister = ParseMainRegisterWithHead();
        if (leftRegister != null) {
            AcceptReservedWord(',');
            if (RightRegister(code, leftRegister.Value, null, multi)) return;
            if (ImmediateByte(code | 0x40, leftRegister.Value, multi)) return;
        }
        ShowSyntaxError(LastToken);
    }

    private void OperateRegister(int code, bool multi = false)
    {
        var leftRegister = ParseMainRegisterWithHead();
        if (leftRegister != null) {
            AcceptReservedWord(',');
            if (RightRegister(code, leftRegister.Value, null, multi)) return;
        }
        ShowSyntaxError(LastToken);
    }


    private void ST()
    {
        {
            var leftRegister = ParseMainRegisterWithHead();
            if (leftRegister != null) {
                AcceptReservedWord(',');
                if (RightRegister(0x02, leftRegister.Value)) return;
                if (LastToken.IsReservedWord('(')) {
                    NextToken();
                    if (Indirect(0x10, leftRegister.Value)) return;
                    if (RightOffset(0x20, leftRegister.Value)) return;
                }
                ShowSyntaxError(LastToken);
                return;
            }
        }
        {
            var leftToken = LastToken;
            var leftValue = Expression();
            if (leftValue != null) {
                AcceptReservedWord(',');
                if (LastToken.IsReservedWord('(')) {
                    NextToken();
                    var rightRegister = ParseSpecificRegisterWithHead();
                    if (rightRegister != null) {
                        AcceptReservedWord(')');
                        WriteByte(0x50);
                        WriteByte(rightRegister.Value);
                        WriteByte(leftToken, leftValue);
                        return;
                    }
                    ShowSyntaxError(LastToken);
                    return;
                }
                {
                    var rightRegister = ParseMainRegisterWithHead();
                    if (rightRegister != null) {
                        WriteByte(0x51);
                        WriteByte(rightRegister.Value);
                        WriteByte(leftToken, leftValue);
                        return;
                    }
                }
            }
        }
        ShowSyntaxError(LastToken);
    }

    private void SingleRegisterWithoutJump(int opCode, int operandBit = 0, bool multi = false)
    {
        var register = ParseMainRegisterWithHead();
        if (register != null) {
            WriteByte(opCode);
            WriteByte(register.Value | operandBit);
            if (multi) {
                WriteByte(ParseCount());
            }
            return;
        }
        ShowSyntaxError(LastToken);
    }

    private void SingleRegisterOnly(int opCode, int operandBit, bool multi = false)
    {
        if (SingleRegister(opCode, operandBit, multi)) return;
        ShowSyntaxError(LastToken);
    }



    private void Put(int operandBit)
    {
        if (SingleRegister(0x14, operandBit)) return;
        {
            var token = LastToken;
            var value = Expression();
            if (value != null) {
                WriteByte(0x54);
                WriteByte(operandBit);
                WriteByte(token, value);
                return;
            }
        }
        ShowSyntaxError(LastToken);
    }

    private void GST()
    {
        var statusRegister = ParseStatusRegister();
        if (statusRegister != null) {
            AcceptReservedWord(',');
            if (StatusRight(0x1e, statusRegister)) return;
        }
        ShowSyntaxError(LastToken);
    }

    private void PST()
    {
        var statusToken = LastToken;
        var statusRegister = ParseStatusRegister();
        if (statusRegister != null) {
            if (statusToken.IsReservedWord(Keyword.TM)) {
                ShowInvalidRegister(statusToken);
                return;
            }
            AcceptReservedWord(',');
            if (StatusRight(0x16, statusRegister)) return;
            {
                var valueToken = LastToken;
                var value = Expression();
                if (value != null) {
                    var code = 0x56 | (statusRegister.Value >> 4);
                    WriteByte(code);
                    var operand = (statusRegister.Value << 4) & 0xf0;
                    WriteByte(operand);
                    WriteByte(valueToken, value);
                    return;
                }
            }
        }
        ShowSyntaxError(LastToken);
    }

    private void STL()
    {
        if (SingleRegister(0x12, 0x00)) return;
        {
            var token = LastToken;
            var value = Expression();
            if (value != null) {
                WriteByte(0x52);
                WriteByte(token, value);
                return;
            }
        }
        ShowSyntaxError(LastToken);
    }

    private void PSR()
    {
        var specificRegister = ParseSpecificRegister();
        if (specificRegister != null) {
            AcceptReservedWord(',');
            var mainRegister = ParseMainRegisterWithHead();
            if (mainRegister != null) {
                var relativeOffset = ParseJumpOption();
                WriteByte(0x1d);
                var operand = specificRegister.Value | mainRegister.Value;
                if (relativeOffset != null) operand |= 0x80;
                WriteByte(operand);
                if (relativeOffset != null) {
                    WriteByte(relativeOffset.Value);
                }
                return;
            }
            {
                var value = Expression();
                if (value != null) {
                    WriteByte(0x55);
                    WriteByte(specificRegister.Value | (value.Value & 0x1f));
                    return;
                }
            }
        }
        ShowSyntaxError(LastToken);
    }

    private void GetPutStatus(int code)
    {
        var specificRegister = ParseSpecificRegister();
        if (specificRegister != null) {
            AcceptReservedWord(',');
            var mainRegister = ParseMainRegisterWithHead();
            if (mainRegister != null) {
                var relativeOffset = ParseJumpOption();
                WriteByte(code);
                var operand = specificRegister.Value | mainRegister.Value;
                if (relativeOffset != null) operand |= 0x80;
                WriteByte(operand);
                if (relativeOffset != null) {
                    WriteByte(relativeOffset.Value);
                }

                return;
            }
        }

        ShowSyntaxError(LastToken);
    }

    private void LDW()
    {
        var leftRegister = ParseMainRegisterWithHead();
        if (leftRegister != null) {
            AcceptReservedWord(',');
            if (RightRegister(0x82, leftRegister.Value)) return;
            if (LastToken.IsReservedWord('(')) {
                NextToken();
                if (Indirect(0x91, leftRegister.Value)) return;
                if (RightOffset(0xa8, leftRegister.Value)) return;
            }
            if (ImmediateWord(0xd1, leftRegister.Value)) return;
        }
        ShowSyntaxError(LastToken);
    }

    private void IncDecWord(int code)
    {
        var leftRegister = ParseMainRegisterWithHead();
        if (leftRegister == null) return;
        AcceptReservedWord(',');
        if (LastToken.IsReservedWord('(')) {
            NextToken();
            if (RightRegisterOffset(code, leftRegister.Value)) return;
        }
        ShowSyntaxError(LastToken);
    }

    private void LDCW()
    {
        var leftRegister = ParseMainRegisterWithHead();
        if (leftRegister != null) {
            AcceptReservedWord(',');
            if (RightRegister(0x83, leftRegister.Value)) return;
        }
        ShowSyntaxError(LastToken);
    }

    private void STW()
    {
        {
            var leftRegister = ParseMainRegisterWithHead();
            if (leftRegister != null) {
                AcceptReservedWord(',');
                if (LastToken.IsReservedWord('(')) {
                    NextToken();
                    if (Indirect(0x90, leftRegister.Value)) return;
                    if (RightRegisterOffset(0xa0, leftRegister.Value)) return;
                }
                ShowSyntaxError(LastToken);
                return;
            }
        }
        {
            var leftToken = LastToken;
            var leftValue = Expression();
            if (leftValue != null) {
                AcceptReservedWord(',');
                if (LastToken.IsReservedWord('(')) {
                    NextToken();
                    var rightRegister = ParseSpecificRegisterWithHead();
                    if (rightRegister != null) {
                        AcceptReservedWord(')');
                        WriteByte(0xd0);
                        WriteByte(rightRegister.Value);
                        WriteWord(leftToken, leftValue);
                        return;
                    }
                    ShowSyntaxError(LastToken);
                    return;
                }
            }
        }
        ShowSyntaxError(LastToken);
    }

    private void GRE()
    {
        var wordRegister = ParseWordRegister();
        if (wordRegister != null) {
            AcceptReservedWord(',');
            if (StatusRight(0x9e, wordRegister)) return;
        }
        ShowSyntaxError(LastToken);
    }

    private void PRE()
    {
        var registerToken = LastToken;
        var wordRegister = ParseWordRegister();
        if (wordRegister != null) {
            if (registerToken.IsReservedWord(Keyword.KY)) {
                ShowInvalidRegister(registerToken);
                return;
            }
            AcceptReservedWord(',');
            if (StatusRight(0x96, wordRegister)) return;
            {
                var valueToken = LastToken;
                var value = Expression();
                if (value != null) {
                    var code = 0xd6 | (wordRegister.Value >> 4);
                    WriteByte(code);
                    var operand = (wordRegister.Value << 4) & 0xf0;
                    WriteByte(operand);
                    WriteWord(valueToken, value);
                    return;
                }
            }
        }
        ShowSyntaxError(LastToken);
    }

    private void AddSubByte(int code1, int code2)
    {
        {
            var leftRegister = ParseMainRegisterWithHead();
            if (leftRegister != null) {
                AcceptReservedWord(',');
                if (RightRegister(code1, leftRegister.Value)) return;
                if (ImmediateByte(code1 | 0x40, leftRegister.Value)) return;
                ShowSyntaxError(LastToken);
                return;
            }
        }
        if (LastToken.IsReservedWord('(')) {
            NextToken();
            var indexRegister = ParseIndexRegister();
            if (indexRegister != null) {
                code2 |= indexRegister.Value;
                var sign = ParseSign();
                if (sign != null) {
                    if (LeftOffset(code2, sign.Value)) return;
                    {
                        var offsetToken = LastToken;
                        var offset = Expression();
                        if (offset != null) {
                            AcceptReservedWord(')');
                            AcceptReservedWord(',');
                            {
                                var rightRegister = ParseMainRegisterWithHead();
                                if (rightRegister != null) {
                                    WriteByte(code2 | 0x40);
                                    WriteByte(sign.Value | rightRegister.Value);
                                    WriteByte(offsetToken, offset);
                                    return;
                                }
                            }
                            ShowSyntaxError(LastToken);
                            return;
                        }
                    }
                }
            }
        }
        ShowSyntaxError(LastToken);
    }



    private void AddSubWord(int code1, int code2)
    {
        {
            var leftRegister = ParseMainRegisterWithHead();
            if (leftRegister != null) {
                AcceptReservedWord(',');
                if (RightRegister(code1, leftRegister.Value)) return;
                ShowSyntaxError(LastToken);
                return;
            }
        }
        if (LastToken.IsReservedWord('(')) {
            NextToken();
            var indexRegister = ParseIndexRegister();
            if (indexRegister != null) {
                code2 |= indexRegister.Value;
                var sign = ParseSign();
                if (sign != null) {
                    if (LeftOffset(code2, sign.Value)) return;
                }
            }
        }
        ShowSyntaxError(LastToken);
    }

    private void JP()
    {
        {
            var register = ParseMainRegisterWithHead();
            if (register != null) {
                WriteByte(0xde);
                WriteByte(register.Value);
                return;
            }
        }
        if (LastToken.IsReservedWord('(')) {
            NextToken();
            var register = ParseMainRegisterWithHead();
            if (register != null) {
                AcceptReservedWord(')');
                WriteByte(0xdf);
                WriteByte(register.Value);
                return;
            }
            ShowSyntaxError(LastToken);
            return;
        }
        {
            var condition = ParseCondition();
            if (condition != null) {
                AcceptReservedWord(',');
                {
                    var addressToken = LastToken;
                    var address = Expression();
                    if (address != null) {
                        WriteByte(0x30 | condition.Value);
                        WriteWord(addressToken, address);
                        return;
                    }
                }
                ShowSyntaxError(LastToken);
                return;
            }
        }
        {
            var addressToken = LastToken;
            var address = Expression();
            if (address != null) {
                WriteByte(0x37);
                WriteWord(addressToken, address);
                return;
            }
        }
        ShowSyntaxError(LastToken);
    }

    private void JR()
    {
        {
            var condition = ParseCondition();
            if (condition != null) {
                AcceptReservedWord(',');
                if (WriteCode(0xb0 | condition.Value)) return;
                ShowSyntaxError(LastToken);
                return;
            }
        }
        if (WriteCode(0xb7)) return;
        ShowSyntaxError(LastToken);
        return;

        bool WriteCode(int code)
        {
            var addressToken = LastToken;
            var address = Expression();
            if (address == null) return false;
            Jump(code, address, addressToken);
            return true;
        }
    }

    private void Jump(int code, Address address, Token addressToken)
    {
        RelativeOffset(addressToken, address, out var offset);
        ++offset;
        if (Math.Abs(offset) < 0x80) {
            if (offset < 0) {
                offset = 0x80 | -offset;
            }
            WriteByte(code);
            WriteByte(offset);
        }
        else {
            WriteByte(code & ~0x80);
            WriteWord(addressToken, address);
        }
    }

    private void CAL()
    {
        {
            var condition = ParseCondition();
            if (condition != null) {
                AcceptReservedWord(',');
                {
                    var addressToken = LastToken;
                    var address = Expression();
                    if (address != null) {
                        WriteByte(0x70 | condition.Value);
                        WriteWord(addressToken, address);
                        return;
                    }
                }
                ShowSyntaxError(LastToken);
                return;
            }
        }
        {
            var addressToken = LastToken;
            var address = Expression();
            if (address != null) {
                WriteByte(0x77);
                WriteWord(addressToken, address);
                return;
            }
        }
        ShowSyntaxError(LastToken);
    }

    private void RTN()
    {
        var condition = ParseCondition();
        if (condition != null) {
            WriteByte(0xf0 | condition.Value);
            return;
        }
        WriteByte(0xf7);
    }

    private void LDM()
    {
        var leftRegister = ParseMainRegisterWithHead();
        if (leftRegister != null) {
            AcceptReservedWord(',');
            if (RightRegister(0xc2, leftRegister.Value, null, true)) return;
            if (LastToken.IsReservedWord('(')) {
                NextToken();
                if (RightRegisterOffset(0xe8, leftRegister.Value, true)) return;
            }
        }
        ShowSyntaxError(LastToken);
    }

    private void IncDecMulti(int code)
    {
        var leftRegister = ParseMainRegisterWithHead();
        if (leftRegister == null) return;
        AcceptReservedWord(',');
        if (LastToken.IsReservedWord('(')) {
            NextToken();
            if (RightOffsetMulti(code, leftRegister.Value)) return;
        }
        ShowSyntaxError(LastToken);
    }

    private void PSRM()
    {
        var specificRegister = ParseSpecificRegister();
        if (specificRegister != null) {
            AcceptReservedWord(',');
            var mainRegister = ParseMainRegisterWithHead();
            if (mainRegister != null) {
                WriteByte(0xd5);
                WriteByte(specificRegister.Value | mainRegister.Value);
                WriteByte(ParseCount());
                return;
            }
        }
        ShowSyntaxError(LastToken);
    }

    private void ConditionalJump(int? condition, Address address)
    {
        if (condition != null) {
            Jump(0xb0 | condition.Value, address, LastToken);
        }
        else {
            Jump(0xb7, address, LastToken);
        }
    }


    private void NegatedConditionalJump(Address address)
    {
        var condition = ParseCondition();
        NegatedConditionalJump(condition, address);
    }

    private void NegatedConditionalJump(int? condition, Address address)
    {
        if (condition != null) {
            switch (condition.Value) {
                case 0x00: // Z
                    NextToken();
                    Jump(0xb4, address, LastToken);
                    return;
                case 0x01: // NC
                    NextToken();
                    Jump(0xb5, address, LastToken);
                    return;
                case 0x02: // LZ
                    NextToken();
                    Jump(0xb6, address, LastToken);
                    return;
                case 0x03: // UZ
                    Skip(0xb3); // UZ
                    return;
                case 0x04: // NZ
                    NextToken();
                    Jump(0xb0, address, LastToken);
                    return;
                case 0x05: // C
                    NextToken();
                    Jump(0xb1, address, LastToken);
                    return;
                case 0x06: // NLZ
                    NextToken();
                    Jump(0xb2, address, LastToken);
                    return;
            }
        }
        ShowError(LastToken.Position, "Missing Condition.");
        return;

        void Skip(int code)
        {
            NextToken();
            WriteByte(code);
            RelativeOffset(LastToken, address, out var offset);
            offset += 2;
            WriteByte(Math.Abs(offset) < 0x80 ? 3 : 4);
            this.Jump(0xb7, address, LastToken);
        }
    }

    private void UnconditionalJump(Address address)
    {
        Jump(0xb7, address, LastToken);
    }


    private void StartIf(IfBlock block)
    {
        var address = SymbolAddress(block.ElseId);
        NegatedConditionalJump(address);
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
        NextToken();
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
        NextToken();
    }

    private void WhileStatement()
    {
        if (LastBlock() is not WhileBlock block) {
            ShowNoStatementError(LastToken, "WHILE");
            NextToken();
            return;
        }

        var repeatAddress = SymbolAddress(block.RepeatId);
        var condition = ParseCondition();
        var next = condition != null ? 0 : 1;
        if (repeatAddress.Type == CurrentSegment.Type && (RelativeOffset(repeatAddress)) <= next) {
            var address = SymbolAddress(block.BeginId);
            ConditionalJump(condition, address);
            block.EraseEndId();
        }
        else {
            var address = SymbolAddress(block.EndId);
            NegatedConditionalJump(condition, address);
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
        NextToken();
    }




    private static readonly Dictionary<int, Action<Assembler>> Actions = new()
    {
        {Keyword.LD, a=>{a.LD();}},
        {Keyword.LDI, a=>{a.IncDecByte(0x2a);}},
        {Keyword.LDD, a=>{a.IncDecByte(0x2c);}},
        {Keyword.LDC, a=>{a.OperateRegisterOrImmediate(0x03);}},
        {Keyword.ST, a=>{a.ST();}},
        {Keyword.STI, a=>{a.IncDecByte(0x22);}},
        {Keyword.STD, a=>{a.IncDecByte(0x24);}},
        {Keyword.PPS, a=>{a.SingleRegisterWithoutJump(0x2e);}},
        {Keyword.PPU, a=>{a.SingleRegisterWithoutJump(0x2f);}},
        {Keyword.PHS, a=>{a.SingleRegisterWithoutJump(0x26);}},
        {Keyword.PHU, a=>{a.SingleRegisterWithoutJump(0x27);}},
        {Keyword.GFL, a=>{a.SingleRegisterOnly(0x1c,0x40);}},
        {Keyword.GPO, a=>{a.SingleRegisterOnly(0x1c,0x00);}},
        {Keyword.PFL, a=>{a.Put(0x40);}},
        {Keyword.PPO, a=>{a.Put(0x00);}},
        {Keyword.GST, a=>{a.GST();}},
        {Keyword.PST, a=>{a.PST();}},
        {Keyword.STL, a=>{a.STL();}},
        {Keyword.LDL, a=>{a.SingleRegisterOnly(0x13, 0x00);}},
        {Keyword.GSR, a=>{a.GetPutStatus(0x1d);}},
        {Keyword.PSR, a=>{a.PSR();}},
        {Keyword.LDW, a=>{a.LDW();}},
        {Keyword.LDIW, a=>{a.IncDecWord(0xaa);}},
        {Keyword.LDDW, a=>{a.IncDecWord(0xac);}},
        {Keyword.LDCW, a=>{a.LDCW();}},
        {Keyword.STW, a=>{a.STW();}},
        {Keyword.STIW, a=>{a.IncDecWord(0xa2);}},
        {Keyword.STDW, a=>{a.IncDecWord(0xa4);}},
        {Keyword.PPSW, a=>{a.SingleRegisterWithoutJump(0xae);}},
        {Keyword.PPUW, a=>{a.SingleRegisterWithoutJump(0xaf);}},
        {Keyword.PHSW, a=>{a.SingleRegisterWithoutJump(0xa6);}},
        {Keyword.PHUW, a=>{a.SingleRegisterWithoutJump(0xa7);}},
        {Keyword.GRE, a=>{a.GRE();}},
        {Keyword.PRE, a=>{a.PRE();}},
        {Keyword.STLW, a=>{a.SingleRegisterOnly(0x92, 0x00);}},
        {Keyword.LDLW, a=>{a.SingleRegisterOnly(0x93, 0x00);}},
        {Keyword.PPOW, a=>{a.SingleRegisterOnly(0x94, 0x00);}},
        {Keyword.GFLW, a=>{a.SingleRegisterOnly(0x9c,0x40);}},
        {Keyword.GPOW, a=>{a.SingleRegisterOnly(0x9c,0x00);}},
        {Keyword.GSRW, a=>{a.GetPutStatus(0x9d);}},
        {Keyword.PSRW, a=>{a.GetPutStatus(0x95);}},
        {Keyword.INV, a=>{a.SingleRegisterOnly(0x1b,0x40);}},
        {Keyword.CMP, a=>{a.SingleRegisterOnly(0x1b,0x00);}},
        {Keyword.AD, a=>{a.AddSubByte(0x08, 0x3c);}},
        {Keyword.SB, a=>{a.AddSubByte(0x09, 0x3e);}},
        {Keyword.ADB, a=>{a.OperateRegisterOrImmediate(0x0a);}},
        {Keyword.SBB, a=>{a.OperateRegisterOrImmediate(0x0b);}},
        {Keyword.ADC, a=>{a.AddSubByte(0x00, 0x38);}},
        {Keyword.SBC, a=>{a.AddSubByte(0x01, 0x3a);}},
        {Keyword.AN, a=>{a.OperateRegisterOrImmediate(0x0c);}},
        {Keyword.ANC, a=>{a.OperateRegisterOrImmediate(0x04);}},
        {Keyword.NA, a=>{a.OperateRegisterOrImmediate(0x0d);}},
        {Keyword.NAC, a=>{a.OperateRegisterOrImmediate(0x05);}},
        {Inu.Assembler.Keyword.Or, a=>{a.OperateRegisterOrImmediate(0x0e);}},
        {Keyword.ORC, a=>{a.OperateRegisterOrImmediate(0x06);}},
        {Keyword.XR, a=>{a.OperateRegisterOrImmediate(0x0f);}},
        {Keyword.XRC, a=>{a.OperateRegisterOrImmediate(0x07);}},
        {Keyword.INVW, a=>{a.SingleRegisterOnly(0x9b,0x40);}},
        {Keyword.CMPW, a=>{a.SingleRegisterOnly(0x9b,0x00);}},
        {Keyword.ADW, a=>{a.AddSubWord(0x88, 0xbc);}},
        {Keyword.SBW, a=>{a.AddSubWord(0x89, 0xbe);}},
        {Keyword.ADBW, a=>{a.OperateRegister(0x8a);}},
        {Keyword.SBBW, a=>{a.OperateRegister(0x8b);}},
        {Keyword.ADCW, a=>{a.AddSubWord(0x80, 0xb8);}},
        {Keyword.SBCW, a=>{a.AddSubWord(0x81, 0xba);}},
        {Keyword.ANW, a=>{a.OperateRegister(0x8c);}},
        {Keyword.ANCW, a=>{a.OperateRegister(0x84);}},
        {Keyword.NAW, a=>{a.OperateRegister(0x8d);}},
        {Keyword.NACW, a=>{a.OperateRegister(0x85);}},
        {Keyword.ORW, a=>{a.OperateRegister(0x8e);}},
        {Keyword.ORCW, a=>{a.OperateRegister(0x86);}},
        {Keyword.XRW, a=>{a.OperateRegister(0x8f);}},
        {Keyword.XRCW, a=>{a.OperateRegister(0x87);}},
        {Keyword.ROD, a=>{a.SingleRegisterOnly(0x18,0x00);}},
        {Keyword.ROU, a=>{a.SingleRegisterOnly(0x18,0x20);}},
        {Keyword.BID, a=>{a.SingleRegisterOnly(0x18,0x40);}},
        {Keyword.BIU, a=>{a.SingleRegisterOnly(0x18,0x60);}},
        {Keyword.DID, a=>{a.SingleRegisterOnly(0x1a,0x00);}},
        {Keyword.DIU, a=>{a.SingleRegisterOnly(0x1a,0x20);}},
        {Keyword.BYD, a=>{a.SingleRegisterOnly(0x1a,0x40);}},
        {Keyword.BYU, a=>{a.SingleRegisterOnly(0x1a,0x60);}},
        {Keyword.RODW, a=>{a.SingleRegisterOnly(0x98,0x00);}},
        {Keyword.ROUW, a=>{a.SingleRegisterOnly(0x98,0x20);}},
        {Keyword.BIDW, a=>{a.SingleRegisterOnly(0x98,0x40);}},
        {Keyword.BIUW, a=>{a.SingleRegisterOnly(0x98,0x60);}},
        {Keyword.DIDW, a=>{a.SingleRegisterOnly(0x9a,0x00);}},
        {Keyword.DIUW, a=>{a.SingleRegisterOnly(0x9a,0x20);}},
        {Keyword.BYDW, a=>{a.SingleRegisterOnly(0x9a,0x40);}},
        {Keyword.BYUW, a=>{a.SingleRegisterOnly(0x9a,0x60);}},
        {Keyword.JP, a=>{a.JP();}},
        {Keyword.JR, a=>{a.JR();}},
        {Keyword.CAL, a=>{a.CAL();}},
        {Keyword.RTN, a=>{a.RTN();}},
        {Keyword.BUP,a=>{a.WriteByte(0xd8);}},
        {Keyword.BDN,a=>{a.WriteByte(0xd9);}},
        {Keyword.SUP, a=>{a.OperateMainRegisterOrImmediate(0x5c);}},
        {Keyword.SDN, a=>{a.OperateMainRegisterOrImmediate(0x5d);}},
        {Keyword.BUPS,a=>{a.ImmediateByteOnly(0x58);}},
        {Keyword.BDNS,a=>{a.ImmediateByteOnly(0x59);}},
        {Keyword.NOP,a=>{a.WriteByte(0xf8);}},
        {Keyword.CLT,a=>{a.WriteByte(0xf9);}},
        {Keyword.FST,a=>{a.WriteByte(0xfa);}},
        {Keyword.SLW,a=>{a.WriteByte(0xfb);}},
        {Keyword.CANI,a=>{a.WriteByte(0xfc);}},
        {Keyword.RTNI,a=>{a.WriteByte(0xfd);}},
        {Keyword.OFF,a=>{a.WriteByte(0xfe);}},
        {Keyword.TRP,a=>{a.WriteByte(0xff);}},
        {Keyword.LDM, a=>{a.LDM();}},
        {Keyword.LDIM, a=>{a.IncDecMulti(0xea);}},
        {Keyword.LDDM, a=>{a.IncDecMulti(0xec);}},
        {Keyword.LDCM, a=>{a.OperateRegister(0xc3, true);}},
        {Keyword.STM, a=>{a.IncDecMulti(0xe0);}},
        {Keyword.STIM, a=>{a.IncDecMulti(0xe2);}},
        {Keyword.STDM, a=>{a.IncDecMulti(0xe4);}},
        {Keyword.PPSM, a=>{a.SingleRegisterWithoutJump(0xee, 0x00, true);}},
        {Keyword.PPUM, a=>{a.SingleRegisterWithoutJump(0xef, 0x00, true);}},
        {Keyword.PHSM, a=>{a.SingleRegisterWithoutJump(0xe6, 0x00, true);}},
        {Keyword.PHUM, a=>{a.SingleRegisterWithoutJump(0xe7, 0x00, true);}},
        {Keyword.STLM, a=>{a.SingleRegisterWithoutJump(0xd2, 0x00, true);}},
        {Keyword.LDLM, a=>{a.SingleRegisterWithoutJump(0xd3, 0x00, true);}},
        {Keyword.PPOM, a=>{a.SingleRegisterWithoutJump(0xd4, 0x00, true);}},
        {Keyword.PSRM, a=>{a.PSRM();}},
        {Keyword.INVM, a=>{a.SingleRegisterWithoutJump(0xdb, 0x40, true);}},
        {Keyword.CMPM, a=>{a.SingleRegisterWithoutJump(0xdb, 0x00, true);}},
        {Keyword.ADBM, a=>{a.OperateRegisterOrImmediate(0xc8, true);}},
        {Keyword.ADBCM, a=>{a.OperateRegister(0xc0, true);}},
        {Keyword.SBBM, a=>{a.OperateRegisterOrImmediate(0xc9, true);}},
        {Keyword.SBBCM, a=>{a.OperateRegister(0xc1, true);}},
        {Keyword.ANM, a=>{a.OperateRegister(0xcc, true);}},
        {Keyword.ANCM, a=>{a.OperateRegister(0xc4, true);}},
        {Keyword.NAM, a=>{a.OperateRegister(0xcd, true);}},
        {Keyword.NACM, a=>{a.OperateRegister(0xc5, true);}},
        {Keyword.ORM, a=>{a.OperateRegister(0xce, true);}},
        {Keyword.ORCM, a=>{a.OperateRegister(0xc6, true);}},
        {Keyword.XRM, a=>{a.OperateRegister(0xcf, true);}},
        {Keyword.XRCM, a=>{a.OperateRegister(0xc7, true);}},
        {Keyword.DIDM, a=>{a.SingleRegisterOnly(0xda, 0x00, true);}},
        {Keyword.DIUM, a=>{a.SingleRegisterOnly(0xda, 0x20, true);}},
        {Keyword.BYDM, a=>{a.SingleRegisterOnly(0xda, 0x40, true);}},
        {Keyword.BYUM, a=>{a.SingleRegisterOnly(0xda, 0x60, true);}},
        //
        {Inu.Assembler.Keyword.If, a=>{a.IfStatement(); }},
        {Inu.Assembler.Keyword.Else, a=>{a.ElseStatement(); }},
        {Inu.Assembler.Keyword.EndIf,a=>{a.EndIfStatement(); }},
        {Inu.Assembler.Keyword.ElseIf, a=>{a.ElseIfStatement(); }},
        {Inu.Assembler.Keyword.Do, a=>{a.DoStatement(); }},
        {Inu.Assembler.Keyword.While, a=>{a.WhileStatement(); }},
        {Inu.Assembler.Keyword.WEnd, a=>{a.WEndStatement(); }},
    };

    protected override bool Instruction()
    {
        var reservedWord = LastToken as ReservedWord;
        Debug.Assert(reservedWord != null);
        if (!Actions.TryGetValue(reservedWord.Id, out var action)) { return false; }
        NextToken();
        action(this);
        return true;
    }
}