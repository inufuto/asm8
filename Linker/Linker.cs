using Inu.Assembler;
using Inu.Language;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;


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

        //private readonly Dictionary<AddressType, List<Segment>> addresses = new Dictionary<AddressType, List<Segment>>();
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
                    var objName = args[index++];
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

                {
                    var changed = true;
                    while (changed) {
                        changed = false;
                        foreach (var library in libraries) {
                            var objects = new HashSet<Assembler.Object>();
                            foreach (var external in externals.Select(pair => pair.Value)) {
                                if (symbols.TryGetValue(external.Id, out _))
                                    continue;
                                var name = identifiers.FromId(external.Id);
                                Debug.Assert(name != null);
                                var obj = library.NameToObject(name);
                                if (obj == null)
                                    continue;
                                if (objects.Add(obj)) {
                                    changed = true;
                                }
                            }

                            foreach (var @object in objects) {
                                ReadObject(@object);
                            }
                        }
                    }
                }

                if (errors.Count > 0) {
                    return Failure;
                }

                ResolveExternals();
                if (errors.Count > 0)
                    return Failure;

                SaveTargetFile(targetName);

                var symbolFileName = directory + Path.DirectorySeparatorChar +
                                     Path.GetFileNameWithoutExtension(targetName) + ".symbols.txt";
                SaveSymbolFile(symbolFileName);

                return errors.Count <= 0 ? Success : Failure;
            }
            catch (Error e) {
                Console.Error.WriteLine(e.Message);
                return Failure;
            }
        }

        private void ParseAddresses(IReadOnlyList<string> args, ref int index)
        {
            if (index >= args.Count) {
                throw new Error("No code address.");
            }
            var arg0 = args[index++];
            if (!ParseAddress(arg0, AddressType.Code)) {
                throw new InvalidAddress(arg0);
            }

            if (index >= args.Count) {
                throw new Error("No data address.");
            }
            var arg1 = args[index++];
            if (!ParseAddress(arg1, AddressType.Data)) {
                throw new InvalidAddress(arg1);
            }

            if (ParseAddress(args[index], AddressType.ZeroPage)) {
                ++index;
            }
        }

        protected bool ParseAddress(string s, AddressType addressType)
        {
            var elements = s.Split(',', StringSplitOptions.RemoveEmptyEntries);
            foreach (var element in elements) {
                var addressees = element.Split('-', StringSplitOptions.RemoveEmptyEntries);
                if (addressees.Length <= 0) continue;
                if (!int.TryParse(addressees[0], NumberStyles.AllowHexSpecifier, null, out var minAddress)) return false;
                int? maxAddress = null;
                if (addressees.Length > 1) {
                    if (!int.TryParse(addressees[1], NumberStyles.AllowHexSpecifier, null, out var result)) {
                        throw new InvalidAddress(s);
                    }
                    maxAddress = result;
                }
                segments[addressType].Add(minAddress, maxAddress);
            }
            return true;
        }

        protected abstract byte[] ToBytes(int value);

        private void ShowError(string error)
        {
            errors.Add(error);
            Console.Error.WriteLine(error);
        }

        private void RegisterSymbol(string name, Address address, Assembler.Object obj)
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

        private void FixAddress(Address location, Address value, int offset, AddressPart part, bool relative)
        {
            Debug.Assert(SegmentAddressTypes.Contains(location.Type));
            Debug.Assert(SegmentAddressTypes.Contains(value.Type));
            var address = value.Value;
            //if (value.Type >= 0) {
            //    address = segments[value.Type].FixedAddress(address);
            //}

            var addedValue = address + offset;
            switch (part) {
                case AddressPart.Word:
                    if (relative) {
                        addedValue -= location.Value + 2;
                    }
                    segments[location.Type].WriteWord(location.Value, ToBytes(addedValue));
                    break;
                case AddressPart.LowByte:
                    if (relative) {
                        addedValue -= location.Value + 1;
                    }
                    segments[location.Type].WriteByte(location.Value, (byte)(addedValue & 0xff));
                    break;
                case AddressPart.HighByte:
                    if (relative) {
                        addedValue -= location.Value + 1;
                    }
                    segments[location.Type].WriteByte(location.Value, (byte)((addedValue >> 8) & 0xff));
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

            var obj = new Assembler.Object();
            obj.Load(fileName);

            ReadObject(obj);
        }

        private void ReadObject(Assembler.Object obj)
        {
            var heads = new Dictionary<AddressType, int>();
            foreach (var objSegment in obj.Segments) {
                segments[objSegment.Type].Append(objSegment);
                heads[objSegment.Type] = segments[objSegment.Type].HeadAddress;
            }
            foreach (var objSymbol in obj.Symbols.Values) {
                var address = objSymbol.Address;
                var name = objSymbol.Name;
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
                        Debug.Assert(address != null);
                        address = address.Add(heads[address.Type]);
                        RegisterSymbol(name, address, obj);
                        break;
                }
            }
            foreach (var addressUsage in obj.AddressUsages) {
                var location = addressUsage.Key;
                Debug.Assert(SegmentAddressTypes.Contains(location.Type));
                Debug.Assert(location != null);
                location = location.Add(heads[location.Type]);
                var value = addressUsage.Value;
                if (value.Type == AddressType.External) {
                    Debug.Assert(value.Id != null);
                    var name = obj.NameFromId(value.Id.Value);
                    var id = this.identifiers.Add(name);
                    externals[location] = new External(id, obj, value.Value, value.Part, value.Relative);
                }
                else {
                    Debug.Assert(SegmentAddressTypes.Contains(value.Type));
                    var head = heads[value.Type];
                    var added = value.Add(head);
                    FixAddress(location, added, 0, added.Part, false);
                }
            }
        }

        private void ResolveExternals()
        {
            var messages = new HashSet<string>();
            foreach (var (key, external) in externals) {
                if (symbols.TryGetValue(external.Id, out var symbol)) {
                    var name = identifiers.FromId(external.Id);
                    FixAddress(key, symbol.Address, external.Offset, external.Part, external.Relative);
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
            var ext = Path.GetExtension(fileName).ToUpper();
            SaveTargetFile(fileName, ext);
        }

        private static void PrintColumn(StreamWriter writer, string s, int maxLength)
        {
            writer.Write(s);
            var n = maxLength + 2 - s.Length;
            for (var i = 0; i < n; ++i) {
                writer.Write(' ');
            }
        }

        public static string ToHex(int addressValue)
        {
            return $"{addressValue:X04}";
        }

        private const int AddressColumnLength = 5;

        private void SaveSymbolFile(string fileName)
        {
            using StreamWriter stream = new StreamWriter(fileName, false, Encoding.UTF8);
            var maxNameLength = 0;
            var maxFileNameLength = 0;
            var nameIndexedSymbols = new SortedDictionary<string, Symbol>();
            foreach (KeyValuePair<int, Symbol> pair in symbols) {
                var name = identifiers.FromId(pair.Key);
                Debug.Assert(name != null);
                Debug.Assert(pair.Value.Object.Name != null);
                var objName = pair.Value.Object.Name;
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
            foreach (var pair in nameIndexedSymbols) {
                var name = pair.Key;
                Debug.Assert(pair.Value.Object.Name != null);
                var objName = pair.Value.Object.Name;
                var address = pair.Value.Address;

                var addressValue = address.Value;
                PrintColumn(stream, name, maxNameLength);
                PrintColumn(stream, ToHex(addressValue), AddressColumnLength);
                PrintColumn(stream, objName, maxFileNameLength);
                stream.WriteLine();
            }

            stream.WriteLine();
            foreach (var addressType in SegmentAddressTypes) {
                var segment = segments[addressType];
                if (segment.Empty) continue;
                stream.Write(SegmentNames[addressType] + " ");
                segment.PrintRanges(stream);
                stream.WriteLine();
            }
        }

        protected void SaveTargetFile(string fileName, string ext)
        {
            var targetFile = ToTargetFile(fileName, ext);
            var segment = segments[0];
            segment.Fill();
            targetFile.Write(segment.MinAddress, segment.Bytes.ToArray());
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
