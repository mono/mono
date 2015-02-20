//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace System.Activities.Presentation.View
{
    using System;
    using System.Activities.Presentation.Internal.PropertyEditing;
    using System.Activities.Presentation.Model;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Windows;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Shapes;

    internal sealed class RubberBandSelector
    {
        private const double RubberBandThickness = 2;
        private EditingContext context;
        private Rectangle rubberBand;
        private List<WorkflowViewElement> views = new List<WorkflowViewElement>();

        public RubberBandSelector(EditingContext context)
        {
            this.context = context;
            this.InitializeRubberBand();
        }

        public bool IsSelected
        {
            get;
            set;
        }

        // Relative to the scrollable content
        private Point StartPoint
        {
            get;
            set;
        }

        // Relative to the scrollable content
        private Point EndPoint
        {
            get;
            set;
        }

        // Relative to the scrollable content
        private Point MouseDownPointToScreen { get; set; }

        private bool IsSelecting
        {
            get
            {
                return this.ExtenstionSurface.Children.Contains(this.rubberBand);
            }
        }

        private bool IsReadyForSelecting
        {
            get;
            set;
        }

        private ExtensionSurface ExtenstionSurface
        {
            get
            {
                return this.Designer.wfViewExtensionSurface;
            }
        }

        private DesignerView Designer
        {
            get
            {
                return this.context.Services.GetService<DesignerView>();
            }
        }

        public void RegisterViewElement(WorkflowViewElement view)
        {
            if (!this.views.Contains(view))
            {
                this.views.Add(view);
            }
        }

        public void UnregisterViewElement(WorkflowViewElement view)
        {
            if (this.views.Contains(view))
            {
                this.views.Remove(view);
            }
        }

        public void OnScrollViewerMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            this.IsSelected = false;

            // Start rubber band selection if left button down is not handled by UI elements other than WorkflowViewElement.
            if (!e.Handled || this.Designer.ShouldStillAllowRubberBandEvenIfMouseLeftButtonDownIsHandled)
            {
                if (DesignerView.IsMouseInViewport(e, this.Designer.scrollViewer) && !this.IsMouseOnDragHandle(e) && !this.IsMouseOverAdorner(e))
                {
                    this.IsReadyForSelecting = true;
                    this.StartPoint = e.GetPosition(this.Designer.scrollableContent);

                    this.MouseDownPointToScreen = this.Designer.scrollableContent.PointToScreen(this.StartPoint);
                    this.EndPoint = this.StartPoint;
                }
            }
        }

        public void OnScrollViewerMouseMove(MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && this.IsReadyForSelecting)
            {
                Point position = e.GetPosition(this.Designer.scrollableContent);
                Point positionToScreen = this.Designer.scrollableContent.PointToScreen(position);
                if (!this.IsSelecting && (Math.Abs(positionToScreen.X - this.MouseDownPointToScreen.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(positionToScreen.Y - this.MouseDownPointToScreen.Y) > SystemParameters.MinimumVerticalDragDistance))
                {
                    this.AddRubberBand();
                    if (!this.Designer.scrollableContent.IsMouseCaptured)
                    {
                        this.Designer.scrollableContent.CaptureMouse();
                    }

                    e.Handled = true;
                }

                if (this.IsSelecting)
                {
                    this.EndPoint = position;
                    this.UpdateRubberBand();
                    AutoScrollHelper.AutoScroll(e.GetPosition(this.Designer.scrollViewer), this.Designer.scrollViewer, 0.2);
                    e.Handled = true;
                }
            }
        }

        public void OnScrollViewerPreviewMouseLeftButtonUp(MouseEventArgs e)
        {
            if (this.IsSelecting)
            {
                this.EndPoint = e.GetPosition(this.Designer.scrollableContent);
                this.Select(Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl));
                this.IsSelected = true;
            }

            this.StopSelecting();
        }

        public void OnScrollViewerMouseLeave()
        {
            this.StopSelecting();
        }

        public void OnScrollViewerEscapeKeyDown()
        {
            this.StopSelecting();
        }

        private static Point ClipPoint(Point point)
        {
            // Negative vaule means the top left corner of the rubber band is outside of the viewport. 
            // We need to clip the rubber band since a very negative value will cause some WPF display issues.
            // But we use -RubberBandThickness instead of 0 to clip so that the border of the rubber band that should be outside of the viewport is still outside.
            return new Point(point.X < -RubberBandThickness ? -RubberBandThickness : point.X, point.Y < -RubberBandThickness ? -RubberBandThickness : point.Y);
        }

        private void StopSelecting()
        {
            this.IsReadyForSelecting = false;
            if (this.IsSelecting)
            {
                this.RemoveRubberBand();
                if (this.Designer.scrollableContent.IsMouseCaptured)
                {
                    this.Designer.scrollableContent.ReleaseMouseCapture();
                }
            }
        }

        private bool IsMouseOnDragHandle(MouseButtonEventArgs e)
        {
            HitTestResult result = VisualTreeHelper.HitTest(this.Designer.scrollableContent, e.GetPosition(this.Designer.scrollableContent));
            if (result != null)
            {
                WorkflowViewElement view = VisualTreeUtils.FindVisualAncestor<WorkflowViewElement>(result.VisualHit);
                if (view != null && view.DragHandle != null)
                {
                    GeneralTransform transform = view.DragHandle.TransformToAncestor(this.Designer);
                    Fx.Assert(transform != null, "transform should not be null");
                    Point topLeft = transform.Transform(new Point(0, 0));
                    Point bottomRight = transform.Transform(new Point(view.DragHandle.ActualWidth, view.DragHandle.ActualHeight));
                    Rect dragHandleRect = new Rect(topLeft, bottomRight);
                    if (dragHandleRect.Contains(e.GetPosition(this.Designer)))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool IsMouseOverAdorner(MouseButtonEventArgs e)
        {
            HitTestResult result = VisualTreeHelper.HitTest(this.Designer.scrollViewer, e.GetPosition(this.Designer.scrollViewer));
            return result != null && VisualTreeUtils.FindVisualAncestor<Adorner>(result.VisualHit) != null;
        }

        private void AddRubberBand()
        {
            if (!this.ExtenstionSurface.Children.Contains(this.rubberBand))
            {
                this.ExtenstionSurface.Children.Add(this.rubberBand);
            }
            else
            {
                Fx.Assert(false, "Old rubber band was not correctly removed.");
            }
        }

        private void RemoveRubberBand()
        {
            if (this.ExtenstionSurface.Children.Contains(this.rubberBand))
            {
                this.ExtenstionSurface.Children.Remove(this.rubberBand);
            }
            else
            {
                Fx.Assert(false, "Rubber band was not correctly added.");
            }
        }

        private void UpdateRubberBand()
        {
            if (this.ExtenstionSurface.Children.Contains(this.rubberBand))
            {
                // Transform the start and end points to be relative to the extension surface by transforming to the common ancestor (DesignerView) first
                GeneralTransform transform1 = this.Designer.scrollableContent.TransformToAncestor(this.Designer);
                GeneralTransform transform2 = this.Designer.TransformToDescendant(this.ExtenstionSurface);
                Point start = ClipPoint(transform2.Transform(transform1.Transform(this.StartPoint)));
                Point end = ClipPoint(transform2.Transform(transform1.Transform(this.EndPoint)));
                Rect rect = new Rect(start, end);
                this.rubberBand.Width = rect.Width;
                this.rubberBand.Height = rect.Height;
                this.rubberBand.InvalidateVisual();
                ExtensionSurface.SetPosition(this.rubberBand, rect.TopLeft);
            }
            else
            {
                Fx.Assert(false, "Rubber band was not correctly added.");
            }
        }

        private void Select(bool isCtrlKeyDown)
        {
            bool isRubberBandEmpty = true;
            Rect rubberBandRect = new Rect(this.StartPoint, this.EndPoint);
            List<ModelItem> selectedModelItems = new List<ModelItem>();
            foreach (WorkflowViewElement view in this.views)
            {
                GeneralTransform transform = view.TransformToAncestor(this.Designer.scrollableContent);
                Point location = transform.Transform(new Point(0, 0));
                Rect rect = new Rect(location.X, location.Y, view.ActualWidth, view.ActualHeight);
                if (rubberBandRect.Contains(rect))
                {
                    isRubberBandEmpty = false;
                    if (isCtrlKeyDown)
                    {
                        Selection.Toggle(this.context, view.ModelItem);
                    }
                    else
                    {
                        // Make sure the rubber-band selection has the same order
                        // and keyboard focus as ctrl+click one by one, which 
                        // 1) model item is added in reverse order
                        // 2) last model item, which is the first in selection array,
                        //    gets focus
                        selectedModelItems.Insert(0, view.ModelItem);
                    }
                }
            }

            if (selectedModelItems.Count > 0)
            {
                Keyboard.Focus(selectedModelItems[0].View as IInputElement);
                this.context.Items.SetValue(new Selection(selectedModelItems));
            }

            if (isRubberBandEmpty && !isCtrlKeyDown
                && this.ShouldClearSelectioinIfNothingSelected())
            {
                this.context.Items.SetValue(new Selection());
            }
        }

        private bool ShouldClearSelectioinIfNothingSelected()
        {
            Selection curSelection = this.context.Items.GetValue<Selection>();
            if (curSelection == null || curSelection.SelectionCount == 0)
            {
                return false;
            }

            // only one ModelItem is selected and the ModelItem is root designer.
            // do not clear selection
            if (curSelection.SelectionCount == 1)
            {
                ModelItem item = curSelection.PrimarySelection;
                WorkflowViewElement view = item == null ? null : (item.View as WorkflowViewElement);
                if (view != null && view.IsRootDesigner)
                {
                    return false;
                }
            }

            return true;
        }

        private void InitializeRubberBand()
        {
            this.rubberBand = new Rectangle();
            this.rubberBand.StrokeThickness = RubberBandThickness;
            this.rubberBand.Stroke = WorkflowDesignerColors.RubberBandRectangleBrush;
            this.rubberBand.Fill = WorkflowDesignerColors.RubberBandRectangleBrush.Clone();
            this.rubberBand.Fill.Opacity = 0.2;
        }
    }
}
