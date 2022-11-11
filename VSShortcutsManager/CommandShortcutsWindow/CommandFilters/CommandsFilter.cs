namespace VSShortcutsManager.CommandShortcutsWindow.CommandFilters
{
    interface ICommandsFilter
    {
        VsCommandShortcutsList Filter(VsCommandShortcutsList commands);
    }
}
