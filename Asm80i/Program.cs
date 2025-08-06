using Inu.Language;

namespace Inu.Assembler.I8080;

internal class Program
{
    static int Main(string[] args)
    {
        var version = 1;
        var normalArgument = new NormalArgument(args, (option, value) =>
        {
            version = option switch
            {
                "V2" => 2,
                _ => version
            };
            return false;
        });
        return new Assembler(version).Main(normalArgument);
    }
}