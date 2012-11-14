extern alias pt;
using Enyim.Caching.Memcached;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using pt::MemcachedTranscoder;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MemcachedTranscoder.Proto.Tests
{
    [Serializable]
    [DataContract]
    public class MyClass
    {
        [DataMember(Order = 1)]
        public int MyProperty { get; set; }
        [DataMember(Order = 2)]
        public string MyProperty2 { get; set; }
    }

    [Serializable]
    [DataContract]
    public class OuterClass
    {
        [DataMember(Order = 1)]
        public MyClass My1 { get; set; }
        [DataMember(Order = 2)]
        public int MyProperty { get; set; }
        [DataMember(Order = 3)]
        public MyClass My2 { get; set; }
    }

    [TestClass]
    public class TranscoderTest
    {
        public TestContext TestContext { get; set; }

        public ConcurrentDictionary<ArraySegment<byte>, Type> GetReadCache()
        {
            var readCache = (ConcurrentDictionary<ArraySegment<byte>, Type>)(typeof(ProtoTranscoder).GetField("readCache", BindingFlags.Static | BindingFlags.GetField | BindingFlags.NonPublic).GetValue(null));
            return readCache;
        }

        public ConcurrentDictionary<Type, byte[]> GetWriteCache()
        {
            var writeCache = (ConcurrentDictionary<Type, byte[]>)(typeof(ProtoTranscoder).GetField("writeCache", BindingFlags.Static | BindingFlags.GetField | BindingFlags.NonPublic).GetValue(null));
            return writeCache;
        }

        public void RunTranscodeTest<T>(T value, Action<T, T> checker)
        {
            GetReadCache().Clear();
            GetWriteCache().Clear();

            var transcoder = (ITranscoder)new ProtoTranscoder();
            var item = transcoder.Serialize(value);

            GetWriteCache().Count.Is(1);

            var clone = (T)transcoder.Deserialize(item);

            checker(value, clone);

            // read/write again
            var item2 = transcoder.Serialize(value);
            var clone2 = (T)transcoder.Deserialize(item2);
            checker(value, clone2);

            GetReadCache().Count.Is(1);

            GetReadCache().Clear();
            GetWriteCache().Clear();
        }

        [TestMethod]
        public void Collection()
        {
            RunTranscodeTest(new List<int> { 1, 2, 3 },
                (x, y) => x.SequenceEqual(y).Is(true));

            RunTranscodeTest(new int[] { 1, 2, 3 },
                (x, y) => x.SequenceEqual(y).Is(true));

            RunTranscodeTest(new Dictionary<string, MyClass> { { "huga", new MyClass { MyProperty = 5, MyProperty2 = "a" } }, { "nano", new MyClass { MyProperty = 10, MyProperty2 = "b" } } },
                (x, y) => x.Select(kvp => new { kvp.Key, kvp.Value.MyProperty, kvp.Value.MyProperty2 }).SequenceEqual(
                    y.Select(kvp => new { kvp.Key, kvp.Value.MyProperty, kvp.Value.MyProperty2 })).Is(true));
        }

        [TestMethod]
        public void Object()
        {
            RunTranscodeTest(new MyClass { MyProperty = 100, MyProperty2 = "hoge" },
                (x, y) => { x.MyProperty.Is(y.MyProperty); x.MyProperty2.Is(y.MyProperty2); });

            RunTranscodeTest(Tuple.Create("huga", 100, new MyClass { MyProperty = 100, MyProperty2 = "hoge" }),
                (x, y) =>
                {
                    x.Item1.Is(y.Item1); x.Item2.Is(y.Item2);
                    x.Item3.MyProperty.Is(y.Item3.MyProperty);
                    x.Item3.MyProperty2.Is(y.Item3.MyProperty2);
                });

            RunTranscodeTest(new OuterClass { My1 = new MyClass { MyProperty = 100, MyProperty2 = "a" }, My2 = new MyClass { MyProperty = 1000, MyProperty2 = "b" }, MyProperty = 10000 },
                (x, y) =>
                {
                    x.MyProperty.Is(y.MyProperty);
                    x.My1.MyProperty.Is(y.My1.MyProperty);
                    x.My1.MyProperty2.Is(y.My1.MyProperty2);
                    x.My2.MyProperty.Is(y.My2.MyProperty);
                    x.My2.MyProperty2.Is(y.My2.MyProperty2);
                });
        }

        // to, ieruhodo, taisita, test ja, nai de su....
        [TestMethod]
        public void Concurrency()
        {
            var transcoder = (ITranscoder)new ProtoTranscoder();
            var value = new MyClass { MyProperty = 1000, MyProperty2 = "hugahuga" };

            Parallel.For(0, 10000, new ParallelOptions { MaxDegreeOfParallelism = 100 }, _ =>
            {
                var item = transcoder.Serialize(value);
                var obj = (MyClass)transcoder.Deserialize(item);
                value.MyProperty.Is(obj.MyProperty);
                value.MyProperty2.Is(obj.MyProperty2);
            });
        }
    }
}