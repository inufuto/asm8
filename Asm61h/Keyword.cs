namespace Inu.Assembler.HD61700;

internal class Keyword : Inu.Assembler.Keyword
{
    public new const int MinId = NextId;

    public const int AD = MinId + 0;
    public const int ADB = MinId + 1;
    public const int ADBCM = MinId + 2;
    public const int ADBM = MinId + 3;
    public const int ADBW = MinId + 4;
    public const int ADC = MinId + 5;
    public const int ADCW = MinId + 6;
    public const int ADW = MinId + 7;
    public const int AN = MinId + 8;
    public const int ANC = MinId + 9;
    public const int ANCM = MinId + 10;
    public const int ANCW = MinId + 11;
    public const int ANM = MinId + 12;
    public const int ANW = MinId + 13;
    public const int BDN = MinId + 14;
    public const int BDNS = MinId + 15;
    public const int BID = MinId + 16;
    public const int BIDW = MinId + 17;
    public const int BIU = MinId + 18;
    public const int BIUW = MinId + 19;
    public const int BUP = MinId + 20;
    public const int BUPS = MinId + 21;
    public const int BYD = MinId + 22;
    public const int BYDM = MinId + 23;
    public const int BYDW = MinId + 24;
    public const int BYU = MinId + 25;
    public const int BYUM = MinId + 26;
    public const int BYUW = MinId + 27;
    public const int C = MinId + 28;
    public const int CAL = MinId + 29;
    public const int CANI = MinId + 30;
    public const int CLT = MinId + 31;
    public const int CMP = MinId + 32;
    public const int CMPM = MinId + 33;
    public const int CMPW = MinId + 34;
    public const int DID = MinId + 35;
    public const int DIDM = MinId + 36;
    public const int DIDW = MinId + 37;
    public const int DIU = MinId + 38;
    public const int DIUM = MinId + 39;
    public const int DIUW = MinId + 40;
    public const int DIW = MinId + 41;
    public const int FST = MinId + 42;
    public const int GFL = MinId + 43;
    public const int GFLW = MinId + 44;
    public const int GPO = MinId + 45;
    public const int GPOW = MinId + 46;
    public const int GRE = MinId + 47;
    public const int GSR = MinId + 48;
    public const int GSRW = MinId + 49;
    public const int GST = MinId + 50;
    public const int IA = MinId + 51;
    public const int IB = MinId + 52;
    public const int IE = MinId + 53;
    public const int INV = MinId + 54;
    public const int INVM = MinId + 55;
    public const int INVW = MinId + 56;
    public const int IX = MinId + 57;
    public const int IY = MinId + 58;
    public const int IZ = MinId + 59;
    public const int JP = MinId + 60;
    public const int JR = MinId + 61;
    public const int KY = MinId + 62;
    public const int LD = MinId + 63;
    public const int LDC = MinId + 64;
    public const int LDCM = MinId + 65;
    public const int LDCW = MinId + 66;
    public const int LDD = MinId + 67;
    public const int LDDM = MinId + 68;
    public const int LDDW = MinId + 69;
    public const int LDI = MinId + 70;
    public const int LDIM = MinId + 71;
    public const int LDIW = MinId + 72;
    public const int LDL = MinId + 73;
    public const int LDLM = MinId + 74;
    public const int LDLW = MinId + 75;
    public const int LDM = MinId + 76;
    public const int LDW = MinId + 77;
    public const int LNZ = MinId + 78;
    public const int LZ = MinId + 79;
    public const int NA = MinId + 80;
    public const int NAC = MinId + 81;
    public const int NACM = MinId + 82;
    public const int NACW = MinId + 83;
    public const int NAM = MinId + 84;
    public const int NAW = MinId + 85;
    public const int NC = MinId + 86;
    public const int NLZ = MinId + 87;
    public const int NOP = MinId + 88;
    public const int NZ = MinId + 89;
    public const int OFF = MinId + 90;
    public const int ORC = MinId + 91;
    public const int ORCM = MinId + 92;
    public const int ORCW = MinId + 93;
    public const int ORM = MinId + 94;
    public const int ORW = MinId + 95;
    public const int PD = MinId + 96;
    public const int PE = MinId + 97;
    public const int PFL = MinId + 98;
    public const int PHS = MinId + 99;
    public const int PHSM = MinId + 100;
    public const int PHSW = MinId + 101;
    public const int PHU = MinId + 102;
    public const int PHUM = MinId + 103;
    public const int PHUW = MinId + 104;
    public const int PPO = MinId + 105;
    public const int PPOM = MinId + 106;
    public const int PPOW = MinId + 107;
    public const int PPS = MinId + 108;
    public const int PPSM = MinId + 109;
    public const int PPSW = MinId + 110;
    public const int PPU = MinId + 111;
    public const int PPUM = MinId + 112;
    public const int PPUW = MinId + 113;
    public const int PRE = MinId + 114;
    public const int PSR = MinId + 115;
    public const int PSRM = MinId + 116;
    public const int PSRW = MinId + 117;
    public const int PST = MinId + 118;
    public const int ROD = MinId + 119;
    public const int RODW = MinId + 120;
    public const int ROU = MinId + 121;
    public const int ROUW = MinId + 122;
    public const int RTN = MinId + 123;
    public const int RTNI = MinId + 124;
    public const int SB = MinId + 125;
    public const int SBB = MinId + 126;
    public const int SBBCM = MinId + 127;
    public const int SBBM = MinId + 128;
    public const int SBBW = MinId + 129;
    public const int SBC = MinId + 130;
    public const int SBCW = MinId + 131;
    public const int SBW = MinId + 132;
    public const int SDN = MinId + 133;
    public const int SLW = MinId + 134;
    public const int SS = MinId + 135;
    public const int ST = MinId + 136;
    public const int STD = MinId + 137;
    public const int STDM = MinId + 138;
    public const int STDW = MinId + 139;
    public const int STI = MinId + 140;
    public const int STIM = MinId + 141;
    public const int STIW = MinId + 142;
    public const int STL = MinId + 143;
    public const int STLM = MinId + 144;
    public const int STLW = MinId + 145;
    public const int STM = MinId + 146;
    public const int STW = MinId + 147;
    public const int SUP = MinId + 148;
    public const int SX = MinId + 149;
    public const int SY = MinId + 150;
    public const int SZ = MinId + 151;
    public const int TM = MinId + 152;
    public const int TRP = MinId + 153;
    public const int UA = MinId + 154;
    public const int US = MinId + 155;
    public const int UZ = MinId + 156;
    public const int XR = MinId + 157;
    public const int XRC = MinId + 158;
    public const int XRCM = MinId + 159;
    public const int XRCW = MinId + 160;
    public const int XRM = MinId + 161;
    public const int XRW = MinId + 162;
    public const int Z = MinId + 163;

