using Inu.Language;

namespace Inu.Assembler.HD61700;

class Program
{
    public static int Main(string[] args)
    {
        var normalArgument = new NormalArgument(args);
        return new Assembler().Main(normalArgument);
    }
}