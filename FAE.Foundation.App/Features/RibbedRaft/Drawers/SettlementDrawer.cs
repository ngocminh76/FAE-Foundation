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

            double margin = 35;
            double chartWidth = width - 2 * margin;
            double chartHeight = height - 2 * margin - 15;

            var layers = result.SettlementLayers;
            double maxZ = result.InfluenceDepth > 0 ? result.InfluenceDepth * 1.15 : layers[layers.Count - 1].Z;
            if (maxZ <= 0) maxZ = 10.0;

            double maxSigma = 0;
            foreach (var l in layers)
            {
                if (l.SigmaZi > maxSigma) maxSigma = l.SigmaZi;
                double limit = (l.Ei < 500.0 ? 0.10 : 0.20) * l.SumGammaHi;
                if (limit > maxSigma) maxSigma = limit;
            }
            if (maxSigma <= 0) maxSigma = 1.5;
            maxSigma *= 1.15; // 15% headroom

            double ScaleY(double z) => margin + (z / maxZ) * chartHeight;
            double ScaleX(double sigma) => margin + (sigma / maxSigma) * chartWidth;

            // 1. Draw Gridlines & Axes
            for (double z = 0; z <= maxZ; z += 1.0)
            {
                double y = ScaleY(z);
                var line = new Line
                {
                    X1 = margin, Y1 = y, X2 = width - margin, Y2 = y,
                    Stroke = new SolidColorBrush(Color.FromRgb(241, 245, 249)),
                    StrokeThickness = 1
                };
                canvas.Children.Add(line);

                var txt = new TextBlock
                {
                    Text = $"{z:F1}m",
                    FontSize = 9,
                    Foreground = new SolidColorBrush(Color.FromRgb(100, 116, 139))
                };
                Canvas.SetLeft(txt, 2);
                Canvas.SetTop(txt, y - 6);
                canvas.Children.Add(txt);
            }

            var axisY = new Line
            {
                X1 = margin, Y1 = margin, X2 = margin, Y2 = margin + chartHeight,
                Stroke = Brushes.DarkGray, StrokeThickness = 1.5
            };
            canvas.Children.Add(axisY);

            var axisX = new Line
            {
                X1 = margin, Y1 = margin, X2 = margin + chartWidth, Y2 = margin,
                Stroke = Brushes.DarkGray, StrokeThickness = 1.5
            };
            canvas.Children.Add(axisX);

            // 2. Draw Curve 1: Ứng suất gia tăng sigma_zp (Red)
            var polylineSigma = new Polyline
            {
                Stroke = new SolidColorBrush(Color.FromRgb(225, 29, 72)),
                StrokeThickness = 2.5
            };

            // 3. Draw Curve 2: Ứng suất giới hạn (Blue Dashed)
            var polylineLimit = new Polyline
            {
                Stroke = new SolidColorBrush(Color.FromRgb(37, 99, 235)),
                StrokeThickness = 2,
                StrokeDashArray = new DoubleCollection { 4, 3 }
            };

            foreach (var l in layers)
            {
                double y = ScaleY(l.Z);
                double xSigma = ScaleX(l.SigmaZi);
                double limitFactor = (l.Ei < 500.0 ? 0.10 : 0.20);
                double xLimit = ScaleX(limitFactor * l.SumGammaHi);

                polylineSigma.Points.Add(new Point(xSigma, y));
                polylineLimit.Points.Add(new Point(xLimit, y));

                var dot = new Ellipse
                {
                    Width = 4, Height = 4,
                    Fill = new SolidColorBrush(Color.FromRgb(225, 29, 72)),
                    Margin = new Thickness(xSigma - 2, y - 2, 0, 0)
                };
                canvas.Children.Add(dot);
            }

            canvas.Children.Add(polylineLimit);
            canvas.Children.Add(polylineSigma);

            // 4. Draw Influence Depth Hc line
            if (result.InfluenceDepth > 0)
            {
                double yHc = ScaleY(result.InfluenceDepth);
                var hcLine = new Line
                {
                    X1 = margin, Y1 = yHc, X2 = width - margin, Y2 = yHc,
                    Stroke = new SolidColorBrush(Color.FromRgb(217, 119, 6)),
                    StrokeThickness = 2,
                    StrokeDashArray = new DoubleCollection { 2, 2 }
                };
                canvas.Children.Add(hcLine);

                var hcText = new TextBlock
                {
                    Text = $"---> Hc = {result.InfluenceDepth:F2} m",
                    FontWeight = FontWeights.Bold,
                    FontSize = 10,
                    Foreground = new SolidColorBrush(Color.FromRgb(217, 119, 6))
                };
                Canvas.SetLeft(hcText, margin + 10);
                Canvas.SetTop(hcText, yHc - 14);
                canvas.Children.Add(hcText);
            }

            // 5. Chart Title & Legend
            var title = new TextBlock
            {
                Text = "BIỂU ĐỒ ỨNG SUẤT VÀ VÙNG ẢNH HƯỞNG LÚN (TCVN 9362:2012)",
                FontWeight = FontWeights.Bold,
                FontSize = 10,
                Foreground = new SolidColorBrush(Color.FromRgb(30, 41, 59))
            };
            Canvas.SetLeft(title, margin);
            Canvas.SetTop(title, 6);
            canvas.Children.Add(title);

            var legend = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(margin, height - 20, 0, 0)
            };

            var leg1Dot = new Border { Width = 10, Height = 3, Background = new SolidColorBrush(Color.FromRgb(225, 29, 72)), VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0,0,4,0) };
            var leg1Txt = new TextBlock { Text = "Ứng suất σzp", FontSize = 9, Foreground = Brushes.DarkSlateGray, Margin = new Thickness(0,0,12,0) };
            legend.Children.Add(leg1Dot);
            legend.Children.Add(leg1Txt);

            var leg2Dot = new Border { Width = 10, Height = 3, Background = new SolidColorBrush(Color.FromRgb(37, 99, 235)), VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0,0,4,0) };
            var leg2Txt = new TextBlock { Text = "Ranh giới 0.1Σγ.h", FontSize = 9, Foreground = Brushes.DarkSlateGray };
            legend.Children.Add(leg2Dot);
            legend.Children.Add(leg2Txt);

            canvas.Children.Add(legend);
        }
    }
}
