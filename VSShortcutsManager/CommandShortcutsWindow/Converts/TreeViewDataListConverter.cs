using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace VSShortcutsManager.CommandShortcutsWindow.Converts
{
    public class TreeViewDataListConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 从 CommandShortcutsControlDataContext 到 IEnumerable<CommandGroup>
            // ------
            return GetCommandsList(value as VsCommandShortcutsList);
        }
        public IEnumerable<object> GetCommandsList(VsCommandShortcutsList? cmds)
        {
            if (cmds == null)
            {
                return Enumerable.Empty<object>();
            }

            IEnumerable<object> result = new ObservableCollection<object>();

            // Fetch all the commands from DTE
            foreach (var cmd in cmds)
            {
                // Check for a valid command name
                string commandName = cmd.CommandText;
                if (string.IsNullOrWhiteSpace(commandName))
                {
                    continue;
                }

                // Create the CommandItem for this command
                var commandItem = new CommandTreeView.CommandItem();
                commandItem.CommandName = commandName;
                commandItem.ShortcutGroup = GetShortcutMap2(cmd.Binding);

                // Parse the bindings (if there are any bound to the command)
                // Note: Binding is a combination of scope and key-combo
                //if (cmd.Bindings != null && cmd.Bindings is object[] bindingsObj && bindingsObj.Length > 0)
                //{
                //    // Build a map of [Scope => (List of shortcuts)]
                //    commandItem.ShortcutGroup = GetShortcutMap(bindingsObj);
                //}
                // 1----


                // Handle the Group name(s)
                string[] commandNameParts = commandName.Split('.');
                CommandTreeView.CommandGroup groupParent = null;
                // Handle case where there is no group (no prefix before '.')
                if (commandNameParts.Length < 2)
                {
                    // No group element to this name. Add it to "Ungrouped" group.
                    groupParent = GetCommandGroup("Ungrouped", (Collection<object>)result);
                }
                else
                {
                    // Loop over the group parts  (not the last part - that's the command name)
                    for (int i = 0; i < commandNameParts.Length - 1; i++)
                    {
                        // Find the group part in the current groupParent's list of groups
                        string groupName = commandNameParts[i];

                        // Top level group. Find it in the results object
                        // Other groups: Find the item in the Items collection of the previous group
                        Collection<object> subGroups = (i == 0) ? (Collection<object>)result : groupParent.Items;

                        groupParent = GetCommandGroup(groupName, subGroups);
                    }
                }

                // groupParent is now the parent group the command item should be added to
                groupParent.Items.Add(commandItem);
            }

            return result;
        }
        private static CommandTreeView.CommandGroup GetCommandGroup(string groupName, Collection<object> groups)
        {
            CommandTreeView.CommandGroup groupParent;
            var thisGroup = groups?.SingleOrDefault(item => item is CommandTreeView.CommandGroup groupItem && groupItem.GroupName.Equals(groupName));
            // Create the CommandGroup if it doesn't exist
            if (thisGroup == null)
            {
                thisGroup = new CommandTreeView.CommandGroup { GroupName = groupName };
                groups.Add(thisGroup);
            }
            // store this item for the next round
            groupParent = (CommandTreeView.CommandGroup)thisGroup;
            return groupParent;
        }


        private static Dictionary<string, List<string>>? GetShortcutMap2(CommandBinding? binding)
        {
            if (binding == null)
            {
                return null;
            }

            var shortcutKeys = new List<string>();
            foreach (var bind in binding.Sequences)
            {
                //
                shortcutKeys.Add(bind.ToString());
            }
            var shortcutGroup = new Dictionary<string, List<string>>();
            shortcutGroup.Add(binding.Scope.Name, shortcutKeys);
            return shortcutGroup;
        }

        private Dictionary<string, List<string>> GetShortcutMap(object[] bindingsObj)
        {
            var shortcutGroup = new Dictionary<string, List<string>>();

            // Process each binding string (Scope and keyCombo)
            foreach (object bindingObj in bindingsObj)
            {
                string bindingString = (string)bindingObj;

                // bindingString looks like: "Text Editor::Ctrl+R,Ctrl+M"  (Scope::Shortcut)
                const string separator = "::";
                if (bindingString.Contains("::"))
                {
                    string scopeName = bindingString.Substring(0, bindingString.IndexOf(separator));
                    string keySequence = bindingString.Substring(bindingString.IndexOf(separator) + separator.Length);

                    // Fetch the list of shortcuts for the given scope (may not exist)
                    bool success = shortcutGroup.TryGetValue(scopeName, out List<string> shortcutKeys);
                    if (!success)
                    {
                        shortcutKeys = new List<string>();
                    }
                    shortcutKeys.Add(keySequence);

                    // Update the map with the new shortcut keys (create or update)
                    shortcutGroup[scopeName] = shortcutKeys;
                }
            }

            return shortcutGroup;
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
