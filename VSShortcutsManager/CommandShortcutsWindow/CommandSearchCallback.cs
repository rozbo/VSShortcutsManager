using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSShortcutsManager.CommandShortcutsWindow
{
    internal class CommandSearchCallback: IVsSearchCallback
    {
        public void ReportProgress(IVsSearchTask pTask, uint dwProgress, uint dwMaxProgress)
        {
            // do nothing....
        }

        public void ReportComplete(IVsSearchTask pTask, uint dwResultsFound)
        {
            // the result here, we need it update the tree view result
            // 
            throw new NotImplementedException();
        }
    }
}
