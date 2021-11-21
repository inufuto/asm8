using Inu.Language;

namespace Inu.Linker
{
    public class Program
    {
        public static int Main(string[] args)
        {
            var normalArgument = new NormalArgument(args);
            return new BigEndianLinker().Main(normalArgument);
        }
    }
}
