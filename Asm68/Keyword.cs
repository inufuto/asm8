using System.Collections.Generic;
using Inu.Language;

namespace Inu.Assembler.Mc6800
{
    class Keyword : Inu.Assembler.Keyword
    {
        public new const int MinId = Inu.Assembler.Keyword.NextId;

        public const int Cc = MinId + 0;
        public const int Cs = MinId + 1;
        public const int Eq = MinId + 2;
        public const int Ge = MinId + 3;
        public const int Gt = MinId + 4;
        public const int Hi = MinId + 5;
        public const int Le = MinId + 6;
        public const int Ls = MinId + 7;
        public const int Lt = MinId + 8;
        public const int Mi = MinId + 9;
        public const int Ne = MinId + 10;
        public const int Pl = MinId + 11;
        public const int Vc = MinId + 12;
        public const int Vs = MinId + 13;
        public const int Aba = MinId + 14;
        public const int AdcA = MinId + 15;
        public const int AdcB = MinId + 16;
        public const int AddA = MinId + 17;
        public const int AddB = MinId + 18;
        public const int AndA = MinId + 19;
        public const int AndB = MinId + 20;
        public const int Asl = MinId + 21;
        public const int AslA = MinId + 22;
        public const int AslB = MinId + 23;
        public const int Asr = MinId + 24;
        public const int AsrA = MinId + 25;
        public const int AsrB = MinId + 26;
        public const int Bcc = MinId + 27;
        public const int Bcs = MinId + 28;
        public const int Beq = MinId + 29;
        public const int Bge = MinId + 30;
        public const int Bgt = MinId + 31;
        public const int Bhi = MinId + 32;
        public const int BitA = MinId + 33;
        public const int BitB = MinId + 34;
        public const int Ble = MinId + 35;
        public const int Bls = MinId + 36;
        public const int Blt = MinId + 37;
        public const int Bmi = MinId + 38;
        public const int Bne = MinId + 39;
        public const int Bpl = MinId + 40;
        public const int Bra = MinId + 41;
        public const int Bsr = MinId + 42;
        public const int Bvc = MinId + 43;
        public const int Bvs = MinId + 44;
        public const int Cba = MinId + 45;
        public const int Clc = MinId + 46;
        public const int Cli = MinId + 47;
        public const int Clr = MinId + 48;
        public const int ClrA = MinId + 49;
        public const int ClrB = MinId + 50;
        public const int Clv = MinId + 51;
        public const int CmpA = MinId + 52;
        public const int CmpB = MinId + 53;
        public const int Com = MinId + 54;
        public const int ComA = MinId + 55;
        public const int ComB = MinId + 56;
        public const int Cpx = MinId + 57;
        public const int Daa = MinId + 58;
        public const int Dec = MinId + 59;
        public const int DecA = MinId + 60;
        public const int DecB = MinId + 61;
        public const int Des = MinId + 62;
        public const int Dex = MinId + 63;
        public const int EorA = MinId + 64;
        public const int EorB = MinId + 65;
        public const int Inc = MinId + 66;
        public const int IncA = MinId + 67;
        public const int IncB = MinId + 68;
        public const int Ins = MinId + 69;
        public const int Inx = MinId + 70;
        public const int Jmp = MinId + 71;
        public const int Jsr = MinId + 72;
        public const int LdaA = MinId + 73;
        public const int LdaB = MinId + 74;
        public const int Lds = MinId + 75;
        public const int Ldx = MinId + 76;
        public const int Lsr = MinId + 77;
        public const int LsrA = MinId + 78;
        public const int LsrB = MinId + 79;
        public const int Neg = MinId + 80;
        public const int NegA = MinId + 81;
        public const int NegB = MinId + 82;
        public const int Nop = MinId + 83;
        public const int OraA = MinId + 84;
        public const int OraB = MinId + 85;
        public const int PshA = MinId + 86;
        public const int PshB = MinId + 87;
        public const int PulA = MinId + 88;
        public const int PulB = MinId + 89;
        public const int Rol = MinId + 90;
        public const int RolA = MinId + 91;
        public const int RolB = MinId + 92;
        public const int Ror = MinId + 93;
        public const int RorA = MinId + 94;
        public const int RorB = MinId + 95;
        public const int Rti = MinId + 96;
        public const int Rts = MinId + 97;
        public const int Sba = MinId + 98;
        public const int SbcA = MinId + 99;
        public const int SbcB = MinId + 100;
        public const int Sec = MinId + 101;
        public const int Sei = MinId + 102;
        public const int Sev = MinId + 103;
        public const int StaA = MinId + 104;
        public const int StaB = MinId + 105;
        public const int Sts = MinId + 106;
        public const int Stx = MinId + 107;
        public const int SubA = MinId + 108;
        public const int SubB = MinId + 109;
        public const int Swi = MinId + 110;
        public const int Tab = MinId + 111;
        public const int Tap = MinId + 112;
        public const int Tba = MinId + 113;
        public const int Tpa = MinId + 114;
        public const int Tst = MinId + 115;
        public const int TstA = MinId + 116;
        public const int TstB = MinId + 117;
        public const int Tsx = MinId + 118;
        public const int Txs = MinId + 119;
        public const int Wai = MinId + 120;
        public const int X = MinId + 121;

