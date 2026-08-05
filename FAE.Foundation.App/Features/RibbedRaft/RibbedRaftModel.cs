using FAE.Foundation.App.Models;

namespace FAE.Foundation.App.Features.RibbedRaft
{
    public class RibbedRaftModel : FoundationBase
    {
        public override string FoundationType => "RibbedRaft";

        // X Direction
        private double _spanX;
        public double SpanX
        {
            get => _spanX;
            set { SetProperty(ref _spanX, value); OnPropertyChanged(nameof(TotalLength)); OnPropertyChanged(nameof(FoundationName)); }
        }

        private double _consLX;
        public double ConsLX
        {
            get => _consLX;
            set { SetProperty(ref _consLX, value); OnPropertyChanged(nameof(TotalLength)); OnPropertyChanged(nameof(FoundationName)); }
        }

        private double _consRX;
        public double ConsRX
        {
            get => _consRX;
            set { SetProperty(ref _consRX, value); OnPropertyChanged(nameof(TotalLength)); OnPropertyChanged(nameof(FoundationName)); }
        }

        // Y Direction
        private double _spanY;
        public double SpanY
        {
            get => _spanY;
            set { SetProperty(ref _spanY, value); OnPropertyChanged(nameof(TotalWidth)); OnPropertyChanged(nameof(FoundationName)); }
        }

        private double _consTY;
        public double ConsTY
        {
            get => _consTY;
            set { SetProperty(ref _consTY, value); OnPropertyChanged(nameof(TotalWidth)); OnPropertyChanged(nameof(FoundationName)); }
        }

        private double _consBY;
        public double ConsBY
        {
            get => _consBY;
            set { SetProperty(ref _consBY, value); OnPropertyChanged(nameof(TotalWidth)); OnPropertyChanged(nameof(FoundationName)); }
        }

        // Components
        private double _baseDimension;
        public double BaseDimension
        {
            get => _baseDimension;
            set { SetProperty(ref _baseDimension, value); OnPropertyChanged(nameof(FoundationName)); }
        }

        private double _holeSize;
        public double HoleSize
        {
            get => _holeSize;
            set => SetProperty(ref _holeSize, value);
        }

        public string FoundationName => $"MB{BaseDimension}-{TotalWidth}x{TotalLength}";

        private double _slabThickness;
        public double SlabThickness
        {
            get => _slabThickness;
            set => SetProperty(ref _slabThickness, value);
        }

        private double _ribWidth;
        public double RibWidth
        {
            get => _ribWidth;
            set => SetProperty(ref _ribWidth, value);
        }

        private double _ribHeight;
        public double RibHeight
        {
            get => _ribHeight;
            set => SetProperty(ref _ribHeight, value);
        }

        private double _columnWidth;
        public double ColumnWidth
        {
            get => _columnWidth;
            set => SetProperty(ref _columnWidth, value);
        }

        private double _depth;
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

        public double TotalLength => CalculateTotalLength();
        public double TotalWidth => CalculateTotalWidth();

        protected virtual double CalculateTotalLength()
        {
            return SpanX + ConsLX + ConsRX;
        }

        protected virtual double CalculateTotalWidth()
        {
            return SpanY + ConsTY + ConsBY;
        }
    }
}
