using System.Collections.Generic;

namespace Inu.Assembler.MuCom87
{
    internal class Keyword : Inu.Assembler.Keyword
    {
        public new const int MinId = Inu.Assembler.Keyword.NextId;

        public const int A = MinId + 0;
        public const int ACI = MinId + 1;
        public const int ADC = MinId + 2;
        public const int ADCX = MinId + 3;
        public const int ADD = MinId + 4;
        public const int ADDNC = MinId + 5;
        public const int ADDNCX = MinId + 6;
        public const int ADDX = MinId + 7;
        public const int ADI = MinId + 8;
        public const int ADINC = MinId + 9;
        public const int ANA = MinId + 10;
        public const int ANAX = MinId + 11;
        public const int ANI = MinId + 12;
        public const int ANIW = MinId + 13;
        public const int B = MinId + 14;
        public const int C = MinId + 15;
        public const int CALF = MinId + 16;
        public const int CALL = MinId + 17;
        public const int CALT = MinId + 18;
        public const int CLC = MinId + 19;
        public const int D = MinId + 20;
        public const int DAA = MinId + 21;
        public const int DCR = MinId + 22;
        public const int DCRW = MinId + 23;
        public const int DCX = MinId + 24;
        public const int DI = MinId + 25;
        public const int E = MinId + 26;
        public const int EI = MinId + 27;
        public const int EQA = MinId + 28;
        public const int EQAX = MinId + 29;
        public const int EQI = MinId + 30;
        public const int EQIW = MinId + 31;
        public const int F0 = MinId + 32;
        public const int F1 = MinId + 33;
        public const int F2 = MinId + 34;
        public const int FS = MinId + 35;
        public const int FT = MinId + 36;
        public const int GTA = MinId + 37;
        public const int GTAX = MinId + 38;
        public const int GTI = MinId + 39;
        public const int GTIW = MinId + 40;
        public const int H = MinId + 41;
        public const int IFC = MinId + 42;
        public const int IFNC = MinId + 43;
        public const int IFNZ = MinId + 44;
        public const int IFZ = MinId + 45;
        public const int IN = MinId + 46;
        public const int INR = MinId + 47;
        public const int INRW = MinId + 48;
        public const int INX = MinId + 49;
        public const int JB = MinId + 50;
        public const int JMP = MinId + 51;
        public const int JR = MinId + 52;
        public const int JRE = MinId + 53;
        public const int L = MinId + 54;
        public const int LBCD = MinId + 55;
        public const int LDAW = MinId + 56;
        public const int LDAX = MinId + 57;
        public const int LDED = MinId + 58;
        public const int LHLD = MinId + 59;
        public const int LSPD = MinId + 60;
        public const int LTA = MinId + 61;
        public const int LTAX = MinId + 62;
        public const int LTI = MinId + 63;
        public const int LTIW = MinId + 64;
        public const int LXI = MinId + 65;
        public const int MB = MinId + 66;
        public const int MC = MinId + 67;
        public const int MK = MinId + 68;
        public const int MOV = MinId + 69;
        public const int MVI = MinId + 70;
        public const int NEA = MinId + 71;
        public const int NEAX = MinId + 72;
        public const int NEI = MinId + 73;
        public const int NEIW = MinId + 74;
        public const int NOP = MinId + 75;
        public const int OFFAX = MinId + 76;
        public const int OFFI = MinId + 77;
        public const int OFFIW = MinId + 78;
        public const int ONAX = MinId + 79;
        public const int ONI = MinId + 80;
        public const int ONIW = MinId + 81;
        public const int ORA = MinId + 82;
        public const int ORAX = MinId + 83;
        public const int ORI = MinId + 84;
        public const int ORIW = MinId + 85;
        public const int OUT = MinId + 86;
        public const int PA = MinId + 87;
        public const int PB = MinId + 88;
        public const int PC = MinId + 89;
        public const int PER = MinId + 90;
        public const int PEX = MinId + 91;
        public const int POP = MinId + 92;
        public const int PUSH = MinId + 93;
        public const int RAL = MinId + 94;
        public const int RAR = MinId + 95;
        public const int REPEAT = MinId + 96;
        public const int RET = MinId + 97;
        public const int RETI = MinId + 98;
        public const int RETS = MinId + 99;
        public const int RLD = MinId + 100;
        public const int RRD = MinId + 101;
        public const int S = MinId + 102;
        public const int SBB = MinId + 103;
        public const int SBBX = MinId + 104;
        public const int SBCD = MinId + 105;
        public const int SBI = MinId + 106;
        public const int SDED = MinId + 107;
        public const int SHLD = MinId + 108;
        public const int SIO = MinId + 109;
        public const int SKNC = MinId + 110;
        public const int SKNIT = MinId + 111;
        public const int SKNZ = MinId + 112;
        public const int SP = MinId + 113;
        public const int SSPD = MinId + 114;
        public const int STAW = MinId + 115;
        public const int STAX = MinId + 116;
        public const int STC = MinId + 117;
        public const int STM = MinId + 118;
        public const int SUB = MinId + 119;
        public const int SUBNB = MinId + 120;
        public const int SUBNBX = MinId + 121;
        public const int SUBX = MinId + 122;
        public const int SUI = MinId + 123;
        public const int SUINB = MinId + 124;
        public const int TM0 = MinId + 125;
        public const int TM1 = MinId + 126;
        public const int V = MinId + 127;
        public const int XRA = MinId + 128;
        public const int XRAX = MinId + 129;
        public const int XRI = MinId + 130;

