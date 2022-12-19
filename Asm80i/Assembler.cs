using Inu.Language;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using System.Runtime.Intrinsics.X86;

namespace Inu.Assembler.I8080
{
    internal class Assembler : LittleEndianAssembler
    {
        public Assembler() : base(new Tokenizer()) { }

        private static readonly int[] Registers = { Keyword.B, Keyword.C, Keyword.D, Keyword.E, Keyword.H, Keyword.L, Keyword.M, Keyword.A };
        private int? Register()
        {
            var token = LastToken;
            if (!(token is ReservedWord reservedWord))
                return null;
            var code = 0;
            foreach (var r in Registers) {
                if (r == reservedWord.Id) {
                    NextToken();
                    return code;
                }
                ++code;
            }
            return null;
        }

        private static readonly int[] RegisterPairs = { Keyword.B, Keyword.D, Keyword.H, Keyword.SP };
        private int? RegisterPair()
        {
            var token = LastToken;
            if (!(token is ReservedWord reservedWord))
                return null;
            var id = reservedWord.Id;
            if (id == Keyword.PSW) {
                id = Keyword.SP;
            }
            var code = 0;
            foreach (var r in RegisterPairs) {
                if (r == id) {
                    NextToken();
                    return code;
                }
                ++code;
            }
            return null;
        }


        private void Move()
        {
            var leftOperand = LastToken;
            var leftRegister = Register();
            if (leftRegister != null) {
                AcceptReservedWord(',');
                var rightOperand = LastToken;
                var rightRegister = Register();
                if (rightRegister != null) {
                    if (leftOperand.IsReservedWord(Keyword.M) && rightOperand.IsReservedWord(Keyword.M)) {
                        ShowInvalidRegister(rightOperand);
                    }
                    else {
                        WriteByte(0b01000000 | (leftRegister.Value << 3) | rightRegister.Value);
                    }
                    return;
                }
            }
            ShowSyntaxError();
        }

        private void MoveImmediate()
        {
            var register = Register();
            if (register != null) {
                AcceptReservedWord(',');
                var value = Expression();
                if (value != null) {
                    WriteByte(0b00000110 | (register.Value << 3));
                    WriteByte(LastToken, value);
                    return;
                }
            }
            ShowSyntaxError();
        }

        private void OperateRegisterLeft(int code)
        {
            var register = Register();
            if (register != null) {
                WriteByte(code | (register.Value << 3));
                return;
            }
            ShowSyntaxError();
        }

        private void OperateRegisterRight(int code)
        {
            var register = Register();
            if (register != null) {
                WriteByte(code | register.Value);
                return;
            }
            ShowSyntaxError();
        }

        private void OperateByte(int code)
        {
            var value = Expression();
            if (value != null) {
                WriteByte(code);
                WriteByte(LastToken, value);
                return;
            }
            ShowSyntaxError();
        }

        private void OperateWord(int code)
        {
            var operand = LastToken;
            var address = Expression();
            if (address != null) {
                WriteByte(code);
                WriteWord(operand, address);
                return;
            }
            ShowSyntaxError();
        }

        private void Restart()
        {
            var operand = LastToken;
            var value = Expression();
            if (value != null) {
                if (value.Value >= 0 && value.Value <= 7) {
                    WriteByte(0b11000111 | (value.Value << 3));
                    return;
                }
                ShowOutOfRange(operand, value.Value);
            }
            ShowSyntaxError();
        }

        private void LoadRegisterPair()
        {
            var registerOperand = LastToken;
            var register = RegisterPair();
            if (register != null) {
                if (registerOperand.IsReservedWord(Keyword.PSW)) {
                    ShowInvalidRegister(registerOperand);
                    return;
                }
                AcceptReservedWord(',');
                var operand = LastToken;
                var address = Expression();
                if (address != null) {
                    WriteByte(0b00000001 | (register.Value << 4));
                    WriteWord(operand, address);
                    return;
                }
            }
            ShowSyntaxError();
        }

        private void OperateRegisterPair(int code, Func<Token, bool> func)
        {
            var registerOperand = LastToken;
            var register = RegisterPair();
            if (register != null) {
                if (!func(registerOperand)) {
                    ShowInvalidRegister(registerOperand);
                    return;
                }
                WriteByte(code | (register.Value << 4));
                return;
            }
            ShowSyntaxError();
        }


