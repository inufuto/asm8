using System;
using Inu.Language;

namespace Inu.Assembler.I8086
{
    internal class Program
    {
        public static int Main(string[] args)
        {
            var normalArgument = new NormalArgument(args);
            return new Assembler().Main(normalArgument);
        }
    }
}
