using System;
using FAE.Foundation.App.Core;

namespace FAE.Foundation.App.Features.RibbedRaft
{
    public class RibbedRaftViewModel : ObservableObject
    {
        private RibbedRaftModel _model;
        public RibbedRaftModel Model
        {
            get => _model;
            set => SetProperty(ref _model, value);
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
            Model.PropertyChanged += (s, e) => RequestDraw();
        }

        public void RequestDraw()
        {
            DrawRequested?.Invoke();
        }
    }
}
