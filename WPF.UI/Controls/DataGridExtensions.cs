using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

namespace WPF.UI.Controls
{
    public class DataGridExtensions
    {
        public static readonly DependencyProperty BindableColumnsProperty =
            DependencyProperty.RegisterAttached(
                "BindableColumns",
                typeof(ObservableCollection<DataGridColumn>),
                typeof(DataGridExtensions),
                new PropertyMetadata(null, OnBindableColumnsChanged));

        public static ObservableCollection<DataGridColumn> GetBindableColumns(DependencyObject obj) =>
            (ObservableCollection<DataGridColumn>)obj.GetValue(BindableColumnsProperty);

        public static void SetBindableColumns(DependencyObject obj, ObservableCollection<DataGridColumn> value) =>
            obj.SetValue(BindableColumnsProperty, value);

        private static void OnBindableColumnsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DataGrid grid)
            {
                grid.Columns.Clear();
                if (e.NewValue is ObservableCollection<DataGridColumn> newColumns)
                {
                    foreach (var col in newColumns)
                        grid.Columns.Add(col);

                    newColumns.CollectionChanged += (s, ev) =>
                    {
                        grid.Columns.Clear();
                        foreach (var col in newColumns)
                            grid.Columns.Add(col);
                    };
                }
            }
        }
    }

}
