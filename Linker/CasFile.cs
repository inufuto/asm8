using System.IO;
using System.Text;
using Inu.Language;
using Inu.Linker;

class CasFile : BinaryTargetFile
{
    private const int NameSize = 6;
    private static readonly byte[] Head = { 0x1F, 0xA6, 0xDE, 0xBA, 0xCC, 0x13, 0x7D, 0x74 };
    private readonly string name;

    public CasFile(string fileName) : base(fileName)
    {
        name = Path.GetFileNameWithoutExtension(fileName).ToUpper();
    }

    public override void Write(int address, byte[] bytes)
    {
        Stream.Write(Head);
        for (var i = 0; i < 10; ++i) {
            Stream.WriteByte(0xd0);
        }
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
        }
        Stream.Write(Head);
        Stream.WriteWord(address);
        Stream.WriteWord(address + bytes.Length - 1);
        Stream.WriteWord(address);
        Stream.Write(bytes, 0, bytes.Length);
    }

    protected override void WriteRecord(int address, byte[] bytes, int offset, int recordSize) { }
}