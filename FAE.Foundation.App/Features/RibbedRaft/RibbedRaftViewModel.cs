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

            AvailableLoadCases = new ObservableCollection<LoadCase>
            {
                new LoadCase { Name = "LỰC NHỔ MAX", Qx = 23.80, Qy = 25.55, N = -198.58, Mx = -54.02, My = 90.67, Mz = -3.89 },
                new LoadCase { Name = "LỰC NÉN MAX", Qx = 32.93, Qy = 29.73, N = 260.89, Mx = -179.72, My = 107.84, Mz = -8.32 },
                new LoadCase { Name = "90 ĐỘ BT GIO MAX", Qx = 1003.91, Qy = 0.0, N = 1256.97, Mx = 0.0, My = 36131.73, Mz = 0.0 },
                new LoadCase { Name = "45 ĐỘ BT GIO MAX", Qx = 688.19, Qy = 506.39, N = 1256.97, Mx = 159368.66, My = 231616.84, Mz = 0.0 }
            };
            SelectedLoadCase = AvailableLoadCases.First();
        }

        public void RequestDraw()
        {
            if (Model != null && SelectedLoadCase != null)
            {
                CalculationResult = GeotechCalculator.Calculate(Model, CurrentBorehole, SelectedLoadCase);
            }
            DrawRequested?.Invoke();
        }
    }
}

