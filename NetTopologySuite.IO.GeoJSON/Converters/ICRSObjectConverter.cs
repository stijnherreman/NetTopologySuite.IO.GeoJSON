using System;
using System.Collections.Generic;
using NetTopologySuite.CoordinateSystems;
using Newtonsoft.Json;

namespace NetTopologySuite.IO.Converters
{
    /// <summary>
    /// Converts <see cref="ICRSObject"/> objects to its JSON representation.
    /// </summary>
    [Obsolete("No longer part of the GeoJSON format specification")]
    public class ICRSObjectConverter : JsonConverter
    {
        /// <summary>
        /// Writes a coordinate reference system to its JSON representation
        /// </summary>
        /// <param name="writer">The writer</param>
        /// <param name="value">The coordinate reference system</param>
        /// <param name="serializer">The serializer</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (writer == null)
                throw new ArgumentNullException("writer");
            if (serializer == null)
                throw new ArgumentNullException("serializer");

            var crs = value as ICRSObject;
            if (crs == null)
            {
                writer.WriteToken(JsonToken.Null);
                return;
            }

            writer.WriteStartObject();
            writer.WritePropertyName("type");
            string type = Enum.GetName(typeof(CRSTypes), crs.Type);
            writer.WriteValue(type.ToLowerInvariant());
            var crsb = value as CRSBase;
            if (crsb != null)
            {
                writer.WritePropertyName("properties");
                serializer.Serialize(writer, crsb.Properties);
            }
            writer.WriteEndObject();
        }

        /// <summary>
        /// Reads a coordinate reference system from its JSON representation
        /// </summary>
        /// <param name="reader">The reader</param>
        /// <param name="objectType">The actual object type</param>
        /// <param name="existingValue">The existing value</param>
        /// <param name="serializer">The serializer</param>
        /// <returns>A coordinate reference system</returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                reader.Read();
                return null;
            }

            if (reader.TokenType != JsonToken.StartObject)
                throw new ArgumentException("Expected token '{' not found.");
            reader.Read();
            if (!(reader.TokenType == JsonToken.PropertyName && (string)reader.Value == "type"))
                throw new ArgumentException("Expected token 'type' not found.");
            reader.Read();
            if (reader.TokenType != JsonToken.String)
                throw new ArgumentException("Expected string value not found.");
            string crsType = (string)reader.Value;
            reader.Read();
            if (!(reader.TokenType == JsonToken.PropertyName && (string)reader.Value == "properties"))
                throw new ArgumentException("Expected token 'properties' not found.");
            reader.Read();
            if (reader.TokenType != JsonToken.StartObject)
                throw new ArgumentException("Expected token '{' not found.");
            var dictionary = serializer.Deserialize<Dictionary<string, object>>(reader);
            CRSBase result = null;
            switch (crsType)
            {
                case "link":
                    object href = dictionary["href"];
                    object type = dictionary["type"];
                    result = new LinkedCRS((string)href, type != null ? (string)type : "");
                    break;
                case "name":
                    object name = dictionary["name"];
                    result = new NamedCRS((string)name);
                    break;
            }
            if (reader.TokenType != JsonToken.EndObject)
                throw new ArgumentException("Expected token '}' not found.");
            reader.Read();
            if (reader.TokenType != JsonToken.EndObject)
                throw new ArgumentException("Expected token '}' not found.");
            reader.Read();
            return result;
        }

        /// <summary>
        /// Predicate function to check if an instance of <paramref name="objectType"/> can be converted using this converter.
        /// </summary>
        /// <param name="objectType">The type of the object to convert</param>
        /// <returns><value>true</value> if the conversion is possible, otherwise <value>false</value></returns>
        public override bool CanConvert(Type objectType)
        {
            return typeof(ICRSObject).IsAssignableFrom(objectType);
        }
    }
}
