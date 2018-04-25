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

    #region ActivityDesigner Class
    /// <summary>
    /// ActivityDesigner provides a simple designer which allows user to visually design activities in the design mode. 
    /// ActivityDesigner provides simple mechanism using which the activities can participate in rendering the Workflow.
    /// ActivityDesigner enables the user to customize layouting, drawing associated with the activity. 
    /// It also enables the user to extend the meta data associated with the activity.
    /// </summary>
    [ActivityDesignerTheme(typeof(ActivityDesignerTheme))]
    [SRCategory("ActivityDesigners", "System.Workflow.ComponentModel.Design.DesignerResources")]
    [DesignerSerializer(typeof(ActivityDesignerLayoutSerializer), typeof(WorkflowMarkupSerializer))]
    [ToolboxItemFilter("Microsoft.Workflow.VSDesigner", ToolboxItemFilterType.Require)]
    [ToolboxItemFilter("System.Workflow.ComponentModel.Design.ActivitySet", ToolboxItemFilterType.Custom)]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class ActivityDesigner : IDisposable,
                                    IDesignerFilter,
                                    IDesigner,
                                    IToolboxUser,
                                    IPersistUIState,
                                    IWorkflowDesignerMessageSink,
                                    IWorkflowRootDesigner,
                                    IConnectableDesigner
    {
        #region Fields
        //Members which determine the linesize of text in ActivityDesigner
        private const int MaximumCharsPerLine = 8;
        private const int MaximumTextLines = 2;
        private const int MaximumIdentifierLength = 25;
        private const int MaximumDescriptionLength = 80;
        private const uint FrameworkVersion_3_5 = 0x00030005;
        private const uint FrameworkVersion_3_0 = 0x00030000;

        //ActivityDesigner Related Members
        private Activity activity;
        private ActivityDesignerAccessibleObject accessibilityObject;
        private ActivityDesignerVerbCollection designerVerbs;
        private List<DesignerAction> designerActions;

        [Flags]
        internal enum DrawingStates { Valid = 0, InvalidPosition = 1, InvalidSize = 2, InvalidDraw = 4 };
        private DrawingStates drawingState = DrawingStates.Valid;

        private Point location = Point.Empty;
        private Size size = Size.Empty;
        private Image image;
        private string text = String.Empty;
        private Size textSize = Size.Empty;
        private bool smartTagVisible = false;
        private SmartTag smartTag = new SmartTag();
        private bool isVisible = true;
        private string rulesText = null;

        //RootDesigner Related Members
        private CompositeActivityDesigner invokingDesigner;
        private WorkflowView workflowView;
        #endregion

        #region Construction and Destruction
        public ActivityDesigner()
        {
        }

        ~ActivityDesigner()
        {
            Dispose(false);
        }
        #endregion

        #region Properties

        #region Public Properties
        /// <summary>
        /// Gets activity being designed by the designer.
        /// </summary>
        public Activity Activity
        {
            get
            {
                return this.activity;
            }
        }

        /// <summary>
        /// Gets the parent designer of existing designer
        /// </summary>
        public CompositeActivityDesigner ParentDesigner
        {
            get
            {
                CompositeActivityDesigner parentDesigner = null;
                IWorkflowRootDesigner rootDesigner = this as IWorkflowRootDesigner;
                if (rootDesigner != null && IsRootDesigner)
                    parentDesigner = rootDesigner.InvokingDesigner;
                else if (Activity != null && Activity.Parent != null)
                    parentDesigner = ActivityDesigner.GetDesigner(Activity.Parent) as CompositeActivityDesigner;
                return parentDesigner;
            }
        }

        /// <summary>
        /// Gets value indicating if the activity associated with the designer is selected.
        /// </summary>
        public bool IsSelected
        {
            get
            {
                ISelectionService selectionService = GetService(typeof(ISelectionService)) as ISelectionService;
                return (selectionService != null && selectionService.GetComponentSelected(Activity));
            }
        }

        /// <summary>
        /// Gets value indicating if the activity associated with the designer is primary selection.
        /// </summary>
        public bool IsPrimarySelection
        {
            get
            {
                ISelectionService selectionService = GetService(typeof(ISelectionService)) as ISelectionService;
                return (selectionService != null && selectionService.PrimarySelection == Activity);
            }
        }

        /// <summary>
        /// Gets the accessibility object associated with the designer
        /// </summary>
        public virtual AccessibleObject AccessibilityObject
        {
            get
            {
                if (this.accessibilityObject == null)
                    this.accessibilityObject = new ActivityDesignerAccessibleObject(this);
                return this.accessibilityObject;
            }
        }

        /// <summary>
        /// Gets the value indicating if a designer is visible on the workflow.
        /// Designer is said to be invisible if the parent of the designer is collapsed or in cases where
        /// designer is not shown on the workflow.
        /// </summary>
        public virtual bool IsVisible
        {
            get
            {
                return this.isVisible;
            }
        }

        /// <summary>
        /// Get value indicating if the activity associated with the designer can be modified. 
        /// This property also controls the position of the designer in the workflow. 
        /// It is used in edit operations such as drag drop, delete, cut/copy/paste etc.
        /// </summary>
        public bool IsLocked
        {
            get
            {
                if (Helpers.IsActivityLocked(Activity))
                    return true;

                if (DrawingState != DrawingStates.Valid)
                    return true;

                WorkflowDesignerLoader loader = GetService(typeof(WorkflowDesignerLoader)) as WorkflowDesignerLoader;
                if (loader != null && loader.InDebugMode)
                    return true;

                IWorkflowRootDesigner rootDesigner = ActivityDesigner.GetSafeRootDesigner(Activity.Site) as IWorkflowRootDesigner;
                if (rootDesigner != null && rootDesigner.InvokingDesigner != null)
                    return true;

                return false;
            }
        }

        /// <summary>
        /// Gets or Sets the location of the designer in logical coordinates.
        /// </summary>
        public virtual Point Location
        {
            get
            {
                return this.location;
            }

            set
            {
                if (ParentDesigner is FreeformActivityDesigner)
                    value = DesignerHelpers.SnapToGrid(value);

                if (this.location != value)
                    this.location = value;
            }
        }

        /// <summary>
        /// Gets or Sets the size of the designer.
        /// </summary>
        public virtual Size Size
        {
            get
            {
                return this.size;
            }

            set
            {
                value.Width = Math.Max(value.Width, MinimumSize.Width);
                value.Height = Math.Max(value.Height, MinimumSize.Height);

                if (this.size != value)
                    this.size = value;
            }
        }

        /// <summary>
        /// Get the minimum size of the designer
        /// </summary>
        public virtual Size MinimumSize
        {
            get
            {
                return DesignerTheme.Size;
            }
        }

        /// <summary>
        /// Get value for enclosing rectangle of the designer in logical coordinates.
        /// </summary>
        public Rectangle Bounds
        {
            get
            {
                //Bounds contain designer rectangle + the selection area
                return new Rectangle(Location, Size);
            }
        }

        /// <summary>
        /// Gets the image associated with the designer.
        /// </summary>
        public virtual Image Image
        {
            get
            {
                return this.image;
            }
            protected set
            {
                this.image = value;
                PerformLayout();
            }
        }

        /// <summary>
        /// Gets the text associated with the designer.
        /// </summary>
        public virtual string Text
        {
            get
            {
                return this.text;
            }
            protected set
            {
                if (value == null || value.Length == 0 || this.text == value)
                    return;

                this.text = value;
                PerformLayout();
            }
        }

        /// <summary>
        /// Gets the current theme to be used for the designer
        /// </summary>
        public ActivityDesignerTheme DesignerTheme
        {
            get
            {
                return WorkflowTheme.CurrentTheme.GetDesignerTheme(this);
            }
        }

        /// <summary>
        /// Gets the value indicating if the designer is root designer.
        /// </summary>
        public bool IsRootDesigner
        {
            get
            {
                bool isRootDesigner = false;
                IDesignerHost designerHost = GetService(typeof(IDesignerHost)) as IDesignerHost;
                if (designerHost != null)
                    isRootDesigner = (designerHost.RootComponent == Activity);
                return isRootDesigner;
            }
        }
        #endregion

        #region Protected Properties
        /// <summary>
        /// Returns the WorkflowView containing the designer
        /// </summary>
        protected internal WorkflowView ParentView
        {
            get
            {
                return GetService(typeof(WorkflowView)) as WorkflowView;
            }
        }

        /// <summary>
        /// Gets the collection of verbs to be associated with the designer. 
        /// The verbs are shown on context menu and the top level workflow menu.
        /// </summary>
        protected virtual ActivityDesignerVerbCollection Verbs
        {
            get
            {
                if (this.designerVerbs == null)
                {
                    this.designerVerbs = new ActivityDesignerVerbCollection();
                    if (!IsLocked)
                        this.designerVerbs.Add(new ActivityDesignerVerb(this, DesignerVerbGroup.General, DR.GetString(DR.GenerateEventHandlers), new EventHandler(OnGenerateEventHandler), new EventHandler(OnGenerateEventHandlerStatusUpdate)));

                    // Add the item to choose an activity datasource
                    WorkflowDesignerLoader loader = GetService(typeof(WorkflowDesignerLoader)) as WorkflowDesignerLoader;
                    if (this.Activity.Parent != null)
                        this.designerVerbs.Add(new ActivityDesignerVerb(this, DesignerVerbGroup.General, DR.GetString(DR.PromoteBindings), new EventHandler(OnPromoteBindings), new EventHandler(OnPromoteBindingsStatusUpdate)));

                    this.designerVerbs.Add(new ActivityDesignerVerb(this, DesignerVerbGroup.General, DR.GetString(DR.BindSelectedProperty), new EventHandler(OnBindProperty), new EventHandler(OnBindPropertyStatusUpdate)));

                    ActivityDesignerVerb designerVerb = new ActivityDesignerVerb(this, DesignerVerbGroup.General, DR.GetString(DR.MoveLeftDesc), new EventHandler(OnMoveBranch), new EventHandler(OnStatusMoveBranch));
                    designerVerb.Properties[DesignerUserDataKeys.MoveBranchKey] = true;
                    this.designerVerbs.Add(designerVerb);

                    designerVerb = new ActivityDesignerVerb(this, DesignerVerbGroup.General, DR.GetString(DR.MoveRightDesc), new EventHandler(OnMoveBranch), new EventHandler(OnStatusMoveBranch));
                    designerVerb.Properties[DesignerUserDataKeys.MoveBranchKey] = false;
                    this.designerVerbs.Add(designerVerb);

                    foreach (ActivityDesignerVerb smartVerb in SmartTagVerbs)
                        this.designerVerbs.Add(smartVerb);
                }

                return this.designerVerbs;
            }
        }

        /// <summary>
        /// Gets or sets the value indicating if smarttag should be shown.
        /// </summary>
        protected virtual bool ShowSmartTag
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the array of actions which the designer wants to associated with the smarttag.
        /// </summary>
        protected virtual ReadOnlyCollection<ActivityDesignerVerb> SmartTagVerbs
        {
            get
            {
                return new List<ActivityDesignerVerb>().AsReadOnly();
            }
        }

        /// <summary>
        /// Gets the Rectangle where the smarttag needs to be displayed
        /// </summary>
        protected virtual Rectangle SmartTagRectangle
        {
            get
            {
                Rectangle smartTagRectangle = Rectangle.Empty;

                Rectangle imageRectangle = ImageRectangle;
                if (!imageRectangle.Size.IsEmpty)
                {
                    smartTagRectangle = imageRectangle;
                }

                return smartTagRectangle;
            }
        }

        /// <summary>
        /// Gets the array of actions associated with the configuration errors.
        /// </summary>
        protected internal virtual ReadOnlyCollection<DesignerAction> DesignerActions
        {
            get
            {
                if (this.designerActions == null)
                {
                    this.designerActions = new List<DesignerAction>();

                    Activity activity = Activity;
                    if (activity != null)
                    {
                        bool isNestedInComment = ActivityDesigner.IsCommentedActivity(activity);

                        WorkflowDesignerLoader loader = GetService(typeof(WorkflowDesignerLoader)) as WorkflowDesignerLoader;
                        bool debugMode = (loader != null && loader.InDebugMode);

                        if (activity.Enabled && !isNestedInComment && !IsLocked && activity.Site != null && !debugMode)
                        {
                            ValidationErrorCollection validationErrors = new ValidationErrorCollection();
                            try
                            {
                                ValidationManager validationManager = new ValidationManager(Activity.Site, false);
                                using (WorkflowCompilationContext.CreateScope(validationManager))
                                {
                                    Activity rootActivity = Helpers.GetRootActivity(this.Activity);
                                    foreach (Validator validator in validationManager.GetValidators(activity.GetType()))
                                        validationErrors.AddRange(validator.Validate(validationManager, activity));
                                }
                            }
                            catch
                            {
                                Debug.WriteLine("Validate call failed");
                            }

                            //Populate the validation errors
                            if (validationErrors.Count > 0)
                            {
                                for (int i = 0; i < validationErrors.Count; i++)
                                {
                                    ValidationError error = validationErrors[i] as ValidationError;
                                    Debug.Assert(error != null, "someone inserted a null or no 'ValidationError' type error in errors collection.");
                                    if (error != null && !error.IsWarning)
                                    {
                                        DesignerAction designerAction = new DesignerAction(this, i, error.ErrorText, AmbientTheme.ConfigErrorImage);
                                        designerAction.PropertyName = error.PropertyName;
                                        foreach (DictionaryEntry entry in error.UserData)
                                            designerAction.UserData[entry.Key] = entry.Value;
                                        this.designerActions.Add(designerAction);
                                    }
                                }
                            }
                        }
                    }
                }

                return this.designerActions.AsReadOnly();
            }
        }

        /// <summary>
        /// Gets the array of glyphs with which to adorn the designer.
        /// </summary>
        protected internal virtual ActivityDesignerGlyphCollection Glyphs
        {
            get
            {
                ActivityDesignerGlyphCollection glyphs = new ActivityDesignerGlyphCollection();

                if (IsSelected)
                {
                    if (IsPrimarySelection)
                        glyphs.Add(PrimarySelectionGlyph.Default);
                    else
                        glyphs.Add(NonPrimarySelectionGlyph.Default);
                }

                bool isNestedInComment = ActivityDesigner.IsCommentedActivity(Activity);

                WorkflowDesignerLoader loader = GetService(typeof(WorkflowDesignerLoader)) as WorkflowDesignerLoader;
                bool debugMode = (loader != null && loader.InDebugMode);

                if (WorkflowTheme.CurrentTheme.AmbientTheme.ShowConfigErrors &&
                    Activity.Enabled && !isNestedInComment && !debugMode &&
                    DesignerActions.Count > 0)
                    glyphs.Add(ConfigErrorGlyph.Default);

                //Add comment glyph only for ctop level comments
                if (!Activity.Enabled && !isNestedInComment)
                    glyphs.Add(CommentGlyph.Default);

                // 
                if (Helpers.IsActivityLocked(Activity))
                    glyphs.Add(LockedActivityGlyph.Default);

                if (SmartTagVisible && ShowSmartTag)
                    glyphs.Add(this.smartTag);

                return glyphs;
            }
        }

        /// <summary>
        /// Gets the value of text rectangle in logical coordinates.
        /// </summary>
        protected virtual Rectangle TextRectangle
        {
            get
            {
                //BY DEFAULT THE TEXT IS ALIGNED TO THE BOTTOM
                if (String.IsNullOrEmpty(Text))
                    return Rectangle.Empty;

                AmbientTheme ambientTheme = WorkflowTheme.CurrentTheme.AmbientTheme;
                ActivityDesignerTheme designerTheme = DesignerTheme;

                Rectangle bounds = Bounds;
                Rectangle textRectangle = Rectangle.Empty;
                textRectangle.X = bounds.Left + ambientTheme.Margin.Width;
                textRectangle.X += (Image != null) ? designerTheme.ImageSize.Width + ambientTheme.Margin.Width : 0;
                textRectangle.Y = bounds.Top + (bounds.Height - this.textSize.Height) / 2;
                textRectangle.Size = this.textSize;
                return textRectangle;
            }
        }

        /// <summary>
        /// Gets the value for enclosing bounds image associated with the designer in logical coordinates.
        /// </summary>
        protected virtual Rectangle ImageRectangle
        {
            get
            {
                //BY DEFAULT THE ICON RECTANGLE IS ALIGNED TO TOP / CENTER
                if (Image == null)
                    return Rectangle.Empty;

                AmbientTheme ambientTheme = WorkflowTheme.CurrentTheme.AmbientTheme;
                ActivityDesignerTheme designerTheme = DesignerTheme;

                Rectangle bounds = Bounds;
                Rectangle imageRectangle = Rectangle.Empty;
                imageRectangle.X = bounds.Left + WorkflowTheme.CurrentTheme.AmbientTheme.Margin.Width;
                imageRectangle.Y = bounds.Top + (bounds.Height - DesignerTheme.ImageSize.Height) / 2;
                imageRectangle.Size = designerTheme.ImageSize;
                return imageRectangle;
            }
        }

        protected virtual CompositeActivityDesigner InvokingDesigner
        {
            get
            {
                return this.invokingDesigner;
            }

            set
            {
                this.invokingDesigner = value;
            }
        }

        protected virtual ReadOnlyCollection<WorkflowDesignerMessageFilter> MessageFilters
        {
            get
            {
                List<WorkflowDesignerMessageFilter> stockFilters = new List<WorkflowDesignerMessageFilter>();
                stockFilters.Add(new ConnectionManager());
                stockFilters.Add(new ResizingMessageFilter());
                stockFilters.Add(new DynamicActionMessageFilter());
                stockFilters.Add(new AutoScrollingMessageFilter());
                stockFilters.Add(new AutoExpandingMessageFilter());
                stockFilters.Add(new DragSelectionMessageFilter());
                stockFilters.Add(new FreeFormDragDropManager());
                return stockFilters.AsReadOnly();
            }
        }

        /// <summary>
        /// Get if the designer is resizable using the rubberbanding when dropped in FreeformDesigner
        /// </summary>
        protected internal virtual bool EnableVisualResizing
        {
            get
            {
                return false;
            }
        }
        #endregion

        #region Private Properties

        #region Properties used during serialization only
        //Note that the following property is used by ActivityDesignerLayoutSerializer to 
        //associate a designer with activity
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        internal string Name
        {
            get
            {
                return ((Activity != null) ? Activity.Name : null);
            }

            set
            {
            }
        }
        #endregion

        internal virtual bool SupportsLayoutPersistence
        {
            get
            {
                bool supportsLayoutPersistence = false;
                IDesignerHost designerHost = GetService(typeof(IDesignerHost)) as IDesignerHost;
                if (designerHost != null)
                {
                    foreach (IComponent component in designerHost.Container.Components)
                    {
                        Activity activity = component as Activity;
                        if (activity != null && ActivityDesigner.GetDesigner(activity) is FreeformActivityDesigner)
                        {
                            supportsLayoutPersistence = true;
                            break;
                        }
                    }
                }
                return supportsLayoutPersistence;
            }
        }

        internal virtual WorkflowLayout SupportedLayout
        {
            get
            {
                return new ActivityRootLayout(Activity.Site);
            }
        }

        internal SmartTag DesignerSmartTag
        {
            get
            {
                return this.smartTag;
            }
        }

        internal DrawingStates DrawingState
        {
            get
            {
                return this.drawingState;
            }

            set
            {
                this.drawingState = value;
            }
        }

        internal Image StockImage
        {
            get
            {
                if (Activity == null)
                    return null;

                Image designerImage = DesignerTheme.DesignerImage;
                if (designerImage == null)
                    designerImage = ActivityToolboxItem.GetToolboxImage(Activity.GetType());

                return designerImage;
            }
        }

        internal virtual bool SmartTagVisible
        {
            get
            {
                if (ShowSmartTag && this.smartTag.ActiveDesigner != null)
                    return true;

                return this.smartTagVisible;
            }

            set
            {
                if (this.smartTagVisible == value)
                    return;

                this.smartTagVisible = value;
                OnSmartTagVisibilityChanged(this.smartTagVisible);
            }
        }

        private PropertyDescriptor[] EventHandlerProperties
        {
            get
            {
                List<PropertyDescriptor> propertyDescriptors = new List<PropertyDescriptor>();

                //Site can be null when we are dragging item from the toolbox onto the design surface
                if (Activity.Site != null)
                {
                    foreach (PropertyDescriptor propertyDescriptor in PropertyDescriptorFilter.GetPropertiesForEvents(Activity.Site, Activity))
                        propertyDescriptors.Add(propertyDescriptor);
                }

                return propertyDescriptors.ToArray();
            }
        }

        private PropertyDescriptor[] BindableProperties
        {
            get
            {
                List<PropertyDescriptor> propertyDescriptors = new List<PropertyDescriptor>();

                if (!Helpers.IsActivityLocked(Activity))
                {
                    PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(Activity, new Attribute[] { new BrowsableAttribute(true) });
                    if (properties != null)
                    {
                        foreach (PropertyDescriptor propDesc in properties)
                        {
                            if (propDesc.Converter is ActivityBindTypeConverter)
                                propertyDescriptors.Add(propDesc);
                        }
                    }
                }
                return propertyDescriptors.ToArray();
            }
        }

        private string InfoTipTitle
        {
            get
            {
                string title = String.Empty;
                if (Activity.Parent == null)
                {
                    title = Activity.GetType().Name;
                }
                else
                {
                    string activityName = (Activity.Name.Length > MaximumIdentifierLength) ? Activity.Name.Substring(0, MaximumIdentifierLength) + "..." : Activity.Name;
                    title = DR.GetString(DR.InfoTipTitle, Activity.GetType().Name, activityName);
                }
                return title;
            }
        }

        private string InfoTipText
        {
            get
            {
                string tipText = (!String.IsNullOrEmpty(Activity.Description)) ? Activity.Description : ActivityDesigner.GetActivityDescription(Activity.GetType());
                tipText = (tipText.Length > MaximumDescriptionLength) ? tipText.Substring(0, MaximumDescriptionLength) + "..." : tipText;
                if (RulesText.Length > 0)
                    tipText += "\n\n" + RulesText;

                return tipText;
            }
        }

        private string RulesText
        {
            get
            {
                if (this.rulesText == null)
                {
                    this.rulesText = String.Empty;

                    IDictionary<string, string> rules = DesignerHelpers.GetDeclarativeRules(Activity);
                    if (rules.Count > 0)
                    {
                        this.rulesText = DR.GetString(DR.Rules);

                        int maxRulesLength = 3 * (MaximumIdentifierLength + MaximumDescriptionLength);
                        foreach (KeyValuePair<string, string> rule in rules)
                        {
                            this.rulesText += "\n";

                            string ruleName = rule.Key as string;
                            ruleName = (ruleName.Length > MaximumIdentifierLength) ? ruleName.Substring(0, MaximumIdentifierLength) + "..." : ruleName;

                            string ruleDescription = rule.Value as string;
                            ruleDescription = (ruleDescription.Length > MaximumDescriptionLength) ? ruleDescription.Substring(0, MaximumDescriptionLength) + "..." : ruleDescription;
                            if (ruleDescription.Length == 0)
                                ruleDescription = DR.GetString(DR.Empty);

                            this.rulesText += String.Format(CultureInfo.CurrentCulture, "{0}: {1}", ruleName, ruleDescription);
                            if (this.rulesText.Length > maxRulesLength)
                                break;
                        }

                        if (this.rulesText.Length > maxRulesLength)
                            this.rulesText += "\n\n" + DR.GetString(DR.More);
                    }
                }

                return this.rulesText;
            }
        }

        private void OnMoveBranch(object sender, EventArgs e)
        {
            ActivityDesignerVerb moveBranchVerb = sender as ActivityDesignerVerb;
            if (moveBranchVerb != null)
                ParallelActivityDesigner.MoveDesigners(this, (bool)moveBranchVerb.Properties[DesignerUserDataKeys.MoveBranchKey]);
        }

        private void OnStatusMoveBranch(object sender, EventArgs e)
        {
            ActivityDesignerVerb moveBranchVerb = sender as ActivityDesignerVerb;
            if (moveBranchVerb == null)
                return;

            bool enableVerb = false;

            CompositeActivityDesigner parentDesigner = ParentDesigner;
            if (!IsLocked && parentDesigner != null)
            {
                List<Activity> activities = new List<Activity>();

                foreach (Activity containedActivity in ((CompositeActivity)parentDesigner.Activity).Activities)
                {
                    if (!Helpers.IsAlternateFlowActivity(containedActivity))
                        activities.Add(containedActivity);
                }

                //ActivityCollection activities = ((CompositeActivity)parentDesigner.Activity).Activities;
                bool moveleft = (bool)moveBranchVerb.Properties[DesignerUserDataKeys.MoveBranchKey];
                int index = activities.IndexOf(Activity as Activity);
                // "Move Left" should be disabled if the immediate proceeding branch is locked.
                int proceedingLockedIndex = (index > 0) ? index - 1 : -1;
                enableVerb = (index >= 0 && ((moveleft && index > 0 && (index - proceedingLockedIndex) > 0) || (!moveleft && index < activities.Count - 1)));
            }

            moveBranchVerb.Visible = (parentDesigner is ParallelActivityDesigner || parentDesigner is ActivityPreviewDesigner && !Helpers.IsAlternateFlowActivity(Activity));
            moveBranchVerb.Enabled = enableVerb;
        }

        #endregion

        #endregion

        #region Methods

        #region Public Static Methods
        public static ActivityDesigner GetRootDesigner(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
                throw new ArgumentNullException("serviceProvider");

            IDesignerHost designerHost = serviceProvider.GetService(typeof(IDesignerHost)) as IDesignerHost;
            return (designerHost != null) ? GetDesigner(designerHost.RootComponent as Activity) as ActivityDesigner : null;
        }

        /// <summary>
        /// Returns if the Activity is commented or is inside commented activity
        /// </summary>
        /// <param name="designer"></param>
        /// <returns></returns>
        public static bool IsCommentedActivity(Activity activity)
        {
            if (activity == null)
                throw new ArgumentNullException("activity");

            bool isNestedComment = false;
            CompositeActivity parentActivity = activity.Parent;
            while (parentActivity != null && !isNestedComment)
            {
                isNestedComment = (parentActivity != null && !parentActivity.Enabled);
                parentActivity = parentActivity.Parent;
            }

            return isNestedComment;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Returns if a CompositeActivity can be set as parent of activity associated with designer.
        /// This method is called in case of insertion operation such as Drag-Drop or Paste.
        /// </summary>
        /// <param name="parentActivity">CompositeActivity which can be potentially set as parent.</param>
        /// <returns>True if the CompositeActivity can be set as parent of activity associated with designer, false otherwise.</returns>
        public virtual bool CanBeParentedTo(CompositeActivityDesigner parentActivityDesigner)
        {
            if (parentActivityDesigner == null)
                throw new ArgumentNullException("parentActivityDesigner");

            return true;
        }

        /// <summary>
        /// Allows detection of the area hit on the designer.
        /// </summary>
        /// <param name="point">Point to test in logical coordinates.</param>
        /// <returns>Information indicating where the hit happened</returns>
        public virtual HitTestInfo HitTest(Point point)
        {
            HitTestInfo hitInfo = HitTestInfo.Nowhere;
            if (ParentDesigner is FreeformActivityDesigner ||
                (ParentDesigner == null && this is FreeformActivityDesigner))
            {
                //Check if the hit is on connection
                ReadOnlyCollection<ConnectionPoint> connectionPoints = GetConnectionPoints(DesignerEdges.All);
                for (int j = 0; j < connectionPoints.Count; j++)
                {
                    if (connectionPoints[j].Bounds.Contains(point))
                    {
                        hitInfo = new ConnectionPointHitTestInfo(connectionPoints[j]);
                        break;
                    }
                }
            }

            Rectangle bounds = Bounds;
            if (bounds.Contains(point) && hitInfo == HitTestInfo.Nowhere)
            {
                HitTestLocations flags = (bounds.Contains(point)) ? HitTestLocations.Designer : HitTestLocations.None;

                Rectangle hitRectangle = new Rectangle(bounds.Left, bounds.Top, bounds.Left - bounds.Left, bounds.Height);
                flags |= (hitRectangle.Contains(point)) ? HitTestLocations.Left : flags;

                hitRectangle = new Rectangle(bounds.Left, bounds.Top, bounds.Width, bounds.Height - bounds.Height);
                flags |= (hitRectangle.Contains(point)) ? HitTestLocations.Top : flags;

                hitRectangle = new Rectangle(bounds.Right, bounds.Top, bounds.Width - bounds.Width, bounds.Height);
                flags |= (hitRectangle.Contains(point)) ? HitTestLocations.Right : flags;

                hitRectangle = new Rectangle(bounds.Left, bounds.Bottom, bounds.Width, bounds.Bottom - bounds.Bottom);
                flags |= (hitRectangle.Contains(point)) ? HitTestLocations.Bottom : flags;

                hitInfo = new HitTestInfo(this, flags);
            }

            return hitInfo;
        }

        /// <summary>
        /// Brings designer in viewable area
        /// </summary>
        public void EnsureVisible()
        {
            WorkflowView parentView = ParentView;
            if (parentView != null)
                parentView.EnsureVisible(Activity);
        }

        /// <summary>
        /// Get the points used to connect the designer. Points are in logical coordinates.
        /// </summary>
        public virtual ReadOnlyCollection<ConnectionPoint> GetConnectionPoints(DesignerEdges edges)
        {
            List<ConnectionPoint> connectionPoints = new List<ConnectionPoint>();

            if ((edges & DesignerEdges.Left) > 0)
            {
                for (int i = 0; i < GetConnections(DesignerEdges.Left).Count; i++)
                    connectionPoints.Add(new ConnectionPoint(this, DesignerEdges.Left, i));
            }

            if ((edges & DesignerEdges.Right) > 0)
            {
                for (int i = 0; i < GetConnections(DesignerEdges.Right).Count; i++)
                    connectionPoints.Add(new ConnectionPoint(this, DesignerEdges.Right, i));
            }

            if ((edges & DesignerEdges.Top) > 0)
            {
                for (int i = 0; i < GetConnections(DesignerEdges.Top).Count; i++)
                    connectionPoints.Add(new ConnectionPoint(this, DesignerEdges.Top, i));
            }

            if ((edges & DesignerEdges.Bottom) > 0)
            {
                for (int i = 0; i < GetConnections(DesignerEdges.Bottom).Count; i++)
                    connectionPoints.Add(new ConnectionPoint(this, DesignerEdges.Bottom, i));
            }

            return connectionPoints.AsReadOnly();
        }

        /// <summary>
        /// Invalidates the entire workflow designer
        /// </summary>
        public void Invalidate()
        {
            WorkflowView parentView = ParentView;
            if (parentView != null)
            {
                parentView.InvalidateLogicalRectangle(Bounds);

                GlyphManager glyphManager = GetService(typeof(IDesignerGlyphProviderService)) as GlyphManager;
                if (glyphManager != null)
                {
                    foreach (DesignerGlyph glyph in glyphManager.GetDesignerGlyphs(this))
                        parentView.InvalidateLogicalRectangle(glyph.GetBounds(this, false));
                }
            }
        }

        /// <summary>
        /// Invalidates specified rectangle on the designer
        /// </summary>
        /// <param name="rectangle">Rectangle to invalidate</param>
        public void Invalidate(Rectangle rectangle)
        {
            WorkflowView parentView = ParentView;
            if (parentView != null)
            {
                rectangle = Rectangle.Intersect(Bounds, rectangle);
                parentView.InvalidateLogicalRectangle(rectangle);
            }
        }

        /// <summary>
        /// Get image of the activity designer on specified graphics
        /// </summary>
        /// <param name="compatibleGraphics">Graphics objects to draw image</param>
        /// <returns>Image which is drawn</returns>
        public Image GetPreviewImage(Graphics compatibleGraphics)
        {
            if (compatibleGraphics == null)
                throw new ArgumentNullException("compatibleGraphics");

            //Update the layout
            if (Activity.Site == null)
            {
                ((IWorkflowDesignerMessageSink)this).OnLayoutSize(compatibleGraphics);
                ((IWorkflowDesignerMessageSink)this).OnLayoutPosition(compatibleGraphics);
            }

            AmbientTheme ambientTheme = WorkflowTheme.CurrentTheme.AmbientTheme;
            Bitmap designerImage = new Bitmap(Size.Width + (4 * ambientTheme.Margin.Width), Size.Height + (4 * ambientTheme.Margin.Height), PixelFormat.Format32bppArgb);

            GlyphManager glyphManager = (Activity != null && Activity.Site != null) ? GetService(typeof(IDesignerGlyphProviderService)) as GlyphManager : null;

            using (Graphics graphics = Graphics.FromImage(designerImage))
            using (Brush transparentBrush = new SolidBrush(Color.FromArgb(0, 255, 0, 255)))
            {
                graphics.FillRectangle(transparentBrush, 0, 0, designerImage.Width, designerImage.Height);
                graphics.TranslateTransform(-Location.X + 2 * ambientTheme.Margin.Width, -Location.Y + 2 * ambientTheme.Margin.Height);

                //We need to go thru nested designers to generate the preview
                Rectangle bounds = Bounds;
                Rectangle viewPort = new Rectangle(bounds.Location, new Size(bounds.Width + 1, bounds.Height + 1));
                Queue<ActivityDesigner> designers = new Queue<ActivityDesigner>();
                designers.Enqueue(this);
                while (designers.Count > 0)
                {
                    ActivityDesigner designer = designers.Dequeue();
                    designer.OnPaint(new ActivityDesignerPaintEventArgs(graphics, designer.Bounds, viewPort, designer.DesignerTheme));

                    ActivityDesignerGlyphCollection glyphs = (glyphManager != null) ? glyphManager.GetDesignerGlyphs(designer) : Glyphs;
                    foreach (DesignerGlyph glyph in glyphs)
                    {
                        //
                        if (!(glyph is SelectionGlyph))
                            glyph.Draw(graphics, designer);
                    }

                    CompositeActivityDesigner compositeDesigner = designer as CompositeActivityDesigner;
                    if (compositeDesigner != null)
                    {
                        foreach (ActivityDesigner containedDesigner in compositeDesigner.ContainedDesigners)
                        {
                            if (containedDesigner != null && compositeDesigner.Expanded && compositeDesigner.IsContainedDesignerVisible(containedDesigner))
                                designers.Enqueue(containedDesigner);
                        }
                    }
                }
            }

            return designerImage;
        }
        #endregion

        #region Protected Methods
        /// <summary>
        /// Initializes the designer with associated activity.
        /// </summary>
        /// <param name="activity">Activity with which the designer needs to be initialized.</param>
        protected virtual void Initialize(Activity activity)
        {
            if (activity == null)
                throw new ArgumentNullException("activity");

            if (IsRootDesigner)
            {
                // Listen for the completed load.  When finished, we need to select the form.  We don'
                // want to do it before we're done, however, or else the dimensions of the selection rectangle
                // could be off because during load, change events are not fired.
                IDesignerHost designerHost = (IDesignerHost)GetService(typeof(IDesignerHost));
                if (designerHost != null && InvokingDesigner == null)
                    designerHost.LoadComplete += new EventHandler(OnLoadComplete);

                IComponentChangeService changeService = GetService(typeof(IComponentChangeService)) as IComponentChangeService;
                if (changeService != null)
                {
                    changeService.ComponentChanged += new ComponentChangedEventHandler(OnComponentChanged);
                    changeService.ComponentRename += new ComponentRenameEventHandler(OnComponentRenamed);
                }
            }

            this.Text = (!String.IsNullOrEmpty(activity.Name)) ? activity.Name : activity.GetType().Name;
            this.Image = StockImage;

            //We need to update the Verbs when the designer first loads up
            RefreshDesignerVerbs();

            if (IsLocked)
                DesignerHelpers.MakePropertiesReadOnly(activity.Site, activity);
        }

        /// <summary>
        /// Disposes the resources held by the designer.
        /// </summary>
        /// <param name="disposing">True if the designer is being disposed, false if the designer is being finalized.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (IsRootDesigner)
                {
                    if (this.workflowView != null)
                    {
                        this.workflowView.Dispose();
                        this.workflowView = null;
                    }

                    IDesignerHost designerHost = GetService(typeof(IDesignerHost)) as IDesignerHost;
                    if (designerHost != null && InvokingDesigner == null && Activity == designerHost.RootComponent)
                        designerHost.LoadComplete -= new EventHandler(OnLoadComplete);

                    IComponentChangeService changeService = GetService(typeof(IComponentChangeService)) as IComponentChangeService;
                    if (changeService != null)
                    {
                        changeService.ComponentChanged -= new ComponentChangedEventHandler(OnComponentChanged);
                        changeService.ComponentRename -= new ComponentRenameEventHandler(OnComponentRenamed);
                    }
                }
            }
        }

        /// <summary>
        /// Get the points used to connect the designer. Points are in logical coordinates.
        /// </summary>
        protected internal virtual ReadOnlyCollection<Point> GetConnections(DesignerEdges edges)
        {
            Rectangle bounds = Bounds;

            List<Point> connections = new List<Point>();
            if ((edges & DesignerEdges.Left) > 0)
                connections.Add(new Point(bounds.Left, bounds.Top + bounds.Height / 2));

            if ((edges & DesignerEdges.Top) > 0)
                connections.Add(new Point(bounds.Left + bounds.Width / 2, bounds.Top));

            if ((edges & DesignerEdges.Right) > 0)
                connections.Add(new Point(bounds.Right, bounds.Top + bounds.Height / 2));

            if ((edges & DesignerEdges.Bottom) > 0)
                connections.Add(new Point(bounds.Left + bounds.Width / 2, bounds.Bottom));

            return connections.AsReadOnly();
        }

        /// <summary>
        /// Perform default UI action associated with the designer. 
        /// Example: Emit method associated with default event in code beside file on double click.
        /// </summary>
        protected virtual void DoDefaultAction()
        {
            if (IsLocked)
                return;

            DefaultEventAttribute defaultEventAttribute = TypeDescriptor.GetAttributes(Activity)[typeof(DefaultEventAttribute)] as DefaultEventAttribute;
            if (defaultEventAttribute == null || defaultEventAttribute.Name == null || defaultEventAttribute.Name.Length == 0)
                return;

            ActivityBindPropertyDescriptor defaultPropEvent = TypeDescriptor.GetProperties(Activity)[defaultEventAttribute.Name] as ActivityBindPropertyDescriptor;
            if (defaultPropEvent != null)
            {
                object value = defaultPropEvent.GetValue(Activity);
                if (!(value is ActivityBind))
                {
                    IEventBindingService eventBindingService = (IEventBindingService)GetService(typeof(IEventBindingService));
                    if (eventBindingService != null)
                    {
                        EventDescriptor eventDesc = eventBindingService.GetEvent(defaultPropEvent.RealPropertyDescriptor);
                        if (eventDesc != null)
                        {
                            string handler = defaultPropEvent.RealPropertyDescriptor.GetValue(Activity) as string;
                            if (string.IsNullOrEmpty(handler))
                                handler = DesignerHelpers.CreateUniqueMethodName(Activity, eventDesc.Name, eventDesc.EventType);

                            defaultPropEvent.SetValue(Activity, handler);
                            eventBindingService.ShowCode(Activity, eventDesc);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// When overridden in a derived class, allows a designer to add items to the set of attributes that it exposes through a TypeDescriptor.
        /// </summary>
        /// <param name="attributes">The Attribute objects for the class of the activity. The keys in the dictionary of attributes are the TypeID values of the attributes.</param>
        protected virtual void PreFilterAttributes(IDictionary attributes)
        {
            if (attributes == null)
                throw new ArgumentNullException("attributes");
        }

        /// <summary>
        /// When overridden in a derived class, allows a designer to add items to the set of properties that it exposes through a TypeDescriptor.
        /// </summary>
        /// <param name="properties">The PropertyDescriptor objects that represent the properties of the class of the activity. The keys in the dictionary of properties are property names.</param>
        protected virtual void PreFilterProperties(IDictionary properties)
        {
            if (properties == null)
                throw new ArgumentNullException("properties");
        }

        /// <summary>
        /// When overridden in a derived class, allows a designer to add items to the set of events that it exposes through a TypeDescriptor.
        /// </summary>
        /// <param name="events">The EventDescriptor objects that represent the events of the class of the activity. The keys in the dictionary of events are event names.</param>
        protected virtual void PreFilterEvents(IDictionary events)
        {
            if (events == null)
                throw new ArgumentNullException("events");
        }

        /// <summary>
        /// When overridden in a derived class, allows a designer to change or remove items from the set of attributes that it exposes through a TypeDescriptor.
        /// </summary>
        /// <param name="attributes">The Attribute objects for the class of the activity. The keys in the dictionary of attributes are the TypeID values of the attributes.</param>
        protected virtual void PostFilterAttributes(IDictionary attributes)
        {
            if (attributes == null)
                throw new ArgumentNullException("attributes");
        }

        /// <summary>
        /// When overridden in a derived class, allows a designer to change or remove items from the set of properties that it exposes through a TypeDescriptor.
        /// </summary>
        /// <param name="properties">The PropertyDescriptor objects that represent the properties of the class of the activity. The keys in the dictionary of properties are property names.</param>
        protected virtual void PostFilterProperties(IDictionary properties)
        {
            if (properties == null)
                throw new ArgumentNullException("properties");

            // NOTE: I have to do this work around, reason being IExtenderProvider.CanExtend is not used
            // to determine whether the properties should be extended or not. So I am fixing it 
            // on designer part 
            StringCollection removedProperties = new StringCollection();
            foreach (DictionaryEntry entry in properties)
            {
                PropertyDescriptor prop = entry.Value as PropertyDescriptor;
                ExtenderProvidedPropertyAttribute eppa = prop.Attributes[typeof(ExtenderProvidedPropertyAttribute)] as ExtenderProvidedPropertyAttribute;
                if (eppa != null && eppa.Provider != null && !eppa.Provider.CanExtend(Activity))
                    removedProperties.Add(entry.Key as string);
            }

            foreach (string removedPropoerty in removedProperties)
                properties.Remove(removedPropoerty);

            PropertyDescriptorFilter.FilterProperties(Activity.Site, Activity, properties);
        }

        /// <summary>
        /// When overridden in a derived class, allows a designer to change or remove items from the set of events that it exposes through a TypeDescriptor.
        /// </summary>
        /// <param name="events">The EventDescriptor objects that represent the events of the class of the activity. The keys in the dictionary of events are event names.</param>
        protected virtual void PostFilterEvents(IDictionary events)
        {
            if (events == null)
                throw new ArgumentNullException("events");
        }

        /// <summary>
        /// Attempts to retrieve the specified type of service from the designer's activity's design mode site.
        /// </summary>
        /// <param name="serviceType">The type of service to request.</param>
        /// <returns>An object implementing the requested service, or a null reference (Nothing in Visual Basic) if the service cannot be resolved.</returns>
        protected object GetService(Type serviceType)
        {
            if (serviceType == null)
                throw new ArgumentNullException("serviceType");

            if (this.activity != null && this.activity.Site != null)
                return this.activity.Site.GetService(serviceType);
            else
                return null;
        }

        /// <summary>
        /// Called when the users begins to drag mouse on the designer.
        /// </summary>
        /// <param name="initialDragPoint">Point where the drag started in logical coordinates</param>
        /// <param name="e">MouseEventArgs containing event data.</param>
        protected virtual void OnMouseDragBegin(Point initialDragPoint, MouseEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException("e");
        }

        /// <summary>
        /// Called when the users drags mouse over the designer.
        /// </summary>
        /// <param name="e">MouseEventArgs containing event data.</param>
        protected virtual void OnMouseDragMove(MouseEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException("e");
        }

        /// <summary>
        /// Called when the mouse drag ends.
        /// </summary>
        protected virtual void OnMouseDragEnd()
        {
        }

        /// <summary>
        /// Called when the mouse cursor enters the designer bounds.
        /// </summary>
        /// <param name="e">MouseEventArgs containing event data.</param>
        protected virtual void OnMouseEnter(MouseEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException("e");

            if (IsVisible)
            {
                if (ShowSmartTag)
                    SmartTagVisible = true;

                ShowInfoTip(InfoTipTitle, InfoTipText);
            }
        }

        /// <summary>
        /// Called when the mouse button is clicked when mouse cursor is in designer bounds.
        /// </summary>
        /// <param name="e">MouseEventArgs containing event data.</param>
        protected virtual void OnMouseDown(MouseEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException("e");
        }

        /// <summary>
        /// Called when the mouse cursor is moving in designer bounds.
        /// </summary>
        /// <param name="e">MouseEventArgs containing event data.</param>
        protected virtual void OnMouseMove(MouseEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException("e");

            if (IsVisible)
                ShowInfoTip(InfoTipTitle, InfoTipText);
        }


        /// <summary>
        /// Called when the mouse cursor is in designer bounds.
        /// </summary>
        /// <param name="e">MouseEventArgs containing event data.</param>
        protected virtual void OnMouseHover(MouseEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException("e");

            if (IsVisible)
                ShowInfoTip(InfoTipTitle, InfoTipText);
        }

        /// <summary>
        /// Called when the moused button is released when mouse cursor is in designer bounds.
        /// </summary>
        /// <param name="e">MouseEventArgs containing event data.</param>
        protected virtual void OnMouseUp(MouseEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException("e");
        }

        /// <summary>
        /// Called when the mouse button is clicked multiple times on the designer
        /// </summary>
        /// <param name="e">MouseEventArgs containing event data.</param>
        protected virtual void OnMouseDoubleClick(MouseEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException("e");
        }

        /// <summary>
        /// Called when the mouse cursor leaves designer bounds.
        /// </summary>
        protected virtual void OnMouseLeave()
        {
            if (ShowSmartTag)
                SmartTagVisible = false;

            ShowInfoTip(String.Empty);
        }

        /// <summary>
        /// Called when the mouse capture changes
        /// </summary>
        protected virtual void OnMouseCaptureChanged()
        {
        }

        /// <summary>
        /// Called when the drag drop operation is in progress and mouse cursor enters the designer bounds.
        /// </summary>
        /// <param name="e">Drag drop event arguments.</param>
        protected virtual void OnDragEnter(ActivityDragEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException("e");
        }

        /// <summary>
        /// Called when the drag drop operation is in progress and mouse cursor is inside the designer bounds.
        /// </summary>
        /// <param name="e">Drag drop event arguments.</param>
        protected virtual void OnDragOver(ActivityDragEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException("e");
        }

        /// <summary>
        /// Called when the drag drop operation is in progress and mouse cursor leaves the designer bounds.
        /// </summary>
        protected virtual void OnDragLeave()
        {
        }

        /// <summary>
        /// Called when the drag drop operation is completed inside designer bounds.
        /// </summary>
        /// <param name="e">Drag drop event arguments.</param>
        protected virtual void OnDragDrop(ActivityDragEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException("e");
        }

        /// <summary>
        /// Updates the visual cues for feedback given to the user when performing drag drop operation.
        /// </summary>
        /// <param name="e">A GiveFeedbackEventArgs that contains the event data.</param>
        protected virtual void OnGiveFeedback(GiveFeedbackEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException("e");
        }

        /// <summary>
        /// Controls if the drag drop operation should continue.
        /// </summary>
        /// <param name="e">A QueryContinueDragEventArgs that contains the event data.</param>
        protected virtual void OnQueryContinueDrag(QueryContinueDragEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException("e");
        }

        /// <summary>
        /// Called when key is pressed when designer has keyboard focus.
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnKeyDown(KeyEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException("e");
        }

        /// <summary>
        /// Called when key is released when designer has keyboard focus.
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnKeyUp(KeyEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException("e");
        }

        /// <summary>
        /// Called when scroll position is changed
        /// </summary>
        /// <param name="sender">Scrollbar sending the message</param>
        /// <param name="value">New value of the scrolled position</param>
        protected virtual void OnScroll(ScrollBar sender, int value)
        {
        }

        /// <summary>
        /// Allows designer to process raw win32 message
        /// </summary>
        /// <param name="message">Message structure containing details of the message to be processed</param>
        protected virtual void OnProcessMessage(Message message)
        {
        }

        /// <summary>
        /// Called to refresh the configuration errors associated with designers.
        /// </summary>
        protected internal virtual void RefreshDesignerActions()
        {
            this.designerActions = null;
        }

        /// <summary>
        /// Called when user clicks on configuration errors associated with the designer.
        /// </summary>
        /// <param name="designerAction">Designer action associated with configuration error.</param>
        protected internal virtual void OnExecuteDesignerAction(DesignerAction designerAction)
        {
            if (designerAction == null)
                throw new ArgumentNullException("designerAction");

            ISelectionService selectionService = GetService(typeof(ISelectionService)) as ISelectionService;
            if (selectionService != null)
                selectionService.SetSelectedComponents(new object[] { Activity }, SelectionTypes.Replace);

            string propName = designerAction.PropertyName as string;
            if (propName != null && propName.Length > 0)
            {
                IExtendedUIService uiService = GetService(typeof(IExtendedUIService)) as IExtendedUIService;
                if (uiService != null)
                    uiService.NavigateToProperty(propName);
            }
        }

        /// <summary>
        /// Called to notify the designer if the SmartTag is being shown or hidden
        /// </summary>
        /// <param name="visible">Indicates if the SmartTag is being shown or hidden</param>
        protected virtual void OnSmartTagVisibilityChanged(bool visible)
        {
            Rectangle rectangle = smartTag.GetBounds(this, true);
            Rectangle textRectangle = TextRectangle;
            if (!textRectangle.Size.IsEmpty)
                rectangle = Rectangle.Union(textRectangle, rectangle);
            Invalidate(rectangle);
        }

        /// <summary>
        /// Shows the designer verbs associated with smarttag at specific point
        /// </summary>
        /// <param name="smartTagPoint">Point at which to show the actions</param>
        protected virtual void OnShowSmartTagVerbs(Point smartTagPoint)
        {
            ActivityDesignerVerb[] verbs = null;
            SmartTagVerbs.CopyTo(verbs, 0);
            DesignerHelpers.ShowDesignerVerbs(this, PointToScreen(smartTagPoint), verbs);
        }

        /// <summary>
        /// Notifies the designer that the associated theme has changed.
        /// </summary>
        protected virtual void OnThemeChange(ActivityDesignerTheme newTheme)
        {
            if (newTheme == null)
                throw new ArgumentNullException("newTheme");

            this.Image = StockImage;
        }

        /// <summary>
        /// Stores the UI state of the designer in binary stream. 
        /// </summary>
        /// <param name="writer">BinaryWriter used to store the state.</param>
        protected virtual void SaveViewState(BinaryWriter writer)
        {
            if (writer == null)
                throw new ArgumentNullException("writer");
        }

        /// <summary>
        /// Restores the UI state of the designer from binary stream. 
        /// </summary>
        /// <param name="reader">BinaryReader used to restore the designer state.</param>
        /// <returns></returns>
        protected virtual void LoadViewState(BinaryReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");
        }

        /// <summary>
        /// Return if an activity type is valid in context of a root designer. This function is called only if 
        /// the designer is a root designer
        /// </summary>
        /// <param name="activityType">Type of the activity being queried</param>
        /// <returns></returns>
        protected virtual bool IsSupportedActivityType(Type activityType)
        {
            return true;
        }

        protected virtual WorkflowView CreateView(ViewTechnology viewTechnology)
        {
            WorkflowView workflowView = new WorkflowView(Activity.Site as IServiceProvider);
            workflowView.ShowToolContainer = true;
            return workflowView;
        }

        /// <summary>
        /// Refreshes the ActivityDesignerVerbs associted with the designer by calling status handler.
        /// </summary>
        protected void RefreshDesignerVerbs()
        {
            if (Activity != null && Activity.Site != null)
            {
                DesignerVerbCollection verbs = ((IDesigner)this).Verbs;
                if (verbs != null)
                {
                    foreach (DesignerVerb verb in verbs)
                    {
                        //This will cause us to send the status update for ActivityDesignerVerb
                        int status = verb.OleStatus;
                        status = 0;
                    }
                }
            }
        }

        /// <summary>
        /// Draws the visual representation of activity at design time.
        /// </summary>
        /// <param name="e">ActivityDesignerPaintEventArgs holding drawing arguments</param>
        protected virtual void OnPaint(ActivityDesignerPaintEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException("e");

            ActivityDesignerPaint.DrawDesignerBackground(e.Graphics, this);

            if (!String.IsNullOrEmpty(Text) && !TextRectangle.Size.IsEmpty)
            {
                Font font = (SmartTagVisible) ? e.DesignerTheme.BoldFont : e.DesignerTheme.Font;
                ActivityDesignerPaint.DrawText(e.Graphics, font, Text, TextRectangle, StringAlignment.Near, e.AmbientTheme.TextQuality, e.DesignerTheme.ForegroundBrush);
            }

            if (Image != null && !ImageRectangle.Size.IsEmpty)
                ActivityDesignerPaint.DrawImage(e.Graphics, Image, ImageRectangle, DesignerContentAlignment.Fill);
        }

        /// <summary>
        /// Called to layout the position of contained visual cues or designers.
        /// </summary>
        /// <param name="e">ActivityDesignerLayoutEventArgs holding layout arguments</param>
        protected virtual void OnLayoutPosition(ActivityDesignerLayoutEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException("e");
        }

        /// <summary>
        /// Called to set the size of the visual cues or designers contained within the designer.
        /// </summary>
        /// <param name="e">ActivityDesignerLayoutEventArgs holding layout arguments</param>
        protected virtual Size OnLayoutSize(ActivityDesignerLayoutEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException("e");

            //GetVisible is an expensive call so we make sure that we keep it buffered in OnLayoutSize
            //otherwise drawing slows down. The visiblity change always triggers layouting and hence we
            //can safely buffer this variable
            this.isVisible = GetVisible();

            if (!String.IsNullOrEmpty(Text))
            {
                //Calculate the size of the text, we allow 10 characters per line and maximum of 2 lines
                Size actualTextSize = ActivityDesignerPaint.MeasureString(e.Graphics, e.DesignerTheme.BoldFont, Text, StringAlignment.Center, Size.Empty);
                Size requestedLineSize = actualTextSize;
                requestedLineSize.Width /= Text.Length;
                requestedLineSize.Width += ((requestedLineSize.Width % Text.Length) > 0) ? 1 : 0;
                requestedLineSize.Width *= Math.Min(Text.Length, ActivityDesigner.MaximumCharsPerLine - 1);

                this.textSize.Width = MinimumSize.Width - 2 * e.AmbientTheme.Margin.Width;
                if (Image != null)
                    this.textSize.Width -= e.DesignerTheme.ImageSize.Width + e.AmbientTheme.Margin.Width;
                this.textSize.Width = Math.Min(this.textSize.Width, actualTextSize.Width);
                this.textSize.Width = Math.Max(this.textSize.Width, requestedLineSize.Width);

                //We calculate the text size in onlayoutsize as we get access to the graphics and font information in this function
                this.textSize.Height = requestedLineSize.Height;
                int textLines = actualTextSize.Width / this.textSize.Width;
                textLines += ((actualTextSize.Width % this.textSize.Width) > 0) ? 1 : 0;
                textLines = Math.Min(textLines, ActivityDesigner.MaximumTextLines);
                this.textSize.Height *= textLines;
            }
            else
            {
                this.textSize = Size.Empty;
            }

            Size size = Size.Empty;
            size.Width = 2 * e.AmbientTheme.Margin.Width + ((Image != null) ? (e.DesignerTheme.ImageSize.Width + e.AmbientTheme.Margin.Width) : 0) + this.textSize.Width;
            size.Height = e.AmbientTheme.Margin.Height + Math.Max(e.DesignerTheme.ImageSize.Height, this.textSize.Height) + e.AmbientTheme.Margin.Height;
            return size;
        }

        /// <summary>
        /// Called when the user starts to visually resize the designer when designer is inside freeform designer
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnBeginResizing(ActivityDesignerResizeEventArgs e)
        {
        }

        /// <summary>
        /// Called when the user is visually resizing the designer when designer is inside freeform designer
        /// </summary>
        /// <param name="e">ActivityDesignerResizeEventArgs specifying the edge being used to resize and new bounds</param>
        protected virtual void OnResizing(ActivityDesignerResizeEventArgs e)
        {
            FreeformActivityDesigner.SetDesignerBounds(this, e.Bounds);
        }

        /// <summary>
        /// Called when user is done resizing the designer
        /// </summary>
        protected virtual void OnEndResizing()
        {
            PerformLayout();
        }

        /// <summary>
        /// Called to check if connection can be established between source and target designer
        /// </summary>
        /// <param name="source">Source connection point</param>
        /// <param name="target">Target connection point</param>
        /// <returns>True if connection can be established, false otherwise</returns>
        protected virtual bool CanConnect(ConnectionPoint source, ConnectionPoint target)
        {
            return true;
        }

        /// <summary>
        /// Called when connection is established between two connection points
        /// </summary>
        /// <param name="source">Source connection point</param>
        /// <param name="target">Target connection point</param>
        protected virtual void OnConnected(ConnectionPoint source, ConnectionPoint target)
        {
        }

        /// <summary>
        /// Called when the activity associated with the designer is changed.
        /// </summary>
        /// <param name="e">A ActivityChangedEventArgs containing information about what changed.</param>
        protected virtual void OnActivityChanged(ActivityChangedEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException("e");

            if (e.Member != null && e.Member.Name != null && e.Member.Name.Equals("Name"))
                this.Text = Activity.Name;

            //Whenever property on the component changes we update the verb status
            //We have to do a type descriptor refresh here as we need to not only update the designer verbs
            //but also the global commands
            //For contained activity changed we refresh thru the activity list changed
            if (!(e.OldValue is ActivityCollectionChangeEventArgs))
                RefreshDesignerVerbs();

            IUIService uiservice = GetService(typeof(IUIService)) as IUIService;
            if (uiservice != null)
                uiservice.SetUIDirty();

            //Clear and refresh the rules text
            this.rulesText = null;
        }

        /// <summary>
        /// Shows specified tooltip 
        /// </summary>
        /// <param name="title">String specifying the tooltip title</param>
        /// <param name="infoTip">String specifying the tooltip to display</param>
        protected void ShowInfoTip(string title, string infoTip)
        {
            WorkflowView parentView = ParentView;
            if (parentView != null)
                parentView.ShowInfoTip(title, infoTip);
        }

        /// <summary>
        /// Shows specified tooltip 
        /// </summary>
        /// <param name="infoTip">String specifying the tooltip to display</param>
        protected void ShowInfoTip(string infoTip)
        {
            WorkflowView parentView = ParentView;
            if (parentView != null)
                parentView.ShowInfoTip(infoTip);
        }

        /// <summary>
        /// Shows tooltip at specified location
        /// </summary>
        /// <param name="infoTip">String specifying the tooltip to display</param>
        /// <param name="rectangle">Rectangle where to display tooltip</param>
        protected void ShowInPlaceTip(string infoTip, Rectangle rectangle)
        {
            WorkflowView parentView = ParentView;
            if (parentView != null)
                parentView.ShowInPlaceToolTip(infoTip, parentView.LogicalRectangleToClient(rectangle));
        }

        /// <summary>
        /// Updates the layout of the designer
        /// </summary>
        protected void PerformLayout()
        {
            WorkflowView parentView = ParentView;
            if (parentView != null)
                parentView.PerformLayout(false);
        }

        /// <summary>
        /// Transforms point from activity designer coordinate system to screen
        /// </summary>
        /// <param name="point">Point in activity designer coordinate system</param>
        /// <returns>Point in screen coordinate system</returns>
        protected Point PointToScreen(Point point)
        {
            WorkflowView parentView = ParentView;
            if (parentView != null)
                return parentView.LogicalPointToScreen(point);
            else
                return point;
        }

        /// <summary>
        /// Transforms point from screen coordinate system to activity designer coordinate system
        /// </summary>
        /// <param name="point">Point in screen coordinate system</param>
        /// <returns>Point in activity designer coordinate system</returns>
        protected Point PointToLogical(Point point)
        {
            WorkflowView parentView = ParentView;
            if (parentView != null)
                return parentView.ScreenPointToLogical(point);
            else
                return point;
        }

        /// <summary>
        /// Transforms rectangle from activity designer coordinate system to screen
        /// </summary>
        /// <param name="rectangle">Rectangle in activity designer coordinate system</param>
        /// <returns>Rectangle in screen coordinate system</returns>
        protected Rectangle RectangleToScreen(Rectangle rectangle)
        {
            WorkflowView parentView = ParentView;
            if (parentView != null)
                return new Rectangle(PointToScreen(rectangle.Location), parentView.LogicalSizeToClient(rectangle.Size));
            else
                return rectangle;

        }

        //for the accessible object
        internal Rectangle InternalRectangleToScreen(Rectangle rectangle)
        {
            return RectangleToScreen(rectangle);
        }

        /// <summary>
        /// Transforms rectangle from screen coordinate system to activity designer coordinate system
        /// </summary>
        /// <param name="rectangle">Rectangle in screen coordinate system</param>
        /// <returns>Rectangle in activity designer coordinate system</returns>
        protected Rectangle RectangleToLogical(Rectangle rectangle)
        {
            WorkflowView parentView = ParentView;
            if (parentView != null)
                return new Rectangle(PointToLogical(rectangle.Location), parentView.ClientSizeToLogical(rectangle.Size));
            else
                return rectangle;
        }
        #endregion

        #region Private static Methods
        internal static ActivityDesigner GetSafeRootDesigner(IServiceProvider serviceProvider)
        {
            return (serviceProvider != null) ? ActivityDesigner.GetRootDesigner(serviceProvider) : null;
        }

        internal static ActivityDesigner GetDesigner(Activity activity)
        {
            ActivityDesigner designer = null;

            if (activity != null && activity.Site != null)
            {
                IDesignerHost designerHost = activity.Site.GetService(typeof(IDesignerHost)) as IDesignerHost;
                if (designerHost != null)
                    designer = designerHost.GetDesigner(activity) as ActivityDesigner;
            }

            return designer;
        }

        internal static string GetActivityDescription(Type activityType)
        {
            if (activityType == null)
                return null;

            object[] attribs = activityType.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (attribs != null && attribs.GetLength(0) == 0)
                attribs = activityType.GetCustomAttributes(typeof(DescriptionAttribute), true);

            DescriptionAttribute descriptionAttribute = (attribs != null && attribs.GetLength(0) > 0) ? attribs[0] as DescriptionAttribute : null;
            return (descriptionAttribute != null) ? descriptionAttribute.Description : String.Empty;
        }

        internal static CompositeActivityDesigner GetParentDesigner(object obj)
        {
            // get parent designer
            CompositeActivityDesigner parentDesigner = null;
            if (obj is HitTestInfo)
            {
                parentDesigner = ((HitTestInfo)obj).AssociatedDesigner as CompositeActivityDesigner;
            }
            else if (obj is Activity)
            {
                ActivityDesigner activityDesigner = ActivityDesigner.GetDesigner(obj as Activity);
                if (activityDesigner != null)
                    parentDesigner = activityDesigner.ParentDesigner;
            }

            return parentDesigner;
        }

        internal static ActivityDesigner CreateTransientDesigner(Activity activity)
        {
            ActivityDesigner activityDesigner = new ActivityDesigner();
            ActivityDesignerTheme designerTheme = activityDesigner.DesignerTheme;

            using (Bitmap temporaryBitmap = new Bitmap(designerTheme.Size.Width, designerTheme.Size.Height, PixelFormat.Format32bppArgb))
            using (Graphics graphics = Graphics.FromImage(temporaryBitmap))
            {
                activityDesigner.Image = ActivityToolboxItem.GetToolboxImage(activity.GetType());
                activityDesigner.Location = new Point(-1, -1);
                activityDesigner.Location = Point.Empty;
                activityDesigner.Size = activityDesigner.OnLayoutSize(new ActivityDesignerLayoutEventArgs(graphics, activityDesigner.DesignerTheme));
            }

            return activityDesigner;
        }

        internal static Type GetDesignerType(IServiceProvider serviceProvider, Type activityType, Type designerBaseType)
        {
            Type designerType = null;
            AttributeCollection attribs = TypeDescriptor.GetAttributes(activityType);
            foreach (Attribute attribute in attribs)
            {
                DesignerAttribute designerAttribute = attribute as DesignerAttribute;
                if (designerAttribute != null && (designerBaseType == null || designerAttribute.DesignerBaseTypeName == designerBaseType.AssemblyQualifiedName))
                {
                    int index = designerAttribute.DesignerTypeName.IndexOf(',');
                    string designerTypeName = (index >= 0) ? designerAttribute.DesignerTypeName.Substring(0, index) : designerAttribute.DesignerTypeName;
                    designerType = activityType.Assembly.GetType(designerTypeName);
                    if (designerType == null && serviceProvider != null)
                    {
                        ITypeResolutionService typeResolutionService = serviceProvider.GetService(typeof(ITypeResolutionService)) as ITypeResolutionService;
                        designerType = (typeResolutionService != null) ? typeResolutionService.GetType(designerAttribute.DesignerTypeName) : null;
                    }

                    if (designerType == null)
                        designerType = Type.GetType(designerAttribute.DesignerTypeName);

                    break;
                }
            }

            return designerType;
        }

        internal static ActivityDesigner CreateDesigner(IServiceProvider serviceProvider, Activity activity)
        {
            IDesigner designer = null;
            Type designerType = GetDesignerType(serviceProvider, activity.GetType(), typeof(IDesigner));
            if (designerType == null)
                designerType = GetDesignerType(serviceProvider, activity.GetType(), null);

            if (designerType != null)
            {
                try
                {
                    designer = Activator.CreateInstance(designerType, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.CreateInstance, null, null, null) as IDesigner;
                    designer.Initialize(activity);
                }
                catch
                {
                    //Eat the exception thrown
                }
            }
            return (designer as ActivityDesigner);
        }
        #endregion

        #region Private Methods
        private bool GetVisible()
        {
            Activity activity = Activity;
            if (activity == null)
                return false;

            while (activity != null)
            {
                ActivityDesigner containedDesigner = ActivityDesigner.GetDesigner(activity);
                if (containedDesigner != null)
                {
                    CompositeActivityDesigner parentDesigner = containedDesigner.ParentDesigner;
                    if (parentDesigner == null && containedDesigner is IRootDesigner)
                        return true;

                    if (parentDesigner == null || !parentDesigner.Expanded || !parentDesigner.IsContainedDesignerVisible(containedDesigner))
                        return false;

                    activity = parentDesigner.Activity;
                }
                else
                {
                    activity = null;
                }
            }
            return true;
        }

        private void OnGenerateEventHandler(object sender, EventArgs e)
        {
            DesignerVerb eventVerb = sender as DesignerVerb;
            if (eventVerb == null)
                return;

            Activity contextActivity = Helpers.GetRootActivity(Activity);
            if (contextActivity == null)
                return;

            PropertyDescriptor methodDescriptor = null;

            PropertyDescriptor[] propertyDescriptors = EventHandlerProperties;
            foreach (PropertyDescriptor propertyDescriptor in propertyDescriptors)
            {
                Type rtType = PropertyDescriptorUtils.GetBaseType(propertyDescriptor, Activity, Activity.Site);
                if (rtType != null)
                {
                    object handler = propertyDescriptor.GetValue(Activity);
                    if (!(handler is String) || String.IsNullOrEmpty((String)handler))
                        handler = DesignerHelpers.CreateUniqueMethodName(Activity, propertyDescriptor.Name, rtType);
                    propertyDescriptor.SetValue(Activity, handler);
                    methodDescriptor = propertyDescriptor;
                }
            }

            IEventBindingService eventBindingService = GetService(typeof(IEventBindingService)) as IEventBindingService;
            if (eventBindingService != null)
            {
                if (methodDescriptor is DynamicPropertyDescriptor)
                    methodDescriptor = ((DynamicPropertyDescriptor)methodDescriptor).RealPropertyDescriptor;

                EventDescriptor eventDescriptor = eventBindingService.GetEvent(methodDescriptor);
                if (eventDescriptor != null)
                    eventBindingService.ShowCode(Activity, eventDescriptor);
                else
                    eventBindingService.ShowCode();
            }
        }

        private void OnPromoteBindings(object sender, EventArgs e)
        {
            DesignerVerb eventVerb = sender as DesignerVerb;
            if (eventVerb == null)
                return;

            IServiceProvider serviceProvider = GetService(typeof(DesignSurface)) as IServiceProvider;
            Debug.Assert(serviceProvider != null);

            List<CustomProperty> properties = CustomActivityDesignerHelper.GetCustomProperties(serviceProvider);
            if (properties == null)
                return;

            // get all the members of the custom activity to ensure uniqueness
            Type customActivityType = CustomActivityDesignerHelper.GetCustomActivityType(serviceProvider);
            List<String> customPropertyNames = new List<String>();
            foreach (MemberInfo memberInfo in customActivityType.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
            {
                if (!customPropertyNames.Contains(memberInfo.Name))
                    customPropertyNames.Add(memberInfo.Name);
            }

            PropertyDescriptor[] propertyDescriptors = BindableProperties;
            Dictionary<PropertyDescriptor, ActivityBind> promotedProperties = new Dictionary<PropertyDescriptor, ActivityBind>();

            foreach (PropertyDescriptor propertyDescriptor in propertyDescriptors)
            {
                ActivityBind bind = propertyDescriptor.GetValue(Activity) as ActivityBind;
                if (bind != null)
                    continue;

                CustomProperty newCustomProperty = CustomProperty.CreateCustomProperty(Activity.Site, DesignerHelpers.GenerateUniqueIdentifier(Activity.Site, Activity.Name + "_" + propertyDescriptor.Name, customPropertyNames.ToArray()), propertyDescriptor, Activity);

                properties.Add(newCustomProperty);

                // verify this name will not repeat
                customPropertyNames.Add(newCustomProperty.Name);

                // set a new bind
                promotedProperties.Add(propertyDescriptor, new ActivityBind(ActivityBind.GetRelativePathExpression(Helpers.GetRootActivity(Activity), Activity), newCustomProperty.Name));
            }

            //We have a restriction that we need to emit the custom properties furst before setting the binding
            CustomActivityDesignerHelper.SetCustomProperties(properties, serviceProvider);

            foreach (PropertyDescriptor promotedProperty in promotedProperties.Keys)
                promotedProperty.SetValue(Activity, promotedProperties[promotedProperty]);
        }

        private void OnBindProperty(object sender, EventArgs e)
        {
            IExtendedUIService extendedUIService = GetService(typeof(IExtendedUIService)) as IExtendedUIService;
            if (extendedUIService != null)
                BindUITypeEditor.EditValue(extendedUIService.GetSelectedPropertyContext());
        }

        private void OnGenerateEventHandlerStatusUpdate(object sender, EventArgs e)
        {
            DesignerVerb verb = sender as DesignerVerb;
            if (verb == null)
                return;

            bool canGenerateHandlers = false;
            PropertyDescriptor[] propertyDescriptors = EventHandlerProperties;
            foreach (PropertyDescriptor propertyDescriptor in propertyDescriptors)
            {
                object handler = propertyDescriptor.GetValue(Activity);
                if (handler == null)
                {
                    canGenerateHandlers = true;
                    break;
                }
            }

            verb.Enabled = canGenerateHandlers;
        }

        private void OnPromoteBindingsStatusUpdate(object sender, EventArgs e)
        {
            DesignerVerb verb = sender as DesignerVerb;
            if (verb == null)
                return;

            bool canPromoteBindings = false;
            PropertyDescriptor[] propertyDescriptors = this.BindableProperties;
            foreach (PropertyDescriptor propertyDescriptor in propertyDescriptors)
            {
                ActivityBind bind = propertyDescriptor.GetValue(Activity) as ActivityBind;
                if (bind == null)
                {
                    canPromoteBindings = true;
                    break;
                }
            }
            verb.Enabled = canPromoteBindings;
        }

        private void OnBindPropertyStatusUpdate(object sender, EventArgs e)
        {
            DesignerVerb verb = sender as DesignerVerb;
            if (verb == null)
                return;

            bool canBindProperty = false;
            string propertyName = null;
            IExtendedUIService extendedUIService = GetService(typeof(IExtendedUIService)) as IExtendedUIService;
            if (extendedUIService != null)
            {
                ITypeDescriptorContext propertyContext = extendedUIService.GetSelectedPropertyContext();
                canBindProperty = (propertyContext != null && ActivityBindPropertyDescriptor.IsBindableProperty(propertyContext.PropertyDescriptor) && !propertyContext.PropertyDescriptor.IsReadOnly);
                propertyName = (propertyContext != null) ? propertyContext.PropertyDescriptor.Name : null;
            }

            verb.Properties["Text"] = (propertyName != null && verb.Enabled) ? string.Format(CultureInfo.CurrentCulture, DR.GetString(DR.BindSelectedPropertyFormat), propertyName) : DR.GetString(DR.BindSelectedProperty);
            verb.Enabled = (canBindProperty && !IsLocked);
        }

        private void OnComponentRenamed(object sender, ComponentRenameEventArgs e)
        {
            ActivityDesigner designer = ActivityDesigner.GetDesigner(e.Component as Activity);
            if (designer != null)
                this.Text = Activity.Name;

        }

        private void OnComponentChanged(object sender, ComponentChangedEventArgs e)
        {
            if (e.Component != null && (e.OldValue is ActivityBind && !(e.NewValue is ActivityBind)) ||
                (!(e.OldValue is ActivityBind) && e.NewValue is ActivityBind))
                TypeDescriptor.Refresh(e.Component);

            IReferenceService referenceService = GetService(typeof(IReferenceService)) as IReferenceService;
            Activity changedActivity = (referenceService != null) ? referenceService.GetComponent(e.Component) as Activity : e.Component as Activity;
            if (changedActivity != null)
            {
                ActivityDesigner designer = ActivityDesigner.GetDesigner(changedActivity);
                if (designer != null)
                    designer.OnActivityChanged(new ActivityChangedEventArgs(changedActivity, e.Member, e.OldValue, e.NewValue));
            }
        }

        private void OnLoadComplete(object sender, EventArgs e)
        {
            WorkflowView workflowView = ((IRootDesigner)this).GetView(ViewTechnology.Default) as WorkflowView;
            if (workflowView != null)
                workflowView.Idle += new EventHandler(OnFirstIdle);
        }

        private void OnFirstIdle(object sender, EventArgs e)
        {
            WorkflowView workflowView = ((IRootDesigner)this).GetView(ViewTechnology.Default) as WorkflowView;
            if (workflowView != null)
                workflowView.Idle -= new EventHandler(OnFirstIdle);

            // Select this component.
            ISelectionService selectionService = (ISelectionService)GetService(typeof(ISelectionService));
            if (selectionService != null && selectionService.SelectionCount == 0)
                selectionService.SetSelectedComponents(new object[] { Activity }, SelectionTypes.Replace);

            DesignerHelpers.RefreshDesignerActions(Activity.Site);
            Invalidate();
        }
        #endregion

        #endregion

        #region Interface Implementation

        #region IDisposable Implementation
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

        #region IDesigner Implementation
        IComponent IDesigner.Component
        {
            get
            {
                return this.activity as IComponent;
            }
        }

        DesignerVerbCollection IDesigner.Verbs
        {
            get
            {
                ActivityDesignerVerbCollection verbs = new ActivityDesignerVerbCollection();
                verbs.AddRange(Verbs);

                IDesignerVerbProviderService verbProviderService = GetService(typeof(IDesignerVerbProviderService)) as IDesignerVerbProviderService;
                if (verbProviderService != null)
                {
                    foreach (IDesignerVerbProvider verbProvider in verbProviderService.VerbProviders)
                        verbs.AddRange(verbProvider.GetVerbs(this));
                }

                return verbs.SafeCollection;
            }
        }

        void IDesigner.Initialize(IComponent component)
        {
            this.activity = component as Activity;
            if (this.activity == null)
                throw new ArgumentException(DR.GetString(DR.Error_InvalidActivity), "component");

            Initialize(this.activity);
        }

        void IDesigner.DoDefaultAction()
        {
            DoDefaultAction();
        }
        #endregion

        #region IDesignerFilter Implementation
        void IDesignerFilter.PreFilterAttributes(IDictionary attributes)
        {
            PreFilterAttributes(attributes);
        }

        void IDesignerFilter.PreFilterProperties(IDictionary properties)
        {
            PreFilterProperties(properties);
        }

        void IDesignerFilter.PreFilterEvents(IDictionary events)
        {
            PreFilterEvents(events);
        }

        void IDesignerFilter.PostFilterAttributes(IDictionary attributes)
        {
            PostFilterAttributes(attributes);
        }

        void IDesignerFilter.PostFilterProperties(IDictionary properties)
        {
            PostFilterProperties(properties);
        }

        void IDesignerFilter.PostFilterEvents(IDictionary events)
        {
            PostFilterEvents(events);
        }
        #endregion

        #region IConnectableDesigner Implementation
        bool IConnectableDesigner.CanConnect(ConnectionPoint source, ConnectionPoint target)
        {
            return CanConnect(source, target);
        }

        void IConnectableDesigner.OnConnected(ConnectionPoint source, ConnectionPoint target)
        {
            OnConnected(source, target);
        }
        #endregion

        #region IWorkflowRootDesigner Implementation
        ViewTechnology[] IRootDesigner.SupportedTechnologies
        {
            get
            {
                return new ViewTechnology[] { ViewTechnology.Default };
            }
        }

        object IRootDesigner.GetView(ViewTechnology technology)
        {
            DesignSurface surface = GetService(typeof(DesignSurface)) as DesignSurface;
            IDesignerHost designerHost = GetService(typeof(IDesignerHost)) as IDesignerHost;
            if (this.workflowView == null && surface != null && designerHost != null && designerHost.RootComponent == Activity)
                this.workflowView = CreateView(technology);
            return this.workflowView;
        }

        CompositeActivityDesigner IWorkflowRootDesigner.InvokingDesigner
        {
            get
            {
                return InvokingDesigner;
            }

            set
            {
                InvokingDesigner = value;
            }
        }

        ReadOnlyCollection<WorkflowDesignerMessageFilter> IWorkflowRootDesigner.MessageFilters
        {
            get
            {
                return MessageFilters;
            }
        }

        bool IWorkflowRootDesigner.IsSupportedActivityType(Type activityType)
        {
            return IsSupportedActivityType(activityType);
        }

        bool IWorkflowRootDesigner.SupportsLayoutPersistence
        {
            get
            {
                return SupportsLayoutPersistence;
            }
        }
        #endregion

        #region IToolboxUser Implementation
        bool IToolboxUser.GetToolSupported(ToolboxItem toolboxItem)
        {
            // Default is true.  If any goes wrong, let the compiler catch it.
            bool itemSupported = true;

            //get ui service refernce
            IExtendedUIService2 uiService = this.GetService(typeof(IExtendedUIService2)) as IExtendedUIService2;
            //and use it to obtain project's target framework version
            if (null != uiService)
            {
                //in case of target framework less than 3.5 - disable ReceiveActivity and SendActivity
                long targetFramework = uiService.GetTargetFrameworkVersion();

                if (targetFramework != 0)
                {
                    // if target framework is less than 3.0 dont show any toolbox item, because workflow didnt ship then.
                    if (targetFramework < ActivityDesigner.FrameworkVersion_3_0)
                    {
                        return false;
                    }

                    if (targetFramework < ActivityDesigner.FrameworkVersion_3_5)
                    {
                        if (string.Equals(toolboxItem.TypeName, "System.Workflow.Activities.ReceiveActivity") ||
                            string.Equals(toolboxItem.TypeName, "System.Workflow.Activities.SendActivity"))
                        {
                            return false;
                        }
                    }
                }
            }

            ITypeProvider typeProvider = GetService(typeof(ITypeProvider)) as ITypeProvider;
            if (typeProvider != null)
            {
                Type itemType = null;
                if (typeProvider.LocalAssembly != null)
                    itemType = typeProvider.LocalAssembly.GetType(toolboxItem.TypeName, false);
                if (itemType == null)
                {
                    try
                    {
                        itemType = Type.GetType(toolboxItem.TypeName + ", " + toolboxItem.AssemblyName);
                    }
                    catch (FileNotFoundException) { }
                    catch (FileLoadException) { }
                }

                if (itemType == null)
                    return itemSupported;

                //check if the activity is NOT supported - we ask this to the Root Designer only. If so, bail out.
                IWorkflowRootDesigner rootDesigner = ActivityDesigner.GetSafeRootDesigner(Activity.Site) as IWorkflowRootDesigner;
                if (rootDesigner != null)
                {
                    if (!rootDesigner.IsSupportedActivityType(itemType))
                    {
                        return false;
                    }
                    else if (rootDesigner.InvokingDesigner != null && rootDesigner.InvokingDesigner.Activity != null)
                    {
                        rootDesigner = ActivityDesigner.GetSafeRootDesigner(rootDesigner.InvokingDesigner.Activity.Site) as IWorkflowRootDesigner;
                        if (rootDesigner != null && !rootDesigner.IsSupportedActivityType(itemType))
                            return false;
                    }
                }

                if (!(toolboxItem is ActivityToolboxItem))
                {
                    object[] attributes = itemType.GetCustomAttributes(typeof(ToolboxItemAttribute), false);
                    if (attributes.Length > 0)
                    {
                        itemSupported = false;
                        foreach (Attribute attribute in attributes)
                        {
                            ToolboxItemAttribute toolBoxItemAttribute = attribute as ToolboxItemAttribute;
                            if (toolBoxItemAttribute != null && typeof(System.Workflow.ComponentModel.Design.ActivityToolboxItem).IsAssignableFrom(toolBoxItemAttribute.ToolboxItemType))
                            {
                                itemSupported = true;
                                break;
                            }
                        }
                    }
                }
            }
            return itemSupported;
        }

        void IToolboxUser.ToolPicked(ToolboxItem toolboxItem)
        {
            ISelectionService selectionService = GetService(typeof(ISelectionService)) as ISelectionService;
            IToolboxService toolboxService = GetService(typeof(IToolboxService)) as IToolboxService;
            if (toolboxItem == null || selectionService == null)
                return;

            object selectedObject = selectionService.PrimarySelection;
            if (!(selectedObject is HitTestInfo) && !(selectedObject is CompositeActivity))
                return;

            //Get the paste target
            HitTestInfo hitInfo = null;
            CompositeActivity compositeActivity = null;
            if (selectedObject is HitTestInfo)
            {
                hitInfo = (HitTestInfo)selectedObject;
                compositeActivity = hitInfo.AssociatedDesigner.Activity as CompositeActivity;
            }
            else if (selectedObject is CompositeActivity)
            {
                compositeActivity = (CompositeActivity)selectedObject;
                hitInfo = new HitTestInfo(ActivityDesigner.GetDesigner(compositeActivity), HitTestLocations.Designer);
            }

            //Get the parent designer for pasting
            CompositeActivityDesigner compositeActivityDesigner = ActivityDesigner.GetDesigner(compositeActivity) as CompositeActivityDesigner;
            if (compositeActivityDesigner == null)
                return;


            Activity[] activities = CompositeActivityDesigner.DeserializeActivitiesFromToolboxItem(Activity.Site, toolboxItem, false);
            if (activities.Length == 0)
                return;

            if (!compositeActivityDesigner.CanInsertActivities(hitInfo, new List<Activity>(activities).AsReadOnly()))
                return;

            try
            {
                activities = CompositeActivityDesigner.DeserializeActivitiesFromToolboxItem(Activity.Site, toolboxItem, true);
                if (activities.Length > 0)
                {
                    CompositeActivityDesigner.InsertActivities(compositeActivityDesigner, hitInfo, new List<Activity>(activities).AsReadOnly(), SR.GetString(SR.PastingActivities));
                    selectionService.SetSelectedComponents(activities, SelectionTypes.Replace);
                    ParentView.EnsureVisible(activities[0]);
                }
            }
            catch (CheckoutException ex)
            {
                if (ex != CheckoutException.Canceled)
                    throw new Exception(DR.GetString(DR.ActivityInsertError) + "\n" + ex.Message, ex);
            }
        }
        #endregion

        #region IPersistUIState Implementation
        void IPersistUIState.SaveViewState(BinaryWriter writer)
        {
            SaveViewState(writer);
        }

        void IPersistUIState.LoadViewState(BinaryReader reader)
        {
            LoadViewState(reader);
        }
        #endregion

        #region IWorkflowDesignerMessageSink Members
        bool IWorkflowDesignerMessageSink.OnMouseDown(MouseEventArgs e)
        {
            WorkflowView parentView = ParentView;
            if (parentView != null)
            {
                try
                {
                    Point logicalPoint = parentView.ClientPointToLogical(new Point(e.X, e.Y));
                    OnMouseDown(new MouseEventArgs(e.Button, e.Clicks, logicalPoint.X, logicalPoint.Y, e.Delta));
                }
                catch
                {
                }
            }
            return true;
        }

        bool IWorkflowDesignerMessageSink.OnMouseMove(MouseEventArgs e)
        {
            WorkflowView parentView = ParentView;
            if (parentView != null)
            {
                try
                {
                    Point logicalPoint = parentView.ClientPointToLogical(new Point(e.X, e.Y));
                    OnMouseMove(new MouseEventArgs(e.Button, e.Clicks, logicalPoint.X, logicalPoint.Y, e.Delta));
                }
                catch
                {
                }
            }
            return true;
        }

        bool IWorkflowDesignerMessageSink.OnMouseUp(MouseEventArgs e)
        {
            WorkflowView parentView = ParentView;
            if (parentView != null)
            {
                try
                {
                    Point logicalPoint = parentView.ClientPointToLogical(new Point(e.X, e.Y));
                    OnMouseUp(new MouseEventArgs(e.Button, e.Clicks, logicalPoint.X, logicalPoint.Y, e.Delta));
                }
                catch
                {
                }
            }
            return true;
        }

        bool IWorkflowDesignerMessageSink.OnMouseDoubleClick(MouseEventArgs e)
        {
            WorkflowView parentView = ParentView;
            if (parentView != null)
            {
                try
                {
                    Point logicalPoint = parentView.ClientPointToLogical(new Point(e.X, e.Y));
                    OnMouseDoubleClick(new MouseEventArgs(e.Button, e.Clicks, logicalPoint.X, logicalPoint.Y, e.Delta));
                }
                catch
                {
                }
            }
            return true;
        }

        bool IWorkflowDesignerMessageSink.OnMouseEnter(MouseEventArgs e)
        {
            WorkflowView parentView = ParentView;
            if (parentView != null)
            {
                try
                {
                    Point logicalPoint = parentView.ClientPointToLogical(new Point(e.X, e.Y));
                    OnMouseEnter(new MouseEventArgs(e.Button, e.Clicks, logicalPoint.X, logicalPoint.Y, e.Delta));
                }
                catch
                {
                }
            }
            return true;
        }

        bool IWorkflowDesignerMessageSink.OnMouseHover(MouseEventArgs e)
        {
            WorkflowView parentView = ParentView;
            if (parentView != null)
            {
                try
                {
                    Point logicalPoint = parentView.ClientPointToLogical(new Point(e.X, e.Y));
                    OnMouseHover(new MouseEventArgs(e.Button, e.Clicks, logicalPoint.X, logicalPoint.Y, e.Delta));
                }
                catch
                {
                }
            }
            return true;
        }

        bool IWorkflowDesignerMessageSink.OnMouseLeave()
        {
            try
            {
                OnMouseLeave();
            }
            catch
            {
            }

            return true;
        }

        bool IWorkflowDesignerMessageSink.OnMouseWheel(MouseEventArgs e)
        {
            //Only used in message filters
            return true;
        }

        bool IWorkflowDesignerMessageSink.OnMouseCaptureChanged()
        {
            try
            {
                OnMouseCaptureChanged();
            }
            catch
            {
            }

            return true;
        }

        bool IWorkflowDesignerMessageSink.OnMouseDragBegin(Point initialPoint, MouseEventArgs e)
        {
            WorkflowView parentView = ParentView;
            if (parentView != null)
            {
                try
                {
                    Point logicalPoint = parentView.ClientPointToLogical(new Point(e.X, e.Y));
                    OnMouseDragBegin(initialPoint, new MouseEventArgs(e.Button, e.Clicks, logicalPoint.X, logicalPoint.Y, e.Delta));
                }
                catch
                {
                }
            }
            return true;
        }

        bool IWorkflowDesignerMessageSink.OnMouseDragMove(MouseEventArgs e)
        {
            WorkflowView parentView = ParentView;
            if (parentView != null)
            {
                try
                {
                    Point logicalPoint = parentView.ClientPointToLogical(new Point(e.X, e.Y));
                    OnMouseDragMove(new MouseEventArgs(e.Button, e.Clicks, logicalPoint.X, logicalPoint.Y, e.Delta));
                }
                catch
                {
                }
            }
            return true;
        }

        bool IWorkflowDesignerMessageSink.OnMouseDragEnd()
        {
            try
            {
                OnMouseDragEnd();
            }
            catch
            {
            }
            return true;
        }

        bool IWorkflowDesignerMessageSink.OnDragEnter(DragEventArgs e)
        {
            try
            {
                OnDragEnter(e as ActivityDragEventArgs);
            }
            catch
            {
            }
            return true;
        }

        bool IWorkflowDesignerMessageSink.OnDragOver(DragEventArgs e)
        {
            try
            {
                OnDragOver(e as ActivityDragEventArgs);
            }
            catch
            {
            }
            return true;
        }

        bool IWorkflowDesignerMessageSink.OnDragLeave()
        {
            try
            {
                OnDragLeave();
            }
            catch
            {
            }
            return true;
        }

        bool IWorkflowDesignerMessageSink.OnDragDrop(DragEventArgs e)
        {
            try
            {
                OnDragDrop(e as ActivityDragEventArgs);
            }
            catch
            {
            }

            return true;
        }

        bool IWorkflowDesignerMessageSink.OnGiveFeedback(GiveFeedbackEventArgs e)
        {
            try
            {
                OnGiveFeedback(e);
            }
            catch
            {
            }
            return true;
        }

        bool IWorkflowDesignerMessageSink.OnQueryContinueDrag(QueryContinueDragEventArgs e)
        {
            try
            {
                OnQueryContinueDrag(e);
            }
            catch
            {
            }
            return true;
        }

        bool IWorkflowDesignerMessageSink.OnKeyDown(KeyEventArgs e)
        {
            try
            {
                OnKeyDown(e);
            }
            catch
            {
            }
            return true;
        }

        bool IWorkflowDesignerMessageSink.OnKeyUp(KeyEventArgs e)
        {
            try
            {
                OnKeyUp(e);
            }
            catch
            {
            }
            return true;
        }

        bool IWorkflowDesignerMessageSink.OnScroll(ScrollBar sender, int value)
        {
            try
            {
                OnScroll(sender, value);
            }
            catch
            {
            }
            return true;
        }

        bool IWorkflowDesignerMessageSink.OnShowContextMenu(Point screenMenuPoint)
        {
            //Only used in message filters
            return true;
        }

        bool IWorkflowDesignerMessageSink.ProcessMessage(Message message)
        {
            try
            {
                OnProcessMessage(message);
            }
            catch
            {
            }
            return true;
        }

        void IWorkflowDesignerMessageSink.OnLayout(LayoutEventArgs layoutEventArgs)
        {
            //Only used in message filters
        }

        void IWorkflowDesignerMessageSink.OnLayoutPosition(Graphics graphics)
        {
            try
            {
                OnLayoutPosition(new ActivityDesignerLayoutEventArgs(graphics, DesignerTheme));
            }
            catch
            {
            }
        }

        void IWorkflowDesignerMessageSink.OnLayoutSize(Graphics graphics)
        {
            try
            {
                Size = OnLayoutSize(new ActivityDesignerLayoutEventArgs(graphics, DesignerTheme));
            }
            catch
            {
            }
        }

        void IWorkflowDesignerMessageSink.OnBeginResizing(DesignerEdges sizingEdge)
        {
            try
            {
                OnBeginResizing(new ActivityDesignerResizeEventArgs(sizingEdge, Bounds));
            }
            catch
            {
            }
        }

        void IWorkflowDesignerMessageSink.OnResizing(DesignerEdges sizingEdge, Rectangle bounds)
        {
            try
            {
                OnResizing(new ActivityDesignerResizeEventArgs(sizingEdge, bounds));
            }
            catch
            {
            }
        }

        void IWorkflowDesignerMessageSink.OnEndResizing()
        {
            try
            {
                OnEndResizing();
            }
            catch
            {
            }
        }

        void IWorkflowDesignerMessageSink.OnThemeChange()
        {
            try
            {
                OnThemeChange(DesignerTheme);
            }
            catch
            {
            }
        }

        bool IWorkflowDesignerMessageSink.OnPaint(PaintEventArgs e, Rectangle viewPort)
        {
            try
            {
                Rectangle bounds = Bounds;
                if (IsVisible && viewPort.IntersectsWith(bounds))
                {
                    GlyphManager glyphManager = GetService(typeof(IDesignerGlyphProviderService)) as GlyphManager;
                    bounds.Width += 1; bounds.Height += 1;

                    using (GraphicsPath graphicsPath = ActivityDesignerPaint.GetDesignerPath(this, Point.Empty, new Size(DesignerTheme.BorderWidth, DesignerTheme.BorderWidth), DesignerEdges.All, false))
                    using (Region clipRegion = new Region(graphicsPath))
                    {
                        Region oldRegion = e.Graphics.Clip;
                        clipRegion.Intersect(oldRegion);
                        clipRegion.Intersect(viewPort);

                        bool restoredClipState = false;
                        try
                        {
                            ActivityDesignerPaintEventArgs eventArgs = new ActivityDesignerPaintEventArgs(e.Graphics, bounds, viewPort, DesignerTheme);

                            e.Graphics.Clip = clipRegion;
                            OnPaint(eventArgs);
                            e.Graphics.Clip = oldRegion;
                            restoredClipState = true;

                            if (glyphManager != null)
                                glyphManager.DrawDesignerGlyphs(eventArgs, this);

                            DrawingState &= (~DrawingStates.InvalidDraw);
                        }
                        catch
                        {
                            //Eat the exception thrown
                            DrawingState |= DrawingStates.InvalidDraw;
                        }
                        finally
                        {
                            if (!restoredClipState)
                                e.Graphics.Clip = oldRegion;

                            if (DrawingState != DrawingStates.Valid)
                                ActivityDesignerPaint.DrawInvalidDesignerIndicator(e.Graphics, this);
                        }
                    }
                }
            }
            catch
            {
            }
            return true;
        }

        bool IWorkflowDesignerMessageSink.OnPaintWorkflowAdornments(PaintEventArgs e, Rectangle viewPort)
        {
            //Only used in message filters
            return true;
        }
        #endregion

        #endregion

        #region Class SmartTag
        internal sealed class SmartTag : DesignerGlyph
        {
            internal const int DefaultHeight = 2;
            private static Image defaultImage = DR.GetImage(DR.SmartTag);
            private ActivityDesigner activeDesigner;

            public override Rectangle GetBounds(ActivityDesigner designer, bool activated)
            {
                Rectangle smartTagRectangle = Rectangle.Empty;
                Rectangle rectangle = designer.SmartTagRectangle;
                if (!rectangle.IsEmpty)
                {
                    Size glyphSize = WorkflowTheme.CurrentTheme.AmbientTheme.GlyphSize;
                    Size imageSize = rectangle.Size;

                    Size margin = WorkflowTheme.CurrentTheme.AmbientTheme.Margin;

                    smartTagRectangle.X = rectangle.Left - margin.Width / 2;
                    smartTagRectangle.Y = rectangle.Top - margin.Height / 2;
                    smartTagRectangle.Width = imageSize.Width + glyphSize.Width / 2 + 3 * margin.Width;
                    smartTagRectangle.Height = imageSize.Height + margin.Height;
                }

                return smartTagRectangle;
            }

            public override bool CanBeActivated
            {
                get
                {
                    return true;
                }
            }

            protected override void OnPaint(Graphics graphics, bool activated, AmbientTheme ambientTheme, ActivityDesigner designer)
            {
                Rectangle activatedBounds = GetBounds(designer, true);

                bool formShown = false; //if the drop down form is shown, draw the arrow up
                if (Form.ActiveForm != null && Form.ActiveForm.GetType().FullName.Equals(typeof(ItemPalette).FullName + "+Palette", StringComparison.Ordinal))
                    formShown = (Form.ActiveForm.Location == designer.PointToScreen(new Point(activatedBounds.Left, activatedBounds.Bottom)));

                //work around: This is in order to show the smarttag activated when the drop down is shown but cursor leaves the active area of glyph
                if (!activated)
                {
                    if (this.activeDesigner != null)
                    {
                        activated = true;
                    }
                    else if (Form.ActiveForm != null && Form.ActiveForm.GetType().FullName.Equals(typeof(ItemPalette).FullName + "+Palette", StringComparison.Ordinal))
                    {
                        activated = formShown;
                    }
                }

                graphics.FillRectangle(WorkflowTheme.CurrentTheme.AmbientTheme.BackgroundBrush, activatedBounds);
                using (Brush transparentSelectionBrush = new SolidBrush(Color.FromArgb(50, WorkflowTheme.CurrentTheme.AmbientTheme.SelectionForeColor)))
                    graphics.FillRectangle(transparentSelectionBrush, activatedBounds);
                graphics.DrawRectangle(SystemPens.ControlDarkDark, activatedBounds);

                //Draw the image
                Image image = designer.Image;
                image = (designer.Image == null) ? SmartTag.defaultImage : image;

                Size glyphSize = WorkflowTheme.CurrentTheme.AmbientTheme.GlyphSize;
                Size imageSize = designer.SmartTagRectangle.Size;

                Size margin = WorkflowTheme.CurrentTheme.AmbientTheme.Margin;

                Rectangle imageRectangle = activatedBounds;
                imageRectangle.X += margin.Width / 2;
                imageRectangle.Y += margin.Height / 2;
                imageRectangle.Size = imageSize;
                ActivityDesignerPaint.DrawImage(graphics, image, imageRectangle, DesignerContentAlignment.Center);

                //Draw the drop down indicator
                Rectangle dropDownRectangle = activatedBounds;
                dropDownRectangle.X += imageSize.Width + 3 * margin.Width / 2;
                dropDownRectangle.Y += margin.Height / 2;
                dropDownRectangle.Width = glyphSize.Width / 2;
                dropDownRectangle.Height -= glyphSize.Height / 4;
                using (GraphicsPath graphicsPath = ActivityDesignerPaint.GetScrollIndicatorPath(dropDownRectangle, ScrollButton.Down))
                {
                    graphics.FillPath(Brushes.Black, graphicsPath);
                    graphics.DrawPath(Pens.Black, graphicsPath);
                }
            }

            protected override void OnActivate(ActivityDesigner designer)
            {
                if (designer.SmartTagVerbs.Count > 0)
                {
                    this.activeDesigner = designer;
                    Rectangle bounds = GetBounds(designer, true);
                    this.activeDesigner.OnShowSmartTagVerbs(new Point(bounds.Left, bounds.Bottom + 1));
                    this.activeDesigner = null;
                }
            }

            internal ActivityDesigner ActiveDesigner
            {
                get
                {
                    return this.activeDesigner;
                }
            }
        }
        #endregion
    }
    #endregion

}
