using System.Windows;
using System.Windows.Controls;

namespace WPF.UI.Themes
{
    /// <summary>
    /// Interaction logic for LabelBlock.xaml
    /// </summary>
    public partial class LabelBlock : UserControl
    {
        public LabelBlock()
        {
            //InitializeComponent();
        }

        public string LabelJP
        {
            get => (string)GetValue(LabelJPProperty);
            set => SetValue(LabelJPProperty, value);
        }

        public static readonly DependencyProperty LabelJPProperty =
            DependencyProperty.Register("LabelJP", typeof(string), typeof(LabelBlock), new PropertyMetadata(""));

        public string LabelEN
        {
            get => (string)GetValue(LabelENProperty);
            set => SetValue(LabelENProperty, value);
        }

        public static readonly DependencyProperty LabelENProperty =
            DependencyProperty.Register("LabelEN", typeof(string), typeof(LabelBlock), new PropertyMetadata(""));
    }
}
