using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WPF.UI.AriDialog
{
    /// <summary>
    /// Interaction logic for AriDialogWd.xaml
    /// </summary>
    public partial class AriDialogWd : Window
    {
        public bool IsConfirmed { get; private set; }
        public enum MessageBoxType
        {
            OK,         // Chỉ hiển thị nút OK
            OKCancel,    // Hiển thị cả OK và Cancel
            Error,
            Warning,
            Information,
            Success
        }
        public Brush TextColor { get; private set; }

        public AriDialogWd(string message, MessageBoxType type = MessageBoxType.OK)
        {
            InitializeComponent();

            TBLOCK_Message.Text = message;
            var color = (Color)ColorConverter.ConvertFromString("#00AB55");
            var brush = new SolidColorBrush(color);
            // Xác định màu sắc dựa trên loại thông báo
            switch (type)
            {
                case MessageBoxType.Error:
                    TextColor = Brushes.Red; // Màu đỏ cho lỗi
                    break;
                case MessageBoxType.Warning:
                    TextColor = Brushes.Orange; // Màu cam cho cảnh báo
                    break;
                case MessageBoxType.Information:
                    TextColor = brush; // Màu xanh dương cho thông tin
                    break;
                case MessageBoxType.Success:
                    TextColor = brush; // Màu xanh lá cho thành công
                    break;
                case MessageBoxType.OKCancel:
                    TextColor = Brushes.Black; // Màu đen cho nội dung hỏi
                    break;
                default:
                    TextColor = Brushes.Black;
                    break;
            }
            TBLOCK_Message.Foreground = TextColor; // Gán màu sắc cho TextBlock
            // Hiển thị nút dựa trên loại
            ConfigureButtons(type);
        }

        private void ConfigureButtons(MessageBoxType type)
        {
            switch (type)
            {
                case MessageBoxType.OK:
                case MessageBoxType.Error:
                case MessageBoxType.Warning:
                case MessageBoxType.Information:
                case MessageBoxType.Success:
                    // Chỉ hiển thị nút OK
                    BTN_CANCEL.Visibility = Visibility.Collapsed;
                    break;

                case MessageBoxType.OKCancel:
                    // Hiển thị cả OK và Cancel
                    BTN_CANCEL.Visibility = Visibility.Visible;
                    break;

                default:
                    BTN_CANCEL.Visibility = Visibility.Collapsed;
                    break;
            }
        }
        private void BTN_OK_Click(object sender, RoutedEventArgs e)
        {
            IsConfirmed = true;
            this.Close();
        }

        private void BTN_CANCEL_Click(object sender, RoutedEventArgs e)
        {
            IsConfirmed = false;
            this.Close();
        }
    }
}
