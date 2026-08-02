using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using FAE.Foundation.App.Features.RibbedRaft.Drawers;

namespace FAE.Foundation.App.Features.RibbedRaft
{
    public partial class RibbedRaftView : UserControl
    {
        private RibbedRaftViewModel _viewModel;

        public RibbedRaftView()
        {
            InitializeComponent();
            this.DataContextChanged += RibbedRaftView_DataContextChanged;
        }

        private void RibbedRaftView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (_viewModel != null)
            {
                _viewModel.DrawRequested -= RedrawCanvas;
            }

            _viewModel = e.NewValue as RibbedRaftViewModel;
            if (_viewModel != null)
            {
                _viewModel.DrawRequested += RedrawCanvas;
                RedrawCanvas();
            }
        }

        // --- 3D Rotation Logic ---
        private bool _isDragging3D = false;
        private Point _lastMousePos;

        private void Viewport3D_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isDragging3D = true;
            _lastMousePos = e.GetPosition(this);
            ((UIElement)sender).CaptureMouse();
        }

        private void Viewport3D_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isDragging3D = false;
            ((UIElement)sender).ReleaseMouseCapture();
        }

        private void Viewport3D_MouseMove(object sender, MouseEventArgs e)
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

        private void DrawingCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize == e.PreviousSize || e.NewSize.Width == 0 || e.NewSize.Height == 0) return;
            RedrawCanvas();
        }

        private void RedrawCanvas()
        {
            if (_viewModel == null) return;
            var model = _viewModel.Model;
            
            if (SectionXCanvas != null)
                SectionDrawer.DrawFoundation(SectionXCanvas, model, isSectionY: false);
            
            if (SectionYCanvas != null)
                SectionDrawer.DrawFoundation(SectionYCanvas, model, isSectionY: true);

            if (PlanCanvas != null)
                PlanDrawer.DrawPlan(PlanCanvas, model);

            if (Model3DGroup != null)
                Viewport3DDrawer.Draw3D(Model3DGroup, model);
        }
    }
}
