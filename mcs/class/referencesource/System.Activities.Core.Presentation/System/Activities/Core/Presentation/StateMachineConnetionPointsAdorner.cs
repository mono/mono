//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Core.Presentation
{
    using System.Activities.Presentation;
    using System.Activities.Presentation.FreeFormEditing;
    using System.Activities.Presentation.View;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Runtime;
    using System.Windows.Controls;

    internal class StateMachineConnectionPointsAdorner : ConnectionPointsAdorner
    {
        public StateMachineConnectionPointsAdorner(UIElement adornedElement, List<ConnectionPoint> connectionPointsToShow, bool isParentShapeSelected)
            : base(adornedElement, connectionPointsToShow, isParentShapeSelected)
        {
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            Point actualPoint;
            Point origin = FreeFormPanel.GetLocation(AdornedElement);
            Thickness margin = ((FrameworkElement)AdornedElement).Margin;
            origin.X += margin.Left;
            origin.Y += margin.Top;

            foreach (ConnectionPoint connPoint in connectionPoints)
            {
                actualPoint = new Point(connPoint.Location.X - origin.X, connPoint.Location.Y - origin.Y);
                this.DrawConnectionPoint(connPoint, actualPoint, drawingContext);
            }
            
            base.OnRender(drawingContext);
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right)
            {
                this.RaiseMouseEvent(e);
            }
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right)
            {
                this.RaiseMouseEvent(e);
            }
            base.OnMouseUp(e);
        }

        protected override void OnContextMenuOpening(ContextMenuEventArgs e)
        {
            this.AdornedElement.RaiseEvent(e);
            base.OnContextMenuOpening(e);
        }

        private void RaiseMouseEvent(MouseButtonEventArgs e)
        {
            MouseButtonEventArgs args = new MouseButtonEventArgs(e.MouseDevice, e.Timestamp, e.ChangedButton);
            args.RoutedEvent = e.RoutedEvent;
            UIElement target = this.AdornedElement;
            if (target is VirtualizedContainerService.VirtualizingContainer)
            {
                target = ((VirtualizedContainerService.VirtualizingContainer)target).Child;
                if (target is StateDesigner)
                {
                    target = ((StateDesigner)target).stateContentPresenter;
                    target.RaiseEvent(args);
                }
            }
        }
    }
}
