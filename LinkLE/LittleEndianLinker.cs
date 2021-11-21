namespace Inu.Linker
{
    internal class LittleEndianLinker : Linker
    {
        protected override byte[] ToBytes(int value)
        {
            return new[] {
                (byte)(value & 0xff),
                (byte)((value >> 8) & 0xff)
            };
        }
    }
}
