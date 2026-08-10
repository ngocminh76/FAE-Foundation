using System.Windows;
using System.Windows.Controls;

namespace WPF.UI.Controls
{
    public static class ListViewBehaviors
    {
        public static readonly DependencyProperty ScrollSelectedIntoViewProperty =
            DependencyProperty.RegisterAttached(
                "ScrollSelectedIntoView",
                typeof(bool),
                typeof(ListViewBehaviors),
                new PropertyMetadata(false, OnScrollSelectedIntoViewChanged));

        public static void SetScrollSelectedIntoView(DependencyObject element, bool value)
            => element.SetValue(ScrollSelectedIntoViewProperty, value);

        public static bool GetScrollSelectedIntoView(DependencyObject element)
            => (bool)element.GetValue(ScrollSelectedIntoViewProperty);

        private static void OnScrollSelectedIntoViewChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ListView lv)
            {
                if ((bool)e.NewValue)
                {
                    lv.SelectionChanged += (s, _) =>
                    {
                        if (lv.SelectedItem != null)
                            lv.Dispatcher.InvokeAsync(() => lv.ScrollIntoView(lv.SelectedItem));
                    };
                }
            }
        }
    }
}
