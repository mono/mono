namespace System.Workflow.Activities
{
    using System;
    using System.Text;
    using System.Reflection;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.CodeDom;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing.Design;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Diagnostics;
    using System.IO;
    using System.Windows.Forms;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;
    using System.Runtime.Serialization;
    using System.Workflow.ComponentModel.Serialization;
    using System.ComponentModel.Design.Serialization;
    using System.Xml;

    [DesignerSerializer(typeof(StateDesignerConnectorLayoutSerializer), typeof(WorkflowMarkupSerializer))]
    internal class StateDesignerConnector : Connector
    {
        internal const int ConnectorPadding = 10;

        private string _setStateName;
        private string _eventHandlerName;
        private string _sourceStateName;
        private string _targetStateName;

        internal StateDesignerConnector(ConnectionPoint source, ConnectionPoint target)
            : base(source, target)
        {
        }

        private StateDesigner RootStateDesigner
        {
            get
            {
                StateDesigner stateDesigner = (StateDesigner)this.ParentDesigner;
                while (true && stateDesigner != null)
                {
                    StateDesigner parentStateDesigner = stateDesigner.ParentDesigner as StateDesigner;
                    if (parentStateDesigner == null)
                        break;

                    stateDesigner = parentStateDesigner;
                }
                return stateDesigner;
            }
        }

        internal string SetStateName
        {
            get
            {
                return _setStateName;
            }
            set
            {
                _setStateName = value;
            }
        }

        internal string EventHandlerName
        {
            get
            {
                return _eventHandlerName;
            }
            set
            {
                _eventHandlerName = value;
            }
        }

        internal string SourceStateName
        {
            get
            {
                return _sourceStateName;
            }
            set
            {
                _sourceStateName = value;
            }
        }

        internal string TargetStateName
        {
            get
            {
                return _targetStateName;
            }
            set
            {
                _targetStateName = value;
            }
        }

        internal void ClearConnectorSegments()
        {
            this.SetConnectorSegments(new Collection<Point>());
        }

        protected override void OnLayout(ActivityDesignerLayoutEventArgs e)
        {
            if (!this.RootStateDesigner.HasActiveDesigner)
                base.OnLayout(e);
        }

        protected override ICollection<Rectangle> ExcludedRoutingRectangles
        {
            get
            {
                StateDesigner sourceStateDesigner = (StateDesigner)this.Source.AssociatedDesigner;
                List<Rectangle> excluded = new List<Rectangle>(base.ExcludedRoutingRectangles);
                if (sourceStateDesigner.IsRootDesigner)
                {
                    excluded.AddRange(sourceStateDesigner.EventHandlersBounds);
                }
                return excluded.AsReadOnly();
            }
        }

        public override bool HitTest(Point point)
        {
            if (this.RootStateDesigner != null && this.RootStateDesigner.ActiveDesigner != null)
                return false;

            return base.HitTest(point);
        }

        protected override void OnPaintEdited(ActivityDesignerPaintEventArgs e, Point[] segments, Point[] segmentEditPoints)
        {
            if (this.RootStateDesigner != null && this.RootStateDesigner.ActiveDesigner != null)
                return; // we don't draw connectors in the EventDriven view

            StateMachineTheme theme = e.DesignerTheme as StateMachineTheme;
            if (theme != null)
            {
                using (Pen lineEditPen = new Pen(WorkflowTheme.CurrentTheme.AmbientTheme.SelectionForeColor, 1))
                {
                    lineEditPen.DashStyle = DashStyle.Dash;
                    e.Graphics.DrawLines(lineEditPen, segments);

                    if (Source != null)
                        Source.OnPaint(e, false);

                    for (int i = 1; i < segments.Length - 1; i++)
                        PaintEditPoints(e, segments[i], false);

                    for (int i = 0; i < segmentEditPoints.Length; i++)
                        PaintEditPoints(e, segmentEditPoints[i], true);

                    if (Target != null)
                        Target.OnPaint(e, false);
                }
            }
        }

        protected override void OnPaintSelected(ActivityDesignerPaintEventArgs e, bool primarySelection, Point[] segmentEditPoints)
        {
            if (this.RootStateDesigner != null && this.RootStateDesigner.ActiveDesigner != null)
                return; // we don't draw connectors in the EventDriven view

            StateMachineTheme theme = e.DesignerTheme as StateMachineTheme;
            if (theme != null)
            {
                Size arrowCapSize = new Size(theme.ConnectorSize.Width / 5, theme.ConnectorSize.Height / 5);
                Size maxCapSize = theme.ConnectorSize;

                using (Pen lineSelectionPen = new Pen(WorkflowTheme.CurrentTheme.AmbientTheme.SelectionForeColor, 1))
                {
                    StateMachineDesignerPaint.DrawConnector(e.Graphics,
                        lineSelectionPen,
                        new List<Point>(ConnectorSegments).ToArray(),
                        arrowCapSize,
                        maxCapSize,
                        theme.ConnectorStartCap,
                        theme.ConnectorEndCap);
                }

                if (this.Source != null)
                    this.Source.OnPaint(e, false);

                ReadOnlyCollection<Point> endSegmentEditPoints = ConnectorSegments;
                for (int i = 1; i < endSegmentEditPoints.Count - 1; i++)
                    PaintEditPoints(e, endSegmentEditPoints[i], false);

                for (int i = 0; i < segmentEditPoints.Length; i++)
                    PaintEditPoints(e, segmentEditPoints[i], true);

                if (this.Target != null)
                    this.Target.OnPaint(e, false);
            }
        }

        protected override void OnPaint(ActivityDesignerPaintEventArgs e)
        {
            if (this.RootStateDesigner != null && this.RootStateDesigner.ActiveDesigner != null)
                return; // we don't draw connectors in the EventDriven view

            StateMachineTheme theme = e.DesignerTheme as StateMachineTheme;
            if (theme != null)
            {
                Size arrowCapSize = new Size(theme.ConnectorSize.Width / 5, theme.ConnectorSize.Height / 5);
                Size maxCapSize = theme.ConnectorSize;

                StateMachineDesignerPaint.DrawConnector(e.Graphics,
                    theme.ConnectorPen,
                    new List<Point>(ConnectorSegments).ToArray(),
                    arrowCapSize,
                    maxCapSize,
                    theme.ConnectorStartCap,
                    theme.ConnectorEndCap);
            }
        }

        private void PaintEditPoints(ActivityDesignerPaintEventArgs e, Point point, bool drawMidSegmentEditPoint)
        {
            Size size = (Source != null) ? Source.Bounds.Size : Size.Empty;
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
    }

    #region StateDesignerConnectorLayoutSerializer
    internal class StateDesignerConnectorLayoutSerializer : ConnectorLayoutSerializer
    {
        protected override PropertyInfo[] GetProperties(WorkflowMarkupSerializationManager serializationManager, object obj)
        {
            if (serializationManager == null)
                throw new ArgumentNullException("serializationManager");
            if (obj == null)
                throw new ArgumentNullException("obj");

            List<PropertyInfo> properties = new List<PropertyInfo>(base.GetProperties(serializationManager, obj));
            properties.Add(typeof(StateDesignerConnector).GetProperty("SetStateName", BindingFlags.Instance | BindingFlags.NonPublic));
            properties.Add(typeof(StateDesignerConnector).GetProperty("SourceStateName", BindingFlags.Instance | BindingFlags.NonPublic));
            properties.Add(typeof(StateDesignerConnector).GetProperty("TargetStateName", BindingFlags.Instance | BindingFlags.NonPublic));
            properties.Add(typeof(StateDesignerConnector).GetProperty("EventHandlerName", BindingFlags.Instance | BindingFlags.NonPublic));
            return properties.ToArray();
        }

        protected override object CreateInstance(WorkflowMarkupSerializationManager serializationManager, Type type)
        {
            if (serializationManager == null)
                throw new ArgumentNullException("serializationManager");
            if (type == null)
                throw new ArgumentNullException("type");

            StateDesignerConnector connector = null;

            IReferenceService referenceService = serializationManager.GetService(typeof(IReferenceService)) as IReferenceService;
            FreeformActivityDesigner freeformDesigner = serializationManager.Context[typeof(FreeformActivityDesigner)] as FreeformActivityDesigner;
            if (freeformDesigner != null && referenceService != null)
            {
                StateDesigner.DesignerLayoutConnectionPoint sourceConnection = null;
                ConnectionPoint targetConnection = null;
                StateDesigner.TransitionInfo transitionInfo = null;
                StateDesigner rootStateDesigner = null;

                try
                {
                    Dictionary<string, string> constructionArguments = GetConnectorConstructionArguments(serializationManager, type);

                    if (constructionArguments.ContainsKey("EventHandlerName") &&
                        constructionArguments.ContainsKey("SetStateName") &&
                        constructionArguments.ContainsKey("TargetStateName"))
                    {
                        CompositeActivity eventHandler = (CompositeActivity)referenceService.GetReference(constructionArguments["EventHandlerName"] as string);
                        SetStateActivity setState = (SetStateActivity)referenceService.GetReference(constructionArguments["SetStateName"] as string);
                        StateActivity targetState = (StateActivity)referenceService.GetReference(constructionArguments["TargetStateName"] as string);
                        transitionInfo = new StateDesigner.TransitionInfo(setState, eventHandler);
                        transitionInfo.TargetState = targetState;
                    }

                    if (constructionArguments.ContainsKey("SourceActivity") &&
                        constructionArguments.ContainsKey("SourceConnectionIndex") &&
                        constructionArguments.ContainsKey("SourceConnectionEdge") &&
                        constructionArguments.ContainsKey("EventHandlerName"))
                    {
                        StateDesigner sourceDesigner = (StateDesigner)StateDesigner.GetDesigner(referenceService.GetReference(constructionArguments["SourceActivity"] as string) as Activity);
                        CompositeActivity eventHandler = (CompositeActivity)referenceService.GetReference(constructionArguments["EventHandlerName"] as string);
                        rootStateDesigner = sourceDesigner.RootStateDesigner;
                        DesignerEdges sourceEdge = (DesignerEdges)Enum.Parse(typeof(DesignerEdges), constructionArguments["SourceConnectionEdge"] as string);
                        int sourceIndex = Convert.ToInt32(constructionArguments["SourceConnectionIndex"] as string, System.Globalization.CultureInfo.InvariantCulture);
                        if (sourceDesigner != null && eventHandler != null && sourceEdge != DesignerEdges.None && sourceIndex >= 0)
                            sourceConnection = new StateDesigner.DesignerLayoutConnectionPoint(sourceDesigner, sourceIndex, eventHandler, sourceEdge);
                    }

                    if (constructionArguments.ContainsKey("TargetActivity") &&
                        constructionArguments.ContainsKey("TargetConnectionIndex") &&
                        constructionArguments.ContainsKey("TargetConnectionEdge"))
                    {
                        ActivityDesigner targetDesigner = StateDesigner.GetDesigner(referenceService.GetReference(constructionArguments["TargetActivity"] as string) as Activity);
                        DesignerEdges targetEdge = (DesignerEdges)Enum.Parse(typeof(DesignerEdges), constructionArguments["TargetConnectionEdge"] as string);
                        int targetIndex = Convert.ToInt32(constructionArguments["TargetConnectionIndex"] as string, System.Globalization.CultureInfo.InvariantCulture);
                        if (targetDesigner != null && targetEdge != DesignerEdges.None && targetIndex >= 0)
                            targetConnection = new ConnectionPoint(targetDesigner, targetEdge, targetIndex);
                    }
                }
                catch
                {
                }

                if (transitionInfo != null && sourceConnection != null && targetConnection != null)
                {
                    connector = rootStateDesigner.FindConnector(transitionInfo);
                    if (connector == null)
                    {
                        rootStateDesigner.AddingSetState = false;
                        try
                        {
                            connector = freeformDesigner.AddConnector(sourceConnection, targetConnection) as StateDesignerConnector;
                        }
                        finally
                        {
                            rootStateDesigner.AddingSetState = true;
                        }
                    }
                    else
                    {
                        connector.Source = sourceConnection;
                        connector.Target = targetConnection;
                        connector.ClearConnectorSegments();
                    }
                }
            }

            return connector;
        }
    }
    #endregion
}
