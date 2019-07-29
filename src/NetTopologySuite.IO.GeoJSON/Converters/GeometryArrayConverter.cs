using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NetTopologySuite.IO.Converters
{
    /// <summary>
    /// Converts an array of <see cref="Geometry"/>s to and from JSON
    /// </summary>
    public class GeometryArrayConverter : JsonConverter
    {
        private readonly GeometryFactory _factory;
        private readonly int _dimension;
        private readonly int _measures;

        /// <summary>
        /// Creates an instance of this class using <see cref="GeoJsonSerializer.Wgs84Factory"/>
        /// </summary>
        public GeometryArrayConverter() : this(GeoJsonSerializer.Wgs84Factory) { }

        /// <summary>
        /// Creates an instance of this class using the provided <see cref="GeometryFactory"/>
        /// </summary>
        /// <param name="factory">The factory</param>
        public GeometryArrayConverter(GeometryFactory factory)
            : this(factory, 2, 0)
        {
        }

        /// <summary>
        /// Creates an instance of this class using the provided <see cref="GeometryFactory"/>
        /// </summary>
        /// <param name="factory">The factory</param>
        /// <param name="dimension">The number of dimensions to handle</param>
        /// <param name="measures">The number of dimensions to handle</param>
        public GeometryArrayConverter(GeometryFactory factory, int dimension, int measures)
        {
            _factory = factory;
            _dimension = dimension;
            _measures = measures;
        }

        /// <summary>
        /// Writes an array of <see cref="Geometry"/>s to JSON
        /// </summary>
        /// <param name="writer">The writer</param>
        /// <param name="value">The geometry</param>
        /// <param name="serializer">The serializer</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            //moved to GeometryConverter:
            //writer.WritePropertyName("geometries");
            WriteGeometries(writer, (IEnumerable<Geometry>)value, serializer);
        }

        private static void WriteGeometries(JsonWriter writer, IEnumerable<Geometry> geometries, JsonSerializer serializer)
        {
            writer.WriteStartArray();
            foreach (var geometry in geometries)
                serializer.Serialize(writer, geometry);
            writer.WriteEndArray();
        }

        /// <summary>
        /// Reads an array of <see cref="Geometry"/>s from JSON
        /// </summary>
        /// <param name="reader">The reader</param>
        /// <param name="objectType">The object type</param>
        /// <param name="existingValue">The existing value</param>
        /// <param name="serializer">The serializer</param>
        /// <returns>The geometry array read</returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            //reader.Read();
            //if (!(reader.TokenType == JsonToken.PropertyName && (string)reader.Value == "geometries"))
            //    throw new Exception();
            reader.Read();
            if (reader.TokenType != JsonToken.StartArray)
                throw new Exception();

            reader.Read();
            var geoms = new List<Geometry>();
            while (reader.TokenType != JsonToken.EndArray)
            {
                var obj = (JObject)serializer.Deserialize(reader);
                var geometryType = (GeoJsonObjectType)Enum.Parse(typeof(GeoJsonObjectType), obj.Value<string>("type"), true);

                switch (geometryType)
                {
                    case GeoJsonObjectType.Point:
                        geoms.Add(_factory.CreatePoint(ToCoordinate(obj.Value<JArray>("coordinates"))));
                        break;
                    case GeoJsonObjectType.LineString:
                        geoms.Add(_factory.CreateLineString(ToCoordinates(obj.Value<JArray>("coordinates"))));
                        break;
                    case GeoJsonObjectType.Polygon:
                        geoms.Add(CreatePolygon(ToListOfCoordinates(obj.Value<JArray>("coordinates"))));
                        break;
                    case GeoJsonObjectType.MultiPoint:
                        geoms.Add(_factory.CreateMultiPointFromCoords(ToCoordinates(obj.Value<JArray>("coordinates"))));
                        break;
                    case GeoJsonObjectType.MultiLineString:
                        geoms.Add(CreateMultiLineString(ToListOfCoordinates(obj.Value<JArray>("coordinates"))));
                        break;
                    case GeoJsonObjectType.MultiPolygon:
                        geoms.Add(CreateMultiPolygon(ToListOfListOfCoordinates(obj.Value<JArray>("coordinates"))));
                        break;
                    case GeoJsonObjectType.GeometryCollection:
                        throw new NotSupportedException();

                }
                reader.Read();
            }
            return geoms;
        }

        /// <summary>
        /// Predicate function to check if an instance of <paramref name="objectType"/> can be converted using this converter.
        /// </summary>
        /// <param name="objectType">The type of the object to convert</param>
        /// <returns><value>true</value> if the conversion is possible, otherwise <value>false</value></returns>
        public override bool CanConvert(Type objectType)
        {
            return typeof(IEnumerable<Geometry>).IsAssignableFrom(objectType);
        }

        private MultiLineString CreateMultiLineString(List<Coordinate[]> coordinates)
        {
            var strings = new LineString[coordinates.Count];
            for (int i = 0; i < coordinates.Count; i++)
                strings[i] = _factory.CreateLineString(coordinates[i]);
            return _factory.CreateMultiLineString(strings);
        }

        private Polygon CreatePolygon(List<Coordinate[]> coordinates)
        {
            var shell = _factory.CreateLinearRing(coordinates[0]);
            var rings = new LinearRing[coordinates.Count - 1];
            for (int i = 1; i < coordinates.Count; i++)
                rings[i - 1] = _factory.CreateLinearRing(coordinates[i]);
            return _factory.CreatePolygon(shell, rings);
        }

        private MultiPolygon CreateMultiPolygon(List<List<Coordinate[]>> coordinates)
        {
            var polygons = new Polygon[coordinates.Count];
            for (int i = 0; i < coordinates.Count; i++)
                polygons[i] = CreatePolygon(coordinates[i]);
            return _factory.CreateMultiPolygon(polygons);
        }

        private Coordinate ToCoordinate(JArray array)
        {
            var c = Coordinates.Create(_dimension, _measures);
            c.X = _factory.PrecisionModel.MakePrecise(Convert.ToDouble(array[0]));
            c.Y = _factory.PrecisionModel.MakePrecise(Convert.ToDouble(array[1]));

            for (int i = 2; i < _dimension && i < array.Count; i++)
            {
                c[i] = Convert.ToDouble(array[i]);
            }

            return c;
        }

        private Coordinate[] ToCoordinates(JArray array)
        {
            var c = new Coordinate[array.Count];
            for (int i = 0; i < array.Count; i++)
                c[i] = ToCoordinate((JArray)array[i]);
            return c;
        }
        private List<Coordinate[]> ToListOfCoordinates(JArray array)
        {
            var c = new List<Coordinate[]>();
            for (int i = 0; i < array.Count; i++)
                c.Add(ToCoordinates((JArray)array[i]));
            return c;
        }
        private List<List<Coordinate[]>> ToListOfListOfCoordinates(JArray array)
        {
            var c = new List<List<Coordinate[]>>();
            for (int i = 0; i < array.Count; i++)
                c.Add(ToListOfCoordinates((JArray)array[i]));
            return c;
        }
    }
}
