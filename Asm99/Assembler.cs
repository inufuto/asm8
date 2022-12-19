using System;
using System.Collections.Generic;
using System.Diagnostics;
using Inu.Language;

namespace Inu.Assembler.Tms99
{
    internal class Assembler : BigEndianAssembler
    {
        private enum AddressingMode
        {
            Register, Indirect, SymbolicOrIndexed, IndirectAutoIncrement
        }

        public Assembler() : base(new Tokenizer()) { }

        private void WriteWord(int value)
        {
            WriteByte((value >> 8) & 0xff);
            WriteByte(value & 0xff);
        }

        protected override bool Instruction()
        {
            Dictionary<int, Action<Assembler>> Actions = new Dictionary<int, Action<Assembler>>
            {
                {Inu.Assembler.Keyword.If, assembler=>assembler.IfStatement()},
                {Inu.Assembler.Keyword.Else, assembler=>assembler.ElseStatement()},
                {Inu.Assembler.Keyword.EndIf,assembler=>assembler.EndIfStatement()},
                {Inu.Assembler.Keyword.ElseIf, assembler=>assembler.ElseIfStatement()},
                {Inu.Assembler.Keyword.Do, assembler=>assembler.DoStatement()},
                {Inu.Assembler.Keyword.While, assembler=>assembler.WhileStatement()},
                {Inu.Assembler.Keyword.WEnd, assembler=>assembler.WEndStatement()},
            };


            if (!(LastToken is ReservedWord reservedWord)) {
                return false;
            }

            if (DualOperandInstructionWithMultipleAddressingForSourceAndDestinationOperand()) {
                return true;
            }
            if (DualOperandInstructionWithMultipleAddressingForSourceOperand()) {
                return true;
            }
            if (XopOrCruInstruction()) {
                return true;
            }
            if (SingleOperandInstruction()) {
                return true;
            }
            if (JumpInstruction()) {
                return true;
            }
            if (ShiftInstruction()) {
                return true;
            }
            if (ImmediateRegisterInstruction()) {
                return true;
            }
            if (InternalRegisterLoadImmediateInstruction()) {
                return true;
            }
            if (InternalRegisterLoadOrStoreInstruction()) {
                return true;
            }
            if (OperandlessInstruction()) {
                return true;
            }

            if (!Actions.TryGetValue(reservedWord.Id, out var action)) {
                return false;
            }
            action(this);
            return true;
        }

        private bool DualOperandInstructionWithMultipleAddressingForSourceAndDestinationOperand()
        {
            Dictionary<int, int> instructionCodes = new Dictionary<int, int>()
            {
                { Keyword.A, 0xA000  },
                { Keyword.Ab, 0xB000  },
                { Keyword.C, 0x8000  },
                { Keyword.Cb, 0x9000  },
                { Keyword.Mov, 0xC000  },
                { Keyword.Movb, 0xD000  },
                { Keyword.S, 0x6000  },
                { Keyword.Sb, 0x7000  },
                { Keyword.Soc, 0xE000  },
                { Keyword.Socb, 0xF000  },
                { Keyword.Szc, 0x4000  },
                { Keyword.Szcb, 0x5000  },
            };

            var reservedWord = LastToken as ReservedWord;
            Debug.Assert(reservedWord != null);
            if (!instructionCodes.TryGetValue(reservedWord.Id, out var instruction)) { return false; }
            NextToken();

            var sourceAddressing = MultipleAddressingOperand(out var sourceRegisterCode, out var sourceAddress);
            AcceptReservedWord(',');
            var destinationAddressing = MultipleAddressingOperand(out var destinationRegisterCode, out var destinationAddress);
            WriteWord(
                (instruction |
                ((int)destinationAddressing << 10) |
                (destinationRegisterCode << 6) |
                ((int)sourceAddressing << 4) |
                sourceRegisterCode)
            );
            if (sourceAddressing == AddressingMode.SymbolicOrIndexed) {
                Debug.Assert(sourceAddress != null);
                WriteWord(LastToken, sourceAddress);
            }
            if (destinationAddressing == AddressingMode.SymbolicOrIndexed) {
                Debug.Assert(destinationAddress != null);
                WriteWord(LastToken, destinationAddress);
            }
            return true;
        }

