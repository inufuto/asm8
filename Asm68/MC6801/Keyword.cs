using System.Collections.Generic;

namespace Inu.Assembler.Mc6801
{
    internal class Keyword : Mc6800.Keyword
    {
        public new const int MinId = Mc6800.Keyword.NextId;

        public const int ABX = MinId + 0;
        public const int ADDD = MinId + 1;
        public const int AIM = MinId + 2;
        public const int ASLD = MinId + 3;
        public const int BRN = MinId + 4;
        public const int EIM = MinId + 5;
        public const int LDD = MinId + 6;
        public const int LSRD = MinId + 7;
        public const int MUL = MinId + 8;
        public const int OIM = MinId + 9;
        public const int PSHX = MinId + 10;
        public const int PULX = MinId + 11;
        public const int SLP = MinId + 12;
        public const int STD = MinId + 13;
        public const int SUBD = MinId + 14;
        public const int TIM = MinId + 15;
        protected new const int NextId = MinId + 16;

        public new static readonly Dictionary<int, string> Words = new Dictionary<int, string>()
        {
            { ABX,"ABX"},
            { ADDD,"ADDD"},
            { AIM,"AIM"},
            { ASLD,"ASLD"},
            { BRN,"BRN"},
            { EIM,"EIM"},
            { LDD,"LDD"},
            { LSRD,"LSRD"},
            { MUL,"MUL"},
            { OIM,"OIM"},
            { PSHX,"PSHX"},
            { PULX,"PULX"},
            { SLP,"SLP"},
            { STD,"STD"},
            { SUBD,"SUBD"},
            { TIM,"TIM"},
        };
    }
}
