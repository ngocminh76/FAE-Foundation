using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using FAE.Foundation.App.Features.RibbedRaft;

namespace FAE.Foundation.App.Features.RibbedRaft.Drawers
{
    public static class SectionDrawer
    {
        public static void DrawFoundation(Canvas canvas, RibbedRaftModel foundation, bool isSectionY)
        {
            canvas.Children.Clear();
            if (canvas.ActualWidth == 0 || canvas.ActualHeight == 0 || foundation == null) return;

            // Extract variables based on section orientation
            double L_span = isSectionY ? foundation.SpanY : foundation.SpanX;
            double L_cons_L = isSectionY ? foundation.ConsTY : foundation.ConsLX;
            double L_cons_R = isSectionY ? foundation.ConsBY : foundation.ConsRX;
            
            // These properties were passed from MainWindow.xaml.cs in the old version:
            // For Section X: B_mong = B_mong, L_mong = L_mong
            // For Section Y: B_mong = L_mong, L_mong = B_mong
            // Wait, in old version Section Y was:
            // FoundationDrawer.DrawFoundation(SectionYCanvas, L_span_Y, L_cons_T, L_cons_B, L_mong, h_ban, b_dam, h_dam, b_cot, d_f, h_doi_trong, h_cat, y_gwt);
            // So B_mong parameter was actually the other direction's length.

            double B_mong_other_dir = isSectionY ? foundation.TotalLength : foundation.TotalWidth;
            double h_ban = foundation.SlabThickness;
            double b_dam = foundation.RibWidth; // The parameter b_dam was not even used in drawing Section X/Y in old code! Wait, it was used for plan view. The old section drawer doesn't use b_dam.
            double h_dam = foundation.RibHeight;
            double b_col = foundation.ColumnWidth;
            double D_f = foundation.Depth;
            double h_doi_trong = foundation.HasMound ? foundation.MoundHeight : 0.0;
            double h_cat = foundation.HasSandCushion ? foundation.SandThickness : 0.0;
            double y_gwt = foundation.HasGroundwater ? foundation.GroundwaterElev : -10.0;

            // --- 1. SET UP COORDINATE SYSTEM ---
            double L_mong = L_cons_L + L_span + L_cons_R;
            double h_lot = 0.1;
            double loe_lot = 0.1;
            double loe_cat = 0.5;
            double taluy_ho_dao = 1.5;

            // X Coordinates relative to center of foundation
            // Left edge is -L_mong/2
            double x_left_edge = -L_mong / 2.0;
            double x_tam_trai = x_left_edge + L_cons_L;
            double x_tam_phai = x_tam_trai + L_span;

            // Calculate heights to find bounding box
            double H_col = D_f - (h_ban + h_dam) + h_doi_trong + 0.5;
            double y_mat_dat = 0.0;
            double y_day_mong = -D_f;
            double y_day_lot = y_day_mong - h_lot;
            double y_day_cat = y_day_lot - h_cat;
            double y_mat_ban = y_day_mong + h_ban;
            double y_mat_dam = y_mat_ban + h_dam;
            double y_dinh_cot = y_mat_dam + H_col;
            double y_dinh_mound = y_mat_dat + h_doi_trong;

            double L_day_ho = L_mong + 2 * loe_lot + 2 * loe_cat + 1.0;
            double x_day_ho = L_day_ho / 2;
            double x_mieng_ho = x_day_ho + taluy_ho_dao * Math.Abs(y_day_cat);

            double max_L = Math.Max(28.0, L_mong + 10.0); // Dynamic width to fit large foundations
            double mathWidth = max_L;
            double mathHeight = 9.5 + (D_f > 5 ? D_f - 5 : 0); // Dynamic height to fit deep foundations

            double scaleX = canvas.ActualWidth / mathWidth;
            double scaleY = canvas.ActualHeight / mathHeight;
            double scale = Math.Min(scaleX, scaleY) * 0.95; // Uniform scale with padding

            double offsetX = canvas.ActualWidth / 2.0; // Center X
            double offsetY = (canvas.ActualHeight / 2.0) + (1.25 * scale) + (D_f > 5 ? (D_f-5)*scale/2 : 0); // Shift center Y down a bit

            // Coordinate transform functions (Y axis is inverted in WPF)
            double WX(double x) => offsetX + x * scale;
            double WY(double y) => offsetY - y * scale;
            double S(double val) => val * scale; // Scale dimension

            // Colors
            SolidColorBrush soil1Brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#dcb897"));
            SolidColorBrush soil2Brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#e6c280"));
            SolidColorBrush pitBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#b59470"));
            SolidColorBrush moundBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#a47c54"));
            SolidColorBrush sandBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f4e5b5"));
            SolidColorBrush concreteBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#d3d3d3"));
            SolidColorBrush waterBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00bfff"));
            SolidColorBrush greenBrush = new SolidColorBrush(Colors.DarkGreen);
            SolidColorBrush redBrush = new SolidColorBrush(Colors.Red);

            // --- 3. DRAW SOIL LAYERS ---
            Rectangle soil1 = new Rectangle { Width = S(max_L), Height = S(2.5), Fill = soil1Brush };
            Canvas.SetLeft(soil1, WX(-max_L/2));
            Canvas.SetTop(soil1, WY(-0.0));
            canvas.Children.Add(soil1);

            Rectangle soil2 = new Rectangle { Width = S(max_L), Height = S(mathHeight - 2.5), Fill = soil2Brush };
            Canvas.SetLeft(soil2, WX(-max_L/2));
            Canvas.SetTop(soil2, WY(-2.5));
            canvas.Children.Add(soil2);

            // --- 4. EXCAVATION PIT ---
            Polygon pit = new Polygon { Fill = pitBrush, Stroke = Brushes.Transparent };
            pit.Points = new PointCollection {
                new Point(WX(-x_mieng_ho), WY(y_mat_dat)),
                new Point(WX(-x_day_ho), WY(y_day_cat)),
                new Point(WX(x_day_ho), WY(y_day_cat)),
                new Point(WX(x_mieng_ho), WY(y_mat_dat))
            };
            canvas.Children.Add(pit);

            // --- 5. SOIL MOUND ---
            Polygon mound = new Polygon { Fill = moundBrush, Stroke = Brushes.Transparent };
            mound.Points = new PointCollection {
                new Point(WX(-x_mieng_ho - 1.5), WY(y_mat_dat)),
                new Point(WX(-x_mieng_ho + 0.5), WY(y_dinh_mound)),
                new Point(WX(x_mieng_ho - 0.5), WY(y_dinh_mound)),
                new Point(WX(x_mieng_ho + 1.5), WY(y_mat_dat))
            };
            canvas.Children.Add(mound);

            // --- 6. CUSHION & LEAN CONCRETE ---
            double L_cat = L_mong + 2 * loe_lot + 2 * loe_cat;
            if (h_cat > 0)
            {
                Rectangle cat = new Rectangle { Width = S(L_cat), Height = S(h_cat), Fill = sandBrush, Stroke = Brushes.Black };
                Canvas.SetLeft(cat, WX(-L_cat / 2));
                Canvas.SetTop(cat, WY(y_day_cat + h_cat)); // Y is top
                canvas.Children.Add(cat);

                Line geoTextile = new Line {
                    X1 = WX(-L_cat / 2), Y1 = WY(y_day_cat),
                    X2 = WX(L_cat / 2), Y2 = WY(y_day_cat),
                    Stroke = Brushes.DarkOrange, StrokeThickness = 3, StrokeDashArray = new DoubleCollection { 2, 2 }
                };
                canvas.Children.Add(geoTextile);
            }

            double L_lot = L_mong + 2 * loe_lot;
            Rectangle lot = new Rectangle { Width = S(L_lot), Height = S(h_lot), Fill = Brushes.Silver, Stroke = Brushes.Black };
            Canvas.SetLeft(lot, WX(-L_lot / 2));
            Canvas.SetTop(lot, WY(y_day_mong));
            canvas.Children.Add(lot);

            // --- 7. CONCRETE FOUNDATION ---
            double lw = 1.5;
            
            // Slab
            Rectangle ban = new Rectangle { Width = S(L_mong), Height = S(h_ban), Fill = concreteBrush, Stroke = Brushes.Black, StrokeThickness = lw };
            Canvas.SetLeft(ban, WX(-L_mong / 2));
            Canvas.SetTop(ban, WY(y_mat_ban));
            canvas.Children.Add(ban);

            // Rib
            Rectangle dam = new Rectangle { Width = S(L_mong), Height = S(h_dam), Fill = concreteBrush, Stroke = Brushes.Black, StrokeThickness = lw };
            Canvas.SetLeft(dam, WX(-L_mong / 2));
            Canvas.SetTop(dam, WY(y_mat_dam));
            canvas.Children.Add(dam);

            // Columns
            Action<double> DrawCol = (xCenter) => {
                Rectangle col = new Rectangle { Width = S(b_col), Height = S(H_col), Fill = concreteBrush, Stroke = Brushes.Black, StrokeThickness = lw };
                Canvas.SetLeft(col, WX(xCenter - b_col / 2));
                Canvas.SetTop(col, WY(y_dinh_cot));
                canvas.Children.Add(col);

                // Column dashed cuts through beam
                Line leftCut = new Line { X1 = WX(xCenter - b_col / 2), Y1 = WY(y_mat_ban), X2 = WX(xCenter - b_col / 2), Y2 = WY(y_mat_dam), Stroke = Brushes.Black, StrokeThickness = lw };
                Line rightCut = new Line { X1 = WX(xCenter + b_col / 2), Y1 = WY(y_mat_ban), X2 = WX(xCenter + b_col / 2), Y2 = WY(y_mat_dam), Stroke = Brushes.Black, StrokeThickness = lw };
                canvas.Children.Add(leftCut);
                canvas.Children.Add(rightCut);

                // Anchor bolts
                Line bolt1 = new Line { X1 = WX(xCenter - 0.1), Y1 = WY(y_dinh_cot), X2 = WX(xCenter - 0.1), Y2 = WY(y_dinh_cot + 0.3), Stroke = Brushes.Black, StrokeThickness = 3 };
                Line bolt2 = new Line { X1 = WX(xCenter + 0.1), Y1 = WY(y_dinh_cot), X2 = WX(xCenter + 0.1), Y2 = WY(y_dinh_cot + 0.3), Stroke = Brushes.Black, StrokeThickness = 3 };
                canvas.Children.Add(bolt1);
                canvas.Children.Add(bolt2);
            };

            DrawCol(x_tam_trai);
            DrawCol(x_tam_phai);

            // Mid section cut
            Line midL = new Line { X1 = WX(-1.0), Y1 = WY(y_day_mong), X2 = WX(-1.0), Y2 = WY(y_mat_ban), Stroke = Brushes.Black, StrokeThickness = lw };
            Line midR = new Line { X1 = WX(1.0), Y1 = WY(y_day_mong), X2 = WX(1.0), Y2 = WY(y_mat_ban), Stroke = Brushes.Black, StrokeThickness = lw };
            canvas.Children.Add(midL);
            canvas.Children.Add(midR);

            // --- 8. GWT & NGL ---
            Line gwtLine = new Line { X1 = WX(-max_L/2), Y1 = WY(y_gwt), X2 = WX(max_L/2), Y2 = WY(y_gwt), Stroke = waterBrush, StrokeThickness = 2, StrokeDashArray = new DoubleCollection { 10, 2, 2, 2 } };
            canvas.Children.Add(gwtLine);
            
            TextBlock gwtText = new TextBlock { Text = $"Mực Nước Ngầm ({y_gwt}m)", Foreground = waterBrush, FontWeight = FontWeights.Bold };
            Canvas.SetLeft(gwtText, WX(-max_L/2 + 1.0));
            Canvas.SetTop(gwtText, WY(y_gwt + 0.2));
            canvas.Children.Add(gwtText);

            Line nglLine = new Line { X1 = WX(-max_L/2), Y1 = WY(y_mat_dat), X2 = WX(max_L/2), Y2 = WY(y_mat_dat), Stroke = greenBrush, StrokeThickness = 2 };
            canvas.Children.Add(nglLine);
            
            TextBlock nglText = new TextBlock { Text = "Mặt đất tự nhiên (NGL)", Foreground = greenBrush, FontWeight = FontWeights.Bold };
            Canvas.SetLeft(nglText, WX(-max_L/2 + 1.0));
            Canvas.SetTop(nglText, WY(y_mat_dat + 0.2));
            canvas.Children.Add(nglText);

            // --- 9. DIMENSION D_F ---
            double dimX = -x_mieng_ho - 1.0;
            Line dimLine = new Line { X1 = WX(dimX), Y1 = WY(y_day_mong), X2 = WX(dimX), Y2 = WY(y_mat_dat), Stroke = redBrush, StrokeThickness = 1.5 };
            canvas.Children.Add(dimLine);

            TextBlock dimText = new TextBlock { Text = $"D_f = {D_f}m", Foreground = redBrush, FontWeight = FontWeights.Bold };
            Canvas.SetLeft(dimText, WX(dimX - 0.2) - 60);
            Canvas.SetTop(dimText, WY(y_day_mong / 2) - 10);
            canvas.Children.Add(dimText);
        }
    }
}