        private bool DualOperandInstructionWithMultipleAddressingForSourceOperand()
        {
            Dictionary<int, int> instructionCodes = new Dictionary<int, int>()
            {
                { Keyword.Coc, 0x2000    },
                { Keyword.Czc, 0x2400    },
                { Keyword.Div, 0x3C00    },
                { Keyword.Mpy, 0x3800    },
                { Inu.Assembler.Keyword.Xor, 0x2800    },
            };

            var reservedWord = LastToken as ReservedWord;
            Debug.Assert(reservedWord != null);
            if (!instructionCodes.TryGetValue(reservedWord.Id, out var instruction)) { return false; }
            NextToken();

            var sourceAddressing = MultipleAddressingOperand(out var sourceRegisterCode, out var sourceAddress);
            AcceptReservedWord(',');
            var destinationRegisterCode = AcceptRegister();

            WriteWord(
                (instruction |
                 (destinationRegisterCode << 6) |
                 ((int)sourceAddressing << 4) |
                 sourceRegisterCode)
            );
            if (sourceAddressing == AddressingMode.SymbolicOrIndexed) {
                Debug.Assert(sourceAddress != null);
                WriteWord(LastToken, sourceAddress);
            }

            return true;
        }

        private bool XopOrCruInstruction()
        {
            Dictionary<int, int> instructionCodes = new Dictionary<int, int>()
            {
                { Keyword.Xop, 0x2C00 },
                { Keyword.Ldcr, 0x3000 },
                { Keyword.Stcr, 0x3400 },
            };

            var reservedWord = LastToken as ReservedWord;
            Debug.Assert(reservedWord != null);
            if (!instructionCodes.TryGetValue(reservedWord.Id, out var instruction)) { return false; }
            NextToken();

            var sourceAddressing = MultipleAddressingOperand(out var sourceRegisterCode, out var sourceAddress);
            AcceptReservedWord(',');

            var token = LastToken;
            var value = AcceptConstant();
            if (value <= 0x00 || value > 0x0f) {
                ShowOutOfRange(token, value);
            }
            WriteWord(
                (instruction |
                 (value << 6) |
                 ((int)sourceAddressing << 4) |
                 sourceRegisterCode)
            );

            return true;
        }

        private bool SingleOperandInstruction()
        {
            var instructionCodes = new Dictionary<int, int>()
            {
                { Keyword.Abs, 0x0740 },
                { Keyword.B, 0x0440 },
                { Keyword.Bl, 0x0680 },
                { Keyword.Blwp, 0x0400 },
                { Keyword.Clr, 0x04C0 },
                { Keyword.Dec, 0x0600 },
                { Keyword.Dect, 0x0640 },
                { Keyword.Inc, 0x0580 },
                { Keyword.Inct, 0x05C0 },
                { Keyword.Inv, 0x0540 },
                { Keyword.Neg, 0x0500 },
                { Keyword.Seto, 0x0700 },
                { Keyword.Swpb, 0x06C0 },
                { Keyword.X, 0x0480 },
            };

            var reservedWord = LastToken as ReservedWord;
            Debug.Assert(reservedWord != null);
            if (!instructionCodes.TryGetValue(reservedWord.Id, out var instruction)) { return false; }
            NextToken();

            var addressing = MultipleAddressingOperand(out var registerCode, out var address);

            WriteWord(
                (instruction |
                 ((int)addressing << 4) |
                 registerCode)
            );
            if (addressing == AddressingMode.SymbolicOrIndexed) {
                Debug.Assert(address != null);
                WriteWord(LastToken, address);
            }

            return true;
        }

