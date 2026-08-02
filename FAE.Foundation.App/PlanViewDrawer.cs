using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace FAE.Foundation.App
{
    public static class PlanViewDrawer
    {
        public static void DrawPlan(Canvas canvas, 
            double L_span_X, double L_cons_L, double L_cons_R, 
            double L_span_Y, double L_cons_T, double L_cons_B, 
            double b_dam, double b_cot)
        {
            canvas.Children.Clear();
            if (canvas.ActualWidth == 0 || canvas.ActualHeight == 0) return;

            double L_mong = L_cons_L + L_span_X + L_cons_R;
            double B_mong = L_cons_T + L_span_Y + L_cons_B;
            
            double mathWidth = Math.Max(15.0, L_mong + 4.0);
            double mathHeight = Math.Max(15.0, B_mong + 4.0);

            double scaleX = canvas.ActualWidth / mathWidth;
            double scaleY = canvas.ActualHeight / mathHeight;
            double scale = Math.Min(scaleX, scaleY) * 0.95;

            double offsetX = canvas.ActualWidth / 2.0;
            double offsetY = canvas.ActualHeight / 2.0;

            double WX(double x) => offsetX + x * scale;
            double WY(double y) => offsetY - y * scale;
            double S(double val) => val * scale;

            SolidColorBrush concreteBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E0E0E0"));
            SolidColorBrush ribBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C0C0C0"));
            SolidColorBrush colBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#808080"));
            
            // 1. Foundation Slab Boundary
            Rectangle slab = new Rectangle { Width = S(L_mong), Height = S(B_mong), Fill = concreteBrush, Stroke = Brushes.Black, StrokeThickness = 2 };
            Canvas.SetLeft(slab, WX(-L_mong/2));
            Canvas.SetTop(slab, WY(B_mong/2)); // Top-left corner (Y is inverted)
            canvas.Children.Add(slab);

            // 2. Ribs (Longitudinal)
            double y_tam_top = B_mong/2 - L_cons_T;
            double y_tam_bot = y_tam_top - L_span_Y;

            Rectangle ribX1 = new Rectangle { Width = S(L_mong), Height = S(b_dam), Fill = ribBrush, Stroke = Brushes.Black, StrokeThickness = 1.5 };
            Canvas.SetLeft(ribX1, WX(-L_mong/2));
            Canvas.SetTop(ribX1, WY(y_tam_top + b_dam/2));
            canvas.Children.Add(ribX1);

            Rectangle ribX2 = new Rectangle { Width = S(L_mong), Height = S(b_dam), Fill = ribBrush, Stroke = Brushes.Black, StrokeThickness = 1.5 };
            Canvas.SetLeft(ribX2, WX(-L_mong/2));
            Canvas.SetTop(ribX2, WY(y_tam_bot + b_dam/2));
            canvas.Children.Add(ribX2);

            // 3. Ribs (Transverse)
            double x_tam_trai = -L_mong/2 + L_cons_L;
            double x_tam_phai = x_tam_trai + L_span_X;

            Rectangle ribY1 = new Rectangle { Width = S(b_dam), Height = S(B_mong), Fill = ribBrush, Stroke = Brushes.Black, StrokeThickness = 1.5 };
            Canvas.SetLeft(ribY1, WX(x_tam_trai - b_dam/2));
            Canvas.SetTop(ribY1, WY(B_mong/2));
            canvas.Children.Add(ribY1);

            Rectangle ribY2 = new Rectangle { Width = S(b_dam), Height = S(B_mong), Fill = ribBrush, Stroke = Brushes.Black, StrokeThickness = 1.5 };
            Canvas.SetLeft(ribY2, WX(x_tam_phai - b_dam/2));
            Canvas.SetTop(ribY2, WY(B_mong/2));
            canvas.Children.Add(ribY2);

            // 4. Columns (4 corners)
            Action<double, double> DrawCol = (cx, cy) => {
                Rectangle col = new Rectangle { Width = S(b_cot), Height = S(b_cot), Fill = colBrush, Stroke = Brushes.Black, StrokeThickness = 2 };
                Canvas.SetLeft(col, WX(cx - b_cot/2));
                Canvas.SetTop(col, WY(cy + b_cot/2));
                canvas.Children.Add(col);

                Line l1 = new Line { X1 = WX(cx - b_cot/2), Y1 = WY(cy + b_cot/2), X2 = WX(cx + b_cot/2), Y2 = WY(cy - b_cot/2), Stroke = Brushes.White, StrokeThickness = 1 };
                Line l2 = new Line { X1 = WX(cx - b_cot/2), Y1 = WY(cy - b_cot/2), X2 = WX(cx + b_cot/2), Y2 = WY(cy + b_cot/2), Stroke = Brushes.White, StrokeThickness = 1 };
                canvas.Children.Add(l1);
                canvas.Children.Add(l2);
            };

            DrawCol(x_tam_trai, y_tam_top); // Top-Left
            DrawCol(x_tam_phai, y_tam_top); // Top-Right
            DrawCol(x_tam_trai, y_tam_bot); // Bot-Left
            DrawCol(x_tam_phai, y_tam_bot); // Bot-Right

            // 5. Centerlines
            Line clX = new Line { X1 = WX(-L_mong/2 - 1), Y1 = WY(0), X2 = WX(L_mong/2 + 1), Y2 = WY(0), Stroke = Brushes.Red, StrokeThickness = 1, StrokeDashArray = new DoubleCollection { 10, 2, 2, 2 } };
            Line clY = new Line { X1 = WX(0), Y1 = WY(B_mong/2 + 1), X2 = WX(0), Y2 = WY(-B_mong/2 - 1), Stroke = Brushes.Red, StrokeThickness = 1, StrokeDashArray = new DoubleCollection { 10, 2, 2, 2 } };
            canvas.Children.Add(clX);
            canvas.Children.Add(clY);
        }
    }
}
