//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation
{
    using System.Activities.Presentation.Hosting;
    using System.Activities.Presentation.Internal.PropertyEditing;
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.Services;
    using System.Activities.Presentation.View;
    using System.Activities.Statements;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime;
    using System.Windows;
    using System.Windows.Automation.Peers;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Shapes;
    using System.Windows.Threading;
    using System.Timers;

    enum InsertionPosition
    {
        Before,
        After,
        None,
    }

    class AutoWrapEventArgs : EventArgs
    {
        public Activity ExistingActivity { get; set; }
        public InsertionPosition InsertionPosition { get; set; }
        public List<Activity> ActivitiesToBeInserted { get; set; }
    }

    // This class provides a visual edit box to edit ModelItems. Textbox offers to edit strings, ints as a TextBlock and a cursor visually,
    // The workflowitempresenter edits modelitems by picking their view using the view service. It presents s the visual for the modelitem
    // pointe by Item property if it is set, it shows the hint text if the property is not set. It allows the associated item to be deleted 
    // visually , and removes the reference to Item when deleted. It also allows droping ModelItems, to set the Item property to the dropped 
    // item.
    public sealed class WorkflowItemPresenter : ContentControl, ICompositeView
    {
        public static readonly DependencyProperty HintTextProperty =
            DependencyProperty.Register("HintText", typeof(string), typeof(WorkflowItemPresenter), new UIPropertyMetadata(String.Empty));

        public static readonly DependencyProperty ItemProperty =
            DependencyProperty.Register("Item", typeof(ModelItem), typeof(WorkflowItemPresenter), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(WorkflowItemPresenter.OnItemChanged)));

        public static readonly DependencyProperty AllowedItemTypeProperty =
            DependencyProperty.Register("AllowedItemType", typeof(Type), typeof(WorkflowItemPresenter), new UIPropertyMetadata(typeof(object)));

        public static readonly DependencyProperty IsDefaultContainerProperty =
            DependencyProperty.Register("IsDefaultContainer", typeof(bool), typeof(WorkflowItemPresenter), new UIPropertyMetadata(false));

        public static readonly DependencyProperty DroppingTypeResolvingOptionsProperty =
            DependencyProperty.Register("DroppingTypeResolvingOptions", typeof(TypeResolvingOptions), typeof(WorkflowItemPresenter));

        SpacerHelper spacerHelper;
        Grid contentGrid;
        StackPanel stackPanel;
        Grid containerGrid;
        TextBlock text;
        EditingContext context = null;
        bool shouldSetFocus = false;
        bool isItemPastedOrDropped = false;

        public WorkflowItemPresenter()
        {
            this.text = new TextBlock();
            this.text.SetBinding(TextBlock.TextProperty, "HintText");
            this.text.DataContext = this;
            this.text.HorizontalAlignment = HorizontalAlignment.Center;
            this.text.VerticalAlignment = VerticalAlignment.Center;
            this.text.Foreground = new SolidColorBrush(SystemColors.GrayTextColor);
            this.text.FontStyle = FontStyles.Italic;

            this.contentGrid = new Grid();
            this.contentGrid.Background = Brushes.Transparent;
            this.contentGrid.VerticalAlignment = VerticalAlignment.Center;
            this.contentGrid.Children.Add(text);

            this.stackPanel = new StackPanel();
            this.stackPanel.HorizontalAlignment = HorizontalAlignment.Center;
            this.stackPanel.VerticalAlignment = VerticalAlignment.Center;
            this.stackPanel.Children.Add(contentGrid);

            this.containerGrid = new Grid();
            this.containerGrid.Children.Add(stackPanel);
            this.containerGrid.Background = Brushes.Transparent;
        }
        
        internal bool AutoWrapInSequenceEnabled
        {
            get
            {
                // Don't allow auto wrap in sequence if allowed item isn't of type of Activity
                return this.Context != null
                    && this.Context.Services.GetService<DesignerConfigurationService>().AutoSurroundWithSequenceEnabled
                    && typeof(Activity).IsAssignableFrom(this.AllowedItemType); 
            }
        }

        Activity MyActivity
        {
            get
            {
                return this.Item == null ? null :
                    this.Item.GetCurrentValue() as Activity;
            }
        }

        List<Activity> ObjectList2ActivityList(IEnumerable<object> droppedObjects)
        {
            List<Activity> activityList = new List<Activity>();
            foreach (object droppedObject in droppedObjects)
            {
                object modelObject = droppedObject;
                if (modelObject is ModelItem)
                {
                    modelObject = ((ModelItem)droppedObject).GetCurrentValue();
                }
                if (modelObject is Activity)
                {
                    activityList.Add(modelObject as Activity);
                }
                else
                {
                    Fx.Assert("A non-activity is found in the list, there must be something seriously wrong!");
                }
            }
            return activityList;
        }

        private List<WorkflowViewElement> ObjectList2WorkflowViewElementList(IEnumerable<object> droppedObjects)
        {
            List<WorkflowViewElement> movedViewElements = new List<WorkflowViewElement>();
            foreach (object droppedObject in droppedObjects)
            {
                if (droppedObject is ModelItem && ((ModelItem)droppedObject).View != null)
                {
                    WorkflowViewElement view = (WorkflowViewElement)((ModelItem)droppedObject).View;
                    WorkflowItemPresenter container = DragDropHelper.GetCompositeView(view) as WorkflowItemPresenter;
                    if (container != this)
                    {
                        movedViewElements.Add(view);
                    }
                }
            }
            return movedViewElements;
        }

        // return true if really something is dropped, otherwise, false.
        bool DoAutoWrapDrop(InsertionPosition insertionPos, IEnumerable<object> droppedObjects)
        {
            List<Activity> activityList = ObjectList2ActivityList(droppedObjects);
            if (activityList.Count == 0)
            {
                return false;
            }

            AutoWrapEventArgs args = new AutoWrapEventArgs()
            {
                InsertionPosition = insertionPos,
                ExistingActivity = this.MyActivity,
                ActivitiesToBeInserted = activityList
            };

            using (ModelEditingScope scope = this.Context.Services.GetService<ModelService>().Root.BeginEdit(SR.WrapInSequenceDescription))
            {
                ModelItem sequenceActivity = WorkflowItemPresenter.AutoWrapInSequenceHandler(this.Context, args);
                if (this.UpdateItem(sequenceActivity, true))
                {
                    scope.Complete();
                    return true;
                }
                else
                {
                    scope.Revert();
                    return false;
                }
            }
        }

        bool DoAutoWrapDrop(InsertionPosition insertionPos, DragEventArgs e, IList<object> droppedObjects = null)
        {
            if (droppedObjects == null)
            {
                ModelTreeManager manager = this.Context.Services.GetRequiredService<ModelTreeManager>();
                EditingScope editingScope = null;

                try
                {
                    editingScope = ModelItemHelper.TryCreateImmediateEditingScope(manager, SR.WrapInSequenceDescription);

                    droppedObjects = this.GetSortedObjectList(e);

                    if (!this.DoAutoWrapDrop(insertionPos, droppedObjects))
                    {
                        return false;
                    }

                    if (editingScope != null)
                    {
                        editingScope.Complete();
                    }
                }
                finally
                {
                    if (editingScope != null)
                    {
                        editingScope.Dispose();
                        editingScope = null;
                    }
                }
            }
            else
            {
                if (!this.DoAutoWrapDrop(insertionPos, droppedObjects))
                {
                    return false;
                }
            }

            if (!DragDropHelper.IsDraggingFromToolbox(e))
            {
                List<WorkflowViewElement> movedViewElements = ObjectList2WorkflowViewElementList(droppedObjects);
                DragDropHelper.SetDragDropMovedViewElements(e, movedViewElements);

                //Backward compatibility for 4.0
                if (droppedObjects.Count == 1 && movedViewElements.Count == 1)
                {
                    #pragma warning disable 618
                    DragDropHelper.SetDragDropCompletedEffects(e, DragDropEffects.Move);
                    #pragma warning restore 618
                }
            }
            else
            {
                Fx.Assert(droppedObjects.Count == 1, "Dropping from Toolbox with count != 1");

                // Set focus if it is dropping from ToolBox.
                // In common drag/drop, the selection setting is done at the end of
                // StartDragging().
                if (this.Item == null)
                {
                    return true;
                }
                
                Fx.Assert(typeof(Sequence).IsAssignableFrom(this.Item.ItemType), 
                    "Auto Wrap didn't add a sequence. Is Item.Properties[\"Activities\"] still correct?");
                foreach (ModelItem item in this.Item.Properties["Activities"].Collection)
                {
                    // Find the ModelItem whose value is an activity from Toolbox.
                    if (item.GetCurrentValue() == droppedObjects[0])
                    {
                        this.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, (Action)(() =>
                        {
                            item.Focus();
                        }));
                        break;
                    }
                }
            }

            return true;
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            this.AllowDrop = true;
            this.Content = containerGrid;
            ICompositeViewEvents containerEvents = null;
            bool isDefault = false;

            this.Loaded += (s, eventArgs) =>
            {
                isDefault = this.IsDefaultContainer;
                DependencyObject parent = VisualTreeHelper.GetParent(this);
                while (null != parent && !typeof(ICompositeViewEvents).IsAssignableFrom(parent.GetType()))
                {
                    parent = VisualTreeHelper.GetParent(parent);
                }
                containerEvents = parent as ICompositeViewEvents;
                if (null != containerEvents)
                {
                    if (isDefault)
                    {
                        containerEvents.RegisterDefaultCompositeView(this);
                    }
                    else
                    {
                        containerEvents.RegisterCompositeView(this);
                    }
                }
                this.shouldSetFocus = true;

                if (this.AutoWrapInSequenceEnabled)
                {
                    // spacer and placer holder
                    this.spacerHelper = new SpacerHelper(this);
                }
            };

            this.Unloaded += (s, eventArgs) =>
            {
                if (null != containerEvents)
                {
                    if (isDefault)
                    {
                        containerEvents.UnregisterDefaultCompositeView(this);
                    }
                    else
                    {
                        containerEvents.UnregisterCompositeView(this);
                    }
                }
                this.shouldSetFocus = false;

                if (this.AutoWrapInSequenceEnabled)
                {
                    if (this.spacerHelper != null)
                    {
                        this.spacerHelper.Unload();
                        this.spacerHelper = null;
                    }
                }
            };
        }

        public string HintText
        {
            get { return (string)GetValue(HintTextProperty); }
            set { SetValue(HintTextProperty, value); }
        }

        [Fx.Tag.KnownXamlExternal]
        public ModelItem Item
        {
            get { return (ModelItem)GetValue(ItemProperty); }
            set { SetValue(ItemProperty, value); }
        }

        public Type AllowedItemType
        {
            get { return (Type)GetValue(AllowedItemTypeProperty); }
            set { SetValue(AllowedItemTypeProperty, value); }
        }

        [Fx.Tag.KnownXamlExternal]
        public TypeResolvingOptions DroppingTypeResolvingOptions
        {
            get { return (TypeResolvingOptions)GetValue(DroppingTypeResolvingOptionsProperty); }
            set { SetValue(DroppingTypeResolvingOptionsProperty, value); }
        }

        EditingContext Context
        {
            get
            {
                if (context == null)
                {
                    IModelTreeItem modelTreeItem = this.Item as IModelTreeItem;
                    if (modelTreeItem != null)
                    {
                        this.context = modelTreeItem.ModelTreeManager.Context;
                    }
                    else // There is no ModelItem yet, try to walk up the tree to find a WorkflowViewElement.
                    {
                        WorkflowViewElement parentViewElement = GetParentWorkflowViewElement();
                        if (parentViewElement != null)
                        {
                            this.context = parentViewElement.Context;
                        }
                    }
                }
                return context;
            }
        }


        public bool IsDefaultContainer
        {
            get { return (bool)GetValue(IsDefaultContainerProperty); }
            set { SetValue(IsDefaultContainerProperty, value); }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            CutCopyPasteHelper.RegisterWithParentViewElement(this);
            base.OnRender(drawingContext);
        }

        private WorkflowViewElement GetParentWorkflowViewElement()
        {
            // Walk the logic tree first.
            FrameworkElement parent = (FrameworkElement)this.Parent;
            while (parent != null && !(parent is WorkflowViewElement))
            {
                parent = parent.Parent as FrameworkElement;
            }
            WorkflowViewElement result = parent as WorkflowViewElement;
            // If not found, walk the visual tree.
            if (null == result)
            {
                parent = VisualTreeHelper.GetParent(this) as FrameworkElement;
                while (parent != null && !(parent is WorkflowViewElement))
                {
                    parent = VisualTreeHelper.GetParent(parent) as FrameworkElement;
                }
                result = parent as WorkflowViewElement;
            }
            return result;
        }


        static void OnItemChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            WorkflowItemPresenter control = (WorkflowItemPresenter)dependencyObject;
            control.OnItemChanged(e);
        }

        void OnItemChanged(DependencyPropertyChangedEventArgs e)
        {
            object newItem = e.NewValue;
            PopulateContent();

            if (newItem != null)
            {
                //We want to set the selection only if the item is dropped or pasted. 
                //We cannot set the selection in UpdateItem, since while pasting that would still be in EditingScope and this.Item will be null.
                if (this.isItemPastedOrDropped)
                {
                    Fx.Assert(this.Item != null, "Item cannot be null");
                    // If we are currently moving from somewhere else to a WorkflowItemPresenter, the currently 
                    // focusing view element will be removed, we need to set the keyboard focus explicitly to 
                    // avoid WPF FocusManager to focus on an element, leading to flashing effect.
                    Keyboard.Focus((UIElement)this.Item.View);
                    this.isItemPastedOrDropped = false;
                }
                if (this.shouldSetFocus)
                {
                    this.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, (Action)(() =>
                    {
                        // check for Item == null, we found an interesting 


                        if (this.Item != null)
                        {
                            UIElement view = (UIElement)(this.Item.View);
                            if (view != null)
                            {
                                Keyboard.Focus(view);
                                Selection.SelectOnly(this.Context, this.Item);
                            }
                        }
                        //this.shouldSetFocus = false;
                    }));
                }
            }
            else
            {
                // remove the selection if the previous value was selected.
                if (this.Context != null)
                {
                    if (this.Context.Items.GetValue<Selection>().SelectedObjects.Contains(e.OldValue))
                    {
                        this.Context.Items.SetValue(new Selection(new ModelItem[] { }));
                    }
                }
            }
        }


        void PopulateContent()
        {
            if (this.Item != null)
            {
                VirtualizedContainerService containerService = this.Context.Services.GetService<VirtualizedContainerService>();
                UIElement itemView = containerService.GetContainer(this.Item, this);
                this.contentGrid.Children.Clear();
                this.contentGrid.Children.Add(itemView);
            }
            else
            {
                contentGrid.Children.Clear();
                contentGrid.Children.Add(text);
            }
        }

        bool UpdateItem(object newItem)
        {
            return UpdateItem(newItem, false);
        }

        bool UpdateItem(object newItem, bool allowReplaceExistingActivity)
        {
            bool updateSucceeded = false;
            ModelItem newModelItem = newItem as ModelItem;
            if (this.Item == null || allowReplaceExistingActivity)
            {
                if (newModelItem == null && newItem != null)
                {
                    // try to wrap the droppedObject in  a ModelItem.
                    ModelServiceImpl modelService = (ModelServiceImpl)this.Context.Services.GetService<ModelService>();
                    newModelItem = modelService.WrapAsModelItem(newItem);
                }
                if (this.CanUpdateItem(newModelItem))
                {
                    // In order to allow for model updates that happens during the model item is drop, this is all done in an atomic unit.
                    using (ModelEditingScope editingScope = this.Context.Services.GetService<ModelService>().Root.BeginEdit(SR.PropertyChangeEditingScopeDescription))
                    {
                        this.Item = newModelItem;
                        editingScope.Complete();
                    }
                    updateSucceeded = true;
                    this.isItemPastedOrDropped = true;
                }
            }
            return updateSucceeded;

        }

        bool CanUpdateItem(ModelItem newModelItem)
        {
            return null != newModelItem
                && TypeUtilities.IsTypeCompatible(newModelItem.ItemType, this.AllowedItemType)
                && !this.IsInParentChain(newModelItem);
        }

        List<object> GetSortedObjectList(DragEventArgs args)
        {
            IEnumerable<object> droppedObjects = DragDropHelper.GetDroppedObjects(this, args, this.context);
            return DragDropHelper.SortSelectedObjects(droppedObjects);
        }

        bool DoSingleDrop(object droppedObject, DragEventArgs args)
        {
            if (UpdateItem(droppedObject))
            {
                args.Handled = true;
                #pragma warning disable 618
                DragDropHelper.SetDragDropCompletedEffects(args, DragDropEffects.Move);
                #pragma warning restore 618
                if (droppedObject is ModelItem && ((ModelItem)droppedObject).View != null)
                {
                    DragDropHelper.SetDragDropMovedViewElements(args, new WorkflowViewElement[] { ((ModelItem)droppedObject).View as WorkflowViewElement });
                }

                return true;
            }

            return false;
        }

        protected override void OnDrop(DragEventArgs e)
        {
            ModelTreeManager manager = this.Context.Services.GetService<ModelTreeManager>();

            // When dragging from toolbox:
            //     editingScope should not be null
            //     there should only be one item
            // When dragging from canvas:
            //     editingScope should be null
            // Call editingScope.Complete() to commit changes, otherwise the editing scope will be aborted
            using (EditingScope editingScope = ModelItemHelper.TryCreateImmediateEditingScope(manager, SR.PropertyChangeEditingScopeDescription))
            {
                List<object> droppedObjects = this.GetSortedObjectList(e);
#pragma warning disable 618
                DragDropHelper.SetDragDropCompletedEffects(e, DragDropEffects.None);
#pragma warning restore 618
                if (droppedObjects == null || droppedObjects.Count == 0)
                {
                    return;
                }
                if (droppedObjects.Count == 1)
                {
                    if (this.DoSingleDrop(droppedObjects[0], e))
                    {
                        if (editingScope != null)
                        {
                            editingScope.Complete();
                        }
                    }
                    return;
                }
                else
                {
                    // multi drop
                    Fx.Assert(editingScope == null, "editingScope should be null for dragging from canvas.");
                    this.DoAutoWrapDrop(InsertionPosition.None, e, droppedObjects);
                }
                base.OnDrop(e);
            }
        }

        void OnDrag(DragEventArgs e)
        {
            if (!e.Handled)
            {
                this.UpdateEffects(e);
                e.Handled = true;
            }
        }

        void UpdateEffects(DragEventArgs args)
        {
            if (!DragDropHelper.AllowDrop(args.Data, this.Context, this.AllowedItemType))
            {
                args.Effects = DragDropEffects.None;
            }
        }

        protected override void OnDragEnter(DragEventArgs e)
        {
            this.OnDrag(e);
            base.OnDragEnter(e);
        }

        protected override void OnDragOver(DragEventArgs e)
        {
            this.OnDrag(e);
            base.OnDragOver(e);
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            // do not move focus if it's a ctrl right click.
            if (e.RightButton == MouseButtonState.Pressed)
            {
                if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                {
                    e.Handled = true;
                    base.OnMouseDown(e);
                    return;
                }
            }

            // Schedule the Keyboard.Focus command to let it execute later than WorkflowViewElement.OnMouseDown, 
            // where WorkflowViewElement will move the keyboard focus on itself
            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
            {
                Keyboard.Focus((FrameworkElement)this);
            }));
            base.OnMouseDown(e);
        }

        private bool IsInParentChain(ModelItem droppedModelItem)
        {
            bool isInParentChain = false;
            // start with immediate workflowviewElement outside this.
            WorkflowViewElement parentViewElement = GetParentWorkflowViewElement();
            if (parentViewElement != null)
            {
                ModelItem parentModelItem = parentViewElement.ModelItem;
                while (parentModelItem != null)
                {
                    if (parentModelItem == droppedModelItem)
                    {
                        isInParentChain = true;
                        break;
                    }
                    parentModelItem = parentModelItem.Parent;
                }
            }
            return isInParentChain;
        }

        void DeleteItem()
        {
            this.Item = null;
            this.PopulateContent();
        }

        void ICompositeView.OnItemMoved(ModelItem modelItem)
        {
            if (this.Item == modelItem)
            {
                this.Item = null;
            }
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new WorkflowItemPresenterAutomationPeer(this);
        }


        object ICompositeView.OnItemsCut(List<ModelItem> itemsToCut)
        {
            Fx.Assert(itemsToCut.Count == 1, "Only one item can be cut");
            Fx.Assert(itemsToCut[0].Equals(this.Item), "Only one item can be cut.");
            this.DeleteItem();
            return null;
        }

        object ICompositeView.OnItemsCopied(List<ModelItem> itemsToCopy)
        {
            return null;
        }

        void ICompositeView.OnItemsPasted(List<object> itemsToPaste, List<object> metaData, Point pastePoint, WorkflowViewElement pastePointReference)
        {
            if (itemsToPaste.Count == 1)
            {
                // Single Paste
                UpdateItem(itemsToPaste[0]);
            }
            else
            {
                // Mutiple Paste.
                IList<object> sortedList = CutCopyPasteHelper.SortFromMetaData(itemsToPaste, metaData);
                Fx.Assert(this.Item == null, "multi-paste on item != null is not supported now");
                this.DoAutoWrapDrop(InsertionPosition.None, sortedList); 
            }
        }

        void ICompositeView.OnItemsDelete(List<ModelItem> itemsToDelete)
        {
            if (null != itemsToDelete && itemsToDelete.Contains(this.Item))
            {
                this.DeleteItem();
            }
        }


        bool ICompositeView.CanPasteItems(List<object> itemsToPaste)
        {
            return null != itemsToPaste &&
                itemsToPaste.Count > 0 &&
                null != itemsToPaste[0] &&
                null == this.Item &&
                ((itemsToPaste[0] is ModelItem && this.CanUpdateItem((ModelItem)itemsToPaste[0])) ||
                (itemsToPaste[0] is Type && this.AllowedItemType.IsAssignableFrom((Type)itemsToPaste[0])) ||
                this.AllowedItemType.IsAssignableFrom(itemsToPaste[0].GetType()));
        }


        class WorkflowItemPresenterAutomationPeer : UIElementAutomationPeer
        {
            WorkflowItemPresenter owner;

            public WorkflowItemPresenterAutomationPeer(WorkflowItemPresenter owner)
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
                string baseAutomationID = base.GetAutomationIdCore();
                if (!string.IsNullOrEmpty(baseAutomationID))
                {
                    return baseAutomationID;
                }
                return this.owner.GetType().Name;
            }

            protected override string GetNameCore()
            {
                // Return an empty string if an activity is dropped on the presenter
                if (owner.Item != null)
                {
                    return string.Empty;
                }
                string name = base.GetNameCore();
                if (string.IsNullOrEmpty(name))
                {
                    name = this.owner.HintText;
                }
                return name;
            }

            protected override string GetClassNameCore()
            {
                return this.owner.GetType().Name;
            }
        }

        private static ModelItem AutoWrapInSequenceHandler(EditingContext editingContext, AutoWrapEventArgs e)
        {
            Fx.Assert(e.ExistingActivity != null || e.InsertionPosition == InsertionPosition.None, 
                "Existing activity must not be null");

            ModelItem sequence = editingContext.Services.GetService<ModelTreeManager>().CreateModelItem(null, new Sequence());
            foreach (Activity activity in e.ActivitiesToBeInserted)
            {
                sequence.Properties["Activities"].Collection.Add(activity);
            }

            switch (e.InsertionPosition)
            {
                case InsertionPosition.Before:
                    sequence.Properties["Activities"].Collection.Add(e.ExistingActivity);
                    break;
                case InsertionPosition.After:
                    sequence.Properties["Activities"].Collection.Insert(0, e.ExistingActivity);
                    break;
                case InsertionPosition.None:
                    break;
                default:
                    Fx.Assert("Invalid insert position");
                    break;
            }

            return sequence;
        }

        // NOTE: This wrapper method is exclusively called by TransitionDesigner, because
        // WIP of Transition.Action would handle the event if the dragged source comes from
        // WIP of Transition.Trigger (see 


        internal void ShowSpacerHelperOnDraggedItems(DragEventArgs arg)
        {
            this.spacerHelper.OnWfItemPresenterPreviewDragEnter(this, arg);
        }

        // classes and helpers for Spacer
        private sealed class SpacerWrapper
        {
            public FrameworkElement Spacer { get; set; }
            public SpacerPlaceholder Placeholder { get; set; }

            public void ShowSpacer()
            {
                if (Spacer != null)
                {
                    Spacer.Visibility = Visibility.Visible;
                }
                if (Placeholder != null)
                {
                    Placeholder.Visibility = Visibility.Collapsed;
                }
            }

            public void HideSpacer()
            {
                if (Spacer != null)
                {
                    Spacer.Visibility = Visibility.Collapsed;
                }
                if (Placeholder != null)
                {
                    Placeholder.Visibility = Visibility.Visible;
                }
            }

            public bool HighlightPlaceholder
            {
                set
                {
                    this.Placeholder.TargetVisiable = value;
                }
            }
        }

        // All the e.Handle = true in OnDragXXXEnter/Leave/Over:
        // Prevent the events to be further handled by OnDrag, which will set DragDropEffects to None
        private sealed class SpacerHelper
        {
            public SpacerWrapper TopSpacerWrapper { get; set; }
            public SpacerWrapper BottomSpacerWrapper { get; set; }
            public Timer SpacerTimer { get; set; }

            private WorkflowItemPresenter wfItemPresenter;
            private SpacerWrapper SpacerToShow { get; set; }

            static private SpacerHelper uniqueSpacerHelper = null;

            static private SpacerHelper UniqueSpacerHelper
            {
                set
                {
                    if (uniqueSpacerHelper == value)
                    {
                        return;
                    }
                    if (uniqueSpacerHelper != null)
                    {
                        uniqueSpacerHelper.HighLighted = false;
                    }
                    uniqueSpacerHelper = value;
                    if (uniqueSpacerHelper != null)
                    {
                        uniqueSpacerHelper.HighLighted = true;
                    }
                }
            }

            public SpacerHelper(WorkflowItemPresenter wfItemPresenter)
            {
                Fx.Assert(wfItemPresenter != null, "null WorkflowItemPresenter");
                this.TopSpacerWrapper = new SpacerWrapper();
                this.BottomSpacerWrapper = new SpacerWrapper();
                this.wfItemPresenter = wfItemPresenter;
                Loaded();
            }

            public bool HighLighted 
            {
                set
                {
                    this.TopSpacerWrapper.HighlightPlaceholder = value;
                    this.BottomSpacerWrapper.HighlightPlaceholder = value;
                }
            }

            public void Unload()
            {
                // event
                this.wfItemPresenter.containerGrid.PreviewDrop  -= new DragEventHandler(OnContainerGridPreviewDrop);
                this.wfItemPresenter.PreviewDragEnter           -= new DragEventHandler(OnWfItemPresenterPreviewDragEnter);
                this.wfItemPresenter.PreviewDragLeave           -= new DragEventHandler(OnWfItemPresenterPreviewDragLeave);

                this.TopSpacerWrapper.Placeholder.DragEnter     -= new DragEventHandler(OnTopPlaceholderDragEnter);
                this.TopSpacerWrapper.Placeholder.DragLeave     -= new DragEventHandler(OnPlaceHoderDragLeave);
                this.TopSpacerWrapper.Spacer.DragEnter          -= new DragEventHandler(OnTopSpacerDragEnter);
                this.TopSpacerWrapper.Spacer.DragLeave          -= new DragEventHandler(OnTopSpacerDragLeave);

                this.BottomSpacerWrapper.Placeholder.DragEnter  -= new DragEventHandler(OnBottomPlaceholderDragEnter);
                this.BottomSpacerWrapper.Placeholder.DragLeave  -= new DragEventHandler(OnPlaceHoderDragLeave);
                this.BottomSpacerWrapper.Spacer.DragEnter       -= new DragEventHandler(OnBottomSpacerDragEnter);
                this.BottomSpacerWrapper.Spacer.DragLeave       -= new DragEventHandler(OnBottomSpacerDragLeave);

                this.SpacerTimer.Elapsed -= new ElapsedEventHandler(OnSpacerTimerElapsed);
            }

            void UpdateEffects(DragEventArgs args)
            {
                if (this.wfItemPresenter.Item == null)
                {
                    // Item is null, then use WIP's UpdateEffects.
                    this.wfItemPresenter.UpdateEffects(args);
                    return;
                }

                if (!this.AllowDropOnSpacer(args))
                {
                    args.Effects = DragDropEffects.None;
                }
            }

            void OnTopPlaceholderDragEnter(object sender, DragEventArgs e)
            {
                this.UpdateEffects(e);
                this.OnPlaceholderEnter(this.TopSpacerWrapper, e);
                e.Handled = true;
            }

            void OnBottomPlaceholderDragEnter(object sender, DragEventArgs e)
            {
                this.UpdateEffects(e);
                this.OnPlaceholderEnter(this.BottomSpacerWrapper, e);
                e.Handled = true;
            }

            void OnPlaceHoderDragLeave(object sender, DragEventArgs e)
            {
                this.UpdateEffects(e);
                this.SpacerTimer.Stop();
                e.Handled = true;
            }

            void OnTopSpacerDragEnter(object sender, DragEventArgs e)
            {
                this.UpdateEffects(e);
                this.TopSpacerWrapper.ShowSpacer();
                e.Handled = true;
            }

            void OnTopSpacerDragLeave(object sender, DragEventArgs e)
            {
                this.TopSpacerWrapper.HideSpacer();
                e.Handled = true;
            }

            void OnBottomSpacerDragEnter(object sender, DragEventArgs e)
            {
                this.BottomSpacerWrapper.ShowSpacer();
                e.Handled = true;
            }

            void OnBottomSpacerDragLeave(object sender, DragEventArgs e)
            {
                this.UpdateEffects(e);
                this.BottomSpacerWrapper.HideSpacer();
                e.Handled = true;
            }

            void OnSpacerOrPlaceholderDragOver(object sender, DragEventArgs e)
            {
                this.UpdateEffects(e);
                e.Handled = true;
            }

            void OnSpacerDrop(object sender, DragEventArgs e)
            {
                if (!this.AllowDropOnSpacer(e))
                {
                    return;
                }
                InsertionPosition insertionPos = (sender == this.BottomSpacerWrapper.Spacer)
                    ? InsertionPosition.After : InsertionPosition.Before;

                ModelItemHelper.TryCreateImmediateEditingScopeAndExecute(this.wfItemPresenter.Context, SR.WrapInSequenceDescription, (es) =>
                {
                    if (this.wfItemPresenter.DoAutoWrapDrop(insertionPos, e))
                    {
                        // auto wrap is successful
                        if (es != null)
                        {
                            // if we created an immedate editing scope, try to complete it.
                            es.Complete();
                        }
                    }
                });
            }

            void OnPlaceholderDrop(object sender, DragEventArgs e)
            {
                if (!this.AllowDropOnSpacer(e))
                {
                    return;
                }
                InsertionPosition insertionPos = (sender == this.BottomSpacerWrapper.Placeholder)
                    ? InsertionPosition.After : InsertionPosition.Before;
                ModelItemHelper.TryCreateImmediateEditingScopeAndExecute(this.wfItemPresenter.Context, SR.WrapInSequenceDescription, (es) =>
                    {
                        if (this.wfItemPresenter.DoAutoWrapDrop(insertionPos, e))
                        {
                            // auto wrap is successful
                            if (es != null)
                            {
                                // if we created an immediate editing scope, try to complete it.
                                es.Complete();
                            }
                        }
                    });
            }

            void OnSpacerTimerElapsed(object sender, ElapsedEventArgs e)
            {
                this.wfItemPresenter.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                {
                    if (this.SpacerToShow != null)
                    {
                        SpacerToShow.ShowSpacer();
                        SpacerToShow = null;
                    }
                }));
            }

            void OnContainerGridPreviewDrop(object sender, DragEventArgs e)
            {
                this.SpacerTimer.Stop();
                this.TopSpacerWrapper.HideSpacer();
                this.BottomSpacerWrapper.HideSpacer();
                UniqueSpacerHelper = null;
            }

            private FrameworkElement CreateAndInitializeSpacer(VerticalAlignment alignment)
            {
                FrameworkElement spacer = (SpacerTemplate != null)
                    ? (FrameworkElement)SpacerTemplate.LoadContent()
                    : new Rectangle();
                spacer.AllowDrop = true;
                spacer.DragOver += new DragEventHandler(OnSpacerOrPlaceholderDragOver);
                spacer.Drop += new DragEventHandler(OnSpacerDrop);
                spacer.VerticalAlignment = alignment;
                spacer.IsHitTestVisible = true;
                spacer.Visibility = Visibility.Collapsed;
                return spacer;
            }

            void OnWfItemPresenterPreviewDragLeave(object sender, DragEventArgs e)
            {
                UniqueSpacerHelper = null;
                this.TopSpacerWrapper.HideSpacer();
                this.BottomSpacerWrapper.HideSpacer();
            }

            // this method is made internal because WorkflowItemPresenter.ShowSpacerHelperOnDraggedItems
            // needs to access this method to show the spacer UI gesture for Auto-surround.
            internal void OnWfItemPresenterPreviewDragEnter(object sender, DragEventArgs arg)
            {
                if (!this.AllowDropOnSpacer(arg))
                {
                    return;
                }
                UniqueSpacerHelper = this;
            }

            private void Loaded()
            {
                this.TopSpacerWrapper.Spacer            = CreateAndInitializeSpacer(VerticalAlignment.Bottom);
                this.TopSpacerWrapper.Placeholder       = CreateSpacerPlaceHolder();
                this.BottomSpacerWrapper.Spacer         = CreateAndInitializeSpacer(VerticalAlignment.Top);
                this.BottomSpacerWrapper.Placeholder    = CreateSpacerPlaceHolder();

                // timer
                this.SpacerTimer = new Timer(500);
                this.SpacerTimer.Elapsed += new ElapsedEventHandler(OnSpacerTimerElapsed);
                this.SpacerTimer.AutoReset = false;

                // view
                this.wfItemPresenter.stackPanel.Children.Insert(0, TopSpacerWrapper.Spacer);
                this.wfItemPresenter.stackPanel.Children.Insert(0, TopSpacerWrapper.Placeholder);
                this.wfItemPresenter.stackPanel.Children.Insert(this.wfItemPresenter.stackPanel.Children.Count, BottomSpacerWrapper.Spacer);
                this.wfItemPresenter.stackPanel.Children.Insert(this.wfItemPresenter.stackPanel.Children.Count, BottomSpacerWrapper.Placeholder);
                this.wfItemPresenter.containerGrid.Background = Brushes.Transparent;

                // event
                this.wfItemPresenter.containerGrid.PreviewDrop  += new DragEventHandler(OnContainerGridPreviewDrop);
                this.wfItemPresenter.PreviewDragEnter           += new DragEventHandler(OnWfItemPresenterPreviewDragEnter);
                this.wfItemPresenter.PreviewDragLeave           += new DragEventHandler(OnWfItemPresenterPreviewDragLeave);
                this.TopSpacerWrapper.Placeholder.DragEnter     += new DragEventHandler(OnTopPlaceholderDragEnter);
                this.TopSpacerWrapper.Placeholder.DragLeave     += new DragEventHandler(OnPlaceHoderDragLeave);
                this.TopSpacerWrapper.Spacer.DragEnter          += new DragEventHandler(OnTopSpacerDragEnter);
                this.TopSpacerWrapper.Spacer.DragLeave          += new DragEventHandler(OnTopSpacerDragLeave);

                this.BottomSpacerWrapper.Placeholder.DragEnter  += new DragEventHandler(OnBottomPlaceholderDragEnter);
                this.BottomSpacerWrapper.Placeholder.DragLeave  += new DragEventHandler(OnPlaceHoderDragLeave);
                this.BottomSpacerWrapper.Spacer.DragEnter       += new DragEventHandler(OnBottomSpacerDragEnter);
                this.BottomSpacerWrapper.Spacer.DragLeave       += new DragEventHandler(OnBottomSpacerDragLeave);
            }

            private static DataTemplate defaultSpacerTemplate = CreateDefaultSpacerTemplate();

            private static DataTemplate SpacerTemplate
            {
                get { return defaultSpacerTemplate; }
            }

            private static DataTemplate CreateDefaultSpacerTemplate()
            {
                FrameworkElementFactory feFactory = new FrameworkElementFactory(typeof(VerticalConnector));
                DataTemplate dt = new DataTemplate() { VisualTree = feFactory };
                dt.Seal();
                return dt;
            }

            private SpacerPlaceholder CreateSpacerPlaceHolder()
            {
                // The place holder should be something that can triger DragEnter
                SpacerPlaceholder spacerPlaceholder = new SpacerPlaceholder { MinHeight = 20, Visibility = Visibility.Visible, AllowDrop = true };
                spacerPlaceholder.DragOver += new DragEventHandler(OnSpacerOrPlaceholderDragOver);
                spacerPlaceholder.Drop += new DragEventHandler(OnPlaceholderDrop);
                return spacerPlaceholder; 
            }

            private void OnPlaceholderEnter(SpacerWrapper wrapper, DragEventArgs e)
            {
                if (!this.AllowDropOnSpacer(e))
                {
                    return;
                }
                this.SpacerToShow = wrapper;
                this.SpacerTimer.Start();
                e.Handled = true;
            }

            private bool AllowDropOnSpacer(DragEventArgs e)
            {
                return (this.wfItemPresenter.Item != null
                    && !this.IsOwnerActivityBeingDragged(e)
                    && DragDropHelper.AllowDrop(typeof(Sequence), this.wfItemPresenter.AllowedItemType)     // Is Sequence allowed to be dropped inside the WIP? Beause it will trigger AutoWrap.
                    && DragDropHelper.AllowDrop(e.Data, this.wfItemPresenter.Context, typeof(Activity)));   // Is the item being dragged allowed to be dropped onto Sequence?
            }

            private bool IsOwnerActivityBeingDragged(DragEventArgs e)
            {
                if (this.wfItemPresenter.Item == null)
                {
                    return false;
                }
                else
                {
                    // In case of a toolbox drop, DragDropHelper.GetObjectsToBeDropped 
                    //  will create an instance, which will possibliy pop up a type picker
                    //  dialog for generic activities. So check for it first and avoid
                    //  pop up dialogs.
                    if (DragDropHelper.IsDraggingFromToolbox(e))
                    {
                        return false;
                    }
                    IEnumerable<ModelItem> draggedObjects = DragDropHelper.GetDraggedModelItems(e);
                    return draggedObjects.Contains(this.wfItemPresenter.Item);
                }
            }
        }

    }
}
