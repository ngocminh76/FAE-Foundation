using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using FAE.Foundation.App.Features.RibbedRaft.Calculations;
using FAE.Foundation.App.Models;

namespace FAE.Foundation.App.Features.RibbedRaft.Drawers
{
    /// <summary>
    /// Vẽ Sơ đồ tính lún chuẩn theo TCVN 9362:2012 (Hình C.1 - Trang 72-73)
    /// - Trục tim móng ở giữa (Central Axis).
    /// - Bên TRÁI: Đường THẲNG TUYẾN TÍNH thể hiện Ứng suất bản thân pdz.
    /// - Bên PHẢI: Đường CONG PHI TUYẾN thể hiện Ứng suất gây lún poz = alpha * p0.
    /// - Đáy vùng lún Hc: Cao trình mà poz' <= 0.1 * pdz' (hoặc 0.2 * pdz').
    /// </summary>
    public static class SettlementDrawer
    {
        public static void DrawSettlementDiagram(Canvas canvas, GeotechCalculationResult result, BoreholeModel borehole)
        {
            canvas.Children.Clear();
            if (result == null || result.SettlementLayers == null || result.SettlementLayers.Count == 0) return;

            double width = canvas.ActualWidth;
            double height = canvas.ActualHeight;
            if (width <= 0 || height <= 0) return;

            var layers = result.SettlementLayers;
            double embedmentDepth = 2.4; // h1 (m)
            double Hc = result.InfluenceDepth > 0 ? result.InfluenceDepth : layers[layers.Count - 1].Z;

            // Target Z range: from Ground (-h1) down to Hc * 1.15
            double minZ = -embedmentDepth;
            double maxZ = Hc * 1.15;
            if (maxZ <= 0) maxZ = 10.0;
            double totalZRange = maxZ - minZ;

            // Margins
            double marginTop = 45;
            double marginBottom = 30;
            double marginLeft = 40;
            double marginRight = 40;
            double chartWidth = width - marginLeft - marginRight;
            double chartHeight = height - marginTop - marginBottom;

            // Central Vertical Axis (Trục tim móng)
            double xCenter = marginLeft + chartWidth * 0.45; // slightly left of visual center to give room for labels

            // Scale for stress (pixels per T/m2)
            double maxPdz = layers[layers.Count - 1].SumGammaHi;
            double maxPoz = result.Sigma0;
            double maxStressVal = Math.Max(maxPdz, maxPoz);
            if (maxStressVal <= 0) maxStressVal = 10.0;

            // Available width for left (pdz) and right (poz)
            double availableWidthRight = (width - marginRight) - xCenter - 20;
            double availableWidthLeft = xCenter - (marginLeft + 20);

            double scaleLeft = availableWidthLeft / maxPdz; // px per T/m2 to the left
            double scaleRight = availableWidthRight / maxPoz; // px per T/m2 to the right

            // Depth Y scale
            double ScaleY(double z) => marginTop + ((z - minZ) / totalZRange) * chartHeight;

            double yGround = ScaleY(-embedmentDepth);
            double yBase = ScaleY(0);
            double yHc = ScaleY(Hc);

            // -------------------------------------------------------------
            // 1. BACKDROP & ACTIVE SETTLEMENT ZONE
            // -------------------------------------------------------------
            // Active zone shading between Base (z=0) and Hc (z=Hc)
            var activeZone = new Rectangle
            {
                Width = availableWidthLeft + availableWidthRight,
                Height = yHc - yBase,
                Fill = new SolidColorBrush(Color.FromArgb(18, 59, 130, 246)) // Light blue tint
            };
            Canvas.SetLeft(activeZone, xCenter - availableWidthLeft);
            Canvas.SetTop(activeZone, yBase);
            canvas.Children.Add(activeZone);

            // -------------------------------------------------------------
            // 2. CENTRAL VERTICAL AXIS (TRỤC TIM MÓNG)
            // -------------------------------------------------------------
            var centerAxis = new Line
            {
                X1 = xCenter, Y1 = marginTop - 15, X2 = xCenter, Y2 = marginTop + chartHeight,
                Stroke = new SolidColorBrush(Color.FromRgb(71, 85, 105)),
                StrokeThickness = 1.5,
                StrokeDashArray = new DoubleCollection { 6, 3, 2, 3 } // Center axis dash dot
            };
            canvas.Children.Add(centerAxis);

            var txtCenter = new TextBlock
            {
                Text = "Trục tim móng",
                FontSize = 9,
                FontStyle = FontStyles.Italic,
                Foreground = new SolidColorBrush(Color.FromRgb(100, 116, 139))
            };
            Canvas.SetLeft(txtCenter, xCenter - 30);
            Canvas.SetTop(txtCenter, marginTop - 28);
            canvas.Children.Add(txtCenter);

            // -------------------------------------------------------------
            // 3. ELEVATION LINES (Ground, Base, Hc)
            // -------------------------------------------------------------
            // Ground Line (-h1)
            var lineGround = new Line
            {
                X1 = marginLeft, Y1 = yGround, X2 = width - marginRight, Y2 = yGround,
                Stroke = new SolidColorBrush(Color.FromRgb(120, 53, 15)),
                StrokeThickness = 1.5
            };
            canvas.Children.Add(lineGround);

            var txtGround = new TextBlock
            {
                Text = "Cao trình thiên nhiên",
                FontWeight = FontWeights.Bold,
                FontSize = 10,
                Foreground = new SolidColorBrush(Color.FromRgb(120, 53, 15))
            };
            Canvas.SetLeft(txtGround, marginLeft);
            Canvas.SetTop(txtGround, yGround - 15);
            canvas.Children.Add(txtGround);

            // Base Line (z=0)
            var lineBase = new Line
            {
                X1 = marginLeft, Y1 = yBase, X2 = width - marginRight, Y2 = yBase,
                Stroke = Brushes.Black,
                StrokeThickness = 2.0
            };
            canvas.Children.Add(lineBase);

            var txtBase = new TextBlock
            {
                Text = "Cao trình đáy móng",
                FontWeight = FontWeights.Bold,
                FontSize = 10,
                Foreground = Brushes.Black
            };
            Canvas.SetLeft(txtBase, marginLeft);
            Canvas.SetTop(txtBase, yBase - 15);
            canvas.Children.Add(txtBase);

            // Hc Line (Giới hạn dưới chiều dày chịu nén)
            var lineHc = new Line
            {
                X1 = marginLeft, Y1 = yHc, X2 = width - marginRight, Y2 = yHc,
                Stroke = new SolidColorBrush(Color.FromRgb(220, 38, 38)), // Red
                StrokeThickness = 2.0
            };
            canvas.Children.Add(lineHc);

            var txtHc = new TextBlock
            {
                Text = $"Giới hạn dưới chiều dày chịu nén (Hc = {Hc:F2}m)",
                FontWeight = FontWeights.Bold,
                FontSize = 10,
                Foreground = new SolidColorBrush(Color.FromRgb(220, 38, 38))
            };
            Canvas.SetLeft(txtHc, marginLeft);
            Canvas.SetTop(txtHc, yHc + 3);
            canvas.Children.Add(txtHc);

            // -------------------------------------------------------------
            // 4. FOOTING SYMBOL AT TOP (z <= 0)
            // -------------------------------------------------------------
            double footingWidthPx = 80;
            var footingRect = new Rectangle
            {
                Width = footingWidthPx,
                Height = yBase - yGround,
                Fill = new SolidColorBrush(Color.FromArgb(40, 148, 163, 184)),
                Stroke = Brushes.DimGray,
                StrokeThickness = 1.5
            };
            Canvas.SetLeft(footingRect, xCenter - footingWidthPx / 2.0);
            Canvas.SetTop(footingRect, yGround);
            canvas.Children.Add(footingRect);

            // Column stub on footing
            var colRect = new Rectangle
            {
                Width = 24,
                Height = yBase - yGround + 10,
                Fill = new SolidColorBrush(Color.FromRgb(203, 213, 225)),
                Stroke = Brushes.SlateGray,
                StrokeThickness = 1
            };
            Canvas.SetLeft(colRect, xCenter - 12);
            Canvas.SetTop(colRect, yGround - 10);
            canvas.Children.Add(colRect);

            // -------------------------------------------------------------
            // 5. STRESS DIAGRAMS (TCVN 9362:2012 HÌNH C.1)
            // -------------------------------------------------------------
            // A. LEFT SIDE: pdz (LINEAR SLOPED LINE FOR OVERBURDEN STRESS)
            var polylinePdz = new Polyline
            {
                Stroke = new SolidColorBrush(Color.FromRgb(22, 163, 74)), // Green
                StrokeThickness = 2.5
            };
            // pd at ground level (z = -h1) is 0
            polylinePdz.Points.Add(new Point(xCenter, yGround));

            // pd at base level (z = 0)
            double pd_base = layers.Count > 0 ? layers[0].SumGammaHi : 1.7778;
            double xPdBase = xCenter - (pd_base * scaleLeft);
            polylinePdz.Points.Add(new Point(xPdBase, yBase));

            // B. LEFT DASHED LINE: 0.1 * pdz (LIMIT LINE ON LEFT)
            var polyline01Pdz = new Polyline
            {
                Stroke = new SolidColorBrush(Color.FromRgb(37, 99, 235)), // Royal Blue
                StrokeThickness = 1.8,
                StrokeDashArray = new DoubleCollection { 4, 3 }
            };
            polyline01Pdz.Points.Add(new Point(xCenter, yGround));
            polyline01Pdz.Points.Add(new Point(xCenter - (0.10 * pd_base * scaleLeft), yBase));

            // C. RIGHT SIDE: poz (NON-LINEAR CURVE FOR ADDITIONAL STRESS)
            var polylinePoz = new Polyline
            {
                Stroke = new SolidColorBrush(Color.FromRgb(225, 29, 72)), // Crimson Red
                StrokeThickness = 2.5
            };
            // p0 at base level (z = 0)
            double xP0Base = xCenter + (result.Sigma0 * scaleRight);
            polylinePoz.Points.Add(new Point(xP0Base, yBase));

            // Add Points along depth z
            foreach (var l in layers)
            {
                double y = ScaleY(l.Z);

                // pdz to the left (Linear piecewise)
                double xPdz = xCenter - (l.SumGammaHi * scaleLeft);
                polylinePdz.Points.Add(new Point(xPdz, y));

                // 0.1 * pdz to the left
                double limitFactor = (l.Ei < 500.0 ? 0.10 : 0.20);
                double x01Pdz = xCenter - (limitFactor * l.SumGammaHi * scaleLeft);
                polyline01Pdz.Points.Add(new Point(x01Pdz, y));

                // poz to the right (Non-linear curve)
                double xPoz = xCenter + (l.SigmaZi * scaleRight);
                polylinePoz.Points.Add(new Point(xPoz, y));

                // Small dot on poz curve
                var dotPoz = new Ellipse
                {
                    Width = 4, Height = 4,
                    Fill = new SolidColorBrush(Color.FromRgb(225, 29, 72)),
                    Margin = new Thickness(xPoz - 2, y - 2, 0, 0)
                };
                canvas.Children.Add(dotPoz);
            }

            canvas.Children.Add(polylinePdz);
            canvas.Children.Add(polyline01Pdz);
            canvas.Children.Add(polylinePoz);

            // -------------------------------------------------------------
            // 6. STRESS ANNOTATIONS & ARROWS (pd, p0, pdz, poz)
            // -------------------------------------------------------------
            // Label pd at base
            var txtPd = new TextBlock
            {
                Text = $"pd = {pd_base:F2}",
                FontSize = 9,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(22, 163, 74))
            };
            Canvas.SetLeft(txtPd, xPdBase - 35);
            Canvas.SetTop(txtPd, yBase + 4);
            canvas.Children.Add(txtPd);

