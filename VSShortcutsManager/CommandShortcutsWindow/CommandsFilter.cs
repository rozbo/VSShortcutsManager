using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Text.PatternMatching;
using System;
using System.Globalization;
using System.Linq;
using VSShortcutsManager.CommandShortcutsWindow;

namespace VSShortcutsManager
{
    interface ICommandsFilter
    {
        VsCommandShortcutsList Filter(VsCommandShortcutsList commands);
    }

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

    class CommandsFilterFactory
    {
        public ICommandsFilter GetCommandsFilter(string searchCriteria, bool matchCase)
        {
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
}
