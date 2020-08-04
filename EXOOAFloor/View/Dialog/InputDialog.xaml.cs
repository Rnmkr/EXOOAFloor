using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Input;

namespace EXOOAFloor.View.Dialog
{
    /// <summary>
    /// Interaction logic for InputDataDialog.xaml
    /// </summary>
    public partial class InputDialog : UserControl
    {
        Regex regex = new Regex("[^0-9]+");
        public InputDialog()
        {
            InitializeComponent();
        }
        private void NumberValidationTextBox(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(InputBox.Text))
            {
                btnAccept.IsEnabled = false;
                return;
            }

            if (!regex.IsMatch(InputBox.Text))
            {
                btnAccept.IsEnabled = true;
            }
            else
            {
                btnAccept.IsEnabled = false;
            }
        }

        private void InputBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
