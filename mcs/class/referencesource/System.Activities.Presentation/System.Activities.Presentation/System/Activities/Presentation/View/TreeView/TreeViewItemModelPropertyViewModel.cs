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
    using System.Activities.Presentation.View.OutlineView;
    using System.Runtime;

    internal sealed class TreeViewItemModelPropertyViewModel : TreeViewItemViewModel<ModelProperty>
    {
        public TreeViewItemModelPropertyViewModel(TreeViewItemViewModel parent, ModelProperty property)
            : base(parent)
        {
            Fx.Assert(property != null, "property cannot be null");
            this.Value = property;
            this.VisualValue = property;
            if (property != null && property.Parent != null)
            {
                this.GetTracker(property);
            }

            this.UpdateState();
            if (this.HasChildren)
            {
                this.InternalChildren.Add(TreeViewItemViewModel.DummyNode);
            }
        }

        internal override ChangeNotificationTracker GetTracker(ModelProperty modelProperty, bool createNew)
        {
            ChangeNotificationTracker tracker = base.GetTracker(modelProperty, createNew);
            if (createNew)
            {
                Fx.Assert(this.Value == modelProperty, "The modelProperty should be the same as this.Value.");
                tracker.Add(modelProperty.Parent, modelProperty);
                ShowInOutlineViewAttribute viewVisible = ExtensibilityAccessor.GetAttribute<ShowInOutlineViewAttribute>(modelProperty);
                if (viewVisible != null && !string.IsNullOrWhiteSpace(viewVisible.PromotedProperty))
                {
                    ModelProperty promotedProperty = modelProperty.Value.Properties.Find(viewVisible.PromotedProperty);
                    tracker.Add(promotedProperty.Parent, promotedProperty);
                }
            }

            return tracker;
        }

        internal override void UpdateChildren(ChangeNotificationTracker tracker, EventArgs e)
        {
            if (this.PerfEventProvider != null)
            {
                this.PerfEventProvider.DesignerTreeViewUpdateStart();
            }

            // 
            base.UpdateChildren(tracker, e);
            tracker.CleanUp();
            this.InternalChildren.Clear();

            this.LoadChildren();

            if (this.PerfEventProvider != null)
            {
                this.PerfEventProvider.DesignerTreeViewUpdateEnd();
            }
        }

        internal override void LoadChildren()
        {
            if (this.PerfEventProvider != null)
            {
                this.PerfEventProvider.DesignerTreeViewLoadChildrenStart();
            }

            base.LoadChildren();

            if (this.Value.IsCollection)
            {
                ModelItemCollection mc = this.Value.Value as ModelItemCollection;
                TreeViewItemViewModel.AddModelItemCollection(this, mc, this.Value);
            }
            else if (this.Value.IsDictionary)
            {
                ModelItemDictionary dictionary = this.Value.Dictionary;
                TreeViewItemViewModel.AddModelItemDictionary(this, dictionary, this.Value);
            }
            else if (this.Value.Value != null)
            {
                TreeViewItemViewModel.AddChild(this, this.Value.Value, this.Value.Value, this.DuplicatedNodeVisible, string.Empty, this.Value);
            }

            if (this.PerfEventProvider != null)
            {
                this.PerfEventProvider.DesignerTreeViewLoadChildrenEnd();
            }
        }

        internal override void UpdateState()
        {
            if (this.Value.Value != null || (this.Value.IsCollection && this.Value.Collection.Count > 0) ||
                (this.Value.IsDictionary && this.Value.Dictionary.Count > 0))
            {
                this.State = TreeViewItemState.HasChildren;
            }

            base.UpdateState();
        }

        protected override EditingContext GetEditingContext()
        {
            if (this.Value != null && this.Value.Parent != null)
            {
                return this.Value.Parent.GetEditingContext();
            }
            else
            {
                return base.GetEditingContext();
            }
        }
    }
}
