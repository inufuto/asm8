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
        bool shortAddress = false;
        var normalArgument = new NormalArgument(args, (option, value) =>
        {
            if (option == "MIN")
            {
                shortAddress = true;
            }
            return false;
        });
        return new Assembler(shortAddress).Main(normalArgument);
    }
}