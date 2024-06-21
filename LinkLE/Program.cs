using Inu.Language;

namespace Inu.Linker
{
    class Program
    {
        public static int Main(string[] args)
        {
            var makeDataSegment = false;
            var normalArgument = new NormalArgument(args, (option, value) =>
            {
                if (option == "DSEG") {
                    makeDataSegment = true;
                }
                return false;
            });
            return new LittleEndianLinker().Main(normalArgument, makeDataSegment);
        }
    }
}
