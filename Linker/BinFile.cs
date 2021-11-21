using System.Collections.Generic;
using System.IO;

namespace Inu.Linker
{
    class BinFile : BinaryTargetFile
    {
        public BinFile(string fileName) : base(fileName)
        {
        }

        public override void Write(int address, byte[] bytes)
        {
            base.Write(address, bytes, 0x10000);
        }


        protected override void WriteRecord(int address, byte[] bytes, int offset, int recordSize)
        {
            Stream.Write(bytes, offset, recordSize);
        }
    }
}
