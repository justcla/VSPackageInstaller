namespace VSPackageInstaller.CommandSearchProvider
{
    using System;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.Shell.Interop;
    using EnvDTE;

    [Guid(SearchProviderGuid)]
    internal sealed class CommandSearchProvider : IVsSearchProvider
    {
        private const string SearchProviderShortcut = "cmd";
        private const string SearchProviderGuid = "7559C72E-5FDD-4C22-A5E3-5CB146D571F4";

        public CommandSearchProvider()
        {
            Task.Run(() => {
                this.Dte = (EnvDTE80.DTE2)Microsoft.VisualStudio.Shell.ServiceProvider.GlobalProvider.GetService(typeof(DTE));
            });
        }

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

        public string DisplayText => "VS Commands Search"; // SearchProviderResources.SearchProvider_DisplayText;

        public string Description => "Sublime style VS commands search"; // SearchProviderResources.SearchProvider_Description;

        public string Tooltip => "Sublime style VS commands search"; // SearchProviderResources.SearchProvider_Description;

        public Guid Category => typeof(CommandSearchProvider).GUID;

        public string Shortcut => SearchProviderShortcut;

        public EnvDTE80.DTE2 Dte { get; private set; }
    }
}