            // Label p0 at base
            var txtP0 = new TextBlock
            {
                Text = $"p0 = {result.Sigma0:F4}",
                FontSize = 9,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(225, 29, 72))
            };
            Canvas.SetLeft(txtP0, xP0Base + 5);
            Canvas.SetTop(txtP0, yBase + 4);
            canvas.Children.Add(txtP0);

            // Sublayer hatching band indicator for sample sublayer
            if (layers.Count >= 5)
            {
                var sampleLayer = layers[4]; // around layer 5
                double ySample = ScaleY(sampleLayer.Z);

                var hatchBand = new Rectangle
                {
                    Width = availableWidthLeft + availableWidthRight,
                    Height = 12,
                    Fill = new SolidColorBrush(Color.FromArgb(30, 234, 179, 8)), // Yellow hatch band
                    Stroke = new SolidColorBrush(Color.FromRgb(234, 179, 8)),
                    StrokeThickness = 1,
                    StrokeDashArray = new DoubleCollection { 2, 2 }
                };
                Canvas.SetLeft(hatchBand, xCenter - availableWidthLeft);
                Canvas.SetTop(hatchBand, ySample - 6);
                canvas.Children.Add(hatchBand);

                var txtPi = new TextBlock
                {
                    Text = $"Giới hạn lớp {sampleLayer.Id} (pi = {sampleLayer.Si:F2}mm)",
                    FontSize = 9,
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush(Color.FromRgb(161, 98, 7))
                };
                Canvas.SetLeft(txtPi, xCenter + (sampleLayer.SigmaZi * scaleRight) + 8);
                Canvas.SetTop(txtPi, ySample - 6);
                canvas.Children.Add(txtPi);
            }

