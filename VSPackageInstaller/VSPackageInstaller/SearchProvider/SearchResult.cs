namespace VSPackageInstaller.SearchProvider
{
    using System.Diagnostics;
    using Microsoft.VisualStudio.Shell.Interop;
    using VSPackageInstaller.Cache;

    internal sealed class SearchResult : IVsSearchItemResult
    {
        private readonly IExtensionDataItemView item;

        public SearchResult(
            IVsSearchProvider searchProvider,
            IExtensionDataItemView item)
        {
            // If either of these are null, we end up with very hard to trace exceptions that
            // in Visual Studio that don't really describe the issue. To save us future headaches..
            Debug.Assert(searchProvider != null);
            Debug.Assert(item != null);

            this.SearchProvider = searchProvider;
            this.item = item;
        }

        internal static VSPackageInstaller.PackageInstaller.PackageInstaller packageInstaller = new PackageInstaller.PackageInstaller();

        public void InvokeAction()
        {
            packageInstaller.Extension = item;
            packageInstaller.InstallPackage();
        }

        public IVsSearchProvider SearchProvider { get; }

        public string DisplayText => this.item.Title;

        public string Description => this.item.Description;

        public string Tooltip => this.item.Description;

        public IVsUIObject Icon => null;

        public string PersistenceData => this.item.ExtensionId.ToString();
    }
}