        protected new const int NextId = MinId + 122;


        public new static readonly Dictionary<int, string> Words = new Dictionary<int, string>()
        {
            { Cc,"CC"},
            { Cs,"CS"},
            { Eq,"EQ"},
            { Ge,"GE"},
            { Gt,"GT"},
            { Hi,"HI"},
            { Le,"LE"},
            { Ls,"LS"},
            { Lt,"LT"},
            { Mi,"MI"},
            { Ne,"NE"},
            { Pl,"PL"},
            { Vc,"VC"},
            { Vs,"VS"},
            { Aba,"ABA"},
            { AdcA,"ADCA"},
            { AdcB,"ADCB"},
            { AddA,"ADDA"},
            { AddB,"ADDB"},
            { AndA,"ANDA"},
            { AndB,"ANDB"},
            { Asl,"ASL"},
            { AslA,"ASLA"},
            { AslB,"ASLB"},
            { Asr,"ASR"},
            { AsrA,"ASRA"},
            { AsrB,"ASRB"},
            { Bcc,"BCC"},
            { Bcs,"BCS"},
            { Beq,"BEQ"},
            { Bge,"BGE"},
            { Bgt,"BGT"},
            { Bhi,"BHI"},
            { BitA,"BITA"},
            { BitB,"BITB"},
            { Ble,"BLE"},
            { Bls,"BLS"},
            { Blt,"BLT"},
            { Bmi,"BMI"},
            { Bne,"BNE"},
            { Bpl,"BPL"},
            { Bra,"BRA"},
            { Bsr,"BSR"},
            { Bvc,"BVC"},
            { Bvs,"BVS"},
            { Cba,"CBA"},
            { Clc,"CLC"},
            { Cli,"CLI"},
            { Clr,"CLR"},
            { ClrA,"CLRA"},
            { ClrB,"CLRB"},
            { Clv,"CLV"},
            { CmpA,"CMPA"},
            { CmpB,"CMPB"},
            { Com,"COM"},
            { ComA,"COMA"},
            { ComB,"COMB"},
            { Cpx,"CPX"},
            { Daa,"DAA"},
            { Dec,"DEC"},
            { DecA,"DECA"},
            { DecB,"DECB"},
            { Des,"DES"},
            { Dex,"DEX"},
            { EorA,"EORA"},
            { EorB,"EORB"},
            { Inc,"INC"},
            { IncA,"INCA"},
            { IncB,"INCB"},
            { Ins,"INS"},
            { Inx,"INX"},
            { Jmp,"JMP"},
            { Jsr,"JSR"},
            { LdaA,"LDAA"},
            { LdaB,"LDAB"},
            { Lds,"LDS"},
            { Ldx,"LDX"},
            { Lsr,"LSR"},
            { LsrA,"LSRA"},
            { LsrB,"LSRB"},
            { Neg,"NEG"},
            { NegA,"NEGA"},
            { NegB,"NEGB"},
            { Nop,"NOP"},
            { OraA,"ORAA"},
            { OraB,"ORAB"},
            { PshA,"PSHA"},
            { PshB,"PSHB"},
            { PulA,"PULA"},
            { PulB,"PULB"},
            { Rol,"ROL"},
            { RolA,"ROLA"},
            { RolB,"ROLB"},
            { Ror,"ROR"},
            { RorA,"RORA"},
            { RorB,"RORB"},
            { Rti,"RTI"},
            { Rts,"RTS"},
            { Sba,"SBA"},
            { SbcA,"SBCA"},
            { SbcB,"SBCB"},
            { Sec,"SEC"},
            { Sei,"SEI"},
            { Sev,"SEV"},
            { StaA,"STAA"},
            { StaB,"STAB"},
            { Sts,"STS"},
            { Stx,"STX"},
            { SubA,"SUBA"},
            { SubB,"SUBB"},
            { Swi,"SWI"},
            { Tab,"TAB"},
            { Tap,"TAP"},
            { Tba,"TBA"},
            { Tpa,"TPA"},
            { Tst,"TST"},
            { TstA,"TSTA"},
            { TstB,"TSTB"},
            { Tsx,"TSX"},
            { Txs,"TXS"},
            { Wai,"WAI"},
            { X,"X"},
        };
    }
}
