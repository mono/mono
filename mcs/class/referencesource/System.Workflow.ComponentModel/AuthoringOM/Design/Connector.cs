using System;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Windows.Forms;
using System.Diagnostics;
using System.ComponentModel.Design.Serialization;
using System.Workflow.ComponentModel.Serialization;

namespace System.Workflow.ComponentModel.Design
{
    #region Class ConnectionPointHitTestInfo
    internal sealed class ConnectionPointHitTestInfo : HitTestInfo
    {
        private ConnectionPoint connectionPoint;

        internal ConnectionPointHitTestInfo(ConnectionPoint connectionPoint)
            : base(connectionPoint.AssociatedDesigner, HitTestLocations.Designer | HitTestLocations.Connector)
        {
            this.connectionPoint = connectionPoint;
        }

        internal ConnectionPoint ConnectionPoint
        {
            get
            {
                return this.connectionPoint;
            }
        }
    }
    #endregion

    #region ConnectorEvent
    public delegate void ConnectorEventHandler(object sender, ConnectorEventArgs e);

    #region Class ConnectorEventArgs
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class ConnectorEventArgs : EventArgs
    {
        private Connector connector;

        internal ConnectorEventArgs(Connector connector)
        {
            this.connector = connector;
        }

        public Connector Connector
        {
            get
            {
                return this.connector;
            }
        }
    }
    #endregion
    #endregion

    #region Class ConnectionPoint
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class ConnectionPoint
    {
        private ActivityDesigner associatedDesigner;
        private DesignerEdges designerEdge;
        private int connectionIndex;

        public ConnectionPoint(ActivityDesigner associatedDesigner, DesignerEdges designerEdge, int connectionIndex)
        {
            if (associatedDesigner == null)
                throw new ArgumentNullException("associatedDesigner");

            if (connectionIndex < 0 || connectionIndex >= associatedDesigner.GetConnections(designerEdge).Count)
                throw new ArgumentException(DR.GetString(DR.Error_ConnectionPoint), "connectionIndex");

            this.associatedDesigner = associatedDesigner;
            this.designerEdge = designerEdge;
            this.connectionIndex = connectionIndex;
        }

        public ActivityDesigner AssociatedDesigner
        {
            get
            {
                return this.associatedDesigner;
            }
        }

        public int ConnectionIndex
        {
            get
            {
                return this.connectionIndex;
            }
        }

        public DesignerEdges ConnectionEdge
        {
            get
            {
                DesignerEdges designerEdge = this.designerEdge;

                if (designerEdge != DesignerEdges.Left && designerEdge != DesignerEdges.Right &&
                    designerEdge != DesignerEdges.Top && designerEdge != DesignerEdges.Bottom)
                {
                    designerEdge = DesignerGeometryHelper.ClosestEdgeToPoint(Location, this.associatedDesigner.Bounds, designerEdge);
                }

                return designerEdge;
            }
        }

        public virtual Point Location
        {
            get
            {
                IList<Point> connections = this.associatedDesigner.GetConnections(this.designerEdge);
                if (this.connectionIndex < connections.Count)
                    return connections[this.connectionIndex];
                else
                    return Point.Empty;
            }
        }

        public virtual Rectangle Bounds
        {
            get
            {
                IList<Point> connections = this.associatedDesigner.GetConnections(DesignerEdges.All);
                if (connections.Count > 0)
                {
                    Point location = Location;
                    Size size = DefaultSize;
                    Rectangle enclosingBounds = new Rectangle(new Point(location.X - size.Width / 2, location.Y - size.Height / 2), size);
                    return enclosingBounds;
                }
                else
                {
                    return Rectangle.Empty;
                }
            }
        }

        public override bool Equals(object obj)
        {
            ConnectionPoint connectionPoint = obj as ConnectionPoint;
            if (connectionPoint == null)
                return false;

            if (connectionPoint.AssociatedDesigner == this.associatedDesigner &&
                connectionPoint.designerEdge == this.designerEdge &&
                connectionPoint.ConnectionIndex == this.connectionIndex)
                return true;
            else
                return false;
        }

        public override int GetHashCode()
        {
            return (this.associatedDesigner.GetHashCode() ^ this.designerEdge.GetHashCode() ^ this.connectionIndex.GetHashCode());
        }

        public void OnPaint(ActivityDesignerPaintEventArgs e, bool drawHighlighted)
        {
            Draw(e, Bounds);
        }

        internal static void Draw(ActivityDesignerPaintEventArgs e, Rectangle bounds)
        {
            bounds.Inflate(-1, -1);
            e.Graphics.FillEllipse(Brushes.White, bounds);
            e.Graphics.DrawEllipse(e.AmbientTheme.SelectionForegroundPen, bounds);

            bounds.Inflate(-1, -1);
            e.Graphics.FillEllipse(e.AmbientTheme.SelectionForegroundBrush, bounds);
        }

        private Size DefaultSize
        {
            get
            {
                Size defaultSize = WorkflowTheme.CurrentTheme.AmbientTheme.Margin;
                defaultSize.Width += defaultSize.Width / 2;
                defaultSize.Height += defaultSize.Height / 2;
                if (this.associatedDesigner != null)
                    defaultSize = new Size(Math.Max(defaultSize.Width, (int)this.associatedDesigner.DesignerTheme.ForegroundPen.Width * 4), Math.Max(defaultSize.Height, (int)this.associatedDesigner.DesignerTheme.ForegroundPen.Width * 4));
                return defaultSize;
            }
        }
    }
    #endregion

    #region Class Connector
    [DesignerSerializer(typeof(ConnectorLayoutSerializer), typeof(WorkflowMarkupSerializer))]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class Connector : IDisposable
    {
        #region Members, Construction, Dispose
        private AccessibleObject accessibilityObject;
        private FreeformActivityDesigner parentDesigner;
        private ConnectionPoint source = null;
        private ConnectionPoint target = null;
        private List<Point> segments = new List<Point>();
        private bool connectorModified = false;

        public Connector(ConnectionPoint source, ConnectionPoint target)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            if (target == null)
                throw new ArgumentNullException("target");

            if (ConnectionManager.GetConnectorContainer(source.AssociatedDesigner) != ConnectionManager.GetConnectorContainer(target.AssociatedDesigner))
                throw new ArgumentException(DR.GetString(DR.Error_Connector1));

            this.source = source;
            this.target = target;
        }

        void IDisposable.Dispose()
        {
        }
        #endregion

        #region Properties and Methods

