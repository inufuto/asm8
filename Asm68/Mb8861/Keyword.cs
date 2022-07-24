using System.Collections.Generic;

namespace Inu.Assembler.Mb8861
{
    internal class Keyword : Mc6800.Keyword
    {
        public new const int MinId = Mc6800.Keyword.NextId;

        public const int NIM = MinId + 0;
        public const int OIM = MinId + 1;
        public const int XIM = MinId + 2;
        public const int TMM = MinId + 3;
        public const int ADX = MinId + 4;

        protected new const int NextId = MinId + 5;

        public new static readonly Dictionary<int, string> Words = new Dictionary<int, string>()
        {
            { NIM, "NIM" },
            { OIM, "OIM" },
            { XIM, "XIM" },
            { TMM, "TMM" },
            { ADX, "ADX" },
        };
    }
}
