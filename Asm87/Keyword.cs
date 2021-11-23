using System.Collections.Generic;

namespace Inu.Assembler.MuCom87
{
    internal class Keyword : Inu.Assembler.Keyword
    {
        public new const int MinId = NextId;

        public const int A = MinId + 0;
        public const int ACI = MinId + 1;
        public const int ADC = MinId + 2;
        public const int ADCW = MinId + 3;
        public const int ADCX = MinId + 4;
        public const int ADD = MinId + 5;
        public const int ADDNC = MinId + 6;
        public const int ADDNCW = MinId + 7;
        public const int ADDNCX = MinId + 8;
        public const int ADDW = MinId + 9;
        public const int ADDX = MinId + 10;
        public const int ADI = MinId + 11;
        public const int ADINC = MinId + 12;
        public const int ANA = MinId + 13;
        public const int ANAW = MinId + 14;
        public const int ANAX = MinId + 15;
        public const int ANI = MinId + 16;
        public const int ANIW = MinId + 17;
        public const int B = MinId + 18;
        public const int BIT = MinId + 19;
        public const int BLOCK = MinId + 20;
        public const int C = MinId + 21;
        public const int CALB = MinId + 22;
        public const int CALF = MinId + 23;
        public const int CALL = MinId + 24;
        public const int CALT = MinId + 25;
        public const int CLC = MinId + 26;
        public const int D = MinId + 27;
        public const int DAA = MinId + 28;
        public const int DCR = MinId + 29;
        public const int DCRW = MinId + 30;
        public const int DCX = MinId + 31;
        public const int DI = MinId + 32;
        public const int E = MinId + 33;
        public const int EI = MinId + 34;
        public const int EQA = MinId + 35;
        public const int EQAW = MinId + 36;
        public const int EQAX = MinId + 37;
        public const int EQI = MinId + 38;
        public const int EQIW = MinId + 39;
        public const int EX = MinId + 40;
        public const int EXX = MinId + 41;
        public const int F0 = MinId + 42;
        public const int F1 = MinId + 43;
        public const int F2 = MinId + 44;
        public const int FS = MinId + 45;
        public const int FT = MinId + 46;
        public const int GTA = MinId + 47;
        public const int GTAW = MinId + 48;
        public const int GTAX = MinId + 49;
        public const int GTI = MinId + 50;
        public const int GTIW = MinId + 51;
        public const int H = MinId + 52;
        public const int HLT = MinId + 53;
        public const int IFC = MinId + 54;
        public const int IFNC = MinId + 55;
        public const int IFNZ = MinId + 56;
        public const int IFZ = MinId + 57;
        public const int IN = MinId + 58;
        public const int INR = MinId + 59;
        public const int INRW = MinId + 60;
        public const int INX = MinId + 61;
        public const int JB = MinId + 62;
        public const int JMP = MinId + 63;
        public const int JR = MinId + 64;
        public const int JRE = MinId + 65;
        public const int L = MinId + 66;
        public const int LBCD = MinId + 67;
        public const int LDAW = MinId + 68;
        public const int LDAX = MinId + 69;
        public const int LDED = MinId + 70;
        public const int LHLD = MinId + 71;
        public const int LSPD = MinId + 72;
        public const int LTA = MinId + 73;
        public const int LTAW = MinId + 74;
        public const int LTAX = MinId + 75;
        public const int LTI = MinId + 76;
        public const int LTIW = MinId + 77;
        public const int LXI = MinId + 78;
        public const int MB = MinId + 79;
        public const int MC = MinId + 80;
        public const int MK = MinId + 81;
        public const int MOV = MinId + 82;
        public const int MVI = MinId + 83;
        public const int MVIW = MinId + 84;
        public const int MVIX = MinId + 85;
        public const int NEA = MinId + 86;
        public const int NEAW = MinId + 87;
        public const int NEAX = MinId + 88;
        public const int NEI = MinId + 89;
        public const int NEIW = MinId + 90;
        public const int NOP = MinId + 91;
        public const int OFFA = MinId + 92;
        public const int OFFAW = MinId + 93;
        public const int OFFAX = MinId + 94;
        public const int OFFI = MinId + 95;
        public const int OFFIW = MinId + 96;
        public const int ONA = MinId + 97;
        public const int ONAW = MinId + 98;
        public const int ONAX = MinId + 99;
        public const int ONI = MinId + 100;
        public const int ONIW = MinId + 101;
        public const int ORA = MinId + 102;
        public const int ORAW = MinId + 103;
        public const int ORAX = MinId + 104;
        public const int ORI = MinId + 105;
        public const int ORIW = MinId + 106;
        public const int OUT = MinId + 107;
        public const int PA = MinId + 108;
        public const int PB = MinId + 109;
        public const int PC = MinId + 110;
        public const int PEN = MinId + 111;
        public const int PER = MinId + 112;
        public const int PEX = MinId + 113;
        public const int POP = MinId + 114;
        public const int PUSH = MinId + 115;
        public const int RAL = MinId + 116;
        public const int RAR = MinId + 117;
        public const int RCL = MinId + 118;
        public const int RCR = MinId + 119;
        public const int RET = MinId + 120;
        public const int RETI = MinId + 121;
        public const int RETS = MinId + 122;
        public const int RLD = MinId + 123;
        public const int RRD = MinId + 124;
        public const int S = MinId + 125;
        public const int SBB = MinId + 126;
        public const int SBBW = MinId + 127;
        public const int SBBX = MinId + 128;
        public const int SBCD = MinId + 129;
        public const int SBI = MinId + 130;
        public const int SDED = MinId + 131;
        public const int SHAL = MinId + 132;
        public const int SHAR = MinId + 133;
        public const int SHCL = MinId + 134;
        public const int SHCR = MinId + 135;
        public const int SHLD = MinId + 136;
        public const int SIO = MinId + 137;
        public const int SKC = MinId + 138;
        public const int SKIT = MinId + 139;
        public const int SKNC = MinId + 140;
        public const int SKNIT = MinId + 141;
        public const int SKNZ = MinId + 142;
        public const int SKZ = MinId + 143;
        public const int SOFTI = MinId + 144;
        public const int SP = MinId + 145;
        public const int SSPD = MinId + 146;
        public const int STAW = MinId + 147;
        public const int STAX = MinId + 148;
        public const int STC = MinId + 149;
        public const int STM = MinId + 150;
        public const int SUB = MinId + 151;
        public const int SUBNB = MinId + 152;
        public const int SUBNBW = MinId + 153;
        public const int SUBNBX = MinId + 154;
        public const int SUBW = MinId + 155;
        public const int SUBX = MinId + 156;
        public const int SUI = MinId + 157;
        public const int SUINB = MinId + 158;
        public const int TABLE = MinId + 159;
        public const int TM0 = MinId + 160;
        public const int TM1 = MinId + 161;
        public const int V = MinId + 162;
        public const int WHC = MinId + 163;
        public const int WHNC = MinId + 164;
        public const int WHNZ = MinId + 165;
        public const int WHZ = MinId + 166;
        public const int XRA = MinId + 167;
        public const int XRAW = MinId + 168;
        public const int XRAX = MinId + 169;
        public const int XRI = MinId + 170;

