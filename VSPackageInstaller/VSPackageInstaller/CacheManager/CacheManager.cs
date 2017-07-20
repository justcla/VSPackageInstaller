namespace VSPackageInstaller.CacheManager
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    internal sealed class CacheManager<TItem>
    {
        public bool CacheFileExists => throw new NotImplementedException();

        public DateTime? LastUpdate => throw new NotImplementedException();

        public IReadOnlyList<TItem> Snapshot;

        public void Init()
        {
            throw new NotImplementedException();
        }

        public void Create(IEnumerable<TItem> items)
        {
            throw new NotImplementedException();
        }

        public void AddRange(IEnumerable<TItem> items)
        {
            throw new NotImplementedException();
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
