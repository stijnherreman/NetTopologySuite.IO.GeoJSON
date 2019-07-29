﻿using System.IO;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;

namespace NetTopologySuite.IO
{
    /// <summary>
    /// Represents a GeoJSON Reader allowing for deserialization of various GeoJSON elements 
    /// or any object containing GeoJSON elements.
    /// </summary>
    public class GeoJsonReader
    {
        private readonly GeometryFactory _factory;
        private readonly JsonSerializerSettings _serializerSettings;
        private readonly int _dimension;
        private readonly int _measures;

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        public GeoJsonReader()
            : this(GeoJsonSerializer.Wgs84Factory, new JsonSerializerSettings())
        {
        }

        /// <summary>
        /// Creates an instance of this class using the provided <see cref="GeometryFactory"/> and
        /// <see cref="JsonSerializerSettings"/>.
        /// </summary>
        /// <param name="factory">The factory to use when creating geometries</param>
        /// <param name="serializerSettings">The serializer setting</param>
        public GeoJsonReader(GeometryFactory factory, JsonSerializerSettings serializerSettings)
            : this(factory, serializerSettings, 2, 0)
        {
        }

        /// <summary>
        /// Creates an instance of this class using the provided <see cref="GeometryFactory"/> and
        /// <see cref="JsonSerializerSettings"/>.
        /// </summary>
        /// <param name="factory">The factory to use when creating geometries</param>
        /// <param name="serializerSettings">The serializer setting</param>
        /// <param name="dimension">The number of dimensions to handle</param>
        /// <param name="measures">The number of measures to handle</param>
        public GeoJsonReader(GeometryFactory factory, JsonSerializerSettings serializerSettings, int dimension, int measures)
        {
            _factory = factory;
            _serializerSettings = serializerSettings;
            _dimension = dimension;
            _measures = measures;
        }

        /// <summary>
        /// Reads the specified json.
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <param name="json">The json.</param>
        /// <returns></returns>
        public TObject Read<TObject>(string json)
            where TObject : class
        {
            var g = GeoJsonSerializer.Create(_serializerSettings, _factory, _dimension, _measures);
            using (StringReader sr = new StringReader(json))
            {
                return g.Deserialize<TObject>(new JsonTextReader(sr));
            }
        }
    }
}
