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

            // Depth range: from Ground (-h1) to Hc * 1.15
            double minZ = -embedmentDepth;
            double maxZ = Hc * 1.15;
            if (maxZ <= 0) maxZ = 10.0;
            double totalZRange = maxZ - minZ;

            // Margins
            double marginTop = 40;
            double marginBottom = 30;
            double marginLeft = 70;
            double marginRight = 40;
            double chartWidth = width - marginLeft - marginRight;
            double chartHeight = height - marginTop - marginBottom;

            // Max Stress calculation for X-scaling
            double maxStress = result.Sigma0;
            foreach (var l in layers)
            {
                if (l.SigmaZi > maxStress) maxStress = l.SigmaZi;
                if (l.SumGammaHi > maxStress) maxStress = l.SumGammaHi;
            }
            if (maxStress <= 0) maxStress = 10.0;
            maxStress *= 1.15; // 15% headroom

            // Scale functions:
            // Y: Top is Ground (-h1), 0 is Base, bottom is maxZ
            double ScaleY(double z) => marginTop + ((z - minZ) / totalZRange) * chartHeight;
            double ScaleX(double sigma) => marginLeft + (sigma / maxStress) * chartWidth;

            double yGround = ScaleY(-embedmentDepth);
            double yBase = ScaleY(0);

            // -------------------------------------------------------------
            // 1. BACKDROP & SOIL LAYERS
            // -------------------------------------------------------------
            // Soil layer background shading below base
            var soilBg = new Rectangle
            {
                Width = chartWidth,
                Height = ScaleY(maxZ) - yBase,
                Fill = new SolidColorBrush(Color.FromRgb(248, 250, 252))
            };
            Canvas.SetLeft(soilBg, marginLeft);
            Canvas.SetTop(soilBg, yBase);
            canvas.Children.Add(soilBg);

            // Active settlement region shaded fill (between Z=0 and Z=Hc)
            var activeZone = new Rectangle
            {
                Width = chartWidth,
                Height = ScaleY(Hc) - yBase,
                Fill = new SolidColorBrush(Color.FromArgb(25, 239, 68, 68)) // Subtle red tint
            };
            Canvas.SetLeft(activeZone, marginLeft);
            Canvas.SetTop(activeZone, yBase);
            canvas.Children.Add(activeZone);

            // -------------------------------------------------------------
            // 2. GRIDLINES & DEPTH LABELS
            // -------------------------------------------------------------
            // Horizontal gridlines every 1.0m
            for (double z = Math.Floor(minZ); z <= maxZ; z += 1.0)
            {
                if (z < minZ) continue;
                double y = ScaleY(z);

                var gridLine = new Line
                {
                    X1 = marginLeft, Y1 = y, X2 = marginLeft + chartWidth, Y2 = y,
                    Stroke = new SolidColorBrush(Color.FromRgb(241, 245, 249)),
                    StrokeThickness = 1
                };
                canvas.Children.Add(gridLine);

                string label = z < 0 ? $"z={z:F1}m (MĐ)" : (z == 0 ? "z=0.0m (Đáy)" : $"z={z:F1}m");
                var txtZ = new TextBlock
                {
                    Text = label,
                    FontSize = 9,
                    Foreground = new SolidColorBrush(Color.FromRgb(100, 116, 139))
                };
                Canvas.SetLeft(txtZ, 5);
                Canvas.SetTop(txtZ, y - 6);
                canvas.Children.Add(txtZ);
            }

            // -------------------------------------------------------------
            // 3. KEY ELEVATION LINES (Ground, Base, Hc)
            // -------------------------------------------------------------
            // Ground Line (-h1)
            var lineGround = new Line
            {
                X1 = marginLeft, Y1 = yGround, X2 = marginLeft + chartWidth, Y2 = yGround,
                Stroke = new SolidColorBrush(Color.FromRgb(120, 53, 15)), // Brown
                StrokeThickness = 2
            };
            canvas.Children.Add(lineGround);

            var txtGround = new TextBlock
            {
                Text = $"▼ MẶT ĐẤT TỰ NHIÊN (h1 = {embedmentDepth:F1}m)",
                FontWeight = FontWeights.Bold,
                FontSize = 10,
                Foreground = new SolidColorBrush(Color.FromRgb(120, 53, 15))
            };
            Canvas.SetLeft(txtGround, marginLeft + 10);
            Canvas.SetTop(txtGround, yGround - 16);
            canvas.Children.Add(txtGround);

            // Base Line (z = 0)
            var lineBase = new Line
            {
                X1 = marginLeft, Y1 = yBase, X2 = marginLeft + chartWidth, Y2 = yBase,
                Stroke = Brushes.Black,
                StrokeThickness = 2.5
            };
            canvas.Children.Add(lineBase);

            var txtBase = new TextBlock
            {
                Text = $"▼ ĐÁY MÓNG (σ0 = {result.Sigma0:F4} T/m²)",
                FontWeight = FontWeights.Bold,
                FontSize = 10,
                Foreground = Brushes.Black
            };
            Canvas.SetLeft(txtBase, marginLeft + 10);
            Canvas.SetTop(txtBase, yBase + 4);
            canvas.Children.Add(txtBase);

            // -------------------------------------------------------------
            // 4. DRAW STRESS CURVES (TCVN 9362:2012 HÌNH C.1)
            // -------------------------------------------------------------
            // A. Curve 1: Ứng suất bản thân sigma_zg (Starts at 0 at Ground Level)
            var polylineZg = new Polyline
            {
                Stroke = new SolidColorBrush(Color.FromRgb(22, 163, 74)), // Green
                StrokeThickness = 2.0
            };
            // Point at Ground (-h1): sigma_zg = 0
            polylineZg.Points.Add(new Point(ScaleX(0), yGround));

            // Point at Base (z=0): sigma_zg_0 = SumGammaHi of layer 1 (approx gamma * h1)
            double sigma_zg_base = layers.Count > 0 ? layers[0].SumGammaHi : (1.7778);
            polylineZg.Points.Add(new Point(ScaleX(sigma_zg_base), yBase));

            // B. Curve 2: Ứng suất giới hạn 0.1 * sigma_zg (Blue Dashed)
            var polylineLimit = new Polyline
            {
                Stroke = new SolidColorBrush(Color.FromRgb(37, 99, 235)), // Royal Blue
                StrokeThickness = 2.0,
                StrokeDashArray = new DoubleCollection { 4, 3 }
            };
            polylineLimit.Points.Add(new Point(ScaleX(0), yGround));
            polylineLimit.Points.Add(new Point(ScaleX(0.10 * sigma_zg_base), yBase));

            // C. Curve 3: Ứng suất gây lún sigma_zp (Starts at sigma0 at Base Level z=0)
            var polylineZp = new Polyline
            {
                Stroke = new SolidColorBrush(Color.FromRgb(225, 29, 72)), // Crimson Red
                StrokeThickness = 2.5
            };

            foreach (var l in layers)
            {
                double y = ScaleY(l.Z);

                // sigma_zg below base
                double xZg = ScaleX(l.SumGammaHi);
                polylineZg.Points.Add(new Point(xZg, y));

                // 0.1 * sigma_zg below base
                double limitFactor = (l.Ei < 500.0 ? 0.10 : 0.20);
                double xLimit = ScaleX(limitFactor * l.SumGammaHi);
                polylineLimit.Points.Add(new Point(xLimit, y));

                // sigma_zp below base
                double xZp = ScaleX(l.SigmaZi);
                polylineZp.Points.Add(new Point(xZp, y));

                // Dot on sigma_zp curve
                var dot = new Ellipse
                {
                    Width = 4, Height = 4,
                    Fill = new SolidColorBrush(Color.FromRgb(225, 29, 72)),
                    Margin = new Thickness(xZp - 2, y - 2, 0, 0)
                };
                canvas.Children.Add(dot);
            }

            canvas.Children.Add(polylineZg);
            canvas.Children.Add(polylineLimit);
            canvas.Children.Add(polylineZp);

            // -------------------------------------------------------------
            // 5. INFLUENCE DEPTH HC LINE & ANNOTATION
            // -------------------------------------------------------------
            if (result.InfluenceDepth > 0)
            {
                double yHc = ScaleY(result.InfluenceDepth);
                var lineHc = new Line
                {
                    X1 = marginLeft, Y1 = yHc, X2 = marginLeft + chartWidth, Y2 = yHc,
                    Stroke = new SolidColorBrush(Color.FromRgb(217, 119, 6)), // Amber
                    StrokeThickness = 2,
                    StrokeDashArray = new DoubleCollection { 3, 2 }
                };
                canvas.Children.Add(lineHc);

                var txtHc = new TextBlock
                {
                    Text = $"---> ĐÁY VÙNG LÚN Hc = {result.InfluenceDepth:F2} m (σzp ≤ 0.1Σγh)",
                    FontWeight = FontWeights.Bold,
                    FontSize = 10,
                    Foreground = new SolidColorBrush(Color.FromRgb(217, 119, 6))
                };
                Canvas.SetLeft(txtHc, marginLeft + 15);
                Canvas.SetTop(txtHc, yHc - 15);
                canvas.Children.Add(txtHc);
            }

            // -------------------------------------------------------------
            // 6. TITLE & LEGEND (TCVN 9362:2012 HÌNH C.1)
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

            // Legend item 1: sigma_zp
            var leg1Dot = new Border { Width = 10, Height = 3, Background = new SolidColorBrush(Color.FromRgb(225, 29, 72)), VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0,0,4,0) };
            var leg1Txt = new TextBlock { Text = "σzp (Gây lún)", FontSize = 9, Foreground = Brushes.DarkSlateGray, Margin = new Thickness(0,0,10,0) };
            legend.Children.Add(leg1Dot);
            legend.Children.Add(leg1Txt);

            // Legend item 2: sigma_zg
            var leg2Dot = new Border { Width = 10, Height = 3, Background = new SolidColorBrush(Color.FromRgb(22, 163, 74)), VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0,0,4,0) };
            var leg2Txt = new TextBlock { Text = "σzg (Bản thân)", FontSize = 9, Foreground = Brushes.DarkSlateGray, Margin = new Thickness(0,0,10,0) };
            legend.Children.Add(leg2Dot);
            legend.Children.Add(leg2Txt);

            // Legend item 3: 0.1 sigma_zg
            var leg3Dot = new Border { Width = 10, Height = 3, Background = new SolidColorBrush(Color.FromRgb(37, 99, 235)), VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0,0,4,0) };
            var leg3Txt = new TextBlock { Text = "0.1Σγ.h (Ranh giới)", FontSize = 9, Foreground = Brushes.DarkSlateGray };
            legend.Children.Add(leg3Dot);
            legend.Children.Add(leg3Txt);

            canvas.Children.Add(legend);
        }
    }
}
