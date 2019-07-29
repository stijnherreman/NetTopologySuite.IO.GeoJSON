using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;

namespace NetTopologySuite.IO.Converters
{
    /// <summary>
    /// Converts Feature object to its JSON representation.
    /// </summary>
    public class FeatureConverter : JsonConverter
    {
        /// <summary>
        /// Gets or sets a value indicating that a feature's id property should be written to the properties block as well
        /// </summary>
        public static bool WriteIdToProperties { get; set; } = false;

        /// <summary>
        /// Writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">The <see cref="T:Newtonsoft.Json.JsonWriter"/> to write to.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (writer == null)
                throw new ArgumentNullException("writer");
            if (serializer == null)
                throw new ArgumentNullException("serializer");

            if (!(value is Feature feature))
                return;

            writer.WriteStartObject();

            // type
            writer.WritePropertyName("type");
            writer.WriteValue("Feature");

            // Add the id here if present in attributes.
            // It will be skipped in serialization of properties
            if (feature.Attributes != null && feature.Attributes.TryGetValue("id", out object id))
            {
                writer.WritePropertyName("id");
                serializer.Serialize(writer, id);
            }

            // bbox (optional)
            if (serializer.NullValueHandling == NullValueHandling.Include || feature.BoundingBox != null)
            {
                var bbox = feature.BoundingBox;
                if (bbox == null && feature.Geometry != null) bbox = feature.Geometry.EnvelopeInternal;

                writer.WritePropertyName("bbox");
                serializer.Serialize(writer, bbox, typeof(Envelope));
            }

            // geometry
            if (serializer.NullValueHandling == NullValueHandling.Include || feature.Geometry != null)
            {
                writer.WritePropertyName("geometry");
                serializer.Serialize(writer, feature.Geometry, typeof(Geometry));
            }

            // properties
            if (serializer.NullValueHandling == NullValueHandling.Include || feature.Attributes != null)
            {
                writer.WritePropertyName("properties");
                var attributes = feature.Attributes;
                if (!WriteIdToProperties && attributes.ContainsKey("id"))
                {
                    attributes = new Dictionary<string, object>(attributes);
                    attributes.Remove("id");
                }

                serializer.Serialize(writer, attributes, typeof(Dictionary<string, object>));
            }

            writer.WriteEndObject();
        }

        /// <summary>
        /// Reads the JSON representation of the object.
        /// </summary>
        /// <param name="reader">The <see cref="T:Newtonsoft.Json.JsonReader"/> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of object being read.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <returns>
        /// The object value.
        /// </returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.StartObject)
                throw new JsonReaderException("Expected Start object '{' Token");

            bool read = reader.Read();
            Utility.SkipComments(reader);

            object featureId = null;
            Feature feature = new Feature();
            while (reader.TokenType == JsonToken.PropertyName)
            {
                string prop = (string)reader.Value;
                switch (prop)
                {
                    case "type":
                        read = reader.Read();
                        if ((string)reader.Value != "Feature")
                            throw new ArgumentException("Expected value 'Feature' not found.");
                        read = reader.Read();
                        break;
                    case "id":
                        read = reader.Read();
                        featureId = reader.Value;
                        feature.Attributes["id"] = featureId;
                        read = reader.Read();
                        break;
                    case "bbox":
                        Envelope bbox = serializer.Deserialize<Envelope>(reader);
                        feature.BoundingBox = bbox;
                        //Debug.WriteLine("BBOX: {0}", bbox.ToString());
                        break;
                    case "geometry":
                        read = reader.Read();
                        if (reader.TokenType == JsonToken.Null)
                        {
                            read = reader.Read();
                            break;
                        }

                        if (reader.TokenType != JsonToken.StartObject)
                            throw new ArgumentException("Expected token '{' not found.");
                        Geometry geometry = serializer.Deserialize<Geometry>(reader);
                        feature.Geometry = geometry;
                        if (reader.TokenType != JsonToken.EndObject)
                            throw new ArgumentException("Expected token '}' not found.");
                        read = reader.Read();
                        break;
                    case "properties":
                        read = reader.Read();
                        if (reader.TokenType != JsonToken.Null)
                        {
                            // #120: ensure "properties" isn't "null"
                            if (reader.TokenType != JsonToken.StartObject)
                                throw new ArgumentException("Expected token '{' not found.");
                            foreach (var kvp in serializer.Deserialize<Dictionary<string, object>>(reader))
                            {
                                feature.Attributes[kvp.Key] = kvp.Value;
                            }
                            if (reader.TokenType != JsonToken.EndObject)
                                throw new ArgumentException("Expected token '}' not found.");
                        }
                        read = reader.Read();
                        break;
                
                    default:
                        read = reader.Read(); // move next                        
                        // jump to next property
                        if (read)
                        {
                            reader.Skip();
                        }
                        read = reader.Read();
                        break;
                }

                Utility.SkipComments(reader);
            }

            if (read && reader.TokenType != JsonToken.EndObject)
                throw new ArgumentException("Expected token '}' not found.");

            return feature;
        }

        /// <summary>
        /// Determines whether this instance can convert the specified object type.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <returns>
        ///   <c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.
        /// </returns>
        public override bool CanConvert(Type objectType)
        {
            return typeof(Feature).IsAssignableFrom(objectType);
        }
    }
}
