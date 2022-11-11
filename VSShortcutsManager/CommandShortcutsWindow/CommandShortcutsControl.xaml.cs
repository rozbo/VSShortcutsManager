using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using VSShortcutsManager.CommandShortcutsWindow;

namespace VSShortcutsManager
{
    /// <summary>
    /// Interaction logic for CommandShortcutsControl.xaml
    /// </summary>
    public partial class CommandShortcutsControl : UserControl
    {


        public CommandShortcutsControlDataContext MyDataContext
        {
            get => (CommandShortcutsControlDataContext)this.DataContext;
            set => this.DataContext = (object)value;
        }
        public CommandShortcutsControl()
        {
            InitializeComponent();

        }

    }

}
