using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Text;
using System.IO;

namespace NetTopologySuite.IO.GeoJSON.Test
{
    ///<summary>
    ///    This is a test class for GeoJsonWriterTest and is intended
    ///    to contain all GeoJsonWriterTest Unit Tests
    ///</summary>
    [TestFixture]
    public class GeoJsonWriterTest
    {
        ///<summary>
        ///    A test for GeoJsonWriter Write method
        ///</summary>
        [Test]
        public void GeoJsonWriterWriteFeatureCollectionTest()
        {
            Dictionary<string, object> attributes = new Dictionary<string, object>();
            attributes.Add("test1", "value1");
            Feature feature = new Feature(new Point(23, 56), attributes);
            FeatureCollection featureCollection = new FeatureCollection { feature };
            var gjw = new GeoJsonWriter();
            gjw.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            string actual = gjw.Write(featureCollection);
            Assert.AreEqual("{\"type\":\"FeatureCollection\",\"features\":[{\"type\":\"Feature\",\"geometry\":{\"type\":\"Point\",\"coordinates\":[23.0,56.0]},\"properties\":{\"test1\":\"value1\"}}]}", actual);
        }

        ///<summary>
        ///    A test for GeoJsonWriter Write method
        ///</summary>
        [Test]
        public void GeoJsonWriterWriteFeatureTest()
        {
            Dictionary<string, object> attributes = new Dictionary<string, object>();
            attributes.Add("test1", "value1");
            Feature feature = new Feature(new Point(23, 56), attributes);
            string actual = new GeoJsonWriter().Write(feature);
            Assert.AreEqual("{\"type\":\"Feature\",\"geometry\":{\"type\":\"Point\",\"coordinates\":[23.0,56.0]},\"properties\":{\"test1\":\"value1\"}}", actual);
        }

        ///<summary>
        ///    A test for GeoJsonWriter Write method
        ///</summary>
        [Test]
        public void GeoJsonWriterWriteGeometryTest()
        {
            string actual = new GeoJsonWriter().Write(new Point(23, 56));
            Assert.AreEqual("{\"type\":\"Point\",\"coordinates\":[23.0,56.0]}", actual);
        }

        ///<summary>
        ///    A test for GeoJsonWriter Write method
        ///</summary>
        [Test]
        public void GeoJsonWriterWriteAttributesTest()
        {
            Dictionary<string, object> attributes = new Dictionary<string, object>();
            attributes.Add("test1", "value1");
            string actual = new GeoJsonWriter().Write(attributes);
            Assert.AreEqual("{\"test1\":\"value1\"}", actual);
        }

        ///<summary>
        ///    A test for GeoJsonWriter Write method
        ///</summary>
        [Test]
        public void GeoJsonWriterWriteAnyObjectTest()
        {
            Dictionary<string, object> attributes = new Dictionary<string, object>();
            DateTime Date = new DateTime(2012, 8, 8).Date;

            JsonSerializer g = GeoJsonSerializer.CreateDefault();
            StringBuilder sb = new StringBuilder();
            using (StringWriter sw = new StringWriter(sb))
                g.Serialize(sw, Date);
            string expectedDateString = sb.ToString();

            string expectedResult = "{\"featureCollection\":{\"type\":\"FeatureCollection\",\"features\":[{\"type\":\"Feature\",\"geometry\":{\"type\":\"Point\",\"coordinates\":[23.0,56.0]},\"properties\":{\"test1\":\"value1\"}}]},\"Date\":" + expectedDateString + "}";
            attributes.Add("test1", "value1");
            Feature feature = new Feature(new Point(23, 56), attributes);

            FeatureCollection featureCollection = new FeatureCollection { feature };
            var gjw = new GeoJsonWriter();
            gjw.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            string actual = gjw.Write(new { featureCollection, Date = Date });
            Assert.AreEqual(expectedResult, actual);
        }
    }
}
