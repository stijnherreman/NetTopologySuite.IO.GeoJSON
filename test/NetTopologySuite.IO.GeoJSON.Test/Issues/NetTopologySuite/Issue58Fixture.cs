﻿using NetTopologySuite.Features;
using NUnit.Framework;

namespace NetTopologySuite.IO.GeoJSON.Test.Issues.NetTopologySuite
{
    [NtsIssueNumber(58)]
    [Category("GitHub Issue")]
    [TestFixture]
    public class Issue58Fixture
    {
        [Test]
        public void geojson_should_serialize_nested_objects()
        {
            const string json = @"
{
  ""type"": ""FeatureCollection"",
  ""features"": [
    {
      ""type"": ""Feature"",
      ""geometry"": {
        ""type"": ""Point"",
        ""coordinates"": [
          1.0,
          2.0
        ]
      },
      ""properties"": {
        ""foo"": {
          ""bar"": ""xyz""
        }
      }
    }
  ]
}
";
            GeoJsonReader reader = new GeoJsonReader();
            FeatureCollection coll = reader.Read<FeatureCollection>(json);
            Assert.IsNotNull(coll);

            GeoJsonWriter writer = new GeoJsonWriter();
            string s = writer.Write(coll);
            Assert.IsNotNull(s);
        }
    }
}