        private bool JumpInstruction()
        {
            var instructionCodes = new Dictionary<int, int>()
            {
                { Keyword.Jeq, 0x1300 },
                { Keyword.Jgt, 0x1500 },
                { Keyword.Jh, 0x1B00 },
                { Keyword.Jhe, 0x1400 },
                { Keyword.Jl, 0x1A00 },
                { Keyword.Jle, 0x1200 },
                { Keyword.Jlt, 0x1100 },
                { Keyword.Jmp, 0x1000 },
                { Keyword.Jnc, 0x1700 },
                { Keyword.Jne, 0x1600 },
                { Keyword.Jno, 0x1900 },
                { Keyword.Joc, 0x1800 },
                { Keyword.Jop, 0x1C00 },
                { Keyword.Sbo, 0x1D00 },
                { Keyword.Sbz, 0x1E00 },
                { Keyword.Tb, 0x1F00 },
            };
            var reverseConditions = new Dictionary<int, int>()
            {
                { Keyword.Jeq,  Keyword.Jne  },
                { Keyword.Jh,  Keyword.Jle  },
                { Keyword.Jhe,  Keyword.Jl  },
                { Keyword.Jl,  Keyword.Jhe  },
                { Keyword.Jle,  Keyword.Jh  },
                { Keyword.Jnc,  Keyword.Joc  },
                { Keyword.Jne,  Keyword.Jeq  },
                { Keyword.Joc,  Keyword.Jnc  },
            };

            var reservedWord = LastToken as ReservedWord;
            Debug.Assert(reservedWord != null);
            if (!instructionCodes.TryGetValue(reservedWord.Id, out var instruction)) { return false; }
            NextToken();

            if (RelativeOffset(out var address, out var offset)) {
                WriteWord(
                    instruction |
                    ((offset >> 1) & 0xff)
                );
                return true;
            }

            if (address.IsUndefined()) return true;

            void WriteJump()
            {
                WriteWord(
                    (0x0440 |
                     ((int)AddressingMode.SymbolicOrIndexed << 4))
                );
                WriteWord(LastToken, address);
            }

            // Out of range
            if (instruction == 0x1000) {
                WriteJump();
                return true;
            }
            if (reverseConditions.TryGetValue(reservedWord.Id, out var reverseKeyword)) {
                var reversedInstruction = instructionCodes[reverseKeyword];
                WriteWord(reversedInstruction | 2);
                WriteJump();
                return true;
            }
            WriteWord(instruction | 1);
            WriteWord(0x1000 | 2);
            WriteJump();
            return true;
        }

        public override bool ZeroPageAvailable => true;

        protected override bool IsRelativeOffsetInRange(int offset)
        {
            return (offset & 1) == 0 && offset >= -256 && offset < 256;
        }

        private bool ShiftInstruction()
        {
            Dictionary<int, int> instructionCodes = new Dictionary<int, int>()
            {
                { Keyword.Sla, 0x0A00 },
                { Keyword.Sra, 0x0800 },
                { Keyword.Src, 0x0B00 },
                { Keyword.Srl, 0x0900 },
            };

            var reservedWord = LastToken as ReservedWord;
            Debug.Assert(reservedWord != null);
            if (!instructionCodes.TryGetValue(reservedWord.Id, out var instruction)) { return false; }
            NextToken();

            var registerCode = AcceptRegister();
            AcceptReservedWord(',');

            int value;
            var token = LastToken;
            if (token.IsReservedWord(Keyword.R0)) {
                NextToken();
                value = 0;
            }
            else {
                value = AcceptConstant();
                if (value <= 0x00 || value > 0x0f) {
                    ShowOutOfRange(token, value);
                }
            }
            WriteWord(
                (instruction |
                 (value << 4) |
                 registerCode)
            );
            return true;
        }

        private bool ImmediateRegisterInstruction()
        {
            Dictionary<int, int> instructionCodes = new Dictionary<int, int>()
            {
                { Keyword.Ai, 0x0220 },
                { Keyword.Andi, 0x0240 },
                { Keyword.Ci, 0x0280 },
                { Keyword.Li, 0x0200 },
                { Keyword.Ori, 0x0260 },
            };

            var reservedWord = LastToken as ReservedWord;
            Debug.Assert(reservedWord != null);
            if (!instructionCodes.TryGetValue(reservedWord.Id, out var instruction)) { return false; }
            NextToken();

            var registerCode = AcceptRegister();
            AcceptReservedWord(',');

            var address = AcceptExpression();

            WriteWord(
                (instruction |
                 registerCode)
            );
            WriteWord(LastToken, address);
            return true;
        }

        private bool InternalRegisterLoadImmediateInstruction()
        {
            Dictionary<int, int> instructionCodes = new Dictionary<int, int>()
            {
                { Keyword.Limi, 0x0300 },
                { Keyword.Lwpi, 0x02E0 },
            };

            var reservedWord = LastToken as ReservedWord;
            Debug.Assert(reservedWord != null);
            if (!instructionCodes.TryGetValue(reservedWord.Id, out var instruction)) { return false; }
            NextToken();

            var address = AcceptExpression();

            WriteWord(instruction);
            WriteWord(LastToken, address);
            return true;
        }

