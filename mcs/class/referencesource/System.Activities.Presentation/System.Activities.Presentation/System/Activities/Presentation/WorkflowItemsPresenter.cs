//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

#pragma warning disable 618

namespace System.Activities.Presentation
{
    using System.Activities.Presentation.Internal.PropertyEditing;
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.Services;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Runtime;
    using System.Windows;
    using System.Windows.Automation.Peers;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Markup;
    using System.Windows.Media;
    using System.Windows.Threading;
    using System.Activities.Presentation.View;
    using System.Windows.Shapes;

    // This is similar to the WorkflowItemPresenter , but its an edit box for collections. It supports drag drop, and delete.
    // it auto refreshes the collection on collection changed events.
    public class WorkflowItemsPresenter : ContentControl, IMultipleDragEnabledCompositeView
    {

        public static readonly DependencyProperty HintTextProperty =
            DependencyProperty.Register("HintText", typeof(string), typeof(WorkflowItemsPresenter), new UIPropertyMetadata(String.Empty, new PropertyChangedCallback(WorkflowItemsPresenter.OnHintTextChanged)));

        public static readonly DependencyProperty ItemsProperty =
            DependencyProperty.Register("Items", typeof(ModelItemCollection), typeof(WorkflowItemsPresenter), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(WorkflowItemsPresenter.OnItemsChanged)));

        public static readonly DependencyProperty SpacerTemplateProperty =
            DependencyProperty.Register("SpacerTemplate", typeof(DataTemplate), typeof(WorkflowItemsPresenter), new UIPropertyMetadata(null));

        public static readonly DependencyProperty HeaderTemplateProperty =
            DependencyProperty.Register("HeaderTemplate", typeof(DataTemplate), typeof(WorkflowItemsPresenter), new UIPropertyMetadata(null));

        public static readonly DependencyProperty FooterTemplateProperty =
            DependencyProperty.Register("FooterTemplate", typeof(DataTemplate), typeof(WorkflowItemsPresenter), new UIPropertyMetadata(null));