        private static readonly int[] ConditionCodes = { Keyword.NZ, Keyword.Z, Keyword.NC, Keyword.C, Keyword.PO, Keyword.PE, Keyword.P, Keyword.M };
        private static int? ConditionCode(Token token)
        {
            if (!(token is ReservedWord reservedWord))
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

        private void ConditionalJump(Address address)
        {
            var condition = LastToken;
            var conditionCode = ConditionCode(condition);
            if (conditionCode == null)
                return;
            NextToken();
            // J cc,else
            WriteByte(0b11000010 | (conditionCode.Value << 3));
            WriteWord(LastToken, address);
        }

        private void NegatedConditionalJump(Address address)
        {
            var condition = LastToken;
            var conditionCode = ConditionCode(condition);
            if (conditionCode == null)
                return;
            NextToken();
            conditionCode ^= 1; // negate condition
            // J !cc,else
            WriteByte(0b11000010 | (conditionCode.Value << 3));
            WriteWord(LastToken, address);
        }

        private void UnconditionalJump(Address address)
        {
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
        }

        private void DoStatement()
        {
            var block = NewWhileBlock();
            DefineSymbol(block.BeginId, CurrentAddress);
            NextToken();
        }
        private void WhileStatement()
        {
            if (!(LastBlock() is WhileBlock block)) {
                ShowNoStatementError(LastToken, "WHILE");
                NextToken();
                return;
            }

            var repeatAddress = SymbolAddress(block.RepeatId);
            if (repeatAddress.Type == CurrentSegment.Type && (RelativeOffset(repeatAddress)) <= 1) {
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

        private static readonly Dictionary<int, int> SingleByteInstructions = new Dictionary<int, int>
        {
            { Keyword.HLT, 0b01110110 },
            { Keyword.RLC, 0b00000111 },
            { Keyword.RRC, 0b00001111 },
            { Keyword.RAL, 0b00010111 },
            { Keyword.RAR, 0b00011111 },
            { Keyword.RET, 0b11001001 },
            { Keyword.RC, 0b11011000 },
            { Keyword.RNC, 0b11010000 },
            { Keyword.RZ, 0b11001000 },
            { Keyword.RNZ, 0b11000000 },
            { Keyword.RP, 0b11110000 },
            { Keyword.RM, 0b11111000 },
            { Keyword.RPE, 0b11101000 },
            { Keyword.RPO, 0b11100000 },
            { Keyword.XCHG, 0b11101011 },
            { Keyword.XTHL, 0b11100011 },
            { Keyword.SPHL, 0b11111001 },
            { Keyword.PCHL, 0b11101001 },
            { Keyword.CMA, 0b00101111 },
            { Keyword.STC, 0b00110111 },
            { Keyword.CMC, 0b00111111 },
            { Keyword.DAA, 0b00100111 },
            { Keyword.EI, 0b11111011 },
            { Keyword.DI, 0b11110011 },
            { Keyword.NOP, 0 },
        };

        private static readonly Dictionary<int, Action<Assembler>> Actions = new Dictionary<int, Action<Assembler>>
        {
            { Keyword.MOV, a=>a.Move()},
            { Keyword.MVI, a=>a.MoveImmediate() },
            { Keyword.INR, a=>a.OperateRegisterLeft(0b00000100) },
            { Keyword.DCR, a=>a.OperateRegisterLeft(0b00000101) },
            { Keyword.ADD, a=>a.OperateRegisterRight(0b10000000) },
            { Keyword.ADC, a=>a.OperateRegisterRight(0b10001000) },
            { Keyword.SUB, a=>a.OperateRegisterRight(0b10010000) },
            { Keyword.SBB, a=>a.OperateRegisterRight(0b10011000) },
            { Keyword.ANA, a=>a.OperateRegisterRight(0b10100000) },
            { Keyword.XRA, a=>a.OperateRegisterRight(0b10101000) },
            { Keyword.ORA, a=>a.OperateRegisterRight(0b10110000) },
            { Keyword.CMP, a=>a.OperateRegisterRight(0b10111000) },
            { Keyword.ADI, a=>a.OperateByte(0b11000110) },
            { Keyword.ACI, a=>a.OperateByte(0b11001110) },
            { Keyword.SUI, a=>a.OperateByte(0b11010110) },
            { Keyword.SBI, a=>a.OperateByte(0b11011110) },
            { Keyword.ANI, a=>a.OperateByte(0b11100110) },
            { Keyword.XRI, a=>a.OperateByte(0b11101110) },
            { Keyword.ORI, a=>a.OperateByte(0b11110110) },
            { Keyword.CPI, a=>a.OperateByte(0b11111110) },
            { Keyword.JMP, a=>a.OperateWord(0b11000011) },
            { Keyword.JC, a=>a.OperateWord(0b11011010) },
            { Keyword.JNC, a=>a.OperateWord(0b11010010) },
            { Keyword.JZ, a=>a.OperateWord(0b11001010) },
            { Keyword.JNZ, a=>a.OperateWord(0b11000010) },
            { Keyword.JP, a=>a.OperateWord(0b11110010) },
            { Keyword.JM, a=>a.OperateWord(0b11111010) },
            { Keyword.JPE, a=>a.OperateWord(0b11101010) },
            { Keyword.JPO, a=>a.OperateWord(0b11100010) },
            { Keyword.CALL, a=>a.OperateWord(0b11001101) },
            { Keyword.CC, a=>a.OperateWord(0b11011100) },
            { Keyword.CNC, a=>a.OperateWord(0b11010100) },
            { Keyword.CZ, a=>a.OperateWord(0b11001100) },
            { Keyword.CNZ, a=>a.OperateWord(0b11000100) },
            { Keyword.CP, a=>a.OperateWord(0b11110100) },
            { Keyword.CM, a=>a.OperateWord(0b11111100) },
            { Keyword.CPE, a=>a.OperateWord(0b11101100) },
            { Keyword.CPO, a=>a.OperateWord(0b11100100) },
            { Keyword.RST, a=>a.Restart() },
            { Keyword.IN, a=>a.OperateByte(0b11011011) },
            { Keyword.OUT, a=>a.OperateByte(0b11010011) },
            { Keyword.LXI, a=>a.LoadRegisterPair() },
            { Keyword.PUSH, a=>a.OperateRegisterPair(0b11000101, t => !t.IsReservedWord(Keyword.SP))},
            { Keyword.POP, a=>a.OperateRegisterPair(0b11000001, t => !t.IsReservedWord(Keyword.SP))},
            { Keyword.STA, a=>a.OperateWord(0b00110010) },
            { Keyword.LDA, a=>a.OperateWord(0b00111010) },
            { Keyword.DAD, a=>a.OperateRegisterPair(0b00001001, t => !t.IsReservedWord(Keyword.PSW))},
            { Keyword.STAX, a=>a.OperateRegisterPair(0b00000010, t => t.IsReservedWord(Keyword.B)||t.IsReservedWord(Keyword.D))},
            { Keyword.LDAX, a=>a.OperateRegisterPair(0b00001010, t => t.IsReservedWord(Keyword.B)||t.IsReservedWord(Keyword.D))},
            { Keyword.INX, a=>a.OperateRegisterPair(0b00000011, t => !t.IsReservedWord(Keyword.PSW))},
            { Keyword.DCX, a=>a.OperateRegisterPair(0b00001011, t => !t.IsReservedWord(Keyword.PSW))},
            { Keyword.SHLD, a=>a.OperateWord(0b00100010) },
            { Keyword.LHLD, a=>a.OperateWord(0b00101010 ) },
            //
            {Keyword.If, (Assembler a)=>{a.IfStatement(); }},
            {Keyword.Else, (Assembler a)=>{a.ElseStatement(); }},
            {Keyword.EndIf,(Assembler a)=>{a.EndIfStatement(); }},
            {Keyword.ElseIf, (Assembler a)=>{a.ElseIfStatement(); }},
            {Keyword.Do, (Assembler a)=>{a.DoStatement(); }},
            {Keyword.While, (Assembler a)=>{a.WhileStatement(); }},
            {Keyword.WEnd, (Assembler a)=>{a.WEndStatement(); }},
        };

        protected override bool Instruction()
        {
            var reservedWord = LastToken as ReservedWord;
            Debug.Assert(reservedWord != null);

            if (SingleByteInstructions.TryGetValue(reservedWord.Id, out var code)) {
                NextToken();
                WriteByte(code);
                return true;
            }
            if (Actions.TryGetValue(reservedWord.Id, out var action)) {
                NextToken();
                action(this);
                return true;
            }
            return false;
        }
    }
}
