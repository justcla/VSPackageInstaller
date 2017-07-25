namespace VSPackageInstaller.UnitTests
{
    using System;
    using System.IO;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using VSPackageInstaller.Cache;
    using VSPackageInstaller.MarketplaceService;

    [TestClass]
    public sealed class CacheIntegrationTests
    {
        public string CacheFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        [TestMethod]
        public void HorriblyBadVeryNaiveSortOfIntegrationTest()
        {
            var dataService = new MarketplaceDataService();
            var cacheManager = new Cache.CacheManager<IExtensionDataItemView, ExtensionDataItem>(CacheFilePath);

            dataService.GetMarketplaceDataItems(
                "15.0",
                new[] { "Pro", "Ultimate" },
                DateTime.MinValue,
                (items) =>
                {
                    cacheManager.AddRange(items);
                    return true;
                });

            // Hit the back-end and check for actual results and write them to disk.
            // Without knowing what to expect, we can't validate these (other than by hand)
            // but a non-zero list is a good sign :)
            Assert.IsTrue(cacheManager.Snapshot.Count > 500);
            var itemsCount = cacheManager.Snapshot.Count;
            Assert.IsTrue(cacheManager.TrySaveCacheFile());
            cacheManager.ReplaceAll(Enumerable.Empty<IExtensionDataItemView>());
            Assert.IsTrue(cacheManager.Snapshot.Count == 0);
            Assert.IsTrue(cacheManager.TryLoadCacheFile());
            Assert.AreEqual(itemsCount, cacheManager.Snapshot.Count);
        }
    }
}
