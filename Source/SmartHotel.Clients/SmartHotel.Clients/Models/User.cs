
using System.Runtime.Serialization;
using System.Collections.Generic;

namespace SmartHotel.Clients.Models
{
    [DataContract]
    public class User : Resource
    {
        [DataMember]
        public string UserName { get; set; }

        [DataMember]
        public string Password { get; set; }

        [DataMember]
        public List<Item> Items { get; set; }



        [DataMember]

        public ShoppingList CurrentShoppingTrip { get; set; }


    }

}