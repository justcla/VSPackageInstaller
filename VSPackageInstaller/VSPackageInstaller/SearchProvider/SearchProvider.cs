namespace VSPackageInstaller.SearchProvider
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Runtime.InteropServices;
    using Microsoft.VisualStudio.Shell.Interop;
    using VSPackageInstaller.Cache;
    using VSPackageInstaller.MarketplaceService;

    [Guid(SearchProviderGuid)]
    internal sealed class SearchProvider : IVsSearchProvider
    {
        private const string SearchProviderShortcut = "ext";
        private const string SearchProviderGuid = "91FA7E7E-5DE9-4776-AAB3-938BE278C2B0";

        private const string CacheFileName = "cache.json";

        // Lazily initialized.
        private static CacheManager<IExtensionDataItemView, ExtensionDataItem> cacheManager;
        private static MarketplaceDataService marketPlaceService;

        public IVsSearchTask CreateSearch(
            uint cookie,
            IVsSearchQuery searchQuery,
            IVsSearchProviderCallback searchCallback)
        {
            return new SearchTask(
                this,
                cookie,
                searchQuery,
                searchCallback);
        }

        public void ProvideSearchSettings(IVsUIDataSource pSearchOptions)
        {
        }

        public IVsSearchItemResult CreateItemResult(string lpszPersistenceData)
        {
            // Asymptotically quite slow, but the trade off here is to either
            // 1) iterate the entire collection to find the correct entry
            // 2) serialize/deserialize the entire single entry in a string.
            // I'm going with 1 to avoid future issues with invalidating these
            // results when switching languages, or when the items are updated.

            try
            {
                var extensionIdGuid = new Guid(lpszPersistenceData);
                var selectedItem = this.CachedItems.SingleOrDefault(cachedItem => cachedItem.ExtensionId == extensionIdGuid);

                return selectedItem != null ? new SearchResult(this, selectedItem) : null;
            }
            catch
            {
                // Very odd, perhaps the serialization format changed?
                Debug.Fail("Failed to deserialize the item persistance data");
            }

            return null;
        }

        public string DisplayText => SearchProviderResources.SearchProvider_DisplayText;

        public string Description => SearchProviderResources.SearchProvider_Description;

        public string Tooltip => SearchProviderResources.SearchProvider_Description;

        public Guid Category => typeof(SearchProvider).GUID;

        public string Shortcut => SearchProviderShortcut;

        public IEnumerable<IExtensionDataItemView> CachedItems
            => cacheManager?.Snapshot ?? Enumerable.Empty<IExtensionDataItemView>();

        // Start process of initialization. The CacheManager instance is provided
        // synchronously, but actual population or loading of the cache happens
        // asynchronously in a fire and forget fashion. If the cache population is
        // in progress at search time, we simply search the set of available results.
        // TODO: surface 'search in progress' message to users in this case.
        public static void EnsureInitializedFireAndForget()
        {
            if (cacheManager == null)
            {
                Task.Run((Action)FirstTimeInitialize);
            }
            else if (DateTime.UtcNow.Subtract(cacheManager.LastCacheFileUpdateTimeStamp.Value) > TimeSpan.FromDays(1))
            {
                // Queue a refresh if it's been longer than 24 hours.
                Task.Run((Action)RefreshCache);
            }
        }

        private static void FirstTimeInitialize()
        {
            cacheManager = new CacheManager<IExtensionDataItemView, ExtensionDataItem>(Path.Combine(Utilities.ExtensionAppDataPath, CacheFileName));
            marketPlaceService = new MarketplaceDataService();

            // Load cached results from disk, or fallback to over the wire refresh, if stale or non-existant.
            if (!cacheManager.TryLoadCacheFile() ||
                DateTime.UtcNow.Subtract(cacheManager.LastCacheFileUpdateTimeStamp.Value) > TimeSpan.FromDays(1))
            {
                RefreshCache();
            }
        }

        private static void RefreshCache()
        {
            if (cacheManager == null)
            {
                throw new InvalidOperationException("Cache has not yet been initialized");
            }

            // Clear the list.
            cacheManager.ReplaceAll(System.Linq.Enumerable.Empty<IExtensionDataItemView>());

            // TODO: correct SKU information.
            // TODO: incremental.
            marketPlaceService.GetMarketplaceDataItems(
                VsEditionUtil.GetCurrentVsVersion(),
                VsEditionUtil.GetSkusList(),
                DateTime.MinValue,
                (items) =>
                {
                    cacheManager.AddRange(items);
                    return true;
                });

            try
            {
                Directory.CreateDirectory(Utilities.ExtensionAppDataPath);
                cacheManager.TrySaveCacheFile();
            }
            catch
            {
                Debug.Fail("Failed to create local app data directory");
            }
        }
    }
}
