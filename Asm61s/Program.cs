using Inu.Language;

namespace Inu.Assembler.SC61860;

class Program
{
    public static int Main(string[] args)
    {
        var normalArgument = new NormalArgument(args);
        return new Assembler().Main(normalArgument);
    }
}