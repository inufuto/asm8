using Inu.Language;
using System.Diagnostics;

namespace Inu.Assembler.Sc62015
{
    public class Assembler : LittleEndianAssembler
    {
        [Flags]
        private enum PreBytePurpose
        {
            First = 1, Second = 2
        }

        private static readonly int[] RegisterIds = { Keyword.A, Keyword.IL, Keyword.BA, Keyword.I, Keyword.X, Keyword.Y, Keyword.U, Keyword.S };
        private static readonly int[] RegisterSizes = { 1, 1, 2, 2, 3, 3, 3, 3 };

        private static readonly Dictionary<int, int> InternalRamAddresses = new()
        {
            { Keyword.BP, 0xec },
            { Keyword.PX, 0xed },
            { Keyword.PY, 0xee },
            { Keyword.IMR, 0xfb },
            { Keyword.SI, 0xda },
            { Keyword.DI, 0xdd },
            { Keyword.BX, 0xd4 },
            { Keyword.CX, 0xd6 },
            { Keyword.DX, 0xd8 },
            { Keyword.BL, 0xd4 },
            { Keyword.BH, 0xd5 },
            { Keyword.CL, 0xd6 },
            { Keyword.CH, 0xd7 },
            { Keyword.DL, 0xd8 },
            { Keyword.DH, 0xd9 },
        };

        private class PreByte
        {
            public readonly int Code;
            public readonly Address Offset;
            public readonly PreBytePurpose Purpose;

            public PreByte(int code, Address? offset, PreBytePurpose purpose)
            {
                Code = code;
                Offset = offset != null ? offset : new Address(0);
                Purpose = purpose;
            }

            public PreByte(int code, PreBytePurpose purpose)
            {
                Code = code;
                Offset = new Address(0);
                Purpose = purpose;
            }
        }

        public override bool ZeroPageAvailable => true;
        public override AddressPart PointerAddressPart => AddressPart.TByte;

        public Assembler() : base(new Tokenizer(), 20) { }

        protected override bool IsRelativeOffsetInRange(int offset)
        {
            return offset is >= -0x100 and <= 0x100;
        }


        private static int? RegisterIndex(Token token)
        {
            if (token is not ReservedWord reservedWord) return null;
            var index = 0;
            foreach (var registerId in RegisterIds) {
                if (reservedWord.Id == registerId) return index;
                ++index;
            }
            return null;
        }

        private void ShowInvalidAddressing(Token token)
        {
            ShowError(token.Position, "Invalid addressing.");
        }

        private Address? Offset()
        {
            if (LastToken.IsReservedWord('+')) {
                NextToken();
            }
            if (LastToken.IsReservedWord('-')) {
                var token = NextToken();
                var expression = Expression();
                if (expression != null && expression.IsConst()) {
                    return new Address(-expression.Value);
                }
                ShowAddressUsageError(token);
                return expression;
            }
            return Expression();
        }


        private PreByte? ParsePreByte(PreBytePurpose purpose)
        {
            if (LastToken is ReservedWord reservedWord && InternalRamAddresses.TryGetValue(reservedWord.Id, out var address)) {
                NextToken();
                if (purpose.HasFlag(PreBytePurpose.First)) {
                    return new PreByte(0x30, new Address(address), PreBytePurpose.First);
                }
                if (purpose.HasFlag(PreBytePurpose.Second)) {
                    return new PreByte(0x22, new Address(address), PreBytePurpose.Second);
                }
            }
            if (!LastToken.IsReservedWord('(')) return null;
            NextToken();
            if (LastToken.IsReservedWord(Keyword.BP)) {
                NextToken();
                if (LastToken.IsReservedWord('+')) {
                    NextToken();
                    if (LastToken.IsReservedWord(Keyword.PX)) {
                        if (purpose.HasFlag(PreBytePurpose.First)) {
                            NextToken();
                            AcceptReservedWord(')');
                            return new PreByte(0x24, PreBytePurpose.First);
                        }
                        ShowInvalidAddressing(LastToken);
                    }
                    if (LastToken.IsReservedWord(Keyword.PY)) {
                        if (purpose.HasFlag(PreBytePurpose.Second)) {
                            NextToken();
                            AcceptReservedWord(')');
                            return new PreByte(0x21, PreBytePurpose.Second);
                        }
                        ShowInvalidAddressing(LastToken);
                    }
                    var preByte = new PreByte(0, Expression(), PreBytePurpose.First | PreBytePurpose.Second);
                    AcceptReservedWord(')');
                    return preByte;
                }
                AcceptReservedWord(')');
                return new PreByte(0, PreBytePurpose.First | PreBytePurpose.Second);
            }
            if (LastToken.IsReservedWord(Keyword.PX)) {
                if (purpose.HasFlag(PreBytePurpose.First)) {
                    NextToken();
                    var preByte = new PreByte(0x34, Offset(), PreBytePurpose.First);
                    AcceptReservedWord(')');
                    return preByte;
                }
                ShowInvalidAddressing(LastToken);
            }
            if (LastToken.IsReservedWord(Keyword.PY)) {
                if (purpose.HasFlag(PreBytePurpose.Second)) {
                    NextToken();
                    var preByte = new PreByte(0x23, Offset(), PreBytePurpose.Second);
                    AcceptReservedWord(')');
                    return preByte;
                }
                ShowInvalidAddressing(LastToken);
            }
            if (purpose.HasFlag(PreBytePurpose.First)) {
                var preByte = new PreByte(0x30, Expression(), PreBytePurpose.First);
                AcceptReservedWord(')');
                return preByte;
            }
            if (purpose.HasFlag(PreBytePurpose.Second)) {
                var preByte = new PreByte(0x22, Expression(), PreBytePurpose.Second);
                AcceptReservedWord(')');
                return preByte;
            }
            ShowInvalidAddressing(LastToken);
            return new PreByte(0, 0);
        }


        private bool ParseExternalRam(PreBytePurpose preBytePurpose, out int? secondCode, out PreByte? preByte, out Address? offset)
        {
            secondCode = null;
            preByte = null;
            offset = null;
            if (!LastToken.IsReservedWord('[')) return false;
            NextToken();
            {
                preByte = ParsePreByte(preBytePurpose);
                if (preByte != null) {
                    // internal RAM
                    if (LastToken.IsReservedWord('+')) {
                        NextToken();
                        secondCode = 0x80;
                        offset = Expression();
                    }
                    else if (LastToken.IsReservedWord('-')) {
                        NextToken();
                        secondCode = 0xc0;
                        offset = Expression();
                    }
                    else {
                        secondCode = 0x00;
                    }
                    AcceptReservedWord(']');
                    return true;
                }
            }
            {
                offset = Expression();
                if (offset != null) {
                    // absolute address
                    AcceptReservedWord(']');
                    secondCode = null;
                    return true;
                }
            }
            {
                if (LastToken.IsReservedWord(Keyword.Decrement)) {
                    NextToken();
                    secondCode = 0x30;
                }
                var register = RegisterIndex(LastToken);
                if (register != null && RegisterSizes[register.Value] == 3) {
                    // pointer register
                    NextToken();
                    if (secondCode != null) {
                        secondCode |= register.Value;
                    }
                    else {
                        if (LastToken.IsReservedWord(Keyword.Increment)) {
                            NextToken();
                            secondCode = 0x20 | register.Value;
                        }
                        else if (LastToken.IsReservedWord('+')) {
                            NextToken();
                            secondCode = 0x80 | register.Value;
                            offset = Expression();
                        }
                        else if (LastToken.IsReservedWord('-')) {
                            NextToken();
                            secondCode = 0xc0 | register.Value;
                            offset = Expression();
                        }
                        else if (secondCode == null) {
                            secondCode = 0x00 | register.Value;
                        }
                    }
                    AcceptReservedWord(']');
                    return true;
                }
            }
            return false;
        }

