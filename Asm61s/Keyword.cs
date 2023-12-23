namespace Inu.Assembler.SC61860;

internal class Keyword : Inu.Assembler.Keyword
{
    public new const int MinId = NextId;

    public const int ADB = MinId + 0;
    public const int ADCM = MinId + 1;
    public const int ADIA = MinId + 2;
    public const int ADIM = MinId + 3;
    public const int ADM = MinId + 4;
    public const int ADN = MinId + 5;
    public const int ADW = MinId + 6;
    public const int ANIA = MinId + 7;
    public const int ANID = MinId + 8;
    public const int ANIM = MinId + 9;
    public const int ANMA = MinId + 10;
    public const int C = MinId + 11;
    public const int CAL = MinId + 12;
    public const int CALL = MinId + 13;
    public const int CASE1 = MinId + 14;
    public const int CASE2 = MinId + 15;
    public const int CDN = MinId + 16;
    public const int CPIA = MinId + 17;
    public const int CPIM = MinId + 18;
    public const int CPMA = MinId + 19;
    public const int CUP = MinId + 20;
    public const int DECA = MinId + 21;
    public const int DECB = MinId + 22;
    public const int DECI = MinId + 23;
    public const int DECJ = MinId + 24;
    public const int DECK = MinId + 25;
    public const int DECL = MinId + 26;
    public const int DECM = MinId + 27;
    public const int DECN = MinId + 28;
    public const int DECP = MinId + 29;
    public const int DX = MinId + 30;
    public const int DXL = MinId + 31;
    public const int DY = MinId + 32;
    public const int DYS = MinId + 33;
    public const int EXAB = MinId + 34;
    public const int EXAM = MinId + 35;
    public const int EXB = MinId + 36;
    public const int EXBD = MinId + 37;
    public const int EXW = MinId + 38;
    public const int EXWD = MinId + 39;
    public const int FILD = MinId + 40;
    public const int FILM = MinId + 41;
    public const int INA = MinId + 42;
    public const int INB = MinId + 43;
    public const int INCA = MinId + 44;
    public const int INCB = MinId + 45;
    public const int INCI = MinId + 46;
    public const int INCJ = MinId + 47;
    public const int INCK = MinId + 48;
    public const int INCL = MinId + 49;
    public const int INCM = MinId + 50;
    public const int INCN = MinId + 51;
    public const int INCP = MinId + 52;
    public const int IX = MinId + 53;
    public const int IXL = MinId + 54;
    public const int IY = MinId + 55;
    public const int IYS = MinId + 56;
    public const int JP = MinId + 57;
    public const int JPC = MinId + 58;
    public const int JPNC = MinId + 59;
    public const int JPNZ = MinId + 60;
    public const int JPZ = MinId + 61;
    public const int JR = MinId + 62;
    public const int JRC = MinId + 63;
    public const int JRCM = MinId + 64;
    public const int JRCP = MinId + 65;
    public const int JRM = MinId + 66;
    public const int JRNC = MinId + 67;
    public const int JRNCM = MinId + 68;
    public const int JRNCP = MinId + 69;
    public const int JRNZ = MinId + 70;
    public const int JRNZM = MinId + 71;
    public const int JRNZP = MinId + 72;
    public const int JRP = MinId + 73;
    public const int JRZ = MinId + 74;
    public const int JRZM = MinId + 75;
    public const int JRZP = MinId + 76;
    public const int LDD = MinId + 77;
    public const int LDM = MinId + 78;
    public const int LDP = MinId + 79;
    public const int LDQ = MinId + 80;
    public const int LDR = MinId + 81;
    public const int LEAVE = MinId + 82;
    public const int LIA = MinId + 83;
    public const int LIB = MinId + 84;
    public const int LIDL = MinId + 85;
    public const int LIDP = MinId + 86;
    public const int LII = MinId + 87;
    public const int LIJ = MinId + 88;
    public const int LIP = MinId + 89;
    public const int LIQ = MinId + 90;
    public const int LOOP = MinId + 91;
    public const int LP = MinId + 92;
    public const int MVB = MinId + 93;
    public const int MVBD = MinId + 94;
    public const int MVDM = MinId + 95;
    public const int MVMD = MinId + 96;
    public const int MVW = MinId + 97;
    public const int MVWD = MinId + 98;
    public const int NC = MinId + 99;
    public const int NOPT = MinId + 100;
    public const int NOPW = MinId + 101;
    public const int NZ = MinId + 102;
    public const int ORIA = MinId + 103;
    public const int ORID = MinId + 104;
    public const int ORIM = MinId + 105;
    public const int ORMA = MinId + 106;
    public const int OUTA = MinId + 107;
    public const int OUTB = MinId + 108;
    public const int OUTC = MinId + 109;
    public const int OUTF = MinId + 110;
    public const int POP = MinId + 111;
    public const int PUSH = MinId + 112;
    public const int RC = MinId + 113;
    public const int RTN = MinId + 114;
    public const int SBB = MinId + 115;
    public const int SBCM = MinId + 116;
    public const int SBIA = MinId + 117;
    public const int SBIM = MinId + 118;
    public const int SBM = MinId + 119;
    public const int SBN = MinId + 120;
    public const int SBW = MinId + 121;
    public const int SC = MinId + 122;
    public const int SL = MinId + 123;
    public const int SLW = MinId + 124;
    public const int SR = MinId + 125;
    public const int SRW = MinId + 126;
    public const int STD = MinId + 127;
    public const int STP = MinId + 128;
    public const int STQ = MinId + 129;
    public const int STR = MinId + 130;
    public const int SWP = MinId + 131;
    public const int TEST = MinId + 132;
    public const int TSIA = MinId + 133;
    public const int TSID = MinId + 134;
    public const int TSIM = MinId + 135;
    public const int TSMA = MinId + 136;
    public const int WAIT = MinId + 137;
    public const int Z = MinId + 138;

