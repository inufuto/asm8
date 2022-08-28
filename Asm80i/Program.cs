using Inu.Language;

namespace Inu.Assembler.I8080
{
    internal class Program
    {
        static int Main(string[] args)
        {
            var normalArgument = new NormalArgument(args);
            return new Assembler().Main(normalArgument);
        }
    }
}
