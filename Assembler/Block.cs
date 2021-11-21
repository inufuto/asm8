using System.Diagnostics;

namespace Inu.Assembler
{
    public abstract class Block
    {
        public const int InvalidId = 0;
    }

    public class IfBlock : Block
    {
        public int ElseId { get; set; }
        public int EndId { get; private set; }

        public IfBlock(int elseId, int endId)
        {
            ElseId = elseId;
            this.EndId = endId;
        }
        public int ConsumeElse()
        {
            int id = ElseId;
            Debug.Assert(id > 0);
            ElseId = InvalidId;
            return id;
        }
        public void SetElseId(int id)
        {
            Debug.Assert(ElseId == InvalidId);
            ElseId = id;
        }
    };

    public class WhileBlock : Block
    {
        public int BeginId { get; private set; }
        public int RepeatId { get; private set; }
        public int EndId { get; private set; }

        public WhileBlock(int beginId, int repeatId, int endId)
        {
            BeginId = beginId;
            RepeatId = repeatId;
            EndId = endId;
        }
        
        public void EraseEndId() { EndId = InvalidId; }
    }
}
