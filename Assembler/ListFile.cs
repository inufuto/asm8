using Inu.Language;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Inu.Assembler;

class ListFile(string fileName) : SourcePrinter, IDisposable
{
    private const int MaxBytes = 6;
    private const string Suffix = "\'\"<";
    public const string Ext = ".lst";

    private readonly StreamWriter writer = new(fileName, false, Encoding.UTF8);
    public int IndentLevel { get; set; }
    public Address? Address { get; set; }
    private readonly List<byte> bytes = new List<byte>();
    private readonly List<string> sourceLines = new List<string>();

    public void Dispose()
    {
        writer.Dispose();
    }


    public void AddByte(int value) { bytes.Add((byte)value); }

    public void PrintLine()
    {
        var lineCount = 0;
        var byteCount = 0;
        while (byteCount < bytes.Count || lineCount < sourceLines.Count) {
            Debug.Assert(Address != null);
            PrintAddress(Address);
            for (int i = 0; i < MaxBytes; ++i) {
                if (byteCount < bytes.Count) {
                    writer.Write(ToHex(bytes[byteCount++], 2));
                }
                else {
                    writer.Write("  ");
                }
            }
            writer.Write("  ");
            if (lineCount < sourceLines.Count) {
                PrintSource(sourceLines[lineCount++]);
            }
            writer.WriteLine();
        }
        Clear();
    }

    public override void AddSourceLine(string sourceLine) => sourceLines.Add(sourceLine);

    private void Clear()
    {
        bytes.Clear();
        sourceLines.Clear();
    }

    public void Close()
    {
        writer.Close();
    }

    private void PrintAddress(Address address)
    {
        writer.Write(ToHex(address.Value, 4) + Suffix[(int)address.Type] + " ");
    }


    private void PrintSource(string sourceLine)
    {
        writer.Write(" ");
        for (int i = 0; i < IndentLevel; ++i) {
            writer.Write("    ");
        }
        writer.Write(sourceLine.Trim());
    }

    private static string ToHex(int value, int length)
    {
        var s = $"{value:X4}";
        Debug.Assert(s.Length <= 4);
        return s.Substring(s.Length - length);
    }

}