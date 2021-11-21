using Inu.Assembler;
using Inu.Language;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Inu.Library;
using Object = Inu.Assembler.Object;

namespace Inu.Linker
{
    internal class Error : Exception
    {
        public Error(string message) : base(message) { }
    }

    class InvalidAddress : Error
    {
        public InvalidAddress(string s) : base("Invalid address: " + s)
        { }
    }

    public abstract class Linker
    {
        public const int Failure = 1;
        public const int Success = 0;

        private static readonly AddressType[] SegmentAddressTypes = { AddressType.Code, AddressType.Data, AddressType.ZeroPage };
        private static readonly Dictionary<AddressType, string> SegmentNames = new Dictionary<AddressType, string>()
        {
            {AddressType.Code,"CSEG"},
            {AddressType.Data,"DSEG"},
            {AddressType.ZeroPage,"ZSEG"},
        };

        private readonly Dictionary<AddressType, int> addresses = new Dictionary<AddressType, int>();
        private readonly Dictionary<AddressType, Segment> segments = new Dictionary<AddressType, Segment>();
        private string? targetName;
        private readonly StringTable identifiers = new StringTable(1);
        private readonly Dictionary<int, Symbol> symbols = new Dictionary<int, Symbol>();
        private readonly SortedDictionary<Address, External> externals = new SortedDictionary<Address, External>();
        private readonly List<string> errors = new List<string>();

        protected Linker()
        {
            foreach (var addressType in SegmentAddressTypes) {
                segments[addressType] = new Segment(addressType);
            }
        }

        public int Main(NormalArgument normalArgument)
        {
            try {
                var args = normalArgument.Values;
                var index = 0;
                if (index >= args.Count) {
                    Console.Error.WriteLine("No target file.");
                    return Failure;
                }

                targetName = args[index++];
                var directory = Path.GetDirectoryName(targetName);
                if (string.IsNullOrEmpty(directory)) {
                    directory = Directory.GetCurrentDirectory();
                }

                ParseAddresses(args, ref index);

                if (index >= args.Count) {
                    Console.Error.WriteLine("No object file.");
                    return Failure;
                }

                var libraries = new List<Library.Library>();
                while (index < args.Count) {
                    string objName = args[index++];
                    //objNames.Add(objName);
                    var objDirectory = Path.GetDirectoryName(objName);
                    if (string.IsNullOrEmpty(objDirectory)) {
                        objName = directory + Path.DirectorySeparatorChar + objName;
                    }

                    var extension = Path.GetExtension(objName);
                    if (extension.ToLower() == Library.Library.Extension) {
                        var library = new Library.Library();
                        library.Load(objName);
                        libraries.Add(library);
                    }
                    else {
                        ReadObject(objName);
                    }
                }

                foreach (var library in libraries) {
                    var objects = new HashSet<Object>();
                    foreach (var external in externals.Select(pair => pair.Value)) {
                        if (symbols.TryGetValue(external.Id, out _))
                            continue;
                        var name = identifiers.FromId(external.Id);
                        Debug.Assert(name != null);
                        var obj = library.NameToObject(name);
                        if (obj == null)
                            continue;
                        objects.Add(obj);
                    }
                    foreach (var @object in objects) {
                        ReadObject(@object);
                    }
                }

                if (errors.Count > 0) {
                    return Failure;
                }

                ResolveExternals();
                if (errors.Count > 0)
                    return Failure;

                SaveTargetFile(targetName);

                string symbolFileName = directory + Path.DirectorySeparatorChar +
                                        Path.GetFileNameWithoutExtension(targetName) + ".symbols.txt";
                SaveSymbolFile(symbolFileName);

                return errors.Count <= 0 ? Success : Failure;
            }
            catch (Error e) {
                Console.Error.WriteLine(e.Message);
                return Failure;
            }
        }

        private void ParseAddresses(List<string> args, ref int index)
        {
            if (index >= args.Count) {
                throw new Error("No code address.");
            }
            addresses[AddressType.Code] = ParseAddress(args[index++]);

            if (index >= args.Count) {
                throw new Error("No data address.");
            }
            addresses[AddressType.Data] = ParseAddress(args[index++]);

            if (index < args.Count && int.TryParse(args[index], NumberStyles.AllowHexSpecifier, null, out var zeroPageAddress)) {
                addresses[AddressType.ZeroPage] = zeroPageAddress;
                ++index;
            }
        }

