#pragma warning disable 1634, 1691
namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.IO;
    using System.Drawing;
    using System.CodeDom;
    using System.Diagnostics;
    using System.Collections;
    using System.Collections.Generic;
    using System.Windows.Forms;
    using System.ComponentModel;
    using System.Globalization;
    using System.Drawing.Design;
    using System.Drawing.Imaging;
    using System.Drawing.Drawing2D;
    using System.Windows.Forms.Design;
    using System.ComponentModel.Design;
    using System.Collections.Specialized;
    using System.ComponentModel.Design.Serialization;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.ComponentModel.Serialization;
    using System.Collections.ObjectModel;
    using System.Reflection;
    using System.Workflow.ComponentModel.Design;
    using System.Runtime.Serialization.Formatters.Binary;

    //



    #region StructuredCompositeActivityDesigner Class
    /// <summary>
    /// Base class for CompositActivityDesigner which have a structured layouts where contained ContainedDesigners
    /// are connected to each other using connectors. Class is used when the user needs to provide different types 
    /// of layouts for CompositeActivityDesigner
    /// </summary>
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public abstract class StructuredCompositeActivityDesigner : CompositeActivityDesigner
    {
        #region Fields
        private int currentDropTarget = -1;

        private List<DesignerView> views = null;
        private DesignerView activeView;
        private ItemPalette itemPalette = null;
        #endregion

        #region Properties

        #region Public Properties
        public override ReadOnlyCollection<ActivityDesigner> ContainedDesigners
        {
            get
            {
                List<ActivityDesigner> containedDesigners = new List<ActivityDesigner>();
                ActivityDesigner activeDesigner = ActiveDesigner;
                if (activeDesigner != null)
                {
                    if (activeDesigner == this)
                    {
                        //We need to remove the secondary activities
                        containedDesigners.AddRange(base.ContainedDesigners);

                        List<ActivityDesigner> designersToRemove = new List<ActivityDesigner>();
                        IList<ActivityDesigner> mappedDesigners = DesignersFromSupportedViews;

                        foreach (ActivityDesigner containedDesigner in containedDesigners)
                        {
                            bool isAlternateFlowActivityAttribute = Helpers.IsAlternateFlowActivity(containedDesigner.Activity);
                            if (mappedDesigners.Contains(containedDesigner) || isAlternateFlowActivityAttribute)
                                designersToRemove.Add(containedDesigner);
                        }

                        foreach (ActivityDesigner activityDesigner in designersToRemove)
                            containedDesigners.Remove(activityDesigner);
                    }
                    else
                    {
                        containedDesigners.Add(activeDesigner);
                    }
                }

                return containedDesigners.AsReadOnly();
            }
        }

        public override object FirstSelectableObject
        {
            get
            {
                ActivityDesigner activeDesigner = ActiveDesigner;
                if (activeDesigner != null && activeDesigner != this)
                    return activeDesigner.Activity;
                else
                    return base.FirstSelectableObject;
            }
        }

        public override object LastSelectableObject
        {
            get
            {
                ActivityDesigner activeDesigner = ActiveDesigner;
                if (activeDesigner != null && activeDesigner != this && activeDesigner is CompositeActivityDesigner)
                    return ((CompositeActivityDesigner)activeDesigner).LastSelectableObject;
                else
                    return base.LastSelectableObject;
            }
        }

        /// <summary>
        /// Gets the ActiveView supported by the designer
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DesignerView ActiveView
        {
            get
            {
                if (this.activeView == null)
                    this.activeView = ValidatedViews[0];
                return this.activeView;
            }

            set
            {
                if (this.activeView == value || value == null)
                    return;

                DesignerView previousView = this.activeView;

                this.activeView = value;

                value.OnActivate();

                ActivityDesigner designer = value.AssociatedDesigner;
                if (designer == null)
                {
                    value.OnDeactivate();
                    this.activeView = previousView;
                    return;
                }

                if (previousView != null)
                    previousView.OnDeactivate();

                OnViewChanged(this.activeView);

                //When we change the view we want to make sure that if we hide any of the child
                //activities with errors we want to reveal these activities
                DesignerHelpers.RefreshDesignerActions(Activity.Site);

                //Keep the dynamic action and designer verbs in [....]
                RefreshDesignerVerbs();
            }
        }

        public override Size MinimumSize
        {
            get
            {
                Size minimumSize = base.MinimumSize;

                ActivityDesigner activeDesigner = ActiveDesigner;
                if (activeDesigner != null && activeDesigner != this && Expanded)
                {
                    minimumSize.Width = Math.Max(minimumSize.Width, 160);
                    minimumSize.Height = Math.Max(minimumSize.Height, 160);
                }

                return minimumSize;
            }
        }

        /// <summary>
        /// Gets the array of views supported by the current designer
        /// </summary>
        public virtual ReadOnlyCollection<DesignerView> Views
        {
            get
            {
                if (this.views == null)
                {
                    this.views = new List<DesignerView>();
                    this.views.AddRange(SecondaryViewProvider.GetViews(this));
                }

                return this.views.AsReadOnly();
            }
        }
        #endregion

        #region Protected Properties
        /// <summary>
        /// Gets the index of the cuurent drop target in the array of drop targets returned by method GetDropTargets
        /// This property is only used when the drag drop operation is in progress
        /// </summary>
        protected virtual int CurrentDropTarget
        {
            get
            {
                return this.currentDropTarget;
            }

            set
            {
                this.currentDropTarget = value;
                Invalidate();
            }
        }

        protected override bool ShowSmartTag
        {
            get
            {
                return (!String.IsNullOrEmpty(Text) && !TextRectangle.Size.IsEmpty && Views.Count > 1);
            }
        }

        protected override ReadOnlyCollection<ActivityDesignerVerb> SmartTagVerbs
        {
            get
            {
                List<ActivityDesignerVerb> smartTagVerbs = new List<ActivityDesignerVerb>(base.SmartTagVerbs);

                //Return smarttag actions only if there is more than one view
                if (Views.Count > 1)
                {
                    for (int i = 0; i < Views.Count; i++)
                    {
                        DesignerView view = Views[i];
                        ActivityDesignerVerb smartVerb = new ActivityDesignerVerb(this, DesignerVerbGroup.Actions, view.Text, new EventHandler(OnSmartTagVerb), new EventHandler(OnSmartTagVerbStatus));
                        smartVerb.Properties[DesignerUserDataKeys.DesignerView] = view;
                        smartVerb.Properties[DesignerUserDataKeys.Image] = view.Image;
                        smartTagVerbs.Add(smartVerb);
                    }
                }

                return smartTagVerbs.AsReadOnly();
            }
        }
        #endregion

        #region Private Properties
        internal ActivityDesigner ActiveDesigner
        {
            get
            {
                if (ActiveView != null)
                    return ActiveView.AssociatedDesigner;
                else
                    return null;
            }
        }

        internal override bool SmartTagVisible
        {
            get
            {
                if (this.itemPalette != null && this.itemPalette.IsVisible)
                    return true;

                return base.SmartTagVisible;
            }

            set
            {
                base.SmartTagVisible = value;
            }
        }

        private ReadOnlyCollection<DesignerView> ValidatedViews
        {
            get
            {
                ReadOnlyCollection<DesignerView> views = Views;
                if (views.Count == 0)
#pragma warning suppress 56503
                    throw new InvalidOperationException(DR.GetString(DR.Error_MultiviewSequentialActivityDesigner));
                return views;
            }
        }

        private IList<ActivityDesigner> DesignersFromSupportedViews
        {
            get
            {
                List<ActivityDesigner> mappedDesigners = new List<ActivityDesigner>();
                foreach (DesignerView view in ValidatedViews)
                {
                    ActivityDesigner mappedDesigner = view.AssociatedDesigner;
                    if (mappedDesigner != null)
                        mappedDesigners.Add(mappedDesigner);
                }

                return mappedDesigners.AsReadOnly();
            }
        }
        #endregion

        #endregion

        #region Methods

        #region Public Methods
        public override bool CanInsertActivities(HitTestInfo insertLocation, ReadOnlyCollection<Activity> activitiesToInsert)
        {
            if (insertLocation == null)
                throw new ArgumentNullException("insertLocation");

            if (activitiesToInsert == null)
                throw new ArgumentNullException("activitiesToInsert");

            ActivityDesigner hostedDesigner = (ActiveView != null) ? ActiveView.AssociatedDesigner : null;
            if (hostedDesigner != this)
                return false;

            IList<Type> secondaryViewTypes = SecondaryViewProvider.GetActivityTypes(this);
            foreach (Activity activity in activitiesToInsert)
            {
                if (activity == null)
                    throw new ArgumentException("activitiesToInsert", SR.GetString(SR.Error_CollectionHasNullEntry));

                if (secondaryViewTypes.Contains(activity.GetType()))
                    return false;
            }

            return base.CanInsertActivities(GetUpdatedLocation(insertLocation), activitiesToInsert);
        }

        public override void InsertActivities(HitTestInfo insertLocation, ReadOnlyCollection<Activity> activitiesToInsert)
        {
            if (insertLocation == null)
                throw new ArgumentNullException("insertLocation");

            if (activitiesToInsert == null)
                throw new ArgumentNullException("activitiesToInsert");

            base.InsertActivities(GetUpdatedLocation(insertLocation), activitiesToInsert);
        }

        public override void MoveActivities(HitTestInfo moveLocation, ReadOnlyCollection<Activity> activitiesToMove)
        {
            if (moveLocation == null)
                throw new ArgumentNullException("moveLocation");

            if (activitiesToMove == null)
                throw new ArgumentNullException("activitiesToMove");

            base.MoveActivities(GetUpdatedLocation(moveLocation), activitiesToMove);
        }

        public override bool CanRemoveActivities(ReadOnlyCollection<Activity> activitiesToRemove)
        {
            if (activitiesToRemove == null)
                throw new ArgumentNullException("activitiesToRemove");

            return base.CanRemoveActivities(activitiesToRemove);
        }

        public override void EnsureVisibleContainedDesigner(ActivityDesigner containedDesigner)
        {
            if (containedDesigner == null)
                throw new ArgumentNullException("containedDesigner");

            //we could be collapsed, make sure the view itself is visible
            this.Expanded = true;

            ActivityDesigner activeDesigner = ActiveDesigner;
            if (containedDesigner != activeDesigner && containedDesigner != this)
            {
                DesignerView viewToActivate = null;
                ReadOnlyCollection<DesignerView> views = ValidatedViews;

                //Go thru the views and check if the child designer is one of the views
                foreach (DesignerView view in views)
                {
                    if (containedDesigner == view.AssociatedDesigner)
                    {
                        viewToActivate = view;
                        break;
                    }
                }

                //This means that the child designer is in our main flow
                if (viewToActivate == null)
                    viewToActivate = views[0];

                ActiveView = viewToActivate;

                //Invoking a verb might change the shown view so we map again
                CompositeActivityDesigner activeCompositeDesigner = ActiveDesigner as CompositeActivityDesigner;
                if (activeCompositeDesigner != null)
                {
                    if (activeCompositeDesigner != this)
                        activeCompositeDesigner.EnsureVisibleContainedDesigner(containedDesigner);
                    else
                        base.EnsureVisibleContainedDesigner(containedDesigner);
                }
            }
        }

        public override object GetNextSelectableObject(object current, DesignerNavigationDirection direction)
        {
            object nextObject = null;

            ActivityDesigner activeDesigner = ActiveDesigner;
            if (activeDesigner != null)
            {
                if (activeDesigner != this)
                {
                    if (current != activeDesigner.Activity && activeDesigner is CompositeActivityDesigner)
                        nextObject = ((CompositeActivityDesigner)activeDesigner).GetNextSelectableObject(current, direction);
                }
                else
                {
                    nextObject = base.GetNextSelectableObject(current, direction);
                }
            }

            return nextObject;
        }
        #endregion

        #region Protected Methods
        protected override void Initialize(Activity activity)
        {
            base.Initialize(activity);

            ActiveView = ValidatedViews[0];
        }

        /// <summary>
        /// Returns the collection of points which represents the inner connections of the designer. The designer can have connectors
        /// within it, the points returned are the connection points used for connectable designer.
        /// </summary>
        /// <param name="edges">Designer Edge along which the connection point lies</param>
        /// <returns>List of connection Points</returns>
        protected virtual ReadOnlyCollection<Point> GetInnerConnections(DesignerEdges edges)
        {
            List<Point> connectionPoints = new List<Point>(GetConnections(edges));
            if (connectionPoints.Count > 0 && (edges & DesignerEdges.Top) > 0)
                connectionPoints[0] = new Point(connectionPoints[0].X, connectionPoints[0].Y + TitleHeight);
            return connectionPoints.AsReadOnly();
        }

        /// <summary>
        /// Returns array of rectangles representing the valid drop locations with the designer
        /// </summary>
        /// <param name="dropPoint"></param>
        /// <returns></returns>
        protected virtual Rectangle[] GetDropTargets(Point dropPoint)
        {
            return new Rectangle[] { Bounds };
        }

        protected override void OnContainedActivitiesChanging(ActivityCollectionChangeEventArgs listChangeArgs)
        {
            base.OnContainedActivitiesChanging(listChangeArgs);

            if (listChangeArgs.Action == ActivityCollectionChangeAction.Remove && listChangeArgs.RemovedItems[0] != null)
            {
                ActivityDesigner activeDesigner = ActiveDesigner;
                if (activeDesigner != null && listChangeArgs.RemovedItems[0] == activeDesigner.Activity)
                    ActiveView = ValidatedViews[0];
                SecondaryViewProvider.OnViewRemoved(this, listChangeArgs.RemovedItems[0].GetType());
            }
        }

        protected void DrawConnectors(Graphics graphics, Pen pen, Point[] points, LineAnchor startCap, LineAnchor endCap)
        {
            Size arrowCapSize = Size.Empty;
            Size maxCapSize = Size.Empty;

            CompositeDesignerTheme compositeDesignerTheme = DesignerTheme as CompositeDesignerTheme;
            if (compositeDesignerTheme != null)
            {
                arrowCapSize = new Size(compositeDesignerTheme.ConnectorSize.Width / 3, compositeDesignerTheme.ConnectorSize.Height / 3);
                maxCapSize = compositeDesignerTheme.ConnectorSize;
            }
            ActivityDesignerPaint.DrawConnectors(graphics, pen, points, arrowCapSize, maxCapSize, startCap, endCap);
        }

        protected override void OnDragEnter(ActivityDragEventArgs e)
        {
            base.OnDragEnter(e);

            CurrentDropTarget = CanDrop(e);
            e.Effect = CheckDragEffect(e);
            e.DragImageSnapPoint = SnapInToDropTarget(e);
        }

        protected override void OnDragOver(ActivityDragEventArgs e)
        {
            base.OnDragOver(e);

            CurrentDropTarget = CanDrop(e);
            e.Effect = CheckDragEffect(e);
            e.DragImageSnapPoint = SnapInToDropTarget(e);
        }

        protected override void OnDragLeave()
        {
            base.OnDragLeave();

            //Clear earlier drop target information
            CurrentDropTarget = -1;
        }

        protected override void OnDragDrop(ActivityDragEventArgs e)
        {
            base.OnDragDrop(e);

            bool ctrlKeyPressed = ((e.KeyState & 8) == 8);
            if (ctrlKeyPressed && (e.AllowedEffect & DragDropEffects.Copy) == DragDropEffects.Copy)
                e.Effect = DragDropEffects.Copy;
            else if ((e.AllowedEffect & DragDropEffects.Move) == DragDropEffects.Move)
                e.Effect = DragDropEffects.Move;

            //If the component is sited then that means that we are moving it 
            try
            {
                CompositeActivityDesigner.InsertActivities(this, new ConnectorHitTestInfo(this, HitTestLocations.Designer, CurrentDropTarget), e.Activities, SR.GetString(SR.DragDropActivities));
            }
            finally
            {
                CurrentDropTarget = -1;
            }
        }

        protected override void OnLayoutPosition(ActivityDesignerLayoutEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException("e");

            base.OnLayoutPosition(e);

            if (Expanded)
            {
                ActivityDesigner activeDesigner = ActiveDesigner;
                if (activeDesigner != null && activeDesigner != this)
                {
                    Point location = Location;
                    location.X += (Size.Width - activeDesigner.Size.Width) / 2;
                    location.Y += e.AmbientTheme.SelectionSize.Height;
                    activeDesigner.Location = location;
                }

                int titleHeight = TitleHeight;
                foreach (ActivityDesigner activityDesigner in ContainedDesigners)
                    activityDesigner.Location = new Point(activityDesigner.Location.X, activityDesigner.Location.Y + titleHeight);
            }
        }

        protected override Size OnLayoutSize(ActivityDesignerLayoutEventArgs e)
        {
            Size containerSize = base.OnLayoutSize(e);

            if (Expanded)
            {
                ActivityDesigner activeDesigner = ActiveDesigner;
                if (activeDesigner != null && activeDesigner != this)
                {
                    containerSize.Width = Math.Max(containerSize.Width, activeDesigner.Size.Width);
                    containerSize.Height += activeDesigner.Size.Height;

                    containerSize.Width += 2 * e.AmbientTheme.SelectionSize.Width;
                    containerSize.Width += 3 * e.AmbientTheme.Margin.Width;
                    containerSize.Height += e.AmbientTheme.Margin.Height;
                    containerSize.Height += 2 * e.AmbientTheme.SelectionSize.Height;
                }
            }

            return containerSize;
        }

        protected override void SaveViewState(BinaryWriter writer)
        {
            if (writer == null)
                throw new ArgumentNullException("writer");

            List<DesignerView> views = new List<DesignerView>(ValidatedViews);
            writer.Write("ActiveView");
            writer.Write(views.IndexOf(this.activeView));

            base.SaveViewState(writer);
        }

        protected override void LoadViewState(BinaryReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");

            string str = reader.ReadString();
            if (str != null && str.Equals("ActiveView", StringComparison.Ordinal))
            {
                int activeDesignerIndex = reader.ReadInt32();
                ReadOnlyCollection<DesignerView> views = ValidatedViews;
                if (activeDesignerIndex != -1 && activeDesignerIndex < views.Count)
                    ActiveView = views[activeDesignerIndex];
            }

            base.LoadViewState(reader);
        }

        /// <summary>
        /// Called when the current view of the designer changes
        /// </summary>
        /// <param name="view">View which is being set.</param>
        protected virtual void OnViewChanged(DesignerView view)
        {
            PerformLayout();
        }

        protected override void OnShowSmartTagVerbs(Point smartTagPoint)
        {
            if (this.itemPalette == null)
            {
                this.itemPalette = new ItemPalette();
                this.itemPalette.Closed += new EventHandler(OnPaletteClosed);
                this.itemPalette.SelectionChanged += new SelectionChangeEventHandler<SelectionChangeEventArgs>(OnSmartAction);
            }

            //we need to update the font every time the menu is shown
            this.itemPalette.SetFont(WorkflowTheme.CurrentTheme.AmbientTheme.Font);

            this.itemPalette.Items.Clear();

            foreach (ActivityDesignerVerb smartVerb in SmartTagVerbs)
            {
                Image image = smartVerb.Properties[DesignerUserDataKeys.Image] as Image;
                ItemInfo smartVerbItem = new ItemInfo(smartVerb.Id, image, smartVerb.Text);
                smartVerbItem.UserData[DesignerUserDataKeys.DesignerVerb] = smartVerb;
                this.itemPalette.Items.Add(smartVerbItem);
            }

            Point location = PointToScreen(smartTagPoint);
            this.itemPalette.Show(location);
        }

        protected override void OnActivityChanged(ActivityChangedEventArgs e)
        {
            ReadOnlyCollection<DesignerView> newViews = SecondaryViewProvider.GetViews(this);
            ReadOnlyCollection<DesignerView> oldViews = Views;
            if (newViews.Count != oldViews.Count)
            {
                this.views = null;

                //


                PerformLayout();
            }

            base.OnActivityChanged(e);
        }
        #endregion

        #region Private Methods
        internal override void OnPaintContainedDesigners(ActivityDesignerPaintEventArgs e)
        {
            //Draw all the activity designers contained by the activity designer
            //We know that all the children which are in drawing range will be always
            //consecutive both for parallel and for sequential containers hence
            //once we go in the invisible range we bail out of drawing logic for rest of
            //the children
            bool bDrawingVisibleChildren = false;

            foreach (ActivityDesigner activityDesigner in ContainedDesigners)
            {
                Rectangle designerBounds = activityDesigner.Bounds;
                if (e.ViewPort.IntersectsWith(designerBounds))
                {
                    bDrawingVisibleChildren = true;
                    using (PaintEventArgs paintEventArgs = new PaintEventArgs(e.Graphics, e.ViewPort))
                    {
                        ((IWorkflowDesignerMessageSink)activityDesigner).OnPaint(paintEventArgs, e.ViewPort);
                    }
                }
                else
                {
                    if (bDrawingVisibleChildren)
                        break;
                }
            }
        }

        private Point SnapInToDropTarget(ActivityDragEventArgs e)
        {
            if (CurrentDropTarget >= 0)
            {
                Rectangle[] dropTargets = GetDropTargets(new Point(e.X, e.Y));
                if (CurrentDropTarget < dropTargets.Length)
                {
                    Rectangle dropConnector = dropTargets[CurrentDropTarget];
                    return new Point(dropConnector.Left + dropConnector.Width / 2, dropConnector.Top + dropConnector.Height / 2);
                }
            }

            return Point.Empty;
        }

        private int CanDrop(ActivityDragEventArgs e)
        {
            if (e.Activities.Count == 0)
                return -1;

            Point dropPoint = new Point(e.X, e.Y);
            int dropIndex = -1;
            Rectangle[] dropTargets = GetDropTargets(dropPoint);
            for (int i = 0; i < dropTargets.Length; i++)
            {
                if (dropTargets[i].Contains(dropPoint))
                {
                    dropIndex = i;
                    break;
                }
            }

            if (dropIndex >= 0 && !CanInsertActivities(new ConnectorHitTestInfo(this, HitTestLocations.Designer, dropIndex), e.Activities))
                dropIndex = -1;

            bool ctrlKeyPressed = ((e.KeyState & 8) == 8);
            if (dropIndex >= 0 && !ctrlKeyPressed && (e.AllowedEffect & DragDropEffects.Move) == DragDropEffects.Move)
            {
                ConnectorHitTestInfo moveLocation = new ConnectorHitTestInfo(this, HitTestLocations.Designer, dropIndex);
                foreach (Activity activity in e.Activities)
                {
                    if (activity.Site != null)
                    {
                        ActivityDesigner activityDesigner = ActivityDesigner.GetDesigner(activity);
                        if (activityDesigner == null || activityDesigner.ParentDesigner == null || !activityDesigner.ParentDesigner.CanMoveActivities(moveLocation, new List<Activity>(new Activity[] { activity }).AsReadOnly()))
                        {
                            dropIndex = -1;
                            break;
                        }
                    }
                }
            }

            return dropIndex;
        }

        private DragDropEffects CheckDragEffect(ActivityDragEventArgs e)
        {
            if (e.Activities.Count == 0)
            {
                return DragDropEffects.None;
            }
            else if (CurrentDropTarget >= 0)
            {
                bool ctrlKeyPressed = ((e.KeyState & 8) == 8);
                if (ctrlKeyPressed && (e.AllowedEffect & DragDropEffects.Copy) == DragDropEffects.Copy)
                    return DragDropEffects.Copy;
                else if ((e.AllowedEffect & DragDropEffects.Move) == DragDropEffects.Move)
                    return DragDropEffects.Move;
            }

            return e.Effect;
        }

        private void OnSmartTagVerbStatus(object sender, EventArgs e)
        {
            ActivityDesignerVerb verb = sender as ActivityDesignerVerb;

            DesignerView view = verb.Properties[DesignerUserDataKeys.DesignerView] as DesignerView;
            if (view != null)
                verb.Checked = (view == ActiveView);
        }

        private void OnSmartTagVerb(object sender, EventArgs e)
        {
            ActivityDesignerVerb verb = sender as ActivityDesignerVerb;

            DesignerView view = verb.Properties[DesignerUserDataKeys.DesignerView] as DesignerView;
            if (view != null)
            {
                ActiveView = view;

                if (Expanded && view.AssociatedDesigner != null)
                {
                    ISelectionService selectionService = GetService(typeof(ISelectionService)) as ISelectionService;
                    if (selectionService != null)
                        selectionService.SetSelectedComponents(new object[] { view.AssociatedDesigner.Activity }, SelectionTypes.Replace);
                }
            }
        }

        private void OnSmartAction(object sender, SelectionChangeEventArgs e)
        {
            ItemInfo itemInfo = e.CurrentItem as ItemInfo;
            if (itemInfo != null)
            {
                ActivityDesignerVerb smartVerb = itemInfo.UserData[DesignerUserDataKeys.DesignerVerb] as ActivityDesignerVerb;
                if (smartVerb != null)
                    smartVerb.Invoke();
            }
        }

        private void OnPaletteClosed(object sender, EventArgs e)
        {
            Invalidate(DesignerSmartTag.GetBounds(this, true));
        }

        private HitTestInfo GetUpdatedLocation(HitTestInfo location)
        {
            int lockedActivityOffset = 0;
            foreach (DesignerView secondaryView in Views)
            {
                if (secondaryView.AssociatedDesigner != null &&
                    this != secondaryView.AssociatedDesigner &&
                    Helpers.IsActivityLocked(secondaryView.AssociatedDesigner.Activity))
                {
                    lockedActivityOffset++;
                }
            }

            return new ConnectorHitTestInfo(this, location.HitLocation, lockedActivityOffset + location.MapToIndex());
        }
        #endregion

        #endregion
    }
    #endregion

}
