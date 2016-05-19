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

    #region ActivityPreviewDesigner Class
    /// <summary>
    /// Base class to display collection of activities one at a time. The activities can be either previewed or
    /// can be edited directly.
    /// </summary>
    [ActivityDesignerTheme(typeof(ActivityPreviewDesignerTheme))]
    [SRCategory("ActivityPreviewDesigners", "System.Workflow.ComponentModel.Design.DesignerResources")]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class ActivityPreviewDesigner : SequentialActivityDesigner
    {
        #region Fields
        private ActivityCollectionAccessibleObject accessibilityObject;
        private ActivityDesignerVerbCollection designerVerbs;

        private PreviewItemStrip previewStrip;
        private PreviewWindow previewWindow;

        private Point[] separatorLine = new Point[2];
        private bool removePreviewedDesigner = false;
        #endregion

        #region Constructor
        /// <summary>
        /// Default constructor to create ActivityPreviewDesigner
        /// </summary>
        public ActivityPreviewDesigner()
        {
            this.previewStrip = new PreviewItemStrip(this);
            this.previewStrip.SelectionChanged += new SelectionChangeEventHandler<SelectionChangeEventArgs>(OnPreviewChanged);
            this.previewStrip.HelpText = DR.GetString(DR.DropActivitiesHere);
        }
        #endregion

        #region Properties

        #region Public Properties
        /// <summary>
        /// Gets or Sets if the activity preview should be shown in the designer
        /// </summary>
        public bool ShowPreview
        {
            get
            {
                return (this.previewWindow != null);
            }

            set
            {
                if (ShowPreview == value)
                    return;

                if (this.previewWindow != null)
                {
                    this.previewWindow.PreviewModeChanged -= new EventHandler(OnPreviewModeChanged);
                    this.previewWindow = null;
                }
                else
                {
                    this.previewWindow = new PreviewWindow(this);
                    this.previewWindow.PreviewModeChanged += new EventHandler(OnPreviewModeChanged);
                }

                this.designerVerbs = null;
                TypeDescriptor.Refresh(Activity);
                RefreshDesignerVerbs();
            }
        }

        /// <summary>
        /// Gets the designer being previewed
        /// </summary>
        public ActivityDesigner PreviewedDesigner
        {
            get
            {
                ItemInfo itemInfo = this.previewStrip.ActiveItem;
                if (itemInfo == null)
                    return null;

                return ActivityDesigner.GetDesigner(itemInfo.UserData[DesignerUserDataKeys.Activity] as Activity);
            }
        }

        public override ReadOnlyCollection<ActivityDesigner> ContainedDesigners
        {
            get
            {
                if (ActiveDesigner == this)
                {
                    List<ActivityDesigner> containedDesigners = new List<ActivityDesigner>();
                    if (PreviewedDesigner != null)
                    {
                        if (ShowPreview)
                        {
                            containedDesigners.AddRange(base.ContainedDesigners);
                            if (this.removePreviewedDesigner)
                                containedDesigners.Remove(PreviewedDesigner);
                        }
                        else
                        {
                            containedDesigners.Add(PreviewedDesigner);
                        }
                    }

                    return containedDesigners.AsReadOnly();
                }
                else
                {
                    return base.ContainedDesigners;
                }
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
                //If activityDesigner's location changes then we need to change location of children
                if (base.Location == value)
                    return;

                Size moveDelta = new Size(value.X - base.Location.X, value.Y - base.Location.Y);

                base.Location = value;

                this.previewStrip.Location = new Point(this.previewStrip.Location.X + moveDelta.Width, this.previewStrip.Location.Y + moveDelta.Height);

                if (ShowPreview)
                {
                    this.previewWindow.Location = new Point(this.previewWindow.Location.X + moveDelta.Width, this.previewWindow.Location.Y + moveDelta.Height);
                    this.separatorLine[0] = new Point(this.separatorLine[0].X + moveDelta.Width, this.separatorLine[0].Y + moveDelta.Height);
                    this.separatorLine[1] = new Point(this.separatorLine[1].X + moveDelta.Width, this.separatorLine[1].Y + moveDelta.Height);
                }
                else
                {
                    int headerHeight = this.previewStrip.Bounds.Bottom - Location.Y;
                    if (PreviewedDesigner != null)
                        PreviewedDesigner.Location = new Point(Location.X + (Size.Width - PreviewedDesigner.Size.Width) / 2, Location.Y + headerHeight + 2 * WorkflowTheme.CurrentTheme.AmbientTheme.Margin.Height);
                }
            }
        }

        public override object FirstSelectableObject
        {
            get
            {
                if (Expanded && IsVisible)
                {
                    if (PreviewedDesigner != null)
                        return PreviewedDesigner.Activity;
                    else if (ContainedDesigners.Count > 0)
                        return ContainedDesigners[0].Activity;
                }

                return null;
            }
        }

        public override object LastSelectableObject
        {
            get
            {
                if (Expanded && IsVisible)
                {
                    if (PreviewedDesigner != null)
                    {
                        CompositeActivityDesigner compositeDesigner = PreviewedDesigner as CompositeActivityDesigner;
                        if (this.previewWindow != null && !this.previewWindow.PreviewMode && compositeDesigner != null)
                            return compositeDesigner.LastSelectableObject;
                        else
                            return PreviewedDesigner.Activity;
                    }
                    else if (ContainedDesigners.Count > 0)
                    {
                        return ContainedDesigners[ContainedDesigners.Count - 1].Activity;
                    }
                }

                return null;
            }
        }

        public override AccessibleObject AccessibilityObject
        {
            get
            {
                if (this.accessibilityObject == null)
                    this.accessibilityObject = new ActivityCollectionAccessibleObject(this);
                return this.accessibilityObject;
            }
        }
        #endregion

        #region Protected Properties
        protected override string HelpText
        {
            get
            {
                return base.HelpText;
            }
            set
            {
                base.HelpText = value;
                this.previewStrip.HelpText = value;
            }
        }


        protected override int CurrentDropTarget
        {
            get
            {
                return base.CurrentDropTarget;
            }

            set
            {
                base.CurrentDropTarget = value;
                this.previewStrip.ActiveDropTarget = value;
            }
        }

        protected override ActivityDesignerVerbCollection Verbs
        {
            get
            {
                ActivityDesignerVerbCollection verbs = new ActivityDesignerVerbCollection();
                verbs.AddRange(base.Verbs);

                if (this.designerVerbs == null)
                {
                    this.designerVerbs = new ActivityDesignerVerbCollection();

                    if (ActiveDesigner == this)
                    {
                        DesignerVerb verb = new ActivityDesignerVerb(this, DesignerVerbGroup.View, DR.GetString(DR.ViewPreviousActivity), new EventHandler(OnViewActivity), new EventHandler(OnViewActivityStatusUpdate));
                        verb.Properties[DesignerUserDataKeys.ViewActivity] = false;
                        this.designerVerbs.Add(verb);
                        verb = new ActivityDesignerVerb(this, DesignerVerbGroup.View, DR.GetString(DR.ViewNextActivity), new EventHandler(OnViewActivity), new EventHandler(OnViewActivityStatusUpdate));
                        verb.Properties[DesignerUserDataKeys.ViewActivity] = true;
                        this.designerVerbs.Add(verb);

                        if (ShowPreview)
                        {
                            verb = new ActivityDesignerVerb(this, DesignerVerbGroup.Edit, DR.GetString(DR.PreviewActivity), new EventHandler(OnChangePreviewMode), new EventHandler(OnPreviewModeStatusUpdate));
                            verb.Properties[DesignerUserDataKeys.PreviewActivity] = true;
                            this.designerVerbs.Add(verb);
                            verb = new ActivityDesignerVerb(this, DesignerVerbGroup.Edit, DR.GetString(DR.EditActivity), new EventHandler(OnChangePreviewMode), new EventHandler(OnPreviewModeStatusUpdate));
                            verb.Properties[DesignerUserDataKeys.PreviewActivity] = false;
                            this.designerVerbs.Add(verb);
                        }
                    }
                }

                verbs.AddRange(this.designerVerbs);
                return verbs;
            }
        }

        protected internal override Rectangle[] GetConnectors()
        {
            if (!Expanded || ContainedDesigners.Count > 0 || ShowPreview || ActiveDesigner != this)
                return new Rectangle[] { };

            AmbientTheme ambientTheme = WorkflowTheme.CurrentTheme.AmbientTheme;

            int headerHeight = ambientTheme.Margin.Height;
            headerHeight += this.previewStrip.Size.Height;
            headerHeight += ambientTheme.Margin.Height;

            //We need to adjust the first and last connectors so as to adjust the 
            //start and end indicator heights
            Rectangle[] connectors = base.GetConnectors();
            if (connectors.Length > 0)
            {
                connectors[0].Y = connectors[0].Y + headerHeight;
                connectors[0].Height -= headerHeight;
            }

            return connectors;
        }

        protected internal override ActivityDesignerGlyphCollection Glyphs
        {
            get
            {
                ActivityDesignerGlyphCollection glyphs = new ActivityDesignerGlyphCollection();

                if (Expanded && ActiveDesigner == this)
                {
                    ISelectionService selectionService = GetService(typeof(ISelectionService)) as ISelectionService;

                    WorkflowDesignerLoader loader = GetService(typeof(WorkflowDesignerLoader)) as WorkflowDesignerLoader;
                    bool designMode = (loader != null && !loader.InDebugMode);

                    foreach (ItemInfo itemInfo in this.previewStrip.Items)
                    {
                        Rectangle itemBounds = this.previewStrip.GetItemBounds(itemInfo);

                        Activity activity = itemInfo.UserData[DesignerUserDataKeys.Activity] as Activity;
                        if (activity != null && !itemBounds.IsEmpty)
                        {
                            if (selectionService != null && selectionService.GetComponentSelected(activity))
                                glyphs.Add(new StripItemSelectionGlyph(this, itemInfo));

                            if (!activity.Enabled && !ActivityDesigner.IsCommentedActivity(activity))
                                glyphs.Add(new StripItemCommentGlyph(this, itemInfo));

                            if (ShowPreview && designMode && Expanded)
                            {
                                ActivityDesigner activityDesigner = ActivityDesigner.GetDesigner(activity);
                                if (activityDesigner != null && activityDesigner.DesignerActions.Count > 0 && designMode)
                                    glyphs.Add(new StripItemConfigErrorGlyph(this, itemInfo));
                            }
                        }
                    }
                }

                glyphs.AddRange(base.Glyphs);
                return glyphs;
            }
        }
        #endregion

        #region Private Properties
        #endregion

        #endregion

        #region Methods

        #region Public Methods
        public void RefreshPreview()
        {
            if (ShowPreview)
                this.previewWindow.Refresh();
        }

        public override HitTestInfo HitTest(Point point)
        {
            HitTestInfo hitInfo = HitTestInfo.Nowhere;

            if (Expanded && ActiveDesigner == this)
            {
                if (ContainedDesigners.Count == 0 && HelpTextRectangle.Contains(point))
                {
                    hitInfo = new ConnectorHitTestInfo(this, HitTestLocations.Designer, 0);
                }
                else if (this.previewStrip.Bounds.Contains(point))
                {
                    ItemInfo itemInfo = this.previewStrip.HitTest(point);
                    ActivityDesigner activityDesigner = (itemInfo != null) ? ActivityDesigner.GetDesigner(itemInfo.UserData[DesignerUserDataKeys.Activity] as Activity) : null;
                    if (activityDesigner != null)
                        hitInfo = new HitTestInfo(activityDesigner, HitTestLocations.Designer);
                    else
                        hitInfo = new HitTestInfo(this, HitTestLocations.Designer | HitTestLocations.ActionArea);
                }
                else if (ShowPreview && this.previewWindow.Bounds.Contains(point) &&
                        (this.previewWindow.PreviewMode || PreviewedDesigner == null || !PreviewedDesigner.Bounds.Contains(point)))
                {
                    hitInfo = new HitTestInfo(this, HitTestLocations.Designer | HitTestLocations.ActionArea);
                }
                else
                {
                    hitInfo = base.HitTest(point);

                    if (ShowPreview && this.previewWindow.PreviewMode && hitInfo.AssociatedDesigner != this)
                        hitInfo = HitTestInfo.Nowhere;
                }
            }
            else
            {
                hitInfo = base.HitTest(point);
            }

            return hitInfo;
        }

        public override bool IsContainedDesignerVisible(ActivityDesigner containedDesigner)
        {
            if (containedDesigner == null)
                throw new ArgumentNullException("containedDesigner");

            if (ActiveDesigner == this)
            {
                if (ShowPreview && this.previewWindow.PreviewMode)
                    return false;

                //We do not draw the child activities if in previewed mode or not showing child activity
                return (this.previewStrip.ActiveItem != null && this.previewStrip.ActiveItem.UserData[DesignerUserDataKeys.Activity] == containedDesigner.Activity);
            }
            else
            {
                return base.IsContainedDesignerVisible(containedDesigner);
            }
        }

        public override void EnsureVisibleContainedDesigner(ActivityDesigner containedDesigner)
        {
            if (containedDesigner == null)
                throw new ArgumentNullException("containedDesigner");

            // call base
            base.EnsureVisibleContainedDesigner(containedDesigner);

            // make sure that we select the children
            if (ActiveDesigner == this)
            {
                foreach (ItemInfo item in this.previewStrip.Items)
                {
                    if (item.UserData[DesignerUserDataKeys.Activity] == containedDesigner.Activity)
                    {
                        this.previewStrip.ActiveItem = item;
                        break;
                    }
                }
            }
        }

        public override object GetNextSelectableObject(object obj, DesignerNavigationDirection direction)
        {
            if (ActiveDesigner != this)
                return base.GetNextSelectableObject(obj, direction);

            if (direction != DesignerNavigationDirection.Left && direction != DesignerNavigationDirection.Right)
                return null;

            object nextObject = null;

            int index = StripItemIndexFromActivity(obj as Activity);
            if (direction == DesignerNavigationDirection.Left && index >= 0)
                nextObject = this.previewStrip.Items[(index > 0) ? index - 1 : this.previewStrip.Items.Count - 1].UserData[DesignerUserDataKeys.Activity];
            else if (direction == DesignerNavigationDirection.Right && index <= this.previewStrip.Items.Count - 1)
                nextObject = this.previewStrip.Items[(index < this.previewStrip.Items.Count - 1) ? index + 1 : 0].UserData[DesignerUserDataKeys.Activity];

            return nextObject;
        }
        #endregion

        #region Protected Methods
        protected override void Initialize(Activity activity)
        {
            base.Initialize(activity);

            ShowPreview = true;

            CompositeActivity compositeActivity = Activity as CompositeActivity;
            if (compositeActivity != null)
            {
                foreach (Activity containedActivity in compositeActivity.Activities)
                {
                    if (!Helpers.IsAlternateFlowActivity(containedActivity))
                    {
                        ItemInfo stripItem = new ItemInfo(containedActivity.GetHashCode());
                        stripItem.UserData[DesignerUserDataKeys.Activity] = containedActivity;
                        this.previewStrip.Items.Add(stripItem);
                    }
                }
            }

            //Start listening to selection change event
            ISelectionService selectionService = GetService(typeof(ISelectionService)) as ISelectionService;
            if (selectionService != null)
                selectionService.SelectionChanged += new EventHandler(OnSelectionChanged);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ISelectionService selectionService = GetService(typeof(ISelectionService)) as ISelectionService;
                if (selectionService != null)
                    selectionService.SelectionChanged -= new EventHandler(OnSelectionChanged);
            }

            base.Dispose(disposing);
        }

        protected override Rectangle[] GetDropTargets(Point dropPoint)
        {
            if (ActiveDesigner == this && this.previewStrip.Bounds.Contains(dropPoint))
                return this.previewStrip.DropTargets;
            else
                return base.GetDropTargets(dropPoint);
        }

        protected override void OnPaint(ActivityDesignerPaintEventArgs e)
        {
            //This is a problem, we need to improve the protocol between the preview window and the designer
            //The reason for this to be done is that the base's draw draws the preview designer and preview window
            //also draws the preview designer. The result is draw is called multiple times which slows down the designer.
            //We have to either bring the functionality of base class's draw here and not call base's draw or change the 
            //designer inheritance hierarchy
            if (ShowPreview && !this.previewWindow.PreviewMode && PreviewedDesigner != null)
                this.removePreviewedDesigner = true;

            base.OnPaint(e);

            if (ShowPreview && !this.previewWindow.PreviewMode && PreviewedDesigner != null)
                this.removePreviewedDesigner = false;

            if (!Expanded || ActiveDesigner != this)
                return;

            this.previewStrip.Draw(e.Graphics);

            //Draw the separator between the strip and canvas
            //Draw the canvas and the strip
            if (ShowPreview)
            {
                e.Graphics.DrawLine(e.DesignerTheme.ForegroundPen, this.separatorLine[0], this.separatorLine[1]);
                this.previewWindow.Draw(e.Graphics, e.ViewPort);
            }
        }

        protected override void OnDragEnter(ActivityDragEventArgs e)
        {
            base.OnDragEnter(e);

            e.DragImageSnapPoint = SnapInToPreviewStripDropTarget(e);
        }

        protected override void OnDragOver(ActivityDragEventArgs e)
        {
            base.OnDragOver(e);

            e.DragImageSnapPoint = SnapInToPreviewStripDropTarget(e);
        }

        protected override void OnLayoutPosition(ActivityDesignerLayoutEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException("e");

            if (ActiveDesigner == this && Expanded)
            {
                Rectangle bounds = Bounds;
                this.previewStrip.Location = new Point(bounds.Left + bounds.Width / 2 - this.previewStrip.Size.Width / 2, Location.Y + TitleHeight + e.AmbientTheme.Margin.Height);

                //Make sure that we dont call this after positioning the preview window
                base.OnLayoutPosition(e);

                //Do not position the preview window before positioning the base designers
                if (ShowPreview)
                {
                    Rectangle previewStripRectangle = this.previewStrip.Bounds;
                    this.previewWindow.Location = new Point(bounds.Left + bounds.Width / 2 - this.previewWindow.Size.Width / 2, previewStripRectangle.Bottom + 3 * e.AmbientTheme.Margin.Height);

                    this.separatorLine[0].X = bounds.Left + e.AmbientTheme.Margin.Width;
                    this.separatorLine[0].Y = previewStripRectangle.Bottom;
                    this.separatorLine[0].Y += e.AmbientTheme.Margin.Height + e.AmbientTheme.Margin.Height / 2;

                    this.separatorLine[1].X = bounds.Right - e.AmbientTheme.Margin.Width;
                    this.separatorLine[1].Y = previewStripRectangle.Bottom;
                    this.separatorLine[1].Y += e.AmbientTheme.Margin.Height + e.AmbientTheme.Margin.Height / 2;
                }
                else
                {
                    int headerHeight = this.previewStrip.Bounds.Bottom - Location.Y;
                    if (PreviewedDesigner != null)
                        PreviewedDesigner.Location = new Point(Location.X + (Size.Width - PreviewedDesigner.Size.Width) / 2, Location.Y + headerHeight + 2 * e.AmbientTheme.Margin.Height);
                }
            }
            else
            {
                base.OnLayoutPosition(e);
            }
        }

        protected override Size OnLayoutSize(ActivityDesignerLayoutEventArgs e)
        {
            Size size = base.OnLayoutSize(e);

            if (Expanded && ActiveDesigner == this)
            {
                this.previewStrip.OnLayoutSize(e.Graphics);

                Size containerSize = Size.Empty;
                containerSize.Width = Math.Max(containerSize.Width, this.previewStrip.Size.Width);
                containerSize.Height += this.previewStrip.Size.Height;
                containerSize.Height += e.AmbientTheme.Margin.Height;

                if (this.previewWindow != null)
                {
                    this.previewWindow.Refresh();
                    this.previewWindow.OnLayoutSize(e.Graphics, containerSize.Width);

                    containerSize.Width = Math.Max(containerSize.Width, this.previewWindow.Size.Width);
                    containerSize.Width += 2 * e.AmbientTheme.Margin.Width;
                    containerSize.Height += TitleHeight;
                    containerSize.Height += 4 * e.AmbientTheme.Margin.Height;
                    containerSize.Height += this.previewWindow.Size.Height;
                    containerSize.Height += e.AmbientTheme.Margin.Height;
                }
                else
                {
                    containerSize.Width = Math.Max(containerSize.Width, size.Width);
                    containerSize.Width += 3 * e.AmbientTheme.Margin.Width;
                    containerSize.Width += 2 * e.AmbientTheme.SelectionSize.Width;
                    containerSize.Height += size.Height;
                }

                containerSize.Width = Math.Max(containerSize.Width, MinimumSize.Width);
                containerSize.Height = Math.Max(containerSize.Height, MinimumSize.Height);

                if (!ShowPreview && PreviewedDesigner != null)
                {
                    ActivityPreviewDesignerTheme previewDesignerTheme = e.DesignerTheme as ActivityPreviewDesignerTheme;
                    if (previewDesignerTheme != null)
                    {
                        containerSize.Height -= previewDesignerTheme.ConnectorSize.Height;
                        containerSize.Height -= 2 * e.AmbientTheme.Margin.Height;
                        containerSize.Height -= 2 * e.AmbientTheme.SelectionSize.Height;
                    }

                    Size margins = new Size(2 * e.AmbientTheme.Margin.Width + 2 * e.AmbientTheme.SelectionSize.Width, 2 * e.AmbientTheme.Margin.Height + 2 * e.AmbientTheme.SelectionSize.Height);
                    PreviewedDesigner.Size = new Size(containerSize.Width - margins.Width, containerSize.Height - (TitleHeight + this.previewStrip.Size.Height + margins.Height));
                }

                size = containerSize;
            }

            return size;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (ActiveDesigner == this)
            {
                Point point = new Point(e.X, e.Y);
                if (this.previewStrip.Bounds.Contains(point))
                    this.previewStrip.OnMouseDown(e);
                else if (ShowPreview && this.previewWindow.Bounds.Contains(point))
                    this.previewWindow.OnMouseDown(e);
            }
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);

            if (ActiveDesigner == this)
            {
                Point point = new Point(e.X, e.Y);
                if (PreviewedDesigner != null && ShowPreview && this.previewWindow.PreviewMode && this.previewWindow.Bounds.Contains(point))
                    this.previewWindow.PreviewMode = false;
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (ActiveDesigner == this)
                this.previewStrip.OnMouseUp(e);
        }

        protected override void OnMouseLeave()
        {
            base.OnMouseLeave();

            if (ActiveDesigner == this)
                this.previewStrip.OnMouseLeave();
        }

        protected override void SaveViewState(BinaryWriter writer)
        {
            if (writer == null)
                throw new ArgumentNullException("writer");

            int selectedItem = -1;
            bool preview = false;

            // save selected item in strip
            if (this.previewStrip.ActiveItem != null)
                selectedItem = this.previewStrip.Items.IndexOf(previewStrip.ActiveItem);

            writer.Write(selectedItem);

            // save preview state (currently used in CAG)
            if (ShowPreview)
                preview = this.previewWindow.PreviewMode;

            writer.Write(preview);

            base.SaveViewState(writer);
        }

        protected override void LoadViewState(BinaryReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");

            // read selected item in strip
            int selectedItem = reader.ReadInt32();

            if (selectedItem != -1 && selectedItem < this.previewStrip.Items.Count)
            {
                ItemInfo itemInfo = this.previewStrip.Items[selectedItem];

                IDesignerHost designerHost = GetService(typeof(IDesignerHost)) as IDesignerHost;
                if (designerHost == null)
                    throw new Exception(SR.GetString(SR.General_MissingService, typeof(IDesignerHost).FullName));
                EnsureVisibleContainedDesigner(designerHost.GetDesigner(((Activity)itemInfo.UserData[DesignerUserDataKeys.Activity])) as ActivityDesigner);
            }

            // read preview mode
            bool preview = reader.ReadBoolean();
            if (ShowPreview)
                this.previewWindow.PreviewMode = preview;

            base.LoadViewState(reader);
        }

        protected override void OnThemeChange(ActivityDesignerTheme newTheme)
        {
            base.OnThemeChange(newTheme);
            RefreshPreview();
        }

        protected internal override void RefreshDesignerActions()
        {
            base.RefreshDesignerActions();
            RefreshPreview();
        }

        protected override void OnContainedActivitiesChanged(ActivityCollectionChangeEventArgs listChangeArgs)
        {
            if (ActiveDesigner == this)
            {
                if (listChangeArgs.Action == ActivityCollectionChangeAction.Add && listChangeArgs.AddedItems.Count > 0 && !Helpers.IsAlternateFlowActivity(listChangeArgs.AddedItems[0]))
                {
                    ItemInfo itemToAdd = new ItemInfo(listChangeArgs.AddedItems[0].GetHashCode());
                    itemToAdd.UserData[DesignerUserDataKeys.Activity] = listChangeArgs.AddedItems[0];
                    if (listChangeArgs.Index < this.previewStrip.Items.Count)
                        this.previewStrip.Items.Insert(listChangeArgs.Index, itemToAdd);
                    else
                        this.previewStrip.Items.Add(itemToAdd);
                }
                else if (listChangeArgs.Action == ActivityCollectionChangeAction.Remove && listChangeArgs.RemovedItems.Count > 0)
                {
                    int index = this.previewStrip.Items.IndexOf(new ItemInfo(listChangeArgs.RemovedItems[0].GetHashCode()));
                    if (index >= 0)
                        this.previewStrip.Items.RemoveAt(index);
                }
            }

            base.OnContainedActivitiesChanged(listChangeArgs);
        }
        #endregion

        #region Private Methods
        private Point SnapInToPreviewStripDropTarget(ActivityDragEventArgs e)
        {
            int activeDropTarget = this.previewStrip.ActiveDropTarget;
            Rectangle[] dropTargets = this.previewStrip.DropTargets;

            if (activeDropTarget < 0 || activeDropTarget >= dropTargets.Length)
                return Point.Empty;

            Rectangle dropConnector = dropTargets[activeDropTarget];
            ActivityPreviewDesignerTheme designerTheme = DesignerTheme as ActivityPreviewDesignerTheme;
            dropConnector.Width = (dropConnector.Width > ((designerTheme != null) ? designerTheme.ConnectorSize.Width : 0)) ? ((designerTheme != null) ? designerTheme.ConnectorSize.Width : 0) : dropConnector.Width;
            return new Point(dropConnector.Left + dropConnector.Width / 2, dropConnector.Top + dropConnector.Height / 2);
        }

        private void OnSelectionChanged(object sender, EventArgs e)
        {
            ISelectionService selectionService = GetService(typeof(ISelectionService)) as ISelectionService;
            if (selectionService == null || ActiveDesigner != this)
                return;

            foreach (ItemInfo itemInfo in this.previewStrip.Items)
            {
                if (itemInfo.UserData[DesignerUserDataKeys.Activity] == selectionService.PrimarySelection)
                {
                    this.previewStrip.ActiveItem = itemInfo;
                    break;
                }
            }

            //We need to make sure that for preview designer; if we select the designer using property grid or
            //any other means then selection is visible
            if (selectionService.SelectionCount == 1)
            {
                ActivityDesigner selectedDesigner = ActivityDesigner.GetDesigner(selectionService.PrimarySelection as Activity);
                if (selectedDesigner != null && !selectedDesigner.IsVisible && this != selectedDesigner.ParentDesigner)
                {
                    //PLEASE NOTE:
                    //We want to search if the current designer is ancestor of selected designer
                    //We do the search till we hit the immediate child of the "this" designer in ancestor chain
                    //so that if we are in preview mode then we make sure that we make the imamediate child of "this" visible
                    ActivityDesigner designer = selectedDesigner;
                    while (designer != null)
                    {
                        CompositeActivityDesigner parentDesigner = designer.ParentDesigner;
                        if (this == parentDesigner)
                            break;
                        designer = parentDesigner;
                    }

                    if (designer != null)
                    {
                        if (this.previewWindow != null && this.previewWindow.PreviewMode)
                            designer.EnsureVisible();
                        else
                            selectedDesigner.EnsureVisible();
                    }
                }
            }
        }

        private void OnPreviewChanged(object sender, SelectionChangeEventArgs e)
        {
            bool redoLayout = (!ShowPreview);
            if (ShowPreview)
            {
                this.previewWindow.PreviewedActivity = (e.CurrentItem != null) ? e.CurrentItem.UserData[DesignerUserDataKeys.Activity] as Activity : null;
                redoLayout = (this.previewWindow.PreviewMode == false);
            }

            if (redoLayout)
                PerformLayout();
        }

        private void OnViewActivity(object sender, EventArgs args)
        {
            DesignerVerb verb = sender as DesignerVerb;
            if (verb != null && verb.Properties.Contains(DesignerUserDataKeys.ViewActivity))
            {
                ItemInfo activeItem = this.previewStrip.ActiveItem;
                if (activeItem != null)
                {
                    bool viewNext = (bool)verb.Properties[DesignerUserDataKeys.ViewActivity];
                    int index = this.previewStrip.Items.IndexOf(activeItem) + ((viewNext) ? 1 : -1);
                    index = (index >= this.previewStrip.Items.Count) ? 0 : (index < 0) ? this.previewStrip.Items.Count - 1 : index;
                    this.previewStrip.ActiveItem = this.previewStrip.Items[index];
                }
            }
        }

        private void OnViewActivityStatusUpdate(object sender, EventArgs args)
        {
            DesignerVerb designerVerb = sender as DesignerVerb;
            if (designerVerb != null)
                designerVerb.Enabled = (this.previewStrip.ActiveItem != null && this.previewStrip.Items.Count > 1 && this.previewStrip.Items.IndexOf(this.previewStrip.ActiveItem) >= 0);
        }

        private void OnChangePreviewMode(object sender, EventArgs args)
        {
            DesignerVerb designerVerb = sender as DesignerVerb;
            if (ShowPreview && designerVerb != null && designerVerb.Properties.Contains(DesignerUserDataKeys.PreviewActivity))
                this.previewWindow.PreviewMode = (bool)designerVerb.Properties[DesignerUserDataKeys.PreviewActivity];
        }

        private void OnPreviewModeStatusUpdate(object sender, EventArgs args)
        {
            DesignerVerb designerVerb = sender as DesignerVerb;
            if (ShowPreview && designerVerb != null && designerVerb.Properties.Contains(DesignerUserDataKeys.PreviewActivity))
                designerVerb.Enabled = (this.previewWindow.PreviewMode != (bool)designerVerb.Properties[DesignerUserDataKeys.PreviewActivity]);
        }

        private void OnPreviewModeChanged(object sender, EventArgs e)
        {
            RefreshDesignerVerbs();
        }

        private int StripItemIndexFromActivity(Activity activity)
        {
            int i = 0;
            foreach (ItemInfo item in this.previewStrip.Items)
            {
                if (item.UserData[DesignerUserDataKeys.Activity] == activity)
                    return i;
                i = i + 1;
            }
            return -1;
        }

        #endregion

        #endregion

        #region StripItemSelectionGlyph Class
        private sealed class StripItemSelectionGlyph : SelectionGlyph
        {
            private ActivityPreviewDesigner parentDesigner;
            private ItemInfo item;

            internal StripItemSelectionGlyph(ActivityPreviewDesigner parentDesigner, ItemInfo item)
            {
                this.parentDesigner = parentDesigner;
                this.item = item;
            }

            public override Rectangle GetBounds(ActivityDesigner designer, bool activated)
            {
                Rectangle itemBounds = this.parentDesigner.previewStrip.GetItemBounds(this.item);
                Size selectionSize = new Size(Math.Max(itemBounds.Width / 6, 1), Math.Max(itemBounds.Height / 6, 1));
                itemBounds.Inflate(selectionSize);
                return itemBounds;
            }

            public override bool IsPrimarySelection
            {
                get
                {
                    ISelectionService selectionService = this.parentDesigner.GetService(typeof(ISelectionService)) as ISelectionService;
                    return (selectionService != null && selectionService.PrimarySelection == this.item.UserData[DesignerUserDataKeys.Activity]);
                }
            }
        }
        #endregion

        #region StripItemCommentGlyph Class
        private sealed class StripItemCommentGlyph : CommentGlyph
        {
            private ActivityPreviewDesigner parentDesigner;
            private ItemInfo item;

            internal StripItemCommentGlyph(ActivityPreviewDesigner parentDesigner, ItemInfo item)
            {
                this.parentDesigner = parentDesigner;
                this.item = item;
            }

            public override Rectangle GetBounds(ActivityDesigner designer, bool activated)
            {
                return this.parentDesigner.previewStrip.GetItemBounds(this.item);
            }
        }
        #endregion

        #region Class StripItemConfigErrorGlyph
        private sealed class StripItemConfigErrorGlyph : ConfigErrorGlyph
        {
            private ActivityPreviewDesigner parentDesigner;
            private ItemInfo item;

            internal StripItemConfigErrorGlyph(ActivityPreviewDesigner parentDesigner, ItemInfo item)
            {
                this.parentDesigner = parentDesigner;
                this.item = item;
                Debug.Assert(item.UserData[DesignerUserDataKeys.Activity] is Activity);
            }

            public override Rectangle GetBounds(ActivityDesigner designer, bool activated)
            {
                Rectangle rectangle = this.parentDesigner.previewStrip.GetItemBounds(this.item);

                Size configErrorSize = WorkflowTheme.CurrentTheme.AmbientTheme.GlyphSize;
                configErrorSize.Width = configErrorSize.Width * 3 / 4;
                configErrorSize.Height = configErrorSize.Height * 3 / 4;

                Point configErrorLocation = new Point(rectangle.Right - configErrorSize.Width / 2, rectangle.Top - configErrorSize.Height / 2);
                rectangle = new Rectangle(configErrorLocation, configErrorSize);

                if (activated)
                {
                    rectangle.Width *= 2;
                    AmbientTheme ambientTheme = WorkflowTheme.CurrentTheme.AmbientTheme;
                    rectangle.Inflate(ambientTheme.Margin.Width / 2, ambientTheme.Margin.Height / 2);
                }

                return rectangle;
            }

            protected override void OnActivate(ActivityDesigner designer)
            {
                ActivityDesigner activityDesigner = ActivityDesigner.GetDesigner(this.item.UserData[DesignerUserDataKeys.Activity] as Activity);
                if (activityDesigner != null)
                    base.OnActivate(activityDesigner);
            }
        }
        #endregion

        #region Class ActivityPreviewAccessibleObject
        private sealed class ActivityCollectionAccessibleObject : SequenceDesignerAccessibleObject
        {
            public ActivityCollectionAccessibleObject(ActivityPreviewDesigner activityDesigner)
                : base(activityDesigner)
            {
            }

            public override int GetChildCount()
            {
                int childCount = 0;

                ActivityPreviewDesigner activityCollectionDesigner = base.ActivityDesigner as ActivityPreviewDesigner;
                if (activityCollectionDesigner != null && activityCollectionDesigner.ActiveDesigner == activityCollectionDesigner)
                {
                    childCount += activityCollectionDesigner.previewStrip.AccessibilityObjects.Length;

                    if (activityCollectionDesigner.ShowPreview)
                        childCount += 1;

                    if ((activityCollectionDesigner.ShowPreview && !activityCollectionDesigner.previewWindow.PreviewMode) ||
                        (!activityCollectionDesigner.ShowPreview))
                        childCount += 1;
                }

                return childCount;
            }

            public override AccessibleObject GetChild(int index)
            {
                ActivityPreviewDesigner activityCollectionDesigner = base.ActivityDesigner as ActivityPreviewDesigner;
                if (activityCollectionDesigner != null && activityCollectionDesigner.ActiveDesigner == activityCollectionDesigner)
                {
                    if (index < activityCollectionDesigner.previewStrip.AccessibilityObjects.Length)
                        return activityCollectionDesigner.previewStrip.AccessibilityObjects[index];
                    index -= activityCollectionDesigner.previewStrip.AccessibilityObjects.Length;

                    if (activityCollectionDesigner.ShowPreview && index == 0)
                        return activityCollectionDesigner.previewWindow.AccessibilityObject;

                    AccessibleObject accessibilityObject = activityCollectionDesigner.PreviewedDesigner.AccessibilityObject;
                    while (accessibilityObject.Bounds.Size.IsEmpty && accessibilityObject.GetChildCount() > 0)
                        accessibilityObject = accessibilityObject.GetChild(0);

                    return accessibilityObject;
                }

                return base.GetChild(index);
            }
        }
        #endregion
    }
    #endregion

}
