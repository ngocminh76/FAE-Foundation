using System;
using System.Threading;
using System.Windows;
using static WPF.UI.AriDialog.AriDialogWd;

namespace WPF.UI.AriDialog
{
    public class AriMsg
    {
        public static void EnsureApplicationInitialized()
        {
            if (Application.Current == null)
            {
                // Nếu chưa có Application, hãy khởi tạo Dispatcher cho WPF
                var app = new Application
                {
                    ShutdownMode = ShutdownMode.OnExplicitShutdown 
                    // Đảm bảo Application không tắt khi không cần thiết
                };
            }
        }

        public static void ShowError(string message, string error = "Error")
        {
            var thread = new Thread(() =>
            {
                EnsureApplicationInitialized();
                var messageBox = new AriDialogWd(message, MessageBoxType.Error)
                {
                    Title = error,
                    Topmost = true,
                };
                messageBox.ShowDialog();
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        public static void ShowError(Exception ex, string error = "Error")
        {
            var thread = new Thread(() =>
            {
                EnsureApplicationInitialized();
                var messageBox = new AriDialogWd(ex.Message, MessageBoxType.Error)
                {
                    Title = error,
                    Topmost = true,
                };
                messageBox.ShowDialog();
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        //public static void ShowWarning(string message,string warning ="Warning")
        //{
        //    EnsureApplicationInitialized();
        //    var messageBox = new AriDialogWd(message, MessageBoxType.Warning)
        //    {
        //        Title = warning,
        //        Topmost = true,
        //    };
        //    messageBox.ShowDialog();
        //}
        public static void ShowWarning(string message, string warning = "Warning")
        {
            var thread = new Thread(() =>
            {
                EnsureApplicationInitialized();

                var messageBox = new AriDialogWd(message, MessageBoxType.Warning)
                {
                    Title = warning,
                    Topmost = true
                };
                messageBox.ShowDialog();
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        /// <summary>
        /// Phiên bản an toàn UI: hiển thị trên dispatcher, fallback MessageBox nếu không có dispatcher.
        /// Dùng khi gọi từ background thread để tránh treo.
        /// </summary>
        public static void ShowWarningSafe(string message, string warning = "Warning")
        {
            EnsureApplicationInitialized();

            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher == null)
            {
                MessageBox.Show(message, warning, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            dispatcher.InvokeAsync(() =>
            {
                var messageBox = new AriDialogWd(message, MessageBoxType.Warning)
                {
                    Title = warning,
                    Topmost = true
                };
                messageBox.ShowDialog();
            });
        }


        public static void ShowInformation(string message, string title = "Information")
        {
            var thread = new Thread(() =>
            {
                EnsureApplicationInitialized();
                var messageBox = new AriDialogWd(message, MessageBoxType.Information)
                {
                    Title = title,
                    Topmost = true,
                };
                messageBox.ShowDialog();
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        public static void ShowSuccess(string message, string title = "Success", int autoCloseMs = 0)
        {
            var thread = new Thread(() =>
            {
                EnsureApplicationInitialized();
                var messageBox = new AriDialogWd(message, MessageBoxType.Success)
                {
                    Title = title,
                    Topmost = true,
                };
                if (autoCloseMs > 0)
                {
                    var timer = new System.Windows.Threading.DispatcherTimer();
                    timer.Interval = TimeSpan.FromMilliseconds(autoCloseMs);
                    timer.Tick += (s, e) =>
                    {
                        timer.Stop();
                        try { messageBox.Close(); } catch { }
                    };
                    timer.Start();
                }
                messageBox.ShowDialog();
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        public static bool ShowQuestion(string message, string question = "Question")
        {
            bool result = false;
            var thread = new Thread(() =>
            {
                EnsureApplicationInitialized();
                var messageBox = new AriDialogWd(message, MessageBoxType.OKCancel)
                {
                    Title = question,
                    Topmost = true,
                };
                messageBox.ShowDialog();
                result = messageBox.IsConfirmed;
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join(); // Wait for the dialog to close
            return result;
        }
    }


}
