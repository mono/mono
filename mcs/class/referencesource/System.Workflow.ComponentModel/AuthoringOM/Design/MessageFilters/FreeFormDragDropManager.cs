namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Text;
    using System.Drawing;
    using System.Diagnostics;
    using System.Collections;
    using System.Windows.Forms;
    using System.ComponentModel;
    using System.Drawing.Drawing2D;
    using System.Collections.Generic;
    using System.ComponentModel.Design;

    #region Class FreeFormDragDropManager
    internal sealed class FreeFormDragDropManager : DragDropManager
    {
        #region Members and Constructor
        private static Cursor DragMoveCursor = new Cursor(typeof(WorkflowView), "Resources.DragMoveCursor.cur");
        private static Cursor DragCopyCursor = new Cursor(typeof(WorkflowView), "Resources.DragCopyCursor.cur");
        private static Cursor MoveCursor = new Cursor(typeof(WorkflowView), "Resources.MoveCursor.cur");

        private List<Image> draggedDesignerImages = null;
        private Cursor previousCursor = Cursors.Default;
        private Point movedDesignerImagePoint = Point.Empty;

        public FreeFormDragDropManager()
        {
        }
        #endregion

        #region Behavior Overrides
        protected override bool OnDragEnter(DragEventArgs eventArgs)
        {
            bool retVal = base.OnDragEnter(eventArgs);
            if (this.draggedDesignerImages == null)
            {
                WorkflowView parentView = ParentView;
                Point clientPoint = parentView.PointToClient(new Point(eventArgs.X, eventArgs.Y));
                Point logicalPoint = parentView.ScreenPointToLogical(new Point(eventArgs.X, eventArgs.Y));
                if (parentView.IsClientPointInActiveLayout(clientPoint))
                    this.movedDesignerImagePoint = logicalPoint;
                else
                    this.movedDesignerImagePoint = DragInitiationPoint;
            }

            return retVal;
        }

        protected override bool OnDragOver(DragEventArgs eventArgs)
        {
            //Invalidate the dragged images
            if (this.draggedDesignerImages != null)
            {
                Point[] previousLocations = GetDesignerLocations(DragInitiationPoint, this.movedDesignerImagePoint, DraggedActivities);
                InvalidateDraggedImages(previousLocations);
            }

            bool retVal = base.OnDragOver(eventArgs);

            if (this.draggedDesignerImages != null)
            {
                WorkflowView parentView = ParentView;
                Point clientPoint = parentView.PointToClient(new Point(eventArgs.X, eventArgs.Y));
                Point logicalPoint = parentView.ScreenPointToLogical(new Point(eventArgs.X, eventArgs.Y));

                if (parentView.IsClientPointInActiveLayout(clientPoint))
                    this.movedDesignerImagePoint = logicalPoint;
                else
                    this.movedDesignerImagePoint = DragInitiationPoint;

                //Invalidate the new locations where the image is shown
                Point[] newLocations = GetDesignerLocations(DragInitiationPoint, this.movedDesignerImagePoint, DraggedActivities);
                InvalidateDraggedImages(newLocations);
            }
            return retVal;
        }

        protected override bool OnGiveFeedback(GiveFeedbackEventArgs gfbevent)
        {
            base.OnGiveFeedback(gfbevent);

            if (this.draggedDesignerImages != null)
            {
                gfbevent.UseDefaultCursors = false;
                if ((gfbevent.Effect & DragDropEffects.Move) == DragDropEffects.Move)
                    Cursor.Current = FreeFormDragDropManager.DragMoveCursor;
                else if ((gfbevent.Effect & DragDropEffects.Copy) == DragDropEffects.Copy)
                    Cursor.Current = FreeFormDragDropManager.DragCopyCursor;
                else
                    Cursor.Current = Cursors.No;

                return true;
            }

            return false;
        }

        protected override bool OnMouseMove(MouseEventArgs eventArgs)
        {
            bool retval = base.OnMouseMove(eventArgs);
            if (eventArgs.Button == MouseButtons.None)
            {
                bool showMoveCursor = false;
                showMoveCursor |= (MessageHitTestContext != null && MessageHitTestContext.AssociatedDesigner != null && ActivityDesigner.GetParentDesigner(MessageHitTestContext.AssociatedDesigner.Activity) is FreeformActivityDesigner && (MessageHitTestContext.HitLocation & HitTestLocations.ActionArea) == 0);
                UpdateCursor(showMoveCursor);
            }
            return retval;
        }

        protected override bool OnMouseLeave()
        {
            UpdateCursor(false);
            return false;
        }

        protected override bool OnScroll(ScrollBar sender, int value)
        {
            if (this.draggedDesignerImages != null)
            {
                Point[] previousLocations = GetDesignerLocations(DragInitiationPoint, this.movedDesignerImagePoint, DraggedActivities);
                InvalidateDraggedImages(previousLocations);
            }

            bool retVal = base.OnScroll(sender, value);

            if (this.draggedDesignerImages != null)
            {
                WorkflowView parentView = ParentView;
                Point clientPoint = parentView.PointToClient(Control.MousePosition);
                Point logicalPoint = parentView.ScreenPointToLogical(Control.MousePosition);

                if (parentView.IsClientPointInActiveLayout(clientPoint))
                    this.movedDesignerImagePoint = logicalPoint;
                else
                    this.movedDesignerImagePoint = DragInitiationPoint;

                //Invalidate the new locations where the image is shown
                Point[] newLocations = GetDesignerLocations(DragInitiationPoint, this.movedDesignerImagePoint, DraggedActivities);
                InvalidateDraggedImages(newLocations);
            }

            return retVal;
        }

        protected override bool OnPaintWorkflowAdornments(PaintEventArgs eventArgs, Rectangle viewPort, AmbientTheme ambientTheme)
        {
            bool messageHandled = false;
            if (this.draggedDesignerImages == null || this.draggedDesignerImages.Count == 0 || !(DropTargetDesigner is FreeformActivityDesigner))
                messageHandled = base.OnPaintWorkflowAdornments(eventArgs, viewPort, ambientTheme);
            return messageHandled;
        }

        protected override bool OnPaint(PaintEventArgs eventArgs, Rectangle viewPort, AmbientTheme ambientTheme)
        {
            bool messageHandled = false;
            if (this.draggedDesignerImages != null && DropTargetDesigner is FreeformActivityDesigner)
            {
                using (Region clipRegion = new Region(ActivityDesignerPaint.GetDesignerPath(ParentView.RootDesigner, false)))
                {
                    Region oldRegion = eventArgs.Graphics.Clip;
                    eventArgs.Graphics.Clip = clipRegion;

                    Point[] locations = GetDesignerLocations(DragInitiationPoint, this.movedDesignerImagePoint, DraggedActivities);
                    Debug.Assert(locations.Length == DraggedActivities.Count);
                    Debug.Assert(this.draggedDesignerImages.Count == DraggedActivities.Count);

                    for (int i = 0; i < this.draggedDesignerImages.Count; i++)
                    {
                        Size imageSize = this.draggedDesignerImages[i].Size;
                        ActivityDesignerPaint.DrawImage(eventArgs.Graphics, this.draggedDesignerImages[i], new Rectangle(new Point(locations[i].X - 2 * ambientTheme.Margin.Width, locations[i].Y - 2 * ambientTheme.Margin.Height), imageSize), new Rectangle(Point.Empty, imageSize), DesignerContentAlignment.Fill, 0.4f, false);
                    }

                    eventArgs.Graphics.Clip = oldRegion;
                }
            }
            else
            {
                messageHandled = base.OnPaint(eventArgs, viewPort, ambientTheme);
            }

            return messageHandled;
        }

        protected override void CreateDragFeedbackImages(IList<Activity> draggedActivities)
        {
            base.CreateDragFeedbackImages(draggedActivities);

            List<Image> imageList = new List<Image>();
            using (Graphics graphics = ParentView.CreateGraphics())
            {
                foreach (Activity activity in draggedActivities)
                {
                    ActivityDesigner previewDesigner = ActivityDesigner.GetDesigner(activity);
                    if (previewDesigner == null)
                        previewDesigner = ActivityDesigner.CreateDesigner(ParentView, activity);
                    imageList.Add(previewDesigner.GetPreviewImage(graphics));
                }
            }

            //We create the designer images for designers associates with existing activities
            ParentView.InvalidateClientRectangle(Rectangle.Empty);
            this.draggedDesignerImages = imageList;
        }

        protected override void DestroyDragFeedbackImages()
        {
            base.DestroyDragFeedbackImages();

            if (this.draggedDesignerImages != null)
            {
                foreach (Bitmap image in this.draggedDesignerImages)
                    image.Dispose();
                this.draggedDesignerImages = null;
                ParentView.InvalidateClientRectangle(Rectangle.Empty);
            }
        }
        #endregion

        #region Helpers
        private void InvalidateDraggedImages(Point[] locations)
        {
            if (this.draggedDesignerImages != null)
            {
                if (locations.Length == this.draggedDesignerImages.Count)
                {
                    AmbientTheme ambientTheme = WorkflowTheme.CurrentTheme.AmbientTheme;
                    WorkflowView parentView = ParentView;

                    //Invalidate the previous location where image was shown
                    for (int i = 0; i < this.draggedDesignerImages.Count; i++)
                    {
                        Rectangle rectangle = new Rectangle(locations[i], this.draggedDesignerImages[i].Size);
                        rectangle.Inflate(2 * ambientTheme.Margin.Width, 2 * ambientTheme.Margin.Height);
                        parentView.InvalidateLogicalRectangle(rectangle);
                    }
                }
                else
                {
                    Debug.Assert(false);
                }
            }
        }

        internal static Point[] GetDesignerLocations(Point startPoint, Point endPoint, ICollection<Activity> activitiesToMove)
        {
            List<Point> locations = new List<Point>();

            //We move all the designers 
            foreach (Activity activityToMove in activitiesToMove)
            {
                Point location = endPoint;
                ActivityDesigner designerToMove = ActivityDesigner.GetDesigner(activityToMove);
                if (designerToMove != null && !startPoint.IsEmpty)
                {
                    Size delta = new Size(endPoint.X - startPoint.X, endPoint.Y - startPoint.Y);
                    location = new Point(designerToMove.Location.X + delta.Width, designerToMove.Location.Y + delta.Height);
                }

                location = DesignerHelpers.SnapToGrid(location);
                locations.Add(location);
            }

            return locations.ToArray();
        }

        private void UpdateCursor(bool showMoveCursor)
        {
            if (showMoveCursor)
            {
                if (ParentView.Cursor != FreeFormDragDropManager.MoveCursor && ParentView.Cursor == Cursors.Default)
                {
                    this.previousCursor = ParentView.Cursor;
                    ParentView.Cursor = FreeFormDragDropManager.MoveCursor;
                }
            }
            else
            {
                ParentView.Cursor = this.previousCursor;
            }
        }
        #endregion
    }
    #endregion
}
