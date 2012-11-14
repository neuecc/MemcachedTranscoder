using Enyim.Caching;
using Enyim.Caching.Configuration;
using Enyim.Caching.Memcached;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.IO;

namespace MemcachedTranscoder.Tests
{
    [TestClass]
    public class Initialize
    {
        static Process memcachedProcess;

        [AssemblyInitialize]
        public static void StartMemcached(TestContext ctx)
        {
            var memcachedPath = Path.Combine(new DirectoryInfo(ctx.TestDir).Parent.Parent.FullName, "Memcached", "memcached.exe");
            memcachedProcess = Process.Start(memcachedPath);
        }

        [AssemblyCleanup]
        public static void CloseMemcached()
        {
            if (memcachedProcess != null)
            {
                memcachedProcess.Kill();
                memcachedProcess.Dispose();
            }
        }
    }

    public static class MemcachedHelper
    {
        public static MemcachedClient CreateClient(ITranscoder transcoder)
        {
            var config = new MemcachedClientConfiguration() { Protocol = MemcachedProtocol.Binary, Transcoder = transcoder };
            config.Servers.Add(new System.Net.IPEndPoint(new System.Net.IPAddress(new byte[] { 127, 0, 0, 1 }), 11211));

            return new MemcachedClient(config);
        }
    }
}