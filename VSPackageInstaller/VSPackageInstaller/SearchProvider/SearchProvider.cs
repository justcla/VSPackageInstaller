namespace VSPackageInstaller.SearchProvider
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
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
        private CacheManager<IExtensionDataItemView, ExtensionDataItem> cacheManager;
        private MarketplaceDataService marketPlaceService;

        public IVsSearchTask CreateSearch(
            uint cookie,
            IVsSearchQuery searchQuery,
            IVsSearchProviderCallback searchCallback)
        {
            EnsureInitialized();
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
            // TODO: this method should be able to take a string and deserialize it into a search result.
            // For now, we just won't have persistent results between QL sessions.
            return null;
        }

        public string DisplayText => SearchProviderResources.SearchProvider_DisplayText;

        public string Description => SearchProviderResources.SearchProvider_Description;

        public string Tooltip => SearchProviderResources.SearchProvider_Description;

        public Guid Category => typeof(SearchProvider).GUID;

        public string Shortcut => SearchProviderShortcut;

        public IReadOnlyList<IExtensionDataItemView> CachedItems
        {
            get
            {
                if (this.cacheManager == null)
                {
                    throw new InvalidOperationException("Cache has not yet been initialized");
                }

                return this.cacheManager.Snapshot;
            }
        }

        private void EnsureInitialized()
        {
            if (this.cacheManager == null)
            {
                this.cacheManager = new CacheManager<IExtensionDataItemView, ExtensionDataItem>(Path.Combine(Utilities.ExtensionAppDataPath, CacheFileName));
                this.marketPlaceService = new MarketplaceDataService();

                // Load cached results from disk, or fallback to over the wire refresh, if stale or non-existant.
                if (!this.cacheManager.TryLoadCacheFile() ||
                    this.cacheManager.LastUpdateTimeStamp.Value > DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)))
                {
                    this.RefreshCache();
                }
            }
            else if (DateTime.UtcNow.Subtract(this.cacheManager.LastUpdateTimeStamp.Value) > TimeSpan.FromDays(1))
            {
                // Queue a refresh if it's been longer than 24 hours.
                // TODO: do this async so we don't delay the current search.
                this.RefreshCache();
            }
        }

        private void RefreshCache()
        {
            if (this.cacheManager == null)
            {
                throw new InvalidOperationException("Cache has not yet been initialized");
            }

            // Clear the list.
            cacheManager.ReplaceAll(System.Linq.Enumerable.Empty<IExtensionDataItemView>());

            // TODO: correct SKU information.
            // TODO: incremental.
            // TODO: async and background.
            marketPlaceService.GetMarketplaceDataItems(
                "15.0",
                new[] { "Pro", "Ultimate" },
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
