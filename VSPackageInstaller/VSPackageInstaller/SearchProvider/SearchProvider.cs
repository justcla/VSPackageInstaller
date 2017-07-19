namespace VSPackageInstaller.SearchProvider
{
    using System;
    using Microsoft.VisualStudio.Shell.Interop;
    using System.Runtime.InteropServices;

    [Guid(SearchProviderGuid)]
    internal sealed class SearchProvider : IVsSearchProvider
    {
        private const string SearchProviderShortcut = "ext";
        private const string SearchProviderGuid = "91FA7E7E-5DE9-4776-AAB3-938BE278C2B0";

        public IVsSearchTask CreateSearch(uint dwCookie, IVsSearchQuery pSearchQuery, IVsSearchProviderCallback pSearchCallback)
        {
            throw new NotImplementedException();
        }

        public void ProvideSearchSettings(IVsUIDataSource pSearchOptions)
        {
        }

        public IVsSearchItemResult CreateItemResult(string lpszPersistenceData)
        {
            throw new NotImplementedException();
        }

        public string DisplayText => SearchProviderResources.SearchProvider_DisplayText;

        public string Description => SearchProviderResources.SearchProvider_Description;

        public string Tooltip => SearchProviderResources.SearchProvider_Description;

        public Guid Category => typeof(SearchProvider).GUID;

        public string Shortcut => SearchProviderShortcut;
    }
}
