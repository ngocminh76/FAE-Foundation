using FAE.Foundation.App.Models;

namespace FAE.Foundation.App.Features.RibbedRaft
{
    public class RibbedRaftModel : FoundationBase
    {
        public override string FoundationType => "RibbedRaft";

        // --- New Inputs ---
        
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
        private double _holeSize;
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

        private double _depth; // H
        public double Depth
        {
            get => _depth;
            set => SetProperty(ref _depth, value);
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

        private bool _hasGroundwater;
        public bool HasGroundwater
        {
            get => _hasGroundwater;
            set => SetProperty(ref _hasGroundwater, value);
        }

        private double _groundwaterElev;
        public double GroundwaterElev
        {
            get => _groundwaterElev;
            set => SetProperty(ref _groundwaterElev, value);
        }

        // --- Calculated Values ---
        public double FoundationArea => TotalLength * TotalWidth - HoleSize * HoleSize;
        
        // I = B*L^3/12 - c*c^3/12. Wx = I / (L/2)
        public double Wx
        {
            get
            {
                if (TotalLength == 0) return 0;
                double Ix = (TotalWidth * Math.Pow(TotalLength, 3) / 12.0) - (HoleSize * Math.Pow(HoleSize, 3) / 12.0);
                return Math.Round(Ix / (TotalLength / 2.0), 1);
            }
        }

        public double Wy
        {
            get
            {
                if (TotalWidth == 0) return 0;
                double Iy = (TotalLength * Math.Pow(TotalWidth, 3) / 12.0) - (HoleSize * Math.Pow(HoleSize, 3) / 12.0);
                return Math.Round(Iy / (TotalWidth / 2.0), 1);
            }
        }
    }
}
