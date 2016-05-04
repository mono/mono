//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation.View.TreeView
{
    using System.Activities.Presentation.Model;
    using System.Collections.ObjectModel;
    using System.Windows;
    using System.Windows.Threading;

    partial class DesignerTreeView : System.Windows.Controls.TreeView
    {
        private TreeViewItemModelItemViewModel rootModelItemViewModel;
        private bool isSelectionSubscribed;

        internal bool IsSelectionChangeHandledByTreeView { get; set; }

        public DesignerTreeView()
        {
            InitializeComponent();
        }

        public EditingContext Context
        {
            get;
            private set;
        }

        public void SetRootDesigner(ModelItem modelItem)
        {
            rootModelItemViewModel = new TreeViewItemModelItemViewModel(null, modelItem);
            this.ItemsSource = new ObservableCollection<TreeViewItemModelItemViewModel>() { rootModelItemViewModel };

            if (!isSelectionSubscribed)
            {
                Selection.Subscribe(Context, ModelItemSelectionChanged);
                isSelectionSubscribed = true;
            }
        }

        public void Initialize(EditingContext context)
        {
            this.Context = context;
        }

        private void ModelItemSelectionChanged(Selection selection)
        {
            // AutoExpand only when designerTreeView didn't handle the selection change in modelItem
            if (this.IsSelectionChangeHandledByTreeView)
            {
                this.IsSelectionChangeHandledByTreeView = false;
            }
            else
            {
                if (selection.PrimarySelection != null)
                {
                    TreeViewItemViewModel itemToBeSelected = DesignerTreeAutoExpandHelper.Expand(this.rootModelItemViewModel, selection.PrimarySelection);
                    // itemToBeSelected == null means needn't AutoExpand.
                    if (itemToBeSelected != null)
                    {
                        this.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() =>
                        {
                            // TreeViewItemViewModel may got removed before Idle.
                            if (itemToBeSelected.IsAlive && itemToBeSelected.TreeViewItem != null)
                            {
                                itemToBeSelected.TreeViewItem.Select();
                            }
                            // reset this flag to false, because the operation is done.
                            // The flag will be used for next operation.
                            this.IsSelectionChangeHandledByTreeView = false;
                        }));
                    }
                }
            }
        }

        public void RestoreDesignerStates()
        {
            this.InvalidateMeasure();
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new DesignerTreeViewItem() { ParentTreeView = this };
        }
    }
}
