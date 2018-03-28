using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Diagnostics;

namespace System.Workflow.ComponentModel.Design
{
    /// <summary>
    /// This class renders the document outline of the workflow being designed. It shows hierarchical
    /// representation of the workflow model. 
    /// </summary>
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class WorkflowOutline : UserControl
    {
        #region Fields
        private Hashtable activityToNodeMapping = new Hashtable();
        private TreeView treeView;
        private IServiceProvider serviceProvider;
        private bool isDirty = false;
        #endregion

        #region Constructor and Dispose
        public WorkflowOutline(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
            Debug.Assert(serviceProvider != null, "Creating WorkflowOutline without service host");

            if (serviceProvider == null)
                throw new ArgumentNullException("serviceProvider");

            IDesignerHost designerHost = GetService(typeof(IDesignerHost)) as IDesignerHost;
            if (designerHost == null)
                throw new InvalidOperationException(SR.GetString(SR.General_MissingService, typeof(IDesignerHost).FullName));

            // listen for loaded and unloaded events
            DesignSurface surface = GetService(typeof(DesignSurface)) as DesignSurface;
            if (surface == null)
                throw new InvalidOperationException(SR.GetString(SR.General_MissingService, typeof(DesignSurface).FullName));
            surface.Loaded += new LoadedEventHandler(OnSurfaceLoaded);

            IComponentChangeService componentChangeService = GetService(typeof(IComponentChangeService)) as IComponentChangeService;
            if (componentChangeService != null)
            {
                componentChangeService.ComponentChanged += new ComponentChangedEventHandler(OnComponentChanged);
                componentChangeService.ComponentRename += new ComponentRenameEventHandler(OnComponentRename);
            }

            WorkflowTheme.ThemeChanged += new EventHandler(OnThemeChanged);

            // Get an ISelectionService service
            ISelectionService selectionService = GetService(typeof(ISelectionService)) as ISelectionService;
            if (selectionService != null)
                selectionService.SelectionChanged += new System.EventHandler(this.OnSelectionChanged);

            IUIService uiService = GetService(typeof(IUIService)) as IUIService;
            if (uiService != null)
                this.Font = (Font)uiService.Styles["DialogFont"];

            // Set up treeview
            this.treeView = new TreeView();
            this.treeView.Dock = DockStyle.Fill;
            this.treeView.HideSelection = false;
            this.treeView.AfterSelect += new TreeViewEventHandler(this.OnTreeViewAfterSelect);
            this.treeView.MouseDown += new MouseEventHandler(this.OnTreeViewMouseDown);
            this.treeView.Font = this.Font;
            this.treeView.ItemHeight = Math.Max(this.treeView.ItemHeight, 18);
            this.Controls.Add(this.treeView);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.IsDirty = false;

                WorkflowTheme.ThemeChanged -= new EventHandler(OnThemeChanged);

                IComponentChangeService componentChangeService = GetService(typeof(IComponentChangeService)) as IComponentChangeService;
                if (componentChangeService != null)
                {
                    componentChangeService.ComponentChanged -= new ComponentChangedEventHandler(OnComponentChanged);
                    componentChangeService.ComponentRename -= new ComponentRenameEventHandler(OnComponentRename);
                }

                ISelectionService selectionService = GetService(typeof(ISelectionService)) as ISelectionService;
                if (selectionService != null)
                    selectionService.SelectionChanged -= new System.EventHandler(OnSelectionChanged);

                DesignSurface surface = GetService(typeof(DesignSurface)) as DesignSurface;
                if (surface != null)
                    surface.Loaded -= new LoadedEventHandler(OnSurfaceLoaded);

                this.serviceProvider = null;
            }

            base.Dispose(disposing);
        }
        #endregion

        #region Public Methods
        //completly reloads the document outline tree
        private bool needsExpandAll = true;
        internal protected bool NeedsExpandAll
        {
            get
            {
                return this.needsExpandAll;
            }
            set
            {
                this.needsExpandAll = value;
            }
        }

        internal protected TreeNode RootNode
        {
            get
            {
                if (this.treeView.Nodes.Count > 0)
                    return this.treeView.Nodes[0];
                else
                    return null;
            }
        }

        public void ReloadWorkflowOutline()
        {
            OnBeginUpdate();

            this.treeView.BeginUpdate();
            try
            {
                this.treeView.Nodes.Clear();
                this.activityToNodeMapping.Clear();

                IRootDesigner rootDesigner = ActivityDesigner.GetSafeRootDesigner(this.serviceProvider) as IRootDesigner;
                if (rootDesigner != null && rootDesigner.Component != null && rootDesigner.Component is Activity)
                    InsertDocOutlineNode(null, rootDesigner.Component as Activity, 0, true);

                if (NeedsExpandAll)
                    this.treeView.ExpandAll();
            }
            finally
            {
                this.treeView.EndUpdate();
            }

            IsDirty = false;

            ISelectionService selectionService = GetService(typeof(ISelectionService)) as ISelectionService;
            if (selectionService != null && selectionService.PrimarySelection != null)
            {
                this.treeView.SelectedNode = this.activityToNodeMapping[selectionService.PrimarySelection] as WorkflowOutlineNode;
                if (this.treeView.SelectedNode != null)
                    this.treeView.SelectedNode.EnsureVisible();
            }

            OnEndUpdate();
        }

