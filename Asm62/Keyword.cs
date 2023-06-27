namespace Inu.Assembler.Sc62015
{
    internal class Keyword : Inu.Assembler.Keyword
    {
        public new const int MinId = NextId;

        public const int A = MinId + 0;
        public const int ADC = MinId + 1;
        public const int ADCL = MinId + 2;
        public const int ADD = MinId + 3;
        public const int B = MinId + 4;
        public const int BA = MinId + 5;
        public const int BH = MinId + 6;
        public const int BL = MinId + 7;
        public const int BP = MinId + 8;
        public const int BX = MinId + 9;
        public const int C = MinId + 10;
        public const int CALL = MinId + 11;
        public const int CALLF = MinId + 12;
        public const int CH = MinId + 13;
        public const int CL = MinId + 14;
        public const int CMP = MinId + 15;
        public const int CMPP = MinId + 16;
        public const int CMPW = MinId + 17;
        public const int CX = MinId + 18;
        public const int DADL = MinId + 19;
        public const int DEC = MinId + 20;
        public const int Decrement = MinId + 21;
        public const int DH = MinId + 22;
        public const int DI = MinId + 23;
        public const int DL = MinId + 24;
        public const int DSBL = MinId + 25;
        public const int DSLL = MinId + 26;
        public const int DSRL = MinId + 27;
        public const int DX = MinId + 28;
        public const int EX = MinId + 29;
        public const int EXL = MinId + 30;
        public const int EXP = MinId + 31;
        public const int EXW = MinId + 32;
        public const int F = MinId + 33;
        public const int HALT = MinId + 34;
        public const int I = MinId + 35;
        public const int IL = MinId + 36;
        public const int IMR = MinId + 37;
        public const int INC = MinId + 38;
        public const int Increment = MinId + 39;
        public const int IR = MinId + 40;
        public const int JP = MinId + 41;
        public const int JPC = MinId + 42;
        public const int JPF = MinId + 43;
        public const int JPNC = MinId + 44;
        public const int JPNZ = MinId + 45;
        public const int JPZ = MinId + 46;
        public const int JR = MinId + 47;
        public const int JRC = MinId + 48;
        public const int JRNC = MinId + 49;
        public const int JRNZ = MinId + 50;
        public const int JRZ = MinId + 51;
        public const int MV = MinId + 52;
        public const int MVL = MinId + 53;
        public const int MVLD = MinId + 54;
        public const int MVP = MinId + 55;
        public const int MVW = MinId + 56;
        public const int NC = MinId + 57;
        public const int NOP = MinId + 58;
        public const int NZ = MinId + 59;
        public const int OFF = MinId + 60;
        public const int PMDF = MinId + 61;
        public const int POPS = MinId + 62;
        public const int POPU = MinId + 63;
        public const int PUSHS = MinId + 64;
        public const int PUSHU = MinId + 65;
        public const int PX = MinId + 66;
        public const int PY = MinId + 67;
        public const int RC = MinId + 68;
        public const int RESET = MinId + 69;
        public const int RET = MinId + 70;
        public const int RETF = MinId + 71;
        public const int RETI = MinId + 72;
        public const int ROL = MinId + 73;
        public const int ROR = MinId + 74;
        public const int S = MinId + 75;
        public const int SBC = MinId + 76;
        public const int SBCL = MinId + 77;
        public const int SC = MinId + 78;
        public const int SI = MinId + 79;
        public const int SUB = MinId + 80;
        public const int SWAP = MinId + 81;
        public const int TEST = MinId + 82;
        public const int U = MinId + 83;
        public const int WAIT = MinId + 84;
        public const int X = MinId + 85;
        public const int Y = MinId + 86;
        public const int Z = MinId + 87;

        public new static readonly Dictionary<int, string> Words = new()
        {
            { A,"A"},
            { ADC,"ADC"},
            { ADCL,"ADCL"},
            { ADD,"ADD"},
            { B,"B"},
            { BA,"BA"},
            { BH,"BH"},
            { BL,"BL"},
            { BP,"BP"},
            { BX,"BX"},
            { C,"C"},
            { CALL,"CALL"},
            { CALLF,"CALLF"},
            { CH,"CH"},
            { CL,"CL"},
            { CMP,"CMP"},
            { CMPP,"CMPP"},
            { CMPW,"CMPW"},
            { CX,"CX"},
            { DADL,"DADL"},
            { DEC,"DEC"},
            { Decrement,"--"},
            { DH,"DH"},
            { DI,"DI"},
            { DL,"DL"},
            { DSBL,"DSBL"},
            { DSLL,"DSLL"},
            { DSRL,"DSRL"},
            { DX,"DX"},
            { EX,"EX"},
            { EXL,"EXL"},
            { EXP,"EXP"},
            { EXW,"EXW"},
            { F,"F"},
            { HALT,"HALT"},
            { I,"I"},
            { IL,"IL"},
            { IMR,"IMR"},
            { INC,"INC"},
            { Increment,"++"},
            { IR,"IR"},
            { JP,"JP"},
            { JPC,"JPC"},
            { JPF,"JPF"},
            { JPNC,"JPNC"},
            { JPNZ,"JPNZ"},
            { JPZ,"JPZ"},
            { JR,"JR"},
            { JRC,"JRC"},
            { JRNC,"JRNC"},
            { JRNZ,"JRNZ"},
            { JRZ,"JRZ"},
            { MV,"MV"},
            { MVL,"MVL"},
            { MVLD,"MVLD"},
            { MVP,"MVP"},
            { MVW,"MVW"},
            { NC,"NC"},
            { NOP,"NOP"},
            { NZ,"NZ"},
            { OFF,"OFF"},
            { PMDF,"PMDF"},
            { POPS,"POPS"},
            { POPU,"POPU"},
            { PUSHS,"PUSHS"},
            { PUSHU,"PUSHU"},
            { PX,"PX"},
            { PY,"PY"},
            { RC,"RC"},
            { RESET,"RESET"},
            { RET,"RET"},
            { RETF,"RETF"},
            { RETI,"RETI"},
            { ROL,"ROL"},
            { ROR,"ROR"},
            { S,"S"},
            { SBC,"SBC"},
            { SBCL,"SBCL"},
            { SC,"SC"},
            { SI,"SI"},
            { SUB,"SUB"},
            { SWAP,"SWAP"},
            { TEST,"TEST"},
            { U,"U"},
            { WAIT,"WAIT"},
            { X,"X"},
            { Y,"Y"},
            { Z,"Z"},
        };
    }
}
