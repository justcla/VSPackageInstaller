namespace VSPackageInstaller.SearchProvider
{
    using System;
    using System.Runtime.InteropServices;
    using Microsoft.VisualStudio.Shell.Interop;

    [Guid(SearchProviderGuid)]
    internal sealed class SearchProvider : IVsSearchProvider
    {
        private const string SearchProviderShortcut = "ext";
        private const string SearchProviderGuid = "91FA7E7E-5DE9-4776-AAB3-938BE278C2B0";

        public IVsSearchTask CreateSearch(
            uint cookie,
            IVsSearchQuery searchQuery,
            IVsSearchProviderCallback searchCallback)
            => new SearchTask(
                this,
                cookie,
                searchQuery,
                searchCallback);

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
    }
}