        private void MV()
        {
            {
                // to register
                var leftRegister = RegisterIndex(LastToken);
                if (leftRegister != null) {
                    var leftToken = LastToken;
                    NextToken();
                    AcceptReservedWord(',');
                    var rightToken = LastToken;
                    {
                        if (ParseExternalRam(PreBytePurpose.First | PreBytePurpose.Second, out var secondCode, out var preByte, out var offset)) {
                            if (secondCode == null) {
                                if (offset != null) {
                                    WriteByte(0x88 | leftRegister.Value);
                                    WritePointer(rightToken, offset);
                                    return;
                                }
                            }
                            else {
                                if (leftToken.IsReservedWord(Keyword.S)) {
                                    ShowInvalidRegister(leftToken);
                                }
                                if (preByte == null) {
                                    WriteByte(0x90 | leftRegister.Value);
                                    WriteByte(secondCode.Value);
                                    if (offset != null) {
                                        WriteByte(rightToken, offset);
                                    }
                                    return;
                                }
                                if (preByte.Code != 0) {
                                    WriteByte(preByte.Code);
                                }
                                WriteByte(0x98 | leftRegister.Value);
                                WriteByte(secondCode.Value);
                                WriteByte(rightToken, preByte.Offset);
                                if (offset != null) {
                                    WriteByte(rightToken, offset);
                                }
                                return;
                            }
                        }
                    }
                    {
                        var offsetToken = LastToken;
                        var preByte = ParsePreByte(PreBytePurpose.First | PreBytePurpose.Second);
                        if (preByte != null) {
                            // from internal RAM
                            if (preByte.Code != 0) {
                                WriteByte(preByte.Code);
                            }

                            WriteByte(0x80 | leftRegister.Value);
                            WriteByte(offsetToken, preByte.Offset);
                            return;
                        }
                    }
                    {
                        // immediate
                        var valueToken = LastToken;
                        var value = Expression();
                        if (value != null) {
                            WriteByte(0x08 | leftRegister.Value);
                            switch (RegisterSizes[leftRegister.Value]) {
                                case 1:
                                    WriteByte(valueToken, value);
                                    break;
                                case 2:
                                    WriteWord(valueToken, value);
                                    break;
                                case 3:
                                    WritePointer(valueToken, value);
                                    break;
                            }
                            return;
                        }
                    }
                    if (LastToken.IsReservedWord(Keyword.B)) {
                        NextToken();
                        WriteByte(0x74);
                        return;
                    }
                    {
                        // from register
                        var rightRegister = RegisterIndex(LastToken);
                        if (rightRegister != null) {
                            NextToken();
                            WriteByte(0xfd);
                            WriteByte((leftRegister.Value << 4) | rightRegister.Value);
                            return;
                        }
                    }
                }
            }
            if (LastToken.IsReservedWord(Keyword.B)) {
                NextToken();
                AcceptReservedWord(',');
                AcceptReservedWord(Keyword.A);
                WriteByte(0x75);
                return;
            }
            {
                var leftToken = LastToken;
                if (ParseExternalRam(PreBytePurpose.First | PreBytePurpose.Second, out var secondCode, out var preByte1, out var offset)) {
                    AcceptReservedWord(',');
                    var rightToken = LastToken;
                    {
                        var rightRegister = RegisterIndex(LastToken);
                        if (rightRegister != null) {
                            NextToken();
                            // from register
                            if (secondCode == null) {
                                if (offset != null) {
                                    WriteByte(0xa8 | rightRegister.Value);
                                    WritePointer(leftToken, offset);
                                    return;
                                }
                            }
                            else {
                                if (rightToken.IsReservedWord(Keyword.S)) {
                                    ShowInvalidRegister(rightToken);
                                }
                                if (preByte1 == null) {
                                    WriteByte(0xb0 | rightRegister.Value);
                                    WriteByte(secondCode.Value);
                                    if (offset != null) {
                                        WriteByte(leftToken, offset);
                                    }
                                    return;
                                }
                                if (preByte1.Code != 0) {
                                    WriteByte(preByte1.Code);
                                }
                                WriteByte(0xb8 | rightRegister.Value);
                                WriteByte(secondCode.Value);
                                WriteByte(rightToken, preByte1.Offset);
                                if (offset != null) {
                                    WriteByte(rightToken, offset);
                                }
                                return;
                            }
                        }
                    }
                    {
                        PreBytePurpose nextPurpose;
                        if (preByte1 != null) {
                            nextPurpose = preByte1.Purpose switch
                            {
                                PreBytePurpose.First => PreBytePurpose.Second,
                                PreBytePurpose.Second => 0,
                                _ => PreBytePurpose.First | PreBytePurpose.Second
                            };
                        }
                        else {
                            nextPurpose = PreBytePurpose.First | PreBytePurpose.Second;
                        }
                        {
                            // from internal RAM
                            var preByte2 = ParsePreByte(nextPurpose);
                            if (preByte2 != null) {
                                var preByte = preByte1 != null ? preByte1.Code | preByte2.Code : preByte2.Code;
                                if (preByte != 0) {
                                    WriteByte(preByte);
                                }

                                if (secondCode == null) {
                                    if (offset != null) {
                                        WriteByte(0xd8);
                                        WritePointer(leftToken, offset);
                                        WriteByte(rightToken, preByte2.Offset);
                                        return;
                                    }
                                }
                                else {
                                    if (preByte1 == null) {
                                        WriteByte(0xe8);
                                        WriteByte(secondCode.Value);
                                        WriteByte(leftToken, preByte2.Offset);
                                        if (offset != null) {
                                            WriteByte(rightToken, offset);
                                        }
                                    }
                                    else {
                                        WriteByte(0xf8);
                                        WriteByte(secondCode.Value);
                                        WriteByte(leftToken, preByte1.Offset);
                                        WriteByte(leftToken, preByte2.Offset);
                                        if (offset != null) {
                                            WriteByte(rightToken, offset);
                                        }
                                    }
                                }

                                return;
                            }
                        }
                    }
                }
            }
            {
                var leftToken = LastToken;
                var preByte1 = ParsePreByte(PreBytePurpose.First | PreBytePurpose.Second);
                if (preByte1 != null) {
                    // to internal RAM
                    AcceptReservedWord(',');
                    {
                        var rightToken = LastToken;
                        var nextPurpose = preByte1.Purpose.HasFlag(PreBytePurpose.Second)
                            ? 0
                            : PreBytePurpose.Second;
                        if (ParseExternalRam(nextPurpose, out var secondCode, out var preByte2, out var offset)) {
                            if (secondCode == null) {
                                if (offset != null) {
                                    if (preByte1.Code != 0) {
                                        WriteByte(preByte1.Code);
                                    }

                                    WriteByte(0xd0);
                                    WriteByte(leftToken, preByte1.Offset);
                                    WritePointer(rightToken, offset);
                                    return;
                                }
                            }
                            else {
                                if (preByte2 == null) {
                                    if (preByte1.Code != 0) {
                                        WriteByte(preByte1.Code);
                                    }

                                    WriteByte(0xe0);
                                    WriteByte(secondCode.Value);
                                    WriteByte(leftToken, preByte1.Offset);
                                    if (offset != null) {
                                        WriteByte(rightToken, offset);
                                    }

                                    return;
                                }

                                var preByte = preByte1.Code | preByte2.Code;
                                if (preByte != 0) {
                                    WriteByte(preByte);
                                }

                                WriteByte(0xf0);
                                WriteByte(secondCode.Value);
                                WriteByte(rightToken, preByte1.Offset);
                                WriteByte(rightToken, preByte2.Offset);
                                if (offset != null) {
                                    WriteByte(rightToken, offset);
                                }

                                return;
                            }
                        }
                    }
                    {
                        // from register
                        var rightRegister = RegisterIndex(LastToken);
                        if (rightRegister != null) {
                            NextToken();
                            if (preByte1.Code != 0) {
                                WriteByte(preByte1.Code);
                            }

                            WriteByte(0xa0 | rightRegister.Value);
                            WriteByte(leftToken, preByte1.Offset);
                            return;
                        }
                    }
                    if (preByte1.Purpose.HasFlag(PreBytePurpose.First)) {
                        var valueToken = LastToken;
                        var preByte2 = ParsePreByte(PreBytePurpose.Second);
                        if (preByte2 != null) {
                            NextToken();
                            // from internal RAM
                            var code = preByte1.Code | preByte2.Code;
                            if (code != 0) {
                                WriteByte(code);
                            }
                            WriteByte(0xc8);
                            WriteByte(leftToken, preByte1.Offset);
                            WriteByte(valueToken, preByte2.Offset);
                            return;
                        }
                    }

                    {
                        // immediate
                        var valueToken = LastToken;
                        var value = Expression();
                        if (value != null) {
                            if (preByte1.Code != 0) {
                                WriteByte(preByte1.Code);
                            }

                            WriteByte(0xcc);
                            WriteByte(leftToken, preByte1.Offset);
                            WriteByte(valueToken, value);
                            return;
                        }
                    }
                }
            }
            ShowSyntaxError(LastToken);
        }


