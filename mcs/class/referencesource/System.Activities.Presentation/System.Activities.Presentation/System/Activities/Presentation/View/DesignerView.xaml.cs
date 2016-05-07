//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation.View
{
    using System.Activities.Presentation.View;
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.Services;
    using System.Activities.Presentation.Xaml;
    using System.Activities.Presentation.Hosting;
    using System.Collections;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.IO.Packaging;
    using System.Printing;
    using System.Reflection;
    using System.Runtime;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Data;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Interop;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Threading;
    using System.Windows.Xps;
    using System.Windows.Xps.Packaging;
    using System.Linq;
    using System.Windows.Shapes;
    using System.Collections.Generic;
    using System.Activities.Presentation.Validation;
    using System.Activities.Presentation.Internal.PropertyEditing;
    using System.ServiceModel.Activities;
    using Microsoft.Tools.Common;
    using System.Activities.Presentation.Sqm;

    // <summary>
    // Interaction logic for DesignerView.xaml
    // </summary>
    [Fx.Tag.XamlVisible(false)]
    public partial class DesignerView : UserControl
    {
        public static readonly DependencyProperty RootDesignerProperty =
            DependencyProperty.Register("RootDesigner", typeof(UIElement), typeof(DesignerView), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(DesignerView.OnRootDesignerChanged)));

        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(DesignerView), new UIPropertyMetadata(OnIsReadOnlyChanged));

        static readonly DependencyPropertyKey ActivitySchemaPropertyKey =
            DependencyProperty.RegisterReadOnly("ActivitySchema", typeof(ModelItem), typeof(DesignerView), new UIPropertyMetadata(OnActivitySchemaChanged));

        public static readonly DependencyProperty ActivitySchemaProperty = ActivitySchemaPropertyKey.DependencyProperty;

        static readonly DependencyPropertyKey FocusedViewElementPropertyKey =
            DependencyProperty.RegisterReadOnly("FocusedViewElement", typeof(WorkflowViewElement), typeof(DesignerView), new UIPropertyMetadata(null));

        public static readonly DependencyProperty InPanModeProperty =
            DependencyProperty.Register("InPanMode", typeof(bool), typeof(DesignerView), new UIPropertyMetadata(OnInPanModeChanged));

        public static readonly DependencyProperty FocusedViewElementProperty = FocusedViewElementPropertyKey.DependencyProperty;

        internal static DependencyProperty ShouldExpandAllProperty = DependencyProperty.Register("ShouldExpandAll", typeof(bool), typeof(DesignerView), new PropertyMetadata(false, new PropertyChangedCallback(OnExpandAllCollapseAllChanged)));
        internal static DependencyProperty ShouldCollapseAllProperty = DependencyProperty.Register("ShouldCollapseAll", typeof(bool), typeof(DesignerView), new PropertyMetadata(false, new PropertyChangedCallback(OnExpandAllCollapseAllChanged)));

        const double scrollDeltaDivider = 100.0;

        GridLength bottomPaneHeight;
        EditingContext context;
        DragDropHelper.ViewElementDragShadow viewElementDragShadow;
        ZoomToTicksConverter zoomToTicksConverter;
        ShellBarItemVisibility shellBarItemVisibility = ShellBarItemVisibility.Variables | ShellBarItemVisibility.Arguments | ShellBarItemVisibility.Imports;
        ShellHeaderItemsVisibility shellHeaderItemsVisibility = ShellHeaderItemsVisibility.Breadcrumb | ShellHeaderItemsVisibility.ExpandAll | ShellHeaderItemsVisibility.CollapseAll;
        Dictionary<ModelItem, ModelItem> selectionMap = new Dictionary<ModelItem, ModelItem>();
        private bool isInErrorState = false;

        const string breadCrumbRootKey = "BreadCrumbRoot";
        const string selectionKey = "Selection";
        const string zoomFactorKey = "ZoomFactor";


        internal WorkflowViewElement lastClickedDesigner;
        IVSSqmService sqmService;
        ScrollViewerPanner scrollViewerPanner;
        RubberBandSelector rubberBandSelector;

        private DesignerViewProxy proxy;

        private DesignerView()
        {
        }

        internal DesignerView(EditingContext context)
        {
            this.proxy = new DesignerViewProxy(this);
            this.context = context;
            InitializeComponent();
            this.InitializeMenuActions();
            foreach (UIElement element in this.designerExtensionSurface.Children)
            {
                element.IsEnabled = false;
            }

            this.buttonArguments1.IsChecked = false;
            UpdateArgumentsButtonVisibility(false);

            this.zoomToTicksConverter = new ZoomToTicksConverter(this, this.zoomSlider, this.zoomPicker);
            this.zoomSlider.ValueChanged += new RoutedPropertyChangedEventHandler<double>(OnZoomSliderValueChanged);
            HideBottomPane();

            this.variables1.VariableCollectionChanged += this.OnVariablesCollectionChanged;
            this.arguments1.ArgumentCollectionChanged += this.OnArgumentsCollectionChanged;
            Dispatcher.UnhandledException += this.proxy.OnDispatcherUnhandledException;
            this.ShouldIgnoreDataGridAutoCommit = false;
            this.sqmService = this.Context.Services.GetService<IVSSqmService>();
            this.buttonPanMode.Visibility = this.IsPanModeEnabled ? Visibility.Visible : Visibility.Collapsed;
            this.rubberBandSelector = this.IsRubberBandSelectionEnabled ? new RubberBandSelector(this.context) : null;
        }

        void OnReadOnlyStateChanged(ReadOnlyState state)
        {
            this.IsReadOnly = state.IsReadOnly;
        }

        void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            if (!e.Handled)
            {
                if (!isInErrorState)
                {
                    isInErrorState = true;

                    //try to prun the visual tree and collapse all the workflow view elements that are too deep
                    //this is due to the limitation of WPF has a visual tree depth limit.
                    if (e.Exception is InvalidOperationException)
                    {
                        ICollection<WorkflowViewElement> deepElements = VisualTreeUtils.PrunVisualTree<WorkflowViewElement>(this.RootDesigner);
                        foreach (WorkflowViewElement viewElement in deepElements)
                        {
                            viewElement.ForceCollapse();
                        }
                    }
                    Exception ex = e.Exception.InnerException ?? e.Exception;
                    ErrorReporting.ShowErrorMessage(ex);
                    isInErrorState = false;
                }
                e.Handled = true;
            }
        }

        public bool IsMultipleSelectionMode
        {
            get;
            private set;
        }

        void OnDesignerViewLoaded(object sender, RoutedEventArgs e)
        {
            ViewStateService viewStateService = this.Context.Services.GetService<ViewStateService>();
            ModelTreeManager modelTreeManager = this.context.Services.GetService<ModelTreeManager>();
            //Initialize ShouldExpandAll if it exists in ViewState.
            object expandAllState = viewStateService.RetrieveViewState(modelTreeManager.Root, DesignerView.ShouldExpandAllProperty.Name);
            if (expandAllState != null)
            {
                this.ShouldExpandAll = (bool)expandAllState;
            }
            if (!this.ShouldExpandAll)
            {
                object collapseAllState = viewStateService.RetrieveViewState(modelTreeManager.Root, DesignerView.ShouldCollapseAllProperty.Name);
                if (collapseAllState != null)
                {
                    this.ShouldCollapseAll = (bool)collapseAllState;
                }
            }
            // SQM: Open Minimap through designer surface
            this.buttonMinimap.Checked += new RoutedEventHandler(SqmOpenMinimap);
            this.expandAllButton.Click += new RoutedEventHandler(SqmExpandAll);
            this.collapseAllButton.Click += new RoutedEventHandler(SqmCollapseAll);

            if (this.IsPanModeEnabled)
            {
                this.scrollViewerPanner = new ScrollViewerPanner(this.ScrollViewer);
                this.scrollViewerPanner.Hand = (Cursor)this.Resources["ReadyToPanCursor"];
                this.scrollViewerPanner.DraggingHand = (Cursor)this.Resources["PanningCursor"];
            }
        }

        void OnDesignerViewUnloaded(object sender, RoutedEventArgs e)
        {
            if (this.scrollViewerPanner != null)
            {
                this.scrollViewerPanner.ScrollViewer = null;
                this.scrollViewerPanner = null;
            }
        }

        void SqmCollapseAll(object sender, RoutedEventArgs e)
        {
            if (this.collapseAllButton.IsChecked == true)
            {
                FeatureUsageCounter.ReportUsage(sqmService, WorkflowDesignerFeatureId.CollapseAll);
            }
            else
            {
                FeatureUsageCounter.ReportUsage(sqmService, WorkflowDesignerFeatureId.Restore);
            }
        }

        void SqmExpandAll(object sender, RoutedEventArgs e)
        {
            if (this.expandAllButton.IsChecked == true)
            {
                FeatureUsageCounter.ReportUsage(sqmService, WorkflowDesignerFeatureId.ExpandAll);
            }
            else
            {
                FeatureUsageCounter.ReportUsage(sqmService, WorkflowDesignerFeatureId.Restore);
            }
        }

        void SqmOpenMinimap(object sender, RoutedEventArgs e)
        {
            FeatureUsageCounter.ReportUsage(sqmService, WorkflowDesignerFeatureId.Minimap);
        }

        static void OnExpandAllCollapseAllChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ((DesignerView)o).OnExpandAllCollapseAllChanged(e);
        }

        void OnExpandAllCollapseAllChanged(DependencyPropertyChangedEventArgs e)
        {
            ViewStateService viewStateService = this.Context.Services.GetService<ViewStateService>();
            ModelTreeManager modelTreeManager = this.context.Services.GetService<ModelTreeManager>();
            {
                viewStateService.StoreViewState(modelTreeManager.Root, e.Property.Name, e.NewValue);
            }
        }


        protected override void OnInitialized(EventArgs e)
        {
            this.AddHandler(UIElement.GotKeyboardFocusEvent, new KeyboardFocusChangedEventHandler(this.OnWorkflowElementGotKeyboardFocus), true);
            this.AddHandler(UIElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(this.OnDesignerSurfaceMouseLeftButtonDown), true);
            this.scrollViewer.AddHandler(UIElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(this.OnScrollViewerMouseLeftButtonDown), true);
            base.OnInitialized(e);
            this.Foreground = new SolidColorBrush(SystemColors.ControlTextColor);
            this.Loaded += this.OnDesignerViewLoaded;
            this.Unloaded += this.OnDesignerViewUnloaded;

            this.IsKeyboardFocusWithinChanged += this.OnDesignerKeyboardFocusWithinChanged;

            this.MenuItemStyle = (Style)this.FindResource("menuItemStyle");
            Fx.Assert(this.MenuItemStyle != null, "menuItemStyle resource not found");
            this.MenuSeparatorStyle = (Style)this.FindResource("separatorStyle");
            Fx.Assert(this.MenuSeparatorStyle != null, "separatorStyle resource not found");

            ReadOnlyState state = this.Context.Items.GetValue<ReadOnlyState>();
            this.IsReadOnly = state.IsReadOnly;
            this.Context.Items.Subscribe<ReadOnlyState>(OnReadOnlyStateChanged);
        }

        public ModelItem ActivitySchema
        {
            get { return (ModelItem)GetValue(ActivitySchemaProperty); }
            private set { SetValue(ActivitySchemaPropertyKey, value); }
        }

        public EditingContext Context
        {
            get { return this.context; }
        }

        public UIElement RootDesigner
        {
            get { return (UIElement)GetValue(RootDesignerProperty); }
            set { SetValue(RootDesignerProperty, value); }
        }

        public bool ShouldExpandAll
        {
            get { return (bool)GetValue(ShouldExpandAllProperty); }
            set { SetValue(ShouldExpandAllProperty, value); }
        }

        public bool ShouldCollapseAll
        {
            get { return (bool)GetValue(ShouldCollapseAllProperty); }
            set { SetValue(ShouldCollapseAllProperty, value); }
        }

        public bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        public WorkflowViewElement FocusedViewElement
        {
            get { return (WorkflowViewElement)GetValue(FocusedViewElementProperty); }
            private set { SetValue(FocusedViewElementPropertyKey, value); }
        }

        public double ZoomFactor
        {
            get
            {
                return this.zoomToTicksConverter.ZoomFactor;
            }
        }

        internal ScrollViewer ScrollViewer
        {
            get
            {
                return this.scrollViewer;
            }
        }

        internal UIElement ScrollableContent
        {
            get
            {
                return this.scrollableContent;
            }
        }

        internal bool SuppressSelectionOnMouseUp
        {
            get
            {
                if (this.rubberBandSelector == null)
                {
                    return false;
                }
                return this.rubberBandSelector.IsSelected;
            }
        }

        internal bool ShouldIgnoreDataGridAutoCommit
        {
            get;
            set;
        }

        internal bool ShouldStillAllowRubberBandEvenIfMouseLeftButtonDownIsHandled
        {
            get;
            set;
        }

        internal bool InPanMode
        {
            get { return (bool)GetValue(InPanModeProperty); }
            set { SetValue(InPanModeProperty, value); }
        }

        private bool IsPanModeEnabled
        {
            get
            {
                DesignerConfigurationService configurationService = this.Context.Services.GetService<DesignerConfigurationService>();
                if (configurationService != null)
                {
                    return configurationService.PanModeEnabled;
                }
                return true;
            }
        }

        private bool IsRubberBandSelectionEnabled
        {
            get
            {
                DesignerConfigurationService configurationService = this.Context.Services.GetService<DesignerConfigurationService>();
                if (configurationService != null)
                {
                    return configurationService.RubberBandSelectionEnabled;
                }
                return true;
            }
        }

        public ShellBarItemVisibility WorkflowShellBarItemVisibility
        {
            get { return this.shellBarItemVisibility; }
            set { this.ApplyShellBarItemVisibility(value); }
        }

        public ShellHeaderItemsVisibility WorkflowShellHeaderItemsVisibility
        {
            get { return this.shellHeaderItemsVisibility; }
            set { this.ApplyShellHeaderItemsVisibility(value); }
        }

        public void MakeRootDesigner(ModelItem modelItem)
        {
            bool checkIfCanBeMadeRoot = true;
            if (modelItem == modelItem.Root)
            {
                checkIfCanBeMadeRoot = false;
            }
            MakeRootDesigner(modelItem, /* setAsSelection = */ true, checkIfCanBeMadeRoot);
        }

        internal void MakeRootDesigner(ModelItem modelItem, bool setAsSelection)
        {
            MakeRootDesigner(modelItem, setAsSelection, true);
        }

        internal void ForceMakeRootDesigner(ModelItem modelItem)
        {
            MakeRootDesigner(modelItem, /* setAsSelection = */ true, false);
        }

        void SelectAll()
        {
            WorkflowViewElement root = this.RootDesigner as WorkflowViewElement;
            ModelItem rootModelItem = null;
            if (root != null)
            {
                rootModelItem = root.ModelItem;
            }
            if (rootModelItem != null)
            {
                ModelTreeManager modelTreeManager = this.Context.Services.GetService<ModelTreeManager>();
                IEnumerable<ModelItem> items = ModelTreeManager.Find(rootModelItem, delegate(ModelItem current)
                {
                    WorkflowViewService viewService = this.Context.Services.GetService<ViewService>() as WorkflowViewService;
                    return (typeof(WorkflowViewElement).IsAssignableFrom(viewService.GetDesignerType(current.ItemType)));
                }, true);
                IEnumerable<ModelItem> itemsToSelect = items
                    // ModelItemKeyValuePair is associated with CaseDesigner. 
                    // So ModelItemKeyValuePair will be returned even if they are not really Cases.
                    // Those ModelItemKeyValuePairs need to be excluded.
                    .Where<ModelItem>(item => !ModelUtilities.IsModelItemKeyValuePair(item.ItemType) || ModelUtilities.IsSwitchCase(item))
                    .Except<ModelItem>(new ModelItem[] { rootModelItem });
                Selection selection = new Selection(itemsToSelect);
                this.Context.Items.SetValue(selection);
            }
        }

        internal void BeginDragShadowTracking(DragDropHelper.ViewElementDragShadow dragShadow)
        {
            // Returns the first adorner layer in the visual tree above a specified Visual.
            AdornerLayer layer = this.GetAdornerLayerForDragShadow();
            if (null != layer)
            {
                layer.Add(dragShadow);
                this.viewElementDragShadow = dragShadow;
                //register for window messages notification
                this.Context.Services.GetService<WindowHelperService>().RegisterWindowMessageHandler(new WindowMessage(OnMessage));
            }
        }

        internal void EndDragShadowTracking(DragDropHelper.ViewElementDragShadow dragShadow)
        {
            AdornerLayer layer = this.GetAdornerLayerForDragShadow();
            if (null != layer)
            {
                //unregister from window message notification
                this.Context.Services.GetService<WindowHelperService>().UnregisterWindowMessageHandler(new WindowMessage(OnMessage));
                layer.Remove(dragShadow);
                this.viewElementDragShadow = null;
            }
        }

        static void UpdateAncestorFlag(ModelItem oldRoot, ModelItem newRoot)
        {
            // Walk up the tree and update the flags from the new root. If we hit the old root in the process, we are done.
            // Otherwise, continue to update the flags from the old root until we hit the new root.
            if (oldRoot == newRoot)
            {
                return;
            }
            bool hitOldRoot = false;
            if (newRoot != null)
            {
                WorkflowViewElement viewElement = newRoot.View as WorkflowViewElement;
                if (viewElement != null)
                {
                    viewElement.IsAncestorOfRootDesigner = false;
                }
                ModelItem parent = newRoot.Parent;
                while (parent != null)
                {
                    WorkflowViewElement view = parent.View as WorkflowViewElement;
                    if (view != null)
                    {
                        view.IsAncestorOfRootDesigner = true;
                    }
                    if (parent == oldRoot)
                    {
                        hitOldRoot = true;
                    }
                    parent = parent.Parent;
                }
            }
            if (oldRoot != null && !hitOldRoot)
            {
                ModelItem parent = oldRoot.Parent;
                while (parent != null && parent != newRoot)
                {
                    WorkflowViewElement view = parent.View as WorkflowViewElement;
                    if (view != null)
                    {
                        view.IsAncestorOfRootDesigner = false;
                    }
                    parent = parent.Parent;
                }
            }
        }

        internal void MakeRootDesigner(ModelItem modelItem, bool setAsSelection, bool checkIfCanBeMadeRoot)
        {
            ModelItem currentRootModelItem = (this.RootDesigner != null) ? ((WorkflowViewElement)this.RootDesigner).ModelItem : null;
            if (modelItem == currentRootModelItem)
            {
                return;
            }
            if (typeof(ActivityBuilder).IsAssignableFrom(modelItem.ItemType))
            {
                this.ActivitySchema = modelItem;
            }

            WorkflowViewService viewService = this.Context.Services.GetService<ViewService>() as WorkflowViewService;

            //try get designer for given model item
            Type designerType = viewService.GetDesignerType(modelItem.ItemType);
            //if one doesn't exist - check its parent tree, perhaps there will be one
            while (null == designerType && null != modelItem.Parent)
            {
                modelItem = modelItem.Parent;
                designerType = viewService.GetDesignerType(modelItem.ItemType);
            }

            if (viewService.ShouldAppearOnBreadCrumb(modelItem, checkIfCanBeMadeRoot))
            {
                UpdateAncestorFlag(currentRootModelItem, modelItem);
                Dictionary<ModelItem, ModelItem> newSelectionMap = new Dictionary<ModelItem, ModelItem>();
                ModelItem newRootModelItem = modelItem;
                ObservableCollection<object> breadCrumbCollection = new ObservableCollection<object>();
                object breadCrumbObjectConnector = null;
                bool isFirstAdded = false;
                while (modelItem != null)
                {
                    bool shouldCheckIfCanBeMadeRoot = true;
                    if (isFirstAdded)
                    {
                        shouldCheckIfCanBeMadeRoot = checkIfCanBeMadeRoot;
                    }
                    if (viewService.ShouldAppearOnBreadCrumb(modelItem, shouldCheckIfCanBeMadeRoot))
                    {
                        if (isFirstAdded)
                        {
                            breadCrumbObjectConnector = new BreadCrumbObjectSeparator();
                            breadCrumbCollection.Insert(0, breadCrumbObjectConnector);
                        }
                        breadCrumbCollection.Insert(0, modelItem);
                        isFirstAdded = true;
                        if (selectionMap.ContainsKey(modelItem))
                        {
                            newSelectionMap.Add(modelItem, selectionMap[modelItem]);
                        }
                    }
                    modelItem = modelItem.Parent;
                }

                //Remember the selection for the current root.
                WorkflowViewElement focusedElement = Keyboard.FocusedElement as WorkflowViewElement;
                //This condition will be true when we are breadcrumbing into a child element.
                if (focusedElement != null && object.Equals(focusedElement.ModelItem, newRootModelItem))
                {
                    if (currentRootModelItem != null)
                    {
                        newSelectionMap[currentRootModelItem] = newRootModelItem;
                    }
                }
                this.selectionMap = newSelectionMap;
                SetAsRootDesignerView(newRootModelItem, setAsSelection);
                breadCrumbListBox.ItemsSource = breadCrumbCollection;
                // Move to the top left so that the display name is visible.
                this.ScrollViewer.ScrollToTop();
                this.ScrollViewer.ScrollToLeftEnd();
            }
        }

        void OnMessage(int msgId, IntPtr wParam, IntPtr lParam)
        {
            //WM_NCHITTEST message is the only message beeing routed when dragging an activity over elements which do not support
            //drag & drop; in order to provide smooth dragging expirience i have to get coordinates from this message and update
            //drag shadow with them
            //consider this message only when we are in drag mode
            if (null != this.viewElementDragShadow && Win32Interop.WM_NCHITTEST == msgId)
            {
                AdornerLayer layer = this.viewElementDragShadow.Parent as AdornerLayer;
                Fx.Assert(layer != null, "viewElementDragShadow's parent should not be null");
                //get current mouse screen coordinates out of LPARAM
                uint pos = (uint)lParam;
                Point scrPoint = new Point((int)(pos & 0xffff), (int)(pos >> 16));
                // Transform a point from screen to AdornerLayer, which is the parent of shadow.                
                Point clientPoint = layer.PointFromScreen(scrPoint);
                this.viewElementDragShadow.UpdatePosition(clientPoint.X, clientPoint.Y);
            }
        }

        void OnWorkflowElementGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            FrameworkElement source = e.NewFocus as FrameworkElement;
            //walk up visual tree, but not above DesignerView - there won't be any design shapes anyway
            while (null != source && this != source)
            {
                //select first visual, which is of type WorkflowViewElement
                if (typeof(WorkflowViewElement).IsAssignableFrom(source.GetType()))
                {
                    break;
                }
                source = VisualTreeHelper.GetParent(source) as FrameworkElement;
            }
            //try to cast source element as WorkflowViewElement
            if (this.FocusedViewElement != source)
            {
                this.FocusedViewElement = source as WorkflowViewElement;

                System.Diagnostics.Debug.WriteLine(
                    string.Format(CultureInfo.InvariantCulture, "{0} ) DesignerView.OnWorkflowElementGotKeyboardFocus(FocusedViewElement {1}, raisedBy {2})",
                    DateTime.Now.ToLocalTime(), (null == this.FocusedViewElement ? "<null>" : this.FocusedViewElement.GetType().Name), e.OriginalSource));
            }
        }

        void OnDesignerKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            //if current designer lost keyboard focus - commit pending edits 
            if (!this.IsKeyboardFocusWithin)
            {
                //delegate the call using dispatcher, so all involved components do consume focus event, then i can commit the edit
                this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        //check if there is an edit in progress inside datagrid, which might have opened other dialog - 
                        //in such case, the desigerView would loose keyboard focus and could ---- non-modal dialog (ie. intelisense window for ETB)
                        if (!this.ShouldIgnoreDataGridAutoCommit)
                        {
                            if (null != this.variables1)
                            {
                                DataGridHelper.CommitPendingEdits(this.variables1.variableDataGrid);
                            }
                            if (null != this.arguments1)
                            {
                                DataGridHelper.CommitPendingEdits(this.arguments1.argumentsDataGrid);
                            }
                        }
                    }), DispatcherPriority.Input);
            }
            else
            {
                ErrorReporting.ActiveDesignerView = this;
            }
        }

        //Suppress handling arrow keys in ScrollViewer
        void OnScrollViewerKeyDown(object sender, KeyEventArgs e)
        {
            if (!e.Handled)
            {
                if ((e.Key == Key.Up) || (e.Key == Key.Down) || (e.Key == Key.Left) || (e.Key == Key.Right))
                {
                    e.Handled = true;
                }

                if (e.Key == Key.Escape)
                {
                    if (this.rubberBandSelector != null)
                    {
                        this.rubberBandSelector.OnScrollViewerEscapeKeyDown();
                    }
                }
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            //look up for unhandled Enter key events
            if (!e.Handled && Keyboard.Modifiers == ModifierKeys.None && e.OriginalSource is WorkflowViewElement)
            {
                switch (e.Key)
                {
                    case Key.Enter:
                        this.navigateToChildFunction((WorkflowViewElement)e.OriginalSource, true);
                        break;

                    case Key.Back:
                        this.navigateToParentFunction((WorkflowViewElement)e.OriginalSource, true);
                        break;
                }
            }
            base.OnKeyDown(e);
        }

        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                this.zoomSlider.Value += e.Delta / scrollDeltaDivider;
                e.Handled = true;
            }
            else
            {
                base.OnPreviewMouseWheel(e);
            }
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            this.ShouldStillAllowRubberBandEvenIfMouseLeftButtonDownIsHandled = false;
            base.OnPreviewMouseLeftButtonDown(e);
        }

        protected override void OnPreviewDragOver(DragEventArgs e)
        {
            AutoScrollHelper.AutoScroll(e, this.scrollViewer, 10);
            base.OnPreviewDragOver(e);
        }

        public void RegisterViewElement(WorkflowViewElement viewElement)
        {
            if (this.rubberBandSelector != null)
            {
                this.rubberBandSelector.RegisterViewElement(viewElement);
            }
        }

        public void UnregisterViewElement(WorkflowViewElement viewElement)
        {
            if (this.rubberBandSelector != null)
            {
                this.rubberBandSelector.UnregisterViewElement(viewElement);
            }
        }

        private void OnScrollViewerMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (this.rubberBandSelector != null)
            {
                this.rubberBandSelector.OnScrollViewerMouseLeftButtonDown(e);
            }
        }

        private void OnScrollViewerMouseMove(object sender, MouseEventArgs e)
        {
            if (this.rubberBandSelector != null)
            {
                this.rubberBandSelector.OnScrollViewerMouseMove(e);
            }
        }

        private void OnScrollViewerMouseLeave(object sender, MouseEventArgs e)
        {
            if (this.rubberBandSelector != null)
            {
                this.rubberBandSelector.OnScrollViewerMouseLeave();
            }
        }

        private void OnScrollViewerPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (this.rubberBandSelector != null)
            {
                this.rubberBandSelector.OnScrollViewerPreviewMouseLeftButtonUp(e);
            }
        }

        private void OnScrollViewerPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.ShouldStillAllowRubberBandEvenIfMouseLeftButtonDownIsHandled = true;
        }

        private void OnRootDesignerPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.ShouldStillAllowRubberBandEvenIfMouseLeftButtonDownIsHandled = false;
        }

        void OnDesignerSurfaceMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //user clicked on designer surface, somwhere around actual designer - try to select root designer
            if (e.OriginalSource == this.scrollViewer || e.OriginalSource == this.scrollableContent || e.OriginalSource == this.scrollViewer.Content)
            {
                //get root designer in given breadcrumb scope
                var root = this.RootDesigner as WorkflowViewElement;
                if (null != root)
                {
                    //if Ctrl is pressed, handle toggling
                    if (Keyboard.Modifiers == ModifierKeys.Control)
                    {
                        Selection.Toggle(this.Context, root.ModelItem);
                    }
                    //else, select the root
                    else
                    {
                        Selection.SelectOnly(this.Context, root.ModelItem);
                    }
                    //update focused view element - keyboard focus is set to scrollview, but designer infrastructure requires updated
                    //FocusViewElement to reference root.
                    this.FocusedViewElement = root;
                }
                e.Handled = true;
            }
        }

        static void OnActivitySchemaChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            DesignerView control = (DesignerView)dependencyObject;
            control.OnActivitySchemaChanged();
        }

        static void OnRootDesignerChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            DesignerView control = (DesignerView)dependencyObject;
            control.OnRootDesignerChanged(e);
        }

        static void OnIsReadOnlyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            DesignerView designerView = (DesignerView)dependencyObject;
            designerView.Context.Items.SetValue(new ReadOnlyState() { IsReadOnly = (bool)e.NewValue });
        }

        static void OnInPanModeChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            DesignerView designerView = (DesignerView)dependencyObject;
            if (designerView.scrollViewerPanner != null)
            {
                designerView.scrollViewerPanner.InPanMode = designerView.InPanMode;
            }
        }

        void HideBottomPane()
        {
            bottomPaneHeight = this.designerViewGrid.RowDefinitions[2].Height;
            this.designerViewGrid.RowDefinitions[2].Height = new GridLength(0);
            this.splitter.Visibility = Visibility.Collapsed;
            this.bottomPanel.Visibility = Visibility.Collapsed;
        }

        void OnActivitySchemaChanged()
        {
            if (null != this.ActivitySchema && typeof(ActivityBuilder).IsAssignableFrom(this.ActivitySchema.ItemType))
            {
                UpdateArgumentsButtonVisibility(true);
            }
            else
            {
                UpdateArgumentsButtonVisibility(false);
            }
        }

        private void OnBottomPanelClose(object sender, RoutedEventArgs e)
        {
            ToggleButton toggleButton = this.bottomPanel.Tag as ToggleButton;
            Fx.Assert(toggleButton != null, "toggleButton cannot be null");
            toggleButton.IsChecked = false;
        }

        void OnBreadCrumbClick(object sender, RoutedEventArgs e)
        {
            //this method can be invoked two ways - left mouse click on element or key press
            ListBoxItem listBoxItem = sender as ListBoxItem;
            //handle only events for items which are actual model items
            if (null != listBoxItem && listBoxItem.Content is ModelItem)
            {
                //determine which event are we handling
                KeyEventArgs keyArgs = e as KeyEventArgs;
                MouseButtonEventArgs mouseArgs = e as MouseButtonEventArgs;
                //in case of key events - accept only Enter, in case of mouse events - i know it is left mouse button
                if ((null != keyArgs && keyArgs.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.None) || null != mouseArgs)
                {
                    //make selection new root designer
                    this.MakeRootDesigner((ModelItem)listBoxItem.Content);
                    //mark event as handled
                    e.Handled = true;
                    // SQM: Breadcrumb
                    FeatureUsageCounter.ReportUsage(sqmService, WorkflowDesignerFeatureId.Breadcrumb);
                }
            }
        }

        void OnBreadCrumbNavigation(object sender, KeyEventArgs e)
        {
            //this method is invoked whenever user presses any key while breadcrumb has focus
            ItemsControl breadcrumbItems = sender as ItemsControl;
            //i expect that there is at least one item in the collection, arrow key is pressed and no keyboard modifiers are active
            if (null != breadcrumbItems && breadcrumbItems.Items.Count > 0 && Keyboard.Modifiers == ModifierKeys.None && (e.Key == Key.Left || e.Key == Key.Right))
            {
                //get first entry from collection
                UIElement first = (UIElement)breadcrumbItems.ItemContainerGenerator.ContainerFromIndex(0);
                //get last entry from collection
                UIElement last = (UIElement)breadcrumbItems.ItemContainerGenerator.ContainerFromIndex(breadcrumbItems.Items.Count - 1);
                //if last is selected, then set focus to the first, so Tab doesn't escape to other control
                if (e.Key == Key.Right && last.IsKeyboardFocusWithin)
                {
                    first.Focus();
                    e.Handled = true;
                }
                else if (e.Key == Key.Left && first.IsKeyboardFocusWithin)
                {
                    last.Focus();
                    e.Handled = true;
                }
            }
        }

        void OnExtensionWindowClosing(object sender, ExtensionWindowClosingRoutedEventArgs e)
        {
            e.Cancel = true;
            e.Handled = true;
            ((ExtensionWindow)sender).IsEnabled = false;
        }

        void OnRootDesignerChanged(DependencyPropertyChangedEventArgs e)
        {
            WorkflowViewElement previousRoot = (WorkflowViewElement)e.OldValue;
            WorkflowViewElement currentRoot = (WorkflowViewElement)e.NewValue;
            if (previousRoot != null)
            {
                previousRoot.IsRootDesigner = false;
            }
            if (currentRoot != null)
            {
                currentRoot.IsRootDesigner = true;
            }
        }

        [SuppressMessage(FxCop.Category.Usage, FxCop.Rule.ReviewUnusedParameters,
            Justification = "The parameters are defined by DependencyPropertyChangedEventHandler delegate")]
        void OnMinimapVisibilityChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ExtensionSurface.PlacementMode mode = ExtensionSurface.GetMode(this.miniMap);
            if (mode == ExtensionSurface.PlacementMode.Relative)
            {
                ExtensionSurface.SetMode(this.miniMap, ExtensionSurface.PlacementMode.Absolute);
            }
        }

        void OnBottomPanelIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == false)
            {
                DependencyObject focusedElement = Keyboard.FocusedElement as DependencyObject;
                // Move the keyboard focus on a proper designer when the bottom panel is closed
                if (focusedElement == null || focusedElement == sender || ((Visual)sender).IsAncestorOf(focusedElement))
                {
                    Keyboard.Focus(this.GetDesignerToFocus());
                }
            }
        }

        void OnToggleButtonCheckChanged(object sender, RoutedEventArgs e)
        {
            ToggleButton button = sender as ToggleButton;
            Fx.Assert(button != null, "Button cannot be null");
            ExtensionWindow window = button.Tag as ExtensionWindow;
            if (null != window)
            {
                window.Visibility = button.IsChecked.Value ? Visibility.Visible : Visibility.Hidden;
            }
            else
            {
                UIElement uiElement = button.Tag as UIElement;
                if (null != uiElement)
                {
                    if (uiElement.Visibility == Visibility.Collapsed)
                    {
                        //remove the previous userControl
                        if (this.bottomPanel.Tag != null)
                        {
                            ToggleButton toggleButton = this.bottomPanel.Tag as ToggleButton;
                            Fx.Assert(toggleButton != null, "toggleButton should not be null");
                            if (button != toggleButton)
                            {
                                toggleButton.IsChecked = false;
                            }
                        }
                        //add the new userControl
                        this.bottomPanel.Visibility = Visibility.Visible;
                        this.bottomPanel.Tag = button;
                        this.splitter.Visibility = Visibility.Visible;
                        uiElement.Visibility = Visibility.Visible;
                        this.designerViewGrid.RowDefinitions[2].Height = bottomPaneHeight;
                    }
                    else
                    {
                        //remove the current userControl
                        this.bottomPanel.Tag = null;
                        HideBottomPane();
                        uiElement.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }

        void OnZoomSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (this.ZoomFactor == 1)
            {
                TextOptions.SetTextFormattingMode(this.scrollableContent, TextFormattingMode.Display);
            }
            else
            {
                TextOptions.SetTextFormattingMode(this.scrollableContent, TextFormattingMode.Ideal);
            }
            this.scrollableContent.LayoutTransform = new ScaleTransform(this.ZoomFactor, this.ZoomFactor);
        }

        void SetAsRootDesignerView(ModelItem root, bool setAsSelection)
        {
            VirtualizedContainerService containerService = this.Context.Services.GetService<VirtualizedContainerService>();

            this.RootDesigner = null;
            //get the root view (route the call through virtualized container serivce, so Loaded and Unloaded events get hooked up)
            VirtualizedContainerService.VirtualizingContainer rootContainer = (VirtualizedContainerService.VirtualizingContainer)containerService.GetContainer(root, null);
            rootContainer.Populate();
            this.RootDesigner = (WorkflowViewElement)rootContainer.Child;

            if (setAsSelection)
            {
                ModelItem selection = root;
                if (selectionMap.ContainsKey(root))
                {
                    ModelItem prevSelection = selectionMap[root];
                    selection = null;
                    if (prevSelection != null && ViewUtilities.IsViewVisible(prevSelection, root, context))
                    {
                        selection = prevSelection;
                    }
                }
                if (selection != null)
                {
                    selection.Focus();
                }
                else
                {
                    this.Context.Items.SetValue(new Selection());
                }
            }
        }

        private void SplitterDragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            bottomPaneHeight = this.designerViewGrid.RowDefinitions[2].Height;
        }

        void CreateXPSDocument(string fileName)
        {
            using (FileStream fs = new FileStream(fileName, FileMode.Create))
            {
                Package package = Package.Open(fs, FileMode.Create);
                XpsDocument document = new XpsDocument(package);
                XpsDocumentWriter documentWriter = XpsDocument.CreateXpsDocumentWriter(document);

                int imageWidth = (int)this.designerPresenter.DesiredSize.Width;
                int imageHeight = (int)this.designerPresenter.DesiredSize.Height;

                PrintTicket ticket = new PrintTicket() { PageMediaSize = new PageMediaSize(imageWidth, imageHeight) };
                if (IsRightToLeft(this.designerPresenter))
                {
                    Transform originalTransform = this.designerPresenter.RenderTransform;
                    try
                    {
                        this.designerPresenter.RenderTransform = new ScaleTransform(-1, 1, imageWidth / 2, 0);
                        documentWriter.Write(this.designerPresenter, ticket);
                    }
                    finally
                    {
                        this.designerPresenter.RenderTransform = originalTransform;
                    }
                }
                else
                {
                    documentWriter.Write(this.designerPresenter, ticket);
                }

                document.Close();
                package.Close();
                fs.Flush();
            }
        }

        void CreateImageFile(string fileName, Type encoderType)
        {
            using (FileStream fs = new FileStream(fileName, FileMode.Create))
            {
                BitmapEncoder encoder = (BitmapEncoder)Activator.CreateInstance(encoderType);
                encoder.Frames.Add(BitmapFrame.Create(this.CreateScreenShot()));
                encoder.Save(fs);
                fs.Close();
            }
        }

        // CreateScreenShot should handle the situation when the FlowDirection of Designer's 
        // parent is RightToLeft
        // 
        // The structure:
        // Root
        //   |--DesignerView
        // The DesignerView is what we're trying to capture.
        // 
        // If Root.FlowDirection is RightToLeft, the DesignerView's capture is a flipped image.
        // Say, if DesignerView is diplayed on screen:
        // -->==>
        // the captured image would be:
        // <==<--
        // It is Root who flips the image before diplaying on screen.
        // But, in our capture, Root will not do the flipping work for us, so we flip the image
        // before return.
        BitmapSource CreateScreenShot()
        {
            const double DPI = 96.0;

            Rect bounds = VisualTreeHelper.GetDescendantBounds(this.designerPresenter);
            int imageWidth = (int)Math.Ceiling(bounds.Right);
            int imageHeight = (int)Math.Ceiling(bounds.Bottom);
            Rectangle background = new Rectangle()
            {
                // WindowBrush:
                //  Gets a SolidColorBrush that is the background 
                //  color in the client area of a window.  
                Fill = new SolidColorBrush(WorkflowDesignerColors.DesignerViewBackgroundColor),
                Width = imageWidth,
                Height = imageHeight,
            };

            background.Arrange(new Rect(0, 0, imageWidth, imageHeight));

            RenderTargetBitmap renderBitmap = new RenderTargetBitmap(imageWidth, imageHeight, DPI, DPI, PixelFormats.Pbgra32);
            renderBitmap.Render(background);
            renderBitmap.Render(this.designerPresenter);

            BitmapSource source = BitmapFrame.Create(renderBitmap);

            if (IsRightToLeft(this.designerPresenter))
            {
                return new TransformedBitmap(source, new ScaleTransform(-1, 1, imageWidth / 2, 0));
            }

            return source;
        }

        public void OnReferenceUpdated(AssemblyName updatedReference, bool isAdded)
        {
            //Queue the work item instead of execute it directly in reference updated handler. Otherwise when in VS, we cannot get the assembly through multi-targeting service.
            Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() => this.imports1.OnReferenceUpdated(updatedReference, isAdded)));
        }

        internal void CheckButtonVariables()
        {
            this.buttonVariables1.IsChecked = true;
        }

        internal void CheckButtonArguments()
        {
            this.buttonArguments1.IsChecked = true;
        }

        void OnVariablesCollectionChanged(object sender, RoutedEventArgs e)
        {
            // Suppress showing the variable designer for 4.0 compatibility. See TFS 631027 for details.
            // CheckButtonVariables();
        }

        void OnArgumentsCollectionChanged(object sender, RoutedEventArgs e)
        {
            CheckButtonArguments();
        }

        void ApplyShellBarItemVisibility(ShellBarItemVisibility visibility)
        {
            // Store user preferences
            this.shellBarItemVisibility = visibility;

            // Variable, Arguments, Imports
            UpdateStatusBarItemVisibility(this.variablesStatusBarItem, CheckItemVisibility(visibility, ShellBarItemVisibility.Variables), this.variables1);
            UpdateArgumentsButtonVisibility(CheckItemVisibility(visibility, ShellBarItemVisibility.Arguments));
            UpdateStatusBarItemVisibility(this.importsStatusBarItem, CheckItemVisibility(visibility, ShellBarItemVisibility.Imports), this.imports1);

            // PanMode
            this.panModeStatusBarItem.Visibility = CheckItemVisibility(visibility, ShellBarItemVisibility.PanMode) ? Visibility.Visible : Visibility.Collapsed;

            // Zoom
            Visibility zoomVisibility = CheckItemVisibility(visibility, ShellBarItemVisibility.Zoom) ? Visibility.Visible : Visibility.Collapsed;
            this.zoomFitToScreenStatusBar.Visibility = zoomVisibility;
            this.zoomIconStatusBar.Visibility = zoomVisibility;
            this.zoomPickerStatusBar.Visibility = zoomVisibility;
            this.zoomSliderStatusBar.Visibility = zoomVisibility;

            // MiniMap
            this.minimapStatusBar.Visibility = CheckItemVisibility(visibility, ShellBarItemVisibility.MiniMap) ? Visibility.Visible : Visibility.Collapsed;

            // Hide entire status bar if nothing is visible 
            this.shellBar.Visibility = (ShellBarItemVisibility.None == visibility) ? Visibility.Collapsed : Visibility.Visible;
        }

        void ApplyShellHeaderItemsVisibility(ShellHeaderItemsVisibility visibility)
        {
            // If all the items on shell header are invisible, the shell header
            // will be hiden automatically. 
            // 
            // Expand All/ Collapse All / Breadcrumb
            this.breadCrumbListBox.Visibility = CheckItemVisibility(visibility, ShellHeaderItemsVisibility.Breadcrumb) ? Visibility.Visible : Visibility.Collapsed;
            this.expandAllButton.Visibility = CheckItemVisibility(visibility, ShellHeaderItemsVisibility.ExpandAll) ? Visibility.Visible : Visibility.Collapsed;
            this.collapseAllButton.Visibility = CheckItemVisibility(visibility, ShellHeaderItemsVisibility.CollapseAll) ? Visibility.Visible : Visibility.Collapsed;
        }

        private static bool CheckItemVisibility(ShellHeaderItemsVisibility visibility, ShellHeaderItemsVisibility itemToCheck)
        {
            return (itemToCheck & visibility) == itemToCheck;
        }

        private static bool CheckItemVisibility(ShellBarItemVisibility visibility, ShellBarItemVisibility itemToCheck)
        {
            return (itemToCheck & visibility) == itemToCheck;
        }

        private static bool IsRightToLeft(FrameworkElement element)
        {
            Fx.Assert(element != null, "element should not be null");
            return element.FlowDirection == FlowDirection.RightToLeft;
        }

        private AdornerLayer GetAdornerLayerForDragShadow()
        {
            return AdornerLayer.GetAdornerLayer(this.scrollableContent);
        }

        private void UpdateArgumentsButtonVisibility(bool visible)
        {
            UpdateStatusBarItemVisibility(this.argumentsStatusBarItem, visible, this.arguments1);
        }

        private void UpdateStatusBarItemVisibility(StatusBarItem item, bool visible, UIElement element)
        {
            if (item == null || element == null)
            {
                return;
            }

            item.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;

            // Hide the correponding UIElement (VariableDesigner, ArgumentDesigner, etc.) if the button shouldn't be visible
            if (item.Visibility != Visibility.Visible)
            {
                element.IsEnabled = false;
            }
            else
            {
                element.IsEnabled = true;
            }
        }

        public void FlushState()
        {
            this.SaveDesignerStates();
        }


        void SaveDesignerStates()
        {
            this.SaveBreadCrumbRoot();
            this.SaveSelection();
            this.SaveZoomFactor();
        }

        internal void RestoreDesignerStates()
        {
            this.RestoreBreadCrumbRoot();
            this.RestoreSelection();
            this.RestoreZoomFactor();
        }

        void SaveSelection()
        {
            IWorkflowDesignerStorageService service = this.Context.Services.GetService<IWorkflowDesignerStorageService>();
            ModelTreeManager modelTreeManager = this.Context.Services.GetService<ModelTreeManager>();
            if (service != null && modelTreeManager != null)
            {
                Selection selection = this.Context.Items.GetValue<Selection>();
                var selectionPathList = new List<string>();
                foreach (ModelItem item in selection.SelectedObjects)
                {
                    if (item.Root == modelTreeManager.Root)
                    {
                        selectionPathList.Add(item.GetModelPath());
                    }
                }
                if (service.ContainsKey(selectionKey))
                {
                    service.SetData(selectionKey, selectionPathList);
                }
                else
                {
                    service.AddData(selectionKey, selectionPathList);
                }
            }
        }

        void RestoreSelection()
        {
            IWorkflowDesignerStorageService service = this.Context.Services.GetService<IWorkflowDesignerStorageService>();
            ModelTreeManager modelTreeManager = this.Context.Services.GetService<ModelTreeManager>();
            if (service != null && service.ContainsKey(selectionKey) && modelTreeManager != null && modelTreeManager.Root != null)
            {
                var selectionPathList = service.GetData(selectionKey) as List<string>;
                if (selectionPathList != null)
                {
                    var modelItemList = new List<ModelItem>();
                    foreach (string path in selectionPathList)
                    {
                        ModelItem item = ModelItemExtensions.GetModelItemFromPath(path, modelTreeManager.Root);
                        if (item != null)
                        {
                            modelItemList.Add(item);
                        }
                    }
                    Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() =>
                    {
                        this.Context.Items.SetValue(new Selection(modelItemList));
                    }));
                }
            }
        }

        void SaveBreadCrumbRoot()
        {
            IWorkflowDesignerStorageService service = this.Context.Services.GetService<IWorkflowDesignerStorageService>();
            DesignerView designerView = this.Context.Services.GetService<DesignerView>();
            if (service != null && designerView != null && designerView.RootDesigner != null)
            {
                WorkflowViewElement rootDesigner = designerView.RootDesigner as WorkflowViewElement;
                if (rootDesigner != null)
                {
                    if (service.ContainsKey(breadCrumbRootKey))
                    {
                        service.SetData(breadCrumbRootKey, rootDesigner.ModelItem.GetModelPath());
                    }
                    else
                    {
                        service.AddData(breadCrumbRootKey, rootDesigner.ModelItem.GetModelPath());
                    }
                }
            }
        }

        void RestoreBreadCrumbRoot()
        {
            IWorkflowDesignerStorageService service = this.Context.Services.GetService<IWorkflowDesignerStorageService>();
            ModelTreeManager modelTreeManager = this.Context.Services.GetService<ModelTreeManager>();
            DesignerView designerView = this.context.Services.GetService<DesignerView>();
            if (service != null && service.ContainsKey(breadCrumbRootKey) && modelTreeManager != null && modelTreeManager.Root != null && designerView != null)
            {
                string path = service.GetData(breadCrumbRootKey) as string;
                if (path != null)
                {
                    ModelItem item = ModelItemExtensions.GetModelItemFromPath(path, modelTreeManager.Root);
                    if (item != null)
                    {
                        designerView.MakeRootDesigner(item);
                    }
                }
            }
        }

        void SaveZoomFactor()
        {
            IWorkflowDesignerStorageService service = this.Context.Services.GetService<IWorkflowDesignerStorageService>();
            if (service != null)
            {
                if (service.ContainsKey(zoomFactorKey))
                {
                    service.SetData(zoomFactorKey, this.zoomSlider.Value);
                }
                else
                {
                    service.AddData(zoomFactorKey, this.zoomSlider.Value);
                }
            }
        }

        void RestoreZoomFactor()
        {
            IWorkflowDesignerStorageService service = this.Context.Services.GetService<IWorkflowDesignerStorageService>();
            if (service != null && service.ContainsKey(zoomFactorKey))
            {
                object data = service.GetData(zoomFactorKey);
                if (data is double)
                {
                    this.zoomSlider.Value = (double)data;
                }
            }
        }

        //this class is used to convert zoom slider ticks to actual zoom percantage
        //the speced range of supported zoom values is between 25 % - 400% (with 25, 50, 100, 200 and 400 predefined steps)
        //since increments are non linear, i use y = a(x*x) + c equation, to calculate zoom factor - zoom will be more glanular
        //for small values, and more coarse for larger ones
        private sealed class ZoomToTicksConverter : IValueConverter
        {
            const double minValue = 25;
            const double maxValue = 400;

            //predefined a value - calculated on assumption that maximum zoom value is 400% 
            const double a = 0.15;
            //predefined c value - calculated on assumption that minimum zoom value is 25%
            const double c = 25;
            IValueConverter baseConverter;

            DesignerView view;
            string zoomFitToScreenLabel;
            double[] keyboardZoomTicks;

            internal ZoomToTicksConverter(DesignerView designer, Slider zoomSlider, ComboBox zoomPicker)
            {
                this.view = designer;
                this.zoomFitToScreenLabel = (this.view.TryFindResource("zoomFitToScreenLabel") as string) ?? "Fit to screen";
                //this.baseConverter = new ZoomPercentageConverter();
                //right now, we want to use our custom ZoomToPercantageConverter due to localization issues with WPF one
                this.baseConverter = new CustomZoomPercentageConverter();

                //initialize zoom slider
                zoomSlider.Minimum = 0;
                zoomSlider.Maximum = 50;
                zoomSlider.Ticks = new DoubleCollection(new double[] { 0, 10, 20, 30, 40, 50 });

                //set initial value - initially, zoom is set to 100%
                zoomSlider.Value = (double)this.ConvertBack(
                    this.baseConverter.Convert(100.0, typeof(string), null, CultureInfo.InvariantCulture), typeof(double), null, CultureInfo.InvariantCulture);

                //insert predefined values to zoomPicker - i use converter to percantage, to ensure text will be formated accordingly to user settings
                zoomPicker.ItemsSource = new object[]
                    {
                        this.baseConverter.Convert(25.0, typeof(string), null, CultureInfo.InvariantCulture),
                        this.baseConverter.Convert(50.0, typeof(string), null, CultureInfo.InvariantCulture),
                        this.baseConverter.Convert(100.0, typeof(string), null, CultureInfo.InvariantCulture),
                        this.baseConverter.Convert(200.0, typeof(string), null, CultureInfo.InvariantCulture),
                        this.baseConverter.Convert(400.0, typeof(string), null, CultureInfo.InvariantCulture)
                    };

                //setup bindings
                zoomPicker.SetBinding(ComboBox.SelectedItemProperty, new Binding()
                {
                    Source = zoomSlider,
                    Path = new PropertyPath(Slider.ValueProperty),
                    Converter = this
                });

                zoomPicker.SetBinding(ComboBox.TextProperty, new Binding()
                {
                    Source = zoomSlider,
                    Path = new PropertyPath(Slider.ValueProperty),
                    Converter = this
                });

                this.keyboardZoomTicks = new double[]
                {
                    (double)this.ConvertBack(
                        this.baseConverter.Convert(25.0, typeof(string), null, CultureInfo.InvariantCulture), typeof(double), null, CultureInfo.InvariantCulture),
                    (double)this.ConvertBack(
                        this.baseConverter.Convert(37.5, typeof(string), null, CultureInfo.InvariantCulture), typeof(double), null, CultureInfo.InvariantCulture),
                    (double)this.ConvertBack(
                        this.baseConverter.Convert(50.0, typeof(string), null, CultureInfo.InvariantCulture), typeof(double), null, CultureInfo.InvariantCulture),
                    (double)this.ConvertBack(
                        this.baseConverter.Convert(75.0, typeof(string), null, CultureInfo.InvariantCulture), typeof(double), null, CultureInfo.InvariantCulture),
                    (double)this.ConvertBack(
                        this.baseConverter.Convert(100.0, typeof(string), null, CultureInfo.InvariantCulture), typeof(double), null, CultureInfo.InvariantCulture),
                    (double)this.ConvertBack(
                        this.baseConverter.Convert(150.0, typeof(string), null, CultureInfo.InvariantCulture), typeof(double), null, CultureInfo.InvariantCulture),
                    (double)this.ConvertBack(
                        this.baseConverter.Convert(200.0, typeof(string), null, CultureInfo.InvariantCulture), typeof(double), null, CultureInfo.InvariantCulture),
                    (double)this.ConvertBack(
                        this.baseConverter.Convert(300.0, typeof(string), null, CultureInfo.InvariantCulture), typeof(double), null, CultureInfo.InvariantCulture),
                    (double)this.ConvertBack(
                        this.baseConverter.Convert(400.0, typeof(string), null, CultureInfo.InvariantCulture), typeof(double), null, CultureInfo.InvariantCulture),

                };

                this.view.zoomPicker.LostFocus += (s, e) =>
                {
                    string text = this.Convert(this.view.zoomSlider.Value, typeof(string), null, CultureInfo.InvariantCulture) as string;
                    if (null != text)
                    {
                        this.view.zoomPicker.Text = string.Empty;
                        this.view.zoomPicker.Text = text;
                    }
                };

            }

            double CalculateY(double x)
            {
                return ((x * x) * a) + c;
            }

            double CalculateX(double y)
            {
                return Math.Sqrt((y - c) / a);
            }

            internal double ZoomFactor
            {
                get
                {
                    return this.CalculateY(this.view.zoomSlider.Value) / 100.0;
                }
            }

            public bool CanZoomIn()
            {
                return this.view.zoomSlider.Value < this.view.zoomSlider.Maximum;
            }

            public void ZoomIn()
            {
                double x = this.view.zoomSlider.Value;
                for (int i = 0; i < this.keyboardZoomTicks.Length; ++i)
                {
                    if (x < this.keyboardZoomTicks[i])
                    {
                        this.view.zoomSlider.Value = this.keyboardZoomTicks[i];
                        break;
                    }
                }
            }

            public void ZoomOut()
            {
                double x = this.view.zoomSlider.Value;
                for (int i = this.keyboardZoomTicks.Length - 1; i >= 0; --i)
                {
                    if (x > this.keyboardZoomTicks[i])
                    {
                        this.view.zoomSlider.Value = this.keyboardZoomTicks[i];
                        break;
                    }
                }
            }

            public bool CanZoomOut()
            {
                return this.view.zoomSlider.Value > this.view.zoomSlider.Minimum;
            }

            public void FitToScreen()
            {
                double y1 = (this.view.scrollViewer.ViewportWidth / this.view.scrollableContent.ActualWidth) * 100.0;
                double y2 = (this.view.scrollViewer.ViewportHeight / this.view.scrollableContent.ActualHeight) * 100.0;
                double y = Math.Min(maxValue, Math.Max(minValue, Math.Min(y1, y2)));
                this.view.zoomSlider.Value = this.CalculateX(y);
            }

            public void ResetZoom()
            {
                this.view.zoomSlider.Value = this.CalculateX(100.0);
            }

            [SuppressMessage(FxCop.Category.Design, FxCop.Rule.DoNotCatchGeneralExceptionTypes,
                Justification = "Catching all exceptions to avoid VS Crash")]
            [SuppressMessage("Reliability", "Reliability108", Justification = "Catching all exceptions to avoid VS Crash")]
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (null != value && value is double)
                {
                    try
                    {
                        return this.baseConverter.Convert(this.CalculateY((double)value), targetType, parameter, culture);
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Debug.WriteLine(e.ToString());
                    }
                }
                return Binding.DoNothing;
            }

            [SuppressMessage(FxCop.Category.Design, FxCop.Rule.DoNotCatchGeneralExceptionTypes,
                Justification = "Catching all exceptions to avoid VS Crash")]
            [SuppressMessage("Reliability", "Reliability108", Justification = "Catching all exceptions to avoid VS Crash")]

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (null != value)
                {
                    try
                    {
                        double y = 0.0;
                        if (string.Equals(this.zoomFitToScreenLabel, value))
                        {
                            double y1 = (this.view.scrollViewer.ViewportWidth / this.view.scrollableContent.ActualWidth) * 100.0;
                            double y2 = (this.view.scrollViewer.ViewportHeight / this.view.scrollableContent.ActualHeight) * 100.0;
                            y = Math.Min(maxValue, Math.Max(minValue, Math.Min(y1, y2)));
                        }
                        else
                        {
                            y = (double)this.baseConverter.ConvertBack(value, targetType, parameter, culture);
                        }
                        return this.CalculateX(y);
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Debug.WriteLine(e.ToString());
                    }
                }
                return Binding.DoNothing;
            }
        }

        internal static bool IsMouseInViewport(MouseButtonEventArgs e, ScrollViewer scrollViewer)
        {
            Point mousePosition = e.GetPosition(scrollViewer);
            return mousePosition.X > 0 && mousePosition.X < scrollViewer.ViewportWidth &&
                mousePosition.Y > 0 && mousePosition.Y < scrollViewer.ViewportHeight;
        }

        /// <summary>
        /// CustomZoomPercentageConverter - used temporary instead of WPF provided ZoomToPercantageConverter due to the problems
        /// in localized builds
        /// </summary>
        private sealed class CustomZoomPercentageConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                object result = DependencyProperty.UnsetValue;
                if (null != value)
                {
                    double valueAsDouble = System.Convert.ToDouble(value, CultureInfo.CurrentCulture);
                    if (valueAsDouble == Math.Floor(valueAsDouble))
                    {
                        // Ignore decimal part if it is an Int value.
                        result = string.Format(CultureInfo.CurrentCulture, "{0}%", valueAsDouble.ToString("F0", CultureInfo.CurrentCulture));
                    }
                    else
                    {
                        result = string.Format(CultureInfo.CurrentCulture, "{0}%", valueAsDouble.ToString("F2", CultureInfo.CurrentCulture));
                    }
                }
                return result;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                object result = DependencyProperty.UnsetValue;
                if (null != value)
                {
                    string valueAsString = value.ToString().Replace("%", "").Trim();
                    result = System.Convert.ToDouble(valueAsString, CultureInfo.CurrentCulture);
                }
                return result;
            }
        }

        //BreadCrumbObjectSeparator - right now, this class has no functionality - object of this class is used as 
        //a separator between different breadcrumb elements. however, i can imagine scenario when additional functionality
        //is added here (i.e. similar to breadcrumb in Vista explorer)
        sealed class BreadCrumbObjectSeparator
        {
            //ItemType property - to avoid binding errors and make this object similar to ModelItem
            public Type ItemType
            {
                get { return typeof(BreadCrumbObjectSeparator); }
            }
        }

        private sealed class DesignerViewProxy
        {
            private WeakReference reference;

            public DesignerViewProxy(DesignerView designerView)
            {
                this.reference = new WeakReference(designerView);
            }

            public void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
            {
                DesignerView designerView = this.reference.Target as DesignerView;
                if (designerView != null)
                {
                    designerView.OnDispatcherUnhandledException(sender, e);
                }
            }
        }
    }


    internal sealed class ContextMenuIconProvider : IMultiValueConverter
    {
        //glyph image cache
        IDictionary<KeyValuePair<string, bool>, DrawingBrush> glyphCache = new Dictionary<KeyValuePair<string, bool>, DrawingBrush>();

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            //get the menu item i'm reffering to
            var menuItem = values[0] as MenuItem;
            //get the icon name as defined in /Resources dictionary
            var iconName = parameter as string;
            if (null != menuItem && !string.IsNullOrEmpty(iconName))
            {
                DrawingBrush glyph = null;
                //check if image has been used alreay - if yes - get it from cache.
                if (!glyphCache.TryGetValue(new KeyValuePair<string, bool>(iconName, menuItem.IsEnabled), out glyph))
                {
                    string key = string.Format(CultureInfo.InvariantCulture, "Operation{0}{1}Icon", iconName, menuItem.IsEnabled ? string.Empty : "Disabled");
                    glyph = WorkflowDesignerIcons.IconResourceDictionary[key] as DrawingBrush;
                    //add it to the cache
                    glyphCache[new KeyValuePair<string, bool>(iconName, menuItem.IsEnabled)] = glyph;
                }
                //return glyph
                return new Rectangle() { Width = 16, Height = 16, Fill = glyph };
            }
            return Binding.DoNothing;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw FxTrace.Exception.AsError(new NotSupportedException());
        }
    }

    [SuppressMessage(FxCop.Category.Naming, FxCop.Rule.FlagsEnumsShouldHavePluralNames)]
    [SuppressMessage(FxCop.Category.Usage, "CA2217", Justification = "This is enum value, we don't have enough enum values to fill 32 discrete values")]
    [Flags]
    public enum ShellBarItemVisibility
    {
        None = 0x0,
        Variables = 0x1,
        Arguments = 0x2,
        Imports = 0x4,
        Zoom = 0x8,
        MiniMap = 0x10,
        PanMode = 0x20,
        All = -1
    }

    [SuppressMessage(FxCop.Category.Naming, FxCop.Rule.FlagsEnumsShouldHavePluralNames)]
    [SuppressMessage(FxCop.Category.Usage, "CA2217", Justification = "This is enum value, we don't have enough enum values to fill 32 discrete values")]
    [Flags]
    public enum ShellHeaderItemsVisibility
    {
        None = 0x0,
        Breadcrumb = 0x1,
        ExpandAll = 0x2,
        CollapseAll = 0x4,
        All = -1
    }


    internal sealed class ExpandAllCollapseAllToggleConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            //values[0] is the corresponding property - For ExpandAllButton - ShouldExpandAllProperty
            //values[1] is the opposite property - For ExpandAllButton - ShouldCollapseAllProperty
            return values[0];
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            //Whenever ExpandAll/CollapseAll toggle button state is changed, the opposite property is always reset.
            return new object[] { value, false };
        }
    }
}
