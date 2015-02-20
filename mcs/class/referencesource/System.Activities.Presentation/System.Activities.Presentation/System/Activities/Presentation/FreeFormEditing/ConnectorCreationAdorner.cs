//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Presentation.FreeFormEditing
{
    using System.Activities.Presentation;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Documents;
    using System.Windows.Media;
    using System.Runtime;

    sealed class ConnectorCreationAdorner : Adorner
    {
        List<Point> linkPoints;
        public ConnectorCreationAdorner(UIElement adornedElement, List<Point> linkPoints)
            : base(adornedElement)
        {
            Fx.Assert(adornedElement != null, "adornedElement is null");
            this.IsHitTestVisible = false;
            this.linkPoints = linkPoints;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (drawingContext != null)
            {
                SolidColorBrush renderBrush = new SolidColorBrush(WorkflowDesignerColors.WorkflowViewElementSelectedBorderColor);
                Pen renderPen = new Pen(renderBrush, FreeFormPanel.ConnectorEditorThickness);
                for (int i = 0; i < linkPoints.Count - 1; i++)
                {
                    drawingContext.DrawLine(renderPen, linkPoints[i], linkPoints[i + 1]);
                }
            }
            base.OnRender(drawingContext);
        }
    }
}
