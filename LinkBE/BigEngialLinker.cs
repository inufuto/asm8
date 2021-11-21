namespace Inu.Linker
{
    internal class BigEndianLinker : Linker
    {
        protected override byte[] ToBytes(int value)
        {
            return new[]
            {
                (byte)((value >> 8) & 0xff),
                (byte)(value & 0xff)
            };
        }
    }
}
