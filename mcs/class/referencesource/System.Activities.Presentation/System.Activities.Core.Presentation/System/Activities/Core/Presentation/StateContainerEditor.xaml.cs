//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Core.Presentation
{
    using System;
    using System.Activities.Presentation;
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.Services;
    using System.Activities.Presentation.View;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Windows;
    using System.Windows.Automation;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Threading;
    using System.Activities.Presentation.FreeFormEditing;
    using System.Activities.Statements;
    using System.Runtime;
    using System.Activities.Presentation.Internal.PropertyEditing;
    using System.Globalization;

    // The StateContainerEditor contains a FreeFormPanel that implements free form editing behaviors
    // for States and Transitions among them. An instance of StateMachineDesigner and an instance of
    // StateDesigner each contains an instance of StateContainerEditor to edit its child States and
    // Transitions.
    partial class StateContainerEditor : IAutoConnectContainer, IAutoSplitContainer
    {
        // Used to find the destination state when pasting transition.
        private static Dictionary<ModelItem, string> modelItemToGuid = new Dictionary<ModelItem, string>();
        internal static ModelItem CopiedTransitionDestinationState { get; set; }

        // Flag to indicate whether the editor has been populated
        bool populated = false;

        // To keep track of all child state designer
        Dictionary<ModelItem, UIElement> modelItemToUIElement;

        // shapeLocations is useful to avoid pasting on existing shapes.
        HashSet<Point> shapeLocations = null;

        // To keep track of transition collections that the outmost editor listens to the CollectionChanged events.
        HashSet<ModelItem> listenedTransitionCollections;

        // Flag whether the view state change has already been committed in the UI.
        bool internalViewStateChange = false;

        // activeSrcConnectionPoint is required for connector creation gesture to store the source of the link.
        ConnectionPoint activeSrcConnectionPoint;

        ConnectionPoint activeDestConnectionPointForAutoSplit;

        ConnectionPoint activeSrcConnectionPointForAutoSplit;

        EdgeLocation entryEdgeForAutoSplit;

        EdgeLocation exitEdgeForAutoSplit;

        // selectedConnector is a placeholder for the last connector selected.
        Connector selectedConnector = null;

        bool updatingSelectedConnector;

        // Used for connector creation
        UIElement lastConnectionPointMouseUpElement = null;

        // Only used by the outmost editor to keep track of transitions added/removed when editing scope completes
        List<ModelItem> transitionModelItemsAdded;
        List<ModelItem> transitionModelItemsRemoved;

        // The outmost editor when the designer is populated.
        // This is used to find the outmost editor when this editor has been removed from the visual tree.
        StateContainerEditor stateMachineContainerEditor = null;

        // To keep track of whether the initial state is changed during an EditingScope
        bool initialStateChanged = false;

        // The initial node symbol
        UIElement initialNode = null;

        // The ModelItem for the initial node
        ModelItem initialModelItem = null;

        // To register / unregister the editor as the default composite view on its parent
        ICompositeViewEvents compositeViewEvents = null;

        Size requiredSize = new Size(0, 0);

        bool suppressAddingConnectorsWhenAddingStateVisuals = false;

        ConnectionPointsAdorner activeConnectionPointsAdorner = null;
        ConnectionPoint activeConnectionPoint = null;

        SubscribeContextCallback<Selection> onSelectionChangedCallback = null;

        bool? isRightToLeft;

        // Constants
        const double startSymbolTopMargin = 10.0;
        // Default size of the state container editor when it is inside of the state designer
        const double DefaultWidthForState = 100;
        const double DefaultHeightForState = 25;
        // Default size of the state container editor when it is inside of the state machine designer
        const double DefaultWidthForStateMachine = 600;
        const double DefaultHeightForStateMachine = 600;
        // Default size of the state designer
        const double DefaultStateDesignerWidth = 114;
        const double DefaultStateDesignerHeight = 61;
        // Default size of the initial / final node
        const double InitialNodeWidth = 60;
        const double InitialNodeHeight = 75;
        const double ConnectionPointMargin = 15;
        const string ShapeLocationViewStateKey = "ShapeLocation";
        const string ShapeSizeViewStateKey = "ShapeSize";
        internal const string ConnectorLocationViewStateKey = "ConnectorLocation";
        internal const string SrcConnectionPointIndexStateKey = "SrcConnectionPointIndex";
        internal const string DestConnectionPointIndexStateKey = "DestConnectionPointIndex";
        internal const string StateContainerWidthViewStateKey = "StateContainerWidth";
        internal const string StateContainerHeightViewStateKey = "StateContainerHeight";
        internal const string DefaultStateDisplayName = "State";
        internal const string DefaultFinalStateDisplayName = "FinalState";
        internal const int ConnectionPointNum = 19;
        private const string TriggerNameToolTip = "Trigger: {0}";
        private const string TransitionNameToolTip = "Transition: {0}";

        public static readonly DependencyProperty StateContainerWidthProperty = DependencyProperty.Register(
            "StateContainerWidth",
            typeof(double),
            typeof(StateContainerEditor),
            new FrameworkPropertyMetadata(DefaultWidthForState));

        public static readonly DependencyProperty StateContainerHeightProperty = DependencyProperty.Register(
            "StateContainerHeight",
            typeof(double),
            typeof(StateContainerEditor),
            new FrameworkPropertyMetadata(DefaultHeightForState));

        public static readonly DependencyProperty PanelMinWidthProperty = DependencyProperty.Register(
            "PanelMinWidth",
            typeof(double),
            typeof(StateContainerEditor),
            new FrameworkPropertyMetadata());

        public static readonly DependencyProperty PanelMinHeightProperty = DependencyProperty.Register(
            "PanelMinHeight",
            typeof(double),
            typeof(StateContainerEditor),
            new FrameworkPropertyMetadata());

        public static readonly DependencyProperty ConnectorModelItemProperty = DependencyProperty.RegisterAttached(
            "ConnectorModelItem",
            typeof(ModelItem),
            typeof(StateContainerEditor),
            new FrameworkPropertyMetadata());

        public static readonly DependencyProperty ConnectionPointsProperty = DependencyProperty.RegisterAttached(
            "ConnectionPoints",
            typeof(List<ConnectionPoint>),
            typeof(StateContainerEditor),
            new FrameworkPropertyMetadata());

        public static readonly DependencyProperty ModelItemProperty = DependencyProperty.Register(
            "ModelItem",
            typeof(ModelItem),
            typeof(StateContainerEditor),
            new FrameworkPropertyMetadata());

        public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register(
            "IsReadOnly",
            typeof(bool), typeof(StateContainerEditor),
            new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty IsStateMachineContainerProperty = DependencyProperty.Register(
           "IsStateMachineContainer",
           typeof(bool), typeof(StateContainerEditor),
           new FrameworkPropertyMetadata(false));

        public StateContainerEditor()
        {
            InitializeComponent();
            this.modelItemToUIElement = new Dictionary<ModelItem, UIElement>();
            this.shapeLocations = new HashSet<Point>();
            this.listenedTransitionCollections = new HashSet<ModelItem>();
            this.transitionModelItemsAdded = new List<ModelItem>();
            this.transitionModelItemsRemoved = new List<ModelItem>();

            Binding readOnlyBinding = new Binding();
            readOnlyBinding.RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(DesignerView), 1);
            readOnlyBinding.Path = new PropertyPath(DesignerView.IsReadOnlyProperty);
            readOnlyBinding.Mode = BindingMode.OneWay;
            this.SetBinding(IsReadOnlyProperty, readOnlyBinding);

            this.Loaded += (s, e) =>
            {
                if (this.ShouldInitialize())
                {
                    WorkflowViewElement parent = VisualTreeUtils.FindVisualAncestor<WorkflowViewElement>(this);
                    this.ModelItem = parent.ModelItem;
                    this.StateMachineModelItem = StateContainerEditor.GetStateMachineModelItem(this.ModelItem);
                    this.Context = parent.Context;
                    this.compositeViewEvents = parent;
                    if (this.compositeViewEvents != null)
                    {
                        this.compositeViewEvents.RegisterDefaultCompositeView(this);
                    }
                    if (!this.populated)
                    {
                        this.Populate();
                        Selection.Subscribe(this.Context, this.OnSelectionChangedCallback);
                        this.populated = true;
                    }
                }
            };

            this.Unloaded += (s, e) =>
            {
                if (this.compositeViewEvents != null)
                {
                    (compositeViewEvents).UnregisterDefaultCompositeView(this);
                    this.compositeViewEvents = null;
                }
                if (this.populated)
                {
                    this.Cleanup();
                    Selection.Unsubscribe(this.Context, this.OnSelectionChangedCallback);
                    this.populated = false;
                }

                this.StateMachineModelItem = null;

                this.activeSrcConnectionPoint = null;
                this.activeDestConnectionPointForAutoSplit = null;
                this.activeSrcConnectionPointForAutoSplit = null;

                // selectedConnector is a placeholder for the last connector selected.
                this.selectedConnector = null;

                // Used for connector creation
                this.lastConnectionPointMouseUpElement = null;
                this.activeConnectionPointsAdorner = null;
                this.activeConnectionPoint = null;
                this.initialNode = null;
                // The ModelItem for the initial node
                this.initialModelItem = null;

                BindingOperations.ClearBinding(this, IsReadOnlyProperty);
            };
        }

        internal FreeFormPanel Panel
        {
            get
            {
                return this.panel;
            }
        }

        ViewStateService ViewStateService
        {
            get
            {
                ViewStateService viewStateService = this.Context.Services.GetService<ViewStateService>();
                return viewStateService;
            }
        }

        DesignerView DesignerView
        {
            get
            {
                return this.Context.Services.GetService<DesignerView>();
            }
        }

        SubscribeContextCallback<Selection> OnSelectionChangedCallback
        {
            get
            {
                if (this.onSelectionChangedCallback == null)
                {
                    this.onSelectionChangedCallback = new SubscribeContextCallback<Selection>(this.OnSelectionChanged);
                }

                return this.onSelectionChangedCallback;
            }
        }

        public double StateContainerWidth
        {
            get { return (double)this.GetValue(StateContainerEditor.StateContainerWidthProperty); }
            set { this.SetValue(StateContainerEditor.StateContainerWidthProperty, value); }
        }

        public double StateContainerHeight
        {
            get { return (double)this.GetValue(StateContainerEditor.StateContainerHeightProperty); }
            set { this.SetValue(StateContainerEditor.StateContainerHeightProperty, value); }
        }

        public double PanelMinWidth
        {
            get { return (double)this.GetValue(StateContainerEditor.PanelMinWidthProperty); }
            set { this.SetValue(StateContainerEditor.PanelMinWidthProperty, value); }
        }

        public double PanelMinHeight
        {
            get { return (double)this.GetValue(StateContainerEditor.PanelMinHeightProperty); }
            set { this.SetValue(StateContainerEditor.PanelMinHeightProperty, value); }
        }

        public ModelItem ModelItem
        {
            get { return (ModelItem)GetValue(ModelItemProperty); }
            set { SetValue(ModelItemProperty, value); }
        }

        protected bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            private set { SetValue(IsReadOnlyProperty, value); }
        }

        protected bool IsStateMachineContainer
        {
            get { return (bool)GetValue(IsStateMachineContainerProperty); }
            private set { SetValue(IsStateMachineContainerProperty, value); }
        }


        public EditingContext Context
        {
            get;
            set;
        }

        ModelItem StateMachineModelItem
        {
            get;
            set;
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            this.DataContext = this;
        }

        #region PopulateCleanup

        void Populate()
        {
            // Keep track of the outmost editor, which may not be accessible by traversing the visual tree when the designer is deleted.
            this.stateMachineContainerEditor = this.GetStateMachineContainerEditor();

            this.panel.LocationChanged += new LocationChangedEventHandler(OnFreeFormPanelLocationChanged);
            this.panel.ConnectorMoved += new ConnectorMovedEventHandler(OnFreeFormPanelConnectorMoved);
            this.panel.LayoutUpdated += new EventHandler(OnFreeFormPanelLayoutUpdated);
            this.panel.RequiredSizeChanged += new RequiredSizeChangedEventHandler(OnFreeFormPanelRequiredSizeChanged);

            this.ViewStateService.ViewStateChanged += new ViewStateChangedEventHandler(OnViewStateChanged);

            if (this.ModelItem.ItemType == typeof(StateMachine))
            {
                // Only StateMachine supports "States" collection
                IsStateMachineContainer = true;
                this.ModelItem.Properties[StateMachineDesigner.StatesPropertyName].Collection.CollectionChanged += new NotifyCollectionChangedEventHandler(OnStateCollectionChanged);
                this.ModelItem.PropertyChanged += new PropertyChangedEventHandler(this.OnModelPropertyChanged);
                ModelTreeManager modelTreeManager = this.Context.Services.GetService<ModelTreeManager>();
                modelTreeManager.EditingScopeCompleted += new EventHandler<EditingScopeEventArgs>(this.OnEditingScopeCompleted);

                foreach (ModelItem modelItem in this.ModelItem.Properties[StateMachineDesigner.StatesPropertyName].Collection)
                {
                    if (modelItem.ItemType == typeof(State))
                    {
                        ModelItemCollection transitions = modelItem.Properties[StateDesigner.TransitionsPropertyName].Collection;
                        if (!this.listenedTransitionCollections.Contains(transitions))
                        {
                            transitions.CollectionChanged += new NotifyCollectionChangedEventHandler(this.OnTransitionCollectionChanged);
                            this.listenedTransitionCollections.Add(transitions);
                        }
                    }
                }
            }


            object widthViewState = this.ViewStateService.RetrieveViewState(this.ModelItem, StateContainerWidthViewStateKey);
            if (widthViewState != null)
            {
                this.StateContainerWidth = (double)widthViewState;
            }

            object heightViewState = this.ViewStateService.RetrieveViewState(this.ModelItem, StateContainerHeightViewStateKey);
            if (heightViewState != null)
            {
                this.StateContainerHeight = (double)heightViewState;
            }

            this.panel.Children.Clear();
            this.modelItemToUIElement.Clear();
            this.shapeLocations.Clear();

            if (this.ModelItem.ItemType == typeof(StateMachine))
            {
                this.AddInitialNode();
                this.AddStateVisuals(this.ModelItem.Properties[StateMachineDesigner.StatesPropertyName].Collection);
            }
        }

        void Cleanup()
        {
            this.panel.Children.Clear();
            // Cleaning up the designers as they might be re-used.
            foreach (UIElement element in this.modelItemToUIElement.Values)
            {
                element.MouseEnter -= new MouseEventHandler(OnChildElementMouseEnter);
                element.MouseLeave -= new MouseEventHandler(OnChildElementMouseLeave);
                ((FrameworkElement)element).SizeChanged -= new SizeChangedEventHandler(OnChildElementSizeChanged);
            }
            this.modelItemToUIElement.Clear();
            this.panel.LocationChanged -= new LocationChangedEventHandler(OnFreeFormPanelLocationChanged);
            this.panel.ConnectorMoved -= new ConnectorMovedEventHandler(OnFreeFormPanelConnectorMoved);
            this.panel.LayoutUpdated -= new EventHandler(OnFreeFormPanelLayoutUpdated);
            this.panel.RequiredSizeChanged -= new RequiredSizeChangedEventHandler(OnFreeFormPanelRequiredSizeChanged);
            this.ViewStateService.ViewStateChanged -= new ViewStateChangedEventHandler(OnViewStateChanged);

            if (this.ModelItem.ItemType == typeof(StateMachine))
            {
                // Only StateMachine supports "States" collection
                this.ModelItem.Properties[StateMachineDesigner.StatesPropertyName].Collection.CollectionChanged -= new NotifyCollectionChangedEventHandler(OnStateCollectionChanged);
                this.ModelItem.PropertyChanged -= new PropertyChangedEventHandler(this.OnModelPropertyChanged);
                ModelTreeManager modelTreeManager = this.Context.Services.GetService<ModelTreeManager>();
                modelTreeManager.EditingScopeCompleted -= new EventHandler<EditingScopeEventArgs>(this.OnEditingScopeCompleted);

                foreach (ModelItem modelItem in this.ModelItem.Properties[StateMachineDesigner.StatesPropertyName].Collection)
                {
                    if (modelItem.ItemType == typeof(State))
                    {
                        ModelItemCollection transitions = modelItem.Properties[StateDesigner.TransitionsPropertyName].Collection;
                        if (this.listenedTransitionCollections.Contains(transitions))
                        {
                            transitions.CollectionChanged -= new NotifyCollectionChangedEventHandler(this.OnTransitionCollectionChanged);
                            this.listenedTransitionCollections.Remove(transitions);
                        }
                    }
                }
            }

            // stateMachineContainerEditor will be null when dropping a State into a WorkflowItemPresenter.
            if (this.ModelItem.ItemType == typeof(State) && this.stateMachineContainerEditor != null)
            {
                this.stateMachineContainerEditor = null;
            }
        }

        #endregion

        #region InitialNode

        void AddInitialNode()
        {
            // Instantiate the initial node
            StartSymbol initialNode = StartSymbol.CreateStartSymbol(this.Context);
            initialNode.Text = "Start";
            this.initialModelItem = initialNode.ModelItem;
            this.modelItemToUIElement.Add(this.initialModelItem, initialNode);
            DragDropHelper.SetCompositeView(initialNode, this);
            this.initialNode = initialNode;
            this.PopulateConnectionPoints(this.initialNode);
            this.initialNode.MouseEnter += new MouseEventHandler(OnChildElementMouseEnter);
            this.initialNode.MouseLeave += new MouseEventHandler(OnChildElementMouseLeave);
            this.panel.Children.Add(this.initialNode);
            this.initialNode.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
            double startHeight = this.initialNode.DesiredSize.Height;
            double startWidth = this.initialNode.DesiredSize.Width;
            object locationOfShape = this.ViewStateService.RetrieveViewState(this.ModelItem, StateContainerEditor.ShapeLocationViewStateKey);
            if (locationOfShape != null)
            {
                Point locationPt = (Point)locationOfShape;
                FreeFormPanel.SetLocation(this.initialNode, locationPt);
            }
            // If the view state is missing, place the initial node in the top middle.
            else
            {
                Point startPoint = new Point(this.panel.MinWidth / 2, startSymbolTopMargin + startHeight / 2);
                Point startLocation = SnapVisualToGrid(this.initialNode, startPoint, new Point(-1, -1), false);
                FreeFormPanel.SetLocation(this.initialNode, startLocation);
                this.internalViewStateChange = true;
                this.StoreShapeLocationViewState(this.ModelItem, startLocation);
                this.internalViewStateChange = false;
            }
            FreeFormPanel.SetChildSize(this.initialNode, new Size(startWidth, startHeight));
        }

        void AddInitialNodeConnector(UIElement initialStateView)
        {
            Fx.Assert(this.ModelItem.ItemType == typeof(StateMachine), "Only StateMachine should have initial state.");
            List<Connector> attachedConnectors = StateContainerEditor.GetAttachedConnectors(this.initialNode);
            if (attachedConnectors.Count == 0)
            {
                ConnectionPoint sourceConnectionPoint = StateContainerEditor.GetConnectionPointClosestToEdgeMidPoint(
                    this.initialNode,
                    StateContainerEditor.GetEmptyConnectionPoints(this.initialNode),
                    EdgeLocation.Bottom);

                Point srcLocation = FreeFormPanel.GetLocation(this.initialNode);
                Size srcSize = FreeFormPanel.GetChildSize(this.initialNode);

                Point destLocation = FreeFormPanel.GetLocation(initialStateView);
                Size destSize = FreeFormPanel.GetChildSize(initialStateView);

                // by default, the connector would connect to the top of the initial state
                EdgeLocation destConnectorEdge = EdgeLocation.Top;

                if (srcLocation.X > destLocation.X + destSize.Width)
                {
                    // if the state is located at the west of the start node, should connect to the right side of initial state.
                    destConnectorEdge = EdgeLocation.Right;
                }
                else if (srcLocation.X + srcSize.Width < destLocation.X)
                {
                    // if the state is located at the east of the start node, should connect to the left side of initial state.
                    destConnectorEdge = EdgeLocation.Left;
                }

                ConnectionPoint destinationConnectionPoint = StateContainerEditor.GetConnectionPointClosestToEdgeMidPoint(
                    initialStateView,
                    StateContainerEditor.GetEmptyConnectionPoints(initialStateView),
                    destConnectorEdge);

                this.AddConnector(this.initialNode,
                    initialStateView,
                    this.ModelItem,
                    sourceConnectionPoint,
                    destinationConnectionPoint);
            }
        }

        #endregion

        #region StateVisuals

        UIElement ProcessAndGetModelView(ModelItem model)
        {
            UIElement element;
            if (!this.modelItemToUIElement.TryGetValue(model, out element))
            {
                element = this.Context.Services.GetService<VirtualizedContainerService>().GetContainer(model, this);
                if (element is VirtualizedContainerService.VirtualizingContainer)
                {
                    // Fix bug 183698 - if the container does not contain other states, the minwidth should
                    // be re-set to the default and let the FreeFormPanel to calculate its actual size.
                    // If a child state was previously expanded, the container's min size would be set
                    // to its expanded size via ContainerService.GetHintSize in GetContainer() method.
                    // But if the item is a simple state, its min size should be reset to the default minimum.
                    ((VirtualizedContainerService.VirtualizingContainer)element).MinWidth = DefaultStateDesignerWidth;
                    ((VirtualizedContainerService.VirtualizingContainer)element).MinHeight = DefaultStateDesignerHeight;
                }
                else
                {
                    Fx.Assert(false, "We expect GetContainer always returns a VirtualizingContainer.");
                }


                element.MouseEnter += new MouseEventHandler(OnChildElementMouseEnter);
                element.MouseLeave += new MouseEventHandler(OnChildElementMouseLeave);
                ((FrameworkElement)element).SizeChanged += new SizeChangedEventHandler(OnChildElementSizeChanged);
                this.modelItemToUIElement.Add(model, element);
                this.PopulateConnectionPoints(element);

                object locationOfShape = this.ViewStateService.RetrieveViewState(model, ShapeLocationViewStateKey);
                object sizeOfShape = this.ViewStateService.RetrieveViewState(model, ShapeSizeViewStateKey);
                if (locationOfShape != null)
                {
                    Point locationPt = (Point)locationOfShape;
                    FreeFormPanel.SetLocation(element, locationPt);
                    this.shapeLocations.Add(locationPt);
                }
                if (sizeOfShape != null)
                {
                    Size size = (Size)sizeOfShape;
                    FreeFormPanel.SetChildSize(element, size);
                    VirtualizedContainerService.VirtualizingContainer virtualizingContainer = element as VirtualizedContainerService.VirtualizingContainer;
                    if (virtualizingContainer != null)
                    {
                        virtualizingContainer.MinWidth = size.Width;
                        virtualizingContainer.MinHeight = size.Height;
                    }
                }
            }
            return element;
        }

        void AddStateVisuals(IList<ModelItem> modelItemCollection)
        {
            List<UIElement> viewsAdded = new List<UIElement>();
            foreach (ModelItem modelItem in modelItemCollection)
            {
                if (!this.modelItemToUIElement.ContainsKey(modelItem))
                {
                    viewsAdded.Add(ProcessAndGetModelView(modelItem));
                }
                else if (!this.panel.Children.Contains(this.modelItemToUIElement[modelItem]))
                {
                    viewsAdded.Add(this.modelItemToUIElement[modelItem]);
                }
            }
            foreach (UIElement view in viewsAdded)
            {
                this.panel.Children.Add(view);
            }

            // We need to wait until after the state visuals are added and displayed.
            this.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
            {
                if (!suppressAddingConnectorsWhenAddingStateVisuals && this.populated)
                {
                    ModelItem stateMachineModelItem = null;
                    foreach (UIElement view in viewsAdded)
                    {
                        ModelItem stateModelItem = StateContainerEditor.GetModelItemFromView(view);
                        this.AddChildTransitionVisualsToStateMachineEditor(stateModelItem);
                        if (stateMachineModelItem == null)
                        {
                            stateMachineModelItem = StateContainerEditor.GetStateMachineModelItem(stateModelItem);
                        }
                        if (stateMachineModelItem.Properties[StateMachineDesigner.InitialStatePropertyName].Value == stateModelItem)
                        {
                            this.AddInitialNodeConnector(view);
                        }
                    }
                }
            }));

        }

        void RemoveStateVisual(UIElement removedStateDesigner)
        {
            HashSet<Connector> connectorsToDelete = new HashSet<Connector>();
            ModelService modelService = this.Context.Services.GetService<ModelService>();
            List<UIElement> removedStateDesigners = new List<UIElement>();
            removedStateDesigners.Add(removedStateDesigner);

            StateContainerEditor stateMachineEditor = this.GetStateMachineContainerEditor();
            foreach (UIElement designer in removedStateDesigners)
            {
                if (stateMachineEditor.activeSrcConnectionPoint != null)
                {
                    List<ConnectionPoint> connectionPoints = GetConnectionPoints(designer);
                    if (connectionPoints.Contains(stateMachineEditor.activeSrcConnectionPoint))
                    {
                        stateMachineEditor.activeSrcConnectionPoint = null;
                        RemoveAdorner(stateMachineEditor.panel, typeof(ConnectorCreationAdorner));
                    }
                }
                if (stateMachineEditor.lastConnectionPointMouseUpElement == designer)
                {
                    stateMachineEditor.lastConnectionPointMouseUpElement = null;
                }
                connectorsToDelete.UnionWith(GetAttachedConnectors(designer));
            }

            // Remove any connector visuals attached to this shape. This is required for the scenarios as follows:
            // Copy and paste two connected States into StateMachine and undo the paste.
            // The Transition is not removed as a model change. Hence the connector visual will remain dangling on the designer.
            foreach (Connector connector in connectorsToDelete)
            {
                this.Remove(connector);
            }

            ModelItem modelItem = ((VirtualizedContainerService.VirtualizingContainer)removedStateDesigner).ModelItem;
            this.modelItemToUIElement.Remove(modelItem);
            removedStateDesigner.MouseEnter -= new MouseEventHandler(OnChildElementMouseEnter);
            removedStateDesigner.MouseLeave -= new MouseEventHandler(OnChildElementMouseLeave);
            ((FrameworkElement)removedStateDesigner).SizeChanged -= new SizeChangedEventHandler(OnChildElementSizeChanged);

            this.panel.Children.Remove(removedStateDesigner);

            // deselect removed item
            if (this.Context != null)
            {
                HashSet<ModelItem> selectedItems = new HashSet<ModelItem>(this.Context.Items.GetValue<Selection>().SelectedObjects);
                if (selectedItems.Contains(modelItem))
                {
                    Selection.Toggle(this.Context, modelItem);
                }
            }

            object locationOfShape = this.ViewStateService.RetrieveViewState(modelItem, StateContainerEditor.ShapeLocationViewStateKey);
            if (locationOfShape != null)
            {
                this.shapeLocations.Remove((Point)locationOfShape);
            }
        }

        #endregion

        #region TransitionVisualsAndConnector

        void AddTransitionVisual(ModelItem transitionModelItem)
        {
            ModelItem sourceState = StateContainerEditor.GetParentStateModelItemForTransition(transitionModelItem);
            ModelItem destinationState = transitionModelItem.Properties[TransitionDesigner.ToPropertyName].Value;
            UIElement sourceDesigner = this.modelItemToUIElement[sourceState];
            UIElement destinationDesigner = this.modelItemToUIElement[destinationState];
            if (sourceDesigner.IsDescendantOf(this) && destinationDesigner.IsDescendantOf(this))
            {
                this.AddConnector(sourceDesigner, destinationDesigner, transitionModelItem);
            }
        }

        void AddTransitionVisuals(IList<ModelItem> transitionModelItemCollection)
        {
            foreach (ModelItem transitionModelItem in transitionModelItemCollection)
            {
                this.AddTransitionVisual(transitionModelItem);
            }
        }

        void AddChildTransitionVisualsToStateMachineEditor(ModelItem stateModelItem)
        {
            Fx.Assert(stateModelItem.ItemType == typeof(State), "The ModelItem should be a State.");
            List<ModelItem> transitions = new List<ModelItem>();
            transitions.AddRange(stateModelItem.Properties[StateDesigner.TransitionsPropertyName].Collection);
            this.GetStateMachineContainerEditor().AddTransitionVisuals(transitions);
        }

        Connector CreateConnector(ConnectionPoint srcConnPoint, ConnectionPoint destConnPoint, PointCollection points, ModelItem connectorModelItem)
        {
            bool isTransition = connectorModelItem != null && connectorModelItem.ItemType == typeof(Transition);
            Connector connector;
            if (isTransition)
            {
                connector = new ConnectorWithStartDot();
            }
            else
            {
                connector = new ConnectorWithoutStartDot();
            }

            connector.FocusVisualStyle = null;
            connector.Focusable = true;
            DesignerView.SetCommandMenuMode(connector, CommandMenuMode.NoCommandMenu);
            if (isTransition)
            {
                SetConnectorLabel(connector, connectorModelItem);
                SetConnectorStartDotToolTip(connector.StartDot, connectorModelItem);
                connector.HighlightOnHover = true;
                connector.ToolTip = new StateConnectionPointToolTip();
                connector.StartDot.MouseDown += new MouseButtonEventHandler(OnConnectorStartDotMouseDown);
                connector.StartDot.MouseUp += new MouseButtonEventHandler(OnConnectorStartDotMouseUp);
            }
            AutomationProperties.SetName(connector, SR.ConnectionAutomationPropertiesName);
            connector.GotKeyboardFocus += new KeyboardFocusChangedEventHandler(OnConnectorGotKeyboardFocus);
            connector.RequestBringIntoView += new RequestBringIntoViewEventHandler(OnConnectorRequestBringIntoView);
            connector.GotFocus += new RoutedEventHandler(OnConnectorGotFocus);
            connector.MouseDoubleClick += new MouseButtonEventHandler(OnConnectorMouseDoubleClick);
            connector.MouseDown += new MouseButtonEventHandler(OnConnectorMouseDown);
            connector.KeyDown += new KeyEventHandler(OnConnectorKeyDown);
            connector.ContextMenuOpening += new ContextMenuEventHandler(OnConnectorContextMenuOpening);
            SetConnectorSrcDestConnectionPoints(connector, srcConnPoint, destConnPoint);
            StateContainerEditor.SetConnectorModelItem(connector, connectorModelItem);
            connector.Unloaded += new RoutedEventHandler(OnConnectorUnloaded);
            connector.Points = points;
            connector.AutoSplitContainer = this;

            if (connectorModelItem.ItemType == typeof(Transition))
            {
                int srcConnectionPointIndex = StateContainerEditor.GetConnectionPoints(srcConnPoint.ParentDesigner).IndexOf(srcConnPoint);
                int destConnectionPointIndex = StateContainerEditor.GetConnectionPoints(destConnPoint.ParentDesigner).IndexOf(destConnPoint);
                this.ViewStateService.StoreViewState(connectorModelItem, SrcConnectionPointIndexStateKey, srcConnectionPointIndex);
                this.ViewStateService.StoreViewState(connectorModelItem, DestConnectionPointIndexStateKey, destConnectionPointIndex);
            }

            return connector;
        }

        private void PopulateVirtualizingContainer(ModelItem item)
        {
            // For a VirtualizedContainer ModelItem, if it is newly created, its View is null.
            // Otherwise its View is still the old view which does not belong to the current stateContainerEditor.
            if (item != null && (item.View == null || DragDropHelper.GetCompositeView((WorkflowViewElement)item.View) != (UIElement)this))
            {
                UIElement element;
                if (modelItemToUIElement.TryGetValue(item, out element) && element is VirtualizedContainerService.VirtualizingContainer)
                {
                    VirtualizedContainerService.VirtualizingContainer container = element as VirtualizedContainerService.VirtualizingContainer;
                    container.Populate();
                }
            }
        }

        private bool IsRightToLeft
        {
            get
            {
                if (!this.isRightToLeft.HasValue)
                {
                    this.isRightToLeft = FreeFormPanelUtilities.IsRightToLeft(this.stateContainerGrid);
                }

                return this.isRightToLeft.Value;
            }
        }

        static bool IsViewStateValid(PointCollection locationPts)
        {
            return locationPts.All<Point>((p) => { return p.X.IsNoLessThan(0) && p.Y.IsNoLessThan(0); });
        }

        // Create a connector from the view state of the connector model item
        Connector CreateConnectorByConnectorModelItemViewState(
            UIElement source,
            UIElement dest,
            ModelItem connectorModelItem)
        {
            Connector connector = null;
            object connectorLocation = this.ViewStateService.RetrieveViewState(connectorModelItem, ConnectorLocationViewStateKey);
            PointCollection locationPts = connectorLocation as PointCollection;
            if (locationPts != null)
            {
                ConnectionPoint srcConnPoint = null, destConnPoint = null;
                if (connectorModelItem.ItemType == typeof(Transition))
                {
                    object srcConnPointIndex = this.ViewStateService.RetrieveViewState(connectorModelItem, SrcConnectionPointIndexStateKey);
                    object destConnPointIndex = this.ViewStateService.RetrieveViewState(connectorModelItem, DestConnectionPointIndexStateKey);

                    if (srcConnPointIndex != null)
                    {
                        List<ConnectionPoint> srcConnPoints = GetConnectionPoints(source);
                        ConnectionPoint viewStateSrcConnPoint = srcConnPoints.ElementAt((int)srcConnPointIndex);

                        if (viewStateSrcConnPoint != null)
                        {
                            srcConnPoint = viewStateSrcConnPoint;
                        }
                    }
                    else if (connectorModelItem.Properties[TransitionDesigner.TriggerPropertyName].Value != null)
                    {
                        srcConnPoint = StateContainerEditor.GetSrcConnectionPointForSharedTrigger(source, connectorModelItem);
                    }

                    if (destConnPointIndex != null)
                    {
                        List<ConnectionPoint> destConnPoints = GetConnectionPoints(dest);
                        ConnectionPoint viewStateDestConnPoint = destConnPoints.ElementAt((int)destConnPointIndex);

                        if (viewStateDestConnPoint != null && GetEmptyConnectionPoints(dest).Contains(viewStateDestConnPoint))
                        {
                            destConnPoint = viewStateDestConnPoint;
                        }
                    }
                }
                if (srcConnPoint == null)
                {
                    srcConnPoint = GetConnectionPoint(source, locationPts[0]);
                }

                if (destConnPoint == null)
                {
                    destConnPoint = GetConnectionPoint(dest, locationPts[locationPts.Count - 1]);
                }

                bool shouldReroute = false;
                if (srcConnPoint == null && destConnPoint == null)
                {
                    StateContainerEditor.GetEmptySrcDestConnectionPoints(source, dest, out srcConnPoint, out destConnPoint);
                    shouldReroute = true;
                }
                else if (srcConnPoint == null && destConnPoint != null)
                {
                    List<ConnectionPoint> srcConnectionPoints = GetEmptyConnectionPoints(source);
                    if (srcConnectionPoints.Count > 0)
                    {
                        srcConnPoint = StateContainerEditor.GetClosestConnectionPointNotOfType(destConnPoint, srcConnectionPoints, ConnectionPointKind.Incoming);
                    }
                    shouldReroute = true;
                }
                else if (destConnPoint == null && srcConnPoint != null)
                {
                    List<ConnectionPoint> destConnectionPoints = GetEmptyConnectionPoints(dest);
                    if (destConnectionPoints.Count > 0)
                    {
                        destConnPoint = StateContainerEditor.GetClosestConnectionPointNotOfType(srcConnPoint, destConnectionPoints, ConnectionPointKind.Outgoing);
                    }
                    shouldReroute = true;
                }
                if (srcConnPoint != null && destConnPoint != null)
                {
                    if (shouldReroute || !IsViewStateValid(locationPts))
                    {
                        PointCollection connectorPoints = new PointCollection(ConnectorRouter.Route(this.panel, srcConnPoint, destConnPoint));
                        this.ViewStateService.StoreViewState(connectorModelItem, ConnectorLocationViewStateKey, connectorPoints);
                        connector = CreateConnector(srcConnPoint, destConnPoint, connectorPoints, connectorModelItem);
                    }
                    else
                    {
                        connector = this.CreateConnector(srcConnPoint, destConnPoint, locationPts, connectorModelItem);
                    }

                }
            }
            return connector;
        }

        PointCollection CreatePointCollectionForAutoConnectOrAutoSplit(UIElement sourceDesigner,
            UIElement destinationDesigner,
            ModelItem connectorModelItem,
            ref ConnectionPoint sourceConnectionPoint,
            ref ConnectionPoint destinationConnectionPoint)
        {
            PointCollection points = null;
            if (this.activeSrcConnectionPointForAutoSplit != null && this.activeSrcConnectionPointForAutoSplit.ParentDesigner == sourceDesigner)
            {
                sourceConnectionPoint = this.activeSrcConnectionPointForAutoSplit;
                this.activeSrcConnectionPointForAutoSplit = null;
                destinationConnectionPoint = StateContainerEditor.GetConnectionPointClosestToEdgeMidPoint(destinationDesigner, StateContainerEditor.GetEmptyConnectionPoints(destinationDesigner), this.entryEdgeForAutoSplit);
            }
            else if (this.activeSrcConnectionPoint != null && this.activeSrcConnectionPoint.ParentDesigner == sourceDesigner)
            {
                sourceConnectionPoint = this.activeSrcConnectionPoint;
                this.activeSrcConnectionPoint = null;
            }
            else
            {
                if (connectorModelItem.ItemType == typeof(Transition) && connectorModelItem.Properties[TransitionDesigner.TriggerPropertyName].Value != null)
                {
                    sourceConnectionPoint = StateContainerEditor.GetSrcConnectionPointForSharedTrigger(sourceDesigner, connectorModelItem);
                }
            }

            if (this.activeDestConnectionPointForAutoSplit != null && this.activeDestConnectionPointForAutoSplit.ParentDesigner == destinationDesigner)
            {
                destinationConnectionPoint = this.activeDestConnectionPointForAutoSplit;
                this.activeDestConnectionPointForAutoSplit = null;
                sourceConnectionPoint = StateContainerEditor.GetConnectionPointClosestToEdgeMidPoint(sourceDesigner, StateContainerEditor.GetEmptyConnectionPoints(sourceDesigner), this.exitEdgeForAutoSplit);
            }

            if (sourceConnectionPoint != null && destinationConnectionPoint == null)
            {
                destinationConnectionPoint = StateContainerEditor.GetClosestDestConnectionPoint(sourceConnectionPoint, destinationDesigner);
            }
            else if (sourceConnectionPoint == null && destinationConnectionPoint != null)
            {
                sourceConnectionPoint = StateContainerEditor.GetClosestSrcConnectionPoint(sourceDesigner, destinationConnectionPoint);
            }
            else if (sourceConnectionPoint == null && destinationConnectionPoint == null)
            {
                StateContainerEditor.GetEmptySrcDestConnectionPoints(sourceDesigner, destinationDesigner, out sourceConnectionPoint, out destinationConnectionPoint);
            }

            if (sourceConnectionPoint != null && destinationConnectionPoint != null)
            {
                points = new PointCollection(ConnectorRouter.Route(this.panel, sourceConnectionPoint, destinationConnectionPoint));
            }
            return points;
        }

        // Create a connector by view state of the connector model item, and if failed, just create a connector using the connection points
        // of the source and destination designers. Then add the connector created to the free form panel.
        void AddConnector(
            UIElement sourceDesigner,
            UIElement destinationDesigner,
            ModelItem connectorModelItem,
            ConnectionPoint sourceConnectionPoint = null,
            ConnectionPoint destinationConnectionPoint = null)
        {
            // Check whether connector already exists.
            // If users programmatically add state with transition to the stateMachine, AddConnector will be called twice for one connectorModelItem.
            if (GetConnectorInStateMachine(connectorModelItem) != null)
            {
                return;
            }

            Connector connector = CreateConnectorByConnectorModelItemViewState(sourceDesigner, destinationDesigner, connectorModelItem);
            if (connector == null)
            {
                PointCollection connectorPoints = this.CreatePointCollectionForAutoConnectOrAutoSplit(sourceDesigner, destinationDesigner, connectorModelItem, ref sourceConnectionPoint, ref destinationConnectionPoint);
                if (connectorPoints != null)
                {
                    connector = CreateConnector(sourceConnectionPoint, destinationConnectionPoint, connectorPoints, connectorModelItem);
                    this.ViewStateService.StoreViewState(connectorModelItem, ConnectorLocationViewStateKey, connectorPoints);
                }
            }
            if (connector != null)
            {
                this.panel.Children.Add(connector);
            }
        }

        void Remove(Connector connector)
        {
            ConnectionPoint srcConnectionPoint = FreeFormPanel.GetSourceConnectionPoint(connector);
            ConnectionPoint destConnectionPoint = FreeFormPanel.GetDestinationConnectionPoint(connector);
            // Update ConnectionPoints
            srcConnectionPoint.AttachedConnectors.Remove(connector);
            destConnectionPoint.AttachedConnectors.Remove(connector);

            StateContainerEditor stateMachineContainer = this.GetStateMachineContainerEditor();
            stateMachineContainer.panel.Children.Remove(connector);
            if (stateMachineContainer.selectedConnector == connector)
            {
                stateMachineContainer.ClearSelectedConnector();
            }

            if (connector is ConnectorWithStartDot)
            {
                connector.StartDot.MouseDown -= new MouseButtonEventHandler(OnConnectorStartDotMouseDown);
                connector.StartDot.MouseUp -= new MouseButtonEventHandler(OnConnectorStartDotMouseUp);
            }

            connector.GotKeyboardFocus -= new KeyboardFocusChangedEventHandler(OnConnectorGotKeyboardFocus);
            connector.RequestBringIntoView -= new RequestBringIntoViewEventHandler(OnConnectorRequestBringIntoView);
            connector.GotFocus -= new RoutedEventHandler(OnConnectorGotFocus);
            connector.MouseDoubleClick -= new MouseButtonEventHandler(OnConnectorMouseDoubleClick);
            connector.MouseDown -= new MouseButtonEventHandler(OnConnectorMouseDown);
            connector.KeyDown -= new KeyEventHandler(OnConnectorKeyDown);
            connector.ContextMenuOpening -= new ContextMenuEventHandler(OnConnectorContextMenuOpening);
            connector.Unloaded -= new RoutedEventHandler(OnConnectorUnloaded);
        }

        #endregion

        #region ConnectionPoint

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

        //widthFraction and heightFraction determine the location of the ConnectionPoint on the UIElement.
        ConnectionPoint CreateConnectionPoint(UIElement element, double widthFraction, double heightFraction, EdgeLocation location, ConnectionPointKind type)
        {
            ConnectionPoint connectionPoint = new ConnectionPoint();
            connectionPoint.EdgeLocation = location;
            connectionPoint.PointType = type;
            connectionPoint.ParentDesigner = element;
            connectionPoint.IsEnabled = false;
            BindingOperations.SetBinding(connectionPoint, ConnectionPoint.LocationProperty, GetConnectionPointBinding(element as FrameworkElement, widthFraction, heightFraction));
            return connectionPoint;
        }

        void PopulateConnectionPoints(UIElement view)
        {
            view.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            List<ConnectionPoint> connectionPoints = new List<ConnectionPoint>();

            const double connectionPointRatio = 0.05;
            ConnectionPointKind connectionPointType = ConnectionPointKind.Default;
            if (view is VirtualizedContainerService.VirtualizingContainer && IsFinalState(((VirtualizedContainerService.VirtualizingContainer)view).ModelItem))
            {
                connectionPointType = ConnectionPointKind.Incoming;
            }
            else if (view is StartSymbol)
            {
                connectionPointType = ConnectionPointKind.Outgoing;
            }

            for (int ii = 1; ii <= ConnectionPointNum; ii++)
            {
                connectionPoints.Add(CreateConnectionPoint(view, 1, ii * connectionPointRatio, EdgeLocation.Right, connectionPointType));
                connectionPoints.Add(CreateConnectionPoint(view, 0, ii * connectionPointRatio, EdgeLocation.Left, connectionPointType));
                connectionPoints.Add(CreateConnectionPoint(view, ii * connectionPointRatio, 0, EdgeLocation.Top, connectionPointType));
                connectionPoints.Add(CreateConnectionPoint(view, ii * connectionPointRatio, 1, EdgeLocation.Bottom, connectionPointType));
            }

            StateContainerEditor.SetConnectionPoints(view, connectionPoints);
        }

        List<ConnectionPoint> ConnectionPointsToShow(UIElement element)
        {
            bool isCreatingConnector = this.IsCreatingConnector();
            List<ConnectionPoint> connectionPointsToShow = new List<ConnectionPoint>();
            if (element is StartSymbol)
            {
                // Don't allow moving the start of a transition to the initial node.
                if (isCreatingConnector || this.IsMovingStartOfConnectorForTransition())
                {
                    return connectionPointsToShow;
                }
                // Don't allow creating more than one connectors from the initial node.
                if ((StateContainerEditor.GetOutgoingConnectors(element).Count > 0) && !this.IsMovingStartOfConnectorFromInitialNode())
                {
                    return connectionPointsToShow;
                }
            }
            else if (element is VirtualizedContainerService.VirtualizingContainer)
            {
                VirtualizedContainerService.VirtualizingContainer container = (VirtualizedContainerService.VirtualizingContainer)element;

                // Don't allow setting final state as the initial state
                if (IsFinalState(container.ModelItem) && this.IsCreatingConnectorFromInitialNode())
                {
                    return connectionPointsToShow;
                }
                // Don't allow moving the start of the initial node connector to a state
                if (this.IsMovingStartOfConnectorFromInitialNode())
                {
                    return connectionPointsToShow;
                }
            }

            List<ConnectionPoint> connectionPoints = StateContainerEditor.GetConnectionPoints(element);
            if (isCreatingConnector)
            {
                connectionPointsToShow.AddRange(connectionPoints.Where<ConnectionPoint>(
                    (p) => { return p.PointType != ConnectionPointKind.Outgoing && p.AttachedConnectors.Count == 0; }));
            }
            else
            {
                connectionPointsToShow.AddRange(connectionPoints.Where<ConnectionPoint>(
                    (p) => { return p.PointType != ConnectionPointKind.Incoming && p.AttachedConnectors.Count == 0; }));
            }

            return connectionPointsToShow;
        }

        private void RemoveConnectionPointsAdorner(UIElement adornedElement)
        {
            IEnumerable<Adorner> adornersRemoved = RemoveAdorner(adornedElement, typeof(ConnectionPointsAdorner));
            Fx.Assert(adornersRemoved.Count() <= 1, "There should be at most one ConnectionPointsAdorner");
            if (adornersRemoved.Count() == 1)
            {
                ConnectionPointsAdorner adorner = (ConnectionPointsAdorner)adornersRemoved.First();
                Fx.Assert(object.Equals(this.activeConnectionPointsAdorner, adorner), "The adorner removed should be the same as the active adorner.");
                this.activeConnectionPointsAdorner = null;
                foreach (ConnectionPoint connectionPoint in adorner.ConnectionPoints)
                {
                    connectionPoint.IsEnabled = false;
                    if (object.Equals(this.activeConnectionPoint, connectionPoint))
                    {
                        this.activeConnectionPoint = null;
                    }
                }
            }
        }

        private List<ConnectionPoint> GetAvailableConnectionPoint(UIElement designer)
        {
            List<ConnectionPoint> connectionPoints = new List<ConnectionPoint>(StateContainerEditor.GetConnectionPoints(designer));
            List<ConnectionPoint> usedConnectionPoints = new List<ConnectionPoint>();
            foreach (ConnectionPoint connectionPoint in connectionPoints.Reverse<ConnectionPoint>())
            {
                if (connectionPoint.AttachedConnectors.Count > 0)
                {
                    usedConnectionPoints.Add(connectionPoint);
                    connectionPoints.Remove(connectionPoint);
                }
            }

            // The active connection point may haven't had connector yet
            if (connectionPoints.Contains(this.activeSrcConnectionPoint))
            {
                connectionPoints.Remove(this.activeSrcConnectionPoint);
                usedConnectionPoints.Add(this.activeSrcConnectionPoint);
            }

            foreach (ConnectionPoint connectionPoint in connectionPoints.Reverse<ConnectionPoint>())
            {
                foreach (ConnectionPoint usedConnectionPoint in usedConnectionPoints)
                {
                    if (DesignerGeometryHelper.ManhattanDistanceBetweenPoints(usedConnectionPoint.Location, connectionPoint.Location) < ConnectionPointMargin)
                    {
                        connectionPoints.Remove(connectionPoint);
                        break;
                    }
                }
            }
            return connectionPoints;
        }


        private void UpdateActiveConnectionPoint(MouseEventArgs e)
        {
            List<ConnectionPoint> connectionPoints = GetAvailableConnectionPoint(this.activeConnectionPointsAdorner.AdornedElement);
            Point mousePosition = e.GetPosition(this.GetStateMachineContainerEditor());
            double minDist;
            ConnectionPoint closestConnectionPoint = ConnectionPoint.GetClosestConnectionPoint(connectionPoints, mousePosition, out minDist);
            if (closestConnectionPoint == null)
            {
                return;
            }

            foreach (ConnectionPoint connectionPoint in this.activeConnectionPointsAdorner.ConnectionPoints)
            {
                if (object.Equals(connectionPoint, closestConnectionPoint))
                {
                    connectionPoint.IsEnabled = true;
                    this.activeConnectionPoint = connectionPoint;
                }
                else
                {
                    connectionPoint.IsEnabled = false;
                    if (object.Equals(connectionPoint, this.activeConnectionPoint))
                    {
                        this.activeConnectionPoint = null;
                    }
                }
            }

            this.activeConnectionPointsAdorner.InvalidateVisual();
        }

        #endregion

        #region ChildElementEventHandlers

        void OnChildElementMouseEnter(object sender, MouseEventArgs e)
        {
            UIElement senderElement = sender as UIElement;
            if (senderElement != null && !this.IsReadOnly)
            {
                if (senderElement is VirtualizedContainerService.VirtualizingContainer)
                {
                    VirtualizedContainerService.VirtualizingContainer stateDesigner =
                        VisualTreeUtils.FindVisualAncestor<VirtualizedContainerService.VirtualizingContainer>(Mouse.DirectlyOver as DependencyObject);
                    // We don't want to show the connection points if the mouse is not directly over this state
                    if (stateDesigner != senderElement)
                    {
                        return;
                    }
                }

                AddConnectionPointsAdorner(senderElement);

                // Remove the adorner on the state designer when entering its child
                if (this.ModelItem.ItemType == typeof(State))
                {
                    VirtualizedContainerService.VirtualizingContainer parent =
                        VisualTreeUtils.FindVisualAncestor<VirtualizedContainerService.VirtualizingContainer>(this);
                    this.RemoveConnectionPointsAdorner(parent);
                }
            }
        }

        void AddConnectionPointsAdorner(UIElement element)
        {
            bool isSelected = false;
            if (element is VirtualizedContainerService.VirtualizingContainer)
            {
                isSelected = (((Selection)this.Context.Items.GetValue<Selection>()).SelectedObjects as ICollection<ModelItem>).Contains(((VirtualizedContainerService.VirtualizingContainer)element).ModelItem);
            }
            ConnectionPointsAdorner connectionPointsAdorner = new StateMachineConnectionPointsAdorner(element, ConnectionPointsToShow(element), isSelected);
            if ((element is VirtualizedContainerService.VirtualizingContainer) && ((VirtualizedContainerService.VirtualizingContainer)element).ModelItem.ItemType == typeof(State))
            {
                connectionPointsAdorner.ToolTip = SR.TransitionConnectionPointTooltip;
            }
            else if (element is StartSymbol)
            {
                connectionPointsAdorner.ToolTip = SR.InitialStateConnectionPointTooltip;
            }

            AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(element);
            Fx.Assert(adornerLayer != null, "Cannot get AdornerLayer.");
            adornerLayer.Add(connectionPointsAdorner);
            // The outmostEditor should handle all the connection point related events for all its descendants
            StateContainerEditor stateMachineContainer = this.GetStateMachineContainerEditor();
            connectionPointsAdorner.MouseDown += new MouseButtonEventHandler(stateMachineContainer.OnConnectionPointMouseDown);
            connectionPointsAdorner.MouseUp += new MouseButtonEventHandler(stateMachineContainer.OnConnectionPointMouseUp);
            connectionPointsAdorner.MouseLeave += new MouseEventHandler(stateMachineContainer.OnConnectionPointMouseLeave);

            this.activeConnectionPointsAdorner = connectionPointsAdorner;
        }

        void OnChildElementMouseLeave(object sender, MouseEventArgs e)
        {
            bool removeConnectionPointsAdorner = true;
            if (Mouse.DirectlyOver != null)
            {
                removeConnectionPointsAdorner = !typeof(ConnectionPointsAdorner).IsAssignableFrom(Mouse.DirectlyOver.GetType());
            }
            if (removeConnectionPointsAdorner)
            {
                this.RemoveConnectionPointsAdorner(sender as UIElement);

                // Add connection points adorner to its containing state
                VirtualizedContainerService.VirtualizingContainer stateDesigner = VisualTreeUtils.FindVisualAncestor<VirtualizedContainerService.VirtualizingContainer>(this);
                StateContainerEditor parentContainer = VisualTreeUtils.FindVisualAncestor<StateContainerEditor>(stateDesigner);
                if (stateDesigner != null && parentContainer != null && !parentContainer.IsReadOnly)
                {
                    this.AddConnectionPointsAdorner(stateDesigner);
                }
            }
        }

        void OnChildElementSizeChanged(object sender, SizeChangedEventArgs e)
        {
            VirtualizedContainerService.VirtualizingContainer view = sender as VirtualizedContainerService.VirtualizingContainer;

            if (view != null)
            {
                // Such size changed events are a result of changes already committed in the UI. Hence we do not want to react to such view state changes.
                // Using internalViewStateChange flag for that purpose.
                this.internalViewStateChange = true;
                ModelItem storageModelItem = view.ModelItem;
                this.ViewStateService.StoreViewState(storageModelItem, ShapeSizeViewStateKey, ((UIElement)sender).DesiredSize);
                this.internalViewStateChange = false;
            }
        }

        #endregion

        #region ConnectorEventHandlers

        void OnConnectorStartDotMouseUp(object sender, MouseButtonEventArgs e)
        {
            this.activeSrcConnectionPoint = null;
            RemoveAdorner(this.panel, typeof(ConnectorCreationAdorner));
        }

        void OnConnectorStartDotMouseDown(object sender, MouseButtonEventArgs e)
        {
            DependencyObject startDot = (DependencyObject)sender;
            Connector connector = VisualTreeUtils.FindVisualAncestor<Connector>(startDot);
            this.activeSrcConnectionPoint = FreeFormPanel.GetSourceConnectionPoint(connector);
            e.Handled = true;
        }


        void OnConnectorMouseDown(object sender, MouseButtonEventArgs e)
        {
            Connector connector = (Connector)sender;
            if (this.panel.Children.Contains(connector))
            {
                this.selectedConnector = connector;
            }

            // In order to not let WorkflowViewElement handle the event, which would cause the
            // ConnectorEditor to be removed.
            e.Handled = true;
        }


        void OnConnectorUnloaded(object sender, RoutedEventArgs e)
        {
            ModelItem primarySelection = this.Context.Items.GetValue<Selection>().PrimarySelection;
            if (object.Equals(primarySelection, StateContainerEditor.GetConnectorModelItem(sender as DependencyObject)))
            {
                if (primarySelection != null)
                {
                    Selection.Toggle(this.Context, primarySelection);
                }
            }
        }

        // Marking e.Handled = true to avoid scrolling in large workflows to bring the
        // area of a connector in the center of the view region.
        void OnConnectorRequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
        {
            e.Handled = true;
        }

        // Marking e.Handled true for the case where a connector is clicked.
        // This is to prevent WorkflowViewElement class from making StateMachine as the current selection.
        void OnConnectorGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            e.Handled = true;
        }

        void OnConnectorGotFocus(object sender, RoutedEventArgs e)
        {
            Connector connector = e.Source as Connector;

            if (this.panel.connectorEditor == null || !connector.Equals(this.panel.connectorEditor.Connector))
            {
                this.panel.RemoveConnectorEditor();
                this.panel.connectorEditor = new ConnectorEditor(this.panel, connector);
            }

            if (this.panel.Children.Contains(connector))
            {
                this.updatingSelectedConnector = true;
                ModelItem connectorModelItem = StateContainerEditor.GetConnectorModelItem(connector);
                Selection newSelection = new Selection();
                if (connectorModelItem != null && connectorModelItem.ItemType == typeof(Transition))
                {
                    newSelection = new Selection(connectorModelItem);
                }
                this.Context.Items.SetValue(newSelection);
                this.selectedConnector = connector;
                this.updatingSelectedConnector = false;

                if (connectorModelItem.ItemType == typeof(Transition))
                {
                    // Populate the source and destination States's View if it is still in Virtualized mode.
                    ModelItem destinationState = connectorModelItem.Properties[TransitionDesigner.ToPropertyName].Value;
                    PopulateVirtualizingContainer(destinationState);
                    ModelItem sourceState = StateContainerEditor.GetParentStateModelItemForTransition(connectorModelItem);
                    PopulateVirtualizingContainer(sourceState);

                    // Assign its destination State's View on the connector model item for copy/paste function.
                    ((IModelTreeItem)connectorModelItem).SetCurrentView(destinationState.View);
                }

                e.Handled = true;
            }
        }

        private void OnSelectionChanged(Selection selection)
        {
            // If selection changed, remove ConnectorEditor if existed.
            // Only if the selection changed is caused by adding ConnectorEditor when OnConnectorGotFocus.
            if (!this.updatingSelectedConnector && this.panel != null && this.panel.connectorEditor != null)
            {
                this.panel.RemoveConnectorEditor();
            }
        }

        void OnConnectorKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Return)
            {
                this.DesignerView.MakeRootDesigner(StateContainerEditor.GetConnectorModelItem(sender as DependencyObject));
                e.Handled = true;
            }
        }

        void OnConnectorMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                ModelItem connectorModelItem = StateContainerEditor.GetConnectorModelItem(sender as DependencyObject);
                if (connectorModelItem != null && connectorModelItem.ItemType == typeof(Transition))
                {
                    this.DesignerView.MakeRootDesigner(connectorModelItem);
                    e.Handled = true;
                }
            }
        }

        void OnConnectorContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            // Disable context menu
            e.Handled = true;
        }

        #endregion

        #region ConnectionPointEventHandlers

        void OnConnectionPointMouseDown(object sender, MouseButtonEventArgs e)
        {
            UIElement srcElement = ((Adorner)sender).AdornedElement as UIElement;
            this.activeSrcConnectionPoint = ConnectionPointHitTest(srcElement, e.GetPosition(this.panel));

            if (this.activeSrcConnectionPoint != null &&
                !this.activeSrcConnectionPoint.ParentDesigner.IsKeyboardFocusWithin)
            {
                // If a floating annotation is visible, it needs to lose the keyboard focus
                // to hide itself again (bug 200739). Therefore, selecting the src connection
                // point would give the keyboard focus back to its source parent state.
                Keyboard.Focus(VirtualizedContainerService.TryGetVirtualizedElement(this.activeSrcConnectionPoint.ParentDesigner));
            }

            e.Handled = true;
        }

        void OnConnectionPointMouseLeave(object sender, MouseEventArgs e)
        {
            UIElement adornedElement = ((Adorner)sender).AdornedElement as UIElement;
            this.RemoveConnectionPointsAdorner(adornedElement);
        }

        void OnConnectionPointMouseUp(object sender, MouseButtonEventArgs e)
        {
            UIElement dest = ((Adorner)sender).AdornedElement as UIElement;
            if (this.activeSrcConnectionPoint != null)
            {
                ConnectionPoint destConnectionPoint = ConnectionPointHitTest(dest, e.GetPosition(this.panel));
                if (destConnectionPoint != null && !this.activeSrcConnectionPoint.Equals(destConnectionPoint))
                {
                    ConnectorCreationResult result = CreateConnectorGesture(this.activeSrcConnectionPoint, destConnectionPoint, null, false);
                    if (result != ConnectorCreationResult.Success)
                    {
                        StateContainerEditor.ReportConnectorCreationError(result);
                    }
                }
                this.activeSrcConnectionPoint = null;
                RemoveAdorner(this.panel, typeof(ConnectorCreationAdorner));
            }
            else
            {
                //This will cause the FreeFormPanel to handle the event and is useful while moving a connector end point.
                this.lastConnectionPointMouseUpElement = dest;
                dest.RaiseEvent(e);
            }
            this.RemoveConnectionPointsAdorner(dest);
        }

        #endregion

        #region FreeFormPanelEventHandlers

        void OnFreeFormPanelLocationChanged(object sender, System.Activities.Presentation.FreeFormEditing.LocationChangedEventArgs e)
        {
            Fx.Assert(sender is UIElement, "Sender should be of type UIElement");
            Connector movedConnector = sender as Connector;
            if (movedConnector != null)
            {
                //ViewState is undoable only when a user gesture moves a connector. If the FreeFormPanel routes a connector,
                //the change is not undoable.
                bool isUndoableViewState = false;
                ModelItem connectorModelItem = StateContainerEditor.GetConnectorModelItem(movedConnector);
                PointCollection existingViewState = this.ViewStateService.RetrieveViewState(connectorModelItem, ConnectorLocationViewStateKey) as PointCollection;
                if (existingViewState != null && existingViewState.Count > 0 && movedConnector.Points.Count > 0
                    && existingViewState[0].Equals(movedConnector.Points[0]) && existingViewState[existingViewState.Count - 1].Equals(movedConnector.Points[movedConnector.Points.Count - 1]))
                {
                    isUndoableViewState = true;
                }
                StoreConnectorLocationViewState(movedConnector, isUndoableViewState);
            }
            else
            {
                //This is called only when a shape without ViewState is auto-layout'd by the FreeFormPanel.
                UIElement view = sender as UIElement;

                if (view != null)
                {
                    StoreShapeLocationViewState(view, e.NewLocation);
                }
            }
        }

        void UpdateStateMachineOnConnectorMoved(ConnectionPoint knownConnectionPoint, Point newPoint, Connector movedConnector, bool isConnectorStartMoved)
        {
            HitTestResult hitTestResult = VisualTreeHelper.HitTest(this.panel, newPoint);
            if (hitTestResult == null)
            {
                return;
            }

            UIElement newViewElement = null;
            ConnectionPoint newConnectionPoint = null;

            //The case where the Connector is dropped on a ConnectionPoint.
            if (this.lastConnectionPointMouseUpElement != null)
            {
                newConnectionPoint = StateContainerEditor.ConnectionPointHitTest(this.lastConnectionPointMouseUpElement, newPoint);
                if (newConnectionPoint != null)
                {
                    newViewElement = this.lastConnectionPointMouseUpElement;
                }
                this.lastConnectionPointMouseUpElement = null;
            }

            //The case where the link is dropped on a shape.
            if (newViewElement == null)
            {
                newViewElement = VisualTreeUtils.FindVisualAncestor<WorkflowViewElement>(hitTestResult.VisualHit);
            }

            if (newViewElement != null)
            {
                if (this.panel.IsAncestorOf(newViewElement))
                {
                    using (EditingScope es = (EditingScope)this.ModelItem.BeginEdit(SR.MoveLink))
                    {
                        // Remove the old connector ModelItem
                        this.DeleteConnectorModelItem(movedConnector, true);
                        // Create new connector
                        ConnectorCreationResult result = ConnectorCreationResult.OtherFailure;
                        if (!isConnectorStartMoved)
                        {
                            if (newConnectionPoint == null)
                            {
                                result = CreateConnectorGesture(knownConnectionPoint, newViewElement, movedConnector, false);
                            }
                            else
                            {
                                result = CreateConnectorGesture(knownConnectionPoint, newConnectionPoint, movedConnector, false);
                            }
                        }
                        else
                        {
                            // Don't allow moving the start of the initial node connector to a state
                            if (!(newViewElement is StateDesigner && StateContainerEditor.IsConnectorFromInitialNode(movedConnector)))
                            {
                                if (newConnectionPoint == null)
                                {
                                    result = CreateConnectorGesture(newViewElement, knownConnectionPoint, movedConnector, true);
                                }
                                else
                                {
                                    result = CreateConnectorGesture(newConnectionPoint, knownConnectionPoint, movedConnector, true);
                                }
                            }
                        }

                        if (result == ConnectorCreationResult.Success)
                        {
                            es.Complete();
                        }
                        else
                        {
                            StateContainerEditor.ReportConnectorCreationError(result);
                            es.Revert();
                        }
                    }
                }
            }
        }

        void OnFreeFormPanelConnectorMoved(object sender, ConnectorMovedEventArgs e)
        {
            Connector movedConnector = sender as Connector;
            int movedEndConnectorPointIndex = movedConnector.Points.Count - 1;
            int newEndConnectorPointIndex = e.NewConnectorLocation.Count - 1;

            if (movedConnector != null)
            {
                Fx.Assert(e.NewConnectorLocation.Count > 0, "Invalid connector editor");
                if (!e.NewConnectorLocation[0].Equals(movedConnector.Points[0]))
                {
                    // source moved
                    ConnectionPoint destConnPoint = FreeFormPanel.GetDestinationConnectionPoint(movedConnector);
                    UpdateStateMachineOnConnectorMoved(destConnPoint, e.NewConnectorLocation[0], movedConnector, true);
                }
                else if (!e.NewConnectorLocation[newEndConnectorPointIndex].Equals(movedConnector.Points[movedEndConnectorPointIndex]))
                {
                    // destination moved
                    ConnectionPoint srcConnPoint = FreeFormPanel.GetSourceConnectionPoint(movedConnector);
                    Point destPoint = e.NewConnectorLocation[newEndConnectorPointIndex];
                    UpdateStateMachineOnConnectorMoved(srcConnPoint, destPoint, movedConnector, false);
                }

                this.selectedConnector = movedConnector;
            }
        }

        // This is to keep this.selectedConnector up to date.
        // Cases included: 1. create a connector, select it and undo, 2. move a connector from one shape to another.
        void OnFreeFormPanelLayoutUpdated(object sender, EventArgs e)
        {
            if (this.selectedConnector != null && !this.panel.Children.Contains(this.selectedConnector))
            {
                this.ClearSelectedConnector();
            }
        }

        void OnFreeFormPanelRequiredSizeChanged(object sender, RequiredSizeChangedEventArgs e)
        {
            this.requiredSize = e.NewRequiredSize;
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (this.requiredSize.Width > this.StateContainerWidth)
                {
                    this.ViewStateService.StoreViewState(
                        this.ModelItem,
                        StateContainerEditor.StateContainerWidthViewStateKey,
                        this.requiredSize.Width);
                }
                if (this.requiredSize.Height > this.StateContainerHeight)
                {
                    this.ViewStateService.StoreViewState(
                        this.ModelItem,
                        StateContainerEditor.StateContainerHeightViewStateKey,
                        this.requiredSize.Height);
                }
            }));
        }

        #endregion

        #region StateContainerGridEventHandlers

        void OnStateContainerGridMouseLeave(object sender, MouseEventArgs e)
        {
            bool endLinkCreation = !IsVisualHit(sender as UIElement, sender as UIElement, e.GetPosition(sender as IInputElement));
            if (endLinkCreation)
            {
                RemoveAdorner(this.panel, typeof(ConnectorCreationAdorner));
                this.activeSrcConnectionPoint = null;
            }
        }

        void OnStateContainerGridMouseMove(object sender, MouseEventArgs e)
        {
            if (this.activeSrcConnectionPoint != null)
            {
                Point[] points = ConnectorRouter.Route(this.panel, this.activeSrcConnectionPoint, e);
                if (points == null)
                {
                    e.Handled = true;
                    return;
                }
                List<Point> segments = new List<Point>(points);
                // Remove the previous adorner.
                RemoveAdorner(this.panel, typeof(ConnectorCreationAdorner));
                // Add new adorner.
                AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(this.panel);
                Fx.Assert(adornerLayer != null, "Adorner Layer does not exist");
                ConnectorCreationAdorner newAdorner = new ConnectorCreationAdorner(this.panel, segments);
                adornerLayer.Add(newAdorner);
                e.Handled = true;
            }
        }

        void OnStateContainerGridPreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (this.activeConnectionPointsAdorner != null)
            {
                this.UpdateActiveConnectionPoint(e);
            }

            // Creating connector
            if (this.activeSrcConnectionPoint != null)
            {
                AutoScrollHelper.AutoScroll(e, this, 1);
            }
            // Reconnecting connector
            else if (this.panel.connectorEditor != null && (this.panel.connectorEditor.IsConnectorEndBeingMoved || this.panel.connectorEditor.IsConnectorStartBeingMoved))
            {
                AutoScrollHelper.AutoScroll(e, this, 1);
            }
        }

        void OnStateContainerGridPreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            VirtualizedContainerService.VirtualizingContainer destElement = VisualTreeUtils.FindVisualAncestor<VirtualizedContainerService.VirtualizingContainer>(e.OriginalSource as DependencyObject);
            if (destElement != null && destElement.IsPopulated && destElement.Child is StateDesigner)
            {
                if (this.activeSrcConnectionPoint != null)
                {
                    ConnectorCreationResult result = this.CreateConnectorGesture(this.activeSrcConnectionPoint, destElement, null, false);
                    if (result != ConnectorCreationResult.Success)
                    {
                        StateContainerEditor.ReportConnectorCreationError(result);
                    }
                    this.RemoveConnectionPointsAdorner(destElement);
                }
            }
            if (this.activeSrcConnectionPoint != null)
            {
                this.activeSrcConnectionPoint = null;
                RemoveAdorner(this.panel, typeof(ConnectorCreationAdorner));
            }
        }

        void SetEndPointsAndInvalidateViewState(Connector connector, Vector offset, bool offsetSrc)
        {
            ConnectionPoint srcConnectionPoint = FreeFormPanel.GetSourceConnectionPoint(connector);
            ConnectionPoint destConnectionPoint = FreeFormPanel.GetDestinationConnectionPoint(connector);
            Point src, dest;
            if (offsetSrc)
            {
                src = Point.Add(FreeFormPanel.GetLocationRelativeToOutmostPanel(srcConnectionPoint), offset);
                dest = FreeFormPanel.GetLocationRelativeToOutmostPanel(destConnectionPoint);
            }
            else
            {
                src = FreeFormPanel.GetLocationRelativeToOutmostPanel(srcConnectionPoint);
                dest = Point.Add(FreeFormPanel.GetLocationRelativeToOutmostPanel(destConnectionPoint), offset);
            }
            PointCollection points = new PointCollection();
            Point invalidPoint = new Point(-1, -1);
            points.Add(src);
            points.Add(invalidPoint); // this invalidates the view state
            points.Add(dest);
            this.StoreConnectorLocationViewState(StateContainerEditor.GetConnectorModelItem(connector), points, true);
        }

        void OffsetConnectorViewState(UIElement view, Point oldLocation, Point newLocation, bool offsetNonContainedConnectors)
        {
            // There is no need to do anything for the StartSymbol
            if (view is VirtualizedContainerService.VirtualizingContainer)
            {
                Vector offset = new Vector(newLocation.X - oldLocation.X, newLocation.Y - oldLocation.Y);

                // connectors whose dest points are outside the state and the src points are inside/on the state
                HashSet<Connector> outgoingConnectors = new HashSet<Connector>();
                // connectors whose src points are outside the state and the dest points are inside/on the state
                HashSet<Connector> incomingConnectors = new HashSet<Connector>();
                // connectors whose src points and dest points are both inside/on the state
                HashSet<ModelItem> containedTransitions = new HashSet<ModelItem>();

                if (view != null)
                {
                    // Here the incomingConnectors and outgoingConnectors contains connectors whose src points and dest
                    // points are both inside/on the state; they will be removed later on
                    List<Connector> connectors = StateContainerEditor.GetIncomingConnectors(view);
                    foreach (Connector connector in connectors)
                    {
                        incomingConnectors.Add(connector);
                    }
                    connectors = StateContainerEditor.GetOutgoingConnectors(view);
                    foreach (Connector connector in connectors)
                    {
                        outgoingConnectors.Add(connector);
                    }
                }

                // Add common connectors to the containedConnectors set and remove them
                // from the outgoingConnectors and incomingConnectors sets
                foreach (Connector connector in outgoingConnectors.Reverse<Connector>())
                {
                    if (incomingConnectors.Contains(connector))
                    {
                        containedTransitions.Add(StateContainerEditor.GetConnectorModelItem(connector));
                        outgoingConnectors.Remove(connector);
                        incomingConnectors.Remove(connector);
                    }
                }

                // For contained connectors, we offset all the points.
                this.OffsetLocationViewStates(offset, null, containedTransitions, true);

                if (offsetNonContainedConnectors)
                {
                    // For incoming connectors, we offset the end point and invalidate the view state.
                    // This way the start and end point will still connect to the same connection points
                    // on the source and destination shapes and later on the connector will be rerouted using
                    // those two fixed points.
                    foreach (Connector connector in incomingConnectors)
                    {
                        this.SetEndPointsAndInvalidateViewState(connector, offset, false);
                    }

                    // for outgoing connectors, we offset the start point and invalidate the view state.
                    foreach (Connector connector in outgoingConnectors)
                    {
                        this.SetEndPointsAndInvalidateViewState(connector, offset, true);
                    }
                }
                else
                {                    
                    HashSet<ModelItem> nonSelfTransitions = new HashSet<ModelItem>();

                    foreach (Connector connector in incomingConnectors)
                    {
                        nonSelfTransitions.Add(StateContainerEditor.GetConnectorModelItem(connector));
                    }
                    
                    foreach (Connector connector in outgoingConnectors)
                    {
                        nonSelfTransitions.Add(StateContainerEditor.GetConnectorModelItem(connector));
                    }

                    // Store ViewState for all non-self transitions to support undo/redo.
                    this.OffsetLocationViewStates(offset, null, nonSelfTransitions, true);
                }
            }
        }

        void OnStateContainerGridDrop(object sender, DragEventArgs e)
        {
            ModelItemHelper.TryCreateImmediateEditingScopeAndExecute(this.ModelItem.GetEditingContext(), System.Activities.Presentation.SR.CollectionAddEditingScopeDescription, (es) =>
            {
                this.DoStateContainerGridDrop(e, AutoConnectDirections.None, null);
                if (es != null)
                {
                    es.Complete();
                }
            });
        }

        // Returns the last dropped item - used for auto-connect and auto-split where only one item is allowed
        ModelItem DoStateContainerGridDrop(DragEventArgs e, AutoConnectDirections autoConnectDirection, Connector connectorToSplit)
        {
            ModelItem droppedModelItem = null;
            e.Effects = DragDropEffects.None;
            IEnumerable<object> droppedObjects = DragDropHelper.GetDroppedObjects(this, e, Context);
            // Marking the event as being handled. In whichever case we want to route the event, it will be unmarked explicitly.
            e.Handled = true;
            List<ModelItem> modelItemsToSelect = new List<ModelItem>();

            Dictionary<WorkflowViewElement, Point> relativeLocations = DragDropHelper.GetDraggedViewElementRelativeLocations(e);
            foreach (object droppedObject in droppedObjects)
            {
                if (droppedObject != null)
                {
                    droppedModelItem = droppedObject as ModelItem;
                    bool isAnchorPointValid = true;
                    Point anchorPoint = DragDropHelper.GetDragDropAnchorPoint(e);

                    // This is the case of dragging from toolbox
                    if (anchorPoint.X < 0 && anchorPoint.Y < 0)
                    {
                        isAnchorPointValid = false;
                    }
                    // This is the case of dragging from the designer surface
                    else if (droppedModelItem != null)
                    {
                        WorkflowViewElement view = droppedModelItem.View as WorkflowViewElement;
                        anchorPoint.Offset(-relativeLocations[view].X, -relativeLocations[view].Y);
                    }

                    StateContainerEditor srcContainer = droppedModelItem != null
                        ? DragDropHelper.GetCompositeView(droppedModelItem.View as WorkflowViewElement) as StateContainerEditor
                        : null;
                    bool externalDrop = false;
                    if (droppedModelItem != null && srcContainer != null && srcContainer.Equals(this))
                    {
                        // Internal move
                        PerformInternalMove(this.modelItemToUIElement[droppedModelItem], e.GetPosition(this.panel), anchorPoint, autoConnectDirection, connectorToSplit);
                    }
                    else
                    {
                        // External model Item drop
                        if (droppedModelItem != null)
                        {
                            if (droppedModelItem.ItemType == typeof(State) && this.ModelItem.ItemType == typeof(StateMachine))
                            {
                                this.InsertState(droppedModelItem);
                                externalDrop = true;
                            }
                        }
                        // Toolbox drop.
                        else
                        {
                            if (droppedObject.GetType() == typeof(State))
                            {
                                if (((State)droppedObject).DisplayName == null)
                                {
                                    ((State)droppedObject).DisplayName = GenerateStateName();
                                }

                                droppedModelItem = InsertState(droppedObject);
                            }
                            else if (droppedObject.GetType() == typeof(FinalState))
                            {
                                droppedModelItem = InsertState(new State()
                                {
                                    DisplayName = DefaultFinalStateDisplayName,
                                    IsFinal = true
                                });
                            }
                        }
                        if (droppedModelItem != null)
                        {
                            modelItemsToSelect.Add(droppedModelItem);
                            UIElement view = null;
                            if (this.modelItemToUIElement.ContainsKey(droppedModelItem))
                            {
                                view = this.modelItemToUIElement[droppedModelItem];
                            }
                            else
                            {
                                view = droppedModelItem.View as WorkflowViewElement;
                                if (view == null)
                                {
                                    view = this.Context.Services.GetService<ViewService>().GetView(droppedModelItem) as WorkflowViewElement;
                                    ViewUtilities.MeasureView(view as WorkflowViewElement, true);
                                }
                            }
                            // If drag anchor point is beyond the size of the shape being dropped,
                            if (anchorPoint.X > view.DesiredSize.Width || anchorPoint.Y > view.DesiredSize.Height)
                            {
                                isAnchorPointValid = false;
                            }
                            Point shapeLocation;
                            if (autoConnectDirection != AutoConnectDirections.None)
                            {
                                shapeLocation = this.CalculateDropLocationForAutoConnect(autoConnectDirection, new Size(DefaultStateDesignerWidth, DefaultStateDesignerHeight));
                            }
                            else
                            {
                                shapeLocation = StateContainerEditor.SnapVisualToGrid(view, e.GetPosition(this.panel), anchorPoint, isAnchorPointValid);
                            }
                            if (connectorToSplit != null)
                            {
                                shapeLocation = this.CalculateDropLocationForAutoSplit(e.GetPosition(this.panel), shapeLocation, connectorToSplit, new Size(DefaultStateDesignerWidth, DefaultStateDesignerHeight));
                            }
                            object viewState = this.ViewStateService.RetrieveViewState(droppedModelItem, ShapeLocationViewStateKey);
                            if (externalDrop)
                            {
                                Fx.Assert(viewState != null, "item dropped from external should already have view states");
                                Fx.Assert(droppedModelItem.View != null, "item dropped from extenal should already have view");
                                VirtualizedContainerService.VirtualizingContainer container = VisualTreeUtils.FindVisualAncestor<VirtualizedContainerService.VirtualizingContainer>(droppedModelItem.View);
                                Fx.Assert(container != null, "container should not be null");
                                Point oldLocation = (Point)viewState;
                                oldLocation = srcContainer.panel.GetLocationRelativeToOutmostPanel(oldLocation);
                                Point newLocation = this.panel.GetLocationRelativeToOutmostPanel(shapeLocation);
                                // To make sure the connectors are still connected to the connection points
                                OffsetConnectorViewState(container, oldLocation, newLocation, true);
                            }
                            this.StoreShapeLocationViewState(droppedModelItem, shapeLocation);
                        }
                    }
                }
            }

            DragDropHelper.SetDragDropMovedViewElements(e, new WorkflowViewElement[] { });
            this.Dispatcher.BeginInvoke(() =>
                {
                    bool first = true;
                    foreach (ModelItem modelItem in modelItemsToSelect)
                    {
                        if (first)
                        {
                            Keyboard.Focus((IInputElement)modelItem.View);
                            Selection.SelectOnly(this.Context, modelItem);
                            first = false;
                        }
                        else
                        {
                            Selection.Union(this.Context, modelItem);
                        }
                    }
                },
                DispatcherPriority.ApplicationIdle);

            return droppedModelItem;
        }

        Point CalculateDropLocationForAutoConnect(AutoConnectDirections autoConnectDirection, Size droppedSize)
        {
            return AutoConnectHelper.CalculateDropLocation(droppedSize, this.Panel.CurrentAutoConnectTarget, autoConnectDirection, this.shapeLocations);
        }

        Point CalculateDropLocationForAutoSplit(Point mousePosition, Point originalDropLocation, Connector connector, Size droppedSize)
        {
            return AutoSplitHelper.CalculateDropLocation(mousePosition, originalDropLocation, connector, droppedSize, this.shapeLocations);
        }

        void OnStateContainerGridDragEnter(object sender, DragEventArgs e)
        {
            OnStateContainerGridDrag(sender, e);
        }

        void OnStateContainerGridDragOver(object sender, DragEventArgs e)
        {
            OnStateContainerGridDrag(sender, e);
        }

        void OnStateContainerGridDrag(object sender, DragEventArgs e)
        {
            if (!e.Handled)
            {
                if (IsDropAllowed(e))
                {
                    e.Effects |= DragDropEffects.Move;
                }
                else
                {
                    e.Effects = DragDropEffects.None;
                }
                e.Handled = true;
            }
        }

        private bool IsDropAllowed(DragEventArgs e)
        {
            // Considering multiple items drag&drop, use ModelItemsDataFormat instead.
            IEnumerable<ModelItem> modelItems = e.Data.GetData(DragDropHelper.ModelItemsDataFormat) as IEnumerable<ModelItem>;
            if (modelItems != null)
            {
                foreach (ModelItem modelItem in modelItems)
                {
                    if (modelItem.ItemType == typeof(StartNode) && modelItem == this.initialModelItem)
                    {
                        // StartNode of current StateMachine allow to drop.
                    }
                    else if (modelItem.ItemType == typeof(State) && this.IsStateMachineContainer && StateContainerEditor.AreInSameStateMachine(modelItem, this.ModelItem))
                    {
                        // When FinalState has been dropped into a StateMachine, it becomes a State instead. So ignore FinalState type.
                        // State within the same StateMachine allow to drop.
                    }
                    else
                    {
                        return false;
                    }
                }

                return true;
            }
            else if (this.ModelItem.ItemType == typeof(StateMachine) && DragDropHelper.AllowDrop(e.Data, this.Context, typeof(State), typeof(FinalState), typeof(StartNode)))
            {
                // Only allow State, FinalState, StartNode to drop into a StateMachine from tool box.
                return true;
            }

            return false;
        }

        void KeyboardMove(Key key)
        {
            Vector moveDir = FreeFormPanel.CalculateMovement(key, this.IsRightToLeft);
            
            using (EditingScope es = (EditingScope)this.ModelItem.BeginEdit(SR.ItemMove))
            {
                foreach (ModelItem selectedModelItem in this.Context.Items.GetValue<Selection>().SelectedObjects)
                {
                    UIElement shapeToMove = this.modelItemToUIElement[selectedModelItem];
                    Point currentLocation = FreeFormPanel.GetLocation(shapeToMove);
                    Point newLocation = Point.Add(currentLocation, moveDir);
                    // Make sure the newLocation is positive.
                    newLocation.X = FreeFormPanel.ZeroIfNegative(newLocation.X);
                    newLocation.Y = FreeFormPanel.ZeroIfNegative(newLocation.Y);
                    if (newLocation == currentLocation)
                    {
                        continue;
                    }
                    PerformInternalMove(shapeToMove, newLocation, null, AutoConnectDirections.None, null);
                }
                es.Complete();
            }
        }

        void OnStateContainerGridKeyDown(object sender, KeyEventArgs e)
        {
            if (this.activeSrcConnectionPoint != null)
            {
                // Ignore Keyboard input when creating connector
                e.Handled = true;
                return;
            }

            Selection currentSelection = this.Context.Items.GetValue<Selection>();
            if (e.Key == Key.Delete && this.selectedConnector != null && currentSelection.SelectionCount <= 1)
            {
                // process the delete if only the connector is selected
                ModelItem primarySelection = currentSelection.PrimarySelection;
                //Delete connector
                ModelItem connectorModelItem = StateContainerEditor.GetConnectorModelItem(this.selectedConnector);
                if (object.Equals(primarySelection, connectorModelItem) ||
                    // Delete initial link
                    primarySelection == null && connectorModelItem != null && connectorModelItem.ItemType != typeof(Transition))
                {
                    this.DeleteConnectorModelItem(this.selectedConnector);
                    e.Handled = true;
                }
            }
            else if ((new List<Key> { Key.Left, Key.Right, Key.Up, Key.Down }).Contains(e.Key)
                && currentSelection.SelectedObjects.All<ModelItem>((p) => { return this.modelItemToUIElement.ContainsKey(p); }))
            {
                this.KeyboardMove(e.Key);
                e.Handled = true;
            }
        }

        void OnStateContainerGridPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            this.selectedConnector = null;
        }

        #endregion

        #region Misc

        string GenerateStateName()
        {
            HashSet<String> existingStateNames = new HashSet<string>();
            foreach (ModelItem stateModelItem in this.ModelItem.Properties[StateMachineDesigner.StatesPropertyName].Collection)
            {
                existingStateNames.Add(((State)stateModelItem.GetCurrentValue()).DisplayName);
            }

            int suffix = 0;
            string name;

            do
            {
                name = string.Format(CultureInfo.CurrentUICulture, "{0}{1}", DefaultStateDisplayName, ++suffix);
            } while (existingStateNames.Contains<string>(name));

            return name;
        }

        internal StateContainerEditor GetStateMachineContainerEditor()
        {
            if (this.ModelItem.ItemType == typeof(StateMachine))
            {
                return this;
            }
            else
            {
                Fx.Assert(this.ModelItem.ItemType == typeof(State), "ModelItem type should be State.");
                StateDesigner designer = VisualTreeUtils.FindVisualAncestor<StateDesigner>(this);
                FreeFormPanel panel = designer.GetStateMachineFreeFormPanel();
                return VisualTreeUtils.FindVisualAncestor<StateContainerEditor>(panel);
            }
        }

        Connector GetConnectorInStateMachine(ModelItem connectorModelItem)
        {
            StateContainerEditor stateMachineContainer = this.GetStateMachineContainerEditor();
            foreach (UIElement element in stateMachineContainer.panel.Children)
            {
                Connector connector = element as Connector;
                if (connector != null)
                {
                    if (StateContainerEditor.GetConnectorModelItem(connector) == connectorModelItem)
                    {
                        return connector;
                    }
                }
            }
            return null;
        }

        bool IsCreatingConnector()
        {
            StateContainerEditor stateMachineContainer = this.GetStateMachineContainerEditor();
            return (stateMachineContainer.activeSrcConnectionPoint != null || (stateMachineContainer.panel.connectorEditor != null && stateMachineContainer.panel.connectorEditor.IsConnectorEndBeingMoved));
        }

        bool IsCreatingConnectorFromInitialNode()
        {
            StateContainerEditor stateMachineContainer = this.GetStateMachineContainerEditor();
            return (stateMachineContainer.activeSrcConnectionPoint != null && stateMachineContainer.activeSrcConnectionPoint.ParentDesigner is StartSymbol) ||
                (stateMachineContainer.panel.connectorEditor != null && stateMachineContainer.panel.connectorEditor.IsConnectorEndBeingMoved &&
                stateMachineContainer.panel.connectorEditor.Connector != null &&
                IsConnectorFromInitialNode(stateMachineContainer.panel.connectorEditor.Connector));
        }

        bool IsMovingStartOfConnectorFromInitialNode()
        {
            StateContainerEditor stateMachineContainer = this.GetStateMachineContainerEditor();
            return (stateMachineContainer.panel.connectorEditor != null && stateMachineContainer.panel.connectorEditor.IsConnectorStartBeingMoved &&
                stateMachineContainer.panel.connectorEditor.Connector != null &&
                IsConnectorFromInitialNode(stateMachineContainer.panel.connectorEditor.Connector));
        }

        bool IsMovingStartOfConnectorForTransition()
        {
            StateContainerEditor stateMachineContainer = this.GetStateMachineContainerEditor();
            return (stateMachineContainer.panel.connectorEditor != null && stateMachineContainer.panel.connectorEditor.IsConnectorStartBeingMoved &&
                stateMachineContainer.panel.connectorEditor.Connector != null &&
                GetConnectorModelItem(stateMachineContainer.panel.connectorEditor.Connector).ItemType == typeof(Transition));
        }

        void InvalidateMeasureForStateMachinePanel()
        {
            this.GetStateMachineContainerEditor().panel.InvalidateMeasure();
        }

        void PerformInternalMove(UIElement movedElement, Point newPoint, Point? shapeAnchorPoint, AutoConnectDirections autoConnectDirection, Connector connectorToSplit)
        {
            using (EditingScope es = (EditingScope)this.ModelItem.BeginEdit(SR.ItemMove))
            {
                StoreShapeSizeWithUndoRecursively(this.ModelItem);
                this.RemoveConnectionPointsAdorner(movedElement);
                Point newLocation;
                Size size = FreeFormPanel.GetChildSize(movedElement);
                if (autoConnectDirection != AutoConnectDirections.None)
                {
                    newLocation = this.CalculateDropLocationForAutoConnect(autoConnectDirection, size);
                }
                else if (shapeAnchorPoint.HasValue)
                {
                    newLocation = SnapVisualToGrid(movedElement, newPoint, shapeAnchorPoint.Value, true);
                }
                else
                {
                    Fx.Assert(newPoint.X.IsNoLessThan(0) && newPoint.Y.IsNoLessThan(0),
                        "newPoint is negative");
                    newLocation = newPoint;
                }
                if (connectorToSplit != null)
                {
                    newLocation = this.CalculateDropLocationForAutoSplit(newPoint, newLocation, connectorToSplit, size);
                }
                ModelItem modelItem = GetModelItemFromView(movedElement);
                object viewState = this.ViewStateService.RetrieveViewState(modelItem, ShapeLocationViewStateKey);
                if (viewState != null)
                {
                    Point oldLocation = (Point)viewState;
                    // To make sure the connectors are still connected to the connection points.
                    // We don't need to offset non-contained connectors because internal move
                    // won't cause the connectors to be recreated and we have code in FreeFormPanel
                    // to guarantee that connectors will still be connected to the connection points
                    this.OffsetConnectorViewState(movedElement, oldLocation, newLocation, false);
                }
                else
                {
                    this.StoreAttachedConnectorViewStates(movedElement);
                }

                this.StoreShapeLocationViewState(movedElement, newLocation);
                // To make sure the connector changes are undoable
                this.panel.RemoveConnectorEditor();

                es.Complete();
            }
        }

        public void StoreAttachedConnectorViewStates(UIElement element)
        {
            foreach (Connector connector in GetAttachedConnectors(element))
            {
                StoreConnectorLocationViewState(connector, true);
            }
        }

        bool ShouldInitialize()
        {
            WorkflowViewElement parent = VisualTreeUtils.FindVisualAncestor<WorkflowViewElement>(this);
            return parent != null && parent.ModelItem != null && (parent.ModelItem.ItemType == typeof(StateMachine) && parent.ShowExpanded ||
                   parent.ModelItem.ItemType == typeof(State) && !parent.IsRootDesigner);
        }

        void ClearSelectedConnector()
        {
            if (this.panel.connectorEditor != null && this.panel.connectorEditor.Connector == this.selectedConnector)
            {
                this.panel.RemoveConnectorEditor();
            }
            this.selectedConnector = null;
        }

        #endregion

        #region AutoConnect

        public void DoAutoConnect(DragEventArgs e, UIElement targetElement, AutoConnectDirections direction)
        {
            UIElement sourceElement = targetElement;
            bool immediatelyCommit = ModelItemHelper.CanCreateImmediateEditingScope(this.ModelItem);

            using (EditingScope scope = (EditingScope)this.ModelItem.BeginEdit(SR.AutoConnect, immediatelyCommit))
            {
                ModelItem connectorModelItem = null;
                Point location = e.GetPosition(sourceElement);
                ModelItem droppedModelItem = this.DoStateContainerGridDrop(e, direction, null);
                if (droppedModelItem != null)
                {
                    connectorModelItem = this.DoAutoConnect(sourceElement, droppedModelItem, null);
                }

                if (connectorModelItem != null)
                {
                    EdgeLocation edgeLocation = AutoConnectHelper.AutoConnectDirection2EdgeLocation(direction);
                    this.GetStateMachineContainerEditor().activeSrcConnectionPoint = this.GetSourceConnectionPointForAutoConnect(sourceElement, edgeLocation);
                    ModelItem sourceModelItem = TryGetModelItemFromView(sourceElement);
                    Fx.Assert(sourceModelItem != null, "sourceModelItem");

                    // add a custom change inside a new editing scope since current editing scope an immediate editing scope
                    using (EditingScope es = (EditingScope)this.ModelItem.BeginEdit(SR.AutoConnect, false))
                    {
                        es.Changes.Add(new StoreAutoConnectorViewStateChange(
                            this.ModelItem, sourceModelItem, droppedModelItem, connectorModelItem, edgeLocation));
                        es.Complete();
                    }
                    scope.Complete();
                }
                else
                {
                    scope.Revert();
                }
            }
        }

        private ModelItem DoAutoConnect(UIElement sourceElement, ModelItem droppedModelItem, Transition transitionToCopy, int insertIndex = InvalidIndex)
        {
            ModelItem sourceModelItem = TryGetModelItemFromView(sourceElement);
            if (sourceModelItem != null && droppedModelItem.ItemType == typeof(State))
            {
                if (sourceModelItem.ItemType == typeof(State))
                {
                    ModelItem stateMachineModelItem = GetStateMachineModelItem(sourceModelItem);
                    Transition transition = new Transition
                    {
                        DisplayName = StateContainerEditor.GenerateTransitionName(stateMachineModelItem),
                        To = droppedModelItem.GetCurrentValue() as State
                    };
                    if (transitionToCopy != null)
                    {
                        transition.Action = transitionToCopy.Action;
                        transition.Condition = transitionToCopy.Condition;
                        transition.DisplayName = transitionToCopy.DisplayName;
                        transition.Trigger = transitionToCopy.Trigger;
                    }
                    ModelItem trasitionModelItem = null;
                    if (insertIndex >= 0)
                    {
                        trasitionModelItem = sourceModelItem.Properties[StateDesigner.TransitionsPropertyName].Collection.Insert(insertIndex, transition);
                    }
                    else
                    {
                        trasitionModelItem = sourceModelItem.Properties[StateDesigner.TransitionsPropertyName].Collection.Add(transition);
                    }
                    Fx.Assert(trasitionModelItem != null, "trasitionModelItem");
                    return trasitionModelItem;
                }
                // auto-connect from the initial node
                else if (sourceModelItem.ItemType == typeof(StartNode))
                {
                    this.ModelItem.Properties[StateMachineDesigner.InitialStatePropertyName].SetValue(droppedModelItem);
                    return this.ModelItem;
                }
            }
            return null;
        }

        public AutoConnectDirections GetDirectionsAllowed(DragEventArgs e, UIElement targetElement)
        {
            if (!this.IsDropAllowed(e))
            {
                return AutoConnectDirections.None;
            }

            if (StateContainerEditor.GetEmptyConnectionPoints(targetElement).Count < 1)
            {
                return AutoConnectDirections.None;
            }

            List<Type> types = DragDropHelper.GetDraggedTypes(e.Data);
            if (types.Count != 1 || (types[0] != typeof(State) && types[0] != typeof(FinalState)))
            {
                return AutoConnectDirections.None;
            }

            if (targetElement is VirtualizedContainerService.VirtualizingContainer && IsFinalState(((VirtualizedContainerService.VirtualizingContainer)targetElement).ModelItem))
            {
                return AutoConnectDirections.None;
            }

            if (targetElement is StartSymbol && this.ModelItem.ItemType == typeof(StateMachine))
            {
                if (this.ModelItem.Properties[StateMachineDesigner.InitialStatePropertyName].Value != null)
                {
                    return AutoConnectDirections.None;
                }

                // Should not allow auto-connecting a final state to the start symbol
                IEnumerable<ModelItem> draggedModelItems = DragDropHelper.GetDraggedModelItems(e);
                if ((draggedModelItems.Count<ModelItem>() == 1 && StateContainerEditor.IsFinalState(draggedModelItems.First<ModelItem>()))
                    || types[0] == typeof(FinalState))
                {
                    return AutoConnectDirections.None;
                }
            }

            return AutoConnectDirections.Top | AutoConnectDirections.Bottom | AutoConnectDirections.Left | AutoConnectDirections.Right;
        }

        private ConnectionPoint GetSourceConnectionPointForAutoConnect(UIElement designer, EdgeLocation edgeLocation)
        {
            List<ConnectionPoint> connectionPoints = this.GetAvailableConnectionPoint(designer);
            return GetConnectionPointClosestToEdgeMidPoint(designer, connectionPoints, edgeLocation);
        }

        static ConnectionPoint GetConnectionPointClosestToEdgeMidPoint(UIElement designer, List<ConnectionPoint> connectionPoints, EdgeLocation edgeLocation)
        {
            Point midPoint = new Point(-1, -1);
            Point location = FreeFormPanel.GetLocation(designer);
            Size size = FreeFormPanel.GetChildSize(designer);
            switch (edgeLocation)
            {
                case EdgeLocation.Left:
                    midPoint = new Point(location.X, location.Y + size.Height / 2);
                    break;
                case EdgeLocation.Right:
                    midPoint = new Point(location.X + size.Width, location.Y + size.Height / 2);
                    break;
                case EdgeLocation.Top:
                    midPoint = new Point(location.X + size.Width / 2, location.Y);
                    break;
                case EdgeLocation.Bottom:
                    midPoint = new Point(location.X + size.Width / 2, location.Y + size.Height);
                    break;
            }
            if (connectionPoints.Count > 0)
            {
                double dist;
                return ConnectionPoint.GetClosestConnectionPoint(connectionPoints, midPoint, out dist);
            }
            return null;
        }

        private static ModelItem TryGetModelItemFromView(UIElement sourceElement)
        {
            Fx.Assert(sourceElement != null, "sourceElement != null");
            ModelItem sourceModelItem = null;
            if (sourceElement is WorkflowViewElement)
            {
                sourceModelItem = ((WorkflowViewElement)sourceElement).ModelItem;
            }
            else if (sourceElement is VirtualizedContainerService.VirtualizingContainer)
            {
                sourceModelItem = ((VirtualizedContainerService.VirtualizingContainer)sourceElement).ModelItem;
            }
            return sourceModelItem;
        }

        private class StoreAutoConnectorViewStateChange : Change
        {
            private EdgeLocation EdgeLocation { get; set; }
            private ModelItem StateMachine { get; set; }
            private ModelItem SrcModelItem { get; set; }
            private ModelItem DstModelItem { get; set; }
            private ModelItem ViewStateOwnerModelItem { get; set; }
            private object OldViewState { get; set; }
            private object NewViewState { get; set; }
            private bool ShouldCreateConnector { get; set; }

            public override string Description { get { return SR.AutoConnect; } }

            public StoreAutoConnectorViewStateChange(
                ModelItem stateMachine, ModelItem srcModelItem, ModelItem desModelItem, ModelItem addedModelItem, EdgeLocation edgeLocatioin)
            {
                this.StateMachine = stateMachine;
                this.SrcModelItem = srcModelItem;
                this.DstModelItem = desModelItem;
                this.EdgeLocation = edgeLocatioin;
                this.ViewStateOwnerModelItem = addedModelItem;
                this.ShouldCreateConnector = true;
            }

            private StoreAutoConnectorViewStateChange()
            {
            }

            public override bool Apply()
            {
                StateMachineDesigner designer = this.StateMachine.View as StateMachineDesigner;
                Fx.Assert(designer != null, "designer");
                StateContainerEditor editor = designer.StateContainerEditor;
                Fx.Assert(editor != null, "editor");
                // only the first time we need to calculate the ViewState,
                // later, we just use the old one.
                if (this.ShouldCreateConnector)
                {
                    UIElement srcElement = editor.modelItemToUIElement[this.SrcModelItem];
                    UIElement desElement = editor.modelItemToUIElement[this.DstModelItem];
                    ConnectionPoint srcConnectionPoint = null;
                    ConnectionPoint desConnectionPoint = null;
                    PointCollection points = editor.CreatePointCollectionForAutoConnectOrAutoSplit(
                        srcElement, desElement, this.SrcModelItem, ref srcConnectionPoint, ref desConnectionPoint);
                    this.OldViewState = editor.ViewStateService.RetrieveViewState(this.ViewStateOwnerModelItem, ConnectorLocationViewStateKey);
                    this.NewViewState = points;

                    // compare old and new values, if they're the same, return false
                    if (this.OldViewState == null && this.NewViewState == null)
                    {
                        return false;
                    }

                    if (this.OldViewState != null
                        && this.NewViewState != null
                        && points.SequenceEqual(this.OldViewState as PointCollection))
                    {
                        return false;
                    }

                    this.ShouldCreateConnector = false;
                }

                editor.ViewStateService.StoreViewState(this.ViewStateOwnerModelItem, ConnectorLocationViewStateKey, this.NewViewState);
                return true;
            }

            public override Change GetInverse()
            {
                return new StoreAutoConnectorViewStateChange
                {
                    StateMachine = this.StateMachine,
                    ViewStateOwnerModelItem = this.ViewStateOwnerModelItem,
                    EdgeLocation = this.EdgeLocation,
                    SrcModelItem = this.SrcModelItem,
                    DstModelItem = this.DstModelItem,
                    OldViewState = this.NewViewState,
                    NewViewState = this.OldViewState,
                    ShouldCreateConnector = false
                };
            }
        }

        #endregion

        #region AutoSplit

        public bool CanAutoSplit(DragEventArgs e)
        {
            if (!this.IsDropAllowed(e))
            {
                return false;
            }
            ModelItem draggedModelItem = e.Data.GetData(DragDropHelper.ModelItemDataFormat) as ModelItem;
            if (draggedModelItem != null && this.modelItemToUIElement.ContainsKey(draggedModelItem))
            {
                if (StateContainerEditor.GetAttachedConnectors(this.modelItemToUIElement[draggedModelItem]).Count > 0)
                {
                    return false;
                }

                if (StateContainerEditor.IsFinalState(draggedModelItem))
                {
                    return false;
                }
            }

            List<Type> draggedTypes = DragDropHelper.GetDraggedTypes(e.Data);
            if (draggedTypes.Count != 1 || draggedTypes[0] != typeof(State))
            {
                return false;
            }

            return true;
        }

        public void DoAutoSplit(DragEventArgs e, Connector connector)
        {
            bool immediatelyCommit = ModelItemHelper.CanCreateImmediateEditingScope(this.ModelItem);

            using (EditingScope scope = (EditingScope)this.ModelItem.BeginEdit(SR.AutoSplit, immediatelyCommit))
            {
                ModelItem droppedModelItem = this.DoStateContainerGridDrop(e, AutoConnectDirections.None, connector);
                bool autoSplit = false;
                ConnectionPoint sourceConnectionPoint = FreeFormPanel.GetSourceConnectionPoint(connector);
                ConnectionPoint destinationConnectionPoint = FreeFormPanel.GetDestinationConnectionPoint(connector);
                if (droppedModelItem != null)
                {
                    ModelItem oldConnectorModelItem = StateContainerEditor.GetConnectorModelItem(connector);
                    int index = this.DeleteConnectorModelItem(connector);
                    bool autoConnected = this.DoAutoConnect(sourceConnectionPoint.ParentDesigner,
                        droppedModelItem, oldConnectorModelItem.GetCurrentValue() as Transition, index) != null;
                    if (autoConnected)
                    {
                        ModelItem destinationModelItem = ((VirtualizedContainerService.VirtualizingContainer)destinationConnectionPoint.ParentDesigner).ModelItem;
                        ModelItem stateMachineModelItem = GetStateMachineModelItem(destinationModelItem);
                        droppedModelItem.Properties[StateDesigner.TransitionsPropertyName].Collection.Add(new Transition()
                        {
                            DisplayName = StateContainerEditor.GenerateTransitionName(stateMachineModelItem),
                            To = destinationModelItem.GetCurrentValue() as State
                        });
                        autoSplit = true;
                    }
                }
                if (autoSplit)
                {
                    // Auto-split generates 4 changes: 1) drop state, 2) remove the old transition, 3) create a transition from the source state
                    // to the dropped state, and 4) create a transition from the dropped state to the destination state.
                    // Step 1 may result in creating the visual of all outgoing transition from the dropped state. Step 4) also creates the visual
                    // of the new transition from the dropped state. So the visual of the transition will be created twice. To solve that problem,
                    // we need to suppress adding connector when adding state visual (in the UI reaction for step 1).
                    // And to support redo, we must place the suppression in the undo stack.
                    this.Context.Services.GetService<ModelTreeManager>().AddToCurrentEditingScope(new SuppressAddingConnectorWhenAddingStateVisual());
                    this.activeSrcConnectionPointForAutoSplit = sourceConnectionPoint;
                    this.activeDestConnectionPointForAutoSplit = destinationConnectionPoint;
                    AutoSplitHelper.CalculateEntryExitEdges(e.GetPosition(this.panel), connector, out this.entryEdgeForAutoSplit, out this.exitEdgeForAutoSplit);
                    scope.Complete();
                }
                else
                {
                    scope.Revert();
                }
            }
        }

        private const int InvalidIndex = -1;

        #endregion

        class SuppressAddingConnectorWhenAddingStateVisual : Change
        {
            public override string Description
            {
                get
                {
                    return null;
                }
            }

            public override bool Apply()
            {
                return false;
            }

            public override Change GetInverse()
            {
                return new SuppressAddingConnectorWhenAddingStateVisual();
            }
        }

        internal enum ConnectorCreationResult
        {
            Success,
            CannotCreateTransitionToCompositeState,
            CannotCreateTransitionFromAncestorToDescendant,
            CannotSetCompositeStateAsInitialState,
            CannotSetFinalStateAsInitialState,
            OtherFailure
        }
    }
}
