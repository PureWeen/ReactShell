using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using ReactiveUI;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Globalization;

namespace Shopanizer.DataObjects
{
    [DataContract]
    public class Resource : ReactiveObject
    {
        [DataMember(Name = "id")]     
        public virtual string Id
        {
            get;
            set; 
        }

        [DataMember(Name = "_rid")]
        public virtual string ResourceId
        {
            get;
            set; 
        }


        [DataMember(Name = "_self")]
        public string SelfLink
        {
            get;
            set; 
        }


        

        [JsonConverter(typeof(UnixDateTimeConverter))]
        [DataMember(Name = "_ts")]
        public DateTime Timestamp
        {
            get;
            set; 
        }


        [DataMember(Name = "_etag")]
        public string ETag
        {
            get; set; 
        }

        public override int GetHashCode()
        {            
            return Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            var resource = (obj as Resource);

            if (!obj.GetType().Equals(this.GetType()))
                return false;

            return resource.Id.Equals(Id);
        }


        public bool IsError { get { return !String.IsNullOrWhiteSpace(ServerErrorMessage); } }
        public string ServerErrorMessage { get; set; }
    }


    internal sealed class UnixDateTimeConverter : DateTimeConverterBase
    {
        private static DateTime UnixStartTime;

        static UnixDateTimeConverter()
        {
            UnixDateTimeConverter.UnixStartTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        }

        public UnixDateTimeConverter()
        {
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.Integer)
            {
                throw new Exception("RMResources.DateTimeConverterInvalidReaderValue");
            }
            double num = 0;
            try
            {
                num = Convert.ToDouble(reader.Value, CultureInfo.InvariantCulture);
            }
            catch
            {
                throw new Exception("RMResources.DateTimeConveterInvalidReaderDoubleValue");
            }
            return UnixDateTimeConverter.UnixStartTime.AddSeconds(num);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (!(value is DateTime))
            {
                throw new ArgumentException("RMResources.DateTimeConverterInvalidDateTime", "value");
            }
            TimeSpan timeSpan = (DateTime)value - UnixDateTimeConverter.UnixStartTime;
            writer.WriteValue((long)timeSpan.TotalSeconds);
        }
    }
}
