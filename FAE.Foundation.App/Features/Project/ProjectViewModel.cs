using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Linq;
using System;
using FAE.Foundation.App.Core;
using FAE.Foundation.App.Models;
using FAE.Foundation.App.Features.RibbedRaft;

namespace FAE.Foundation.App.Features.Project
{
    public class ProjectViewModel : ObservableObject
    {
        private ObservableCollection<TowerModel> _towers;
        public ObservableCollection<TowerModel> Towers
        {
            get => _towers;
            set => SetProperty(ref _towers, value);
        }

        private ICollectionView _towersView;
        public ICollectionView TowersView 
        {
            get => _towersView;
            set => SetProperty(ref _towersView, value);
        }

        private ObservableCollection<BoreholeModel> _boreholes;
        public ObservableCollection<BoreholeModel> Boreholes
        {
            get => _boreholes;
            set => SetProperty(ref _boreholes, value);
        }

        private TowerModel _selectedTower;
        public TowerModel SelectedTower
        {
            get => _selectedTower;
            set
            {
                if (SetProperty(ref _selectedTower, value))
                {
                    UpdateDetailView();
                }
            }
        }

        private BoreholeModel _selectedBorehole;
        public BoreholeModel SelectedBorehole
        {
            get => _selectedBorehole;
            set => SetProperty(ref _selectedBorehole, value);
        }

        // We wrap the RibbedRaftViewModel so the existing UI can bind to it
        private RibbedRaftViewModel _detailViewModel;
        public RibbedRaftViewModel DetailViewModel
        {
            get => _detailViewModel;
            set => SetProperty(ref _detailViewModel, value);
        }
        
        private string _excelFilePath;
        public string ExcelFilePath
        {
            get => _excelFilePath;
            set => SetProperty(ref _excelFilePath, value);
        }

        private ObservableCollection<string> _excelSheets;
        public ObservableCollection<string> ExcelSheets
        {
            get => _excelSheets;
            set => SetProperty(ref _excelSheets, value);
        }

        private string _selectedExcelSheet;
        public string SelectedExcelSheet
        {
            get => _selectedExcelSheet;
            set => SetProperty(ref _selectedExcelSheet, value);
        }

        private string _geologyExcelFilePath;
        public string GeologyExcelFilePath
        {
            get => _geologyExcelFilePath;
            set => SetProperty(ref _geologyExcelFilePath, value);
        }

        private ObservableCollection<string> _geologyExcelSheets;
        public ObservableCollection<string> GeologyExcelSheets
        {
            get => _geologyExcelSheets;
            set => SetProperty(ref _geologyExcelSheets, value);
        }

        private string _selectedGeologyExcelSheet;
        public string SelectedGeologyExcelSheet
        {
            get => _selectedGeologyExcelSheet;
            set => SetProperty(ref _selectedGeologyExcelSheet, value);
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
                        if (TowersView.Cast<object>().FirstOrDefault() is TowerModel firstMatch)
                        {
                            SelectedTower = firstMatch;
                        }
                    }
                }
            }
        }

        public ProjectViewModel()
        {
            Towers = new ObservableCollection<TowerModel>();
            TowersView = CollectionViewSource.GetDefaultView(Towers);
            TowersView.Filter = o => 
            {
                if (string.IsNullOrWhiteSpace(SearchQuery)) return true;
                var t = o as TowerModel;
                return t != null && t.TowerName != null && t.TowerName.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase);
            };
            Boreholes = new ObservableCollection<BoreholeModel>();
            ExcelSheets = new ObservableCollection<string>();
            GeologyExcelSheets = new ObservableCollection<string>();
            DetailViewModel = new RibbedRaftViewModel();

            // Mock Data for Geology
            var hk1 = new BoreholeModel { BoreholeName = "HK01" };
            hk1.Layers.Add(new SoilLayer { LayerId = "1a", LayerName = "Lớp đất thổ cư", Thickness = 1.5, GammaW = 1.88, Delta = 2.76, E0 = 0.971, Phi = 24.93, C = 0.64, E = 1520, GammaDn = 0.893 });
            hk1.Layers.Add(new SoilLayer { LayerId = "1", LayerName = "Bùn sét, bùn", Thickness = 10.5, GammaW = 1.77, Delta = 2.72, E0 = 1.208, Phi = 6.35, C = 0.74, E = 183.33, GammaDn = 0.779 });
            
            var hk2 = new BoreholeModel { BoreholeName = "HK02" };
            hk2.Layers.Add(new SoilLayer { LayerId = "1a", LayerName = "Sét, á sét", Thickness = 2.0, GammaW = 1.9, Delta = 2.7, E0 = 0.894, Phi = 24.93, C = 0.64, E = 1520, GammaDn = 0.892 });
            hk2.Layers.Add(new SoilLayer { LayerId = "1", LayerName = "Bùn sét", Thickness = 3.5, GammaW = 1.8, Delta = 2.7, E0 = 0.943, Phi = 11.20, C = 1.27, E = 470, GammaDn = 0.885 });

            Boreholes.Add(hk1);
            Boreholes.Add(hk2);
            for (int i = 3; i <= 7; i++)
            {
                var hk = new BoreholeModel { BoreholeName = "HK" + i.ToString("D2") };
                hk.Layers.Add(new SoilLayer { LayerId = "1a", LayerName = "Sét pha", Thickness = 2.5, GammaW = 1.8, E = 1000 });
                hk.Layers.Add(new SoilLayer { LayerId = "2", LayerName = "Cát hạt trung", Thickness = 8.0, GammaW = 1.9, E = 2000 });
                Boreholes.Add(hk);
            }

            if (Boreholes.Count > 0)
            {
                SelectedBorehole = Boreholes[0];
            }

            // Removed Mock Data for Towers to ensure users see only imported data.
        }

        private void UpdateDetailView()
        {
            if (SelectedTower != null)
            {
                // Push the selected tower's foundation model into the DetailViewModel
                DetailViewModel.Model = SelectedTower.Foundation;
                DetailViewModel.CurrentBorehole = SelectedTower.Borehole;
            }
        }
    }
}
