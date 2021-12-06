using System.Collections.Generic;
using System.IO;
using System.Linq;
using Inu.Assembler;
using Inu.Language;

namespace Inu.Library
{
    public class Library
    {
        public const string Extension = ".lib";
        private const int Version = 0x0100;
        private static readonly List<Object> Objects = new List<Assembler.Object>();

        private readonly Dictionary<string, Assembler.Object> symbols = new Dictionary<string, Assembler.Object>();

        public void Save(string fileName)
        {
            using Stream stream = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            stream.WriteWord(Version);

            stream.WriteWord(Objects.Count);
            foreach (var obj in Objects) {
                obj.Write(stream);
            }
            stream.WriteWord(symbols.Count);
            foreach (var (name, value) in symbols) {
                var index = Objects.IndexOf(value);
                stream.WriteString(name);
                stream.WriteWord(index);
            }
        }

        public Object? NameToObject(string name)
        {
            return symbols.TryGetValue(name, out var obj) ? obj : null;
        }

        public void Add(Object obj)
        {
            Objects.Add(obj);
            foreach (var symbol in obj.Symbols.Values.Where(symbol => symbol.Public)) {
                if (symbol.Address.Type != AddressType.External) {
                    symbols[symbol.Name] = obj;
                }
            }
        }

        public void Load(string fileName)
        {
            using Stream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            stream.ReadWord();  //  Version
            {
                var n = stream.ReadWord();
                for (var i = 0; i < n; ++i) {
                    var obj = new Object(fileName);
                    obj.Read(stream);
                    Objects.Add(obj);
                }
            }
            {
                var n = stream.ReadWord();
                for (var i = 0; i < n; ++i) {
                    var name = stream.ReadString();
                    var index = stream.ReadWord();
                    symbols[name] = Objects[index];
                }
            }
        }
    }
}
