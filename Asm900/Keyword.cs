namespace Inu.Assembler.Tlcs900;

internal class Keyword : Inu.Assembler.Keyword
{
    public new const int MinId = NextId;

    public const int A = MinId + 0;
    public const int ADash = MinId + 1;
    public const int ADC = MinId + 2;
    public const int ADCW = MinId + 3;
    public const int ADD = MinId + 4;
    public const int ADDW = MinId + 5;
    public const int ANDCF = MinId + 6;
    public const int ANDW = MinId + 7;
    public const int B = MinId + 8;
    public const int BDash = MinId + 9;
    public const int BC = MinId + 10;
    public const int BCDash = MinId + 11;
    public const int BIT = MinId + 12;
    public const int BS1B = MinId + 13;
    public const int BS1F = MinId + 14;
    public const int C = MinId + 15;
    public const int CDash = MinId + 16;
    public const int CALL = MinId + 17;
    public const int CALR = MinId + 18;
    public const int CCF = MinId + 19;
    public const int CHG = MinId + 20;
    public const int CP = MinId + 21;
    public const int CPD = MinId + 22;
    public const int CPDR = MinId + 23;
    public const int CPI = MinId + 24;
    public const int CPIR = MinId + 25;
    public const int CPL = MinId + 26;
    public const int CPW = MinId + 27;
    public const int D = MinId + 28;
    public const int Ddash = MinId + 29;
    public const int DAA = MinId + 30;
    public const int DD = MinId + 31;
    public const int DE = MinId + 32;
    public const int DEDash = MinId + 33;
    public const int DEC = MinId + 34;
    public const int DECF = MinId + 35;
    public const int DECW = MinId + 36;
    public const int DEFD = MinId + 37;
    public const int DI = MinId + 38;
    public const int DIV = MinId + 39;
    public const int DIVS = MinId + 40;
    public const int DJNZ = MinId + 41;
    public const int DMAC0 = MinId + 42;
    public const int DMAC1 = MinId + 43;
    public const int DMAC2 = MinId + 44;
    public const int DMAC3 = MinId + 45;
    public const int DMAC4 = MinId + 46;
    public const int DMAC5 = MinId + 47;
    public const int DMAC6 = MinId + 48;
    public const int DMAC7 = MinId + 49;
    public const int DMAD0 = MinId + 50;
    public const int DMAD1 = MinId + 51;
    public const int DMAD2 = MinId + 52;
    public const int DMAD3 = MinId + 53;
    public const int DMAD4 = MinId + 54;
    public const int DMAD5 = MinId + 55;
    public const int DMAD6 = MinId + 56;
    public const int DMAD7 = MinId + 57;
    public const int DMAM0 = MinId + 58;
    public const int DMAM1 = MinId + 59;
    public const int DMAM2 = MinId + 60;
    public const int DMAM3 = MinId + 61;
    public const int DMAM4 = MinId + 62;
    public const int DMAM5 = MinId + 63;
    public const int DMAM6 = MinId + 64;
    public const int DMAM7 = MinId + 65;
    public const int DMAS0 = MinId + 66;
    public const int DMAS1 = MinId + 67;
    public const int DMAS2 = MinId + 68;
    public const int DMAS3 = MinId + 69;
    public const int DMAS4 = MinId + 70;
    public const int DMAS5 = MinId + 71;
    public const int DMAS6 = MinId + 72;
    public const int DMAS7 = MinId + 73;
    public const int DWNZ = MinId + 74;
    public const int E = MinId + 75;
    public const int EDash = MinId + 76;
    public const int EI = MinId + 77;
    public const int EQ = MinId + 78;
    public const int EX = MinId + 79;
    public const int EXTS = MinId + 80;
    public const int EXTZ = MinId + 81;
    public const int F = MinId + 82;
    public const int FDash = MinId + 83;
    public const int GE = MinId + 84;
    public const int GT = MinId + 85;
    public const int H = MinId + 86;
    public const int Hdash = MinId + 87;
    public const int HALT = MinId + 88;
    public const int HL = MinId + 89;
    public const int HLDash = MinId + 90;
    public const int INC = MinId + 91;
    public const int INCF = MinId + 92;
    public const int INCW = MinId + 93;
    public const int INTNEST = MinId + 94;
    public const int IX = MinId + 95;
    public const int IXH = MinId + 96;
    public const int IXL = MinId + 97;
    public const int IY = MinId + 98;
    public const int IYH = MinId + 99;
    public const int IYL = MinId + 100;
    public const int IZ = MinId + 101;
    public const int IZH = MinId + 102;
    public const int IZL = MinId + 103;
    public const int JP = MinId + 104;
    public const int JR = MinId + 105;
    public const int JRL = MinId + 106;
    public const int L = MinId + 107;
    public const int LDash = MinId + 108;
    public const int LD = MinId + 109;
    public const int LDA = MinId + 110;
    public const int LDAR = MinId + 111;
    public const int LDC = MinId + 112;
    public const int LDCF = MinId + 113;
    public const int LDD = MinId + 114;
    public const int LDDR = MinId + 115;
    public const int LDDRW = MinId + 116;
    public const int LDDW = MinId + 117;
    public const int LDF = MinId + 118;
    public const int LDI = MinId + 119;
    public const int LDIR = MinId + 120;
    public const int LDIRW = MinId + 121;
    public const int LDIW = MinId + 122;
    public const int LDW = MinId + 123;
    public const int LE = MinId + 124;
    public const int LINK = MinId + 125;
    public const int LT = MinId + 126;
    public const int M = MinId + 127;
    public const int MDEC1 = MinId + 128;
    public const int MDEC2 = MinId + 129;
    public const int MDEC4 = MinId + 130;
    public const int MI = MinId + 131;
    public const int MINC1 = MinId + 132;
    public const int MINC2 = MinId + 133;
    public const int MINC4 = MinId + 134;
    public const int MIRR = MinId + 135;
    public const int MUL = MinId + 136;
    public const int MULA = MinId + 137;
    public const int MULS = MinId + 138;
    public const int NC = MinId + 139;
    public const int NE = MinId + 140;
    public const int NEG = MinId + 141;
    public const int NOP = MinId + 142;
    public const int NOV = MinId + 143;
    public const int NZ = MinId + 144;
    public const int ORCF = MinId + 145;
    public const int ORW = MinId + 146;
    public const int OV = MinId + 147;
    public const int P = MinId + 148;
    public const int PAA = MinId + 149;
    public const int PE = MinId + 150;
    public const int PL = MinId + 151;
    public const int PO = MinId + 152;
    public const int POP = MinId + 153;
    public const int POPW = MinId + 154;
    public const int PUSH = MinId + 155;
    public const int PUSHW = MinId + 156;
    public const int QA = MinId + 157;
    public const int QADash = MinId + 158;
    public const int QA0 = MinId + 159;
    public const int QA1 = MinId + 160;
    public const int QA2 = MinId + 161;
    public const int QA3 = MinId + 162;
    public const int QB = MinId + 163;
    public const int QBDash = MinId + 164;
    public const int QB0 = MinId + 165;
    public const int QB1 = MinId + 166;
    public const int QB2 = MinId + 167;
    public const int QB3 = MinId + 168;
    public const int QBC = MinId + 169;
    public const int QBCDash = MinId + 170;
    public const int QBC0 = MinId + 171;
    public const int QBC1 = MinId + 172;
    public const int QBC2 = MinId + 173;
    public const int QBC3 = MinId + 174;
    public const int QC = MinId + 175;
    public const int QCDash = MinId + 176;
    public const int QC0 = MinId + 177;
    public const int QC1 = MinId + 178;
    public const int QC2 = MinId + 179;
    public const int QC3 = MinId + 180;
    public const int QD = MinId + 181;
    public const int QDDash = MinId + 182;
    public const int QD0 = MinId + 183;
    public const int QD1 = MinId + 184;
    public const int QD2 = MinId + 185;
    public const int QD3 = MinId + 186;
    public const int QDE = MinId + 187;
    public const int QDEDash = MinId + 188;
    public const int QDE0 = MinId + 189;
    public const int QDE1 = MinId + 190;
    public const int QDE2 = MinId + 191;
    public const int QDE3 = MinId + 192;
    public const int QE = MinId + 193;
    public const int QEDash = MinId + 194;
    public const int QE0 = MinId + 195;
    public const int QE1 = MinId + 196;
    public const int QE2 = MinId + 197;
    public const int QE3 = MinId + 198;
    public const int QH = MinId + 199;
    public const int QHDash = MinId + 200;
    public const int QH0 = MinId + 201;
    public const int QH1 = MinId + 202;
    public const int QH2 = MinId + 203;
    public const int QH3 = MinId + 204;
    public const int QHL = MinId + 205;
    public const int QHLDash = MinId + 206;
    public const int QHL0 = MinId + 207;
    public const int QHL1 = MinId + 208;
    public const int QHL2 = MinId + 209;
    public const int QHL3 = MinId + 210;
    public const int QIX = MinId + 211;
    public const int QIXH = MinId + 212;
    public const int QIXL = MinId + 213;
    public const int QIY = MinId + 214;
    public const int QIYH = MinId + 215;
    public const int QIYL = MinId + 216;
    public const int QIZ = MinId + 217;
    public const int QIZH = MinId + 218;
    public const int QIZL = MinId + 219;
    public const int QL = MinId + 220;
    public const int QLDash = MinId + 221;
    public const int QL0 = MinId + 222;
    public const int QL1 = MinId + 223;
    public const int QL2 = MinId + 224;
    public const int QL3 = MinId + 225;
    public const int QSP = MinId + 226;
    public const int QSPH = MinId + 227;
    public const int QSPL = MinId + 228;
    public const int QW = MinId + 229;
    public const int QWDash = MinId + 230;
    public const int QW0 = MinId + 231;
    public const int QW1 = MinId + 232;
    public const int QW2 = MinId + 233;
    public const int QW3 = MinId + 234;
    public const int QWA = MinId + 235;
    public const int QWADash = MinId + 236;
    public const int QWA0 = MinId + 237;
    public const int QWA1 = MinId + 238;
    public const int QWA2 = MinId + 239;
    public const int QWA3 = MinId + 240;
    public const int RA0 = MinId + 241;
    public const int RA1 = MinId + 242;
    public const int RA2 = MinId + 243;
    public const int RA3 = MinId + 244;
    public const int RB0 = MinId + 245;
    public const int RB1 = MinId + 246;
    public const int RB2 = MinId + 247;
    public const int RB3 = MinId + 248;
    public const int RBC0 = MinId + 249;
    public const int RBC1 = MinId + 250;
    public const int RBC2 = MinId + 251;
    public const int RBC3 = MinId + 252;
    public const int RC0 = MinId + 253;
    public const int RC1 = MinId + 254;
    public const int RC2 = MinId + 255;
    public const int RC3 = MinId + 256;
    public const int RCF = MinId + 257;
    public const int RD0 = MinId + 258;
    public const int RD1 = MinId + 259;
    public const int RD2 = MinId + 260;
    public const int RD3 = MinId + 261;
    public const int RDE0 = MinId + 262;
    public const int RDE1 = MinId + 263;
    public const int RDE2 = MinId + 264;
    public const int RDE3 = MinId + 265;
    public const int RE0 = MinId + 266;
    public const int RE1 = MinId + 267;
    public const int RE2 = MinId + 268;
    public const int RE3 = MinId + 269;
    public const int RES = MinId + 270;
    public const int RET = MinId + 271;
    public const int RETD = MinId + 272;
    public const int RETI = MinId + 273;
    public const int RH0 = MinId + 274;
    public const int RH1 = MinId + 275;
    public const int RH2 = MinId + 276;
    public const int RH3 = MinId + 277;
    public const int RHL0 = MinId + 278;
    public const int RHL1 = MinId + 279;
    public const int RHL2 = MinId + 280;
    public const int RHL3 = MinId + 281;
    public const int RL = MinId + 282;
    public const int RL0 = MinId + 283;
    public const int RL1 = MinId + 284;
    public const int RL2 = MinId + 285;
    public const int RL3 = MinId + 286;
    public const int RLC = MinId + 287;
    public const int RLCW = MinId + 288;
    public const int RLD = MinId + 289;
    public const int RLW = MinId + 290;
    public const int RR = MinId + 291;
    public const int RRC = MinId + 292;
    public const int RRCW = MinId + 293;
    public const int RRD = MinId + 294;
    public const int RRW = MinId + 295;
    public const int RW0 = MinId + 296;
    public const int RW1 = MinId + 297;
    public const int RW2 = MinId + 298;
    public const int RW3 = MinId + 299;
    public const int RWA0 = MinId + 300;
    public const int RWA1 = MinId + 301;
    public const int RWA2 = MinId + 302;
    public const int RWA3 = MinId + 303;
    public const int SBC = MinId + 304;
    public const int SBCW = MinId + 305;
    public const int SCC = MinId + 306;
    public const int SCF = MinId + 307;
    public const int SET = MinId + 308;
    public const int SLA = MinId + 309;
    public const int SLAW = MinId + 310;
    public const int SLL = MinId + 311;
    public const int SLLW = MinId + 312;
    public const int SP = MinId + 313;
    public const int SPH = MinId + 314;
    public const int SPL = MinId + 315;
    public const int SR = MinId + 316;
    public const int SRA = MinId + 317;
    public const int SRAW = MinId + 318;
    public const int SRL = MinId + 319;
    public const int SRLW = MinId + 320;
    public const int STCF = MinId + 321;
    public const int SUB = MinId + 322;
    public const int SUBW = MinId + 323;
    public const int SWI = MinId + 324;
    public const int TSET = MinId + 325;
    public const int UGE = MinId + 326;
    public const int UGT = MinId + 327;
    public const int ULE = MinId + 328;
    public const int ULT = MinId + 329;
    public const int UNLK = MinId + 330;
    public const int W = MinId + 331;
    public const int WDash = MinId + 332;
    public const int WA = MinId + 333;
    public const int WADash = MinId + 334;
    public const int XBC = MinId + 335;
    public const int XBCDash = MinId + 336;
    public const int XBC0 = MinId + 337;
    public const int XBC1 = MinId + 338;
    public const int XBC2 = MinId + 339;
    public const int XBC3 = MinId + 340;
    public const int XDE = MinId + 341;
    public const int XDEDash = MinId + 342;
    public const int XDE0 = MinId + 343;
    public const int XDE1 = MinId + 344;
    public const int XDE2 = MinId + 345;
    public const int XDE3 = MinId + 346;
    public const int XHL = MinId + 347;
    public const int XHLDash = MinId + 348;
    public const int XHL0 = MinId + 349;
    public const int XHL1 = MinId + 350;
    public const int XHL2 = MinId + 351;
    public const int XHL3 = MinId + 352;
    public const int XIX = MinId + 353;
    public const int XIY = MinId + 354;
    public const int XIZ = MinId + 355;
    public const int XORCF = MinId + 356;
    public const int XORW = MinId + 357;
    public const int XSP = MinId + 358;
    public const int XWA = MinId + 359;
    public const int XWADash = MinId + 360;
    public const int XWA0 = MinId + 361;
    public const int XWA1 = MinId + 362;
    public const int XWA2 = MinId + 363;
    public const int XWA3 = MinId + 364;
    public const int Z = MinId + 365;
    public const int ZCF = MinId + 366;

