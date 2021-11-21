using Inu.Language;
using System;
using System.IO;
using System.Text;

namespace Inu.Linker
{
    class PrgFile : BinaryTargetFile
    {
        private const int Version = 1;
        private static readonly byte[] Head = Encoding.ASCII.GetBytes("PROG");
        private string name;

        public PrgFile(string fileName) : base(fileName)
        {
            name = Path.GetFileNameWithoutExtension(fileName).ToUpper();
        }

        public override void Write(int address, byte[] bytes)
        {
            Stream.Write(Head, 0, Head.Length);
            Stream.WriteDWord(Version);

            byte[] nameBytes = Encoding.ASCII.GetBytes(name);
            int length = nameBytes.Length;
            Stream.WriteDWord((ushort)length);
            for (var i = 0; i < length; ++i) {
                Stream.WriteByte(name[i]);
            }

            Stream.WriteDWord((ushort)address);
            Stream.WriteDWord((ushort)bytes.Length);
            Stream.WriteDWord(1);
            Stream.Write(bytes,0,bytes.Length);
        }

        protected override void WriteRecord(int address, byte[] bytes, int offset, int recordSize) { }
    }
}
