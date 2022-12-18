using System.Collections.Generic;

namespace Inu.Assembler.I8086
{
    internal class Keyword : Inu.Assembler.Keyword
    {
        public new const int MinId = NextId;

        public const int A = MinId + 0;
        public const int AAA = MinId + 1;
        public const int AAD = MinId + 2;
        public const int AAM = MinId + 3;
        public const int AAS = MinId + 4;
        public const int ADC = MinId + 5;
        public const int ADD = MinId + 6;
        public const int AE = MinId + 7;
        public const int AH = MinId + 8;
        public const int AL = MinId + 9;
        public const int AX = MinId + 10;
        public const int B = MinId + 11;
        public const int BE = MinId + 12;
        public const int BH = MinId + 13;
        public const int BL = MinId + 14;
        public const int BP = MinId + 15;
        public const int BX = MinId + 16;
        public const int BYTE = MinId + 17;
        public const int C = MinId + 18;
        public const int CALL = MinId + 19;
        public const int CBW = MinId + 20;
        public const int CH = MinId + 21;
        public const int CL = MinId + 22;
        public const int CLC = MinId + 23;
        public const int CLD = MinId + 24;
        public const int CLI = MinId + 25;
        public const int CMC = MinId + 26;
        public const int CMI = MinId + 27;
        public const int CMP = MinId + 28;
        public const int CMPSB = MinId + 29;
        public const int CMPSW = MinId + 30;
        public const int CS = MinId + 31;
        public const int CWD = MinId + 32;
        public const int CX = MinId + 33;
        public const int CXZ = MinId + 34;
        public const int DAA = MinId + 35;
        public const int DAS = MinId + 36;
        public const int DEC = MinId + 37;
        public const int DH = MinId + 38;
        public const int DI = MinId + 39;
        public const int DIV = MinId + 40;
        public const int DL = MinId + 41;
        public const int DX = MinId + 42;
        public const int E = MinId + 43;
        public const int ES = MinId + 44;
        public const int FAR = MinId + 45;
        public const int G = MinId + 46;
        public const int GE = MinId + 47;
        public const int HLT = MinId + 48;
        public const int IDIV = MinId + 49;
        public const int IMUL = MinId + 50;
        public const int IN = MinId + 51;
        public const int INC = MinId + 52;
        public const int INT = MinId + 53;
        public const int INTO = MinId + 54;
        public const int IRET = MinId + 55;
        public const int JA = MinId + 56;
        public const int JAE = MinId + 57;
        public const int JB = MinId + 58;
        public const int JBE = MinId + 59;
        public const int JCXZ = MinId + 60;
        public const int JE = MinId + 61;
        public const int JG = MinId + 62;
        public const int JGE = MinId + 63;
        public const int JL = MinId + 64;
        public const int JLE = MinId + 65;
        public const int JMP = MinId + 66;
        public const int JNA = MinId + 67;
        public const int JNAE = MinId + 68;
        public const int JNB = MinId + 69;
        public const int JNBE = MinId + 70;
        public const int JNE = MinId + 71;
        public const int JNG = MinId + 72;
        public const int JNGE = MinId + 73;
        public const int JNL = MinId + 74;
        public const int JNLE = MinId + 75;
        public const int JNO = MinId + 76;
        public const int JNP = MinId + 77;
        public const int JNS = MinId + 78;
        public const int JNZ = MinId + 79;
        public const int JO = MinId + 80;
        public const int JP = MinId + 81;
        public const int JPE = MinId + 82;
        public const int JPO = MinId + 83;
        public const int JS = MinId + 84;
        public const int JZ = MinId + 85;
        public const int L = MinId + 86;
        public const int LAHF = MinId + 87;
        public const int LDS = MinId + 88;
        public const int LE = MinId + 89;
        public const int LEA = MinId + 90;
        public const int LES = MinId + 91;
        public const int LOCK = MinId + 92;
        public const int LODS = MinId + 93;
        public const int LODSB = MinId + 94;
        public const int LODSW = MinId + 95;
        public const int LOOP = MinId + 96;
        public const int LOOPE = MinId + 97;
        public const int LOOPNE = MinId + 98;
        public const int LOOPNZ = MinId + 99;
        public const int LOOPZ = MinId + 100;
        public const int MOV = MinId + 101;
        public const int MOVSB = MinId + 102;
        public const int MOVSW = MinId + 103;
        public const int MP = MinId + 104;
        public const int MUL = MinId + 105;
        public const int NA = MinId + 106;
        public const int NAE = MinId + 107;
        public const int NB = MinId + 108;
        public const int NBE = MinId + 109;
        public const int NC = MinId + 110;
        public const int NE = MinId + 111;
        public const int NEG = MinId + 112;
        public const int NG = MinId + 113;
        public const int NGE = MinId + 114;
        public const int NL = MinId + 115;
        public const int NLE = MinId + 116;
        public const int NO = MinId + 117;
        public const int NOP = MinId + 118;
        public const int NP = MinId + 119;
        public const int NS = MinId + 120;
        public const int NZ = MinId + 121;
        public const int O = MinId + 122;
        public const int OUT = MinId + 123;
        public const int P = MinId + 124;
        public const int PE = MinId + 125;
        public const int PO = MinId + 126;
        public const int POP = MinId + 127;
        public const int POPF = MinId + 128;
        public const int PTR = MinId + 129;
        public const int PUSH = MinId + 130;
        public const int PUSHF = MinId + 131;
        public const int RCL = MinId + 132;
        public const int RCR = MinId + 133;
        public const int REP = MinId + 134;
        public const int REPE = MinId + 135;
        public const int REPNE = MinId + 136;
        public const int REPNZ = MinId + 137;
        public const int REPZ = MinId + 138;
        public const int RET = MinId + 139;
        public const int RETF = MinId + 140;
        public const int RETN = MinId + 141;
        public const int ROL = MinId + 142;
        public const int ROR = MinId + 143;
        public const int S = MinId + 144;
        public const int SAHF = MinId + 145;
        public const int SAL = MinId + 146;
        public const int SAR = MinId + 147;
        public const int SBB = MinId + 148;
        public const int SCAS = MinId + 149;
        public const int SCASB = MinId + 150;
        public const int SCASW = MinId + 151;
        public const int SI = MinId + 152;
        public const int SP = MinId + 153;
        public const int SS = MinId + 154;
        public const int STC = MinId + 155;
        public const int STD = MinId + 156;
        public const int STI = MinId + 157;
        public const int STOS = MinId + 158;
        public const int STOSB = MinId + 159;
        public const int STOSW = MinId + 160;
        public const int SUB = MinId + 161;
        public const int TEST = MinId + 162;
        public const int WAIT = MinId + 163;
        public const int WCXZ = MinId + 164;
        public const int WLOOP = MinId + 165;
        public const int WORD = MinId + 166;
        public const int XCHG = MinId + 167;
        public const int XLAT = MinId + 168;
        public const int Z = MinId + 169;

