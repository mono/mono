namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Drawing;
    using System.Diagnostics;
    using System.Collections;
    using System.Windows.Forms;
    using System.ComponentModel.Design;

    #region Class DragRectangleMessageFilter
    /// This behavior requires the coordinates in logical coordinate system
    internal class DragRectangleMessageFilter : WorkflowDesignerMessageFilter
    {
        #region Members and Constructor
        private Point startDrag = Point.Empty;
        private Point endDrag = Point.Empty;
        private bool dragStarted = false;
        internal event EventHandler DragComplete;
        private Cursor previousCursor = Cursors.Default;

        internal DragRectangleMessageFilter()
        {
        }
        #endregion

        #region MessageFilter Overrides
        protected override void Dispose(bool disposing)
        {
            try
            {
                DragStarted = false;
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        protected override bool OnMouseCaptureChanged()
        {
            DragStarted = false;
            return false;
        }

        protected override bool OnMouseDown(MouseEventArgs eventArgs)
        {
            Debug.Assert(this.dragStarted == false);
            if (eventArgs.Button == MouseButtons.Left)
                this.startDrag = this.endDrag = ParentView.ClientPointToLogical(new Point(eventArgs.X, eventArgs.Y));

            return false;
        }

        protected override bool OnMouseMove(MouseEventArgs eventArgs)
        {
            WorkflowView parentView = ParentView;
            Point logicalPoint = parentView.ClientPointToLogical(new Point(eventArgs.X, eventArgs.Y));

            //We do nothing if the logical coordinate is not in the active layout
            //



            float dragMultiply = 2.0f / (float)parentView.Zoom * 100.0f;
            if (!this.dragStarted && (eventArgs.Button & MouseButtons.Left) > 0 &&
                (Math.Abs(this.startDrag.X - logicalPoint.X) > (int)(dragMultiply * (float)SystemInformation.DragSize.Width) || Math.Abs(this.startDrag.Y - logicalPoint.Y) > (int)(dragMultiply * (float)SystemInformation.DragSize.Height)))
                DragStarted = true;

            if (this.dragStarted)
            {
                if (!DragRectangle.IsEmpty)
                    parentView.InvalidateLogicalRectangle(DragRectangle);

                this.endDrag = logicalPoint;

                if (!DragRectangle.IsEmpty)
                    parentView.InvalidateLogicalRectangle(DragRectangle);
            }

            return this.dragStarted;
        }

        protected override bool OnMouseUp(MouseEventArgs eventArgs)
        {
            if (this.dragStarted)
            {
                WorkflowView parentView = ParentView;

                //Invalidate the old rectangle so that we state the drag is complete
                if (!DragRectangle.IsEmpty)
                    parentView.InvalidateLogicalRectangle(DragRectangle);

                //End the actual drag/drop
                this.endDrag = parentView.ClientPointToLogical(new Point(eventArgs.X, eventArgs.Y));
                DragStarted = false;

                //Send the notification of successful drag
                if (this.DragComplete != null)
                    this.DragComplete(this, EventArgs.Empty);

                return true;
            }

            return false;
        }

        protected override bool OnKeyDown(KeyEventArgs eventArgs)
        {
            if (eventArgs.KeyValue == (int)Keys.Escape)
                DragStarted = false;
            return false;
        }

        protected override bool OnPaint(PaintEventArgs e, Rectangle viewPort, AmbientTheme ambientTheme)
        {
            if (this.dragStarted)
            {
                using (Brush dragRectangleBrush = new SolidBrush(Color.FromArgb(10, ambientTheme.SelectionForeColor)))
                {
                    Rectangle dragRectangle = DragRectangle;
                    e.Graphics.FillRectangle(dragRectangleBrush, dragRectangle.X, dragRectangle.Y, dragRectangle.Width - 1, dragRectangle.Height - 1);
                    e.Graphics.DrawRectangle(ambientTheme.SelectionForegroundPen, dragRectangle.X, dragRectangle.Y, dragRectangle.Width - 1, dragRectangle.Height - 1);
                }
            }
            return false;
        }
        #endregion

        #region Helpers
        internal Rectangle DragRectangle
        {
            get
            {
                return new Rectangle(Math.Min(this.startDrag.X, this.endDrag.X), Math.Min(this.startDrag.Y, this.endDrag.Y), Math.Abs(this.endDrag.X - this.startDrag.X), Math.Abs(this.endDrag.Y - this.startDrag.Y));
            }
        }

        protected bool DragStarted
        {
            get
            {
                return this.dragStarted;
            }

            set
            {
                if (this.dragStarted != value)
                {
                    WorkflowView parentView = ParentView;
                    if (value)
                    {
                        this.dragStarted = true;
                        this.previousCursor = parentView.Cursor;
                        parentView.Cursor = Cursors.Cross;
                        parentView.Capture = true;
                    }
                    else
                    {
                        parentView.Capture = false;
                        this.dragStarted = false;
                        if (this.previousCursor != null)
                            parentView.Cursor = this.previousCursor;

                        if (!DragRectangle.IsEmpty)
                            parentView.InvalidateLogicalRectangle(DragRectangle);
                    }
                }
            }
        }
        #endregion
    }
    #endregion

    #region Class DragSelectionMessageFilter
    //This behavior needs logical coordinates
    internal sealed class DragSelectionMessageFilter : DragRectangleMessageFilter
    {
        #region Members and Constructor
        internal DragSelectionMessageFilter()
        {
        }
        #endregion

        #region MessageFilter Overrides
        protected override bool OnMouseDown(MouseEventArgs eventArgs)
        {
            if ((Control.ModifierKeys & Keys.Shift) > 0)
            {
                base.OnMouseDown(eventArgs);
                return true;
            }
            return false;
        }

        protected override bool OnMouseMove(MouseEventArgs eventArgs)
        {
            if ((Control.ModifierKeys & Keys.Shift) > 0)
            {
                base.OnMouseMove(eventArgs);
                return true;
            }
            return false;
        }

        protected override bool OnMouseUp(MouseEventArgs eventArgs)
        {
            if ((Control.ModifierKeys & Keys.Shift) > 0)
            {
                base.OnMouseUp(eventArgs);

                //Select all the shapes
                WorkflowView parentView = ParentView;
                if (!DragRectangle.IsEmpty && parentView.RootDesigner != null)
                {
                    ActivityDesigner[] intersectingDesigners = CompositeActivityDesigner.GetIntersectingDesigners(parentView.RootDesigner, DragRectangle);
                    ArrayList selectableComponents = new ArrayList();
                    foreach (ActivityDesigner activityDesigner in intersectingDesigners)
                        selectableComponents.Add(activityDesigner.Activity);

                    ISelectionService selectionService = GetService(typeof(ISelectionService)) as ISelectionService;
                    if (selectableComponents.Count > 0 && selectionService != null)
                        selectionService.SetSelectedComponents((object[])selectableComponents.ToArray(typeof(object)), SelectionTypes.Replace);
                }

                return true;
            }
            return false;
        }

        protected override bool OnKeyUp(KeyEventArgs eventArgs)
        {
            base.OnKeyUp(eventArgs);
            if ((Control.ModifierKeys & Keys.Shift) == 0)
                DragStarted = false;

            return false;
        }
        #endregion
    }
    #endregion
}
