using FAE.Foundation.App.Core;

namespace FAE.Foundation.App.Models
{
    public class SoilLayer : ObservableObject
    {
        private string _layerId = string.Empty;
        public string LayerId
        {
            get => _layerId;
            set => SetProperty(ref _layerId, value);
        }

        private string _layerName = string.Empty;
        public string LayerName
        {
            get => _layerName;
            set => SetProperty(ref _layerName, value);
        }

        private double _thickness;
        public double Thickness
        {
            get => _thickness;
            set => SetProperty(ref _thickness, value);
        }

        private double _gammaW;
        public double GammaW
        {
            get => _gammaW;
            set => SetProperty(ref _gammaW, value);
        }

        private double _delta;
        public double Delta
        {
            get => _delta;
            set => SetProperty(ref _delta, value);
        }

        private double _e0;
        public double E0
        {
            get => _e0;
            set => SetProperty(ref _e0, value);
        }

        private double _phi;
        public double Phi
        {
            get => _phi;
            set => SetProperty(ref _phi, value);
        }

        private double _c;
        public double C
        {
            get => _c;
            set => SetProperty(ref _c, value);
        }

        private double _e;
        public double E
        {
            get => _e;
            set => SetProperty(ref _e, value);
        }

        private double _gammaDn;
        public double GammaDn
        {
            get => _gammaDn;
            set => SetProperty(ref _gammaDn, value);
        }

        public SoilLayer()
        {
        }
    }
}
