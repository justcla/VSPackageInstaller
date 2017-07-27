namespace VSPackageInstaller.CommandSearchProvider
{
    using System;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;

    internal sealed class SearchTask : VsSearchTask
    {
        private readonly CommandSearchProvider searchProvider;

        public SearchTask(
            CommandSearchProvider searchProvider,
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

            // TODO: more validation
            if (searchProvider.Dte == null)
            {
                return;
            }
            if (string.IsNullOrEmpty(this.SearchQuery.SearchString))
            {
                return;
            }
            foreach (EnvDTE.Command command in searchProvider.Dte.Commands)
            {
                if (command.Name.Contains(this.SearchQuery.SearchString) && command.IsAvailable)
                {
                    this.SearchCallback.ReportResult(
                        this,
                        new SearchResult(
                            this.searchProvider,
                            command.Name,
                            command.Name,
                            command.Name,
                            () => {
                                System.Diagnostics.Debug.WriteLine($"Guid={command.Guid}, Name={command.Name}, Id={command.ID}");
                                object customIn = new object(), customOut = new object();
                                searchProvider.Dte.Commands.Raise(command.Guid, command.ID, customIn, customOut);
                            }
                        ));
                    ++this.SearchResults;
                }
            }

            // TODO: report incremental progress and errors.
            // this.SearchCallback.ReportProgress(this, 10, 100);
            // this.SetTaskStatus(VSConstants.VsSearchTaskStatus.Error);
        }

        protected override void OnStopSearch()
        {
            // TODO: stop search operation here.
            this.SearchCallback.ReportComplete(this, this.SearchResults);
        }
    }
}
