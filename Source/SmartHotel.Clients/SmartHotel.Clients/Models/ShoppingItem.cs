
using System.Runtime.Serialization;
using System;

namespace SmartHotel.Clients.Models
{
    [DataContract]
    public class ShoppingItem
    {
        public ShoppingItem()
        {
            
        }

        public ShoppingItem(Item item)
        {
            Procured = false;

            if (item != null)
            {
                Name = item.Name;
                Aisle = item.Aisle;
                OriginalItemId = item.Id;
            }
        }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public bool Procured { get; set; }

        [DataMember]
        public Aisle Aisle { get; set; }

        [DataMember]
        public string OriginalItemId { get; set; }

        public override int GetHashCode()
        {
            if (String.IsNullOrWhiteSpace(OriginalItemId))
                return base.GetHashCode();

            return OriginalItemId.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            ShoppingItem compareTo = obj as ShoppingItem;

            if (compareTo == null)
                return false;

            if(String.IsNullOrWhiteSpace(OriginalItemId) || String.IsNullOrWhiteSpace(compareTo.OriginalItemId))
            {
                return Object.ReferenceEquals(obj, compareTo);
            }

            return compareTo.OriginalItemId.Equals(OriginalItemId);
        }
    }

}