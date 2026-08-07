using FAE.Foundation.App.Models;

namespace FAE.Foundation.App.Features.RibbedRaft
{
    public class RibbedRaftModel : FoundationBase
    {
        public override string FoundationType => "RibbedRaft";

        // --- New Inputs ---
        
        private double _towerBaseDimension = 9160;
        public double TowerBaseDimension
        {
            get => _towerBaseDimension;
            set => SetProperty(ref _towerBaseDimension, value);
        }

        private double _lx;
        public double Lx
        {
            get => _lx;
            set { SetProperty(ref _lx, value); OnPropertyChanged(nameof(ConsLX)); OnPropertyChanged(nameof(ConsRX)); OnPropertyChanged(nameof(SpanX)); OnPropertyChanged(nameof(FoundationName)); }
        }

        private double _ly;
        public double Ly
        {
            get => _ly;
            set { SetProperty(ref _ly, value); OnPropertyChanged(nameof(ConsTY)); OnPropertyChanged(nameof(ConsBY)); OnPropertyChanged(nameof(SpanY)); OnPropertyChanged(nameof(FoundationName)); }
        }

        private double _totalLength;
        public double TotalLength
        {
            get => _totalLength;
            set { SetProperty(ref _totalLength, value); OnPropertyChanged(nameof(ConsLX)); OnPropertyChanged(nameof(ConsRX)); OnPropertyChanged(nameof(FoundationName)); OnPropertyChanged(nameof(FoundationArea)); OnPropertyChanged(nameof(Wx)); OnPropertyChanged(nameof(Wy)); }
        }

        private double _totalWidth;
        public double TotalWidth
        {
            get => _totalWidth;
            set { SetProperty(ref _totalWidth, value); OnPropertyChanged(nameof(ConsTY)); OnPropertyChanged(nameof(ConsBY)); OnPropertyChanged(nameof(FoundationName)); OnPropertyChanged(nameof(FoundationArea)); OnPropertyChanged(nameof(Wx)); OnPropertyChanged(nameof(Wy)); }
        }

        // --- Computed Properties for Drawers ---
        public double SpanX => Lx;
        public double SpanY => Ly;
        public double ConsLX => (TotalLength - Lx) / 2.0;
        public double ConsRX => (TotalLength - Lx) / 2.0;
        public double ConsTY => (TotalWidth - Ly) / 2.0;
        public double ConsBY => (TotalWidth - Ly) / 2.0;

        // --- Other Inputs ---
        private double _holeSize = 2.3;
        public double HoleSize
        {
            get => _holeSize;
            set { SetProperty(ref _holeSize, value); OnPropertyChanged(nameof(FoundationArea)); OnPropertyChanged(nameof(Wx)); OnPropertyChanged(nameof(Wy)); }
        }

        public string FoundationName => $"MB{Lx}-{TotalWidth}x{TotalLength}";

        private double _slabThickness; // h1 or c in image? Image says h1 = 0.6, c = 2.3
        public double SlabThickness
        {
            get => _slabThickness;
            set => SetProperty(ref _slabThickness, value);
        }

        private double _ribWidth; // a
        public double RibWidth
        {
            get => _ribWidth;
            set => SetProperty(ref _ribWidth, value);
        }

        private double _ribHeight; // h
        public double RibHeight
        {
            get => _ribHeight;
            set => SetProperty(ref _ribHeight, value);
        }

        private double _b1;
        public double B1
        {
            get => _b1;
            set => SetProperty(ref _b1, value);
        }

        private double _b2;
        public double B2
        {
            get => _b2;
            set => SetProperty(ref _b2, value);
        }
        
        // Keep ColumnWidth for compatibility with drawers, defaulting to B1
        public double ColumnWidth => B1;

        private double _depth; // H (total height)
        public double Depth
        {
            get => _depth;
            set => SetProperty(ref _depth, value);
        }

        private double _embedmentDepth; // Chiều sâu chôn móng
        public double EmbedmentDepth
        {
            get => _embedmentDepth;
            set => SetProperty(ref _embedmentDepth, value);
        }

        private bool _hasMound;
        public bool HasMound
        {
            get => _hasMound;
            set => SetProperty(ref _hasMound, value);
        }

        private double _moundHeight;
        public double MoundHeight
        {
            get => _moundHeight;
            set => SetProperty(ref _moundHeight, value);
        }

        private bool _hasSandCushion;
        public bool HasSandCushion
        {
            get => _hasSandCushion;
            set => SetProperty(ref _hasSandCushion, value);
        }

        private double _sandThickness;
        public double SandThickness
        {
            get => _sandThickness;
            set => SetProperty(ref _sandThickness, value);
        }

        // Removed MNN properties as requested

        // --- Calculated Values ---
        // Area = B*L - c^2 (Trừ đi diện tích đục lỗ c x c)
        public double FoundationArea => TotalLength * TotalWidth - HoleSize * HoleSize;
        
        // Wx = (L*B^3 - c^4) / (6*B)
        public double Wx
        {
            get
            {
                if (TotalWidth == 0) return 0;
                double Ix = (TotalLength * Math.Pow(TotalWidth, 3) - Math.Pow(HoleSize, 4)) / 12.0;
                return Math.Round(Ix / (TotalWidth / 2.0), 2);
            }
        }

        // Wy = (B*L^3 - c^4) / (6*L)
        public double Wy
        {
            get
            {
                if (TotalLength == 0) return 0;
                double Iy = (TotalWidth * Math.Pow(TotalLength, 3) - Math.Pow(HoleSize, 4)) / 12.0;
                return Math.Round(Iy / (TotalLength / 2.0), 2);
            }
        }
    }
}
