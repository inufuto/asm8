using System.Collections.Generic;

namespace Inu.Assembler.I8080
{
    internal class Keyword : Inu.Assembler.Keyword
    {
        public new const int MinId = NextId;


        public const int A = MinId + 0;
        public const int ACI = MinId + 1;
        public const int ADC = MinId + 2;
        public const int ADD = MinId + 3;
        public const int ADI = MinId + 4;
        public const int ANA = MinId + 5;
        public const int ANI = MinId + 6;
        public const int B = MinId + 7;
        public const int C = MinId + 8;
        public const int CALL = MinId + 9;
        public const int CC = MinId + 10;
        public const int CM = MinId + 11;
        public const int CMA = MinId + 12;
        public const int CMC = MinId + 13;
        public const int CMP = MinId + 14;
        public const int CNC = MinId + 15;
        public const int CNZ = MinId + 16;
        public const int CP = MinId + 17;
        public const int CPE = MinId + 18;
        public const int CPI = MinId + 19;
        public const int CPO = MinId + 20;
        public const int CZ = MinId + 21;
        public const int D = MinId + 22;
        public const int DAA = MinId + 23;
        public const int DAD = MinId + 24;
        public const int DCR = MinId + 25;
        public const int DCX = MinId + 26;
        public const int DI = MinId + 27;
        public const int E = MinId + 28;
        public const int EI = MinId + 29;
        public const int H = MinId + 30;
        public const int HLT = MinId + 31;
        public const int IN = MinId + 32;
        public const int INR = MinId + 33;
        public const int INX = MinId + 34;
        public const int JC = MinId + 35;
        public const int JM = MinId + 36;
        public const int JMP = MinId + 37;
        public const int JNC = MinId + 38;
        public const int JNZ = MinId + 39;
        public const int JP = MinId + 40;
        public const int JPE = MinId + 41;
        public const int JPO = MinId + 42;
        public const int JZ = MinId + 43;
        public const int L = MinId + 44;
        public const int LDA = MinId + 45;
        public const int LDAX = MinId + 46;
        public const int LHLD = MinId + 47;
        public const int LXI = MinId + 48;
        public const int M = MinId + 49;
        public const int MOV = MinId + 50;
        public const int MVI = MinId + 51;
        public const int NC = MinId + 52;
        public const int NOP = MinId + 53;
        public const int NZ = MinId + 54;
        public const int ORA = MinId + 55;
        public const int ORI = MinId + 56;
        public const int OUT = MinId + 57;
        public const int P = MinId + 58;
        public const int PCHL = MinId + 59;
        public const int PE = MinId + 60;
        public const int PO = MinId + 61;
        public const int POP = MinId + 62;
        public const int PSW = MinId + 63;
        public const int PUSH = MinId + 64;
        public const int RAL = MinId + 65;
        public const int RAR = MinId + 66;
        public const int RC = MinId + 67;
        public const int RET = MinId + 68;
        public const int RLC = MinId + 69;
        public const int RM = MinId + 70;
        public const int RNC = MinId + 71;
        public const int RNZ = MinId + 72;
        public const int RP = MinId + 73;
        public const int RPE = MinId + 74;
        public const int RPO = MinId + 75;
        public const int RRC = MinId + 76;
        public const int RST = MinId + 77;
        public const int RZ = MinId + 78;
        public const int SBB = MinId + 79;
        public const int SBI = MinId + 80;
        public const int SHLD = MinId + 81;
        public const int SP = MinId + 82;
        public const int SPHL = MinId + 83;
        public const int STA = MinId + 84;
        public const int STAX = MinId + 85;
        public const int STC = MinId + 86;
        public const int SUB = MinId + 87;
        public const int SUI = MinId + 88;
        public const int XCHG = MinId + 89;
        public const int XRA = MinId + 90;
        public const int XRI = MinId + 91;
        public const int XTHL = MinId + 92;
        public const int Z = MinId + 93;

        public new static readonly Dictionary<int, string> Words = new Dictionary<int, string>()
        {

            { A,"A"},
            { ACI,"ACI"},
            { ADC,"ADC"},
            { ADD,"ADD"},
            { ADI,"ADI"},
            { ANA,"ANA"},
            { ANI,"ANI"},
            { B,"B"},
            { C,"C"},
            { CALL,"CALL"},
            { CC,"CC"},
            { CM,"CM"},
            { CMA,"CMA"},
            { CMC,"CMC"},
            { CMP,"CMP"},
            { CNC,"CNC"},
            { CNZ,"CNZ"},
            { CP,"CP"},
            { CPE,"CPE"},
            { CPI,"CPI"},
            { CPO,"CPO"},
            { CZ,"CZ"},
            { D,"D"},
            { DAA,"DAA"},
            { DAD,"DAD"},
            { DCR,"DCR"},
            { DCX,"DCX"},
            { DI,"DI"},
            { E,"E"},
            { EI,"EI"},
            { H,"H"},
            { HLT,"HLT"},
            { IN,"IN"},
            { INR,"INR"},
            { INX,"INX"},
            { JC,"JC"},
            { JM,"JM"},
            { JMP,"JMP"},
            { JNC,"JNC"},
            { JNZ,"JNZ"},
            { JP,"JP"},
            { JPE,"JPE"},
            { JPO,"JPO"},
            { JZ,"JZ"},
            { L,"L"},
            { LDA,"LDA"},
            { LDAX,"LDAX"},
            { LHLD,"LHLD"},
            { LXI,"LXI"},
            { M,"M"},
            { MOV,"MOV"},
            { MVI,"MVI"},
            { NC,"NC"},
            { NOP,"NOP"},
            { NZ,"NZ"},
            { ORA,"ORA"},
            { ORI,"ORI"},
            { OUT,"OUT"},
            { P,"P"},
            { PCHL,"PCHL"},
            { PE,"PE"},
            { PO,"PO"},
            { POP,"POP"},
            { PSW,"PSW"},
            { PUSH,"PUSH"},
            { RAL,"RAL"},
            { RAR,"RAR"},
            { RC,"RC"},
            { RET,"RET"},
            { RLC,"RLC"},
            { RM,"RM"},
            { RNC,"RNC"},
            { RNZ,"RNZ"},
            { RP,"RP"},
            { RPE,"RPE"},
            { RPO,"RPO"},
            { RRC,"RRC"},
            { RST,"RST"},
            { RZ,"RZ"},
            { SBB,"SBB"},
            { SBI,"SBI"},
            { SHLD,"SHLD"},
            { SP,"SP"},
            { SPHL,"SPHL"},
            { STA,"STA"},
            { STAX,"STAX"},
            { STC,"STC"},
            { SUB,"SUB"},
            { SUI,"SUI"},
            { XCHG,"XCHG"},
            { XRA,"XRA"},
            { XRI,"XRI"},
            { XTHL,"XTHL"},
            { Z,"Z"},
        };
    }
}
