﻿using NetTopologySuite.Features;
using NUnit.Framework;

namespace NetTopologySuite.IO.GeoJSON.Test.Issues.NetTopologySuite
{
    /// <summary>
    /// see: https://github.com/NetTopologySuite/NetTopologySuite/issues/32
    /// </summary>
    [NtsIssueNumber(32)]
    [Category("GitHub Issue")]
    [TestFixture]
    public class Issue32Fixture
    {
        [Test]
        public void geojson_feature_with_fid_should_be_serialized()
        {
            const string json = @"
{
	""type"" : ""FeatureCollection"",
	""totalFeatures"" : 5,
	""features"" : [{
			""type"" : ""Feature"",
			""id"" : ""vfs_kommun.256"",
			""geometry"" : null,
			""properties"" : {
				""kommunnamn"" : ""Karlskrona""
			}
		}
	]
}
";
            GeoJsonReader reader = new GeoJsonReader();
            FeatureCollection coll = reader.Read<FeatureCollection>(json);
            Assert.That(coll, Is.Not.Null);
            Assert.That(coll.Count, Is.EqualTo(1));

            Feature feature = coll[0];
            Assert.That(feature.Geometry, Is.Null);
            Assert.That(feature.Attributes, Is.Not.Null);
            Assert.That(feature.Attributes.TryGetValue("id", out object val));
            Assert.That(val, Is.EqualTo("vfs_kommun.256"));
        }

        [Test]
        public void geojson_feature_with_null_crs_should_be_serialized()
        {
            const string json = @"
{
	""type"" : ""FeatureCollection"",
	""totalFeatures"" : 5,
	""features"" : [{
			""type"" : ""Feature"",
			""geometry"" : null,
			""properties"" : {
				""kommunnamn"" : ""Karlskrona""
			}
		}
	],
	""crs"" : null
}
";
            GeoJsonReader reader = new GeoJsonReader();
            FeatureCollection coll = reader.Read<FeatureCollection>(json);
            Assert.That(coll, Is.Not.Null);
            Assert.That(coll.Count, Is.EqualTo(1));
        }

        [Test]
        public void geojson_feature_with_both_id_and_null_crs_should_be_serialized()
        {
            const string json = @"
{
	""type"" : ""FeatureCollection"",
	""totalFeatures"" : 5,
	""features"" : [{
			""type"" : ""Feature"",
			""id"" : ""vfs_kommun.256"",
			""geometry"" : null,
			""properties"" : {
				""kommunnamn"" : ""Karlskrona""
			}
		}
	],
	""crs"" : null
}
";
            GeoJsonReader reader = new GeoJsonReader();
            FeatureCollection coll = reader.Read<FeatureCollection>(json);
            Assert.That(coll, Is.Not.Null);
            Assert.That(coll.Count, Is.EqualTo(1));

            Feature feature = coll[0];
            Assert.That(feature.Geometry, Is.Null);
            Assert.That(feature.Attributes, Is.Not.Null);
            Assert.That(feature.Attributes.TryGetValue("id", out object val));
            Assert.That(val, Is.EqualTo("vfs_kommun.256"));
        }

        [Test]
        public void geojson_feature_without_additional_fields_should_be_serialized()
        {
            const string json = @"
{
	""type"" : ""FeatureCollection"",
	""totalFeatures"" : 1,
	""features"" : [{
			""type"" : ""Feature"",
			""id"" : ""Road.1249"",
			""geometry"" : {
				""type"" : ""MultiLineString"",
				""coordinates"" : [[[1610822.0864005657, 7249990.524654245], [1610819.2585326964, 7250021.630828278], [1611733.0938508697, 7250797.447679726]]]
			},
			""properties"" : {
				""road_id"" : 173922
			}
		}
	],
	""crs"" : {
		""type"" : ""name"",
		""properties"" : {
			""name"" : ""urn:ogc:def:crs:EPSG::2400""
		}
	}
}
";
            GeoJsonReader reader = new GeoJsonReader();
            FeatureCollection coll = reader.Read<FeatureCollection>(json);
            Assert.That(coll, Is.Not.Null);
            Assert.That(coll.Count, Is.EqualTo(1));

            Feature feature = coll[0];
            Assert.That(feature.Geometry, Is.Not.Null);
            Assert.That(feature.Attributes, Is.Not.Null);
            Assert.That(feature.Attributes.TryGetValue("id", out object val));
            Assert.That(val, Is.EqualTo("Road.1249"));
            Assert.That(feature.Attributes.TryGetValue("road_id", out val));
            Assert.That(val, Is.EqualTo(173922));
        }

        [Test]
        public void geojson_feature_with_additional_fields_should_be_serialized()
        {
            const string json = @"
{
	""type"" : ""FeatureCollection"",
	""totalFeatures"" : 1,
	""features"" : [{
			""type"" : ""Feature"",
			""id"" : ""Road.1249"",
			""geometry"" : {
				""type"" : ""MultiLineString"",
				""coordinates"" : [[[1610822.0864005657, 7249990.524654245], [1610819.2585326964, 7250021.630828278], [1611733.0938508697, 7250797.447679726]]]
			},
			""geometry_name"" : ""geom"",
			""properties"" : {
				""road_id"" : 173922
			}
		}
	],
	""crs"" : {
		""type"" : ""name"",
		""properties"" : {
			""name"" : ""urn:ogc:def:crs:EPSG::2400""
		}
	}
}
";
            GeoJsonReader reader = new GeoJsonReader();
            FeatureCollection coll = reader.Read<FeatureCollection>(json);
            Assert.That(coll, Is.Not.Null);
            Assert.That(coll.Count, Is.EqualTo(1));

            Feature feature = coll[0];
            Assert.That(feature.Geometry, Is.Not.Null);
            Assert.That(feature.Attributes, Is.Not.Null);
            Assert.That(feature.Attributes.TryGetValue("id", out object val));
            Assert.That(val, Is.EqualTo("Road.1249"));
            Assert.That(feature.Attributes.TryGetValue("road_id", out val));
            Assert.That(val, Is.EqualTo(173922));
            Assert.That(!feature.Attributes.ContainsKey("geometry_name"));
        }
    }
}
