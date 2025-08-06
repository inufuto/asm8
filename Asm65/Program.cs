using System;
using Inu.Assembler.Mos6502;
using Inu.Language;

namespace Asm65;

public class Program
{
    private enum CpuType
    {
        Mos6502,
        Wdc65C02,
        Wdc65816,
    };

    public static int Main(string[] args)
    {
        var cpuType = CpuType.Mos6502;
        var version = 1;
        var normalArgument = new NormalArgument(args, (option, value) =>
        {
            switch (option)
            {
                case "6502":
                    cpuType = CpuType.Mos6502;
                    break;
                case "65C02":
                    cpuType = CpuType.Wdc65C02;
                    break;
                case "65816":
                    cpuType = CpuType.Wdc65816;
                    break;
                case "V2":
                    version = 2;
                    break;
            }
            return false;
        });
        var assembler = cpuType switch
        {
            CpuType.Wdc65C02 => new Inu.Assembler.Wdc65c02.Assembler(version),
            CpuType.Wdc65816 => new Inu.Assembler.Wdc65816.Assembler(version),
            _ => new Assembler(version)
        };
        return assembler.Main(normalArgument);
    }
}