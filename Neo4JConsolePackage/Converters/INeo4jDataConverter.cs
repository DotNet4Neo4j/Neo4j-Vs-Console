using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anabranch.Neo4JConsolePackage.Converters
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class INeo4jDataConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (objectType != typeof(INeo4jData))
                return null;

            JObject jObject = null;

            try
            {
                //Load our object
                jObject = GetJObject(reader);
            }
            catch (JsonReaderException)
            {
                var res = new Neo4jSimpleData {Data = reader.Value.ToString()};
                return res;
            }

            var data = Deserialize<Neo4jData>(jObject.ToString());
            if (data != null)
                return data;

            var result = Deserialize<Neo4jSimpleData>(jObject.ToString());
            return result;


//            var latest = UnJsonifyProperty<SkillRating>(jObject, LatestPropertyName);
//            var historical = UnJsonifyProperty<List<SkillRating>>(jObject, HistoricalPropertyName);
//
//
//            var output = Deserialize<SkillsRating>(jObject.ToString(), c => true);
//
//            output.Latest = latest;
//            output.HistoricalSkillRatings = historical;

//            return output;

//            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(INeo4jData);
        }



        protected JObject GetJObject(JsonReader reader)
        {
            var jObject = JObject.Load(reader);

//            var data = jObject.Property("data");
//            if (data != null)
//                jObject = data.Value as JObject;

            return jObject;
        }
        protected static void JsonifyProperty(JProperty property, JObject o, string name)
        {
            o.Remove(name);
            if (property == null || property.Value == null)
                return;

            o.Add(name, property.Value.ToString());
        }

        protected static T UnJsonifyProperty<T>(JObject o, string name) where T : new()
        {
            var property = o.Property(name);
            if (property == null)
                return new T();
            var token = property.Value;
            o.Remove(name);
            T result = JsonConvert.DeserializeObject<T>(token.ToString());
            return result;
        }

        protected T Deserialize<T>(string value) where T : class
        {
            return Deserialize<T>(value, x => true);
        }

        protected T Deserialize<T>(string value, Func<T, bool> isValid) where T : class
        {
            var t = JsonConvert.DeserializeObject<T>(value);
            if (isValid(t))
                return t;

            var node = JsonConvert.DeserializeObject<T>(value);
            if (isValid(node))
                return node;

            return null;
        }
    }
}
