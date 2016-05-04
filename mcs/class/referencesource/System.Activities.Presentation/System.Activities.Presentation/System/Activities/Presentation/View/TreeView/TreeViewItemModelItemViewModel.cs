//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace System.Activities.Presentation.View.TreeView
{
    using System;
    using System.Activities.Presentation.Internal.PropertyEditing;
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.Services;
    using System.Activities.Presentation.Utility;
    using System.Activities.Presentation.View;
    using System.Activities.Presentation.View.OutlineView;
    using System.Activities.Statements;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;
    using System.Windows;
    using System.Windows.Media;

    internal sealed class TreeViewItemModelItemViewModel : TreeViewItemViewModel<ModelItem>
    {
        private ModelProperty promotedProperty;

        public TreeViewItemModelItemViewModel(TreeViewItemViewModel parent, ModelItem modelItem, bool lazyLoad)
            : base(parent)
        {
            this.Value = modelItem;
            ShowInOutlineViewAttribute attr = ExtensibilityAccessor.GetAttribute<ShowInOutlineViewAttribute>(modelItem);
            if (attr != null && !string.IsNullOrEmpty(attr.PromotedProperty))
            {
                // Only consider one level of promoted property.
                this.promotedProperty = modelItem.Properties[attr.PromotedProperty];
                if (this.promotedProperty != null)
                {
                    this.VisualValue = this.promotedProperty.Value;
                }
                else
                {
                    // is this what we really want?
                    Fx.Assert(attr.PromotedProperty + " not found on " + modelItem.Name);
                    this.VisualValue = null;
                }
            }
            else
            {
                this.VisualValue = modelItem;
            }

            IsHighlighted = Selection.IsSelection(this.VisualValue);

            if (lazyLoad)
            {
                this.UpdateState();
                if (this.HasChildren)
                {
                    this.InternalChildren.Add(TreeViewItemViewModel.DummyNode);
                }
            }
            else
            {
                this.LoadChildren();
            }
        }

        public TreeViewItemModelItemViewModel(TreeViewItemViewModel parent, ModelItem modelItem)
            : this(parent, modelItem, true)
        {
        }

        /// <summary>
        /// Set VisualValue will update Icon and binding to designer.
        /// </summary>
        public override ModelItem VisualValue
        {
            get
            {
                return base.VisualValue;
            }

            internal set
            {
                if (base.VisualValue != value)
                {
                    // Remove old event chain
                    if (base.VisualValue != null)
                    {
                        base.VisualValue.PropertyChanged -= this.VisualValue_PropertyChanged;
                    }

                    base.VisualValue = value;

                    if (ModelItemHasDesigner(base.VisualValue))
                    {
                        base.VisualValue.PropertyChanged += this.VisualValue_PropertyChanged;
                    }

                    this.Icon = this.GetIconByVisualValue();
                    NotifyPropertyChanged("VisualValue");
                }
            }
        }

        internal bool HasDesigner
        {
            get
            {
                return ModelItemHasDesigner(this.VisualValue);
            }
        }

        internal override void LoadChildren()
        {
            if (this.PerfEventProvider != null)
            {
                this.PerfEventProvider.DesignerTreeViewLoadChildrenStart();
            }

            base.LoadChildren();
            TreeViewItemViewModel.AddModelItem(this, this.Value, null);
            if (this.PerfEventProvider != null)
            {
                this.PerfEventProvider.DesignerTreeViewLoadChildrenEnd();
            }
        }

        internal override void UpdateChildren(ChangeNotificationTracker tracker, EventArgs e)
        {
            if (this.PerfEventProvider != null)
            {
                this.PerfEventProvider.DesignerTreeViewUpdateStart();
            }

            // Update VisualValue when promotedProperty's got changed.
            if (this.promotedProperty != null && this.promotedProperty == tracker.ParentProperty)
            {
                this.VisualValue = this.promotedProperty.Value;
            }

            if (this.Children.Count == 1 && this.Children[0] == DummyNode)
            {
                // If the node never expanded before, LoadChildren instead of UpdateChildren.
                // Otherwise, when expanding node, the LoadChildren method won't invoke 
                // Then other tracking properties cannot be setup correctly.
                this.InternalChildren.Remove(DummyNode);
                this.LoadChildren();
            }
            else
            {
                // If requireUpdateChildren = false, the related TreeViewItemModelPropertyViewModel take care of updating child nodes.
                bool requireUpdateChildren = true;
                if (e is PropertyChangedEventArgs && this.IsModelPropertyNodeExisted(tracker.ParentProperty))
                {
                    ModelProperty modelProperty = tracker.ParentProperty;
                    if (modelProperty.Value != null)
                    {
                        string changedPropertyName = ((PropertyChangedEventArgs)e).PropertyName;
                        bool isPromotedPropertyChanged = TreeViewItemViewModel.IsPromotedProperty(modelProperty.Value, changedPropertyName);
                        if (isPromotedPropertyChanged)
                        {
                            if (modelProperty.Value.Properties[changedPropertyName].Value != null)
                            {
                                requireUpdateChildren = false;
                            }
                        }
                        else
                        {
                            requireUpdateChildren = false;
                        }
                    }
                }

                if (requireUpdateChildren)
                {
                    base.UpdateChildren(tracker, e);
                    tracker.CleanUp();
                    TreeViewItemViewModel.AddModelProperty(this, this.Value, tracker.ParentProperty, tracker.ParentProperty);
                }
            }

            if (this.PerfEventProvider != null)
            {
                this.PerfEventProvider.DesignerTreeViewUpdateEnd();
            }
        }

        internal override void UpdateState()
        {
            base.UpdateState();
            if (this.Value != null)
            {
                this.State |= this.UpdateModelItemState(this.Value);
            }
        }

        internal override int FindInsertionIndex(ChangeNotificationTracker tracker)
        {
            int insertionIndex = 0;
            if (tracker != null && (tracker.ChildViewModels == null || tracker.ChildViewModels.Count < 1))
            {
                foreach (ModelProperty property in this.Value.Properties)
                {
                    if (property != tracker.ParentProperty)
                    {
                        // assume this would increament
                        ChangeNotificationTracker propertyTracker = this.GetTracker(property, false);
                        if (propertyTracker != null)
                        {
                            insertionIndex = base.FindInsertionIndex(propertyTracker);
                        }
                    }
                    else
                    {
                        // we've reach the property and hence the last of the previous property
                        break;
                    }
                }
            }
            else
            {
                insertionIndex = base.FindInsertionIndex(tracker);
            }

            return insertionIndex;
        }

        internal override ChangeNotificationTracker GetTracker(ModelProperty modelProperty, bool createNew)
        {
            ChangeNotificationTracker tracker = base.GetTracker(modelProperty, createNew);
            if (createNew)
            {
                if (this.VisualValue == modelProperty.Parent)
                {
                    // If this TreeViewModelItem use Promopted property and the property belongs to Promoted activity
                    // add the tracked property by default.
                    tracker.Add(this.VisualValue, modelProperty);
                }
                else
                {
                    // If it's an model item, add the tracked property by default
                    tracker.Add(this.Value, modelProperty);
                }
            }

            return tracker;
        }

        protected override void CleanUpCore()
        {
            if (this.VisualValue != null)
            {
                this.VisualValue.PropertyChanged -= this.VisualValue_PropertyChanged;
            }

            this.promotedProperty = null;

            base.CleanUpCore();
        }

        protected override EditingContext GetEditingContext()
        {
            if (this.Value != null)
            {
                return this.Value.GetEditingContext();
            }
            else
            {
                return base.GetEditingContext();
            }
        }

        private static bool ModelItemHasDesigner(ModelItem modelItem)
        {
            if (modelItem != null)
            {
                DesignerAttribute attribute = WorkflowViewService.GetAttribute<DesignerAttribute>(modelItem.ItemType);
                if (attribute != null && !string.IsNullOrEmpty(attribute.DesignerTypeName))
                {
                    return true;
                }
            }

            return false;
        }

        private static DrawingBrush GetIconFromUnInitializedDesigner(ActivityDesigner designer)
        {
            DrawingBrush icon = null;
            if (designer != null)
            {
                // force the designer to load
                designer.BeginInit();

                // An exception will be thrown, if BeginInit is called more than once on 
                // the same activity designer prior to EndInit being called.  So we call
                // EndInit to avoid that, note this will cause an Initialized event.
                designer.EndInit();

                if (designer.Icon == null)
                {
                    // the loading of the default icon depends on Activity.Loaded event.
                    // however the designer might not be loaded unless it is added to the
                    // designer surface.  So we load the default icon manually here.
                    icon = designer.GetDefaultIcon();
                }
                else
                {
                    icon = designer.Icon;
                }
            }

            return icon;
        }

        private void ExpandToNode()
        {
            TreeViewItemViewModel viewModel = this.Parent;
            while (viewModel != null)
            {
                viewModel.IsExpanded = true;
                viewModel = viewModel.Parent;
            }
        }

        private DrawingBrush GetIconByVisualValue()
        {
            if (this.VisualValue != null)
            {
                DrawingBrush icon = null;
                Type modelItemType = this.VisualValue.ItemType;
                if (modelItemType.IsGenericType)
                {
                    // If Type is generic type, whatever T, it should display same icon, so use generic type instead.
                    modelItemType = this.VisualValue.ItemType.GetGenericTypeDefinition();
                }

                // If the user specifies the attribute, then the Designer would be providing the icon,
                // bypassing the pipeline of retrieving the icons via reflection and attached properties.
                ActivityDesignerOptionsAttribute attr = ExtensibilityAccessor.GetAttribute<ActivityDesignerOptionsAttribute>(modelItemType);
                if (attr != null && attr.OutlineViewIconProvider != null)
                {
                    icon = attr.OutlineViewIconProvider(this.VisualValue);
                }

                if (icon == null && !TreeViewItemViewModel.IconCache.TryGetValue(modelItemType, out icon))
                {
                    EditingContext context = this.VisualValue.GetEditingContext();
                    ViewService service = context.Services.GetService<ViewService>();
                    WorkflowViewService workflowViewService = service as WorkflowViewService;
                    ActivityDesigner designer = null;

                    // first try to create an detached view element that won't participate in the designer,
                    // if the view service is WorkflowViewService
                    if (workflowViewService != null)
                    {
                        designer = workflowViewService.CreateDetachedViewElement(this.VisualValue) as ActivityDesigner;
                        icon = GetIconFromUnInitializedDesigner(designer);
                    }
                    else
                    {
                        // fall back if the view service is not the default implementation
                        // We only need to get the icon from the designer, so we don't need to make sure the view is parented.
                        designer = this.VisualValue.View as ActivityDesigner;
                        if (designer == null && service != null)
                        {
                            designer = service.GetView(this.VisualValue) as ActivityDesigner;
                        }

                        if (designer != null)
                        {
                            if (designer.Icon != null || designer.IsLoaded)
                            {
                                icon = designer.Icon;
                            }
                            else
                            {
                                icon = GetIconFromUnInitializedDesigner(designer);
                            }
                        }
                    }

                    // Cache even a null icon since answers found above won't change within this AppDomain
                    TreeViewItemViewModel.IconCache.Add(modelItemType, icon);
                }

                return icon;
            }
            else
            {
                return null;
            }
        }

        private bool IsModelPropertyNodeExisted(ModelProperty property)
        {
            bool isModelPropertyNodeExisted = false;

            foreach (TreeViewItemViewModel viewModel in Children)
            {
                TreeViewItemModelPropertyViewModel modelPropertyViewModel = viewModel as TreeViewItemModelPropertyViewModel;
                if (modelPropertyViewModel != null)
                {
                    if (modelPropertyViewModel.Value == property)
                    {
                        isModelPropertyNodeExisted = true;
                        break;
                    }
                }
            }

            return isModelPropertyNodeExisted;
        }

        private TreeViewItemState UpdateModelItemState(ModelItem modelItem)
        {
            TreeViewItemState state = TreeViewItemState.Default;

            foreach (ModelProperty property in modelItem.Properties)
            {
                if (ExtensibilityAccessor.GetAttribute<HidePropertyInOutlineViewAttribute>(property) != null)
                {
                    continue;
                }
                else if (ExtensibilityAccessor.GetAttribute<ShowPropertyInOutlineViewAttribute>(property) != null)
                {
                    if (property.Value != null)
                    {
                        state |= TreeViewItemState.HasChildren;
                    }

                    // create the property change notification tracker
                    this.GetTracker(property, true);
                }
                else if (ExtensibilityAccessor.GetAttribute<ShowPropertyInOutlineViewAsSiblingAttribute>(property) != null)
                {
                    // First of all, ShowPropertyInOutlineViewAsSiblingAttribute property's tracker will be setup at the LoadChildren() time.
                    // The reason we cannot do it here is because during the constructor, this.Parent is null.
                    // If all other properties don't flag HasChildren, the current node won't be able to expand.
                    // So we cannot rely on expand operation to invoke LoadChildren() to setup tracker.
                    // TreeViewItemViewModel.AddChild(TreeViewItemViewModel, ModelProperty) will by default invoke LoadChildren() if the node HasSibling.
                    // So even if property's value == null, we should flag it HasSibling, and let LoadChildren invoked by default.
                    state |= TreeViewItemState.HasSibling;
                }
                else if (ExtensibilityAccessor.GetAttribute<ShowInOutlineViewAttribute>(property) != null)
                {
                    if (property.Value != null)
                    {
                        // Only consider one level of PromotedProperty.
                        if (property != this.promotedProperty)
                        {
                            state |= TreeViewItemState.HasChildren;
                        }
                        else
                        {
                            // Since the property has been promoted, need to check whether this property has children.
                            // If this promoted property.Value has children, those children should belong to this node.
                            state |= this.UpdateModelItemState(property.Value);
                        }
                    }

                    this.GetTracker(property, true);
                }
                else if (property.IsDictionary && property.PropertyType.IsGenericType)
                {
                    // if the values in the dictionary is viewvisible, note this only works with generic dictionary
                    Type[] arguments = property.PropertyType.GetGenericArguments();
                    if (ExtensibilityAccessor.GetAttribute<ShowInOutlineViewAttribute>(arguments[1]) != null)
                    {
                        if (property.Value != null)
                        {
                            state |= TreeViewItemState.HasChildren;
                        }

                        this.GetTracker(property, true);
                    }
                }
            }

            return state;
        }

        private void VisualValue_PropertyChanged(object sender, ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("IsSelection"))
            {
                this.IsHighlighted = Selection.IsSelection(this.VisualValue);
                if (this.IsHighlighted)
                {
                    this.ExpandToNode();
                }
            }
            else if (e.PropertyName.Equals("IsPrimarySelection"))
            {
                if (TreeViewItem == null)
                {
                    // TreeViewItem is not initialized at the first time of SelectionChanged by WorkflowViewElement.OnGotFocusEvent.
                    return;
                }

                bool isPrimarySelection = Selection.IsPrimarySelection(this.VisualValue);
                if (isPrimarySelection)
                {
                    TreeViewItem.Select();
                }
                else
                {
                    TreeViewItem.Unselect();
                }
            }
        }
    }
}
