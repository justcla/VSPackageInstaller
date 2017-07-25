namespace VSPackageInstaller.UnitTests
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using VSPackageInstaller.Cache;

    [TestClass]
    public class CacheManagerTests
    {
        public string CacheFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Constructor_NullCacheFilePath()
        {
            new CacheManager<int, int>(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Constructor_WhiteSpaceCacheFilePath()
        {
            new CacheManager<int, int>("  ");
        }

        [TestMethod]
        public void Constructor_InitiallyEmpty()
        {
            var manager = new CacheManager<int, int>(CacheFilePath);

            Assert.IsFalse(manager.CacheFileExists);
            Assert.IsNull(manager.LastCacheFileUpdateTimeStamp);
            Assert.IsTrue(DateTime.UtcNow.Subtract(manager.LastUpdateTimeStamp.Value).TotalMilliseconds < 100);
            Assert.IsNotNull(manager.Snapshot);
            Assert.AreEqual(0, manager.Snapshot.Count);
        }

        [TestMethod]
        public void CacheFilePath_ResolvesToFullPath()
        {
            var manager = new CacheManager<int, int>("foofile");

            Assert.IsTrue(manager.CacheFilePath.Split(Path.DirectorySeparatorChar).Length > 1);
        }

        [TestMethod]
        public void CacheFileExists_LastUpdated_CorrectResults()
        {
            var manager = new CacheManager<int, int>(CacheFilePath);

            Assert.IsFalse(File.Exists(CacheFilePath));
            Assert.IsFalse(manager.CacheFileExists);
            Assert.IsNull(manager.LastCacheFileUpdateTimeStamp);
            Assert.IsTrue(DateTime.UtcNow.Subtract(manager.LastUpdateTimeStamp.Value).TotalMilliseconds < 20);

            Thread.Sleep(1000);

            Assert.IsTrue(DateTime.UtcNow.Subtract(manager.LastUpdateTimeStamp.Value).TotalMilliseconds >= 1000);

            File.WriteAllText(CacheFilePath, string.Empty);

            Assert.IsTrue(File.Exists(CacheFilePath));
            Assert.IsTrue(manager.CacheFileExists);
            Assert.IsTrue(DateTime.UtcNow.Subtract(manager.LastCacheFileUpdateTimeStamp.Value).TotalMilliseconds < 20);
            Assert.IsTrue(DateTime.UtcNow.Subtract(manager.LastUpdateTimeStamp.Value).TotalMilliseconds < 20);

            Thread.Sleep(1000);

            Assert.IsTrue(DateTime.UtcNow.Subtract(manager.LastUpdateTimeStamp.Value).TotalMilliseconds >= 1000);

            File.Delete(CacheFilePath);

            Assert.IsFalse(File.Exists(CacheFilePath));
            Assert.IsFalse(manager.CacheFileExists);
            Assert.IsNull(manager.LastCacheFileUpdateTimeStamp);
            Assert.IsTrue(DateTime.UtcNow.Subtract(manager.LastUpdateTimeStamp.Value).TotalMilliseconds >= 2000);

            manager.AddRange(new[] { 3 });

            Assert.IsTrue(DateTime.UtcNow.Subtract(manager.LastUpdateTimeStamp.Value).TotalMilliseconds < 20);

            File.WriteAllText(CacheFilePath, string.Empty);

            Assert.IsTrue(DateTime.UtcNow.Subtract(manager.LastUpdateTimeStamp.Value).TotalMilliseconds < 20);
        }

        [TestMethod]
        public void AddRange_Create_CorrectResults()
        {
            var manager = new CacheManager<int, int>(CacheFilePath);

            Assert.AreEqual(0, manager.Snapshot.Count);

            manager.AddRange(new[] { 3, 2, 1 });

            Assert.AreEqual(3, manager.Snapshot.Count);
            Assert.AreEqual(3, manager.Snapshot[0]);
            Assert.AreEqual(2, manager.Snapshot[1]);
            Assert.AreEqual(1, manager.Snapshot[2]);

            manager.AddRange(new[] { 7, 8, 9 });

            Assert.AreEqual(6, manager.Snapshot.Count);
            Assert.AreEqual(3, manager.Snapshot[0]);
            Assert.AreEqual(2, manager.Snapshot[1]);
            Assert.AreEqual(1, manager.Snapshot[2]);
            Assert.AreEqual(7, manager.Snapshot[3]);
            Assert.AreEqual(8, manager.Snapshot[4]);
            Assert.AreEqual(9, manager.Snapshot[5]);

            manager.ReplaceAll(new[] { 4, 5, 6 });

            Assert.AreEqual(3, manager.Snapshot.Count);
            Assert.AreEqual(4, manager.Snapshot[0]);
            Assert.AreEqual(5, manager.Snapshot[1]);
            Assert.AreEqual(6, manager.Snapshot[2]);
        }

        [TestMethod]
        public void AddOrUpdateRange_CorrectResults()
        {
            var manager = new CacheManager<int, int>(CacheFilePath);

            Assert.AreEqual(0, manager.Snapshot.Count);

            manager.AddRange(new[] { 100, 101, 102 });

            Assert.AreEqual(3, manager.Snapshot.Count);
            Assert.AreEqual(100, manager.Snapshot[0]);
            Assert.AreEqual(101, manager.Snapshot[1]);
            Assert.AreEqual(102, manager.Snapshot[2]);

            manager.AddOrUpdateRange(new[] { 203, 200, 202, 204 }, (val) => (val % 10));

            Assert.AreEqual(5, manager.Snapshot.Count);
            Assert.IsTrue(manager.Snapshot.Contains(203));
            Assert.IsTrue(manager.Snapshot.Contains(200));
            Assert.IsTrue(manager.Snapshot.Contains(101));
            Assert.IsTrue(manager.Snapshot.Contains(202));
            Assert.IsTrue(manager.Snapshot.Contains(204));
            Assert.IsFalse(manager.Snapshot.Contains(100));
            Assert.IsFalse(manager.Snapshot.Contains(102));
        }

        [TestMethod]
        public void Load_Save_LoadIfOlderThan_CorrectResults()
        {
            var manager = new CacheManager<IExtensionDataItemView, ExtensionDataItem>(CacheFilePath);

            manager.AddRange(
                new[]
                {
                    new ExtensionDataItem()
                    {
                        Title = "Title1",
                        Description = "Description1",
                        Version = "Version1",
                        Author = "Author1",
                        Link = "Link1",
                        Installer = "Installer1"
                    },
                    new ExtensionDataItem()
                    {
                        Title = "Title2",
                        Description = "Description2",
                        Version = "Version2",
                        Author = "Author2",
                        Link = "Link2",
                        Installer = "Installer2"
                    },
                     new ExtensionDataItem()
                    {
                        Title = "Title3",
                        Description = "Description3",
                        Version = "Version3",
                        Author = "Author3",
                        Link = "Link3",
                        Installer = "Installer3"
                    }
                });

            // Save twice to ensure that we didn't open the file in 'append' mode.
            Assert.IsTrue(manager.TrySaveCacheFile());
            Assert.IsTrue(manager.TrySaveCacheFile());

            Assert.IsTrue(manager.CacheFileExists);
            Assert.IsTrue(File.Exists(CacheFilePath));

            // Clear cache.
            manager.ReplaceAll(Enumerable.Empty<ExtensionDataItem>());
            Assert.AreEqual(0, manager.Snapshot.Count);

            Assert.IsTrue(manager.TryLoadCacheFile());

            // Check for proper deserialization.
            Assert.AreEqual(3, manager.Snapshot.Count);

            Assert.AreEqual("Title1", manager.Snapshot[0].Title);
            Assert.AreEqual("Description1", manager.Snapshot[0].Description);
            Assert.AreEqual("Version1", manager.Snapshot[0].Version);
            Assert.AreEqual("Author1", manager.Snapshot[0].Author);
            Assert.AreEqual("Link1", manager.Snapshot[0].Link);
            Assert.AreEqual("Installer1", manager.Snapshot[0].Installer);

            Assert.AreEqual("Title2", manager.Snapshot[1].Title);
            Assert.AreEqual("Description2", manager.Snapshot[1].Description);
            Assert.AreEqual("Version2", manager.Snapshot[1].Version);
            Assert.AreEqual("Author2", manager.Snapshot[1].Author);
            Assert.AreEqual("Link2", manager.Snapshot[1].Link);
            Assert.AreEqual("Installer2", manager.Snapshot[1].Installer);

            Assert.AreEqual("Title3", manager.Snapshot[2].Title);
            Assert.AreEqual("Description3", manager.Snapshot[2].Description);
            Assert.AreEqual("Version3", manager.Snapshot[2].Version);
            Assert.AreEqual("Author3", manager.Snapshot[2].Author);
            Assert.AreEqual("Link3", manager.Snapshot[2].Link);
            Assert.AreEqual("Installer3", manager.Snapshot[2].Installer);
        }
    }
}