            // -------------------------------------------------------------
            // 7. TITLE & LEGEND (TCVN 9362:2012 HÌNH C.1)
            // -------------------------------------------------------------
            var title = new TextBlock
            {
                Text = "SƠ ĐỒ TÍNH LÚN THEO TCVN 9362:2012 (HÌNH C.1)",
                FontWeight = FontWeights.Bold,
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(30, 41, 59))
            };
            Canvas.SetLeft(title, marginLeft);
            Canvas.SetTop(title, 8);
            canvas.Children.Add(title);

            // Legend at bottom
            var legend = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(marginLeft, height - 22, 0, 0)
            };

            var leg1Dot = new Border { Width = 10, Height = 3, Background = new SolidColorBrush(Color.FromRgb(22, 163, 74)), VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0,0,4,0) };
            var leg1Txt = new TextBlock { Text = "◄ pdz (Ứng suất bản thân - Tuyến tính)", FontSize = 9, Foreground = Brushes.DarkSlateGray, Margin = new Thickness(0,0,12,0) };
            legend.Children.Add(leg1Dot);
            legend.Children.Add(leg1Txt);

            var leg2Dot = new Border { Width = 10, Height = 3, Background = new SolidColorBrush(Color.FromRgb(225, 29, 72)), VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0,0,4,0) };
            var leg2Txt = new TextBlock { Text = "poz (Ứng suất gây lún - Đường cong) ►", FontSize = 9, Foreground = Brushes.DarkSlateGray, Margin = new Thickness(0,0,12,0) };
            legend.Children.Add(leg2Dot);
            legend.Children.Add(leg2Txt);

            var leg3Dot = new Border { Width = 10, Height = 3, Background = new SolidColorBrush(Color.FromRgb(37, 99, 235)), VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0,0,4,0) };
            var leg3Txt = new TextBlock { Text = "◄ 0.1 pdz (Ranh giới lún)", FontSize = 9, Foreground = Brushes.DarkSlateGray };
            legend.Children.Add(leg3Dot);
            legend.Children.Add(leg3Txt);

            canvas.Children.Add(legend);
        }
    }
}
