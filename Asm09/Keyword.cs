using System.Collections.Generic;
using Inu.Language;

namespace Inu.Assembler.Mc6809
{
    class Keyword : Inu.Assembler.Keyword
    {
        public new const int MinId = NextId;

        public const int Cc = MinId + 0;
        public const int Cs = MinId + 1;
        public const int Eq = MinId + 2;
        public const int Ge = MinId + 3;
        public const int Gt = MinId + 4;
        public const int Hi = MinId + 5;
        public const int Hs = MinId + 6;
        public const int Le = MinId + 7;
        public const int Lo = MinId + 8;
        public const int Ls = MinId + 9;
        public const int Lt = MinId + 10;
        public const int Mi = MinId + 11;
        public const int Ne = MinId + 12;
        public const int Pl = MinId + 13;
        public const int Vc = MinId + 14;
        public const int Vs = MinId + 15;
        public const int Abx = MinId + 16;
        public const int AdcA = MinId + 17;
        public const int AdcB = MinId + 18;
        public const int AddA = MinId + 19;
        public const int AddB = MinId + 20;
        public const int AddD = MinId + 21;
        public const int AndA = MinId + 22;
        public const int AndB = MinId + 23;
        public const int AndCc = MinId + 24;
        public const int Asl = MinId + 25;
        public const int AslA = MinId + 26;
        public const int AslB = MinId + 27;
        public const int Asr = MinId + 28;
        public const int AsrA = MinId + 29;
        public const int AsrB = MinId + 30;
        public const int Bcc = MinId + 31;
        public const int Bcs = MinId + 32;
        public const int Beq = MinId + 33;
        public const int Bge = MinId + 34;
        public const int Bgt = MinId + 35;
        public const int Bhi = MinId + 36;
        public const int Bhs = MinId + 37;
        public const int BitA = MinId + 38;
        public const int BitB = MinId + 39;
        public const int Ble = MinId + 40;
        public const int Blo = MinId + 41;
        public const int Bls = MinId + 42;
        public const int Blt = MinId + 43;
        public const int Bmi = MinId + 44;
        public const int Bne = MinId + 45;
        public const int Bpl = MinId + 46;
        public const int Bra = MinId + 47;
        public const int Brn = MinId + 48;
        public const int Bsr = MinId + 49;
        public const int Bvc = MinId + 50;
        public const int Bvs = MinId + 51;
        public const int Clr = MinId + 52;
        public const int ClrA = MinId + 53;
        public const int ClrB = MinId + 54;
        public const int CmpA = MinId + 55;
        public const int CmpB = MinId + 56;
        public const int CmpD = MinId + 57;
        public const int CmpS = MinId + 58;
        public const int CmpU = MinId + 59;
        public const int CmpX = MinId + 60;
        public const int CmpY = MinId + 61;
        public const int Com = MinId + 62;
        public const int ComA = MinId + 63;
        public const int ComB = MinId + 64;
        public const int CWai = MinId + 65;
        public const int Daa = MinId + 66;
        public const int Dec = MinId + 67;
        public const int DecA = MinId + 68;
        public const int DecB = MinId + 69;
        public const int EorA = MinId + 70;
        public const int EorB = MinId + 71;
        public const int Exg = MinId + 72;
        public const int Inc = MinId + 73;
        public const int IncA = MinId + 74;
        public const int IncB = MinId + 75;
        public const int Jmp = MinId + 76;
        public const int Jsr = MinId + 77;
        public const int LBcc = MinId + 78;
        public const int LBcs = MinId + 79;
        public const int LBeq = MinId + 80;
        public const int LBge = MinId + 81;
        public const int LBgt = MinId + 82;
        public const int LBhi = MinId + 83;
        public const int LBhs = MinId + 84;
        public const int LBle = MinId + 85;
        public const int LBlo = MinId + 86;
        public const int LBls = MinId + 87;
        public const int LBlt = MinId + 88;
        public const int LBmi = MinId + 89;
        public const int LBne = MinId + 90;
        public const int LBpl = MinId + 91;
        public const int LBra = MinId + 92;
        public const int LBrn = MinId + 93;
        public const int LBsr = MinId + 94;
        public const int LBvc = MinId + 95;
        public const int LBvs = MinId + 96;
        public const int LdA = MinId + 97;
        public const int LdB = MinId + 98;
        public const int LdD = MinId + 99;
        public const int LdS = MinId + 100;
        public const int LdU = MinId + 101;
        public const int LdX = MinId + 102;
        public const int LdY = MinId + 103;
        public const int LeaS = MinId + 104;
        public const int LeaU = MinId + 105;
        public const int LeaX = MinId + 106;
        public const int LeaY = MinId + 107;
        public const int Lsl = MinId + 108;
        public const int LslA = MinId + 109;
        public const int LslB = MinId + 110;
        public const int Lsr = MinId + 111;
        public const int LsrA = MinId + 112;
        public const int LsrB = MinId + 113;
        public const int Mul = MinId + 114;
        public const int Neg = MinId + 115;
        public const int NegA = MinId + 116;
        public const int NegB = MinId + 117;
        public const int Nop = MinId + 118;
        public const int OrA = MinId + 119;
        public const int OrB = MinId + 120;
        public const int OrCc = MinId + 121;
        public const int PcR = MinId + 122;
        public const int PshS = MinId + 123;
        public const int PshU = MinId + 124;
        public const int PulS = MinId + 125;
        public const int PulU = MinId + 126;
        public const int Rol = MinId + 127;
        public const int RolA = MinId + 128;
        public const int RolB = MinId + 129;
        public const int Ror = MinId + 130;
        public const int RorA = MinId + 131;
        public const int RorB = MinId + 132;
        public const int Rti = MinId + 133;
        public const int Rts = MinId + 134;
        public const int SbcA = MinId + 135;
        public const int SbcB = MinId + 136;
        public const int Sex = MinId + 137;
        public const int StA = MinId + 138;
        public const int StB = MinId + 139;
        public const int StD = MinId + 140;
        public const int StS = MinId + 141;
        public const int StU = MinId + 142;
        public const int StX = MinId + 143;
        public const int StY = MinId + 144;
        public const int SubA = MinId + 145;
        public const int SubB = MinId + 146;
        public const int SubD = MinId + 147;
        public const int Swi = MinId + 148;
        public const int Swi2 = MinId + 149;
        public const int Swi3 = MinId + 150;
        public const int Sync = MinId + 151;
        public const int Tfr = MinId + 152;
        public const int Tst = MinId + 153;
        public const int TstA = MinId + 154;
        public const int TstB = MinId + 155;
        public const int A = MinId + 156;
        public const int B = MinId + 157;
        public const int D = MinId + 158;
        public const int Dp = MinId + 159;
        public const int Pc = MinId + 160;
        public const int S = MinId + 161;
        public const int U = MinId + 162;
        public const int X = MinId + 163;
        public const int Y = MinId + 164;

