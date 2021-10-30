using System;
using System.Collections.Generic;
using System.Linq;

namespace RestManagerLogic
{
    public class Table
    {
        public Guid Guid { get; }
        public int Size { get; }
        public string Name { get; set; }

        private readonly List<ClientsGroup> _seatedClientsGroups = new();
        public IEnumerable<ClientsGroup> SeatedClientGroups => _seatedClientsGroups;

        public int AvailableChairs { get; private set; }
        public bool IsOccupied => _seatedClientsGroups.Any();

        public Table(Guid guid, int size)
        {
            Guid = guid;
            Size = size;
        }

        public Table(int size)
        {
            Guid = Guid.NewGuid();
            Size = size;
            AvailableChairs = Size;
        }

        public void SeatClientsGroup(ClientsGroup group)
        {
            if (group.Size > this.AvailableChairs)
            {
                throw new ArgumentOutOfRangeException(nameof(group), group.Size, "Group does not fit in table");
            }

            _seatedClientsGroups.Add(group);
            AvailableChairs -= group.Size;
        }

        public void ReleaseChairs(ClientsGroup group)
        {
            if (!_seatedClientsGroups.Remove(group))
            {
                throw new ArgumentOutOfRangeException(nameof(group), "Group was not at this table");
            }
            AvailableChairs += group.Size;
        }

        public bool HasGroup(ClientsGroup group)
        {
            return _seatedClientsGroups.Contains(group);
        }
    }
}
