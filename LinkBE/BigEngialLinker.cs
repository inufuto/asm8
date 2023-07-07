namespace Inu.Linker
{
    internal class BigEndianLinker : Linker
    {
        protected override byte[] ToBytes(int value, int size)
        {
            var bytes = new byte[size];
            for (var i = 0; i < size; ++i) {
                bytes[size - 1 - i] = (byte)(value & 0xff);
                value >>= 8;
            }
            return bytes;
        }
    }
}
