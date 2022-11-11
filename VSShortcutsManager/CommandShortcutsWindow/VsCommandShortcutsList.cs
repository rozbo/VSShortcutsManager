using System.Collections.Generic;

namespace VSShortcutsManager.CommandShortcutsWindow
{
    public class VsCommandShortcutsList : List<CommandShortcut>
    {
        public VsCommandShortcutsList()
            : base()
        { }

        public VsCommandShortcutsList(IEnumerable<CommandShortcut> collection)
            : base(collection)
        { }

        public VsCommandShortcutsList Clone()
        {
            // Shallow clone is enough
            return new VsCommandShortcutsList(this);
        }
    }
}