extern alias pt;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using TopNamespace.SubNameSpace;

namespace TopNamespace.SubNameSpace
{
    public class ContainingClass
    {
        public class NestedClass
        {

        }
    }

    public class GenericClass<T>
    {

    }
}

public class TopLevelContainingClass
{
    public class NestedClass
    {

    }
}

namespace MemcachedTranscoder.Tests
{
    [TestClass]
    public class TypeHelperTest
    {
        public TestContext TestContext { get; set; }

        public void RunTypeTest()
        {
            TestContext.Run((Type type, string typeName) =>
            {
                CheckType(type, typeName);
            });
        }

        public void CheckType(Type type, string typeName)
        {
            pt::MemcachedTranscoder.TypeHelper.BuildTypeName(type).Is(typeName);
            Type.GetType(typeName).Is(type);
        }

        [TestMethod]
        [TestCase(typeof(int), "System.Int32, mscorlib")]
        [TestCase(typeof(string), "System.String, mscorlib")]
        [TestCase(typeof(long), "System.Int64, mscorlib")]
        public void BasicType()
        {
            RunTypeTest();
        }

        [TestMethod]
        [TestCase(typeof(ContainingClass), "TopNamespace.SubNameSpace.ContainingClass, MemcachedTranscoder.Tests")]
        [TestCase(typeof(ContainingClass.NestedClass), "TopNamespace.SubNameSpace.ContainingClass+NestedClass, MemcachedTranscoder.Tests")]
        [TestCase(typeof(TopLevelContainingClass), "TopLevelContainingClass, MemcachedTranscoder.Tests")]
        [TestCase(typeof(TopLevelContainingClass.NestedClass), "TopLevelContainingClass+NestedClass, MemcachedTranscoder.Tests")]
        public void NestedClass()
        {
            RunTypeTest();
        }

        [TestMethod]
        [TestCase(typeof(int[]), "System.Int32[], mscorlib")]
        [TestCase(typeof(List<int>[]), "System.Collections.Generic.List`1[[System.Int32, mscorlib]][], mscorlib")]
        [TestCase(typeof(List<int[]>[]), "System.Collections.Generic.List`1[[System.Int32[], mscorlib]][], mscorlib")]
        public void Array()
        {
            RunTypeTest();
        }

        [TestMethod]
        [TestCase(typeof(Nullable<int>), "System.Nullable`1[[System.Int32, mscorlib]], mscorlib")]
        [TestCase(typeof(Dictionary<string, ContainingClass.NestedClass>), "System.Collections.Generic.Dictionary`2[[System.String, mscorlib],[TopNamespace.SubNameSpace.ContainingClass+NestedClass, MemcachedTranscoder.Tests]], mscorlib")]
        [TestCase(typeof(Tuple<string, int, ContainingClass, ContainingClass.NestedClass>), "System.Tuple`4[[System.String, mscorlib],[System.Int32, mscorlib],[TopNamespace.SubNameSpace.ContainingClass, MemcachedTranscoder.Tests],[TopNamespace.SubNameSpace.ContainingClass+NestedClass, MemcachedTranscoder.Tests]], mscorlib")]
        [TestCase(typeof(GenericClass<int>), "TopNamespace.SubNameSpace.GenericClass`1[[System.Int32, mscorlib]], MemcachedTranscoder.Tests")]
        public void Generics()
        {
            RunTypeTest();
        }

        [TestMethod]
        [TestCase(typeof(int*), "System.Int32*, mscorlib")]
        [TestCase(typeof(int**), "System.Int32**, mscorlib")]
        public void Unmanaged()
        {
            RunTypeTest();
        }

        [TestMethod]
        [TestCase(typeof(int[,]), "System.Int32[,], mscorlib")]
        [TestCase(typeof(int[, , , ,]), "System.Int32[,,,,], mscorlib")]
        [TestCase(typeof(int[][]), "System.Int32[][], mscorlib")]
        public void SpecialArray()
        {
            RunTypeTest();
            CheckType(System.Array.CreateInstance(typeof(int), new[] { 10 }, new[] { 3 }).GetType(), "System.Int32[*], mscorlib");
            CheckType(System.Array.CreateInstance(typeof(int), new[] { 10, 3 }, new[] { 3, 3 }).GetType(), "System.Int32[,], mscorlib");
            CheckType(System.Array.CreateInstance(typeof(int), new[] { 10, 3, 1 }, new[] { 3, 3, 5 }).GetType(), "System.Int32[,,], mscorlib");
        }
    }
}
