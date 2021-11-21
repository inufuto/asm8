using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Inu.Linker
{
    internal class SRecordFile : AsciiTargetFile
    {
        private enum RecordType { Data = 1, End = 9 }

        private class Record
        {
            private const string HeadChar = "S";

            private RecordType type;
            private int address;
            private readonly List<byte> bytes = new List<byte>();

            public Record(RecordType type, int address)
            {
                this.type = type;
                this.address = address;
            }

            public void SetBytes(byte[] sourceBytes, int offset, int recordSize)
            {
                Debug.Assert(recordSize > 0 && recordSize < 0x100);
                for (int i = 0; i < recordSize; ++i) {
                    bytes.Add(sourceBytes[offset++]);
                }
            }

            public void Write(StreamWriter writer)
            {
                int size = 3 + bytes.Count;
                var low = (byte)(address);
                var high = (byte)(address >> 8);
                var sum = size + low + high + (int)type;

                writer.Write(HeadChar + (char)(type + '0') + ToHex(size) + ToHex(high) + ToHex(low));
                foreach (byte b in bytes) {
                    writer.Write(ToHex(b));
                    sum += b;
                }
                writer.WriteLine(ToHex(~sum));
            }

            private static string ToHex(int b)
            {
                return string.Format("{0:X02}", b & 0xff);
            }
        }

        private const int MaxRecordSize = 32;
        public SRecordFile(string fileName) : base(fileName) { }

        public override void Write(int address, byte[] bytes)
        {
            base.Write(address, bytes, MaxRecordSize);
            Record record = new Record(RecordType.End, address);
            record.Write(Writer);
        }

        protected override void WriteRecord(int address, byte[] bytes, int offset, int recordSize)
        {
            Record record = new Record(RecordType.Data, address);
            record.SetBytes(bytes, offset, recordSize);
            record.Write(Writer);
        }
    }
}
