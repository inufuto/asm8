using Inu.Language;
using System.Diagnostics;

namespace Inu.Assembler.SC61860
{
    internal class Assembler() : BigEndianAssembler(new Tokenizer())
    {
        protected override bool IsRelativeOffsetInRange(int offset)
        {
            return Math.Abs(offset) <= 0x100;
        }

        private void OneByteOperand(int code)
        {
            var valueToken = LastToken;
            var value = Expression();
            if (value != null) {
                WriteByte(code);
                WriteByte(valueToken, value);
                return;
            }
            ShowSyntaxError(LastToken);
        }

        private void TwoByteOperand(int code)
        {
            var valueToken = LastToken;
            var value = Expression();
            if (value != null) {
                WriteByte(code);
                WriteWord(valueToken, value);
                return;
            }
            ShowSyntaxError(LastToken);
        }

        private void SixBitOperand(int code)
        {
            var valueToken = LastToken;
            var value = Expression();
            if (value != null) {
                if (value.IsConst()) {
                    var constValue = value.Value;
                    if (constValue is >= 0 and < 0b01000000) {
                        WriteByte(code | (constValue & 0b111111));
                        return;
                    }
                    ShowError(valueToken.Position, "Out of range:" + value);
                    return;
                }
                ShowAddressUsageError(valueToken);
            }
            ShowSyntaxError(LastToken);
        }

        private void JumpPlus(int code)
        {
            var valueToken = LastToken;
            if (RelativeOffset(out var address, out var offset)) {
                ++offset;
                if (offset < 0) {
                    ShowOutOfRange(valueToken, offset);
                    return;
                }
            }
            else {
                offset = 0;
            }
            WriteByte(code);
            WriteByte(offset);
        }

        private void JumpMinus(int code)
        {
            var valueToken = LastToken;
            if (RelativeOffset(out var address, out var offset)) {
                ++offset;
                if (offset > 0) {
                    ShowOutOfRange(valueToken, offset);
                    return;
                }
            }
            else {
                offset = 0;
            }
            WriteByte(code);
            WriteByte(-offset);
        }

        private void Case1()
        {
            var countToken = LastToken;
            var count = Expression();
            if (count != null) {
                if (count.IsConst()) {
                    AcceptReservedWord(',');
                    var addressToken = LastToken;
                    var address = Expression();
                    if (address != null) {
                        WriteByte(0b01111010);
                        WriteByte(count.Value);
                        WriteWord(addressToken, address);
                        return;
                    }
                }
                else {
                    ShowAddressUsageError(countToken);
                    return;
                }
            }
            ShowSyntaxError(LastToken);
        }

        private void Case2()
        {
            var valueToken = LastToken;
            var value = Expression();
            if (value != null) {
                WriteByte(0b01101001);
                while (LastToken.IsReservedWord(',')) {
                    var addressToken = NextToken();
                    var address = Expression();
                    if (address != null) {
                        WriteByte(value.Value);
                        WriteWord(addressToken, address);
                    }
                    AcceptReservedWord(',');
                    valueToken = LastToken;
                    value = Expression();
                    if (value != null) continue;
                    ShowSyntaxError(LastToken);
                    return;
                }
                WriteWord(valueToken, value);
                return;
            }
            ShowSyntaxError(LastToken);
        }

        private void Cal()
        {
            var valueToken = LastToken;
            var value = Expression();
            if (value != null) {
                if (value.IsConst()) {
                    var address = value.Value;
                    if (address >= 0 && address < 0x2000) {
                        WriteByte(0b11100000 | (address >> 8));
                        WriteByte(address & 0xff);
                        return;
                    }
                    ShowOutOfRange(valueToken, address);
                    return;
                }
                ShowAddressUsageError(valueToken);
            }
            ShowSyntaxError(LastToken);
        }

        private void Jump(int relativeCode, int absoluteCode)
        {
            var addressToken = LastToken;
            var address = Expression();
            if (address == null) {
                ShowSyntaxError();
                address = Address.Default;
            }
            Jump(relativeCode, absoluteCode, addressToken, address);
        }

        private void Jump(int relativeCode, int absoluteCode, Token addressToken, Address address)
        {
            if (RelativeOffset(LastToken, address, out var offset)) {
                ++offset;
                if (offset is >= 0 and < 0x100) {
                    WriteByte(relativeCode);
                    WriteByte(offset);
                    return;
                }

                if (offset is <= 0 and > -0x100) {
                    WriteByte(relativeCode | 0b00000001);
                    WriteByte(-offset);
                    return;
                }
            }

            WriteByte(absoluteCode);
            WriteWord(addressToken, address);
        }

