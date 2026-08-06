using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using FAE.Foundation.App.Features.RibbedRaft;

namespace FAE.Foundation.App.Features.RibbedRaft.Drawers
{
    public static class PlanDrawer
    {
        public static void DrawPlan(Canvas canvas, RibbedRaftModel foundation)
        {
            canvas.Children.Clear();
            if (canvas.ActualWidth == 0 || canvas.ActualHeight == 0 || foundation == null) return;

            double L_mong = foundation.TotalLength;
            double B_mong = foundation.TotalWidth;
            
            // Allocate space for dimensions and axes (about 3-4m on right and bottom)
            double mathWidth = Math.Max(15.0, L_mong + 8.0);
            double mathHeight = Math.Max(15.0, B_mong + 8.0);

            double scaleX = canvas.ActualWidth / mathWidth;
            double scaleY = canvas.ActualHeight / mathHeight;
            double scale = Math.Min(scaleX, scaleY) * 0.9;

            // Shift drawing up and left slightly to make room for dimensions at bottom/right
            double offsetX = canvas.ActualWidth / 2.0 - scale * 1.5;
            double offsetY = canvas.ActualHeight / 2.0 - scale * 1.5;

            double WX(double x) => offsetX + x * scale;
            double WY(double y) => offsetY - y * scale;
            double S(double val) => Math.Max(0, val * scale);

            SolidColorBrush concreteBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E0E0E0"));
            SolidColorBrush ribBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C0C0C0"));
            SolidColorBrush colBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#808080"));
            
            // 1. Foundation Slab Boundary
            Rectangle slab = new Rectangle { Width = S(L_mong), Height = S(B_mong), Fill = concreteBrush, Stroke = Brushes.Black, StrokeThickness = 2 };
            Canvas.SetLeft(slab, WX(-L_mong/2));
            Canvas.SetTop(slab, WY(B_mong/2)); // Top-left corner (Y is inverted)
            canvas.Children.Add(slab);

            // 2. Ribs (Longitudinal)
            double y_tam_top = B_mong/2 - foundation.ConsTY;
            double y_tam_bot = y_tam_top - foundation.SpanY;

            Rectangle ribX1 = new Rectangle { Width = S(L_mong), Height = S(foundation.RibWidth), Fill = ribBrush, Stroke = Brushes.Black, StrokeThickness = 1.5 };
            Canvas.SetLeft(ribX1, WX(-L_mong/2));
            Canvas.SetTop(ribX1, WY(y_tam_top + foundation.RibWidth/2));
            canvas.Children.Add(ribX1);

            Rectangle ribX2 = new Rectangle { Width = S(L_mong), Height = S(foundation.RibWidth), Fill = ribBrush, Stroke = Brushes.Black, StrokeThickness = 1.5 };
            Canvas.SetLeft(ribX2, WX(-L_mong/2));
            Canvas.SetTop(ribX2, WY(y_tam_bot + foundation.RibWidth/2));
            canvas.Children.Add(ribX2);

            // 3. Ribs (Transverse)
            double x_tam_trai = -L_mong/2 + foundation.ConsLX;
            double x_tam_phai = x_tam_trai + foundation.SpanX;

            Rectangle ribY1 = new Rectangle { Width = S(foundation.RibWidth), Height = S(B_mong), Fill = ribBrush, Stroke = Brushes.Black, StrokeThickness = 1.5 };
            Canvas.SetLeft(ribY1, WX(x_tam_trai - foundation.RibWidth/2));
            Canvas.SetTop(ribY1, WY(B_mong/2));
            canvas.Children.Add(ribY1);

            Rectangle ribY2 = new Rectangle { Width = S(foundation.RibWidth), Height = S(B_mong), Fill = ribBrush, Stroke = Brushes.Black, StrokeThickness = 1.5 };
            Canvas.SetLeft(ribY2, WX(x_tam_phai - foundation.RibWidth/2));
            Canvas.SetTop(ribY2, WY(B_mong/2));
            canvas.Children.Add(ribY2);

            // 4. Columns (4 corners)
            Action<double, double> DrawCol = (cx, cy) => {
                double w = foundation.B1;
                double h = foundation.B2;
                Rectangle col = new Rectangle { Width = S(w), Height = S(h), Fill = colBrush, Stroke = Brushes.Black, StrokeThickness = 2 };
                Canvas.SetLeft(col, WX(cx - w/2));
                Canvas.SetTop(col, WY(cy + h/2));
                canvas.Children.Add(col);

                Line l1 = new Line { X1 = WX(cx - w/2), Y1 = WY(cy + h/2), X2 = WX(cx + w/2), Y2 = WY(cy - h/2), Stroke = Brushes.White, StrokeThickness = 1 };
                Line l2 = new Line { X1 = WX(cx - w/2), Y1 = WY(cy - h/2), X2 = WX(cx + w/2), Y2 = WY(cy + h/2), Stroke = Brushes.White, StrokeThickness = 1 };
                canvas.Children.Add(l1);
                canvas.Children.Add(l2);
            };

            DrawCol(x_tam_trai, y_tam_top);
            DrawCol(x_tam_phai, y_tam_top);
            DrawCol(x_tam_trai, y_tam_bot);
            DrawCol(x_tam_phai, y_tam_bot);

            // 4.5. Hole
            if (foundation.HoleSize > 0)
            {
                Rectangle hole = new Rectangle { Width = S(foundation.HoleSize), Height = S(foundation.HoleSize), Fill = Brushes.White, Stroke = Brushes.Black, StrokeThickness = 1.5 };
                Canvas.SetLeft(hole, WX(-foundation.HoleSize/2));
                Canvas.SetTop(hole, WY(foundation.HoleSize/2)); // Y is inverted
                canvas.Children.Add(hole);

                Line hl1 = new Line { X1 = WX(-foundation.HoleSize/2), Y1 = WY(foundation.HoleSize/2), X2 = WX(foundation.HoleSize/2), Y2 = WY(-foundation.HoleSize/2), Stroke = Brushes.Black, StrokeThickness = 1 };
                Line hl2 = new Line { X1 = WX(-foundation.HoleSize/2), Y1 = WY(-foundation.HoleSize/2), X2 = WX(foundation.HoleSize/2), Y2 = WY(foundation.HoleSize/2), Stroke = Brushes.Black, StrokeThickness = 1 };
                canvas.Children.Add(hl1);
                canvas.Children.Add(hl2);
            }

            // 5. Axes (Grid lines)
            Action<double, double, double, double, string> DrawAxis = (x1, y1, x2, y2, label) => {
                Line axis = new Line { X1 = WX(x1), Y1 = WY(y1), X2 = WX(x2), Y2 = WY(y2), Stroke = Brushes.Black, StrokeThickness = 1, StrokeDashArray = new DoubleCollection { 10, 2, 2, 2 } };
                canvas.Children.Add(axis);

                Ellipse circle = new Ellipse { Width = 20, Height = 20, Stroke = Brushes.Black, StrokeThickness = 1, Fill = Brushes.White };
                
                if (label == "A" || label == "B") // Vertical axes (circles at bottom)
                {
                    Canvas.SetLeft(circle, WX(x2) - 10);
                    Canvas.SetTop(circle, WY(y2) - 10);
                }
                else // Horizontal axes (circles at right)
                {
                    Canvas.SetLeft(circle, WX(x2) - 10);
                    Canvas.SetTop(circle, WY(y2) - 10);
                }
                
                canvas.Children.Add(circle);

                TextBlock txt = new TextBlock { Text = label, FontSize = 12, FontWeight = FontWeights.Bold, Foreground = Brushes.Black };
                if (label == "A" || label == "B")
                {
                    Canvas.SetLeft(txt, WX(x2) - 4);
                    Canvas.SetTop(txt, WY(y2) - 8);
                }
                else
                {
                    Canvas.SetLeft(txt, WX(x2) - 4);
                    Canvas.SetTop(txt, WY(y2) - 8);
                }
                canvas.Children.Add(txt);
            };

            DrawAxis(x_tam_trai, B_mong/2 + 0.5, x_tam_trai, -B_mong/2 - 3.5, "A");
            DrawAxis(x_tam_phai, B_mong/2 + 0.5, x_tam_phai, -B_mong/2 - 3.5, "B");
            DrawAxis(-L_mong/2 - 0.5, y_tam_top, L_mong/2 + 3.5, y_tam_top, "1");
            DrawAxis(-L_mong/2 - 0.5, y_tam_bot, L_mong/2 + 3.5, y_tam_bot, "2");

            // 6. Dimensions
            Action<double, double, double, double, string, bool> DrawDim = (x1, y1, x2, y2, text, isVertical) => {
                Line dline = new Line { X1 = WX(x1), Y1 = WY(y1), X2 = WX(x2), Y2 = WY(y2), Stroke = Brushes.Black, StrokeThickness = 1 };
                canvas.Children.Add(dline);
                
                // Ticks
                double tickSize = 4 / scale;
                if (isVertical)
                {
                    canvas.Children.Add(new Line { X1 = WX(x1 - tickSize), Y1 = WY(y1 + tickSize), X2 = WX(x1 + tickSize), Y2 = WY(y1 - tickSize), Stroke = Brushes.Black, StrokeThickness = 1 });
                    canvas.Children.Add(new Line { X1 = WX(x2 - tickSize), Y1 = WY(y2 + tickSize), X2 = WX(x2 + tickSize), Y2 = WY(y2 - tickSize), Stroke = Brushes.Black, StrokeThickness = 1 });
                }
                else
                {
                    canvas.Children.Add(new Line { X1 = WX(x1 - tickSize), Y1 = WY(y1 - tickSize), X2 = WX(x1 + tickSize), Y2 = WY(y1 + tickSize), Stroke = Brushes.Black, StrokeThickness = 1 });
                    canvas.Children.Add(new Line { X1 = WX(x2 - tickSize), Y1 = WY(y2 - tickSize), X2 = WX(x2 + tickSize), Y2 = WY(y2 + tickSize), Stroke = Brushes.Black, StrokeThickness = 1 });
                }

                TextBlock txt = new TextBlock { Text = text, FontSize = 12, Foreground = Brushes.Black };
                if (isVertical)
                {
                    txt.RenderTransform = new RotateTransform(-90);
                    Canvas.SetLeft(txt, WX(x1) - 18);
                    Canvas.SetTop(txt, WY((y1+y2)/2) + 12);
                }
                else
                {
                    Canvas.SetLeft(txt, WX((x1+x2)/2) - 12);
                    Canvas.SetTop(txt, WY(y1) - 18);
                }
                canvas.Children.Add(txt);
            };

            // Bottom dimensions (Horizontal)
            double dimY = -B_mong/2 - 1.2;
            DrawDim(-L_mong/2, dimY, x_tam_trai, dimY, foundation.ConsLX.ToString("F2"), false);
            DrawDim(x_tam_trai, dimY, x_tam_phai, dimY, foundation.SpanX.ToString("F2"), false);
            DrawDim(x_tam_phai, dimY, L_mong/2, dimY, foundation.ConsRX.ToString("F2"), false);

            double dimY2 = dimY - 1.2;
            DrawDim(-L_mong/2, dimY2, L_mong/2, dimY2, foundation.TotalLength.ToString("F2"), false);

            // Right dimensions (Vertical)
            double dimX = L_mong/2 + 1.2;
            DrawDim(dimX, B_mong/2, dimX, y_tam_top, foundation.ConsTY.ToString("F2"), true);
            DrawDim(dimX, y_tam_top, dimX, y_tam_bot, foundation.SpanY.ToString("F2"), true);
            DrawDim(dimX, y_tam_bot, dimX, -B_mong/2, foundation.ConsBY.ToString("F2"), true);

            double dimX2 = dimX + 1.2;
            DrawDim(dimX2, B_mong/2, dimX2, -B_mong/2, foundation.TotalWidth.ToString("F2"), true);
            
            // Extension lines for dimensions
            Action<double, double, double, double> DrawExt = (x1, y1, x2, y2) => {
                canvas.Children.Add(new Line { X1 = WX(x1), Y1 = WY(y1), X2 = WX(x2), Y2 = WY(y2), Stroke = Brushes.Gray, StrokeThickness = 0.5 });
            };
            
            // Bottom extensions
            DrawExt(-L_mong/2, -B_mong/2, -L_mong/2, dimY2 - 0.2);
            DrawExt(x_tam_trai, -B_mong/2, x_tam_trai, dimY - 0.2);
            DrawExt(x_tam_phai, -B_mong/2, x_tam_phai, dimY - 0.2);
            DrawExt(L_mong/2, -B_mong/2, L_mong/2, dimY2 - 0.2);
            
            // Right extensions
            DrawExt(L_mong/2, B_mong/2, dimX2 + 0.2, B_mong/2);
            DrawExt(L_mong/2, y_tam_top, dimX + 0.2, y_tam_top);
            DrawExt(L_mong/2, y_tam_bot, dimX + 0.2, y_tam_bot);
            DrawExt(L_mong/2, -B_mong/2, dimX2 + 0.2, -B_mong/2);

            // 7. Hole Dimensions
            if (foundation.HoleSize > 0)
            {
                double hs = foundation.HoleSize;
                double hdimY = -hs/2 - 0.8;
                DrawDim(-hs/2, hdimY, hs/2, hdimY, hs.ToString("F2"), false);
                DrawExt(-hs/2, -hs/2, -hs/2, hdimY - 0.2);
                DrawExt(hs/2, -hs/2, hs/2, hdimY - 0.2);

                double hdimX = hs/2 + 0.8;
                DrawDim(hdimX, hs/2, hdimX, -hs/2, hs.ToString("F2"), true);
                DrawExt(hs/2, hs/2, hdimX + 0.2, hs/2);
                DrawExt(hs/2, -hs/2, hdimX + 0.2, -hs/2);
            }
        }
    }
}
