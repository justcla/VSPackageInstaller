namespace VSPackageInstaller.Cache
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    internal sealed class CacheManager<TItem>
    {
        // Modified cross-thread.
        private Tuple<DateTime?, ImmutableList<TItem>> cacheSnapshot;

        public CacheManager(string cacheFilePath)
        {
            if (string.IsNullOrWhiteSpace(cacheFilePath))
            {
                throw new ArgumentException($"{nameof(cacheFilePath)} must be non-empty and non-null");
            }

            this.CacheFilePath = Path.GetFullPath(cacheFilePath);

            this.ReplaceAll(Enumerable.Empty<TItem>());
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

        public IReadOnlyList<TItem> Snapshot => Volatile.Read(ref this.cacheSnapshot).Item2;

        public void ReplaceAll(IEnumerable<TItem> items)
        {
            Volatile.Write(
                ref this.cacheSnapshot,
                Tuple.Create<DateTime?, ImmutableList<TItem>>(DateTime.UtcNow, ImmutableList.CreateRange<TItem>(items)));
        }

        public void AddRange(IEnumerable<TItem> items)
        {
            var oldItems = Volatile.Read(ref this.cacheSnapshot).Item2;

            Volatile.Write(
                ref this.cacheSnapshot,
                Tuple.Create<DateTime?, ImmutableList<TItem>>(DateTime.UtcNow, oldItems.AddRange(items)));
        }

        public void AddOrUpdateRange<TEqualityKey>(IEnumerable<TItem> newOrUpdatedItems, Func<TItem, TEqualityKey> equalityKeySelector)
        {
            var oldItems = Volatile.Read(ref this.cacheSnapshot).Item2;

            // Prithee not judge me for the quality of these source codes...

            var updatedItemsDictionary = new Dictionary<TEqualityKey, TItem>();

            // Add all new and updated items to dictionary based upon their selected 'equality' property.
            foreach (var newOrUpdatedItem in newOrUpdatedItems)
            {
                updatedItemsDictionary.Add(equalityKeySelector(newOrUpdatedItem), newOrUpdatedItem);
            }

            var mergedListBuilder = ImmutableList.CreateBuilder<TItem>();

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
                Tuple.Create<DateTime?, ImmutableList<TItem>>(DateTime.UtcNow, mergedListBuilder.ToImmutable()));
        }

        public async Task LoadCacheFileAsync()
        {
            throw new NotImplementedException();
        }

        public async Task SaveCacheFileAsync()
        {
            throw new NotImplementedException();
        }

        public async Task LoadIfCacheFileOlderThan(DateTime cutoffDate)
        {
            throw new NotImplementedException();
        }
    }
}
