using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Collections;
using System.Drawing;
using System.Workflow.ComponentModel;
using System.Workflow.ComponentModel.Design;
using System.Windows.Forms;
using System.Diagnostics;

namespace System.Workflow.ComponentModel.Design
{
    internal sealed class ResizingMessageFilter : WorkflowDesignerMessageFilter
    {
        private DesignerTransaction designerTransaction;
        private ActivityDesigner designerToResize = null;
        private DesignerEdges designerSizingEdge = DesignerEdges.None;

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        protected override bool OnMouseDown(MouseEventArgs eventArgs)
        {
            if (eventArgs.Button == MouseButtons.Left)
            {
                WorkflowView workflowView = ParentView;
                if (workflowView == null)
                    throw new InvalidOperationException(DR.GetString(DR.WorkflowViewNull));

                Point clientPoint = new Point(eventArgs.X, eventArgs.Y);
                Point logicalPoint = workflowView.ClientPointToLogical(clientPoint);

                DesignerEdges sizingEdge = DesignerEdges.None;
                ActivityDesigner designer = GetDesignerToResize(logicalPoint, out sizingEdge);
                if (designer != null && sizingEdge != DesignerEdges.None && CanResizeDesigner(designer))
                    SetResizableDesigner(designer, sizingEdge);
            }

            return (this.designerToResize != null);
        }

        protected override bool OnMouseMove(MouseEventArgs eventArgs)
        {
            WorkflowView workflowView = ParentView;
            if (workflowView == null)
                throw new InvalidOperationException(DR.GetString(DR.WorkflowViewNull));

            bool handledMessage = false;
            Point clientPoint = new Point(eventArgs.X, eventArgs.Y);
            Point logicalPoint = workflowView.ClientPointToLogical(clientPoint);
            DesignerEdges sizingEdge = DesignerEdges.None;

            if (this.designerToResize != null)
            {
                sizingEdge = this.designerSizingEdge;
                UpdateDesignerSize(logicalPoint, this.designerToResize, this.designerSizingEdge);
                handledMessage = true;
            }
            else if (eventArgs.Button == MouseButtons.None)
            {
                ActivityDesigner designer = GetDesignerToResize(logicalPoint, out sizingEdge);
                if (designer != null && sizingEdge != DesignerEdges.None && CanResizeDesigner(designer))
                    handledMessage = true;
            }

            UpdateCursor(sizingEdge);
            return handledMessage;
        }

        protected override bool OnMouseUp(MouseEventArgs eventArgs)
        {
            if (this.designerToResize != null && eventArgs.Button == MouseButtons.Left)
            {
                WorkflowView workflowView = ParentView;
                if (workflowView == null)
                    throw new InvalidOperationException(DR.GetString(DR.WorkflowViewNull));

                UpdateDesignerSize(workflowView.ClientPointToLogical(new Point(eventArgs.X, eventArgs.Y)), this.designerToResize, this.designerSizingEdge);
            }
            SetResizableDesigner(null, DesignerEdges.None);
            return false;
        }

        protected override bool OnMouseLeave()
        {
            if (this.designerToResize != null)
                SetResizableDesigner(null, DesignerEdges.None);
            else
                UpdateCursor(DesignerEdges.None);
            return false;
        }

        protected override bool OnKeyDown(KeyEventArgs eventArgs)
        {
            if (eventArgs.KeyValue == (int)Keys.Escape && this.designerToResize != null)
            {
                SetResizableDesigner(null, DesignerEdges.None);
                return true;
            }
            else
            {
                return false;
            }
        }

        protected override bool OnMouseCaptureChanged()
        {
            if (this.designerToResize != null)
                SetResizableDesigner(null, DesignerEdges.None);
            return false;
        }

