using System;
using System.Collections.ObjectModel;
using System.Linq;
using FAE.Foundation.App.Core;
using FAE.Foundation.App.Models;
using FAE.Foundation.App.Features.RibbedRaft.Calculations;

namespace FAE.Foundation.App.Features.RibbedRaft
{
    public class RibbedRaftViewModel : ObservableObject
    {
        private RibbedRaftModel _model;
        public RibbedRaftModel Model
        {
            get => _model;
            set
            {
                if (_model != null)
                {
                    _model.PropertyChanged -= Model_PropertyChanged;
                }
                if (SetProperty(ref _model, value))
                {
                    if (_model != null)
                    {
                        _model.PropertyChanged += Model_PropertyChanged;
                    }
                    RequestDraw();
                }
            }
        }

        private BoreholeModel _currentBorehole;
        public BoreholeModel CurrentBorehole
        {
            get => _currentBorehole;
            set
            {
                if (SetProperty(ref _currentBorehole, value))
                {
                    RequestDraw();
                }
            }
        }

        private ObservableCollection<BoreholeModel> _availableBoreholes;
        public ObservableCollection<BoreholeModel> AvailableBoreholes
        {
            get => _availableBoreholes;
            set => SetProperty(ref _availableBoreholes, value);
        }

        private ObservableCollection<LoadCase> _availableLoadCases;
        public ObservableCollection<LoadCase> AvailableLoadCases
        {
            get => _availableLoadCases;
            set => SetProperty(ref _availableLoadCases, value);
        }

        private LoadCase _selectedLoadCase;
        public LoadCase SelectedLoadCase
        {
            get => _selectedLoadCase;
            set
            {
                if (SetProperty(ref _selectedLoadCase, value))
                {
                    RequestDraw();
                }
            }
        }

        private GeotechCalculationResult _calculationResult;
        public GeotechCalculationResult CalculationResult
        {
            get => _calculationResult;
            set => SetProperty(ref _calculationResult, value);
        }

        private void Model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            RequestDraw();
        }

        public event Action DrawRequested;

        public RibbedRaftViewModel()
        {
            // Default Initialization
            Model = new RibbedRaftModel
            {
                Lx = 9.16,
                Ly = 9.16,
                TotalLength = 19.0,
                TotalWidth = 17.0,
                HoleSize = 2.3, // c = ROUND(l1/4, 1) = 2.3m
                SlabThickness = 0.6,
                RibWidth = 0.8,
                RibHeight = 1.8,
                B1 = 1.2,
                B2 = 1.2,
                Depth = 3.9,
                EmbedmentDepth = 2.4, // Chiều sâu chôn móng
                HasSandCushion = true,
                SandThickness = 0.5,
                HasMound = false,
                MoundHeight = 0.0,
                // Groundwater removed
            };

            // Subscribe to model property changes to trigger redraw
            Model.PropertyChanged += Model_PropertyChanged;

            var hk1 = new BoreholeModel { BoreholeName = "HK01" };
            hk1.Layers.Add(new SoilLayer { LayerId = "1", LayerName = "Bùn sét", Thickness = 2.4, GammaW = 1.72, Delta = 2.72, E0 = 1.322, Phi = 6.5, C = 0.87, E = 180, GammaDn = 0.741 });
            hk1.Layers.Add(new SoilLayer { LayerId = "2", LayerName = "Bùn sét", Thickness = 9.1, GammaW = 1.72, Delta = 2.72, E0 = 1.322, Phi = 6.5, C = 0.87, E = 180, GammaDn = 0.741 });

            AvailableBoreholes = new ObservableCollection<BoreholeModel> { hk1 };
            CurrentBorehole = hk1;

            AvailableLoadCases = new ObservableCollection<LoadCase>
            {
                new LoadCase { Name = "GIÓ 45 ĐỘ MAX (Tổ hợp kiểm tra chính)", N = 125.70, Qx = 59.84, Qy = 44.03, Mx = 1385.81, My = 2014.06, Mz = 0.0 },
                new LoadCase { Name = "GIÓ 90 ĐỘ MAX", N = 125.70, Qx = 87.30, Qy = 0.0, Mx = 0.0, My = 3141.89, Mz = 0.0 }
            };
            SelectedLoadCase = AvailableLoadCases.First();
        }

        public void RequestDraw()
        {
            if (Model != null && AvailableLoadCases != null && AvailableLoadCases.Count >= 2)
            {
                // Luôn truyền cả 2 tổ hợp — Calculator tự biện luận chi phối động
                CalculationResult = GeotechCalculator.Calculate(Model, CurrentBorehole, AvailableLoadCases[0], AvailableLoadCases[1]);
            }
            DrawRequested?.Invoke();
        }
    }
}

