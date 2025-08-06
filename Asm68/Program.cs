using Inu.Assembler.Mc6800;
using Inu.Language;
using System;

namespace Asm68
{
    public class Program
    {
        private enum CpuType
        {
            Mc6800,
            Mc6801,
            Hd6301,
            Mb8861
        };


        public static int Main(string[] args)
        {
            var cpuType = CpuType.Mc6800;
            var version=1;
            var normalArgument = new NormalArgument(args, (option, value) =>
            {
                switch (option)
                {
                    case "6801":
                        cpuType = CpuType.Mc6801;
                        break;
                    case "6301":
                        cpuType = CpuType.Hd6301;
                        break;
                    case "8861":
                        cpuType = CpuType.Mb8861;
                        break;
                    case "V2":
                        version = 2;
                        break;
                }
                return false;
            });
            var assembler = cpuType switch
            {
                CpuType.Mc6801 => new Inu.Assembler.Mc6801.Assembler(version),
                CpuType.Hd6301 => new Inu.Assembler.Hd6301.Assembler(version),
                CpuType.Mb8861 => new Inu.Assembler.Mb8861.Assembler(version),
                _ => new Assembler(version)
            };
            return assembler.Main(normalArgument);
        }
    }
}
