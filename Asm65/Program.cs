using Inu.Assembler.Mos6502;
using Inu.Language;

namespace Asm65;

public class Program
{
    private enum CpuType
    {
        Mos6502,
        Wdc65C02,
    };

    public static int Main(string[] args)
    {
        var cpuType = CpuType.Mos6502;
        var normalArgument = new NormalArgument(args, (option, value) =>
        {
            cpuType = option switch
            {
                "6502" => CpuType.Mos6502,
                "65C02" => CpuType.Wdc65C02,
                _ => cpuType
            };
            return false;
        });
        var assembler = cpuType switch
        {
            CpuType.Wdc65C02 => new Inu.Assembler.Wdc65c02.Assembler(),
            _ => new Assembler()
        };
        return assembler.Main(normalArgument);
    }
}