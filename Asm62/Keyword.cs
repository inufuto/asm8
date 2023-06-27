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
        public const int BP = MinId + 6;
        public const int C = MinId + 7;
        public const int CALL = MinId + 8;
        public const int CALLF = MinId + 9;
        public const int CMP = MinId + 10;
        public const int CMPP = MinId + 11;
        public const int CMPW = MinId + 12;
        public const int DADL = MinId + 13;
        public const int DEC = MinId + 14;
        public const int Decrement = MinId + 15;
        public const int DSBL = MinId + 16;
        public const int DSLL = MinId + 17;
        public const int DSRL = MinId + 18;
        public const int EX = MinId + 19;
        public const int EXL = MinId + 20;
        public const int EXP = MinId + 21;
        public const int EXW = MinId + 22;
        public const int F = MinId + 23;
        public const int HALT = MinId + 24;
        public const int I = MinId + 25;
        public const int IL = MinId + 26;
        public const int IMR = MinId + 27;
        public const int INC = MinId + 28;
        public const int Increment = MinId + 29;
        public const int IR = MinId + 30;
        public const int JP = MinId + 31;
        public const int JPC = MinId + 32;
        public const int JPF = MinId + 33;
        public const int JPNC = MinId + 34;
        public const int JPNZ = MinId + 35;
        public const int JPZ = MinId + 36;
        public const int JR = MinId + 37;
        public const int JRC = MinId + 38;
        public const int JRNC = MinId + 39;
        public const int JRNZ = MinId + 40;
        public const int JRZ = MinId + 41;
        public const int MV = MinId + 42;
        public const int MVL = MinId + 43;
        public const int MVLD = MinId + 44;
        public const int MVP = MinId + 45;
        public const int MVW = MinId + 46;
        public const int NC = MinId + 47;
        public const int NOP = MinId + 48;
        public const int NZ = MinId + 49;
        public const int OFF = MinId + 50;
        public const int PMDF = MinId + 51;
        public const int POPS = MinId + 52;
        public const int POPU = MinId + 53;
        public const int PUSHS = MinId + 54;
        public const int PUSHU = MinId + 55;
        public const int PX = MinId + 56;
        public const int PY = MinId + 57;
        public const int RC = MinId + 58;
        public const int RESET = MinId + 59;
        public const int RET = MinId + 60;
        public const int RETF = MinId + 61;
        public const int RETI = MinId + 62;
        public const int ROL = MinId + 63;
        public const int ROR = MinId + 64;
        public const int S = MinId + 65;
        public const int SBC = MinId + 66;
        public const int SBCL = MinId + 67;
        public const int SC = MinId + 68;
        public const int SUB = MinId + 69;
        public const int SWAP = MinId + 70;
        public const int TEST = MinId + 71;
        public const int U = MinId + 72;
        public const int WAIT = MinId + 73;
        public const int X = MinId + 74;
        public const int Y = MinId + 75;
        public const int Z = MinId + 76;

        public new static readonly Dictionary<int, string> Words = new()
        {
            { A,"A"},
            { ADC,"ADC"},
            { ADCL,"ADCL"},
            { ADD,"ADD"},
            { B,"B"},
            { BA,"BA"},
            { BP,"BP"},
            { C,"C"},
            { CALL,"CALL"},
            { CALLF,"CALLF"},
            { CMP,"CMP"},
            { CMPP,"CMPP"},
            { CMPW,"CMPW"},
            { DADL,"DADL"},
            { DEC,"DEC"},
            { Decrement,"--"},
            { DSBL,"DSBL"},
            { DSLL,"DSLL"},
            { DSRL,"DSRL"},
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
