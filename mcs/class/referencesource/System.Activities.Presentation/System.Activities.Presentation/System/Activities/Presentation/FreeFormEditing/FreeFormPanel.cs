//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Activities.Presentation.FreeFormEditing
{
    using System;
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Threading;
    using System.Activities.Presentation;
    using System.Activities.Presentation.View;
    using System.Activities.Presentation.Internal.PropertyEditing;
    using System.Runtime;
    using System.Linq;
    using System.Activities.Presentation.Model;

    internal class FreeFormPanel : Panel
    {
        public static readonly DependencyProperty ChildSizeProperty = DependencyProperty.RegisterAttached("ChildSize", typeof(Size), typeof(FreeFormPanel), new FrameworkPropertyMetadata());
        public static readonly DependencyProperty LocationProperty = DependencyProperty.RegisterAttached("Location", typeof(Point), typeof(FreeFormPanel), new FrameworkPropertyMetadata(new Point(-1, -1)));
        public static readonly DependencyProperty RequiredWidthProperty = DependencyProperty.Register("RequiredWidth", typeof(Double), typeof(FreeFormPanel), new FrameworkPropertyMetadata(double.NaN));
        public static readonly DependencyProperty RequiredHeightProperty = DependencyProperty.Register("RequiredHeight", typeof(Double), typeof(FreeFormPanel), new FrameworkPropertyMetadata(double.NaN));
        public static readonly DependencyProperty DestinationConnectionPointProperty = DependencyProperty.RegisterAttached("DestinationConnectionPoint", typeof(ConnectionPoint), typeof(FreeFormPanel), new FrameworkPropertyMetadata());
        public static readonly DependencyProperty SourceConnectionPointProperty = DependencyProperty.RegisterAttached("SourceConnectionPoint", typeof(ConnectionPoint), typeof(FreeFormPanel), new FrameworkPropertyMetadata());
        public static readonly DependencyProperty DisabledProperty = DependencyProperty.Register("Disabled", typeof(bool), typeof(FreeFormPanel), new UIPropertyMetadata(false));
        public static readonly DependencyProperty AutoConnectContainerProperty = DependencyProperty.Register("AutoConnectContainer", typeof(IAutoConnectContainer), typeof(FreeFormPanel), new UIPropertyMetadata(null));

        public const double ConnectorEditorOpacity = 1.0;
        public const double ConnectorEditorThickness = 1.5;
        public const double LeftStackingMargin = 50;
        public const double TopStackingMargin = 80;
        public const double VerticalStackingDistance = 50;
        public const double GridSize = 10;
        public ConnectorEditor connectorEditor;
        double lastYPosition;
        bool measureConnectors = false;
        bool measureConnectorsPosted = false;
        AutoConnectHelper autoConnectHelper = null;
        DesignerConfigurationService designerConfigurationService = null;

        public FreeFormPanel()
        {
            connectorEditor = null;
            this.autoConnectHelper = new AutoConnectHelper(this);
            lastYPosition = FreeFormPanel.TopStackingMargin;

            this.Unloaded += (sender, e) =>
            {
                this.RemoveConnectorEditor();
            };
        }

        public event LocationChangedEventHandler LocationChanged;
        public event ConnectorMovedEventHandler ConnectorMoved;
        public event RequiredSizeChangedEventHandler RequiredSizeChanged;

        public static Size GetChildSize(DependencyObject obj)
        {
            return (Size)obj.GetValue(FreeFormPanel.ChildSizeProperty);
        }

        public static void SetChildSize(DependencyObject obj, Size size)
        {
            obj.SetValue(FreeFormPanel.ChildSizeProperty, size);
        }

        public double RequiredHeight
        {
            get { return (double)GetValue(FreeFormPanel.RequiredHeightProperty); }
            private set { SetValue(FreeFormPanel.RequiredHeightProperty, value); }
        }

        public double RequiredWidth
        {
            get { return (double)GetValue(FreeFormPanel.RequiredWidthProperty); }
            private set { SetValue(FreeFormPanel.RequiredWidthProperty, value); }
        }

        public bool Disabled
        {
            get { return (bool)GetValue(DisabledProperty); }
            set { SetValue(DisabledProperty, value); }
        }

        public IAutoConnectContainer AutoConnectContainer
        {
            get { return (IAutoConnectContainer)GetValue(AutoConnectContainerProperty); }
            set { SetValue(AutoConnectContainerProperty, value); }
        }

        public static Vector CalculateMovement(Key key, bool isRightToLeft)
        {
            Vector moveDir;
            switch (key)
            {
                case Key.Down:
                    moveDir = new Vector(0, FreeFormPanel.GridSize);
                    break;
                case Key.Up:
                    moveDir = new Vector(0, -FreeFormPanel.GridSize);
                    break;
                case Key.Right:
                    moveDir = new Vector(FreeFormPanel.GridSize, 0);
                    break;
                case Key.Left:
                    moveDir = new Vector(-FreeFormPanel.GridSize, 0);
                    break;
                default:
                    Fx.Assert(false, "Invalid case");
                    moveDir = new Vector(0, 0);
                    break;
            }

            if (isRightToLeft)
            {
                moveDir.X = -moveDir.X;
            }

            return moveDir;
        }

        public static double ZeroIfNegative(double val)
        {
            return val.IsNoGreaterThan(0) ? 0 : val;
        }

        internal UIElement CurrentAutoConnectTarget
        {
            get
            {
                return this.autoConnectHelper.CurrentTarget;
            }
        }

        internal Connector CurrentAutoSplitTarget
        {
            get;
            set;
        }

        bool AutoConnectEnabled
        {
            get
            {
                if (this.designerConfigurationService == null)
                {
                    DesignerView view = VisualTreeUtils.FindVisualAncestor<DesignerView>(this);
                    if (view != null)
                    {
                        this.designerConfigurationService = view.Context.Services.GetService<DesignerConfigurationService>();
                        return this.designerConfigurationService.AutoConnectEnabled;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return this.designerConfigurationService.AutoConnectEnabled;
                }
            }
        }

        public static ConnectionPoint GetDestinationConnectionPoint(DependencyObject obj)
        {
            return (ConnectionPoint)obj.GetValue(FreeFormPanel.DestinationConnectionPointProperty);
        }

        public static void SetDestinationConnectionPoint(DependencyObject obj, ConnectionPoint connectionPoint)
        {
            obj.SetValue(FreeFormPanel.DestinationConnectionPointProperty, connectionPoint);
        }

        public static ConnectionPoint GetSourceConnectionPoint(DependencyObject obj)
        {
            return (ConnectionPoint)obj.GetValue(FreeFormPanel.SourceConnectionPointProperty);
        }

        public static void SetSourceConnectionPoint(DependencyObject obj, ConnectionPoint connectionPoint)
        {
            obj.SetValue(FreeFormPanel.SourceConnectionPointProperty, connectionPoint);
        }

        public static Point GetLocation(DependencyObject obj)
        {
            return (Point)obj.GetValue(FreeFormPanel.LocationProperty);
        }

        public static void SetLocation(DependencyObject obj, Point point)
        {
            obj.SetValue(FreeFormPanel.LocationProperty, point);
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            this.SnapsToDevicePixels = true;
            this.AllowDrop = true;
        }

        internal void RemoveAutoConnectAdorner()
        {
            this.autoConnectHelper.RemoveDropTargets();
        }

        internal List<DependencyObject> GetChildShapes(DependencyObject excluded)
        {
            List<DependencyObject> children = new List<DependencyObject>();
            foreach (UIElement element in this.Children)
            {
                if (element is Connector)
                {
                    continue;
                }
                if (object.Equals(element, excluded))
                {
                    continue;
                }
                else if (element is VirtualizedContainerService.VirtualizingContainer)
                {
                    if (object.Equals(excluded, ((VirtualizedContainerService.VirtualizingContainer)element).Child))
                    {
                        continue;
                    }
                }
                children.Add(element);
            }
            return children;
        }

        protected override void OnPreviewDragOver(DragEventArgs e)
        {
            if (this.IsOutmostPanel())
            {
                if (this.AutoConnectEnabled && DragDropHelper.GetDraggedObjectCount(e) == 1)
                {
                    this.autoConnectHelper.OnPreviewDragOverPanel(e);
                }
            }
            base.OnPreviewDragOver(e);
        }

        public void UpdateConnectorPoints(Connector connector, List<Point> points)
        {
            PointCollection pointCollection = new PointCollection();
            foreach (Point point in points)
            {
                pointCollection.Add(new Point(point.X < 0 ? 0 : point.X, point.Y < 0 ? 0 : point.Y));
            }
            connector.Points = pointCollection;
            OnLocationChanged(connector, null);
        }

        static public List<Point> GetEdgeRelativeToOutmostPanel(ConnectionPoint connectionPoint)
        {
            return connectionPoint.Edge;
        }

        static public Point GetLocationRelativeToOutmostPanel(ConnectionPoint connectionPoint)
        {
            return connectionPoint.Location;
        }

        public Point GetLocationRelativeToOutmostPanel(Point location)
        {
            return this.TranslatePoint(location, this.GetOutmostPanel());
        }

        FreeFormPanel GetOutmostPanel()
        {
            DependencyObject obj = this;
            do
            {
                obj = VisualTreeHelper.GetParent(obj);
            }
            while (obj != null && !typeof(INestedFreeFormPanelContainer).IsAssignableFrom(obj.GetType()));

            if (obj != null)
            {
                INestedFreeFormPanelContainer container = (INestedFreeFormPanelContainer)obj;
                if (container.GetChildFreeFormPanel() == this)
                {
                    return container.GetOutmostFreeFormPanel();
                }
            }
            return this;
        }

        internal bool IsOutmostPanel()
        {
            return this == this.GetOutmostPanel();
        }

        internal static ConnectionPoint ConnectionPointHitTest(Point hitPoint, ConnectionPointsAdorner adorner)
        {
            FreeFormPanel panel = VisualTreeUtils.FindVisualAncestor<FreeFormPanel>(adorner.AdornedElement);
            return ConnectionPointHitTest(hitPoint, adorner.ConnectionPoints, panel);
        }

        internal static ConnectionPoint ConnectionPointHitTest(Point hitPoint, List<ConnectionPoint> connectionPoints, FreeFormPanel panel)
        {
            ConnectionPoint hitConnectionPoint = null;
            FreeFormPanel outmost = panel.GetOutmostPanel();
            foreach (ConnectionPoint connPoint in connectionPoints)
            {
                if (connPoint != null && connPoint.IsEnabled)
                {
                    if (new Rect(panel.TranslatePoint(connPoint.Location, outmost) + connPoint.HitTestOffset, connPoint.HitTestSize).Contains(hitPoint))
                    {
                        hitConnectionPoint = connPoint;
                        break;
                    }
                }
            }
            return hitConnectionPoint;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            double height = 0;
            double width = 0;
            for (int i = 0; i < Children.Count; i++)
            {
                Point pt = new Point(0, 0);
                Size size = Children[i].DesiredSize;
                if (Children[i].GetType() == typeof(Connector))
                {
                    ((UIElement)Children[i]).Arrange(new Rect(pt, size));
                }
                else
                {
                    pt = FreeFormPanel.GetLocation(Children[i]);
                    ((UIElement)Children[i]).Arrange(new Rect(pt, size));
                }
                if (width < (size.Width + pt.X))
                {
                    width = size.Width + pt.X;
                }
                if (height < (size.Height + pt.Y))
                {
                    height = size.Height + pt.Y;
                }
            }
            width = (width < this.MinWidth) ? this.MinWidth : width;
            width = (width < this.Width) ? (this.Width < Double.MaxValue ? this.Width : width) : width;

            height = (height < this.MinHeight) ? this.MinHeight : height;
            height = (height < this.Height) ? (this.Height < Double.MaxValue ? this.Height : height) : height;

            return new Size(width, height);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            bool isOutmostPanel = this.IsOutmostPanel();
            base.MeasureOverride(availableSize);
            double height;
            double width;
            this.MeasureChildren(out height, out width);
            if (this.RequiredSizeChanged != null)
            {
                this.RequiredSizeChanged(this, new RequiredSizeChangedEventArgs(new Size(width, height)));
            }
            this.RequiredWidth = width;
            this.RequiredHeight = height;

            if (isOutmostPanel)
            {
                Action MeasureConnectors = () =>
                {
                    //This action will execute at Input priority. 
                    //Enabling measuring on Connectors and forcing a MeasureOverride by calling InvalidateMeasure.
                    this.measureConnectors = true;
                    this.InvalidateMeasure();
                };
                if (!measureConnectorsPosted)
                {
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Input, MeasureConnectors);
                    measureConnectorsPosted = true;
                }
                if (measureConnectors)
                {
                    measureConnectors = false;
                    measureConnectorsPosted = false;
                }
            }
            width = (width < this.Width) ? (this.Width < Double.MaxValue ? this.Width : width) : width;
            height = (height < this.Height) ? (this.Height < Double.MaxValue ? this.Height : height) : height;
            return new Size(width, height);
        }

        private void MeasureChildren(out double height, out double width)
        {
            height = 0;
            width = 0;
            Point pt = new Point(0, 0);
            bool isOutmostPanel = this.IsOutmostPanel();
            foreach (UIElement child in Children)
            {
                Connector connectorChild = child as Connector;
                if (connectorChild != null && isOutmostPanel)
                {
                    pt = new Point(0, 0);

                    if (measureConnectors)
                    {
                        Point srcPoint = FreeFormPanel.GetLocationRelativeToOutmostPanel(FreeFormPanel.GetSourceConnectionPoint(connectorChild));
                        Point destPoint = FreeFormPanel.GetLocationRelativeToOutmostPanel(FreeFormPanel.GetDestinationConnectionPoint(connectorChild));
                        if (connectorChild.Points.Count == 0 || !this.Disabled &&
                            ((DesignerGeometryHelper.ManhattanDistanceBetweenPoints(connectorChild.Points[0], srcPoint) > ConnectorRouter.EndPointTolerance)
                            || (DesignerGeometryHelper.ManhattanDistanceBetweenPoints(connectorChild.Points[connectorChild.Points.Count - 1], destPoint) > ConnectorRouter.EndPointTolerance)))
                        {
                            connectorChild.Points = new PointCollection();
                            RoutePolyLine(connectorChild);
                        }
                        connectorChild.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
                    }
                    else
                    {
                        continue;
                    }
                }
                else //Measure non-connector elements.
                {
                    child.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
                    if (!child.DesiredSize.Equals(((Size)FreeFormPanel.GetChildSize(child))))
                    {
                        FreeFormPanel.SetChildSize(child, child.DesiredSize);
                    }
                    pt = FreeFormPanel.GetLocation(child);
                    if (!IsLocationValid(pt))
                    {
                        pt = new Point(LeftStackingMargin, lastYPosition);
                        OnLocationChanged(child, new LocationChangedEventArgs(pt));
                        FreeFormPanel.SetLocation(child, pt);
                        lastYPosition += child.DesiredSize.Height + VerticalStackingDistance;
                    }
                }
                if (height < child.DesiredSize.Height + pt.Y)
                {
                    height = child.DesiredSize.Height + pt.Y;
                }
                if (width < child.DesiredSize.Width + pt.X)
                {
                    width = child.DesiredSize.Width + pt.X;
                }
            }

            width = (width < this.MinWidth) ? this.MinWidth : width;
            height = (height < this.MinHeight) ? this.MinHeight : height;
        }

        static bool IsLocationValid(Point location)
        {
            return location.X >= 0 && location.Y >= 0;
        }

        void OnLocationChanged(Object sender, LocationChangedEventArgs e)
        {
            if (LocationChanged != null)
            {
                LocationChanged(sender, e);
            }
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            if (e != null && !this.Disabled && this.IsOutmostPanel())
            {
                if (connectorEditor != null && connectorEditor.BeingEdited
                    && Mouse.DirectlyOver != null
                    && !(Mouse.DirectlyOver is ConnectionPointsAdorner))
                {
                    SaveConnectorEditor(e.GetPosition(this));
                }
            }
            base.OnMouseLeave(e);
        }

        protected override void OnMouseMove(System.Windows.Input.MouseEventArgs e)
        {
            if (e != null && !this.Disabled && this.IsOutmostPanel())
            {
                if (this.AutoConnectEnabled)
                {
                    this.RemoveAutoConnectAdorner();
                }
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    if (connectorEditor != null && connectorEditor.BeingEdited)
                    {
                        AutoScrollHelper.AutoScroll(e, this, 1);
                        connectorEditor.Update(e.GetPosition(this));
                        e.Handled = true;
                    }
                }
            }
            base.OnMouseMove(e);
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (e != null && !this.Disabled && this.IsOutmostPanel())
            {
                if (connectorEditor != null && connectorEditor.BeingEdited)
                {
                    SaveConnectorEditor(e.GetPosition(this));
                }
            }
            base.OnMouseLeftButtonUp(e);
        }

        public void RemoveConnectorEditor()
        {
            if (connectorEditor != null)
            {
                connectorEditor.Remove();
                connectorEditor = null;
            }

        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e != null && !this.Disabled && this.IsOutmostPanel())
            {
                if (connectorEditor != null && connectorEditor.BeingEdited)
                {
                    if (e.Key == Key.Escape)
                    {
                        //If escape key is hit while dragging a connector, end dragging.

                        Connector affectedConnector = connectorEditor.Connector;
                        RemoveConnectorEditor();
                        this.connectorEditor = new ConnectorEditor(this, affectedConnector);
                    }

                    // Ignore all other Keyboard input when rerouting connector
                    e.Handled = true;
                }
            }

            base.OnKeyDown(e);
        }

        static bool ShouldCreateNewConnectorEditor(MouseButtonEventArgs e)
        {
            Connector connector = e.Source as Connector;
            // Don't create new connector editor when clicking on the start dot.
            if (connector == null || (connector.StartDot != null && connector.StartDot.IsAncestorOf(e.MouseDevice.DirectlyOver as DependencyObject)))
            {
                return false;
            }
            return true;
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (e != null && !this.Disabled && this.IsOutmostPanel() && e.ClickCount == 1)
            {
                //If one of the edit points is clicked, update the connector editor.
                if ((connectorEditor != null) && connectorEditor.EditPointsHitTest(e.GetPosition(this)))
                {
                    connectorEditor.Update(e.GetPosition(this));
                    e.Handled = true;
                }
                else if (ShouldCreateNewConnectorEditor(e))
                {
                    CreateNewConnectorEditor(e);
                }
            }
            base.OnPreviewMouseLeftButtonDown(e);
        }

        void CreateNewConnectorEditor(MouseButtonEventArgs e)
        {
            if (connectorEditor == null || !e.Source.Equals(connectorEditor.Connector))
            {
                //If user clicks anywhere other than the connector editor, destroy it.
                RemoveConnectorEditor();
                if (typeof(Connector).IsAssignableFrom(e.Source.GetType()))
                {
                    this.connectorEditor = new ConnectorEditor(this, e.Source as Connector);
                }
            }
        }

        //Calls the Line routing algorithm and populates the points collection of the connector.
        void RoutePolyLine(Connector connector)
        {
            Point[] pts = ConnectorRouter.Route(this, FreeFormPanel.GetSourceConnectionPoint(connector), FreeFormPanel.GetDestinationConnectionPoint(connector));
            List<Point> points = new List<Point>(pts);
            if (pts != null)
            {
                UpdateConnectorPoints(connector, points);
            }
        }


        //Connector editing is complete, save the final connectorEditor state into the connector.
        void SaveConnectorEditor(Point pt)
        {
            bool isConnectionEndPointMoved = !connectorEditor.Persist(pt);

            if (this.ConnectorMoved != null)
            {
                Connector connector = this.connectorEditor.Connector;
                List<Point> points = this.connectorEditor.ConnectorEditorLocation;
                ConnectorMoved(connector, new ConnectorMovedEventArgs(points));
            }

            if (isConnectionEndPointMoved)
            {
                //Persist will return false, when the ConnectionEndPoint has been moved.
                RemoveConnectorEditor();
            }
            else
            {
                this.InvalidateMeasure();
            }
        }
    }
}

