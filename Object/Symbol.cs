using Inu.Language;
using System.IO;

namespace Inu.Assembler;

public class Symbol(int pass, int id, string name, Address address, Scope scope)
{
    public int Pass { get; private set; } = pass;
    public readonly int Id = id;
    public readonly string Name = name;
    public Address Address { get; set; } = address;

    public readonly Scope Scope = scope;
    public bool Public { get; set; } = false;

    public SymbolKey Key => new SymbolKey(Scope, Id);

    public override string ToString()
    {
        return Scope.Id + ":" + Id + ":" + Name + ":" + Public;
    }

    public Symbol(Stream stream) : this(0, stream.ReadWord(), stream.ReadString(), new Address(stream), new Scope(0, null))
    { }

    public void Write(Stream stream)
    {
        stream.WriteWord(Id);
        stream.WriteString(Name);
        Address.Write(stream);
    }

    public bool IsExternal()
    {
        return Address.IsExternal() && Address.Id == Id;
    }
}