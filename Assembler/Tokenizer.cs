using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using Inu.Language;

namespace Inu.Assembler
{
    public abstract class Tokenizer : Language.Tokenizer
    {
        private const char Comment = ';';
        private const char HexValueHead = '$';
        private const char HexValueTail = 'H';

        protected Tokenizer()
        {
            ReservedWord.AddWords(Keyword.Words);
        }

        protected override bool IsSpace(char c)
        {
            return c != SourceReader.EndOfLine && base.IsSpace(c);
        }
        protected override bool IsQuotation(char c)
        {
            return "\'\"".Contains(c);
        }
        protected override bool IsIdentifierHead(char c)
        {
            return base.IsIdentifierHead(c) || "$.?@".Contains(c);
        }
        protected override bool IsIdentifierElement(char c)
        {
            return base.IsIdentifierElement(c) || c == '\'';
        }

        protected override void SkipSpaces()
        {
        repeat:
            base.SkipSpaces();
            if (LastChar == Comment) {
                Debug.Assert(SourceReader.Current != null);
                SourceReader.Current.SkipToEndOfLine();
                NextChar();
                goto repeat;
            }
        }

        protected sealed override bool IsNumericValueHead(char c)
        {
            return IsHexValueHead(c) || base.IsNumericValueHead(c);
        }

        protected virtual bool IsHexValueHead(char c)
        {
            return c == HexValueHead;
        }

        protected sealed override int ReadNumericValue()
        {
            if (!IsHexValueHead(LastChar)) return ReadHexValue(out var value) ? value : ReadDecValue();
            NextChar();
            var chars = new StringBuilder();

            var upper = char.ToUpper(LastChar);
            while (IsHexDigit(upper)) {
                chars.Append(upper);
                NextChar();
                upper = char.ToUpper(LastChar);
            }
            return chars.Length > 0 ? int.Parse(chars.ToString(), NumberStyles.AllowHexSpecifier) : 0;
        }

        private bool ReadHexValue(out int value)
        {
            value = 0;
            string s = ReadWord(IsHexDigit);
            {
                var c = char.ToUpper(LastChar);
                if (c == HexValueTail) {
                    NextChar();
                    value = int.Parse(s, NumberStyles.AllowHexSpecifier);
                    return true;
                }
            }
            ReturnChars(s);
            return false;
        }
    }
}
