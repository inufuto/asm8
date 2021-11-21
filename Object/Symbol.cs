using System.Collections.Generic;
using Inu.Language;
using System.IO;

namespace Inu.Assembler
{
    public class Symbol
    {
        public int Pass { get; private set; }
        public readonly int Id;
        public readonly string Name;
        public Address Address { get; set; }
        public bool Public { get; set; } = false;

        public Symbol(int pass, int id, string name, Address address)
        {
            Pass = pass;
            Id = id;
            Name = name;
            Address = address;
        }

        public override string ToString()
        {
            return Id + ":" + Name;
        }

        public Symbol(Stream stream)
        {
            Id = stream.ReadWord();
            Name = stream.ReadString();
            Address = new Address(stream);
        }

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
}
