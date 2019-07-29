﻿using System;
using System.IO;
using System.Text;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;

namespace NetTopologySuite.IO
{
    /// <summary>
    /// Represents a GeoJSON Writer allowing for serialization of various GeoJSON elements 
    /// or any object containing GeoJSON elements.
    /// </summary>
    public class GeoJsonWriter
    {
        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        public GeoJsonWriter()
        {
            SerializerSettings = new JsonSerializerSettings();
            SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
        }

        /// <summary>
        /// Gets or sets a value that is used to create and configure the underlying <see cref="GeoJsonSerializer"/>.
        /// </summary>
        public JsonSerializerSettings SerializerSettings { get; set; }

        /// <summary>
        /// Writes the specified geometry.
        /// </summary>
        /// <param name="geometry">The geometry.</param>
        /// <returns>A string representing the geometry's JSON representation</returns>
        public string Write(Geometry geometry)
        {
            if (geometry == null)
                throw new ArgumentNullException(nameof(geometry));

            var g = GeoJsonSerializer.Create(SerializerSettings, geometry.Factory);

            var sb = new StringBuilder();
            using (var sw = new StringWriter(sb))
                g.Serialize(sw, geometry);
            return sb.ToString();
        }

        /// <summary>
        /// Writes the specified feature.
        /// </summary>
        /// <param name="feature">The feature.</param>
        /// <returns>A string representing the feature's JSON representation</returns>
        public string Write(Feature feature)
        {
            if (feature == null)
                throw new ArgumentNullException(nameof(feature));

            var factory = feature.Geometry?.Factory ?? GeoJsonSerializer.Wgs84Factory;
            var g = GeoJsonSerializer.Create(SerializerSettings, factory);
            var sb = new StringBuilder();
            using (var sw = new StringWriter(sb))
                g.Serialize(sw, feature);
            return sb.ToString();
        }

        /// <summary>
        /// Writes the specified feature collection.
        /// </summary>
        /// <param name="featureCollection">The feature collection.</param>
        /// <returns>A string representing the feature collection's JSON representation</returns>
        public string Write(FeatureCollection featureCollection)
        {
            var factory = SearchForFactory(featureCollection) ?? GeoJsonSerializer.Wgs84Factory;
            var g = GeoJsonSerializer.Create(SerializerSettings, factory);
            var sb = new StringBuilder();
            using (var sw = new StringWriter(sb))
                g.Serialize(sw, featureCollection);
            return sb.ToString();
        }

        private static GeometryFactory SearchForFactory(FeatureCollection features)
        {
            if (features == null)
                return null;

            foreach (var feature in features)
            {
                if (feature.Geometry != null)
                    return feature.Geometry.Factory;
            }
            return null;
        }

        /// <summary>
        /// Writes any specified object.
        /// </summary>
        /// <param name="value">Any object.</param>
        /// <returns>A string representing the object's JSON representation</returns>
        public string Write(object value)
        {
            JsonSerializer g = GeoJsonSerializer.Create(SerializerSettings, GeoJsonSerializer.Wgs84Factory);
            StringBuilder sb = new StringBuilder();
            using (StringWriter sw = new StringWriter(sb))
                g.Serialize(sw, value);
            return sb.ToString();
        }
    }
}
