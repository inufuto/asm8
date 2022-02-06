using Inu.Language;

namespace Inu.Assembler.Tms99
{
    internal class Program
    {
        public static int Main(string[] args)
        {
            var normalArgument = new NormalArgument(args);
            return new Assembler().Main(normalArgument);
        }
    }
}
