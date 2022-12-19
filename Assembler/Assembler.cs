using Inu.Language;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Inu.Assembler
{
    public abstract class Assembler : TokenReader
    {
        protected const int MaxErrorCount = 100;
        protected const char EndOfStatement = '|';
        protected const int AutoLabelMinId = 0x8000;

        public Segment CurrentSegment { get; private set; }
        public int Pass { get; private set; }

        private readonly Object @object = new Object();
        private bool addressChanged;
        private ListFile? listFile;
        private int nextAutoLabelId;
        private readonly Stack<Block> blocks = new Stack<Block>();

        protected Assembler(Language.Tokenizer tokenizer) : base(tokenizer)
        {
            CurrentSegment = @object.Segments[0];
        }

        public virtual bool ZeroPageAvailable => false;

        public bool DefineSymbol(int id, Address address)
        {
            var symbol = FindSymbol(id);
            if (symbol == null) {
                string name;
                if (Identifier.IsIdentifierId(id)) {
                    name = Identifier.FromId(id);
                }
                else {
                    name = "@" + id;
                }
                @object.Symbols[id] = new Symbol(Pass, id, name, address);
                return true;
            }
            if (symbol.Address == address) { return true; }
            if (symbol.Pass == Pass) {
                // duplicate
                return false;
            }

            addressChanged = true;
            symbol.Address = address;
            return true;
        }

        protected void DefineSymbol(Identifier identifier, Address address)
        {
            if (!DefineSymbol(identifier.Id, address)) {
                ShowError(identifier.Position, "Multiple definition: " + identifier.ToString());
            }
        }

        protected Symbol? FindSymbol(int id)
        {
            return @object.Symbols.TryGetValue(id, out var symbol) ? symbol : null;
        }

        protected Symbol? FindSymbol(Identifier identifier)
        {
            var symbol = FindSymbol(identifier.Id);
            if (symbol != null) {
                if (Pass > 1 && symbol.Address.IsUndefined()) {
                    ShowUndefinedError(identifier);
                }
                return symbol;
            }
            if (Pass > 1) {
                ShowUndefinedError(identifier);
            }
            return null;
        }

        protected Address SymbolAddress(int id)
        {
            var symbol = FindSymbol(id);
            return symbol != null ? symbol.Address : new Address(AddressType.Undefined, 0);
        }

        protected Address SymbolAddress(Identifier identifier)
        {
            Address address = SymbolAddress(identifier.Id);
            if (Pass > 1 && address.IsUndefined()) {
                ShowUndefinedError(identifier);
            }
            return address;
        }

        protected virtual void WriteByte(int value)
        {
            CurrentSegment.WriteByte(value);
            Debug.Assert(listFile != null);
            listFile.AddByte(value);
        }

        protected void WriteByte(Token token, Address value)
        {
            if (value.IsRelocatable() || value.Type == AddressType.External) {
                if (value.Part == AddressPart.Word) {
                    value = value.Low() ?? value;
                }
                @object.AddressUsages[CurrentAddress] = value;
            }
            else if (!value.IsConst()) {
                ShowAddressUsageError(token);
            }
            WriteByte(value.Value);
        }

        protected abstract byte[] ToBytes(int value);

        protected void WriteWord(Token token, Address value)
        {
            if (value.IsRelocatable() || value.Type == AddressType.External) {
                Debug.Assert(value.Part == AddressPart.Word);
                @object.AddressUsages[CurrentAddress] = value;
            }
            else if (!value.IsConst()) {
                ShowAddressUsageError(token);
            }

            byte[] bytes = ToBytes(value.Value);
            foreach (var b in bytes) {
                WriteByte(b);
            }
        }

        protected void WriteSpace(int value)
        {
            for (int i = 0; i < value; ++i) {
                CurrentSegment.WriteByte(0);
            }
        }




        protected virtual Token SkipEndOfStatement()
        {
            Debug.Assert(listFile != null);
            bool newLine = false;
            Debug.Assert(LastToken != null);
            if (LastToken.IsReservedWord(SourceReader.EndOfLine)) {
                listFile.PrintLine();
                newLine = true;
            }
            while (LastToken.IsReservedWord(SourceReader.EndOfLine) || LastToken.IsReservedWord(EndOfStatement)) {
                NextToken();
                if (LastToken.IsReservedWord(SourceReader.EndOfLine)) {
                    listFile.PrintLine();
                    newLine = true;
                }
            }
            if (newLine) {
                listFile.Address = CurrentAddress;
                listFile.IndentLevel = blocks.Count;
            }
            return LastToken;
        }

        protected void ShowAddressUsageError(Token token)
        {
            if (Pass > 1) {
                ShowError(token.Position, "Cannot use relocatable symbols in expressions: " + token.ToString());
            }
        }

        protected void ShowOutOfRange(Token token, int value)
        {
            ShowError(token.Position, "Out of range: " + value);
        }

        protected void ShowNoStatementError(Token token, string statementName)
        {
            ShowError(token.Position, "No " + statementName + " statement: " + token.ToString());
        }

        protected void ShowInvalidRegister(Token token)
        {
            ShowError(token.Position, "Invalid register: " + token.ToString());
        }


        protected virtual Address CurrentAddress => CurrentSegment.Tail;


        private Address? CharConstant()
        {
            Debug.Assert(LastToken != null);
            if (!(LastToken is StringValue stringValue))
                return null;
            var s = stringValue.ToString();
            Debug.Assert(s != null);
            Address value = new Address(s.ToCharArray()[0]);
            NextToken();
            return value;
        }

        private Address? ParenthesisExpression()
        {
            Debug.Assert(LastToken != null);
            if (!LastToken.IsReservedWord('(')) {
                return null;
            }
            NextToken();
            var value = Expression();
            AcceptReservedWord(')');
            if (value == null) {
                ShowSyntaxError();
                return null;
            }
            value.Parenthesized = true;
            return value;
        }

        private readonly Dictionary<int, Func<int, int>> Monomials = new Dictionary<int, Func<int, int>> {
            { '+', (int value) => {return value; }},
            { '-', (int value) =>{return -value; } },
            { Keyword.Not, (int right) =>{return ~right; } },
            { Keyword.Low, (int right) =>{return right & 0xff; } },
            { Keyword.High, (int right) =>{return (right >> 8) & 0xff; } },
        };
        private Address? Monomial()
        {
            var reservedWord = LastToken as ReservedWord;
            Debug.Assert(reservedWord != null);
            if (!Monomials.TryGetValue(reservedWord.Id, out var function)) {
                return null;
            }
            Token rightToken = NextToken();
            var right = Factor();
            if (right == null) {
                ShowSyntaxError();
                return null;
            }
            if (!right.IsConst()) {
                switch (reservedWord.Id) {
                    case Keyword.Low:
                        return right.Low();
                    case Keyword.High:
                        return right.High();
                    default:
                        ShowAddressUsageError(rightToken);
                        break;
                }
            }
            var value = function(right.Value);
            return new Address(value);
        }

        private Address? Factor()
        {
            Debug.Assert(LastToken != null);
            var type = LastToken.Type;
            Address? value;
            if (LastToken is NumericValue numericValue) {
                value = new Address(numericValue.Value);
                NextToken();
                return value;
            }

            if (LastToken is Identifier identifier) {
                value = SymbolAddress(identifier);
                NextToken();
                return value;
            }

            if (LastToken is ReservedWord) {
                value = Monomial();
                if (value != null) { return value; }
            }
            value = CharConstant();
            if (value != null) { return value; }
            value = ParenthesisExpression();
            if (value != null) { return value; }
            return null;
        }

        private readonly Dictionary<int, Func<int, int, int>>[] Binomials = {
            new Dictionary<int, Func<int, int, int>> {
                { Keyword.Or, (int left, int right) =>{ return left | right; } },
                { Keyword.Xor, (int left, int right)=> { return left ^ right; } },
            },
            new Dictionary<int, Func<int, int, int>> {
                { Keyword.And, (int left, int right) =>{ return left & right; } },
            },
            new Dictionary<int, Func<int, int, int>> {
                { Keyword.Shl, (int left, int right)=> { return left << right; } },
                { Keyword.Shr, (int left, int right)=> { return left >> right; } },
            },
            new Dictionary<int, Func<int, int, int>> {
                { '+', (int left, int right) =>{ return left + right; } },
                { '-', (int left, int right) =>{ return left - right; } },
            },
            new Dictionary<int, Func<int, int, int>> {
                { '*', (int left, int right) =>{ return left* right; } },
                { '/', (int left, int right) =>{ return left / right; } },
                { Keyword.Mod, (int left, int right) =>{ return left % right; } },
            },
            new Dictionary<int, Func<int, int, int>> {
            }
        };

        private Address? Binomial(int level)
        {
            Func<Address?> factorFunction;
            if (Binomials[level + 1].Count == 0) {
                factorFunction = Factor;
            }
            else {
                factorFunction = () => Binomial(level + 1);
            }
            Debug.Assert(LastToken != null);
            Token leftToken = LastToken;
            var left = factorFunction();
            if (left == null) {
                //ShowSyntaxError();
                return null;
            }

        repeat:
            {
                if (LastToken is ReservedWord operatorToken) {
                    if (Binomials[level].TryGetValue(operatorToken.Id, out var operation)) {
                        Token rightToken = NextToken();
                        var right = factorFunction();
                        if (right == null) {
                            ShowSyntaxError();
                            return null;
                        }
                        if (Address.IsOperationAvailable(operatorToken.Id, left, right)) {
                            var type = left.Type == right.Type ? AddressType.Const : left.Type;
                            left = new Address(type, operation(left.Value, right.Value), left.Id);
                        }
                        else {
                            ShowAddressUsageError(leftToken);
                        }
                        goto repeat;
                    }
                }
            }
            return left;
        }
        private Address? Binomial() { return Binomial(0); }

        protected Address? Expression()
        {
            return Binomial();
        }

        private void IncludeDirective()
        {
            Token token = NextToken();
            if (token is StringValue stringValue) {
                var fileName = stringValue.ToString();
                try {
                    Debug.Assert(fileName != null);
                    OpenSourceFile(fileName);
                }
                catch (IOException e) {
                    ShowError(token.Position, e.Message);
                }
                NextToken();
            }
            else {
                ShowError(token.Position, "Missing filename.");
            }
        }

        protected void SegmentDirective(AddressType type)
        {
            CurrentSegment = @object.Segments[(int)type];
            Debug.Assert(listFile != null);
            listFile.Address = CurrentAddress;
            NextToken();
        }
        private void PublicDirective()
        {
            Debug.Assert(LastToken != null);
            do {
                NextToken();
                if (LastToken is Identifier identifier) {
                    var symbol = FindSymbol(identifier);
                    if (symbol != null) {
                        symbol.Public = true;
                    }
                    NextToken();
                }
                else {
                    ShowMissingIdentifier(LastToken.Position);
                }
            } while (LastToken.IsReservedWord(','));
        }
        private void ExternDirective(AddressType type)
        {
            Debug.Assert(LastToken != null);
            do {
                Token token = NextToken();
                if (token is Identifier label) {
                    NextToken();
                    DefineSymbol(label, new Address(type, 0, label.Id));
                }
                else {
                    ShowMissingIdentifier(token.Position);
                }
            } while (LastToken.IsReservedWord(','));
        }

        private bool ByteStorageOperand()
        {
            Debug.Assert(LastToken != null);
            Token token = LastToken;
            if (token is StringValue stringValue) {
                var s = stringValue.ToString();
                Debug.Assert(s != null);
                foreach (var c in s) {
                    WriteByte(c);
                }
                NextToken();
                return true;
            }
            var value = Expression();
            if (value == null)
                return false;
            WriteByte(value.Value);
            return true;
        }
        private bool WordStorageOperand()
        {
            Token token = LastToken;
            var value = Expression();
            if (value == null) { return false; }
            WriteWord(token, value);
            return true;
        }
        private bool SpaceStorageOperand()
        {
            Token token = LastToken;
            var value = Expression();
            if (value == null) { return false; }
            if (!value.IsConst()) {
                ShowAddressUsageError(token);
            }
            WriteSpace(value.Value);
            return true;
        }

        private static readonly Dictionary<int, Func<Assembler, bool>> StorageDirectives = new Dictionary<int, Func<Assembler, bool>>
        {
            { Keyword.DefB, (Assembler t)=>t.ByteStorageOperand()},
            { Keyword.DefW, (Assembler t)=>t.WordStorageOperand()},
            { Keyword.DefS, (Assembler t)=>t.SpaceStorageOperand()},
            { Keyword.Db, (Assembler t)=>t.ByteStorageOperand()},
            { Keyword.Dw, (Assembler t)=>t.WordStorageOperand()},
            { Keyword.Ds, (Assembler t)=>t.SpaceStorageOperand()},
        };

        private bool StorageDirective()
        {
            var reservedWord = LastToken as ReservedWord;
            Debug.Assert(reservedWord != null);
            if (!StorageDirectives.TryGetValue(reservedWord.Id, out var function))
                return false;
            do {
                NextToken();
                if (!function(this)) {
                    ShowSyntaxError();
                }
            } while (LastToken.IsReservedWord(','));
            return true;
        }
        private void EquDirective(Identifier label)
        {
            NextToken();
            var value = Expression();
            if (value == null) {
                ShowSyntaxError();
                return;
            }
            DefineSymbol(label, value);
        }
        private bool AfterLabel(Identifier label)
        {
            if (!(LastToken is ReservedWord reservedWord))
                return false;
            switch (reservedWord.Id) {
                case Keyword.Equ:
                    EquDirective(label);
                    return true;
                default:
                    Address address = CurrentAddress;
                    if (StorageDirective()) {
                        DefineSymbol(label, address);
                        return true;
                    }
                    break;
            }
            return false;
        }
        private void Label()
        {
            var identifier = LastToken as Identifier;
            Debug.Assert(identifier != null);
            Token token = NextToken();
            if (token.IsReservedWord(':')) {
                Address address = CurrentAddress;
                DefineSymbol(identifier, address);
                NextToken();
            }
            else {
                if (AfterLabel(identifier)) { return; }
                ShowSyntaxError(identifier);
            }
        }
        protected bool Directive()
        {
            var reservedWord = LastToken as ReservedWord;
            Debug.Assert(reservedWord != null);
            return Directive(reservedWord) || StorageDirective();
        }

        protected virtual bool Directive(ReservedWord reservedWord)
        {
            switch (reservedWord.Id) {
                case Keyword.Include:
                    IncludeDirective();
                    return true;
                case Keyword.CSeg:
                    SegmentDirective(AddressType.Code);
                    return true;
                case Keyword.DSeg:
                    SegmentDirective(AddressType.Data);
                    return true;
                case Keyword.ZSeg:
                    if (ZeroPageAvailable) {
                        SegmentDirective(AddressType.ZeroPage);
                        return true;
                    }

                    break;
                case Keyword.Public:
                    PublicDirective();
                    return true;
                case Keyword.Extrn:
                case Keyword.Ext:
                    ExternDirective(AddressType.External);
                    return true;
            }

            return false;
        }


        protected virtual bool IsRelativeOffsetInRange(int offset)
        {
            return IsRelativeOffsetInByte(offset);
        }

        protected static bool IsRelativeOffsetInByte(int offset)
        {
            return offset >= -128 && offset <= 128;
        }

        protected int RelativeOffset(Address address)
        {
            const int instructionLength = 2;
            return address.Value - (CurrentAddress.Value + instructionLength);
        }
        protected bool RelativeOffset(out Address address, out int offset)
        {
            offset = 0;
            var operand = LastToken;
            var expression = Expression();
            if (expression == null) {
                ShowSyntaxError();
                address = Address.Default;
                return false;
            }
            address = expression;
            return RelativeOffset(operand, address, out offset);
        }

        protected bool RelativeOffset(Token token, Address address, out int offset)
        {
            offset = 0;
            switch (address.Type) {
                case AddressType.Undefined:
                    return false;
                case AddressType.Const:
                case AddressType.External:
                    ShowAddressUsageError(token);
                    return false;
                default:
                    if (address.Type == CurrentSegment.Type) {
                        offset = RelativeOffset(address);
                    }
                    else {
                        ShowAddressUsageError(token);
                        return false;
                    }

                    break;
            }
            return IsRelativeOffsetInRange(offset);
        }

        protected abstract bool Instruction();


        protected int AutoLabel()
        {
            int id = nextAutoLabelId++;
            //DefineSymbol(id, Address(AddressType.Undefined, 0));
            return id++;
        }

        protected Block? LastBlock()
        {
            Debug.Assert(listFile != null);
            listFile.IndentLevel = blocks.Count - 1;
            return blocks.Count > 0 ? blocks.Peek() : null;
        }

        protected IfBlock NewIfBlock()
        {
            IfBlock block = new IfBlock(AutoLabel(), AutoLabel());
            blocks.Push(block);
            return block;
        }

        protected WhileBlock NewWhileBlock()
        {
            WhileBlock block = new WhileBlock(AutoLabel(), AutoLabel(), AutoLabel());
            blocks.Push(block);
            return block;
        }

        protected void EndBlock()
        {
            Debug.Assert(blocks.Count > 0);
            Debug.Assert(listFile != null);
            blocks.Pop();
            listFile.IndentLevel = blocks.Count;
        }


        private void Assemble()
        {
            while (NextToken().IsReservedWord('\n')) { }

            Token token;
            while (!(token = LastToken).IsEof() && ErrorCount < MaxErrorCount) {
                switch (token.Type) {
                    case TokenType.ReservedWord:
                        if (!Directive() && !Instruction()) {
                            if (LastToken.IsEof()) { break; }
                            ShowSyntaxError();
                            NextToken();
                        }
                        break;
                    case TokenType.Identifier:
                        Label();
                        break;
                    default:
                        ShowSyntaxError();
                        NextToken();
                        break;
                }
                SkipEndOfStatement();
            }
        }
        private int Assemble(string sourceName, string objName, string listName)
        {
            for (Pass = 1; (Pass <= 2 || addressChanged) && ErrorCount <= 0; ++Pass) {
                Console.Out.WriteLine("Pass " + Pass);
                addressChanged = false;
                listFile = new ListFile(listName);
                SourceReader.Printer = listFile;
                @object.AddressUsages.Clear();
                nextAutoLabelId = AutoLabelMinId;
                try {
                    OpenSourceFile(sourceName);
                }
                catch (IOException e) {
                    Console.Error.WriteLine(e.Message);
                    return Failure;
                }
                foreach (Segment segment in @object.Segments) {
                    segment.Clear();
                }
                listFile.Address = CurrentAddress;
                Assemble();
                listFile.Close();
            }
            if (ErrorCount > 0) { return Failure; }

            @object.Save(objName);
            return Success;
        }

        public int Main(NormalArgument normalArgument)
        {
            var args = normalArgument.Values;
            if (args.Count < 1) {
                Console.Error.WriteLine("No source file.");
                return Failure;
            }

            string sourceName = Path.GetFullPath(args[0]);
            string objName = Path.ChangeExtension(sourceName, Object.Extension);
            string listName = Path.ChangeExtension(sourceName, ListFile.Ext);
            return Assemble(sourceName, objName, listName);
        }
    }

    public abstract class LittleEndianAssembler : Assembler
    {
        protected LittleEndianAssembler(Tokenizer tokenizer) : base(tokenizer) { }

        protected override byte[] ToBytes(int value)
        {
            return new[]
            {
                (byte)(value & 0xff),
                (byte)((value >> 8) & 0xff)
            };
        }
    }

    public abstract class BigEndianAssembler : Assembler
    {
        protected BigEndianAssembler(Tokenizer tokenizer) : base(tokenizer) { }

        protected override byte[] ToBytes(int value)
        {
            return new[]
            {
                (byte)((value >> 8) & 0xff),
                (byte)(value & 0xff)
            };
        }
    }
}
