using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Inu.Linker
{
    class HexFile : AsciiTargetFile
    {
        private enum RecordType
        {
            Data,
            End
        };

        private class Record
        {
            private const char HeadChar = ':';
            private readonly RecordType type;
            private readonly int address;
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
                var size = bytes.Count;
                var low = (byte)(address);
                var high = (byte)(address >> 8);
                var sum = size + low + high + (int)type;

                writer.Write(HeadChar + ToHex(size) + ToHex(high) + ToHex(low) + ToHex((int)type));
                foreach (var b in bytes) {
                    writer.Write(ToHex(b));
                    sum += b;
                }
                writer.WriteLine(ToHex(-sum));
            }

            private static string ToHex(int b)
            {
                return $"{b & 0xff:X02}";
            }
        }

        private const int MaxRecordSize = 32;

        public HexFile(string fileName) : base(fileName) { }

        public override void Write(int address, byte[] bytes)
        {
            base.Write(address, bytes, MaxRecordSize);
            Record end = new Record(RecordType.End, 0);
            end.Write(Writer);
        }

        protected override void WriteRecord(int address, byte[] bytes, int offset, int recordSize)
        {
            Record record = new Record(RecordType.Data, address);
            record.SetBytes(bytes, offset, recordSize);
            record.Write(Writer);
        }
    }
}
