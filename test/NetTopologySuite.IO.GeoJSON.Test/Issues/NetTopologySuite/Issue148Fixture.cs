using System;
using System.IO;
using System.Text;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;
using NUnit.Framework;

namespace NetTopologySuite.IO.GeoJSON.Test.Issues.NetTopologySuite
{
    [NtsIssueNumber(148)]
    [Category("GitHub Issue")]
    [TestFixture]
    public class Issue148Fixture
    {
        readonly GeometryFactory factory = GeometryFactory.Default;

        private static Geometry[] geometries;
        private static GeometryCollection collection;
        private static string serializedGeometries;
        private static string serializedCollection;

        [SetUp]
        public void SetUp()
        {
            Point point = factory.CreatePoint(new Coordinate(1, 1));
            LineString linestring = factory.CreateLineString(new[] { new Coordinate(1, 1), new Coordinate(2, 2), new Coordinate(3, 3), });
            LinearRing shell = factory.CreateLinearRing(new[] { new Coordinate(1, 1), new Coordinate(2, 2), new Coordinate(3, 3), new Coordinate(1, 1) });
            Polygon polygon = factory.CreatePolygon(shell);
            geometries = new Geometry[] { point, linestring, polygon };
            serializedGeometries = //"\"geometries\":[{\"type\":\"Point\",\"coordinates\":[1.0,1.0]},{\"type\":\"LineString\",\"coordinates\":[[1.0,1.0],[2.0,2.0],[3.0,3.0]]},{\"type\":\"Polygon\",\"coordinates\":[[[1.0,1.0],[2.0,2.0],[3.0,3.0],[1.0,1.0]]]}]";
                "[{\"type\":\"Point\",\"coordinates\":[1.0,1.0]},{\"type\":\"LineString\",\"coordinates\":[[1.0,1.0],[2.0,2.0],[3.0,3.0]]},{\"type\":\"Polygon\",\"coordinates\":[[[1.0,1.0],[2.0,2.0],[3.0,3.0],[1.0,1.0]]]}]";

            collection = factory.CreateGeometryCollection(geometries);
            serializedCollection = "{\"type\":\"GeometryCollection\",\"geometries\":[{\"type\":\"Point\",\"coordinates\":[1.0,1.0]},{\"type\":\"LineString\",\"coordinates\":[[1.0,1.0],[2.0,2.0],[3.0,3.0]]},{\"type\":\"Polygon\",\"coordinates\":[[[1.0,1.0],[2.0,2.0],[3.0,3.0],[1.0,1.0]]]}]}";
        }

        [Test]
        public void serialize_an_array_of_geometries_should_return_a_json_fragment()
        {
            StringBuilder sb = new StringBuilder();
            JsonTextWriter writer = new JsonTextWriter(new StringWriter(sb));
            JsonSerializer serializer = GeoJsonSerializer.Create(new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }, factory);
            serializer.Serialize(writer, geometries);
            string actual = sb.ToString();
            Console.WriteLine(actual);
            Assert.That(actual, Is.EqualTo(serializedGeometries));
        }

        [Test]
        public void serialize_a_geometrycollection_should_return_a_valid_json()
        {
            StringBuilder sb = new StringBuilder();
            JsonTextWriter writer = new JsonTextWriter(new StringWriter(sb));
            JsonSerializer serializer = GeoJsonSerializer.Create();
            serializer.Serialize(writer, collection);
            string actual = sb.ToString();
            Console.WriteLine(actual);
            Assert.That(actual, Is.EqualTo(serializedCollection));
        }

        [Test, Ignore("Behavior changed")]
        public void deserialize_a_json_fragment_should_throws_an_error()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader(serializedGeometries));
            JsonSerializer serializer = GeoJsonSerializer.Create(factory);
            Assert.Throws<JsonReaderException>(() => serializer.Deserialize<Geometry[]>(reader));
        }

        [Test]
        public void deserialize_a_valid_json_should_return_a_geometrycollection()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader(serializedCollection));
            JsonSerializer serializer = GeoJsonSerializer.Create(factory);
            GeometryCollection actual = serializer.Deserialize<GeometryCollection>(reader);
            Assert.That(actual, Is.Not.Null);
            Assert.That(actual.EqualsExact(collection), Is.True);
        }

        [Test]
        public void howto_serialize_geometries()
        {
            GeoJsonWriter writer = new GeoJsonWriter();
            string actual = writer.Write(collection);
            Assert.That(actual, Is.Not.Null);
            Assert.That(actual, Is.EqualTo(serializedCollection));
        }

        [Test]
        public void howto_deserialize_geometries()
        {
            GeoJsonReader reader = new GeoJsonReader();
            Geometry actual = reader.Read<GeometryCollection>(serializedCollection);
            Assert.That(actual, Is.Not.Null);
            Assert.That(actual.EqualsExact(collection), Is.True);
        }
    }
}
