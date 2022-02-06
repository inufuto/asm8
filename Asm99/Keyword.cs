using System.Collections.Generic;

namespace Inu.Assembler.Tms99
{
    internal class Keyword : Inu.Assembler.Keyword
    {
        public new const int MinId = NextId;


        public const int A = MinId + 0;
        public const int Ab = MinId + 1;
        public const int Abs = MinId + 2;
        public const int Ai = MinId + 3;
        public const int Andi = MinId + 4;
        public const int B = MinId + 5;
        public const int Bl = MinId + 6;
        public const int Blwp = MinId + 7;
        public const int C = MinId + 8;
        public const int Cb = MinId + 9;
        public const int Ci = MinId + 10;
        public const int Ckof = MinId + 11;
        public const int Ckon = MinId + 12;
        public const int Clr = MinId + 13;
        public const int Coc = MinId + 14;
        public const int Czc = MinId + 15;
        public const int Dec = MinId + 16;
        public const int Dect = MinId + 17;
        public const int Div = MinId + 18;
        public const int Idle = MinId + 19;
        public const int Inc = MinId + 20;
        public const int Inct = MinId + 21;
        public const int Inv = MinId + 22;
        public const int Jeq = MinId + 23;
        public const int Jgt = MinId + 24;
        public const int Jh = MinId + 25;
        public const int Jhe = MinId + 26;
        public const int Jl = MinId + 27;
        public const int Jle = MinId + 28;
        public const int Jlt = MinId + 29;
        public const int Jmp = MinId + 30;
        public const int Jnc = MinId + 31;
        public const int Jne = MinId + 32;
        public const int Jno = MinId + 33;
        public const int Joc = MinId + 34;
        public const int Jop = MinId + 35;
        public const int Ldcr = MinId + 36;
        public const int Li = MinId + 37;
        public const int Limi = MinId + 38;
        public const int Lrex = MinId + 39;
        public const int Lwpi = MinId + 40;
        public const int Mov = MinId + 41;
        public const int Movb = MinId + 42;
        public const int Mpy = MinId + 43;
        public const int Neg = MinId + 44;
        public const int Nop = MinId + 45;
        public const int Ori = MinId + 46;
        public const int R0 = MinId + 47;
        public const int R1 = MinId + 48;
        public const int R10 = MinId + 49;
        public const int R11 = MinId + 50;
        public const int R12 = MinId + 51;
        public const int R13 = MinId + 52;
        public const int R14 = MinId + 53;
        public const int R15 = MinId + 54;
        public const int R2 = MinId + 55;
        public const int R3 = MinId + 56;
        public const int R4 = MinId + 57;
        public const int R5 = MinId + 58;
        public const int R6 = MinId + 59;
        public const int R7 = MinId + 60;
        public const int R8 = MinId + 61;
        public const int R9 = MinId + 62;
        public const int Rset = MinId + 63;
        public const int Rt = MinId + 64;
        public const int Rtwp = MinId + 65;
        public const int S = MinId + 66;
        public const int Sb = MinId + 67;
        public const int Sbo = MinId + 68;
        public const int Sbz = MinId + 69;
        public const int Seto = MinId + 70;
        public const int Sla = MinId + 71;
        public const int Soc = MinId + 72;
        public const int Socb = MinId + 73;
        public const int Sra = MinId + 74;
        public const int Src = MinId + 75;
        public const int Srl = MinId + 76;
        public const int Stcr = MinId + 77;
        public const int Stst = MinId + 78;
        public const int Stwp = MinId + 79;
        public const int Swpb = MinId + 80;
        public const int Szc = MinId + 81;
        public const int Szcb = MinId + 82;
        public const int Tb = MinId + 83;
        public const int X = MinId + 84;
        public const int Xop = MinId + 85;
        public const int Eq = MinId + 86;
        public const int Gt = MinId + 87;
        public const int H = MinId + 88;
        public const int He = MinId + 89;
        public const int L = MinId + 90;
        public const int Le = MinId + 91;
        public const int Lt = MinId + 92;
        public const int Nc = MinId + 93;
        public const int Ne = MinId + 94;
        public const int No = MinId + 95;
        public const int Oc = MinId + 96;
        public const int Op = MinId + 97;

        public new static readonly Dictionary<int, string> Words = new Dictionary<int, string>()
        {

            { A,"A"},
            { Ab,"AB"},
            { Abs,"ABS"},
            { Ai,"AI"},
            { Andi,"ANDI"},
            { B,"B"},
            { Bl,"BL"},
            { Blwp,"BLWP"},
            { C,"C"},
            { Cb,"CB"},
            { Ci,"CI"},
            { Ckof,"CKOF"},
            { Ckon,"CKON"},
            { Clr,"CLR"},
            { Coc,"COC"},
            { Czc,"CZC"},
            { Dec,"DEC"},
            { Dect,"DECT"},
            { Div,"DIV"},
            { Idle,"IDLE"},
            { Inc,"INC"},
            { Inct,"INCT"},
            { Inv,"INV"},
            { Jeq,"JEQ"},
            { Jgt,"JGT"},
            { Jh,"JH"},
            { Jhe,"JHE"},
            { Jl,"JL"},
            { Jle,"JLE"},
            { Jlt,"JLT"},
            { Jmp,"JMP"},
            { Jnc,"JNC"},
            { Jne,"JNE"},
            { Jno,"JNO"},
            { Joc,"JOC"},
            { Jop,"JOP"},
            { Ldcr,"LDCR"},
            { Li,"LI"},
            { Limi,"LIMI"},
            { Lrex,"LREX"},
            { Lwpi,"LWPI"},
            { Mov,"MOV"},
            { Movb,"MOVB"},
            { Mpy,"MPY"},
            { Neg,"NEG"},
            { Nop,"NOP"},
            { Ori,"ORI"},
            { R0,"R0"},
            { R1,"R1"},
            { R10,"R10"},
            { R11,"R11"},
            { R12,"R12"},
            { R13,"R13"},
            { R14,"R14"},
            { R15,"R15"},
            { R2,"R2"},
            { R3,"R3"},
            { R4,"R4"},
            { R5,"R5"},
            { R6,"R6"},
            { R7,"R7"},
            { R8,"R8"},
            { R9,"R9"},
            { Rset,"RSET"},
            { Rt,"RT"},
            { Rtwp,"RTWP"},
            { S,"S"},
            { Sb,"SB"},
            { Sbo,"SBO"},
            { Sbz,"SBZ"},
            { Seto,"SETO"},
            { Sla,"SLA"},
            { Soc,"SOC"},
            { Socb,"SOCB"},
            { Sra,"SRA"},
            { Src,"SRC"},
            { Srl,"SRL"},
            { Stcr,"STCR"},
            { Stst,"STST"},
            { Stwp,"STWP"},
            { Swpb,"SWPB"},
            { Szc,"SZC"},
            { Szcb,"SZCB"},
            { Tb,"TB"},
            { X,"X"},
            { Xop,"XOP"},
            { Eq,"EQ"},
            { Gt,"GT"},
            { H,"H"},
            { He,"HE"},
            { L,"L"},
            { Le,"LE"},
            { Lt,"LT"},
            { Nc,"NC"},
            { Ne,"NE"},
            { No,"NO"},
            { Oc,"OC"},
            { Op,"OP"},
        };
    }
}