        private bool InternalRegisterLoadOrStoreInstruction()
        {
            Dictionary<int, int> instructionCodes = new Dictionary<int, int>()
            {
                { Keyword.Stst, 0x02C0 },
                { Keyword.Stwp, 0x02A0 },
            };

            var reservedWord = LastToken as ReservedWord;
            Debug.Assert(reservedWord != null);
            if (!instructionCodes.TryGetValue(reservedWord.Id, out var instruction)) { return false; }
            NextToken();

            var registerCode = AcceptRegister();

            WriteWord(
                (instruction |
                 registerCode)
            );
            return true;
        }

        private bool OperandlessInstruction()
        {
            Dictionary<int, int> instructionCodes = new Dictionary<int, int>()
            {
                { Keyword.Ckof, 0x03C0 },
                { Keyword.Ckon, 0x03A0 },
                { Keyword.Idle, 0x0340 },
                { Keyword.Lrex, 0x03E0 },
                { Keyword.Rset, 0x0360 },
                { Keyword.Rtwp, 0x0380 },
                { Keyword.Nop, 0x1000 },
                { Keyword.Rt, 0x045B },
            };

            var reservedWord = LastToken as ReservedWord;
            Debug.Assert(reservedWord != null);
            if (!instructionCodes.TryGetValue(reservedWord.Id, out var instruction)) { return false; }
            NextToken();

            WriteWord(instruction);
            return true;
        }


        private AddressingMode MultipleAddressingOperand(out int registerCode, out Address? address)
        {
            registerCode = 0;
            address = null;
            if (LastToken.IsReservedWord('*')) {
                //indirect
                NextToken();
                registerCode = AcceptRegister();
                if (!LastToken.IsReservedWord('+'))
                    return AddressingMode.Indirect;
                NextToken();
                return AddressingMode.IndirectAutoIncrement;
            }
            if (LastToken.IsReservedWord('@')) {
                NextToken();
                address = AcceptExpression();

                if (!LastToken.IsReservedWord('('))
                    return AddressingMode.SymbolicOrIndexed;
                NextToken();
                var token = LastToken;
                registerCode = AcceptRegister();
                if (registerCode == 0) {
                    ShowError(token.Position, "InvalidRegister.");
                }
                AcceptReservedWord(')');
                return AddressingMode.SymbolicOrIndexed;
            }
            registerCode = AcceptRegister();
            return AddressingMode.Register;
        }

        private int AcceptRegister()
        {
            var r = RegisterCode();
            if (r != null) {
                NextToken();
                return r.Value;
            }
            ShowSyntaxError(LastToken);
            return 0;
        }

        private int? RegisterCode()
        {
            int[] registers = new int[]
            {
                Keyword.R0,Keyword.R1,Keyword.R2,Keyword.R3,Keyword.R4,Keyword.R5,Keyword.R6,Keyword.R7,
                Keyword.R8,Keyword.R9,Keyword.R10,Keyword.R11,Keyword.R12,Keyword.R13,Keyword.R14,Keyword.R15
            };

            var registerCode = 0;
            foreach (var id in registers) {
                if (LastToken.IsReservedWord(id)) {
                    return registerCode;
                }
                ++registerCode;
            }
            return null;
        }

        private Address AcceptExpression()
        {
            var address = Expression();
            if (address != null)
                return address;
            ShowSyntaxError(LastToken);
            return new Address(AddressType.Const, 0);
        }

        private int AcceptConstant()
        {
            var token = LastToken;
            var address = AcceptExpression();
            if (address.IsConst())
                return address.Value;
            ShowError(token.Position, "Must be constant: " + token);
            return 0;
        }

        private readonly struct JumpElement
        {
            public readonly int[] trueInstructions;
            public readonly int[] falseInstructions;

            public JumpElement(int[] trueInstructions, int[] falseInstructions)
            {
                this.trueInstructions = trueInstructions;
                this.falseInstructions = falseInstructions;
            }

            public JumpElement(int trueInstruction, int falseInstruction)
                : this(new int[] { trueInstruction }, new int[] { falseInstruction })
            { }

            public JumpElement(int trueInstruction, int[] falseInstructions)
                : this(new[] { trueInstruction }, falseInstructions)
            { }

            public JumpElement(int[] trueInstructions, int falseInstruction)
                : this(trueInstructions, new[] { falseInstruction })
            { }
        }

