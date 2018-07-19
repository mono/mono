namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Text;
    using System.Drawing;
    using System.Diagnostics;
    using System.Collections;
    using System.Windows.Forms;
    using System.Drawing.Drawing2D;
    using System.Collections.Generic;
    using System.ComponentModel.Design;
    using System.Collections.ObjectModel;
    using System.Workflow.ComponentModel.Design;

    #region Class ConnectionManager
    //This behavior deals in logical coordinates
    internal sealed class ConnectionManager : WorkflowDesignerMessageFilter, IDesignerGlyphProvider
    {
        #region Members and Constructor
        internal static Cursor SnappedConnectionCursor = new Cursor(typeof(WorkflowView), "Resources.Connector.cur");
        internal static Cursor NewConnectorCursor = new Cursor(typeof(WorkflowView), "Resources.ConnectorDraw.cur");

        private const int HighlightDistance = 20;
        private const int SnapHighlightDistance = 20;

        private ConnectionPoint[] connectablePoints;
        private ConnectionPoint snappedConnectionPoint;
        private ConnectorEditor connectorEditor;

        //Temporary variable indicating potential for drag drop
        private Nullable<Point> initialDragPoint = null;
        private HitTestInfo dragPointHitInfo = null;

        public ConnectionManager()
        {

        }

        protected override void Initialize(WorkflowView parentView)
        {
            base.Initialize(parentView);

            IServiceContainer serviceContainer = GetService(typeof(IServiceContainer)) as IServiceContainer;
            if (serviceContainer != null)
            {
                serviceContainer.RemoveService(typeof(ConnectionManager));
                serviceContainer.AddService(typeof(ConnectionManager), this);
            }

            IDesignerGlyphProviderService glyphProviderService = GetService(typeof(IDesignerGlyphProviderService)) as IDesignerGlyphProviderService;
            if (glyphProviderService != null)
                glyphProviderService.AddGlyphProvider(this);
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    IServiceContainer serviceContainer = GetService(typeof(IServiceContainer)) as IServiceContainer;
                    if (serviceContainer != null)
                        serviceContainer.RemoveService(typeof(ConnectionManager));

                    IDesignerGlyphProviderService glyphProviderService = GetService(typeof(IDesignerGlyphProviderService)) as IDesignerGlyphProviderService;
                    if (glyphProviderService != null)
                        glyphProviderService.RemoveGlyphProvider(this);
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }
        #endregion

        #region Behavior Overrides
        protected override bool OnMouseDown(MouseEventArgs eventArgs)
        {
            Point cursorPoint = Point.Empty;

            //Check if we can start editing
            if ((eventArgs.Button & MouseButtons.Left) == MouseButtons.Left)
            {
                WorkflowView workflowView = ParentView;
                Point clientPoint = new Point(eventArgs.X, eventArgs.Y);

                //If the point is not in clickable area then return
                if (workflowView != null && workflowView.IsClientPointInActiveLayout(clientPoint))
                {
                    Point logicalPoint = workflowView.ClientPointToLogical(clientPoint);
                    if (CanBeginEditing(logicalPoint, MessageHitTestContext))
                    {
                        this.initialDragPoint = new Point?(logicalPoint);
                        this.dragPointHitInfo = MessageHitTestContext;
                    }
                    cursorPoint = logicalPoint;
                }
            }
            else
            {
                EndEditing(null);
            }

            //Update the cursor 
            bool processedMessage = (this.initialDragPoint != null);
            processedMessage |= UpdateCursor(cursorPoint);
            return processedMessage;
        }

        protected override bool OnMouseMove(MouseEventArgs eventArgs)
        {
            Point cursorPoint = Point.Empty;
            WorkflowView workflowView = ParentView;
            Point clientPoint = new Point(eventArgs.X, eventArgs.Y);

            if (workflowView != null && workflowView.IsClientPointInActiveLayout(clientPoint))
            {
                Point logicalPoint = workflowView.ClientPointToLogical(clientPoint);

                if ((eventArgs.Button & MouseButtons.Left) == MouseButtons.Left)
                {
                    //Check if we can start editing a connector
                    if (!EditingInProgress && this.initialDragPoint != null &&
                        (Math.Abs(this.initialDragPoint.Value.X - logicalPoint.X) > SystemInformation.DragSize.Width ||
                        Math.Abs(this.initialDragPoint.Value.Y - logicalPoint.Y) > SystemInformation.DragSize.Height))
                    {
                        BeginEditing(GetConnectorEditor(this.initialDragPoint.Value, this.dragPointHitInfo), this.initialDragPoint.Value);
                    }

                    //If the editing is in progress then pump the messages to the edited connector 
                    if (EditingInProgress)
                    {
                        ContinueEditing(logicalPoint);
                        if (SnappedConnectionPoint != null)
                            logicalPoint = SnappedConnectionPoint.Location;
                    }
                }
                else
                {
                    //Show the points from where we can start drawing connectors
                    FreeformActivityDesigner connectorContainer = ConnectionManager.GetConnectorContainer(MessageHitTestContext.AssociatedDesigner);
                    ConnectablePoints = (connectorContainer != null && connectorContainer.EnableUserDrawnConnectors) ? GetHighlightableConnectionPoints(logicalPoint, MessageHitTestContext.AssociatedDesigner) : null;
                }

                cursorPoint = logicalPoint;
            }

            bool processedMessage = EditingInProgress;
            processedMessage |= UpdateCursor(cursorPoint);
            return processedMessage;
        }

        protected override bool OnMouseEnter(MouseEventArgs eventArgs)
        {
            Point cursorPoint = Point.Empty;
            Point clientPoint = new Point(eventArgs.X, eventArgs.Y);
            WorkflowView workflowView = ParentView;

            if (workflowView != null && workflowView.IsClientPointInActiveLayout(clientPoint) && !EditingInProgress)
            {
                //Highlight the connection points to indicate where user can start drawing connectors
                FreeformActivityDesigner connectorContainer = ConnectionManager.GetConnectorContainer(MessageHitTestContext.AssociatedDesigner);
                if (connectorContainer != null && connectorContainer.EnableUserDrawnConnectors)
                {
                    Point logicalPoint = workflowView.ClientPointToLogical(clientPoint);
                    ConnectablePoints = GetHighlightableConnectionPoints(logicalPoint, MessageHitTestContext.AssociatedDesigner);
                    cursorPoint = logicalPoint;
                }
            }

            bool processedMessage = UpdateCursor(cursorPoint);
            return processedMessage;
        }

        protected override bool OnMouseLeave()
        {
            //Make sure that when the mouse leaves we are not in edit mode and we restore the cursor
            EndEditing(null);
            UpdateCursor(null);
            return false;
        }

        protected override bool OnMouseCaptureChanged()
        {
            //Make sure that when the mouse leaves we are not in edit mode and we restore the cursor
            EndEditing(null);
            UpdateCursor(null);
            return false;
        }

        protected override bool OnMouseUp(MouseEventArgs eventArgs)
        {
            //If left button is not down then return 
            Point cursorPoint = Point.Empty;
            bool processedMessage = EditingInProgress;

            if ((eventArgs.Button & MouseButtons.Left) == MouseButtons.Left)
            {
                WorkflowView workflowView = ParentView;
                Point clientPoint = new Point(eventArgs.X, eventArgs.Y);
                if (workflowView != null && workflowView.IsClientPointInActiveLayout(clientPoint))
                {
                    Point logicalPoint = workflowView.ClientPointToLogical(clientPoint);
                    if (EditingInProgress)
                        EndEditing(logicalPoint);
                    cursorPoint = logicalPoint;
                }
            }

            //Make sure that whenever the mouse button is up we end the editing mode and update the cursor correctly
            EndEditing(null);
            processedMessage |= UpdateCursor(cursorPoint);
            return processedMessage;
        }

        protected override bool OnKeyDown(KeyEventArgs eventArgs)
        {
            if (EditingInProgress && eventArgs.KeyValue == (int)Keys.Escape)
            {
                EndEditing(null);
                eventArgs.Handled = true;
            }

            return eventArgs.Handled;
        }

        protected override bool OnPaint(PaintEventArgs e, Rectangle viewPort, AmbientTheme ambientTheme)
        {
            //Draw the selected connectors at top of the z level
            Connector selectedConnector = null;
            ISelectionService selectionService = GetService(typeof(ISelectionService)) as ISelectionService;
            foreach (object selectedComponents in selectionService.GetSelectedComponents())
            {
                Connector connector = Connector.GetConnectorFromSelectedObject(selectedComponents);
                if (connector != null)
                {
                    connector.OnPaintSelected(new ActivityDesignerPaintEventArgs(e.Graphics, connector.ParentDesigner.Bounds, viewPort, connector.ParentDesigner.DesignerTheme), (selectedComponents == selectionService.PrimarySelection), new Point[] { });
                    if (selectedComponents == selectionService.PrimarySelection)
                        selectedConnector = connector;
                }
            }

            //Draw selected connector adorned with the edit points
            if (selectedConnector != null)
            {
                ConnectorEditor editableConnector = new ConnectorEditor(selectedConnector);
                editableConnector.OnPaint(new ActivityDesignerPaintEventArgs(e.Graphics, selectedConnector.ParentDesigner.Bounds, viewPort, selectedConnector.ParentDesigner.DesignerTheme), true, true);
            }

            //If editing is in progress then draw the connector being edited
            if (EditingInProgress)
            {
                FreeformActivityDesigner designer = (this.connectorEditor.EditedConnector.ParentDesigner != null) ? this.connectorEditor.EditedConnector.ParentDesigner : ConnectionManager.GetConnectorContainer(this.connectorEditor.EditedConnector.Source.AssociatedDesigner);
                this.connectorEditor.OnPaint(new ActivityDesignerPaintEventArgs(e.Graphics, designer.Bounds, viewPort, designer.DesignerTheme), false, false);
            }

            return false;
        }
        #endregion

        #region Helpers
        private ConnectorEditor GetConnectorEditor(Point editPoint, HitTestInfo messageContext)
        {
            Connector connector = null;

            //First check if we are editing a existing selected connector
            ISelectionService selectionService = GetService(typeof(ISelectionService)) as ISelectionService;
            if (selectionService != null)
            {
                Connector selectedConnector = Connector.GetConnectorFromSelectedObject(selectionService.PrimarySelection);
                if (selectedConnector != null && selectedConnector.ParentDesigner.EnableUserDrawnConnectors && new ConnectorEditor(selectedConnector).HitTest(editPoint))
                    connector = selectedConnector;
            }

            //Then check if the hit is on a ConnectionPoint for drawing new connectors
            if (connector == null)
            {
                ConnectionPointHitTestInfo connectionPointHitTestInfo = messageContext as ConnectionPointHitTestInfo;
                if (connectionPointHitTestInfo != null && connectionPointHitTestInfo.ConnectionPoint != null)
                {
                    FreeformActivityDesigner connectorContainer = ConnectionManager.GetConnectorContainer(connectionPointHitTestInfo.AssociatedDesigner);
                    if (connectorContainer != null && connectorContainer.EnableUserDrawnConnectors)
                        connector = connectorContainer.CreateConnector(connectionPointHitTestInfo.ConnectionPoint, connectionPointHitTestInfo.ConnectionPoint);
                }
            }

            return ((connector != null) ? new ConnectorEditor(connector) : null);
        }

        private bool CanBeginEditing(Point editPoint, HitTestInfo messageContext)
        {
            ISelectionService selectionService = GetService(typeof(ISelectionService)) as ISelectionService;
            if (selectionService != null)
            {
                Connector selectedConnector = Connector.GetConnectorFromSelectedObject(selectionService.PrimarySelection);
                if (selectedConnector != null && selectedConnector.ParentDesigner.EnableUserDrawnConnectors && new ConnectorEditor(selectedConnector).HitTest(editPoint))
                    return true;
            }

            ConnectionPointHitTestInfo connectionPointHitTestInfo = messageContext as ConnectionPointHitTestInfo;
            if (connectionPointHitTestInfo != null && connectionPointHitTestInfo.ConnectionPoint != null)
            {
                FreeformActivityDesigner connectorContainer = ConnectionManager.GetConnectorContainer(connectionPointHitTestInfo.AssociatedDesigner);
                if (connectorContainer != null && connectorContainer.EnableUserDrawnConnectors)
                    return true;
            }

            return false;
        }

        private void BeginEditing(ConnectorEditor editableConnector, Point editPoint)
        {
            WorkflowView workflowView = ParentView;
            if (workflowView != null && editableConnector != null)
            {
                this.connectorEditor = editableConnector;
                workflowView.Capture = true;
                this.connectorEditor.OnBeginEditing(editPoint);
            }
        }

        private void ContinueEditing(Point editPoint)
        {
            if (!EditingInProgress)
            {
                Debug.Assert(false);
                return;
            }

            ConnectionPoint[] snapableConnectionPoints = null;
            if (this.connectorEditor.EditedConectionPoint != null)
            {
                ConnectionPoint sourceConnectionPoint = this.connectorEditor.EditedConnector.Source == this.connectorEditor.EditedConectionPoint ?
                    this.connectorEditor.EditedConnector.Target :
                    this.connectorEditor.EditedConnector.Source;
                snapableConnectionPoints = GetSnappableConnectionPoints(editPoint, sourceConnectionPoint, this.connectorEditor.EditedConectionPoint, MessageHitTestContext.AssociatedDesigner, out this.snappedConnectionPoint);
            }

            ConnectablePoints = snapableConnectionPoints;
            if (SnappedConnectionPoint != null)
                editPoint = SnappedConnectionPoint.Location;

            this.connectorEditor.OnContinueEditing(editPoint);
        }

        private void EndEditing(Nullable<Point> editPoint)
        {
            WorkflowView workflowView = ParentView;
            if (workflowView == null)
                return;

            if (EditingInProgress)
            {
                if (editPoint != null)
                {
                    ConnectionPoint[] snapableConnectionPoints = null;
                    if (this.connectorEditor.EditedConectionPoint != null)
                    {
                        ConnectionPoint sourceConnectionPoint = this.connectorEditor.EditedConnector.Source == this.connectorEditor.EditedConectionPoint ?
                            this.connectorEditor.EditedConnector.Target :
                            this.connectorEditor.EditedConnector.Source;
                        snapableConnectionPoints = GetSnappableConnectionPoints(editPoint.Value, sourceConnectionPoint, this.connectorEditor.EditedConectionPoint, MessageHitTestContext.AssociatedDesigner, out this.snappedConnectionPoint);
                    }

                    if (SnappedConnectionPoint != null)
                        editPoint = SnappedConnectionPoint.Location;
                }

                this.connectorEditor.OnEndEditing((editPoint != null) ? editPoint.Value : Point.Empty, (editPoint != null));
            }

            this.initialDragPoint = null;
            this.dragPointHitInfo = null;
            this.snappedConnectionPoint = null;
            ConnectablePoints = null;
            workflowView.Capture = false;
            this.connectorEditor = null;
        }

        private bool EditingInProgress
        {
            get
            {
                return (this.connectorEditor != null);
            }
        }

        private ConnectionPoint[] ConnectablePoints
        {
            get
            {
                return this.connectablePoints;
            }

            set
            {
                WorkflowView workflowView = ParentView;
                if (workflowView == null)
                    return;

                if (this.connectablePoints != null)
                {
                    foreach (ConnectionPoint snapPoint in this.connectablePoints)
                        workflowView.InvalidateLogicalRectangle(snapPoint.Bounds);
                }

                this.connectablePoints = value;

                if (this.connectablePoints != null)
                {
                    foreach (ConnectionPoint snapPoint in this.connectablePoints)
                        workflowView.InvalidateLogicalRectangle(snapPoint.Bounds);
                }
            }
        }

        private bool UpdateCursor(Nullable<Point> cursorPoint)
        {
            Cursor cursorToSet = Cursors.Default;

            if (cursorPoint != null)
            {
                if (EditingInProgress)
                    cursorToSet = this.connectorEditor.GetCursor(cursorPoint.Value);

                //Connector snap cursor always takes precedence over other cursors
                if (SnappedConnectionPoint != null)
                {
                    cursorToSet = ConnectionManager.SnappedConnectionCursor;
                }
                else if (ConnectablePoints != null)
                {
                    foreach (ConnectionPoint connectablePoint in ConnectablePoints)
                    {
                        if (connectablePoint.Bounds.Contains(cursorPoint.Value))
                        {
                            cursorToSet = ConnectionManager.SnappedConnectionCursor;
                            break;
                        }
                    }

                    //Fall back and check if we are hovering on any edit points
                    if (cursorToSet == Cursors.Default)
                    {
                        ISelectionService selectionService = GetService(typeof(ISelectionService)) as ISelectionService;
                        if (selectionService != null)
                        {
                            Connector selectedConnector = Connector.GetConnectorFromSelectedObject(selectionService.PrimarySelection);
                            if (selectedConnector != null && selectedConnector.ParentDesigner.EnableUserDrawnConnectors)
                            {
                                ConnectorEditor connectorEditor = new ConnectorEditor(selectedConnector);
                                cursorToSet = connectorEditor.GetCursor(cursorPoint.Value);
                            }
                        }
                    }
                }
            }

            WorkflowView workflowView = ParentView;
            if (workflowView != null &&
                (cursorToSet != Cursors.Default ||
                    workflowView.Cursor == ConnectionManager.SnappedConnectionCursor ||
                    workflowView.Cursor == ConnectionManager.NewConnectorCursor))
            {
                workflowView.Cursor = cursorToSet;
            }

            return (cursorToSet != Cursors.Default);
        }

        internal ConnectionPoint SnappedConnectionPoint
        {
            get
            {
                return this.snappedConnectionPoint;
            }
        }

        internal static FreeformActivityDesigner GetConnectorContainer(ActivityDesigner associatedDesigner)
        {
            //This function will walk up the parent chain of the designers and give the topmost container of connectors
            FreeformActivityDesigner connectorContainer = null;

            if (associatedDesigner != null)
            {
                ActivityDesigner connectedDesigner = associatedDesigner;
                while (connectedDesigner != null)
                {
                    if (connectedDesigner is FreeformActivityDesigner)
                        connectorContainer = connectedDesigner as FreeformActivityDesigner;
                    else if (connectedDesigner is InvokeWorkflowDesigner)
                        break; //state machine withing invoke workflow activity is the root
                    connectedDesigner = connectedDesigner.ParentDesigner;
                }
            }

            return connectorContainer;
        }

        private static ConnectionPoint[] GetSnappableConnectionPoints(Point currentPoint, ConnectionPoint sourceConnectionPoint, ConnectionPoint activeConnectionPoint, ActivityDesigner activityDesigner, out ConnectionPoint snappedConnectionPoint)
        {
            //If the activity designer is composite activity designer then we will go through its children else we will go through its connection points
            snappedConnectionPoint = null;

            List<ConnectionPoint> snappableConnectionPoints = new List<ConnectionPoint>();

            FreeformActivityDesigner connectorContainer = ConnectionManager.GetConnectorContainer(activeConnectionPoint.AssociatedDesigner);
            if (connectorContainer != null)
            {
                FreeformActivityDesigner freeFormDesigner = activityDesigner as FreeformActivityDesigner;
                List<ActivityDesigner> designersToCheck = new List<ActivityDesigner>();
                designersToCheck.Add(activityDesigner);
                if (freeFormDesigner != null)
                    designersToCheck.AddRange(freeFormDesigner.ContainedDesigners);

                double minimumDistance = ConnectionManager.SnapHighlightDistance;
                foreach (ActivityDesigner designer in designersToCheck)
                {
                    if (ConnectionManager.GetConnectorContainer(designer) == connectorContainer)
                    {
                        bool addValidSnapPoints = false;
                        List<ConnectionPoint> validSnapPoints = new List<ConnectionPoint>();

                        ReadOnlyCollection<ConnectionPoint> snapPoints = designer.GetConnectionPoints(DesignerEdges.All);
                        foreach (ConnectionPoint snapPoint in snapPoints)
                        {
                            if (!snapPoint.Equals(activeConnectionPoint) &&
                                connectorContainer.CanConnectContainedDesigners(sourceConnectionPoint, snapPoint))
                            {
                                validSnapPoints.Add(snapPoint);

                                double distanceToDesigner = DesignerGeometryHelper.DistanceFromPointToRectangle(currentPoint, snapPoint.Bounds);
                                if (distanceToDesigner <= ConnectionManager.SnapHighlightDistance)
                                {
                                    addValidSnapPoints = true;
                                    if (distanceToDesigner < minimumDistance)
                                    {
                                        snappedConnectionPoint = snapPoint;
                                        minimumDistance = distanceToDesigner;
                                    }
                                }
                            }
                        }

                        if (addValidSnapPoints)
                            snappableConnectionPoints.AddRange(validSnapPoints);
                    }
                }

                if (snappedConnectionPoint != null)
                {
                    foreach (ConnectionPoint connectionPoint in snappedConnectionPoint.AssociatedDesigner.GetConnectionPoints(DesignerEdges.All))
                    {
                        if (!snappableConnectionPoints.Contains(connectionPoint))
                            snappableConnectionPoints.Add(connectionPoint);
                    }
                }
            }

            return snappableConnectionPoints.ToArray();
        }

        private static ConnectionPoint[] GetHighlightableConnectionPoints(Point currentPoint, ActivityDesigner activityDesigner)
        {
            List<ConnectionPoint> highlightablePoints = new List<ConnectionPoint>();
            List<ActivityDesigner> designersToCheck = new List<ActivityDesigner>();

            FreeformActivityDesigner freeFormDesigner = activityDesigner as FreeformActivityDesigner;
            if (freeFormDesigner != null)
                designersToCheck.AddRange(freeFormDesigner.ContainedDesigners);

            designersToCheck.Add(activityDesigner);

            foreach (ActivityDesigner designer in designersToCheck)
            {
                bool addSnapPoints = (designer.Bounds.Contains(currentPoint));
                ReadOnlyCollection<ConnectionPoint> snapPoints = designer.GetConnectionPoints(DesignerEdges.All);
                if (!addSnapPoints)
                {
                    foreach (ConnectionPoint snapPoint in snapPoints)
                    {
                        if (snapPoint.Bounds.Contains(currentPoint))
                        {
                            addSnapPoints = true;
                            break;
                        }
                    }
                }

                if (addSnapPoints)
                    highlightablePoints.AddRange(snapPoints);
            }

            return highlightablePoints.ToArray();
        }
        #endregion

        #region IDesignerGlyphProvider Members
        ActivityDesignerGlyphCollection IDesignerGlyphProvider.GetGlyphs(ActivityDesigner activityDesigner)
        {
            ActivityDesignerGlyphCollection glyphCollection = new ActivityDesignerGlyphCollection();
            ConnectionPoint[] connectablePoints = ConnectablePoints;
            if (connectablePoints != null)
            {
                foreach (ConnectionPoint connectablePoint in connectablePoints)
                {
                    if (activityDesigner == connectablePoint.AssociatedDesigner)
                        glyphCollection.Add(new ConnectionPointGlyph(connectablePoint));
                }
            }

            return glyphCollection;
        }
        #endregion
    }
    #endregion
}