        public virtual AccessibleObject AccessibilityObject
        {
            get
            {
                if (this.accessibilityObject == null)
                    this.accessibilityObject = new ConnectorAccessibleObject(this);
                return this.accessibilityObject;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ConnectionPoint Source
        {
            get
            {
                return this.source;
            }

            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                if (this.source.Equals(value))
                    return;

                if (ConnectionManager.GetConnectorContainer(value.AssociatedDesigner) != ConnectionManager.GetConnectorContainer(this.target.AssociatedDesigner))
                    throw new ArgumentException(SR.GetString(SR.Error_InvalidConnectorSource), "value");

                this.source = value;
                PerformLayout();
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ConnectionPoint Target
        {
            get
            {
                return this.target;
            }

            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                if (this.target.Equals(value))
                    return;

                if (ConnectionManager.GetConnectorContainer(value.AssociatedDesigner) != ConnectionManager.GetConnectorContainer(this.source.AssociatedDesigner))
                    throw new ArgumentException(SR.GetString(SR.Error_InvalidConnectorSource), "value");

                this.target = value;
                PerformLayout();
            }
        }

        public virtual ReadOnlyCollection<Point> ConnectorSegments
        {
            get
            {
                List<Point> connectorSegments = new List<Point>();

                if (this.source != null && this.target != null)
                {
                    if (this.segments.Count == 0 || this.segments[0] != this.source.Location)
                        connectorSegments.Add(this.source.Location);

                    connectorSegments.AddRange(this.segments);

                    if (this.segments.Count == 0 || this.segments[this.segments.Count - 1] != this.target.Location)
                        connectorSegments.Add(this.target.Location);
                }

                return connectorSegments.AsReadOnly();
            }
        }

        public Rectangle Bounds
        {
            get
            {
                Rectangle bounds = DesignerGeometryHelper.RectangleFromLineSegments(new List<Point>(ConnectorSegments).ToArray());
                bounds.Inflate(1, 1);
                return bounds;
            }
        }

        public FreeformActivityDesigner ParentDesigner
        {
            get
            {
                return this.parentDesigner;
            }
        }

        public virtual bool HitTest(Point point)
        {
            Size selectionSize = WorkflowTheme.CurrentTheme.AmbientTheme.SelectionSize;

            //We go thru the line seagments and hittest
            ReadOnlyCollection<Point> segments = ConnectorSegments;
            for (int i = 1; i < segments.Count; i++)
            {
                if (DesignerGeometryHelper.PointOnLineSegment(point, new Point[] { segments[i - 1], segments[i] }, selectionSize))
                    return true;
            }

            return false;
        }

        public virtual void Offset(Size size)
        {
            for (int i = 0; i < this.segments.Count; i++)
                this.segments[i] = new Point(this.segments[i].X + size.Width, this.segments[i].Y + size.Height);
        }

        public override bool Equals(object obj)
        {
            Connector connector = obj as Connector;
            if (connector == null)
                return false;

            return (connector.Source == this.source && connector.target == this.target);
        }

        public override int GetHashCode()
        {
            if (this.source != null && this.target != null)
                return this.source.GetHashCode() ^ this.target.GetHashCode();
            else
                return base.GetHashCode();
        }

        public void Invalidate()
        {
            WorkflowView workflowView = ParentView;
            if (workflowView != null)
                workflowView.InvalidateLogicalRectangle(Bounds);
        }

        public bool ConnectorModified
        {
            get
            {
                return this.connectorModified;
            }
        }

        protected WorkflowView ParentView
        {
            get
            {
                return GetService(typeof(WorkflowView)) as WorkflowView;
            }
        }

        protected void PerformLayout()
        {
            WorkflowView workflowView = ParentView;
            if (workflowView != null)
                workflowView.PerformLayout(false);
        }

        protected internal virtual ICollection<Rectangle> ExcludedRoutingRectangles
        {
            get
            {
                return new Rectangle[] { };
            }
        }

        //We want to allow framework or the derived classes only to set the connector segments
        //We do not want any outside entity to set the connector segments randomly
        protected internal void SetConnectorSegments(ICollection<Point> segments)
        {
            //Called by derived classes when they want to change connector routing
            //
            if (segments == null)
                throw new ArgumentNullException("segments");

            this.connectorModified = (this.parentDesigner != null && segments.Count > 0);
            if (this.connectorModified)
                Invalidate();

            this.segments.Clear();
            this.segments.AddRange(segments);

            if (this.connectorModified)
                Invalidate();
        }

        protected internal virtual void OnLayout(ActivityDesignerLayoutEventArgs e)
        {
            if (this.segments.Count > 0 &&
                (this.segments[0] != Source.Location || this.segments[this.segments.Count - 1] != Target.Location))
                this.connectorModified = false;

            if (!connectorModified && ParentDesigner != null)
            {
                Point[] newSegments = ActivityDesignerConnectorRouter.Route(Source.AssociatedDesigner.Activity.Site, Source, Target, ExcludedRoutingRectangles);
                this.segments.Clear();
                this.segments.AddRange(newSegments);
            }
        }

        protected internal virtual void OnPaint(ActivityDesignerPaintEventArgs e)
        {
            CompositeDesignerTheme theme = e.DesignerTheme as CompositeDesignerTheme;
            if (theme != null)
            {
                Size arrowCapSize = new Size(theme.ConnectorSize.Width / 5, theme.ConnectorSize.Height / 5);
                Size maxCapSize = theme.ConnectorSize;
                ActivityDesignerPaint.DrawConnectors(e.Graphics, e.DesignerTheme.ForegroundPen, new List<Point>(ConnectorSegments).ToArray(), arrowCapSize, maxCapSize, theme.ConnectorStartCap, theme.ConnectorEndCap);
            }
        }

        protected internal virtual void OnPaintSelected(ActivityDesignerPaintEventArgs e, bool primarySelection, Point[] segmentEditPoints)
        {
            CompositeDesignerTheme theme = e.DesignerTheme as CompositeDesignerTheme;
            if (theme == null)
                return;

            using (Pen lineSelectionPen = new Pen(WorkflowTheme.CurrentTheme.AmbientTheme.SelectionForeColor, 1))
            {
                Size arrowCapSize = new Size(theme.ConnectorSize.Width / 5, theme.ConnectorSize.Height / 5);
                Size maxCapSize = theme.ConnectorSize;
                ActivityDesignerPaint.DrawConnectors(e.Graphics, lineSelectionPen, new List<Point>(ConnectorSegments).ToArray(), arrowCapSize, maxCapSize, theme.ConnectorStartCap, theme.ConnectorEndCap);
            }

            if (this.source != null)
                this.source.OnPaint(e, false);

            ReadOnlyCollection<Point> endSegmentEditPoints = ConnectorSegments;
            for (int i = 1; i < endSegmentEditPoints.Count - 1; i++)
                PaintEditPoints(e, endSegmentEditPoints[i], false);

            for (int i = 0; i < segmentEditPoints.Length; i++)
                PaintEditPoints(e, segmentEditPoints[i], true);

            if (this.target != null)
                this.target.OnPaint(e, false);
        }

        protected internal virtual void OnPaintEdited(ActivityDesignerPaintEventArgs e, Point[] segments, Point[] segmentEditPoints)
        {
            CompositeDesignerTheme theme = e.DesignerTheme as CompositeDesignerTheme;
            if (theme == null)
                return;

            using (Pen editableConnectorPen = new Pen(e.AmbientTheme.SelectionForegroundPen.Color, e.AmbientTheme.SelectionForegroundPen.Width))
            {
                editableConnectorPen.DashStyle = DashStyle.Dash;
                Size arrowCapSize = new Size(theme.ConnectorSize.Width / 5, theme.ConnectorSize.Height / 5);
                Size maxCapSize = theme.ConnectorSize;
                ActivityDesignerPaint.DrawConnectors(e.Graphics, editableConnectorPen, segments, arrowCapSize, maxCapSize, theme.ConnectorStartCap, theme.ConnectorEndCap);
            }

            if (this.source != null)
                this.source.OnPaint(e, false);

            for (int i = 1; i < segments.Length - 1; i++)
                PaintEditPoints(e, segments[i], false);

            for (int i = 0; i < segmentEditPoints.Length; i++)
                PaintEditPoints(e, segmentEditPoints[i], true);

            if (this.target != null)
                this.target.OnPaint(e, false);
        }

        protected virtual object GetService(Type serviceType)
        {
            object service = null;
            if (this.parentDesigner != null && this.parentDesigner.Activity != null && this.parentDesigner.Activity.Site != null)
                service = this.parentDesigner.Activity.Site.GetService(serviceType);
            return service;
        }
        #endregion

        #region Helpers
        private void PaintEditPoints(ActivityDesignerPaintEventArgs e, Point point, bool drawMidSegmentEditPoint)
        {
            Size size = (this.source != null) ? this.source.Bounds.Size : Size.Empty;
            if (!size.IsEmpty)
            {
                Rectangle bounds = new Rectangle(point.X - size.Width / 2, point.Y - size.Height / 2, size.Width, size.Height);
                if (drawMidSegmentEditPoint)
                {
                    using (GraphicsPath path = new GraphicsPath())
                    {
                        path.AddLine(new Point(bounds.Left + bounds.Width / 2, bounds.Top), new Point(bounds.Right, bounds.Top + bounds.Height / 2));
                        path.AddLine(new Point(bounds.Right, bounds.Top + bounds.Height / 2), new Point(bounds.Left + bounds.Width / 2, bounds.Bottom));
                        path.AddLine(new Point(bounds.Left + bounds.Width / 2, bounds.Bottom), new Point(bounds.Left, bounds.Top + bounds.Height / 2));
                        path.AddLine(new Point(bounds.Left, bounds.Top + bounds.Height / 2), new Point(bounds.Left + bounds.Width / 2, bounds.Top));

                        e.Graphics.FillPath(Brushes.White, path);
                        e.Graphics.DrawPath(e.AmbientTheme.SelectionForegroundPen, path);
                    }
                }
                else
                {
                    bounds.Inflate(-1, -1);
                    e.Graphics.FillEllipse(e.AmbientTheme.SelectionForegroundBrush, bounds);
                }
            }
        }

        internal void SetConnectorModified(bool modified)
        {
            this.connectorModified = modified;
        }

        internal FreeformActivityDesigner RenderingOwner
        {
            get
            {
                if (this.source == null || this.target == null)
                    return null;

                //




                List<FreeformActivityDesigner> targetParents = new List<FreeformActivityDesigner>();
                ActivityDesigner designer = this.target.AssociatedDesigner;
                while (designer != null)
                {
                    FreeformActivityDesigner parentFreeFormDesigner = designer as FreeformActivityDesigner;
                    if (parentFreeFormDesigner != null)
                        targetParents.Add(parentFreeFormDesigner);
                    designer = designer.ParentDesigner;
                }

                //Go through the parent freeforms of source and match it with target, the first common
                //parent freeform is the rendering designer
                designer = this.source.AssociatedDesigner;
                while (designer != null)
                {
                    FreeformActivityDesigner parentFreeFormDesigner = designer as FreeformActivityDesigner;
                    if (parentFreeFormDesigner != null && targetParents.Contains(parentFreeFormDesigner))
                        break;
                    designer = designer.ParentDesigner;
                }

                return designer as FreeformActivityDesigner;
            }
        }

        internal void SetParent(FreeformActivityDesigner parentDesigner)
        {
            //Make sure that this is in parent chain of both source and the target connection points
            WorkflowView workflowView = ParentView;
            if (this.parentDesigner != null && workflowView != null)
                workflowView.InvalidateLogicalRectangle(this.parentDesigner.Bounds);

            this.parentDesigner = parentDesigner;

            if (this.parentDesigner != null && workflowView != null)
                workflowView.InvalidateLogicalRectangle(this.parentDesigner.Bounds);
        }

        internal static Connector GetConnectorFromSelectedObject(object selectedObject)
        {
            Connector connector = null;
            ConnectorHitTestInfo connectorHitTestInfo = selectedObject as ConnectorHitTestInfo;
            if (connectorHitTestInfo != null)
            {
                FreeformActivityDesigner connectorContainer = connectorHitTestInfo.AssociatedDesigner as FreeformActivityDesigner;
                int index = connectorHitTestInfo.MapToIndex();
                if (connectorContainer != null && index >= 0 && index < connectorContainer.Connectors.Count)
                    connector = connectorContainer.Connectors[index];
            }

            return connector;
        }

        #region Properties used during serialization only
        //NOTE THAT THIS WILL ONLY BE USED FOR SERIALIZATION PURPOSES
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        internal string SourceActivity
        {
            get
            {
                string activityName = String.Empty;
                if (this.source != null)
                    activityName = this.source.AssociatedDesigner.Activity.QualifiedName;
                return activityName;
            }

            set
            {
            }
        }

        //NOTE THAT THIS WILL ONLY BE USED FOR SERIALIZATION PURPOSES
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        internal int SourceConnectionIndex
        {
            get
            {
                int connectionIndex = 0;
                if (this.source != null)
                    connectionIndex = this.source.ConnectionIndex;
                return connectionIndex;
            }

            set
            {
            }
        }

        //NOTE THAT THIS WILL ONLY BE USED FOR SERIALIZATION PURPOSES
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        internal DesignerEdges SourceConnectionEdge
        {
            get
            {
                DesignerEdges connectionEdge = DesignerEdges.None;
                if (this.source != null)
                    connectionEdge = this.source.ConnectionEdge;
                return connectionEdge;
            }

            set
            {
            }
        }

        //NOTE THAT THIS WILL ONLY BE USED FOR SERIALIZATION PURPOSES
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        internal string TargetActivity
        {
            get
            {
                string activityName = String.Empty;
                if (this.target != null)
                    activityName = this.target.AssociatedDesigner.Activity.QualifiedName;
                return activityName;
            }

            set
            {
            }
        }

        //NOTE THAT THIS WILL ONLY BE USED FOR SERIALIZATION PURPOSES
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        internal int TargetConnectionIndex
        {
            get
            {
                int connectionIndex = 0;
                if (this.target != null)
                    connectionIndex = this.target.ConnectionIndex;
                return connectionIndex;
            }

            set
            {
            }
        }

        //NOTE THAT THIS WILL ONLY BE USED FOR SERIALIZATION PURPOSES
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        internal DesignerEdges TargetConnectionEdge
        {
            get
            {
                DesignerEdges connectionEdge = DesignerEdges.None;
                if (this.target != null)
                    connectionEdge = this.target.ConnectionEdge;
                return connectionEdge;
            }

            set
            {
            }
        }

        //NOTE THAT THIS WILL ONLY BE USED FOR SERIALIZATION PURPOSES
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        internal List<Point> Segments
        {
            get
            {
                return this.segments;
            }
        }
        #endregion

        #endregion
    }
    #endregion

    #region Class ConnectorAccessibleObject

    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class ConnectorAccessibleObject : AccessibleObject
    {
        private Connector connector;

        public ConnectorAccessibleObject(Connector connector)
        {
            if (connector == null)
                throw new ArgumentNullException("connector");
            this.connector = connector;
        }

        public override Rectangle Bounds
        {
            get
            {
                WorkflowView parentView = this.connector.ParentDesigner.ParentView;
                Rectangle bounds = this.connector.Bounds;
                return new Rectangle(parentView.LogicalPointToScreen(bounds.Location), parentView.LogicalSizeToClient(bounds.Size));
            }
        }

        public override AccessibleObject HitTest(int x, int y)
        {
            WorkflowView parentView = this.connector.ParentDesigner.ParentView;
            if (this.connector.HitTest(parentView.ScreenPointToLogical(new Point(x, y))))
                return this;
            else
                return null;
        }

        public override string Name
        {
            get
            {
                return this.connector.GetType().Name;
            }
            set
            {
            }
        }

        public override AccessibleObject Parent
        {
            get
            {
                return connector.ParentDesigner.AccessibilityObject;
            }
        }

        public override AccessibleRole Role
        {
            get
            {
                return AccessibleRole.Diagram;
            }
        }
    }

    #endregion Class ConnectorAccessibleObject

    #region Class ConnectorEditor
    internal sealed class ConnectorEditor
    {
        private IServiceProvider serviceProvider;
        private Connector editedConnector;
        private EditPoint activeEditPoint;
        private List<EditPoint> editPoints = new List<EditPoint>();

        public ConnectorEditor(Connector connectorEdited)
        {
            this.editedConnector = connectorEdited;
            this.serviceProvider = this.editedConnector.Source.AssociatedDesigner.Activity.Site;
            CreateEditPoints();
        }

        public ConnectionPoint EditedConectionPoint
        {
            get
            {
                if (this.activeEditPoint != null)
                    return this.activeEditPoint.EditedConnectionPoint;
                else
                    return null;
            }
        }

        public Connector EditedConnector
        {
            get
            {
                return this.editedConnector;
            }
        }

        public Cursor GetCursor(Point cursorPoint)
        {
            Cursor cursor = Cursors.Default;
            if (this.activeEditPoint != null)
            {
                cursor = ConnectionManager.NewConnectorCursor;
            }
            else
            {
                foreach (EditPoint editPoint in this.editPoints)
                {
                    if (editPoint.Bounds.Contains(cursorPoint))
                    {
                        cursor = ConnectionManager.SnappedConnectionCursor;
                        break;
                    }
                }
            }

            return cursor;
        }

        public bool OnBeginEditing(Point point)
        {
            //Get all the editable points
            CreateEditPoints();

            EditPoint editPointHit = null;
            for (int i = this.editPoints.Count - 1; i >= 0; i--)
            {
                if (this.editPoints[i].Bounds.Contains(point))
                {
                    editPointHit = this.editPoints[i];
                    break;
                }
            }

            if (editPointHit != null &&
                (editPointHit.EditedConnectionPoint == null ||
                ConnectionManager.GetConnectorContainer(editPointHit.EditedConnectionPoint.AssociatedDesigner) != null))
            {
                editPointHit.Location = point;
                this.activeEditPoint = editPointHit;
            }

            Invalidate();
            return (this.activeEditPoint != null);
        }

        public void OnContinueEditing(Point point)
        {
            if (this.activeEditPoint == null)
                return;

            Invalidate();
            UpdateEditPoints(point);
            Invalidate();

#if DISPLAYESCAPEREGIONS
            WorkflowView workflowView = GetService(typeof(WorkflowView)) as WorkflowView;
            if (workflowView != null)
                workflowView.InvalidateClientRectangle(Rectangle.Empty);
#endif
        }

        public void OnEndEditing(Point point, bool commitChanges)
        {
            if (this.activeEditPoint == null)
                return;

            Invalidate();
            if (commitChanges)
            {
                //This is to update the edit points based on the activepoint
                UpdateEditPoints(point);

                EditPoint activeEditPoint = this.activeEditPoint;
                this.activeEditPoint = null;

                //This call is to optimize the segments
                UpdateEditPoints(point);

                //If we were creating a new connector or modifying the connection end points
                bool updateSegments = false;
                if (activeEditPoint.Type == EditPoint.EditPointTypes.ConnectionEditPoint)
                {
                    ConnectionManager connectionManager = GetService(typeof(ConnectionManager)) as ConnectionManager;
                    FreeformActivityDesigner connectorContainer = ConnectionManager.GetConnectorContainer(activeEditPoint.EditedConnectionPoint.AssociatedDesigner);
                    if (connectionManager != null && connectionManager.SnappedConnectionPoint != null && connectorContainer != null)
                    {
                        //Get the original source and targets
                        ConnectionPoint source = this.editedConnector.Source;
                        ConnectionPoint target = this.editedConnector.Target;

                        //Make sure that we set the source and target correctly
                        if (target.Equals(activeEditPoint.EditedConnectionPoint))
                            target = connectionManager.SnappedConnectionPoint;
                        else if (source.Equals(activeEditPoint.EditedConnectionPoint))
                            source = connectionManager.SnappedConnectionPoint;

                        //Check if it is a valid connection ie source can be connected to target
                        if (connectorContainer == ConnectionManager.GetConnectorContainer(target.AssociatedDesigner) &&
                            connectorContainer.CanConnectContainedDesigners(source, target))
                        {
                            this.editedConnector.Source = source;
                            this.editedConnector.Target = target;

                            if (this.editedConnector.ParentDesigner == null)
                            {
                                this.editedConnector = connectorContainer.AddConnector(source, target);

                                WorkflowDesignerLoader loader = GetService(typeof(WorkflowDesignerLoader)) as WorkflowDesignerLoader;
                                if (loader != null)
                                    loader.SetModified(true);
                            }

                            connectorContainer.OnContainedDesignersConnected(source, target);
                        }

                        updateSegments = true;
                    }
                }
                else
                {
                    updateSegments = true;
                }

                //Make sure that we apply the edit points to the connector
                if (updateSegments)
                {
                    this.editedConnector.SetConnectorSegments(GetPointsFromEditPoints(this.editPoints));
                    if (this.editedConnector.ParentDesigner != null)
                    {
                        this.editedConnector.ParentDesigner.OnConnectorChanged(new ConnectorEventArgs(this.editedConnector));

                        WorkflowDesignerLoader loader = GetService(typeof(WorkflowDesignerLoader)) as WorkflowDesignerLoader;
                        if (loader != null)
                            loader.SetModified(true);
                    }
                }

                PerformLayout();
            }

            Invalidate();
        }

        public bool HitTest(Point point)
        {
            for (int i = 0; i < this.editPoints.Count; i++)
            {
                EditPoint editPoint = this.editPoints[i];
                if (editPoint.Bounds.Contains(point))
                    return true;
            }

            return false;
        }

        public void OnPaint(ActivityDesignerPaintEventArgs e, bool drawSelected, bool drawPrimarySelection)
        {
            List<Point> segments = new List<Point>();
            List<Point> segmentEditPoints = new List<Point>();
            for (int i = 0; i < this.editPoints.Count; i++)
            {
                EditPoint editPoint = this.editPoints[i];
                if (editPoint.Type == EditPoint.EditPointTypes.ConnectionEditPoint ||
                    editPoint.Type == EditPoint.EditPointTypes.MultiSegmentEditPoint)
                    segments.Add(editPoint.Location);
                else
                    segmentEditPoints.Add(editPoint.Location);
            }

            //If the editing is in progress then we want to draw the dashed lines
            if (drawSelected)
                this.editedConnector.OnPaintSelected(e, drawPrimarySelection, segmentEditPoints.ToArray());

            if (this.activeEditPoint != null)
                this.editedConnector.OnPaintEdited(e, segments.ToArray(), segmentEditPoints.ToArray());

            //
#if DISPLAYESCAPEREGIONS
            if (this.activeEditPoint != null && this.activeEditPoint.Type == EditPoint.EditPointTypes.ConnectionEditPoint)
            {
                object source = null, target = null;
                if (this.activeEditPoint.EditedConnectionPoint.Equals(Target))
                {
                    target = this.activeEditPoint.Location;
                    source = Source;
                }
                else
                {
                    source = this.activeEditPoint.Location;
                    target = Target;
                }
                
                List<Rectangle> rectanglesToExclude;
                List<Point> linesToExclude, pointsToExclude;
                ActivityDesignerConnectorRouter.GetRoutingObstacles(this.serviceProvider, source, target, out rectanglesToExclude, out linesToExclude, out pointsToExclude);

                ICollection<Rectangle> userDefinedObstacles = this.editedConnector.ExcludedRoutingRectangles;
                if (userDefinedObstacles != null)
                {
                    foreach (Rectangle rectangle in userDefinedObstacles)
                        e.Graphics.DrawRectangle(Pens.DarkGreen, rectangle);
                }

                foreach (Rectangle rectangle in rectanglesToExclude)
                    e.Graphics.DrawRectangle(Pens.Red, rectangle);        

                for (int i = 0; i < linesToExclude.Count / 2; i++)
                    e.Graphics.DrawLine(Pens.Red, linesToExclude[i * 2], linesToExclude[(i * 2) + 1]);
            }
#endif
        }
        #region Helpers

        #region EditPointUpdation Logic
        private ConnectionPoint Source
        {
            get
            {
                return this.editedConnector.Source;
            }
        }

        private ConnectionPoint Target
        {
            get
            {
                return this.editedConnector.Target;
            }
        }

        private void PerformLayout()
        {
            WorkflowView workflowView = GetService(typeof(WorkflowView)) as WorkflowView;
            if (workflowView != null)
                workflowView.PerformLayout(false);
        }

        private void Invalidate()
        {
            WorkflowView workflowView = GetService(typeof(WorkflowView)) as WorkflowView;
            if (workflowView != null)
            {
                Rectangle bounds = DesignerGeometryHelper.RectangleFromLineSegments(GetPointsFromEditPoints(this.editPoints).ToArray());
                bounds.Inflate(1, 1);
                workflowView.InvalidateLogicalRectangle(bounds);
            }
        }

        private object GetService(Type serviceType)
        {
            object service = null;
            if (this.serviceProvider != null)
                service = this.serviceProvider.GetService(serviceType);
            return service;
        }

        private void CreateEditPoints()
        {
            this.editPoints.Clear();

            AddEditPoints(EditPoint.EditPointTypes.ConnectionEditPoint);
            AddEditPoints(EditPoint.EditPointTypes.MultiSegmentEditPoint);
            AddEditPoints(EditPoint.EditPointTypes.MidSegmentEditPoint);

            bool validEditPoints = ValidateEditPoints();
            Debug.Assert(validEditPoints);
        }

        private void UpdateEditPoints(Point newPoint)
        {
            if (this.editPoints.Count < 2 ||
                this.editPoints[0].Type != EditPoint.EditPointTypes.ConnectionEditPoint ||
                this.editPoints[this.editPoints.Count - 1].Type != EditPoint.EditPointTypes.ConnectionEditPoint)
            {
                Debug.Assert(false);
                return;
            }

            //STEP1: First we delete all the midsegmentpoints except the one which is being edited for simplicity
            RemoveEditPoints(EditPoint.EditPointTypes.MidSegmentEditPoint);

            //STEP2: Update points as per the type of edit point
            if (this.activeEditPoint != null)
            {
                int activeEditPointIndex = this.editPoints.IndexOf(this.activeEditPoint);
                EditPoint previous = (activeEditPointIndex > 0) ? this.editPoints[activeEditPointIndex - 1] : null;
                EditPoint next = (activeEditPointIndex < this.editPoints.Count - 1) ? this.editPoints[activeEditPointIndex + 1] : null;

                //Note that extra edit points are only added if we are connected to connection point
                if (previous != null && previous.Type == EditPoint.EditPointTypes.ConnectionEditPoint)
                {
                    float slopeOfLine = DesignerGeometryHelper.SlopeOfLineSegment(previous.Location, this.activeEditPoint.Location);
                    Orientation orientation = (Math.Abs(slopeOfLine) < 1) ? Orientation.Horizontal : Orientation.Vertical;

                    int editPointOffset = Convert.ToInt32(DesignerGeometryHelper.DistanceBetweenPoints(previous.Location, (next != null) ? next.Location : this.activeEditPoint.Location)) / 4;
                    if (orientation == Orientation.Horizontal)
                        editPointOffset *= (previous.Location.X < this.activeEditPoint.Location.X) ? 1 : -1;
                    else
                        editPointOffset *= (previous.Location.Y < this.activeEditPoint.Location.X) ? 1 : -1;

                    activeEditPointIndex = this.editPoints.IndexOf(this.activeEditPoint);
                    Point editPointLocation = (orientation == Orientation.Horizontal) ? new Point(previous.Location.X + editPointOffset, previous.Location.Y) : new Point(previous.Location.X, previous.Location.Y + editPointOffset);
                    previous = new EditPoint(this, EditPoint.EditPointTypes.MultiSegmentEditPoint, editPointLocation);
                    this.editPoints.InsertRange(activeEditPointIndex, new EditPoint[] { new EditPoint(this, EditPoint.EditPointTypes.MultiSegmentEditPoint, editPointLocation), previous });
                }

                if (next != null && next.Type == EditPoint.EditPointTypes.ConnectionEditPoint)
                {
                    float slopeOfLine = DesignerGeometryHelper.SlopeOfLineSegment(this.activeEditPoint.Location, next.Location);
                    Orientation orientation = (Math.Abs(slopeOfLine) < 1) ? Orientation.Horizontal : Orientation.Vertical;

                    int editPointOffset = Convert.ToInt32(DesignerGeometryHelper.DistanceBetweenPoints((previous != null) ? previous.Location : this.activeEditPoint.Location, next.Location)) / 4;
                    if (orientation == Orientation.Horizontal)
                        editPointOffset *= (this.activeEditPoint.Location.X < next.Location.X) ? -1 : 1;
                    else
                        editPointOffset *= (this.activeEditPoint.Location.Y < next.Location.Y) ? -1 : 1;

                    activeEditPointIndex = this.editPoints.IndexOf(this.activeEditPoint);
                    Point editPointLocation = (orientation == Orientation.Horizontal) ? new Point(next.Location.X + editPointOffset, next.Location.Y) : new Point(next.Location.X, next.Location.Y + editPointOffset);
                    next = new EditPoint(this, EditPoint.EditPointTypes.MultiSegmentEditPoint, editPointLocation);
                    this.editPoints.InsertRange(activeEditPointIndex + 1, new EditPoint[] { next, new EditPoint(this, EditPoint.EditPointTypes.MultiSegmentEditPoint, editPointLocation) });
                }

                //STEP2: UPDATE THE EDIT POINTS
                if (this.activeEditPoint.Type == EditPoint.EditPointTypes.ConnectionEditPoint)
                {
                    this.activeEditPoint.Location = newPoint;

                    //When we start editing the end point we need to clear the slate and start over
                    RemoveEditPoints(EditPoint.EditPointTypes.MultiSegmentEditPoint);

                    object source = null, target = null;
                    if (this.activeEditPoint.EditedConnectionPoint.Equals(Target))
                    {
                        target = newPoint;
                        source = Source;
                    }
                    else
                    {
                        source = newPoint;
                        target = Target;
                    }

                    int newEditPointIndex = (this.editPoints.Count == 2) ? 1 : 0;
                    List<EditPoint> newEditPoints = new List<EditPoint>();
                    Point[] points = ActivityDesignerConnectorRouter.Route(this.serviceProvider, source, target, this.editedConnector.ExcludedRoutingRectangles);
                    for (int i = newEditPointIndex; i < points.Length - newEditPointIndex; i++)
                        newEditPoints.Add(new EditPoint(this, EditPoint.EditPointTypes.MultiSegmentEditPoint, points[i]));
                    this.editPoints.InsertRange(1, newEditPoints.ToArray());
                }
                else if (this.activeEditPoint.Type == EditPoint.EditPointTypes.MultiSegmentEditPoint)
                {
                    if (previous != null && previous.Type != EditPoint.EditPointTypes.ConnectionEditPoint && next != null && next.Type != EditPoint.EditPointTypes.ConnectionEditPoint)
                    {
                        //Update the previous point
                        float slopeOfLine = DesignerGeometryHelper.SlopeOfLineSegment(previous.Location, this.activeEditPoint.Location);
                        Orientation orientation = (Math.Abs(slopeOfLine) < 1) ? Orientation.Horizontal : Orientation.Vertical;
                        previous.Location = (orientation == Orientation.Horizontal) ? new Point(previous.Location.X, newPoint.Y) : new Point(newPoint.X, previous.Location.Y);

                        //Update the next point
                        slopeOfLine = DesignerGeometryHelper.SlopeOfLineSegment(this.activeEditPoint.Location, next.Location);
                        orientation = (Math.Abs(slopeOfLine) < 1) ? Orientation.Horizontal : Orientation.Vertical;
                        next.Location = (orientation == Orientation.Horizontal) ? new Point(next.Location.X, newPoint.Y) : new Point(newPoint.X, next.Location.Y);

                        //Update the current point
                        this.activeEditPoint.Location = newPoint;
                    }
                    else
                    {
                        Debug.Assert(false);
                    }
                }
                else if (this.activeEditPoint.Type == EditPoint.EditPointTypes.MidSegmentEditPoint)
                {
                    if (previous != null && previous.Type != EditPoint.EditPointTypes.ConnectionEditPoint && next != null && next.Type != EditPoint.EditPointTypes.ConnectionEditPoint)
                    {
                        float slopeOfLine = DesignerGeometryHelper.SlopeOfLineSegment(previous.Location, next.Location);
                        Orientation orientation = (Math.Abs(slopeOfLine) < 1) ? Orientation.Horizontal : Orientation.Vertical;

                        //If the orientation is horizontal then we need to move the points vertically else we need to move the points horizontally
                        if (orientation == Orientation.Horizontal)
                        {
                            previous.Location = new Point(previous.Location.X, newPoint.Y);
                            next.Location = new Point(next.Location.X, newPoint.Y);
                            this.activeEditPoint.Location = new Point(this.activeEditPoint.Location.X, newPoint.Y);
                        }
                        else
                        {
                            previous.Location = new Point(newPoint.X, previous.Location.Y);
                            next.Location = new Point(newPoint.X, next.Location.Y);
                            this.activeEditPoint.Location = new Point(newPoint.X, this.activeEditPoint.Location.Y);
                        }
                    }
                    else
                    {
                        Debug.Assert(false);
                    }
                }
            }

            //STEP3: Remove all the redundant edit points
            RemoveCoincidingEditPoints();

            //STEP4: Add back the segment mid points
            AddEditPoints(EditPoint.EditPointTypes.MidSegmentEditPoint);

            bool validEditPoints = ValidateEditPoints();
            Debug.Assert(validEditPoints);
        }

        //Add edit points of specified type
        private void AddEditPoints(EditPoint.EditPointTypes editPointType)
        {
            if (editPointType == EditPoint.EditPointTypes.ConnectionEditPoint)
            {
                if (this.editPoints.Count == 0 || !this.editPoints[0].EditedConnectionPoint.Equals(Source))
                    this.editPoints.Insert(0, new EditPoint(this, Source));

                if (this.editPoints.Count < 2 || !this.editPoints[this.editPoints.Count - 1].EditedConnectionPoint.Equals(Target))
                    editPoints.Add(new EditPoint(this, Target));
            }
            else if (editPointType == EditPoint.EditPointTypes.MidSegmentEditPoint)
            {
                int minLengthForSegmentEditPoint = Source.Bounds.Width * 4;
                for (int i = 0; i < this.editPoints.Count - 1; i++)
                {
                    if (this.editPoints[i].Type != EditPoint.EditPointTypes.MidSegmentEditPoint &&
                        this.editPoints[i + 1].Type != EditPoint.EditPointTypes.MidSegmentEditPoint &&
                        DesignerGeometryHelper.DistanceOfLineSegments(new Point[] { this.editPoints[i].Location, this.editPoints[i + 1].Location }) > minLengthForSegmentEditPoint)
                    {
                        Point midPoint = DesignerGeometryHelper.MidPointOfLineSegment(this.editPoints[i].Location, this.editPoints[i + 1].Location);
                        this.editPoints.Insert(i + 1, new EditPoint(this, EditPoint.EditPointTypes.MidSegmentEditPoint, midPoint));
                    }
                }
            }
            else if (editPointType == EditPoint.EditPointTypes.MultiSegmentEditPoint)
            {
                if (this.editPoints.Count == 2)
                {
                    List<Point> segments = new List<Point>(this.editedConnector.ConnectorSegments);
                    if (segments.Count > 0 && segments[0] == Source.Location)
                        segments.RemoveAt(0);
                    if (segments.Count > 0 && segments[segments.Count - 1] == Target.Location)
                        segments.RemoveAt(segments.Count - 1);

                    List<EditPoint> editPointsToAdd = new List<EditPoint>();
                    for (int i = 0; i < segments.Count; i++)
                        editPointsToAdd.Add(new EditPoint(this, EditPoint.EditPointTypes.MultiSegmentEditPoint, segments[i]));
                    this.editPoints.InsertRange(this.editPoints.Count - 1, editPointsToAdd.ToArray());
                }
                else
                {
                    Debug.Assert(false);
                }
            }
        }

        //Remove edit points of specified type
        private void RemoveEditPoints(EditPoint.EditPointTypes editPointType)
        {
            List<EditPoint> editPointsToRemove = new List<EditPoint>();
            for (int i = 0; i < this.editPoints.Count; i++)
            {
                EditPoint editPoint = this.editPoints[i];
                if (editPoint.Type == editPointType)
                    editPointsToRemove.Add(editPoint);
            }

            for (int i = 0; i < editPointsToRemove.Count; i++)
            {
                EditPoint editPoint = editPointsToRemove[i];
                if (editPoint != this.activeEditPoint)
                    this.editPoints.Remove(editPoint);
            }
        }

        //Remove points with same slope
        private void RemoveCoincidingEditPoints()
        {
            if (this.editPoints.Count < 2 ||
                this.editPoints[0].Type != EditPoint.EditPointTypes.ConnectionEditPoint ||
                this.editPoints[this.editPoints.Count - 1].Type != EditPoint.EditPointTypes.ConnectionEditPoint ||
                (this.activeEditPoint != null && this.activeEditPoint.Type == EditPoint.EditPointTypes.ConnectionEditPoint))
            {
                return;
            }

            //Just make sure that there are no mid segmment edit points or algorithm will fail
            RemoveEditPoints(EditPoint.EditPointTypes.MidSegmentEditPoint);

            //Create list of points to retain
            List<EditPoint> editPointsToRetain = new List<EditPoint>();
            for (int i = 0; i < this.editPoints.Count; i++)
            {
                if (this.editPoints[i].Type != EditPoint.EditPointTypes.MultiSegmentEditPoint ||
                    this.editPoints[i] == this.activeEditPoint ||
                    (i > 0 && this.editPoints[i - 1].Type == EditPoint.EditPointTypes.MidSegmentEditPoint) ||
                    (i < this.editPoints.Count - 1 && this.editPoints[i + 1].Type == EditPoint.EditPointTypes.MidSegmentEditPoint))
                {
                    editPointsToRetain.Add(this.editPoints[i]);
                }
            }

            //Step1: Get rid of all the line segments which are within tolerance range
            for (int i = 1; i < this.editPoints.Count - 1; i++)
            {
                EditPoint previous = this.editPoints[i - 1];
                EditPoint current = this.editPoints[i];
                EditPoint next = this.editPoints[i + 1];

                if (!editPointsToRetain.Contains(current))
                {
                    double distance = DesignerGeometryHelper.DistanceOfLineSegments(new Point[] { previous.Location, current.Location });
                    if ((distance < current.Bounds.Width || distance < current.Bounds.Height) && next.Type == EditPoint.EditPointTypes.MultiSegmentEditPoint)
                    {
                        float slope = DesignerGeometryHelper.SlopeOfLineSegment(current.Location, next.Location);
                        next.Location = (slope < 1) ? new Point(next.Location.X, previous.Location.Y) : new Point(previous.Location.X, next.Location.Y);
                        this.editPoints.Remove(current);
                        i -= 1;
                    }
                    else
                    {
                        distance = DesignerGeometryHelper.DistanceOfLineSegments(new Point[] { current.Location, next.Location });
                        if ((distance < current.Bounds.Width || distance < current.Bounds.Height) && previous.Type == EditPoint.EditPointTypes.MultiSegmentEditPoint)
                        {
                            float slope = DesignerGeometryHelper.SlopeOfLineSegment(previous.Location, current.Location);
                            previous.Location = (slope < 1) ? new Point(previous.Location.X, next.Location.Y) : new Point(next.Location.X, previous.Location.Y);
                            this.editPoints.Remove(current);
                            i -= 1;
                        }
                    }
                }
            }

            //Step2: We should make sure that the active edit point is always retained but those points which are coincidental are always removed
            for (int i = 1; i < this.editPoints.Count - 1; i++)
            {
                EditPoint current = this.editPoints[i];
                EditPoint previous = this.editPoints[i - 1];
                EditPoint next = this.editPoints[i + 1];

                if (!editPointsToRetain.Contains(current))
                {
                    float slope1 = DesignerGeometryHelper.SlopeOfLineSegment(previous.Location, current.Location);
                    float slope2 = DesignerGeometryHelper.SlopeOfLineSegment(current.Location, next.Location);
                    if (Math.Abs(slope1) == Math.Abs(slope2))
                    {
                        this.editPoints.Remove(current);
                        i -= 1;
                    }
                }
            }

            //Step3: Go thorugh each segment and ensure that there all the segments are either vertical or horizontal
            for (int i = 0; i < this.editPoints.Count - 1; i++)
            {
                EditPoint current = this.editPoints[i];
                EditPoint next = this.editPoints[i + 1];

                float slope = DesignerGeometryHelper.SlopeOfLineSegment(current.Location, next.Location);
                if (slope != 0 && slope != float.MaxValue)
                {
                    Point location = (slope < 1) ? new Point(next.Location.X, current.Location.Y) : new Point(current.Location.X, next.Location.Y);
                    this.editPoints.Insert(i + 1, new EditPoint(this, EditPoint.EditPointTypes.MultiSegmentEditPoint, location));
                }
            }
        }

        private bool ValidateEditPoints()
        {
            if (this.editPoints.Count < 2)
                return false;

            ConnectionPoint sourceConnection = this.editPoints[0].EditedConnectionPoint;
            if (sourceConnection == null || !sourceConnection.Equals(Source))
                return false;

            ConnectionPoint targetConnection = this.editPoints[this.editPoints.Count - 1].EditedConnectionPoint;
            if (targetConnection == null || !targetConnection.Equals(Target))
                return false;

            for (int i = 0; i < this.editPoints.Count - 1; i++)
            {
                if (this.editPoints[i].Type == EditPoint.EditPointTypes.MidSegmentEditPoint &&
                    this.editPoints[i + 1].Type == EditPoint.EditPointTypes.MidSegmentEditPoint)
                    return false;
            }

            return true;
        }
        #endregion

        private List<Point> GetPointsFromEditPoints(List<EditPoint> editPoints)
        {
            List<Point> segments = new List<Point>();

            for (int i = 0; i < editPoints.Count; i++)
            {
                EditPoint editPoint = editPoints[i];
                if (editPoint.Type == EditPoint.EditPointTypes.ConnectionEditPoint ||
                    editPoint.Type == EditPoint.EditPointTypes.MultiSegmentEditPoint)
                    segments.Add(editPoint.Location);
            }

            return segments;
        }
        #endregion

        #region Class EditPoint
        private sealed class EditPoint
        {
            [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
            public enum EditPointTypes { ConnectionEditPoint = 1, MultiSegmentEditPoint, MidSegmentEditPoint };

            private ConnectorEditor owner;
            private EditPointTypes editPointType;
            private Point point;
            private ConnectionPoint connectionPoint;

            public EditPoint(ConnectorEditor owner, EditPointTypes editPointType, Point point)
            {
                this.owner = owner;
                this.editPointType = editPointType;
                this.point = point;
            }

            public EditPoint(ConnectorEditor owner, ConnectionPoint connectionPoint)
            {
                this.owner = owner;
                this.editPointType = EditPointTypes.ConnectionEditPoint;
                this.connectionPoint = connectionPoint;
                this.point = connectionPoint.Location;
            }

            public EditPointTypes Type
            {
                get
                {
                    return this.editPointType;
                }
            }

            public Point Location
            {
                get
                {
                    return this.point;
                }

                set
                {
                    this.point = value;
                }
            }

            public Rectangle Bounds
            {
                get
                {
                    Size controlPointSize = this.owner.Source.Bounds.Size;
                    return new Rectangle(this.point.X - controlPointSize.Width / 2, this.point.Y - controlPointSize.Height / 2, controlPointSize.Width, controlPointSize.Height);
                }
            }

            public ConnectionPoint EditedConnectionPoint
            {
                get
                {
                    return this.connectionPoint;
                }
            }
        }
        #endregion
    }
    #endregion

    #region Class ActivityDesignerConnectorRouter
    internal static class ActivityDesignerConnectorRouter
    {
        public static Point[] Route(IServiceProvider serviceProvider, object source, object target, ICollection<Rectangle> userDefinedRoutingObstacles)
        {
            //Now call routing algorithm
            List<Rectangle> rectanglesToExclude;
            List<Point> linesToExclude, pointsToExclude;
            ActivityDesignerConnectorRouter.GetRoutingObstacles(serviceProvider, source, target, out rectanglesToExclude, out linesToExclude, out pointsToExclude);

            if (userDefinedRoutingObstacles != null)
                rectanglesToExclude.AddRange(userDefinedRoutingObstacles);

            ActivityDesigner rootDesigner = ActivityDesigner.GetSafeRootDesigner(serviceProvider);
            AmbientTheme ambientTheme = WorkflowTheme.CurrentTheme.AmbientTheme;
            Point sourcePoint = (source is ConnectionPoint) ? ((ConnectionPoint)source).Location : (Point)source;
            Point targetPoint = (target is ConnectionPoint) ? ((ConnectionPoint)target).Location : (Point)target;

            Point[] routedPoints = ConnectorRouter.Route(sourcePoint, targetPoint, new Size(2 * ambientTheme.Margin.Width, 2 * ambientTheme.Margin.Height), rootDesigner.Bounds, rectanglesToExclude.ToArray(), linesToExclude.ToArray(), pointsToExclude.ToArray());

            //



            if (!AreAllSegmentsVerticalOrHorizontal(routedPoints))
                routedPoints = ConnectorRouter.Route(sourcePoint, targetPoint, ambientTheme.Margin, rootDesigner.Bounds, new Rectangle[] { }, linesToExclude.ToArray(), new Point[] { });

            //FALLBACK1
            if (!AreAllSegmentsVerticalOrHorizontal(routedPoints))
            {
                float slope = DesignerGeometryHelper.SlopeOfLineSegment(sourcePoint, targetPoint);
                Point intermediatePoint = (slope < 1) ? new Point(targetPoint.X, sourcePoint.Y) : new Point(sourcePoint.X, targetPoint.Y);
                routedPoints = new Point[] { sourcePoint, intermediatePoint, targetPoint };
            }

            return routedPoints;
        }

        public static void GetRoutingObstacles(IServiceProvider serviceProvider, object source, object target, out List<Rectangle> rectanglesToExclude, out List<Point> linesToExclude, out List<Point> pointsToExclude)
        {
            //Source or Targets can be either ConnectionPoint or a Point
            AmbientTheme ambientTheme = WorkflowTheme.CurrentTheme.AmbientTheme;
            ActivityDesigner rootDesigner = ActivityDesigner.GetSafeRootDesigner(serviceProvider);

            ConnectionPoint sourceConnectionPoint = source as ConnectionPoint;
            Point sourcePoint = (sourceConnectionPoint != null) ? sourceConnectionPoint.Location : (Point)source;
            ActivityDesigner sourceDesigner = (sourceConnectionPoint != null) ? sourceConnectionPoint.AssociatedDesigner : rootDesigner.HitTest(sourcePoint).AssociatedDesigner;

            ConnectionPoint targetConnectionPoint = target as ConnectionPoint;
            Point targetPoint = (targetConnectionPoint != null) ? targetConnectionPoint.Location : (Point)target;
            ActivityDesigner targetDesigner = (targetConnectionPoint != null) ? targetConnectionPoint.AssociatedDesigner : rootDesigner.HitTest(targetPoint).AssociatedDesigner;

            //Collect the common parent chain of source and target
            Dictionary<int, ActivityDesigner> parentDesignerDictionary = new Dictionary<int, ActivityDesigner>();
            if (sourceDesigner != null)
            {
                //Collect designers in the source parent chain
                CompositeActivityDesigner parentDesigner = sourceDesigner.ParentDesigner;
                while (parentDesigner != null)
                {
                    if (!parentDesignerDictionary.ContainsKey(parentDesigner.GetHashCode()))
                        parentDesignerDictionary.Add(parentDesigner.GetHashCode(), parentDesigner);
                    else
                        break;
                    parentDesigner = parentDesigner.ParentDesigner;
                }
            }

            if (targetDesigner != null)
            {
                //Collect designer from target chain which are not in source chain
                CompositeActivityDesigner parentDesigner = targetDesigner.ParentDesigner;
                while (parentDesigner != null)
                {
                    if (!parentDesignerDictionary.ContainsKey(parentDesigner.GetHashCode()))
                        parentDesignerDictionary.Add(parentDesigner.GetHashCode(), parentDesigner);
                    else
                        break;
                    parentDesigner = parentDesigner.ParentDesigner;
                }
            }

            //Now go through the dictionary and add all the children that are not in the chain
            rectanglesToExclude = new List<Rectangle>();
            pointsToExclude = new List<Point>();
            foreach (CompositeActivityDesigner parentDesigner in parentDesignerDictionary.Values)
            {
                ReadOnlyCollection<ActivityDesigner> containedDesigners = parentDesigner.ContainedDesigners;
                for (int j = 0; j < containedDesigners.Count; j++)
                {
                    ActivityDesigner activityDesigner = containedDesigners[j];
                    if (activityDesigner.IsVisible &&
                        !parentDesignerDictionary.ContainsKey(activityDesigner.GetHashCode()) &&
                        activityDesigner != sourceDesigner &&
                        activityDesigner != targetDesigner)
                    {
                        Rectangle rectangleToExclude = activityDesigner.Bounds;
                        rectangleToExclude.Inflate(ambientTheme.Margin);
                        rectanglesToExclude.Add(rectangleToExclude);
                    }
                }

                //







            }

            //Now get the exclusion paths for source designer and target designer
            linesToExclude = new List<Point>();
            if (sourceDesigner != null && sourceDesigner == targetDesigner && !sourceDesigner.IsRootDesigner)
            {
                linesToExclude.AddRange(GetDesignerEscapeCover(sourceDesigner, new object[] { source, target }));
            }
            else
            {
                if (sourceDesigner != null && !sourceDesigner.IsRootDesigner)
                    linesToExclude.AddRange(GetDesignerEscapeCover(sourceDesigner, new object[] { source }));

                if (targetDesigner != null && !targetDesigner.IsRootDesigner)
                {
                    bool needToEscapeTargetDesigner = true;

                    CompositeActivityDesigner sourceParentDesigner = (sourceDesigner != null) ? sourceDesigner.ParentDesigner : null;
                    while (sourceParentDesigner != null)
                    {
                        if (targetDesigner == sourceParentDesigner)
                        {
                            needToEscapeTargetDesigner = false;
                            break;
                        }
                        sourceParentDesigner = (sourceDesigner != null) ? sourceParentDesigner.ParentDesigner : null;
                    }

                    //need to escape the target designer only if the source is not inside the parent
                    if (needToEscapeTargetDesigner)
                        linesToExclude.AddRange(GetDesignerEscapeCover(targetDesigner, new object[] { target }));
                }
            }
        }

        private static IList<Point> GetDesignerEscapeCover(ActivityDesigner designer, ICollection<object> escapeLocations)
        {
            Rectangle bounds = designer.Bounds;

            //Create dictionary of escape points
            Dictionary<DesignerEdges, List<Point>> escapeEdges = new Dictionary<DesignerEdges, List<Point>>();
            foreach (object escapeLocation in escapeLocations)
            {
                DesignerEdges escapeEdge = DesignerEdges.None;
                Point escapePoint = Point.Empty;
                if (escapeLocation is ConnectionPoint)
                {
                    escapeEdge = ((ConnectionPoint)escapeLocation).ConnectionEdge;
                    escapePoint = ((ConnectionPoint)escapeLocation).Location;
                }
                else if (escapeLocation is Point)
                {
                    escapePoint = (Point)escapeLocation;
                    escapeEdge = DesignerGeometryHelper.ClosestEdgeToPoint((Point)escapeLocation, bounds, DesignerEdges.All);
                }

                if (escapeEdge != DesignerEdges.None)
                {
                    List<Point> escapePoints = null;
                    if (!escapeEdges.ContainsKey(escapeEdge))
                    {
                        escapePoints = new List<Point>();
                        escapeEdges.Add(escapeEdge, escapePoints);
                    }
                    else
                    {
                        escapePoints = escapeEdges[escapeEdge];
                    }

                    escapePoints.Add(escapePoint);
                }
            }

            //Create a dictionary of four edges of the designer which will form cover which need to be escaped            
            Size margin = WorkflowTheme.CurrentTheme.AmbientTheme.Margin;
            bounds.Inflate(margin);
            Dictionary<DesignerEdges, Point[]> designerEdgeCover = new Dictionary<DesignerEdges, Point[]>();
            designerEdgeCover.Add(DesignerEdges.Left, new Point[] { new Point(bounds.Left, bounds.Top), new Point(bounds.Left, bounds.Bottom) });
            designerEdgeCover.Add(DesignerEdges.Top, new Point[] { new Point(bounds.Left, bounds.Top), new Point(bounds.Right, bounds.Top) });
            designerEdgeCover.Add(DesignerEdges.Right, new Point[] { new Point(bounds.Right, bounds.Top), new Point(bounds.Right, bounds.Bottom) });
            designerEdgeCover.Add(DesignerEdges.Bottom, new Point[] { new Point(bounds.Left, bounds.Bottom), new Point(bounds.Right, bounds.Bottom) });

            List<Point> linesToExclude = new List<Point>();
            foreach (DesignerEdges designerEdge in designerEdgeCover.Keys)
            {
                if (escapeEdges.ContainsKey(designerEdge))
                {
                    Point[] edge = designerEdgeCover[designerEdge];
                    List<Point> escapePoints = escapeEdges[designerEdge];
                    List<Point> escapeCover = new List<Point>();
                    if (designerEdge == DesignerEdges.Left)
                    {
                        escapeCover.Add(new Point(edge[0].X, edge[0].Y));
                        for (int k = 0; k < escapePoints.Count; k++)
                        {
                            Point escapePoint = escapePoints[k];
                            if (escapePoint.X > edge[0].X && escapePoint.Y > edge[0].Y && escapePoint.Y < edge[1].Y)
                            {
                                escapeCover.Add(new Point(edge[0].X, escapePoint.Y - 1));
                                escapeCover.Add(new Point(escapePoint.X + 1, escapePoint.Y - 1));
                                escapeCover.Add(new Point(escapePoint.X + 1, escapePoint.Y + 1));
                                escapeCover.Add(new Point(edge[0].X, escapePoint.Y + 1));
                            }
                        }
                        escapeCover.Add(new Point(edge[0].X, edge[1].Y));
                    }
                    else if (designerEdge == DesignerEdges.Right)
                    {
                        escapeCover.Add(new Point(edge[0].X, edge[0].Y));
                        for (int k = 0; k < escapePoints.Count; k++)
                        {
                            Point escapePoint = escapePoints[k];
                            if (escapePoint.X < edge[0].X && escapePoint.Y > edge[0].Y && escapePoint.Y < edge[1].Y)
                            {
                                escapeCover.Add(new Point(edge[0].X, escapePoint.Y - 1));
                                escapeCover.Add(new Point(escapePoint.X - 1, escapePoint.Y - 1));
                                escapeCover.Add(new Point(escapePoint.X - 1, escapePoint.Y + 1));
                                escapeCover.Add(new Point(edge[0].X, escapePoint.Y + 1));
                            }
                        }
                        escapeCover.Add(new Point(edge[0].X, edge[1].Y));
                    }
                    else if (designerEdge == DesignerEdges.Top)
                    {
                        escapeCover.Add(new Point(edge[0].X, edge[0].Y));
                        for (int k = 0; k < escapePoints.Count; k++)
                        {
                            Point escapePoint = escapePoints[k];
                            if (escapePoint.Y > edge[0].Y && escapePoint.X > edge[0].X && escapePoint.X < edge[1].X)
                            {
                                escapeCover.Add(new Point(escapePoint.X - 1, edge[0].Y));
                                escapeCover.Add(new Point(escapePoint.X - 1, escapePoint.Y + 1));
                                escapeCover.Add(new Point(escapePoint.X + 1, escapePoint.Y + 1));
                                escapeCover.Add(new Point(escapePoint.X + 1, edge[0].Y));
                            }
                        }
                        escapeCover.Add(new Point(edge[1].X, edge[0].Y));
                    }
                    else if (designerEdge == DesignerEdges.Bottom)
                    {
                        //Add edge so that the connector does not pass through
                        escapeCover.Add(new Point(edge[0].X, edge[0].Y));
                        for (int k = 0; k < escapePoints.Count; k++)
                        {
                            Point escapePoint = escapePoints[k];
                            if (escapePoint.Y < edge[0].Y && escapePoint.X > edge[0].X && escapePoint.X < edge[1].X)
                            {
                                escapeCover.Add(new Point(escapePoint.X - 1, edge[0].Y));
                                escapeCover.Add(new Point(escapePoint.X - 1, escapePoint.Y - 1));
                                escapeCover.Add(new Point(escapePoint.X + 1, escapePoint.Y - 1));
                                escapeCover.Add(new Point(escapePoint.X + 1, edge[0].Y));
                            }
                        }
                        escapeCover.Add(new Point(edge[1].X, edge[0].Y));
                    }

                    for (int i = 1; i < escapeCover.Count; i++)
                    {
                        linesToExclude.Add(escapeCover[i - 1]);
                        linesToExclude.Add(escapeCover[i]);
                    }
                }
                else
                {
                    linesToExclude.AddRange(designerEdgeCover[designerEdge]);
                }
            }

            return linesToExclude.AsReadOnly();
        }

        private static bool AreAllSegmentsVerticalOrHorizontal(Point[] segments)
        {
            if (segments == null || segments.Length == 0)
                return false;

            for (int i = 1; i < segments.Length; i++)
            {
                if (segments[i - 1].X != segments[i].X && segments[i - 1].Y != segments[i].Y)
                    return false;
            }

            return true;
        }
    }
    #endregion

    #region Class ConnectorRouter
    internal static class ConnectorRouter
    {
        private static readonly Size DefaultSize = new Size(4, 4);

        public static Point[] Route(Point begin, Point end, Size margin, Rectangle enclosingRectangle, Rectangle[] rectanglesToExclude, Point[] linesToExclude, Point[] pointsToExclude)
        {
            List<Rectangle> excludedRectangles = new List<Rectangle>(rectanglesToExclude);
            if (!enclosingRectangle.IsEmpty)
            {
                begin.X = Math.Min(Math.Max(begin.X, enclosingRectangle.Left + 1), enclosingRectangle.Right - 1);
                begin.Y = Math.Min(Math.Max(begin.Y, enclosingRectangle.Top + 1), enclosingRectangle.Bottom - 1);

                excludedRectangles.Insert(0, enclosingRectangle);
            }

            List<Point> excludedLines = new List<Point>(linesToExclude);
            int width = Math.Max(margin.Width / 2, 1), height = Math.Max(margin.Height / 2, 1);
            foreach (Point point in pointsToExclude)
            {
                excludedLines.Add(new Point(point.X - width, point.Y));
                excludedLines.Add(new Point(point.X + width, point.Y));

                excludedLines.Add(new Point(point.X, point.Y - height));
                excludedLines.Add(new Point(point.X, point.Y + height));
            }

            return GetRoutedLineSegments(begin, end, margin, excludedRectangles.ToArray(), excludedLines.ToArray());
        }

        private static Point[] GetRoutedLineSegments(Point begin, Point end, Size margin, Rectangle[] rectanglesToExclude, Point[] linesToExclude)
        {
            if (rectanglesToExclude == null)
                throw new ArgumentNullException("rectanglesToExclude");

            if (linesToExclude == null)
                throw new ArgumentNullException("linesToExclude");

            if ((linesToExclude.Length % 2) > 0)
                throw new ArgumentException(DR.GetString(DR.Error_Connector2));

            CoverSet coverSet = new CoverSet(rectanglesToExclude, linesToExclude);
            coverSet.ClearUsedLines();

            Point A = begin;
            Point B = end;

            //escape points
            List<Point> LeA = new List<Point>(); //escape points from begin to end
            List<Point> LeB = new List<Point>(); //escape points from end to begin

            //horizontal/vertical escape segments from A
            List<ConnectorSegment> LhA = new List<ConnectorSegment>();
            List<ConnectorSegment> LvA = new List<ConnectorSegment>();

            //horizontal/vertical escape segments from B
            List<ConnectorSegment> LhB = new List<ConnectorSegment>();
            List<ConnectorSegment> LvB = new List<ConnectorSegment>();

            Orientation orientationA = Orientation.Horizontal;
            Orientation orientationB = Orientation.Horizontal;

            //P0
            LeA.Add(begin);
            LeB.Add(end);

            bool noEscapeA = false;
            bool noEscapeB = false;

            Nullable<Point> intersection = null;
            ConnectorSegment intersectionSegmentA = null;
            ConnectorSegment intersectionSegmentB = null;

            try
            {
                do
                {
                    //P1
                    if (noEscapeA)
                    {
                        if (noEscapeB)
                        {
                            //we failed to find the point
                            break;
                        }
                        else
                        {
                            //P2
                            #region swap A and B (with all appropriate lists and flags...)
                            List<Point> tempList = LeA;
                            LeA = LeB;
                            LeB = tempList;

                            Point tempPoint = A;
                            A = B;
                            B = tempPoint;

                            bool tempBool = noEscapeA;
                            noEscapeA = noEscapeB;
                            noEscapeB = tempBool;

                            Orientation tempOrientation = orientationA;
                            orientationA = orientationB;
                            orientationB = tempOrientation;

                            List<ConnectorSegment> tempListSegm = LhA;
                            LhA = LhB;
                            LhB = tempListSegm;

                            tempListSegm = LvA;
                            LvA = LvB;
                            LvB = tempListSegm;
                            #endregion

                            continue;
                        }
                    }

                    Point objectPoint = LeA[LeA.Count - 1];
                    Point targetPoint = B;

                    intersection = EscapeAlgorithm(coverSet, objectPoint, targetPoint,
                        ref LeA, ref LhA, ref LvA, ref LhB, ref LvB, ref orientationA,
                        out intersectionSegmentA, out intersectionSegmentB, margin, ref noEscapeA);
                    if (intersection != null)
                    {
                        break;
                    }
                    else
                    {
                        #region swap A and B (with all appropriate lists and flags...)
                        List<Point> tempList = LeA;
                        LeA = LeB;
                        LeB = tempList;

                        Point tempPoint = A;
                        A = B;
                        B = tempPoint;

                        bool tempBool = noEscapeA;
                        noEscapeA = noEscapeB;
                        noEscapeB = tempBool;

                        Orientation tempOrientation = orientationA;
                        orientationA = orientationB;
                        orientationB = tempOrientation;

                        List<ConnectorSegment> tempListSegm = LhA;
                        LhA = LhB;
                        LhB = tempListSegm;

                        tempListSegm = LvA;
                        LvA = LvB;
                        LvB = tempListSegm;
                        #endregion
                    }

                } while (true);

                //we failed
                if (intersection == null)
                    return null;
                //{
                //    //






                //    return newPath.ToArray();
                //}

                List<Point> refinedPath = new List<Point>();

                #region P3 apply refinement algorithms
                //first refinement algorithm
                LeA = FirstRefinementAlgorithm(LeA, (Point)intersection, intersectionSegmentA);
                LeB = FirstRefinementAlgorithm(LeB, (Point)intersection, intersectionSegmentB);

                //before going into the second refinement, construct the full path
                for (int j = LeA.Count - 1; j >= 0; j--)
                    refinedPath.Add(LeA[j]);
                refinedPath.Add((Point)intersection);
                for (int j = 0; j < LeB.Count; j++)
                    refinedPath.Add(LeB[j]);

                //perform second refinement algorithm on the full path
                SecondRefinementAlgorithm(coverSet, ref refinedPath, margin);
                #endregion

                if (refinedPath.Count > 1 && refinedPath[refinedPath.Count - 1] == begin)
                    refinedPath.Reverse();

                return refinedPath.ToArray();
            }
            catch
            {
                //

                return null;
            }
        }

        #region Escape Algorithm
        private static Nullable<Point> EscapeAlgorithm(CoverSet coverSet, Point Z, Point targetPoint,
            ref List<Point> LeA, ref List<ConnectorSegment> LhA, ref List<ConnectorSegment> LvA, ref List<ConnectorSegment> LhB, ref List<ConnectorSegment> LvB,
            ref Orientation orientationA, out ConnectorSegment intersectionSegmentA, out ConnectorSegment intersectionSegmentB, Size margin, ref bool noEscapeA)
        {
            Nullable<Point> intersection = null;
            intersectionSegmentA = null;
            intersectionSegmentB = null;

            ConnectorSegment leftCover = coverSet.GetCover(Z, DesignerEdges.Left);
            ConnectorSegment rightCover = coverSet.GetCover(Z, DesignerEdges.Right);
            ConnectorSegment bottomCover = coverSet.GetCover(Z, DesignerEdges.Bottom);
            ConnectorSegment topCover = coverSet.GetCover(Z, DesignerEdges.Top);

            #region P1: construct escape line(s) - on the beginning of the algorithm it will create two lines
            ConnectorSegment h = ConnectorSegment.SegmentFromLeftToRightCover(coverSet, Z);
            LhA.Add(h);

            ConnectorSegment v = ConnectorSegment.SegmentFromBottomToTopCover(coverSet, Z);
            LvA.Add(v);
            #endregion

            #region P2 check if the new escape line(s) intersect with the existing ones
            for (int i = 0; i < LvB.Count; i++)
            {
                ConnectorSegment segment = LvB[i];
                intersection = h.Intersect(segment);
                if (intersection != null)
                {
                    intersectionSegmentA = h;
                    intersectionSegmentB = segment;
                    return intersection;
                }
            }

            for (int i = 0; i < LhB.Count; i++)
            {
                ConnectorSegment segment = LhB[i];
                intersection = v.Intersect(segment);
                if (intersection != null)
                {
                    intersectionSegmentA = v;
                    intersectionSegmentB = segment;
                    return intersection;
                }
            }
            #endregion

            //there was no intersection found above, continue on

            //P3 find an escape point

            #region Call Escape Process I
            //Escape process I P1
            Nullable<Point> escapePoint = EscapeProcessI(coverSet, Z, v, Orientation.Horizontal, margin);
            if (escapePoint != null)
            {
                orientationA = Orientation.Vertical;
                LeA.Add((Point)escapePoint);
                return null;
            }

            //Escape process I P2
            escapePoint = EscapeProcessI(coverSet, Z, h, Orientation.Vertical, margin);
            if (escapePoint != null)
            {
                orientationA = Orientation.Horizontal;
                LeA.Add((Point)escapePoint);
                return null;
            }
            #endregion

            #region Call Escape process II
            bool intersectionFlag = false;
            //flags indicating if we can still continue in the given directions
            bool continue1, continue2, continue3, continue4;
            Point r1 = Point.Empty, r2 = Point.Empty, r3 = Point.Empty, r4 = Point.Empty;

            if (topCover != null)
                r1 = new Point(Z.X, topCover.A.Y);
            if (rightCover != null)
                r2 = new Point(rightCover.A.X, Z.Y);
            if (bottomCover != null)
                r3 = new Point(Z.X, bottomCover.A.Y);
            if (leftCover != null)
                r4 = new Point(leftCover.A.X, Z.Y);

            do
            {
                continue1 = continue2 = continue3 = continue4 = false;
                if (topCover != null)
                {
                    r1.Y -= margin.Height;
                    if (r1.Y > Z.Y)
                    {
                        continue1 = true;
                        Nullable<Point> escape = EscapeProcessII(coverSet, Orientation.Vertical,
                            ref LeA, ref LhA, ref LvA, ref LhB, ref LvB, Z, r1, margin, out intersectionFlag, out intersectionSegmentA, out intersectionSegmentB);
                        if (escape != null)
                        {
                            LvA.Add(v);
                            if (intersectionFlag)
                                return escape;

                            orientationA = Orientation.Horizontal;
                            coverSet.AddUsedEscapeLine(new ConnectorSegment(Z, r1));
                            coverSet.AddUsedEscapeLine(new ConnectorSegment(r1, (Point)escape));
                            LeA.Add((Point)escape);
                            return null;
                        }
                    }
                }

                if (rightCover != null)
                {
                    r2.X -= margin.Width;
                    if (r2.X > Z.X)
                    {
                        continue2 = true;
                        Nullable<Point> escape = EscapeProcessII(coverSet, Orientation.Horizontal,
                            ref LeA, ref LhA, ref LvA, ref LhB, ref LvB, Z, r2, margin, out intersectionFlag, out intersectionSegmentA, out intersectionSegmentB);
                        if (escape != null)
                        {
                            LhA.Add(h);
                            if (intersectionFlag)
                                return escape;

                            orientationA = Orientation.Vertical;
                            coverSet.AddUsedEscapeLine(new ConnectorSegment(Z, r2));
                            coverSet.AddUsedEscapeLine(new ConnectorSegment(r2, (Point)escape));
                            LeA.Add((Point)escape);
                            return null;
                        }
                    }
                }

                if (bottomCover != null)
                {
                    r3.Y += margin.Height;
                    if (r3.Y < Z.Y)
                    {
                        continue3 = true;
                        Nullable<Point> escape = EscapeProcessII(coverSet, Orientation.Vertical,
                            ref LeA, ref LhA, ref LvA, ref LhB, ref LvB, Z, r3, margin, out intersectionFlag, out intersectionSegmentA, out intersectionSegmentB);
                        if (escape != null)
                        {
                            LvA.Add(v);
                            if (intersectionFlag)
                                return escape;

                            orientationA = Orientation.Horizontal;
                            coverSet.AddUsedEscapeLine(new ConnectorSegment(Z, r3));
                            coverSet.AddUsedEscapeLine(new ConnectorSegment(r3, (Point)escape));
                            LeA.Add((Point)escape);
                            return null;
                        }
                    }
                }

                if (leftCover != null)
                {
                    r4.X += margin.Width;
                    if (r4.X < Z.X)
                    {
                        continue4 = true;
                        Nullable<Point> escape = EscapeProcessII(coverSet, Orientation.Horizontal,
                            ref LeA, ref LhA, ref LvA, ref LhB, ref LvB, Z, r4, margin, out intersectionFlag, out intersectionSegmentA, out intersectionSegmentB);
                        if (escape != null)
                        {
                            LhA.Add(h);
                            if (intersectionFlag)
                                return escape;

                            orientationA = Orientation.Vertical;
                            coverSet.AddUsedEscapeLine(new ConnectorSegment(Z, r4));
                            coverSet.AddUsedEscapeLine(new ConnectorSegment(r4, (Point)escape));
                            LeA.Add((Point)escape);
                            return null;
                        }
                    }
                }
                //continue the loop until there is a place to go in either of the directions
            } while (continue1 || continue2 || continue3 || continue4);
            #endregion

            noEscapeA = true;
            return null;
        }

        #region Escape Process I
        private static Nullable<Point> EscapeProcessI(CoverSet coverSet, Point Z,
            ConnectorSegment escapeLine, Orientation orientation, Size margin)
        {
            //perform extremity point permutation by the distance from the object point
            //when sorting points store to which segments they belong (this is needed further when
            //deciding to side of the point to move if the extremity coincide with the object point
            //on abscissa or ordinata axis)
            List<DistanceFromPoint> extremitiesList = new List<DistanceFromPoint>(4); //at most four points

            ConnectorSegment lesserCover = coverSet.GetCover(Z, (orientation == Orientation.Horizontal) ? DesignerEdges.Left : DesignerEdges.Bottom);
            if (lesserCover != null)
            {
                AddBoundPoint(ref extremitiesList, lesserCover.A, lesserCover, Z);
                AddBoundPoint(ref extremitiesList, lesserCover.B, lesserCover, Z);
            }

            ConnectorSegment higherCover = coverSet.GetCover(Z, (orientation == Orientation.Horizontal) ? DesignerEdges.Right : DesignerEdges.Top);
            if (higherCover != null)
            {
                AddBoundPoint(ref extremitiesList, higherCover.A, higherCover, Z);
                AddBoundPoint(ref extremitiesList, higherCover.B, higherCover, Z);
            }

            if (extremitiesList.Count == 0)
                return null;

            DistanceSorter.Sort(ref extremitiesList);
            for (int i = 0; i < extremitiesList.Count; i++)
            {
                Point p = extremitiesList[i].P;
                Point direction = new Point(Math.Sign(p.X - Z.X), Math.Sign(p.Y - Z.Y));
                if (((orientation == Orientation.Vertical) ? direction.X : direction.Y) == 0)
                {
                    //if extremity is on the same abscissa(ordinata) line with the object point, need to be more careful
                    ConnectorSegment segment = extremitiesList[i].ConnectorSegment;
                    p = segment.ExtendPointOutwards(p);
                    direction = new Point(Math.Sign(p.X - Z.X), Math.Sign(p.Y - Z.Y));
                    p = extremitiesList[i].P;
                }

                DesignerEdges side; //which side is the cover we are trying to escape
                if (orientation == Orientation.Vertical)
                    //we are escaping either top or bottom
                    side = (direction.Y < 0) ? DesignerEdges.Bottom : DesignerEdges.Top;
                else
                    //we are escaping either left or right
                    side = (direction.X < 0) ? DesignerEdges.Left : DesignerEdges.Right;

                Point escapePoint;
                if ((orientation == Orientation.Vertical))
                    escapePoint = new Point(p.X + direction.X * margin.Width, Z.Y);
                else
                    escapePoint = new Point(Z.X, p.Y + direction.Y * margin.Height);

                //the new escape point should
                //1) lay on the given escape line (except the bounding points since they belong to covers)
                //2) not lay on any of the already tested escape segments - all points belonging to them are already worthless
                ConnectorSegment newEscapeLine = new ConnectorSegment(Z, escapePoint);
                if (!coverSet.EscapeLineHasBeenUsed(newEscapeLine, escapePoint) &&
                    escapeLine.IsPointOnSegment(escapePoint) && escapeLine.A != escapePoint && escapeLine.B != escapePoint &&
                    coverSet.IsEscapePoint(Z, escapePoint, side))
                {
                    coverSet.AddUsedEscapeLine(newEscapeLine);
                    return escapePoint;
                }
            }

            return null;
        }

        private static void AddBoundPoint(ref List<DistanceFromPoint> extremitiesList, Point p, ConnectorSegment segment, Point Z)
        {
            if (p.X != int.MinValue && p.X != int.MaxValue && p.Y != int.MinValue && p.Y != int.MaxValue)
                extremitiesList.Add(new DistanceFromPoint(segment, Z, p));
        }
        #endregion

        #region Escape Process II
        private static Nullable<Point> EscapeProcessII(CoverSet coverSet, Orientation orientation, ref List<Point> LeA,
            ref List<ConnectorSegment> LhA, ref List<ConnectorSegment> LvA, ref List<ConnectorSegment> LhB, ref List<ConnectorSegment> LvB,
            Point Z, Point R, Size margin, out bool intersectionFlag, out ConnectorSegment intersectionSegmentA, out ConnectorSegment intersectionSegmentB)
        {
            intersectionFlag = false;
            intersectionSegmentA = null;
            intersectionSegmentB = null;

            //rebuild invalidated covers
            ConnectorSegment h = ConnectorSegment.SegmentFromLeftToRightCover(coverSet, R);
            ConnectorSegment v = ConnectorSegment.SegmentFromBottomToTopCover(coverSet, R);

            //check intersections
            for (int i = 0; i < LvB.Count; i++)
            {
                ConnectorSegment segment = LvB[i];
                Nullable<Point> itersection = h.Intersect(segment);
                if (itersection != null)
                {
                    intersectionFlag = true;
                    intersectionSegmentA = h;
                    intersectionSegmentB = segment;
                    LeA.Add(R);
                    return itersection;
                }
            }
            for (int i = 0; i < LhB.Count; i++)
            {
                ConnectorSegment segment = LhB[i];
                Nullable<Point> itersection = v.Intersect(segment);
                if (itersection != null)
                {
                    intersectionFlag = true;
                    intersectionSegmentA = v;
                    intersectionSegmentB = segment;
                    LeA.Add(R);
                    return itersection;
                }
            }

            Nullable<Point> escapePointI = null;

            //now do both horizontal and vertical escape processes I from that point...
            //the order is important based on 'orientation' argument
            if (orientation == Orientation.Horizontal)
            {
                escapePointI = EscapeProcessI(coverSet, R, v, Orientation.Horizontal, margin);
                if (escapePointI != null)
                {
                    LvA.Add(v);
                    LeA.Add(R);
                    return escapePointI;
                }

                escapePointI = EscapeProcessI(coverSet, R, h, Orientation.Vertical, margin);
                if (escapePointI != null)
                {
                    LhA.Add(h);
                    LeA.Add(R);
                    return escapePointI;
                }
            }
            else
            {
                escapePointI = EscapeProcessI(coverSet, R, h, Orientation.Vertical, margin);
                if (escapePointI != null)
                {
                    LhA.Add(h);
                    LeA.Add(R);
                    return escapePointI;
                }

                escapePointI = EscapeProcessI(coverSet, R, v, Orientation.Horizontal, margin);
                if (escapePointI != null)
                {
                    LvA.Add(v);
                    LeA.Add(R);
                    return escapePointI;
                }
            }

            return null;
        }
        #endregion

        #endregion

        #region ConnectorSegment Refinement Algorithms
        //remove all non-corner points
        private static List<Point> FirstRefinementAlgorithm(List<Point> Le, Point intersection, ConnectorSegment intersectionSegment)
        {
            List<Point> refinedSet = new List<Point>();
            ConnectorSegment k = intersectionSegment;

            while (Le.Count > 0)
            {
                Point pi;
                int i = Le.Count - 1;

                //find the first point that lies on k
                while (!k.PointLiesOnThisLine(Le[i]) && i > 0)
                    i--;

                //find the least point that lies on k
                while (i > 0 && k.PointLiesOnThisLine(Le[i - 1]))
                    i--;

                //save the point
                pi = Le[i];
                refinedSet.Add(pi);

                //remove all the points up to the found one from the original list
                while (Le.Count > i)
                    Le.RemoveAt(i);

                //continue with the points lying on the line perpendecular to the previous one and passing through the 
                //found point
                k = k.PeprendecularThroughPoint(pi);
            }

            return refinedSet;
        }

        //remove superflous parts from the path
        private static void SecondRefinementAlgorithm(CoverSet coverSet, ref List<Point> refinedPath, Size margin)
        {
            List<Point> newPath = new List<Point>();

            #region Part I: extend every segment in the path to see if it intersects any other segment
            int currentSegment = 0;
            while (currentSegment < refinedPath.Count - 1)
            {
                Point a1 = refinedPath[currentSegment];
                Point a2 = refinedPath[currentSegment + 1];

                //need to construct a segment through the points that is limited by the covers
                ConnectorSegment a = ConnectorSegment.ConstructBoundSegment(coverSet, a1, a2);

                //try to intersect it with every segment after the current one 
                //and the next one (which we know does intersect with the current one)
                int intersectingSegment = currentSegment + 2;
                while (intersectingSegment < refinedPath.Count - 1)
                {
                    Point b1 = refinedPath[intersectingSegment];
                    Point b2 = refinedPath[intersectingSegment + 1];
                    ConnectorSegment b = ConnectorSegment.ConstructBoundSegment(coverSet, b1, b2);

                    Nullable<Point> intersection = a.Intersect(b);
                    if (intersection != null)
                    {
                        newPath.Clear();
                        //need to remove all points between a1 and b2 and insert the point of intersectin there
                        for (int i = 0; i <= currentSegment; i++)
                            newPath.Add(refinedPath[i]);
                        newPath.Add((Point)intersection);
                        for (int i = intersectingSegment + 1; i < refinedPath.Count; i++)
                            newPath.Add(refinedPath[i]);

                        List<Point> temp = refinedPath;
                        refinedPath = newPath;
                        newPath = temp;
                        newPath.Clear();

                        //reset the second segment number and go through all segments once more 
                        //until there are no intersections left
                        intersectingSegment = currentSegment + 2;
                    }
                    else
                    {
                        intersectingSegment++;
                    }
                }

                //we need to keep looking for intersections until we reach the very last one
                currentSegment++;
            }
            #endregion

            #region Part II: construct segments perpendecular to every segment and see if they intersect any other segment
            currentSegment = 0;
            while (currentSegment < refinedPath.Count - 1)
            {
                Point a1 = refinedPath[currentSegment];
                Point a2 = refinedPath[currentSegment + 1];

                //need to construct a segment through the points that is limited by the covers
                bool intersected = false;
                ConnectorSegment a = ConnectorSegment.ConstructBoundSegment(coverSet, a1, a2);
                if (a != null)
                {
                    Point direction = new Point(a2.X - a1.X, a2.Y - a1.Y);

                    //see how many intemediate points we can construct
                    int steps = Math.Max(Math.Abs(direction.X / margin.Width), Math.Abs(direction.Y / margin.Height)); //one of the values will be null
                    direction.X = Math.Sign(direction.X);
                    direction.Y = Math.Sign(direction.Y);

                    for (int i = 1; i <= steps; i++)
                    {
                        Point k = new Point(a1.X + i * margin.Width * direction.X, a1.Y + i * margin.Height * direction.Y);
                        if (k == a2)
                            break;

                        ConnectorSegment b = ConnectorSegment.ConstructBoundSegment(coverSet, k, (a.Orientation == Orientation.Horizontal) ? Orientation.Vertical : Orientation.Horizontal);
                        //now try to intersect that segment with every segment after the current one and the one after it
                        int intersectingSegment = currentSegment + 2;
                        while (intersectingSegment < refinedPath.Count - 1 && !intersected)
                        {
                            Point c1 = refinedPath[intersectingSegment];
                            Point c2 = refinedPath[intersectingSegment + 1];
                            ConnectorSegment c = new ConnectorSegment(c1, c2);

                            Nullable<Point> intersection = b.Intersect(c);
                            if (intersection != null && c.IsPointOnSegment((Point)intersection))
                            {
                                intersected = true;

                                newPath.Clear();
                                //need to remove all points between a1 and b2 and insert k and the point of intersection there
                                for (int j = 0; j <= currentSegment; j++)
                                    newPath.Add(refinedPath[j]);
                                newPath.Add(k);
                                newPath.Add((Point)intersection);
                                for (int j = intersectingSegment + 1; j < refinedPath.Count; j++)
                                    newPath.Add(refinedPath[j]);

                                List<Point> temp = refinedPath;
                                refinedPath = newPath;
                                newPath = temp;
                                newPath.Clear();
                                break;
                            }

                            intersectingSegment++;
                        }

                        //if there was an intersection, exit the for loop
                        if (intersected)
                            break;
                    }
                }

                //if there was an intersection, run the same segment again to see if there are other intersecting segments
                if (!intersected)
                    currentSegment++;
            }
            #endregion
        }
        #endregion

        #region Struct DistanceFromPoint
        //sorting of extremities by closedness to point Z
        private struct DistanceFromPoint
        {
            public ConnectorSegment ConnectorSegment;
            public Point P;
            public double Distance;

            public DistanceFromPoint(ConnectorSegment segment, Point z, Point p)
            {
                this.ConnectorSegment = segment;
                this.P = p;
                this.Distance = ConnectorSegment.DistanceBetweenPoints(z, p);
            }
        }
        #endregion

        #region Class DistanceSorter
        private sealed class DistanceSorter : IComparer<DistanceFromPoint>
        {
            private DistanceSorter() { }

            public static void Sort(ref List<DistanceFromPoint> distances)
            {
                DistanceSorter sorter = new DistanceSorter();
                distances.Sort(sorter);
            }

            int IComparer<DistanceFromPoint>.Compare(DistanceFromPoint lhs, DistanceFromPoint rhs)
            {
                if (lhs.Distance == rhs.Distance)
                    return 0;
                else if (lhs.Distance > rhs.Distance)
                    return 1;
                else return -1;
            }
        }
        #endregion

        #region Class ConnectorSegment
        // represents a segment - the main entity in the routing algorithm
        [DebuggerDisplay("Segment ( {A.X}, {A.Y} ) - ( {B.X},{B.Y} ), {Orientation}")]
        private sealed class ConnectorSegment
        {
            private Point point1;
            private Point point2;
            private Orientation orientation;

            public ConnectorSegment(Point point1, Point point2)
            {
                if (point1.X != point2.X && point1.Y != point2.Y)
                    throw new InvalidOperationException(SR.GetString(SR.Error_InvalidConnectorSegment));

                this.point1 = point1;
                this.point2 = point2;
                this.orientation = ((this.point1.X == this.point2.X) ? Orientation.Vertical : Orientation.Horizontal);
            }

            public Point A
            {
                get
                {
                    return this.point1;
                }
            }

            public Point B
            {
                get
                {
                    return this.point2;
                }
            }

            public Orientation Orientation
            {
                get
                {
                    return this.orientation;
                }
            }

            //"segment l covers a point p, if the perpendicular from p to the line of which l is a segment intersects l"
            //since a) we only have horizotal or vertical segments and b) some segments will be unbound, we have to store and use orientation
            //flag to do analyzis
            public bool Covers(Point p)
            {
                return (this.orientation == Orientation.Horizontal) ? (p.X >= Math.Min(this.point1.X, this.point2.X) && p.X <= Math.Max(this.point1.X, this.point2.X)) : (p.Y >= Math.Min(this.point1.Y, this.point2.Y) && p.Y <= Math.Max(this.point1.Y, this.point2.Y));
            }

            //see if the two segments intersect
            //greatly simplified by the fact that we only have vertical or horizontal segments
            //should work fine with {Max, Min}Value values
            public Nullable<Point> Intersect(ConnectorSegment segment)
            {
                if (this.orientation == segment.Orientation)
                    return null;

                ConnectorSegment vertical = (this.orientation == Orientation.Vertical) ? this : segment;
                ConnectorSegment horizontal = (this.orientation == Orientation.Vertical) ? segment : this;

                if (vertical.A.X < Math.Min(horizontal.A.X, horizontal.B.X) || vertical.A.X > Math.Max(horizontal.A.X, horizontal.B.X))
                    return null;

                if (horizontal.A.Y < Math.Min(vertical.A.Y, vertical.B.Y) || horizontal.A.Y > Math.Max(vertical.A.Y, vertical.B.Y))
                    return null;

                return new Point(vertical.A.X, horizontal.A.Y);
            }

            //we consider the whole line to which this segment belongs for this test
            public bool PointLiesOnThisLine(Point p)
            {
                return (this.orientation == Orientation.Horizontal) ? p.Y == this.point1.Y : p.X == this.point1.X;
            }

            //we consider just the segment for this test
            public bool IsPointOnSegment(Point p)
            {
                if ((this.orientation == Orientation.Horizontal && p.Y != this.point1.Y) || (this.orientation == Orientation.Vertical && p.X != this.point1.X))
                    return false;

                int k = (this.orientation == Orientation.Horizontal) ? p.X : p.Y;
                int k1 = (this.orientation == Orientation.Horizontal) ? this.point1.X : this.point1.Y;
                int k2 = (this.orientation == Orientation.Horizontal) ? this.point2.X : this.point2.Y;
                return k >= Math.Min(k1, k2) && k <= Math.Max(k1, k2);
            }

            //does the given segment overlap us?
            //public bool Overlaps(ConnectorSegment segment)
            //{
            //    if (Orientation != segment.Orientation)
            //        return false;

            //    int k0 = (this.Orientation == Orientation.Horizontal) ? this.point1.Y : this.point1.X;
            //    int k1 = (this.Orientation == Orientation.Horizontal) ? this.point1.X : this.point1.Y;
            //    int k2 = (this.Orientation == Orientation.Horizontal) ? this.point2.X : this.point2.Y;

            //    int l0 = (this.Orientation == Orientation.Horizontal) ? segment.A.Y : segment.A.X;
            //    int l1 = (this.Orientation == Orientation.Horizontal) ? segment.A.X : segment.A.Y;
            //    int l2 = (this.Orientation == Orientation.Horizontal) ? segment.B.X : segment.B.Y;

            //    if (k0 != l0)
            //        return false;

            //    int min = Math.Min(k1, k2);
            //    k2 = Math.Max(k1, k2);
            //    k1 = min;

            //    min = Math.Min(l1, l2);
            //    l2 = Math.Max(l1, l2);
            //    l1 = min;

            //    if (l1 >= k2 || k1 >= l2)
            //        return false;

            //    return true;
            //}

            public ConnectorSegment PeprendecularThroughPoint(Point p)
            {
                Orientation newOrientation = (this.orientation == Orientation.Horizontal) ? Orientation.Vertical : Orientation.Horizontal;
                Point newPoint = new Point(p.X, p.Y);
                if (newOrientation == Orientation.Horizontal)
                    newPoint.X = int.MaxValue;
                else
                    newPoint.Y = int.MaxValue;

                return new ConnectorSegment(p, newPoint);
            }

            public Point ExtendPointOutwards(Point p)
            {
                Debug.Assert(!(p != this.point1 && p != this.point2), "wrong extension requested");
                if (p != this.point1 && p != this.point2)
                    return p;

                int k = (this.orientation == Orientation.Horizontal) ? p.X : p.Y;
                int k1 = (this.orientation == Orientation.Horizontal) ? this.point1.X : this.point1.Y;
                int k2 = (this.orientation == Orientation.Horizontal) ? this.point2.X : this.point2.Y;

                if (k == Math.Min(k1, k2))
                    k--;
                else
                    k++;

                return new Point((this.orientation == Orientation.Horizontal) ? k : p.X, (this.orientation == Orientation.Horizontal) ? p.Y : k);
            }

            public static double DistanceBetweenPoints(Point p, Point q)
            {
                return Math.Sqrt((double)(p.X - q.X) * (p.X - q.X) + (double)(p.Y - q.Y) * (p.Y - q.Y));
            }

            //given two points construct a segment through them from lesser cover to higher
            public static ConnectorSegment ConstructBoundSegment(CoverSet coverSet, Point a, Point b)
            {
                //

                if (a.X != b.X && a.Y != b.Y)
                    return null;

                return ConstructBoundSegment(coverSet, a, (a.X == b.X) ? Orientation.Vertical : Orientation.Horizontal);
            }

            public static ConnectorSegment SegmentFromLeftToRightCover(CoverSet coverSet, Point p)
            {
                ConnectorSegment leftCover = coverSet.GetCover(p, DesignerEdges.Left);
                ConnectorSegment rightCover = coverSet.GetCover(p, DesignerEdges.Right);

                //construct horizontal escape segment
                Point left = new Point((leftCover != null) ? leftCover.A.X : int.MinValue, p.Y);
                Point right = new Point((rightCover != null) ? rightCover.A.X : int.MaxValue, p.Y);
                ConnectorSegment h = new ConnectorSegment(left, right);
                return h;
            }

            public static ConnectorSegment SegmentFromBottomToTopCover(CoverSet coverSet, Point p)
            {
                ConnectorSegment bottomCover = coverSet.GetCover(p, DesignerEdges.Bottom);
                ConnectorSegment topCover = coverSet.GetCover(p, DesignerEdges.Top);

                //construct vertical escape segment
                Point bottom = new Point(p.X, (bottomCover != null) ? bottomCover.A.Y : int.MinValue);
                Point top = new Point(p.X, (topCover != null) ? topCover.A.Y : int.MaxValue);
                ConnectorSegment v = new ConnectorSegment(bottom, top);
                return v;
            }

            public static ConnectorSegment ConstructBoundSegment(CoverSet coverSet, Point a, Orientation orientation)
            {
                return (orientation == Orientation.Horizontal) ? SegmentFromLeftToRightCover(coverSet, a) : SegmentFromBottomToTopCover(coverSet, a);
            }

            public override bool Equals(object obj)
            {
                ConnectorSegment segment = obj as ConnectorSegment;
                if (segment == null)
                    return false;
                return (this.point1 == segment.A && this.point2 == segment.B && Orientation == segment.Orientation);
            }

            public override int GetHashCode()
            {
                return this.point1.GetHashCode() ^ this.point2.GetHashCode() ^ Orientation.GetHashCode();
            }
        }
        #endregion

        #region Class CoverSet (Set of vertical and horizontal covers)
        private sealed class CoverSet
        {
            private List<ConnectorSegment> verticalCovers = new List<ConnectorSegment>();
            private List<ConnectorSegment> horizontalCovers = new List<ConnectorSegment>();
            private List<ConnectorSegment> usedEscapeLine = new List<ConnectorSegment>();

            public CoverSet(Rectangle[] rectanglesToExclude, Point[] linesToExclude)
            {
                foreach (Rectangle rectangle in rectanglesToExclude)
                {
                    AddCover(new ConnectorSegment(new Point(rectangle.Left, rectangle.Top), new Point(rectangle.Left, rectangle.Bottom)));
                    AddCover(new ConnectorSegment(new Point(rectangle.Right, rectangle.Top), new Point(rectangle.Right, rectangle.Bottom)));
                    AddCover(new ConnectorSegment(new Point(rectangle.Left, rectangle.Top), new Point(rectangle.Right, rectangle.Top)));
                    AddCover(new ConnectorSegment(new Point(rectangle.Left, rectangle.Bottom), new Point(rectangle.Right, rectangle.Bottom)));
                }

                //Add the linesegments to cover
                for (int i = 0; i < linesToExclude.Length / 2; i++)
                    AddCover(new ConnectorSegment(linesToExclude[i * 2], linesToExclude[(i * 2) + 1]));
            }

            public void ClearUsedLines()
            {
                this.usedEscapeLine.Clear();
            }

            public void AddCover(ConnectorSegment cover)
            {
                List<ConnectorSegment> covers = (cover.Orientation == Orientation.Vertical) ? this.verticalCovers : this.horizontalCovers;

                for (int i = 0; i < covers.Count; i++)
                {
                    ConnectorSegment existingCover = covers[i];
                    if (cover.IsPointOnSegment(existingCover.A) && cover.IsPointOnSegment(existingCover.B))
                    {
                        //both points of vertical are on the new segment, delete the vertical and add the new one instead
                        covers.RemoveAt(i);
                        break;
                    }
                    else if (existingCover.IsPointOnSegment(cover.A) && existingCover.IsPointOnSegment(cover.B))
                    {
                        //both points of the new segment are on an existing segment already - skip this one
                        return;
                    }
                }

                covers.Add(cover);
            }

            //public ReadOnlyCollection<ConnectorSegment> VerticalCovers
            //{
            //    get
            //    {
            //        return this.verticalCovers.AsReadOnly();
            //    }
            //}

            //public ReadOnlyCollection<ConnectorSegment> HorizontalCovers
            //{
            //    get
            //    {
            //        return this.horizontalCovers.AsReadOnly();
            //    }
            //}

            public void AddUsedEscapeLine(ConnectorSegment segment)
            {
                this.usedEscapeLine.Add(segment);
            }

            //the new escape point should not lay on any of the already used escape segments
            public bool EscapeLineHasBeenUsed(ConnectorSegment segment, Point escapePoint)
            {
                for (int i = 0; i < this.usedEscapeLine.Count; i++)
                {
                    ConnectorSegment usedSegment = this.usedEscapeLine[i];
                    if (usedSegment.IsPointOnSegment(escapePoint))
                        return true;
                }
                return false;
            }

            //get just the vertical covers for the given point out of all stored segments
            //public List<ConnectorSegment> GetVerticalCovers(Point p)
            //{
            //    List<ConnectorSegment> covers = new List<ConnectorSegment>();
            //    foreach (ConnectorSegment segment in this.verticalCovers)
            //    {
            //        if (segment.Covers(p))
            //            covers.Add(segment);
            //    }
            //    return covers;
            //}

            //get just the horizontal covers for the given point out of all stored segments
            //public List<ConnectorSegment> GetHorizontalCovers(Point p)
            //{
            //    List<ConnectorSegment> covers = new List<ConnectorSegment>();
            //    foreach (ConnectorSegment segment in this.horizontalCovers)
            //    {
            //        if (segment.Covers(p))
            //            covers.Add(segment);
            //    }
            //    return covers;
            //}

            //get the cover on the given side (closest cover to the given side) for the point out of all stored segments
            public ConnectorSegment GetCover(Point p, DesignerEdges side)
            {
                ConnectorSegment cover = null;
                int distance = 0;

                if (side == DesignerEdges.Left || side == DesignerEdges.Right)
                {
                    for (int i = 0; i < this.verticalCovers.Count; i++)
                    {
                        ConnectorSegment segment = this.verticalCovers[i];
                        int currentDistance = (side == DesignerEdges.Left) ? p.X - segment.A.X : segment.A.X - p.X;
                        if (currentDistance > 0 && segment.Covers(p))
                        {
                            if (cover == null || distance > currentDistance)
                            {
                                cover = segment;
                                distance = currentDistance;
                            }
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < this.horizontalCovers.Count; i++)
                    {
                        ConnectorSegment segment = this.horizontalCovers[i];
                        int currentDistance = (side == DesignerEdges.Bottom) ? p.Y - segment.A.Y : segment.A.Y - p.Y;
                        if (currentDistance > 0 && segment.Covers(p))
                        {
                            if (cover == null || distance > currentDistance)
                            {
                                cover = segment;
                                distance = currentDistance;
                            }
                        }
                    }
                }

                return cover;
            }

            //get the cover on the given side (closest cover to the given side) for the point out of all stored segments
            public List<ConnectorSegment> GetCovers(Point p, DesignerEdges side)
            {
                List<ConnectorSegment> covers = new List<ConnectorSegment>();

                if (side == DesignerEdges.Left || side == DesignerEdges.Right)
                {
                    for (int i = 0; i < this.verticalCovers.Count; i++)
                    {
                        ConnectorSegment segment = this.verticalCovers[i];
                        int currentDistance = (side == DesignerEdges.Left) ? p.X - segment.A.X : segment.A.X - p.X;
                        if (currentDistance > 0 && segment.Covers(p))
                            covers.Add(segment);
                    }
                }
                else
                {
                    for (int i = 0; i < this.horizontalCovers.Count; i++)
                    {
                        ConnectorSegment segment = this.horizontalCovers[i];
                        int currentDistance = (side == DesignerEdges.Bottom) ? p.Y - segment.A.Y : segment.A.Y - p.Y;
                        if (currentDistance > 0 && segment.Covers(p))
                            covers.Add(segment);
                    }
                }

                return covers;
            }

            public bool IsEscapePoint(Point origin, Point escape, DesignerEdges side)
            {
                //get the original cover
                ConnectorSegment originalCover = this.GetCover(origin, side);
                int originalDistance;
                if (side == DesignerEdges.Left || side == DesignerEdges.Right)
                    originalDistance = originalCover.A.X - escape.X;
                else
                    originalDistance = originalCover.A.Y - escape.Y;

                // the escape point should not be covered by the the original cover
                if (originalCover.Covers(escape))
                    return false;

                //it should not also be covered by any member of other segments between the original cover and the original point
                List<ConnectorSegment> newCovers = this.GetCovers(escape, side);
                for (int i = 0; i < newCovers.Count; i++)
                {
                    ConnectorSegment newCover = newCovers[i];
                    //should never happen, just in case...
                    if (newCover == originalCover)
                        return false;

                    int newDistance;
                    if (side == DesignerEdges.Left || side == DesignerEdges.Right)
                        newDistance = Math.Abs(newCover.A.X - escape.X);
                    else
                        newDistance = Math.Abs(newCover.A.Y - escape.Y);

                    if (Math.Sign(newDistance) == Math.Sign(originalDistance) && Math.Abs(newDistance) < Math.Abs(originalDistance))
                        return false;
                }

                return true;
            }
        }
        #endregion
    }
    #endregion
}
