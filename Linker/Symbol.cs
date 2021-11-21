using Inu.Assembler;

namespace Inu.Linker
{
    class Symbol
    {
        public int Id { get; private set; }
        public Address Address { get; set; }
        public Assembler.Object Object { get; private set; }
        public Symbol(int id, Address address, Assembler.Object obj)
        {
            Id = id;
            Address = address;
            Object = obj;
        }
    }
}
