using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Design;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using VSShortcutsManager.CommandShortcutsWindow;
using VSShortcutsManager.CommandShortcutsWindow.Converts;

namespace VSShortcutsManager
{
    /// <summary>
    /// This class implements the tool window exposed by this package and hosts a user control.
    /// </summary>
    /// <remarks>
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane,
    /// usually implemented by the package implementer.
    /// <para>
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its
    /// implementation of the IVsUIElementPane interface.
    /// </para>
    /// </remarks>
    [Guid("124118e3-7c75-490e-8ace-742c96f001da")]
    public class CommandShortcutsToolWindow : ToolWindowPane, IVsSearchCallback
    {
        public const string guidVSShortcutsManagerCmdSet = "cca0811b-addf-4d7b-9dd6-fdb412c44d8a";
        public const int CommandShortcutsToolWinToolbar = 0x2004;

        public static readonly Guid VSShortcutsManagerCmdSetGuid = new Guid("cca0811b-addf-4d7b-9dd6-fdb412c44d8a");
        public const int ShowTreeViewCmdId = 0x1815;
        public const int ShowListViewCmdId = 0x1825;

        public override object Content => _contentControl;
        private CommandShortcutsControl _contentControl;

        // the only one data context. neither tree or list must use this context! 

        private CommandTreeView.CommandShortcutsTree treeControl;


        private CommandShortcutsList? _cmdListView;
        public CommandTreeView.CommandShortcutsTree TreeControl
        {
            get
            {
                if (treeControl == null)
                {
                    treeControl = new CommandTreeView.CommandShortcutsTree();
                }
                return treeControl;
            }
            set => treeControl = value;
        }

        private VSShortcutQueryEngine QueryEngine;
        EnvDTE.Commands DTECommands;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandShortcutsToolWindow"/> class.
        /// </summary>
        public CommandShortcutsToolWindow() : base(null)
        {
            this.Caption = "Command Shortcuts";

            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on
            // the object returned by the Content property.
            this._contentControl = new CommandShortcutsControl();
            this.CmdDataContext = new CommandShortcutsControlDataContext(this);
        }

        protected override void Initialize()
        {
            base.Initialize();

            this.QueryEngine = VSShortcutsManager.Instance.queryEngine;
            this.DTECommands = QueryEngine.DTECommands;

            this.ToolBar = new CommandID(new Guid(guidVSShortcutsManagerCmdSet), CommandShortcutsToolWinToolbar);

            //_contentControl.MyDataContext = new CommandShortcutsControlDataContext(this);

            RegisterCommandHandlers();

            // Set the default initial opening layout and data
            _contentControl.Content = TreeControl;  // Lazy-loading tree control property

            // Get list of commands and shortcuts (from DTE?)
            // Convert to the correct objects for the CommandShortcutsTree
            // Push the data onto the TreeControl
            //TreeControl.Source = commands;
            //TreeControl.Source = _cmdDataContext.Commands;
            // 对 TreeView的item source进行数据绑定
            var bind = new Binding
            {
                Converter = new TreeViewDataListConverter(),
                Source = CmdDataContext,
                Path = new PropertyPath("Commands")
            };
            TreeControl.trvCommands.SetBinding(ItemsControl.ItemsSourceProperty, bind);

            // 初始化list view

        }



        private void RegisterCommandHandlers()
        {
            //IVsActivityLog log = Package.GetGlobalService(typeof(IMenuCommandService)) as IVsActivityLog;
            //if (log == null) return;
            if (GetService(typeof(IMenuCommandService)) is OleMenuCommandService commandService)
            {
                // Switch to Tree View
                commandService.AddCommand(CreateMenuItem(ShowTreeViewCmdId, this.ShowTreeViewEventHandler));
                // Switch to List View
                commandService.AddCommand(CreateMenuItem(ShowListViewCmdId, this.ShowListViewEventHandler));
            }
        }

        private void ShowTreeViewEventHandler(object sender, EventArgs e)
        {
            ((CommandShortcutsControl)Content).Content = TreeControl;
        }

        private void ShowListViewEventHandler(object sender, EventArgs e)
        {
            // 启用list view
            _cmdListView ??= new CommandShortcutsList
            {
                // 绑定其数据源
                DataContext = CmdDataContext
            };
            _contentControl.Content = _cmdListView;
        }

        private OleMenuCommand CreateMenuItem(int cmdId, EventHandler menuItemCallback)
        {
            return new OleMenuCommand(menuItemCallback, new CommandID(VSShortcutsManagerCmdSetGuid, cmdId));
        }

        private CommandShortcutsControlDataContext GetDataContext()
        {
            var cmdShortcutsControl = (CommandShortcutsControl)Content;
            var cmdShortcutsDataContext = (CommandShortcutsControlDataContext)cmdShortcutsControl.DataContext;
            return cmdShortcutsDataContext;
        }


        #region Search

        public void ReportProgress(IVsSearchTask pTask, uint dwProgress, uint dwMaxProgress)
        {
            // do nothing....
        }

        public void ReportComplete(IVsSearchTask pTask, uint dwResultsFound)
        {
            // the result here, we need it update the tree view result
            // 
            //
            var task = (CommandShortcutsSearchTask)pTask;
            // 确保UI线程
            this.TreeControl.Dispatcher.Invoke(() =>
            {

                // this.TreeControl.Source = task.SearchResult;
            });
        }

        public override IVsSearchTask CreateSearch(uint dwCookie, IVsSearchQuery pSearchQuery, IVsSearchCallback pSearchCallback)
        {
            if (pSearchQuery == null || pSearchCallback == null)
            {
                return null;
            }

            return new CommandShortcutsSearchTask(dwCookie, pSearchQuery, pSearchCallback, this);
        }

        public override void ClearSearch()
        {
            CmdDataContext.ClearSearch();
        }

        private IVsEnumWindowSearchOptions _mOptionsEnum;
        public override IVsEnumWindowSearchOptions SearchOptionsEnum
        {
            get
            {
                if (_mOptionsEnum == null)
                {
                    List<IVsWindowSearchOption> list = new List<IVsWindowSearchOption>();

                    list.Add(this.MatchCaseOption);
                    list.Add(this.IsSearchingHotKey);

                    _mOptionsEnum = new WindowSearchOptionEnumerator(list) as IVsEnumWindowSearchOptions;
                }

                return _mOptionsEnum;
            }
        }

        private WindowSearchBooleanOption m_matchCaseOption;

        public WindowSearchBooleanOption MatchCaseOption
        {
            get
            {
                if (m_matchCaseOption == null)
                {
                    m_matchCaseOption = new WindowSearchBooleanOption("Match case", "Match case", false);
                }

                return m_matchCaseOption;
            }
        }


        private WindowSearchBooleanOption _isSearchingHotKey;

        public WindowSearchBooleanOption IsSearchingHotKey
        {
            get
            {
                if (_isSearchingHotKey == null)
                {
                    _isSearchingHotKey = new WindowSearchBooleanOption("HotKey", "Is search hot key?", false);
                }
                return _isSearchingHotKey;
            }
        }


        public override bool SearchEnabled => true;

        public CommandShortcutsControlDataContext CmdDataContext { get; }

        #endregion // Search
    }
}