        public static readonly DependencyProperty ItemsPanelProperty =
            DependencyProperty.Register("ItemsPanel", typeof(ItemsPanelTemplate), typeof(WorkflowItemsPresenter), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(WorkflowItemsPresenter.OnItemsPanelChanged)));

        public static readonly DependencyProperty IndexProperty =
            DependencyProperty.RegisterAttached("Index", typeof(int), typeof(WorkflowItemsPresenter), new UIPropertyMetadata(addAtEndMarker));

        public static readonly DependencyProperty AllowedItemTypeProperty =
            DependencyProperty.Register("AllowedItemType", typeof(Type), typeof(WorkflowItemsPresenter), new UIPropertyMetadata(typeof(object)));

        public static readonly DependencyProperty IsDefaultContainerProperty =
            DependencyProperty.Register("IsDefaultContainer", typeof(bool), typeof(WorkflowItemsPresenter), new UIPropertyMetadata(false));

        public static readonly DependencyProperty DroppingTypeResolvingOptionsProperty =
            DependencyProperty.Register("DroppingTypeResolvingOptions", typeof(TypeResolvingOptions), typeof(WorkflowItemsPresenter));


        const int addAtEndMarker = -2;

        int selectedSpacerIndex;


        ItemsControl panel;
        Grid hintTextGrid;
        EditingContext context = null;
        bool isRegisteredWithParent = false;
        bool populateOnLoad = false;
        bool handleSpacerGotKeyboardFocus = false;
        Grid outerGrid;

        public WorkflowItemsPresenter()
        {
            panel = new ItemsControl();
            panel.Focusable = false;
            hintTextGrid = new Grid();
            hintTextGrid.Focusable = false;
            hintTextGrid.Background = Brushes.Transparent;
            hintTextGrid.DataContext = this;
            hintTextGrid.SetBinding(Grid.MinHeightProperty, "MinHeight");
            hintTextGrid.SetBinding(Grid.MinWidthProperty, "MinWidth");
            TextBlock text = new TextBlock();
            text.Focusable = false;
            text.SetBinding(TextBlock.TextProperty, "HintText");
            text.HorizontalAlignment = HorizontalAlignment.Center;
            text.VerticalAlignment = VerticalAlignment.Center;
            text.DataContext = this;
            text.Foreground = new SolidColorBrush(SystemColors.GrayTextColor);
            text.FontStyle = FontStyles.Italic;
            ((IAddChild)hintTextGrid).AddChild(text);

            this.outerGrid = new Grid()
            {
                RowDefinitions = { new RowDefinition(), new RowDefinition() },
                ColumnDefinitions = { new ColumnDefinition() }
            };
            Grid.SetRow(this.panel, 0);
            Grid.SetColumn(this.panel, 0);
            Grid.SetRow(this.hintTextGrid, 1);
            Grid.SetColumn(this.hintTextGrid, 0);
            this.outerGrid.Children.Add(panel);
            this.outerGrid.Children.Add(hintTextGrid);
        }


        public Type AllowedItemType
        {
            get { return (Type)GetValue(AllowedItemTypeProperty); }
            set { SetValue(AllowedItemTypeProperty, value); }
        }

        public string HintText
        {
            get { return (string)GetValue(HintTextProperty); }
            set { SetValue(HintTextProperty, value); }
        }

        [Fx.Tag.KnownXamlExternal]
        public DataTemplate SpacerTemplate
        {
            get { return (DataTemplate)GetValue(SpacerTemplateProperty); }
            set { SetValue(SpacerTemplateProperty, value); }
        }

        [Fx.Tag.KnownXamlExternal]
        public DataTemplate HeaderTemplate
        {
            get { return (DataTemplate)GetValue(HeaderTemplateProperty); }
            set { SetValue(HeaderTemplateProperty, value); }
        }

        [Fx.Tag.KnownXamlExternal]
        public DataTemplate FooterTemplate
        {
            get { return (DataTemplate)GetValue(FooterTemplateProperty); }
            set { SetValue(FooterTemplateProperty, value); }
        }

        [Fx.Tag.KnownXamlExternal]
        public ItemsPanelTemplate ItemsPanel
        {
            get { return (ItemsPanelTemplate)GetValue(ItemsPanelProperty); }
            set { SetValue(ItemsPanelProperty, value); }
        }

        [SuppressMessage(FxCop.Category.Usage, FxCop.Rule.CollectionPropertiesShouldBeReadOnly,
            Justification = "Setter is provided to enable setting this property in code.")]
        [Fx.Tag.KnownXamlExternal]
        public ModelItemCollection Items
        {
            get { return (ModelItemCollection)GetValue(ItemsProperty); }
            set { SetValue(ItemsProperty, value); }
        }

        EditingContext Context
        {
            get
            {
                if (context == null)
                {
                    IModelTreeItem modelTreeItem = this.Items as IModelTreeItem;
                    if (modelTreeItem != null)
                    {
                        this.context = modelTreeItem.ModelTreeManager.Context;
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

        [Fx.Tag.KnownXamlExternal]
        public TypeResolvingOptions DroppingTypeResolvingOptions
        {
            get { return (TypeResolvingOptions)GetValue(DroppingTypeResolvingOptionsProperty); }
            set { SetValue(DroppingTypeResolvingOptionsProperty, value); }
        }

        static void OnHintTextChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            WorkflowItemsPresenter itemsPresenter = (WorkflowItemsPresenter)dependencyObject;
            itemsPresenter.UpdateHintTextVisibility(e.NewValue as string);
        }

        static void OnItemsChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            WorkflowItemsPresenter itemsPresenter = (WorkflowItemsPresenter)dependencyObject;
            itemsPresenter.OnItemsChanged((ModelItemCollection)e.OldValue, (ModelItemCollection)e.NewValue);
        }

        static void OnItemsPanelChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            WorkflowItemsPresenter itemsPresenter = (WorkflowItemsPresenter)dependencyObject;
            itemsPresenter.panel.ItemsPanel = (ItemsPanelTemplate)e.NewValue;
        }

        void OnItemsChanged(ModelItemCollection oldItemsCollection, ModelItemCollection newItemsCollection)
        {
            if (oldItemsCollection != null)
            {
                oldItemsCollection.CollectionChanged -= this.OnCollectionChanged;
            }

            if (newItemsCollection != null)
            {
                newItemsCollection.CollectionChanged += this.OnCollectionChanged;
            }

            if (!isRegisteredWithParent)
            {
                CutCopyPasteHelper.RegisterWithParentViewElement(this);
                isRegisteredWithParent = true;
            }
            populateOnLoad = false;
            PopulateContent();
        }

        void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // if this.Items is null, and we are getting a collection changed that 
            // means this event some how happened before this can get the unloaded event
            // and unsubscribe from this event.
            if (this.Items == null)
            {
                return;
            }
            bool fullRepopulateNeeded = true;

            // when one item is dropped into this items presenter focus on the new view element for it.
            if (e.Action == NotifyCollectionChangedAction.Add
                && e.NewItems != null
                && e.NewItems.Count == 1)
            {
                // insert itemview and spacer
                fullRepopulateNeeded = false;
                int itemViewIndex = GetViewIndexForItem(e.NewStartingIndex);
                VirtualizedContainerService containerService = this.Context.Services.GetService<VirtualizedContainerService>();
                UIElement itemView = containerService.GetContainer((ModelItem)e.NewItems[0], this);
                this.panel.Items.Insert(itemViewIndex, itemView as UIElement);
                // index 2 + i*2 + 1 is spacer i+1
                FrameworkElement spacer = CreateSpacer();
                this.panel.Items.Insert(itemViewIndex + 1, spacer);


                ModelItem insertedItem = (ModelItem)e.NewItems[0];
                this.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, (Action)(() =>
                {
                    UIElement view = (UIElement)insertedItem.View;
                    if (view != null)
                    {
                        Keyboard.Focus(view);
                    }
                }));
            }

            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                if (e.OldItems != null && e.OldItems.Count == 1)
                {
                    fullRepopulateNeeded = false;
                    int itemViewIndex = GetViewIndexForItem(e.OldStartingIndex);
                    this.panel.Items.RemoveAt(itemViewIndex);
                    //remove spacer also
                    this.panel.Items.RemoveAt(itemViewIndex);
                }

                if (this.Items.Count == 0)
                {
                    fullRepopulateNeeded = true;
                }

                // deselect removed items
                if (this.Context != null)
                {
                    IList<ModelItem> selectedItems = this.Context.Items.GetValue<Selection>().SelectedObjects.ToList();
                    foreach (ModelItem selectedAndRemovedItem in selectedItems.Intersect(e.OldItems.Cast<ModelItem>()))
                    {
                        Selection.Toggle(this.Context, selectedAndRemovedItem);
                    }
                }
            }
            if (this.Items.Count > 0)
            {
                this.hintTextGrid.Visibility = Visibility.Collapsed;
            }
            else
            {
                this.hintTextGrid.Visibility = Visibility.Visible;
            }

            if (fullRepopulateNeeded)
            {
                PopulateContent();
            }
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            this.AllowDrop = true;
            this.Content = outerGrid;
            if (this.ItemsPanel != null)
            {
                this.panel.ItemsPanel = this.ItemsPanel;
            }

            ICompositeViewEvents containerEvents = null;
            bool isDefault = false;

            this.Loaded += (s, eventArgs) =>
            {
                isDefault = this.IsDefaultContainer;
                selectedSpacerIndex = addAtEndMarker;
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
                if (this.Items != null)
                {
                    //UnRegistering because of 137896: Inside tab control multiple Loaded events happen without an Unloaded event.
                    this.Items.CollectionChanged -= new NotifyCollectionChangedEventHandler(OnCollectionChanged);
                    this.Items.CollectionChanged += new NotifyCollectionChangedEventHandler(OnCollectionChanged);
                }
                if (populateOnLoad)
                {
                    this.PopulateContent();
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
                if (this.Items != null)
                {
                    this.Items.CollectionChanged -= new NotifyCollectionChangedEventHandler(OnCollectionChanged);
                }
                populateOnLoad = true;
            };
        }

        void PopulateContent()
        {
            this.panel.Items.Clear();

            if (this.Items != null)
            {

                // index 0 is header.
                ContentControl header = new ContentControl();
                header.Focusable = false;
                header.ContentTemplate = this.HeaderTemplate;
                header.SetValue(IndexProperty, 0);
                header.Drop += new DragEventHandler(OnSpacerDrop);
                this.panel.Items.Add(header);

                // index 1 is first spacer
                FrameworkElement startSpacer = CreateSpacer();
                this.panel.Items.Add(startSpacer);

                foreach (ModelItem item in this.Items)
                {
                    // index 2 + i*2 is itemView i
                    VirtualizedContainerService containerService = this.Context.Services.GetService<VirtualizedContainerService>();
                    UIElement itemView = containerService.GetContainer(item, this);
                    this.panel.Items.Add(itemView as UIElement);
                    // index 2 + i*2 + 1 is spacer i+1
                    FrameworkElement spacer = CreateSpacer();
                    this.panel.Items.Add(spacer);
                }
                // index 2 + count*2 is footer 
                ContentControl footer = new ContentControl();
                footer.ContentTemplate = this.FooterTemplate;
                footer.Focusable = true;
                footer.IsHitTestVisible = true;
                footer.IsTabStop = true;
                footer.SetValue(IndexProperty, addAtEndMarker);
                footer.Drop += new DragEventHandler(OnSpacerDrop);
                footer.LostFocus += new RoutedEventHandler(OnSpacerLostFocus);
                footer.GotFocus += new RoutedEventHandler(OnSpacerGotFocus);
                this.panel.Items.Add(footer);
                footer.Focusable = false;
            }
            UpdateHintTextVisibility(HintText);
        }

        int GetViewIndexForItem(int itemIndex)
        {
            return 2 + itemIndex * 2;
        }

        int GetViewIndexForSpacer(int spacerIndex)
        {
            return 2 + spacerIndex * 2 + 1;
        }

        int GetSpacerIndex(int viewIndex)
        {
            if (viewIndex == 1)
            {
                return 0;
            }
            else
            {
                return (viewIndex - 3) / 2 + 1;
            }
        }

        void UpdateHintTextVisibility(string hintText)
        {
            if (this.hintTextGrid != null && this.Items != null)
            {
                this.hintTextGrid.Visibility = (this.Items.Count == 0 && !string.IsNullOrEmpty(hintText)) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private IList<object> GetOrderMetaData(List<ModelItem> items)
        {
            List<ModelItem> sortedList = this.SortSelectedItems(new List<ModelItem>(items));
            this.CheckListConsistentAndThrow(items, sortedList);
            return sortedList.Select((m) => m.GetCurrentValue()).ToList();
        }

        private FrameworkElement CreateSpacer()
        {
            FrameworkElement spacer = (this.SpacerTemplate != null) ? (FrameworkElement)this.SpacerTemplate.LoadContent() : new Rectangle();
            spacer.IsHitTestVisible = true;
            Control spacerControl = spacer as Control;
            if (spacerControl != null)
            {
                spacerControl.IsTabStop = true;
            }
            spacer.Drop += new DragEventHandler(OnSpacerDrop);
            spacer.LostFocus += new RoutedEventHandler(OnSpacerLostFocus);
            spacer.GotFocus += new RoutedEventHandler(OnSpacerGotFocus);
            spacer.GotKeyboardFocus += new KeyboardFocusChangedEventHandler(OnSpacerGotKeyboardFocus);
            spacer.MouseDown += new MouseButtonEventHandler(OnSpacerMouseDown);
            return spacer;
        }

        void OnSpacerDrop(object sender, DragEventArgs e)
        {
            int index = GetSpacerIndexFromView(sender);
            OnItemsDropped(e, index);
        }

        private int GetSpacerIndexFromView(object sender)
        {
            if (((DependencyObject)sender).ReadLocalValue(IndexProperty) != DependencyProperty.UnsetValue)
            {
                int index = (int)((DependencyObject)sender).GetValue(IndexProperty);
                return index;
            }
            else
            {
                return GetSpacerIndex(this.panel.Items.IndexOf(sender));
            }
        }

        void OnSpacerGotFocus(object sender, RoutedEventArgs e)
        {
            int index = GetSpacerIndexFromView(sender);
            selectedSpacerIndex = index;
        }

        void OnSpacerGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            // Handle the event so that it won't be routed to the containing designer to affect selection
            if (handleSpacerGotKeyboardFocus)
            {
                e.Handled = true;
            }
        }

        void OnSpacerLostFocus(object sender, RoutedEventArgs e)
        {
            int index = GetSpacerIndexFromView(sender);
            selectedSpacerIndex = addAtEndMarker;
        }

        void OnSpacerMouseDown(object sender, MouseButtonEventArgs e)
        {
            // do not move focus if it's a ctrl right click.
            if (e.RightButton == MouseButtonState.Pressed)
            {
                if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                {
                    return;
                }
            }

            // Schedule the Keyboard.Focus command to let it execute later than WorkflowViewElement.OnMouseDown
            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
            {
                this.handleSpacerGotKeyboardFocus = true;
                Keyboard.Focus((FrameworkElement)sender);
                this.handleSpacerGotKeyboardFocus = false;
            }));
        }

        private bool ShouldMoveItems(List<ModelItem> sortedModelItems, int index)
        {
            if (sortedModelItems.Count == 0)
            {
                return false;
            }

            // Should move if the items are not next to each other
            if (!AreItemsConsecutive(sortedModelItems))
            {
                return true;
            }

            // Should not move if the new position is just before the first item or just after the last item or between them.
            return index < this.Items.IndexOf(sortedModelItems[0]) || index > this.Items.IndexOf(sortedModelItems.Last()) + 1;
        }

        private bool AreItemsConsecutive(List<ModelItem> sortedModelItems)
        {
            Fx.Assert(sortedModelItems.Count > 0, "Should have at least one item.");
            int oldIndex = this.Items.IndexOf(sortedModelItems[0]);
            foreach (ModelItem item in sortedModelItems)
            {
                if (oldIndex != this.Items.IndexOf(item))
                {
                    return false;
                }
                oldIndex++;
            }
            return true;
        }

        public List<ModelItem> SortSelectedItems(List<ModelItem> selectedItems)
        {
            if (selectedItems == null)
            {
                throw FxTrace.Exception.ArgumentNull("selectedItems");
            }
            if (selectedItems.Count < 2)
            {
                return selectedItems;
            }

            List<ModelItem> list = new List<ModelItem>();
            // If the performance here is bad, we can use HashSet for selectedItems
            // to improve
            foreach (ModelItem item in this.Items)
            {
                int index = selectedItems.IndexOf(item);
                if (index >= 0)
                {
                    // use the reference in selectedItems.
                    list.Add(selectedItems[index]);
                }
            }

            // in case passing some items that are not in
            // my container.
            if (list.Count != selectedItems.Count)
            {
                // throw FxTrace.Exception.
                throw FxTrace.Exception.AsError(new ArgumentException(SR.Error_CantFindItemInWIsP));
            }
            return list;
        }

        public void OnItemsMoved(List<ModelItem> movedItems)
        {
            if (movedItems == null)
            {
                throw FxTrace.Exception.ArgumentNull("movedItems");
            }

            DragDropHelper.ValidateItemsAreOnView(movedItems, this.Items);
            this.OnItemsDelete(movedItems);
        }

        void OnItemsDropped(DragEventArgs e, int index)
        {
            ModelItemHelper.TryCreateImmediateEditingScopeAndExecute(this.Items.GetEditingContext(), System.Activities.Presentation.SR.CollectionAddEditingScopeDescription, (es) =>
            {
                DragDropHelper.SetDragDropCompletedEffects(e, DragDropEffects.None);
                List<object> droppedObjects = new List<object>(DragDropHelper.GetDroppedObjects(this, e, Context));
                List<WorkflowViewElement> movedViewElements = new List<WorkflowViewElement>();

                List<object> externalMoveList = new List<object>();
                List<ModelItem> internalMoveList = new List<ModelItem>();

                // Step 1: Sort the list
                List<object> sortedDroppingList = DragDropHelper.SortSelectedObjects(droppedObjects);


                // Step 2: Categorize dropped objects by their source container.
                foreach (object droppedObject in sortedDroppingList)
                {
                    ModelItem modelItem = droppedObject as ModelItem;
                    WorkflowViewElement view = (modelItem == null) ? null : (modelItem.View as WorkflowViewElement);
                    if (view == null)
                    {
                        externalMoveList.Add(droppedObject);
                        continue;
                    }
                    UIElement container = DragDropHelper.GetCompositeView(view);
                    if (container == this)
                    {
                        internalMoveList.Add(modelItem);
                        continue;
                    }
                    movedViewElements.Add(view);
                    externalMoveList.Add(droppedObject);
                }

                // Step 3: Internal movement
                if (this.ShouldMoveItems(internalMoveList, index))
                {
                    foreach (ModelItem modelItem in internalMoveList)
                    {
                        int oldIndex = this.Items.IndexOf(modelItem);
                        this.Items.Remove(modelItem);

                        //if element is placed ahead of old location, decrement the index not to include moved object
                        if (oldIndex < index)
                        {
                            this.InsertItem(index - 1, modelItem);
                        }
                        else
                        {
                            this.InsertItem(index, modelItem);
                            index++;
                        }
                    }
                }

                // Step 4: External move and drop from toolbox
                foreach (object droppedObject in externalMoveList)
                {
                    if (!this.IsDropAllowed(droppedObject))
                    {
                        continue;
                    }
                    this.InsertItem(index++, droppedObject);
                    DragDropHelper.SetDragDropCompletedEffects(e, DragDropEffects.Move);
                }
                DragDropHelper.SetDragDropMovedViewElements(e, movedViewElements);
                e.Handled = true;
                if (es != null)
                {
                    es.Complete();
                }
            });
        }

        private void CheckListConsistentAndThrow(List<ModelItem> src, List<ModelItem> copied)
        {
            bool valid = DragDropHelper.AreListsIdenticalExceptOrder(src, copied);
            if (!valid)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.Error_BadOutputFromSortSelectedItems));
            }
        }

        private bool IsDropAllowed(object droppedObject)
        {
            bool isDropAllowed = false;
            ModelItem modelItem = droppedObject as ModelItem;
            if (modelItem != null && !IsInParentChain(modelItem))
            {
                if (this.AllowedItemType.IsAssignableFrom(modelItem.ItemType))
                {
                    isDropAllowed = true;
                }
            }
            else if (droppedObject is Type && this.AllowedItemType.IsAssignableFrom((Type)droppedObject))
            {
                isDropAllowed = true;
            }
            else
            {
                if (this.AllowedItemType.IsAssignableFrom(droppedObject.GetType()))
                {
                    isDropAllowed = true;
                }
            }
            return isDropAllowed;
        }

        private bool IsInParentChain(ModelItem droppedModelItem)
        {
            bool isInParentChain = false;
            ModelItem parentModelItem = this.Items;
            while (parentModelItem != null)
            {
                if (parentModelItem == droppedModelItem)
                {
                    isInParentChain = true;
                    break;
                }
                parentModelItem = parentModelItem.Parent;
            }
            return isInParentChain;
        }

        void InsertItem(int index, object droppedObject)
        {
            ModelItem insertedItem = null;
            if (index == addAtEndMarker)
            {
                insertedItem = this.Items.Add(droppedObject);
            }
            else
            {
                insertedItem = this.Items.Insert(index, droppedObject);
            }
            if (insertedItem != null)
            {
                Selection.SelectOnly(this.Context, insertedItem);
            }
        }

        protected override void OnDrop(DragEventArgs e)
        {
            int index = addAtEndMarker;
            WorkflowViewElement dropTarget = null;
            if (e.OriginalSource is WorkflowViewElement)
            {
                dropTarget = (WorkflowViewElement)e.OriginalSource;
            }
            else
            {
                dropTarget = VisualTreeUtils.FindFocusableParent<WorkflowViewElement>((UIElement)e.OriginalSource);
            }

            if (null != dropTarget && null != dropTarget.ModelItem)
            {
                int targetIndex = this.Items.IndexOf(dropTarget.ModelItem);
                if (-1 != targetIndex)
                {
                    index = targetIndex + 1;
                }
            }
            OnItemsDropped(e, index);
            base.OnDrop(e);
        }

        void OnDrag(DragEventArgs e)
        {
            if (!e.Handled)
            {
                if (!DragDropHelper.AllowDrop(e.Data, this.Context, this.AllowedItemType))
                {
                    e.Effects = DragDropEffects.None;
                }
                e.Handled = true;
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


        public void OnItemMoved(ModelItem modelItem)
        {
            if (this.Items.Contains(modelItem))
            {
                this.Items.Remove(modelItem);
            }
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new WorkflowItemsPresenterAutomationPeer(this);
        }

        public object OnItemsCut(List<ModelItem> itemsToCut)
        {
            List<object> orderMetaData = GetOrderMetaData(itemsToCut).ToList();
            foreach (ModelItem item in itemsToCut)
            {
                this.Items.Remove(item);
                this.Context.Items.SetValue(new Selection(new ArrayList()));
            }
            return orderMetaData;
        }
       
        public object OnItemsCopied(List<ModelItem> itemsToCopy)
        {
            return this.GetOrderMetaData(itemsToCopy);
        }

        public void OnItemsPasted(List<object> itemsToPaste, List<object> metaData, Point pastePoint, WorkflowViewElement pastePointReference)
        {
            // first see if a spacer is selected.
            int index = this.selectedSpacerIndex;
            // else see if we can paste after a selected child
            if (index < 0)
            {
                Selection currentSelection = this.Context.Items.GetValue<Selection>();
                index = this.Items.IndexOf(currentSelection.PrimarySelection);
                //paste after the selected child
                if (index >= 0)
                {
                    index++;
                }
            }
            if (index < 0)
            {
                index = addAtEndMarker;
            }

            IList<object> mergedItemsToPaste = CutCopyPasteHelper.SortFromMetaData(itemsToPaste, metaData);

            List<ModelItem> modelItemsToSelect = new List<ModelItem>();

            foreach (object itemToPaste in mergedItemsToPaste)
            {
                if (IsDropAllowed(itemToPaste))
                {
                    if (index == addAtEndMarker)
                    {
                        modelItemsToSelect.Add(this.Items.Add(itemToPaste));
                    }
                    else
                    {
                        modelItemsToSelect.Add(this.Items.Insert(index, itemToPaste));
                    }
                    if (index >= 0)
                    {
                        index++;
                    }
                }
            }

            this.Dispatcher.BeginInvoke(
                new Action(() =>
                {
                    this.Context.Items.SetValue(new Selection(modelItemsToSelect));
                }),
                Windows.Threading.DispatcherPriority.ApplicationIdle,
                null);
        }

        public void OnItemsDelete(List<ModelItem> itemsToDelete)
        {
            if (null != itemsToDelete)
            {
                itemsToDelete.ForEach(p =>
                {
                    if (null != this.Items && this.Items.Contains(p))
                    {
                        this.Items.Remove(p);
                    }
                }
                    );
            }
        }

        public bool CanPasteItems(List<object> itemsToPaste)
        {
            bool result = false;
            if (null != itemsToPaste && itemsToPaste.Count > 0)
            {
                result = itemsToPaste.All(p => this.IsDropAllowed(p));
            }
            return result;
        }

        class WorkflowItemsPresenterAutomationPeer : UIElementAutomationPeer
        {
            WorkflowItemsPresenter owner;

            public WorkflowItemsPresenterAutomationPeer(WorkflowItemsPresenter owner)
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
                string automationId = base.GetAutomationIdCore();
                if (string.IsNullOrEmpty(automationId))
                {
                    automationId = base.GetNameCore();
                    if (string.IsNullOrEmpty(automationId))
                    {
                        automationId = this.owner.GetType().Name;
                    }
                }
                return automationId;
            }

            protected override string GetNameCore()
            {
                // Return an empty string if some activites are dropped on the presenter
                if (owner.Items.Count > 0)
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

    }
}
