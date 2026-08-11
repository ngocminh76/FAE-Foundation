using ExcelDataReader;
using FAE.Foundation.App.Core;
using FAE.Foundation.App.Models;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace FAE.Foundation.App.Functions.MongBan.ViewModels
{
    public class GeoViewModel : ObservableObject
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

        private ObservableCollection<BoreholeModel> _boreholes;
        public ObservableCollection<BoreholeModel> Boreholes
        {
            get => _boreholes;
            set => SetProperty(ref _boreholes, value);
        }

        private BoreholeModel _selectedBorehole;
        public BoreholeModel SelectedBorehole
        {
            get => _selectedBorehole;
            set => SetProperty(ref _selectedBorehole, value);
        }

        private DataSet _excelDataSet;

        public ICommand BrowseCommand { get; }
        public ICommand RunCommand { get; }

        public GeoViewModel()
        {
            Sheets = new ObservableCollection<string>();
            Boreholes = new ObservableCollection<BoreholeModel>();

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
                    MessageBox.Show($"Lỗi khi đọc file Excel địa chất: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
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

                ProcessGeologyRows(rows.ToArray());

                if (Boreholes.Count > 0)
                {
                    SelectedBorehole = Boreholes.Last();
                }

                MessageBox.Show("Đã nhập dữ liệu địa chất từ Excel thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi nhập dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ProcessGeologyRows(string[] rows)
        {
            Boreholes.Clear();
            BoreholeModel currentBorehole = null;
            int hkCount = 0;

            foreach (var row in rows)
            {
                var cells = row.Split(new[] { '\t' }, StringSplitOptions.None);
                for (int i = 0; i < cells.Length; i++) cells[i] = cells[i].Replace("\r", "").Replace("\n", "").Trim();

                if (cells.Length < 3 || string.IsNullOrWhiteSpace(cells[1]) || cells[1].ToLower().Contains("tên lớp"))
                {
                    continue;
                }

                if (!double.TryParse(cells[2].Replace(",", ""), out double testTh) && !double.TryParse(cells[3].Replace(",", ""), out double testGw))
                {
                    continue;
                }

                if (currentBorehole == null)
                {
                    hkCount++;
                    currentBorehole = new BoreholeModel { BoreholeName = "HK " + hkCount };
                    Boreholes.Add(currentBorehole);
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
    }
}
