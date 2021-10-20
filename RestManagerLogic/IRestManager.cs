using System.Collections.Generic;

namespace RestManagerLogic
{
    public interface IRestManager
    {
        IEnumerable<Table> Tables { get; }

        void OnArrive(ClientsGroup group);
        void OnLeave(ClientsGroup group);
        Table Lookup(ClientsGroup group);
    }
}