        private void MVW()
        {
            var leftToken = LastToken;
            {
                if (ParseExternalRam(PreBytePurpose.First | PreBytePurpose.Second, out var secondCode, out var preByte1,
                        out var offset)) {
                    AcceptReservedWord(',');
                    var rightToken = LastToken;
                    {
                        PreBytePurpose nextPurpose;
                        if (preByte1 != null) {
                            nextPurpose = preByte1.Purpose switch
                            {
                                PreBytePurpose.First => PreBytePurpose.Second,
                                PreBytePurpose.Second => 0,
                                _ => PreBytePurpose.First | PreBytePurpose.Second
                            };
                        }
                        else {
                            nextPurpose = PreBytePurpose.First | PreBytePurpose.Second;
                        }

                        {
                            // from internal RAM
                            var preByte2 = ParsePreByte(nextPurpose);
                            if (preByte2 != null) {
                                var preByte = preByte1 != null ? preByte1.Code | preByte2.Code : preByte2.Code;
                                if (preByte != 0) {
                                    WriteByte(preByte);
                                }

                                if (secondCode == null) {
                                    if (offset != null) {
                                        WriteByte(0xd9);
                                        WritePointer(leftToken, offset);
                                        WriteByte(rightToken, preByte2.Offset);
                                        return;
                                    }
                                }
                                else {
                                    if (preByte1 == null) {
                                        WriteByte(0xe9);
                                        WriteByte(secondCode.Value);
                                        WriteByte(leftToken, preByte2.Offset);
                                        if (offset != null) {
                                            WriteByte(rightToken, offset);
                                        }
                                    }
                                    else {
                                        WriteByte(0xf9);
                                        WriteByte(secondCode.Value);
                                        WriteByte(leftToken, preByte1.Offset);
                                        WriteByte(leftToken, preByte2.Offset);
                                        if (offset != null) {
                                            WriteByte(rightToken, offset);
                                        }
                                    }
                                }
                                return;
                            }
                        }
                    }
                }
            }
            {
                var preByte1 = ParsePreByte(PreBytePurpose.First | PreBytePurpose.Second);
                if (preByte1 != null) {
                    // to Internal RAM
                    AcceptReservedWord(',');
                    {
                        var rightToken = LastToken;
                        var nextPurpose = preByte1.Purpose.HasFlag(PreBytePurpose.Second) ? 0 : PreBytePurpose.Second;
                        if (ParseExternalRam(nextPurpose, out var secondCode, out var preByte2, out var offset)) {
                            if (secondCode == null) {
                                if (offset != null) {
                                    if (preByte1.Code != 0) {
                                        WriteByte(preByte1.Code);
                                    }

                                    WriteByte(0xd1);
                                    WriteByte(leftToken, preByte1.Offset);
                                    WritePointer(rightToken, offset);
                                    return;
                                }
                            }
                            else {
                                if (preByte2 == null) {
                                    if (preByte1.Code != 0) {
                                        WriteByte(preByte1.Code);
                                    }

                                    WriteByte(0xe1);
                                    WriteByte(secondCode.Value);
                                    WriteByte(leftToken, preByte1.Offset);
                                    if (offset != null) {
                                        WriteByte(rightToken, offset);
                                    }

                                    return;
                                }

                                var preByte = preByte1.Code | preByte2.Code;
                                if (preByte != 0) {
                                    WriteByte(preByte);
                                }

                                WriteByte(0xf1);
                                WriteByte(secondCode.Value);
                                WriteByte(rightToken, preByte1.Offset);
                                WriteByte(rightToken, preByte2.Offset);
                                if (offset != null) {
                                    WriteByte(rightToken, offset);
                                }

                                return;
                            }
                        }
                    }
                    if (preByte1.Purpose.HasFlag(PreBytePurpose.First)) {
                        var valueToken = LastToken;
                        var preByte2 = ParsePreByte(PreBytePurpose.Second);
                        if (preByte2 != null) {
                            // from internal RAM
                            var preByte = preByte1.Code | preByte2.Code;
                            if (preByte != 0) {
                                WriteByte(preByte);
                            }

                            WriteByte(0xc9);
                            WriteByte(leftToken, preByte1.Offset);
                            WriteByte(valueToken, preByte2.Offset);
                            return;
                        }
                    }
                    {
                        // immediate
                        var valueToken = LastToken;
                        var value = Expression();
                        if (value != null) {
                            if (preByte1.Code != 0) {
                                WriteByte(preByte1.Code);
                            }

                            WriteByte(0xcd);
                            WriteByte(leftToken, preByte1.Offset);
                            WriteWord(valueToken, value);
                            return;
                        }
                    }
                }
            }
            ShowSyntaxError(LastToken);
        }

        private void MVP()
        {
            var leftToken = LastToken;
            {
                if (ParseExternalRam(PreBytePurpose.First | PreBytePurpose.Second, out var secondCode, out var preByte1,
                        out var offset)) {
                    AcceptReservedWord(',');
                    var rightToken = LastToken;
                    {
                        PreBytePurpose nextPurpose;
                        if (preByte1 != null) {
                            nextPurpose = preByte1.Purpose switch
                            {
                                PreBytePurpose.First => PreBytePurpose.Second,
                                PreBytePurpose.Second => 0,
                                _ => PreBytePurpose.First | PreBytePurpose.Second
                            };
                        }
                        else {
                            nextPurpose = PreBytePurpose.First | PreBytePurpose.Second;
                        }
                        {
                            var preByte2 = ParsePreByte(nextPurpose);
                            if (preByte2 != null) {
                                // from internal RAM
                                var preByte = preByte1 != null ? preByte1.Code | preByte2.Code : preByte2.Code;
                                if (preByte != 0) {
                                    WriteByte(preByte);
                                }
                                if (secondCode == null) {
                                    if (offset != null) {
                                        WriteByte(0xda);
                                        WritePointer(leftToken, offset);
                                        WriteByte(rightToken, preByte2.Offset);
                                        return;
                                    }
                                }
                                else {
                                    if (preByte1 == null) {
                                        WriteByte(0xea);
                                        WriteByte(secondCode.Value);
                                        WriteByte(leftToken, preByte2.Offset);
                                        if (offset != null) {
                                            WriteByte(rightToken, offset);
                                        }
                                    }
                                    else {
                                        WriteByte(0xfa);
                                        WriteByte(secondCode.Value);
                                        WriteByte(leftToken, preByte1.Offset);
                                        WriteByte(leftToken, preByte2.Offset);
                                        if (offset != null) {
                                            WriteByte(rightToken, offset);
                                        }
                                    }
                                }
                                return;
                            }
                        }
                    }
                }
            }
            {
                var preByte1 = ParsePreByte(PreBytePurpose.First | PreBytePurpose.Second);
                if (preByte1 != null) {
                    // to Internal RAM
                    AcceptReservedWord(',');
                    {
                        var rightToken = LastToken;
                        var nextPurpose = preByte1.Purpose.HasFlag(PreBytePurpose.Second) ? 0 : PreBytePurpose.Second;
                        if (ParseExternalRam(nextPurpose, out var secondCode, out var preByte2, out var offset)) {
                            if (secondCode == null) {
                                if (offset != null) {
                                    if (preByte1.Code != 0) {
                                        WriteByte(preByte1.Code);
                                    }

                                    WriteByte(0xd1);
                                    WriteByte(leftToken, preByte1.Offset);
                                    WritePointer(rightToken, offset);
                                    return;
                                }
                            }
                            else {
                                if (preByte2 == null) {
                                    if (preByte1.Code != 0) {
                                        WriteByte(preByte1.Code);
                                    }

                                    WriteByte(0xe1);
                                    WriteByte(secondCode.Value);
                                    WriteByte(leftToken, preByte1.Offset);
                                    if (offset != null) {
                                        WriteByte(rightToken, offset);
                                    }

                                    return;
                                }

                                var preByte = preByte1.Code | preByte2.Code;
                                if (preByte != 0) {
                                    WriteByte(preByte);
                                }

                                WriteByte(0xf1);
                                WriteByte(secondCode.Value);
                                WriteByte(rightToken, preByte1.Offset);
                                WriteByte(rightToken, preByte2.Offset);
                                if (offset != null) {
                                    WriteByte(rightToken, offset);
                                }

                                return;
                            }
                        }
                    }
                    if (preByte1.Purpose.HasFlag(PreBytePurpose.First)) {
                        var valueToken = LastToken;
                        var preByte2 = ParsePreByte(PreBytePurpose.Second);
                        if (preByte2 != null) {
                            // from internal RAM
                            var preByte = preByte1.Code | preByte2.Code;
                            if (preByte != 0) {
                                WriteByte(preByte);
                            }
                            WriteByte(0xca);
                            WriteByte(leftToken, preByte1.Offset);
                            WriteByte(valueToken, preByte2.Offset);
                            return;
                        }
                    }
                    {
                        // immediate
                        var valueToken = LastToken;
                        var value = Expression();
                        if (value != null) {
                            if (preByte1.Code != 0) {
                                WriteByte(preByte1.Code);
                            }

                            WriteByte(0xdc);
                            WriteByte(leftToken, preByte1.Offset);
                            WritePointer(valueToken, value);
                            return;
                        }
                    }
                }
            }
            ShowSyntaxError(LastToken);
        }

