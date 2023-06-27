using Inu.Language;

namespace Inu.Assembler.Sc62015
{
    internal class Tokenizer : Inu.Assembler.Tokenizer
    {
        public Tokenizer()
        {
            ReservedWord.AddWords(Keyword.Words);
        }

        private static bool IsOperatorChar(char c)
        {
            return c is '+' or '-';
        }

        protected override bool IsSequenceHead(char c)
        {
            return IsOperatorChar(c);
        }

        protected override int ReadSequence()
        {
            using var backup = new Backup(this);
            var word = ReadCharSequence(IsOperatorChar);
            var id = ReservedWord.ToId(word);
            if (id <= 0) {
                backup.Restore();
            }
            return id;
        }
    }
}
