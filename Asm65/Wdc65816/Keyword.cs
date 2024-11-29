using System.Collections.Generic;

namespace Inu.Assembler.Wdc65816;

internal class Keyword : Mos6502.Keyword
{
    public new const int MinId = Mos6502.Keyword.NextId;

    public const int A16 = MinId + 0;
    public const int A8 = MinId + 1;
    public const int BGE = MinId + 2;
    public const int BLT = MinId + 3;
    public const int BRA = MinId + 4;
    public const int BRL = MinId + 5;
    public const int COP = MinId + 6;
    public const int I16 = MinId + 7;
    public const int I8 = MinId + 8;
    public const int JML = MinId + 9;
    public const int JSL = MinId + 10;
    public const int MVN = MinId + 11;
    public const int MVP = MinId + 12;
    public const int PEA = MinId + 13;
    public const int PEI = MinId + 14;
    public const int PER = MinId + 15;
    public const int PHB = MinId + 16;
    public const int PHD = MinId + 17;
    public const int PHK = MinId + 18;
    public const int PHX = MinId + 19;
    public const int PHY = MinId + 20;
    public const int PLB = MinId + 21;
    public const int PLD = MinId + 22;
    public const int PLX = MinId + 23;
    public const int PLY = MinId + 24;
    public const int REP = MinId + 25;
    public const int RTL = MinId + 26;
    public const int S = MinId + 27;
    public const int SEP = MinId + 28;
    public const int STP = MinId + 29;
    public const int STZ = MinId + 30;
    public const int SWA = MinId + 31;
    public const int TAD = MinId + 32;
    public const int TAS = MinId + 33;
    public const int TCD = MinId + 34;
    public const int TCS = MinId + 35;
    public const int TDA = MinId + 36;
    public const int TDC = MinId + 37;
    public const int TRB = MinId + 38;
    public const int TSA = MinId + 39;
    public const int TSB = MinId + 40;
    public const int TSC = MinId + 41;
    public const int TXY = MinId + 42;
    public const int TYX = MinId + 43;
    public const int WAI = MinId + 44;
    public const int WDM = MinId + 45;
    public const int XBA = MinId + 46;
    public const int XCE = MinId + 47;

    public new static readonly Dictionary<int, string> Words = new()
    {

        { A16,"A16"},
        { A8,"A8"},
        { BGE,"BGE"},
        { BLT,"BLT"},
        { BRA,"BRA"},
        { BRL,"BRL"},
        { COP,"COP"},
        { I16,"I16"},
        { I8,"I8"},
        { JML,"JML"},
        { JSL,"JSL"},
        { MVN,"MVN"},
        { MVP,"MVP"},
        { PEA,"PEA"},
        { PEI,"PEI"},
        { PER,"PER"},
        { PHB,"PHB"},
        { PHD,"PHD"},
        { PHK,"PHK"},
        { PHX,"PHX"},
        { PHY,"PHY"},
        { PLB,"PLB"},
        { PLD,"PLD"},
        { PLX,"PLX"},
        { PLY,"PLY"},
        { REP,"REP"},
        { RTL,"RTL"},
        { S,"S"},
        { SEP,"SEP"},
        { STP,"STP"},
        { STZ,"STZ"},
        { SWA,"SWA"},
        { TAD,"TAD"},
        { TAS,"TAS"},
        { TCD,"TCD"},
        { TCS,"TCS"},
        { TDA,"TDA"},
        { TDC,"TDC"},
        { TRB,"TRB"},
        { TSA,"TSA"},
        { TSB,"TSB"},
        { TSC,"TSC"},
        { TXY,"TXY"},
        { TYX,"TYX"},
        { WAI,"WAI"},
        { WDM,"WDM"},
        { XBA,"XBA"},
        { XCE,"XCE"},
    };
}