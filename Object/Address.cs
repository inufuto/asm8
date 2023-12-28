using Inu.Language;
using System;
using System.Diagnostics;
using System.IO;

namespace Inu.Assembler
{
    public enum AddressType
    {
        Code = 0, Data = 1, ZeroPage = 2,
        Undefined = -1, Const = -2, External = -3
    }

    public enum AddressPart
    {
        Word, LowByte, HighByte,
        TripleByte, RevertedWord
    }

    public class Address : IComparable<Address>
    {
        public const int RelativeBit = 0x80;

        private static int LowByte(int value)
        {
            return value & 0xff;
        }
        private static int HighByte(int value)
        {
            return (value >> 8) & 0xff;
        }

        public readonly AddressType Type;
        public readonly bool Relative;
        public readonly AddressPart Part;
        public readonly int Value;
        public readonly int? Id;

        public bool Parenthesized { get; set; } = false;
        public static Address Default => new Address(AddressType.Const, 0);
        public Address RelativeValue => new Address(Type, Value, true, Id, Part);

        public Address(AddressType type, int value, bool relative, int? id = null, AddressPart part = AddressPart.Word)
        {
            if (IsExternal(type)) {
                Debug.Assert(id != null);
            }
            Type = type;
            Part = part;
            Relative = relative;
            Value = value;
            Id = id;
        }

        public Address(AddressType type, int value, int? id = null, AddressPart part = AddressPart.Word)
            : this(type, value, false, id, part) { }

        private static bool IsExternal(AddressType type)
        {
            return type == AddressType.External;
        }

        public Address(int constValue) : this(AddressType.Const, constValue) { }

        public Address(Stream stream)
        {
            var b = stream.ReadByte();
            if (b == RelativeBit) {
                Relative = true;
                b = stream.ReadByte();
            }
            Type = (AddressType)(sbyte)b;
            Value = stream.ReadWord();
            if (Type == AddressType.External) {
                Id = stream.ReadWord();
            }
            if (Type != AddressType.Const) {
                Part = (AddressPart)stream.ReadByte();
            }
        }

        public bool IsUndefined() { return Type == AddressType.Undefined; }

        public bool IsConst() { return Type == AddressType.Const; }

        public bool IsByte()
        {
            if (IsConst()) {
                return Value is >= 0 and < 0x100;
            }
            return Part is AddressPart.HighByte or AddressPart.LowByte;
        }


        public bool IsRelocatable() { return Type >= 0 || IsExternal(); }

        public bool IsExternal() => IsExternal(Type);

        public void Write(Stream stream)
        {
            if (Relative) {
                stream.WriteByte(RelativeBit);
            }
            stream.WriteByte((int)Type);
            stream.WriteWord(Value);
            if (Type == AddressType.External) {
                Debug.Assert(Id != null);
                stream.WriteWord(Id.Value);
            }
            if (Type != AddressType.Const) {
                stream.WriteByte((int)Part);
            }
        }

        public Address Add(int offset)
        {
            return new Address(Type, Value + offset, Id, Part);
        }

        public static bool operator ==(Address? a, Address? b)
        {
            if ((a as object) == null) { return (b as object) == null; }
            if ((b as object) == null) { return false; }
            return a.Type == b.Type && a.Value == b.Value;
        }

        public static bool operator !=(Address? a, Address? b)
        {
            return !(a == b);
        }

        public override bool Equals(object? obj)
        {
            return obj is Address address && this == address;
        }

        public override int GetHashCode()
        {
            return Type.GetHashCode() + Value.GetHashCode();
        }

        public int CompareTo(Address? address)
        {
            Debug.Assert(address != null);
            var compare = Type.CompareTo(address.Type);
            if (compare == 0) {
                compare = Value.CompareTo(address.Value);
            }
            return compare;
        }

        public Address Low()
        {
            return Type == AddressType.Const ? new Address(Type, LowByte(Value)) : new Address(Type, Value, Id, AddressPart.LowByte);
        }


        public Address High()
        {
            return Type == AddressType.Const ? new Address(Type, HighByte(Value)) : new Address(Type, Value, Id, AddressPart.HighByte);
        }

        public static bool IsOperationAvailable(int operatorId, Address left, Address right)
        {
            if (left.Type == AddressType.Const && right.Type == AddressType.Const)
                return true;

            return operatorId switch
            {
                '+' => left.Type == AddressType.Const || right.Type == AddressType.Const,
                '-' => right.Type == AddressType.Const ||
                       (left.Type == AddressType.Code && right.Type == AddressType.Code ||
                        left.Type == AddressType.Data && right.Type == AddressType.Data),
                _ => false
            };
        }

        public Address PartOf(AddressPart newPart)
        {
            Debug.Assert(Type != AddressType.Const);
            return new Address(Type, Value, Id, newPart);
        }

        public Address Revert()
        {
            return Type == AddressType.Const ? new Address(Type, (LowByte(Value) << 8) | HighByte(Value)) : new Address(Type, Value, Id, AddressPart.RevertedWord);
        }
    }
}