        private void MVL()
        {
            var leftToken = LastToken;
            {
                if (ParseExternalRam(PreBytePurpose.First | PreBytePurpose.Second, out var secondCode, out var preByte1,
                        out var offset)) {
                    AcceptReservedWord(',');
                    var rightToken = LastToken;
                    {
                        PreBytePurpose nextPurpose;
                        if (preByte1 != null) {
                            nextPurpose = preByte1.Purpose switch
                            {
                                PreBytePurpose.First => PreBytePurpose.Second,
                                PreBytePurpose.Second => 0,
                                _ => PreBytePurpose.First | PreBytePurpose.Second
                            };
                        }
                        else {
                            nextPurpose = PreBytePurpose.First | PreBytePurpose.Second;
                        }

                        {
                            var preByte2 = ParsePreByte(nextPurpose);
                            if (preByte2 != null) {
                                // from internal RAM
                                var preByte = preByte1 != null ? preByte1.Code | preByte2.Code : preByte2.Code;
                                if (preByte != 0) {
                                    WriteByte(preByte);
                                }

                                if (secondCode == null) {
                                    if (offset != null) {
                                        WriteByte(0xdb);
                                        WritePointer(leftToken, offset);
                                        WriteByte(rightToken, preByte2.Offset);
                                        return;
                                    }
                                }
                                else {
                                    if (preByte1 == null) {
                                        if (offset != null) {
                                            WriteByte(0x5e);
                                            WriteByte(secondCode.Value);
                                            WriteByte(leftToken, preByte2.Offset);
                                            WriteByte(rightToken, offset);
                                        }
                                        else {
                                            WriteByte(0xeb);
                                            WriteByte(secondCode.Value);
                                            WriteByte(leftToken, preByte2.Offset);
                                        }
                                    }
                                    else {
                                        WriteByte(0xfb);
                                        WriteByte(secondCode.Value);
                                        WriteByte(leftToken, preByte1.Offset);
                                        WriteByte(leftToken, preByte2.Offset);
                                        if (offset != null) {
                                            WriteByte(rightToken, offset);
                                        }
                                    }
                                }
                                return;
                            }
                        }
                    }
                }
            }
            {
                var preByte1 = ParsePreByte(PreBytePurpose.First | PreBytePurpose.Second);
                if (preByte1 != null) {
                    // to Internal RAM
                    AcceptReservedWord(',');
                    {
                        var rightToken = LastToken;
                        var nextPurpose = preByte1.Purpose.HasFlag(PreBytePurpose.Second) ? 0 : PreBytePurpose.Second;
                        if (ParseExternalRam(nextPurpose, out var secondCode, out var preByte2, out var offset)) {
                            if (secondCode == null) {
                                if (offset != null) {
                                    if (preByte1.Code != 0) {
                                        WriteByte(preByte1.Code);
                                    }
                                    WriteByte(0xd3);
                                    WriteByte(leftToken, preByte1.Offset);
                                    WritePointer(rightToken, offset);
                                    return;
                                }
                            }
                            else {
                                if (preByte2 == null) {
                                    if (preByte1.Code != 0) {
                                        WriteByte(preByte1.Code);
                                    }
                                    if (offset != null) {
                                        WriteByte(0x56);
                                        WriteByte(secondCode.Value);
                                        WriteByte(leftToken, preByte1.Offset);
                                        WriteByte(rightToken, offset);
                                    }
                                    else {
                                        WriteByte(0xe3);
                                        WriteByte(secondCode.Value);
                                        WriteByte(leftToken, preByte1.Offset);
                                    }
                                    return;
                                }
                                var preByte = preByte1.Code | preByte2.Code;
                                if (preByte != 0) {
                                    WriteByte(preByte);
                                }
                                WriteByte(0xf3);
                                WriteByte(secondCode.Value);
                                WriteByte(rightToken, preByte1.Offset);
                                WriteByte(rightToken, preByte2.Offset);
                                if (offset != null) {
                                    WriteByte(rightToken, offset);
                                }
                                return;
                            }
                        }
                    }
                }
            }
            ShowSyntaxError(LastToken);
        }

        private void InterInternal(int code)
        {
            {
                var leftToken = LastToken;
                var preByte1 = ParsePreByte(PreBytePurpose.First);
                if (preByte1 != null) {
                    AcceptReservedWord(',');
                    var rightToken = LastToken;
                    var preByte2 = ParsePreByte(PreBytePurpose.Second);
                    if (preByte2 != null) {
                        var preByte = preByte1.Code | preByte2.Code;
                        if (preByte != 0) {
                            WriteByte(preByte);
                        }
                        WriteByte(code);
                        WriteByte(leftToken, preByte1.Offset);
                        WriteByte(rightToken, preByte2.Offset);
                        return;
                    }
                }
            }
            ShowSyntaxError(LastToken);
        }

        private void EX()
        {
            if (LastToken.IsReservedWord(Keyword.A)) {
                NextToken();
                AcceptReservedWord(',');
                AcceptReservedWord(Keyword.B);
                WriteByte(0xdd);
                return;
            }
            {
                var leftRegister = RegisterIndex(LastToken);
                if (leftRegister != null) {
                    NextToken();
                    var registerSize = RegisterSizes[leftRegister.Value];
                    if (registerSize is 2 or 3) {
                        AcceptReservedWord(',');
                        var rightToken = LastToken;
                        var rightRegister = RegisterIndex(rightToken);
                        if (rightRegister != null) {
                            NextToken();
                            if (RegisterSizes[rightRegister.Value] != registerSize) {
                                ShowInvalidRegister(rightToken);
                            }
                            WriteByte(0xed);
                            WriteByte((leftRegister.Value << 4) | rightRegister.Value);
                            return;
                        }
                        ShowSyntaxError(rightToken);
                        return;
                    }
                }
            }
            InterInternal(0xc0);
        }

