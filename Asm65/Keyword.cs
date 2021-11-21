using System.Collections.Generic;

namespace Inu.Assembler.Mos6502
{
    internal class Keyword : Inu.Assembler.Keyword
    {
        public new const int MinId = NextId;

        public const int Cc = MinId + 0;
        public const int Cs = MinId + 1;
        public const int Eq = MinId + 2;
        public const int Mi = MinId + 3;
        public const int Ne = MinId + 4;
        public const int Pl = MinId + 5;
        public const int Vc = MinId + 6;
        public const int Vs = MinId + 7;
        public const int A = MinId + 8;
        public const int Adc = MinId + 9;
        public const int Asl = MinId + 10;
        public const int Bcc = MinId + 11;
        public const int Bcs = MinId + 12;
        public const int Beq = MinId + 13;
        public const int Bit = MinId + 14;
        public const int Bmi = MinId + 15;
        public const int Bne = MinId + 16;
        public const int Bpl = MinId + 17;
        public const int Brk = MinId + 18;
        public const int Bvc = MinId + 19;
        public const int Bvs = MinId + 20;
        public const int Clc = MinId + 21;
        public const int Cld = MinId + 22;
        public const int Cli = MinId + 23;
        public const int Clv = MinId + 24;
        public const int Cmp = MinId + 25;
        public const int Cpx = MinId + 26;
        public const int Cpy = MinId + 27;
        public const int Daa = MinId + 28;
        public const int Dec = MinId + 29;
        public const int Dex = MinId + 30;
        public const int Dey = MinId + 31;
        public const int Eor = MinId + 32;
        public const int Inc = MinId + 33;
        public const int Inx = MinId + 34;
        public const int Iny = MinId + 35;
        public const int Jmp = MinId + 36;
        public const int Jsr = MinId + 37;
        public const int Lda = MinId + 38;
        public const int Ldx = MinId + 39;
        public const int Ldy = MinId + 40;
        public const int Lsr = MinId + 41;
        public const int Nop = MinId + 42;
        public const int Ora = MinId + 43;
        public const int PhA = MinId + 44;
        public const int Php = MinId + 45;
        public const int Pla = MinId + 46;
        public const int Plp = MinId + 47;
        public const int Rol = MinId + 48;
        public const int Ror = MinId + 49;
        public const int Rti = MinId + 50;
        public const int Rts = MinId + 51;
        public const int Sbc = MinId + 52;
        public const int Sec = MinId + 53;
        public const int Sed = MinId + 54;
        public const int Sei = MinId + 55;
        public const int Sta = MinId + 56;
        public const int Stx = MinId + 57;
        public const int Sty = MinId + 58;
        public const int Tax = MinId + 59;
        public const int Tay = MinId + 60;
        public const int Tsx = MinId + 61;
        public const int Txa = MinId + 62;
        public const int Txs = MinId + 63;
        public const int Tya = MinId + 64;
        public const int X = MinId + 65;
        public const int Y = MinId + 66;

        public new static readonly Dictionary<int, string> Words = new Dictionary<int, string>()
        {
            { Cc,"CC"},
            { Cs,"CS"},
            { Eq,"EQ"},
            { Mi,"MI"},
            { Ne,"NE"},
            { Pl,"PL"},
            { Vc,"VC"},
            { Vs,"VS"},
            { A,"A"},
            { Adc,"ADC"},
            { Asl,"ASL"},
            { Bcc,"BCC"},
            { Bcs,"BCS"},
            { Beq,"BEQ"},
            { Bit,"BIT"},
            { Bmi,"BMI"},
            { Bne,"BNE"},
            { Bpl,"BPL"},
            { Brk,"BRK"},
            { Bvc,"BVC"},
            { Bvs,"BVS"},
            { Clc,"CLC"},
            { Cld,"CLD"},
            { Cli,"CLI"},
            { Clv,"CLV"},
            { Cmp,"CMP"},
            { Cpx,"CPX"},
            { Cpy,"CPY"},
            { Daa,"DAA"},
            { Dec,"DEC"},
            { Dex,"DEX"},
            { Dey,"DEY"},
            { Eor,"EOR"},
            { Inc,"INC"},
            { Inx,"INX"},
            { Iny,"INY"},
            { Jmp,"JMP"},
            { Jsr,"JSR"},
            { Lda,"LDA"},
            { Ldx,"LDX"},
            { Ldy,"LDY"},
            { Lsr,"LSR"},
            { Nop,"NOP"},
            { Ora,"ORA"},
            { PhA,"PHA"},
            { Php,"PHP"},
            { Pla,"PLA"},
            { Plp,"PLP"},
            { Rol,"ROL"},
            { Ror,"ROR"},
            { Rti,"RTI"},
            { Rts,"RTS"},
            { Sbc,"SBC"},
            { Sec,"SEC"},
            { Sed,"SED"},
            { Sei,"SEI"},
            { Sta,"STA"},
            { Stx,"STX"},
            { Sty,"STY"},
            { Tax,"TAX"},
            { Tay,"TAY"},
            { Tsx,"TSX"},
            { Txa,"TXA"},
            { Txs,"TXS"},
            { Tya,"TYA"},
            { X,"X"},
            { Y,"Y"},
        };
    }
}
