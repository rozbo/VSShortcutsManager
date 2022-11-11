using System.Globalization;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Text.PatternMatching;
using VSShortcutsManager.CommandShortcutsWindow.CommandFilters;

namespace VSShortcutsManager.CommandShortcutsWindow;

class CommandsFilterFactory
{
    public ICommandsFilter GetCommandsFilter(string searchCriteria, bool matchCase, bool isSearchingHotKey)
    {
        if (isSearchingHotKey)
        {
            return new HotKeyCommandsFilter(searchCriteria, matchCase);
        }

        if (matchCase)
        {
            return new ExactMatchCommandsFilter(searchCriteria, matchCase: true);
        }

        var componentModel = VSShortcutsManagerPackage.GetGlobalService(typeof(SComponentModel)) as IComponentModel;
        if (componentModel == null)
        {
            return new ExactMatchCommandsFilter(searchCriteria, matchCase: false);
        }

        var patternMatcherFactory = componentModel.GetService<IPatternMatcherFactory>();
        if (patternMatcherFactory == null)
        {
            return new ExactMatchCommandsFilter(searchCriteria, matchCase: false);
        }

        var patternMatcher = patternMatcherFactory.CreatePatternMatcher(
            searchCriteria,
            new PatternMatcherCreationOptions(
                CultureInfo.CurrentCulture,
                PatternMatcherCreationFlags.AllowFuzzyMatching));

        return new FuzzyMatchCommandsFilter(patternMatcher);
    }
}