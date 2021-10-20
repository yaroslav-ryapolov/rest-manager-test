using System;
using System.Collections.Generic;

namespace RestManagerLogic.RestManagerLinkedList
{
    public class ClientsManager
    {
        private readonly LinkedList<ClientsGroup> _clientsQueue = new();
        private readonly Dictionary<Guid, GroupInRest> _clients = new();

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
            _clientsQueue.AddLast(groupInRest.Node);
        }

        public bool DequeueGroup(ClientsGroup group)
        {
            var groupNode = _clients[group.Guid].Node;
            if (groupNode?.List != null)
            {
                _clientsQueue.Remove(groupNode);
                return true;
            }

            return false;
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
            public LinkedListNode<ClientsGroup> Next;
            public ClientsGroup Current;
        }

        private class GroupInRest
        {
            public LinkedListNode<ClientsGroup> Node { get; private set; }
            public Table Table { get; private set; }

            public GroupInRest(ClientsGroup group)
            {
                Node = new LinkedListNode<ClientsGroup>(group);
            }

            public void SetTable(Table table)
            {
                Node = null;
                Table = table;
            }
        }
    }
}