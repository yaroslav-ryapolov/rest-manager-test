using System;
using System.Collections.Generic;
using System.Linq;

namespace RestManagerLogic
{
    public class Table
    {
        public readonly Guid Guid;
        public readonly int Size;
        private readonly List<ClientsGroup> _seatedClientsGroups = new List<ClientsGroup>();
        public IReadOnlyCollection<ClientsGroup> SeatedClientGroups => _seatedClientsGroups.AsReadOnly();
        
        // есть смысл "кешировать" доступное количество стульев, чтобы не суммировать группу каждый раз (при посадке и наоборот)
        public int AvailableChairs => this.Size - _seatedClientsGroups.Sum((g) => g.Size);
        public bool IsOccupied => _seatedClientsGroups.Any();

        public Table(int size)
        {
            this.Guid = Guid.NewGuid();
            this.Size = size;
        }

        // public bool TryToSeatClientsGroup(ClientsGroup group)
        public void SeatClientsGroup(ClientsGroup group)
        {
            // TO LOCK Table
            
            // попробовать посадить группу за стол
            if (group.Size > this.AvailableChairs)
            {
                throw new ArgumentOutOfRangeException(nameof(@group), group.Size, "Group does not fit in table");
            }

            _seatedClientsGroups.Add(group);
                
            // return false;
        }

        public void ReleaseChairs(ClientsGroup group)
        {
            // TO LOCK Table
            
            // освободить стулья
            if (!_seatedClientsGroups.Remove(group))
            {
                throw new ArgumentOutOfRangeException(nameof(@group), "Group was not at this table");
            }
        }
    }
}