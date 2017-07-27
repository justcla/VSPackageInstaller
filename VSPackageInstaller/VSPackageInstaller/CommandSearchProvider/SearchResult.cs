namespace VSPackageInstaller.CommandSearchProvider
{
    using System;
    using Microsoft.VisualStudio.Shell.Interop;

    internal sealed class SearchResult : IVsSearchItemResult
    {
        private readonly Action invokeAction;

        // TODO: refactor this class as needed to population with extension gallery results.
        public SearchResult(
            IVsSearchProvider searchProvider,
            string displayText,
            string description,
            string tooltip,
            Action invokeAction)
        {
            this.SearchProvider = searchProvider;
            this.DisplayText = displayText;
            this.Description = description;
            this.Tooltip = tooltip;
            this.invokeAction = invokeAction;
        }

        public void InvokeAction() => this.invokeAction.Invoke();

        public IVsSearchProvider SearchProvider { get; }

        public string DisplayText { get; }

        public string Description { get; }

        public string Tooltip { get; }

        public IVsUIObject Icon => null;

        // TODO: serialized version of this search result for persistent quick launch results.
        public string PersistenceData => null;
    }
}
