using System.Collections.Generic;

namespace Inu.Assembler.Z80
{
    internal class Keyword : Inu.Assembler.Keyword
    {
        public new const int MinId = NextId;

        public const int A = MinId + 0;
        public const int Adc = MinId + 1;
        public const int Add = MinId + 2;
        public const int Af = MinId + 3;
        public const int AfX = MinId + 4;
        public const int B = MinId + 5;
        public const int Bc = MinId + 6;
        public const int Bit = MinId + 7;
        public const int C = MinId + 8;
        public const int Call = MinId + 9;
        public const int Ccf = MinId + 10;
        public const int Cp = MinId + 11;
        public const int Cpd = MinId + 12;
        public const int CpdR = MinId + 13;
        public const int Cpi = MinId + 14;
        public const int CpiR = MinId + 15;
        public const int Cpl = MinId + 16;
        public const int D = MinId + 17;
        public const int Daa = MinId + 18;
        public const int De = MinId + 19;
        public const int Dec = MinId + 20;
        public const int Di = MinId + 21;
        public const int DjNz = MinId + 22;
        public const int DWNz = MinId + 23;
        public const int E = MinId + 24;
        public const int Ei = MinId + 25;
        public const int Ex = MinId + 26;
        public const int Exx = MinId + 27;
        public const int H = MinId + 28;
        public const int Halt = MinId + 29;
        public const int Hl = MinId + 30;
        public const int I = MinId + 31;
        public const int Im = MinId + 32;
        public const int In = MinId + 33;
        public const int Inc = MinId + 34;
        public const int Ind = MinId + 35;
        public const int IndR = MinId + 36;
        public const int Ini = MinId + 37;
        public const int IniR = MinId + 38;
        public const int Ix = MinId + 39;
        public const int Iy = MinId + 40;
        public const int Jp = MinId + 41;
        public const int Jr = MinId + 42;
        public const int L = MinId + 43;
        public const int Ld = MinId + 44;
        public const int LdD = MinId + 45;
        public const int LdDr = MinId + 46;
        public const int LdI = MinId + 47;
        public const int LdIr = MinId + 48;
        public const int M = MinId + 49;
        public const int N = MinId + 50;
        public const int Nc = MinId + 51;
        public const int Neg = MinId + 52;
        public const int Nop = MinId + 53;
        public const int Nz = MinId + 54;
        public const int OtDr = MinId + 55;
        public const int OtIr = MinId + 56;
        public const int Out = MinId + 57;
        public const int OutD = MinId + 58;
        public const int OutDr = MinId + 59;
        public const int OutI = MinId + 60;
        public const int OutIr = MinId + 61;
        public const int P = MinId + 62;
        public const int Pe = MinId + 63;
        public const int Po = MinId + 64;
        public const int Pop = MinId + 65;
        public const int Push = MinId + 66;
        public const int R = MinId + 67;
        public const int Res = MinId + 68;
        public const int Ret = MinId + 69;
        public const int RetI = MinId + 70;
        public const int RetN = MinId + 71;
        public const int Rl = MinId + 72;
        public const int Rla = MinId + 73;
        public const int Rlc = MinId + 74;
        public const int RlcA = MinId + 75;
        public const int Rld = MinId + 76;
        public const int Rr = MinId + 77;
        public const int Rra = MinId + 78;
        public const int Rrc = MinId + 79;
        public const int RrcA = MinId + 80;
        public const int Rrd = MinId + 81;
        public const int Rst = MinId + 82;
        public const int Sbc = MinId + 83;
        public const int Scf = MinId + 84;
        public const int Set = MinId + 85;
        public const int Sla = MinId + 86;
        public const int Sp = MinId + 87;
        public const int Sra = MinId + 88;
        public const int Srl = MinId + 89;
        public const int Sub = MinId + 90;
        public const int Z = MinId + 91;

        public new static readonly Dictionary<int, string> Words = new Dictionary<int, string>()
        {
            { A,"A"},
            { Adc,"ADC"},
            { Add,"ADD"},
            { Af,"AF"},
            { AfX,"AF'"},
            { B,"B"},
            { Bc,"BC"},
            { Bit,"BIT"},
            { C,"C"},
            { Call,"CALL"},
            { Ccf,"CCF"},
            { Cp,"CP"},
            { Cpd,"CPD"},
            { CpdR,"CPDR"},
            { Cpi,"CPI"},
            { CpiR,"CPIR"},
            { Cpl,"CPL"},
            { D,"D"},
            { Daa,"DAA"},
            { De,"DE"},
            { Dec,"DEC"},
            { Di,"DI"},
            { DjNz,"DJNZ"},
            { DWNz,"DWNZ"},
            { E,"E"},
            { Ei,"EI"},
            { Ex,"EX"},
            { Exx,"EXX"},
            { H,"H"},
            { Halt,"HALT"},
            { Hl,"HL"},
            { I,"I"},
            { Im,"IM"},
            { In,"IN"},
            { Inc,"INC"},
            { Ind,"IND"},
            { IndR,"INDR"},
            { Ini,"INI"},
            { IniR,"INIR"},
            { Ix,"IX"},
            { Iy,"IY"},
            { Jp,"JP"},
            { Jr,"JR"},
            { L,"L"},
            { Ld,"LD"},
            { LdD,"LDD"},
            { LdDr,"LDDR"},
            { LdI,"LDI"},
            { LdIr,"LDIR"},
            { M,"M"},
            { N,"N"},
            { Nc,"NC"},
            { Neg,"NEG"},
            { Nop,"NOP"},
            { Nz,"NZ"},
            { OtDr,"OTDR"},
            { OtIr,"OTIR"},
            { Out,"OUT"},
            { OutD,"OUTD"},
            { OutDr,"OUTDR"},
            { OutI,"OUTI"},
            { OutIr,"OUTIR"},
            { P,"P"},
            { Pe,"PE"},
            { Po,"PO"},
            { Pop,"POP"},
            { Push,"PUSH"},
            { R,"R"},
            { Res,"RES"},
            { Ret,"RET"},
            { RetI,"RETI"},
            { RetN,"RETN"},
            { Rl,"RL"},
            { Rla,"RLA"},
            { Rlc,"RLC"},
            { RlcA,"RLCA"},
            { Rld,"RLD"},
            { Rr,"RR"},
            { Rra,"RRA"},
            { Rrc,"RRC"},
            { RrcA,"RRCA"},
            { Rrd,"RRD"},
            { Rst,"RST"},
            { Sbc,"SBC"},
            { Scf,"SCF"},
            { Set,"SET"},
            { Sla,"SLA"},
            { Sp,"SP"},
            { Sra,"SRA"},
            { Srl,"SRL"},
            { Sub,"SUB"},
            { Z,"Z"},


        };
    }
}
