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
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    internal sealed class TreeViewItemKeyValuePairModelItemViewModel : TreeViewItemViewModel<KeyValuePair<ModelItem, ModelItem>>
    {
        public TreeViewItemKeyValuePairModelItemViewModel(TreeViewItemViewModel parent, KeyValuePair<ModelItem, ModelItem> value) : base(parent)
        {
            this.Value = value;
            this.VisualValue = value;
            this.UpdateState();
            if (this.HasChildren)
            {
                this.InternalChildren.Add(TreeViewItemViewModel.DummyNode);
            }
        }

        internal override void LoadChildren()
        {
            if (this.PerfEventProvider != null)
            {
                this.PerfEventProvider.DesignerTreeViewLoadChildrenStart();
            }

            base.LoadChildren();
            if (this.Value.Value != null)
            {
                ChangeNotificationTracker tracker = this.Parent.GetTracker(this);

                this.AddChild(TreeViewItemViewModel.CreateViewModel(this, this.Value.Value), tracker.ParentProperty);
            }

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

            base.UpdateChildren(tracker, e);
            tracker.CleanUp();
            if (this.Value.Value != null)
            {
                this.AddChild(TreeViewItemViewModel.CreateViewModel(this, this.Value.Value), tracker.ParentProperty);
            }

            if (this.PerfEventProvider != null)
            {
                this.PerfEventProvider.DesignerTreeViewUpdateEnd();
            }
        }

        internal override void UpdateState()
        {
            base.UpdateState();
            if (this.Value.Value != null)
            {
                this.State |= TreeViewItemState.HasChildren;
            }
        }

        protected override EditingContext GetEditingContext()
        {
            if (this.Value.Key != null)
            {
                return this.Value.Key.GetEditingContext();
            }
            else
            {
                return base.GetEditingContext();
            }
        }
    }
}
