
using System.Runtime.Serialization;
using Newtonsoft.Json;
using System.Collections.Generic;
using System;
using System.Collections;
using System.ComponentModel;

namespace SmartHotel.Clients.Models
{
    [DataContract]
    public class Item : Resource
    {
        public Item()
        {
            ItemLists = new List<ItemList>();
        }

        public Item(string userId, string name)
        {
            UserId = userId;
            Name = name;

            Id = String.Format("{0}:{1}", UserId, Name);            
            ItemLists = new List<ItemList>();
        }

        [DataMember]
        public string UserId { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public Aisle Aisle { get; set; }

        [DataMember]
        public bool Need { get; set; }

        [DataMember]
        public List<ItemList> ItemLists { get; set; }
    }

}