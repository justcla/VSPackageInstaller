namespace VSPackageInstaller.Cache
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization.Json;
    using System.Threading;

    internal sealed class CacheManager<TItemView, TItem> where TItem : TItemView
    {
        // Modified cross-thread.
        private Tuple<DateTime?, ImmutableList<TItemView>> cacheSnapshot;

        public CacheManager(string cacheFilePath)
        {
            if (string.IsNullOrWhiteSpace(cacheFilePath))
            {
                throw new ArgumentException($"{nameof(cacheFilePath)} must be non-empty and non-null");
            }

            this.CacheFilePath = Path.GetFullPath(cacheFilePath);

            this.ReplaceAll(Enumerable.Empty<TItemView>());
        }

        public bool CacheFileExists
        {
            get
            {
                try
                {
                    return File.Exists(this.CacheFilePath);
                }
                catch
                {
                    // Ok, not quite accurate, but what else can we do?
                    return false;
                }
            }
        }

        public string CacheFilePath { get; }

        public DateTime? LastUpdateTimeStamp
        {
            get
            {
                var snapshotTimestamp = Volatile.Read(ref cacheSnapshot).Item1;
                var cacheFileTimestamp = this.LastCacheFileUpdateTimeStamp;

                if (cacheFileTimestamp == null)
                {
                    return snapshotTimestamp;
                }
                else if (snapshotTimestamp > cacheFileTimestamp)
                {
                    return snapshotTimestamp;
                }
                else
                {
                    return cacheFileTimestamp;
                }
            }
        }

        public DateTime? LastCacheFileUpdateTimeStamp => (CacheFileExists ? File.GetLastWriteTimeUtc(this.CacheFilePath) : (DateTime?)null);

        public IReadOnlyList<TItemView> Snapshot => Volatile.Read(ref this.cacheSnapshot).Item2;

        public void ReplaceAll(IEnumerable<TItemView> items)
        {
            Volatile.Write(
                ref this.cacheSnapshot,
                Tuple.Create<DateTime?, ImmutableList<TItemView>>(DateTime.UtcNow, ImmutableList.CreateRange<TItemView>(items)));
        }

        public void AddRange(IEnumerable<TItemView> items)
        {
            var oldItems = Volatile.Read(ref this.cacheSnapshot).Item2;

            Volatile.Write(
                ref this.cacheSnapshot,
                Tuple.Create<DateTime?, ImmutableList<TItemView>>(DateTime.UtcNow, oldItems.AddRange(items)));
        }

        public void AddOrUpdateRange<TEqualityKey>(IEnumerable<TItemView> newOrUpdatedItems, Func<TItemView, TEqualityKey> equalityKeySelector)
        {
            var oldItems = Volatile.Read(ref this.cacheSnapshot).Item2;

            // Prithee not judge me for the quality of these source codes...

            var updatedItemsDictionary = new Dictionary<TEqualityKey, TItemView>();

            // Add all new and updated items to dictionary based upon their selected 'equality' property.
            foreach (var newOrUpdatedItem in newOrUpdatedItems)
            {
                updatedItemsDictionary.Add(equalityKeySelector(newOrUpdatedItem), newOrUpdatedItem);
            }

            var mergedListBuilder = ImmutableList.CreateBuilder<TItemView>();

            // Iterate through all old items and add their replacements to the new list, if one exists.
            foreach (var oldItem in oldItems)
            {
                var key = equalityKeySelector(oldItem);

                if (updatedItemsDictionary.TryGetValue(key, out var updatedItem))
                {
                    mergedListBuilder.Add(updatedItem);
                    updatedItemsDictionary.Remove(key);
                }
                else
                {
                    mergedListBuilder.Add(oldItem);
                }
            }

            // Anything left in the dictionary is a new item, append it to the list.
            mergedListBuilder.AddRange(updatedItemsDictionary.Values);

            Volatile.Write(
                ref this.cacheSnapshot,
                Tuple.Create<DateTime?, ImmutableList<TItemView>>(DateTime.UtcNow, mergedListBuilder.ToImmutable()));
        }

        public bool TryLoadCacheFile()
        {
            try
            {
                // TODO: use streams?
                if (JsonConvert.DeserializeObject<List<TItem>>(File.ReadAllText(this.CacheFilePath)) is List<TItem> items)
                {
                    this.ReplaceAll(items.Cast<TItemView>());
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool TrySaveCacheFile()
        {
            try
            {
                // TODO: use streams?
                File.WriteAllText(
                    this.CacheFilePath,
                    JsonConvert.SerializeObject(this.Snapshot.Cast<TItem>().ToList()));

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
