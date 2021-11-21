using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Inu.Language;

namespace Inu.Assembler
{
    public class Object
    {
        private const int Version = 0x0100;
        public const string Extension = ".obj";

        public string? Name { get; private set; }
        //public readonly Dictionary<int, string> Names = new Dictionary<int, string>();
        public readonly Segment[] Segments = { new Segment(AddressType.Code), new Segment(AddressType.Data), new Segment(AddressType.ZeroPage) };
        public readonly Dictionary<int, Symbol> Symbols = new Dictionary<int, Symbol>();
        public readonly Dictionary<Address, Address> AddressUsages = new Dictionary<Address, Address>();


        public Object(string? fileName)
        {
            Name = Path.GetFileName(fileName);
        }

        public Object() : this(null) { }


        public void Save(string fileName)
        {
            using Stream stream = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            Write(stream);
            Name = Path.GetFileName(fileName);
        }

        public void Write(Stream stream)
        {
            stream.WriteWord(Version);

            foreach (Segment segment in Segments) {
                segment.Write(stream);
            }

            var publicSymbols = Symbols.Where(p => p.Value.Public).Select(p => p.Value).ToList();
            var publicIds = publicSymbols.Select(s => s.Id).ToList();
            var externalIds = AddressUsages.Values.Where(address => address.IsExternal()).Select(a =>
            {
                Debug.Assert(a.Id != null, "a.Id != null");
                return a.Id.Value;
            });
            var ids = publicIds.Union(externalIds).ToHashSet();

            stream.WriteWord(ids.Count);
            foreach (var symbol in ids.Select(id => Symbols[id]))
            {
                symbol.Write(stream);
            }

            stream.WriteWord(AddressUsages.Count);
            foreach (var (key, value) in AddressUsages) {
                key.Write(stream);
                value.Write(stream);
            }
        }

        public void Load(string fileName)
        {
            using Stream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            Read(stream);
            Name = Path.GetFileName(fileName);
        }

        public void Read(Stream stream)
        {
            stream.ReadWord(); // version

            foreach (Segment segment in Segments) {
                segment.Read(stream);
            }

            {
                var n = stream.ReadWord();
                for (var i = 0; i < n; ++i) {
                    var symbol = new Symbol(stream) { Public = true };
                    Symbols[symbol.Id] = symbol;
                }
            }
            {
                var n = stream.ReadWord();
                for (var i = 0; i < n; ++i) {
                    var location = new Address(stream);
                    var value = new Address(stream);
                    AddressUsages[location] = value;
                }
            }
        }

        public string NameFromId(int id) => Symbols[id].Name;
    }
}
