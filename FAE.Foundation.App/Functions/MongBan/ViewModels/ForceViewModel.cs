using System;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using ExcelDataReader;
using Microsoft.Win32;
using FAE.Foundation.App.Core;
using FAE.Foundation.App.Models;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Data;

namespace FAE.Foundation.App.Functions.MongBan.ViewModels
{
    public class ForceViewModel : ObservableObject
    {
        private string _filePath;
        public string FilePath
        {
            get => _filePath;
            set => SetProperty(ref _filePath, value);
        }

        private ObservableCollection<string> _sheets;
        public ObservableCollection<string> Sheets
        {
            get => _sheets;
            set => SetProperty(ref _sheets, value);
        }

        private string _selectedSheet;
        public string SelectedSheet
        {
            get => _selectedSheet;
            set => SetProperty(ref _selectedSheet, value);
        }

        private ObservableCollection<TowerModel> _towers;
        public ObservableCollection<TowerModel> Towers
        {
            get => _towers;
            set => SetProperty(ref _towers, value);
        }

        private TowerModel _selectedTower;
        public TowerModel SelectedTower
        {
            get => _selectedTower;
            set => SetProperty(ref _selectedTower, value);
        }

        private DataSet _excelDataSet;

        private ICollectionView _towersView;
        public ICollectionView TowersView
        {
            get => _towersView;
            set => SetProperty(ref _towersView, value);
        }

