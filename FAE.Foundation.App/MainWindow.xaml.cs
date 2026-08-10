using System.Windows;
using FAE.Foundation.App.Functions.MongBan.Views;

namespace FAE.Foundation.App
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtnMongTru_Click(object sender, RoutedEventArgs e)
        {
            // TODO: mở MongTruView khi có
            MessageBox.Show("Chức năng Móng Trụ đang phát triển.", "Thông báo");
        }

        private void BtnMongBan_Click(object sender, RoutedEventArgs e)
        {
            var win = new MongBanView();
            win.Owner = this;
            win.Show();
        }

        private void BtnMongCoc_Click(object sender, RoutedEventArgs e)
        {
            // TODO: mở MongCocView khi có
            MessageBox.Show("Chức năng Móng Cọc đang phát triển.", "Thông báo");
        }
    }
}