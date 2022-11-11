using System;
using System.Linq;

namespace VSShortcutsManager.CommandShortcutsWindow.CommandFilters;

class HotKeyCommandsFilter : ICommandsFilter
{
    public HotKeyCommandsFilter(string searchCriteria, bool matchCase)
    {
        this._searchCriteria = searchCriteria;
        this._stringComparison = matchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
    }

    public VsCommandShortcutsList Filter(VsCommandShortcutsList commands)
    {
        var result = commands
            .Where(command => command.ShortcutText?.IndexOf(this._searchCriteria, this._stringComparison) >= 0);

        return new VsCommandShortcutsList(result);
    }

    private readonly string _searchCriteria;
    private readonly StringComparison _stringComparison;
}