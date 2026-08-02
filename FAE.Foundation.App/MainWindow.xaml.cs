using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media.Imaging;
using System;
using System.Windows.Controls;

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
        }



        private bool _isDragging3D = false;
        private Point _lastMousePos;

        private void Viewport3D_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _isDragging3D = true;
            _lastMousePos = e.GetPosition(this);
            ((UIElement)sender).CaptureMouse();
        }

        private void Viewport3D_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _isDragging3D = false;
            ((UIElement)sender).ReleaseMouseCapture();
        }

        private void Viewport3D_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (_isDragging3D && RotX != null && RotY != null)
            {
                Point currentPos = e.GetPosition(this);
                double deltaX = currentPos.X - _lastMousePos.X;
                double deltaY = currentPos.Y - _lastMousePos.Y;
                
                RotY.Angle += deltaX * 0.5;
                RotX.Angle += deltaY * 0.5;

                _lastMousePos = currentPos;
            }
        }

        private void Input_Changed(object sender, RoutedEventArgs e)
        {
            if (DrawingCanvas == null) return;
            RedrawCanvas();
        }

        private void DrawingCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RedrawCanvas();
        }

        private void RedrawCanvas()
        {
            try
            {
                double L_span_X = double.TryParse(txt_khoang_cach_cot?.Text, out var v4) ? v4 : 5.0;
                double L_cons_L = double.TryParse(txt_L_cons_L?.Text, out var cL) ? cL : 1.5;
                double L_cons_R = double.TryParse(txt_L_cons_R?.Text, out var cR) ? cR : 1.5;
                
                double L_mong = L_span_X + L_cons_L + L_cons_R;
                if (lbl_L_mong != null) lbl_L_mong.Text = L_mong.ToString("F2");

                double L_span_Y = double.TryParse(txt_L_span_Y?.Text, out var sy) ? sy : 5.0;
                double L_cons_T = double.TryParse(txt_L_cons_T?.Text, out var ct) ? ct : 1.5;
                double L_cons_B = double.TryParse(txt_L_cons_B?.Text, out var cb) ? cb : 1.5;
                
                double B_mong = L_span_Y + L_cons_T + L_cons_B;
                if (lbl_B_mong != null) lbl_B_mong.Text = B_mong.ToString("F2");

                double h_ban = double.TryParse(txt_h_ban?.Text, out var v1) ? v1 : 0.4;
                double b_dam = double.TryParse(txt_b_dam?.Text, out var vbd) ? vbd : 0.4;
                double h_dam = double.TryParse(txt_h_dam?.Text, out var v2) ? v2 : 0.4;
                double b_cot = double.TryParse(txt_b_cot?.Text, out var v3) ? v3 : 0.6;
                double d_f = double.TryParse(txt_D_f?.Text, out var v5) ? v5 : 3.5;
                
                double h_doi_trong = chk_mound?.IsChecked == true && double.TryParse(txt_h_doi_trong?.Text, out var v6) ? v6 : 0;
                double h_cat = chk_cushion?.IsChecked == true && double.TryParse(txt_h_cat?.Text, out var v7) ? v7 : 0;
                double y_gwt = chk_gwt?.IsChecked == true && double.TryParse(txt_y_gwt?.Text, out var v8) ? v8 : -10;

                if (DrawingCanvas != null)
                    FoundationDrawer.DrawFoundation(DrawingCanvas, L_span_X, L_cons_L, L_cons_R, B_mong, h_ban, b_dam, h_dam, b_cot, d_f, h_doi_trong, h_cat, y_gwt);
                
                if (SectionYCanvas != null)
                    FoundationDrawer.DrawFoundation(SectionYCanvas, L_span_Y, L_cons_T, L_cons_B, L_mong, h_ban, b_dam, h_dam, b_cot, d_f, h_doi_trong, h_cat, y_gwt);

                if (PlanCanvas != null)
                    PlanViewDrawer.DrawPlan(PlanCanvas, L_span_X, L_cons_L, L_cons_R, L_span_Y, L_cons_T, L_cons_B, b_dam, b_cot);

                if (Model3DGroup != null)
                    Viewport3DDrawer.Draw3D(Model3DGroup, L_span_X, L_cons_L, L_cons_R, L_span_Y, L_cons_T, L_cons_B, h_ban, b_dam, h_dam, b_cot, d_f);
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