        private void PUSHS()
        {
            if (LastToken.IsReservedWord(Keyword.F)) {
                NextToken();
                WriteByte(0x4f);
                return;
            }
            if (LastToken.IsReservedWord(Keyword.IMR)) {
                NextToken();
                WriteByte(0x30);
                WriteByte(0xe8);
                WriteByte(0x37);
                WriteByte(0xfb);
                return;
            }
            {
                var registerIndex = RegisterIndex(LastToken);
                if (registerIndex != null) {
                    if (RegisterSizes[registerIndex.Value] <= 2 || LastToken.IsReservedWord(Keyword.X) || LastToken.IsReservedWord(Keyword.Y)) {
                        NextToken();
                        WriteByte(0xb0 | registerIndex.Value);
                        WriteByte(0x37);
                        return;
                    }
                    ShowInvalidRegister(LastToken);
                }
            }
            ShowSyntaxError(LastToken);
        }

        private void POPS()
        {
            if (LastToken.IsReservedWord(Keyword.F)) {
                NextToken();
                WriteByte(0x5f);
                return;
            }
            if (LastToken.IsReservedWord(Keyword.IMR)) {
                NextToken();
                WriteByte(0x30);
                WriteByte(0xe0);
                WriteByte(0x27);
                WriteByte(0xfb);
                return;
            }
            {
                var registerIndex = RegisterIndex(LastToken);
                if (registerIndex != null) {
                    if (RegisterSizes[registerIndex.Value] <= 2 || LastToken.IsReservedWord(Keyword.X) || LastToken.IsReservedWord(Keyword.Y)) {
                        NextToken();
                        WriteByte(0x90 | registerIndex.Value);
                        WriteByte(0x27);
                        return;
                    }
                    ShowInvalidRegister(LastToken);
                }
            }
            ShowSyntaxError(LastToken);
        }

        private void PUSHU()
        {
            if (LastToken.IsReservedWord(Keyword.F)) {
                NextToken();
                WriteByte(0x2e);
                return;
            }
            if (LastToken.IsReservedWord(Keyword.IMR)) {
                NextToken();
                WriteByte(0x2f);
                return;
            }
            {
                var registerIndex = RegisterIndex(LastToken);
                if (registerIndex != null) {
                    if (RegisterSizes[registerIndex.Value] <= 2 || LastToken.IsReservedWord(Keyword.X) || LastToken.IsReservedWord(Keyword.Y)) {
                        NextToken();
                        WriteByte(0x28 | registerIndex.Value);
                        return;
                    }
                    ShowInvalidRegister(LastToken);
                }
            }
            ShowSyntaxError(LastToken);
        }

        private void POPU()
        {
            if (LastToken.IsReservedWord(Keyword.F)) {
                NextToken();
                WriteByte(0x3e);
                return;
            }
            if (LastToken.IsReservedWord(Keyword.IMR)) {
                NextToken();
                WriteByte(0x3f);
                return;
            }
            {
                var registerIndex = RegisterIndex(LastToken);
                if (registerIndex != null) {
                    if (RegisterSizes[registerIndex.Value] <= 2 || LastToken.IsReservedWord(Keyword.X) || LastToken.IsReservedWord(Keyword.Y)) {
                        NextToken();
                        WriteByte(0x38 | registerIndex.Value);
                        return;
                    }
                    ShowInvalidRegister(LastToken);
                }
            }
            ShowSyntaxError(LastToken);
        }

        private void ADDorSUB(int code)
        {
            if (LastToken.IsReservedWord(Keyword.A)) {
                NextToken();
                AcceptReservedWord(',');
                {
                    var rightToken = LastToken;
                    var preByte = ParsePreByte(PreBytePurpose.First | PreBytePurpose.Second);
                    if (preByte != null) {
                        WriteByte(code | 0x02);
                        WriteByte(rightToken, preByte.Offset);
                        return;
                    }
                }
                {
                    var rightRegister = RegisterIndex(LastToken);
                    if (rightRegister != null) {
                        NextToken();
                        WriteByte(code | 0x06);
                        WriteByte(rightRegister.Value);
                        return;
                    }
                }
                {
                    var rightToken = LastToken;
                    var value = Expression();
                    if (value != null) {
                        WriteByte(code);
                        WriteByte(rightToken, value);
                        return;
                    }
                }
            }
            {
                var leftRegister = RegisterIndex(LastToken);
                if (leftRegister != null) {
                    NextToken();
                    AcceptReservedWord(',');
                    var rightRegister = RegisterIndex(LastToken);
                    if (rightRegister != null) {
                        NextToken();
                        switch (RegisterSizes[leftRegister.Value]) {
                            case 1:
                                code |= 0x06;
                                break;
                            case 2:
                                code |= 0x04;
                                break;
                            case 3:
                                code |= 0x05;
                                break;
                        }
                        WriteByte(code);
                        WriteByte((leftRegister.Value << 4) | rightRegister.Value);
                        return;
                    }
                }
            }
            {
                var leftToken = LastToken;
                var preByte = ParsePreByte(PreBytePurpose.First);
                if (preByte != null) {
                    AcceptReservedWord(',');
                    if (LastToken.IsReservedWord(Keyword.A)) {
                        NextToken();
                        if (preByte.Code != 0) {
                            WriteByte(preByte.Code);
                        }

                        WriteByte(code | 0x03);
                        WriteByte(leftToken, preByte.Offset);
                        return;
                    }
                    {
                        var rightToken = LastToken;
                        var value = Expression();
                        if (value != null) {
                            if (preByte.Code != 0) {
                                WriteByte(preByte.Code);
                            }

                            WriteByte(code | 0x01);
                            WriteByte(leftToken, preByte.Offset);
                            WriteByte(rightToken, value);
                            return;
                        }
                    }
                }
            }
            ShowSyntaxError(LastToken);
        }

        private void ADCorSBC(int code)
        {
            if (LastToken.IsReservedWord(Keyword.A)) {
                NextToken();
                AcceptReservedWord(',');
                {
                    var rightToken = LastToken;
                    var preByte = ParsePreByte(PreBytePurpose.First | PreBytePurpose.Second);
                    if (preByte != null) {
                        WriteByte(code | 0x02);
                        WriteByte(rightToken, preByte.Offset);
                        return;
                    }
                }
                {
                    var rightToken = LastToken;
                    var value = Expression();
                    if (value != null) {
                        WriteByte(code);
                        WriteByte(rightToken, value);
                        return;
                    }
                }
            }
            {
                var leftToken = LastToken;
                var preByte = ParsePreByte(PreBytePurpose.First);
                if (preByte != null) {
                    AcceptReservedWord(',');
                    if (LastToken.IsReservedWord(Keyword.A)) {
                        NextToken();
                        if (preByte.Code != 0) {
                            WriteByte(preByte.Code);
                        }

                        WriteByte(code | 0x03);
                        WriteByte(leftToken, preByte.Offset);
                        return;
                    }

                    {
                        var rightToken = LastToken;
                        var value = Expression();
                        if (value != null) {
                            if (preByte.Code != 0) {
                                WriteByte(preByte.Code);
                            }

                            WriteByte(code | 0x01);
                            WriteByte(leftToken, preByte.Offset);
                            WriteByte(rightToken, value);
                            return;
                        }
                    }
                }
            }
            ShowSyntaxError(LastToken);
        }

        private void ADCLorSBCL(int code)
        {
            var leftToken = LastToken;
            var preByte1 = ParsePreByte(PreBytePurpose.First | PreBytePurpose.Second);
            if (preByte1 != null) {
                AcceptReservedWord(',');
                if (LastToken.IsReservedWord(Keyword.A)) {
                    NextToken();
                    if (preByte1.Code != 0) {
                        WriteByte(preByte1.Code);
                    }

                    WriteByte(0x01 | code);
                    WriteByte(leftToken, preByte1.Offset);
                    return;
                }
                var rightToken = LastToken;
                PreBytePurpose nextPurpose = preByte1.Purpose switch
                {
                    PreBytePurpose.First => PreBytePurpose.Second,
                    PreBytePurpose.Second => 0,
                    _ => PreBytePurpose.First | PreBytePurpose.Second
                };
                var preByte2 = ParsePreByte(nextPurpose);
                if (preByte2 != null) {
                    var preByte = preByte1.Code | preByte2.Code;
                    if (preByte != 0) {
                        WriteByte(preByte);
                    }

                    WriteByte(0x00 | code);
                    WriteByte(leftToken, preByte1.Offset);
                    WriteByte(rightToken, preByte2.Offset);
                    return;
                }
            }
            ShowSyntaxError(LastToken);
        }

