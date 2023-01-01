using Inu.Language;

namespace Inu.Assembler.MuCom87
{
    public class Program
    {
        private enum CpuType
        {
            MuPd7800,
            MuPd7805,
        };
        public static int Main(string[] args)
        {
            var cpuType = CpuType.MuPd7800;
            var normalArgument = new NormalArgument(args, (option, value) =>
            {
                cpuType = option switch
                {
                    "7801" => CpuType.MuPd7800,
                    "7805" => CpuType.MuPd7805,
                    _ => cpuType
                };
                return false;
            });

            Assembler assembler;
            if (cpuType == CpuType.MuPd7805)
                assembler = new MuPD7805.Assembler();
            else
                assembler = new MuPD7800.Assembler();
            return assembler.Main(normalArgument);
        }
    }
}
