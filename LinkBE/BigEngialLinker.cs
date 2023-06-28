using System.Drawing;

namespace Inu.Linker
{
    internal class BigEndianLinker : Linker
    {
        protected override byte[] ToBytes(int value, int size)
        {
            var bytes = new byte[size];
            for (var i = 0; i < size; ++i) {
                bytes[size - i] = (byte)(value & 0xff);
                value >>= 8;
            }
            return bytes;
        }
    }
}