        //refreshes color and icons of all document outline nodes
        public void RefreshWorkflowOutline()
        {
            if (this.treeView.Nodes.Count > 0)
                RefreshNode(this.treeView.Nodes[0] as WorkflowOutlineNode, true);
        }
        #endregion

        #region Protected Methods
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            ReloadWorkflowOutline();
        }

        //gets called when the control is about to update the treeview
        protected virtual void OnBeginUpdate()
        {
        }

        //gets called when the control is done updating the treeview
        protected virtual void OnEndUpdate()
        {
        }

        protected virtual void OnRefreshNode(WorkflowOutlineNode node)
        {
            if (node == null)
                return;

            Activity activity = node.Activity;
            if (activity == null)
                return;

            //Update the designer image
            int imageIndex = (this.treeView.ImageList != null) ? this.treeView.ImageList.Images.IndexOfKey(activity.GetType().FullName) : -1;
            if (imageIndex == -1)
            {
                ActivityDesigner activityDesigner = ActivityDesigner.GetDesigner(activity);
                if (activityDesigner != null)
                {
                    Bitmap activityImage = activityDesigner.StockImage as Bitmap;
                    if (activityImage != null)
                    {
                        if (this.treeView.ImageList == null)
                        {
                            this.treeView.ImageList = new ImageList();
                            this.treeView.ImageList.ColorDepth = ColorDepth.Depth32Bit;
                        }

                        this.treeView.ImageList.Images.Add(activity.GetType().FullName, activityImage);
                        imageIndex = this.treeView.ImageList.Images.Count - 1;
                    }
                }
            }
            node.ImageIndex = node.SelectedImageIndex = imageIndex;

            //ask the node to update its color, text, etc
            node.RefreshNode();
        }

        //calls to create a new workflow node for a given activity
        //could return null which will indicate that we dont want to create a node for it
        protected virtual WorkflowOutlineNode CreateNewNode(Activity activity)
        {
            return new WorkflowOutlineNode(activity);
        }

        //gets called when user selected a node in the activity tree
        protected virtual void OnNodeSelected(WorkflowOutlineNode node)
        {
            ISelectionService selectionService = GetService(typeof(ISelectionService)) as ISelectionService;
            if (selectionService != null && node != null && selectionService.PrimarySelection != node.Activity)
            {
                // make the selected shape visible (note - the order is important, EnsureVisible will 
                // expand appropiate branches and set CAG into edit mode, which is crucial for 
                // SetSelectedComponents to perform well)
                WorkflowView workflowView = GetService(typeof(WorkflowView)) as WorkflowView;
                if (workflowView != null)
                    workflowView.EnsureVisible(node.Activity);

                // select it
                selectionService.SetSelectedComponents(new object[] { node.Activity }, SelectionTypes.Replace);
            }
        }

        //gets called after adding a new not-null node to the activity tree
        protected virtual void OnNodeAdded(WorkflowOutlineNode node)
        {
        }

        //gets a tree node for the given activity
        protected WorkflowOutlineNode GetNode(Activity activity)
        {
            return this.activityToNodeMapping[activity] as WorkflowOutlineNode;
        }

        protected void RefreshNode(WorkflowOutlineNode nodeToUpdate, bool refreshChildNodes)
        {
            this.treeView.BeginUpdate();

            Queue<WorkflowOutlineNode> nodesQ = new Queue<WorkflowOutlineNode>();
            nodesQ.Enqueue(nodeToUpdate);
            while (nodesQ.Count > 0)
            {
                WorkflowOutlineNode node = nodesQ.Dequeue();
                OnRefreshNode(node);

                if (refreshChildNodes)
                {
                    foreach (TreeNode childNode in node.Nodes)
                    {
                        WorkflowOutlineNode childOutlineNode = childNode as WorkflowOutlineNode;
                        if (childOutlineNode != null)
                        {
                            Activity childActivity = childOutlineNode.Activity;
                            if (childActivity != null)
                                nodesQ.Enqueue(childOutlineNode);
                        }
                    }
                }

                this.treeView.EndUpdate();
            }
        }

        protected override object GetService(Type serviceType)
        {
            object service = null;
            if (this.serviceProvider != null)
                service = this.serviceProvider.GetService(serviceType);
            else
                service = base.GetService(serviceType);
            return service;
        }
        #endregion

        #region Protected Properties
        protected internal TreeView TreeView
        {
            get
            {
                return this.treeView;
            }
        }

        protected internal event TreeViewCancelEventHandler Expanding
        {
            add
            {
                this.treeView.BeforeExpand += value;
            }
            remove
            {
                this.treeView.BeforeExpand -= value;
            }
        }
        #endregion

        #region Private Properties
        private bool IsDirty
        {
            get
            {
                return this.isDirty;
            }
            set
            {
                if (this.isDirty != value)
                {
                    this.isDirty = value;
                    if (value)
                        Application.Idle += new System.EventHandler(this.OnIdle);
                    else
                        Application.Idle -= new System.EventHandler(this.OnIdle);
                }
            }
        }
        #endregion

        #region Private Methods
        private void InsertDocOutlineNode(WorkflowOutlineNode parentNode, Activity activity, int childIndex, bool addNestedActivities)
        {
            if (this.activityToNodeMapping.Contains(activity))
                return;

            WorkflowOutlineNode newNode = CreateNewNode(activity);
            if (newNode == null)
                return;

            RefreshNode(newNode, false);
            this.activityToNodeMapping.Add(activity, newNode);

            if (addNestedActivities && activity is CompositeActivity)
            {
                foreach (Activity childActivity in ((CompositeActivity)activity).Activities)
                    InsertDocOutlineNode(newNode, childActivity, newNode.Nodes.Count, addNestedActivities);
            }

            if (parentNode != null)
                parentNode.Nodes.Insert(childIndex, newNode);
            else
                this.treeView.Nodes.Add(newNode);

            OnNodeAdded(newNode);
        }

        private void OnComponentChanged(object sender, ComponentChangedEventArgs e)
        {
            ActivityCollectionChangeEventArgs changeEventArgs = e.OldValue as ActivityCollectionChangeEventArgs;
            if (changeEventArgs != null)
            {
                this.IsDirty = true;
            }
            else if (e.Member != null && e.Component is Activity)
            {
                WorkflowOutlineNode node = this.activityToNodeMapping[e.Component] as WorkflowOutlineNode;
                if (node != null && string.Equals(e.Member.Name, "Enabled", StringComparison.Ordinal))
                    RefreshNode(node, true);
            }
        }

        private void OnComponentRename(object sender, ComponentRenameEventArgs e)
        {
            if (e.Component is Activity)
            {
                //
                WorkflowOutlineNode node = this.activityToNodeMapping[e.Component] as WorkflowOutlineNode;
                if (node != null)
                    node.OnActivityRename(e.NewName);
            }
        }

        private void OnIdle(object sender, EventArgs e)
        {
            if (this.IsDirty && this.treeView.Visible)
                ReloadWorkflowOutline();
        }

        private void OnSelectionChanged(object sender, EventArgs e)
        {
            ISelectionService selectionService = GetService(typeof(ISelectionService)) as ISelectionService;
            if (selectionService != null && selectionService.PrimarySelection != null)
            {
                this.treeView.SelectedNode = this.activityToNodeMapping[selectionService.PrimarySelection] as WorkflowOutlineNode;
                if (this.treeView.SelectedNode != null)
                    this.treeView.SelectedNode.EnsureVisible();
            }
        }

        private void OnSurfaceLoaded(object sender, LoadedEventArgs e)
        {
            ReloadWorkflowOutline();
        }

        private void OnTreeViewAfterSelect(object sender, TreeViewEventArgs e)
        {
            // Change the primary selection of the selection service
            WorkflowOutlineNode node = e.Node as WorkflowOutlineNode;
            OnNodeSelected(node);
        }
        private void OnTreeViewMouseDown(object sender, MouseEventArgs e)
        {
            if (this.treeView.GetNodeAt(e.Location) != null)
                this.treeView.SelectedNode = this.treeView.GetNodeAt(e.Location);
        }

        private void OnThemeChanged(object sender, EventArgs e)
        {
            if (this.treeView.Nodes.Count > 0)
            {
                this.treeView.ImageList.Images.Clear();
                RefreshWorkflowOutline();
            }
        }
        #endregion
    }

    #region Class WorkflowOutlineNode
    /// <summary>
    /// Class representing the node in document outline
    /// </summary>
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class WorkflowOutlineNode : TreeNode
    {
        private Activity activity;

        public WorkflowOutlineNode(Activity activity)
            : base()
        {
            if (activity == null)
                throw new ArgumentNullException("activity");

            this.activity = activity;
            this.Name = activity.Name;
        }

        public Activity Activity
        {
            get
            {
                return this.activity;
            }
        }

        //gets called to update node properties (like color, text)
        public virtual void RefreshNode()
        {
            Activity activity = this.Activity;
            if (activity == null)
                return;

            //Update the enabled color
            this.ForeColor = (!activity.Enabled || ActivityDesigner.IsCommentedActivity(activity)) ? WorkflowTheme.CurrentTheme.AmbientTheme.CommentIndicatorColor : SystemColors.WindowText;
            this.Text = activity.Name;
        }

        public virtual void OnActivityRename(string newName)
        {
            this.Text = newName;
        }
    }
    #endregion

}

