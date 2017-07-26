namespace VSPackageInstaller.SearchProvider
{
    using System;
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

            var nonNullTokens = tokens
                .Where(t => t != null)
                .Select(str => str.ParsedTokenText.ToLowerInvariant());

            if (this.SearchQuery.GetTokens(MaxTokens, tokens) > 0)
            {
                var searchResults = new List<Tuple<int, IExtensionDataItemView>>();

                foreach (var item in this.searchProvider.CachedItems)
                {
                    ScoreItem(searchResults, item, nonNullTokens);

                    // Cap search results at 20.
                    if (this.SearchResults >= MaxSearchResults ||
                        (this.TaskStatus == Microsoft.VisualStudio.VSConstants.VsSearchTaskStatus.Stopped))
                    {
                        break;
                    }
                }

                // Order results by score.
                var orderedResults = searchResults
                    .OrderByDescending(result => result.Item1)
                    .Select(result => result.Item2)
                    .Select(result => new SearchResult(this.searchProvider, result));

                // Report all results.
                this.SearchCallback.ReportResults(
                    this,
                    (this.SearchResults = (uint)searchResults.Count),
                    orderedResults.ToArray());
            }

            this.SearchCallback.ReportComplete(this, this.SearchResults);
        }

        private void ScoreItem(
            IList<Tuple<int, IExtensionDataItemView>> searchResults,
            IExtensionDataItemView item,
            IEnumerable<string> loweredTokens)
        {
            int score = 0;
            int tokensMatched = 0;

            var loweredTitle = item.Title.ToLowerInvariant();
            var loweredDescription = item.Description.ToLowerInvariant();

            foreach (var token in loweredTokens)
            {
                var titleContainsToken = loweredTitle.Contains(token);
                var descriptionContainsToken = loweredDescription.Contains(token);

                // TODO: less naive.
                if (titleContainsToken || descriptionContainsToken)
                {
                    ++tokensMatched;

                    // Title matches are worth twice as much.
                    if (titleContainsToken)
                    {
                        score += 2;
                    }

                    if (descriptionContainsToken)
                    {
                        score += 1;
                    }
                }
            }

            // Only return items that matched all of the tokens.
            if (tokensMatched == loweredTokens.Count())
            {
                searchResults.Add(Tuple.Create(score, item));
            }
        }

        protected override void OnStopSearch()
        {
            // TODO: stop search operation here.
            this.SearchCallback.ReportComplete(this, this.SearchResults);
        }
    }
}