    public new static readonly Dictionary<int, string> Words = new()
    {

        { ADB,"ADB"},
        { ADCM,"ADCM"},
        { ADIA,"ADIA"},
        { ADIM,"ADIM"},
        { ADM,"ADM"},
        { ADN,"ADN"},
        { ADW,"ADW"},
        { ANIA,"ANIA"},
        { ANID,"ANID"},
        { ANIM,"ANIM"},
        { ANMA,"ANMA"},
        { C,"C"},
        { CAL,"CAL"},
        { CALL,"CALL"},
        { CASE1,"CASE1"},
        { CASE2,"CASE2"},
        { CDN,"CDN"},
        { CPIA,"CPIA"},
        { CPIM,"CPIM"},
        { CPMA,"CPMA"},
        { CUP,"CUP"},
        { DECA,"DECA"},
        { DECB,"DECB"},
        { DECI,"DECI"},
        { DECJ,"DECJ"},
        { DECK,"DECK"},
        { DECL,"DECL"},
        { DECM,"DECM"},
        { DECN,"DECN"},
        { DECP,"DECP"},
        { DX,"DX"},
        { DXL,"DXL"},
        { DY,"DY"},
        { DYS,"DYS"},
        { EXAB,"EXAB"},
        { EXAM,"EXAM"},
        { EXB,"EXB"},
        { EXBD,"EXBD"},
        { EXW,"EXW"},
        { EXWD,"EXWD"},
        { FILD,"FILD"},
        { FILM,"FILM"},
        { INA,"INA"},
        { INB,"INB"},
        { INCA,"INCA"},
        { INCB,"INCB"},
        { INCI,"INCI"},
        { INCJ,"INCJ"},
        { INCK,"INCK"},
        { INCL,"INCL"},
        { INCM,"INCM"},
        { INCN,"INCN"},
        { INCP,"INCP"},
        { IX,"IX"},
        { IXL,"IXL"},
        { IY,"IY"},
        { IYS,"IYS"},
        { JP,"JP"},
        { JPC,"JPC"},
        { JPNC,"JPNC"},
        { JPNZ,"JPNZ"},
        { JPZ,"JPZ"},
        { JR,"JR"},
        { JRC,"JRC"},
        { JRCM,"JRCM"},
        { JRCP,"JRCP"},
        { JRM,"JRM"},
        { JRNC,"JRNC"},
        { JRNCM,"JRNCM"},
        { JRNCP,"JRNCP"},
        { JRNZ,"JRNZ"},
        { JRNZM,"JRNZM"},
        { JRNZP,"JRNZP"},
        { JRP,"JRP"},
        { JRZ,"JRZ"},
        { JRZM,"JRZM"},
        { JRZP,"JRZP"},
        { LDD,"LDD"},
        { LDM,"LDM"},
        { LDP,"LDP"},
        { LDQ,"LDQ"},
        { LDR,"LDR"},
        { LEAVE,"LEAVE"},
        { LIA,"LIA"},
        { LIB,"LIB"},
        { LIDL,"LIDL"},
        { LIDP,"LIDP"},
        { LII,"LII"},
        { LIJ,"LIJ"},
        { LIP,"LIP"},
        { LIQ,"LIQ"},
        { LOOP,"LOOP"},
        { LP,"LP"},
        { MVB,"MVB"},
        { MVBD,"MVBD"},
        { MVDM,"MVDM"},
        { MVMD,"MVMD"},
        { MVW,"MVW"},
        { MVWD,"MVWD"},
        { NC,"NC"},
        { NOPT,"NOPT"},
        { NOPW,"NOPW"},
        { NZ,"NZ"},
        { ORIA,"ORIA"},
        { ORID,"ORID"},
        { ORIM,"ORIM"},
        { ORMA,"ORMA"},
        { OUTA,"OUTA"},
        { OUTB,"OUTB"},
        { OUTC,"OUTC"},
        { OUTF,"OUTF"},
        { POP,"POP"},
        { PUSH,"PUSH"},
        { RC,"RC"},
        { RTN,"RTN"},
        { SBB,"SBB"},
        { SBCM,"SBCM"},
        { SBIA,"SBIA"},
        { SBIM,"SBIM"},
        { SBM,"SBM"},
        { SBN,"SBN"},
        { SBW,"SBW"},
        { SC,"SC"},
        { SL,"SL"},
        { SLW,"SLW"},
        { SR,"SR"},
        { SRW,"SRW"},
        { STD,"STD"},
        { STP,"STP"},
        { STQ,"STQ"},
        { STR,"STR"},
        { SWP,"SWP"},
        { TEST,"TEST"},
        { TSIA,"TSIA"},
        { TSID,"TSID"},
        { TSIM,"TSIM"},
        { TSMA,"TSMA"},
        { WAIT,"WAIT"},
        { Z,"Z"},
    };
}