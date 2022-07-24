using System.Collections.Generic;

namespace Inu.Assembler.Mc6801
{
    internal class Keyword : Mc6800.Keyword
    {
        public new const int MinId = Mc6800.Keyword.NextId;

        public const int ABX = MinId + 0;
        public const int ADDD = MinId + 1;
        public const int ASLD = MinId + 2;
        public const int BRN = MinId + 3;
        public const int LDD = MinId + 4;
        public const int LSRD = MinId + 5;
        public const int MUL = MinId + 6;
        public const int PSHX = MinId + 7;
        public const int PULX = MinId + 8;
        public const int SLP = MinId + 9;
        public const int STD = MinId + 10;
        public const int SUBD = MinId + 11;
        public const int XGDX = MinId + 12;

        protected new const int NextId = MinId + 13;

        public new static readonly Dictionary<int, string> Words = new Dictionary<int, string>()
        {
            { ABX,"ABX"},
            { ADDD,"ADDD"},
            { ASLD,"ASLD"},
            { BRN,"BRN"},
            { LDD,"LDD"},
            { LSRD,"LSRD"},
            { MUL,"MUL"},
            { PSHX,"PSHX"},
            { PULX,"PULX"},
            { SLP,"SLP"},
            { STD,"STD"},
            { SUBD,"SUBD"},
            { XGDX,"XGDX"},
        };
    }
}
