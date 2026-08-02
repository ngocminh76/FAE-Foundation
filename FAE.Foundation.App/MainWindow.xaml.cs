using System.Windows;
using FAE.Foundation.App.ViewModels;

namespace FAE.Foundation.App
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