using System.Collections.Generic;
using System.Diagnostics;
using Inu.Language;

namespace Inu.Assembler
{
    public class Keyword
    {
        public const int MinId = Identifier.MinId;

        public const int CSeg = MinId + 0;
        public const int Db = MinId + 1;
        public const int Ds = MinId + 2;
        public const int DSeg = MinId + 3;
        public const int Dw = MinId + 4;
        public const int Equ = MinId + 5;
        public const int Ext = MinId + 6;
        public const int Extrn = MinId + 7;
        public const int Include = MinId + 8;
        public const int Public = MinId + 9;
        public const int ZSeg = MinId + 10;
        public const int And = MinId + 11;
        public const int High = MinId + 12;
        public const int Low = MinId + 13;
        public const int Mod = MinId + 14;
        public const int Not = MinId + 15;
        public const int Or = MinId + 16;
        public const int Shl = MinId + 17;
        public const int Shr = MinId + 18;
        public const int Xor = MinId + 19;
        public const int DefB = MinId + 20;
        public const int DefS = MinId + 21;
        public const int DefW = MinId + 22;
        public const int Do = MinId + 23;
        public const int Else = MinId + 24;
        public const int ElseIf = MinId + 25;
        public const int EndIf = MinId + 26;
        public const int If = MinId + 27;
        public const int WEnd = MinId + 28;
        public const int While = MinId + 29;

        protected const int NextId = MinId + 30;

        public static readonly Dictionary<int, string> Words = new Dictionary<int, string>()
        {
            { CSeg,"CSEG"},
            { Db,"DB"},
            { Ds,"DS"},
            { DSeg,"DSEG"},
            { Dw,"DW"},
            { Equ,"EQU"},
            { Ext,"EXT"},
            { Extrn,"EXTRN"},
            { Include,"INCLUDE"},
            { Public,"PUBLIC"},
            { ZSeg,"ZSEG"},
            { And,"AND"},
            { High,"HIGH"},
            { Low,"LOW"},
            { Mod,"MOD"},
            { Not,"NOT"},
            { Or,"OR"},
            { Shl,"SHL"},
            { Shr,"SHR"},
            { Xor,"XOR"},
            { DefB,"DEFB"},
            { DefS,"DEFS"},
            { DefW,"DEFW"},
            { Do,"DO"},
            { Else,"ELSE"},
            { ElseIf,"ELSEIF"},
            { EndIf,"ENDIF"},
            { If,"IF"},
            { WEnd,"WEND"},
            { While,"WHILE"},
        };

        public static Dictionary<int, string> Append(Dictionary<int, string> words)
        {
            var appended = new Dictionary<int, string>(Words);
            foreach (var (key, value) in words) {
                Debug.Assert(!appended.ContainsKey(key) && !appended.ContainsValue(value));
                appended.Add(key, value);
            }
            return appended;
        }
    }
}
