using System;
using System.Collections.Generic;
using System.Text;
using Inu.Assembler;
using Object = Inu.Assembler.Object;

namespace Inu.Linker
{
    class External
    {
        public readonly int Id;
        public readonly Assembler.Object Object;
        public readonly int Offset;
        public readonly AddressPart Part;
        public readonly bool Relative;

        public External(int id, Object obj, int offset, AddressPart part, bool relative)
        {
            Id = id;
            Object = obj;
            Offset = offset;
            Part = part;
            Relative = relative;
        }
    }
}