        private void INCorDEC(int code)
        {
            {
                var register = RegisterIndex(LastToken);
                if (register != null) {
                    NextToken();
                    WriteByte(0x00 | code);
                    WriteByte(register.Value);
                    return;
                }
            }
            {
                var token = LastToken;
                var preByte = ParsePreByte(PreBytePurpose.First | PreBytePurpose.Second);
                if (preByte != null) {
                    if (preByte.Code != 0) {
                        WriteByte(preByte.Code);
                    }
                    WriteByte(0x01 | code);
                    WriteByte(token, preByte.Offset);
                    return;
                }
            }
            ShowSyntaxError(LastToken);
        }

        private void PMDF()
        {
            var leftToken = LastToken;
            var preByte = ParsePreByte(PreBytePurpose.First | PreBytePurpose.Second);
            if (preByte != null) {
                AcceptReservedWord(',');
                if (LastToken.IsReservedWord(Keyword.A)) {
                    NextToken();
                    if (preByte.Code != 0) {
                        WriteByte(preByte.Code);
                    }
                    WriteByte(0x57);
                    WriteByte(leftToken, preByte.Offset);
                    return;
                }
                {
                    var rightToken = LastToken;
                    var value = Expression();
                    if (value != null) {
                        if (preByte.Code != 0) {
                            WriteByte(preByte.Code);
                        }
                        WriteByte(0x47);
                        WriteByte(leftToken, preByte.Offset);
                        WriteByte(rightToken, value);
                        return;
                    }
                }
            }
            ShowSyntaxError(LastToken);
        }

        private void CMP()
        {
            if (LastToken.IsReservedWord(Keyword.A)) {
                NextToken();
                AcceptReservedWord(',');
                var rightToken = LastToken;
                var value = Expression();
                if (value != null) {
                    WriteByte(0x60);
                    WriteByte(rightToken, value);
                    return;
                }
                ShowSyntaxError(rightToken);
            }
            if (LastToken.IsReservedWord('[')) {
                NextToken();
                var leftToken = LastToken;
                var address = Expression();
                if (address != null) {
                    AcceptReservedWord(']');
                    AcceptReservedWord(',');
                    var rightToken = LastToken;
                    var value = Expression();
                    if (value != null) {
                        WriteByte(0x62);
                        WritePointer(leftToken, address);
                        WriteByte(rightToken, value);
                        return;
                    }
                }
            }
            {
                var leftToken = LastToken;
                var preByte1 = ParsePreByte(PreBytePurpose.First | PreBytePurpose.Second);
                if (preByte1 != null) {
                    AcceptReservedWord(',');
                    if (LastToken.IsReservedWord(Keyword.A)) {
                        NextToken();
                        if (preByte1.Code != 0) {
                            WriteByte(preByte1.Code);
                        }
                        WriteByte(0x63);
                        WriteByte(leftToken, preByte1.Offset);
                        return;
                    }
                    {
                        var rightToken = LastToken;
                        var nextPurpose = preByte1.Purpose.HasFlag(PreBytePurpose.Second) ? 0 : PreBytePurpose.Second;
                        var preByte2 = ParsePreByte(nextPurpose);
                        if (preByte2 != null) {
                            var preByte = preByte1.Code | preByte2.Code;
                            if (preByte != 0) {
                                WriteByte(preByte);
                            }
                            WriteByte(0xb7);
                            WriteByte(leftToken, preByte1.Offset);
                            WriteByte(rightToken, preByte2.Offset);
                            return;
                        }
                    }
                    {
                        var rightToken = LastToken;
                        var value = Expression();
                        if (value != null) {
                            if (preByte1.Code != 0) {
                                WriteByte(preByte1.Code);
                            }
                            WriteByte(0x61);
                            WriteByte(leftToken, preByte1.Offset);
                            WriteByte(rightToken, value);
                            return;
                        }
                    }
                }
            }
            ShowSyntaxError(LastToken);
        }

        private void CMP(int code, int size)
        {
            var leftToken = LastToken;
            var preByte1 = ParsePreByte(PreBytePurpose.First | PreBytePurpose.Second);
            if (preByte1 != null) {
                AcceptReservedWord(',');
                if (RegisterIndex(LastToken) is { } rightRegister) {
                    var rightToken = LastToken;
                    NextToken();
                    if (RegisterSizes[rightRegister] == size) {
                        if (preByte1.Code != 0) {
                            WriteByte(preByte1.Code);
                        }
                        WriteByte(0xd0 | code);
                        WriteByte(rightRegister);
                        WriteByte(leftToken, preByte1.Offset);
                        return;
                    }
                    ShowInvalidRegister(rightToken);
                    return;
                }
                {
                    var rightToken = LastToken;
                    var nextPurpose = preByte1.Purpose.HasFlag(PreBytePurpose.Second) ? 0 : PreBytePurpose.Second;
                    var preByte2 = ParsePreByte(nextPurpose);
                    if (preByte2 != null) {
                        var preByte = preByte1.Code | preByte2.Code;
                        if (preByte != 0) {
                            WriteByte(preByte);
                        }
                        WriteByte(0xc0 | code);
                        WriteByte(leftToken, preByte1.Offset);
                        WriteByte(rightToken, preByte2.Offset);
                        return;
                    }
                }
            }
            ShowSyntaxError(LastToken);
        }

        private void BitOp(int code)
        {
            if (LastToken.IsReservedWord(Keyword.A)) {
                NextToken();
                AcceptReservedWord(',');
                {
                    var rightToken = LastToken;
                    var preByte = ParsePreByte(PreBytePurpose.First | PreBytePurpose.Second);
                    if (preByte != null) {
                        if (preByte.Code != 0) {
                            WriteByte(preByte.Code);
                        }
                        WriteByte(code | 0x07);
                        WriteByte(rightToken, preByte.Offset);
                        return;
                    }
                    {
                        var value = Expression();
                        if (value != null) {
                            WriteByte(code | 0x00);
                            WriteByte(rightToken, value);
                            return;
                        }
                    }
                }
            }
            if (LastToken.IsReservedWord('[')) {
                NextToken();
                var leftToken = LastToken;
                var address = Expression();
                if (address != null) {
                    AcceptReservedWord(']');
                    AcceptReservedWord(',');
                    var rightToken = LastToken;
                    var value = Expression();
                    if (value != null) {
                        WriteByte(code | 0x02);
                        WritePointer(leftToken, address);
                        WriteByte(rightToken, value);
                        return;
                    }
                }
            }
            {
                var leftToken = LastToken;
                var preByte1 = ParsePreByte(PreBytePurpose.First | PreBytePurpose.Second);
                if (preByte1 != null) {
                    AcceptReservedWord(',');
                    if (LastToken.IsReservedWord(Keyword.A)) {
                        NextToken();
                        if (preByte1.Code != 0) {
                            WriteByte(preByte1.Code);
                        }
                        WriteByte(code | 0x03);
                        WriteByte(leftToken, preByte1.Offset);
                        return;
                    }
                    {
                        var rightToken = LastToken;
                        var nextPurpose = preByte1.Purpose.HasFlag(PreBytePurpose.Second) ? 0 : PreBytePurpose.Second;
                        var preByte2 = ParsePreByte(nextPurpose);
                        if (preByte2 != null) {
                            var preByte = preByte1.Code | preByte2.Code;
                            if (preByte != 0) {
                                WriteByte(preByte);
                            }
                            WriteByte(code | 0x06);
                            WriteByte(leftToken, preByte1.Offset);
                            WriteByte(rightToken, preByte2.Offset);
                            return;
                        }
                    }
                    {
                        var rightToken = LastToken;
                        var value = Expression();
                        if (value != null) {
                            if (preByte1.Code != 0) {
                                WriteByte(preByte1.Code);
                            }
                            WriteByte(code | 0x01);
                            WriteByte(leftToken, preByte1.Offset);
                            WriteByte(rightToken, value);
                            return;
                        }
                    }
                }
            }
            ShowSyntaxError(LastToken);
        }

