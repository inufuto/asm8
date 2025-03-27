using Inu.Language;

namespace Inu.Assembler.Tlcs900;

internal class Program
{
    private enum AddressSize
    {
        Max,
        Min,
    };

    static int Main(string[] args)
    {
        var normalArgument = new NormalArgument(args);
        return new Assembler().Main(normalArgument);
    }
}