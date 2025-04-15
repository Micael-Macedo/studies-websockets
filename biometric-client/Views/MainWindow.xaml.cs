using biometric_client.ViewModels;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Windows;

namespace biometric_client
{
    public partial class MainWindow : Window
    {
        

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new MainViewModel();
        }

    }
}