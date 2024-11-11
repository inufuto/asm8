namespace Inu.Assembler.Sm8521;

internal class Keyword : Inu.Assembler.Keyword
{
    public new const int MinId = NextId;

    public const int ADC = MinId + 0;
    public const int ADCW = MinId + 1;
    public const int ADD = MinId + 2;
    public const int ADDW = MinId + 3;
    public const int ANDW = MinId + 4;
    public const int BAND = MinId + 5;
    public const int BBC = MinId + 6;
    public const int BBS = MinId + 7;
    public const int BC = MinId + 8;
    public const int BCLR = MinId + 9;
    public const int BCMP = MinId + 10;
    public const int BF = MinId + 11;
    public const int BMOV = MinId + 12;
    public const int BOR = MinId + 13;
    public const int BR = MinId + 14;
    public const int BS = MinId + 15;
    public const int BSET = MinId + 16;
    public const int BTST = MinId + 17;
    public const int BXOR = MinId + 18;
    public const int C = MinId + 19;
    public const int CALL = MinId + 20;
    public const int CALS = MinId + 21;
    public const int CLR = MinId + 22;
    public const int CLRC = MinId + 23;
    public const int CMP = MinId + 24;
    public const int CMPW = MinId + 25;
    public const int COM = MinId + 26;
    public const int COMC = MinId + 27;
    public const int DA = MinId + 28;
    public const int DBNZ = MinId + 29;
    public const int DEC = MinId + 30;
    public const int DECW = MinId + 31;
    public const int DI = MinId + 32;
    public const int DIV = MinId + 33;
    public const int DWNZ = MinId + 34;
    public const int EI = MinId + 35;
    public const int EQ = MinId + 36;
    public const int EXTS = MinId + 37;
    public const int F = MinId + 38;
    public const int GE = MinId + 39;
    public const int GT = MinId + 40;
    public const int HALT = MinId + 41;
    public const int IE0 = MinId + 42;
    public const int IE1 = MinId + 43;
    public const int INC = MinId + 44;
    public const int INCW = MinId + 45;
    public const int IR0 = MinId + 46;
    public const int IR1 = MinId + 47;
    public const int IRET = MinId + 48;
    public const int JMP = MinId + 49;
    public const int LE = MinId + 50;
    public const int LT = MinId + 51;
    public const int MI = MinId + 52;
    public const int MOV = MinId + 53;
    public const int MOVM = MinId + 54;
    public const int MOVW = MinId + 55;
    public const int MULT = MinId + 56;
    public const int NC = MinId + 57;
    public const int NE = MinId + 58;
    public const int NEG = MinId + 59;
    public const int NOP = MinId + 60;
    public const int NOV = MinId + 61;
    public const int NZ = MinId + 62;
    public const int ORW = MinId + 63;
    public const int OV = MinId + 64;
    public const int P0 = MinId + 65;
    public const int P1 = MinId + 66;
    public const int P2 = MinId + 67;
    public const int P3 = MinId + 68;
    public const int PL = MinId + 69;
    public const int POP = MinId + 70;
    public const int POPW = MinId + 71;
    public const int PS0 = MinId + 72;
    public const int PUSH = MinId + 73;
    public const int PUSHW = MinId + 74;
    public const int RET = MinId + 75;
    public const int RL = MinId + 76;
    public const int RLC = MinId + 77;
    public const int RR = MinId + 78;
    public const int RRC = MinId + 79;
    public const int SBC = MinId + 80;
    public const int SBCW = MinId + 81;
    public const int SETC = MinId + 82;
    public const int SLL = MinId + 83;
    public const int SRA = MinId + 84;
    public const int SRL = MinId + 85;
    public const int STOP = MinId + 86;
    public const int SUB = MinId + 87;
    public const int SUBW = MinId + 88;
    public const int SWAP = MinId + 89;
    public const int T = MinId + 90;
    public const int UGE = MinId + 91;
    public const int UGT = MinId + 92;
    public const int ULE = MinId + 93;
    public const int ULT = MinId + 94;
    public const int XORW = MinId + 95;
    public const int Z = MinId + 96;

    public new static readonly Dictionary<int, string> Words = new()
    {
        { ADC,"ADC"},
        { ADCW,"ADCW"},
        { ADD,"ADD"},
        { ADDW,"ADDW"},
        { ANDW,"ANDW"},
        { BAND,"BAND"},
        { BBC,"BBC"},
        { BBS,"BBS"},
        { BC,"BC"},
        { BCLR,"BCLR"},
        { BCMP,"BCMP"},
        { BF,"BF"},
        { BMOV,"BMOV"},
        { BOR,"BOR"},
        { BR,"BR"},
        { BS,"BS"},
        { BSET,"BSET"},
        { BTST,"BTST"},
        { BXOR,"BXOR"},
        { C,"C"},
        { CALL,"CALL"},
        { CALS,"CALS"},
        { CLR,"CLR"},
        { CLRC,"CLRC"},
        { CMP,"CMP"},
        { CMPW,"CMPW"},
        { COM,"COM"},
        { COMC,"COMC"},
        { DA,"DA"},
        { DBNZ,"DBNZ"},
        { DEC,"DEC"},
        { DECW,"DECW"},
        { DI,"DI"},
        { DIV,"DIV"},
        { DWNZ,"DWNZ"},
        { EI,"EI"},
        { EQ,"EQ"},
        { EXTS,"EXTS"},
        { F,"F"},
        { GE,"GE"},
        { GT,"GT"},
        { HALT,"HALT"},
        { IE0,"IE0"},
        { IE1,"IE1"},
        { INC,"INC"},
        { INCW,"INCW"},
        { IR0,"IR0"},
        { IR1,"IR1"},
        { IRET,"IRET"},
        { JMP,"JMP"},
        { LE,"LE"},
        { LT,"LT"},
        { MI,"MI"},
        { MOV,"MOV"},
        { MOVM,"MOVM"},
        { MOVW,"MOVW"},
        { MULT,"MULT"},
        { NC,"NC"},
        { NE,"NE"},
        { NEG,"NEG"},
        { NOP,"NOP"},
        { NOV,"NOV"},
        { NZ,"NZ"},
        { ORW,"ORW"},
        { OV,"OV"},
        { P0,"P0"},
        { P1,"P1"},
        { P2,"P2"},
        { P3,"P3"},
        { PL,"PL"},
        { POP,"POP"},
        { POPW,"POPW"},
        { PS0,"PS0"},
        { PUSH,"PUSH"},
        { PUSHW,"PUSHW"},
        { RET,"RET"},
        { RL,"RL"},
        { RLC,"RLC"},
        { RR,"RR"},
        { RRC,"RRC"},
        { SBC,"SBC"},
        { SBCW,"SBCW"},
        { SETC,"SETC"},
        { SLL,"SLL"},
        { SRA,"SRA"},
        { SRL,"SRL"},
        { STOP,"STOP"},
        { SUB,"SUB"},
        { SUBW,"SUBW"},
        { SWAP,"SWAP"},
        { T,"T"},
        { UGE,"UGE"},
        { UGT,"UGT"},
        { ULE,"ULE"},
        { ULT,"ULT"},
        { XORW,"XORW"},
        { Z,"Z"},
    };
}