using System.Collections.ObjectModel;
using FAE.Foundation.App.Core;

namespace FAE.Foundation.App.Models
{
    public class BoreholeModel : ObservableObject
    {
        private string _boreholeName;
        public string BoreholeName
        {
            get => _boreholeName;
            set => SetProperty(ref _boreholeName, value);
        }

        private ObservableCollection<SoilLayer> _layers;
        public ObservableCollection<SoilLayer> Layers
        {
            get => _layers;
            set => SetProperty(ref _layers, value);
        }

        public BoreholeModel()
        {
            BoreholeName = "HK01";
            Layers = new ObservableCollection<SoilLayer>();
        }
    }
}