        public new static readonly Dictionary<int, string> Words = new Dictionary<int, string>()
        {
            { A,"A"},
            { AAA,"AAA"},
            { AAD,"AAD"},
            { AAM,"AAM"},
            { AAS,"AAS"},
            { ADC,"ADC"},
            { ADD,"ADD"},
            { AE,"AE"},
            { AH,"AH"},
            { AL,"AL"},
            { AX,"AX"},
            { B,"B"},
            { BE,"BE"},
            { BH,"BH"},
            { BL,"BL"},
            { BP,"BP"},
            { BX,"BX"},
            { BYTE,"BYTE"},
            { C,"C"},
            { CALL,"CALL"},
            { CBW,"CBW"},
            { CH,"CH"},
            { CL,"CL"},
            { CLC,"CLC"},
            { CLD,"CLD"},
            { CLI,"CLI"},
            { CMC,"CMC"},
            { CMI,"CMI"},
            { CMP,"CMP"},
            { CMPSB,"CMPSB"},
            { CMPSW,"CMPSW"},
            { CS,"CS"},
            { CWD,"CWD"},
            { CX,"CX"},
            { CXZ,"CXZ"},
            { DAA,"DAA"},
            { DAS,"DAS"},
            { DEC,"DEC"},
            { DH,"DH"},
            { DI,"DI"},
            { DIV,"DIV"},
            { DL,"DL"},
            { DX,"DX"},
            { E,"E"},
            { ES,"ES"},
            { FAR,"FAR"},
            { G,"G"},
            { GE,"GE"},
            { HLT,"HLT"},
            { IDIV,"IDIV"},
            { IMUL,"IMUL"},
            { IN,"IN"},
            { INC,"INC"},
            { INT,"INT"},
            { INTO,"INTO"},
            { IRET,"IRET"},
            { JA,"JA"},
            { JAE,"JAE"},
            { JB,"JB"},
            { JBE,"JBE"},
            { JCXZ,"JCXZ"},
            { JE,"JE"},
            { JG,"JG"},
            { JGE,"JGE"},
            { JL,"JL"},
            { JLE,"JLE"},
            { JMP,"JMP"},
            { JNA,"JNA"},
            { JNAE,"JNAE"},
            { JNB,"JNB"},
            { JNBE,"JNBE"},
            { JNE,"JNE"},
            { JNG,"JNG"},
            { JNGE,"JNGE"},
            { JNL,"JNL"},
            { JNLE,"JNLE"},
            { JNO,"JNO"},
            { JNP,"JNP"},
            { JNS,"JNS"},
            { JNZ,"JNZ"},
            { JO,"JO"},
            { JP,"JP"},
            { JPE,"JPE"},
            { JPO,"JPO"},
            { JS,"JS"},
            { JZ,"JZ"},
            { L,"L"},
            { LAHF,"LAHF"},
            { LDS,"LDS"},
            { LE,"LE"},
            { LEA,"LEA"},
            { LES,"LES"},
            { LOCK,"LOCK"},
            { LODS,"LODS"},
            { LODSB,"LODSB"},
            { LODSW,"LODSW"},
            { LOOP,"LOOP"},
            { LOOPE,"LOOPE"},
            { LOOPNE,"LOOPNE"},
            { LOOPNZ,"LOOPNZ"},
            { LOOPZ,"LOOPZ"},
            { MOV,"MOV"},
            { MOVSB,"MOVSB"},
            { MOVSW,"MOVSW"},
            { MP,"MP"},
            { MUL,"MUL"},
            { NA,"NA"},
            { NAE,"NAE"},
            { NB,"NB"},
            { NBE,"NBE"},
            { NC,"NC"},
            { NE,"NE"},
            { NEG,"NEG"},
            { NG,"NG"},
            { NGE,"NGE"},
            { NL,"NL"},
            { NLE,"NLE"},
            { NO,"NO"},
            { NOP,"NOP"},
            { NP,"NP"},
            { NS,"NS"},
            { NZ,"NZ"},
            { O,"O"},
            { OUT,"OUT"},
            { P,"P"},
            { PE,"PE"},
            { PO,"PO"},
            { POP,"POP"},
            { POPF,"POPF"},
            { PTR,"PTR"},
            { PUSH,"PUSH"},
            { PUSHF,"PUSHF"},
            { RCL,"RCL"},
            { RCR,"RCR"},
            { REP,"REP"},
            { REPE,"REPE"},
            { REPNE,"REPNE"},
            { REPNZ,"REPNZ"},
            { REPZ,"REPZ"},
            { RET,"RET"},
            { RETF,"RETF"},
            { RETN,"RETN"},
            { ROL,"ROL"},
            { ROR,"ROR"},
            { S,"S"},
            { SAHF,"SAHF"},
            { SAL,"SAL"},
            { SAR,"SAR"},
            { SBB,"SBB"},
            { SCAS,"SCAS"},
            { SCASB,"SCASB"},
            { SCASW,"SCASW"},
            { SI,"SI"},
            { SP,"SP"},
            { SS,"SS"},
            { STC,"STC"},
            { STD,"STD"},
            { STI,"STI"},
            { STOS,"STOS"},
            { STOSB,"STOSB"},
            { STOSW,"STOSW"},
            { SUB,"SUB"},
            { TEST,"TEST"},
            { WAIT,"WAIT"},
            { WCXZ,"WCXZ"},
            { WLOOP,"WLOOP"},
            { WORD,"WORD"},
            { XCHG,"XCHG"},
            { XLAT,"XLAT"},
            { Z,"Z"},
        };
    }
}
