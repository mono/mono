//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation.View
{
    using System.Activities.Presentation;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Documents;
    using System.Windows.Media;
    using System.Windows.Media.Effects;

    class SearchToolTipAdorner : Adorner
    {
        Border tooltip;
        double scrollViewerToScreenDistance;
        public SearchToolTipAdorner(UIElement adornedElement, DesignerView designerView, string text)
            : base(adornedElement)
        {
            this.scrollViewerToScreenDistance = designerView.ScrollViewer.PointToScreen(new Point(0, 0)).Y;;
            tooltip = new Border
            {
                Background = new SolidColorBrush(WorkflowDesignerColors.DesignerViewBackgroundColor),
                BorderBrush = new SolidColorBrush(WorkflowDesignerColors.WorkflowViewElementBorderColor),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(4),
                Margin = new Thickness(10),
                Child = new TextBlock
                {
                    Foreground = new SolidColorBrush(WorkflowDesignerColors.WorkflowViewElementCaptionColor),
                    Margin = new Thickness(4, 0, 4, 0),
                    MaxWidth = 300,
                    Text = text,
                    TextWrapping = TextWrapping.Wrap,
                    TextTrimming = TextTrimming.CharacterEllipsis
                },
                Effect = new DropShadowEffect
                {
                    Color = Colors.Black,
                    BlurRadius = 4,
                    Opacity = 0.5
                },
            };
            tooltip.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            VisualBrush brush = new VisualBrush()
            {
                Visual = tooltip
            };

            double adornerElementToVisibleScrollViewDistance =
                this.AdornedElement.PointToScreen(new Point(0, 0)).Y - this.scrollViewerToScreenDistance;
            double Y = adornerElementToVisibleScrollViewDistance < tooltip.DesiredSize.Height ?
                tooltip.DesiredSize.Height :
                -tooltip.DesiredSize.Height;
            Rect tooltipRect = new Rect(new Point(0, Y), tooltip.DesiredSize);
            
            Pen renderPen = new Pen();
            drawingContext.DrawRectangle(brush, renderPen, tooltipRect);
        }
    }
}
