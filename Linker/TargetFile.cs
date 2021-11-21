using System;

namespace Inu.Linker
{
    public abstract class TargetFile : IDisposable
    {
        public abstract void Write(int address, byte[] bytes);

        protected int Write(int address, byte[] bytes, int maxRecordSize)
        {
            var offset = 0;
            while (offset < bytes.Length) {
                int recordSize = Math.Min(maxRecordSize, bytes.Length - offset);
                WriteRecord(address + offset, bytes, offset, recordSize);
                offset += recordSize;
            }
            return offset;
        }

        protected abstract void WriteRecord(int address, byte[] bytes, int offset, int recordSize);
        public abstract void Dispose();
    }
}
