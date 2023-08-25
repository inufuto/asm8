using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Inu.Assembler;

namespace Inu.Linker
{
    public class Segment : Assembler.Segment
    {
        private class Element
        {
            public readonly int MinAddress;
            public readonly int? MaxAddress;
            private int tailAddress;
            public int? Size => MaxAddress - MinAddress + 1;

            public Element(int minAddress, int? maxAddress)
            {
                MinAddress = minAddress;
                MaxAddress = maxAddress;
                tailAddress = minAddress - 1;
            }

            public void PrintRange(TextWriter stream, int length)
            {
                stream.Write(Linker.ToHex(MinAddress, length) + '-' + Linker.ToHex(tailAddress, length));
            }

            public void Expand()
            {
                ++tailAddress;
            }
        }

        public int MinAddress { get; private set; }

        public int? MaxAddress
        {
            get
            {
                var enumerable = elements.Where(s => s.MaxAddress != null).ToList();
                if (enumerable.Any()) {
                    return enumerable.Max(s =>
                    {
                        Debug.Assert(s.MaxAddress != null);
                        return s.MaxAddress.Value;
                    });
                }
                return null;
            }
        }

        private readonly List<Element> elements = new List<Element>();
        private int lastIndex = 0;
        public int HeadAddress { get; private set; }
        public int TailAddress { get; private set; }
        public bool Empty => elements.Count == 0;


        public Segment(AddressType type) : base(type) { }

        public void Add(int minAddress, int? maxAddress)
        {
            if (elements.Count == 0 || minAddress < MinAddress) {
                MinAddress = minAddress;
            }
            if (elements.Count == 0) {
                HeadAddress = TailAddress = minAddress;
            }
            elements.Add(new Element(minAddress, maxAddress));
        }

        public int FixedAddress(int address)
        {
            address += MinAddress;
            return address;
        }

        public void Append(Assembler.Segment segment)
        {
            if (segment.Size == 0) return;

            HeadAddress = TailAddress;
            var lastElement = elements[lastIndex];
            while (lastElement.MaxAddress != null && TailAddress + segment.Size > lastElement.MaxAddress + 1) {
                ++lastIndex;
                lastElement = elements[lastIndex];
                HeadAddress = TailAddress = lastElement.MinAddress;
            }
            var offset = TailAddress - MinAddress;
            foreach (var b in segment.Bytes) {
                if (offset < Bytes.Count) {
                    Bytes[offset] = b;
                }
                else {
                    while (Bytes.Count < offset) {
                        Bytes.Add(0);
                    }
                    Bytes.Add(b);
                }
                ++offset;
                ++TailAddress;
                lastElement.Expand();
            }
        }

        public void WriteByte(int address, byte b)
        {
            var offset = address - MinAddress;
            Debug.Assert(offset >= 0 && offset <= Size - 1);
            Bytes[offset] = b;
        }

        public void WriteBytes(int address, byte[] bytes)
        {
            foreach (var b in bytes) {
                WriteByte(address++, b);
            }
        }

        public void Fill()
        {
            if (MaxAddress == null) return;
            var maxSize = MaxAddress - MinAddress + 1;
            while (Bytes.Count < maxSize) {
                Bytes.Add(0);
            }
        }

        public void PrintRanges(StreamWriter stream, int length)
        {
            var index = 0;
            foreach (var element in elements) {
                if (index > 0) {
                    stream.Write(',');
                }
                element.PrintRange(stream, length);
                ++index;
            }
        }
    }
}
