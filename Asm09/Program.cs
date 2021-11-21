using Inu.Assembler.Mc6809;
using Inu.Language;

namespace Asm09
{
    public class Program
    {
        public static int Main(string[] args)
        {
            var normalArgument = new NormalArgument(args);
            return new Assembler().Main(normalArgument);
        }
    }
}
