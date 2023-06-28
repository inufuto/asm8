namespace Inu.Linker
{
    internal class LittleEndianLinker : Linker
    {
        protected override byte[] ToBytes(int value, int size)
        {
            var bytes = new byte[size];
            for (var i = 0; i < size; ++i) {
                bytes[i] = (byte)(value & 0xff);
                value >>= 8;
            }
            return bytes;
        }
    }
}
