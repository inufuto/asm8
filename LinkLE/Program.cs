using Inu.Language;

namespace Inu.Linker
{
    class Program
    {
        public static int Main(string[] args)
        {
            var normalArgument = new NormalArgument(args);
            return new LittleEndianLinker().Main(normalArgument);
        }
    }
}
