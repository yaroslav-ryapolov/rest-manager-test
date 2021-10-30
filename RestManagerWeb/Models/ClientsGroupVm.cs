using System;

namespace RestManagerWeb.Models
{
    public record ClientsGroupVm
    {
        public Guid Guid { get; set; }
        public int Size { get; set; }

        public ClientsGroupVm(Guid guid, int size)
        {
            Guid = guid;
            Size = size;
        }
    }
}
