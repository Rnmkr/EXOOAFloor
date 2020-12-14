using EXOOAFloor.ViewModel;
using System.Windows.Controls;

namespace EXOOAFloor.View.Dialog
{
    /// <summary>
    /// Interaction logic for LogsDialog.xaml
    /// </summary>
    public partial class LogsDialog : UserControl
    {
        public LogsDialog()
        {
            InitializeComponent();
            this.DataContext = new LogsDialogViewModel();
        }
    }
}