        private void UpdateCursor(DesignerEdges sizingEdge)
        {
            WorkflowView workflowView = ParentView;
            if (workflowView == null)
                throw new InvalidOperationException(DR.GetString(DR.WorkflowViewNull));

            Cursor cursorToSet = workflowView.Cursor;
            if (((sizingEdge & DesignerEdges.Left) > 0 && (sizingEdge & DesignerEdges.Top) > 0) ||
                ((sizingEdge & DesignerEdges.Right) > 0 && (sizingEdge & DesignerEdges.Bottom) > 0))
            {
                cursorToSet = Cursors.SizeNWSE;
            }
            else if (((sizingEdge & DesignerEdges.Right) > 0 && (sizingEdge & DesignerEdges.Top) > 0) ||
                ((sizingEdge & DesignerEdges.Left) > 0 && (sizingEdge & DesignerEdges.Bottom) > 0))
            {
                cursorToSet = Cursors.SizeNESW;
            }
            else if ((sizingEdge & DesignerEdges.Top) > 0 || (sizingEdge & DesignerEdges.Bottom) > 0)
            {
                cursorToSet = Cursors.SizeNS;
            }
            else if ((sizingEdge & DesignerEdges.Left) > 0 || (sizingEdge & DesignerEdges.Right) > 0)
            {
                cursorToSet = Cursors.SizeWE;
            }
            else if (sizingEdge == DesignerEdges.None &&
                 (workflowView.Cursor == Cursors.SizeNWSE || workflowView.Cursor == Cursors.SizeNESW ||
                 workflowView.Cursor == Cursors.SizeNS || workflowView.Cursor == Cursors.SizeWE))
            {
                cursorToSet = Cursors.Default;
            }

            if (workflowView.Cursor != cursorToSet)
                workflowView.Cursor = cursorToSet;
        }

