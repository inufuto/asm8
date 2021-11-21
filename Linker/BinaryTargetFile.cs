using System.IO;

namespace Inu.Linker
{
    internal abstract class BinaryTargetFile : TargetFile
    {
        protected Stream Stream { get; private set; }

        protected BinaryTargetFile(string fileName)
        {
            Stream = new FileStream(fileName, FileMode.Create, FileAccess.Write);
        }

        public override void Dispose()
        {
            Stream.Dispose();
        }
    }
}