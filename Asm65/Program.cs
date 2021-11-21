using Inu.Assembler.Mos6502;
using Inu.Language;

namespace Asm65
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
