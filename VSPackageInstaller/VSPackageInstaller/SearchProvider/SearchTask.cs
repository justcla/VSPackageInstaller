namespace VSPackageInstaller.SearchProvider
{
    using System.Linq;
    using System.Windows.Forms; // TODO: For MessageBox, remove.
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;

    internal sealed class SearchTask : VsSearchTask
    {
        private readonly IVsSearchProvider searchProvider;

        public SearchTask(
            IVsSearchProvider searchProvider,
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
            // TODO: start search operation here. This is already running in a threadpool task.
            // NOTE: you must return results in approx. 200ms or else your results are not shown.

            ReportDummyResults();

            // TODO: report incremental progress and errors.
            // this.SearchCallback.ReportProgress(this, 10, 100);
            // this.SetTaskStatus(VSConstants.VsSearchTaskStatus.Error);
        }

        protected override void OnStopSearch()
        {
            // TODO: stop search operation here.
            this.SearchCallback.ReportComplete(this, this.SearchResults);
        }

        private void ReportDummyResults()
        {
            const int MaxTokens = 25;
            var tokens = new IVsSearchToken[MaxTokens];

            uint NumOfTokens = this.SearchQuery.GetTokens(MaxTokens, tokens);

            if (NumOfTokens > 0)
            {
                // Very lame dummy search logic:
                try
                {
                    if (tokens.Any(token => ((token.ParsedTokenText == "hot") || (token.ParsedTokenText == "commands"))))
                    {
                        this.SearchCallback.ReportResult(
                            this,
                            new SearchResult(
                                this.searchProvider,
                                "Hot Commands",
                                "Some commands...that are hot",
                                null,
                                () => MessageBox.Show("Installing hot commands...")));
                        ++this.SearchResults;
                    }
                }
                catch (System.ArgumentNullException e)
                {
                }

                try
                {
                    if (tokens.Any(token => ((token.ParsedTokenText == "cold") || (token.ParsedTokenText == "commands"))))
                    {
                        this.SearchCallback.ReportResult(
                            this,
                            new SearchResult(
                                this.searchProvider,
                                "Cold Commands",
                                "Some commands...that are cold",
                                null,
                                () => MessageBox.Show("Installing cold commands...")));
                        ++this.SearchResults;
                    }
                }
                catch (System.ArgumentNullException e)
                {
                }

                try
                {
                    if (tokens.Any(token => ((token.ParsedTokenText == "easy") || (token.ParsedTokenText == "motion"))))
                    {
                        this.SearchCallback.ReportResult(
                            this,
                            new SearchResult(
                                this.searchProvider,
                                "Easy Motion",
                                "Motion.. that is easy",
                                null,
                                () => MessageBox.Show("Installing easy motion...")));
                        ++this.SearchResults;
                    }
                }
                catch (System.ArgumentNullException e)
                {
                }

                try
                {
                    if (tokens.Any(token => ((token.ParsedTokenText == "hard") || (token.ParsedTokenText == "motion"))))
                    {
                        this.SearchCallback.ReportResult(
                            this,
                            new SearchResult(
                                this.searchProvider,
                                "Hard Motion",
                                "Motion.. that is hard",
                                null,
                                () => MessageBox.Show("Installing hard motion...")));
                        ++this.SearchResults;
                    }
                }
                catch (System.ArgumentNullException e)
                {
                }
            }
        }
    }
}
