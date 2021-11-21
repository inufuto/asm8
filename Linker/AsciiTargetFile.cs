using System;
using System.IO;
using System.Text;

namespace Inu.Linker
{
    internal abstract class AsciiTargetFile : TargetFile
    {
        protected StreamWriter Writer { get; private set; }

        protected AsciiTargetFile(string fileName)
        {
            Writer = new StreamWriter(fileName, false, Encoding.ASCII);
        }

        public override void Dispose()
        {
            Writer.Dispose();
        }
    }
}