    public new static readonly Dictionary<int, string> Words = new Dictionary<int, string>()
    {

        { AD,"AD"},
        { ADB,"ADB"},
        { ADBCM,"ADBCM"},
        { ADBM,"ADBM"},
        { ADBW,"ADBW"},
        { ADC,"ADC"},
        { ADCW,"ADCW"},
        { ADW,"ADW"},
        { AN,"AN"},
        { ANC,"ANC"},
        { ANCM,"ANCM"},
        { ANCW,"ANCW"},
        { ANM,"ANM"},
        { ANW,"ANW"},
        { BDN,"BDN"},
        { BDNS,"BDNS"},
        { BID,"BID"},
        { BIDW,"BIDW"},
        { BIU,"BIU"},
        { BIUW,"BIUW"},
        { BUP,"BUP"},
        { BUPS,"BUPS"},
        { BYD,"BYD"},
        { BYDM,"BYDM"},
        { BYDW,"BYDW"},
        { BYU,"BYU"},
        { BYUM,"BYUM"},
        { BYUW,"BYUW"},
        { C,"C"},
        { CAL,"CAL"},
        { CANI,"CANI"},
        { CLT,"CLT"},
        { CMP,"CMP"},
        { CMPM,"CMPM"},
        { CMPW,"CMPW"},
        { DID,"DID"},
        { DIDM,"DIDM"},
        { DIDW,"DIDW"},
        { DIU,"DIU"},
        { DIUM,"DIUM"},
        { DIUW,"DIUW"},
        { DIW,"DIW"},
        { FST,"FST"},
        { GFL,"GFL"},
        { GFLW,"GFLW"},
        { GPO,"GPO"},
        { GPOW,"GPOW"},
        { GRE,"GRE"},
        { GSR,"GSR"},
        { GSRW,"GSRW"},
        { GST,"GST"},
        { IA,"IA"},
        { IB,"IB"},
        { IE,"IE"},
        { INV,"INV"},
        { INVM,"INVM"},
        { INVW,"INVW"},
        { IX,"IX"},
        { IY,"IY"},
        { IZ,"IZ"},
        { JP,"JP"},
        { JR,"JR"},
        { KY,"KY"},
        { LD,"LD"},
        { LDC,"LDC"},
        { LDCM,"LDCM"},
        { LDCW,"LDCW"},
        { LDD,"LDD"},
        { LDDM,"LDDM"},
        { LDDW,"LDDW"},
        { LDI,"LDI"},
        { LDIM,"LDIM"},
        { LDIW,"LDIW"},
        { LDL,"LDL"},
        { LDLM,"LDLM"},
        { LDLW,"LDLW"},
        { LDM,"LDM"},
        { LDW,"LDW"},
        { LNZ,"LNZ"},
        { LZ,"LZ"},
        { NA,"NA"},
        { NAC,"NAC"},
        { NACM,"NACM"},
        { NACW,"NACW"},
        { NAM,"NAM"},
        { NAW,"NAW"},
        { NC,"NC"},
        { NLZ,"NLZ"},
        { NOP,"NOP"},
        { NZ,"NZ"},
        { OFF,"OFF"},
        { ORC,"ORC"},
        { ORCM,"ORCM"},
        { ORCW,"ORCW"},
        { ORM,"ORM"},
        { ORW,"ORW"},
        { PD,"PD"},
        { PE,"PE"},
        { PFL,"PFL"},
        { PHS,"PHS"},
        { PHSM,"PHSM"},
        { PHSW,"PHSW"},
        { PHU,"PHU"},
        { PHUM,"PHUM"},
        { PHUW,"PHUW"},
        { PPO,"PPO"},
        { PPOM,"PPOM"},
        { PPOW,"PPOW"},
        { PPS,"PPS"},
        { PPSM,"PPSM"},
        { PPSW,"PPSW"},
        { PPU,"PPU"},
        { PPUM,"PPUM"},
        { PPUW,"PPUW"},
        { PRE,"PRE"},
        { PSR,"PSR"},
        { PSRM,"PSRM"},
        { PSRW,"PSRW"},
        { PST,"PST"},
        { ROD,"ROD"},
        { RODW,"RODW"},
        { ROU,"ROU"},
        { ROUW,"ROUW"},
        { RTN,"RTN"},
        { RTNI,"RTNI"},
        { SB,"SB"},
        { SBB,"SBB"},
        { SBBCM,"SBBCM"},
        { SBBM,"SBBM"},
        { SBBW,"SBBW"},
        { SBC,"SBC"},
        { SBCW,"SBCW"},
        { SBW,"SBW"},
        { SDN,"SDN"},
        { SLW,"SLW"},
        { SS,"SS"},
        { ST,"ST"},
        { STD,"STD"},
        { STDM,"STDM"},
        { STDW,"STDW"},
        { STI,"STI"},
        { STIM,"STIM"},
        { STIW,"STIW"},
        { STL,"STL"},
        { STLM,"STLM"},
        { STLW,"STLW"},
        { STM,"STM"},
        { STW,"STW"},
        { SUP,"SUP"},
        { SX,"SX"},
        { SY,"SY"},
        { SZ,"SZ"},
        { TM,"TM"},
        { TRP,"TRP"},
        { UA,"UA"},
        { US,"US"},
        { UZ,"UZ"},
        { XR,"XR"},
        { XRC,"XRC"},
        { XRCM,"XRCM"},
        { XRCW,"XRCW"},
        { XRM,"XRM"},
        { XRW,"XRW"},
        { Z,"Z"},
    };
}