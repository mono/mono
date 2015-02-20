
//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation.View.TreeView
{
    using System.Activities.Presentation.Model;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime;
    using System.Text;
    using System.Windows;
    using System.Windows.Automation.Peers;
    using System.Windows.Controls;
    using System.Windows.Data;

    class DesignerTreeViewItem : TreeViewItem, ITreeViewItemSelectionHandler
    {
        private bool shouldUpdateViewModel = true;

        public DesignerTreeView ParentTreeView { get; set; }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new DesignerTreeViewItemAutomationPeer(this);
        }

        protected override void OnItemsChanged(Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                // Don't select itself when its child items got removed. (Default action of base.OnItemsChanged)
                // The selection should be managed by designer surface.
                return;
            }

            base.OnItemsChanged(e);
        }

        internal Dictionary<string, string> GetAutomationItemStatus()
        {
            Dictionary<string, string> itemStatus = new Dictionary<string, string>();
            TreeViewItemViewModel viewModel = this.Header as TreeViewItemViewModel;
            if (viewModel != null && viewModel.IsHighlighted)
            {
                itemStatus.Add("IsSelection", "True");
            }
            else
            {
                itemStatus.Add("IsSelection", "False");
            }

            return itemStatus;
        }

        protected override Windows.DependencyObject GetContainerForItemOverride()
        {
            DesignerTreeViewItem item = new DesignerTreeViewItem() { ParentTreeView = this.ParentTreeView };
            return item;
        }

        protected override void OnHeaderChanged(object oldHeader, object newHeader)
        {
            base.OnHeaderChanged(oldHeader, newHeader);

            TreeViewItemViewModel source = newHeader as TreeViewItemViewModel;
            if (source != null)
            {
                source.TreeViewItem = this;
            }
        }

        protected override void OnSelected(Windows.RoutedEventArgs e)
        {
            base.OnSelected(e);
            ParentTreeView.IsSelectionChangeHandledByTreeView = true;
            UpdateTreeViewItemViewModel();
        }

        protected override void OnUnselected(RoutedEventArgs e)
        {
            base.OnUnselected(e);
            if (shouldUpdateViewModel)
            {
                TreeViewItemViewModel viewModel = this.Header as TreeViewItemViewModel;
                if (viewModel != null)
                {
                    if (viewModel is TreeViewItemModelPropertyViewModel || viewModel is TreeViewItemKeyValuePairModelItemViewModel)
                    {
                        viewModel.IsHighlighted = false;
                    }
                    else if (viewModel is TreeViewItemModelItemViewModel)
                    {
                        TreeViewItemModelItemViewModel modelItemViewModel = (TreeViewItemModelItemViewModel)viewModel;
                        if (!modelItemViewModel.HasDesigner)
                        {
                            modelItemViewModel.IsHighlighted = false;
                        }
                    }
                }
            }
        }

        private void UpdateTreeViewItemViewModel()
        {
            if (!shouldUpdateViewModel)
            {
                return;
            }

            if (this.Header is TreeViewItemModelItemViewModel)
            {
                TreeViewItemModelItemViewModel modelItemViewModel = (TreeViewItemModelItemViewModel)this.Header;
                if (modelItemViewModel.HasDesigner)
                {
                    HighlightModelItemOnDesigner(modelItemViewModel.VisualValue);
                }
                else
                {
                    HighlightTreeViewItemAndClearSelectionOnDesigner(modelItemViewModel);
                }
            }
            else if (this.Header is TreeViewItemModelPropertyViewModel)
            {
                TreeViewItemModelPropertyViewModel modelPropertyViewModel = (TreeViewItemModelPropertyViewModel)this.Header;
                HighlightTreeViewItemAndClearSelectionOnDesigner(modelPropertyViewModel);
            }
            else if (this.Header is TreeViewItemKeyValuePairModelItemViewModel)
            {
                TreeViewItemKeyValuePairModelItemViewModel keyValuePairItem = (TreeViewItemKeyValuePairModelItemViewModel)this.Header;
                HighlightTreeViewItemAndClearSelectionOnDesigner(keyValuePairItem);
            }
            else
            {
                Fx.Assert("Invaild logic if Header is not TreeViewItemModelItemViewModel/TreeViewItemModelPropertyViewModel/TreeViewItemDictionaryModelItemViewModel");
            }
        }

        private void HighlightModelItemOnDesigner(ModelItem selectedModelItem)
        {
            if (selectedModelItem != null)
            {
                Selection.SelectOnly(this.ParentTreeView.Context, selectedModelItem);
                // Set highlight to visual value.  
                // Don't use "Focus()" since it will steal keyboard focus and disable 
                // keyboard navigation.
                selectedModelItem.Highlight();
            }
        }

        private void HighlightTreeViewItemAndClearSelectionOnDesigner(TreeViewItemViewModel viewModel)
        {
            this.ParentTreeView.Context.Items.SetValue(new Selection());
            viewModel.IsHighlighted = true;
        }

        void ITreeViewItemSelectionHandler.Select()
        {
            // Invoked from ViewModel, needn't update ViewModel again.
            shouldUpdateViewModel = false;
            this.IsSelected = true;
            shouldUpdateViewModel = true;
        }

        void ITreeViewItemSelectionHandler.Unselect()
        {
            // Invoked from ViewModel, needn't update ViewModel again.
            shouldUpdateViewModel = false;
            this.IsSelected = false;
            shouldUpdateViewModel = true;
        }

        private class DesignerTreeViewItemAutomationPeer : TreeViewItemAutomationPeer
        {
            private DesignerTreeViewItem owner;

            public DesignerTreeViewItemAutomationPeer(DesignerTreeViewItem owner)
                : base(owner)
            {
                Fx.Assert(owner != null, "DesignerTreeViewItemAutomationPeer should not accept a null owner.");
                this.owner = owner;
            }

            protected override AutomationControlType GetAutomationControlTypeCore()
            {
                return AutomationControlType.TreeItem;
            }

            protected override string GetItemStatusCore()
            {
                Dictionary<string, string> itemStatus = this.owner.GetAutomationItemStatus();
                StringBuilder builder = new StringBuilder();
                foreach (KeyValuePair<string, string> item in itemStatus)
                {
                    builder.AppendFormat("{0}={1} ", item.Key, item.Value);
                }

                return builder.ToString();
            }
        }
    }
}
