using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Inu.Language;

namespace Inu.Linker
{
    internal class RamPakFile : BinaryTargetFile
    {
        private static readonly byte[] HeadBytes = {
            0xAA, 0x1F, 0x04, 0x00, 0x04, 0x80, 0x00, 0x01, 0x04,
            0x04, 0x01, 0x03, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00
        };

        private class Cluster
        {
            private const int Size = 256;
            private readonly byte[] bytes = new byte[Size];
            private int position = 0;

            public bool IsFull()
            {
                return position >= Size;
            }

            public void AddByte(byte b)
            {
                Debug.Assert(!IsFull());
                bytes[position++] = b;
            }

            public void Write(Stream stream)
            {
                stream.Write(bytes);
            }
        }

        private class Directory
        {
            private const int Size = 16;
            public const int NameSize = 6;
            public const byte BasicType = 0;
            public const byte BinaryType = 1;
            private readonly string name;
            private readonly byte type;
            public readonly List<Cluster> clusters = new List<Cluster>();

            public Directory(string name, byte type)
            {
                this.name = name;
                this.type = type;
            }

            public void AddLine(string line)
            {
                AddBytes(Encoding.ASCII.GetBytes(line + '\r'));
            }

            public void AddBytes(byte[] bytes)
            {
                foreach (var b in bytes) {
                    AddByte(b);
                }
            }

            public void AddByte(byte b)
            {
                if (clusters.Count == 0 || clusters[^1].IsFull()) {
                    clusters.Add(new Cluster());
                }
                clusters[^1].AddByte(b);
            }

            public void Write(Stream stream, int clusterCount)
            {
                byte[] nameBytes = Encoding.ASCII.GetBytes(name);
                var i = 0;
                while (i < NameSize && i < nameBytes.Length) {
                    stream.WriteByte(nameBytes[i++]);
                }
                while (i < NameSize + 3) {
                    stream.WriteByte(0x20);
                    ++i;
                }
                stream.WriteByte(type);
                stream.WriteByte((byte)(firstClusterNumber + clusterCount));
                stream.WriteBytes(0xff, 5);
            }

            public static void WriteEmpty(Stream stream)
            {
                stream.WriteBytes(0xff, Size);
            }

            public void AddWord(int word)
            {
                AddByte((byte)(word & 0xff));
                AddByte((byte)((word >> 8) & 0xff));
            }
        }

        private const int firstClusterNumber = 4;
        private readonly List<Directory> directories = new List<Directory>();
        private readonly List<Cluster> clusters = new List<Cluster>();
        private readonly string name;

        public RamPakFile(string fileName) : base(fileName)
        {
            name = Path.GetFileNameWithoutExtension(fileName).ToUpper();
            byte[] nameBytes = Encoding.ASCII.GetBytes(name);
            var bytes = new List<byte>();
            var i = 0;
            while (i < Directory.NameSize && i < nameBytes.Length) {
                bytes.Add(nameBytes[i++]);
            }
            while (i < Directory.NameSize) {
                bytes.Add(0x20);
                ++i;
            }
            name = Encoding.ASCII.GetString(bytes.ToArray());
        }

        public override void Write(int address, byte[] bytes)
        {
            var ramLoader = new Directory("LD", Directory.BasicType);
            ramLoader.AddLine("10 A=&H" + address.ToString("X4"));
            ramLoader.AddLine("20 BLOAD \"" + name + "\",A");
            ramLoader.AddLine("30 CALL A");
            ramLoader.AddByte(0x1a);
            directories.Add(ramLoader);

            var tapeLoader = new Directory("LOADER", Directory.BasicType);
            tapeLoader.AddLine("10 A=&H" + address.ToString("X4"));
            tapeLoader.AddLine("20 BLOAD#-1,\"" + name + "\",A");
            tapeLoader.AddLine("30 CALL A");
            tapeLoader.AddByte(0x1a);
            directories.Add(tapeLoader);

            var tapeSaver = new Directory("SAVER", Directory.BasicType);
            tapeSaver.AddLine("10 A=&H" + address.ToString("X4"));
            tapeSaver.AddLine("20 BLOAD \"" + name + "\",A");
            tapeSaver.AddLine("30 BSAVE#-1,\"" + name + "\",A,&H" + bytes.Length.ToString("X4"));
            tapeSaver.AddByte(0x1a);
            directories.Add(tapeSaver);

            var binary = new Directory(name, Directory.BinaryType);
            binary.AddWord(address);
            binary.AddWord(bytes.Length);
            binary.AddBytes(bytes);
            directories.Add(binary);

            Stream.Write(HeadBytes);

            var fat = new List<byte>();
            var directoryIndex = 0;
            foreach (var directory in directories) {
                directory.Write(Stream, clusters.Count);
                var clusterIndex = 0;
                foreach (var cluster in directory.clusters) {
                    clusters.Add(cluster);
                    if (clusterIndex < directory.clusters.Count - 1) {
                        fat.Add((byte)(firstClusterNumber + clusters.Count));
                    }
                    else {
                        fat.Add(0xc1);
                    }
                    ++clusterIndex;
                }
                ++directoryIndex;
            }
            while (directoryIndex < 32) {
                Directory.WriteEmpty(Stream);
                ++directoryIndex;
            }
            Stream.WriteBytes(0, 224);

            var fatIndex = 0;
            Stream.WriteBytes(0xfe, firstClusterNumber);
            foreach (var clusterNumber in fat) {
                Stream.WriteByte(clusterNumber);
                ++fatIndex;
            }
            while (fatIndex < 128 - 4) {
                Stream.WriteByte(0);
                ++fatIndex;
            }
            Stream.WriteBytes(0, 29);
            Stream.WriteByte(0xff);
            Stream.WriteBytes(0, 98);

            foreach (var cluster in clusters) {
                cluster.Write(Stream);
            }
        }

        protected override void WriteRecord(int address, byte[] bytes, int offset, int recordSize) { }
    }
}