        public new static readonly Dictionary<int, string> Words = new Dictionary<int, string>()
        {
            { A,"A"},
            { ACI,"ACI"},
            { ADC,"ADC"},
            { ADCW,"ADCW"},
            { ADCX,"ADCX"},
            { ADD,"ADD"},
            { ADDNC,"ADDNC"},
            { ADDNCW,"ADDNCW"},
            { ADDNCX,"ADDNCX"},
            { ADDW,"ADDW"},
            { ADDX,"ADDX"},
            { ADI,"ADI"},
            { ADINC,"ADINC"},
            { ANA,"ANA"},
            { ANAW,"ANAW"},
            { ANAX,"ANAX"},
            { ANI,"ANI"},
            { ANIW,"ANIW"},
            { B,"B"},
            { BIT,"BIT"},
            { BLOCK,"BLOCK"},
            { C,"C"},
            { CALB,"CALB"},
            { CALF,"CALF"},
            { CALL,"CALL"},
            { CALT,"CALT"},
            { CLC,"CLC"},
            { D,"D"},
            { DAA,"DAA"},
            { DCR,"DCR"},
            { DCRW,"DCRW"},
            { DCX,"DCX"},
            { DI,"DI"},
            { E,"E"},
            { EI,"EI"},
            { EQA,"EQA"},
            { EQAW,"EQAW"},
            { EQAX,"EQAX"},
            { EQI,"EQI"},
            { EQIW,"EQIW"},
            { EX,"EX"},
            { EXX,"EXX"},
            { F0,"F0"},
            { F1,"F1"},
            { F2,"F2"},
            { FS,"FS"},
            { FT,"FT"},
            { GTA,"GTA"},
            { GTAW,"GTAW"},
            { GTAX,"GTAX"},
            { GTI,"GTI"},
            { GTIW,"GTIW"},
            { H,"H"},
            { HLT,"HLT"},
            { IFC,"IFC"},
            { IFNC,"IFNC"},
            { IFNZ,"IFNZ"},
            { IFZ,"IFZ"},
            { IN,"IN"},
            { INR,"INR"},
            { INRW,"INRW"},
            { INX,"INX"},
            { JB,"JB"},
            { JMP,"JMP"},
            { JR,"JR"},
            { JRE,"JRE"},
            { L,"L"},
            { LBCD,"LBCD"},
            { LDAW,"LDAW"},
            { LDAX,"LDAX"},
            { LDED,"LDED"},
            { LHLD,"LHLD"},
            { LSPD,"LSPD"},
            { LTA,"LTA"},
            { LTAW,"LTAW"},
            { LTAX,"LTAX"},
            { LTI,"LTI"},
            { LTIW,"LTIW"},
            { LXI,"LXI"},
            { MB,"MB"},
            { MC,"MC"},
            { MK,"MK"},
            { MOV,"MOV"},
            { MVI,"MVI"},
            { MVIW,"MVIW"},
            { MVIX,"MVIX"},
            { NEA,"NEA"},
            { NEAW,"NEAW"},
            { NEAX,"NEAX"},
            { NEI,"NEI"},
            { NEIW,"NEIW"},
            { NOP,"NOP"},
            { OFFA,"OFFA"},
            { OFFAW,"OFFAW"},
            { OFFAX,"OFFAX"},
            { OFFI,"OFFI"},
            { OFFIW,"OFFIW"},
            { ONA,"ONA"},
            { ONAW,"ONAW"},
            { ONAX,"ONAX"},
            { ONI,"ONI"},
            { ONIW,"ONIW"},
            { ORA,"ORA"},
            { ORAW,"ORAW"},
            { ORAX,"ORAX"},
            { ORI,"ORI"},
            { ORIW,"ORIW"},
            { OUT,"OUT"},
            { PA,"PA"},
            { PB,"PB"},
            { PC,"PC"},
            { PEN,"PEN"},
            { PER,"PER"},
            { PEX,"PEX"},
            { POP,"POP"},
            { PUSH,"PUSH"},
            { RAL,"RAL"},
            { RAR,"RAR"},
            { RCL,"RCL"},
            { RCR,"RCR"},
            { RET,"RET"},
            { RETI,"RETI"},
            { RETS,"RETS"},
            { RLD,"RLD"},
            { RRD,"RRD"},
            { S,"S"},
            { SBB,"SBB"},
            { SBBW,"SBBW"},
            { SBBX,"SBBX"},
            { SBCD,"SBCD"},
            { SBI,"SBI"},
            { SDED,"SDED"},
            { SHAL,"SHAL"},
            { SHAR,"SHAR"},
            { SHCL,"SHCL"},
            { SHCR,"SHCR"},
            { SHLD,"SHLD"},
            { SIO,"SIO"},
            { SKC,"SKC"},
            { SKIT,"SKIT"},
            { SKNC,"SKNC"},
            { SKNIT,"SKNIT"},
            { SKNZ,"SKNZ"},
            { SKZ,"SKZ"},
            { SOFTI,"SOFTI"},
            { SP,"SP"},
            { SSPD,"SSPD"},
            { STAW,"STAW"},
            { STAX,"STAX"},
            { STC,"STC"},
            { STM,"STM"},
            { SUB,"SUB"},
            { SUBNB,"SUBNB"},
            { SUBNBW,"SUBNBW"},
            { SUBNBX,"SUBNBX"},
            { SUBW,"SUBW"},
            { SUBX,"SUBX"},
            { SUI,"SUI"},
            { SUINB,"SUINB"},
            { TABLE,"TABLE"},
            { TM0,"TM0"},
            { TM1,"TM1"},
            { V,"V"},
            { WHC,"WHC"},
            { WHNC,"WHNC"},
            { WHNZ,"WHNZ"},
            { WHZ,"WHZ"},
            { XRA,"XRA"},
            { XRAW,"XRAW"},
            { XRAX,"XRAX"},
            { XRI,"XRI"},
        };
    }
}
