using System.Windows;
using System.Windows.Controls;
using FAE.Foundation.App.Features.RibbedRaft.Drawers;

namespace FAE.Foundation.App.Features.RibbedRaft
{
    public partial class GeotechCheckView : UserControl
    {
        private RibbedRaftViewModel _viewModel;

        public GeotechCheckView()
        {
            InitializeComponent();
            this.DataContextChanged += GeotechCheckView_DataContextChanged;
        }

        private void GeotechCheckView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
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

        private void DrawingCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize == e.PreviousSize || e.NewSize.Width == 0 || e.NewSize.Height == 0) return;
            RedrawCanvas();
        }

        private void RedrawCanvas()
        {
            if (_viewModel == null || _viewModel.Model == null) return;
            var model = _viewModel.Model;
            
            if (SectionXCanvas != null)
                SectionDrawer.DrawFoundation(SectionXCanvas, model, _viewModel.CurrentBorehole, isSectionY: false);
            
            if (SectionYCanvas != null)
                SectionDrawer.DrawFoundation(SectionYCanvas, model, _viewModel.CurrentBorehole, isSectionY: true);

            if (SettlementCanvas != null)
                SettlementDrawer.DrawSettlementDiagram(SettlementCanvas, _viewModel.CalculationResult, _viewModel.CurrentBorehole);
        }
    }
}