    public new static readonly Dictionary<int, string> Words = new()
    {

{ A,"A"},
{ ADash,"A'"},
{ ADC,"ADC"},
{ ADCW,"ADCW"},
{ ADD,"ADD"},
{ ADDW,"ADDW"},
{ ANDCF,"ANDCF"},
{ ANDW,"ANDW"},
{ B,"B"},
{ BDash,"B'"},
{ BC,"BC"},
{ BCDash,"BC'"},
{ BIT,"BIT"},
{ BS1B,"BS1B"},
{ BS1F,"BS1F"},
{ C,"C"},
{ CDash,"C'"},
{ CALL,"CALL"},
{ CALR,"CALR"},
{ CCF,"CCF"},
{ CHG,"CHG"},
{ CP,"CP"},
{ CPD,"CPD"},
{ CPDR,"CPDR"},
{ CPI,"CPI"},
{ CPIR,"CPIR"},
{ CPL,"CPL"},
{ CPW,"CPW"},
{ D,"D"},
{ Ddash,"D'"},
{ DAA,"DAA"},
{ DD,"DD"},
{ DE,"DE"},
{ DEDash,"DE'"},
{ DEC,"DEC"},
{ DECF,"DECF"},
{ DECW,"DECW"},
{ DEFD,"DEFD"},
{ DI,"DI"},
{ DIV,"DIV"},
{ DIVS,"DIVS"},
{ DJNZ,"DJNZ"},
{ DMAC0,"DMAC0"},
{ DMAC1,"DMAC1"},
{ DMAC2,"DMAC2"},
{ DMAC3,"DMAC3"},
{ DMAC4,"DMAC4"},
{ DMAC5,"DMAC5"},
{ DMAC6,"DMAC6"},
{ DMAC7,"DMAC7"},
{ DMAD0,"DMAD0"},
{ DMAD1,"DMAD1"},
{ DMAD2,"DMAD2"},
{ DMAD3,"DMAD3"},
{ DMAD4,"DMAD4"},
{ DMAD5,"DMAD5"},
{ DMAD6,"DMAD6"},
{ DMAD7,"DMAD7"},
{ DMAM0,"DMAM0"},
{ DMAM1,"DMAM1"},
{ DMAM2,"DMAM2"},
{ DMAM3,"DMAM3"},
{ DMAM4,"DMAM4"},
{ DMAM5,"DMAM5"},
{ DMAM6,"DMAM6"},
{ DMAM7,"DMAM7"},
{ DMAS0,"DMAS0"},
{ DMAS1,"DMAS1"},
{ DMAS2,"DMAS2"},
{ DMAS3,"DMAS3"},
{ DMAS4,"DMAS4"},
{ DMAS5,"DMAS5"},
{ DMAS6,"DMAS6"},
{ DMAS7,"DMAS7"},
{ DWNZ,"DWNZ"},
{ E,"E"},
{ EDash,"E'"},
{ EI,"EI"},
{ EQ,"EQ"},
{ EX,"EX"},
{ EXTS,"EXTS"},
{ EXTZ,"EXTZ"},
{ F,"F"},
{ FDash,"F'"},
{ GE,"GE"},
{ GT,"GT"},
{ H,"H"},
{ Hdash,"H'"},
{ HALT,"HALT"},
{ HL,"HL"},
{ HLDash,"HL'"},
{ INC,"INC"},
{ INCF,"INCF"},
{ INCW,"INCW"},
{ INTNEST,"INTNEST"},
{ IX,"IX"},
{ IXH,"IXH"},
{ IXL,"IXL"},
{ IY,"IY"},
{ IYH,"IYH"},
{ IYL,"IYL"},
{ IZ,"IZ"},
{ IZH,"IZH"},
{ IZL,"IZL"},
{ JP,"JP"},
{ JR,"JR"},
{ JRL,"JRL"},
{ L,"L"},
{ LDash,"L'"},
{ LD,"LD"},
{ LDA,"LDA"},
{ LDAR,"LDAR"},
{ LDC,"LDC"},
{ LDCF,"LDCF"},
{ LDD,"LDD"},
{ LDDR,"LDDR"},
{ LDDRW,"LDDRW"},
{ LDDW,"LDDW"},
{ LDF,"LDF"},
{ LDI,"LDI"},
{ LDIR,"LDIR"},
{ LDIRW,"LDIRW"},
{ LDIW,"LDIW"},
{ LDW,"LDW"},
{ LE,"LE"},
{ LINK,"LINK"},
{ LT,"LT"},
{ M,"M"},
{ MDEC1,"MDEC1"},
{ MDEC2,"MDEC2"},
{ MDEC4,"MDEC4"},
{ MI,"MI"},
{ MINC1,"MINC1"},
{ MINC2,"MINC2"},
{ MINC4,"MINC4"},
{ MIRR,"MIRR"},
{ MUL,"MUL"},
{ MULA,"MULA"},
{ MULS,"MULS"},
{ NC,"NC"},
{ NE,"NE"},
{ NEG,"NEG"},
{ NOP,"NOP"},
{ NOV,"NOV"},
{ NZ,"NZ"},
{ ORCF,"ORCF"},
{ ORW,"ORW"},
{ OV,"OV"},
{ P,"P"},
{ PAA,"PAA"},
{ PE,"PE"},
{ PL,"PL"},
{ PO,"PO"},
{ POP,"POP"},
{ POPW,"POPW"},
{ PUSH,"PUSH"},
{ PUSHW,"PUSHW"},
{ QA,"QA"},
{ QADash,"QA'"},
{ QA0,"QA0"},
{ QA1,"QA1"},
{ QA2,"QA2"},
{ QA3,"QA3"},
{ QB,"QB"},
{ QBDash,"QB'"},
{ QB0,"QB0"},
{ QB1,"QB1"},
{ QB2,"QB2"},
{ QB3,"QB3"},
{ QBC,"QBC"},
{ QBCDash,"QBC'"},
{ QBC0,"QBC0"},
{ QBC1,"QBC1"},
{ QBC2,"QBC2"},
{ QBC3,"QBC3"},
{ QC,"QC"},
{ QCDash,"QC'"},
{ QC0,"QC0"},
{ QC1,"QC1"},
{ QC2,"QC2"},
{ QC3,"QC3"},
{ QD,"QD"},
{ QDDash,"QD'"},
{ QD0,"QD0"},
{ QD1,"QD1"},
{ QD2,"QD2"},
{ QD3,"QD3"},
{ QDE,"QDE"},
{ QDEDash,"QDE'"},
{ QDE0,"QDE0"},
{ QDE1,"QDE1"},
{ QDE2,"QDE2"},
{ QDE3,"QDE3"},
{ QE,"QE"},
{ QEDash,"QE'"},
{ QE0,"QE0"},
{ QE1,"QE1"},
{ QE2,"QE2"},
{ QE3,"QE3"},
{ QH,"QH"},
{ QHDash,"QH'"},
{ QH0,"QH0"},
{ QH1,"QH1"},
{ QH2,"QH2"},
{ QH3,"QH3"},
{ QHL,"QHL"},
{ QHLDash,"QHL'"},
{ QHL0,"QHL0"},
{ QHL1,"QHL1"},
{ QHL2,"QHL2"},
{ QHL3,"QHL3"},
{ QIX,"QIX"},
{ QIXH,"QIXH"},
{ QIXL,"QIXL"},
{ QIY,"QIY"},
{ QIYH,"QIYH"},
{ QIYL,"QIYL"},
{ QIZ,"QIZ"},
{ QIZH,"QIZH"},
{ QIZL,"QIZL"},
{ QL,"QL"},
{ QLDash,"QL'"},
{ QL0,"QL0"},
{ QL1,"QL1"},
{ QL2,"QL2"},
{ QL3,"QL3"},
{ QSP,"QSP"},
{ QSPH,"QSPH"},
{ QSPL,"QSPL"},
{ QW,"QW"},
{ QWDash,"QW'"},
{ QW0,"QW0"},
{ QW1,"QW1"},
{ QW2,"QW2"},
{ QW3,"QW3"},
{ QWA,"QWA"},
{ QWADash,"QWA'"},
{ QWA0,"QWA0"},
{ QWA1,"QWA1"},
{ QWA2,"QWA2"},
{ QWA3,"QWA3"},
{ RA0,"RA0"},
{ RA1,"RA1"},
{ RA2,"RA2"},
{ RA3,"RA3"},
{ RB0,"RB0"},
{ RB1,"RB1"},
{ RB2,"RB2"},
{ RB3,"RB3"},
{ RBC0,"RBC0"},
{ RBC1,"RBC1"},
{ RBC2,"RBC2"},
{ RBC3,"RBC3"},
{ RC0,"RC0"},
{ RC1,"RC1"},
{ RC2,"RC2"},
{ RC3,"RC3"},
{ RCF,"RCF"},
{ RD0,"RD0"},
{ RD1,"RD1"},
{ RD2,"RD2"},
{ RD3,"RD3"},
{ RDE0,"RDE0"},
{ RDE1,"RDE1"},
{ RDE2,"RDE2"},
{ RDE3,"RDE3"},
{ RE0,"RE0"},
{ RE1,"RE1"},
{ RE2,"RE2"},
{ RE3,"RE3"},
{ RES,"RES"},
{ RET,"RET"},
{ RETD,"RETD"},
{ RETI,"RETI"},
{ RH0,"RH0"},
{ RH1,"RH1"},
{ RH2,"RH2"},
{ RH3,"RH3"},
{ RHL0,"RHL0"},
{ RHL1,"RHL1"},
{ RHL2,"RHL2"},
{ RHL3,"RHL3"},
{ RL,"RL"},
{ RL0,"RL0"},
{ RL1,"RL1"},
{ RL2,"RL2"},
{ RL3,"RL3"},
{ RLC,"RLC"},
{ RLCW,"RLCW"},
{ RLD,"RLD"},
{ RLW,"RLW"},
{ RR,"RR"},
{ RRC,"RRC"},
{ RRCW,"RRCW"},
{ RRD,"RRD"},
{ RRW,"RRW"},
{ RW0,"RW0"},
{ RW1,"RW1"},
{ RW2,"RW2"},
{ RW3,"RW3"},
{ RWA0,"RWA0"},
{ RWA1,"RWA1"},
{ RWA2,"RWA2"},
{ RWA3,"RWA3"},
{ SBC,"SBC"},
{ SBCW,"SBCW"},
{ SCC,"SCC"},
{ SCF,"SCF"},
{ SET,"SET"},
{ SLA,"SLA"},
{ SLAW,"SLAW"},
{ SLL,"SLL"},
{ SLLW,"SLLW"},
{ SP,"SP"},
{ SPH,"SPH"},
{ SPL,"SPL"},
{ SR,"SR"},
{ SRA,"SRA"},
{ SRAW,"SRAW"},
{ SRL,"SRL"},
{ SRLW,"SRLW"},
{ STCF,"STCF"},
{ SUB,"SUB"},
{ SUBW,"SUBW"},
{ SWI,"SWI"},
{ TSET,"TSET"},
{ UGE,"UGE"},
{ UGT,"UGT"},
{ ULE,"ULE"},
{ ULT,"ULT"},
{ UNLK,"UNLK"},
{ W,"W"},
{ WDash,"W'"},
{ WA,"WA"},
{ WADash,"WA'"},
{ XBC,"XBC"},
{ XBCDash,"XBC'"},
{ XBC0,"XBC0"},
{ XBC1,"XBC1"},
{ XBC2,"XBC2"},
{ XBC3,"XBC3"},
{ XDE,"XDE"},
{ XDEDash,"XDE'"},
{ XDE0,"XDE0"},
{ XDE1,"XDE1"},
{ XDE2,"XDE2"},
{ XDE3,"XDE3"},
{ XHL,"XHL"},
{ XHLDash,"XHL'"},
{ XHL0,"XHL0"},
{ XHL1,"XHL1"},
{ XHL2,"XHL2"},
{ XHL3,"XHL3"},
{ XIX,"XIX"},
{ XIY,"XIY"},
{ XIZ,"XIZ"},
{ XORCF,"XORCF"},
{ XORW,"XORW"},
{ XSP,"XSP"},
{ XWA,"XWA"},
{ XWADash,"XWA'"},
{ XWA0,"XWA0"},
{ XWA1,"XWA1"},
{ XWA2,"XWA2"},
{ XWA3,"XWA3"},
{ Z,"Z"},
{ ZCF,"ZCF"},
    };
}