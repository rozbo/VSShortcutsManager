using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using VSShortcutsManager.CommandShortcutsWindow;

namespace VSShortcutsManager
{
    internal class CommandShortcutsSearchTask : VsSearchTask
    {
        private CommandShortcutsToolWindow parentWindow;
        private CommandShortcutsControlDataContext _searchControlDataContext;

        private VsCommandShortcutsList _searchResult;
        public CommandShortcutsSearchTask(uint dwCookie, IVsSearchQuery pSearchQuery, IVsSearchCallback pSearchCallback, CommandShortcutsToolWindow parentWindow)
            : base(dwCookie, pSearchQuery, pSearchCallback)
        {
            this.parentWindow = parentWindow;
            this._searchControlDataContext = parentWindow.CmdDataContext;

        }

        public VsCommandShortcutsList SearchResult => _searchResult;

        protected override void OnStartSearch()
        {
            uint resultCount = 0;
            this.ErrorCode = VSConstants.S_OK;

            try
            {
                string searchString = this.SearchQuery.SearchString;
                bool matchCase = this.parentWindow.MatchCaseOption.Value;
                resultCount = this._searchControlDataContext.SearchCommands(searchString, matchCase);
                _searchResult = this._searchControlDataContext.Commands;
            }
            catch (Exception e)
            {
                this.ErrorCode = VSConstants.E_FAIL;
            }
            finally
            {
                this.SearchResults = resultCount;
            }

            // This sets the task status to complete and reports task completion.   
            base.OnStartSearch();
        }

        protected override void OnStopSearch()
        {
            this.SearchResults = 0;
        }
    }
}