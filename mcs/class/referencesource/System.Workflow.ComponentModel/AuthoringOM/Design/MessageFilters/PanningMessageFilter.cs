namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;
    using System.ComponentModel.Design;

    #region Class PanningMessageFilter
    /// This behavior needs and stores coordinates in client coordinates
    internal sealed class PanningMessageFilter : WorkflowDesignerMessageFilter
    {
        #region Members and Constructor
        private static Cursor PanBeganCursor = new Cursor(typeof(WorkflowView), "Resources.panClosed.cur");
        private static Cursor PanReadyCursor = new Cursor(typeof(WorkflowView), "Resources.panOpened.cur");

        private Point panPoint = Point.Empty;
        private bool panningActive = false;

        private CommandID previousCommand;
        private Cursor previousCursor = Cursors.Default;

        internal PanningMessageFilter()
        {
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
                menuCommandService.ShowContextMenu(WorkflowMenuCommands.ZoomMenu, menuPoint.X, menuPoint.Y);

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
                SetPanPoint(new Point(eventArgs.X, eventArgs.Y));
            return true;
        }

        protected override bool OnMouseMove(MouseEventArgs eventArgs)
        {
            if (this.panningActive && (eventArgs.Button & MouseButtons.Left) > 0)
            {
                Size panSize = new Size(eventArgs.X - this.panPoint.X, eventArgs.Y - this.panPoint.Y);
                WorkflowView parentView = ParentView;
                parentView.ScrollPosition = new Point(parentView.ScrollPosition.X - panSize.Width, parentView.ScrollPosition.Y - panSize.Height);
                SetPanPoint(new Point(eventArgs.X, eventArgs.Y));
            }

            return true;
        }

        protected override bool OnMouseUp(MouseEventArgs eventArgs)
        {
            SetPanPoint(Point.Empty);
            return true;
        }

        protected override bool OnDragEnter(DragEventArgs eventArgs)
        {
            ParentView.RemoveDesignerMessageFilter(this);
            return false;
        }

        protected override bool OnKeyDown(KeyEventArgs eventArgs)
        {
            if (eventArgs.KeyValue == (int)Keys.Escape)
                ParentView.RemoveDesignerMessageFilter(this);
            return true;
        }
        #endregion

        #region Helpers
        private void SetPanPoint(Point value)
        {
            this.panPoint = value;
            this.panningActive = (this.panPoint != Point.Empty);
            ParentView.Capture = this.panningActive;
            RefreshUIState();
        }

        private void RefreshUIState()
        {
            //Update the cursor
            ParentView.Cursor = (this.panningActive) ? PanningMessageFilter.PanBeganCursor : PanningMessageFilter.PanReadyCursor;

            //Update the menu command
            IMenuCommandService menuCommandService = GetService(typeof(IMenuCommandService)) as IMenuCommandService;
            if (menuCommandService != null)
            {
                CommandID[] affectedCommands = new CommandID[] { WorkflowMenuCommands.ZoomIn, WorkflowMenuCommands.ZoomOut, WorkflowMenuCommands.Pan, WorkflowMenuCommands.DefaultFilter };
                foreach (CommandID affectedCommand in affectedCommands)
                {
                    MenuCommand menuCommand = menuCommandService.FindCommand(affectedCommand);
                    if (menuCommand != null && menuCommand.Enabled)
                        menuCommand.Checked = (menuCommand.CommandID == WorkflowMenuCommands.Pan);
                }
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
        #endregion
    }
    #endregion
}