        public new const int NextId = MinId + 140;

        public new static readonly Dictionary<int, string> Words = new Dictionary<int, string>()
        {
            { A,"A"},
            { ACI,"ACI"},
            { ADC,"ADC"},
            { ADCX,"ADCX"},
            { ADD,"ADD"},
            { ADDNC,"ADDNC"},
            { ADDNCX,"ADDNCX"},
            { ADDX,"ADDX"},
            { ADI,"ADI"},
            { ADINC,"ADINC"},
            { ANA,"ANA"},
            { ANAX,"ANAX"},
            { ANI,"ANI"},
            { ANIW,"ANIW"},
            { B,"B"},
            { C,"C"},
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
            { EQAX,"EQAX"},
            { EQI,"EQI"},
            { EQIW,"EQIW"},
            { F0,"F0"},
            { F1,"F1"},
            { F2,"F2"},
            { FS,"FS"},
            { FT,"FT"},
            { GTA,"GTA"},
            { GTAX,"GTAX"},
            { GTI,"GTI"},
            { GTIW,"GTIW"},
            { H,"H"},
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
            { LTAX,"LTAX"},
            { LTI,"LTI"},
            { LTIW,"LTIW"},
            { LXI,"LXI"},
            { MB,"MB"},
            { MC,"MC"},
            { MK,"MK"},
            { MOV,"MOV"},
            { MVI,"MVI"},
            { NEA,"NEA"},
            { NEAX,"NEAX"},
            { NEI,"NEI"},
            { NEIW,"NEIW"},
            { NOP,"NOP"},
            { OFFAX,"OFFAX"},
            { OFFI,"OFFI"},
            { OFFIW,"OFFIW"},
            { ONAX,"ONAX"},
            { ONI,"ONI"},
            { ONIW,"ONIW"},
            { ORA,"ORA"},
            { ORAX,"ORAX"},
            { ORI,"ORI"},
            { ORIW,"ORIW"},
            { OUT,"OUT"},
            { PA,"PA"},
            { PB,"PB"},
            { PC,"PC"},
            { PER,"PER"},
            { PEX,"PEX"},
            { POP,"POP"},
            { PUSH,"PUSH"},
            { RAL,"RAL"},
            { RAR,"RAR"},
            { REPEAT,"REPEAT"},
            { RET,"RET"},
            { RETI,"RETI"},
            { RETS,"RETS"},
            { RLD,"RLD"},
            { RRD,"RRD"},
            { S,"S"},
            { SBB,"SBB"},
            { SBBX,"SBBX"},
            { SBCD,"SBCD"},
            { SBI,"SBI"},
            { SDED,"SDED"},
            { SHLD,"SHLD"},
            { SIO,"SIO"},
            { SKNC,"SKNC"},
            { SKNIT,"SKNIT"},
            { SKNZ,"SKNZ"},
            { SP,"SP"},
            { SSPD,"SSPD"},
            { STAW,"STAW"},
            { STAX,"STAX"},
            { STC,"STC"},
            { STM,"STM"},
            { SUB,"SUB"},
            { SUBNB,"SUBNB"},
            { SUBNBX,"SUBNBX"},
            { SUBX,"SUBX"},
            { SUI,"SUI"},
            { SUINB,"SUINB"},
            { TM0,"TM0"},
            { TM1,"TM1"},
            { V,"V"},
            { XRA,"XRA"},
            { XRAX,"XRAX"},
            { XRI,"XRI"},
        };
    }
}
