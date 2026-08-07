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
            TowerName = "VT522-55";
            BaseDimension = 9160;
            Borehole = new BoreholeModel();
            
            MaxTensionLeg = new LoadCase {
                Name = "LỰC NHỔ MAX",
                Qx = 23.80, Qy = 25.55, N = -198.58, Mx = -54.02, My = 90.67, Mz = -3.89
            };
            MaxCompressionLeg = new LoadCase {
                Name = "LỰC NÉN MAX",
                Qx = 32.93, Qy = 29.73, N = 260.89, Mx = -179.72, My = 107.84, Mz = -8.32
            };
            Wind90Tower = new LoadCase {
                Name = "90 ĐỘ BT GIO MAX",
                Qx = 1003.91, Qy = 0.0, N = 1256.97, Mx = 0.0, My = 36131.73, Mz = 0.0
            };
            Wind45Tower = new LoadCase {
                Name = "45 ĐỘ BT GIO MAX",
                Qx = 688.19, Qy = 506.39, N = 1256.97, Mx = 159368.66, My = 231616.84, Mz = 0.0
            };

            // Create a default foundation model
            Foundation = new RibbedRaftModel
            {
                Lx = 9.16,
                Ly = 9.16,
                TotalLength = 19.0,
                TotalWidth = 17.0,
                SlabThickness = 0.6,
                RibWidth = 0.8,
                RibHeight = 1.8,
                B1 = 1.2,
                B2 = 1.2,
                Depth = 3.9,
                EmbedmentDepth = 2.4,
                HoleSize = 2.3,
                HasSandCushion = true,
                SandThickness = 0.5,
                HasMound = false,
                MoundHeight = 0.0,
                // Groundwater removed
            };
        }
    }
}

