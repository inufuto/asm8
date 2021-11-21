using Inu.Language;
using System.Diagnostics;

namespace Inu.Linker
{
    class CmtFile : BinaryTargetFile
    {
        private const int HeadByte = 0x3a;
        private const int MaxRecordSize = 0xff;

        public CmtFile(string fileName) : base(fileName) { }

        public override void Write(int address, byte[] bytes)
        {
            WriteHead(address);
            base.Write(address, bytes, MaxRecordSize);
            WriteTail();
        }

        private void WriteHead(int address)
        {
            byte low = (byte)address;
            byte high = (byte)(address >> 8);
            int sum = low + high;
            Stream.WriteByte(HeadByte);
            Stream.WriteByte(high);
            Stream.WriteByte(low);
            Stream.WriteByte(-sum & 0xff);
        }

        private void WriteTail()
        {
            Stream.WriteByte(HeadByte);
            Stream.WriteWord(0);
        }


        protected override void WriteRecord(int address, byte[] bytes, int offset, int recordSize)
        {
            Debug.Assert(recordSize > 0 && recordSize < 0x100);
            Stream.WriteByte(HeadByte);
            Stream.WriteByte(recordSize);
            int sum = recordSize;
            for (int i = 0; i < recordSize; ++i) {
                byte b = bytes[offset + i];
                Stream.WriteByte(b);
                sum += b;
            }
            Stream.WriteByte(-sum & 0xff);
        }
    }
}
