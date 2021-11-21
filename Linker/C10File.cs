using Inu.Language;
using System;
using System.IO;
using System.Text;

namespace Inu.Linker
{
    class C10File : BinaryTargetFile
    {
        private const int MaxRecordSize = 255;
        private const int NameSize = 8;

        private readonly string name;

        public C10File(string fileName) : base(fileName)
        {
            name = Path.GetFileNameWithoutExtension(fileName).ToUpper();
        }

        public override void Write(int address, byte[] bytes)
        {
            void LeaderBytes()
            {
                for (var i = 0; i < 128; ++i)
                {
                    Stream.WriteByte(0x55);
                }
            }

            LeaderBytes();
            WriteHead(address);
            LeaderBytes();
            Write(address, bytes, MaxRecordSize);
            WriteTail();
        }

        protected override void WriteRecord(int address, byte[] bytes, int offset, int recordSize)
        {
            WriteBlock(1, bytes, offset, recordSize);
        }


        private void WriteHead(int address)
        {
            byte[] headBytes = new byte[NameSize+7];
            byte[] nameBytes = Encoding.ASCII.GetBytes(name);
            var i = 0;
            foreach (var c in nameBytes) {
                if (i >= NameSize) { break; }
                headBytes[i++] = c;
            }
            while (i < NameSize) {
                headBytes[i++] = 0x20;
            }
            headBytes[i++] = 2;
            headBytes[i++] = 0;
            headBytes[i++] = 0;

            var low = (byte)(address & 0xff);
            var high = (byte)(address >> 8);
            headBytes[i++] = high;
            headBytes[i++] = low;
            headBytes[i++] = high;
            headBytes[i] = low;
            
            WriteBlock(0, headBytes, 0, headBytes.Length);
        }

        private void WriteTail()
        {
            WriteBlock(0xff, Array.Empty<byte>(), 0, 0);
        }

        private void WriteBlock(byte type, byte[] bytes, int offset, int length)
        {
            Stream.WriteByte(0x55);
            Stream.WriteByte(0x3c);
            Stream.WriteByte(type);
            int sum = type;
            Stream.WriteByte(length);
            sum += length;
            sum &= 0xff;
            for (var i = 0; i < length; ++i) {
                var b = bytes[offset + i];
                Stream.WriteByte(b);
                sum += b;
                sum &= 0xff;
            }
            Stream.WriteByte(sum & 0xff);
            Stream.WriteByte(0x55);
        }
    }
}
