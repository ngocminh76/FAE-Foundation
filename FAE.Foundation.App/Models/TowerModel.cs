using FAE.Foundation.App.Core;
using FAE.Foundation.App.Features.RibbedRaft;

namespace FAE.Foundation.App.Models
{
    public class TowerModel : ObservableObject
    {
        private string _towerName;
        public string TowerName
        {
            get => _towerName;
            set => SetProperty(ref _towerName, value);
        }

        private double _baseDimension;
        public double BaseDimension
        {
            get => _baseDimension;
            set => SetProperty(ref _baseDimension, value);
        }

        private BoreholeModel _borehole;
        public BoreholeModel Borehole
        {
            get => _borehole;
            set => SetProperty(ref _borehole, value);
        }

        // Từng chân: Lực Nhổ Max
        private LoadCase _maxTensionLeg;
        public LoadCase MaxTensionLeg
        {
            get => _maxTensionLeg;
            set => SetProperty(ref _maxTensionLeg, value);
        }

        // Từng chân: Lực Nén Max
        private LoadCase _maxCompressionLeg;
        public LoadCase MaxCompressionLeg
        {
            get => _maxCompressionLeg;
            set => SetProperty(ref _maxCompressionLeg, value);
        }

        // Cả cột: 90 Độ Gió Max
        private LoadCase _wind90Tower;
        public LoadCase Wind90Tower
        {
            get => _wind90Tower;
            set => SetProperty(ref _wind90Tower, value);
        }

        // Cả cột: 45 Độ Gió Max
        private LoadCase _wind45Tower;
        public LoadCase Wind45Tower
        {
            get => _wind45Tower;
            set => SetProperty(ref _wind45Tower, value);
        }

        private RibbedRaftModel _foundation;
        public RibbedRaftModel Foundation
        {
            get => _foundation;
            set => SetProperty(ref _foundation, value);
        }

        public TowerModel()
        {
            TowerName = "VT01";
            Borehole = new BoreholeModel();
            
            MaxTensionLeg = new LoadCase();
            MaxCompressionLeg = new LoadCase();
            Wind90Tower = new LoadCase();
            Wind45Tower = new LoadCase();

            // Create a default foundation model
            Foundation = new RibbedRaftModel
            {
                Lx = 5.0,
                Ly = 5.0,
                TotalLength = 8.0,
                TotalWidth = 8.0,
                SlabThickness = 0.4,
                RibWidth = 0.4,
                RibHeight = 0.4,
                B1 = 0.6,
                B2 = 0.6,
                Depth = 3.5,
                HasSandCushion = true,
                SandThickness = 0.5,
                HasMound = true,
                MoundHeight = 1.5,
                HasGroundwater = true,
                GroundwaterElev = -1.2
            };
        }
    }
}
