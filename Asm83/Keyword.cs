namespace Inu.Assembler.Sm83;

internal class Keyword : Inu.Assembler.Keyword
{
    public new const int MinId = NextId;

    public const int A = MinId + 0;
    public const int ADC = MinId + 1;
    public const int ADD = MinId + 2;
    public const int AF = MinId + 3;
    public const int B = MinId + 4;
    public const int BC = MinId + 5;
    public const int BIT = MinId + 6;
    public const int C = MinId + 7;
    public const int CALL = MinId + 8;
    public const int CCF = MinId + 9;
    public const int CP = MinId + 10;
    public const int CPL = MinId + 11;
    public const int D = MinId + 12;
    public const int DAA = MinId + 13;
    public const int DE = MinId + 14;
    public const int DEC = MinId + 15;
    public const int DI = MinId + 16;
    public const int E = MinId + 17;
    public const int EI = MinId + 18;
    public const int H = MinId + 19;
    public const int HALT = MinId + 20;
    public const int HL = MinId + 21;
    public const int INC = MinId + 22;
    public const int JP = MinId + 23;
    public const int JR = MinId + 24;
    public const int L = MinId + 25;
    public const int LD = MinId + 26;
    public const int LDH = MinId + 27;
    public const int NC = MinId + 28;
    public const int NOP = MinId + 29;
    public const int NZ = MinId + 30;
    public const int POP = MinId + 31;
    public const int PUSH = MinId + 32;
    public const int RES = MinId + 33;
    public const int RET = MinId + 34;
    public const int RETI = MinId + 35;
    public const int RL = MinId + 36;
    public const int RLA = MinId + 37;
    public const int RLC = MinId + 38;
    public const int RLCA = MinId + 39;
    public const int RR = MinId + 40;
    public const int RRA = MinId + 41;
    public const int RRC = MinId + 42;
    public const int RRCA = MinId + 43;
    public const int RST = MinId + 44;
    public const int SBC = MinId + 45;
    public const int SCF = MinId + 46;
    public const int SET = MinId + 47;
    public const int SLA = MinId + 48;
    public const int SP = MinId + 49;
    public const int SRA = MinId + 50;
    public const int SRL = MinId + 51;
    public const int STOP = MinId + 52;
    public const int SUB = MinId + 53;
    public const int SWAP = MinId + 54;
    public const int Z = MinId + 55;

    public new static readonly Dictionary<int, string> Words = new()
    {
        { A,"A"},
        { ADC,"ADC"},
        { ADD,"ADD"},
        { AF,"AF"},
        { B,"B"},
        { BC,"BC"},
        { BIT,"BIT"},
        { C,"C"},
        { CALL,"CALL"},
        { CCF,"CCF"},
        { CP,"CP"},
        { CPL,"CPL"},
        { D,"D"},
        { DAA,"DAA"},
        { DE,"DE"},
        { DEC,"DEC"},
        { DI,"DI"},
        { E,"E"},
        { EI,"EI"},
        { H,"H"},
        { HALT,"HALT"},
        { HL,"HL"},
        { INC,"INC"},
        { JP,"JP"},
        { JR,"JR"},
        { L,"L"},
        { LD,"LD"},
        { LDH,"LDH"},
        { NC,"NC"},
        { NOP,"NOP"},
        { NZ,"NZ"},
        { POP,"POP"},
        { PUSH,"PUSH"},
        { RES,"RES"},
        { RET,"RET"},
        { RETI,"RETI"},
        { RL,"RL"},
        { RLA,"RLA"},
        { RLC,"RLC"},
        { RLCA,"RLCA"},
        { RR,"RR"},
        { RRA,"RRA"},
        { RRC,"RRC"},
        { RRCA,"RRCA"},
        { RST,"RST"},
        { SBC,"SBC"},
        { SCF,"SCF"},
        { SET,"SET"},
        { SLA,"SLA"},
        { SP,"SP"},
        { SRA,"SRA"},
        { SRL,"SRL"},
        { STOP,"STOP"},
        { SUB,"SUB"},
        { SWAP,"SWAP"},
        { Z,"Z"},
    };
}