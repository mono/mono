//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

#pragma warning disable 618

namespace System.Activities.Presentation
{
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.Services;
    using System.Activities.Presentation.View;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Runtime;
    using System.Windows;
    using System.Windows.Automation.Peers;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Controls.Primitives;
    using System.Windows.Data;
    using System.Timers;
    using System.Windows.Threading;
    using System.Text;
    using System.Linq;
    using System.Activities.Presentation.Internal.PropertyEditing;

    // This is the base class of all things visual that are associated with ModelItems.
    // e.g state designer, workflowelement designer, activity designe etc.
    // This provides access to the ModelItem attached to it,  and the EditingContext.
    public class WorkflowViewElement : ContentControl, ICompositeViewEvents
    {
        public static readonly DependencyProperty ModelItemProperty =
            DependencyProperty.Register("ModelItem", typeof(ModelItem), typeof(WorkflowViewElement), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(WorkflowViewElement.OnModelItemChanged)));
        public static readonly DependencyProperty ContextProperty =
            DependencyProperty.Register("Context", typeof(EditingContext), typeof(WorkflowViewElement), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(WorkflowViewElement.OnContextChanged)));
        public static readonly DependencyProperty ExpandStateProperty = 
            DependencyProperty.Register("ExpandState", typeof(bool), typeof(WorkflowViewElement), new FrameworkPropertyMetadata(true, new PropertyChangedCallback(WorkflowViewElement.OnExpandStateChanged)));
        public static readonly DependencyProperty PinStateProperty =
            DependencyProperty.Register("PinState", typeof(bool), typeof(WorkflowViewElement), new FrameworkPropertyMetadata(false, new PropertyChangedCallback(WorkflowViewElement.OnPinStateChanged)));
        public static readonly DependencyProperty ShowExpandedProperty =
            DependencyProperty.Register("ShowExpanded", typeof(bool), typeof(WorkflowViewElement), new FrameworkPropertyMetadata(false, new PropertyChangedCallback(WorkflowViewElement.OnShowExpandedChanged)));
        internal readonly static DependencyProperty IsRootDesignerProperty =
            DependencyProperty.Register("IsRootDesigner", typeof(bool), typeof(WorkflowViewElement), new FrameworkPropertyMetadata(false));
        static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(WorkflowViewElement), new FrameworkPropertyMetadata(false, new PropertyChangedCallback(WorkflowViewElement.OnReadOnlyChanged)));

        const string ExpandViewStateKey = "IsExpanded";
        internal const string PinnedViewStateKey = "IsPinned";
        Timer breadCrumbTimer;
        int lastMouseButtonDownTimeStamp;

        internal string CustomItemStatus { get; set; }

        static void OnExpandStateChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            WorkflowViewElement viewElement = obj as WorkflowViewElement;
            viewElement.OnExpandStateChanged((bool)e.NewValue);
        }

        static void OnPinStateChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            WorkflowViewElement viewElement = obj as WorkflowViewElement;
            viewElement.OnPinStateChanged((bool)e.NewValue);
        }


        static void OnContextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            WorkflowViewElement viewElement = obj as WorkflowViewElement;
            viewElement.OnContextChanged();
        }

        static void OnShowExpandedChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            WorkflowViewElement viewElement = obj as WorkflowViewElement;
            viewElement.OnShowExpandedChanged((bool)e.NewValue);
        }

        static void OnReadOnlyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            WorkflowViewElement viewElement = obj as WorkflowViewElement;
            viewElement.OnReadOnlyChanged((bool)e.NewValue);
        }

        protected virtual void OnShowExpandedChanged(bool newValue)
        {
        }

        protected virtual void OnReadOnlyChanged(bool isReadOnly)
        {
        }

        internal void ForceCollapse()
        {
            this.ExpandState = false;
            if (this.Designer.ShouldExpandAll)
            {
                this.PinState = true;
            }
        }

        void OnContextChanged()
        {
            //Setting the binding here so that we have a handle to DesignerView.
            SetShowExpandedBindings();
        }

        void OnExpandStateChanged(bool newValue)
        {
            if (this.ModelItem != null && this.Context != null)
            {
                this.ViewStateService.StoreViewState(this.ModelItem, ExpandViewStateKey, newValue);
            }
        }

        void OnPinStateChanged(bool newValue)
        {
            if (this.ModelItem != null && this.Context != null)
            {
                this.ViewStateService.StoreViewState(this.ModelItem, WorkflowViewElement.PinnedViewStateKey, newValue);
            }
        }

        bool leftMouseButtonDown = false;
        Point lastMouseDownPoint;
        UIElement lastActivationElement;
        List<ICompositeView> compositeViews;
        ICompositeView defaultCompositeView;
        bool rightMouseClickWithCtrlDown = false;
        bool rightMouseClick = false;
        bool shouldChangeSelectionOnMouseUp = false;

        public WorkflowViewElement()
        {
            this.Collapsible = true;
            this.IsAncestorOfRootDesigner = false;
            this.DragHandle = this;

            this.Loaded += (sender, eventArgs) =>
                {
                    // When the designer is loaded in Cider, the Context is not available and thus we cannot access the DesignerView (nor is it necessary)
                    if (this.Context != null)
                    {
                        this.Designer.RegisterViewElement(this);
                    }

                    this.GotFocus += new RoutedEventHandler(OnGotFocusEvent);
                    this.breadCrumbTimer = new Timer(2000);
                    this.breadCrumbTimer.Elapsed += new ElapsedEventHandler(OnBreadCrumbTimerElapsed);
                    this.breadCrumbTimer.AutoReset = false;
                    this.lastActivationElement = null;
                    this.SetValue(CutCopyPasteHelper.ChildContainersProperty, null);
                    if (this.ModelItem != null)
                    {
                        ((IModelTreeItem)this.ModelItem).SetCurrentView(this);
                    }

                    //Get the ExpandState from ViewState.
                    if (this.ModelItem != null)
                    {
                        object expandCollapseViewState = this.ViewStateService.RetrieveViewState(this.ModelItem, ExpandViewStateKey);
                        object pinViewState = this.ViewStateService.RetrieveViewState(this.ModelItem, WorkflowViewElement.PinnedViewStateKey);
                        if (expandCollapseViewState != null)
                        {
                            this.ExpandState = (bool)expandCollapseViewState;
                        }
                        if (pinViewState != null)
                        {
                            this.PinState = (bool)pinViewState;
                        }
                    }
                    this.UseLayoutRounding = true;
                };

            this.Unloaded += (sender, eventArgs) =>
                {
                    // When the designer is loaded in Cider, the Context is not available and thus we cannot access the DesignerView (nor is it necessary)
                    if (this.Context != null)
                    {
                        this.Designer.UnregisterViewElement(this);
                    }

                    this.GotFocus -= new RoutedEventHandler(OnGotFocusEvent);
                    Fx.Assert(this.breadCrumbTimer != null, "The timer should not be null.");
                    this.breadCrumbTimer.Elapsed -= new ElapsedEventHandler(OnBreadCrumbTimerElapsed);
                    this.breadCrumbTimer.Close();
                };
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            Binding readOnlyBinding = new Binding();
            readOnlyBinding.RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(DesignerView), 1);
            readOnlyBinding.Path = new PropertyPath(DesignerView.IsReadOnlyProperty);
            readOnlyBinding.Mode = BindingMode.OneWay;
            this.SetBinding(IsReadOnlyProperty, readOnlyBinding);
        }

        void SetShowExpandedBindings()
        {
            MultiBinding multiBinding = new MultiBinding();
            //Bind to ModelItem
            Binding modelItemBinding = new Binding();
            modelItemBinding.Source = this;
            modelItemBinding.Path = new PropertyPath(WorkflowViewElement.ModelItemProperty);
            //Bind to IsRootDesigner
            Binding isRootDesignerBinding = new Binding();
            isRootDesignerBinding.Source = this;
            isRootDesignerBinding.Path = new PropertyPath(WorkflowViewElement.IsRootDesignerProperty);
            //Bind to DesignerView.ExpandAll
            Binding expandAllBinding = new Binding();
            DesignerView view = this.Context.Services.GetService<DesignerView>();
            expandAllBinding.Source = view;
            expandAllBinding.Path = new PropertyPath(DesignerView.ShouldExpandAllProperty);
            //Bind to DesignerView.CollapseAll
            Binding collapseAllBinding = new Binding();
            collapseAllBinding.Source = view;
            collapseAllBinding.Path = new PropertyPath(DesignerView.ShouldCollapseAllProperty);
            //Bind to ExpandState
            Binding expandStateBinding = new Binding();
            expandStateBinding.Source = this;
            expandStateBinding.Path = new PropertyPath(WorkflowViewElement.ExpandStateProperty);
            //Bind to PinState
            Binding pinStateBinding = new Binding();
            pinStateBinding.Source = this;
            pinStateBinding.Path = new PropertyPath(WorkflowViewElement.PinStateProperty);
            //Bind to self
            Binding selfBinding = new Binding();
            selfBinding.Source = this;
            //Bind to container (to recalculate on drag-drop.)
            Binding containerBinding = new Binding();
            containerBinding.Source = this;
            containerBinding.Path = new PropertyPath(DragDropHelper.DragSourceProperty);
            multiBinding.Bindings.Add(modelItemBinding);
            multiBinding.Bindings.Add(isRootDesignerBinding);
            multiBinding.Bindings.Add(expandAllBinding);
            multiBinding.Bindings.Add(collapseAllBinding);
            multiBinding.Bindings.Add(expandStateBinding);
            multiBinding.Bindings.Add(pinStateBinding);
            multiBinding.Bindings.Add(selfBinding);
            multiBinding.Bindings.Add(containerBinding);

            multiBinding.Mode = BindingMode.OneWay;
            multiBinding.Converter = new ShowExpandedMultiValueConverter();
            BindingOperations.SetBinding(this, WorkflowViewElement.ShowExpandedProperty, multiBinding );
        }

        [Fx.Tag.KnownXamlExternal]
        public EditingContext Context
        {
            get { return (EditingContext)GetValue(ContextProperty); }
            set { SetValue(ContextProperty, value); }
        }

        public bool ExpandState
        {
            get { return (bool)GetValue(ExpandStateProperty); }
            set { SetValue(ExpandStateProperty, value); }
        }

        public bool PinState
        {
            get { return (bool)GetValue(PinStateProperty); }
            set { SetValue(PinStateProperty, value); }
        }

        //This guides us whether to show the expand collapse button for this ViewElement or not.
        public bool Collapsible
        {
            get;
            set;
        }

        public bool IsRootDesigner
        {
            get { return (bool)GetValue(IsRootDesignerProperty); }
            internal set { SetValue(IsRootDesignerProperty, value); }
        }

        internal bool IsAncestorOfRootDesigner
        {
            get;
            set;
        }
        
        public bool ShowExpanded
        {
            get { return (bool)GetValue(ShowExpandedProperty); }
        }

        internal bool DoesParentAlwaysExpandChild()
        {
            return ViewUtilities.DoesParentAlwaysExpandChildren(this.ModelItem, this.Context);
        }

        internal bool DoesParentAlwaysCollapseChildren()
        {
            return ViewUtilities.DoesParentAlwaysCollapseChildren(this.ModelItem, this.Context);
        }


        [Fx.Tag.KnownXamlExternal]
        public ModelItem ModelItem
        {
            get { return (ModelItem)GetValue(ModelItemProperty); }
            set { SetValue(ModelItemProperty, value); }
        }

        public FrameworkElement DragHandle
        {
            get;
            set;
        }

        protected bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            private set { SetValue(IsReadOnlyProperty, value); }
        }

        internal ICompositeView ActiveCompositeView
        {
            get
            {
                if (!this.ShowExpanded)
                {
                    return null;
                }
                ICompositeView activeContainer = null;
                if (null != this.compositeViews && null != this.lastActivationElement)
                {
                    activeContainer = this.compositeViews.Find(p =>
                        {
                            Visual visual = p as Visual;
                            return (null != visual &&
                                visual == this.lastActivationElement.FindCommonVisualAncestor(visual));
                        });
                }
                activeContainer = activeContainer ?? this.DefaultCompositeView;

                System.Diagnostics.Debug.WriteLine(string.Format(
                    CultureInfo.InvariantCulture,
                    "Active ICompositeView in '{0}' is '{1}'",
                    this.GetType().Name, activeContainer == null ? "<null>" : activeContainer.GetHashCode().ToString(CultureInfo.InvariantCulture)));

                return activeContainer;
            }
        }

        // useful shortcuts for things we know we will be using a lot.
        // Shortcut to viewservice in editingcontext.services
        protected internal ViewService ViewService
        {
            get
            {
                Fx.Assert(this.Context != null, "Context should not be null ");
                ViewService viewService = this.Context.Services.GetService<ViewService>();
                Fx.Assert(viewService != null, "View service should never be null if we are in a valid view tree");
                return viewService;
            }
        }

        // Shortcut to ViewStateService in editingcontext.services
        protected internal ViewStateService ViewStateService
        {
            get
            {
                ViewStateService viewStateService = this.Context.Services.GetService<ViewStateService>();
                Fx.Assert(viewStateService != null, "ViewState service should never be null if we are in a valid view tree");
                return viewStateService;
            }
        }

        protected internal DesignerView Designer
        {
            get
            {
                DesignerView designer = this.Context.Services.GetService<DesignerView>();
                Fx.Assert(designer != null, "DesignerView service should never be null if we are in a valid state");
                return designer;
            }
        }

        protected IList<ICompositeView> CompositeViews
        {
            get { return this.compositeViews; }
        }

        protected ICompositeView DefaultCompositeView
        {
            get { return this.defaultCompositeView; }
        }

        internal bool DraggingMultipleItemsEnabled
        {
            get { return this.Context.Services.GetService<DesignerConfigurationService>().MultipleItemsDragDropEnabled; }
        }

        protected virtual string GetAutomationIdMemberName()
        {
            return null;
        }

        protected virtual string GetAutomationHelpText()
        {
            return string.Empty;
        }

        protected internal virtual string GetAutomationItemStatus()
        {
            if (this.CustomItemStatus == null)
            {
                return string.Empty;
            }
            else
            {
                if (!this.CustomItemStatus.EndsWith(" ", StringComparison.Ordinal))
                {
                    this.CustomItemStatus += " ";
                }
                return this.CustomItemStatus;
            }
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new WorkflowViewElementAutomationPeer(this);
        }

        protected internal virtual void OnEditAnnotation()
        {
            return;
        }

        public void RegisterDefaultCompositeView(ICompositeView container)
        {
            if (null == container)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("container"));
            }

            this.defaultCompositeView = container;

            System.Diagnostics.Debug.WriteLine(string.Format(
                CultureInfo.InvariantCulture,
                "Default ICompositeView of type '{0}' for '{1}' loaded. hashcode = {2}",
                this.defaultCompositeView.GetType().Name, this.GetType().Name, this.defaultCompositeView.GetHashCode()));
        }

        public void UnregisterDefaultCompositeView(ICompositeView container)
        {
            if (null == container)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("container"));
            }

            if (object.Equals(this.defaultCompositeView, container))
            {
                this.defaultCompositeView = null;
            }
        }

        public void RegisterCompositeView(ICompositeView container)
        {
            if (null == container)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("container"));
            }

            if (null == this.CompositeViews)
            {
                this.compositeViews = new List<ICompositeView>();
            }
            if (!this.compositeViews.Contains(container))
            {
                System.Diagnostics.Debug.WriteLine(string.Format(
                    CultureInfo.InvariantCulture,
                    "ICompositeView of type '{0}' for '{1}' loaded. hashcode = {2}",
                    container.GetType().Name, this.GetType().Name, container.GetHashCode()));

                this.compositeViews.Add(container);
            }
        }

        public void UnregisterCompositeView(ICompositeView container)
        {
            if (null == container)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("container"));
            }

            if (null != this.compositeViews && this.compositeViews.Contains(container))
            {
                System.Diagnostics.Debug.WriteLine(string.Format(
                    CultureInfo.InvariantCulture,
                    "ICompositeView of type '{0}' for '{1}' unloaded",
                    container.GetType().Name, this.GetType().Name));

                this.compositeViews.Remove(container);
                if (0 == this.compositeViews.Count)
                {
                    this.compositeViews = null;
                }
            }
        }

        void OnGotFocusEvent(object sender, RoutedEventArgs e)
        {
            DesignerView designerView = this.Context.Services.GetService<DesignerView>();
            if (!e.Handled && this.ModelItem != null && !designerView.IsMultipleSelectionMode)
            {
                Selection selection = this.Context.Items.GetValue<Selection>();
                //update selection when following conditions apply:
                //1. We're not trying to open context menu using right click + ctrl key.
                //2. We're not clicking with left mouse - selection will be updated on left mouse button up
                //3. Current selection does not contain this.ModelItem if it's mouse right click
                bool becomesSelection = true;
                if (this.rightMouseClickWithCtrlDown || this.leftMouseButtonDown)
                {
                    becomesSelection = false;
                }
                else if (this.rightMouseClick)
                {
                    foreach (ModelItem item in selection.SelectedObjects)
                    {
                        if (item == this.ModelItem)
                        {
                            becomesSelection = false;
                            break;
                        }
                    }
                }

                //When there is only one selected model item, we want to change the selection when we tab into other items.
                if (becomesSelection)
                {
                    Selection.SelectOnly(this.Context, this.ModelItem);
                }

                System.Diagnostics.Debug.WriteLine(
                    string.Format(CultureInfo.InvariantCulture, "{0} ) WorkflowViewElement.OnGotFocusEvent ({1}, selection: {2}, becomesSelection {3}, raisedBy {4})",
                    DateTime.Now.ToLocalTime(), this.GetType().Name, selection.SelectionCount, becomesSelection, e.OriginalSource));

                //do not override last activation element if we get a reference to this (this will be passed as original source
                //whenever focus is set manualy - by direct call to Keyboard.SetFocus)
                if (!object.Equals(this, e.OriginalSource))
                {
                    this.lastActivationElement = e.OriginalSource as UIElement;
                }
                e.Handled = true;
            }
            else
            {
                this.lastActivationElement = null;
            }
        }

        protected virtual void OnModelItemChanged(object newItem)
        {
        }

        protected virtual void OnContextMenuLoaded(ContextMenu menu)
        {
        }

        void MakeRootDesigner()
        {
            DesignerView designerView = this.Context.Services.GetService<DesignerView>();
            if (!this.Equals(designerView.RootDesigner))
            {
                designerView.MakeRootDesigner(this.ModelItem);
                this.leftMouseButtonDown = false;
            }
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            this.lastMouseButtonDownTimeStamp = e.Timestamp;
            bool shouldSetFocus = false;
            bool shouldUpdateLastActivationPoint = false;
            this.leftMouseButtonDown = false;
            this.rightMouseClickWithCtrlDown = false;
            this.rightMouseClick = false;
            bool shouldToggle = false;

            if (this.ModelItem == null)
            {
                return;
            }

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.leftMouseButtonDown = true;
                this.lastMouseDownPoint = e.GetPosition(this);
                if (!Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl))
                {
                    this.CaptureMouse();
                }
                else
                {
                    shouldToggle = true;
                }

                shouldSetFocus = Keyboard.FocusedElement != this;
                e.Handled = true;
                this.Designer.ShouldStillAllowRubberBandEvenIfMouseLeftButtonDownIsHandled = true;
                shouldUpdateLastActivationPoint = true;
            }

            if (e.LeftButton == MouseButtonState.Pressed && e.ClickCount == 2 && this.Designer.lastClickedDesigner == this)
            {
                this.MakeRootDesigner();
                Mouse.Capture(null);
                e.Handled = true;
                this.Designer.ShouldStillAllowRubberBandEvenIfMouseLeftButtonDownIsHandled = false;
            }

            if (e.RightButton == MouseButtonState.Pressed)
            {
                rightMouseClick = true;
                this.lastMouseDownPoint = e.GetPosition(this);
                if (!Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl))
                {
                    shouldSetFocus = Keyboard.FocusedElement != this;
                }
                else
                {
                    rightMouseClickWithCtrlDown = true;
                }

                e.Handled = true;
                shouldUpdateLastActivationPoint = true;
            }

            System.Diagnostics.Debug.WriteLine(
                string.Format(CultureInfo.InvariantCulture, "{0} ) WorkflowViewElement.OnMouseDown ({1}, shouldSetFocus {2}, mouseCaptured {3}, raisedBy {4})",
                DateTime.Now.ToLocalTime(), this.GetType().Name, shouldSetFocus, this.IsMouseCaptured, e.OriginalSource));

            base.OnMouseDown(e);

            if (shouldSetFocus)
            {
                System.Diagnostics.Debug.WriteLine(
                    string.Format(CultureInfo.InvariantCulture, "{0} ) WorkflowViewElement.OnMouseDown.SetFocus ({1})",
                    DateTime.Now.ToLocalTime(), this.GetType().Name));
                //attempt to set focused and keyboard focused element to new designer
                Keyboard.Focus(this);
            }

            bool isSelected = this.Context.Items.GetValue<Selection>().SelectedObjects.Contains<ModelItem>(this.ModelItem);

            if (shouldToggle)
            {
                if (!isSelected)
                {
                    Selection.Toggle(this.Context, this.ModelItem);
                }
                else
                {
                    this.shouldChangeSelectionOnMouseUp = true;
                }
            }
            else
            {
                if (!rightMouseClickWithCtrlDown)
                {
                    if (rightMouseClick)
                    {
                        // if it's right mouse click without ctrl, change selection only if the current item is not selected.
                        bool alreadySelected = false;
                        Selection selection = this.Context.Items.GetValue<Selection>();
                        foreach (ModelItem item in selection.SelectedObjects)
                        {
                            if (item == this.ModelItem)
                            {
                                alreadySelected = true;
                                break;
                            }
                        }
                        if (!alreadySelected)
                        {
                            Selection.SelectOnly(this.Context, this.ModelItem);
                        }
                    }
                    else if (this.leftMouseButtonDown)
                    {
                        if (!isSelected)
                        {
                            Selection.SelectOnly(this.Context, this.ModelItem);
                        }
                        else
                        {
                            this.shouldChangeSelectionOnMouseUp = true;
                        }
                    }
                }
            }

            this.rightMouseClickWithCtrlDown = false;
            this.rightMouseClick = false;

            if (shouldUpdateLastActivationPoint)
            {
                this.lastActivationElement = e.OriginalSource as UIElement;
            }

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.Designer.lastClickedDesigner = this;
            }
        }

        bool AllowDragging()
        {
            Selection selection = this.Context.Items.GetValue<Selection>();
            
            if (!this.DraggingMultipleItemsEnabled)
            {
                if (selection.SelectionCount != 1)
                {
                    return false;
                }
            }

            return selection.SelectedObjects.All<ModelItem>((p) =>
                {
                    return p != null && p != p.Root && p.View != null && ((WorkflowViewElement)p.View).IsVisible &&
                        (DragDropHelper.GetCompositeView((WorkflowViewElement)p.View) as ICompositeView) != null;
                });
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            // if model item is removed, uncapture mouse and return
            if (this.ModelItem != null && this.ModelItem != this.ModelItem.Root && this.ModelItem.Parent == null)
            {
                if (this.IsMouseCaptured)
                {
                    Mouse.Capture(null);
                }
                return;
            }

            if (e.LeftButton == MouseButtonState.Pressed &&
               this.leftMouseButtonDown &&
               this.ModelItem != null &&
               this.IsMouseOnDragHandle(this.lastMouseDownPoint) &&
               e.Timestamp - this.lastMouseButtonDownTimeStamp > 100)
            {
                //get new position
                Point newPosition = e.GetPosition(this);
                //calculate distance
                Vector difference = newPosition - this.lastMouseDownPoint;
                if (Math.Abs(difference.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(difference.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    Selection selection = this.Context.Items.GetValue<Selection>();
                    // If the current model item is not selected, add it to the selection. 
                    if (!selection.SelectedObjects.Contains(this.ModelItem))
                    {
                        if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                        {
                            Selection.Toggle(this.Context, this.ModelItem);
                        }
                        else
                        {
                            Selection.SelectOnly(this.Context, this.ModelItem);
                        }
                    }

                    if (this.AllowDragging())
                    {
                        //if mouse is caputured - release capture, drag&drop infrastructure will take care now for tracking mouse move
                        if (this.IsMouseCaptured)
                        {
                            Mouse.Capture(null);
                            StartDragging();
                        }

                        this.leftMouseButtonDown = false;
                        e.Handled = true;
                    }
                }
            }
            
            base.OnMouseMove(e);
        }

        private bool IsMouseOnDragHandle(Point mousePoint)
        {
            if (this.DragHandle != null)
            {
                GeneralTransform transform = this.DragHandle.TransformToAncestor(this);
                Point dragHandleLocation = transform.Transform(new Point(0, 0));
                Rect dragHandleRect = new Rect(dragHandleLocation, new Size(this.DragHandle.ActualWidth, this.DragHandle.ActualHeight));
                if (dragHandleRect.Contains(mousePoint))
                {
                    return true;
                }
            }
            return false;
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(
                CultureInfo.InvariantCulture,
                string.Format(CultureInfo.CurrentUICulture, "{0} ) WorkflowViewElement.OnMouseUp ({1}, mouseCaptured {2})",
                DateTime.Now.ToLocalTime(), this.GetType().Name, this.IsMouseCaptured));

            if (this.leftMouseButtonDown)
            {
                if (this.IsMouseCaptured)
                {
                    Mouse.Capture(null);
                }

                if (!this.Designer.SuppressSelectionOnMouseUp && this.shouldChangeSelectionOnMouseUp)
                {
                    if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                    {
                        Selection.Toggle(this.Context, this.ModelItem);
                    }
                    else
                    {
                        Selection.SelectOnly(this.Context, this.ModelItem);
                    }
                }

                this.leftMouseButtonDown = false;
                e.Handled = true;
            }

            this.shouldChangeSelectionOnMouseUp = false;
            base.OnMouseUp(e);
        }

        void OnBreadCrumbTimerElapsed(object sender, ElapsedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                {
                    this.breadCrumbTimer.Stop();
                    this.MakeRootDesigner();
                }));
        }

        void OnDrag(DragEventArgs e)
        {
            if (!e.Handled)
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
            }
        }

        // This is to set the cursor to the forbidden icon when dragging to the designer.
        // It doesn't affect the drag-drop behavior of components that have AllowDrop == ture within the designer.
        protected override void OnDragEnter(DragEventArgs e)
        {
            this.OnDrag(e);
            base.OnDragEnter(e);
        }

        // This is to set the cursor to the forbidden icon when dragging within the designer.
        // It doesn't affect the drag-drop behavior of components that have AllowDrop == ture within the designer.
        protected override void OnDragOver(DragEventArgs e)
        {
            this.OnDrag(e);
            base.OnDragOver(e);
        }

        protected override void OnPreviewDragEnter(DragEventArgs e)
        {
            if (this.ShowExpanded == false)
            {
                this.breadCrumbTimer.Start();
            }
            base.OnPreviewDragEnter(e);
        }

        protected override void OnPreviewDragLeave(DragEventArgs e)
        {
            this.breadCrumbTimer.Stop();
            base.OnPreviewDragLeave(e);
        }

        protected override void OnPreviewMouseUp(MouseButtonEventArgs e)
        {
            this.breadCrumbTimer.Stop();
            base.OnPreviewMouseUp(e);
        }

        static void OnModelItemChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            WorkflowViewElement viewElement = (WorkflowViewElement)dependencyObject;
            viewElement.OnModelItemChanged(e.NewValue);
        }

        void BeginDropAnimation(WorkflowViewElement target)
        {
            DropAnimation opacityAnimation = new DropAnimation();
            target.BeginAnimation(FrameworkElement.OpacityProperty, opacityAnimation);
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.DoNotCatchGeneralExceptionTypes,
            Justification = "Catching all exceptions to avoid VS Crash.")]
        [SuppressMessage("Reliability", "Reliability108",
            Justification = "Catching all exceptions to avoid VS crash.")]

        void StartDragging()
        {
            try
            {
                using (ModelEditingScope editingScope = this.ModelItem.BeginEdit(SR.MoveEditingScopeDescription, true))
                {
                    HashSet<WorkflowViewElement> draggedViews = new HashSet<WorkflowViewElement>();
                    Dictionary<ModelItem, ICompositeView> sourceContainers = new Dictionary<ModelItem, ICompositeView>();
                    HashSet<ICompositeView> compViewSet = new HashSet<ICompositeView>();
                    Selection selection = this.Context.Items.GetValue<Selection>();
                    IEnumerable<ModelItem> selectedObjects = selection.SelectedObjects;
                    IEnumerable<ModelItem> modelItemsToDrag = DragDropHelper.GetModelItemsToDrag(selectedObjects);
                    
                    // Save the source containers for the dragged items
                    foreach (ModelItem modelItem in modelItemsToDrag)
                    {
                        WorkflowViewElement view = (WorkflowViewElement)modelItem.View;
                        draggedViews.Add(view);
                        ICompositeView container = DragDropHelper.GetCompositeView(view) as ICompositeView;
                        sourceContainers.Add(modelItem, container);
                        // If Add returns true => the container is added the first time, which is always ok
                        // If Add returns false => the container is added more than once
                        //    it must be a IMultipleDragEnabledCompositeView, otherwise, return, because 
                        //    we don't support dragging from ICompositeView.
                        if (!compViewSet.Add(container) && !(container is IMultipleDragEnabledCompositeView))
                        {
                            return;
                        }
                    }
                    
                    // Calculate the anchor point for the dragged items
                    Point relativeLocation = GetRelativeLocation(draggedViews);
                    Point referencePoint = this.lastMouseDownPoint;
                    referencePoint.Offset(relativeLocation.X, relativeLocation.Y);


                    DataObject dataObject = DragDropHelper.DoDragMoveImpl(draggedViews, referencePoint);
                    IEnumerable<WorkflowViewElement> movedViewElements = DragDropHelper.GetDragDropMovedViewElements(dataObject);

                    // once drag drop is done make sure the CompositeView is notified of the change in data
                    if (movedViewElements != null)
                    {
                        Dictionary<ICompositeView, List<ModelItem>> containerMovedModelItemList = new Dictionary<ICompositeView, List<ModelItem>>();
                        
                        // Create containerMovedModelItemList
                        foreach (WorkflowViewElement view in movedViewElements)
                        {
                            ICompositeView compView = DragDropHelper.GetCompositeView(view) as ICompositeView;
                            Fx.Assert(compView != null, "not an ICompositeView");
                            if (!containerMovedModelItemList.ContainsKey(compView))
                            {
                                containerMovedModelItemList.Add(compView, new List<ModelItem>());
                            }
                            containerMovedModelItemList[compView].Add(view.ModelItem);
                        }

                        // Call OnItemsMoved to notify the source container.
                        foreach (KeyValuePair<ICompositeView, List<ModelItem>> pair in containerMovedModelItemList)
                        {
                            if (pair.Key is IMultipleDragEnabledCompositeView)
                            {
                                ((IMultipleDragEnabledCompositeView)pair.Key).OnItemsMoved(pair.Value);
                            }
                            else
                            {
                                if (pair.Value.Count >= 2)
                                {
                                    throw FxTrace.Exception.AsError(
                                        new InvalidOperationException(SR.Error_MovingMoreThanOneItemsFromICompositeView));
                                }
                                pair.Key.OnItemMoved(pair.Value[0]);
                            }
                        }

                        // animation
                        foreach (WorkflowViewElement view in movedViewElements)
                        {
                            BeginDropAnimation(view);
                        }
                    }
                    // the drop target is using old DragDropHelper API and did not set the moved view elements
                    else
                    {
                        DragDropEffects executedDragDropEffect = DragDropHelper.GetDragDropCompletedEffects(dataObject);
                        if (executedDragDropEffect == DragDropEffects.Move)
                        {
                            if (modelItemsToDrag.Count() == 1)
                            {
                                ModelItem movedItem = modelItemsToDrag.First<ModelItem>();
                                sourceContainers[movedItem].OnItemMoved(movedItem);
                                BeginDropAnimation((WorkflowViewElement)movedItem.View);
                            }
                            else
                            {
                                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.DraggingMulitpleItemsError));
                            }
                        }
                    }
                    editingScope.Complete();

                    bool dropHappened = movedViewElements != null
                        || DragDropHelper.GetDragDropCompletedEffects(dataObject) == DragDropEffects.Move;
                    if (dropHappened)
                    {
                        // add the selected objects back into selection.
                        this.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, (Action)(() =>
                        {

                            foreach (ModelItem item in selectedObjects)
                            {
                                // We need only the first one
                                IInputElement viewToFocus = item == null ? null : item.View as IInputElement;
                                if (viewToFocus != null)
                                {
                                    Keyboard.Focus(viewToFocus);
                                    break;
                                }
                            }
                            this.Context.Items.SetValue(new Selection(selectedObjects));
                        }));
                    }
                }
            }
            catch (Exception e)
            {
                ErrorReporting.ShowErrorMessage(e.Message);
            }
        }

        private Point GetRelativeLocation(IEnumerable<WorkflowViewElement> draggedViews)
        {
            HashSet<WorkflowViewElement> viewElements = new HashSet<WorkflowViewElement>(draggedViews);
            if (!viewElements.Contains(this))
            {
                viewElements.Add(this);
            }
            Dictionary<WorkflowViewElement, Point> locations = DragDropHelper.GetViewElementRelativeLocations(viewElements);
            return locations[this];
        }

        internal void NotifyContextMenuLoaded(ContextMenu menu)
        {
            if (null != menu)
            {
                OnContextMenuLoaded(menu);
            }
        }

        class WorkflowViewElementAutomationPeer : UIElementAutomationPeer
        {
            WorkflowViewElement owner;

            public WorkflowViewElementAutomationPeer(WorkflowViewElement owner)
                : base(owner)
            {
                this.owner = owner;
            }

            protected override AutomationControlType GetAutomationControlTypeCore()
            {
                return AutomationControlType.Custom;
            }


            protected override string GetAutomationIdCore()
            {
                string automationId = this.GetClassNameCore();
                string automationIdVariablePartMemberName = owner.GetAutomationIdMemberName();
                if (!string.IsNullOrEmpty(automationIdVariablePartMemberName))
                {
                    ModelItem modelItem = this.owner.ModelItem;
                    string variablePartOfAutomationId = string.Empty;
                    if (modelItem != null)
                    {
                        ModelProperty property = modelItem.Properties[automationIdVariablePartMemberName];
                        Fx.Assert(property != null, "property to use for Automation ID variable part missing ? are you using the right property Name?");
                        if (property.Value != null)
                        {
                            variablePartOfAutomationId = property.Value.GetCurrentValue().ToString();
                        }
                    }
                    automationId = variablePartOfAutomationId + "(" + this.GetClassNameCore() + ")";
                }
                return automationId;
            }

            protected override string GetNameCore()
            {
                Type itemType = null;
                if (this.owner.ModelItem != null)
                {
                    itemType = this.owner.ModelItem.ItemType;
                }
                else
                {
                    itemType = this.owner.GetType();
                }

                if (itemType.IsGenericType)
                {
                    //append the argument types for generic types
                    //we expect the single level of generic is sufficient for the screen reader, so we're no going into
                    //nesting of generic types
                    Type[] argumentTypes = itemType.GetGenericArguments();
                    StringBuilder name = new StringBuilder(itemType.Name);
                    name.Append('[');
                    foreach (Type argument in argumentTypes)
                    {
                        name.Append(argument.Name);
                        name.Append(',');
                    }
                    name.Replace(',', ']', name.Length - 1, 1);
                    return name.ToString();
                }
                else
                {
                    return itemType.Name;
                }
            }

            protected override string GetHelpTextCore()
            {
                return this.owner.GetAutomationHelpText();
            }

            protected override string GetItemStatusCore()
            {
                return this.owner.GetAutomationItemStatus();
            }

            protected override string GetClassNameCore()
            {
                return this.owner.GetType().Name;
            }
        }
    }

}