        private void IfStatement()
        {
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

        private void StartIf(IfBlock block)
        {
            var address = SymbolAddress(block.ElseId);
            NegatedConditionalJump(address);
        }

        private void ConditionalJump(Address address)
        {
            if (LastToken is ReservedWord reservedWord) {
                switch (reservedWord.Id) {
                    case Keyword.Z:
                        NextToken();
                        Jump(0b00111000, 0b00111110, LastToken, address); // Z
                        return;
                    case Keyword.NZ:
                        NextToken();
                        Jump(0b00101000, 0b00111100, LastToken, address); // NZ
                        return;
                    case Keyword.C:
                        NextToken();
                        Jump(0b00101100, 0b00111111, LastToken, address); // C
                        return;
                    case Keyword.NC:
                        NextToken();
                        Jump(0b00101010, 0b01101101, LastToken, address); // NC
                        return;
                }
            }
            ShowError(LastToken.Position, "Missing Condition.");
        }

        private void NegatedConditionalJump(Address address)
        {
            if (LastToken is ReservedWord reservedWord) {
                switch (reservedWord.Id) {
                    case Keyword.Z:
                        NextToken();
                        Jump(0b00101000, 0b00111100, LastToken, address); // NZ
                        return;
                    case Keyword.NZ:
                        NextToken();
                        Jump(0b00111000, 0b00111110, LastToken, address); // Z
                        return;
                    case Keyword.C:
                        NextToken();
                        Jump(0b00101010, 0b01101101, LastToken, address); // NC
                        return;
                    case Keyword.NC:
                        NextToken();
                        Jump(0b00101100, 0b00111111, LastToken, address); // C
                        return;
                }
            }
            ShowError(LastToken.Position, "Missing Condition.");
        }

        private void UnconditionalJump(Address address)
        {
            Jump(0b00101100, 0b01111001, LastToken, address);
        }


        private static readonly Dictionary<int, Action<Assembler>> Actions = new()
        {
            { Keyword.LII, a=>a.OneByteOperand(0b00000000) },
            { Keyword.LIJ, a=>a.OneByteOperand(0b00000001) },
            { Keyword.LIA, a=>a.OneByteOperand(0b00000010) },
            { Keyword.LIB, a=>a.OneByteOperand(0b00000011) },
            { Keyword.LIP, a=>a.OneByteOperand(0b00010010) },
            { Keyword.LIQ, a=>a.OneByteOperand(0b00010011) },
            { Keyword.LIDP, a=>a.TwoByteOperand(0b00010000) },
            { Keyword.LIDL, a=>a.OneByteOperand(0b00010001) },
            { Keyword.LP, a=>a.SixBitOperand(0b10000000) },
            { Keyword.LDP, a=>a.WriteByte(0b00100000) },
            { Keyword.LDQ, a=>a.WriteByte(0b00100001) },
            { Keyword.LDR, a=>a.WriteByte(0b00100010) },
            { Keyword.STP, a=>a.WriteByte(0b00110000) },
            { Keyword.STQ, a=>a.WriteByte(0b00110001) },
            { Keyword.STR, a=>a.WriteByte(0b00110010) },
            { Keyword.LDM, a=>a.WriteByte(0b01011001) },
            { Keyword.LDD, a=>a.WriteByte(0b01010111) },
            { Keyword.STD, a=>a.WriteByte(0b01010010) },
            { Keyword.MVMD, a=>a.WriteByte(0b01010101) },
            { Keyword.MVDM, a=>a.WriteByte(0b01010011) },
            { Keyword.EXAM, a=>a.WriteByte(0b11011011) },
            { Keyword.EXAB, a=>a.WriteByte(0b11011010) },
            { Keyword.MVW, a=>a.WriteByte(0b00001000) },
            { Keyword.MVB, a=>a.WriteByte(0b00001010) },
            { Keyword.MVWD, a=>a.WriteByte(0b00011000) },
            { Keyword.MVBD, a=>a.WriteByte(0b00011010) },
            { Keyword.EXW, a=>a.WriteByte(0b00001001) },
            { Keyword.EXB, a=>a.WriteByte(0b00001011) },
            { Keyword.EXWD, a=>a.WriteByte(0b00011001) },
            { Keyword.EXBD, a=>a.WriteByte(0b00011011) },
            { Keyword.INCP, a=>a.WriteByte(0b01010000) },
            { Keyword.DECP, a=>a.WriteByte(0b01010001) },
            { Keyword.INCI, a=>a.WriteByte(0b01000000) },
            { Keyword.INCJ, a=>a.WriteByte(0b11000000) },
            { Keyword.INCA, a=>a.WriteByte(0b01000010) },
            { Keyword.INCB, a=>a.WriteByte(0b11000010) },
            { Keyword.INCK, a=>a.WriteByte(0b01001000) },
            { Keyword.INCL, a=>a.WriteByte(0b11001000) },
            { Keyword.INCM, a=>a.WriteByte(0b01001010) },
            { Keyword.INCN, a=>a.WriteByte(0b11001010) },
            { Keyword.DECI, a=>a.WriteByte(0b01000001) },
            { Keyword.DECJ, a=>a.WriteByte(0b11000001) },
            { Keyword.DECA, a=>a.WriteByte(0b01000011) },
            { Keyword.DECB, a=>a.WriteByte(0b11000011) },
            { Keyword.DECK, a=>a.WriteByte(0b01001001) },
            { Keyword.DECL, a=>a.WriteByte(0b11001001) },
            { Keyword.DECM, a=>a.WriteByte(0b01001011) },
            { Keyword.DECN, a=>a.WriteByte(0b11001011) },
            { Keyword.IX, a=>a.WriteByte(0b00000100) },
            { Keyword.IY, a=>a.WriteByte(0b00000110) },
            { Keyword.DX, a=>a.WriteByte(0b00000101) },
            { Keyword.DY, a=>a.WriteByte(0b00000111) },
            { Keyword.IXL, a=>a.WriteByte(0b00100100) },
            { Keyword.DXL, a=>a.WriteByte(0b00100101) },
            { Keyword.IYS, a=>a.WriteByte(0b00100110) },
            { Keyword.DYS, a=>a.WriteByte(0b00100111) },
            { Keyword.FILM, a=>a.WriteByte(0b00011110) },
            { Keyword.FILD, a=>a.WriteByte(0b00011111) },
            { Keyword.ADIA, a=>a.OneByteOperand(0b01110100) },
            { Keyword.SBIA, a=>a.OneByteOperand(0b01110101) },
            { Keyword.ADIM, a=>a.OneByteOperand(0b01110000) },
            { Keyword.SBIM, a=>a.OneByteOperand(0b01110001) },
            { Keyword.ADM, a=>a.WriteByte(0b01000100) },
            { Keyword.SBM, a=>a.WriteByte(0b01000101) },
            { Keyword.ADCM, a=>a.WriteByte(0b11000100) },
            { Keyword.SBCM, a=>a.WriteByte(0b11000101) },
            { Keyword.ADB, a=>a.WriteByte(0b00010100) },
            { Keyword.SBB, a=>a.WriteByte(0b00010101) },
            { Keyword.ADN, a=>a.WriteByte(0b00001100) },
            { Keyword.SBN, a=>a.WriteByte(0b00001101) },
            { Keyword.ADW, a=>a.WriteByte(0b00001110) },
            { Keyword.SBW, a=>a.WriteByte(0b00001111) },
            { Keyword.SRW, a=>a.WriteByte(0b00011100) },
            { Keyword.SLW, a=>a.WriteByte(0b00011101) },
            { Keyword.ORIA, a=>a.OneByteOperand(0b01100101) },
            { Keyword.ORIM, a=>a.OneByteOperand(0b01100001) },
            { Keyword.ORID, a=>a.OneByteOperand(0b11010101) },
            { Keyword.ORMA, a=>a.WriteByte(0b01000111) },
            { Keyword.ANIA, a=>a.OneByteOperand(0b01100100) },
            { Keyword.ANIM, a=>a.OneByteOperand(0b01100000) },
            { Keyword.ANID, a=>a.OneByteOperand(0b11010100) },
            { Keyword.ANMA, a=>a.WriteByte(0b01000110) },
            { Keyword.TSIA, a=>a.OneByteOperand(0b01100110) },
            { Keyword.TSIM, a=>a.OneByteOperand(0b01100010) },
            { Keyword.TSID, a=>a.OneByteOperand(0b11010110) },
            { Keyword.TSMA, a=>a.WriteByte(0b11000110) },
            { Keyword.CPIA, a=>a.OneByteOperand(0b01100111) },
            { Keyword.CPIM, a=>a.OneByteOperand(0b01100011) },
            { Keyword.CPMA, a=>a.WriteByte(0b11000111) },
            { Keyword.SWP, a=>a.WriteByte(0b01011000) },
            { Keyword.SR, a=>a.WriteByte(0b11010010) },
            { Keyword.SL, a=>a.WriteByte(0b01011010) },
            { Keyword.SC, a=>a.WriteByte(0b11010000) },
            { Keyword.RC, a=>a.WriteByte(0b11010001) },
            { Keyword.JRZP, a=>a.JumpPlus(0b00111000) },
            { Keyword.JRNZP, a=>a.JumpPlus(0b00101000) },
            { Keyword.JRCP, a=>a.JumpPlus(0b00111010) },
            { Keyword.JRNCP, a=>a.JumpPlus(0b00101010) },
            { Keyword.JRP, a=>a.JumpPlus(0b00101100) },
            { Keyword.JRZM, a=>a.JumpMinus(0b00111001) },
            { Keyword.JRNZM, a=>a.JumpMinus(0b00101001) },
            { Keyword.JRCM, a=>a.JumpMinus(0b00111011) },
            { Keyword.JRNCM, a=>a.JumpMinus(0b00101011) },
            { Keyword.JRM, a=>a.JumpMinus(0b00101101) },
            { Keyword.JPZ, a=>a.TwoByteOperand(0b01111110) },
            { Keyword.JPNZ, a=>a.TwoByteOperand(0b01111100) },
            { Keyword.JPC, a=>a.TwoByteOperand(0b01111111) },
            { Keyword.JPNC, a=>a.TwoByteOperand(0b01111101) },
            { Keyword.JP, a=>a.TwoByteOperand(0b01111001) },
            { Keyword.CASE1, a=>a.Case1() },
            { Keyword.CASE2, a=>a.Case2() },
            { Keyword.PUSH, a=>a.WriteByte(0b00110100) },
            { Keyword.POP, a=>a.WriteByte(0b01011011) },
            { Keyword.LOOP, a=>a.JumpMinus(0b00101111) },
            { Keyword.LEAVE, a=>a.WriteByte(0b11011000) },
            { Keyword.CAL, a=>a.Cal() },
            { Keyword.CALL, a=>a.TwoByteOperand(0b01111000) },
            { Keyword.RTN, a=>a.WriteByte(0b00110111) },
            { Keyword.NOPW, a=>a.WriteByte(0b01001101) },
            { Keyword.NOPT, a=>a.WriteByte(0b11001110) },
            { Keyword.WAIT, a=>a.OneByteOperand(0b01001110) },
            { Keyword.OUTC, a=>a.WriteByte(0b11011111) },
            { Keyword.OUTA, a=>a.WriteByte(0b01011101) },
            { Keyword.OUTB, a=>a.WriteByte(0b11011101) },
            { Keyword.OUTF, a=>a.WriteByte(0b01011111) },
            { Keyword.INA, a=>a.WriteByte(0b01001100) },
            { Keyword.INB, a=>a.WriteByte(0b11001100) },
            { Keyword.TEST, a=>a.OneByteOperand(0b01101011) },
            { Keyword.CUP, a=>a.WriteByte(0b01001111) },
            { Keyword.CDN, a=>a.WriteByte(0b01101111) },
            //
            { Keyword.JRZ, a=>a.Jump(0b00111000, 0b01111110) },
            { Keyword.JRNZ, a=>a.Jump(0b00101000, 0b01111100) },
            { Keyword.JRC, a=>a.Jump(0b00111010, 0b01111111) },
            { Keyword.JRNC, a=>a.Jump(0b00101010, 0b01111101) },
            { Keyword.JR, a=>a.Jump(0b00101100, 0b01111001) },
            //
            {Inu.Assembler.Keyword.If, a=>{a.IfStatement(); }},
            {Inu.Assembler.Keyword.Else, a=>{a.ElseStatement(); }},
            {Inu.Assembler.Keyword.EndIf,a=>{a.EndIfStatement(); }},
            {Inu.Assembler.Keyword.ElseIf, a=>{a.ElseIfStatement(); }},
            { Inu.Assembler.Keyword.Do, a=>{a.DoStatement(); }},
            { Inu.Assembler.Keyword.While, a=>{a.WhileStatement(); }},
            { Inu.Assembler.Keyword.WEnd, a=>{a.WEndStatement(); }},
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
}
