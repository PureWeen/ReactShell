
using System.Runtime.Serialization;
using System;

namespace Shopanizer.DataObjects
{
    [DataContract]
	public class Aisle
    {
        [DataMember]
		public string Name { get; set; }


        public override int GetHashCode()
        {
            return Name.ToLower().GetHashCode();
        }

        public override bool Equals(object obj)
        {
            Aisle thing = obj as Aisle;

            if (thing == null)
                return false;

            return Name.Equals(thing.Name, StringComparison.CurrentCultureIgnoreCase);
        }
    }

}