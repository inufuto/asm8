using System.Collections.Generic;

namespace Inu.Assembler.Wdc65c02;

internal class Keyword : Mos6502.Keyword
{
    public new const int MinId = Mos6502.Keyword.NextId;

    public const int BBR0 = MinId + 0;
    public const int BBR1 = MinId + 1;
    public const int BBR2 = MinId + 2;
    public const int BBR3 = MinId + 3;
    public const int BBR4 = MinId + 4;
    public const int BBR5 = MinId + 5;
    public const int BBR6 = MinId + 6;
    public const int BBR7 = MinId + 7;
    public const int BBS0 = MinId + 8;
    public const int BBS1 = MinId + 9;
    public const int BBS2 = MinId + 10;
    public const int BBS3 = MinId + 11;
    public const int BBS4 = MinId + 12;
    public const int BBS5 = MinId + 13;
    public const int BBS6 = MinId + 14;
    public const int BBS7 = MinId + 15;
    public const int BR0 = MinId + 16;
    public const int BR1 = MinId + 17;
    public const int BR2 = MinId + 18;
    public const int BR3 = MinId + 19;
    public const int BR4 = MinId + 20;
    public const int BR5 = MinId + 21;
    public const int BR6 = MinId + 22;
    public const int BR7 = MinId + 23;
    public const int BRA = MinId + 24;
    public const int BS0 = MinId + 25;
    public const int BS1 = MinId + 26;
    public const int BS2 = MinId + 27;
    public const int BS3 = MinId + 28;
    public const int BS4 = MinId + 29;
    public const int BS5 = MinId + 30;
    public const int BS6 = MinId + 31;
    public const int BS7 = MinId + 32;
    public const int DEA = MinId + 33;
    public const int INA = MinId + 34;
    public const int PHX = MinId + 35;
    public const int PHY = MinId + 36;
    public const int PLX = MinId + 37;
    public const int PLY = MinId + 38;
    public const int RMB0 = MinId + 39;
    public const int RMB1 = MinId + 40;
    public const int RMB2 = MinId + 41;
    public const int RMB3 = MinId + 42;
    public const int RMB4 = MinId + 43;
    public const int RMB5 = MinId + 44;
    public const int RMB6 = MinId + 45;
    public const int RMB7 = MinId + 46;
    public const int SMB0 = MinId + 47;
    public const int SMB1 = MinId + 48;
    public const int SMB2 = MinId + 49;
    public const int SMB3 = MinId + 50;
    public const int SMB4 = MinId + 51;
    public const int SMB5 = MinId + 52;
    public const int SMB6 = MinId + 53;
    public const int SMB7 = MinId + 54;
    public const int STP = MinId + 55;
    public const int STZ = MinId + 56;
    public const int TRB = MinId + 57;
    public const int TSB = MinId + 58;
    public const int WAI = MinId + 59;

    public new static readonly Dictionary<int, string> Words = new()
    {
        { BBR0,"BBR0"},
        { BBR1,"BBR1"},
        { BBR2,"BBR2"},
        { BBR3,"BBR3"},
        { BBR4,"BBR4"},
        { BBR5,"BBR5"},
        { BBR6,"BBR6"},
        { BBR7,"BBR7"},
        { BBS0,"BBS0"},
        { BBS1,"BBS1"},
        { BBS2,"BBS2"},
        { BBS3,"BBS3"},
        { BBS4,"BBS4"},
        { BBS5,"BBS5"},
        { BBS6,"BBS6"},
        { BBS7,"BBS7"},
        { BR0,"BR0"},
        { BR1,"BR1"},
        { BR2,"BR2"},
        { BR3,"BR3"},
        { BR4,"BR4"},
        { BR5,"BR5"},
        { BR6,"BR6"},
        { BR7,"BR7"},
        { BRA,"BRA"},
        { BS0,"BS0"},
        { BS1,"BS1"},
        { BS2,"BS2"},
        { BS3,"BS3"},
        { BS4,"BS4"},
        { BS5,"BS5"},
        { BS6,"BS6"},
        { BS7,"BS7"},
        { DEA,"DEA"},
        { INA,"INA"},
        { PHX,"PHX"},
        { PHY,"PHY"},
        { PLX,"PLX"},
        { PLY,"PLY"},
        { RMB0,"RMB0"},
        { RMB1,"RMB1"},
        { RMB2,"RMB2"},
        { RMB3,"RMB3"},
        { RMB4,"RMB4"},
        { RMB5,"RMB5"},
        { RMB6,"RMB6"},
        { RMB7,"RMB7"},
        { SMB0,"SMB0"},
        { SMB1,"SMB1"},
        { SMB2,"SMB2"},
        { SMB3,"SMB3"},
        { SMB4,"SMB4"},
        { SMB5,"SMB5"},
        { SMB6,"SMB6"},
        { SMB7,"SMB7"},
        { STP,"STP"},
        { STZ,"STZ"},
        { TRB,"TRB"},
        { TSB,"TSB"},
        { WAI,"WAI"},
    };
}