using System;

namespace RestManagerLogic
{
    public class ClientsGroup
    {
        public readonly Guid Guid;
        public readonly int Size;

        public ClientsGroup(int size)
        {
            this.Guid = Guid.NewGuid();
            this.Size = size;
        }
    }
}