namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Drawing;
    using System.Diagnostics;
    using System.Windows.Forms;
    using System.ComponentModel.Design;

    #region Class ZoomingMessageFilter
    /// This MessageFilter needs coordinates in client coordinate system
    internal sealed class ZoomingMessageFilter : WorkflowDesignerMessageFilter
    {
        #region Members and Constructor
        private static int ZoomIncrement = 20;
        private static Cursor ZoomInCursor = new Cursor(typeof(WorkflowView), "Resources.zoomin.cur");
        private static Cursor ZoomOutCursor = new Cursor(typeof(WorkflowView), "Resources.zoomout.cur");
        private static Cursor ZoomDisabledCursor = new Cursor(typeof(WorkflowView), "Resources.zoomno.cur");

        private enum ZoomState { In, Out }
        private ZoomState initialState = ZoomState.In; //which tool to show by default
        private ZoomState currentState = ZoomState.In; //current tool
        private DragRectangleMessageFilter fastZoomingMessageFilter = null;

        private CommandID previousCommand;
        private Cursor previousCursor = Cursors.Default;

        internal ZoomingMessageFilter(bool initiateZoomIn)
        {
            this.currentState = this.initialState = (initiateZoomIn) ? ZoomState.In : ZoomState.Out;
        }
        #endregion

        #region MessageFilter Overrides
        protected override void Initialize(WorkflowView parentView)
        {
            base.Initialize(parentView);

            StoreUIState();
            RefreshUIState();
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (this.fastZoomingMessageFilter != null)
                {
                    this.fastZoomingMessageFilter.DragComplete -= new EventHandler(OnZoomRectComplete);
                    ParentView.RemoveDesignerMessageFilter(this.fastZoomingMessageFilter);
                    this.fastZoomingMessageFilter.Dispose();
                    this.fastZoomingMessageFilter = null;
                }
                RestoreUIState();
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        protected override bool OnShowContextMenu(Point menuPoint)
        {
            IMenuCommandService menuCommandService = (IMenuCommandService)GetService(typeof(IMenuCommandService));
            if (menuCommandService != null)
            {
                menuCommandService.ShowContextMenu(WorkflowMenuCommands.ZoomMenu, menuPoint.X, menuPoint.Y);
                RefreshUIState();
            }
            return true;
        }

        protected override bool OnMouseEnter(MouseEventArgs eventArgs)
        {
            RefreshUIState();
            return true;
        }

        protected override bool OnMouseDown(MouseEventArgs eventArgs)
        {
            if (eventArgs.Button == MouseButtons.Left)
            {
                this.currentState = ((Control.ModifierKeys & Keys.Shift) != 0) ? ((this.initialState == ZoomState.In) ? ZoomState.Out : ZoomState.In) : this.initialState;

                bool forwardMessage = (this.fastZoomingMessageFilter == null);
                RefreshUIState();
                if (forwardMessage && this.fastZoomingMessageFilter != null)
                    ((IWorkflowDesignerMessageSink)this.fastZoomingMessageFilter).OnMouseDown(eventArgs);
            }
            return true;
        }

        protected override bool OnMouseMove(MouseEventArgs eventArgs)
        {
            //We do not allow other behaviors to handle this message
            return true;
        }

        protected override bool OnMouseDoubleClick(MouseEventArgs eventArgs)
        {
            return true;
        }

        protected override bool OnMouseLeave()
        {
            return true;
        }

        protected override bool OnMouseCaptureChanged()
        {
            return true;
        }

        protected override bool OnMouseUp(MouseEventArgs eventArgs)
        {
            if (eventArgs.Button == MouseButtons.Left && CanContinueZooming)
            {
                WorkflowView parentView = ParentView;
                int zoom = parentView.Zoom + ((this.currentState == ZoomState.In) ? ZoomingMessageFilter.ZoomIncrement : (-1 * ZoomingMessageFilter.ZoomIncrement));
                Point center = new Point((this.currentState == ZoomState.In) ? eventArgs.X : parentView.ViewPortSize.Width / 2, (this.currentState == ZoomState.In) ? eventArgs.Y : parentView.ViewPortSize.Height / 2);
                UpdateZoom(zoom, center);
            }

            return true;
        }

        protected override bool OnKeyDown(KeyEventArgs eventArgs)
        {
            if (eventArgs.KeyValue == (int)Keys.Escape)
            {
                ParentView.RemoveDesignerMessageFilter(this);
            }
            else
            {
                this.currentState = ((eventArgs.Modifiers & Keys.Shift) != 0) ? ((this.initialState == ZoomState.In) ? ZoomState.Out : ZoomState.In) : this.initialState;
                RefreshUIState();
            }
            return true;
        }

        protected override bool OnKeyUp(KeyEventArgs eventArgs)
        {
            this.currentState = ((eventArgs.Modifiers & Keys.Shift) != 0) ? ((this.initialState == ZoomState.In) ? ZoomState.Out : ZoomState.In) : this.initialState;
            RefreshUIState();
            return true;
        }

        protected override bool OnDragEnter(DragEventArgs eventArgs)
        {
            ParentView.RemoveDesignerMessageFilter(this);
            return false;
        }
        #endregion

        #region Helpers
        internal bool ZoomingIn
        {
            get
            {
                return (this.initialState == ZoomState.In);
            }
        }

        private void OnZoomRectComplete(object sender, EventArgs e)
        {
            Debug.Assert(this.currentState == ZoomState.In && CanContinueZooming && this.fastZoomingMessageFilter != null);
            if (CanContinueZooming && this.currentState == ZoomState.In && this.fastZoomingMessageFilter != null && !this.fastZoomingMessageFilter.DragRectangle.IsEmpty)
            {
                Rectangle dragRectangle = this.fastZoomingMessageFilter.DragRectangle;
                WorkflowView parentView = ParentView;
                Point center = parentView.LogicalPointToClient(new Point(dragRectangle.Location.X + dragRectangle.Width / 2, dragRectangle.Location.Y + dragRectangle.Height / 2));
                int zoom = (int)(Math.Min((float)parentView.ViewPortSize.Width / (float)dragRectangle.Width, (float)parentView.ViewPortSize.Height / (float)dragRectangle.Height) * 100.0f);
                UpdateZoom(zoom, center);
            }
        }

        private void UpdateZoom(int zoomLevel, Point center)
        {
            PointF relativeCenterF = PointF.Empty;
            WorkflowView parentView = ParentView;

            Point layoutOrigin = parentView.LogicalPointToClient(Point.Empty);
            center.X -= layoutOrigin.X; center.Y -= layoutOrigin.Y;
            relativeCenterF = new PointF((float)center.X / (float)parentView.HScrollBar.Maximum, (float)center.Y / (float)parentView.VScrollBar.Maximum);

            parentView.Zoom = Math.Min(Math.Max(zoomLevel, AmbientTheme.MinZoom), AmbientTheme.MaxZoom);

            Point newCenter = new Point((int)((float)parentView.HScrollBar.Maximum * relativeCenterF.X), (int)((float)parentView.VScrollBar.Maximum * relativeCenterF.Y));
            parentView.ScrollPosition = new Point(newCenter.X - parentView.HScrollBar.LargeChange / 2, newCenter.Y - parentView.VScrollBar.LargeChange / 2);

            this.currentState = ((Control.ModifierKeys & Keys.Shift) != 0) ? ((this.initialState == ZoomState.In) ? ZoomState.Out : ZoomState.In) : this.initialState;
            RefreshUIState();
        }

        private bool CanContinueZooming
        {
            get
            {
                WorkflowView parentView = ParentView;
                return ((this.currentState == ZoomState.Out && parentView.Zoom > AmbientTheme.MinZoom) || (this.currentState == ZoomState.In && parentView.Zoom < AmbientTheme.MaxZoom));
            }
        }

        private void StoreUIState()
        {
            IMenuCommandService menuCommandService = GetService(typeof(IMenuCommandService)) as IMenuCommandService;
            if (menuCommandService != null)
            {
                foreach (CommandID affectedCommand in CommandSet.NavigationToolCommandIds)
                {
                    MenuCommand menuCommand = menuCommandService.FindCommand(affectedCommand);
                    if (menuCommand != null && menuCommand.Enabled && menuCommand.Checked)
                    {
                        this.previousCommand = menuCommand.CommandID;
                        break;
                    }
                }
            }

            this.previousCursor = ParentView.Cursor;
        }

        private void RestoreUIState()
        {
            IMenuCommandService menuCommandService = GetService(typeof(IMenuCommandService)) as IMenuCommandService;
            if (menuCommandService != null)
            {
                foreach (CommandID affectedCommand in CommandSet.NavigationToolCommandIds)
                {
                    MenuCommand menuCommand = menuCommandService.FindCommand(affectedCommand);
                    if (menuCommand != null && menuCommand.Enabled)
                        menuCommand.Checked = (menuCommand.CommandID == this.previousCommand);
                }
            }

            ParentView.Cursor = this.previousCursor;
        }

        private void RefreshUIState()
        {
            //Update the cursor
            WorkflowView parentView = ParentView;
            if (!CanContinueZooming)
                parentView.Cursor = ZoomingMessageFilter.ZoomDisabledCursor;
            else if (this.currentState == ZoomState.In)
                parentView.Cursor = ZoomingMessageFilter.ZoomInCursor;
            else
                parentView.Cursor = ZoomingMessageFilter.ZoomOutCursor;

            //Update the fast zoom
            if (this.fastZoomingMessageFilter == null && CanContinueZooming && this.currentState == ZoomState.In)
            {
                this.fastZoomingMessageFilter = new DragRectangleMessageFilter();
                this.fastZoomingMessageFilter.DragComplete += new EventHandler(OnZoomRectComplete);
                parentView.AddDesignerMessageFilter(this.fastZoomingMessageFilter);
            }
            else if (this.fastZoomingMessageFilter != null && (!CanContinueZooming || this.currentState != ZoomState.In))
            {
                this.fastZoomingMessageFilter.DragComplete -= new EventHandler(OnZoomRectComplete);
                parentView.RemoveDesignerMessageFilter(this.fastZoomingMessageFilter);
                this.fastZoomingMessageFilter = null;
            }

            //Update the menu
            IMenuCommandService menuCommandService = GetService(typeof(IMenuCommandService)) as IMenuCommandService;
            if (menuCommandService != null)
            {
                foreach (CommandID affectedCommand in CommandSet.NavigationToolCommandIds)
                {
                    MenuCommand menuCommand = menuCommandService.FindCommand(affectedCommand);
                    if (menuCommand != null && menuCommand.Enabled)
                        menuCommand.Checked = (menuCommand.CommandID == ((this.initialState == ZoomState.In) ? WorkflowMenuCommands.ZoomIn : WorkflowMenuCommands.ZoomOut));
                }
            }
        }
        #endregion
    }
    #endregion
}
