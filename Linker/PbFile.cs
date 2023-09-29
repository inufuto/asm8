using System;
using System.IO;
using System.Xml.Linq;

namespace Inu.Linker
{
    internal class PbFile : AsciiTargetFile
    {
        private const int MaxRecordSize = 120;

        private readonly string name;

        public PbFile(string fileName) : base(fileName)
        {
            name = Path.GetFileNameWithoutExtension(fileName).ToUpper() + ".EXE";
        }

        public override void Write(int address, byte[] bytes)
        {
            Writer.WriteLine($"{name},{address},{address + bytes.Length - 1},{address}");
            Write(address, bytes, MaxRecordSize);
        }


        protected override void WriteRecord(int address, byte[] bytes, int offset, int recordSize)
        {
            var sum = 0;
            for (var i = 0; i < recordSize; ++i) {
                var b = bytes[offset + i];
                Writer.Write($"{b:X02}");
                sum += b;
            }
            Writer.WriteLine($",{sum}");
        }
    }
}
