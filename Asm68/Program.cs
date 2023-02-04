using Inu.Assembler.Mc6800;
using Inu.Language;

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
            var normalArgument = new NormalArgument(args, (option, value) =>
            {
                cpuType = option switch
                {
                    "6801" => CpuType.Mc6801,
                    "6301" => CpuType.Hd6301,
                    "8861" => CpuType.Mb8861,
                    _ => cpuType
                };
                return false;
            });
            var assembler = cpuType switch
            {
                CpuType.Mc6801 => new Inu.Assembler.Mc6801.Assembler(),
                CpuType.Hd6301 => new Inu.Assembler.Hd6301.Assembler(),
                CpuType.Mb8861 => new Inu.Assembler.Mb8861.Assembler(),
                _ => new Assembler()
            };
            return assembler.Main(normalArgument);
        }
    }
}
