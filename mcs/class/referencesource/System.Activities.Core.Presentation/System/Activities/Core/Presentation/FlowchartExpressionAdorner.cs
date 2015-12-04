//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Core.Presentation
{
    using System.Activities.Presentation;
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.View;
    using System.Runtime;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Documents;
    using System.Windows.Media;
    using System.Windows.Media.Effects;

    class FlowchartExpressionAdorner : Adorner
    {
        string expressionString;
        bool isTextRightToLeft;

        public FlowchartExpressionAdorner(UIElement adornedElement)
            : base(adornedElement)
        {
            expressionString = GetExpressionString();
            this.isTextRightToLeft = FreeFormPanelUtilities.IsRightToLeft(adornedElement);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            Transform transform = isTextRightToLeft ? new MatrixTransform(-1, 0, 0, 1, 0, 0) : Transform.Identity;
            Border tooltip = new Border
            {
                Background = new SolidColorBrush(Colors.White),
                BorderBrush = new SolidColorBrush(WorkflowDesignerColors.WorkflowViewElementBorderColor),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(4),
                Margin = new Thickness(10),
                Child = new TextBlock
                {
                    Margin = new Thickness(4, 0, 4, 0),
                    MaxHeight = 100,
                    Text = expressionString,
                },
                Effect = new DropShadowEffect
                {
                    Color = Colors.Black,
                    BlurRadius = 4,
                    Opacity = 0.5
                },
                RenderTransform = transform
            };
            tooltip.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));

            VisualBrush brush = new VisualBrush()
            {
                Visual = tooltip
            };

            Rect adornedElementRect = new Rect(this.AdornedElement.DesiredSize);
            Rect tooltipRect = new Rect(adornedElementRect.TopLeft + new Vector(adornedElementRect.Width, -tooltip.DesiredSize.Height), tooltip.DesiredSize);
            Pen renderPen = new Pen();
            drawingContext.DrawRectangle(brush, renderPen, tooltipRect);
        }        

        public static string GetExpressionString(UIElement adornedElement)
        {
            string expressionPropertyName;
            ModelItem modelItem;
            if (adornedElement is FlowDecisionDesigner)
            {
                expressionPropertyName = "Condition";
                modelItem = ((FlowDecisionDesigner)adornedElement).ModelItem;
            }
            else
            {
                Fx.Assert(adornedElement is FlowSwitchDesigner, "this.AdornedElement is either FlowDecisionDesigner or FlowSwitchDesigner");
                expressionPropertyName = "Expression";
                modelItem = ((FlowSwitchDesigner)adornedElement).ModelItem;
            }
            Activity expressionActivity = modelItem.Properties[expressionPropertyName].ComputedValue as Activity;
            return ExpressionHelper.GetExpressionString(expressionActivity, modelItem);
        }

        string GetExpressionString()
        {
            return GetExpressionString(this.AdornedElement);
        }
    }
}
