using System;
using System.Diagnostics;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.Converters;
using Newtonsoft.Json;

namespace NetTopologySuite.IO
{
    /// <summary>
    /// Json Serializer with support for GeoJson object structure.
    /// </summary>
    public class GeoJsonSerializer : JsonSerializer
    {
        /// <summary>
        /// Gets a default GeometryFactory
        /// </summary>
        internal static GeometryFactory Wgs84Factory { get; } = new GeometryFactory(new PrecisionModel(), 4326);

        /// <summary>
        /// Factory method to create a (Geo)JsonSerializer
        /// </summary>
        /// <remarks>Calls <see cref="GeoJsonSerializer.CreateDefault()"/> internally</remarks>
        /// <returns>A <see cref="JsonSerializer"/></returns>
        public new static JsonSerializer Create()
        {
            return CreateDefault();
        }


        /// <summary>
        /// Factory method to create a (Geo)JsonSerializer
        /// </summary>
        /// <remarks>
        /// <see cref="GeoJsonSerializer.Wgs84Factory"/> is used.</remarks>
        /// <returns>A <see cref="JsonSerializer"/></returns>
        public new static JsonSerializer CreateDefault()
        {
            var s = JsonSerializer.CreateDefault();
            s.NullValueHandling = NullValueHandling.Ignore;

            AddGeoJsonConverters(s, GeoJsonSerializer.Wgs84Factory, 2, 0);
            return s;
        }

        /// <summary>
        /// Factory method to create a (Geo)JsonSerializer
        /// </summary>
        /// <remarks>
        /// <see cref="GeoJsonSerializer.Wgs84Factory"/> is used.</remarks>
        /// <returns>A <see cref="JsonSerializer"/></returns>
        public new static JsonSerializer CreateDefault(JsonSerializerSettings settings)
        {
            var s = JsonSerializer.Create(settings);
            AddGeoJsonConverters(s, GeoJsonSerializer.Wgs84Factory, 2, 0);

            return s;
        }
        /// <summary>
        /// Factory method to create a (Geo)JsonSerializer
        /// </summary>
        /// <remarks>
        /// Creates a serializer using <see cref="GeoJsonSerializer.Create(GeometryFactory,int)"/> internally.
        /// </remarks>
        /// <param name="factory">A factory to use when creating geometries. The factories <see cref="PrecisionModel"/>
        /// is also used to format <see cref="Coordinate.X"/> and <see cref="Coordinate.Y"/> of the coordinates.</param>
        /// <returns>A <see cref="JsonSerializer"/></returns>
        public static JsonSerializer Create(GeometryFactory factory)
        {
            return Create(factory, 2, 0);
        }

        /// <summary>
        /// Factory method to create a (Geo)JsonSerializer
        /// </summary>
        /// <remarks>
        /// Creates a serializer using <see cref="GeoJsonSerializer.Create(JsonSerializerSettings,GeometryFactory,int)"/> internally.
        /// </remarks>
        /// <param name="factory">A factory to use when creating geometries. The factories <see cref="PrecisionModel"/>
        /// is also used to format <see cref="Coordinate.X"/> and <see cref="Coordinate.Y"/> of the coordinates.</param>
        /// <param name="dimension">A number of dimensions that are handled.  Must be non-negative and at least 2 after subtracting measures.</param>
        /// <param name="measures">A number of measures that are handled.  Must be non-negative and less than dimension.</param>
        /// <returns>A <see cref="JsonSerializer"/></returns>
        public static JsonSerializer Create(GeometryFactory factory, int dimension, int measures)
        {
            return Create(new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }, factory, dimension, measures);
        }

        /// <summary>
        /// Factory method to create a (Geo)JsonSerializer using the provider serializer settings and geometry factory
        /// </summary>
        /// <returns>A <see cref="JsonSerializer"/></returns>
        public static JsonSerializer Create(JsonSerializerSettings settings, GeometryFactory factory)
        {
            return Create(settings, factory, 2, 0);
        }

        /// <summary>
        /// Factory method to create a (Geo)JsonSerializer using the provider serializer settings and geometry factory
        /// </summary>
        /// <param name="settings">Serializer settings</param>
        /// <param name="factory">The factory to use when creating a new geometry</param>
        /// <param name="dimension">A number of dimensions that are handled.  Must be non-negative and at least 2 after subtracting measures.</param>
        /// <param name="measures">A number of measures that are handled.  Must be non-negative and less than dimension.</param>
        /// <returns>A <see cref="JsonSerializer"/></returns>
        public static JsonSerializer Create(JsonSerializerSettings settings, GeometryFactory factory, int dimension, int measures)
        {
            if (dimension < 0)
            {
                throw new ArgumentException("Must be non-negative", nameof(dimension));
            }

            if (measures < 0)
            {
                throw new ArgumentException("Must be non-negative", nameof(measures));
            }

            if (dimension - measures < 2)
            {
                throw new ArgumentException("Must have at least two spatial dimensions", nameof(dimension));
            }

            var s = JsonSerializer.Create(settings);
            AddGeoJsonConverters(s, factory, dimension, measures);
            return s;
        }

        private static void AddGeoJsonConverters(JsonSerializer s, GeometryFactory factory, int dimension, int measures)
        {
            if (factory.SRID != 4326)
                Trace.WriteLine($"Factory with SRID of unsupported coordinate reference system.");

            var c = s.Converters;
            c.Add(new FeatureCollectionConverter());
            c.Add(new FeatureConverter());
            c.Add(new GeometryConverter(factory, dimension, measures));
            c.Add(new GeometryArrayConverter(factory, dimension, measures));
            c.Add(new CoordinateConverter(factory.PrecisionModel, dimension, measures));
            c.Add(new EnvelopeConverter());

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GeoJsonSerializer"/> class.
        /// </summary>
        [Obsolete("Use GeoJsonSerializer.Create...() functions")]
        public GeoJsonSerializer() : this(Wgs84Factory) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="GeoJsonSerializer"/> class.
        /// </summary>
        /// <param name="geometryFactory">The geometry factory.</param>
        [Obsolete("Use GeoJsonSerializer.Create...() functions")]
        public GeoJsonSerializer(GeometryFactory geometryFactory)
        {
            base.Converters.Add(new FeatureCollectionConverter());
            base.Converters.Add(new FeatureConverter());
            base.Converters.Add(new GeometryConverter(geometryFactory));
            base.Converters.Add(new GeometryArrayConverter(geometryFactory));
            base.Converters.Add(new CoordinateConverter(geometryFactory.PrecisionModel, 2, 0));
            base.Converters.Add(new EnvelopeConverter());
        }
    }
}
