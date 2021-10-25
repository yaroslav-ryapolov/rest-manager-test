using System;

namespace RestManagerWeb.Models
{
    public record ClientsGroupViewModel
    {
        public Guid Guid { get; set; }
        public int Size { get; set; }

        public ClientsGroupViewModel(Guid guid, int size)
        {
            Guid = guid;
            Size = size;
        }
    }
}
