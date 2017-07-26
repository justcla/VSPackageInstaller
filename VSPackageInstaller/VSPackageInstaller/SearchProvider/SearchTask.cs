namespace VSPackageInstaller.SearchProvider
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using VSPackageInstaller.Cache;

    internal sealed class SearchTask : VsSearchTask
    {
        private const int MaxSearchResults = 20;
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

                    // Cap search results at 20.
                    if (this.SearchResults >= MaxSearchResults ||
                        (this.TaskStatus == Microsoft.VisualStudio.VSConstants.VsSearchTaskStatus.Stopped))
                    {
                        break;
                    }
                }
            }

            this.SearchCallback.ReportComplete(this, this.SearchResults);
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
                if (item.Title.Contains(token.ParsedTokenText) || item.Description.Contains(token.ParsedTokenText))
                {
                    this.SearchCallback.ReportResult(
                        this,
                        new SearchResult(this.searchProvider, item));

                    ++this.SearchResults;
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
