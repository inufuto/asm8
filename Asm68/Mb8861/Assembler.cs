using System.Collections.Generic;
using Inu.Language;

namespace Inu.Assembler.Mb8861
{
    internal class Assembler : Mc6800.Assembler
    {
        public Assembler() : base(new Tokenizer()) { }

        protected override bool Instruction()
        {
            if (MemoryOperation()) return true;
            if (IndexRegisterOperation()) return true;
            return base.Instruction();
        }

        private bool IndexRegisterOperation()
        {
            if (!LastToken.IsReservedWord(Keyword.ADX)) return false;
            NextToken();
            if (LastToken.IsReservedWord('#')) {
                NextToken();
                var valueToken = LastToken;
                var value = Expression();
                if (value == null) {
                    ShowSyntaxError(valueToken);
                    return true;
                }
                WriteByte(0xec);
                WriteByte(valueToken, value);
                return true;
            }
            else {
                var token = LastToken;
                var value = Expression();
                if (value == null) {
                    ShowSyntaxError(token);
                    return true;
                }

                WriteByte(0xfc);
                WriteWord(LastToken, value);
                return true;
            }
        }

        private static readonly Dictionary<int, int> OperationCodes = new Dictionary<int, int>()
        {
            {Keyword.NIM, 0x71},
            {Keyword.OIM, 0x72},
            {Keyword.XIM, 0x75},
            {Keyword.TMM, 0x7b},
        };

        private bool MemoryOperation()
        {
            if (!(LastToken is ReservedWord reservedWord) ||
                !OperationCodes.TryGetValue(reservedWord.Id, out var instruction)) return false;
            NextToken();
            AcceptReservedWord('#');
            var valueToken = LastToken;
            var value = Expression();
            if (value == null) {
                ShowSyntaxError(valueToken);
                return true;
            }
            AcceptReservedWord(',');
            var offsetToken = LastToken;
            if (offsetToken.IsReservedWord('<') || offsetToken.IsReservedWord('>')) {
                //ignore
                offsetToken = NextToken();
            }
            var offset = Expression();
            if (offset == null) {
                ShowSyntaxError(offsetToken);
                return true;
            }
            AcceptReservedWord(',');
            AcceptReservedWord(Mc6800.Keyword.X);

            WriteByte(instruction);
            WriteByte(valueToken, value);
            WriteByte(offsetToken, offset);
            return true;
        }
    }
}
