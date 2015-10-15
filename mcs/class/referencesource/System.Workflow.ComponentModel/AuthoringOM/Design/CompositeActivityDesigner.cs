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

    #region CompositeActivityDesigner Class
    /// <summary>
    /// CompositeActivityDesigner provides a designer which allows user to visually design composite activities in the design mode. 
    /// CompositeActivityDesigner enables the user to customize layouting, drawing associated with the CompositeActivity, it also allows
    /// managing the layouting, drawing and eventing for the contained activity designers.
    /// </summary>
    [ActivityDesignerTheme(typeof(CompositeDesignerTheme))]
    [SRCategory("CompositeActivityDesigners", "System.Workflow.ComponentModel.Design.DesignerResources")]
    [DesignerSerializer(typeof(CompositeActivityDesignerLayoutSerializer), typeof(WorkflowMarkupSerializer))]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public abstract class CompositeActivityDesigner : ActivityDesigner
    {
        #region Fields
        private const string CF_DESIGNER = "CF_WINOEDESIGNERCOMPONENTS";
        private const string CF_DESIGNERSTATE = "CF_WINOEDESIGNERCOMPONENTSSTATE";
        private const int MaximumCharsPerLine = 8;
        private const int MaximumTextLines = 1;

        private Size actualTextSize = Size.Empty;
        private CompositeDesignerAccessibleObject accessibilityObject;
        private List<ActivityDesigner> containedActivityDesigners;
        private bool expanded = true;
        #endregion

        #region Construction / Destruction
        /// <summary>
        /// Default constructor for CompositeActivityDesigner
        /// </summary>
        protected CompositeActivityDesigner()
        {
        }
        #endregion

        #region Properties

        #region Public Properties
        /// <summary>
        /// Gets if the activity associated with designer is structurally locked that is no activities can be added to it.
        /// </summary>
        public bool IsEditable
        {
            get
            {
                if (!(Activity is CompositeActivity))
                    return false;

                if (IsLocked)
                    return false;

                if (Helpers.IsCustomActivity(Activity as CompositeActivity))
                    return false;

                return true;
            }
        }

        /// <summary>
        /// Gets if the designer can be collapsed, collapsed designer has expand/collapse button added to it
        /// </summary>
        public virtual bool CanExpandCollapse
        {
            get
            {
                return !IsRootDesigner;
            }
        }

        /// <summary>
        /// Gets or Sets if the designer currently in expanded state
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual bool Expanded
        {
            get
            {
                if (!CanExpandCollapse && !this.expanded)
                    Expanded = true;
                return this.expanded;
            }

            set
            {
                if (this.expanded == value)
                    return;

                //If the designer can not expand or collapse then we need to make sure that
                //user does not collapse it
                if (!CanExpandCollapse && !value)
                    return;

                this.expanded = value;

                PerformLayout();
            }
        }

        /// <summary>
        /// Gets the array of activity designer contained within.
        /// </summary>
        public virtual ReadOnlyCollection<ActivityDesigner> ContainedDesigners
        {
            get
            {
                List<ActivityDesigner> designers = new List<ActivityDesigner>();

                CompositeActivity compositeActivity = Activity as CompositeActivity;
                if (compositeActivity != null)
                {
                    if (this.containedActivityDesigners == null)
                    {
                        bool foundAllDesigners = true;

                        //In certain cases users might try to access the activity designers
                        //in Initialize method of composite activity. In that case the child activities
                        //might not be inserted in the container. If this happens then we might not get the 
                        //designer of the contained activity. When such a case happens we should not buffer the
                        //designers as it might lead to erroneous results
                        foreach (Activity activity in compositeActivity.Activities)
                        {
                            ActivityDesigner activityDesigner = ActivityDesigner.GetDesigner(activity);
                            if (activityDesigner != null)
                                designers.Add(activityDesigner);
                            else
                                foundAllDesigners = false;
                        }

                        if (foundAllDesigners)
                            this.containedActivityDesigners = designers;
                    }
                    else
                    {
                        designers = this.containedActivityDesigners;
                    }
                }

                return designers.AsReadOnly();
            }
        }

        /// <summary>
        /// Gets the first selectable object in the navigation order.
        /// </summary>
        public virtual object FirstSelectableObject
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the last selected object in the navigation order
        /// </summary>
        public virtual object LastSelectableObject
        {
            get
            {
                return null;
            }
        }

        public override AccessibleObject AccessibilityObject
        {
            get
            {
                if (this.accessibilityObject == null)
                    this.accessibilityObject = new CompositeDesignerAccessibleObject(this);
                return this.accessibilityObject;
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
                ///If designers's location changes then we need to change location of children
                if (base.Location == value)
                    return;

                Size moveDelta = new Size(value.X - base.Location.X, value.Y - base.Location.Y);
                foreach (ActivityDesigner activityDesigner in ContainedDesigners)
                    activityDesigner.Location = new Point(activityDesigner.Location.X + moveDelta.Width, activityDesigner.Location.Y + moveDelta.Height);

                base.Location = value;
            }
        }
        #endregion

        #region Protected Properties
        /// <summary>
        /// Gets the rectangle associated with expand/collapse button
        /// </summary>
        protected virtual Rectangle ExpandButtonRectangle
        {
            get
            {
                if (!CanExpandCollapse)
                    return Rectangle.Empty;

                CompositeDesignerTheme designerTheme = DesignerTheme as CompositeDesignerTheme;
                if (designerTheme == null)
                    return Rectangle.Empty;

                Size textSize = TextRectangle.Size;
                Size imageSize = (Image != null) ? designerTheme.ImageSize : Size.Empty;
                Rectangle bounds = Bounds;

                Size anchorSize = (!textSize.IsEmpty) ? textSize : imageSize;
                Rectangle expandButtonRectangle = new Rectangle(bounds.Location, designerTheme.ExpandButtonSize);
                expandButtonRectangle.X += (bounds.Width - ((3 * designerTheme.ExpandButtonSize.Width / 2) + anchorSize.Width)) / 2;
                expandButtonRectangle.Y += 2 * WorkflowTheme.CurrentTheme.AmbientTheme.Margin.Height;
                if (anchorSize.Height > expandButtonRectangle.Height)
                    expandButtonRectangle.Y += (anchorSize.Height - expandButtonRectangle.Height) / 2;
                return expandButtonRectangle;
            }
        }

        /// <summary>
        /// Gets the height for the title area of the designer, typically this can contain the heading, icon and expand/collapse button
        /// </summary>
        protected virtual int TitleHeight
        {
            get
            {
                Size margin = WorkflowTheme.CurrentTheme.AmbientTheme.Margin;
                Rectangle expandButtonRectangle = ExpandButtonRectangle;
                Rectangle textRectangle = TextRectangle;
                Rectangle imageRectangle = ImageRectangle;

                int titleHeight = 0;
                if (!textRectangle.Size.IsEmpty)
                {
                    titleHeight = Math.Max(expandButtonRectangle.Height, textRectangle.Height);
                    titleHeight += imageRectangle.Height;
                }
                else
                {
                    titleHeight = Math.Max(expandButtonRectangle.Height, imageRectangle.Height);
                }

                if (!expandButtonRectangle.Size.IsEmpty || !textRectangle.Size.IsEmpty || !imageRectangle.Size.IsEmpty)
                    titleHeight += (Expanded ? 2 : 3) * margin.Height;

                if (!imageRectangle.Size.IsEmpty && !textRectangle.Size.IsEmpty)
                    titleHeight += margin.Height;

                return titleHeight;
            }
        }

        protected override Rectangle ImageRectangle
        {
            get
            {
                if (Image == null)
                    return Rectangle.Empty;

                CompositeDesignerTheme designerTheme = DesignerTheme as CompositeDesignerTheme;
                if (designerTheme == null)
                    return Rectangle.Empty;

                Rectangle bounds = Bounds;
                Size expandButtonSize = ExpandButtonRectangle.Size;
                Size imageSize = designerTheme.ImageSize;
                Size textSize = TextRectangle.Size;
                Size margin = WorkflowTheme.CurrentTheme.AmbientTheme.Margin;

                Rectangle imageRectangle = new Rectangle(bounds.Location, imageSize);
                if (textSize.Width > 0)
                {
                    imageRectangle.X += (bounds.Width - imageSize.Width) / 2;
                }
                else
                {
                    imageRectangle.X += (bounds.Width - (imageSize.Width + 3 * expandButtonSize.Width / 2)) / 2;
                    imageRectangle.X += 3 * expandButtonSize.Width / 2;
                }

                imageRectangle.Y += 2 * margin.Height;
                if (textSize.Height > 0)
                    imageRectangle.Y += textSize.Height + margin.Height;
                return imageRectangle;
            }
        }

        protected override Rectangle TextRectangle
        {
            get
            {
                if (String.IsNullOrEmpty(Text))
                    return Rectangle.Empty;

                CompositeDesignerTheme designerTheme = DesignerTheme as CompositeDesignerTheme;
                if (designerTheme == null)
                    return Rectangle.Empty;

                Rectangle bounds = Bounds;
                Size margin = WorkflowTheme.CurrentTheme.AmbientTheme.Margin;
                Size expandButtonSize = (CanExpandCollapse) ? designerTheme.ExpandButtonSize : Size.Empty;

                //Calculate the text size
                int maxAvailableWidth = bounds.Width - (2 * margin.Width + 3 * expandButtonSize.Width / 2);

                Size requestedLineSize = this.actualTextSize;
                requestedLineSize.Width /= Text.Length;
                requestedLineSize.Width += ((requestedLineSize.Width % Text.Length) > 0) ? 1 : 0;
                requestedLineSize.Width *= Math.Min(Text.Length, CompositeActivityDesigner.MaximumCharsPerLine - 1);

                Size textSize = Size.Empty;
                textSize.Width = Math.Min(maxAvailableWidth, this.actualTextSize.Width);
                textSize.Width = Math.Max(1, Math.Max(textSize.Width, requestedLineSize.Width));

                textSize.Height = requestedLineSize.Height;
                int textLines = this.actualTextSize.Width / textSize.Width;
                textLines += ((this.actualTextSize.Width % textSize.Width) > 0) ? 1 : 0;
                textLines = Math.Min(textLines, CompositeActivityDesigner.MaximumTextLines);
                textSize.Height *= textLines;

                //Calculate the text rectangle
                Rectangle textRectangle = new Rectangle(bounds.Location, textSize);
                textRectangle.X += (bounds.Width - (3 * expandButtonSize.Width / 2 + textSize.Width)) / 2;
                textRectangle.X += 3 * expandButtonSize.Width / 2;
                textRectangle.Y += 2 * margin.Height;
                if (expandButtonSize.Height > textSize.Height)
                    textRectangle.Y += (expandButtonSize.Height - textSize.Height) / 2;
                textRectangle.Size = textSize;
                return textRectangle;
            }
        }

        protected internal override ActivityDesignerGlyphCollection Glyphs
        {
            get
            {
                ActivityDesignerGlyphCollection glyphs = new ActivityDesignerGlyphCollection();
                glyphs.AddRange(base.Glyphs);

                CompositeDesignerTheme compositeTheme = DesignerTheme as CompositeDesignerTheme;
                if (compositeTheme != null && compositeTheme.ShowDropShadow)
                    glyphs.Add(ShadowGlyph.Default);

                return glyphs;
            }
        }
        #endregion

        #region Private Properties

        #region Properties used during serialization only
        //NOTE THAT THIS WILL ONLY BE USED FOR SERIALIZATION PURPOSES
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        internal List<ActivityDesigner> Designers
        {
            get
            {
                List<ActivityDesigner> childDesigners = new List<ActivityDesigner>();
                CompositeActivity compositeActivity = Activity as CompositeActivity;
                IDesignerHost host = GetService(typeof(IDesignerHost)) as IDesignerHost;
                if (host != null && compositeActivity != null)
                {
                    foreach (Activity childActivity in compositeActivity.Activities)
                    {
                        ActivityDesigner designer = host.GetDesigner(childActivity) as ActivityDesigner;
                        if (designer != null)
                            childDesigners.Add(designer);
                    }
                }

                return childDesigners;
            }
        }
        #endregion

        #endregion

        #endregion

        #region Methods

        #region Public Static Methods
        /// <summary>
        /// Inserts activities in specified composite activity designer by creating transaction
        /// </summary>
        /// <param name="compositeActivityDesigner">Designer in which to insert the activities</param>
        /// <param name="insertLocation">Insertion location</param>
        /// <param name="activitiesToInsert">Array of activities to insert</param>
        /// <param name="undoTransactionDescription">Text for the designer transaction which will be created</param>
        public static void InsertActivities(CompositeActivityDesigner compositeActivityDesigner, HitTestInfo insertLocation, ReadOnlyCollection<Activity> activitiesToInsert, string undoTransactionDescription)
        {
            if (compositeActivityDesigner == null)
                throw new ArgumentNullException("compositeActivityDesigner");

            if (compositeActivityDesigner.Activity == null ||
                compositeActivityDesigner.Activity.Site == null ||
                !(compositeActivityDesigner.Activity is CompositeActivity))
                throw new ArgumentException("compositeActivityDesigner");

            if (insertLocation == null)
                throw new ArgumentNullException("insertLocation");

            if (activitiesToInsert == null)
                throw new ArgumentNullException("activitiesToInsert");

            ISite site = compositeActivityDesigner.Activity.Site;

            // now insert the actual activities
            IDesignerHost designerHost = site.GetService(typeof(IDesignerHost)) as IDesignerHost;
            DesignerTransaction trans = null;
            if (designerHost != null && !string.IsNullOrEmpty(undoTransactionDescription))
                trans = designerHost.CreateTransaction(undoTransactionDescription);

            bool moveCase = false;
            try
            {
                //Detect if the activities are being moved or inserted
                foreach (Activity activity in activitiesToInsert)
                {
                    if (activity == null)
                        throw new ArgumentException("activitiesToInsert", SR.GetString(SR.Error_CollectionHasNullEntry));

                    moveCase = ((IComponent)activity).Site != null;
                    break;
                }

                //We purposely create a new instance of activities list so that we do not modify the original one
                if (moveCase)
                    compositeActivityDesigner.MoveActivities(insertLocation, activitiesToInsert);
                else
                    compositeActivityDesigner.InsertActivities(insertLocation, activitiesToInsert);

                if (trans != null)
                    trans.Commit();
            }
            catch (Exception e)
            {
                if (trans != null)
                    trans.Cancel();

                throw e;
            }

            //If we are just moving the activities then we do not need to emit the class
            //for scopes; only when we are adding the activities the class needs to be emitted for
            //scope in code beside file
            if (!moveCase)
            {
                // if everything was successful then generate classes correposnding to new scopes
                // get all the activities underneath the child activities
                ArrayList allActivities = new ArrayList();
                foreach (Activity activity in activitiesToInsert)
                {
                    allActivities.Add(activity);
                    if (activity is CompositeActivity)
                        allActivities.AddRange(Helpers.GetNestedActivities((CompositeActivity)activity));
                }
            }
        }

        /// <summary>
        /// Removes activities from designer by creating designer transaction
        /// </summary>
        /// <param name="serviceProvider">Service Provider associated to providing services</param>
        /// <param name="activitiesToRemove">Array of activities to remove</param>
        /// <param name="transactionDescription">Transaction text used to name the designer transaction</param>
        public static void RemoveActivities(IServiceProvider serviceProvider, ReadOnlyCollection<Activity> activitiesToRemove, string transactionDescription)
        {
            if (serviceProvider == null)
                throw new ArgumentNullException();

            if (activitiesToRemove == null)
                throw new ArgumentNullException("activitiesToRemove");

            Activity nextSelectableActivity = null;

            IDesignerHost designerHost = serviceProvider.GetService(typeof(IDesignerHost)) as IDesignerHost;
            DesignerTransaction trans = null;
            if (designerHost != null && !string.IsNullOrEmpty(transactionDescription))
                trans = designerHost.CreateTransaction(transactionDescription);

            try
            {
                foreach (Activity activity in activitiesToRemove)
                {
                    ActivityDesigner designer = ActivityDesigner.GetDesigner(activity);
                    if (designer != null)
                    {
                        CompositeActivityDesigner parentDesigner = designer.ParentDesigner;
                        if (parentDesigner != null)
                        {
                            nextSelectableActivity = DesignerHelpers.GetNextSelectableActivity(activity);
                            parentDesigner.RemoveActivities(new List<Activity>(new Activity[] { activity }).AsReadOnly());
                        }
                    }
                }

                if (trans != null)
                    trans.Commit();
            }
            catch
            {
                if (trans != null)
                    trans.Cancel();
                throw;
            }

            if (nextSelectableActivity != null && nextSelectableActivity.Site != null)
            {
                ISelectionService selectionService = nextSelectableActivity.Site.GetService(typeof(ISelectionService)) as ISelectionService;
                if (selectionService != null)
                    selectionService.SetSelectedComponents(new Activity[] { nextSelectableActivity }, SelectionTypes.Replace);
            }
        }

        public static IDataObject SerializeActivitiesToDataObject(IServiceProvider serviceProvider, Activity[] activities)
        {
            if (serviceProvider == null)
                throw new ArgumentNullException("serviceProvider");

            if (activities == null)
                throw new ArgumentNullException("activities");

            // get component serialization service
            ComponentSerializationService css = (ComponentSerializationService)serviceProvider.GetService(typeof(ComponentSerializationService));
            if (css == null)
                throw new InvalidOperationException(SR.GetString(SR.General_MissingService, typeof(ComponentSerializationService).Name));

            // serialize all activities to the store
            SerializationStore store = css.CreateStore();
            using (store)
            {
                foreach (Activity activity in activities)
                    css.Serialize(store, activity);
            }

            // wrap it with clipboard style object
            Stream stream = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, store);
            stream.Seek(0, SeekOrigin.Begin);
            DataObject dataObject = new DataObject(CF_DESIGNER, stream);
            dataObject.SetData(CF_DESIGNERSTATE, Helpers.SerializeDesignersToStream(activities));
            return dataObject;
        }

        public static Activity[] DeserializeActivitiesFromDataObject(IServiceProvider serviceProvider, IDataObject dataObj)
        {
            return DeserializeActivitiesFromDataObject(serviceProvider, dataObj, false);
        }

        internal static Activity[] DeserializeActivitiesFromDataObject(IServiceProvider serviceProvider, IDataObject dataObj, bool addAssemblyReference)
        {
            if (serviceProvider == null)
                throw new ArgumentNullException("serviceProvider");

            if (dataObj == null)
                return new Activity[] { };

            IDesignerHost designerHost = (IDesignerHost)serviceProvider.GetService(typeof(IDesignerHost));
            if (designerHost == null)
                throw new InvalidOperationException(SR.GetString(SR.General_MissingService, typeof(IDesignerHost).Name));

            object data = dataObj.GetData(CF_DESIGNER);
            ICollection activities = null;

            if (data is Stream)
            {
                BinaryFormatter formatter = new BinaryFormatter();
                ((Stream)data).Seek(0, SeekOrigin.Begin);
                object serializationData = formatter.Deserialize((Stream)data);
                if (serializationData is SerializationStore)
                {
                    // get component serialization service
                    ComponentSerializationService css = serviceProvider.GetService(typeof(ComponentSerializationService)) as ComponentSerializationService;
                    if (css == null)
                        throw new Exception(SR.GetString(SR.General_MissingService, typeof(ComponentSerializationService).Name));

                    // deserialize data
                    activities = css.Deserialize((SerializationStore)serializationData);
                }
            }
            else
            {
                // Now check for a toolbox item.
                IToolboxService ts = (IToolboxService)serviceProvider.GetService(typeof(IToolboxService));
                if (ts != null && ts.IsSupported(dataObj, designerHost))
                {
                    ToolboxItem toolBoxItem = ts.DeserializeToolboxItem(dataObj, designerHost);
                    if (toolBoxItem != null)
                    {
                        activities = GetActivitiesFromToolboxItem(serviceProvider, addAssemblyReference, designerHost, activities, toolBoxItem);
                    }
                }
            }

            if (activities != null && Helpers.AreAllActivities(activities))
                return (Activity[])new ArrayList(activities).ToArray(typeof(Activity));
            else
                return new Activity[] { };
        }

        private static ICollection GetActivitiesFromToolboxItem(IServiceProvider serviceProvider, bool addAssemblyReference, IDesignerHost designerHost, ICollection activities, ToolboxItem toolBoxItem)
        {
            // this will make sure that we add the assembly reference to project
            if (addAssemblyReference && toolBoxItem.AssemblyName != null)
            {
                ITypeResolutionService trs = serviceProvider.GetService(typeof(ITypeResolutionService)) as ITypeResolutionService;
                if (trs != null)
                    trs.ReferenceAssembly(toolBoxItem.AssemblyName);
            }

            ActivityToolboxItem ActivityToolboxItem = toolBoxItem as ActivityToolboxItem;
            if (addAssemblyReference && ActivityToolboxItem != null)
                activities = ActivityToolboxItem.CreateComponentsWithUI(designerHost);
            else
                activities = toolBoxItem.CreateComponents(designerHost);
            return activities;
        }

        internal static Activity[] DeserializeActivitiesFromToolboxItem(IServiceProvider serviceProvider, ToolboxItem toolboxItem, bool addAssemblyReference)
        {
            if (serviceProvider == null)
                throw new ArgumentNullException("serviceProvider");

            IDesignerHost designerHost = (IDesignerHost)serviceProvider.GetService(typeof(IDesignerHost));
            if (designerHost == null)
                throw new InvalidOperationException(SR.GetString(SR.General_MissingService, typeof(IDesignerHost).Name));

            ICollection activities = null;

            if (toolboxItem != null)
            {
                activities = GetActivitiesFromToolboxItem(serviceProvider, addAssemblyReference, designerHost, activities, toolboxItem);
            }

            if (activities != null && Helpers.AreAllActivities(activities))
                return (Activity[])new ArrayList(activities).ToArray(typeof(Activity));
            else
                return new Activity[] { };
        }

        public static ActivityDesigner[] GetIntersectingDesigners(ActivityDesigner topLevelDesigner, Rectangle rectangle)
        {
            if (topLevelDesigner == null)
                throw new ArgumentNullException("topLevelDesigner");

            List<ActivityDesigner> intersectingDesigners = new List<ActivityDesigner>();

            if (!rectangle.IntersectsWith(topLevelDesigner.Bounds))
                return intersectingDesigners.ToArray();

            if (!topLevelDesigner.Bounds.Contains(rectangle))
                intersectingDesigners.Add(topLevelDesigner);

            if (topLevelDesigner is CompositeActivityDesigner)
            {
                Queue compositeDesigners = new Queue();
                compositeDesigners.Enqueue(topLevelDesigner);
                while (compositeDesigners.Count > 0)
                {
                    CompositeActivityDesigner compositeDesigner = compositeDesigners.Dequeue() as CompositeActivityDesigner;
                    if (compositeDesigner != null)
                    {
                        bool bDrawingVisibleChildren = false;

                        foreach (ActivityDesigner activityDesigner in compositeDesigner.ContainedDesigners)
                        {
                            if (activityDesigner.IsVisible && rectangle.IntersectsWith(activityDesigner.Bounds))
                            {
                                bDrawingVisibleChildren = true;

                                if (!activityDesigner.Bounds.Contains(rectangle))
                                    intersectingDesigners.Add(activityDesigner);

                                if (activityDesigner is CompositeActivityDesigner)
                                    compositeDesigners.Enqueue(activityDesigner);
                            }
                            else
                            {
                                if ((!(compositeDesigner is FreeformActivityDesigner)) && bDrawingVisibleChildren)
                                    break;
                            }
                        }
                    }
                }
            }
            return intersectingDesigners.ToArray();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Checks if the activities can be inserted into the composite activity associated with the designer
        /// </summary>
        /// <param name="insertLocation">Location at which to insert activities</param>
        /// <param name="activitiesToInsert">Array of activities to be inserted</param>
        /// <returns>True of activities can be inserted, false otherwise</returns>
        public virtual bool CanInsertActivities(HitTestInfo insertLocation, ReadOnlyCollection<Activity> activitiesToInsert)
        {
            if (insertLocation == null)
                throw new ArgumentNullException("insertLocation");

            if (activitiesToInsert == null)
                throw new ArgumentNullException("activitiesToInsert");

            CompositeActivity compositeActivity = Activity as CompositeActivity;
            if (compositeActivity == null)
                return false;

            //If the activity state is locked then we can not insert activities.
            if (!IsEditable)
                return false;

            IExtendedUIService2 extendedUIService = this.GetService(typeof(IExtendedUIService2)) as IExtendedUIService2;

            foreach (Activity activity in activitiesToInsert)
            {
                if (activity == null)
                    throw new ArgumentException("activitiesToInsert", SR.GetString(SR.Error_CollectionHasNullEntry));

                if (extendedUIService != null)
                {
                    if (!extendedUIService.IsSupportedType(activity.GetType()))
                    {
                        return false;
                    }
                }

                if (activity is CompositeActivity && Helpers.IsAlternateFlowActivity(activity))
                    return false;

                ActivityDesigner designerToInsert = null;
#pragma warning disable 56506//
                if (activity.Site != null)
                {
                    //get an existing designer
                    IDesignerHost designerHost = activity.Site.GetService(typeof(IDesignerHost)) as IDesignerHost;
                    designerToInsert = (designerHost != null) ? designerHost.GetDesigner((IComponent)activity) as ActivityDesigner : null;
                }
                else
                {
                    //we dont want to create a designer instance every time, so we'll cache it
                    //this is a fix for a perf issue - this function gets called in a loop when doing drag'n'drop operation
                    //from a toolbox
                    if (activity.UserData.Contains(typeof(ActivityDesigner)))
                    {
                        designerToInsert = activity.UserData[typeof(ActivityDesigner)] as ActivityDesigner;
                    }
                    else
                    {
                        //create a new one
                        designerToInsert = ActivityDesigner.CreateDesigner(Activity.Site, activity);
                        activity.UserData[typeof(ActivityDesigner)] = designerToInsert;
                    }
                }
#pragma warning restore 56506//

                if (designerToInsert == null)
                    return false;

                if (!designerToInsert.CanBeParentedTo(this))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Inserts activity at specified location within designer
        /// </summary>
        /// <param name="insertLocation">Location at which to insert activities</param>
        /// <param name="activitiesToInsert">Array of activities to insert</param>
        public virtual void InsertActivities(HitTestInfo insertLocation, ReadOnlyCollection<Activity> activitiesToInsert)
        {
            if (insertLocation == null)
                throw new ArgumentNullException("insertLocation");

            if (activitiesToInsert == null)
                throw new ArgumentNullException("activitiesToInsert");

            CompositeActivity compositeActivity = Activity as CompositeActivity;
            if (compositeActivity == null)
                throw new Exception(SR.GetString(SR.Error_DragDropInvalid));

            int index = insertLocation.MapToIndex();

            IIdentifierCreationService identifierCreationService = GetService(typeof(IIdentifierCreationService)) as IIdentifierCreationService;
            if (identifierCreationService != null)
                identifierCreationService.EnsureUniqueIdentifiers(compositeActivity, activitiesToInsert);
            else
                throw new InvalidOperationException(SR.GetString(SR.General_MissingService, typeof(IIdentifierCreationService).FullName));

            foreach (Activity activity in activitiesToInsert)
            {
                if (activity == null)
                    throw new ArgumentException("activitiesToInsert", SR.GetString(SR.Error_CollectionHasNullEntry));

                if (activity.Parent == null)
                {
                    compositeActivity.Activities.Insert(index++, activity);
                    WorkflowDesignerLoader.AddActivityToDesigner(Activity.Site, activity);
                }
            }

            // filter out unsupported Dependency properties
            foreach (Activity activity in activitiesToInsert)
            {
                Walker walker = new Walker();
                walker.FoundActivity += delegate(Walker w, WalkerEventArgs walkerEventArgs)
                {
                    ExtenderHelpers.FilterDependencyProperties(this.Activity.Site, walkerEventArgs.CurrentActivity);
                };
                walker.Walk(activity);
            }

        }

        /// <summary>
        /// Checks if the activities can be moved out from the composite activity associated with designer
        /// </summary>
        /// <param name="moveLocation">Location from which to move the activities</param>
        /// <param name="activitiesToMove">Array of activities to move</param>
        /// <returns></returns>
        public virtual bool CanMoveActivities(HitTestInfo moveLocation, ReadOnlyCollection<Activity> activitiesToMove)
        {
            if (moveLocation == null)
                throw new ArgumentNullException("moveLocation");

            if (activitiesToMove == null)
                throw new ArgumentNullException("activitiesToMove");

            //If the activity has locked structure then we do not allow user to move the activity out
            if (!IsEditable)
                return false;

            //Now go through all the movable activities and check if their position is locked
            foreach (Activity activity in activitiesToMove)
            {
                ActivityDesigner designer = ActivityDesigner.GetDesigner(activity);
                if (designer == null || designer.IsLocked)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Moves activities from one designer to other
        /// </summary>
        /// <param name="moveLocation">Location at which to move the activities</param>
        /// <param name="activitiesToMove">Array of activities to move</param>
        public virtual void MoveActivities(HitTestInfo moveLocation, ReadOnlyCollection<Activity> activitiesToMove)
        {
            if (moveLocation == null)
                throw new ArgumentNullException("moveLocation");

            if (activitiesToMove == null)
                throw new ArgumentNullException("activitiesToMove");

            //Make sure that we get the composite activity
            CompositeActivity compositeActivity = Activity as CompositeActivity;
            if (compositeActivity == null)
                throw new Exception(SR.GetString(SR.Error_DragDropInvalid));

            IIdentifierCreationService identifierCreationService = GetService(typeof(IIdentifierCreationService)) as IIdentifierCreationService;
            if (identifierCreationService == null)
                throw new InvalidOperationException(SR.GetString(SR.General_MissingService, typeof(IIdentifierCreationService).FullName));

            IDesignerHost designerHost = GetService(typeof(IDesignerHost)) as IDesignerHost;
            if (designerHost == null)
                throw new InvalidOperationException(SR.GetString(SR.General_MissingService, typeof(IDesignerHost).FullName));

            int index = moveLocation.MapToIndex();
            foreach (Activity activity in activitiesToMove)
            {
                ActivityDesigner designer = ActivityDesigner.GetDesigner(activity);
                if (designer != null)
                {
                    CompositeActivityDesigner parentDesigner = designer.ParentDesigner;
                    if (parentDesigner == this)
                    {
                        int originalIndex = compositeActivity.Activities.IndexOf(activity);
                        if (index > originalIndex)
                            index--;
                    }
                }

                //In some cases we might get activities which are added newly, in such cases we check if the activity
                //is existing activity or new one based on this decision we add it to the designer host.
                Debug.Assert(activity.Parent != null);
                CompositeActivity parentActivity = activity.Parent;
                int positionInParent = parentActivity.Activities.IndexOf(activity);
                activity.Parent.Activities.Remove(activity);

                //We need to make sure that the activity is going to have unique identifier
                //This might just cause problems
                identifierCreationService.EnsureUniqueIdentifiers(compositeActivity, new Activity[] { activity });

                //We do not need to read the activity in the designer host as this is move operation
                //assign unique temporary name to avoid conflicts 
                DesignerHelpers.UpdateSiteName(activity, "_activityonthemove_");
                CompositeActivity compositeActivityMoved = activity as CompositeActivity;
                if (compositeActivityMoved != null)
                {
                    int i = 1;
                    foreach (Activity nestedActivity in Helpers.GetNestedActivities(compositeActivityMoved))
                    {
                        DesignerHelpers.UpdateSiteName(nestedActivity, "_activityonthemove_" + i.ToString(CultureInfo.InvariantCulture));
                        i += 1;
                    }
                }

                try
                {
                    compositeActivity.Activities.Insert(index++, activity);
                }
                catch (Exception ex)
                {
                    // reconnect the activity
                    parentActivity.Activities.Insert(positionInParent, activity);

                    //

                    throw ex;
                }

                DesignerHelpers.UpdateSiteName(activity, activity.Name);
                if (compositeActivityMoved != null)
                {
                    foreach (Activity nestedActivity in Helpers.GetNestedActivities(compositeActivityMoved))
                        DesignerHelpers.UpdateSiteName(nestedActivity, nestedActivity.Name);
                }
            }

            // filter out unsupported Dependency properties and refresh propertyDescriptors
            foreach (Activity activity in activitiesToMove)
            {
                Walker walker = new Walker();
                walker.FoundActivity += delegate(Walker w, WalkerEventArgs walkerEventArgs)
                {
                    ExtenderHelpers.FilterDependencyProperties(this.Activity.Site, walkerEventArgs.CurrentActivity);
                    TypeDescriptor.Refresh(walkerEventArgs.CurrentActivity);
                };
                walker.Walk(activity);
            }
        }

        /// <summary>
        /// Checks if activities can be removed from the activity associated with the designer
        /// </summary>
        /// <param name="activitiesToRemove">Array of activities to remove</param>
        /// <returns>True of the activities can be removed, False otherwise.</returns>
        public virtual bool CanRemoveActivities(ReadOnlyCollection<Activity> activitiesToRemove)
        {
            if (activitiesToRemove == null)
                throw new ArgumentNullException("activitiesToRemove");

            if (!IsEditable)
                return false;

            foreach (Activity activity in activitiesToRemove)
            {
                ActivityDesigner designer = ActivityDesigner.GetDesigner(activity);
                if (designer == null || designer.IsLocked)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Removes activities from composite activity associated with the designer
        /// </summary>
        /// <param name="activitiesToRemove">Array of activities to remove</param>
        public virtual void RemoveActivities(ReadOnlyCollection<Activity> activitiesToRemove)
        {
            if (activitiesToRemove == null)
                throw new ArgumentNullException("activitiesToRemove");

            CompositeActivity compositeActivity = Activity as CompositeActivity;
            if (compositeActivity == null)
                throw new Exception(SR.GetString(SR.Error_DragDropInvalid));

            foreach (Activity activity in activitiesToRemove)
            {
                compositeActivity.Activities.Remove(activity);

                //Before we destroy the activity make sure that the references it and its child activities store to its parent
                //are set to null or else an undo unit will be created
                //For details look at,
                //\\cpvsbuild\drops\whidbey\pd6\raw\40903.19\sources\ndp\fx\src\Designer\Host\UndoEngine.cs
                //OnComponentRemoving function which retains the references we hold to the parent
                //This 

                activity.SetParent(null);
                if (activity is CompositeActivity)
                {
                    foreach (Activity nestedActivity in Helpers.GetNestedActivities(activity as CompositeActivity))
                        nestedActivity.SetParent(null);
                }

                WorkflowDesignerLoader.RemoveActivityFromDesigner(Activity.Site, activity);
            }
        }

        /// <summary>
        /// Checks if the contained designer is to be made visible
        /// </summary>
        /// <param name="containedDesigner">Contained designer to check for visibility</param>
        /// <returns></returns>
        public virtual bool IsContainedDesignerVisible(ActivityDesigner containedDesigner)
        {
            if (containedDesigner == null)
                throw new ArgumentNullException("containedDesigner");

            return true;
        }

        /// <summary>
        /// Makes sure that the child designer will be made visible
        /// </summary>
        /// <param name="containedDesigner">Contained designer to make visible</param>
        public virtual void EnsureVisibleContainedDesigner(ActivityDesigner containedDesigner)
        {
            if (containedDesigner == null)
                throw new ArgumentNullException("containedDesigner");

            Expanded = true;
        }

        /// <summary>
        /// Gets the object which is next in the order of navigation
        /// </summary>
        /// <param name="current">Current object in the navigation order</param>
        /// <param name="navigate">Navigation direction</param>
        /// <returns></returns>
        public virtual object GetNextSelectableObject(object current, DesignerNavigationDirection direction)
        {
            return null;
        }

        public override HitTestInfo HitTest(Point point)
        {
            HitTestInfo hitInfo = HitTestInfo.Nowhere;

            if (ExpandButtonRectangle.Contains(point))
            {
                hitInfo = new HitTestInfo(this, HitTestLocations.Designer | HitTestLocations.ActionArea);
            }
            else if (Expanded && Bounds.Contains(point))
            {
                //First check if any of our children are hit.
                ReadOnlyCollection<ActivityDesigner> containedDesigners = ContainedDesigners;
                for (int i = containedDesigners.Count - 1; i >= 0; i--)
                {
                    ActivityDesigner activityDesigner = containedDesigners[i] as ActivityDesigner;
                    if (activityDesigner != null && activityDesigner.IsVisible)
                    {
                        hitInfo = activityDesigner.HitTest(point);
                        if (hitInfo.HitLocation != HitTestLocations.None)
                            break;
                    }
                }
            }

            //If no children are hit then call base class's hittest
            if (hitInfo == HitTestInfo.Nowhere)
                hitInfo = base.HitTest(point);

            //This is to create default hittest info in case the drawing state is invalid
            if (hitInfo.AssociatedDesigner != null && hitInfo.AssociatedDesigner.DrawingState != DrawingStates.Valid)
                hitInfo = new HitTestInfo(hitInfo.AssociatedDesigner, HitTestLocations.Designer | HitTestLocations.ActionArea);

            return hitInfo;
        }
        #endregion

        #region Protected Methods
        /// <summary>
        /// Notifies that the activity associated with contained designer has changed.
        /// </summary>
        /// <param name="e">ActivityChangedEventArgs containing information about the change.</param>
        protected virtual void OnContainedActivityChanged(ActivityChangedEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException("e");
        }

        /// <summary>
        /// Notifies that number of activities contained within the designers are changing
        /// </summary>
        /// <param name="listChangeArgs">ActivityCollectionChangeEventArgs containing information about what is about to change</param>
        protected virtual void OnContainedActivitiesChanging(ActivityCollectionChangeEventArgs listChangeArgs)
        {
            if (listChangeArgs == null)
                throw new ArgumentNullException("listChangeArgs");
        }

        /// <summary>
        /// Notifies that number of activities contained within the designers have changed
        /// </summary>
        /// <param name="listChangeArgs">ItemListChangeEventArgs containing information about what has changed</param>
        protected virtual void OnContainedActivitiesChanged(ActivityCollectionChangeEventArgs listChangeArgs)
        {
            if (listChangeArgs == null)
                throw new ArgumentNullException("listChangeArgs");

            // Update the status of all the designers
            foreach (ActivityDesigner activityDesigner in ContainedDesigners)
            {
                foreach (DesignerVerb designerVerb in ((IDesigner)activityDesigner).Verbs)
                {
                    int status = designerVerb.OleStatus;
                    status = 0;
                }
            }

            RefreshDesignerVerbs();

            //clear the list of activity designers (force to create the new list the next time it is requested)
            this.containedActivityDesigners = null;

            PerformLayout();
        }

        protected override void Initialize(Activity activity)
        {
            base.Initialize(activity);

            ///

            CompositeActivity compositeActivity = activity as CompositeActivity;
            if (compositeActivity != null)
            {
                compositeActivity.Activities.ListChanging += new EventHandler<ActivityCollectionChangeEventArgs>(OnActivityListChanging);
                compositeActivity.Activities.ListChanged += new EventHandler<ActivityCollectionChangeEventArgs>(OnActivityListChanged);
            }

            if (IsRootDesigner)
            {
                IComponentChangeService componentChangeService = GetService(typeof(IComponentChangeService)) as IComponentChangeService;
                if (componentChangeService != null)
                {
                    componentChangeService.ComponentAdded += new ComponentEventHandler(OnComponentAdded);
                    componentChangeService.ComponentChanged += new ComponentChangedEventHandler(OnComponentChanged);
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                CompositeActivity compositeActivity = Activity as CompositeActivity;
                if (compositeActivity != null)
                {
                    compositeActivity.Activities.ListChanging -= new EventHandler<ActivityCollectionChangeEventArgs>(OnActivityListChanging);
                    compositeActivity.Activities.ListChanged -= new EventHandler<ActivityCollectionChangeEventArgs>(OnActivityListChanged);
                }

                if (IsRootDesigner)
                {
                    IComponentChangeService componentChangeService = GetService(typeof(IComponentChangeService)) as IComponentChangeService;
                    if (componentChangeService != null)
                    {
                        componentChangeService.ComponentAdded -= new ComponentEventHandler(OnComponentAdded);
                        componentChangeService.ComponentChanged -= new ComponentChangedEventHandler(OnComponentChanged);
                    }
                }
            }

            base.Dispose(disposing);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (ExpandButtonRectangle.Contains(new Point(e.X, e.Y)))
            {
                //In order to property update the menu items for expand collapse, rather than setting the property
                //directly we go thru menu command service
                IMenuCommandService menuCommandService = GetService(typeof(IMenuCommandService)) as IMenuCommandService;
                if (menuCommandService != null)
                    menuCommandService.GlobalInvoke((Expanded) ? WorkflowMenuCommands.Collapse : WorkflowMenuCommands.Expand);
                else
                    Expanded = !Expanded;
            }
        }

        protected override void OnPaint(ActivityDesignerPaintEventArgs e)
        {
            base.OnPaint(e);

            CompositeDesignerTheme compositeDesignerTheme = e.DesignerTheme as CompositeDesignerTheme;
            if (compositeDesignerTheme == null)
                return;

            //Draw the expand collapse button and the connection
            if (CanExpandCollapse)
            {
                Rectangle expandButtonRectangle = ExpandButtonRectangle;
                if (!expandButtonRectangle.Size.IsEmpty)
                {
                    ActivityDesignerPaint.DrawExpandButton(e.Graphics, expandButtonRectangle, !Expanded, compositeDesignerTheme);
                }
            }

            if (Expanded)
                PaintContainedDesigners(e);
        }

        protected override void OnLayoutPosition(ActivityDesignerLayoutEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException("e");

            base.OnLayoutPosition(e);

            foreach (ActivityDesigner activityDesigner in ContainedDesigners)
            {
                try
                {
                    ((IWorkflowDesignerMessageSink)activityDesigner).OnLayoutPosition(e.Graphics);
                    activityDesigner.DrawingState &= (~DrawingStates.InvalidPosition);
                }
                catch
                {
                    //Eat the exception thrown
                    activityDesigner.DrawingState |= DrawingStates.InvalidPosition;
                }
            }
        }

        protected override Size OnLayoutSize(ActivityDesignerLayoutEventArgs e)
        {
            Size containerSize = base.OnLayoutSize(e);

            //Calculate the size of internal designers
            foreach (ActivityDesigner activityDesigner in ContainedDesigners)
            {
                try
                {
                    ((IWorkflowDesignerMessageSink)activityDesigner).OnLayoutSize(e.Graphics);
                    activityDesigner.DrawingState &= (~DrawingStates.InvalidSize);
                }
                catch
                {
                    //Eat the exception thrown
                    activityDesigner.Size = activityDesigner.DesignerTheme.Size;
                    activityDesigner.DrawingState |= DrawingStates.InvalidSize;
                }
            }

            if (!String.IsNullOrEmpty(Text))
                this.actualTextSize = ActivityDesignerPaint.MeasureString(e.Graphics, e.DesignerTheme.BoldFont, Text, StringAlignment.Center, Size.Empty);
            else
                this.actualTextSize = Size.Empty;

            if (Expanded)
                containerSize.Height = TitleHeight;
            else
                containerSize.Height = TitleHeight + WorkflowTheme.CurrentTheme.AmbientTheme.Margin.Height;

            return containerSize;
        }

        protected override void SaveViewState(BinaryWriter writer)
        {
            if (writer == null)
                throw new ArgumentNullException("writer");

            writer.Write(Expanded);
            base.SaveViewState(writer);
        }

        protected override void LoadViewState(BinaryReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");

            Expanded = reader.ReadBoolean();
            base.LoadViewState(reader);
        }

        protected override void OnThemeChange(ActivityDesignerTheme designerTheme)
        {
            base.OnThemeChange(designerTheme);

            CompositeActivity compositeActivity = Activity as CompositeActivity;
            if (compositeActivity != null)
            {
                foreach (Activity activity in compositeActivity.Activities)
                {
                    IWorkflowDesignerMessageSink containedDesigner = ActivityDesigner.GetDesigner(activity) as IWorkflowDesignerMessageSink;
                    if (containedDesigner != null)
                        containedDesigner.OnThemeChange();
                }
            }
        }

        /// <summary>
        /// Call the OnPaint on the contained designers
        /// </summary>
        /// <param name="e">EventArgs to be used for painting</param>
        protected void PaintContainedDesigners(ActivityDesignerPaintEventArgs e)
        {
            OnPaintContainedDesigners(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException("e");

            ISelectionService selectionService = GetService(typeof(ISelectionService)) as ISelectionService;
            object selectedObject = (selectionService != null) ? selectionService.PrimarySelection : null;
            if (selectedObject == null)
                return;

            //handling of the key move events for all designers
            //the freeform designer will override that to allow moving of the designers vs moving selection
            object nextSelectedObject = null;

            if (e.KeyCode == Keys.Down ||
                (e.KeyCode == Keys.Tab && !e.Shift))
            {
                CompositeActivityDesigner selectedDesigner = ActivityDesigner.GetDesigner(selectedObject as Activity) as CompositeActivityDesigner;
                if (selectedDesigner != null)
                    nextSelectedObject = selectedDesigner.FirstSelectableObject;

                if (nextSelectedObject == null)
                {
                    do
                    {
                        // get parent designer
                        CompositeActivityDesigner parentDesigner = ActivityDesigner.GetParentDesigner(selectedObject);
                        if (parentDesigner == null)
                        {
                            // IMPORTANT: This will only happen when the focus is on the last connector in ServiceDesigner
                            nextSelectedObject = selectedObject;
                            break;
                        }

                        nextSelectedObject = parentDesigner.GetNextSelectableObject(selectedObject, DesignerNavigationDirection.Down);
                        if (nextSelectedObject != null)
                            break;

                        selectedObject = parentDesigner.Activity;
                    } while (true);
                }
            }
            else if (e.KeyCode == Keys.Up ||
                    (e.KeyCode == Keys.Tab && e.Shift))
            {
                // get parent designer
                CompositeActivityDesigner parentDesigner = ActivityDesigner.GetParentDesigner(selectedObject);
                if (parentDesigner == null)
                {
                    // IMPORTANT: This will only happen when the focus is on the ServiceDesigner it self
                    CompositeActivityDesigner selectedDesigner = ActivityDesigner.GetDesigner(selectedObject as Activity) as CompositeActivityDesigner;
                    if (selectedDesigner != null)
                        nextSelectedObject = selectedDesigner.LastSelectableObject;
                }
                else
                {
                    // ask for previous component
                    nextSelectedObject = parentDesigner.GetNextSelectableObject(selectedObject, DesignerNavigationDirection.Up);
                    if (nextSelectedObject != null)
                    {
                        CompositeActivityDesigner nextSelectedDesigner = ActivityDesigner.GetDesigner(nextSelectedObject as Activity) as CompositeActivityDesigner;
                        // when we go up, and then upper selection is parent designer then we look for last component
                        if (nextSelectedDesigner != null)
                        {
                            object lastObject = nextSelectedDesigner.LastSelectableObject;
                            if (lastObject != null)
                                nextSelectedObject = lastObject;
                        }
                    }
                    else
                    {
                        nextSelectedObject = parentDesigner.Activity;
                    }
                }
            }
            else if (e.KeyCode == Keys.Left)
            {
                do
                {
                    CompositeActivityDesigner parentDesigner = ActivityDesigner.GetParentDesigner(selectedObject);
                    if (parentDesigner == null)
                        break;

                    nextSelectedObject = parentDesigner.GetNextSelectableObject(selectedObject, DesignerNavigationDirection.Left);
                    if (nextSelectedObject != null)
                        break;

                    selectedObject = parentDesigner.Activity;
                } while (true);
            }
            else if (e.KeyCode == Keys.Right)
            {
                do
                {
                    CompositeActivityDesigner parentDesigner = ActivityDesigner.GetParentDesigner(selectedObject);
                    if (parentDesigner == null)
                        break;

                    nextSelectedObject = parentDesigner.GetNextSelectableObject(selectedObject, DesignerNavigationDirection.Right);
                    if (nextSelectedObject != null)
                        break;

                    selectedObject = parentDesigner.Activity;
                } while (true);
            }

            // now select the component
            if (nextSelectedObject != null)
            {
                selectionService.SetSelectedComponents(new object[] { nextSelectedObject }, SelectionTypes.Replace);

                // make the selected designer visible
                ParentView.EnsureVisible(nextSelectedObject);
            }
        }
        #endregion

        #region Private Methods
        internal virtual void OnPaintContainedDesigners(ActivityDesignerPaintEventArgs e)
        {
            foreach (ActivityDesigner activityDesigner in ContainedDesigners)
            {
                using (PaintEventArgs paintEventArgs = new PaintEventArgs(e.Graphics, e.ViewPort))
                {
                    ((IWorkflowDesignerMessageSink)activityDesigner).OnPaint(paintEventArgs, e.ViewPort);
                }
            }
        }

        private void OnComponentAdded(object sender, ComponentEventArgs e)
        {
            ActivityDesigner designer = ActivityDesigner.GetDesigner(e.Component as Activity);
            if (Activity != e.Component && designer != null && designer.IsLocked)
                DesignerHelpers.MakePropertiesReadOnly(e.Component.Site, designer.Activity);
        }

        private void OnComponentChanged(object sender, ComponentChangedEventArgs e)
        {
            IReferenceService referenceService = GetService(typeof(IReferenceService)) as IReferenceService;
            Activity changedActivity = (referenceService != null) ? referenceService.GetComponent(e.Component) as Activity : e.Component as Activity;
            if (changedActivity != null)
            {
                ActivityDesigner designer = ActivityDesigner.GetDesigner(changedActivity);
                if (designer != null)
                {
                    CompositeActivityDesigner parentDesigner = designer.ParentDesigner;
                    if (parentDesigner != null)
                        parentDesigner.OnContainedActivityChanged(new ActivityChangedEventArgs(changedActivity, e.Member, e.OldValue, e.NewValue));
                }
            }
        }

        private void OnActivityListChanging(object sender, ActivityCollectionChangeEventArgs e)
        {
            OnContainedActivitiesChanging(e);
        }

        private void OnActivityListChanged(object sender, ActivityCollectionChangeEventArgs e)
        {
            OnContainedActivitiesChanged(e);
        }
        #endregion

        #endregion

        #region Properties and Methods
        //
        public static void MoveDesigners(ActivityDesigner activityDesigner, bool moveBack)
        {
            if (activityDesigner == null)
                throw new ArgumentNullException("activityDesigner");

            Activity activity = activityDesigner.Activity as Activity;
            if (activity == null || activity.Parent == null)
                return;

            CompositeActivity compositeActivity = activity.Parent as CompositeActivity;
            if (compositeActivity == null || !compositeActivity.Activities.Contains(activity))
                return;

            int index = compositeActivity.Activities.IndexOf(activity);
            index += (moveBack) ? -1 : 1;
            if (index < 0 || index >= compositeActivity.Activities.Count)
                return;

            IDesignerHost designerHost = compositeActivity.Site.GetService(typeof(IDesignerHost)) as IDesignerHost;
            if (designerHost == null)
                return;

            DesignerTransaction trans = designerHost.CreateTransaction(SR.GetString(SR.MovingActivities));
            try
            {
                compositeActivity.Activities.Remove(activity);
                compositeActivity.Activities.Insert(index, activity);

                ISelectionService selectionService = compositeActivity.Site.GetService(typeof(ISelectionService)) as ISelectionService;
                if (selectionService != null)
                    selectionService.SetSelectedComponents(new object[] { activity });

                if (trans != null)
                    trans.Commit();
            }
            catch
            {
                if (trans != null)
                    trans.Cancel();
                throw;
            }

            CompositeActivityDesigner compositeDesigner = ActivityDesigner.GetDesigner(compositeActivity) as CompositeActivityDesigner;
            if (compositeDesigner != null)
                compositeDesigner.PerformLayout();
        }
        #endregion
    }
    #endregion

}
