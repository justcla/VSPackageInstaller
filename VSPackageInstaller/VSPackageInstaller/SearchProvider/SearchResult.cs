namespace VSPackageInstaller.SearchProvider
{
    using Microsoft.VisualStudio.Shell.Interop;
    using VSPackageInstaller.Cache;

    internal sealed class SearchResult : IVsSearchItemResult
    {
        private readonly IExtensionDataItemView item;

        public SearchResult(
            IVsSearchProvider searchProvider,
            IExtensionDataItemView item)
        {
            this.item = item;
        }

        public void InvokeAction()
        {
            // TODO: install the extension.
        }

        public IVsSearchProvider SearchProvider { get; }

        public string DisplayText => this.item.Title;

        public string Description => this.item.Description;

        public string Tooltip => this.item.Description;

        public IVsUIObject Icon => null;

        // TODO: serialized version of this search result for persistent quick launch results.
        public string PersistenceData => null;
    }
}
