using Inu.Language;

namespace Inu.Assembler.Mc6809;

public class Program
{
    public static int Main(string[] args)
    {
        var normalArgument = new NormalArgument(args);
        return new Assembler().Main(normalArgument);
    }
}