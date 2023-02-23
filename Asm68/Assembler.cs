using Inu.Language;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Inu.Assembler.Mc6800
{
    internal class Assembler : BigEndianAssembler
    {
        public Assembler() : base(new Tokenizer()) { }

        protected Assembler(Inu.Assembler.Tokenizer tokenizer) : base(tokenizer) { }

        private void TowModeInstruction(byte instruction)
        {
            var token = NextToken();

            if (token.IsReservedWord('<') || token.IsReservedWord('>')) {
                //ignore
                NextToken();
            }

            var address = Expression();
            if (address == null) {
                ShowSyntaxError(token);
                return;
            }

            if (LastToken.IsReservedWord(',')) {
                //	indexed
                NextToken();
                AcceptReservedWord(Keyword.X);
                if (address.Type != AddressType.Const) { ShowAddressUsageError(LastToken); }
                int value = address.Value;
                if (value < 0 || value > 0xff) { ShowOutOfRange(LastToken, value); }
                WriteByte(instruction | 0x20);
                WriteByte(address.Value);
                return;
            }
            //	extended
            WriteByte(instruction | 0x30);
            WriteWord(LastToken, address);
        }

        private static readonly Dictionary<int, byte> TowModeElements = new Dictionary<int, byte>{
            {Keyword.Neg, 0x60},
            {Keyword.Com, 0x63},
            {Keyword.Lsr, 0x64},
            {Keyword.Ror, 0x66},
            {Keyword.Asr, 0x67},
            {Keyword.Asl, 0x68},
            {Keyword.Rol, 0x69},
            {Keyword.Dec, 0x6A},
            {Keyword.Inc, 0x6C},
            {Keyword.Tst, 0x6D},
            {Keyword.Jmp, 0x6e},
            {Keyword.Clr, 0x6f},
            {Keyword.Jsr, 0xad},
        };
        private bool TowModeInstruction()
        {
            var reservedWord = LastToken as ReservedWord;
            Debug.Assert(reservedWord != null);
            if (!TowModeElements.TryGetValue(reservedWord.Id, out var instruction)) { return false; }
            TowModeInstruction(instruction);
            return true;
        }

        private void FourModeInstruction(byte instruction, bool immediateModeAvailable, bool wordRegister)
        {
            var token = NextToken();
            Address? address;
            if (immediateModeAvailable && token.IsReservedWord('#')) {
                //  indexed mode
                token = NextToken();
                if (wordRegister) {
                    address = Expression();
                    if (address == null) {
                        ShowSyntaxError(token);
                        return;
                    }
                    WriteByte(instruction);
                    WriteWord(token, address);
                    return;
                }
                var value = Expression();
                if (value == null) {
                    ShowSyntaxError(token);
                    return;
                }
                WriteByte(instruction);
                WriteByte(token, value);
                return;
            }
            var direct = false;
            if (LastToken.IsReservedWord('<')) {
                // zero page
                direct = true;
                NextToken();
            }
            var extended = false;
            if (LastToken.IsReservedWord('>')) {
                // absolute
                extended = true;
                NextToken();
            }
            address = Expression();
            if (address == null) { ShowSyntaxError(token); return; }

            if (LastToken.IsReservedWord(',')) {
                //	indexed
                NextToken();
                AcceptReservedWord(Keyword.X);
                if (address.Type != AddressType.Const) { ShowAddressUsageError(LastToken); }
                var value = address.Value;
                if (value < 0 || value > 0xff) { ShowOutOfRange(LastToken, value); }
                WriteByte(instruction | 0x20);
                WriteByte(value);
                return;
            }
            if (direct || (!extended && address.IsByte())) {
                //	direct
                WriteByte(instruction | 0x10);
                var value = address.Low();
                Debug.Assert(value != null);
                WriteByte(LastToken, value);
                return;
            }
            //	extended
            WriteByte(instruction | 0x30);
            WriteWord(LastToken, address);
        }

        protected readonly struct FourModeElement
        {
            public readonly byte Code;
            public readonly bool ImmediateModeAvailable;
            public readonly bool WordRegister;

            public FourModeElement(byte code, bool immediateModeAvailable, bool wordRegister)
            {
                Code = code;
                ImmediateModeAvailable = immediateModeAvailable;
                WordRegister = wordRegister;
            }
        };

        protected static readonly Dictionary<int, FourModeElement> FourModeElements = new Dictionary<int, FourModeElement>{
            {Keyword.SubA, new FourModeElement(0x80,true,false)},
            {Keyword.SubB, new FourModeElement(0xC0,true,false)},
            {Keyword.CmpA, new FourModeElement(0x81,true,false)},
            {Keyword.CmpB, new FourModeElement(0xC1,true,false)},
            {Keyword.SbcA, new FourModeElement(0x82,true,false)},
            {Keyword.SbcB, new FourModeElement(0xC2,true,false)},
            {Keyword.AndA, new FourModeElement(0x84,true,false)},
            {Keyword.AndB, new FourModeElement(0xC4,true,false)},
            {Keyword.BitA, new FourModeElement(0x85,true,false)},
            {Keyword.BitB, new FourModeElement(0xC5,true,false)},
            {Keyword.LdaA, new FourModeElement(0x86,true,false)},
            {Keyword.LdaB, new FourModeElement(0xC6,true,false)},
            {Keyword.StaA, new FourModeElement(0x87,false,false)},
            {Keyword.StaB, new FourModeElement(0xC7,false,false)},
            {Keyword.EorA, new FourModeElement(0x88,true,false)},
            {Keyword.EorB, new FourModeElement(0xC8,true,false)},
            {Keyword.AdcA, new FourModeElement(0x89,true,false)},
            {Keyword.AdcB, new FourModeElement(0xC9,true,false)},
            {Keyword.OraA, new FourModeElement(0x8A,true,false)},
            {Keyword.OraB, new FourModeElement(0xCA,true,false)},
            //{Keyword.OraA, new FourModeElement(0x8A,true,false)},
            //{Keyword.OraB, new FourModeElement(0xCA,true,false)},
            {Keyword.AddA, new FourModeElement(0x8B,true,false)},
            {Keyword.AddB, new FourModeElement(0xCB,true,false)},
            {Keyword.Cpx, new FourModeElement(0x8C,true,true)},
            {Keyword.Lds, new FourModeElement(0x8E,true,true)},
            {Keyword.Ldx, new FourModeElement(0xCE,true,true)},
            {Keyword.Sts, new FourModeElement(0x8F,false,true)},
            {Keyword.Stx, new FourModeElement(0xCF,false,true)},
        };
        private bool FourModeInstruction()
        {
            var reservedWord = LastToken as ReservedWord;
            Debug.Assert(reservedWord != null);
            if (!FourModeElements.TryGetValue(reservedWord.Id, out FourModeElement element)) { return false; }
            FourModeInstruction(element.Code, element.ImmediateModeAvailable, element.WordRegister);
            return true;
        }

        protected static readonly Dictionary<int, byte> BranchElements = new Dictionary<int, byte>{
            {Keyword.Bra, 0x20},
            {Keyword.Bhi, 0x22},
            {Keyword.Bls, 0x23},
            {Keyword.Bcc, 0x24},
            {Keyword.Bcs, 0x25},
            {Keyword.Bne, 0x26},
            {Keyword.Beq, 0x27},
            {Keyword.Bvc, 0x28},
            {Keyword.Bvs, 0x29},
            {Keyword.Bpl, 0x2A},
            {Keyword.Bmi, 0x2B},
            {Keyword.Bge, 0x2C},
            {Keyword.Blt, 0x2D},
            {Keyword.Bgt, 0x2E},
            {Keyword.Ble, 0x2F},
            {Keyword.Bsr, 0x8d},
        };
        private bool BranchInstruction()
        {
            var reservedWord = LastToken as ReservedWord;
            Debug.Assert(reservedWord != null);
            var id = reservedWord.Id;
            if (!BranchElements.TryGetValue(id, out byte instruction)) { return false; }
            NextToken();

            if (RelativeOffset(out var address, out int offset)) {
                WriteByte(instruction);
                WriteByte(offset);
                return true;
            }
            switch (id) {
                case Keyword.Bra:
                    //	JMP
                    WriteByte(0x7E);
                    WriteWord(LastToken, address);
                    return true;
                case Keyword.Bsr:
                    //	JSR
                    WriteByte(0xBD);
                    WriteWord(LastToken, address);
                    return true;
            }

            // 
            WriteByte(instruction ^ 0x01);
            WriteByte(3);
            WriteByte(0x7E);    // JMP
            WriteWord(LastToken, address);
            return true;
        }

        protected static readonly Dictionary<int, byte> ImpliedElements = new Dictionary<int, byte> {
            {Keyword.Nop, 0x01},
            {Keyword.Tap, 0x06},
            {Keyword.Tpa, 0x07},
            {Keyword.Inx, 0x08},
            {Keyword.Dex, 0x09},
            {Keyword.Clv, 0x0A},
            {Keyword.Sev, 0x0B},
            {Keyword.Clc, 0x0C},
            {Keyword.Sec, 0x0D},
            {Keyword.Cli, 0x0E},
            {Keyword.Sei, 0x0F},
		    //
		    {Keyword.Sba, 0x10},
            {Keyword.Cba, 0x11},
            {Keyword.Tab, 0x16},
            {Keyword.Tba, 0x17},
            {Keyword.Daa, 0x19},
            {Keyword.Aba, 0x1B},
		    //
		    {Keyword.Tsx, 0x30},
            {Keyword.Ins, 0x31},
            {Keyword.PulA, 0x32},
            {Keyword.PulB, 0x33},
            {Keyword.Des, 0x34},
            {Keyword.Txs, 0x35},
            {Keyword.PshA, 0x36},
            {Keyword.PshB, 0x37},
            {Keyword.Rts, 0x39},
            {Keyword.Rti, 0x3B},
            {Keyword.Wai, 0x3E},
            {Keyword.Swi, 0x3F},
		    //
		    {Keyword.NegA, 0x40},
            {Keyword.ComA, 0x43},
            {Keyword.LsrA, 0x44},
            {Keyword.RorA, 0x46},
            {Keyword.AsrA, 0x47},
            {Keyword.AslA, 0x48},
            {Keyword.RolA, 0x49},
            {Keyword.DecA, 0x4A},
            {Keyword.IncA, 0x4C},
            {Keyword.TstA, 0x4D},
            {Keyword.ClrA, 0x4f},
            {Keyword.NegB, 0x50},
            {Keyword.ComB, 0x53},
            {Keyword.LsrB, 0x54},
            {Keyword.RorB, 0x56},
            {Keyword.AsrB, 0x57},
            {Keyword.AslB, 0x58},
            {Keyword.RolB, 0x59},
            {Keyword.DecB, 0x5A},
            {Keyword.IncB, 0x5C},
            {Keyword.TstB, 0x5D},
            {Keyword.ClrB, 0x5f},
        };
        private bool InstructionWithoutOperand()
        {
            var reservedWord = LastToken as ReservedWord;
            Debug.Assert(reservedWord != null);
            var id = reservedWord.Id;
            if (!ImpliedElements.TryGetValue(id, out byte instruction)) { return false; }
            NextToken();
            WriteByte(instruction);
            return true;
        }

        private static readonly Dictionary<int, byte> ConditionElements = new Dictionary<int, byte>{
            {Keyword.Hi, 0x22},
            {Keyword.Ls, 0x23},
            {Keyword.Cc, 0x24},
            {Keyword.Cs, 0x25},
            {Keyword.Ne, 0x26},
            {Keyword.Eq, 0x27},
            {Keyword.Vc, 0x28},
            {Keyword.Vs, 0x29},
            {Keyword.Pl, 0x2A},
            {Keyword.Mi, 0x2B},
            {Keyword.Ge, 0x2C},
            {Keyword.Lt, 0x2D},
            {Keyword.Gt, 0x2E},
            {Keyword.Le, 0x2F},
        };
        private void ConditionalBranch(Address address, byte invertedBits)
        {
            var token = LastToken;
            if (token is ReservedWord reservedWord) {
                if (!ConditionElements.TryGetValue(reservedWord.Id, out var instruction)) {
                    ShowSyntaxError(token);
                    return;
                }

                if (!address.IsUndefined()) {
                    int offset = RelativeOffset(address);
                    if (IsRelativeOffsetInRange(offset)) {
                        NextToken();
                        // branch to else/endif
                        WriteByte(instruction ^ invertedBits);
                        WriteByte(offset);
                        return;
                    }
                }

                NextToken();
                WriteByte(instruction ^ (invertedBits ^ 0x01));
                WriteByte(3);
                WriteByte(0x7E); // JMP
                WriteWord(LastToken, address);
            }
            else {
                ShowSyntaxError(token);
            }
        }

        private void UnconditionalBranch(Address address)
        {
            if (!address.IsUndefined()) {
                var offset = RelativeOffset(address);
                if (IsRelativeOffsetInRange(offset)) {
                    WriteByte(0x20);    //	BRA
                    WriteByte(offset);
                    return;
                }
            }
            WriteByte(0x7e);    //	JMP
            WriteWord(LastToken, address);
        }

        private void StartIf(IfBlock block)
        {
            var address = SymbolAddress(block.ElseId);
            ConditionalBranch(address, 0x01);
        }

        private void IfStatement()
        {
            NextToken();
            var block = NewIfBlock();
            StartIf(block);
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
                var address = SymbolAddress(block.EndId);
                UnconditionalBranch(address);
                DefineSymbol(block.ConsumeElse(), CurrentAddress);
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
            if (!(LastBlock() is IfBlock block)) {
                ShowNoStatementError(LastToken, "IF");
            }
            else
            {
                DefineSymbol(block.ElseId <= 0 ? block.EndId : block.ConsumeElse(), CurrentAddress);
                EndBlock();
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
            if (!(LastBlock() is WhileBlock block)) {
                ShowNoStatementError(LastToken, "WHILE");
                NextToken();
                return;
            }

            var repeatAddress = SymbolAddress(block.RepeatId);
            int offset;
            if (repeatAddress.Type == CurrentSegment.Type && (offset = RelativeOffset(repeatAddress)) <= 0 && offset >= -2) {
                var address = SymbolAddress(block.BeginId);
                ConditionalBranch(address, 0x00);
                block.EraseEndId();
            }
            else {
                var address = SymbolAddress(block.EndId);
                ConditionalBranch(address, 0x01);
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
                    var address = SymbolAddress(block.BeginId);
                    UnconditionalBranch(address);
                    DefineSymbol(block.EndId, CurrentAddress);
                }
                EndBlock();
            }
            NextToken();
        }

        private static readonly Dictionary<int, Action<Assembler>> Actions = new Dictionary<int, Action<Assembler>>{
            {Inu.Assembler.Keyword.If, assembler=>assembler.IfStatement()},
            {Inu.Assembler.Keyword.Else, assembler=>assembler.ElseStatement()},
            {Inu.Assembler.Keyword.EndIf,assembler=>assembler.EndIfStatement()},
            {Inu.Assembler.Keyword.ElseIf, assembler=>assembler.ElseIfStatement()},
            {Inu.Assembler.Keyword.Do, assembler=>assembler.DoStatement()},
            {Inu.Assembler.Keyword.While, assembler=>assembler.WhileStatement()},
            {Inu.Assembler.Keyword.WEnd, assembler=>assembler.WEndStatement()},
        };

        public override bool ZeroPageAvailable => true;

        protected override bool Instruction()
        {
            if (!(LastToken is ReservedWord reservedWord))
                return false;
            if (TowModeInstruction()) {
                return true;
            }

            if (FourModeInstruction()) {
                return true;
            }

            if (BranchInstruction()) {
                return true;
            }

            if (InstructionWithoutOperand()) {
                return true;
            }

            if (!Actions.TryGetValue(reservedWord.Id, out var action)) {
                return false;
            }

            action(this);
            return true;

        }
    }
}
