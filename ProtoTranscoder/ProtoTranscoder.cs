using Enyim.Caching.Memcached;
using ProtoBuf;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;

namespace MemcachedTranscoder
{
    // The idea and code based on protobuf-net.Extensions/Caching/Enyim/MemcachedTranscoders.cs(r282)
    public class ProtoTranscoder : DefaultTranscoder
    {
        static readonly ConcurrentDictionary<ArraySegment<byte>, Type> readCache = new ConcurrentDictionary<ArraySegment<byte>, Type>(new ByteSegmentComparer());
        static readonly ConcurrentDictionary<Type, byte[]> writeCache = new ConcurrentDictionary<Type, byte[]>();
        static readonly Encoding defaultEncoding = Encoding.UTF8;

        protected override object DeserializeObject(ArraySegment<byte> value)
        {
            var raw = value.Array;
            var count = value.Count;
            var offset = value.Offset;
            var type = ReadType(raw, ref offset, ref count);

            using (var ms = new MemoryStream(raw, offset, count, writable: false))
            {
                return Serializer.NonGeneric.Deserialize(type, ms);
            }
        }

        protected override ArraySegment<byte> SerializeObject(object value)
        {
            using (var ms = new MemoryStream())
            {
                WriteType(ms, value.GetType());
                Serializer.NonGeneric.Serialize(ms, value);

                return new ArraySegment<byte>(ms.ToArray(), 0, (int)ms.Length);
            }
        }

        static Type ReadType(byte[] buffer, ref int offset, ref int count)
        {
            if (count < 4) throw new EndOfStreamException();

            // len is size of header typeName(string)
            var len = (int)buffer[offset++]
                    | (buffer[offset++] << 8)
                    | (buffer[offset++] << 16)
                    | (buffer[offset++] << 24);
            count -= 4; // count is message total size, decr typeName length(int)
            if (count < len) throw new EndOfStreamException();
            var keyOffset = offset;
            offset += len; // skip typeName body size
            count -= len; // decr typeName body size

            // avoid encode string
            var key = new ArraySegment<byte>(buffer, keyOffset, len);
            Type type;
            if (!readCache.TryGetValue(key, out type))
            {
                var typeName = defaultEncoding.GetString(key.Array, key.Offset, key.Count);
                type = Type.GetType(typeName, throwOnError: true);

                // create ArraySegment has only typeName
                var cacheBuffer = new byte[key.Count];
                Buffer.BlockCopy(key.Array, key.Offset, cacheBuffer, 0, key.Count);
                key = new ArraySegment<byte>(cacheBuffer, 0, cacheBuffer.Length);
                readCache.TryAdd(key, type);
            }

            return type;
        }

        static void WriteType(MemoryStream ms, Type type)
        {
            var typeArray = writeCache.GetOrAdd(type, x =>
            {
                var typeName = TypeHelper.BuildTypeName(x);
                var buffer = defaultEncoding.GetBytes(typeName);
                return buffer;
            });

            var len = typeArray.Length;
            // BinaryWrite Int32
            ms.WriteByte((byte)len);
            ms.WriteByte((byte)(len >> 8));
            ms.WriteByte((byte)(len >> 16));
            ms.WriteByte((byte)(len >> 24));
            // BinaryWrite String
            ms.Write(typeArray, 0, len);
        }
    }
}