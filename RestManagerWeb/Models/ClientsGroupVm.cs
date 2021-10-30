using System;
using RestManagerLogic;

namespace RestManagerWeb.Models
{
    public record ClientsGroupVm
    {
        public Guid Guid { get; set; }
        public int Size { get; set; }

        public static ClientsGroupVm From(ClientsGroup group)
        {
            return new ClientsGroupVm
            {
                Guid = group.Guid,
                Size = group.Size,
            };
        }
    }
}
