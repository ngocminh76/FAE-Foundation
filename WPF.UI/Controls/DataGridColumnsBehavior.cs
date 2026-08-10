using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace WPF.UI.Controls
{
    public static class DataGridColumnsBehavior
    {
        public static readonly DependencyProperty BindableColumnsProperty =
            DependencyProperty.RegisterAttached(
                "BindableColumns",
                typeof(ObservableCollection<DataGridColumn>),
                typeof(DataGridColumnsBehavior),
                new UIPropertyMetadata(null, OnBindableColumnsChanged));

        public static ObservableCollection<DataGridColumn> GetBindableColumns(DependencyObject obj)
            => (ObservableCollection<DataGridColumn>)obj.GetValue(BindableColumnsProperty);

        public static void SetBindableColumns(DependencyObject obj, ObservableCollection<DataGridColumn> value)
            => obj.SetValue(BindableColumnsProperty, value);

        private static void OnBindableColumnsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var dataGrid = d as DataGrid;
            if (dataGrid == null) return;

            dataGrid.Columns.Clear();

            var newColumns = e.NewValue as ObservableCollection<DataGridColumn>;
            if (newColumns == null) return;

            foreach (var col in newColumns)
            {
                dataGrid.Columns.Add(col);
            }

            newColumns.CollectionChanged += (s, args) =>
            {
                if (args.NewItems != null)
                {
                    foreach (DataGridColumn col in args.NewItems)
                    {
                        dataGrid.Columns.Add(col);
                    }
                }

                if (args.OldItems != null)
                {
                    foreach (DataGridColumn col in args.OldItems)
                    {
                        dataGrid.Columns.Remove(col);
                    }
                }
            };
        }

    }
}
