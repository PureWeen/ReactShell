
using System.Runtime.Serialization;
using System;

namespace Shopanizer.DataObjects
{
    [DataContract]
    public class ItemList
    {

        [DataMember]
        public string Name { get; set; }
        
        [DataMember]
		public string Description { get; set; }


        public override int GetHashCode()
        {
            return Name.ToLower().GetHashCode();
        }


        public override bool Equals(object obj)
        {
            ItemList target = obj as ItemList;

            if (target == null)
                return false;

            return target.Name.Equals(Name, StringComparison.CurrentCultureIgnoreCase);
        }
    }

}