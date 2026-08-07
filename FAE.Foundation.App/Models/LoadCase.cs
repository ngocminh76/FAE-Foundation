using FAE.Foundation.App.Core;

namespace FAE.Foundation.App.Models
{
    public class LoadCase : ObservableObject
    {
        private string _name;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private double _qx;
        public double Qx
        {
            get => _qx;
            set => SetProperty(ref _qx, value);
        }

        private double _qy;
        public double Qy
        {
            get => _qy;
            set => SetProperty(ref _qy, value);
        }

        private double _n;
        public double N
        {
            get => _n;
            set => SetProperty(ref _n, value);
        }

        private double _mx;
        public double Mx
        {
            get => _mx;
            set => SetProperty(ref _mx, value);
        }

        private double _my;
        public double My
        {
            get => _my;
            set => SetProperty(ref _my, value);
        }

        private double _mz;
        public double Mz
        {
            get => _mz;
            set => SetProperty(ref _mz, value);
        }

        public LoadCase(double qx = 0, double qy = 0, double n = 0, double mx = 0, double my = 0, double mz = 0)
        {
            Qx = qx;
            Qy = qy;
            N = n;
            Mx = mx;
            My = my;
            Mz = mz;
        }
    }
}
