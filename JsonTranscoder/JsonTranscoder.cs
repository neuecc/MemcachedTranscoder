using Enyim.Caching.Memcached;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.IO;

namespace MemcachedTranscoder
{
    public class JsonTranscoder : DefaultTranscoder
    {
        static readonly ConcurrentDictionary<string, Type> readCache = new ConcurrentDictionary<string, Type>();
        static readonly ConcurrentDictionary<Type, string> writeCache = new ConcurrentDictionary<Type, string>();
        static readonly JsonSerializer jsonSerializer = new JsonSerializer();

        protected override object DeserializeObject(ArraySegment<byte> value)
        {
            using (var ms = new MemoryStream(value.Array, value.Offset, value.Count, writable: false))
            using (var tr = new StreamReader(ms))
            using (var jr = new Newtonsoft.Json.JsonTextReader(tr))
            {
                jr.Read();
                if (jr.TokenType == JsonToken.StartArray)
                {
                    // read type
                    var typeName = jr.ReadAsString();
                    var type = readCache.GetOrAdd(typeName, x => Type.GetType(x, throwOnError: true)); // Get type or Register type

                    // read object
                    jr.Read();
                    var deserializedValue = jsonSerializer.Deserialize(jr, type);

                    return deserializedValue;
                }
                else
                {
                    throw new InvalidDataException("JsonTranscoder only supports [\"TypeName\", object]");
                }
            }
        }

        protected override ArraySegment<byte> SerializeObject(object value)
        {
            var type = value.GetType();
            var typeName = writeCache.GetOrAdd(type, x => TypeHelper.BuildTypeName(x)); // Get type or Register type

            using (var ms = new MemoryStream())
            using (var tw = new StreamWriter(ms))
            using (var jw = new Newtonsoft.Json.JsonTextWriter(tw))
            {
                jw.WriteStartArray(); // [
                jw.WriteValue(typeName); // "type",
                jsonSerializer.Serialize(jw, value); // obj

                jw.WriteEndArray(); // ]

                jw.Flush();

                return new ArraySegment<byte>(ms.ToArray(), 0, (int)ms.Length);
            }
        }
    }
}