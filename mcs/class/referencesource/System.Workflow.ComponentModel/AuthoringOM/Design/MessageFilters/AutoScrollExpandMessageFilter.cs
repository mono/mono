namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    #region Class AutoScrollingMessageFilter
    //This behavior needs the coordinates in client coordinates
    internal sealed class AutoScrollingMessageFilter : WorkflowDesignerMessageFilter
    {
        #region Members and Constructor
        private enum ScrollDirection { None = 1, Left = 2, Up = 4, Right = 8, Down = 16 }
        private bool startAutoScroll = false;
        private EventHandler autoScrollEventHandler = null;
        private ScrollDirection autoScrollDirection = ScrollDirection.None;

        internal AutoScrollingMessageFilter()
        {
        }
        #endregion

        #region Behavior Overrides
        protected override bool OnDragEnter(DragEventArgs eventArgs)
        {
            //Invalidate the autoscrollindicator area
            this.startAutoScroll = true;

            Rectangle[] scrollIndicatorRectangles = ScrollIndicatorRectangles;
            foreach (Rectangle rectangle in scrollIndicatorRectangles)
                ParentView.InvalidateClientRectangle(rectangle);

            return false;
        }

        protected override bool OnDragOver(DragEventArgs eventArgs)
        {
            this.startAutoScroll = true;

            //We do not allow drag drop when we are auto scrolling
            //Also the drag image updation occures automatically as on AutoScroll we invalidate the whole client area
            //todo add quick and medium autoscroll zones
            Point clientPoint = ParentView.PointToClient(new Point(eventArgs.X, eventArgs.Y));
            AutoScrollDirection = AutoScrollDirectionFromPoint(clientPoint);
            if (AutoScrollDirection != ScrollDirection.None)
                return true;
            else
                return false;
        }

        protected override bool OnDragLeave()
        {
            this.startAutoScroll = false;
            AutoScrollDirection = ScrollDirection.None;
            return false;
        }

        protected override bool OnDragDrop(DragEventArgs eventArgs)
        {
            this.startAutoScroll = false;
            AutoScrollDirection = ScrollDirection.None;
            return false;
        }

        protected override bool OnPaintWorkflowAdornments(PaintEventArgs e, Rectangle viewPort, AmbientTheme ambientTheme)
        {
            if (ShowAutoScrollIndicators)
                DrawScrollIndicators(e.Graphics);
            return false;
        }
        #endregion

        #region Helpers
        private ScrollDirection AutoScrollDirection
        {
            get
            {
                return this.autoScrollDirection;
            }

            set
            {
                if (this.autoScrollDirection == value)
                    return;

                this.autoScrollDirection = value;

                Rectangle[] scrollIndicatorRectangles = ScrollIndicatorRectangles;
                foreach (Rectangle rectangle in scrollIndicatorRectangles)
                    ParentView.InvalidateClientRectangle(rectangle);

                if (ScrollDirection.None == value)
                {
                    if (this.autoScrollEventHandler != null)
                    {
                        WorkflowTimer.Default.Unsubscribe(this.autoScrollEventHandler);
                        this.autoScrollEventHandler = null;
                    }
                }
                else
                {
                    if (this.autoScrollEventHandler == null)
                    {
                        this.autoScrollEventHandler = new EventHandler(OnAutoScroll);
                        WorkflowTimer.Default.Subscribe(50, this.autoScrollEventHandler);
                    }
                }
            }
        }

        private ScrollDirection AutoScrollDirectionFromPoint(Point clientPoint)
        {
            Rectangle clientRectangle = new Rectangle(Point.Empty, ParentView.ViewPortSize);
            if (!clientRectangle.Contains(clientPoint))
                return ScrollDirection.None;

            ScrollDirection autoScrollDirection = ScrollDirection.None;

            ScrollBar hScrollBar = ParentView.HScrollBar;
            if (clientPoint.X <= clientRectangle.Width / 10 && hScrollBar.Value > 0)
                autoScrollDirection |= ScrollDirection.Left;
            else if (clientPoint.X >= clientRectangle.Right - clientRectangle.Width / 10 && hScrollBar.Value < hScrollBar.Maximum - hScrollBar.LargeChange)
                autoScrollDirection |= ScrollDirection.Right;

            ScrollBar vScrollBar = ParentView.VScrollBar;
            if (clientPoint.Y <= clientRectangle.Height / 10 && vScrollBar.Value > 0)
                autoScrollDirection |= ScrollDirection.Up;
            else if (clientPoint.Y >= clientRectangle.Bottom - clientRectangle.Height / 10 && vScrollBar.Value < vScrollBar.Maximum - vScrollBar.LargeChange)
                autoScrollDirection |= ScrollDirection.Down;

            return autoScrollDirection;
        }

        private bool ShowAutoScrollIndicators
        {
            get
            {
                AmbientTheme ambientTheme = WorkflowTheme.CurrentTheme.AmbientTheme;
                if (!this.startAutoScroll)
                    return false;

                Size viewPortSize = ParentView.ViewPortSize;
                Size indicatorSize = ambientTheme.ScrollIndicatorSize;
                indicatorSize.Width += 2 * ambientTheme.Margin.Width;
                indicatorSize.Height += 2 * ambientTheme.Margin.Height;
                return (viewPortSize.Width > 2 * indicatorSize.Width && viewPortSize.Height > 2 * indicatorSize.Height);
            }
        }

        private Rectangle[] ScrollIndicatorRectangles
        {
            get
            {
                Rectangle clientRectangle = new Rectangle(Point.Empty, ParentView.ViewPortSize);
                Size indicatorMargins = WorkflowTheme.CurrentTheme.AmbientTheme.Margin;
                Size scrollIndicatorSize = WorkflowTheme.CurrentTheme.AmbientTheme.ScrollIndicatorSize;

                Rectangle[] scrollIndicatorRectangles = new Rectangle[4];

                //Left indicator
                scrollIndicatorRectangles[0].X = indicatorMargins.Width;
                scrollIndicatorRectangles[0].Y = (clientRectangle.Height - scrollIndicatorSize.Height) / 2;
                scrollIndicatorRectangles[0].Size = scrollIndicatorSize;

                //Right indicator
                scrollIndicatorRectangles[1].X = clientRectangle.Right - indicatorMargins.Width - scrollIndicatorSize.Width;
                scrollIndicatorRectangles[1].Y = (clientRectangle.Height - scrollIndicatorSize.Height) / 2;
                scrollIndicatorRectangles[1].Size = scrollIndicatorSize;

                //Top indicator
                scrollIndicatorRectangles[2].X = (clientRectangle.Width - scrollIndicatorSize.Width) / 2;
                scrollIndicatorRectangles[2].Y = indicatorMargins.Height;
                scrollIndicatorRectangles[2].Size = scrollIndicatorSize;

                //Bottom indicator
                scrollIndicatorRectangles[3].X = (clientRectangle.Width - scrollIndicatorSize.Width) / 2;
                scrollIndicatorRectangles[3].Y = clientRectangle.Bottom - indicatorMargins.Height - scrollIndicatorSize.Height;
                scrollIndicatorRectangles[3].Size = scrollIndicatorSize;

                return scrollIndicatorRectangles;
            }
        }

        private void OnAutoScroll(object sender, EventArgs eventArgs)
        {
            WorkflowView parentView = ParentView;
            Point scrollPosition = parentView.ScrollPosition;

            if ((this.autoScrollDirection & ScrollDirection.Left) > 0)
                scrollPosition.X = scrollPosition.X - AmbientTheme.ScrollUnit;
            else if ((this.autoScrollDirection & ScrollDirection.Right) > 0)
                scrollPosition.X = scrollPosition.X + AmbientTheme.ScrollUnit;

            if ((this.autoScrollDirection & ScrollDirection.Up) > 0)
                scrollPosition.Y = scrollPosition.Y - AmbientTheme.ScrollUnit;
            else if ((this.autoScrollDirection & ScrollDirection.Down) > 0)
                scrollPosition.Y = scrollPosition.Y + AmbientTheme.ScrollUnit;

            parentView.ScrollPosition = scrollPosition;
        }

        private void DrawScrollIndicators(Graphics graphics)
        {
            Image indicator = AmbientTheme.ScrollIndicatorImage;
            if (indicator == null)
                return;

            WorkflowView parentView = ParentView;
            Size viewPortSize = parentView.ViewPortSize;
            Point scrollPosition = parentView.ScrollPosition;

            //Left Right
            Rectangle[] scrollIndicatorRectangles = ScrollIndicatorRectangles;
            if (scrollPosition.X > 0)
                ActivityDesignerPaint.DrawImage(graphics, AmbientTheme.ScrollIndicatorImage, scrollIndicatorRectangles[0], AmbientTheme.ScrollIndicatorTransparency);

            if (scrollPosition.X < parentView.HScrollBar.Maximum - viewPortSize.Width)
            {
                indicator.RotateFlip(RotateFlipType.Rotate180FlipY);
                ActivityDesignerPaint.DrawImage(graphics, indicator, scrollIndicatorRectangles[1], AmbientTheme.ScrollIndicatorTransparency);
                indicator.RotateFlip(RotateFlipType.Rotate180FlipY);
            }

            //Up Down
            if (scrollPosition.Y > 0)
            {
                indicator.RotateFlip(RotateFlipType.Rotate90FlipX);
                ActivityDesignerPaint.DrawImage(graphics, indicator, scrollIndicatorRectangles[2], AmbientTheme.ScrollIndicatorTransparency);
                indicator.RotateFlip(RotateFlipType.Rotate270FlipY);
            }

            if (scrollPosition.Y < parentView.VScrollBar.Maximum - viewPortSize.Height)
            {
                indicator.RotateFlip(RotateFlipType.Rotate270FlipNone);
                ActivityDesignerPaint.DrawImage(graphics, indicator, scrollIndicatorRectangles[3], AmbientTheme.ScrollIndicatorTransparency);
                indicator.RotateFlip(RotateFlipType.Rotate90FlipNone);
            }
        }
        #endregion
    }
    #endregion

    #region Class AutoExpandingMessageFilter
    //This behavior needs coordinates in logical coordinate system
    internal sealed class AutoExpandingMessageFilter : WorkflowDesignerMessageFilter
    {
        #region Members and Constructor
        private CompositeActivityDesigner autoExpandableDesigner = null;
        private EventHandler autoExpandEventHandler = null;

        internal AutoExpandingMessageFilter()
        {
        }
        #endregion

        #region Behavior Overrides
        protected override bool OnDragEnter(DragEventArgs eventArgs)
        {
            WorkflowView parentView = ParentView;
            if (parentView.IsClientPointInActiveLayout(parentView.PointToClient(new Point(eventArgs.X, eventArgs.Y))))
                SetAutoExpandableDesigner(parentView.MessageHitTestContext.AssociatedDesigner as CompositeActivityDesigner);
            else
                SetAutoExpandableDesigner(null);

            return false;
        }

        protected override bool OnDragOver(DragEventArgs eventArgs)
        {
            WorkflowView parentView = ParentView;
            if (parentView.IsClientPointInActiveLayout(parentView.PointToClient(new Point(eventArgs.X, eventArgs.Y))))
                SetAutoExpandableDesigner(parentView.MessageHitTestContext.AssociatedDesigner as CompositeActivityDesigner);
            else
                SetAutoExpandableDesigner(null);

            return false;
        }

        protected override bool OnDragDrop(DragEventArgs eventArgs)
        {
            SetAutoExpandableDesigner(null);
            return false;
        }

        protected override bool OnDragLeave()
        {
            SetAutoExpandableDesigner(null);
            return false;
        }
        #endregion

        #region Helpers
        private void SetAutoExpandableDesigner(CompositeActivityDesigner value)
        {
            if (this.autoExpandableDesigner == value)
                return;

            //We always remove this so that the timer counter gets reset
            if (value == null || value.Expanded || !value.CanExpandCollapse)
            {
                this.autoExpandableDesigner = null;
                if (this.autoExpandEventHandler != null)
                {
                    WorkflowTimer.Default.Unsubscribe(this.autoExpandEventHandler);
                    this.autoExpandEventHandler = null;
                }
            }
            else
            {
                //User has to hover for 2 seconds
                this.autoExpandableDesigner = value;
                if (this.autoExpandEventHandler == null)
                {
                    this.autoExpandEventHandler = new EventHandler(OnAutoExpand);
                    WorkflowTimer.Default.Subscribe(500, this.autoExpandEventHandler);
                }
            }
        }

        private void OnAutoExpand(object sender, EventArgs eventArgs)
        {
            if (this.autoExpandableDesigner != null)
            {
                this.autoExpandableDesigner.Expanded = true;
                ParentView.PerformLayout(true);
            }
            SetAutoExpandableDesigner(null);
        }
        #endregion
    }
    #endregion
}
