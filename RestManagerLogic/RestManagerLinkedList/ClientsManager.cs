using System;
using System.Collections.Generic;
using System.Linq;

namespace RestManagerLogic.RestManagerLinkedList
{
    public class ClientsManager
    {
        private readonly Dictionary<Guid, GroupInRest> _clients = new();
        private readonly LinkedList<GroupInRest> _clientsQueue = new();
        private readonly List<LinkedList<GroupInRest>> _clientsQueueBySize = new();

        public ClientsManager(int sizeLimit)
        {
            for (int i = 0; i < sizeLimit + 1; i++)
            {
                _clientsQueueBySize.Add(new LinkedList<GroupInRest>());
            }
        }

        public void AddGroup(ClientsGroup group)
        {
            if (!_clients.TryAdd(group.Guid, new GroupInRest(group)))
            {
                throw new ArgumentException("Cannot add already presented group", nameof(group));
            }
        }

        public void RemoveGroup(ClientsGroup group)
        {
            DequeueGroup(group);
            if (!_clients.Remove(group.Guid))
            {
                throw new ArgumentException("Cannot remove not presented group", nameof(group));
            }
        }

        public void EnqueueGroup(ClientsGroup group)
        {
            var groupInRest = _clients[group.Guid];
            _clientsQueue.AddLast(groupInRest.QueueNode);

            _clientsQueueBySize[group.Size].AddLast(groupInRest.QueueBySizeNode);
        }

        public bool DequeueGroup(ClientsGroup group)
        {
            // TODO: as result of this method GroupInRest would be in incorrect state (as Node fields will point to some un-actual nodes
            var groupInRest = _clients[group.Guid];
            if (groupInRest.QueueNode?.List != null)
            {
                _clientsQueue.Remove(groupInRest.QueueNode);
                _clientsQueueBySize[group.Size].Remove(groupInRest.QueueBySizeNode);
                return true;
            }

            return false;
        }

        public ClientsGroup FindNextSmallerOrEqualGroupInQueue(int size)
        {
            GroupInRest result = null;
            for (int i = size; i > 0; i--)
            {
                var candidate = _clientsQueueBySize[i].First?.Value;

                if (result == null || candidate?.ArrivalTime < result?.ArrivalTime)
                {
                    result = candidate;
                }
            }

            return result?.Group;
        }

        public void SeatGroupAtTable(ClientsGroup group, Table table)
        {
            var groupInRest = _clients[group.Guid];
            if (groupInRest.Table != null)
            {
                throw new InvalidOperationException("Cannot re-assign already seated group");
            }

            DequeueGroup(group);
            groupInRest.SetTable(table);
        }

        public Table GetGroupTable(ClientsGroup group)
        {
            return _clients[group.Guid].Table;
        }

        public QueueItem GetCurrentAndMoveNext(QueueItem current = null)
        {
            var groupNode = _clientsQueue.First;
            if (current != null)
            {
                groupNode = current.Next;
            }

            if (groupNode == null)
            {
                return null;
            }

            return new QueueItem
            {
                Next = groupNode.Next,
                Current = groupNode.Value,
            };
        }

        public class QueueItem
        {
            public LinkedListNode<GroupInRest> Next;
            public GroupInRest Current;
        }

        public class GroupInRest
        {
            public readonly DateTime ArrivalTime;
            public readonly ClientsGroup Group;
            public LinkedListNode<GroupInRest> QueueNode { get; private set; }
            public LinkedListNode<GroupInRest> QueueBySizeNode { get; private set; }
            public Table Table { get; private set; }

            public GroupInRest(ClientsGroup group)
            {
                ArrivalTime = DateTime.Now;
                Group = group;

                QueueNode = new LinkedListNode<GroupInRest>(this);
                QueueBySizeNode = new LinkedListNode<GroupInRest>(this);
            }

            public void SetTable(Table table)
            {
                QueueNode = null;
                QueueBySizeNode = null;
                Table = table;
            }
        }
    }
}