        protected int ParseAddress(string s)
        {
            if (!int.TryParse(s, NumberStyles.AllowHexSpecifier, null, out var address)) {
                throw new InvalidAddress(s);
            }
            return address;
        }

        protected abstract byte[] ToBytes(int value);

        private void ShowError(string error)
        {
            errors.Add(error);
            Console.Error.WriteLine(error);
        }

        private void RegisterSymbol(string name, Address address, Object obj)
        {
            var id = identifiers.Add(name);
            if (symbols.TryGetValue(id, out var symbol)) {
                if (symbol.Address.Type == AddressType.External) {
                    if (address.Type != AddressType.External) {
                        symbol.Address = address;
                    }
                }
                else if (address.Type != AddressType.External) {
                    ShowError("Duplicated symbol: " + name + "\n\t" + symbol.Object.Name + "\n\t" + obj.Name);
                }
            }
            else {
                symbols[id] = new Symbol(id, address, obj);
            }
        }

        private void FixAddress(Address location, Address value, int offset, AddressPart part)
        {
            Debug.Assert(SegmentAddressTypes.Contains(location.Type));
            Debug.Assert(SegmentAddressTypes.Contains(value.Type));
            var address = value.Value;
            if (value.Type >= 0) {
                address += addresses[value.Type];
            }

            var addedValue = address + offset;
            switch (part) {
                case AddressPart.Word:
                    segments[location.Type].WriteAddress(location.Value, ToBytes(addedValue));
                    break;
                case AddressPart.LowByte:
                    segments[location.Type].Bytes[location.Value] = (byte)(addedValue & 0xff);
                    break;
                case AddressPart.HighByte:
                    segments[location.Type].Bytes[location.Value] = (byte)((addedValue >> 8) & 0xff);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ReadObject(string fileName)
        {
            if (!File.Exists(fileName)) {
                ShowError("File not found: " + fileName);
                return;
            }

            var obj = new Object();
            obj.Load(fileName);

            ReadObject(obj);
        }

        private void ReadObject(Object obj)
        {
            var offsets = new Dictionary<AddressType, int>();
            foreach (var objSegment in obj.Segments) {
                offsets[objSegment.Type] = segments[objSegment.Type].Size;
                segments[objSegment.Type].Append(objSegment);
            }
            foreach (var objSymbol in obj.Symbols.Values) {
                Address address = objSymbol.Address;
                string name = objSymbol.Name;
                switch (address.Type) {
                    case AddressType.Undefined:
                    case AddressType.External:
                        //Debug.Assert(false);
                        break;
                    case AddressType.Const:
                        RegisterSymbol(name, address, obj);
                        break;
                    default:
                        Debug.Assert(SegmentAddressTypes.Contains(address.Type));
                        address = address.Add(offsets[address.Type]);
                        RegisterSymbol(name, address, obj);
                        break;
                }
            }

            foreach (var addressUsage in obj.AddressUsages) {
                Address location = addressUsage.Key;
                Debug.Assert(SegmentAddressTypes.Contains(location.Type));
                location = location.Add(offsets[location.Type]);
                Address value = addressUsage.Value;
                if (value.Type == AddressType.External) {
                    Debug.Assert(value.Id != null);
                    string name = obj.NameFromId(value.Id.Value);
                    var id = this.identifiers.Add(name);
                    externals[location] = new External(id, obj, value.Value, value.Part);
                }
                else {
                    Debug.Assert(SegmentAddressTypes.Contains(value.Type));
                    value = value.Add(offsets[value.Type]);
                    FixAddress(location, value, 0, value.Part);
                }
            }
        }

        private void ResolveExternals()
        {
            var messages = new HashSet<string>();
            foreach (var (key, external) in externals) {
                if (symbols.TryGetValue(external.Id, out var symbol)) {
                    FixAddress(key, symbol.Address, external.Offset, external.Part);
                }
                else {
                    var name = identifiers.FromId(external.Id);
                    messages.Add("Undefined external: " + name + " in " + external.Object.Name);
                }
            }
            foreach (var message in messages) {
                ShowError(message);
            }
        }


        private void SaveTargetFile(string fileName)
        {
            string ext = Path.GetExtension(fileName).ToUpper();
            SaveTargetFile(fileName, ext);
        }

        private static void PrintColumn(StreamWriter writer, string s, int maxLength)
        {
            writer.Write(s);
            int n = maxLength + 2 - s.Length;
            for (int i = 0; i < n; ++i) {
                writer.Write(' ');
            }
        }

        private static string ToHex(int addressValue)
        {
            return string.Format("{0:X04}", addressValue);
        }

        private const int AddressColumnLength = 5;

        private void SaveSymbolFile(string fileName)
        {
            using (StreamWriter stream = new StreamWriter(fileName, false, Encoding.UTF8)) {
                int maxNameLength = 0;
                int maxFileNameLength = 0;
                SortedDictionary<string, Symbol> nameIndexedSymbols = new SortedDictionary<string, Symbol>();
                foreach (KeyValuePair<int, Symbol> pair in symbols) {
                    var name = identifiers.FromId(pair.Key);
                    Debug.Assert(name != null);
                    Debug.Assert(pair.Value.Object.Name != null);
                    string objName = pair.Value.Object.Name;
                    maxNameLength = Math.Max(name.Length, maxNameLength);
                    maxFileNameLength = Math.Max(objName.Length, maxFileNameLength);
                    nameIndexedSymbols[name] = pair.Value;
                }
                PrintColumn(stream, "Symbol", maxNameLength);
                PrintColumn(stream, "Value", AddressColumnLength);
                PrintColumn(stream, "File", maxNameLength);
                stream.WriteLine();
                for (int i = 0; i < (maxNameLength + AddressColumnLength + maxFileNameLength) * 4 / 3; ++i) {
                    stream.Write('=');
                }
                stream.WriteLine();
                foreach (KeyValuePair<string, Symbol> pair in nameIndexedSymbols) {
                    string name = pair.Key;
                    Debug.Assert(pair.Value.Object.Name != null);
                    string objName = pair.Value.Object.Name;
                    Address address = pair.Value.Address;

                    int addressValue;
                    if (address.Type == AddressType.Const) {
                        addressValue = address.Value;
                    }
                    else {
                        Debug.Assert(SegmentAddressTypes.Contains(address.Type));
                        addressValue = addresses[address.Type] + address.Value;
                    }
                    PrintColumn(stream, name, maxNameLength);
                    PrintColumn(stream, ToHex(addressValue), AddressColumnLength);
                    PrintColumn(stream, objName, maxFileNameLength);
                    stream.WriteLine();
                }

                stream.WriteLine();
                foreach (var addressType in SegmentAddressTypes) {
                    if (addresses.ContainsKey(addressType)) {
                        stream.WriteLine(SegmentNames[addressType] + " " + ToHex(addresses[addressType]) + '-' + ToHex(addresses[addressType] + (segments[addressType].Size - 1)));
                    }
                }
            }
        }

        protected void SaveTargetFile(string fileName, string ext)
        {
            TargetFile targetFile = ToTargetFile(fileName, ext);
            targetFile.Write(addresses[0], segments[0].Bytes.ToArray());
            targetFile.Dispose();
        }

        protected virtual TargetFile ToTargetFile(string fileName, string ext)
        {
            return ext switch
            {
                ".CMT" => new CmtFile(fileName),
                ".P6" => new P6File(fileName),
                ".MZT" => new MztFile(fileName),
                ".CAS" => new CasFile(fileName),
                ".RAM" => new RamPakFile(fileName),
                ".HEX" => new HexFile(fileName),
                ".PRG" => new PrgFile(fileName),
                ".CJR" => new CjrFile(fileName),
                ".L3" => new Level3File(fileName),
                ".T64" => new T64File(fileName),
                ".C10" => new C10File(fileName),
                ".S" => new SRecordFile(fileName),
                _ => new BinFile(fileName)
            };
        }
    }
}
