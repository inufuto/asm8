using Inu.Language;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Inu.Linker
{
    class CjrFile : BinaryTargetFile
    {
        private const int MaxRecordSize = 256;
        private const int NameSize = 16;
        private const int HeadSize = 26;

        private string name;
        private int recordIndex = 0;

        public CjrFile(string fileName) : base(fileName)
        {
            name = Path.GetFileNameWithoutExtension(fileName);
        }

        public override void Write(int address, byte[] bytes)
        {
            WriteHead();
            int offset = base.Write(address, bytes, MaxRecordSize);
            WriteTail(offset);
        }

        private void WriteHead()
        {
            byte[] headBytes = new byte[HeadSize];
            byte[] nameBytes = Encoding.ASCII.GetBytes(name);
            var i = 0;
            foreach (var c in nameBytes) {
                if (i >= NameSize) { break; }
                headBytes[i++] = c;
            }
            while (i < NameSize) {
                headBytes[i++] = 0;
            }
            headBytes[i++] = 1;
            headBytes[i++] = 0;
            while (i < HeadSize) {
                headBytes[i++] = 0xff;
            }
            WriteRecord(0xffff, headBytes, 0, HeadSize);
        }

        private void WriteTail(int offset)
        {
            Stream.WriteByte(2);
            Stream.WriteByte(0x2a);
            Stream.WriteWord(0xffff);
            Stream.WriteWord(offset);
        }


        protected override void WriteRecord(int address, byte[] bytes, int offset, int recordSize)
        {
            List<byte> head = new List<byte>();

            head.Add(2);
            head.Add(0x2a);
            head.Add((byte)recordIndex++);
            head.Add((byte)recordSize);
            head.Add((byte)(address >> 8));
            head.Add((byte)(address & 0xff));
            int sum = 0;
            foreach (byte b in head) {
                Stream.WriteByte(b);
                sum += b;
            }
            for (int i = 0; i < recordSize; ++i) {
                byte b = bytes[offset + i];
                Stream.WriteByte(b);
                sum += b;
            }
            Stream.WriteByte(sum);
        }
    }
}
