using System;
using System.Collections.Generic;
using System.Text;

namespace Inu.Assembler.Hd6301
{
    internal class Keyword : Mc6801.Keyword
    {
        public new const int MinId = Mc6801.Keyword.NextId;

        public const int XGDX = MinId + 0;
        protected new const int NextId = MinId + 12;

        public new static readonly Dictionary<int, string> Words = new Dictionary<int, string>()
        {
            { XGDX,"XGDX"},
        };
    }
}
