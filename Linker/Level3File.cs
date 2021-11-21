using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Inu.Linker
{
    class Level3File : BinaryTargetFile
    {
        private const int NameSize = 8;

        private abstract class Block
        {
            private readonly int gapSize;
            private readonly byte type;
            private readonly List<byte> bytes = new List<byte>();

            protected Block(int gapSize, byte type)
            {
                this.gapSize = gapSize;
                this.type = type;
            }

            protected void Add(byte b)
            {
                bytes.Add(b);
            }
            public void AddRange(IEnumerable<byte> bs)
            {
                bytes.AddRange(bs);
            }

            public void Write(Stream stream)
            {
                for (var i = 0; i < gapSize; ++i) {
                    stream.WriteByte(0xff);
                }
                stream.WriteByte(0x01);
                stream.WriteByte(0x3c);

                stream.WriteByte(type);
                var sum = type;

                var size = (byte)bytes.Count;
                stream.WriteByte(size);
                sum += size;

                foreach (var b in bytes) {
                    stream.WriteByte(b);
                    sum += b;
                }
                stream.WriteByte(sum);

                for (var i = 0; i < 4; ++i) {
                    stream.WriteByte(0);
                }
            }
        }

        private class HeadBlock : Block
        {
            public HeadBlock(string name) : base(90, 0)
            {
                {
                    byte[] nameBytes = Encoding.ASCII.GetBytes(name.ToUpper());
                    var i = 0;
                    foreach (var c in nameBytes) {
                        if (i >= NameSize) {
                            break;
                        }

                        Add(c);
                        ++i;
                    }

                    while (i < NameSize) {
                        Add(0x20);
                        ++i;
                    }
                }
                Add(2);
                Add(0);
                Add(0);
                for (var i = 0; i < 9; ++i) {
                    Add(0);
                }
            }
        }

        private class BodyBlock : Block
        {
            public BodyBlock(int gapSize) : base(gapSize, 1)
            { }
        }

        private class TailBlock : Block
        {
            public TailBlock() : base(20, 0xff)
            { }
        }

        private readonly string name;

        public Level3File(string fileName) : base(fileName)
        {
            name = Path.GetFileNameWithoutExtension(fileName);
        }

        public override void Write(int address, byte[] bytes)
        {
            List<byte> fileBytes = new List<byte> { 0 };

            var size = bytes.Length;
            fileBytes.Add((byte)((size >> 8) & 0xff));
            fileBytes.Add((byte)(size & 0xff));
            fileBytes.Add((byte)((address >> 8) & 0xff));
            fileBytes.Add((byte)(address & 0xff));
            fileBytes.AddRange(bytes);
            fileBytes.Add(0xff);
            fileBytes.Add(0);
            fileBytes.Add(0);
            fileBytes.Add((byte)((address >> 8) & 0xff));
            fileBytes.Add((byte)(address & 0xff));

            new HeadBlock(name).Write(Stream);
            var index = 0;
            while (index < fileBytes.Count) {
                var block = new BodyBlock(index == 0 ? 90 : 20);
                var blockSize = Math.Min(fileBytes.Count - index, 0xff);
                block.AddRange(fileBytes.GetRange(index, blockSize));
                block.Write(Stream);
                index += blockSize;
            }
            new TailBlock().Write(Stream);
        }

        protected override void WriteRecord(int address, byte[] bytes, int offset, int recordSize)
        { }
    }
}
