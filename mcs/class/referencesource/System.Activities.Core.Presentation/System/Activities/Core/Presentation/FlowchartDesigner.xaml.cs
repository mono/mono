//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Activities.Core.Presentation
{
    using System.Activities;
    using System.Activities.Presentation;
    using System.Activities.Presentation.FreeFormEditing;
    using System.Activities.Presentation.Hosting;
    using System.Activities.Presentation.Internal.PropertyEditing;
    using System.Activities.Presentation.Metadata;
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.View;
    using System.Activities.Presentation.View.OutlineView;
    using System.Activities.Presentation.ViewState;
    using System.Activities.Statements;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Reflection;
    using System.Runtime;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Threading;

    [ActivityDesignerOptions(AlwaysCollapseChildren = true)]
    partial class FlowchartDesigner : IAutoConnectContainer, IAutoSplitContainer
    {
        public static readonly DependencyProperty ConnectionPointsProperty = DependencyProperty.RegisterAttached("ConnectionPoints", typeof(List<ConnectionPoint>), typeof(FlowchartDesigner), new FrameworkPropertyMetadata());
        public static readonly DependencyProperty LinkModelItemProperty = DependencyProperty.RegisterAttached("LinkModelItem", typeof(ModelItem), typeof(FlowchartDesigner), new FrameworkPropertyMetadata());
        public static readonly DependencyProperty FlowElementModelItemProperty = DependencyProperty.RegisterAttached("FlowElementModelItem", typeof(ModelItem), typeof(FlowchartDesigner), new FrameworkPropertyMetadata());

        public static readonly DependencyProperty FlowchartWidthProperty = DependencyProperty.Register("FlowchartWidth", typeof(double), typeof(FlowchartDesigner), new FrameworkPropertyMetadata());
        public static readonly DependencyProperty FlowchartHeightProperty = DependencyProperty.Register("FlowchartHeight", typeof(double), typeof(FlowchartDesigner), new FrameworkPropertyMetadata());

        public static readonly DependencyProperty TrueConnectionPointProperty = DependencyProperty.RegisterAttached("TrueConnectionPoint", typeof(ConnectionPoint), typeof(FlowchartDesigner), new FrameworkPropertyMetadata());
        public static readonly DependencyProperty FalseConnectionPointProperty = DependencyProperty.RegisterAttached("FalseConnectionPoint", typeof(ConnectionPoint), typeof(FlowchartDesigner), new FrameworkPropertyMetadata());

        public static readonly DependencyProperty ShowAllConditionsProperty = DependencyProperty.Register("ShowAllConditions", typeof(bool), typeof(FlowchartDesigner));

        public static readonly RoutedCommand SetAsStartNodeCommand = new RoutedCommand("SetAsStartNode", typeof(FlowchartDesigner));
        //public static readonly RoutedCommand ConnectNodesCommand = new RoutedCommand("ConnectNodes", typeof(FlowchartDesigner));
        public static readonly RoutedCommand ShowAllConditionsCommand = new RoutedCommand("ShowAllConditionsCommand", typeof(FlowchartDesigner));
        public static readonly RoutedCommand HideAllConditionsCommand = new RoutedCommand("HideAllConditionsCommand", typeof(FlowchartDesigner));

        const double flowElementCaptionFontSize = 11;
        const double DebugTimeMaxConnectorShapeDist = 10;
        static readonly FontFamily flowElementCaptionFontFamily = new FontFamily("Tohoma");
        static readonly FontStyle flowElementCaptionFontStyle = new FontStyle();
        static readonly Typeface flowElementCaptionTypeface = new Typeface("Tohoma");
        
        internal Dictionary<ModelItem, UIElement> modelElement;
        //Consider FlowStep.Action = SomeActivity. FlowStep modelItem is referred as FlowNodeMI, SomeActivity modelItem is shapeMI and the designer for SomeActivity is the shape on canvas.
        //To go from the FlowNodeMI to the shape on canvas, we can use the path: FlowNodeMI(FlowStep.Action)-> shapeMI (modelElement Dictionary)-> Actual UIElement shape
        //However this path does not always work.  For instance in delete case: FlowStep.Action is set to null to update the ModelItem.Parents property on the shapeMI
        //flowNodeToUIElement dictionary is used to solve this problem.
        Dictionary<ModelItem, UIElement> flowNodeToUIElement;

        const double startSymbolTopMargin = 10.0;
        const string shapeLocation = "ShapeLocation";
        const string shapeSize = "ShapeSize";
        const string TrueConnectorViewStateKey = "TrueConnector";
        const string FalseConnectorViewStateKey = "FalseConnector";
        const string CaseViewStateKeyAppendString = "Connector";
        const string FlowSwitchDefaultViewStateKey = "Default";
        const string ConnectorViewStateKey = "ConnectorLocation";
        static Color ConnectionPointColor = Colors.LightGray;
        UIElement lastConnectionPointMouseUpElement = null;
        //shapeLocations is useful to avoid pasting on existing shapes.
        //This is populated in 2 cases 1. When the shape with existing Viewstate is added 2. On ViewState changed.
        HashSet<Point> shapeLocations = null;
        //selectedConnector is a placeholder for the last connector selected.
        //This removes the need for a dictionary mapping modelitem to connector for deletion.
        //This will change if in future we plan to support multi-select + delete.
        Connector selectedConnector;
        //srcConnectionPoint is required for link addition gesture to store the source of the link.
        ConnectionPoint srcConnectionPoint;
        ConnectionPoint srcConnectionPointForAutoConnect;
        ConnectionPoint srcConnectionPointForAutoSplit;
        ConnectionPoint destConnectionPointForAutoSplit;
        EdgeLocation entryEdgeForAutoSplit;
        EdgeLocation exitEdgeForAutoSplit;
        bool internalViewStateChange = false;
        bool startNodeAdded = false;
        bool updatingSelectedConnector;
        internal FreeFormPanel panel = null;
        AdornerLayer adornerLayer;
        MenuItem setAsStartNode;
        bool? isRightToLeft;
        private bool isLoaded = false;

        internal bool IsResizing { get; set; }

        public FlowchartDesigner()
        {
            InitializeComponent();
            this.modelElement = new Dictionary<ModelItem, UIElement>();
            this.flowNodeToUIElement = new Dictionary<ModelItem, UIElement>();
            this.shapeLocations = new HashSet<Point>();
            this.selectedConnector = null;
            ConstructSetAsStartNodeMenuItem();

            this.Loaded += (s, e) =>
            {
                this.isLoaded = true;

                if (this.ShowExpanded)
                {
                    ((ICompositeViewEvents)this).RegisterDefaultCompositeView(this);
                }
                DesignerView designerView = this.Context.Services.GetService<DesignerView>() as DesignerView;
                if (!designerView.ContextMenu.Items.Contains(setAsStartNode))
                {
                    designerView.ContextMenu.Items.Add(setAsStartNode);
                }

                WorkflowCommandExtensionItem item = this.Context.Items.GetValue<WorkflowCommandExtensionItem>();
                if (item != null)
                {
                    if (item.CommandExtensionCallback is DefaultCommandExtensionCallback)
                    {
                        this.InputBindings.Add(new KeyBinding(FlowchartDesignerCommands.ConnectNodesCommand, new DefaultCommandExtensionCallback.ChordKeyGesture(Key.E, Key.F)));
                    }
                }

                Selection.Subscribe(Context, OnSelectionChanged);
            };

            this.Unloaded += (s, e) =>
            {
                this.isLoaded = false;

                if (object.Equals(this.DefaultCompositeView, this))
                {
                    ((ICompositeViewEvents)this).UnregisterDefaultCompositeView(this);
                }
                DesignerView designerView = this.Context.Services.GetService<DesignerView>() as DesignerView;
                designerView.ContextMenu.Items.Remove(setAsStartNode);

                Selection.Unsubscribe(Context, OnSelectionChanged);
            };
        }

        public static double FlowNodeCaptionFontSize
        {
            get { return flowElementCaptionFontSize; }
        }

        public static FontFamily FlowNodeCaptionFontFamily
        {
            get { return flowElementCaptionFontFamily; }
        }

        public static FontStyle FlowNodeCaptionFontStyle
        {
            get { return flowElementCaptionFontStyle; }
        }

        public static Typeface FlowElementCaptionTypeface
        {
            get { return flowElementCaptionTypeface; }
        }

        void OnSetAsStartNodeCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            //The condition is necessary so that the child flowchart inside a flowchart doesn't try to handle the event.
            if (!object.Equals(e.Source, this))
            {
                e.CanExecute = !this.IsReadOnly;
                e.Handled = true;
            }
        }

        void OnShowAllConditionsMenuLoaded(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            if (null != item)
            {
                bool expanded = (bool)GetValue(WorkflowViewElement.ShowExpandedProperty);
                if (expanded)
                {
                    item.Visibility = Visibility.Visible;
                }
                else
                {
                    item.Visibility = Visibility.Collapsed;
                }
            }
            e.Handled = true;
        }

        void OnHideAllConditionsMenuLoaded(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            if (null != item)
            {
                bool expanded = (bool)GetValue(WorkflowViewElement.ShowExpandedProperty);
                if (expanded)
                {
                    item.Visibility = Visibility.Visible;
                }
                else
                {
                    item.Visibility = Visibility.Collapsed;
                }
            }
            e.Handled = true;
        }

        void OnSetAsStartNodeCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            ModelItem selection = this.Context.Items.GetValue<Selection>().PrimarySelection;
            Fx.Assert(this.modelElement.ContainsKey(selection), "Selection is not contained in this container");
            this.ModelItem.Properties["StartNode"].SetValue(this.GetFlowElementMI(selection));
            e.Handled = true;
        }

        void ConstructSetAsStartNodeMenuItem()
        {
            setAsStartNode = new MenuItem();
            setAsStartNode.Command = FlowchartDesigner.SetAsStartNodeCommand;
            setAsStartNode.Header = this.SetAsStartNodeMenuItemHeader;
            setAsStartNode.Visibility = Visibility.Collapsed;
            setAsStartNode.Loaded += new RoutedEventHandler(OnSetAsStartNodeLoaded);
            //AutomationProperties
            setAsStartNode.SetValue(System.Windows.Automation.AutomationProperties.AutomationIdProperty, "SetAsStartNodeMenuItem");
        }

        string SetAsStartNodeMenuItemHeader
        {
            get { return (string)this.FindResource("SetAsStartNodeMenuItemHeader"); }
        }

        void OnSetAsStartNodeLoaded(object sender, RoutedEventArgs e)
        {
            MenuItem setAsStartNodeMenuItem = sender as MenuItem;
            setAsStartNodeMenuItem.Visibility = Visibility.Collapsed;
            Selection selection = this.Context.Items.GetValue<Selection>();
            if (selection.SelectionCount == 1 && this.modelElement.ContainsKey(selection.PrimarySelection))
            {
                setAsStartNodeMenuItem.Visibility = Visibility.Visible;
            }
            e.Handled = true;
        }

        public static void RegisterMetadata(AttributeTableBuilder builder)
        {
            Type type = typeof(Flowchart);
            builder.AddCustomAttributes(type, new DesignerAttribute(typeof(FlowchartDesigner)));
            builder.AddCustomAttributes(type, type.GetProperty("StartNode"), BrowsableAttribute.No);
            builder.AddCustomAttributes(type, type.GetProperty("Nodes"), BrowsableAttribute.No);
            builder.AddCustomAttributes(type, type.GetProperty("Variables"), BrowsableAttribute.No);
            builder.AddCustomAttributes(type, new FeatureAttribute(typeof(FlowchartSizeFeature)));

            PropertyInfo nodesProperty = type.GetProperty("Nodes");
            builder.AddCustomAttributes(type, nodesProperty, new ShowPropertyInOutlineViewAttribute());

            type = typeof(FlowStep);
            builder.AddCustomAttributes(type, type.GetProperty("Action"), BrowsableAttribute.No);
            builder.AddCustomAttributes(type, type.GetProperty("Next"), BrowsableAttribute.No);

            builder.AddCustomAttributes(type, new ShowInOutlineViewAttribute() { PromotedProperty = "Action" });
            builder.AddCustomAttributes(type, type.GetProperty("Next"), new ShowPropertyInOutlineViewAsSiblingAttribute());

            builder.AddCustomAttributes(typeof(FlowNode), new ShowInOutlineViewAttribute());
            builder.AddCustomAttributes(typeof(Collection<FlowNode>), new ShowInOutlineViewAttribute());

            CutCopyPasteHelper.AddDisallowedTypeForCopy(typeof(StartNode));
        }

        //Unregister all events. Reset startNodeAdded to enable reuse of the designer.
        void CleanupFlowchart()
        {
            this.startNodeAdded = false;
            this.panel.Children.Clear();
            this.flowNodeToUIElement.Clear();
            // Cleaning up the designers as they might be re-used.
            foreach (UIElement element in this.modelElement.Values)
            {
                element.MouseEnter -= new MouseEventHandler(ChildElement_MouseEnter);
                element.MouseLeave -= new MouseEventHandler(ChildElement_MouseLeave);
            }
            this.panel.LocationChanged -= new LocationChangedEventHandler(OnFreeFormPanelLocationChanged);
            this.panel.ConnectorMoved -= new ConnectorMovedEventHandler(OnFreeFormPanelConnectorMoved);
            this.panel.LayoutUpdated -= new EventHandler(OnFreeFormPanelLayoutUpdated);
            this.panel.RequiredSizeChanged -= new RequiredSizeChangedEventHandler(OnFreeFormPanelRequiredSizeChanged);
            this.panel = null;
            ModelTreeManager modelTreeManager = (this.ModelItem as IModelTreeItem).ModelTreeManager;
            modelTreeManager.EditingScopeCompleted -= new EventHandler<EditingScopeEventArgs>(ModelTreeManager_EditingScopeCompleted);
            this.ViewStateService.ViewStateChanged -= new ViewStateChangedEventHandler(OnViewStateChanged);
        }

        void OnFreeFormPanelLoaded(object sender, RoutedEventArgs eventArgs)
        {
            //Adding the following check because of 137896: Inside tab control multiple Loaded events happen without an Unloaded event.
            if (this.panel != null)
            {
                CleanupFlowchart();
            }
            this.panel = (FreeFormPanel)sender;
            if (this.ShowExpanded)
            {
                PopulateFlowchartChildren();
            }
        }

        void OnFreeFormPanelUnLoaded(object sender, RoutedEventArgs eventArgs)
        {
            if (object.Equals(sender, this.panel))
            {
                CleanupFlowchart();
            }
        }

        void PopulateFlowchartChildren()
        {
            Fx.Assert(this.ShowExpanded, "This method should be called only when the flowchart designer is shown expanded.");
            Fx.Assert(this.panel != null, "panel cannot be null");
            this.panel.LocationChanged += new LocationChangedEventHandler(OnFreeFormPanelLocationChanged);
            this.panel.ConnectorMoved += new ConnectorMovedEventHandler(OnFreeFormPanelConnectorMoved);
            this.panel.LayoutUpdated += new EventHandler(OnFreeFormPanelLayoutUpdated);
            this.panel.RequiredSizeChanged += new RequiredSizeChangedEventHandler(OnFreeFormPanelRequiredSizeChanged);

            DesignerPerfEventProvider perfEventProvider = this.Context.Services.GetService<DesignerPerfEventProvider>();
            perfEventProvider.FlowchartDesignerLoadStart();
            ModelTreeManager modelTreeManager = (this.ModelItem as IModelTreeItem).ModelTreeManager;
            modelTreeManager.EditingScopeCompleted += new EventHandler<EditingScopeEventArgs>(ModelTreeManager_EditingScopeCompleted);
            this.ViewStateService.ViewStateChanged += new ViewStateChangedEventHandler(OnViewStateChanged);

            this.startNodeAdded = false;
            panel.Children.Clear();
            this.modelElement.Clear();
            this.flowNodeToUIElement.Clear();
            this.shapeLocations.Clear();

            this.FlowchartWidth = (double)TypeDescriptor.GetProperties(this.ModelItem)[FlowchartSizeFeature.WidthPropertyName].GetValue(this.ModelItem);
            this.FlowchartHeight = (double)TypeDescriptor.GetProperties(this.ModelItem)[FlowchartSizeFeature.HeightPropertyName].GetValue(this.ModelItem);

            CreateStartSymbol();
            AddFlowElementsToDesigner(this.ModelItem.Properties["Nodes"].Collection, true);
            perfEventProvider.FlowchartDesignerLoadEnd();
        }

        //This is to keep this.selectedConnector upto date.
        //Eg. cases included 1. create a link, select it and undo, 2. Move a link from one shape to another.
        void OnFreeFormPanelLayoutUpdated(object sender, EventArgs e)
        {
            if (!this.panel.Children.Contains(this.selectedConnector))
            {
                this.selectedConnector = null;
            }
        }

        public UIElement StartSymbol { get; set; }

        internal static List<ConnectionPoint> GetConnectionPoints(DependencyObject obj)
        {
            return (List<ConnectionPoint>)obj.GetValue(FlowchartDesigner.ConnectionPointsProperty);
        }

        internal static ConnectionPoint GetFalseConnectionPoint(DependencyObject obj)
        {
            return (ConnectionPoint)obj.GetValue(FlowchartDesigner.FalseConnectionPointProperty);
        }

        internal static ModelItem GetLinkModelItem(DependencyObject obj)
        {
            return (ModelItem)obj.GetValue(FlowchartDesigner.LinkModelItemProperty);
        }

        internal static ModelItem GetFlowElementModelItem(DependencyObject obj)
        {
            return (ModelItem)obj.GetValue(FlowchartDesigner.FlowElementModelItemProperty);
        }

        internal static ConnectionPoint GetTrueConnectionPoint(DependencyObject obj)
        {
            return (ConnectionPoint)obj.GetValue(FlowchartDesigner.TrueConnectionPointProperty);
        }

        public double FlowchartWidth
        {
            get { return (double)this.GetValue(FlowchartDesigner.FlowchartWidthProperty); }
            set { this.SetValue(FlowchartDesigner.FlowchartWidthProperty, value); }
        }

        public double FlowchartHeight
        {
            get { return (double)this.GetValue(FlowchartDesigner.FlowchartHeightProperty); }
            set { this.SetValue(FlowchartDesigner.FlowchartHeightProperty, value); }
        }

        public bool ShowAllConditions
        {
            get { return (bool)GetValue(ShowAllConditionsProperty); }
            set { SetValue(ShowAllConditionsProperty, value); }
        }

        ModelItem GetFlowElementMI(ModelItem shapeModelItem)
        {
            Fx.Assert(this.modelElement.ContainsKey(shapeModelItem), "The ModelItem does not exist.");
            UIElement element = this.modelElement[shapeModelItem];
            ModelItem flowElementMI = FlowchartDesigner.GetFlowElementModelItem(element);
            Fx.Assert(flowElementMI != null, "FlowNode dependency property not set.");
            return flowElementMI;
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
        }

        //Returns actual link destination - Activity ModelItem in case of a FlowStep.
        ModelItem GetCorrespondingElementOnCanvas(ModelItem model)
        {
            ModelItem destModelItem = model;
            if (typeof(FlowStep).IsAssignableFrom(model.ItemType)
                && model.Properties["Action"].Value != null)
            {
                destModelItem = model.Properties["Action"].Value;
            }
            if (typeof(Flowchart) == model.ItemType)
            {
                destModelItem = flowStart;
            }
            return destModelItem;
        }

        private bool IsRightToLeft
        {
            get
            {
                if (!this.isRightToLeft.HasValue)
                {
                    this.isRightToLeft = FreeFormPanelUtilities.IsRightToLeft(this.flowchartContentPresenter);
                }

                return this.isRightToLeft.Value;
            }
        }

        private void OnFlowchartGridMouseLeave(object sender, MouseEventArgs e)
        {
            bool endLinkCreation = !IsVisualHit(sender as UIElement, sender as UIElement, e.GetPosition(sender as IInputElement));
            if (endLinkCreation)
            {
                RemoveAdorner(this.panel, typeof(ConnectorCreationAdorner));
                this.srcConnectionPoint = null;
            }
        }

        private void OnFlowchartGridMouseMove(object sender, MouseEventArgs e)
        {
            if (this.srcConnectionPoint != null)
            {
                AutoScrollHelper.AutoScroll(e, this, 1);
                Point[] points = ConnectorRouter.Route(this.panel, this.srcConnectionPoint, e);
                if (points == null)
                {
                    e.Handled = true;
                    return;
                }
                List<Point> segments = new List<Point>(points);
                //Remove the previous adorner.
                RemoveAdorner(this.panel, typeof(ConnectorCreationAdorner));
                //Add new adorner.
                AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(this.srcConnectionPoint.ParentDesigner);
                Fx.Assert(adornerLayer != null, "Adorner Layer does not exist");
                ConnectorCreationAdorner newAdorner = new ConnectorCreationAdorner(this.panel, segments);
                adornerLayer.Add(newAdorner);
                e.Handled = true;
            }

        }

        private void OnFlowchartGridMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (this.srcConnectionPoint != null)
            {
                RemoveAdorner(this.panel, typeof(ConnectorCreationAdorner));
                this.srcConnectionPoint = null;
            }
        }


        static void SetConnectionPoints(DependencyObject obj, List<ConnectionPoint> connectionPoints)
        {
            obj.SetValue(FlowchartDesigner.ConnectionPointsProperty, connectionPoints);
        }


        static void SetFalseConnectionPoint(DependencyObject obj, ConnectionPoint connectionPoint)
        {
            obj.SetValue(FlowchartDesigner.FalseConnectionPointProperty, connectionPoint);
        }

        static void SetLinkModelItem(DependencyObject obj, ModelItem modelItem)
        {
            obj.SetValue(FlowchartDesigner.LinkModelItemProperty, modelItem);
        }

        static void SetFlowElementModelItem(DependencyObject obj, ModelItem modelItem)
        {
            obj.SetValue(FlowchartDesigner.FlowElementModelItemProperty, modelItem);
        }

        static void SetTrueConnectionPoint(DependencyObject obj, ConnectionPoint connectionPoint)
        {
            obj.SetValue(FlowchartDesigner.TrueConnectionPointProperty, connectionPoint);
        }

        void ChildElement_MouseEnter(object sender, MouseEventArgs e)
        {
            Fx.Assert(this.panel != null, "This code should not be hit if panel is null");
            VirtualizedContainerService.VirtualizingContainer senderElement = sender as VirtualizedContainerService.VirtualizingContainer;
            if ((senderElement != null || sender is StartSymbol) && !this.IsReadOnly)
            {
                AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer((Visual)sender);
                Fx.Assert(adornerLayer != null, "Cannot get AdornerLayer.");
                ConnectionPointsAdorner adorner = null;
                if (sender is StartSymbol)
                {
                    adorner = new FlowchartConnectionPointsAdorner((UIElement)sender, ConnectionPointsToShow((UIElement)sender, this.ModelItem), false, this.IsRightToLeft);
                }
                else
                {
                    bool isSenderElementSelected = (((Selection)this.Context.Items.GetValue<Selection>()).SelectedObjects as ICollection<ModelItem>).Contains(senderElement.ModelItem);
                    adorner = new FlowchartConnectionPointsAdorner(senderElement, ConnectionPointsToShow(senderElement, senderElement.ModelItem), isSenderElementSelected, this.IsRightToLeft);
                }
                adornerLayer.Add(adorner);
                adorner.MouseDown += new MouseButtonEventHandler(ConnectionPoint_MouseDown);
                adorner.MouseUp += new MouseButtonEventHandler(ConnectionPoint_MouseUp);
                adorner.MouseLeave += new MouseEventHandler(ConnectionPoint_MouseLeave);
            }
        }


        //This method returns which connection points should be shown on hover of a shape.
        List<ConnectionPoint> ConnectionPointsToShow(UIElement element, ModelItem model)
        {
            bool isInComingConnection = false;

            //This condition checks if it is an incoming connection.
            if (this.srcConnectionPoint != null || (this.panel.connectorEditor != null && this.panel.connectorEditor.IsConnectorEndBeingMoved))
            {
                isInComingConnection = true;
            }
            List<ConnectionPoint> connectionPointsToShow = new List<ConnectionPoint>();

            if (GenericFlowSwitchHelper.IsGenericFlowSwitch(model.ItemType))
            {
                connectionPointsToShow.AddRange(FlowchartDesigner.GetConnectionPoints(element));
            }
            else if (typeof(FlowDecision).IsAssignableFrom(model.ItemType))
            {
                if (isInComingConnection)
                {
                    connectionPointsToShow.AddRange(FlowchartDesigner.GetConnectionPoints(element));
                }
                else
                {
                    connectionPointsToShow.Add(FlowchartDesigner.GetTrueConnectionPoint(element));
                    connectionPointsToShow.Add(FlowchartDesigner.GetFalseConnectionPoint(element));
                    List<Connector> outGoingConnectors = GetOutGoingConnectors(element);
                    if (this.panel.connectorEditor != null && this.panel.connectorEditor.IsConnectorStartBeingMoved)
                    {
                        //If the start of an outgoing connector is moved, its not an outgoing connector any more.
                        outGoingConnectors.Remove(this.panel.connectorEditor.Connector);
                    }
                    //Do not show True/False connection point if a link already exists.
                    foreach (Connector connector in outGoingConnectors)
                    {
                        connectionPointsToShow.Remove(FreeFormPanel.GetSourceConnectionPoint(connector));
                    }
                }
            }
            else// Case where only one out going connector is allowed - Start and FlowStep.
            {
                ConnectionPointKind allowedType = ConnectionPointKind.Default;
                bool isConnectionAllowed = false;
                if (isInComingConnection)
                {
                    allowedType = ConnectionPointKind.Incoming;
                    isConnectionAllowed = true;
                }
                else
                {
                    List<Connector> outGoingConnectors = GetOutGoingConnectors(element);
                    if (this.panel.connectorEditor != null && this.panel.connectorEditor.IsConnectorStartBeingMoved)
                    {
                        outGoingConnectors.Remove(this.panel.connectorEditor.Connector);
                    }
                    //Outgoing Connection is allowed only if there are no outgoing connectors already.
                    if (outGoingConnectors.Count == 0)
                    {
                        allowedType = ConnectionPointKind.Outgoing;
                        isConnectionAllowed = true;
                    }
                }

                if (isConnectionAllowed)
                {
                    foreach (ConnectionPoint connPoint in FlowchartDesigner.GetConnectionPoints(element))
                    {
                        if (connPoint.PointType == allowedType || connPoint.PointType == ConnectionPointKind.Default)
                        {
                            connectionPointsToShow.Add(connPoint);
                        }
                    }
                }
            }
            //Do not show the connection points of a selected connector.
            if (this.selectedConnector != null)
            {
                connectionPointsToShow.Remove(FreeFormPanel.GetSourceConnectionPoint(this.selectedConnector));
                connectionPointsToShow.Remove(FreeFormPanel.GetDestinationConnectionPoint(this.selectedConnector));
            }
            return connectionPointsToShow;
        }


        void ChildElement_MouseLeave(object sender, MouseEventArgs e)
        {
            Fx.Assert(this.panel != null, "This code should not be hit if panel is null");
            bool removeConnectionPointsAdorner = true;
            if (Mouse.DirectlyOver != null)
            {
                removeConnectionPointsAdorner = !typeof(ConnectionPointsAdorner).IsAssignableFrom(Mouse.DirectlyOver.GetType());
            }
            if (removeConnectionPointsAdorner)
            {
                RemoveAdorner(sender as UIElement, typeof(ConnectionPointsAdorner));
            }
        }


        void ChildSizeChanged(object sender, SizeChangedEventArgs e)
        {
            Fx.Assert(this.panel != null, "This code should not be hit if panel is null");
            VirtualizedContainerService.VirtualizingContainer container = sender as VirtualizedContainerService.VirtualizingContainer;
            if (container != null || sender is StartSymbol)
            {
                this.internalViewStateChange = true;
                //Initializing storageModelItem for the case of FlowchartStartNode.
                ModelItem storageModelItem = this.ModelItem;
                if (container != null)
                {
                    storageModelItem = GetFlowElementMI(container.ModelItem);
                }
                this.ViewStateService.StoreViewState(storageModelItem, shapeSize, ((UIElement)sender).DesiredSize);
                this.internalViewStateChange = false;
            }
        }

        void ConnectionPoint_MouseDown(object sender, MouseButtonEventArgs e)
        {
            UIElement srcElement = ((Adorner)sender).AdornedElement as UIElement;
            this.srcConnectionPoint = ConnectionPointHitTest(srcElement, e.GetPosition(this.panel));
            e.Handled = true;
        }

        void ConnectionPoint_MouseLeave(object sender, MouseEventArgs e)
        {
            Fx.Assert(this.panel != null, "This code should not be hit if panel is null");
            UIElement adornedElement = ((Adorner)sender).AdornedElement as UIElement;
            RemoveAdorner(adornedElement, typeof(ConnectionPointsAdorner));
        }


        void ConnectionPoint_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Fx.Assert(this.panel != null, "This code should not be hit if panel is null");
            UIElement dest = ((Adorner)sender).AdornedElement as UIElement;
            Fx.Assert(dest != null, "Adorned element is not a UIElement");
            if (this.srcConnectionPoint != null)
            {
                ConnectionPoint destConnectionPoint = ConnectionPointHitTest(dest, e.GetPosition(this.panel));
                if (destConnectionPoint != null && !this.srcConnectionPoint.Equals(destConnectionPoint))
                {
                    string errorMessage = string.Empty;
                    if (!CreateLinkGesture(this.srcConnectionPoint, destConnectionPoint, out errorMessage, null) && !errorMessage.Equals(string.Empty))
                    {
                        ErrorReporting.ShowErrorMessage(errorMessage);
                    }
                }
                this.srcConnectionPoint = null;
                RemoveAdorner(this.panel, typeof(ConnectorCreationAdorner));
                RemoveAdorner(dest, typeof(FlowchartConnectionPointsAdorner));
            }
            else
            {
                //This will cause the FreeFormPanel to handle the event and is useful while moving connection end points of a connector.
                lastConnectionPointMouseUpElement = dest;
                dest.RaiseEvent(e);
            }
        }

        void OnFreeFormPanelRequiredSizeChanged(object sender, RequiredSizeChangedEventArgs e)
        {
            Dispatcher.BeginInvoke(() =>
            {
                // Access the view state dictionary directly to avoid generating an undo item because of ViewStateAttachedPropertyFeature implementation.
                Dictionary<string, object> viewState = WorkflowViewStateService.GetViewState(this.ModelItem.GetCurrentValue());
                if (viewState == null)
                {
                    viewState = new Dictionary<string, object>();
                    WorkflowViewStateService.SetViewState(this.ModelItem.GetCurrentValue(), viewState);
                }

                if (e.NewRequiredSize.Width > this.FlowchartWidth)
                {
                    viewState[FlowchartSizeFeature.WidthPropertyName] = e.NewRequiredSize.Width;
                    this.FlowchartWidth = e.NewRequiredSize.Width;
                }

                if (e.NewRequiredSize.Height > this.FlowchartHeight)
                {
                    viewState[FlowchartSizeFeature.HeightPropertyName] = e.NewRequiredSize.Height;
                    this.FlowchartHeight = e.NewRequiredSize.Height;
                }
            });
        }

        void OnFreeFormPanelLocationChanged(object sender, System.Activities.Presentation.FreeFormEditing.LocationChangedEventArgs e)
        {
            Fx.Assert(this.panel != null, "This code should not be hit if panel is null");
            Fx.Assert(sender is UIElement, "Sender should be of type UIElement");
            Connector movedConnector = sender as Connector;
            if (movedConnector != null)
            {
                //ViewState is undoable only when a user gesture moves a connector. If the freeformpanel routes a connector,
                //the change is not undoable.
                bool isUndoableViewState = false;
                ModelItem linkModelItem = FlowchartDesigner.GetLinkModelItem(movedConnector);
                ConnectionPoint source = FreeFormPanel.GetSourceConnectionPoint(movedConnector);
                string viewStateKey = GetConnectorViewStateKey(linkModelItem, source);
                ModelItem storageModelItem = GetConnectorViewStateStorageModelItem(linkModelItem);
                PointCollection existingVS = this.ViewStateService.RetrieveViewState(storageModelItem, viewStateKey) as PointCollection;
                if (existingVS != null && existingVS.Count > 0 && movedConnector.Points.Count > 0
                    && existingVS[0].Equals(movedConnector.Points[0]) && existingVS[existingVS.Count - 1].Equals(movedConnector.Points[movedConnector.Points.Count - 1]))
                {
                    isUndoableViewState = true;
                }
                StoreConnectorViewState(movedConnector, isUndoableViewState);
            }
            else
            {
                //Save the location property of each shape on the CFx object for serialization and viewstate maintenance.
                //This is called only when a shape without viewstate is autolayed out by the freeform panel.
                VirtualizedContainerService.VirtualizingContainer container = sender as VirtualizedContainerService.VirtualizingContainer;
                if (container != null)
                {
                    StoreShapeViewState(container, e.NewLocation);
                }
            }
        }


        void UpdateFlowchartOnLinkVisualMoved(ConnectionPoint knownConnectionPoint, Point newPoint, Connector movedConnector, bool isSourceKnown)
        {
            Fx.Assert(this.panel != null, "This code should not be hit if panel is null");
            HitTestResult hitTestResult = VisualTreeHelper.HitTest(this.panel, newPoint);
            if (hitTestResult == null)
            {
                return;
            }
            //Test if the last connectionPoint hit, is the new location for the connector.
            UIElement newViewElement = null;
            ConnectionPoint newConnectionPoint = null;

            //The case where the link is dropped on a connectionpoint.
            if (this.lastConnectionPointMouseUpElement != null)
            {
                newConnectionPoint = this.ConnectionPointHitTest(this.lastConnectionPointMouseUpElement, newPoint);
                if (newConnectionPoint != null)
                {
                    newViewElement = this.lastConnectionPointMouseUpElement;
                }
            }
            //The case where the link is dropped on a shape.
            if (newViewElement == null)
            {
                newViewElement = VisualTreeUtils.FindVisualAncestor<StartSymbol>(hitTestResult.VisualHit);
                if (newViewElement == null)
                {
                    newViewElement = VisualTreeUtils.FindVisualAncestor<VirtualizedContainerService.VirtualizingContainer>(hitTestResult.VisualHit);
                }
            }
            if (newViewElement != null)
            {
                if (this.panel.Children.Contains(newViewElement))
                {
                    using (EditingScope es = (EditingScope)this.ModelItem.BeginEdit(SR.FCLinkMove))
                    {
                        //Delete the existing link and keep the caseKey
                        IFlowSwitchLink oldCaseKey = this.DeleteLink(movedConnector, true);

                        //Create new link
                        bool linkCreated = false;
                        string errorMessage = string.Empty;
                        if (isSourceKnown)
                        {
                            if (newConnectionPoint == null)
                            {
                                linkCreated = CreateLinkGesture(knownConnectionPoint, newViewElement, newPoint, out errorMessage, true, oldCaseKey);
                            }
                            else
                            {
                                linkCreated = CreateLinkGesture(knownConnectionPoint, newConnectionPoint, out errorMessage, true, oldCaseKey);
                            }
                        }
                        else
                        {
                            //If the Link source is dropped onto itself, we need to set the isLinkValidDueToLinkMove flag.
                            bool isLinkValidDueToLinkMove = FreeFormPanel.GetSourceConnectionPoint(movedConnector).ParentDesigner.Equals(newViewElement);
                            if (newConnectionPoint == null)
                            {
                                linkCreated = CreateLinkGesture(newViewElement, knownConnectionPoint, newPoint, out errorMessage, isLinkValidDueToLinkMove, oldCaseKey);
                            }
                            else
                            {
                                linkCreated = CreateLinkGesture(newConnectionPoint, knownConnectionPoint, out errorMessage, isLinkValidDueToLinkMove, oldCaseKey);
                            }
                        }
                        if (!linkCreated)
                        {
                            if (!errorMessage.Equals(string.Empty))
                            {
                                ErrorReporting.ShowErrorMessage(errorMessage);
                            }
                            es.Revert();
                        }
                        else
                        {
                            es.Complete();
                        }
                    }
                }
            }
        }

        void OnFreeFormPanelConnectorMoved(object sender, ConnectorMovedEventArgs e)
        {
            Fx.Assert(this.panel != null, "This code should not be hit if panel is null");
            Connector movedConnector = sender as Connector;
            int movedEndConnectorPointIndex = movedConnector.Points.Count - 1;
            int newEndConnectorPointIndex = e.NewConnectorLocation.Count - 1;

            if (movedConnector != null)
            {
                Fx.Assert(e.NewConnectorLocation.Count > 0, "Invalid connector editor");
                if (!e.NewConnectorLocation[0].Equals(movedConnector.Points[0]))
                {
                    //srcMoved
                    ConnectionPoint destConnPoint = FreeFormPanel.GetDestinationConnectionPoint(movedConnector);
                    UpdateFlowchartOnLinkVisualMoved(destConnPoint, e.NewConnectorLocation[0], movedConnector, false);
                }
                else if (!e.NewConnectorLocation[newEndConnectorPointIndex].Equals(movedConnector.Points[movedEndConnectorPointIndex]))
                {
                    //DestMoved
                    ConnectionPoint srcConnPoint = FreeFormPanel.GetSourceConnectionPoint(movedConnector);
                    Point destPoint = e.NewConnectorLocation[newEndConnectorPointIndex];
                    UpdateFlowchartOnLinkVisualMoved(srcConnPoint, destPoint, movedConnector, true);
                }

                this.selectedConnector = movedConnector;
            }
        }

        MultiBinding GetConnectionPointBinding(FrameworkElement element, double widthFraction, double heightFraction)
        {
            Fx.Assert(element != null, "FrameworkElement is null.");
            MultiBinding bindings = new MultiBinding();
            Binding sizeBinding = new Binding();
            sizeBinding.Source = element;
            sizeBinding.Path = new PropertyPath(FreeFormPanel.ChildSizeProperty);
            Binding locationBinding = new Binding();
            locationBinding.Source = element;
            locationBinding.Path = new PropertyPath(FreeFormPanel.LocationProperty);
            bindings.Bindings.Add(sizeBinding);
            bindings.Bindings.Add(locationBinding);
            bindings.Converter = new ConnectionPointConverter();
            bindings.ConverterParameter = new List<Object> { widthFraction, heightFraction, element.Margin };
            return bindings;
        }


        Connector GetConnector(ModelItem linkModelItem, ConnectionPoint srcConnPoint, ConnectionPoint destConnPoint)
        {
            Fx.Assert(this.panel != null, "This code should not be hit if panel is null");
            ConnectorWithoutStartDot connector = new ConnectorWithoutStartDot();
            connector.FocusVisualStyle = null;
            connector.Focusable = true;
            DesignerView.SetCommandMenuMode(connector, CommandMenuMode.NoCommandMenu);
            SetConnectorLabel(connector, srcConnPoint, linkModelItem);

            connector.GotKeyboardFocus += new KeyboardFocusChangedEventHandler(OnConnectorGotKeyboardFocus);
            connector.RequestBringIntoView += new RequestBringIntoViewEventHandler(OnConnectorRequestBringIntoView);
            connector.MouseDown += new MouseButtonEventHandler(OnConnectorMouseDown);
            connector.GotFocus += new RoutedEventHandler(OnConnectorGotFocus);
            SetConnectorSrcDestConnectionPoints(connector, srcConnPoint, destConnPoint);
            FlowchartDesigner.SetLinkModelItem(connector, linkModelItem);
            connector.Unloaded += new RoutedEventHandler(OnConnectorUnloaded);
            connector.AutoSplitContainer = this;
            return connector;
        }

        // To prevent the parent FlowchartDesigner from handling mouse events and setting the selection to itself.
        void OnConnectorMouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        void SetConnectorLabel(Connector connector, ConnectionPoint srcConnPoint, ModelItem linkModelItem)
        {
            BindingBase labelBinding = null;

            if (typeof(FlowDecision).IsAssignableFrom(linkModelItem.ItemType))
            {
                if (FlowchartDesigner.GetTrueConnectionPoint(srcConnPoint.ParentDesigner).Equals(srcConnPoint))
                {
                    labelBinding = new Binding { Source = linkModelItem, Path = new PropertyPath("TrueLabel") };
                }
                else
                {
                    labelBinding = new Binding { Source = linkModelItem, Path = new PropertyPath("FalseLabel") };
                }

                SetConnectorLabelToolTip(connector, labelBinding);
            }
            else if (typeof(IFlowSwitchLink).IsAssignableFrom(linkModelItem.ItemType))
            {
                IFlowSwitchLink flowSwitchLink = (IFlowSwitchLink)linkModelItem.GetCurrentValue();
                labelBinding = flowSwitchLink.CreateConnectorLabelTextBinding();
                SetConnectorLabelToolTip(connector, labelBinding);
            }
        }

        void SetConnectorLabelToolTip(Connector connector, BindingBase binding)
        {
            connector.SetBinding(Connector.LabelTextProperty, binding);
            ToolTip toolTip = new ToolTip();
            toolTip.SetBinding(UserControl.ContentProperty, binding);
            connector.SetLabelToolTip(toolTip);
        }

        void OnConnectorUnloaded(object sender, RoutedEventArgs e)
        {
            ModelItem primarySelection = this.Context.Items.GetValue<Selection>().PrimarySelection;
            if (object.Equals(primarySelection, FlowchartDesigner.GetLinkModelItem(sender as DependencyObject)))
            {
                if (primarySelection != null)
                {
                    Selection.Toggle(this.Context, primarySelection);
                }
            }
        }

        //Marking e.Handled = true to avoid scrolling in large workflows to bring the
        //area of a connector in the center of the view region.
        //Area covered by a connector includes the region between 0,0 of the panel and the edges of the connector.
        void OnConnectorRequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
        {
            e.Handled = true;
        }



        //Returns a new connector if viewstate exists, null otherwise.
        Connector GetConnectorViewState(UIElement source, UIElement dest, ModelItem linkModelItem, ConnectionPoint sourceConnectionPoint)
        {
            Fx.Assert(this.panel != null, "This code should not be hit if panel is null");
            Connector connector = null;
            object connectorLocation = null;
            if (typeof(FlowDecision).IsAssignableFrom(linkModelItem.ItemType))
            {
                Fx.Assert(sourceConnectionPoint != null, "Source connection point is null.");
                if (sourceConnectionPoint.Equals(FlowchartDesigner.GetTrueConnectionPoint(this.modelElement[linkModelItem])))
                {
                    connectorLocation = this.ViewStateService.RetrieveViewState(linkModelItem, TrueConnectorViewStateKey);
                }
                else
                {
                    connectorLocation = this.ViewStateService.RetrieveViewState(linkModelItem, FalseConnectorViewStateKey);
                }
            }
            else if (typeof(IFlowSwitchLink).IsAssignableFrom(linkModelItem.ItemType))
            {
                string key = null;
                IFlowSwitchLink link = (IFlowSwitchLink)linkModelItem.GetCurrentValue();
                if (link.IsDefaultCase)
                {
                    key = FlowSwitchDefaultViewStateKey;
                }
                else
                {
                    key = link.CaseName + CaseViewStateKeyAppendString;
                }
                //Transitioning from fake ModelItem world to real ModelItem world.
                ModelItem realFSModelItem = (this.ModelItem as IModelTreeItem).ModelTreeManager.WrapAsModelItem(link.ParentFlowSwitch);
                connectorLocation = this.ViewStateService.RetrieveViewState(realFSModelItem, key);
            }
            else
            {
                connectorLocation = this.ViewStateService.RetrieveViewState(linkModelItem, ConnectorViewStateKey);
            }
            PointCollection locationPts = connectorLocation as PointCollection;
            if (locationPts != null)
            {
                ConnectionPoint srcConnPoint, destConnPoint;
                System.Diagnostics.Debug.WriteLine(this.isLoaded ? "About to call ConnectionPointHitTest - Loaded" : "About to call ConnectionPointHitTest - Not Loaded");
                srcConnPoint = ConnectionPointHitTest(source, locationPts[0]);
                destConnPoint = ConnectionPointHitTest(dest, locationPts[locationPts.Count - 1]);
                //In Debug mode, the size of the designer changes due to the debug adorner(border). Because of this connection points will move and
                //won't coincide with the viewstate.
                //The following code path is added for the scenario where we reload the flowchart designer by navigating back and forth on breadcrumb
                //when one of the flowchart activities has the debug border.
                //In this scenario we try to find the closest connection point from the end point stored in viewstate. If the distance between the two
                //is within the acceptable range, we will reuse the viewstate and avoid re-drawing the connector.
                if (this.IsReadOnly)
                {
                    ConnectionPoint pt;
                    double dist;
                    if (srcConnPoint == null)
                    {
                        pt = FindClosestConnectionPoint(locationPts[0], FlowchartDesigner.GetConnectionPoints(source), out dist);
                        if (pt != null && pt.PointType != ConnectionPointKind.Incoming && dist <= DebugTimeMaxConnectorShapeDist)
                        {
                            srcConnPoint = pt;
                        }
                    }
                    if (destConnPoint == null)
                    {
                        pt = FindClosestConnectionPoint(locationPts[locationPts.Count - 1], FlowchartDesigner.GetConnectionPoints(dest), out dist);
                        if (pt != null && pt.PointType != ConnectionPointKind.Outgoing && dist <= DebugTimeMaxConnectorShapeDist)
                        {
                            destConnPoint = pt;
                        }
                    }
                }
                if (srcConnPoint != null && destConnPoint != null)
                {
                    connector = GetConnector(linkModelItem, srcConnPoint, destConnPoint);
                    connector.Points = locationPts;
                }
            }
            return connector;
        }


        //Marking e.Handled true for the case where a connector is clicked on.
        //This is to prevent WorkflowViewElement class from making Flowchart as the current selection.
        void OnConnectorGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            e.Handled = true;
        }

        void OnConnectorGotFocus(object sender, RoutedEventArgs e)
        {
            Connector clickedLine = e.Source as Connector;
            DesignerView designerView = this.Context.Services.GetService<DesignerView>();

            if (!designerView.IsMultipleSelectionMode)
            {

                if (this.panel.connectorEditor == null || !clickedLine.Equals(this.panel.connectorEditor.Connector))
                {
                    this.panel.RemoveConnectorEditor();
                    this.panel.connectorEditor = new ConnectorEditor(this.panel, clickedLine);
                }

                if (this.panel.Children.Contains(clickedLine))
                {
                    this.updatingSelectedConnector = true;
                    ModelItem lineModelItem = FlowchartDesigner.GetLinkModelItem(clickedLine);
                    Selection newSelection = new Selection();
                    // If the linkModelItem is FlowDecision or Flowchart, we don't want to add it to the selection
                    if (IsLinkModelItemSelectable(lineModelItem))
                    {
                        newSelection = new Selection(lineModelItem);
                    }
                    this.Context.Items.SetValue(newSelection);
                    this.selectedConnector = clickedLine;
                    this.updatingSelectedConnector = false;
                    e.Handled = true;
                }
            }
        }

        private void OnSelectionChanged(Selection selection)
        {
            // If selection changed, remove ConnectorEditor if existed.
            // Only if the selection changed is caused by adding ConnectorEditor when OnConnectorGotFocus, ignore.
            if (!this.updatingSelectedConnector && this.panel != null && this.panel.connectorEditor != null)
            {
                this.panel.RemoveConnectorEditor();
            }
        }

        //widthFraction, heightFraction determine location of connectionpoint on the shape.
        ConnectionPoint CreateConnectionPoint(UIElement element, double widthFraction, double heightFraction, EdgeLocation location)
        {
            ConnectionPoint connectionPoint = new ConnectionPoint();
            connectionPoint.EdgeLocation = location;
            connectionPoint.PointType = ConnectionPointKind.Default;
            connectionPoint.ParentDesigner = element;
            BindingOperations.SetBinding(connectionPoint, ConnectionPoint.LocationProperty, GetConnectionPointBinding(element as FrameworkElement, widthFraction, heightFraction));
            return connectionPoint;
        }

        void PopulateConnectionPoints(UIElement element, ModelItem model)
        {
            element.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            List<ConnectionPoint> connectionPoints;
            if (model != null &&
                GenericFlowSwitchHelper.IsGenericFlowSwitch(model.ItemType))
            {
                connectionPoints = new List<ConnectionPoint>
                    {
                        //Top edge
                        CreateConnectionPoint(element, 0.25, 0, EdgeLocation.Top),
                        CreateConnectionPoint(element, 0.5, 0, EdgeLocation.Top),
                        CreateConnectionPoint(element, 0.75, 0, EdgeLocation.Top),
                        //Right edge
                        CreateConnectionPoint(element, 1, 0.25, EdgeLocation.Right),
                        CreateConnectionPoint(element, 1, 0.50, EdgeLocation.Right),
                        CreateConnectionPoint(element, 1, 0.75, EdgeLocation.Right),
                        //Bottom edge
                        CreateConnectionPoint(element, 0.25, 1, EdgeLocation.Bottom),
                        CreateConnectionPoint(element, 0.5, 1, EdgeLocation.Bottom),
                        CreateConnectionPoint(element, 0.75, 1, EdgeLocation.Bottom),
                        //Left edge
                        CreateConnectionPoint(element, 0, 0.25, EdgeLocation.Left),
                        CreateConnectionPoint(element, 0, 0.50, EdgeLocation.Left),
                        CreateConnectionPoint(element, 0, 0.75, EdgeLocation.Left),
                    };
            }
            else if (model != null && typeof(FlowDecision).IsAssignableFrom(model.ItemType))
            {
                ConnectionPoint trueConnectionPoint = CreateConnectionPoint(element, 0, 0.50, EdgeLocation.Left);
                trueConnectionPoint.PointType = ConnectionPointKind.Outgoing;
                FlowchartDesigner.SetTrueConnectionPoint(element, trueConnectionPoint);

                ConnectionPoint falseConnectionPoint = CreateConnectionPoint(element, 1, 0.50, EdgeLocation.Right);
                falseConnectionPoint.PointType = ConnectionPointKind.Outgoing;
                FlowchartDesigner.SetFalseConnectionPoint(element, falseConnectionPoint);

                connectionPoints = new List<ConnectionPoint>
                    {
                        //Top edge
                        CreateConnectionPoint(element, 0.25, 0, EdgeLocation.Top),
                        CreateConnectionPoint(element, 0.5, 0, EdgeLocation.Top),
                        CreateConnectionPoint(element, 0.75, 0, EdgeLocation.Top),
                        //Bottom edge
                        CreateConnectionPoint(element, 0.5, 1, EdgeLocation.Bottom),
                    };
                connectionPoints.ForEach((point) => point.PointType = ConnectionPointKind.Incoming);
            }
            else
            {
                //First adding top, right, bottom and left default connection points in that order on all shapes other than flowswitch.
                //For shapes that do not need any of the points, we will remove that point explicitly.
                connectionPoints = new List<ConnectionPoint>
                    {
                        CreateConnectionPoint(element, 0.5, 0, EdgeLocation.Top),
                        CreateConnectionPoint(element, 1, 0.5, EdgeLocation.Right),
                        CreateConnectionPoint(element, 0.5, 1, EdgeLocation.Bottom),
                        CreateConnectionPoint(element, 0, 0.5, EdgeLocation.Left)
                    };
            }

            if (model == null) // Start symbol: model = null
            {
                foreach (ConnectionPoint connPoint in connectionPoints)
                {
                    connPoint.PointType = ConnectionPointKind.Outgoing;
                }
            }
            FlowchartDesigner.SetConnectionPoints(element, connectionPoints);
        }

        void SetFlowElementModelItem(UIElement view, ModelItem model)
        {
            ModelItem flowElementMI = model;
            if (flowElementMI != null && !IsFlowNode(flowElementMI))
            {
                ModelItem flowStepMI = null;
                //Select the right FlowStep ModelItem out of view.ModelItem.Parents.
                foreach (ModelItem parentModelItem in flowElementMI.Parents)
                {
                    if (IsFlowNode(parentModelItem)
                        && this.ModelItem.Properties["Nodes"].Collection.Contains(parentModelItem))
                    {
                        flowStepMI = parentModelItem;
                        break;
                    }
                }
                flowElementMI = flowStepMI;
            }
            Fx.Assert(flowElementMI != null, "Non FlowNode present on Flowchart");
            FlowchartDesigner.SetFlowElementModelItem(view as DependencyObject, flowElementMI);
            this.flowNodeToUIElement[flowElementMI] = view;

        }

        UIElement ProcessAndGetModelView(ModelItem model)
        {
            Fx.Assert(this.panel != null, "This code should not be hit if panel is null");
            UIElement container;
            if (!this.modelElement.TryGetValue(model, out container))
            {
                VirtualizedContainerService containerService = this.Context.Services.GetService<VirtualizedContainerService>();
                Fx.Assert(this.ViewService != null, "ViewService is null");
                container = containerService.GetContainer(model, this);
                //WorkflowViewElement view = (WorkflowViewElement)this.ViewService.GetView(model);
                //Fx.Assert(view != null, "View does not exist for a model Item");
                //DragDropHelper.SetSourceContainer(view, this);
                //element = (UIElement)view;
                //element.MouseEnter += new MouseEventHandler(ChildElement_MouseEnter);
                //element.MouseLeave += new MouseEventHandler(ChildElement_MouseLeave);
                container.MouseEnter += new MouseEventHandler(ChildElement_MouseEnter);
                container.MouseLeave += new MouseEventHandler(ChildElement_MouseLeave);

                ((FrameworkElement)container).SizeChanged += new SizeChangedEventHandler(ChildSizeChanged);
                this.modelElement.Add(model, container);
                PopulateConnectionPoints(container, model);
                this.SetFlowElementModelItem(container, model);

                //Getting the View state information.
                ModelItem flowElementMI = GetFlowElementMI(model);
                object locationOfShape = this.ViewStateService.RetrieveViewState(flowElementMI, shapeLocation);
                object sizeOfShape = this.ViewStateService.RetrieveViewState(flowElementMI, shapeSize);
                if (locationOfShape != null)
                {
                    Point locationPt = (Point)locationOfShape;
                    FreeFormPanel.SetLocation(container, locationPt);
                    this.shapeLocations.Add(locationPt);
                }
                if (sizeOfShape != null)
                {
                    Size size = (Size)sizeOfShape;
                    FreeFormPanel.SetChildSize(container, size);
                    VirtualizedContainerService.VirtualizingContainer virtualizingContainer = container as VirtualizedContainerService.VirtualizingContainer;
                    if (virtualizingContainer != null)
                    {
                        virtualizingContainer.MinWidth = size.Width;
                        virtualizingContainer.MinHeight = size.Height;
                    }

                }
            }
            return container;
        }

        void GetSrcDestConnectionPoints(UIElement source, UIElement dest, out ConnectionPoint srcConnPoint, out ConnectionPoint destConnPoint, out string errorMessage)
        {
            srcConnPoint = null;
            destConnPoint = null;
            errorMessage = string.Empty;
            VirtualizedContainerService.VirtualizingContainer sourceContainer = source as VirtualizedContainerService.VirtualizingContainer;
            if (sourceContainer != null && typeof(FlowDecision).IsAssignableFrom(sourceContainer.ModelItem.ItemType))
            {
                srcConnPoint = FindFlowDecisionSrcConnectionPoint(source, out errorMessage);
                if (srcConnPoint != null)
                {
                    destConnPoint = FindBestMatchDestConnectionPoint(srcConnPoint, dest, out errorMessage);
                }
            }
            else
            {

                List<ConnectionPoint> srcConnectionPoints = FlowchartDesigner.GetConnectionPoints(source);
                List<ConnectionPoint> destConnectionPoints = FlowchartDesigner.GetConnectionPoints(dest);
                if (sourceContainer != null && GenericFlowSwitchHelper.IsGenericFlowSwitch(sourceContainer.ModelItem.ItemType))
                {
                    FindBestMatchConnectionPointPair(srcConnectionPoints, destConnectionPoints, out srcConnPoint, out destConnPoint);
                }
                else
                {
                    // Flowstep
                    FindBestMatchConnectionPointPair(srcConnectionPoints, destConnectionPoints, out srcConnPoint, out destConnPoint);
                }
            }
        }

        //This returns the closest non-outgoing connectionPoint on dest. Return value will be different than sourceConnectionPoint.
        ConnectionPoint ClosestDestConnectionPoint(ConnectionPoint sourceConnectionPoint, UIElement dest, out string errorMessage)
        {
            ConnectionPoint destConnectionPoint = null;
            errorMessage = string.Empty;
            if (sourceConnectionPoint.PointType != ConnectionPointKind.Incoming)
            {
                destConnectionPoint = FindClosestConnectionPointNotOfType(sourceConnectionPoint, FlowchartDesigner.GetConnectionPoints(dest), ConnectionPointKind.Outgoing);
            }
            else
            {
                errorMessage = SR.FCInvalidLink;
            }
            return destConnectionPoint;

        }

        //This returns the closest non-Incoming connectionPoint on source. Return value will be different than destConnectionPoint.
        ConnectionPoint ClosestSrcConnectionPoint(UIElement src, ConnectionPoint destConnectionPoint, out string errorMessage)
        {
            ConnectionPoint sourceConnectionPoint = null;
            errorMessage = string.Empty;
            if (destConnectionPoint.PointType != ConnectionPointKind.Outgoing)
            {
                VirtualizedContainerService.VirtualizingContainer srcContainer = src as VirtualizedContainerService.VirtualizingContainer;
                if (srcContainer != null && typeof(FlowDecision).IsAssignableFrom(srcContainer.ModelItem.ItemType))
                {
                    sourceConnectionPoint = FindFlowDecisionSrcConnectionPoint(src, out errorMessage);
                }
                else
                {
                    sourceConnectionPoint = FindClosestConnectionPointNotOfType(destConnectionPoint, FlowchartDesigner.GetConnectionPoints(src), ConnectionPointKind.Incoming);
                }
            }
            else
            {
                errorMessage = SR.FCInvalidLink;
            }
            return sourceConnectionPoint;
        }

        //Priority of selection: 1st true then false.
        ConnectionPoint FindFlowDecisionSrcConnectionPoint(UIElement decisionDesigner, out string errorMessage)
        {
            ConnectionPoint sourceConnectionPoint = null;
            errorMessage = string.Empty;
            ConnectionPoint trueConnPoint = FlowchartDesigner.GetTrueConnectionPoint(decisionDesigner);
            ConnectionPoint falseConnPoint = FlowchartDesigner.GetFalseConnectionPoint(decisionDesigner);
            if (trueConnPoint.AttachedConnectors.Count == 0)
            {
                sourceConnectionPoint = trueConnPoint;
            }
            else if (falseConnPoint.AttachedConnectors.Count == 0)
            {
                sourceConnectionPoint = falseConnPoint;
            }
            else
            {
                errorMessage = SR.FCFlowConditionLinksExist;
            }
            return sourceConnectionPoint;
        }

        void SetConnectorSrcDestConnectionPoints(Connector connector, ConnectionPoint sourceConnectionPoint, ConnectionPoint destConnectionPoint)
        {
            FreeFormPanel.SetSourceConnectionPoint(connector, sourceConnectionPoint);
            FreeFormPanel.SetDestinationConnectionPoint(connector, destConnectionPoint);
            sourceConnectionPoint.AttachedConnectors.Add(connector);
            destConnectionPoint.AttachedConnectors.Add(connector);
        }

        //Save the connector.Points property on the CFx object for serialization and viewstate maintenance.
        void StoreConnectorViewState(ModelItem linkModelItem, PointCollection viewState, ConnectionPoint srcConnPoint, bool isUndoableViewState)
        {
            ModelItem storageModelItem = GetConnectorViewStateStorageModelItem(linkModelItem);
            string viewStateKey = GetConnectorViewStateKey(linkModelItem, srcConnPoint);
            StoreConnectorViewState(storageModelItem, viewStateKey, viewState, isUndoableViewState);
        }

        void StoreConnectorViewState(ModelItem storageModelItem, string viewStateKey, PointCollection viewState, bool isUndoableViewState)
        {
            if (isUndoableViewState)
            {
                this.ViewStateService.StoreViewStateWithUndo(storageModelItem, viewStateKey, viewState);
            }
            else
            {
                this.ViewStateService.StoreViewState(storageModelItem, viewStateKey, viewState);
            }
        }

        void StoreConnectorViewState(ModelItem linkModelItem, PointCollection viewState, ConnectionPoint srcConnPoint)
        {
            StoreConnectorViewState(linkModelItem, viewState, srcConnPoint, true);
        }

        void StoreConnectorViewState(Connector connector, bool isUndoableViewState)
        {
            //This method will be called whenever the FreeFormPanel raises a location changed event on a connector.
            //Such location changed events are a result of changes already commited in the UI. Hence we do not want to react to such view state changes.
            //Using internalViewStateChange flag for that purpose.
            this.internalViewStateChange = true;
            this.StoreConnectorViewState(FlowchartDesigner.GetLinkModelItem(connector), connector.Points, FreeFormPanel.GetSourceConnectionPoint(connector), isUndoableViewState);
            this.internalViewStateChange = false;
        }

        string GetConnectorViewStateKey(ModelItem linkModelItem, ConnectionPoint srcConnPoint)
        {
            string viewStateKey = ConnectorViewStateKey;
            if ((typeof(FlowDecision).IsAssignableFrom(linkModelItem.ItemType)))
            {
                if (srcConnPoint.Equals(FlowchartDesigner.GetTrueConnectionPoint(this.modelElement[linkModelItem])))
                {
                    viewStateKey = TrueConnectorViewStateKey;
                }
                else
                {
                    viewStateKey = FalseConnectorViewStateKey;
                }
            }
            else if (typeof(IFlowSwitchLink).IsAssignableFrom(linkModelItem.ItemType))
            {
                IFlowSwitchLink link = (IFlowSwitchLink)linkModelItem.GetCurrentValue();
                if (link.IsDefaultCase)
                {
                    viewStateKey = FlowSwitchDefaultViewStateKey;
                }
                else
                {
                    viewStateKey = link.CaseName + CaseViewStateKeyAppendString;
                }
            }
            return viewStateKey;
        }

        ModelItem GetConnectorViewStateStorageModelItem(ModelItem linkModelItem)
        {
            ModelItem storageModelItem = linkModelItem;
            if (typeof(IFlowSwitchLink).IsAssignableFrom(linkModelItem.ItemType))
            {
                IFlowSwitchLink link = (IFlowSwitchLink)linkModelItem.GetCurrentValue();
                //Getting FlowSwitch ModelItem since there is no CFx object for linkModelItem.
                IModelTreeItem modelTreeItem = this.ModelItem as IModelTreeItem;
                storageModelItem = modelTreeItem.ModelTreeManager.WrapAsModelItem(link.ParentFlowSwitch);
            }
            return storageModelItem;
        }


        //Save the shape location on the CFx object for serialization and viewstate maintenance.
        void StoreShapeViewState(UIElement movedElement, Point newLocation)
        {
            ModelItem storageModelItem;
            if (movedElement is StartSymbol)
            {
                storageModelItem = this.ModelItem;
            }
            else
            {
                ModelItem model = ((VirtualizedContainerService.VirtualizingContainer)movedElement).ModelItem;
                storageModelItem = GetFlowElementMI(model);
            }
            StoreShapeViewState(storageModelItem, newLocation);
        }

        void StoreShapeViewState(ModelItem storageModelItem, Point newLocation)
        {
            if (this.ViewStateService.RetrieveViewState(storageModelItem, shapeLocation) != null)
            {
                this.ViewStateService.StoreViewStateWithUndo(storageModelItem, shapeLocation, newLocation);
            }
            else
            {
                this.ViewStateService.StoreViewState(storageModelItem, shapeLocation, newLocation);
            }
        }

        void PerformInternalMove(UIElement movedElement, Point newPoint, Point? shapeAnchorPoint,
            AutoConnectDirections autoConnectDirection, Connector connectorToSplit)
        {
            using (EditingScope es = (EditingScope)this.ModelItem.BeginEdit(SR.FCLinkMove))
            {
                RemoveAdorner(movedElement, typeof(ConnectionPointsAdorner));
                Point shapeLocation;
                Size size = FreeFormPanel.GetChildSize(movedElement);
                if (autoConnectDirection != AutoConnectDirections.None)
                {
                    shapeLocation = this.CalculateDropLocationForAutoConnect(autoConnectDirection, size);
                }
                else if (shapeAnchorPoint.HasValue)
                {
                    shapeLocation = SnapVisualToGrid(movedElement, newPoint, shapeAnchorPoint.Value, true);
                }
                else
                {
                    Fx.Assert(newPoint.X.IsNoLessThan(0) && newPoint.Y.IsNoLessThan(0),
                        "newPoint is negative");
                    shapeLocation = newPoint;
                }
                if (connectorToSplit != null)
                {
                    shapeLocation = this.CalculateDropLocationForAutoSplit(newPoint, shapeLocation, connectorToSplit, size);
                }
                StoreShapeViewState(movedElement, shapeLocation);
                RerouteAttachedConnectors(movedElement);
                es.Complete();
            }
        }

        void RerouteAttachedConnectors(UIElement movedElement)
        {
            foreach (Connector connector in GetAttachedConnectors(movedElement))
            {
                Reroute(connector, true);
            }
        }

        void Reroute(Connector connector, bool withUndo)
        {
            ConnectionPoint source = FreeFormPanel.GetSourceConnectionPoint(connector);
            ConnectionPoint destination = FreeFormPanel.GetDestinationConnectionPoint(connector);

            //Nulling out the PointCollection so that it doesn't interfere in line routing.
            connector.Points = new PointCollection();
            PointCollection viewState = new PointCollection(ConnectorRouter.Route(this.panel, source, destination));
            StoreConnectorViewState(FlowchartDesigner.GetLinkModelItem(connector), viewState, source, withUndo);
        }



        // Returns the last dropped item - used for auto-connect and auto-split where only one item is allowed
        ModelItem DoFlowchartGridDrop(DragEventArgs e, AutoConnectDirections autoConnectDirection, Connector connectorToSplit)
        {
            ModelItem droppedModelItem = null;
            ModelItem newFlowStepMI = null;
            e.Effects = DragDropEffects.None;
            IEnumerable<object> droppedObjects = DragDropHelper.GetDroppedObjects(this, e, Context);
            //Marking the event as being handled. In whichever case we want to route the event, it will be unmarked explicitly.
            e.Handled = true;
            List<WorkflowViewElement> movedViewElements = new List<WorkflowViewElement>();
            ShapeOffsetter shapeOffsetter = new ShapeOffsetter();
            Dictionary<WorkflowViewElement, Point> relativeLocations = DragDropHelper.GetDraggedViewElementRelativeLocations(e);
            ModelItem modelItemDroppedFromToolBox = null;
            Dictionary<object, FlowNode> objToNewFlowNodeMap = null;
            Dictionary<FlowNode, ModelItem> flowNodeModelItemMap = null;
            Dictionary<FlowNode, FlowNode> oldNewFlowNodeMap = null;
            this.PrepareForDrop(droppedObjects,
                out objToNewFlowNodeMap,
                out flowNodeModelItemMap,
                out oldNewFlowNodeMap);
            bool shouldStoreCurrentSizeViewState = true;
            foreach (object droppedObject in droppedObjects)
            {
                if (droppedObject == null)
                {
                    continue;
                }
                droppedModelItem = droppedObject as ModelItem;

                // archor point
                Point anchorPoint = DragDropHelper.GetDragDropAnchorPoint(e);


                ICompositeView srcContainer = droppedModelItem != null
                    ? DragDropHelper.GetCompositeView(droppedModelItem.View as WorkflowViewElement) as ICompositeView
                    : null;
                bool keepRelativePosition = srcContainer is FlowchartDesigner;
                // This is the case of dragging from toolbox
                if (anchorPoint.X < 0 && anchorPoint.Y < 0)
                {
                    keepRelativePosition = false;
                }

                // This is the case of dragging from the designer surface
                else if (droppedModelItem != null)
                {
                    WorkflowViewElement view = (WorkflowViewElement)droppedModelItem.View;
                    anchorPoint.Offset(-relativeLocations[view].X, -relativeLocations[view].Y);
                }


                if (droppedModelItem != null && srcContainer != null && srcContainer.Equals(this))
                {
                    if (shouldStoreCurrentSizeViewState)
                    {
                        // Moving may change the size of flowchart; need this to undo the size change.
                        this.StoreCurrentSizeViewStateWithUndo();
                        shouldStoreCurrentSizeViewState = false;
                    }
                    //InternalMove
                    PerformInternalMove(modelElement[droppedModelItem], e.GetPosition(this.panel), anchorPoint, autoConnectDirection, connectorToSplit);
                }
                else
                {
                    //External model Item drop.
                    if (droppedModelItem != null)
                    {
                        if ((IsFlowStepAction(droppedModelItem)
                            || IsFlowNode(droppedModelItem))
                            && !IsParentOf(droppedModelItem, this.ModelItem))
                        {
                            if (shouldStoreCurrentSizeViewState)
                            {
                                // Drop may change the size of flowchart; need this to undo the size change.
                                this.StoreCurrentSizeViewStateWithUndo();
                                shouldStoreCurrentSizeViewState = false;
                            }

                            FlowNode flowElement = objToNewFlowNodeMap[droppedObject];
                            ModelItem flowElementMI;
                            if (flowNodeModelItemMap.TryGetValue(flowElement, out flowElementMI))
                            {
                                // FlowNode comes from some other flowchart. 
                                this.ModelItem.Properties["Nodes"].Collection.Add(flowElementMI);
                            }
                            else
                            {
                                // FlowNode is a new created one, which means this is an Activity dragged
                                // from somewhere else, outside of Flowchart.
                                flowElementMI = this.ModelItem.Properties["Nodes"].Collection.Add(flowElement);
                                flowNodeModelItemMap[flowElement] = flowElementMI;
                            }
                            newFlowStepMI = flowElementMI;
                        }
                        else
                        {
                            //We want to route the event in the case that the flowchart is dropped upon itself.
                            if (droppedModelItem.Equals(this.ModelItem))
                            {
                                e.Handled = false;
                            }
                            //Don't add anything for what is neither a Activity nor a flowlink.
                            continue;
                        }

                        if (droppedModelItem != null && droppedModelItem.View != null)
                        {
                            movedViewElements.Add((WorkflowViewElement)droppedModelItem.View);
                        }

                        // the external item may come from other panel (sequence) which is already given
                        // a size by its previous layout panel.  That might give an inaccurate size to the
                        // dropped object (i.e. Bug 198290).  Therefore, when the object is dropped externally
                        // the FC should erases its previous hint size, forcing the FC to recompute an appropriate
                        // size based on the workflowelementview size.
                        VirtualizedContainerService.SetHintSize(droppedModelItem.GetCurrentValue(), null);
                    }
                    //Tool box drop.
                    else
                    {
                        if (typeof(Activity).IsAssignableFrom(droppedObject.GetType()))
                        {
                            FlowStep flowStep = new FlowStep();
                            flowStep.Action = (Activity)droppedObject;
                            if (shouldStoreCurrentSizeViewState)
                            {
                                // Drop may change the size of flowchart; need this to undo the size change.
                                this.StoreCurrentSizeViewStateWithUndo();
                                shouldStoreCurrentSizeViewState = false;
                            }

                            newFlowStepMI = this.ModelItem.Properties["Nodes"].Collection.Add(flowStep);
                            droppedModelItem = newFlowStepMI.Properties["Action"].Value;
                        }
                        else if (typeof(FlowNode).IsAssignableFrom(droppedObject.GetType()))
                        {
                            if (shouldStoreCurrentSizeViewState)
                            {
                                // Drop may change the size of flowchart; need this to undo the size change.
                                this.StoreCurrentSizeViewStateWithUndo();
                                shouldStoreCurrentSizeViewState = false;
                            }
                            droppedModelItem = this.ModelItem.Properties["Nodes"].Collection.Add(droppedObject);
                            newFlowStepMI = droppedModelItem;
                        }

                        // Now,  toolbox drop doesn't support multiple drop
                        // If multi-drop from tool box, use an array here.
                        modelItemDroppedFromToolBox = droppedModelItem;
                        keepRelativePosition = false;
                    } // tool box 

                    WorkflowViewElement view = droppedModelItem.View as WorkflowViewElement;
                    if (view == null || view.ExpandState)
                    {
                        //Creating a new view to get the size of collapsed view.
                        view = this.ViewService.GetView(droppedModelItem) as WorkflowViewElement;
                        ViewUtilities.MeasureView(view, true);
                    }

                    if (view != null)
                    {
                        PostDropUpdateViewState(view,
                            newFlowStepMI,
                            autoConnectDirection,
                            connectorToSplit,
                            e.GetPosition(this.panel),
                            anchorPoint,
                            keepRelativePosition,
                            shapeOffsetter);
                    }
                } // external move
            } // foreach

            // Remap references.
            // The re-map here is different from the remaping in copy/paste.
            // In copy paste, all the values are copied. but here, some value are
            // set by Properties["key"].SetValue().
            // Don't move this into PrepareMove. Some value setting is added to 
            // Change. So the operation here will decide the order of Change.Apply().
            // PropertyChange in some case, must happen after ModelItem is moved to 
            // new places.
            foreach (FlowNode flowNode in oldNewFlowNodeMap.Keys)
            {
                UpdateCloneReferenceByModelItem(flowNode, flowNodeModelItemMap, oldNewFlowNodeMap);
            }

            DragDropHelper.SetDragDropMovedViewElements(e, movedViewElements);
            
            //Backward compatibility for 4.0
            if (droppedObjects.Count() == 1 && movedViewElements.Count == 1)
            {
                #pragma warning disable 618
                DragDropHelper.SetDragDropCompletedEffects(e, DragDropEffects.Move);
                #pragma warning restore 618
            }

            if (modelItemDroppedFromToolBox != null)
            {
                // if it is dropped from toolbox, select
                this.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, (Action)(() =>
                {
                    UIElement view = (UIElement)(modelItemDroppedFromToolBox.View);
                    if (view != null)
                    {
                        Keyboard.Focus(view);
                        Selection.SelectOnly(this.Context, modelItemDroppedFromToolBox);
                    }
                }));
            }

            if (droppedModelItem != null)
            {
                if (IsFlowNode(droppedModelItem))
                {
                    return droppedModelItem;
                }
                else if (IsFlowStepAction(droppedModelItem))
                {
                    if (newFlowStepMI != null)
                    {
                        return newFlowStepMI;
                    }
                    else
                    {
                        return this.GetParentFlowStepModelItem(droppedModelItem);
                    }
                }
                return null;
            }

            return null;
        }

        Point CalculateDropLocationForAutoConnect(AutoConnectDirections autoConnectDirection, Size droppedSize)
        {
            return AutoConnectHelper.CalculateDropLocation(droppedSize, this.panel.CurrentAutoConnectTarget, autoConnectDirection, this.shapeLocations);
        }

        Point CalculateDropLocationForAutoSplit(Point mousePosition, Point originalDropLocation, Connector connector, Size droppedSize)
        {
            return AutoSplitHelper.CalculateDropLocation(mousePosition, originalDropLocation, connector, droppedSize, this.shapeLocations);
        }

        private void OnFlowchartGridDrop(object sender, DragEventArgs e)
        {
            ModelItemHelper.TryCreateImmediateEditingScopeAndExecute(this.ModelItem.GetEditingContext(), System.Activities.Presentation.SR.CollectionAddEditingScopeDescription, (es) =>
                {
                    this.DoFlowchartGridDrop(e, AutoConnectDirections.None, null);
                    if (es != null)
                    {
                        es.Complete();
                    }
                });
        }

        // Prepare some maps for drag/drop in Flowchart
        // objToNewFlowNodeMap : 
        //    <DroppedObject, FlowNodeToBeDropInDestFlowchart>
        // flowNodeNewModelItemMap : 
        //    <OldFlowNode, NewModelItemInDestFlowchart>
        //    For FlowSwitch/FlowDecision: NewModelItemINDestFlowchart is the source ModelItem.
        //    For FlowStep, leeve NewModeItemInDestFlowchart Null, this value could not be decided here.
        // flowNodeMovingMap: 
        //    <OldFlowNode, NewFlowNode>
        //    If a droppedObject is not a flownode, say it is from tool box, this value is empty.
        private void PrepareForDrop(IEnumerable<object> objects,
                out Dictionary<object, FlowNode> objToNewFlowNodeMap,
                out Dictionary<FlowNode, ModelItem> flowNodeNewModelItemMap,
                out Dictionary<FlowNode, FlowNode> oldNewFlowNodeMap)
        {
            Fx.Assert(objects != null, "dropping null objects");
            objToNewFlowNodeMap = new Dictionary<object, FlowNode>();
            oldNewFlowNodeMap = new Dictionary<FlowNode, FlowNode>();
            flowNodeNewModelItemMap = new Dictionary<FlowNode, ModelItem>();
            // 1) Get flow node and Composite view
            foreach (object obj in objects)
            {
                if (obj == null)
                {
                    Fx.Assert("obj == null");
                    continue;
                }

                objToNewFlowNodeMap[obj] = null;
                ModelItem modelItem = obj as ModelItem;
                if (modelItem == null)
                {
                    // if not a model item, return
                    continue;
                }

                ICompositeView compositeView = DragDropHelper.GetCompositeView(modelItem.View as WorkflowViewElement) as ICompositeView;
                if (compositeView == null)
                {
                    continue;
                }

                // This means a internal move, no extra thing needed.
                if (compositeView.Equals(this))
                {
                    // internal move.
                    continue;
                }

                if (!IsFlowStepAction(modelItem)
                    && !IsFlowNode(modelItem))
                {

                    Fx.Assert(modelItem.ItemType.Equals(typeof(StartNode)),
                        "should not happen. Not a Activity, FlowNode or StartSymbol");
                    // don't do anything if the dropped object is either an Activity or
                    // Flownode.
                    continue;
                }

                FlowNode flowNode = null;
                FlowchartDesigner fcDesigner = compositeView as FlowchartDesigner;
                if (fcDesigner != null)
                {
                    // If the source view is a FlowchartDesigner, we need to do some to prepare
                    // the remap.
                    ModelItem flowElementMI = fcDesigner.GetFlowElementMI(modelItem);
                    Fx.Assert(flowElementMI != null, "flowElementMI != null");
                    FlowNode oldFlowNode = flowElementMI.GetCurrentValue() as FlowNode;
                    Fx.Assert(oldFlowNode != null, "oldFlowNode != null");
                    flowNode = oldFlowNode;
                    flowNodeNewModelItemMap[oldFlowNode] = flowElementMI;
                    oldNewFlowNodeMap[oldFlowNode] = flowNode;
                }
                else
                {
                    // The object is moved from somewhere else, say Sequence. 
                    // We create a FlowStep for it.
                    FlowStep flowStep = new FlowStep();
                    flowStep.Action = (Activity)modelItem.GetCurrentValue();
                    flowNode = flowStep;
                }
                objToNewFlowNodeMap[obj] = flowNode;
            }
        }

        // Move the object to correct position after drop
        private void PostDropUpdateViewState(WorkflowViewElement view,
            ModelItem flownodeMI,
            AutoConnectDirections autoConnectDirection,
            Connector connectorToSplit,
            Point newPoint,
            Point anchorPoint,
            bool keepRelativePosition,
            ShapeOffsetter shapeOffsetter)
        {
            Fx.Assert((view != null && flownodeMI != null),
            "movedItem != null && flownodeMI != null");
            Point shapeLocationPtr;
            if (autoConnectDirection != AutoConnectDirections.None)
            {
                shapeLocationPtr = this.CalculateDropLocationForAutoConnect(autoConnectDirection, view.DesiredSize);
            }
            else
            {
                shapeLocationPtr = SnapVisualToGrid(view, newPoint, anchorPoint, keepRelativePosition);
                if (!keepRelativePosition)
                {
                    // To avoid overlaps
                    shapeLocationPtr = shapeOffsetter.OffsetShapeLocation(shapeLocationPtr);
                }
            }

            if (connectorToSplit != null)
            {
                shapeLocationPtr = this.CalculateDropLocationForAutoSplit(newPoint, shapeLocationPtr, connectorToSplit, view.DesiredSize);
            }

            // 
            if (keepRelativePosition)
            {
                this.OffsetDroppedItemToNewPosition(flownodeMI, shapeLocationPtr);
            }
            else
            {
                this.StoreShapeViewState(flownodeMI, shapeLocationPtr);
            }
        }

        private void OffsetDroppedItemToNewPosition(ModelItem flownodeMI, Point newLocationPtr)
        {
            object locationOfShape = this.ViewStateService.RetrieveViewState(flownodeMI, shapeLocation);
            if (locationOfShape == null)
            {
                return;
            }
            Point oldLocationPoint = (Point)locationOfShape;
            Vector offset = newLocationPtr - oldLocationPoint;
            this.OffSetViewState(offset, flownodeMI, true);
        }

        private void OnFlowchartGridDragEnter(object sender, DragEventArgs e)
        {
            OnFlowchartGridDrag(sender, e);
        }

        private void OnFlowchartGridDragOver(object sender, DragEventArgs e)
        {
            OnFlowchartGridDrag(sender, e);
        }

        private bool IsDropAllowed(DragEventArgs e)
        {
            return DragDropHelper.AllowDrop(e.Data, this.Context, typeof(Activity), typeof(FlowNode), typeof(StartNode));
        }

        private void OnFlowchartGridDrag(object sender, DragEventArgs e)
        {
            if (!e.Handled)
            {
                if (!this.IsDropAllowed(e))
                {
                    e.Effects = DragDropEffects.None;
                }
                e.Handled = true;
            }
        }

        static bool IsLinkModelItemSelectable(ModelItem linkModelItem)
        {
            return linkModelItem != null &&
                // link from FlowDecision
                   !typeof(FlowDecision).IsAssignableFrom(linkModelItem.ItemType) &&
                // link from StartNode
                   !typeof(Flowchart).IsAssignableFrom(linkModelItem.ItemType);
        }

        private void OnFlowchartGridKeyDown(object sender, KeyEventArgs e)
        {
            if (srcConnectionPoint != null)
            {
                // Ignore KeyBoard input when creating connector.
                e.Handled = true;
                return;
            }

            Selection currentSelection = this.Context.Items.GetValue<Selection>();
            if (e.Key == Key.Delete && this.selectedConnector != null && currentSelection.SelectionCount <= 1)
            {
                // process the delete if only the connector is selected
                ModelItem primarySelection = currentSelection.PrimarySelection;
                //Delete connector
                ModelItem linkModelItem = FlowchartDesigner.GetLinkModelItem(this.selectedConnector);
                if ((primarySelection == null && !IsLinkModelItemSelectable(linkModelItem)) ||
                    object.Equals(primarySelection, linkModelItem))
                {
                    DeleteLink(this.selectedConnector);
                    this.selectedConnector = null;
                    e.Handled = true;
                }
            }
            else if ((new List<Key> { Key.Left, Key.Right, Key.Up, Key.Down }).Contains(e.Key)
                && currentSelection.SelectedObjects.All<ModelItem>((p) => { return this.modelElement.ContainsKey(p); }))
            {
                KeyboardMove(e.Key);
                e.Handled = true;
            }
        }

        private void FlowchartDesignerKeyDown(object sender, KeyEventArgs e)
        {
            // Ignore KeyBoard input when in resizing mode.
            e.Handled = IsResizing;
        }

        private void FlowchartDesignerPreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Enter cannot be captured in KeyDown, so handle it in PreviewKeyDown event.
            e.Handled = IsResizing && e.Key == Key.Enter;
        }

        void KeyboardMove(Key key)
        {
            Vector moveDir = FreeFormPanel.CalculateMovement(key, this.IsRightToLeft);

            using (EditingScope es = (EditingScope)this.ModelItem.BeginEdit(SR.ItemMove))
            {
                bool shouldStoreCurrentSizeViewState = true;
                foreach (ModelItem selectedModelItem in this.Context.Items.GetValue<Selection>().SelectedObjects)
                {
                    UIElement shapeToMove = this.modelElement[selectedModelItem];
                    Point currentLocation = FreeFormPanel.GetLocation(shapeToMove);
                    Point newLocation = Point.Add(currentLocation, moveDir);

                    // Make sure the newLocation is positive.
                    newLocation.X = FreeFormPanel.ZeroIfNegative(newLocation.X);
                    newLocation.Y = FreeFormPanel.ZeroIfNegative(newLocation.Y);

                    if (newLocation == currentLocation)
                    {
                        continue;
                    }

                    if (shouldStoreCurrentSizeViewState)
                    {
                        // Moving may change the size of flowchart; need this to undo the size change.
                        this.StoreCurrentSizeViewStateWithUndo();
                        shouldStoreCurrentSizeViewState = false;
                    }
                    PerformInternalMove(shapeToMove, newLocation, null, AutoConnectDirections.None, null);
                }
                es.Complete();
            }
        }

        private void OnFlowchartGridPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            this.selectedConnector = null;
        }

        private void OnFlowchartGridPreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (this.srcConnectionPoint != null)
            {
                UIElement destElement = VisualTreeUtils.FindVisualAncestor<VirtualizedContainerService.VirtualizingContainer>(e.OriginalSource as DependencyObject);
                if (destElement != null && this.panel.Children.Contains(destElement))
                {
                    string errorMessage = string.Empty;
                    Point mouseUpLocation = e.GetPosition(sender as IInputElement);

                    if (!CreateLinkGesture(this.srcConnectionPoint, destElement, mouseUpLocation, out errorMessage, false, null) && !errorMessage.Equals(string.Empty))
                    {
                        ErrorReporting.ShowErrorMessage(errorMessage);
                    }
                    this.srcConnectionPoint = null;
                    RemoveAdorner(this.panel, typeof(ConnectorCreationAdorner));
                    RemoveAdorner(destElement, typeof(FlowchartConnectionPointsAdorner));
                    // Simulate a MouseEnter to show connection points
                    ChildElement_MouseEnter(destElement, null);
                }
            }
        }

        enum ConnectorType
        {
            Default = 0, ErrorConnector = 1
        };

        void OnConnectNodesCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
            Selection selection = this.Context.Items.GetValue<Selection>();
            if (selection.SelectionCount > 1)
            {
                e.CanExecute = true;
                foreach (ModelItem item in selection.SelectedObjects)
                {
                    if (!this.modelElement.ContainsKey(item))
                    {
                        e.CanExecute = false;
                        break;
                    }
                }
            }
            e.Handled = true;
        }

        void OnConnectNodesCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            using (EditingScope es = (EditingScope)this.ModelItem.BeginEdit(SR.FCCreateLink))
            {
                List<ModelItem> selectedFlowchartItems = new List<ModelItem>(this.Context.Items.GetValue<Selection>().SelectedObjects);
                selectedFlowchartItems.Reverse();
                CreateLinks(selectedFlowchartItems);
                es.Complete();
            }
        }

        void OnShowAllConditionsCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.ShowAllConditions = false;
            this.ShowAllConditions = true;
        }

        void OnShowAllConditionsCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        void OnHideAllConditionsCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.ShowAllConditions = true;
            this.ShowAllConditions = false;
        }

        void OnHideAllConditionsCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        internal static void DropActivityBelow(ViewStateService viewStateService, ModelItem modelItem, Activity activity, double interval)
        {
            // Extracting information
            ModelItem flowStep = modelItem.Parent;
            ModelItemCollection nodes = flowStep.Parent as ModelItemCollection;
            ModelItem flowchart = nodes.Parent;
            FlowchartDesigner flowchartDesigner = ((FlowchartDesigner)flowchart.View);

            // Creating FlowStep ModelItem
            ModelTreeManager modelTreeManager = (modelItem as IModelTreeItem).ModelTreeManager;
            FlowStep step = new FlowStep() { Action = activity };
            ModelItem activityModelItem = modelTreeManager.WrapAsModelItem(step);

            // Compute the 'correct' location
            Point point = (Point)viewStateService.RetrieveViewState(flowStep, shapeLocation);
            point.Y += (((WorkflowViewElement)modelItem.View).ActualHeight + interval);
            viewStateService.StoreViewState(activityModelItem, shapeLocation, point);
            flowchartDesigner.UpdateViewStateToAvoidOverlapOnPaste(new List<ModelItem> { activityModelItem });

            // Add it to the model tree
            nodes.Add(activityModelItem);
        }

        void OnAdornerDecoratorLoaded(object sender, RoutedEventArgs e)
        {
            this.adornerLayer = ((AdornerDecorator)sender).AdornerLayer;
            // This might not be the best event to handle, ideally we would like to have the event when the list of adorner changes.
            this.adornerLayer.LayoutUpdated += new EventHandler(OnAdornerLayerLayoutUpdated);
        }

        void OnAdornerLayerLayoutUpdated(object sender, EventArgs e)
        {
            // Extract the set of all adorners
            List<Adorner> adornerList = new List<Adorner>();
            foreach (object logicalChild in LogicalTreeHelper.GetChildren(adornerLayer))
            {
                Fx.Assert(logicalChild is Adorner, "What else could an adornerLayer hold?");
                adornerList.Add((Adorner)logicalChild);
            }
            Adorner[] adorners = adornerList.ToArray();
            if (FlowchartDesigner.Pack(adorners, (adorner => adorner is FlowchartExpressionAdorner)))
            {
                foreach (Adorner adorner in adorners)
                {
                    adornerLayer.Remove(adorner);
                }
                foreach (Adorner adorner in adorners)
                {
                    adornerLayer.Add(adorner);
                }
            }
        }

        // do not proprogate up to FlowchartDesigner, because designer will set selection to itself on GotFocus event.
        private void OnAdornerLayerGotFocus(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
        }

        // Within the connection points with least number of connectors, get the one closest to the midpoint.
        private static ConnectionPoint GetConnectionPointForAutoConnect(List<ConnectionPoint> availableConnectionPoints)
        {
            int minConnectorCount = availableConnectionPoints.Min<ConnectionPoint>((p) =>
                {
                    return p.AttachedConnectors.Count;
                });

            List<ConnectionPoint> connectionPoints = new List<ConnectionPoint>(availableConnectionPoints.Where<ConnectionPoint>((p) =>
                {
                    return p.AttachedConnectors.Count == minConnectorCount;
                }));

            ConnectionPoint midPoint = availableConnectionPoints[availableConnectionPoints.Count / 2];
            if (connectionPoints.Contains(midPoint))
            {
                return midPoint;
            }
            double dist;
            return ConnectionPoint.GetClosestConnectionPoint(connectionPoints, midPoint.Location, out dist);
        }

        private ConnectionPoint GetSourceConnectionPointForAutoConnect(UIElement designer, EdgeLocation edgeLocation)
        {
            List<ConnectionPoint> connectionPoints = FlowchartDesigner.GetAllConnectionPoints(designer);
            connectionPoints = new List<ConnectionPoint>(connectionPoints.Where<ConnectionPoint>((p) =>
            {
                return p != null && p.PointType != ConnectionPointKind.Incoming && p.EdgeLocation == edgeLocation;
            }));
            Fx.Assert(connectionPoints.Count > 0, "There should be at least one src connection point available");
            return FlowchartDesigner.GetConnectionPointForAutoConnect(connectionPoints);
        }

        internal static ConnectionPoint GetDestinationConnectionPointForAutoConnect(UIElement dest, ConnectionPoint srcConnPoint)
        {
            EdgeLocation destEdgeLocation = EdgeLocation.Top;
            if (!((dest is VirtualizedContainerService.VirtualizingContainer) && ((VirtualizedContainerService.VirtualizingContainer)dest).ModelItem.ItemType == typeof(FlowDecision)))
            {
                switch (srcConnPoint.EdgeLocation)
                {
                    case EdgeLocation.Top:
                        destEdgeLocation = EdgeLocation.Bottom;
                        break;
                    case EdgeLocation.Bottom:
                        destEdgeLocation = EdgeLocation.Top;
                        break;
                    case EdgeLocation.Left:
                        destEdgeLocation = EdgeLocation.Right;
                        break;
                    case EdgeLocation.Right:
                        destEdgeLocation = EdgeLocation.Left;
                        break;
                }
            }
            List<ConnectionPoint> destConnectionPoints = new List<ConnectionPoint>(FlowchartDesigner.GetConnectionPoints(dest).Where<ConnectionPoint>((p) =>
            {
                return p.PointType != ConnectionPointKind.Outgoing && p.EdgeLocation == destEdgeLocation;
            }));
            Fx.Assert(destConnectionPoints.Count > 0, "There should be at least one dest connection point available");
            return FlowchartDesigner.GetConnectionPointForAutoConnect(destConnectionPoints);
        }

        private ModelItem GetSourceModelItemForAutoConnect(UIElement sourceElement)
        {
            ModelItem sourceModelItem = null;
            if (sourceElement is WorkflowViewElement)
            {
                sourceModelItem = ((WorkflowViewElement)sourceElement).ModelItem;
            }
            else if (sourceElement is VirtualizedContainerService.VirtualizingContainer)
            {
                sourceModelItem = ((VirtualizedContainerService.VirtualizingContainer)sourceElement).ModelItem;
            }
            if (sourceModelItem != null && IsFlowStepAction(sourceModelItem))
            {
                sourceModelItem = this.GetParentFlowStepModelItem(sourceModelItem);
                Fx.Assert(typeof(FlowStep).IsAssignableFrom(sourceModelItem.ItemType), "The parent should be FlowNode");
            }
            return sourceModelItem;
        }

        private ModelItem GetParentFlowStepModelItem(ModelItem activityModelItem)
        {
            foreach (ModelItem flowNodeModelItem in this.ModelItem.Properties["Nodes"].Collection)
            {
                if (typeof(FlowStep).IsAssignableFrom(flowNodeModelItem.ItemType))
                {
                    if (flowNodeModelItem.Properties["Action"].Value == activityModelItem)
                    {
                        return flowNodeModelItem;
                    }
                }
            }
            return null;
        }

        public void DoAutoConnect(DragEventArgs e, UIElement targetElement, AutoConnectDirections direction)
        {
            UIElement sourceElement = targetElement;
            bool immediatelyCommit = ModelItemHelper.CanCreateImmediateEditingScope(this.ModelItem);

            using (EditingScope scope = (EditingScope)this.ModelItem.BeginEdit(SR.AutoConnect, immediatelyCommit))
            {
                ModelItem droppedModelItem = this.DoFlowchartGridDrop(e, direction, null);
                bool autoConnected = false;
                if (droppedModelItem != null)
                {
                    ModelItem sourceModelItem = this.GetSourceModelItemForAutoConnect(sourceElement);
                    if (sourceModelItem != null)
                    {
                        if (sourceModelItem.ItemType == typeof(FlowStep))
                        {
                            sourceModelItem.Properties["Next"].SetValue(droppedModelItem);
                            autoConnected = true;
                        }
                        else if (sourceModelItem.ItemType == typeof(FlowDecision))
                        {
                            if (direction == AutoConnectDirections.Left)
                            {
                                sourceModelItem.Properties["True"].SetValue(droppedModelItem);
                                autoConnected = true;
                            }
                            else if (direction == AutoConnectDirections.Right)
                            {
                                sourceModelItem.Properties["False"].SetValue(droppedModelItem);
                                autoConnected = true;
                            }
                        }
                        else if (GenericFlowSwitchHelper.IsGenericFlowSwitch(sourceModelItem.ItemType))
                        {
                            string message = string.Empty;
                            autoConnected = this.CreateFlowSwitchLink(this.srcConnectionPointForAutoConnect, sourceModelItem, droppedModelItem, null, null, ref message);
                        }
                        else if (sourceModelItem.ItemType == typeof(StartNode))
                        {
                            this.ModelItem.Properties["StartNode"].SetValue(droppedModelItem);
                            autoConnected = true;
                        }
                    }
                }
                if (autoConnected)
                {
                    this.srcConnectionPointForAutoConnect = this.GetSourceConnectionPointForAutoConnect(sourceElement, AutoConnectHelper.AutoConnectDirection2EdgeLocation(direction));
                    scope.Complete();
                }
                else
                {
                    scope.Revert();
                }
            }
        }

        public AutoConnectDirections GetDirectionsAllowed(DragEventArgs e, UIElement targetElement)
        {
            List<Type> draggedTypes = DragDropHelper.GetDraggedTypes(e.Data);
            if (draggedTypes.Count != 1)
            {
                return AutoConnectDirections.None;
            }
            Type draggedType = draggedTypes[0];
            if (!typeof(Activity).IsAssignableFrom(draggedType) &&
                  !typeof(FlowNode).IsAssignableFrom(draggedType) &&
                  !IsActivityTemplateFactory(draggedType))
            {
                return AutoConnectDirections.None;
            }
            ModelItem sourceModelItem = this.GetSourceModelItemForAutoConnect(targetElement);
            if (sourceModelItem != null)
            {
                if (sourceModelItem.ItemType == typeof(FlowStep))
                {
                    if (sourceModelItem.Properties["Next"].Value != null)
                    {
                        return AutoConnectDirections.None;
                    }
                }
                else if (sourceModelItem.ItemType == typeof(FlowDecision))
                {
                    AutoConnectDirections directions = AutoConnectDirections.None;
                    if (sourceModelItem.Properties["True"].Value == null)
                    {
                        directions |= AutoConnectDirections.Left;
                    }
                    if (sourceModelItem.Properties["False"].Value == null)
                    {
                        directions |= AutoConnectDirections.Right;
                    }
                    return directions;
                }
                else if (sourceModelItem.ItemType == typeof(StartNode))
                {
                    if (this.ModelItem.Properties["StartNode"].Value != null)
                    {
                        return AutoConnectDirections.None;
                    }
                }
            }
            return AutoConnectDirections.Top | AutoConnectDirections.Bottom | AutoConnectDirections.Left | AutoConnectDirections.Right;
        }

        public bool CanAutoSplit(DragEventArgs e)
        {
            if (!this.IsDropAllowed(e))
            {
                return false;
            }
            ModelItem draggedModelItem = e.Data.GetData(DragDropHelper.ModelItemDataFormat) as ModelItem;

            // The start node
            if (draggedModelItem is FakeModelItemImpl)
            {
                return false;
            }

            if (draggedModelItem != null && this.modelElement.ContainsKey(draggedModelItem))
            {
                if (this.GetAttachedConnectors(this.modelElement[draggedModelItem]).Count > 0)
                {
                    return false;
                }
            }
            return true;
        }

        internal static int GetConnectionPointIndex(UIElement element, ConnectionPoint pnt)
        {
            List<ConnectionPoint> list = FlowchartDesigner.GetAllConnectionPoints(element);
            return list.IndexOf(pnt);
        }

        internal static ConnectionPoint GetConnectionPointFromIndex(UIElement element, int index)
        {
            List<ConnectionPoint> list = FlowchartDesigner.GetAllConnectionPoints(element);
            if (index >= 0 && index < list.Count)
            {
                return list[index];
            }
            return null;
        }

        internal UIElement GetView(ModelItem item)
        {
            if (item == this.ModelItem)
            {
                return this.StartSymbol;
            }
            return this.modelElement[item];
        }

        public void DoAutoSplit(DragEventArgs e, Connector connector)
        {
            bool immediatelyCommit = ModelItemHelper.CanCreateImmediateEditingScope(this.ModelItem);

            using (EditingScope scope = (EditingScope)this.ModelItem.BeginEdit(SR.AutoSplit, immediatelyCommit))
            {
                ModelItem droppedModelItem = this.DoFlowchartGridDrop(e, AutoConnectDirections.None, connector);
                bool autoSplit = false;
                ModelItem sourceModelItem = null;
                ModelItem destinationModelItem = null;
                if (droppedModelItem != null)
                {
                    this.StoreConnectorViewState(connector, true);
                    IFlowSwitchLink flowSwitchLink = this.DeleteLink(connector, true);

                    bool linkCreated = true;
                    string message = string.Empty;
                    UIElement srcDesigner = FreeFormPanel.GetSourceConnectionPoint(connector).ParentDesigner;
                    UIElement destDesigner = FreeFormPanel.GetDestinationConnectionPoint(connector).ParentDesigner;
                    if (srcDesigner is StartSymbol)
                    {
                        sourceModelItem = this.ModelItem;
                        this.ModelItem.Properties["StartNode"].SetValue(droppedModelItem);
                    }
                    else if (srcDesigner is VirtualizedContainerService.VirtualizingContainer)
                    {
                        sourceModelItem = ((VirtualizedContainerService.VirtualizingContainer)srcDesigner).ModelItem;
                        ModelItem srcFlowNodeModelItem = sourceModelItem;
                        if (!IsFlowNode(srcFlowNodeModelItem))
                        {
                            srcFlowNodeModelItem = this.GetParentFlowStepModelItem(srcFlowNodeModelItem);
                        }
                        Fx.Assert(IsFlowNode(srcFlowNodeModelItem), "srcFlowNodeModelItem should be a FlowNode");

                        if (typeof(FlowStep) == srcFlowNodeModelItem.ItemType)
                        {
                            srcFlowNodeModelItem.Properties["Next"].SetValue(droppedModelItem);
                        }
                        else if (typeof(FlowDecision) == srcFlowNodeModelItem.ItemType && FreeFormPanel.GetSourceConnectionPoint(connector).Equals(GetTrueConnectionPoint(srcDesigner)))
                        {
                            srcFlowNodeModelItem.Properties["True"].SetValue(droppedModelItem);
                        }
                        else if (typeof(FlowDecision) == srcFlowNodeModelItem.ItemType && FreeFormPanel.GetSourceConnectionPoint(connector).Equals(GetFalseConnectionPoint(srcDesigner)))
                        {
                            srcFlowNodeModelItem.Properties["False"].SetValue(droppedModelItem);
                        }
                        else if (GenericFlowSwitchHelper.IsGenericFlowSwitch(srcFlowNodeModelItem.ItemType))
                        {
                            linkCreated = CreateFlowSwitchLink(FreeFormPanel.GetSourceConnectionPoint(connector), srcFlowNodeModelItem, droppedModelItem, flowSwitchLink, null, ref message);
                        }
                    }

                    if (linkCreated && string.IsNullOrEmpty(message))
                    {
                        destinationModelItem = ((VirtualizedContainerService.VirtualizingContainer)destDesigner).ModelItem;
                        ModelItem destFlowNodeModelItem = destinationModelItem;
                        if (!IsFlowNode(destFlowNodeModelItem))
                        {
                            destFlowNodeModelItem = this.GetParentFlowStepModelItem(destFlowNodeModelItem);
                        }

                        Fx.Assert(IsFlowNode(destFlowNodeModelItem), "destFlowNodeModelItem should be a FlowNode");

                        if (droppedModelItem.ItemType == typeof(FlowStep))
                        {
                            droppedModelItem.Properties["Next"].SetValue(destFlowNodeModelItem);
                            autoSplit = true;
                        }
                        else if (GenericFlowSwitchHelper.IsGenericFlowSwitch(droppedModelItem.ItemType))
                        {
                            droppedModelItem.Properties["Default"].SetValue(destFlowNodeModelItem);
                            autoSplit = true;
                        }
                        else if (droppedModelItem.ItemType == typeof(FlowDecision))
                        {
                            droppedModelItem.Properties["True"].SetValue(destFlowNodeModelItem);
                            autoSplit = true;
                        }
                    }
                }

                if (autoSplit)
                {
                    Fx.Assert(sourceModelItem != null, "sourceModelItem != null");
                    Fx.Assert(destinationModelItem != null, "destinationModelItem != null");

                    int srcIndex = GetConnectionPointIndex(this.GetView(sourceModelItem), FreeFormPanel.GetSourceConnectionPoint(connector));
                    int desIndex = GetConnectionPointIndex(this.GetView(destinationModelItem), FreeFormPanel.GetDestinationConnectionPoint(connector));

                    EdgeLocation entryEdgeForAutoSplit;
                    EdgeLocation exitEdgeForAutoSplit;
                    AutoSplitHelper.CalculateEntryExitEdges(e.GetPosition(this.panel),
                        connector, out entryEdgeForAutoSplit, out exitEdgeForAutoSplit);

                    FlowchartDesigner.SetAutoSplitDataWithUndo(
                        this.ModelItem, sourceModelItem, destinationModelItem, srcIndex, desIndex, entryEdgeForAutoSplit, exitEdgeForAutoSplit);

                    scope.Complete();
                }
                else
                {
                    scope.Revert();
                }
            }
        }

        private ConnectionPoint GetEmptyEdgeMidConnectionPointNotOfType(UIElement designer, EdgeLocation edgeLocation, ConnectionPointKind invalidType)
        {
            List<ConnectionPoint> connectionPoints = FlowchartDesigner.GetAllConnectionPoints(designer);
            connectionPoints = new List<ConnectionPoint>(connectionPoints.Where<ConnectionPoint>((p) =>
            {
                return p != null && p.PointType != invalidType && p.AttachedConnectors.Count == 0 && p.EdgeLocation == edgeLocation;
            }));

            if (connectionPoints.Count > 0)
            {
                return connectionPoints[connectionPoints.Count / 2];
            }

            return null;
        }

        private ConnectionPoint GetDestinationConnectionPointForAutoSplit(ConnectionPoint srcConnPoint, UIElement destDesigner)
        {
            this.MeasureView(destDesigner);
            ConnectionPoint point = this.GetEmptyEdgeMidConnectionPointNotOfType(destDesigner, this.entryEdgeForAutoSplit, ConnectionPointKind.Outgoing);
            if (point == null)
            {
                point = this.FindClosestConnectionPointNotOfType(srcConnPoint, new List<ConnectionPoint>(FlowchartDesigner.GetConnectionPoints(destDesigner).Where<ConnectionPoint>(p =>
                {
                    return p.AttachedConnectors.Count == 0;
                })), ConnectionPointKind.Outgoing);
            }

            return point;
        }

        private ConnectionPoint GetSourceConnectionPointForAutoSplit(ConnectionPoint destConnPoint, UIElement srcDesigner)
        {
            this.MeasureView(srcDesigner);
            ConnectionPoint point = this.GetEmptyEdgeMidConnectionPointNotOfType(srcDesigner, this.exitEdgeForAutoSplit, ConnectionPointKind.Incoming);
            if (point == null)
            {
                point = this.FindClosestConnectionPointNotOfType(destConnPoint, new List<ConnectionPoint>(FlowchartDesigner.GetConnectionPoints(srcDesigner).Where<ConnectionPoint>(p =>
                {
                    return p.AttachedConnectors.Count == 0;
                })), ConnectionPointKind.Incoming);
            }

            return point;
        }

        private void MeasureView(UIElement view)
        {
            if (this.panel.Children.Contains(view))
            {
                this.panel.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            }
            else if (VisualTreeHelper.GetParent(view) == null)
            {
                StackPanel stackPanel = new StackPanel();
                stackPanel.Children.Add(view);
                if (view is VirtualizedContainerService.VirtualizingContainer)
                {
                    ((VirtualizedContainerService.VirtualizingContainer)view).Populate();
                }
                stackPanel.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                stackPanel.UpdateLayout();
                stackPanel.Children.Remove(view);
                FreeFormPanel.SetChildSize(view, view.DesiredSize);
            }
            else
            {
                Fx.Assert(false, "The view should either be un-rooted or contained in the FreeFormPanel");
            }
        }

        private void StoreCurrentSizeViewStateWithUndo()
        {
            this.ViewStateService.StoreViewStateWithUndo(
                this.ModelItem,
                FlowchartSizeFeature.WidthPropertyName,
                this.ViewStateService.RetrieveViewState(this.ModelItem, FlowchartSizeFeature.WidthPropertyName));

            this.ViewStateService.StoreViewStateWithUndo(
                this.ModelItem,
                FlowchartSizeFeature.HeightPropertyName,
                this.ViewStateService.RetrieveViewState(this.ModelItem, FlowchartSizeFeature.HeightPropertyName));
        }

        static private bool IsFlowStepAction(ModelItem modelItem)
        {
            // modelItem.CurrentValue is an Activity which is an Action of a FlowStep
            return typeof(Activity).IsAssignableFrom(modelItem.ItemType);
        }

        static private bool IsFlowNode(ModelItem modelItem)
        {
            return typeof(FlowNode).IsAssignableFrom(modelItem.ItemType);
        }

        sealed private class ShapeOffsetter
        {
            private Point lastShape;
            private bool isFirstShape = true;

            public Point OffsetShapeLocation(Point shapeLocation)
            {
                if (this.isFirstShape)
                {
                    this.lastShape = shapeLocation;
                    this.isFirstShape = false;
                    return shapeLocation;
                }

                // the shapeLocation must be at least at right-down of lastShape
                Double detX = shapeLocation.X - (lastShape.X + FreeFormPanel.GridSize);
                Double detY = shapeLocation.Y - (lastShape.Y + FreeFormPanel.GridSize);
                if (detX < 0 || detY < 0)
                {
                    // overlapped
                    // then offset shapeLocation. 
                    // offsetX and offsetY must be from Integer * FreeFormPanel.GridSize, because
                    // shapeLocation is aligned to grid, and we expect after the offset, it is 
                    // still aligned.
                    Double offsetX = Math.Ceiling(-detX / FreeFormPanel.GridSize) * FreeFormPanel.GridSize;
                    Double offsetY = Math.Ceiling(-detY / FreeFormPanel.GridSize) * FreeFormPanel.GridSize;
                    shapeLocation.Offset(offsetX, offsetY);
                }
                this.lastShape = shapeLocation;
                return this.lastShape;
            }
        }

        private static void SetAutoSplitDataWithUndo(
            ModelItem fcModelItem,
            ModelItem srcModelItem,
            ModelItem destModelItem,
            int srcIndex,
            int destIndex,
            EdgeLocation entryEdgeForAutoSplit,
            EdgeLocation exitEdgeForAutoSplit)
        {
            using (EditingScope es = (EditingScope)fcModelItem.BeginEdit(SR.AutoSplit, false))
            {
                es.Changes.Add(
                    new SetAutoSplitConnectionPointChange(
                        fcModelItem,
                        srcModelItem,
                        destModelItem,
                        srcIndex,
                        destIndex,
                        entryEdgeForAutoSplit,
                        exitEdgeForAutoSplit
                        ));
                es.Complete();
            }
        }

        private static bool IsActivityTemplateFactory(Type type)
        {
            return type.GetInterface(typeof(IActivityTemplateFactory).FullName) != null ||
                   type.GetInterface(typeof(IActivityTemplateFactory<>).FullName) != null;
        }

        // In AutoSplit, a mark,this.SrcConnPntAutoSplit & this.DestConnPntAutoSplit,
        // is set telling the CreateLink, which is called later in Complete(), to use AutoSplit
        // way to route the connector. 
        // SetAutoSplitConnectionPointChange makes sure the mark is set during Redo.
        // This Change does nothing in Undo.
        private class SetAutoSplitConnectionPointChange : Change
        {
            private ModelItem Owner { get; set; }
            private EdgeLocation EntryEdgeForAutoSplit { get; set; }
            private EdgeLocation ExitEdgeForAutoSplit { get; set; }
            private ModelItem SrcModelItem { get; set; }
            private ModelItem DestModelItem { get; set; }
            private int SrcConnPntIndex { get; set; }
            private int DestConnPntIndex { get; set; }

            private bool IsUndo { get; set; }

            public SetAutoSplitConnectionPointChange(
                ModelItem fcModelItem,
                ModelItem srcModelItem,
                ModelItem destModelItem,
                int srcIndex,
                int destIndex,
                EdgeLocation entryEdgeForAutoSplit,
                EdgeLocation exitEdgeForAutoSplit
                )
            {
                this.Owner = fcModelItem;
                this.SrcModelItem = srcModelItem;
                this.DestModelItem = destModelItem;
                this.SrcConnPntIndex = srcIndex;
                this.DestConnPntIndex = destIndex;
                this.EntryEdgeForAutoSplit = entryEdgeForAutoSplit;
                this.ExitEdgeForAutoSplit = exitEdgeForAutoSplit;
                this.IsUndo = false;
            }

            private SetAutoSplitConnectionPointChange()
            {
            }

            public override string Description
            {
                get { return SR.AutoSplit; }
            }

            public override bool Apply()
            {
                if (this.IsUndo)
                {
                    return true;
                }
                FlowchartDesigner designer = this.Owner.View as FlowchartDesigner;
                Fx.Assert(designer != null, "null designer");
                UIElement srcElem = designer.GetView(this.SrcModelItem);
                UIElement desElem = designer.GetView(this.DestModelItem);
                ConnectionPoint srcConnPnt = GetConnectionPointFromIndex(srcElem, this.SrcConnPntIndex);
                ConnectionPoint desConnPnt = GetConnectionPointFromIndex(desElem, this.DestConnPntIndex);
                Fx.Assert(srcConnPnt != null, "srcConnPnt");
                Fx.Assert(desConnPnt != null, "desConnPnt");

                // setting values
                designer.srcConnectionPointForAutoSplit = srcConnPnt;
                designer.destConnectionPointForAutoSplit = desConnPnt;
                designer.entryEdgeForAutoSplit = this.EntryEdgeForAutoSplit;
                designer.exitEdgeForAutoSplit = this.ExitEdgeForAutoSplit;
                return true;
            }

            public override Change GetInverse()
            {
                return new SetAutoSplitConnectionPointChange
                {
                    Owner = this.Owner,
                    IsUndo = !this.IsUndo,
                    EntryEdgeForAutoSplit = this.EntryEdgeForAutoSplit,
                    ExitEdgeForAutoSplit = this.ExitEdgeForAutoSplit,
                    SrcModelItem = this.SrcModelItem,
                    DestModelItem = this.DestModelItem,
                    SrcConnPntIndex = this.SrcConnPntIndex,
                    DestConnPntIndex = this.DestConnPntIndex
                };
            }
        }
    }
}
