
using System.Runtime.Serialization;
using System.Collections.Generic;

namespace SmartHotel.Clients.Models
{
    [DataContract]
    public class ShoppingList : Resource
    {
        public ShoppingList()
        {
            ShoppingItems = new List<ShoppingItem>();
        }

        [DataMember]
        public string UserId { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public List<ShoppingItem> ShoppingItems { get; set; }

        [DataMember]
        public ItemList ItemList { get; set; }

        [DataMember]
        public bool IsCompleted { get; set; }
    }

}