        private ActivityDesigner GetDesignerToResize(Point point, out DesignerEdges sizingEdge)
        {
            ActivityDesigner designerToResize = null;
            sizingEdge = DesignerEdges.None;

            ISelectionService selectionService = GetService(typeof(ISelectionService)) as ISelectionService;
            if (selectionService != null)
            {
                ArrayList selectedComponents = new ArrayList(selectionService.GetSelectedComponents());
                for (int i = 0; i < selectedComponents.Count && designerToResize == null; i++)
                {
                    Activity activity = selectedComponents[i] as Activity;
                    if (activity != null)
                    {
                        ActivityDesigner potentialResizableDesigner = ActivityDesigner.GetDesigner(activity);
                        if (potentialResizableDesigner != null)
                        {
                            SelectionGlyph selectionGlyph = potentialResizableDesigner.Glyphs[typeof(SelectionGlyph)] as SelectionGlyph;
                            if (selectionGlyph != null)
                            {
                                foreach (Rectangle grabHandle in selectionGlyph.GetGrabHandles(potentialResizableDesigner))
                                {
                                    if (grabHandle.Contains(point))
                                    {
                                        designerToResize = potentialResizableDesigner;
                                        sizingEdge = GetSizingEdge(designerToResize, point);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return designerToResize;
        }

        private DesignerEdges GetSizingEdge(ActivityDesigner designer, Point point)
        {
            DesignerEdges sizingEdge = DesignerEdges.None;

            Size selectionSize = WorkflowTheme.CurrentTheme.AmbientTheme.SelectionSize;
            Rectangle designerBounds = designer.Bounds;
            if (Math.Floor(DesignerGeometryHelper.DistanceFromPointToLineSegment(point, new Point[] { new Point(designerBounds.Left, designerBounds.Top), new Point(designerBounds.Left, designerBounds.Bottom) })) <= selectionSize.Width + 1)
                sizingEdge |= DesignerEdges.Left;
            if (Math.Floor(DesignerGeometryHelper.DistanceFromPointToLineSegment(point, new Point[] { new Point(designerBounds.Left, designerBounds.Top), new Point(designerBounds.Right, designerBounds.Top) })) <= selectionSize.Height + 1)
                sizingEdge |= DesignerEdges.Top;
            if (Math.Floor(DesignerGeometryHelper.DistanceFromPointToLineSegment(point, new Point[] { new Point(designerBounds.Right, designerBounds.Top), new Point(designerBounds.Right, designerBounds.Bottom) })) <= selectionSize.Width + 1)
                sizingEdge |= DesignerEdges.Right;
            if (Math.Floor(DesignerGeometryHelper.DistanceFromPointToLineSegment(point, new Point[] { new Point(designerBounds.Left, designerBounds.Bottom), new Point(designerBounds.Right, designerBounds.Bottom) })) <= selectionSize.Height + 1)
                sizingEdge |= DesignerEdges.Bottom;

            return sizingEdge;
        }

        private bool CanResizeDesigner(ActivityDesigner designer)
        {
            if (!designer.EnableVisualResizing)
                return false;

            if (designer.ParentDesigner != null)
            {
                FreeformActivityDesigner freeFormDesigner = designer.ParentDesigner as FreeformActivityDesigner;
                if (freeFormDesigner != null)
                    return freeFormDesigner.CanResizeContainedDesigner(designer);
                else
                    return false;
            }

            return true;
        }

        private void SetResizableDesigner(ActivityDesigner designer, DesignerEdges sizingEdge)
        {
            if (this.designerToResize == designer)
                return;

            if (designer != null && !CanResizeDesigner(designer))
            {
                Debug.Assert(false);
                return;
            }

            WorkflowView workflowView = ParentView;
            if (workflowView == null)
                throw new InvalidOperationException(DR.GetString(DR.WorkflowViewNull));

            if (designer != null)
            {
                if (this.designerTransaction != null)
                    this.designerTransaction.Cancel();

                IDesignerHost designerHost = GetService(typeof(IDesignerHost)) as IDesignerHost;
                if (designerHost != null)
                    this.designerTransaction = designerHost.CreateTransaction(DR.GetString(DR.ResizeUndoDescription, designer.Text));

                ((IWorkflowDesignerMessageSink)designer).OnBeginResizing(sizingEdge);
            }
            else
            {
                if (this.designerTransaction != null)
                {
                    this.designerTransaction.Commit();
                    this.designerTransaction = null;
                }

                ((IWorkflowDesignerMessageSink)this.designerToResize).OnEndResizing();
            }

            this.designerToResize = designer;
            this.designerSizingEdge = sizingEdge;

            workflowView.Capture = (this.designerToResize != null);
            UpdateCursor(this.designerSizingEdge);
        }

        private void UpdateDesignerSize(Point point, ActivityDesigner designerToSize, DesignerEdges sizingEdge)
        {
            WorkflowView workflowView = ParentView;
            if (workflowView == null)
                throw new InvalidOperationException(DR.GetString(DR.WorkflowViewNull));

            Rectangle clipBounds = Rectangle.Empty;
            if (designerToSize.ParentDesigner != null)
            {
                clipBounds = designerToSize.ParentDesigner.Bounds;
                Size selectionSize = WorkflowTheme.CurrentTheme.AmbientTheme.SelectionSize;
                clipBounds.Inflate(-2 * selectionSize.Width, -2 * selectionSize.Height);
            }

            Rectangle designerBounds = designerToSize.Bounds;
            if ((sizingEdge & DesignerEdges.Left) > 0)
            {
                int x = point.X;
                if (!clipBounds.IsEmpty)
                    x = Math.Max(x, clipBounds.X);
                x = DesignerHelpers.SnapToGrid(new Point(x, 0)).X;

                designerBounds.Width += (designerBounds.Left - x);
                int delta = (designerBounds.Width < designerToSize.MinimumSize.Width) ? designerBounds.Width - designerToSize.MinimumSize.Width : 0;
                designerBounds.X = x + delta;
            }

            if ((sizingEdge & DesignerEdges.Top) > 0)
            {
                int y = point.Y;
                if (!clipBounds.IsEmpty)
                    y = Math.Max(y, clipBounds.Y);
                y = DesignerHelpers.SnapToGrid(new Point(0, y)).Y;

                designerBounds.Height += (designerBounds.Top - y);
                int delta = (designerBounds.Height < designerToSize.MinimumSize.Height) ? designerBounds.Height - designerToSize.MinimumSize.Height : 0;
                designerBounds.Y = y + delta;
            }

            if ((sizingEdge & DesignerEdges.Right) > 0)
                designerBounds.Width += (point.X - designerBounds.Right);

            if ((sizingEdge & DesignerEdges.Bottom) > 0)
                designerBounds.Height += (point.Y - designerBounds.Bottom);

            //Clip to lower bounds and upper bounds
            designerBounds.Width = Math.Max(designerBounds.Width, designerToSize.MinimumSize.Width);
            designerBounds.Height = Math.Max(designerBounds.Height, designerToSize.MinimumSize.Height);
            if (!clipBounds.IsEmpty)
                designerBounds = Rectangle.Intersect(designerBounds, clipBounds);

            ((IWorkflowDesignerMessageSink)designerToSize).OnResizing(sizingEdge, designerBounds);
        }
    }
}
