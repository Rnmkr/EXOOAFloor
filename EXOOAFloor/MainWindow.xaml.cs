using EXOOAFloor.ViewModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace EXOOAFloor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Title = "Consulta Serial ID 2.0 (Producción) - v." + Assembly.GetExecutingAssembly().GetName().Version.ToString();
            this.DataContext = new MainWindowViewModel(MainSnackbar.MessageQueue);
        }

        private void Search_OnKeyDown(object sender, KeyEventArgs e)
        {
            var textBox = (TextBox)sender;

            if (string.IsNullOrEmpty(textBox.Text))
                return;

            if (e.Key == Key.Enter)
                SearchButton.Command.Execute(textBox.Text);

            if (e.Key == Key.Escape)
                textBox.Text = string.Empty;
        }
    }
}
