using System;

namespace RestManagerLogic
{
    public class Table
    {
        public readonly Guid Guid;
        public readonly int Size;

        public Table(int size)
        {
            this.Guid = Guid.NewGuid();
            this.Size = size;
        }
    }
}