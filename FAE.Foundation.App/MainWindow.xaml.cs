using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media.Imaging;
using System;

namespace FAE.Foundation.App
{
    public partial class MainWindow : Window
    {
        public ObservableCollection<LoadData> Loads { get; set; }
        public ObservableCollection<SoilData> Soils { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            
            Loads = new ObservableCollection<LoadData>
            {
                new LoadData { Name = "Chân 1", N = 1250.0, Mx = 240.5, My = 15.0, Qx = 50.2, Qy = 10.5 },
                new LoadData { Name = "Chân 2", N = 1300.0, Mx = 210.0, My = -10.0, Qx = 45.0, Qy = -12.0 },
                new LoadData { Name = "Chân 3", N = 1280.0, Mx = -220.5, My = 18.0, Qx = -48.0, Qy = 11.0 },
                new LoadData { Name = "Chân 4", N = 1220.0, Mx = -200.0, My = -15.0, Qx = -42.0, Qy = -10.0 }
            };

            Soils = new ObservableCollection<SoilData>
            {
                new SoilData { Name = "Lớp 1: Sét dẻo", Thickness = 2.5, Gamma = 18.5, C = 15.0, Phi = 10.0, E0 = 5000 },
                new SoilData { Name = "Lớp 2: Cát trung", Thickness = 5.5, Gamma = 19.2, C = 0.0, Phi = 30.0, E0 = 15000 },
                new SoilData { Name = "Lớp 3: Sét nửa cứng", Thickness = 12.0, Gamma = 19.8, C = 25.0, Phi = 18.0, E0 = 22000 }
            };

            this.DataContext = this;
            
            try
            {
                // Try to load the actual generated images for maximum fidelity
                string basePath = @"C:\Users\qnbk1\.gemini\antigravity\brain\532fcab8-feeb-4929-b8e3-e0fffa788c40";
                Canvas2DImage.Source = new BitmapImage(new Uri($@"{basePath}\final_hybrid_foundation.png", UriKind.Absolute));
                Canvas3DImage.Source = new BitmapImage(new Uri($@"{basePath}\force_moment_diagram.png", UriKind.Absolute));
            }
            catch { }
        }
    }

    public class LoadData
    {
        public string Name { get; set; }
        public double N { get; set; }
        public double Mx { get; set; }
        public double My { get; set; }
        public double Qx { get; set; }
        public double Qy { get; set; }
    }

    public class SoilData
    {
        public string Name { get; set; }
        public double Thickness { get; set; }
        public double Gamma { get; set; }
        public double C { get; set; }
        public double Phi { get; set; }
        public double E0 { get; set; }
    }
}