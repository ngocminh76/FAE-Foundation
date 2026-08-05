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
                SpanX = 5.0,
                ConsLX = 1.5,
                ConsRX = 1.5,
                SpanY = 5.0,
                ConsTY = 1.5,
                ConsBY = 1.5,
                SlabThickness = 0.4,
                RibWidth = 0.4,
                RibHeight = 0.4,
                ColumnWidth = 0.6,
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
