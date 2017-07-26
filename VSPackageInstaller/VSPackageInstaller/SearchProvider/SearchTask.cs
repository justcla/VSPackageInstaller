namespace VSPackageInstaller.SearchProvider
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Forms; // TODO: For MessageBox, remove.
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using VSPackageInstaller.Cache;

    internal sealed class SearchTask : VsSearchTask
    {
        private readonly SearchProvider searchProvider;

        public SearchTask(
            SearchProvider searchProvider,
            uint dwCookie,
            IVsSearchQuery pSearchQuery,
            IVsSearchProviderCallback searchCallback)
            : base(dwCookie,
                  pSearchQuery,
                  searchCallback)
        {
            this.searchProvider = searchProvider;
            this.SearchCallback = searchCallback;
        }

        public new IVsSearchProviderCallback SearchCallback { get; }

        protected override void OnStartSearch()
        {
            // Start search operation here. This is already running in a threadpool task.
            // NOTE: you must return results in approx. 200ms or else your results are not shown.

            const int MaxTokens = 25;
            var tokens = new IVsSearchToken[MaxTokens];

            this.SearchQuery.GetTokens(MaxTokens, tokens);

            var nonNullTokens = tokens.Where(t => t != null);

            if (this.SearchQuery.GetTokens(MaxTokens, tokens) > 0)
            {
                foreach (var item in this.searchProvider.CachedItems)
                {
                    MatchItem(item, nonNullTokens);
                }

                // TODO: report incremental progress and errors.
                // this.SearchCallback.ReportProgress(this, 10, 100);
                // this.SetTaskStatus(VSConstants.VsSearchTaskStatus.Error);
            }
        }

        private void MatchItem(IExtensionDataItemView item, IEnumerable<IVsSearchToken> nonNullTokens)
        {
            foreach (var token in nonNullTokens)
            {
                if (token == null)
                {
                    return;
                }

                // TODO: less naive.
                // TODO: cancellable.
                if (item.Title.Contains(token.ParsedTokenText) && item.Description.Contains(token.ParsedTokenText))
                {
                    this.SearchCallback.ReportResult(
                        this,
                        new SearchResult(this.searchProvider, item));

                    ++this.SearchResults;
                    this.SearchCallback.ReportProgress(this, this.SearchResults, (uint)this.searchProvider.CachedItems.Count);
                    return;
                }
            }
        }

        protected override void OnStopSearch()
        {
            // TODO: stop search operation here.
            this.SearchCallback.ReportComplete(this, this.SearchResults);
        }
    }
}
