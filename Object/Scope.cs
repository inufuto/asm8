namespace Inu.Assembler;

public class Scope(int id, Scope? parent)
{
    public readonly int Id = id;
    public readonly Scope? Parent = parent;
}