        private string _searchQuery;
        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                if (SetProperty(ref _searchQuery, value))
                {
                    if (TowersView != null)
                    {
                        TowersView.Refresh();
                        OnPropertyChanged(nameof(TowersCount));
                    }
                }
            }
        }

        public int TowersCount => TowersView?.Cast<object>().Count() ?? 0;

        private DataTable _forceTable;
        public DataTable ForceTable
        {
            get => _forceTable;
            set => SetProperty(ref _forceTable, value);
        }

        public ICommand BrowseCommand { get; }
        public ICommand RunCommand { get; }

        public ForceViewModel()
        {
            Towers = new ObservableCollection<TowerModel>();
            TowersView = CollectionViewSource.GetDefaultView(Towers);
            TowersView.Filter = o =>
            {
                if (string.IsNullOrWhiteSpace(SearchQuery)) return true;
                if (o is TowerModel t)
                {
                    return t.TowerName?.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) == true;
                }
                return false;
            };

            Sheets = new ObservableCollection<string>();
            
            BrowseCommand = new RelayCommand(ExecuteBrowse);
            RunCommand = new RelayCommand(ExecuteRun, CanExecuteRun);
        }

        private void ExecuteBrowse(object parameter)
        {
            var dlg = new OpenFileDialog { Filter = "Excel Files (*.xlsx;*.xls)|*.xlsx;*.xls" };
            if (dlg.ShowDialog() == true)
            {
                try
                {
                    FilePath = dlg.FileName;
                    System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                    using (var stream = File.Open(FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        using (var reader = ExcelReaderFactory.CreateReader(stream))
                        {
                            _excelDataSet = reader.AsDataSet();
                            
                            Sheets.Clear();
                            foreach (DataTable table in _excelDataSet.Tables)
                            {
                                Sheets.Add(table.TableName);
                            }
                            
                            if (Sheets.Count > 0)
                            {
                                SelectedSheet = Sheets[0];
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi đọc file Excel tải trọng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool CanExecuteRun(object parameter)
        {
            return _excelDataSet != null && !string.IsNullOrEmpty(SelectedSheet);
        }

        private void ExecuteRun(object parameter)
        {
            if (_excelDataSet == null || string.IsNullOrEmpty(SelectedSheet))
            {
                MessageBox.Show("Vui lòng chọn file và sheet Excel trước khi nhập!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            try
            {
                var dataTable = _excelDataSet.Tables[SelectedSheet];
                if (dataTable == null) return;
                
                List<string> rows = new List<string>();
                foreach (DataRow row in dataTable.Rows)
                {
                    var cells = row.ItemArray.Select(c => (c?.ToString() ?? "").Replace("\r", " ").Replace("\n", " ").Trim()).ToArray();
                    rows.Add(string.Join("\t", cells));
                }
                
                ProcessExcelRows(rows.ToArray());
                
                if (Towers.Count > 0)
                {
                    SelectedTower = Towers.First();
                }

                OnPropertyChanged(nameof(TowersCount));

                MessageBox.Show("Đã nhập dữ liệu nội lực (tải trọng) từ Excel thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi nhập dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void ProcessExcelRows(string[] rows)
        {
            Towers.Clear();
            TowerModel currentTower = null;

            foreach (var row in rows)
            {
                var upperRow = row.ToUpper();
                if (upperRow.Contains("LOẠI CỘT"))
                {
                    currentTower = new TowerModel();
                    
                    var cells = row.Split('\t').Where(c => !string.IsNullOrWhiteSpace(c)).ToArray();
                    if (cells.Length > 1)
                    {
                        currentTower.TowerName = cells[1].Replace(";", "").Trim();
                    }
                    else
                    {
                        currentTower.TowerName = "Imported";
                    }
                    
                    Towers.Add(currentTower);
                }
                else if (currentTower != null)
                {
                    if (upperRow.Contains("NHỔ") || upperRow.Contains("NHO"))
                    {
                        currentTower.MaxTensionLeg = ParseLoadCase(row);
                    }
                    else if (upperRow.Contains("NÉN") || upperRow.Contains("NEN"))
                    {
                        currentTower.MaxCompressionLeg = ParseLoadCase(row);
                    }
                    else if (upperRow.Contains("90 ĐỘ") || upperRow.Contains("90 DO"))
                    {
                        currentTower.Wind90Tower = ParseLoadCase(row);
                    }
                    else if (upperRow.Contains("45 ĐỘ") || upperRow.Contains("45 DO"))
                    {
                        currentTower.Wind45Tower = ParseLoadCase(row);
                    }
                    else if (upperRow.Contains("QX") || upperRow.Contains("QY"))
                    {
                        var cells = row.Split('\t').Where(c => !string.IsNullOrWhiteSpace(c)).ToArray();
                        if (cells.Length > 0 && double.TryParse(cells[0].Replace(",", ""), out double dim))
                        {
                            currentTower.BaseDimension = dim;
                            double l_val = dim / 1000.0;
                            currentTower.Foundation.Lx = l_val;
                            currentTower.Foundation.Ly = l_val;
                            currentTower.Foundation.HoleSize = Math.Round(l_val / 4.0, 1);
                            
                            currentTower.Foundation.TotalWidth = l_val + 3.0;
                            currentTower.Foundation.TotalLength = l_val + 5.0;
                        }
                    }
                }
            }

            // Sinh DataTable sau khi parse xong
            GenerateDataTable();
        }

        private void GenerateDataTable()
        {
            var dt = new DataTable("Forces");
            dt.Columns.Add("Tên Cột", typeof(string));
            dt.Columns.Add("Tổ Hợp Lực", typeof(string));
            dt.Columns.Add("Qx", typeof(double));
            dt.Columns.Add("Qy", typeof(double));
            dt.Columns.Add("N", typeof(double));
            dt.Columns.Add("Mx", typeof(double));
            dt.Columns.Add("My", typeof(double));
            dt.Columns.Add("Mz", typeof(double));

            foreach (var tower in Towers)
            {
                AddLoadCaseRow(dt, tower.TowerName, "LỰC NHỔ MAX", tower.MaxTensionLeg);
                AddLoadCaseRow(dt, "", "LỰC NÉN MAX", tower.MaxCompressionLeg);
                AddLoadCaseRow(dt, "", "GIÓ 90 ĐỘ", tower.Wind90Tower);
                AddLoadCaseRow(dt, "", "GIÓ 45 ĐỘ", tower.Wind45Tower);
            }

            ForceTable = dt;
        }

        private void AddLoadCaseRow(DataTable dt, string towerName, string caseName, LoadCase lc)
        {
            if (lc == null) return;
            dt.Rows.Add(towerName, caseName, lc.Qx, lc.Qy, lc.N, lc.Mx, lc.My, lc.Mz);
        }

        private LoadCase ParseLoadCase(string row)
        {
            var cells = row.Split('\t').Where(c => !string.IsNullOrWhiteSpace(c)).ToArray();
            var lc = new LoadCase();
            
            var forceCells = cells.Skip(Math.Max(0, cells.Length - 6)).ToArray();
            
            if (forceCells.Length >= 6)
            {
                if (double.TryParse(forceCells[0].Replace(",", ""), out double qx)) lc.Qx = qx;
                if (double.TryParse(forceCells[1].Replace(",", ""), out double qy)) lc.Qy = qy;
                if (double.TryParse(forceCells[2].Replace(",", ""), out double n)) lc.N = n;
                if (double.TryParse(forceCells[3].Replace(",", ""), out double mx)) lc.Mx = mx;
                if (double.TryParse(forceCells[4].Replace(",", ""), out double my)) lc.My = my;
                if (double.TryParse(forceCells[5].Replace(",", ""), out double mz)) lc.Mz = mz;
            }
            
            return lc;
        }
    }
}
