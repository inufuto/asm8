using System.Globalization;
using Inu.Language;

namespace Inu.Assembler.Sc62015
{
    internal class Program
    {
        public static int Main(string[] args)
        {
            var memoryPage = 0x0b;
            var normalArgument = new NormalArgument(args, (key, value) =>
            {
                switch (key) {
                    case "MEMPAGE" when !string.IsNullOrEmpty(value):
                        try {
                            memoryPage = int.Parse(value, NumberStyles.AllowHexSpecifier);
                        }
                        catch (FormatException) {
                        }
                        return true;
                    default:
                        return false;
                }
            });
            return new Assembler(memoryPage).Main(normalArgument);
        }
    }
}