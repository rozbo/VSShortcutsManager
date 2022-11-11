using System.Linq;
using Microsoft.VisualStudio.Text.PatternMatching;

namespace VSShortcutsManager.CommandShortcutsWindow.CommandFilters;

class FuzzyMatchCommandsFilter : ICommandsFilter
{
    public FuzzyMatchCommandsFilter(IPatternMatcher patternMatcher)
    {
        this.patternMatcher = patternMatcher;
    }

    public VsCommandShortcutsList Filter(VsCommandShortcutsList commands)
    {
        var result = commands
            .Select(command => (command: command, match: patternMatcher.TryMatch(command.CommandText)))
            .Where(tuple => tuple.match != null)
            .OrderBy(tuple => tuple.match)
            .Select(tuple => tuple.command);

        return new VsCommandShortcutsList(result);
    }

    private readonly IPatternMatcher patternMatcher;
}