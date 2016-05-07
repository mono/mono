//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation.View.TreeView
{
    using System;
    using System.Activities.Presentation.Internal.PropertyEditing;
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.View.OutlineView;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Globalization;
    using System.Linq;
    using System.Runtime;
    using System.Windows.Media;

    class TreeViewItemViewModel : INotifyPropertyChanged
    {
        string nodePrefixText = string.Empty;
        bool isExpanded = false;
        bool isHighlighted = false;
        DrawingBrush icon = null;
        HashSet<ModelItem> uniqueChildren = null;
        internal static TreeViewItemViewModel DummyNode = new TreeViewItemViewModel();
        // the IconCache is a static singleton based on the assumption that the Icon for a particular type is 
        // the same within an AppDomain
        internal static Dictionary<Type, DrawingBrush> IconCache = new Dictionary<Type, DrawingBrush>();
        DesignerPerfEventProvider provider = null;

        public event PropertyChangedEventHandler PropertyChanged;
        protected TreeViewItemViewModel()
        {
            this.IsAlive = true;
            InternalChildren = new ObservableCollection<TreeViewItemViewModel>();
            InternalChildren.CollectionChanged += new Collections.Specialized.NotifyCollectionChangedEventHandler(InternalChildren_CollectionChanged);
            Children = new ReadOnlyObservableCollection<TreeViewItemViewModel>(InternalChildren);
            ChildrenValueCache = new HashSet<object>();
            DuplicatedNodeVisible = true;

            this.Trackers = new Dictionary<ModelProperty, ChangeNotificationTracker>();
        }

        internal ObservableCollection<TreeViewItemViewModel> InternalChildren { get; private set; }

        public ReadOnlyObservableCollection<TreeViewItemViewModel> Children { get; private set; }

        public string NodePrefixText
        {
            get
            {
                return nodePrefixText;
            }
            set
            {
                nodePrefixText = value;
                this.NotifyPropertyChanged("NodePrefixText");
            }
        }

        public DrawingBrush Icon
        {
            get { return icon; }
            set
            {
                if (value != this.icon)
                {
                    this.icon = value;
                    this.NotifyPropertyChanged("Icon");
                }
            }
        }

        internal bool IsAlive { get; private set; }

        public bool IsExpanded
        {
            get
            {
                return isExpanded;
            }
            set
            {
                if (value != this.isExpanded)
                {
                    if (this.PerfEventProvider != null)
                    {
                        this.PerfEventProvider.DesignerTreeViewExpandStart();
                    }
                    this.isExpanded = value;
                    if (this.isExpanded && this.Children.Count == 1 && this.Children[0] == DummyNode)
                    {
                        this.InternalChildren.Remove(DummyNode);
                        this.LoadChildren();
                    }
                    this.NotifyPropertyChanged("IsExpanded");
                    if (this.PerfEventProvider != null)
                    {
                        this.PerfEventProvider.DesignerTreeViewExpandEnd();
                    }
                }
            }
        }

        public bool IsHighlighted
        {
            get
            {
                return this.isHighlighted;
            }
            set
            {
                if (this.isHighlighted != value)
                {
                    this.isHighlighted = value;
                    this.NotifyPropertyChanged("IsHighlighted");
                }
            }
        }

        internal bool DuplicatedNodeVisible { get; set; }

        internal TreeViewItemState State { get; set; }

        internal bool HasChildren
        {
            get
            {
                return (this.State & TreeViewItemState.HasChildren) == TreeViewItemState.HasChildren;
            }
        }

        internal bool HasSibling
        {
            get
            {
                return (this.State & TreeViewItemState.HasSibling) == TreeViewItemState.HasSibling;
            }
        }

        public TreeViewItemViewModel Parent { get; set; }

        internal ITreeViewItemSelectionHandler TreeViewItem { get; set; }

        protected HashSet<object> ChildrenValueCache { get; set; }

        internal Dictionary<ModelProperty, ChangeNotificationTracker> Trackers { get; private set; }

        protected DesignerPerfEventProvider PerfEventProvider
        {
            get
            {
                if (provider == null)
                {
                    EditingContext context = this.GetEditingContext();
                    if (context != null)
                    {
                        provider = context.Services.GetService<DesignerPerfEventProvider>();
                    }
                }
                return provider;
            }
        }

        protected virtual EditingContext GetEditingContext()
        {
            return null;
        }

        internal virtual object GetValue()
        {
            return null;
        }

        internal virtual void LoadChildren()
        {
            foreach (ChangeNotificationTracker t in this.Trackers.Values)
            {
                t.CleanUp();
            }

            foreach (TreeViewItemViewModel child in InternalChildren)
            {
                child.CleanUp();
            }

            this.InternalChildren.Clear();
            this.ChildrenValueCache.Clear();
        }

        internal virtual void UpdateChildren(ChangeNotificationTracker tracker, EventArgs e)
        {
            this.uniqueChildren = null;
        }

        //if child is null then only add the modelProperty for tracking purpose
        internal virtual void AddChild(TreeViewItemViewModel child, ModelProperty modelProperty)
        {
            //check for duplicate first
            if (child != null)
            {
                object childValue = child.GetValue();
                if (!ChildrenValueCache.Contains(childValue))
                {
                    ChildrenValueCache.Add(childValue);
                }
                else
                {
                    child.CleanUp();
                    return;
                }
            }

            ChangeNotificationTracker tracker = GetTracker(modelProperty);
            if (child != null)
            {
                // May be adding a node before it's expanded; get rid of the dummy
                if (this.Children.Count == 1 && this.Children[0] == DummyNode)
                {
                    this.InternalChildren.Remove(DummyNode);
                }

                int insertIndex = this.FindInsertionIndex(tracker);
                this.InternalChildren.Insert(insertIndex, child);
                tracker.ChildViewModels.Add(child);
                if (child.HasSibling)
                {
                    //loading children rather than just add the sibling of the children
                    //if this turn out to be a big performance impact then we'll need to optimise this
                    child.LoadChildren();
                }
            }
        }

        internal static TreeViewItemViewModel CreateViewModel(TreeViewItemViewModel parent, object value)
        {
            TreeViewItemViewModel viewModel = null;
            if (typeof(ModelItem).IsAssignableFrom(value.GetType()))
            {
                viewModel = new TreeViewItemModelItemViewModel(parent, value as ModelItem);
            }
            else if (typeof(ModelProperty).IsAssignableFrom(value.GetType()))
            {
                viewModel = new TreeViewItemModelPropertyViewModel(parent, value as ModelProperty);
            }
            else if (typeof(KeyValuePair<ModelItem, ModelItem>).IsAssignableFrom(value.GetType()))
            {
                viewModel = new TreeViewItemKeyValuePairModelItemViewModel(parent, (KeyValuePair<ModelItem, ModelItem>)value);
            }
            return viewModel;
        }

        internal static void AddModelItem(TreeViewItemViewModel parent, ModelItem item, ModelProperty trackingProperty)
        {
            if (item != null)
            {
                bool updateTrackingProperty = trackingProperty == null;

                foreach (ModelProperty property in item.Properties)
                {
                    if (updateTrackingProperty)
                    {
                        trackingProperty = property;
                    }
                    AddModelProperty(parent, item, trackingProperty, property);
                }
            }
        }

        internal static void AddModelItemCollection(TreeViewItemViewModel parent, ModelItemCollection collection, ModelProperty trackingProperty)
        {
            parent.GetTracker(trackingProperty).AddCollection(collection);

            bool duplicatedNodeVisible = true;
            string childNodePrefix = string.Empty;
            ShowPropertyInOutlineViewAttribute viewChild = ExtensibilityAccessor.GetAttribute<ShowPropertyInOutlineViewAttribute>(trackingProperty);
            if (viewChild != null)
            {
                duplicatedNodeVisible = viewChild.DuplicatedChildNodesVisible;
                childNodePrefix = viewChild.ChildNodePrefix;
            }

            foreach (ModelItem item in collection)
            {
                AddChild(parent, item, item, duplicatedNodeVisible, childNodePrefix, trackingProperty);
            }
        }

        internal static void AddModelItemDictionary(TreeViewItemViewModel parent, ModelItemDictionary dictionary, ModelProperty trackingProperty)
        {
            parent.GetTracker(trackingProperty).AddCollection(dictionary);

            bool duplicatedNodeVisible = true;
            string childNodePrefix = string.Empty;
            ShowPropertyInOutlineViewAttribute viewChild = ExtensibilityAccessor.GetAttribute<ShowPropertyInOutlineViewAttribute>(trackingProperty);
            if (viewChild != null)
            {
                duplicatedNodeVisible = viewChild.DuplicatedChildNodesVisible;
                childNodePrefix = viewChild.ChildNodePrefix;
            }

            foreach (var pair in dictionary)
            {
                ModelItem item = null;
                //AddChild(parent, pair.Value, pair, duplicatedNodeVisible, trackingProperty);
                AddChild(parent, item, pair, duplicatedNodeVisible, childNodePrefix, trackingProperty);
            }
        }

        internal static void AddModelProperty(TreeViewItemViewModel parent, ModelItem item, ModelProperty trackingProperty, ModelProperty property)
        {
            //in the case of multiple attributes, they go in this order
            //HidePropertyInOutlineViewAttribute
            //[item.ShowInOutlineViewAttribute.PromotedProperty = property.Name]. Set VisualValue by property and ignore itself. Usage ActivityDelegate, FlowStep.
            //ShowPropertyInOutlineViewAttribute
            //ShowPropertyInOutlineViewAsSiblingAttribute
            //ShowInOutlineViewAttribute
            ShowPropertyInOutlineViewAttribute viewChild = ExtensibilityAccessor.GetAttribute<ShowPropertyInOutlineViewAttribute>(property);
            if (ExtensibilityAccessor.GetAttribute<HidePropertyInOutlineViewAttribute>(property) != null)
            {
                //ignore
                return;
            }
            else if (IsPromotedProperty(item, property))
            {
                if (property.IsCollection)
                {
                    ModelItemCollection mc = property.Value as ModelItemCollection;
                    AddModelItemCollection(parent, mc, trackingProperty);
                }
                else if (property.IsDictionary)
                {
                    ModelItemDictionary dictionary = property.Dictionary;
                    AddModelItemDictionary(parent, dictionary, trackingProperty);
                }
                else
                {
                    parent.GetTracker(trackingProperty).Add(item, property);

                    //if property.Value is null, then this would not add any node
                    // Use promoted ModelItem's property to track, so pass null to AddModelItem method.
                    AddModelItem(parent, property.Value, null);
                }
            }
            else if (viewChild != null)
            {
                if (viewChild.CurrentPropertyVisible) //property node visible
                {
                    if (property.Value != null)
                    {
                        TreeViewItemViewModel childModel = TreeViewItemViewModel.CreateViewModel(parent, property);
                        childModel.DuplicatedNodeVisible = viewChild.DuplicatedChildNodesVisible;
                        parent.AddChild(childModel, trackingProperty);
                    }
                    else
                    {
                        //just add the notification tracker without adding the empty child
                        parent.GetTracker(trackingProperty, true).Add(item, trackingProperty);
                    }
                }
                else
                {
                    if (property.IsCollection)
                    {
                        ModelItemCollection mc = property.Value as ModelItemCollection;
                        AddModelItemCollection(parent, mc, trackingProperty);
                    }
                    else if (property.IsDictionary)
                    {
                        ModelItemDictionary dictionary = property.Dictionary;
                        AddModelItemDictionary(parent, dictionary, trackingProperty);
                    }
                    else
                    {
                        if (property.Value != null)
                        {
                            TreeViewItemViewModel childModel = TreeViewItemViewModel.CreateViewModel(parent, property.Value);
                            childModel.DuplicatedNodeVisible = viewChild.DuplicatedChildNodesVisible;
                            parent.AddChild(childModel, trackingProperty);
                        }
                        else
                        {
                            parent.GetTracker(trackingProperty).Add(item, property);
                        }

                    }
                }
            }
            else if (ExtensibilityAccessor.GetAttribute<ShowPropertyInOutlineViewAsSiblingAttribute>(property) != null)
            {
                //add notification to the tracker that is responsible for this node
                ChangeNotificationTracker tracker = parent.Parent.GetTracker(parent);
                tracker.Add(item, property);
                TreeViewItemViewModel siblingNode = null;
                if (property.Value != null)
                {
                    siblingNode = TreeViewItemViewModel.CreateViewModel(parent.Parent, property.Value);
                }
                parent.Parent.AddChild(siblingNode, tracker.ParentProperty);
            }
            else if (ExtensibilityAccessor.GetAttribute<ShowInOutlineViewAttribute>(property) != null)
            {
                if (property.Value != null)
                {
                    ShowInOutlineViewAttribute outlineView = ExtensibilityAccessor.GetAttribute<ShowInOutlineViewAttribute>(property);
                    if (string.IsNullOrWhiteSpace(outlineView.PromotedProperty))
                    {
                        parent.AddChild(TreeViewItemViewModel.CreateViewModel(parent, property), trackingProperty);
                    }
                    else
                    {
                        ModelProperty promotedProperty = property.Value.Properties.Find(outlineView.PromotedProperty);
                        if (promotedProperty == null)
                        {
                            throw FxTrace.Exception.AsError(new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, SR.PromotedPropertyNotFound, outlineView.PromotedProperty, property.Value.Name)));
                        }

                        // Add promoted ModelItem and property into tracker. So when property got changed, the grandparent of promoted ModelItem will be notified.
                        ChangeNotificationTracker tracker = parent.GetTracker(trackingProperty, true);
                        tracker.Add(property.Value, promotedProperty);

                        if (promotedProperty.Value == null)
                        {
                            tracker.Add(item, property);
                        }
                        else
                        {
                            parent.AddChild(TreeViewItemViewModel.CreateViewModel(parent, property), trackingProperty);
                        }
                    }
                }
                else
                {
                    parent.GetTracker(trackingProperty, true).Add(item, property);
                }

            }
            //if the values in the dictionary is viewvisible, note this only works with generic dictionary
            else if (property.IsDictionary && property.PropertyType.IsGenericType)
            {
                Type[] arguments = property.PropertyType.GetGenericArguments();
                if (ExtensibilityAccessor.GetAttribute<ShowInOutlineViewAttribute>(arguments[1]) != null)
                {
                    if (property.Value != null)
                    {
                        parent.AddChild(TreeViewItemViewModel.CreateViewModel(parent, property), trackingProperty);
                    }
                    else
                    {
                        parent.GetTracker(trackingProperty, true).Add(item, property);
                    }

                }
            }
        }

        internal static bool IsPromotedProperty(ModelItem modelItem, ModelProperty property)
        {
            return IsPromotedProperty(modelItem, property.Name);
        }

        internal static bool IsPromotedProperty(ModelItem modelItem, string propertyName)
        {
            ShowInOutlineViewAttribute attr = ExtensibilityAccessor.GetAttribute<ShowInOutlineViewAttribute>(modelItem);
            if (attr != null && !string.IsNullOrEmpty(attr.PromotedProperty))
            {
                return string.Equals(propertyName, attr.PromotedProperty, StringComparison.Ordinal);
            }
            return false;
        }

        internal static void AddChild(TreeViewItemViewModel parent, ModelItem item, object value, bool duplicatedNodeVisible, string childNodePrefix, ModelProperty trackingProperty)
        {
            // If necessary, evaluate uniqueness of given item
            bool isUnique = false;
            if (!duplicatedNodeVisible)
            {
                // Note: These evaluations expect item to be an immediate child of trackingProperty.Value
                // Intermediate nodes would for example undermine simple check just below
                //
                // Caveat 1: Aim is to greatly reduce, not to eliminate, display of nodes visible elsewhere
                // Caveat 1a: Nodes reachable from other isolated nodes are included in the collection
                // Caveat 1b: Nodes that are not isolated may be reachable from isolated nodes and thus
                // displayed together with the isolated ones; ShowPropertyInOutlineViewAsSiblingAttribute may make this seem normal
                // (If complete duplicate elimination were the aim, would likely need a "never expand"
                // display mode for duplicateNodeVisible=false children)
                // Caveat 2: Use of single uniqueChildren field may cause all children of a second
                // duplcatedNodeVisible=false property to be ignored if (fortunately only if) neither
                // property uses ShowPropertyInOutlineViewAttribute(true) -- that attribute's default
                // Caveat 3-n: Please see caveats described at top of UniqueModelItemHelper
                if (1 >= item.Parents.Count())
                {
                    isUnique = true;
                }
                else
                {
                    // Avoided a thorough evaluation as long as we can
                    if (null == parent.uniqueChildren)
                    {
                        parent.uniqueChildren = UniqueModelItemHelper.FindUniqueChildren(trackingProperty);
                    }

                    isUnique = parent.uniqueChildren.Contains(item);
                }
            }

            // If displayable now, create the view model node
            if (duplicatedNodeVisible || isUnique)
            {
                TreeViewItemViewModel child = TreeViewItemViewModel.CreateViewModel(parent, value);
                child.NodePrefixText = childNodePrefix;
                parent.AddChild(child, trackingProperty);
            }
            // Track for potential addition or removal of parents even if not presently visible
            if (!duplicatedNodeVisible)
            {
                ModelItemImpl itemImpl = item as ModelItemImpl;
                if (null != itemImpl)
                {
                    ChangeNotificationTracker tracker = parent.GetTracker(trackingProperty);
                    tracker.AddCollection(itemImpl.InternalParents);
                    tracker.AddCollection(itemImpl.InternalSources);
                }
            }
        }

        internal ChangeNotificationTracker GetTracker(ModelProperty modelProperty)
        {
            return GetTracker(modelProperty, true);
        }

        internal virtual ChangeNotificationTracker GetTracker(ModelProperty modelProperty, bool createNew)
        {
            ChangeNotificationTracker tracker = null;
            if (!this.Trackers.TryGetValue(modelProperty, out tracker) && createNew)
            {
                tracker = new ChangeNotificationTracker(this, modelProperty);
                Trackers.Add(modelProperty, tracker);
            }
            return tracker;
        }

        internal ChangeNotificationTracker GetTracker(TreeViewItemViewModel child)
        {
            ChangeNotificationTracker trackerForChild = null;
            foreach (ChangeNotificationTracker tracker in this.Trackers.Values)
            {
                if (tracker.ChildViewModels.Contains(child))
                {
                    trackerForChild = tracker;
                    break;
                }
            }
            Fx.Assert(trackerForChild != null, "Tracker should not be null");
            return trackerForChild;
        }

        internal virtual int FindInsertionIndex(ChangeNotificationTracker tracker)
        {
            int insertIndex = 0;
            if (tracker != null && tracker.ChildViewModels != null && tracker.ChildViewModels.Count > 0)
            {
                //assume the childViewModels are in order
                insertIndex = this.InternalChildren.IndexOf(tracker.ChildViewModels.Last()) + 1;
            }
            return insertIndex;
        }

        internal ModelProperty GetTrackingModelPropertyForChild(TreeViewItemViewModel child)
        {
            ModelProperty property = null;
            foreach (ChangeNotificationTracker tracker in this.Trackers.Values)
            {
                if (tracker.ChildViewModels.Contains(child))
                {
                    property = tracker.ParentProperty;
                    break;
                }
            }
            return property;
        }

        internal virtual void UpdateState()
        {
        }

        void InternalChildren_CollectionChanged(object sender, Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                foreach (TreeViewItemViewModel item in e.OldItems)
                {
                    this.ChildrenValueCache.Remove(item.GetValue());
                }
            }
        }

        protected void NotifyPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        [Flags]
        internal enum TreeViewItemState
        {
            Default = 0,
            HasChildren = 1,
            HasSibling = 2
        }

        // <summary>
        // Disposes this editing context.
        // </summary>
        public void CleanUp()
        {
            if (this.IsAlive)
            {
                CleanUpCore();
                this.IsAlive = false;
            }
        }

        protected virtual void CleanUpCore()
        {
            foreach (ChangeNotificationTracker t in this.Trackers.Values)
            {
                t.CleanUp();
            }

            foreach (TreeViewItemViewModel child in InternalChildren)
            {
                child.CleanUp();
            }

            this.InternalChildren.CollectionChanged -= InternalChildren_CollectionChanged;
            this.Trackers = null;
            this.InternalChildren = null;
            this.ChildrenValueCache = null;
            this.Children = null;
            this.icon = null;
            this.TreeViewItem = null;
            this.Parent = null;
        }
    }

    internal class TreeViewItemViewModel<T> : TreeViewItemViewModel
    {
        private T visualValue;
        //this is the value the UI tree bind to
        public virtual T VisualValue
        {
            get
            {
                return visualValue;
            }
            internal set
            {
                if (!Equals(visualValue, value))
                {
                    visualValue = value;
                    NotifyPropertyChanged("VisualValue");
                }
            }
        }
        //this is for the view model processing
        public T Value { get; protected set; }

        public TreeViewItemViewModel(TreeViewItemViewModel parent)
        {
            this.Parent = parent;
        }

        public override string ToString()
        {
            if (Value != null)
            {
                return Value.ToString();
            }

            return base.ToString();
        }

        internal override object GetValue()
        {
            return this.Value;
        }

        protected override void CleanUpCore()
        {
            visualValue = default(T);
            Value = default(T);
            base.CleanUpCore();
        }
    }
}