        private void ConditionalJump(Address address, bool not)
        {
            var conditionElements = new Dictionary<int, JumpElement>{
                {
                    Keyword.Eq,
                    new JumpElement(0x1300, 0x1600)
                },
                {
                    Keyword.Gt,
                    new JumpElement(0x1500, new[]{ 0x1100, 0x1300 })
                },
                {
                    Keyword.Ngt,
                    new JumpElement( new[]{ 0x1100, 0x1300 },0x1500)
                },
                {
                    Keyword.H,
                    new JumpElement(0x1B00, 0x1200)
                },
                {
                    Keyword.He,
                    new JumpElement(0x1400, 0x1A00)
                },
                {
                    Keyword.L,
                    new JumpElement(0x1A00,0x1400)
                },
                {
                    Keyword.Le,
                    new JumpElement(0x1200, 0x1B00)
                },
                {
                    Keyword.Lt,
                    new JumpElement(0x1100,new []{ 0x1500, 0x1300 })
                },
                {
                    Keyword.NLt,
                    new JumpElement(new []{ 0x1500, 0x1300 },0x1100)
                },
                {
                    Keyword.Nc,
                    new JumpElement(0x1700, 0x1800)
                },
                {
                    Keyword.Ne,
                    new JumpElement(0x1600, 0x1300)
                },
                {
                    Keyword.No,
                    new JumpElement(0x1900, new int[]{})
                },
                {
                    Keyword.Oc,
                    new JumpElement(0x1800  ,0x1700)
                },
                {
                    Keyword.Op,
                    new JumpElement(0x1C00, new int[]{})
                },
            };

            var token = LastToken;
            if (token is ReservedWord reservedWord) {
                if (!conditionElements.TryGetValue(reservedWord.Id, out var element)) {
                    ShowSyntaxError(token);
                    return;
                }
                NextToken();
                int[] trueInstructions, falseInstructions;
                if (not) {
                    trueInstructions = element.falseInstructions;
                    falseInstructions = element.trueInstructions;
                }
                else {
                    trueInstructions = element.trueInstructions;
                    falseInstructions = element.falseInstructions;
                }

                if (trueInstructions.Length > 0 && !address.IsUndefined()) {
                    var offset = RelativeOffset(address);
                    if (
                        IsRelativeOffsetInRange(offset)
                        && IsRelativeOffsetInRange(offset - (trueInstructions.Length - 1) * 2)
                    ) {
                        foreach (var instruction in trueInstructions) {
                            WriteWord(
                                instruction |
                                (offset >> 1) & 0xff
                            );
                            offset -= 2;
                        }
                        return;
                    }
                }

                if (falseInstructions.Length > 0) {
                    var offset = 4 + (falseInstructions.Length) * 2;
                    foreach (var instruction in falseInstructions) {
                        offset -= 2;
                        WriteWord(
                            instruction |
                            (offset >> 1) & 0xff
                        );
                    }
                    AbsoluteJump(address);
                }
                else {
                    var offset = 4 + (trueInstructions.Length) * 2;
                    foreach (var instruction in trueInstructions) {
                        WriteWord(
                            instruction |
                            (offset >> 1) & 0xff
                        );
                        offset -= 2;
                    }
                    WriteWord(
                        0x1000 | // jmp
                        2
                    );
                    AbsoluteJump(address);
                }
            }
            else {
                ShowSyntaxError(token);
            }
        }



        private void UnconditionalJump(Address address)
        {
            if (!address.IsUndefined()) {
                var offset = RelativeOffset(address);
                if (IsRelativeOffsetInRange(offset)) {
                    WriteWord(
                        0x1000 | // jmp
                        (offset >> 1) & 0xff
                    );
                    return;
                }
            }
            AbsoluteJump(address);
        }

        private void AbsoluteJump(Address address)
        {
            WriteWord(
                0x0440 | // b
                ((int)AddressingMode.SymbolicOrIndexed) << 4
            );
            WriteWord(LastToken, address);
        }

        private void StartIf(IfBlock block)
        {
            Address address = SymbolAddress(block.ElseId);
            ConditionalJump(address, true);
        }

        private void IfStatement()
        {
            NextToken();
            IfBlock block = NewIfBlock();
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
                Address address = SymbolAddress(block.EndId);
                UnconditionalJump(address);
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
            else {
                if (block.ElseId <= 0) {
                    DefineSymbol(block.EndId, CurrentAddress);
                }
                else {
                    DefineSymbol(block.ConsumeElse(), CurrentAddress);
                }
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
                ConditionalJump(address, false);
                block.EraseEndId();
            }
            else {
                Address address = SymbolAddress(block.EndId);
                ConditionalJump(address, true);
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
    }
}
