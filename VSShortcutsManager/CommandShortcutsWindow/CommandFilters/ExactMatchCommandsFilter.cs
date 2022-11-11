using System;
using System.Linq;

namespace VSShortcutsManager.CommandShortcutsWindow.CommandFilters;

class ExactMatchCommandsFilter : ICommandsFilter
{
    public ExactMatchCommandsFilter(string searchCriteria, bool matchCase)
    {
        this.searchCriteria = searchCriteria;
        this.stringComparison = matchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
    }

    public VsCommandShortcutsList Filter(VsCommandShortcutsList commands)
    {
        var result = commands
            .Where(command => command.CommandText.IndexOf(this.searchCriteria, this.stringComparison) >= 0);

        return new VsCommandShortcutsList(result);
    }

    private readonly string searchCriteria;
    private readonly StringComparison stringComparison;
}