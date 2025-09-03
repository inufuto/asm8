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
            var version=1;
            var normalArgument = new NormalArgument(args, (option, value) =>
            {
                switch (option)
                {
                    case "7801":
                        cpuType = CpuType.MuPd7800;
                        break;
                    case "7805":
                        cpuType = CpuType.MuPd7805;
                        break;
                    case "V2":
                        version = 2;
                        break;
                }
                return false;
            });

            Assembler assembler;
            if (cpuType == CpuType.MuPd7805)
                assembler = new MuPD7805.Assembler(version);
            else
                assembler = new MuPD7800.Assembler(version);
            return assembler.Main(normalArgument);
        }
    }
}