        private void TEST()
        {
            if (LastToken.IsReservedWord(Keyword.A)) {
                NextToken();
                AcceptReservedWord(',');
                {
                    var rightToken = LastToken;
                    var value = Expression();
                    if (value != null) {
                        WriteByte(0x64);
                        WriteByte(rightToken, value);
                        return;
                    }
                }
            }
            if (LastToken.IsReservedWord('[')) {
                NextToken();
                var leftToken = LastToken;
                var address = Expression();
                if (address != null) {
                    AcceptReservedWord(']');
                    AcceptReservedWord(',');
                    var rightToken = LastToken;
                    var value = Expression();
                    if (value != null) {
                        WriteByte(0x65);
                        WritePointer(leftToken, address);
                        WriteByte(rightToken, value);
                        return;
                    }
                }
            }
            {
                var leftToken = LastToken;
                var preByte1 = ParsePreByte(PreBytePurpose.First | PreBytePurpose.Second);
                if (preByte1 != null) {
                    AcceptReservedWord(',');
                    if (LastToken.IsReservedWord(Keyword.A)) {
                        NextToken();
                        if (preByte1.Code != 0) {
                            WriteByte(preByte1.Code);
                        }
                        WriteByte(0x67);
                        WriteByte(leftToken, preByte1.Offset);
                        return;
                    }
                    {
                        var rightToken = LastToken;
                        var value = Expression();
                        if (value != null) {
                            if (preByte1.Code != 0) {
                                WriteByte(preByte1.Code);
                            }
                            WriteByte(0x65);
                            WriteByte(leftToken, preByte1.Offset);
                            WriteByte(rightToken, value);
                            return;
                        }
                    }
                }
            }
            ShowSyntaxError(LastToken);
        }
        private void Shift(int code)
        {
            if (LastToken.IsReservedWord(Keyword.A)) {
                NextToken();
                WriteByte(code | 0x00);
                return;
            }
            {
                var token = LastToken;
                var preByte = ParsePreByte(PreBytePurpose.First | PreBytePurpose.Second);
                if (preByte != null) {
                    if (preByte.Code != 0) {
                        WriteByte(preByte.Code);
                    }
                    WriteByte(code | 0x01);
                    WriteByte(token, preByte.Offset);
                    return;
                }
            }
            ShowSyntaxError(LastToken);
        }

        private void DSL(int code)
        {
            var token = LastToken;
            var preByte = ParsePreByte(PreBytePurpose.First | PreBytePurpose.Second);
            if (preByte != null) {
                if (preByte.Code != 0) {
                    WriteByte(preByte.Code);
                }
                WriteByte(code);
                WriteByte(token, preByte.Offset);
                return;
            }
            ShowSyntaxError(LastToken);
        }

        private void JR(int shortCode, int longCode)
        {
            var token = LastToken;
            if (RelativeOffset(out var address, out int offset)) {
                if (offset < 0) {
                    WriteByte(shortCode | 0x01);
                    WriteByte(-offset);
                }
                else {
                    WriteByte(shortCode);
                    WriteByte(offset);
                }
                return;
            }
            WriteByte(longCode);
            WriteWord(token, address);
        }

        private void JP()
        {
            {
                var token = LastToken;
                var preByte = ParsePreByte(PreBytePurpose.First | PreBytePurpose.Second);
                if (preByte != null) {
                    if (preByte.Code != 0) {
                        WriteByte(preByte.Code);
                    }
                    WriteByte(0x10);
                    WriteByte(token, preByte.Offset);
                    return;
                }
            }
            {
                var token = LastToken;
                var register = RegisterIndex(token);
                if (register != null) {
                    NextToken();
                    if (RegisterSizes[register.Value] == 3) {
                        WriteByte(0x11);
                        WriteByte(register.Value);
                        return;
                    }
                    ShowInvalidRegister(token);
                    return;
                }
            }
            JP(0x02);
        }

        private void JP(int code)
        {
            var token = LastToken;
            var address = Expression();
            if (address != null) {
                WriteByte(code);
                WriteWord(token, address.PartOf(AddressPart.Word));
                return;
            }
            ShowSyntaxError(LastToken);
        }

        private void JPF(int code)
        {
            var token = LastToken;
            var address = Expression();
            if (address != null) {
                WriteByte(code);
                WritePointer(token, address);
                return;
            }
            ShowSyntaxError(LastToken);
        }

        private void SWAP()
        {
            AcceptReservedWord(Keyword.A);
            WriteByte(0xee);
        }


        private static int? JumpCode(Token token)
        {
            if (token.IsReservedWord(Keyword.Z)) return 0x14;
            if (token.IsReservedWord(Keyword.NZ)) return 0x15;
            if (token.IsReservedWord(Keyword.C)) return 0x14;
            if (token.IsReservedWord(Keyword.NC)) return 0x15;
            return null;
        }

        private static int? RelativeJumpCode(Token token)
        {
            if (token.IsReservedWord(Keyword.Z)) return 0x18;
            if (token.IsReservedWord(Keyword.NZ)) return 0x1a;
            if (token.IsReservedWord(Keyword.C)) return 0x1c;
            if (token.IsReservedWord(Keyword.NC)) return 0x1e;
            return null;
        }

        private void ConditionalJump(Address address)
        {
            var condition = LastToken;
            {
                var code = RelativeJumpCode(LastToken);
                if (code != null) {
                    if (!address.IsUndefined()) {
                        var offset = RelativeOffset(address);
                        if (IsRelativeOffsetInRange(offset)) {
                            NextToken();
                            // JR cc, else
                            if (offset < 0) {
                                WriteByte(code.Value | 0x01);
                                WriteByte(-offset);
                            }
                            else {
                                WriteByte(code.Value);
                                WriteByte(offset);
                            }
                            return;
                        }
                    }
                }
            }
            {
                var code = JumpCode(condition);
                if (code == null)
                    return;
                NextToken();
                // JP cc,else
                WriteByte(code.Value);
                WriteWord(LastToken, address);
            }
        }

        private void NegatedConditionalJump(Address address)
        {
            var condition = LastToken;
            {
                int? code;
                if (!address.IsUndefined() && (code = RelativeJumpCode(condition)) != null) {
                    code ^= 0x02;  // negate condition
                    var offset = RelativeOffset(address);
                    if (IsRelativeOffsetInRange(offset)) {
                        NextToken();
                        // JR !cc, else
                        if (offset < 0) {
                            WriteByte(code.Value | 0x01);
                            WriteByte(-offset);
                        }
                        else {
                            WriteByte(code.Value);
                            WriteByte(offset);
                        }
                        return;
                    }
                }
            }
            {
                var code = JumpCode(condition);
                if (code == null)
                    return;
                NextToken();
                code ^= 0x01; // negate condition
                // JP !cc,else
                WriteByte(code.Value);
                WriteWord(LastToken, address);
            }
        }

        private void UnconditionalJump(Address address)
        {
            if (!address.IsUndefined()) {
                var offset = RelativeOffset(address);
                if (IsRelativeOffsetInRange(offset)) {
                    // JR endif
                    if (offset < 0) {
                        WriteByte(0x13);
                        WriteByte(-offset);
                    }
                    else {
                        WriteByte(0x12);
                        WriteByte(offset);
                    }
                    return;
                }
            }
            // JP endif
            WriteByte(0x02);
            WriteWord(LastToken, address);
        }


        private void StartIf(IfBlock block)
        {
            var address = SymbolAddress(block.ElseId);
            NegatedConditionalJump(address);
        }

        private void IfStatement()
        {
            var block = NewIfBlock();
            StartIf(block);
        }

        private void ElseStatement()
        {
            if (LastBlock() is IfBlock block) {
                if (block.ElseId <= 0) {
                    ShowError(LastToken.Position, "Multiple ELSE statement.");
                }

                var address = SymbolAddress(block.EndId);
                UnconditionalJump(address);
                DefineSymbol(block.ConsumeElse(), CurrentAddress);
            }
            else {
                ShowNoStatementError(LastToken, "IF");
            }
            //NextToken();
        }

