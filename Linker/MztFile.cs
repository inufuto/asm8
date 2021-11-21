using Inu.Language;
using System.IO;
using System.Text;

namespace Inu.Linker
{
    class MztFile : BinaryTargetFile
    {
        private const int NameSize = 16;
        private string name;


        public MztFile(string fileName) : base(fileName)
        {
            name = Path.GetFileNameWithoutExtension(fileName).ToUpper();
        }

        public override void Write(int address, byte[] bytes)
        {
            Stream.WriteByte(1);
            {
                byte[] nameBytes = Encoding.ASCII.GetBytes(name);
                int i = 0;
                while (i < NameSize && i < nameBytes.Length) {
                    Stream.WriteByte(nameBytes[i++]);
                }
                while (i < NameSize) {
                    Stream.WriteByte(0x20);
                    ++i;
                }
                Stream.WriteByte(0x0d);
            }
            Stream.WriteWord(bytes.Length);
            Stream.WriteWord(address);
            Stream.WriteWord(address);
            for (int i = 0; i < 0x80 - 0x18; ++i) {
                Stream.WriteByte(0);
            }
            Stream.Write(bytes, 0, bytes.Length);
        }

        protected override void WriteRecord(int address, byte[] bytes, int offset, int recordSize) { }
    }
}
