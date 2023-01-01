using System;
using System.Collections.Generic;
using System.Text;

namespace Inu.Assembler.MuCom87.MuPD7800
{
    internal class Keyword : MuCom87.Keyword
    {
        public new const int MinId = MuCom87.Keyword.NextId;

        public const int ADCW = MinId + 0;
        public const int ADDNCW = MinId + 1;
        public const int ADDW = MinId + 2;
        public const int ANAW = MinId + 3;
        public const int BIT = MinId + 4;
        public const int BLOCK = MinId + 5;
        public const int CALB = MinId + 6;
        public const int EQAW = MinId + 7;
        public const int EX = MinId + 8;
        public const int EXX = MinId + 9;
        public const int GTAW = MinId + 10;
        public const int HLT = MinId + 11;
        public const int LTAW = MinId + 12;
        public const int MVIW = MinId + 13;
        public const int MVIX = MinId + 14;
        public const int NEAW = MinId + 15;
        public const int OFFA = MinId + 16;
        public const int OFFAW = MinId + 17;
        public const int ONA = MinId + 18;
        public const int ONAW = MinId + 19;
        public const int ORAW = MinId + 20;
        public const int PEN = MinId + 21;
        public const int RCL = MinId + 22;
        public const int RCR = MinId + 23;
        public const int SBBW = MinId + 24;
        public const int SHAL = MinId + 25;
        public const int SHAR = MinId + 26;
        public const int SHCL = MinId + 27;
        public const int SHCR = MinId + 28;
        public const int SKC = MinId + 29;
        public const int SKIT = MinId + 30;
        public const int SKZ = MinId + 31;
        public const int SOFTI = MinId + 32;
        public const int SUBNBW = MinId + 33;
        public const int SUBW = MinId + 34;
        public const int TABLE = MinId + 35;
        public const int XRAW = MinId + 36;

        protected new const int NextId = MinId + 40;

        public new static readonly Dictionary<int, string> Words = new Dictionary<int, string>()
        {
            { ADCW,"ADCW"},
            { ADDNCW,"ADDNCW"},
            { ADDW,"ADDW"},
            { ANAW,"ANAW"},
            { BIT,"BIT"},
            { BLOCK,"BLOCK"},
            { CALB,"CALB"},
            { EQAW,"EQAW"},
            { EX,"EX"},
            { EXX,"EXX"},
            { GTAW,"GTAW"},
            { HLT,"HLT"},
            { LTAW,"LTAW"},
            { MVIW,"MVIW"},
            { MVIX,"MVIX"},
            { NEAW,"NEAW"},
            { OFFA,"OFFA"},
            { OFFAW,"OFFAW"},
            { ONA,"ONA"},
            { ONAW,"ONAW"},
            { ORAW,"ORAW"},
            { PEN,"PEN"},
            { RCL,"RCL"},
            { RCR,"RCR"},
            { SBBW,"SBBW"},
            { SHAL,"SHAL"},
            { SHAR,"SHAR"},
            { SHCL,"SHCL"},
            { SHCR,"SHCR"},
            { SKC,"SKC"},
            { SKIT,"SKIT"},
            { SKZ,"SKZ"},
            { SOFTI,"SOFTI"},
            { SUBNBW,"SUBNBW"},
            { SUBW,"SUBW"},
            { TABLE,"TABLE"},
            { XRAW,"XRAW"},
        };
    }
}
