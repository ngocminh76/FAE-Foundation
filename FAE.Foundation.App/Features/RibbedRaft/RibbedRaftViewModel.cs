using System;
using FAE.Foundation.App.Core;
using FAE.Foundation.App.Models;

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
                Lx = 9.0,
                Ly = 9.0,
                TotalLength = 18.0,
                TotalWidth = 18.0,
                HoleSize = 2.3, // Math.Round(9.0 / 4, 1)
                SlabThickness = 0.6,
                RibWidth = 0.6,
                RibHeight = 1.2,
                B1 = 1.2,
                B2 = 1.2,
                Depth = 3.5,
                HasSandCushion = true,
                SandThickness = 0.5,
                HasMound = true,
                MoundHeight = 1.5,
                HasGroundwater = true,
                GroundwaterElev = -1.2
            };

            // Subscribe to model property changes to trigger redraw
            Model.PropertyChanged += Model_PropertyChanged;
        }

        public void RequestDraw()
        {
            DrawRequested?.Invoke();
        }
    }
}
