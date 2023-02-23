using System.Diagnostics;
using Inu.Language;

namespace Inu.Assembler.Mc6809
{
    class Assembler : BigEndianAssembler
    {
        public Assembler() : base(new Tokenizer()) { }

        public override bool ZeroPageAvailable => true;

        protected override bool Instruction()
        {
            if (!(LastToken is ReservedWord reservedWord)) return false;
            switch (reservedWord.Id) {
                case Keyword.LdA:
                    NextToken();
                    FourModeByte(0x86);
                    return true;
                case Keyword.LdB:
                    NextToken();
                    FourModeByte(0xc6);
                    return true;
                case Keyword.LdD:
                    NextToken();
                    FourModeWord(0xcc);
                    return true;
                case Keyword.LdX:
                    NextToken();
                    FourModeWord(0x8e);
                    return true;
                case Keyword.LdY:
                    WriteByte(0x10);
                    NextToken();
                    FourModeWord(0x8e);
                    return true;
                case Keyword.LdU:
                    NextToken();
                    FourModeWord(0xce);
                    return true;
                case Keyword.LdS:
                    NextToken();
                    WriteByte(0x10);
                    FourModeWord(0xce);
                    return true;
                case Keyword.StA:
                    NextToken();
                    ThreeMode9Ab(0x87);
                    return true;
                case Keyword.StB:
                    NextToken();
                    ThreeMode9Ab(0xc7);
                    return true;
                case Keyword.StD:
                    NextToken();
                    ThreeMode9Ab(0xcd);
                    return true;
                case Keyword.StX:
                    NextToken();
                    ThreeMode9Ab(0x8f);
                    return true;
                case Keyword.StY:
                    NextToken();
                    WriteByte(0x10);
                    ThreeMode9Ab(0x8f);
                    return true;
                case Keyword.StU:
                    NextToken();
                    ThreeMode9Ab(0xcf);
                    return true;
                case Keyword.StS:
                    NextToken();
                    WriteByte(0x10);
                    ThreeMode9Ab(0xcf);
                    return true;
                case Keyword.AddA:
                    NextToken();
                    FourModeByte(0x8b);
                    return true;
                case Keyword.AddB:
                    NextToken();
                    FourModeByte(0xcb);
                    return true;
                case Keyword.AddD:
                    NextToken();
                    FourModeWord(0xc3);
                    return true;
                case Keyword.AdcA:
                    NextToken();
                    FourModeByte(0x89);
                    return true;
                case Keyword.AdcB:
                    NextToken();
                    FourModeByte(0xc9);
                    return true;
                case Keyword.SubA:
                    NextToken();
                    FourModeByte(0x80);
                    return true;
                case Keyword.SubB:
                    NextToken();
                    FourModeByte(0xC0);
                    return true;
                case Keyword.SubD:
                    NextToken();
                    FourModeWord(0x83);
                    return true;
                case Keyword.SbcA:
                    NextToken();
                    FourModeByte(0x82);
                    return true;
                case Keyword.SbcB:
                    NextToken();
                    FourModeByte(0xc2);
                    return true;
                case Keyword.CmpA:
                    NextToken();
                    FourModeByte(0x81);
                    return true;
                case Keyword.CmpB:
                    NextToken();
                    FourModeByte(0xC1);
                    return true;
                case Keyword.CmpD:
                    NextToken();
                    WriteByte(0x10);
                    FourModeWord(0x83);
                    return true;
                case Keyword.CmpX:
                    NextToken();
                    FourModeWord(0x8c);
                    return true;
                case Keyword.CmpY:
                    NextToken();
                    WriteByte(0x10);
                    FourModeWord(0x8c);
                    return true;
                case Keyword.CmpU:
                    NextToken();
                    WriteByte(0x11);
                    FourModeWord(0x83);
                    return true;
                case Keyword.CmpS:
                    NextToken();
                    WriteByte(0x11);
                    FourModeWord(0x8c);
                    return true;
                case Keyword.Mul:
                    NextToken();
                    WriteByte(0x3d);
                    return true;
                case Keyword.AndA:
                    NextToken();
                    FourModeByte(0x84);
                    return true;
                case Keyword.AndB:
                    NextToken();
                    FourModeByte(0xC4);
                    return true;
                case Keyword.OrA:
                    NextToken();
                    FourModeByte(0x8a);
                    return true;
                case Keyword.OrB:
                    NextToken();
                    FourModeByte(0xCa);
                    return true;
                case Keyword.EorA:
                    NextToken();
                    FourModeByte(0x88);
                    return true;
                case Keyword.EorB:
                    NextToken();
                    FourModeByte(0xc8);
                    return true;
                case Keyword.BitA:
                    NextToken();
                    FourModeByte(0x85);
                    return true;
                case Keyword.BitB:
                    NextToken();
                    FourModeByte(0xc5);
                    return true;
                case Keyword.Clr:
                    NextToken();
                    ThreeMode067(0x0f);
                    return true;
                case Keyword.ClrA:
                    NextToken();
                    WriteByte(0x4f);
                    return true;
                case Keyword.ClrB:
                    NextToken();
                    WriteByte(0x5f);
                    return true;
                case Keyword.Inc:
                    NextToken();
                    ThreeMode067(0x0c);
                    return true;
                case Keyword.IncA:
                    NextToken();
                    WriteByte(0x4c);
                    return true;
                case Keyword.IncB:
                    NextToken();
                    WriteByte(0x5c);
                    return true;
                case Keyword.Dec:
                    NextToken();
                    ThreeMode067(0x0a);
                    return true;
                case Keyword.DecA:
                    NextToken();
                    WriteByte(0x4a);
                    return true;
                case Keyword.DecB:
                    NextToken();
                    WriteByte(0x5a);
                    return true;
                case Keyword.Tst:
                    NextToken();
                    ThreeMode067(0x0d);
                    return true;
                case Keyword.TstA:
                    NextToken();
                    WriteByte(0x4d);
                    return true;
                case Keyword.TstB:
                    NextToken();
                    WriteByte(0x5d);
                    return true;
                case Keyword.Com:
                    NextToken();
                    ThreeMode067(0x03);
                    return true;
                case Keyword.ComA:
                    NextToken();
                    WriteByte(0x43);
                    return true;
                case Keyword.ComB:
                    NextToken();
                    WriteByte(0x53);
                    return true;
                case Keyword.Neg:
                    NextToken();
                    ThreeMode067(0x00);
                    return true;
                case Keyword.NegA:
                    NextToken();
                    WriteByte(0x40);
                    return true;
                case Keyword.NegB:
                    NextToken();
                    WriteByte(0x50);
                    return true;
                case Keyword.Asl:
                case Keyword.Lsl:
                    NextToken();
                    ThreeMode067(0x08);
                    return true;
                case Keyword.AslA:
                case Keyword.LslA:
                    NextToken();
                    WriteByte(0x48);
                    return true;
                case Keyword.AslB:
                case Keyword.LslB:
                    NextToken();
                    WriteByte(0x58);
                    return true;
                case Keyword.Asr:
                    NextToken();
                    ThreeMode067(0x07);
                    return true;
                case Keyword.AsrA:
                    NextToken();
                    WriteByte(0x47);
                    return true;
                case Keyword.AsrB:
                    NextToken();
                    WriteByte(0x57);
                    return true;
                case Keyword.Lsr:
                    NextToken();
                    ThreeMode067(0x04);
                    return true;
                case Keyword.LsrA:
                    NextToken();
                    WriteByte(0x44);
                    return true;
                case Keyword.LsrB:
                    NextToken();
                    WriteByte(0x54);
                    return true;
                case Keyword.Rol:
                    NextToken();
                    ThreeMode067(0x09);
                    return true;
                case Keyword.RolA:
                    NextToken();
                    WriteByte(0x49);
                    return true;
                case Keyword.RolB:
                    NextToken();
                    WriteByte(0x59);
                    return true;
                case Keyword.Ror:
                    NextToken();
                    ThreeMode067(0x06);
                    return true;
                case Keyword.RorA:
                    NextToken();
                    WriteByte(0x46);
                    return true;
                case Keyword.RorB:
                    NextToken();
                    WriteByte(0x56);
                    return true;
                case Keyword.LeaX:
                    NextToken();
                    IndexedMode(0x30);
                    return true;
                case Keyword.LeaY:
                    NextToken();
                    IndexedMode(0x31);
                    return true;
                case Keyword.LeaU:
                    NextToken();
                    IndexedMode(0x33);
                    return true;
                case Keyword.LeaS:
                    NextToken();
                    IndexedMode(0x32);
                    return true;
                case Keyword.Exg:
                    NextToken();
                    WriteByte(0x1e);
                    PostExgTfr();
                    return true;
                case Keyword.Tfr:
                    NextToken();
                    WriteByte(0x1f);
                    PostExgTfr();
                    return true;
                case Keyword.Abx:
                    NextToken();
                    WriteByte(0x3a);
                    return true;
                case Keyword.Daa:
                    NextToken();
                    WriteByte(0x19);
                    return true;
                case Keyword.Sex:
                    NextToken();
                    WriteByte(0x1d);
                    return true;
                case Keyword.PshS:
                    NextToken();
                    WriteByte(0x34);
                    PostPushPull(Keyword.S);
                    return true;
                case Keyword.PulS:
                    NextToken();
                    WriteByte(0x35);
                    PostPushPull(Keyword.S);
                    return true;
                case Keyword.PshU:
                    NextToken();
                    WriteByte(0x36);
                    PostPushPull(Keyword.U);
                    return true;
                case Keyword.PulU:
                    NextToken();
                    WriteByte(0x37);
                    PostPushPull(Keyword.U);
                    return true;
                case Keyword.AndCc:
                    NextToken();
                    WriteByte(0x1c);
                    ImmediateByte();
                    return true;
                case Keyword.OrCc:
                    NextToken();
                    WriteByte(0x1a);
                    ImmediateByte();
                    return true;
                case Keyword.Sync:
                    NextToken();
                    WriteByte(0x13);
                    return true;
                case Keyword.CWai:
                    NextToken();
                    WriteByte(0x3c);
                    ImmediateByte();
                    return true;
                case Keyword.Swi:
                    NextToken();
                    WriteByte(0x3f);
                    return true;
                case Keyword.Swi2:
                    NextToken();
                    WriteByte(0x10);
                    WriteByte(0x3f);
                    return true;
                case Keyword.Swi3:
                    NextToken();
                    WriteByte(0x11);
                    WriteByte(0x3f);
                    return true;
                case Keyword.Rti:
                    NextToken();
                    WriteByte(0x3b);
                    return true;
                case Keyword.Rts:
                    NextToken();
                    WriteByte(0x39);
                    return true;
                case Keyword.Nop:
                    NextToken();
                    WriteByte(0x12);
                    return true;
                case Keyword.Jsr:
                    NextToken();
                    ThreeMode9Ab(0x8d);
                    return true;
                case Keyword.Jmp:
                    NextToken();
                    ThreeMode067(0x0e);
                    return true;
                case Keyword.Bsr:
                    NextToken();
                    Branch(0x8d, 0x17);
                    return true;
                case Keyword.LBsr:
                    NextToken();
                    WriteByte(0x17);
                    LongOffset();
                    return true;
                case Keyword.Bra:
                    NextToken();
                    Branch(0x20, 0x16);
                    return true;
                case Keyword.LBra:
                    NextToken();
                    WriteByte(0x16);
                    LongOffset();
                    return true;
                case Keyword.Brn:
                    NextToken();
                    Branch(0x21, null);
                    return true;
                case Keyword.LBrn:
                    NextToken();
                    WriteByte(0x10);
                    WriteByte(0x21);
                    LongOffset();
                    return true;
                case Keyword.Bhi:
                    NextToken();
                    Branch(0x22, null);
                    return true;
                case Keyword.LBhi:
                    NextToken();
                    WriteByte(0x10);
                    WriteByte(0x22);
                    LongOffset();
                    return true;
                case Keyword.Bls:
                    NextToken();
                    Branch(0x23, null);
                    return true;
                case Keyword.LBls:
                    NextToken();
                    WriteByte(0x10);
                    WriteByte(0x23);
                    LongOffset();
                    return true;
                case Keyword.Bcc:
                case Keyword.Bhs:
                    NextToken();
                    Branch(0x24, null);
                    return true;
                case Keyword.LBcc:
                case Keyword.LBhs:
                    NextToken();
                    WriteByte(0x10);
                    WriteByte(0x24);
                    LongOffset();
                    return true;
                case Keyword.Bcs:
                case Keyword.Blo:
                    NextToken();
                    Branch(0x25, null);
                    return true;
                case Keyword.LBcs:
                case Keyword.LBlo:
                    NextToken();
                    WriteByte(0x10);
                    WriteByte(0x25);
                    LongOffset();
                    return true;
                case Keyword.Bne:
                    NextToken();
                    Branch(0x26, null);
                    return true;
                case Keyword.LBne:
                    NextToken();
                    WriteByte(0x10);
                    WriteByte(0x26);
                    LongOffset();
                    return true;
                case Keyword.Beq:
                    NextToken();
                    Branch(0x27, null);
                    return true;
                case Keyword.LBeq:
                    NextToken();
                    WriteByte(0x10);
                    WriteByte(0x27);
                    LongOffset();
                    return true;
                case Keyword.Bvc:
                    NextToken();
                    Branch(0x28, null);
                    return true;
                case Keyword.LBvc:
                    NextToken();
                    WriteByte(0x10);
                    WriteByte(0x28);
                    LongOffset();
                    return true;
                case Keyword.Bvs:
                    NextToken();
                    Branch(0x29, null);
                    return true;
                case Keyword.LBvs:
                    NextToken();
                    WriteByte(0x10);
                    WriteByte(0x29);
                    LongOffset();
                    return true;
                case Keyword.Bpl:
                    NextToken();
                    Branch(0x2a, null);
                    return true;
                case Keyword.LBpl:
                    NextToken();
                    WriteByte(0x10);
                    WriteByte(0x2a);
                    LongOffset();
                    return true;
                case Keyword.Bmi:
                    NextToken();
                    Branch(0x2b, null);
                    return true;
                case Keyword.LBmi:
                    NextToken();
                    WriteByte(0x10);
                    WriteByte(0x2b);
                    LongOffset();
                    return true;
                case Keyword.Bge:
                    NextToken();
                    Branch(0x2c, null);
                    return true;
                case Keyword.LBge:
                    NextToken();
                    WriteByte(0x10);
                    WriteByte(0x2c);
                    LongOffset();
                    return true;
                case Keyword.Blt:
                    NextToken();
                    Branch(0x2d, null);
                    return true;
                case Keyword.LBlt:
                    NextToken();
                    WriteByte(0x10);
                    WriteByte(0x2d);
                    LongOffset();
                    return true;
                case Keyword.Bgt:
                    NextToken();
                    Branch(0x2e, null);
                    return true;
                case Keyword.LBgt:
                    NextToken();
                    WriteByte(0x10);
                    WriteByte(0x2e);
                    LongOffset();
                    return true;
                case Keyword.Ble:
                    NextToken();
                    Branch(0x2f, null);
                    return true;
                case Keyword.LBle:
                    NextToken();
                    WriteByte(0x10);
                    WriteByte(0x2f);
                    LongOffset();
                    return true;
                case Keyword.If:
                    IfStatement();
                    return true;
                case Keyword.ElseIf:
                    ElseIfStatement();
                    return true;
                case Keyword.Else:
                    ElseStatement();
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

        private void LongOffset()
        {
            var token = LastToken;
            RelativeOffset(out var address, out int offset);
            WriteWord(token, new Address(AddressType.Const, offset));
        }

        private void Branch(int code, int? longCode)
        {
            var token = LastToken;
            if (RelativeOffset(out var address, out int offset)) {
                WriteByte(code);
                WriteByte(offset);
                return;
            }
            if (longCode != null) {
                WriteByte(longCode.Value);
            }
            else {
                WriteByte(0x10);
                WriteByte(code);
                --offset;
            }
            --offset;
            WriteWord(token, new Address(AddressType.Const, offset));
        }

        private void ImmediateByte()
        {
            if (LastToken.IsReservedWord('#')) {
                //	immediate mode
                var token = NextToken();
                var value = Expression();
                if (value == null) {
                    ShowSyntaxError(token);
                    return;
                }

                var low = value.Low();
                Debug.Assert(low != null);
                WriteByte(token, low);
            }
            else
                ShowSyntaxError(LastToken);
        }

        private void FourModeByte(int instruction)
        {
            if (LastToken.IsReservedWord('#')) {
                //	immediate mode
                var token = NextToken();
                var value = Expression();
                if (value == null) {
                    ShowSyntaxError(token);
                    return;
                }

                WriteByte(instruction);
                WriteByte(token, value);
                return;
            }
            ThreeMode9Ab(instruction);
        }

        private void FourModeWord(int instruction)
        {
            if (LastToken.IsReservedWord('#')) {
                //	immediate mode
                var token = NextToken();
                var value = Expression();
                if (value == null) {
                    ShowSyntaxError(token);
                    return;
                }
                WriteByte(instruction);
                WriteWord(token, value);
                return;
            }
            ThreeMode9Ab(instruction);
        }


        private void ThreeMode9Ab(int instruction)
        {
            ThreeMode(instruction, 0x90, 0xa0, 0xb0);
        }

        private void ThreeMode067(int instruction)
        {
            ThreeMode(instruction, 0x00, 0x60, 0x70);
        }

        private void PostExgTfr()
        {
            bool RegisterCode(out int code)
            {
                code = 0;
                if (!(LastToken is ReservedWord reservedWord)) return false;
                switch (reservedWord.Id) {
                    case Keyword.D:
                        code = 0;
                        break;
                    case Keyword.X:
                        code = 1;
                        break;
                    case Keyword.Y:
                        code = 2;
                        break;
                    case Keyword.U:
                        code = 3;
                        break;
                    case Keyword.S:
                        code = 4;
                        break;
                    case Keyword.Pc:
                        code = 5;
                        break;
                    case Keyword.A:
                        code = 8;
                        break;
                    case Keyword.B:
                        code = 9;
                        break;
                    case Keyword.Cc:
                        code = 10;
                        break;
                    case Keyword.Dp:
                        code = 11;
                        break;
                    default:
                        return false;
                }

                return true;

            }

            var b = 0;
            if (RegisterCode(out var source)) {
                NextToken();
                AcceptReservedWord(',');
                if (RegisterCode(out var destination)) {
                    if ((source & 8) != (destination & 8)) {
                        ShowError(LastToken.Position, "Register size mismatch.");
                    }
                    NextToken();
                    b = source << 4 | destination;
                }
                else {
                    ShowSyntaxError(LastToken);
                }
            }
            else {
                ShowSyntaxError(LastToken);
            }
            WriteByte(b);
        }

        private void PostPushPull(in int stack)
        {
            var b = 0;
            while (LastToken.Type == TokenType.ReservedWord) {
                var reservedWord = LastToken as ReservedWord;
                Debug.Assert(reservedWord != null);
                switch (reservedWord.Id) {
                    case Keyword.Pc:
                        b |= 0x80;
                        break;
                    case Keyword.U:
                        b |= 0x40;
                        if (stack == Keyword.U) {
                            ShowInvalidRegister(LastToken);
                        }
                        break;
                    case Keyword.S:
                        b |= 0x40;
                        if (stack == Keyword.S) {
                            ShowInvalidRegister(LastToken);
                        }
                        break;
                    case Keyword.Y:
                        b |= 0x20;
                        break;
                    case Keyword.X:
                        b |= 0x10;
                        break;
                    case Keyword.Dp:
                        b |= 0x08;
                        break;
                    case Keyword.B:
                        b |= 0x04;
                        break;
                    case Keyword.A:
                        b |= 0x02;
                        break;
                    case Keyword.Cc:
                        b |= 0x01;
                        break;
                    default:
                        goto exit;
                }
                NextToken();
                if (!LastToken.IsReservedWord(',')) break;
                NextToken();
            }
            exit:
            WriteByte(b);
        }


        private void ThreeMode(int instruction, int directModeBits, int indexedModeBits, int extendedModeBits)
        {
            if (AccumulatorOffset(instruction | indexedModeBits, 0)) {
                return;
            }

            var expressionToken = LastToken;
            var value = Expression();
            if (value != null) {
                if (LastToken.IsReservedWord(',')) {
                    NextToken();
                    //  indexed mode
                    WriteByte(instruction | indexedModeBits);
                    if (ConstantOffset(value, expressionToken, 0)) return;
                    if (PcOffset(value, expressionToken, 0)) return;
                    if (LastToken.IsReservedWord(Keyword.Pc)) return;
                    ShowSyntaxError(LastToken);
                    return;
                }

                if (value.IsConst()) {
                    var constValue = value.Value;
                    if (constValue > 0x00 && constValue <= 0xff) {
                        // direct mode
                        WriteByte(instruction | directModeBits);
                        WriteByte(constValue);
                        return;
                    }
                }

                // extended mode
                WriteByte(instruction | extendedModeBits);
                WriteWord(expressionToken, value);
                return;
            }

            if (LastToken.IsReservedWord('<')) {
                // direct mode
                expressionToken = NextToken();
                value = Expression();
                if (value == null) {
                    ShowSyntaxError(expressionToken);
                    value = new Address(AddressType.Const, 0);
                }

                var low = value.Low();
                Debug.Assert(low != null);
                WriteByte(instruction | directModeBits);
                WriteByte(expressionToken, low);
                return;
            }

            if (LastToken.IsReservedWord('>')) {
                // extended mode
                expressionToken = NextToken();
                value = Expression();
                if (value == null) {
                    ShowSyntaxError(expressionToken);
                    value = new Address(AddressType.Const, 0);
                }

                WriteByte(instruction | extendedModeBits);
                WriteWord(expressionToken, value);
                return;
            }

            if (NoOffset(instruction | indexedModeBits, 0)) return;

            if (!LastToken.IsReservedWord('[')) return;
            NextToken();
            //  indirect mode
            if (AccumulatorOffset(instruction | indexedModeBits, 0b10000)) goto exitIndirectMode;
            if (NoOffset(instruction | indexedModeBits, 0b10000)) goto exitIndirectMode;

            WriteByte(instruction | indexedModeBits);
            expressionToken = LastToken;
            value = Expression();
            if (value != null) {
                if (LastToken.IsReservedWord(',')) {
                    var token = NextToken();
                    if (ConstantOffset(value, token, 0b10000)) goto exitIndirectMode;
                    if (PcOffset(value, token, 0b10000)) goto exitIndirectMode;
                    ShowSyntaxError(LastToken);
                    goto exitIndirectMode;
                }

                // extended mode
                WriteByte(0b10011111);
                WriteWord(expressionToken, value);
            }

            exitIndirectMode:
            AcceptReservedWord(']');
        }

        private void IndexedMode(int instruction)
        {
            if (AccumulatorOffset(instruction, 0)) {
                return;
            }

            var expressionToken = LastToken;
            var value = Expression();
            if (value != null) {
                if (LastToken.IsReservedWord(',')) {
                    NextToken();
                    //  indexed mode
                    WriteByte(instruction);
                    if (ConstantOffset(value, expressionToken, 0)) return;
                    if (PcOffset(value, expressionToken, 0)) return;
                    if (LastToken.IsReservedWord(Keyword.Pc)) return;
                    ShowSyntaxError(LastToken);
                    return;
                }
            }
            if (NoOffset(instruction, 0)) return;

            if (!LastToken.IsReservedWord('[')) return;
            NextToken();
            //  indirect mode
            if (AccumulatorOffset(instruction, 0b10000)) goto exitIndirectMode;
            if (NoOffset(instruction, 0b10000)) goto exitIndirectMode;

            WriteByte(instruction);
            expressionToken = LastToken;
            value = Expression();
            if (value != null) {
                if (LastToken.IsReservedWord(',')) {
                    var token = NextToken();
                    if (ConstantOffset(value, token, 0b10000)) goto exitIndirectMode;
                    if (PcOffset(value, token, 0b10000)) goto exitIndirectMode;
                    ShowSyntaxError(LastToken);
                    goto exitIndirectMode;
                }

                // extended mode
                WriteByte(0b10011111);
                WriteWord(expressionToken, value);
            }

            exitIndirectMode:
            AcceptReservedWord(']');
        }


        private bool ConstantOffset(Address value, Token token, int indirectBit)
        {
            if (!IndexRegisterCode(out var registerCode)) return false;
            //  constant offset
            if (value.IsConst()) {
                var constOffset = value.Value;
                if (indirectBit == 0 && constOffset >= -16 && constOffset <= 15) {
                    // 5 bits
                    WriteByte(registerCode | (constOffset & 0b11111));
                    return true;
                }
                if (constOffset >= -128 && constOffset <= 127) {
                    // 8 bits
                    WriteByte(0b10001000 | registerCode | indirectBit);
                    WriteByte(constOffset);
                    return true;
                }
            }
            // 16 bits
            WriteByte(0b10001001 | registerCode | indirectBit);
            WriteWord(token, value);
            return true;
        }

        private bool PcOffset(Address value, Token token, int indirectBit)
        {
            if (LastToken.IsReservedWord(Keyword.PcR)) {
                NextToken();

                // PCR offset
                var offset = 0;
                if (value.Type == CurrentSegment.Type) {
                    const int instructionLength = 3;
                    offset = value.Value - (CurrentAddress.Value - 1 + instructionLength);
                }
                else {
                    ShowAddressUsageError(token);
                }
                if (offset >= -128 && offset <= 127) {
                    // 8 bits
                    WriteByte(0b10001100 | indirectBit);
                    WriteByte(offset);
                    return true;
                }
                // 16 bits
                --offset;
                WriteByte(0b10001101 | indirectBit);
                WriteByte(offset >> 8);
                WriteByte(offset);
                return true;
            }
            if (LastToken.IsReservedWord(Keyword.Pc)) {
                NextToken();
                // PC offset
                if (!value.IsConst()) {
                    ShowAddressUsageError(token);
                }
                var constOffset = value.Value;
                if (constOffset >= -128 && constOffset <= 127) {
                    // 8 bits
                    WriteByte(0b10001100 | indirectBit);
                    WriteByte(constOffset);
                    return true;
                }
                // 16 bits
                WriteByte(0b10001101 | indirectBit);
                WriteWord(token, value);
                return true;
            }
            return false;
        }

        private bool AccumulatorOffset(int instruction, int indirectBit)
        {
            if (!(LastToken is ReservedWord reservedWord)) return false;
            int bits;
            switch (reservedWord.Id) {
                case Keyword.A:
                    NextToken();
                    //  accumulator(A) offset 
                    bits = (0b10000110 | indirectBit);
                    break;
                case Keyword.B:
                    NextToken();
                    //  accumulator(B) offset
                    bits = (0b10000101 | indirectBit);
                    break;
                case Keyword.D:
                    NextToken();
                    //  accumulator(D) offset
                    bits = (0b10001011 | indirectBit);
                    break;
                default:
                    return false;
            }

            AcceptReservedWord(',');
            if (!IndexRegisterCode(out var registerCode)) {
                ShowSyntaxError(LastToken);
            }

            WriteByte(instruction);
            WriteByte(bits | registerCode);
            return true;

        }


        private bool NoOffset(int instruction, int indirectBit)
        {
            if (LastToken.IsReservedWord(',')) {
                NextToken();
                //  no offset
                var bits = 0b10000100 | indirectBit;
                if (LastToken.IsReservedWord('-')) {
                    NextToken();
                    if (LastToken.IsReservedWord('-')) {
                        NextToken();
                        bits = 0b10000011 | indirectBit;
                    }
                    else if (indirectBit == 0) {
                        bits = 0b10000010;
                    }
                    else {
                        ShowSyntaxError(LastToken);
                    }
                }

                if (!IndexRegisterCode(out var registerCode)) {
                    ShowSyntaxError(LastToken);
                }

                if (LastToken.IsReservedWord('+')) {
                    NextToken();
                    if (LastToken.IsReservedWord('+')) {
                        NextToken();
                        bits = 0b10000001 | indirectBit;
                    }
                    else if (indirectBit == 0) {
                        bits = 0b10000000;
                    }
                    else {
                        ShowSyntaxError(LastToken);
                    }
                }
                WriteByte(instruction);
                WriteByte(bits | registerCode);
                return true;
            }
            return false;
        }

        private bool IndexRegisterCode(out int registerCode)
        {
            registerCode = 0;
            if (!(LastToken is ReservedWord reservedWord)) return false;
            switch (reservedWord.Id)
            {
                case Keyword.X:
                    registerCode = 0b0000000;
                    break;
                case Keyword.Y:
                    registerCode = 0b0100000;
                    break;
                case Keyword.U:
                    registerCode = 0b1000000;
                    break;
                case Keyword.S:
                    registerCode = 0b1100000;
                    break;
                default:
                    return false;
            }

            NextToken();
            return true;

        }

        private void IfStatement()
        {
            NextToken();
            IfBlock block = NewIfBlock();
            StartIf(block);
        }

        private void StartIf(IfBlock block)
        {
            Address address = SymbolAddress(block.ElseId);
            ConditionalBranch(address, 1);
        }

        private void ConditionalBranch(Address address, int invertedBit)
        {
            Token token = LastToken;
            if (token is ReservedWord reservedWord) {
                int code;
                switch (reservedWord.Id) {
                    case Keyword.Hi:
                        code = 0x22;
                        break;
                    case Keyword.Ls:
                        code = 0x23;
                        break;
                    case Keyword.Cc:
                    case Keyword.Hs:
                        code = 0x24;
                        break;
                    case Keyword.Cs:
                    case Keyword.Lo:
                        code = 0x25;
                        break;
                    case Keyword.Ne:
                        code = 0x26;
                        break;
                    case Keyword.Eq:
                        code = 0x27;
                        break;
                    case Keyword.Vc:
                        code = 0x28;
                        break;
                    case Keyword.Vs:
                        code = 0x29;
                        break;
                    case Keyword.Pl:
                        code = 0x2a;
                        break;
                    case Keyword.Mi:
                        code = 0x2b;
                        break;
                    case Keyword.Ge:
                        code = 0x2c;
                        break;
                    case Keyword.Lt:
                        code = 0x2d;
                        break;
                    case Keyword.Gt:
                        code = 0x2e;
                        break;
                    case Keyword.Le:
                        code = 0x2f;
                        break;
                    default:
                        ShowSyntaxError(token);
                        return;
                }

                token = NextToken();
                var offset = RelativeOffset(address);
                if (IsRelativeOffsetInRange(offset)) {
                    // branch to else/endif
                    WriteByte(code ^ invertedBit);
                    WriteByte(offset);
                    return;
                }

                offset -= 2;
                WriteByte(0x10);
                WriteByte(code ^ invertedBit);
                WriteWord(token, new Address(AddressType.Const, offset));
            }
            else {
                ShowSyntaxError(token);
            }
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

        private void UnconditionalBranch(Address address)
        {
            var offset = RelativeOffset(address);
            if (IsRelativeOffsetInRange(offset)) {
                WriteByte(0x20);    //	BRA
                WriteByte(offset);
                return;
            }
            offset -= 1;
            WriteByte(0x16);    // LBRA
            WriteWord(LastToken, new Address(AddressType.Const, offset));
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
            if (!(LastBlock() is IfBlock block)) {
                ShowNoStatementError(LastToken, "IF");
            }
            else {
                DefineSymbol(block.ElseId <= 0 ? block.EndId : block.ConsumeElse(), CurrentAddress);
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
                ConditionalBranch(address, 0);
                block.EraseEndId();
            }
            else {
                Address address = SymbolAddress(block.EndId);
                ConditionalBranch(address, 1);
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
    }
}