        public new static readonly Dictionary<int, string> Words = new Dictionary<int, string>()
        {
            { Cc,"CC"},
            { Cs,"CS"},
            { Eq,"EQ"},
            { Ge,"GE"},
            { Gt,"GT"},
            { Hi,"HI"},
            { Hs,"HS"},
            { Le,"LE"},
            { Lo,"LO"},
            { Ls,"LS"},
            { Lt,"LT"},
            { Mi,"MI"},
            { Ne,"NE"},
            { Pl,"PL"},
            { Vc,"VC"},
            { Vs,"VS"},
            { Abx,"ABX"},
            { AdcA,"ADCA"},
            { AdcB,"ADCB"},
            { AddA,"ADDA"},
            { AddB,"ADDB"},
            { AddD,"ADDD"},
            { AndA,"ANDA"},
            { AndB,"ANDB"},
            { AndCc,"ANDCC"},
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
            { Bhs,"BHS"},
            { BitA,"BITA"},
            { BitB,"BITB"},
            { Ble,"BLE"},
            { Blo,"BLO"},
            { Bls,"BLS"},
            { Blt,"BLT"},
            { Bmi,"BMI"},
            { Bne,"BNE"},
            { Bpl,"BPL"},
            { Bra,"BRA"},
            { Brn,"BRN"},
            { Bsr,"BSR"},
            { Bvc,"BVC"},
            { Bvs,"BVS"},
            { Clr,"CLR"},
            { ClrA,"CLRA"},
            { ClrB,"CLRB"},
            { CmpA,"CMPA"},
            { CmpB,"CMPB"},
            { CmpD,"CMPD"},
            { CmpS,"CMPS"},
            { CmpU,"CMPU"},
            { CmpX,"CMPX"},
            { CmpY,"CMPY"},
            { Com,"COM"},
            { ComA,"COMA"},
            { ComB,"COMB"},
            { CWai,"CWAI"},
            { Daa,"DAA"},
            { Dec,"DEC"},
            { DecA,"DECA"},
            { DecB,"DECB"},
            { EorA,"EORA"},
            { EorB,"EORB"},
            { Exg,"EXG"},
            { Inc,"INC"},
            { IncA,"INCA"},
            { IncB,"INCB"},
            { Jmp,"JMP"},
            { Jsr,"JSR"},
            { LBcc,"LBCC"},
            { LBcs,"LBCS"},
            { LBeq,"LBEQ"},
            { LBge,"LBGE"},
            { LBgt,"LBGT"},
            { LBhi,"LBHI"},
            { LBhs,"LBHS"},
            { LBle,"LBLE"},
            { LBlo,"LBLO"},
            { LBls,"LBLS"},
            { LBlt,"LBLT"},
            { LBmi,"LBMI"},
            { LBne,"LBNE"},
            { LBpl,"LBPL"},
            { LBra,"LBRA"},
            { LBrn,"LBRN"},
            { LBsr,"LBSR"},
            { LBvc,"LBVC"},
            { LBvs,"LBVS"},
            { LdA,"LDA"},
            { LdB,"LDB"},
            { LdD,"LDD"},
            { LdS,"LDS"},
            { LdU,"LDU"},
            { LdX,"LDX"},
            { LdY,"LDY"},
            { LeaS,"LEAS"},
            { LeaU,"LEAU"},
            { LeaX,"LEAX"},
            { LeaY,"LEAY"},
            { Lsl,"LSL"},
            { LslA,"LSLA"},
            { LslB,"LSLB"},
            { Lsr,"LSR"},
            { LsrA,"LSRA"},
            { LsrB,"LSRB"},
            { Mul,"MUL"},
            { Neg,"NEG"},
            { NegA,"NEGA"},
            { NegB,"NEGB"},
            { Nop,"NOP"},
            { OrA,"ORA"},
            { OrB,"ORB"},
            { OrCc,"ORCC"},
            { PcR,"PCR"},
            { PshS,"PSHS"},
            { PshU,"PSHU"},
            { PulS,"PULS"},
            { PulU,"PULU"},
            { Rol,"ROL"},
            { RolA,"ROLA"},
            { RolB,"ROLB"},
            { Ror,"ROR"},
            { RorA,"RORA"},
            { RorB,"RORB"},
            { Rti,"RTI"},
            { Rts,"RTS"},
            { SbcA,"SBCA"},
            { SbcB,"SBCB"},
            { Sex,"SEX"},
            { StA,"STA"},
            { StB,"STB"},
            { StD,"STD"},
            { StS,"STS"},
            { StU,"STU"},
            { StX,"STX"},
            { StY,"STY"},
            { SubA,"SUBA"},
            { SubB,"SUBB"},
            { SubD,"SUBD"},
            { Swi,"SWI"},
            { Swi2,"SWI2"},
            { Swi3,"SWI3"},
            { Sync,"SYNC"},
            { Tfr,"TFR"},
            { Tst,"TST"},
            { TstA,"TSTA"},
            { TstB,"TSTB"},
            { A,"A"},
            { B,"B"},
            { D,"D"},
            { Dp,"DP"},
            { Pc,"PC"},
            { S,"S"},
            { U,"U"},
            { X,"X"},
            { Y,"Y"},
        };
    }
}
