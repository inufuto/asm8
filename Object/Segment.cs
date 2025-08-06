using Inu.Language;
using System.Collections.Generic;
using System.IO;

namespace Inu.Assembler;

public class Segment(AddressType type)
{
    public AddressType Type { get; private set; } = type;
    public readonly List<byte> Bytes = [];

    public int Size => Bytes.Count;

    public Address Tail => new Address(Type, Size);

    public void Clear() { Bytes.Clear(); }

    public void WriteByte(int value) { Bytes.Add((byte)value); }

    public void Write(Stream stream)
    {
        stream.WriteWord(Size);
        stream.Write(Bytes.ToArray());
    }

    public void Read(Stream stream)
    {
        var n = stream.ReadWord();
        if (n <= 0) return;
        var bytes = new byte[n];
        stream.Read(bytes, 0, n);
        Bytes.AddRange(bytes);
    }
}