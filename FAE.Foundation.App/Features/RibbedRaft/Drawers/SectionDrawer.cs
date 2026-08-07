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
        public static void DrawFoundation(Canvas canvas, RibbedRaftModel foundation, FAE.Foundation.App.Models.BoreholeModel borehole, bool isSectionY)
        {
            canvas.Children.Clear();
            if (canvas.ActualWidth == 0 || foundation == null) return;

            double L_span = isSectionY ? foundation.SpanY : foundation.SpanX;
            double L_cons_L = isSectionY ? foundation.ConsTY : foundation.ConsLX;
            double L_cons_R = isSectionY ? foundation.ConsBY : foundation.ConsRX;
            
            double h_ban = foundation.SlabThickness;
            double h_dam = foundation.RibHeight;
            double b_col = foundation.ColumnWidth;
            double D_f = foundation.EmbedmentDepth; // Chiều sâu chôn móng (2.4m)
            double H_total = foundation.Depth; // Chiều cao móng (3.9m)
            double h_doi_trong = foundation.HasMound ? foundation.MoundHeight : 0.0;
            double h_cat = foundation.HasSandCushion ? foundation.SandThickness : 0.0;

            double L_mong = L_cons_L + L_span + L_cons_R;
            double h_lot = 0.1;
            double loe_lot = 0.1;
            
            double loe_cat = 0.0;
            if (foundation.HasSandCushion && h_cat > 0) {
                if (h_cat < 1.0) loe_cat = 0.6;
                else loe_cat = 0.3;
            } else {
                loe_cat = 0.25;
            }
            
            double taluy_ho_dao = 1.0; 

            double x_left_edge = -L_mong / 2.0;
            double x_tam_trai = x_left_edge + L_cons_L;
            double x_tam_phai = x_tam_trai + L_span;

            double H_col = H_total - h_dam; // Chiều cao cổ móng = 3.9 - 1.8 = 2.1m
            double y_mat_dat = 0.0;
            double y_day_mong = -D_f; // -2.40m
            double y_day_lot = y_day_mong - h_lot;
            double y_day_cat = foundation.HasSandCushion ? y_day_lot - h_cat : y_day_lot;
            double y_mat_ban = y_day_mong + h_ban; // -1.80m
            double y_mat_dam = y_day_mong + h_dam; // -0.60m
            double y_dinh_cot = y_mat_dam + H_col; // +1.50m
            double y_dinh_mound = y_mat_dat + h_doi_trong;

            double L_day_ho = L_mong + 2 * loe_cat;
            double x_day_ho = L_day_ho / 2;
            double x_mieng_ho = x_day_ho + taluy_ho_dao * Math.Abs(y_day_cat);

            // Tăng khoảng cách hai bên để không bị lẹm text và vòng tròn
            double mathWidth = Math.Max(L_mong + 12.0, x_mieng_ho * 2 + 14.0);
            double max_L = mathWidth;
            
            double totalSoilDepth = 15.0;
            if (borehole != null && borehole.Layers.Count > 0)
            {
                double sum = 0;
                foreach(var l in borehole.Layers) sum += l.Thickness;
                totalSoilDepth = sum;
            }
            if (totalSoilDepth < D_f + 5.0) totalSoilDepth = D_f + 5.0;
            
            // Tính toán khoảng đệm phía trên để không bị mất đường Dim
            double topPadding = Math.Max(y_dinh_mound, y_dinh_cot) + 2.5;
            double mathHeight = totalSoilDepth + topPadding + 2.0;

            // Giảm tỷ lệ scale một chút để an toàn không lẹm biên (0.88 thay vì 0.9)
            double scale = (canvas.ActualWidth / mathWidth) * 0.88;
            double desiredHeight = mathHeight * scale;
            if (Math.Abs(canvas.Height - desiredHeight) > 1.0)
            {
                canvas.Height = desiredHeight;
            }

            // Dịch tâm canvas sang trái 20px để chừa chỗ cho text cao độ bên phải
            double offsetX = (canvas.ActualWidth / 2.0) - 20;
            double offsetY = topPadding * scale;

            double WX(double x) => offsetX + x * scale;
            double WY(double y) => offsetY - y * scale;
            double S(double val) => Math.Max(0, val * scale);

            SolidColorBrush soil1Brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#e8dcc8"));
            SolidColorBrush pitBrush = new SolidColorBrush(Color.FromArgb(30, 181, 148, 112));
            SolidColorBrush moundBrush = new SolidColorBrush(Color.FromArgb(80, 164, 124, 84));
            SolidColorBrush sandBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f4e5b5"));
            SolidColorBrush concreteBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#d3d3d3"));
            SolidColorBrush waterBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00bfff"));

            // 1. DRAW SOIL LAYERS
            if (borehole != null && borehole.Layers.Count > 0)
            {
                double currentY = 0.0;
                string[] soilColors = new[] { "#e8dcc8", "#d9c5a3", "#ebd8b7", "#d1bb9e", "#e8dcc8", "#d9c5a3" };
                for (int i = 0; i < borehole.Layers.Count; i++)
                {
                    var layer = borehole.Layers[i];
                    double thickness = layer.Thickness;
                    
                    SolidColorBrush soilBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(soilColors[i % soilColors.Length]));
                    Rectangle soilRect = new Rectangle { Width = S(max_L), Height = S(thickness), Fill = soilBrush };
                    Canvas.SetLeft(soilRect, WX(-max_L/2));
                    Canvas.SetTop(soilRect, WY(-currentY));
                    canvas.Children.Add(soilRect);
                    
                    Line sepLine = new Line { X1 = WX(-max_L/2), Y1 = WY(-currentY), X2 = WX(max_L/2), Y2 = WY(-currentY), Stroke = Brushes.Gray, StrokeThickness = 1 };
                    canvas.Children.Add(sepLine);
                    
                    double textY = WY(-currentY - Math.Min(thickness, 2.0) / 2);
                    double textX = WX(-max_L/2 + 1.2);
                    
                    Ellipse circle = new Ellipse { Width = 18, Height = 18, Stroke = Brushes.SaddleBrown, StrokeThickness = 1, Fill = Brushes.White };
                    Canvas.SetLeft(circle, textX - 9);
                    Canvas.SetTop(circle, textY - 9);
                    canvas.Children.Add(circle);
                    
                    TextBlock soilNum = new TextBlock { Text = layer.LayerId, Foreground = Brushes.SaddleBrown, FontWeight = FontWeights.Bold, FontSize = 10, TextAlignment = TextAlignment.Center, Width = 18 };
                    Canvas.SetLeft(soilNum, textX - 9);
                    Canvas.SetTop(soilNum, textY - 7);
                    canvas.Children.Add(soilNum);

                    TextBlock thickTxt = new TextBlock { Text = $"h={layer.Thickness:F2}m", Foreground = Brushes.SaddleBrown, FontSize = 11 };
                    Canvas.SetLeft(thickTxt, textX + 14);
                    Canvas.SetTop(thickTxt, textY - 7);
                    canvas.Children.Add(thickTxt);
                    
                    currentY += thickness;
                }
            }
            else
            {
                Rectangle soil1 = new Rectangle { Width = S(max_L), Height = S(mathHeight * 1.5), Fill = soil1Brush };
                Canvas.SetLeft(soil1, WX(-max_L/2));
                Canvas.SetTop(soil1, WY(0));
                canvas.Children.Add(soil1);
            }

            // 2. EXCAVATION PIT
            Polygon pit = new Polygon { Fill = pitBrush, Stroke = Brushes.SaddleBrown, StrokeThickness = 2, StrokeDashArray = new DoubleCollection { 4, 4 } };
            pit.Points = new PointCollection {
                new Point(WX(-x_mieng_ho), WY(y_mat_dat)),
                new Point(WX(-x_day_ho), WY(y_day_cat)),
                new Point(WX(x_day_ho), WY(y_day_cat)),
                new Point(WX(x_mieng_ho), WY(y_mat_dat))
            };
            canvas.Children.Add(pit);

            // 3. SOIL MOUND
            if (h_doi_trong > 0 && foundation.HasMound)
            {
                Polygon mound = new Polygon { Fill = moundBrush, Stroke = Brushes.SaddleBrown, StrokeThickness = 1, StrokeDashArray = new DoubleCollection { 2, 2 } };
                mound.Points = new PointCollection {
                    new Point(WX(-x_mieng_ho - 1.5), WY(y_mat_dat)),
                    new Point(WX(-x_mieng_ho + 0.5), WY(y_dinh_mound)),
                    new Point(WX(x_mieng_ho - 0.5), WY(y_dinh_mound)),
                    new Point(WX(x_mieng_ho + 1.5), WY(y_mat_dat))
                };
                canvas.Children.Add(mound);
            }

            // 4. SAND CUSHION & LEAN CONCRETE
            if (foundation.HasSandCushion && h_cat > 0)
            {
                double L_cat = L_day_ho;
                Rectangle cat = new Rectangle { Width = S(L_cat), Height = S(h_cat), Fill = sandBrush, Stroke = Brushes.SaddleBrown };
                Canvas.SetLeft(cat, WX(-L_cat / 2));
                Canvas.SetTop(cat, WY(y_day_cat + h_cat));
                canvas.Children.Add(cat);
            }

            double L_lot = L_mong + 2 * loe_lot;
            Rectangle lot = new Rectangle { Width = S(L_lot), Height = S(h_lot), Fill = Brushes.Silver, Stroke = Brushes.Black };
            Canvas.SetLeft(lot, WX(-L_lot / 2));
            Canvas.SetTop(lot, WY(y_day_mong));
            canvas.Children.Add(lot);

            // 5. CONCRETE FOUNDATION (Bê tông móng khối liền)
            double lw = 1.5;
            
            // Bản móng h1 (0.6m)
            Rectangle ban = new Rectangle { Width = S(L_mong), Height = S(h_ban), Fill = concreteBrush, Stroke = Brushes.Black, StrokeThickness = lw };
            Canvas.SetLeft(ban, WX(-L_mong / 2));
            Canvas.SetTop(ban, WY(y_mat_ban));
            canvas.Children.Add(ban);

            // Dầm móng sườn (Rib) chạy dọc theo mặt cắt (từ mép này sang mép kia)
            Rectangle dam = new Rectangle { Width = S(L_mong), Height = S(h_dam - h_ban), Fill = concreteBrush, Stroke = Brushes.Black, StrokeThickness = lw };
            Canvas.SetLeft(dam, WX(-L_mong / 2));
            Canvas.SetTop(dam, WY(y_mat_dam));
            canvas.Children.Add(dam);

            // 2 Cổ móng (Pedestals) vươn từ dầm móng (-0.60m) lên đến đỉnh cổ móng (+1.50m)
            Action<double> DrawCol = (xCenter) => {
                Rectangle col = new Rectangle { Width = S(b_col), Height = S(H_col), Fill = concreteBrush, Stroke = Brushes.Black, StrokeThickness = lw };
                Canvas.SetLeft(col, WX(xCenter - b_col / 2));
                Canvas.SetTop(col, WY(y_dinh_cot));
                canvas.Children.Add(col);

                // Đường phân cách trang trí ở chân cột giao với dầm
                Line botLine = new Line { X1 = WX(xCenter - b_col / 2), Y1 = WY(y_mat_dam), X2 = WX(xCenter + b_col / 2), Y2 = WY(y_mat_dam), Stroke = Brushes.Gray, StrokeThickness = 0.5, StrokeDashArray = new DoubleCollection { 2, 2 } };
                canvas.Children.Add(botLine);
            };

            DrawCol(x_tam_trai);
            DrawCol(x_tam_phai);

            // 6. GWT & NGL & ELEVATIONS
            Line nglLine = new Line { X1 = WX(-max_L/2), Y1 = WY(y_mat_dat), X2 = WX(max_L/2), Y2 = WY(y_mat_dat), Stroke = Brushes.DarkGreen, StrokeThickness = 1.5 };
            canvas.Children.Add(nglLine);
            
            Action<double, double, string, string, bool> DrawElev = (x, y, text, valText, textBelow) => {
                Polygon marker = new Polygon { Fill = Brushes.Black, Points = new PointCollection { new Point(WX(x), WY(y)), new Point(WX(x)-5, WY(y)-8), new Point(WX(x)+5, WY(y)-8) } };
                canvas.Children.Add(marker);
                Line l = new Line { X1 = WX(x)-15, Y1 = WY(y)-8, X2 = WX(x)+15, Y2 = WY(y)-8, Stroke = Brushes.Black, StrokeThickness = 1 };
                canvas.Children.Add(l);
                TextBlock tb = new TextBlock { Text = $"{text} {valText}", FontSize = 11, FontWeight = FontWeights.Bold, Foreground = Brushes.Black };
                Canvas.SetLeft(tb, WX(x) + 10);
                Canvas.SetTop(tb, WY(y) - 8 + (textBelow ? 4 : -14));
                canvas.Children.Add(tb);
            };

            DrawElev(x_mieng_ho + 1.0, y_mat_dat, "MĐTN:", "0.00", false);
            DrawElev(x_mieng_ho + 1.0, y_day_mong, "Đáy móng:", $"-{D_f:F2}", false);


            // 7. DIMENSIONS
            Action<double, double, double, string, bool> DrawDim = (x1, x2, y, text, isBottom) => {
                double textOffset = isBottom ? 5 : -20;
                Line dimL = new Line { X1 = WX(x1), Y1 = WY(y), X2 = WX(x2), Y2 = WY(y), Stroke = Brushes.Black, StrokeThickness = 1 };
                
                canvas.Children.Add(new Line { X1 = WX(x1)-4, Y1 = WY(y)+4, X2 = WX(x1)+4, Y2 = WY(y)-4, Stroke = Brushes.Black, StrokeThickness = 1.5 });
                canvas.Children.Add(new Line { X1 = WX(x2)-4, Y1 = WY(y)+4, X2 = WX(x2)+4, Y2 = WY(y)-4, Stroke = Brushes.Black, StrokeThickness = 1.5 });
                
                Line ext1 = new Line { X1 = WX(x1), Y1 = WY(y) + (isBottom ? -10 : 10), X2 = WX(x1), Y2 = WY(y) + (isBottom ? 10 : -10), Stroke = Brushes.Gray, StrokeThickness = 0.5 };
                Line ext2 = new Line { X1 = WX(x2), Y1 = WY(y) + (isBottom ? -10 : 10), X2 = WX(x2), Y2 = WY(y) + (isBottom ? 10 : -10), Stroke = Brushes.Gray, StrokeThickness = 0.5 };
                
                canvas.Children.Add(dimL);
                canvas.Children.Add(ext1); canvas.Children.Add(ext2);
                
                TextBlock tb = new TextBlock { Text = text, Foreground = Brushes.Black, FontSize = 12 };
                Canvas.SetLeft(tb, WX((x1+x2)/2) - 15);
                Canvas.SetTop(tb, WY(y) + textOffset);
                canvas.Children.Add(tb);
            };

            double dimY = y_day_cat - 1.0;
            DrawDim(-L_mong/2, L_mong/2, dimY, $"{L_mong:F2}", true);
            DrawDim(x_tam_trai, x_tam_phai, y_dinh_cot + 1.0, $"{L_span:F2}", false);

        }
}
}

