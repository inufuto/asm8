using Inu.Assembler.Mc6800;
using Inu.Language;

namespace Asm68
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
