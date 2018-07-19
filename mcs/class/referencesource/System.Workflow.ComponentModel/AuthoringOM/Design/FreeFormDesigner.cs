namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.IO;
    using System.Drawing;
    using System.Diagnostics;
    using System.Collections;
    using System.Windows.Forms;
    using System.ComponentModel;
    using System.Drawing.Drawing2D;
    using System.Collections.Generic;
    using System.ComponentModel.Design;
    using System.Collections.ObjectModel;
    using System.Workflow.ComponentModel.Design;
    using System.ComponentModel.Design.Serialization;
    using System.Workflow.ComponentModel.Serialization;

    #region Interface IConnectableDesigner
    internal interface IConnectableDesigner
    {
        bool CanConnect(ConnectionPoint source, ConnectionPoint target);
        void OnConnected(ConnectionPoint source, ConnectionPoint target);
    }
    #endregion

    #region Enum ZOrder
    internal enum ZOrder
    {
        Foreground = 1,
        Background = 2
    }
    #endregion

    #region Class FreeformActivityDesigner
    //




    //By default this designer will use the CompositeActivityDesigner theme
    [DesignerSerializer(typeof(FreeformActivityDesignerLayoutSerializer), typeof(WorkflowMarkupSerializer))]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class FreeformActivityDesigner : CompositeActivityDesigner
    {
        #region Class FreeformDesignerAccessibleObject

        internal class FreeformDesignerAccessibleObject : CompositeDesignerAccessibleObject
        {
            public FreeformDesignerAccessibleObject(FreeformActivityDesigner activityDesigner)
                : base(activityDesigner)
            {
            }

            public override AccessibleObject GetChild(int index)
            {
                FreeformActivityDesigner designer = (FreeformActivityDesigner)this.ActivityDesigner;
                if (designer.ShowConnectorsInForeground)
                {
                    int connectorsCount = designer.Connectors.Count;
                    if (index < connectorsCount)
                        return designer.Connectors[index].AccessibilityObject;
                    else
                        return designer.ContainedDesigners[index - connectorsCount].AccessibilityObject;
                }
                else
                {
                    int containedDesignersCount = designer.ContainedDesigners.Count;
                    if (index < containedDesignersCount)
                        return designer.ContainedDesigners[index].AccessibilityObject;
                    else
                        return designer.Connectors[index - containedDesignersCount].AccessibilityObject;
                }
            }

            public override int GetChildCount()
            {
                FreeformActivityDesigner designer = (FreeformActivityDesigner)this.ActivityDesigner;
                int count = base.GetChildCount() + designer.Connectors.Count;
                return count;
            }
        }

        #endregion Class FreeformDesignerAccessibleObject

        #region Members
        internal static Size DefaultAutoSizeMargin = new Size(40, 40);

        private FreeformDesignerAccessibleObject accessibilityObject;
        private bool autoSize = true;
        private AutoSizeMode autoSizeMode = AutoSizeMode.GrowOnly;
        private Size autoSizeMargin = FreeformActivityDesigner.DefaultAutoSizeMargin;

        private bool enableUserDrawnConnectors = true;
        private List<Connector> connectors = new List<Connector>();

        private bool retainContainedDesignerLocations = false;

        public event ConnectorEventHandler ConnectorAdded;
        public event ConnectorEventHandler ConnectorChanged;
        public event ConnectorEventHandler ConnectorRemoved;
        #endregion

        #region Construction / Destruction
        public FreeformActivityDesigner()
        {
        }
        #endregion

        #region Properties

        #region Public Properties

        public override AccessibleObject AccessibilityObject
        {
            get
            {
                if (this.accessibilityObject == null)
                    this.accessibilityObject = new FreeformDesignerAccessibleObject(this);
                return this.accessibilityObject;
            }
        }

        /// <summary>
        /// Gets or Sets value indicating if the designer will be resized automatically based on the child designers
        /// </summary>
        [DefaultValue(true)]
        public bool AutoSize
        {
            get
            {
                return this.autoSize;
            }

            set
            {
                if (this.autoSize == value)
                    return;

                this.autoSize = value;
                PerformLayout();
            }
        }

        /// <summary>
        /// Gets or Sets the auto sizing mode
        /// </summary>
        [DefaultValue(AutoSizeMode.GrowOnly)]
        public AutoSizeMode AutoSizeMode
        {
            get
            {
                return this.autoSizeMode;
            }

            set
            {
                if (this.autoSizeMode == value)
                    return;

                this.autoSizeMode = value;
                PerformLayout();
            }
        }

        /// <summary>
        /// Gets the margins left around the contained designers when autosizing
        /// </summary>
        public Size AutoSizeMargin
        {
            get
            {
                Size margin = this.autoSizeMargin;
                if (WorkflowTheme.CurrentTheme.AmbientTheme.ShowGrid)
                {
                    Size gridSize = WorkflowTheme.CurrentTheme.AmbientTheme.GridSize;
                    margin.Width += gridSize.Width / 2;
                    margin.Height += gridSize.Height / 2;
                }

                return margin;
            }

            set
            {
                if (this.autoSizeMargin == value)
                    return;

                this.autoSizeMargin = value;
                PerformLayout();
            }
        }

        /// <summary>
        /// Allows user to drag and draw connectors on the design surface
        /// </summary>
        [DefaultValue(true)]
        public bool EnableUserDrawnConnectors
        {
            get
            {
                return (this.enableUserDrawnConnectors && IsEditable);
            }

            set
            {
                this.enableUserDrawnConnectors = value;
            }
        }

        public override bool CanExpandCollapse
        {
            get
            {
                return false;
            }
        }

        public override object FirstSelectableObject
        {
            get
            {
                IList<ActivityDesigner> childdesigners = ContainedDesigners;
                return (childdesigners.Count > 0) ? childdesigners[0].Activity : null;
            }
        }

        public override object LastSelectableObject
        {
            get
            {
                IList<ActivityDesigner> childdesigners = ContainedDesigners;
                return (childdesigners.Count > 0) ? childdesigners[childdesigners.Count - 1].Activity : null;
            }
        }

        public override Size MinimumSize
        {
            get
            {
                //Now go thru all the activity designers and consider the bottom coordinate + selection size + page separator as the min size
                Size minimumSize = base.MinimumSize;

                if (Activity != null && ((IComponent)Activity).Site != null && !(ParentDesigner is FreeformActivityDesigner))
                {
                    minimumSize.Width *= 4;
                    minimumSize.Height *= 4;
                }

                //If the designer is root designer and not inlined then we should occupy the area of workflow view
                if (IsRootDesigner && InvokingDesigner == null)
                {
                    WorkflowView workflowView = ParentView;
                    minimumSize.Width = Math.Max(minimumSize.Width, workflowView.ViewPortSize.Width - 2 * WorkflowRootLayout.Separator.Width);
                    minimumSize.Height = Math.Max(minimumSize.Height, workflowView.ViewPortSize.Height - 2 * WorkflowRootLayout.Separator.Height);
                }

                if (AutoSize)
                {
                    Rectangle childRectangle = GetEnclosingRectangle();
                    if (!childRectangle.IsEmpty)
                    {
                        minimumSize.Width = Math.Max(minimumSize.Width, childRectangle.Width);
                        minimumSize.Height = Math.Max(minimumSize.Height, childRectangle.Height);
                    }
                }

                return minimumSize;
            }
        }

        public override Point Location
        {
            get
            {
                return base.Location;
            }

            set
            {
                if (Location == value)
                    return;

                //Please note that we require this logic to maintain the contained designer location when
                //resizing the designer
                ReadOnlyCollection<ActivityDesigner> containedDesigners = ContainedDesigners;
                List<Point> containedDesignerLocations = new List<Point>();
                if (this.retainContainedDesignerLocations)
                {
                    foreach (ActivityDesigner activityDesigner in containedDesigners)
                        containedDesignerLocations.Add(activityDesigner.Location);
                }
                else
                {
                    Size moveDelta = new Size(value.X - base.Location.X, value.Y - base.Location.Y);
                    FreeformActivityDesigner freeFormActivityDesigner = this;
                    Collection<Connector> connectors = new Collection<Connector>();
                    while (freeFormActivityDesigner != null)
                    {
                        foreach (Connector connector in freeFormActivityDesigner.Connectors)
                            if (connector.RenderingOwner == this)
                                connectors.Add(connector);

                        freeFormActivityDesigner = freeFormActivityDesigner.ParentDesigner as FreeformActivityDesigner;
                    }
                    foreach (Connector connector in connectors)
                    {
                        connector.Offset(moveDelta);
                    }
                }

                base.Location = value;

                if (this.retainContainedDesignerLocations && containedDesigners.Count == containedDesignerLocations.Count)
                {
                    for (int i = 0; i < containedDesigners.Count; i++)
                        containedDesigners[i].Location = containedDesignerLocations[i];
                }

                Invalidate();
            }
        }

        /// <summary>
        /// Returns collection of connectors
        /// </summary>
        public ReadOnlyCollection<Connector> Connectors
        {
            get
            {
                return this.connectors.AsReadOnly();
            }
        }
        #endregion

        #region Protected Properties
        /// <summary>
        /// Get the value indicating if the connectors are drawn in the forground 
        /// </summary>
        protected virtual bool ShowConnectorsInForeground
        {
            get
            {
                return false;
            }
        }

        protected internal override bool EnableVisualResizing
        {
            get
            {
                if (AutoSize && AutoSizeMode == AutoSizeMode.GrowAndShrink)
                    return false;
                else
                    return true;
            }
        }

        protected internal override ActivityDesignerGlyphCollection Glyphs
        {
            get
            {
                ActivityDesignerGlyphCollection glyphs = new ActivityDesignerGlyphCollection();
                glyphs.AddRange(base.Glyphs);

                //Now go thru all the designers and for the designers which are connectable, return connection glyphs
                //Also return the move glyphs for movable designer
                ISelectionService selectionService = GetService(typeof(ISelectionService)) as ISelectionService;
                if (selectionService != null)
                {
                    foreach (object selectedObject in selectionService.GetSelectedComponents())
                    {
                        ConnectorHitTestInfo connectorHitInfo = selectedObject as ConnectorHitTestInfo;
                        if (connectorHitInfo != null && connectorHitInfo.AssociatedDesigner == this)
                            glyphs.Add(new FreeFormConnectorSelectionGlyph(connectorHitInfo.MapToIndex(), (connectorHitInfo == selectionService.PrimarySelection)));
                    }
                }

                return glyphs;
            }
        }
        #endregion

        #region Private Properties
        internal override WorkflowLayout SupportedLayout
        {
            get
            {
                return new WorkflowRootLayout(Activity.Site);
            }
        }

        #region Properties used during serialization only
        //NOTE THAT THIS WILL ONLY BE USED FOR SERIALIZATION PURPOSES
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        internal List<Connector> DesignerConnectors
        {
            get
            {
                List<Connector> connectors = new List<Connector>(this.connectors);
                return connectors;
            }
        }
        #endregion

        #endregion

        #endregion

        #region Methods

        #region Public Methods
        /// <summary>
        /// Adds connector to the designer
        /// </summary>
        /// <param name="connector">Connector to add</param>
        public Connector AddConnector(ConnectionPoint source, ConnectionPoint target)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            if (source.AssociatedDesigner == null)
                throw new ArgumentException("source", SR.GetString(SR.Error_AssociatedDesignerMissing));

            if (target == null)
                throw new ArgumentNullException("target");

            if (target.AssociatedDesigner == null)
                throw new ArgumentException("target", SR.GetString(SR.Error_AssociatedDesignerMissing));

            //This check can be eliminated if it slows down connections, this is just to ensure we are adding connectors
            //to correct activity designer
            FreeformActivityDesigner connectorContainer = ConnectionManager.GetConnectorContainer(source.AssociatedDesigner);
            if (this != connectorContainer)
                throw new InvalidOperationException(DR.GetString(DR.Error_AddConnector1));

            if ((Activity != source.AssociatedDesigner.Activity && !Helpers.IsChildActivity(Activity as CompositeActivity, source.AssociatedDesigner.Activity)) ||
                (Activity != target.AssociatedDesigner.Activity && !Helpers.IsChildActivity(Activity as CompositeActivity, target.AssociatedDesigner.Activity)))
                throw new ArgumentException(DR.GetString(DR.Error_AddConnector2));

            Connector connector = CreateConnector(source, target);
            if (connector != null)
            {
                if (this.connectors.Contains(connector))
                    throw new InvalidOperationException(DR.GetString(DR.Error_AddConnector3));

                this.connectors.Add(connector);
                connector.SetParent(this);
                OnConnectorAdded(new ConnectorEventArgs(connector));
            }

            PerformLayout();

            return connector;
        }

        /// <summary>
        /// Removes connector from the designer
        /// </summary>
        /// <param name="connector">Connector to remove</param>
        public void RemoveConnector(Connector connector)
        {
            if (connector == null)
                throw new ArgumentNullException("connector");

            if (this.connectors.Contains(connector))
            {
                OnConnectorRemoved(new ConnectorEventArgs(connector));
                connector.SetParent(null);
                this.connectors.Remove(connector);
            }
        }

        /// <summary>
        /// Sends the contained designer to back of z-order
        /// </summary>
        /// <param name="containedDesigner">Designer to send to back</param>
        public void SendToBack(ActivityDesigner containedDesigner)
        {
            if (containedDesigner == null)
                throw new ArgumentNullException("containedDesigner");

            if (!ContainedDesigners.Contains(containedDesigner))
                throw new ArgumentException(DR.GetString(DR.InvalidDesignerSpecified, "containedDesigner"));

            UpdateZOrder(containedDesigner, ZOrder.Background);
        }


        /// <summary>
        /// Brings the contained designer to top of z-order
        /// </summary>
        /// <param name="containedDesigner">Designer to bring to front</param>
        public void BringToFront(ActivityDesigner containedDesigner)
        {
            if (containedDesigner == null)
                throw new ArgumentNullException("containedDesigner");

            if (!ContainedDesigners.Contains(containedDesigner))
                throw new ArgumentException(DR.GetString(DR.InvalidDesignerSpecified, "containedDesigner"));

            UpdateZOrder(containedDesigner, ZOrder.Foreground);
        }

        /// <summary>
        /// Moves specified contained designer to specified location inside designer
        /// </summary>
        /// <param name="containedDesigner">Designer to move</param>
        /// <param name="newLocation">Location to move to.</param>
        public void MoveContainedDesigner(ActivityDesigner containedDesigner, Point newLocation)
        {
            if (containedDesigner == null)
                throw new ArgumentNullException("containedDesigner");

            if (!ContainedDesigners.Contains(containedDesigner))
                throw new ArgumentException(DR.GetString(DR.InvalidDesignerSpecified, "containedDesigner"));

            FreeformActivityDesigner.SetDesignerBounds(containedDesigner, new Rectangle(newLocation, containedDesigner.Size));

            PerformLayout();
            Invalidate();
        }

        /// <summary>
        /// Resizes specified contained designer to specified size
        /// </summary>
        /// <param name="containedDesigner">Designer to resize</param>
        /// <param name="newSize">Size to resize it to</param>
        public void ResizeContainedDesigner(ActivityDesigner containedDesigner, Size newSize)
        {
            if (containedDesigner == null)
                throw new ArgumentNullException("containedDesigner");

            if (!ContainedDesigners.Contains(containedDesigner))
                throw new ArgumentException(DR.GetString(DR.InvalidDesignerSpecified, "containedDesigner"));

            FreeformActivityDesigner.SetDesignerBounds(containedDesigner, new Rectangle(containedDesigner.Location, newSize));

            PerformLayout();
        }

        public override HitTestInfo HitTest(Point point)
        {
            HitTestInfo hitInfo = base.HitTest(point);

            //First check that if the drag drop is in progress and the drop operation has been initiated based on
            //A child designer then if the queried point is also on the drop initiated designer then we return freeform
            //designer as the designer where hittest occured
            ReadOnlyCollection<ActivityDesigner> containedDesigners = ContainedDesigners;

            WorkflowView workflowView = ParentView;
            DragDropManager dragDropManager = GetService(typeof(DragDropManager)) as DragDropManager;
            if (workflowView != null && dragDropManager != null &&
                workflowView.DragDropInProgress && hitInfo.AssociatedDesigner != null &&
                dragDropManager.DraggedActivities.Contains(hitInfo.AssociatedDesigner.Activity) &&
                hitInfo.AssociatedDesigner.Bounds.Contains(point))
            {
                if (Activity == hitInfo.AssociatedDesigner.Activity)
                    return HitTestInfo.Nowhere;
                else if (containedDesigners.Contains(hitInfo.AssociatedDesigner))
                    return new HitTestInfo(this, HitTestLocations.Designer);
            }

            //Now try going through the connectors
            if (!(hitInfo is ConnectionPointHitTestInfo) &&
                (hitInfo.HitLocation == HitTestLocations.None || hitInfo.AssociatedDesigner == this || ShowConnectorsInForeground))
            {
                //Now go thru all the connectors and try to select them
                for (int i = 0; i < this.connectors.Count; i++)
                {
                    if (this.connectors[i].HitTest(point))
                        return new ConnectorHitTestInfo(this, HitTestLocations.Designer | HitTestLocations.Connector, i);
                }
            }

            return hitInfo;
        }

        //activities being moved within the same container
        private List<ActivityDesigner> movedActivities = null;
        private List<ActivityDesigner> MovingActivities
        {
            get
            {
                if (this.movedActivities == null)
                    this.movedActivities = new List<ActivityDesigner>();

                return this.movedActivities;
            }
        }

        public override void MoveActivities(HitTestInfo moveLocation, ReadOnlyCollection<Activity> activitiesToMove)
        {
            if (moveLocation == null)
                throw new ArgumentNullException("moveLocation");

            if (activitiesToMove == null)
                throw new ArgumentNullException("activitiesToMove");

            FreeformActivityDesigner connectorContainer = ConnectionManager.GetConnectorContainer(this);
            try
            {
                connectorContainer.MovingActivities.Clear();
                if (connectorContainer != null && connectorContainer.Connectors.Count > 0)
                {
                    foreach (Activity movingActivity in activitiesToMove)
                    {
                        ActivityDesigner designerToMove = ActivityDesigner.GetDesigner(movingActivity);
                        FreeformActivityDesigner topMostParentDesigner = ConnectionManager.GetConnectorContainer(designerToMove);

                        if (topMostParentDesigner == connectorContainer)
                            connectorContainer.MovingActivities.Add(designerToMove);
                    }
                }

                base.MoveActivities(moveLocation, activitiesToMove);
            }
            finally
            {
                connectorContainer.MovingActivities.Clear();
            }
        }

        public override object GetNextSelectableObject(object current, DesignerNavigationDirection navigate)
        {
            object nextObject = null;

            ArrayList activityDesigners = new ArrayList(ContainedDesigners);
            ActivityDesigner activityDesigner = (current is Activity) ? ActivityDesigner.GetDesigner(current as Activity) : ActivityDesigner.GetParentDesigner(current);
            int index = (activityDesigner != null) ? activityDesigners.IndexOf(activityDesigner) : -1;
            if ((navigate == DesignerNavigationDirection.Left || navigate == DesignerNavigationDirection.Up) && index >= 0 && index < activityDesigners.Count)
                nextObject = ((ActivityDesigner)activityDesigners[(index > 0) ? index - 1 : activityDesigners.Count - 1]).Activity;
            else if ((navigate == DesignerNavigationDirection.Right || navigate == DesignerNavigationDirection.Down) && index <= activityDesigners.Count - 1)
                nextObject = ((ActivityDesigner)activityDesigners[(index < activityDesigners.Count - 1) ? index + 1 : 0]).Activity;

            return nextObject;
        }
        #endregion

        #region Protected Methods
        protected override void Initialize(Activity activity)
        {
            base.Initialize(activity);

            //We only add the designer property extender here, it will be removed when the design surface is
            //destroyed
            EnsureDesignerExtender();
        }

        protected override void Dispose(bool disposing)
        {
            //Dispose the connectors
            for (int i = 0; i < this.connectors.Count; i++)
                ((IDisposable)this.connectors[i]).Dispose();
            this.connectors.Clear();

            base.Dispose(disposing);
        }

        //


        protected override void OnContainedActivitiesChanging(ActivityCollectionChangeEventArgs listChangeArgs)
        {
            base.OnContainedActivitiesChanging(listChangeArgs);

            if (listChangeArgs.Action == ActivityCollectionChangeAction.Remove)
            {
                FreeformActivityDesigner connectorContainer = ConnectionManager.GetConnectorContainer(this);
                List<Connector> connectorsToDelete = new List<Connector>();

                //check if the removed designer is being moved within the same container
                //in this case dont remove the connector...
                ActivityDesigner activityDesigner = this;
                while (activityDesigner != null)
                {
                    FreeformActivityDesigner freeFormDesigner = activityDesigner as FreeformActivityDesigner;
                    if (freeFormDesigner != null && freeFormDesigner.Connectors.Count > 0)
                    {
                        foreach (Activity activityToRemove in listChangeArgs.RemovedItems)
                        {
                            ActivityDesigner designerToRemove = ActivityDesigner.GetDesigner(activityToRemove);

                            //if the designer is being moved within the same container, ignore it
                            //otherwise remove all related connectors
                            if (!connectorContainer.MovingActivities.Contains(designerToRemove))
                            {
                                foreach (Connector connector in freeFormDesigner.Connectors)
                                {
                                    if (designerToRemove == connector.Source.AssociatedDesigner || designerToRemove == connector.Target.AssociatedDesigner)
                                        connectorsToDelete.Add(connector);
                                }
                            }
                        }
                    }

                    activityDesigner = activityDesigner.ParentDesigner;
                }

                foreach (Connector connectorToDelete in connectorsToDelete)
                {
                    connectorToDelete.ParentDesigner.RemoveConnector(connectorToDelete);
                    ((IDisposable)connectorToDelete).Dispose();
                }
            }
        }

        /// <summary>
        /// Creates the connector between specified connection points.
        /// </summary>
        /// <param name="source">Source connection point</param>
        /// <param name="target">Target connection point</param>
        /// <returns></returns>
        protected internal virtual Connector CreateConnector(ConnectionPoint source, ConnectionPoint target)
        {
            return new Connector(source, target);
        }

        /// <summary>
        /// Called to check if contained designers can be connected
        /// </summary>
        /// <param name="source">Source connection point</param>
        /// <param name="target">Target connection point</param>
        /// <returns></returns>
        protected internal virtual bool CanConnectContainedDesigners(ConnectionPoint source, ConnectionPoint target)
        {
            return (((IConnectableDesigner)source.AssociatedDesigner).CanConnect(source, target) &&
                    ((IConnectableDesigner)target.AssociatedDesigner).CanConnect(source, target));
        }

        /// <summary>
        /// Called when the connection between contained designers is established
        /// </summary>
        /// <param name="source">Source connection point</param>
        /// <param name="target">Target connection point</param>
        protected internal virtual void OnContainedDesignersConnected(ConnectionPoint source, ConnectionPoint target)
        {
            ((IConnectableDesigner)source.AssociatedDesigner).OnConnected(source, target);
            ((IConnectableDesigner)target.AssociatedDesigner).OnConnected(source, target);
        }

        /// <summary>
        /// Called to find if the contained designer can be visually resized by the user
        /// </summary>
        /// <param name="containedDesigner">Designer to visually resize</param>
        /// <returns>True if the designer can be visually resize, false otherwise</returns>
        protected internal virtual bool CanResizeContainedDesigner(ActivityDesigner containedDesigner)
        {
            return (containedDesigner is FreeformActivityDesigner);
        }

        /// <summary>
        /// Called when a new connector has been added in the freeform designer
        /// </summary>
        /// <param name="e">Event args containing connector</param>
        protected virtual void OnConnectorAdded(ConnectorEventArgs e)
        {
            if (ConnectorAdded != null)
                ConnectorAdded(this, e);
        }

        /// <summary>
        /// Called when the user changed the end points of the connector
        /// </summary>
        /// <param name="e"></param>
        protected internal virtual void OnConnectorChanged(ConnectorEventArgs e)
        {
            if (ConnectorChanged != null)
                ConnectorChanged(this, e);
        }

        protected virtual void OnConnectorRemoved(ConnectorEventArgs e)
        {
            if (ConnectorRemoved != null)
                ConnectorRemoved(this, e);
        }

        protected override void OnLayoutPosition(ActivityDesignerLayoutEventArgs e)
        {
            base.OnLayoutPosition(e);

            if (AutoSize)
            {
                Point newLocation = Location;

                Rectangle childRectangle = GetEnclosingRectangle();
                if (!childRectangle.IsEmpty)
                {
                    if (AutoSizeMode == AutoSizeMode.GrowOnly)
                    {
                        newLocation.X = Math.Min(newLocation.X, childRectangle.Left);
                        newLocation.Y = Math.Min(newLocation.Y, childRectangle.Top);
                    }
                    else
                    {
                        newLocation = childRectangle.Location;
                    }
                }

                this.retainContainedDesignerLocations = true;
                Location = newLocation;
                this.retainContainedDesignerLocations = false;
            }

            foreach (Connector connector in this.connectors)
                connector.OnLayout(e);
        }

        protected override Size OnLayoutSize(ActivityDesignerLayoutEventArgs e)
        {
            Rectangle bounds = Bounds;
            Size size = bounds.Size;

            base.OnLayoutSize(e);

            if (AutoSize)
            {
                if (AutoSizeMode == AutoSizeMode.GrowOnly)
                {
                    Rectangle childRectangle = GetEnclosingRectangle();
                    if (!childRectangle.IsEmpty)
                    {
                        size.Width += Math.Max(bounds.Left - childRectangle.Left, 0);
                        size.Width += Math.Max(childRectangle.Right - bounds.Right, 0);
                        size.Height += Math.Max(bounds.Top - childRectangle.Top, 0);
                        size.Height += Math.Max(childRectangle.Bottom - bounds.Bottom, 0);
                    }
                }
                else
                {
                    size = MinimumSize;
                }
            }

            return size;
        }

        protected override void OnThemeChange(ActivityDesignerTheme newTheme)
        {
            base.OnThemeChange(newTheme);

            if (WorkflowTheme.CurrentTheme.AmbientTheme.ShowGrid)
            {
                foreach (ActivityDesigner containedDesigner in ContainedDesigners)
                    containedDesigner.Location = DesignerHelpers.SnapToGrid(containedDesigner.Location);
                PerformLayout();
            }
        }

        protected override void OnDragOver(ActivityDragEventArgs e)
        {
            bool ctrlKeyPressed = ((e.KeyState & 8) == 8);
            if (ctrlKeyPressed && (e.AllowedEffect & DragDropEffects.Copy) == DragDropEffects.Copy)
                e.Effect = DragDropEffects.Copy;
            else if ((e.AllowedEffect & DragDropEffects.Move) == DragDropEffects.Move)
                e.Effect = DragDropEffects.Move;
        }

        protected override void OnDragDrop(ActivityDragEventArgs e)
        {
            //Set the correct drag drop effect
            bool ctrlKeyPressed = ((e.KeyState & 8) == 8);
            if (ctrlKeyPressed && (e.AllowedEffect & DragDropEffects.Copy) == DragDropEffects.Copy)
                e.Effect = DragDropEffects.Copy;
            else if ((e.AllowedEffect & DragDropEffects.Move) == DragDropEffects.Move)
                e.Effect = DragDropEffects.Move;

            //Now there can be set of activities which are inserted and set of activities which are moved
            //So first lets get the list of activities which need to be inserted
            List<Activity> activitiesToInsert = new List<Activity>();
            List<Activity> newActivities = new List<Activity>();
            foreach (Activity activity in e.Activities)
            {
                if (activity.Site == null || activity.Parent != Activity)
                    activitiesToInsert.Add(activity);

                if (activity.Site == null)
                    newActivities.Add(activity);
            }

            //If the component are sited then that means that we are inserting it 
            if (activitiesToInsert.Count > 0)
                CompositeActivityDesigner.InsertActivities(this, new ConnectorHitTestInfo(this, HitTestLocations.Designer, ((CompositeActivity)Activity).Activities.Count), activitiesToInsert.AsReadOnly(), SR.GetString(SR.DragDropActivities));

            Point dropPoint = new Point(e.X, e.Y);
            Point[] movedLocations = FreeFormDragDropManager.GetDesignerLocations(e.DragInitiationPoint, dropPoint, e.Activities);
            if (movedLocations.Length == e.Activities.Count)
            {
                for (int i = 0; i < e.Activities.Count; i++)
                {
                    ActivityDesigner designerToMove = ActivityDesigner.GetDesigner(e.Activities[i]);
                    if (designerToMove != null)
                    {
                        Point location = (newActivities.Contains(designerToMove.Activity)) ? dropPoint : movedLocations[i];
                        MoveContainedDesigner(designerToMove, location);
                    }
                }
            }
            else
            {
                Debug.Assert(false);
            }
        }

        public override void InsertActivities(HitTestInfo insertLocation, ReadOnlyCollection<Activity> activitiesToInsert)
        {
            base.InsertActivities(insertLocation, activitiesToInsert);

            //Now go through all the designers for activities and make sure that if their locations are 0,0 then we set the
            //locations at Location.X + AutoSizeMargin
            if (AutoSize)
            {
                Size autoSizeMargin = AutoSizeMargin;
                Point location = Location;
                foreach (Activity activity in activitiesToInsert)
                {
                    ActivityDesigner designer = ActivityDesigner.GetDesigner(activity);
                    if (designer.Location.IsEmpty)
                        designer.Location = new Point(location.X + autoSizeMargin.Width, location.Y + autoSizeMargin.Height);
                }
            }
        }

        protected override void OnResizing(ActivityDesignerResizeEventArgs e)
        {
            //If we are in AutoSize mode with grow and shrink option then dont allow resizing
            if (AutoSize)
            {
                if (AutoSizeMode == AutoSizeMode.GrowOnly)
                {
                    Rectangle minRectangle = GetEnclosingRectangle();
                    if (!minRectangle.IsEmpty)
                    {
                        Rectangle bounds = Rectangle.Empty;
                        bounds.X = Math.Min(minRectangle.Left, e.Bounds.Left);
                        bounds.Y = Math.Min(minRectangle.Top, e.Bounds.Top);
                        bounds.Width = Math.Max(minRectangle.Right - bounds.Left, e.Bounds.Right - bounds.Left);
                        bounds.Height = Math.Max(minRectangle.Bottom - bounds.Top, e.Bounds.Bottom - bounds.Top);

                        if (bounds != e.Bounds)
                            e = new ActivityDesignerResizeEventArgs(e.SizingEdge, bounds);
                    }
                }
                else
                {
                    PerformLayout();
                }
            }

            this.retainContainedDesignerLocations = true;
            base.OnResizing(e);
            this.retainContainedDesignerLocations = false;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException("e");

            ISelectionService selectionService = GetService(typeof(ISelectionService)) as ISelectionService;
            object selectedObject = (selectionService != null) ? selectionService.PrimarySelection : null;
            if (selectedObject == null)
                return;

            List<Activity> topLevelActivities = new List<Activity>(Helpers.GetTopLevelActivities(selectionService.GetSelectedComponents()));

            if (e.KeyCode == Keys.Left || e.KeyCode == Keys.Right || e.KeyCode == Keys.Up || e.KeyCode == Keys.Down)
            {
                Size direction = Size.Empty;
                const int step = 5;

                if (e.KeyCode == Keys.Left)
                    direction = new Size(-step, 0);
                else if (e.KeyCode == Keys.Right)
                    direction = new Size(step, 0);
                else if (e.KeyCode == Keys.Up)
                    direction = new Size(0, -step);
                else if (e.KeyCode == Keys.Down)
                    direction = new Size(0, step);

                foreach (Activity selectedActivity in topLevelActivities)
                {
                    //move designer by 'direction'
                    ActivityDesigner designer = ActivityDesigner.GetDesigner(selectedActivity);
                    if (designer != null)
                    {
                        //refresh designer area both before and after move
                        ParentView.InvalidateClientRectangle(new Rectangle(designer.Location, designer.Size));
                        designer.Location += direction;
                        ParentView.InvalidateClientRectangle(new Rectangle(designer.Location, designer.Size));
                    }
                }

                //update layout to grow the parent if needed after all designers have been moved
                //
                PerformLayout();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Delete)
            {
                //if there is a connector selected, delete it
                ICollection components = selectionService.GetSelectedComponents();
                foreach (object component in components)
                {
                    ConnectorHitTestInfo connector = component as ConnectorHitTestInfo;
                    if (connector != null)
                    {
                        FreeformActivityDesigner freeformDesigner = connector.AssociatedDesigner as FreeformActivityDesigner;
                        if (freeformDesigner != null)
                        {
                            ReadOnlyCollection<Connector> connectors = freeformDesigner.Connectors;
                            int connectorIndex = connector.MapToIndex();
                            if (connectorIndex < connectors.Count)
                            {
                                selectionService.SetSelectedComponents(new object[] { connector }, SelectionTypes.Remove);
                                freeformDesigner.RemoveConnector(connectors[connectorIndex]);

                                object nextSelectableObject = freeformDesigner;
                                if (connectors.Count > 0)
                                    nextSelectableObject = new ConnectorHitTestInfo(freeformDesigner, HitTestLocations.Connector | HitTestLocations.Designer, (connectorIndex > 0) ? connectorIndex - 1 : connectorIndex);

                                selectionService.SetSelectedComponents(new object[] { nextSelectableObject }, SelectionTypes.Replace);
                            }
                        }
                    }
                }
                e.Handled = true;
            }

            if (!e.Handled)
            {
                //let the base handle all other keys including tabs
                base.OnKeyDown(e);
            }
        }
        #endregion

        #region Private Methods
        internal static void SetDesignerBounds(ActivityDesigner designer, Rectangle bounds)
        {
            if (designer == null || designer.Activity == null || designer.Activity.Site == null)
                return;

            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(designer);

            PropertyDescriptor sizeProperty = (properties != null) ? properties["Size"] : null;
            if (sizeProperty != null)
                sizeProperty.SetValue(designer.Activity, bounds.Size);
            else
                designer.Size = bounds.Size;

            PropertyDescriptor locationProperty = (properties != null) ? properties["Location"] : null;
            if (locationProperty != null)
                locationProperty.SetValue(designer.Activity, bounds.Location);
            else
                designer.Location = bounds.Location;

            WorkflowView workflowView = designer.Activity.Site.GetService(typeof(WorkflowView)) as WorkflowView;
            if (workflowView != null)
            {
                if (designer.ParentDesigner != null)
                    workflowView.InvalidateLogicalRectangle(designer.ParentDesigner.Bounds);
                else
                    workflowView.Invalidate();
            }
        }

        internal override void OnPaintContainedDesigners(ActivityDesignerPaintEventArgs e)
        {
            if (ShowConnectorsInForeground)
                base.OnPaintContainedDesigners(e);

            FreeformActivityDesigner connectorContainer = ConnectionManager.GetConnectorContainer(this);
            if (connectorContainer != null && Activity != null && Activity.Site != null)
            {
                Region clipRegion = null;
                Region oldClipRegion = e.Graphics.Clip;

                try
                {
                    if (oldClipRegion != null)
                    {
                        clipRegion = new Region(connectorContainer.Bounds);
                        clipRegion.Intersect(e.ViewPort);
                        e.Graphics.Clip = clipRegion;
                    }

                    //Lets draw all the connectors before the designers so that designers always overlap connectors
                    foreach (Connector connector in connectorContainer.Connectors)
                    {
                        if (this == connector.RenderingOwner)
                            connector.OnPaint(e);
                    }
                }
                finally
                {
                    if (oldClipRegion != null)
                    {
                        e.Graphics.Clip = oldClipRegion;
                        clipRegion.Dispose();
                    }
                }
            }

            if (!ShowConnectorsInForeground)
                base.OnPaintContainedDesigners(e);
        }

        private Rectangle GetEnclosingRectangle()
        {
            Point leftTop = new Point(int.MaxValue, int.MaxValue), rightBottom = new Point(int.MinValue, int.MinValue);
            foreach (ActivityDesigner activityDesigner in ContainedDesigners)
            {
                if (activityDesigner.IsVisible)
                {
                    leftTop.X = (activityDesigner.Bounds.Left < leftTop.X) ? activityDesigner.Bounds.Left : leftTop.X;
                    leftTop.Y = (activityDesigner.Bounds.Top < leftTop.Y) ? activityDesigner.Bounds.Top : leftTop.Y;
                    rightBottom.X = (rightBottom.X < activityDesigner.Bounds.Right) ? activityDesigner.Bounds.Right : rightBottom.X;
                    rightBottom.Y = (rightBottom.Y < activityDesigner.Bounds.Bottom) ? activityDesigner.Bounds.Bottom : rightBottom.Y;
                }
            }

            //for the invoked workflow dont take connectors into account
            //this causes un-necessary growth of the workflow...
            if (this.InvokingDesigner == null)
            {
                foreach (Connector connector in Connectors)
                {
                    leftTop.X = (connector.Bounds.Left < leftTop.X) ? connector.Bounds.Left : leftTop.X;
                    leftTop.Y = (connector.Bounds.Top < leftTop.Y) ? connector.Bounds.Top : leftTop.Y;
                    rightBottom.X = (rightBottom.X < connector.Bounds.Right) ? connector.Bounds.Right : rightBottom.X;
                    rightBottom.Y = (rightBottom.Y < connector.Bounds.Bottom) ? connector.Bounds.Bottom : rightBottom.Y;
                }
            }

            Rectangle enclosingRectangle = Rectangle.Empty;
            if (leftTop.X != int.MaxValue && rightBottom.X != int.MinValue)
            {
                enclosingRectangle.X = leftTop.X;
                enclosingRectangle.Width = rightBottom.X - leftTop.X;
            }
            if (leftTop.Y != int.MaxValue && rightBottom.Y != int.MinValue)
            {
                enclosingRectangle.Y = leftTop.Y;
                enclosingRectangle.Height = rightBottom.Y - leftTop.Y;
            }

            if (!enclosingRectangle.IsEmpty)
                enclosingRectangle.Inflate(AutoSizeMargin);

            return enclosingRectangle;
        }

        internal bool CanUpdateZOrder(ActivityDesigner activityDesigner, ZOrder zorder)
        {
            bool canUpdateZOrder = false;
            CompositeActivityDesigner parentDesigner = this;
            ActivityDesigner childDesigner = activityDesigner;
            while (parentDesigner != null && childDesigner != null)
            {
                if (parentDesigner is FreeformActivityDesigner)
                {
                    ReadOnlyCollection<ActivityDesigner> containedDesigners = parentDesigner.ContainedDesigners;
                    if (containedDesigners.Count > 1 && containedDesigners[(zorder == ZOrder.Background) ? 0 : containedDesigners.Count - 1] != childDesigner)
                    {
                        canUpdateZOrder = true;
                        break;
                    }
                }

                childDesigner = parentDesigner;
                parentDesigner = parentDesigner.ParentDesigner;
            }

            return canUpdateZOrder;
        }

        private void UpdateZOrder(ActivityDesigner activityDesigner, ZOrder zorder)
        {
            IDesignerHost designerHost = GetService(typeof(IDesignerHost)) as IDesignerHost;
            DesignerTransaction transaction = null;
            if (designerHost != null)
                transaction = designerHost.CreateTransaction(DR.GetString(DR.ZOrderUndoDescription, activityDesigner.Text));

            try
            {
                bool zOrderChanged = false;
                CompositeActivityDesigner parentDesigner = this;
                ActivityDesigner childDesigner = activityDesigner;
                while (parentDesigner != null && childDesigner != null)
                {
                    if (parentDesigner is FreeformActivityDesigner)
                    {
                        ReadOnlyCollection<ActivityDesigner> containedDesigners = parentDesigner.ContainedDesigners;
                        if (containedDesigners.Count > 1 && containedDesigners[(zorder == ZOrder.Background) ? 0 : containedDesigners.Count - 1] != childDesigner)
                        {
                            int moveIndex = (zorder == ZOrder.Background) ? 0 : containedDesigners.Count;
                            parentDesigner.MoveActivities(new ConnectorHitTestInfo(this, HitTestLocations.Designer, moveIndex), new List<Activity>(new Activity[] { childDesigner.Activity }).AsReadOnly());
                            zOrderChanged = true;
                        }
                    }

                    childDesigner = parentDesigner;
                    parentDesigner = parentDesigner.ParentDesigner;
                }

                if (zOrderChanged)
                    Invalidate();

                if (transaction != null)
                    transaction.Commit();
            }
            catch (Exception e)
            {
                if (transaction != null)
                    transaction.Cancel();

                throw e;
            }
        }

        private void EnsureDesignerExtender()
        {
            bool addExtender = true;
            IExtenderListService extenderListService = GetService(typeof(IExtenderListService)) as IExtenderListService;
            if (extenderListService != null)
            {
                foreach (IExtenderProvider extenderProvider in extenderListService.GetExtenderProviders())
                {
                    if (extenderProvider.GetType() == typeof(FreeFormDesignerPropertyExtender))
                    {
                        addExtender = false;
                        break;
                    }
                }
            }

            if (addExtender)
            {
                IExtenderProviderService extenderProviderService = GetService(typeof(IExtenderProviderService)) as IExtenderProviderService;
                if (extenderProviderService != null)
                {
                    extenderProviderService.AddExtenderProvider(new FreeFormDesignerPropertyExtender());
                    TypeDescriptor.Refresh(Activity);
                }
            }
        }
        #endregion

        #endregion

        #region Private Classes

        #region Class FreeFormConnectorSelectionGlyph
        //
        private sealed class FreeFormConnectorSelectionGlyph : ConnectorSelectionGlyph
        {
            internal FreeFormConnectorSelectionGlyph(int connectorIndex, bool isPrimarySelectionGlyph)
                : base(connectorIndex, isPrimarySelectionGlyph)
            {
            }

            public override Rectangle GetBounds(ActivityDesigner designer, bool activated)
            {
                //FreeformActivityDesigner connectorContainer = designer as FreeformActivityDesigner;
                //return (connectorContainer != null) ? DesignerGeometryHelper.RectangleFromLineSegments(connectorContainer.Connectors[this.connectorIndex].ConnectorSegments) : Rectangle.Empty;
                return Rectangle.Empty;
            }

            protected override void OnPaint(Graphics graphics, bool activated, AmbientTheme ambientTheme, ActivityDesigner designer)
            {
                /*IConnectorContainer connectorContainer = designer as IConnectorContainer;
                if (connectorContainer == null)
                    return;

                Connector connector = connectorContainer[this.connectorIndex];
                Rectangle[] grabHandles = new Rectangle[connector.Segments.Length];
                for (int i = 0; i < connector.Segments.Length; i++)
                {
                    grabHandles[i].X = connector.Segments[i].X - ambientTheme.SelectionSize.Width / 2;
                    grabHandles[i].Y = connector.Segments[i].Y - ambientTheme.SelectionSize.Height / 2;
                    grabHandles[i].Size = ambientTheme.SelectionSize;
                }

                ActivityDesignerPaint.DrawGrabHandles(graphics, grabHandles, this.isPrimarySelectionGlyph);*/
            }

            public override bool IsPrimarySelection
            {
                get
                {
                    return this.isPrimarySelectionGlyph;
                }
            }
        }
        #endregion

        #region Class FreeFormDesignerPropertyExtender
        [ProvideProperty("Location", typeof(Activity))]
        [ProvideProperty("Size", typeof(Activity))]
        private sealed class FreeFormDesignerPropertyExtender : IExtenderProvider
        {
            #region Properties
            [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            [DesignOnly(true)]
            [MergableProperty(false)]
            [Browsable(false)]
            public Point GetLocation(Activity activity)
            {
                Point location = Point.Empty;
                ActivityDesigner designer = ActivityDesigner.GetDesigner(activity);
                if (designer != null)
                    location = designer.Location;
                return location;
            }

            public void SetLocation(Activity activity, Point location)
            {
                ActivityDesigner designer = ActivityDesigner.GetDesigner(activity);
                if (designer != null)
                {
                    FreeformActivityDesigner freeformDesigner = (designer.ParentDesigner != null) ? designer.ParentDesigner as FreeformActivityDesigner : designer as FreeformActivityDesigner;
                    if (freeformDesigner != null)
                    {
                        designer.Location = location;

                        if (freeformDesigner.AutoSize)
                            freeformDesigner.PerformLayout();
                    }
                }
            }

            [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            [DesignOnly(true)]
            [MergableProperty(false)]
            [Browsable(false)]
            public Size GetSize(Activity activity)
            {
                Size size = Size.Empty;
                ActivityDesigner designer = ActivityDesigner.GetDesigner(activity);
                if (designer != null)
                    size = designer.Size;
                return size;
            }

            public void SetSize(Activity activity, Size size)
            {
                ActivityDesigner designer = ActivityDesigner.GetDesigner(activity);
                if (designer != null)
                {
                    FreeformActivityDesigner freeformDesigner = (designer.ParentDesigner != null) ? designer.ParentDesigner as FreeformActivityDesigner : designer as FreeformActivityDesigner;
                    if (freeformDesigner != null)
                    {
                        designer.Size = size;

                        if (freeformDesigner.AutoSize)
                            freeformDesigner.PerformLayout();
                    }
                }
            }

            #endregion
            bool IExtenderProvider.CanExtend(object extendee)
            {
                bool canExtend = false;

                Activity activity = extendee as Activity;
                if (activity != null)
                {
                    ActivityDesigner designer = ActivityDesigner.GetDesigner(activity);
                    if (designer != null)
                    {
                        FreeformActivityDesigner freeformDesigner = (designer.ParentDesigner != null) ? designer.ParentDesigner as FreeformActivityDesigner : designer as FreeformActivityDesigner;
                        if (freeformDesigner != null)
                            canExtend = true;
                    }
                }

                return canExtend;
            }
        }
        #endregion

        #endregion
    }
    #endregion

    #region Class FreeFormDesignerVerbProvider
    internal sealed class FreeFormDesignerVerbProvider : IDesignerVerbProvider
    {
        #region IDesignerVerbProvider Members
        ActivityDesignerVerbCollection IDesignerVerbProvider.GetVerbs(ActivityDesigner activityDesigner)
        {
            ActivityDesignerVerbCollection verbs = new ActivityDesignerVerbCollection();
            if (activityDesigner.ParentDesigner is FreeformActivityDesigner)
            {
                ActivityDesignerVerb verb = new ActivityDesignerVerb(activityDesigner, DesignerVerbGroup.Actions, DR.GetString(DR.BringToFront), new EventHandler(OnZOrderChanged), new EventHandler(OnZOrderStatusUpdate));
                verb.Properties[DesignerUserDataKeys.ZOrderKey] = ZOrder.Foreground;
                verbs.Add(verb);
                verb = new ActivityDesignerVerb(activityDesigner, DesignerVerbGroup.Actions, DR.GetString(DR.SendToBack), new EventHandler(OnZOrderChanged), new EventHandler(OnZOrderStatusUpdate));
                verb.Properties[DesignerUserDataKeys.ZOrderKey] = ZOrder.Background;
                verbs.Add(verb);
            }
            return verbs;
        }
        #endregion

        #region Helpers
        private void OnZOrderChanged(object sender, EventArgs e)
        {
            ActivityDesignerVerb designerVerb = sender as ActivityDesignerVerb;
            if (designerVerb != null && designerVerb.Properties.Contains(DesignerUserDataKeys.ZOrderKey))
            {
                FreeformActivityDesigner freeformDesigner = designerVerb.ActivityDesigner.ParentDesigner as FreeformActivityDesigner;
                if (freeformDesigner != null)
                {
                    if ((ZOrder)designerVerb.Properties[DesignerUserDataKeys.ZOrderKey] == ZOrder.Foreground)
                        freeformDesigner.BringToFront(designerVerb.ActivityDesigner);
                    else if ((ZOrder)designerVerb.Properties[DesignerUserDataKeys.ZOrderKey] == ZOrder.Background)
                        freeformDesigner.SendToBack(designerVerb.ActivityDesigner);
                }
            }
        }

        private void OnZOrderStatusUpdate(object sender, EventArgs e)
        {
            ActivityDesignerVerb designerVerb = sender as ActivityDesignerVerb;
            if (designerVerb != null && designerVerb.Properties.Contains(DesignerUserDataKeys.ZOrderKey))
            {
                FreeformActivityDesigner freeformDesigner = designerVerb.ActivityDesigner.ParentDesigner as FreeformActivityDesigner;
                if (freeformDesigner != null)
                    designerVerb.Enabled = freeformDesigner.CanUpdateZOrder(designerVerb.ActivityDesigner, (ZOrder)designerVerb.Properties[DesignerUserDataKeys.ZOrderKey]);
            }
        }
        #endregion
    }
    #endregion
}