        private void ElseIfStatement()
        {
            ElseStatement();
            if (LastBlock() is not IfBlock block) { return; }
            Debug.Assert(block.ElseId == Block.InvalidId);
            block.ElseId = AutoLabel();
            StartIf(block);
        }
        private void EndIfStatement()
        {
            if (LastBlock() is IfBlock block) {
                switch (block.ElseId) {
                    case <= 0:
                        DefineSymbol(block.EndId, CurrentAddress);
                        break;
                    default:
                        DefineSymbol(block.ConsumeElse(), CurrentAddress);
                        break;
                }
                EndBlock();
            }
            else {
                ShowNoStatementError(LastToken, "IF");
            }
            //NextToken();
        }

        private void DoStatement()
        {
            var block = NewWhileBlock();
            DefineSymbol(block.BeginId, CurrentAddress);
            //NextToken();
        }

        private void WhileStatement()
        {
            //NextToken();
            if (LastBlock() is not WhileBlock block) {
                ShowNoStatementError(LastToken, "WHILE");
                NextToken();
                return;
            }

            var repeatAddress = SymbolAddress(block.RepeatId);
            var code = RelativeJumpCode(LastToken);
            var next = code != null ? 0 : 1;
            if (repeatAddress.Type == CurrentSegment.Type && (RelativeOffset(repeatAddress)) <= next) {
                var address = SymbolAddress(block.BeginId);
                ConditionalJump(address);
                block.EraseEndId();
            }
            else {
                var address = SymbolAddress(block.EndId);
                NegatedConditionalJump(address);
            }
        }
        private void WEndStatement()
        {
            if (LastBlock() is not WhileBlock block) {
                ShowNoStatementError(LastToken, "WHILE");
            }
            else {
                if (block.EndId > 0) {
                    DefineSymbol(block.RepeatId, CurrentAddress);
                    var address = SymbolAddress(block.BeginId);
                    UnconditionalJump(address);
                    DefineSymbol(block.EndId, CurrentAddress);
                }
                EndBlock();
            }
            //NextToken();
        }


        private static readonly IDictionary<int, Action<Assembler>> Actions = new Dictionary<int, Action<Assembler>>
        {
            { Keyword.MV, a=>a.MV() },
            { Keyword.MVW, a=>a.MVW() },
            { Keyword.MVP, a=>a.MVP() },
            { Keyword.MVL, a=>a.MVL() },
            { Keyword.MVLD, a=>a.InterInternal(0xcf) },
            { Keyword.EX, a=>a.EX() },
            { Keyword.EXW, a=>a.InterInternal(0xc1) },
            { Keyword.EXP, a=>a.InterInternal(0xc2) },
            { Keyword.EXL, a=>a.InterInternal(0xc3) },
            { Keyword.PUSHS, a=>a.PUSHS() },
            { Keyword.POPS, a=>a.POPS() },
            { Keyword.PUSHU, a=>a.PUSHU() },
            { Keyword.POPU, a=>a.POPU() },
            { Keyword.ADD, a=>a.ADDorSUB(0x40) },
            { Keyword.SUB, a=>a.ADDorSUB(0x48) },
            { Keyword.ADC, a=>a.ADCorSBC(0x50) },
            { Keyword.SBC, a=>a.ADCorSBC(0x58) },
            { Keyword.ADCL, a=>a.ADCLorSBCL(0x54) },
            { Keyword.SBCL, a=>a.ADCLorSBCL(0x5c) },
            { Keyword.DADL, a=>a.ADCLorSBCL(0xc4) },
            { Keyword.DSBL, a=>a.ADCLorSBCL(0xd4) },
            { Keyword.INC, a=>a.INCorDEC(0x6c) },
            { Keyword.DEC, a=>a.INCorDEC(0x7c) },
            { Keyword.PMDF, a=>a.PMDF() },
            { Keyword.CMP, a=>a.CMP() },
            { Keyword.CMPW, a=>a.CMP(0x06, 2) },
            { Keyword.CMPP, a=>a.CMP(0x07, 3) },
            { Inu.Assembler.Keyword.And, a=>a.BitOp(0x70) },
            { Inu.Assembler.Keyword.Or, a=>a.BitOp(0x78) },
            { Inu.Assembler.Keyword.Xor, a=>a.BitOp(0x68) },
            { Keyword.TEST, a =>a.TEST() },
            { Keyword.ROR, a=>a.Shift(0xe4) },
            { Keyword.ROL, a=>a.Shift(0xe6) },
            { Inu.Assembler.Keyword.Shr, a=>a.Shift(0xf4) },
            { Inu.Assembler.Keyword.Shl, a=>a.Shift(0xf6) },
            { Keyword.DSRL, a=>a.DSL(0xfc) },
            { Keyword.DSLL, a=>a.DSL(0xec) },
            { Keyword.JR, a =>a.JR(0x12, 0x02) },
            { Keyword.JRZ, a =>a.JR(0x18, 0x14) },
            { Keyword.JRNZ, a =>a.JR(0x1a, 0x15) },
            { Keyword.JRC, a =>a.JR(0x1c, 0x16) },
            { Keyword.JRNC, a =>a.JR(0x1e, 0x17) },
            { Keyword.JP, a =>a.JP() },
            { Keyword.JPZ, a =>a.JP(0x14) },
            { Keyword.JPNZ, a =>a.JP(0x15) },
            { Keyword.JPC, a =>a.JP(0x16) },
            { Keyword.JPNC, a =>a.JP(0x17) },
            { Keyword.JPF, a =>a.JPF(0x03) },
            { Keyword.CALL, a =>a.JP(0x04) },
            { Keyword.CALLF, a =>a.JPF(0x05) },
            { Keyword.RET, a =>a.WriteByte(0x06) },
            { Keyword.RETF, a =>a.WriteByte(0x07) },
            { Keyword.SWAP, a =>a.SWAP() },
            { Keyword.SC, a =>a.WriteByte(0x97) },
            { Keyword.RC, a =>a.WriteByte(0x9f) },
            { Keyword.NOP, a =>a.WriteByte(0x00) },
            { Keyword.WAIT, a =>a.WriteByte(0xef) },
            { Keyword.HALT, a =>a.WriteByte(0xde) },
            { Keyword.OFF, a =>a.WriteByte(0xdf) },
            { Keyword.IR, a =>a.WriteByte(0xfe) },
            { Keyword.RETI, a =>a.WriteByte(0x01) },
            { Keyword.RESET, a =>a.WriteByte(0xff) },
            { Inu.Assembler.Keyword.If, a=>{a.IfStatement(); }},
            { Inu.Assembler.Keyword.Else, a=>{a.ElseStatement(); }},
            { Inu.Assembler.Keyword.EndIf,a=>{a.EndIfStatement(); }},
            { Inu.Assembler.Keyword.ElseIf, a=>{a.ElseIfStatement(); }},
            { Inu.Assembler.Keyword.Do, a=>{a.DoStatement(); }},
            { Inu.Assembler.Keyword.While, a=>{a.WhileStatement(); }},
            { Inu.Assembler.Keyword.WEnd, a=>{a.WEndStatement(); }},
        };



        protected override bool Instruction()
        {
            var reservedWord = LastToken as ReservedWord;
            Debug.Assert(reservedWord != null);
            if (!Actions.TryGetValue(reservedWord.Id, out var func)) return false;
            NextToken();
            func(this);
            return true;
        }

        protected override Dictionary<int, Func<bool>> StorageDirectives
        {
            get
            {
                var storageDirectives = base.StorageDirectives;
                storageDirectives[Keyword.DEFP] = PointerStorageOperand;
                storageDirectives[Keyword.DP] = PointerStorageOperand;
                return storageDirectives;
            }
        }

        private bool PointerStorageOperand()
        {
            var token = LastToken;
            var value = Expression();
            if (value == null) { return false; }
            WritePointer(token, value);
            return true;
        }
    }
}

