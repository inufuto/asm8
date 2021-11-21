using System.Collections.Generic;
using System.IO;
using System.Text;
using Inu.Language;

namespace Inu.Linker
{
    class T64File : BinaryTargetFile
    {
        private const int TapeVersion = 0x0100;
        private const int DirectorySize = 32;

        private class File
        {
            private readonly string name;
            private readonly int address;
            private readonly byte[] bytes;

            public File(string name, int address, byte[] bytes)
            {
                this.name = name;
                this.address = address;
                this.bytes = bytes;
            }

            public uint WriteDirectory(Stream stream, uint position)
            {
                stream.WriteByte(1);
                stream.WriteByte(0x82);
                stream.WriteWord(address);
                stream.WriteWord(address + bytes.Length);
                stream.WriteWord(0);
                stream.WriteDWord(position);
                stream.WriteDWord(0);
                WriteString(stream, name, 16, 0x20);
                return (uint)(position + bytes.Length);
            }


            public void WriteBytes(Stream stream)
            {
                stream.Write(bytes);
            }
        }

        private static void WriteString(Stream stream, string s, int size, int filler)
        {
            var bytes = Encoding.ASCII.GetBytes(s);
            var i = 0;
            foreach (var b in bytes) {
                if (i >= size)
                    break;
                stream.WriteByte(b);
                ++i;
            }
            while (i < size) {
                stream.WriteByte(filler);
                ++i;
            }
        }

        private readonly string name;

        public T64File(string fileName) : base(fileName)
        {
            name = Path.GetFileNameWithoutExtension(fileName).ToUpper();
        }

        public override void Write(int address, byte[] bytes)
        {
            List<File> files = new List<File> { new File(name, address, bytes) };

            WriteString(Stream, "C64 tape image file", 32, 0);
            Stream.WriteWord(TapeVersion);
            Stream.WriteWord(files.Count);
            Stream.WriteWord(files.Count);
            Stream.WriteWord(0);
            Stream.WriteBytes(0x20, 24);

            var position = (uint)(0x40 + DirectorySize * files.Count);
            foreach (var file in files) {
                position = file.WriteDirectory(Stream, position);
            }
            foreach (var file in files) {
                file.WriteBytes(Stream);
            }
        }

        protected override void WriteRecord(int address, byte[] bytes, int offset, int recordSize) { }
    }
}
