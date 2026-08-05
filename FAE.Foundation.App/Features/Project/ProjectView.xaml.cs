using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using Microsoft.Win32;
using ExcelDataReader;
using FAE.Foundation.App.Models;

namespace FAE.Foundation.App.Features.Project
{
    public partial class ProjectView : UserControl
    {
        public ProjectView()
        {
            InitializeComponent();
        }

        private void PasteButton_Click(object sender, RoutedEventArgs e)
        {
            var vm = this.DataContext as ProjectViewModel;
            if (vm == null) return;

            string clipboardData = Clipboard.GetText();
            if (string.IsNullOrEmpty(clipboardData)) return;

            string[] rows = clipboardData.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            ProcessExcelRows(rows, vm);
        }
        
        private System.Data.DataSet _currentExcelDataSet;
        private System.Data.DataSet _currentGeologyExcelDataSet;

        private void BrowseExcel_Click(object sender, RoutedEventArgs e)
        {
            var vm = this.DataContext as ProjectViewModel;
            if (vm == null) return;
            
            var dlg = new OpenFileDialog { Filter = "Excel Files (*.xlsx;*.xls)|*.xlsx;*.xls" };
            if (dlg.ShowDialog() == true)
            {
                try
                {
                    vm.ExcelFilePath = dlg.FileName;
                    System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                    using (var stream = File.Open(dlg.FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        using (var reader = ExcelReaderFactory.CreateReader(stream))
                        {
                            _currentExcelDataSet = reader.AsDataSet();
                            
                            vm.ExcelSheets.Clear();
                            foreach (System.Data.DataTable table in _currentExcelDataSet.Tables)
                            {
                                vm.ExcelSheets.Add(table.TableName);
                            }
                            
                            if (vm.ExcelSheets.Count > 0)
                            {
                                vm.SelectedExcelSheet = vm.ExcelSheets[0];
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi đọc file Excel: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        
        private void ImportExcel_Click(object sender, RoutedEventArgs e)
        {
            var vm = this.DataContext as ProjectViewModel;
            if (vm == null) return;
            
            if (_currentExcelDataSet == null || string.IsNullOrEmpty(vm.SelectedExcelSheet))
            {
                MessageBox.Show("Vui lòng chọn file và sheet Excel trước khi nhập!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            try
            {
                var dataTable = _currentExcelDataSet.Tables[vm.SelectedExcelSheet];
                if (dataTable == null) return;
                
                List<string> rows = new List<string>();
                foreach (System.Data.DataRow row in dataTable.Rows)
                {
                    // Remove newlines inside cells so they don't break string.Join("\t")
                    var cells = row.ItemArray.Select(c => (c?.ToString() ?? "").Replace("\r", " ").Replace("\n", " ").Trim()).ToArray();
                    rows.Add(string.Join("\t", cells));
                }
                
                ProcessExcelRows(rows.ToArray(), vm);
                MessageBox.Show("Đã nhập dữ liệu nội lực từ Excel thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi nhập dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void ProcessExcelRows(string[] rows, ProjectViewModel vm)
        {
            TowerModel currentTower = null;

            foreach (var row in rows)
            {
                var upperRow = row.ToUpper();
                if (upperRow.Contains("LOẠI CỘT"))
                {
                    // New tower block starts
                    currentTower = new TowerModel();
                    
                    // Try to extract tower name
                    var cells = row.Split('\t').Where(c => !string.IsNullOrWhiteSpace(c)).ToArray();
                    if (cells.Length > 1)
                    {
                        currentTower.TowerName = cells[1].Replace(";", "").Trim();
                    }
                    else
                    {
                        currentTower.TowerName = "Imported";
                    }
                    
                    if (vm.Boreholes.Count > 0)
                    {
                        currentTower.Borehole = vm.Boreholes[0];
                    }
                    
                    vm.Towers.Add(currentTower);
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
                            currentTower.Foundation.BaseDimension = dim / 1000.0;
                            currentTower.Foundation.SpanX = dim / 1000.0;
                            currentTower.Foundation.SpanY = dim / 1000.0;
                            currentTower.Foundation.HoleSize = Math.Round((dim / 1000.0) / 4.0, 1);
                            
                            // Set Cons so TotalLength - TotalWidth = 2m
                            currentTower.Foundation.ConsTY = 1.5;
                            currentTower.Foundation.ConsBY = 1.5;
                            currentTower.Foundation.ConsLX = 2.5;
                            currentTower.Foundation.ConsRX = 2.5;
                        }
                    }
                }
            }
        }

        private LoadCase ParseLoadCase(string row)
        {
            var cells = row.Split('\t').Where(c => !string.IsNullOrWhiteSpace(c)).ToArray();
            var lc = new LoadCase();
            
            // Expected Excel format puts Qx, Qy, N, Mx, My, Mz at the end.
            var forceCells = cells.Skip(Math.Max(0, cells.Length - 6)).ToArray();
            
            if (forceCells.Length >= 6)
            {
                // Remove commas from numbers if they exist (e.g. 97,333.12 -> 97333.12)
                if (double.TryParse(forceCells[0].Replace(",", ""), out double qx)) lc.Qx = qx;
                if (double.TryParse(forceCells[1].Replace(",", ""), out double qy)) lc.Qy = qy;
                if (double.TryParse(forceCells[2].Replace(",", ""), out double n)) lc.N = n;
                if (double.TryParse(forceCells[3].Replace(",", ""), out double mx)) lc.Mx = mx;
                if (double.TryParse(forceCells[4].Replace(",", ""), out double my)) lc.My = my;
                if (double.TryParse(forceCells[5].Replace(",", ""), out double mz)) lc.Mz = mz;
            }
            
            return lc;
        }
        
        private void AddEmptyBorehole_Click(object sender, RoutedEventArgs e)
        {
            var vm = this.DataContext as ProjectViewModel;
            if (vm == null) return;
            
            var hk = new BoreholeModel { BoreholeName = "HK " + (vm.Boreholes.Count + 1) };
            vm.Boreholes.Add(hk);
            vm.SelectedBorehole = hk;
        }

        private void PasteGeologyButton_Click(object sender, RoutedEventArgs e)
        {
            var vm = this.DataContext as ProjectViewModel;
            if (vm == null) return;

            string clipboardData = Clipboard.GetText();
            if (string.IsNullOrEmpty(clipboardData)) return;

            string[] rows = clipboardData.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            ProcessGeologyRows(rows, vm);
            
            if (vm.Boreholes.Count > 0)
                vm.SelectedBorehole = vm.Boreholes.Last();
        }

        private void BrowseGeologyExcel_Click(object sender, RoutedEventArgs e)
        {
            var vm = this.DataContext as ProjectViewModel;
            if (vm == null) return;
            
            var dlg = new OpenFileDialog { Filter = "Excel Files (*.xlsx;*.xls)|*.xlsx;*.xls" };
            if (dlg.ShowDialog() == true)
            {
                try
                {
                    vm.GeologyExcelFilePath = dlg.FileName;
                    System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                    using (var stream = File.Open(dlg.FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        using (var reader = ExcelReaderFactory.CreateReader(stream))
                        {
                            _currentGeologyExcelDataSet = reader.AsDataSet();
                            
                            vm.GeologyExcelSheets.Clear();
                            foreach (System.Data.DataTable table in _currentGeologyExcelDataSet.Tables)
                            {
                                vm.GeologyExcelSheets.Add(table.TableName);
                            }
                            
                            if (vm.GeologyExcelSheets.Count > 0)
                            {
                                vm.SelectedGeologyExcelSheet = vm.GeologyExcelSheets[0];
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi đọc file Excel địa chất: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ImportGeologyExcel_Click(object sender, RoutedEventArgs e)
        {
            var vm = this.DataContext as ProjectViewModel;
            if (vm == null) return;
            
            if (_currentGeologyExcelDataSet == null || string.IsNullOrEmpty(vm.SelectedGeologyExcelSheet))
            {
                MessageBox.Show("Vui lòng chọn file và sheet Excel trước khi nhập!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            try
            {
                var dataTable = _currentGeologyExcelDataSet.Tables[vm.SelectedGeologyExcelSheet];
                if (dataTable == null) return;
                
                List<string> rows = new List<string>();
                foreach (System.Data.DataRow row in dataTable.Rows)
                {
                    var cells = row.ItemArray.Select(c => (c?.ToString() ?? "").Replace("\r", " ").Replace("\n", " ").Trim()).ToArray();
                    rows.Add(string.Join("\t", cells));
                }
                
                ProcessGeologyRows(rows.ToArray(), vm);
                
                if (vm.Boreholes.Count > 0)
                    vm.SelectedBorehole = vm.Boreholes.Last();
                    
                MessageBox.Show("Đã nhập dữ liệu địa chất từ Excel thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi nhập dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ProcessGeologyRows(string[] rows, ProjectViewModel vm)
        {
            BoreholeModel currentBorehole = null;
            int hkCount = vm.Boreholes.Count;
            
            foreach (var row in rows)
            {
                var cells = row.Split(new[] { '\t' }, StringSplitOptions.None);
                for(int i=0; i<cells.Length; i++) cells[i] = cells[i].Replace("\r", "").Replace("\n", "").Trim();
                
                // Skip empty or header rows
                if (cells.Length < 3 || string.IsNullOrWhiteSpace(cells[1]) || cells[1].ToLower().Contains("tên lớp"))
                {
                    continue;
                }
                
                // If the row contains "Kiểm tra khả năng chịu tải" or similar, we should break or skip, 
                // but checking string.IsNullOrWhiteSpace(cells[1]) handles many empty layer name cases.
                // We'll also skip if cells[2] (Thickness) is not a number.
                if (!double.TryParse(cells[2].Replace(",", ""), out double testTh) && !double.TryParse(cells[3].Replace(",", ""), out double testGw))
                {
                    continue; // Skip non-data rows
                }

                if (currentBorehole == null)
                {
                    hkCount++;
                    currentBorehole = new BoreholeModel { BoreholeName = "HK " + hkCount };
                    vm.Boreholes.Add(currentBorehole);
                }

                var soil = new SoilLayer();
                soil.LayerId = cells[0];
                soil.LayerName = cells[1];
                
                if (cells.Length > 2 && double.TryParse(cells[2].Replace(",", ""), out double th)) soil.Thickness = th;
                if (cells.Length > 3 && double.TryParse(cells[3].Replace(",", ""), out double gw)) soil.GammaW = gw;
                if (cells.Length > 4 && double.TryParse(cells[4].Replace(",", ""), out double delta)) soil.Delta = delta;
                if (cells.Length > 5 && double.TryParse(cells[5].Replace(",", ""), out double e0)) soil.E0 = e0;
                if (cells.Length > 6 && double.TryParse(cells[6].Replace(",", ""), out double phi)) soil.Phi = phi;
                if (cells.Length > 7 && double.TryParse(cells[7].Replace(",", ""), out double c)) soil.C = c;
                if (cells.Length > 8 && double.TryParse(cells[8].Replace(",", ""), out double ev)) soil.E = ev;
                if (cells.Length > 9 && double.TryParse(cells[9].Replace(",", ""), out double gdn)) soil.GammaDn = gdn;

                currentBorehole.Layers.Add(soil);
            }
        }
        
        // --- SAVE / LOAD DATA ---
        
        private class ProjectState 
        {
            public List<TowerModel> Towers { get; set; } = new List<TowerModel>();
            public List<BoreholeModel> Boreholes { get; set; } = new List<BoreholeModel>();
        }

        private void SaveDataButton_Click(object sender, RoutedEventArgs e)
        {
            var vm = this.DataContext as ProjectViewModel;
            if (vm == null) return;

            var dlg = new SaveFileDialog { Filter = "FAE Project (*.fae)|*.fae", DefaultExt = ".fae" };
            if (dlg.ShowDialog() == true)
            {
                try
                {
                    var state = new ProjectState 
                    { 
                        Towers = vm.Towers.ToList(), 
                        Boreholes = vm.Boreholes.ToList() 
                    };
                    
                    var json = JsonSerializer.Serialize(state, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(dlg.FileName, json);
                    MessageBox.Show("Đã lưu dự án thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi lưu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void LoadDataButton_Click(object sender, RoutedEventArgs e)
        {
            var vm = this.DataContext as ProjectViewModel;
            if (vm == null) return;

            var dlg = new OpenFileDialog { Filter = "FAE Project (*.fae)|*.fae", DefaultExt = ".fae" };
            if (dlg.ShowDialog() == true)
            {
                try
                {
                    var json = File.ReadAllText(dlg.FileName);
                    var state = JsonSerializer.Deserialize<ProjectState>(json);
                    
                    if (state != null)
                    {
                        vm.Boreholes.Clear();
                        foreach (var b in state.Boreholes) vm.Boreholes.Add(b);
                        
                        vm.Towers.Clear();
                        foreach (var t in state.Towers) 
                        {
                            // Reconnect borehole references
                            if (t.Borehole != null)
                            {
                                t.Borehole = vm.Boreholes.FirstOrDefault(b => b.BoreholeName == t.Borehole.BoreholeName) ?? t.Borehole;
                            }
                            vm.Towers.Add(t);
                        }
                        
                        if (vm.Towers.Any()) vm.SelectedTower = vm.Towers[0];
                        if (vm.Boreholes.Any()) vm.SelectedBorehole = vm.Boreholes[0];
                        
                        MessageBox.Show("Đã mở dự án thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi mở: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
