namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Drawing;
    using System.Collections;
    using System.Collections.ObjectModel;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.CodeDom;
    using System.IO;
    using System.Reflection;
    using System.Drawing.Printing;
    using System.Drawing.Drawing2D;
    using System.Runtime.InteropServices;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;
    using System.Security.Permissions;

    #region Workflow Interfaces

    #region Interface IIdentifierCreationService

    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public interface IIdentifierCreationService
    {
        void EnsureUniqueIdentifiers(CompositeActivity parentActivity, ICollection childActivities);
        void ValidateIdentifier(Activity activity, string identifier);
    }

    #endregion

    #region Interface IMemberCreationService

    //Revisit the functions in this interface for consistency and performance
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public interface IMemberCreationService
    {
        void CreateField(string className, string fieldName, Type fieldType, Type[] genericParameterTypes, MemberAttributes attributes, CodeSnippetExpression initializationExpression, bool overwriteExisting);
        void CreateProperty(string className, string propertyName, Type propertyType, AttributeInfo[] attributes, bool emitDependencyProperty, bool isMetaProperty, bool isAttached, Type ownerType, bool isReadOnly);
        void CreateEvent(string className, string eventName, Type eventType, AttributeInfo[] attributes, bool emitDependencyProperty);
        void UpdateTypeName(string oldClassName, string newClassName);
        void UpdateBaseType(string className, Type baseType);
        void UpdateProperty(string className, string oldPropertyName, Type oldPropertyType, string newPropertyName, Type newPropertyType, AttributeInfo[] attributes, bool emitDependencyProperty, bool isMetaProperty);
        void UpdateEvent(string className, string oldEventName, Type oldEventType, string newEventName, Type newEventType, AttributeInfo[] attributes, bool emitDependencyProperty, bool isMetaProperty);
        void RemoveProperty(string className, string propertyName, Type propertyType);
        void RemoveEvent(string className, string eventName, Type eventType);

        void ShowCode(Activity activity, string methodName, Type delegateType);
        void ShowCode();
    }

    #endregion

    #region Interface IExtendedUIService
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public interface IExtendedUIService
    {
        DialogResult AddWebReference(out Uri url, out Type proxyClass);
        Uri GetUrlForProxyClass(Type proxyClass);
        Type GetProxyClassForUrl(Uri url);

        //Task provider related functionality
        void AddDesignerActions(DesignerAction[] actions);
        void RemoveDesignerActions();

        //Property Grid related functionality
        bool NavigateToProperty(string propName);

        //Get the selected property context from property grid
        ITypeDescriptorContext GetSelectedPropertyContext();

        //Show the ToolsOptions
        void ShowToolsOptions();

        //Retrieve Xsd project Item information
        Dictionary<string, Type> GetXsdProjectItemsInfo();

        //Add assembly reference (including dynamic assembly resolution)
        void AddAssemblyReference(AssemblyName assemblyName);


    }

    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public interface IExtendedUIService2
    {
        //Get current project's target version
        long GetTargetFrameworkVersion();

        //Get if the given type is supported in the current target framework
        bool IsSupportedType(Type type);

        //Get the assembly loaded in reflection context for the current target framework.
        Assembly GetReflectionAssembly(AssemblyName assemblyName);

        //Get the current runtime type for the passed in reflection type. Reflection type is target framework type.
        Type GetRuntimeType(Type reflectionType);
    }
    #endregion

    #endregion

    #region Design Surface related classes and Interfaces

    #region Interface IWorkflowDesignerMessageSink
    internal interface IWorkflowDesignerMessageSink
    {
        bool OnMouseDown(MouseEventArgs e);
        bool OnMouseMove(MouseEventArgs e);
        bool OnMouseUp(MouseEventArgs e);
        bool OnMouseDoubleClick(MouseEventArgs e);
        bool OnMouseEnter(MouseEventArgs e);
        bool OnMouseHover(MouseEventArgs e);
        bool OnMouseLeave();
        bool OnMouseWheel(MouseEventArgs e);
        bool OnMouseCaptureChanged();
        bool OnMouseDragBegin(Point initialPoint, MouseEventArgs e);
        bool OnMouseDragMove(MouseEventArgs e);
        bool OnMouseDragEnd();

        bool OnDragEnter(DragEventArgs e);
        bool OnDragOver(DragEventArgs e);
        bool OnDragLeave();
        bool OnDragDrop(DragEventArgs e);
        bool OnGiveFeedback(GiveFeedbackEventArgs e);
        bool OnQueryContinueDrag(QueryContinueDragEventArgs e);

        bool OnKeyDown(KeyEventArgs e);
        bool OnKeyUp(KeyEventArgs e);

        bool OnScroll(ScrollBar sender, int value);
        bool OnShowContextMenu(Point screenMenuPoint);
        bool ProcessMessage(Message message);

        void OnLayout(LayoutEventArgs layoutEventArgs);
        void OnLayoutPosition(Graphics graphics);
        void OnLayoutSize(Graphics graphics);
        void OnThemeChange();

        void OnBeginResizing(DesignerEdges sizingEdge);
        void OnResizing(DesignerEdges sizingEdge, Rectangle bounds);
        void OnEndResizing();

        bool OnPaint(PaintEventArgs e, Rectangle viewPort);
        bool OnPaintWorkflowAdornments(PaintEventArgs e, Rectangle viewPort);
    }
    #endregion

    #region Class ViewPortData
    //

    internal sealed class ViewPortData
    {
        public Rectangle LogicalViewPort;
        public Bitmap MemoryBitmap;
        public SizeF Scaling = new SizeF(1.0f, 1.0f);
        public Point Translation = Point.Empty;
        public Size ShadowDepth = Size.Empty;
        public Color TransparentColor = Color.White;
        public Size ViewPortSize = Size.Empty;
    }
    #endregion

    #region Class ActivityDragEventArgs
    /// <summary>
    /// EventArgs passed to the ActivityDesigners when drag drop operation is in progress on workflow.
    /// ActivityDesigners can access the information contained to influence the drag drop behavior.
    /// </summary>
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class ActivityDragEventArgs : DragEventArgs
    {
        private Point snapPoint = Point.Empty;
        private Point dragInitiationPoint = Point.Empty;
        private List<Activity> draggedActivities;

        internal ActivityDragEventArgs(DragEventArgs dragEventArgs, Point dragInitiationPoint, Point point, List<Activity> draggedActivities)
            : base(dragEventArgs.Data, dragEventArgs.KeyState, point.X, point.Y, dragEventArgs.AllowedEffect, dragEventArgs.Effect)
        {
            this.dragInitiationPoint = dragInitiationPoint;

            if (draggedActivities == null)
                this.draggedActivities = new List<Activity>();
            else
                this.draggedActivities = new List<Activity>(draggedActivities);
        }

        /// <summary>
        /// Returns Activities being dragged drop
        /// </summary>
        public ReadOnlyCollection<Activity> Activities
        {
            get
            {
                return this.draggedActivities.AsReadOnly();
            }
        }

        /// <summary>
        /// WorkflowView creates drag image for the activities being dragged. 
        /// ActivityDesigners can choose to snap this image to a drop target to indicate that activities can be dropped at a particular location
        /// </summary>
        public Point DragImageSnapPoint
        {
            get
            {
                return this.snapPoint;
            }

            set
            {
                this.snapPoint = value;
            }
        }

        /// <summary>
        /// Returns point at which the drag drop operation was initiated in logical coordinates
        /// </summary>
        public Point DragInitiationPoint
        {
            get
            {
                return this.dragInitiationPoint;
            }
        }
    }
    #endregion

    #endregion

    #region Classes, Interfaces and enums used by Designers

    #region Class ActivityChangeEventArgs
    /// <summary>
    /// Contains information about the changes made to the activity associated with the designer
    /// </summary>
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class ActivityChangedEventArgs : EventArgs
    {
        private Activity activity;
        private MemberDescriptor member;
        private object oldValue;
        private object newValue;

        public ActivityChangedEventArgs(Activity activity, MemberDescriptor member, object oldValue, object newValue)
        {
            this.activity = activity;
            this.member = member;
            this.oldValue = oldValue;
            this.newValue = newValue;
        }

        /// <summary>
        /// Activity being changed
        /// </summary>
        public Activity Activity
        {
            get
            {
                return this.activity;
            }
        }

        /// <summary>
        /// Member of the activity being changed
        /// </summary>
        public MemberDescriptor Member
        {
            get
            {
                return this.member;
            }
        }

        /// <summary>
        /// OldValue of the member before the change
        /// </summary>
        public object OldValue
        {
            get
            {
                return this.oldValue;
            }
        }

        /// <summary>
        /// New value of the member after the change
        /// </summary>
        public object NewValue
        {
            get
            {
                return this.newValue;
            }
        }
    }
    #endregion

    #region Class ActivityDesignerLayoutEventArgs
    /// <summary>
    /// Contains arguments passed to layout functions of ActivityDesigner
    /// </summary>
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class ActivityDesignerLayoutEventArgs : EventArgs
    {
        private Graphics graphics;
        private ActivityDesignerTheme designerTheme;

        public ActivityDesignerLayoutEventArgs(Graphics graphics, ActivityDesignerTheme designerTheme)
        {
            this.graphics = graphics;
            this.designerTheme = designerTheme;
        }

        /// <summary>
        /// Gets the ambient theme associated with workflow
        /// </summary>
        public AmbientTheme AmbientTheme
        {
            get
            {
                return WorkflowTheme.CurrentTheme.AmbientTheme;
            }
        }

        /// <summary>
        /// Gets the designet theme associated with activity designer
        /// </summary>
        public ActivityDesignerTheme DesignerTheme
        {
            get
            {
                return this.designerTheme;
            }
        }

        /// <summary>
        /// Gets the graphics object on which the activity designer will be drawn
        /// </summary>
        public Graphics Graphics
        {
            get
            {
                return this.graphics;
            }
        }
    }
    #endregion

    #region Class ActivityDesignerPaintEventArgs
    /// <summary>
    /// Contains arguments passed to draw function of ActivityDesigner
    /// </summary>
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class ActivityDesignerPaintEventArgs : EventArgs
    {
        private Graphics graphics;
        private Rectangle clipRectangle;
        private Rectangle viewPort;
        private ActivityDesignerTheme designerTheme;

        public ActivityDesignerPaintEventArgs(Graphics graphics, Rectangle clipRectangle, Rectangle viewPort, ActivityDesignerTheme designerTheme)
        {
            this.graphics = graphics;
            this.clipRectangle = Rectangle.Inflate(clipRectangle, 1, 1);
            this.viewPort = viewPort;
            this.designerTheme = designerTheme;
        }

        /// <summary>
        /// Graphics object associated with design surface on which the activity needs to draw
        /// </summary>
        public Graphics Graphics
        {
            get
            {
                return this.graphics;
            }
        }

        /// <summary>
        /// Bounding rectangle outside which the activity can not draw
        /// </summary>
        public Rectangle ClipRectangle
        {
            get
            {
                return this.clipRectangle;
            }
        }

        /// <summary>
        /// Gets the ambient theme associated with Workflow
        /// </summary>
        public AmbientTheme AmbientTheme
        {
            get
            {
                return WorkflowTheme.CurrentTheme.AmbientTheme;
            }
        }

        /// <summary>
        /// Gets the theme associated with designer
        /// </summary>
        public ActivityDesignerTheme DesignerTheme
        {
            get
            {
                return this.designerTheme;
            }
        }

        internal Rectangle ViewPort
        {
            get
            {
                return this.viewPort;
            }
        }
    }
    #endregion

    #region Class ActivityDesignerResizeEventArgs
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class ActivityDesignerResizeEventArgs : EventArgs
    {
        private DesignerEdges sizingEdge;
        private Rectangle newBounds;

        public ActivityDesignerResizeEventArgs(DesignerEdges sizingEdge, Rectangle newBounds)
        {
            this.sizingEdge = sizingEdge;
            this.newBounds = newBounds;
        }

        public DesignerEdges SizingEdge
        {
            get
            {
                return this.sizingEdge;
            }
        }

        public Rectangle Bounds
        {
            get
            {
                return this.newBounds;
            }
        }
    }
    #endregion

    #region Enum DesignerEdges
    [Flags]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public enum DesignerEdges
    {
        None = 0,
        Left = 1,
        Top = 2,
        Right = 4,
        Bottom = 8,
        All = 15
    }
    #endregion

    #region Interface IDesignerGlyphProvider
    /// <summary>
    /// Allows the user to add custom glyph providers. 
    /// Custom glyph providers are called to render the glyphs on designer.
    /// </summary>
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public interface IDesignerGlyphProviderService
    {
        void AddGlyphProvider(IDesignerGlyphProvider glyphProvider);
        void RemoveGlyphProvider(IDesignerGlyphProvider glyphProvider);
        ReadOnlyCollection<IDesignerGlyphProvider> GlyphProviders { get; }
    }

    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public interface IDesignerGlyphProvider
    {
        ActivityDesignerGlyphCollection GetGlyphs(ActivityDesigner activityDesigner);
    }
    #endregion

    #region Interface IDesignerVerbProvider
    /// <summary>
    /// Allows the user to add custom verb providers. 
    /// Custom verb providers are called in order to return the set of verbs associated with the designer.
    /// </summary>
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public interface IDesignerVerbProviderService
    {
        void AddVerbProvider(IDesignerVerbProvider verbProvider);
        void RemoveVerbProvider(IDesignerVerbProvider verbProvider);
        ReadOnlyCollection<IDesignerVerbProvider> VerbProviders { get; }
    }

    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public interface IDesignerVerbProvider
    {
        ActivityDesignerVerbCollection GetVerbs(ActivityDesigner activityDesigner);
    }
    #endregion

    #region Interface IPersistUIState
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public interface IPersistUIState
    {
        void SaveViewState(BinaryWriter writer);
        void LoadViewState(BinaryReader reader);
    }
    #endregion

    #region Interface IWorkflowRootDesigner
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public interface IWorkflowRootDesigner : IRootDesigner
    {
        CompositeActivityDesigner InvokingDesigner { get; set; }
        ReadOnlyCollection<WorkflowDesignerMessageFilter> MessageFilters { get; }
        bool IsSupportedActivityType(Type activityType);
        bool SupportsLayoutPersistence { get; }
    }
    #endregion

    #region Used for keyboard navigation
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public enum DesignerNavigationDirection
    {
        Down = 0, //Next
        Up = 1, //Previous
        Left = 2,
        Right = 3
    }
    #endregion

    #region enum HitTestLocations
    /// <summary>
    /// Enumeration returning area of the designer which was under the point passed to hit test.
    /// </summary>
    [FlagsAttribute]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public enum HitTestLocations
    {
        None = 0,
        Designer = 1,
        ActionArea = 2,
        Left = 4,
        Top = 8,
        Right = 16,
        Bottom = 32,
        Connector = 64
    }
    #endregion

    #region Class HitTestInfo
    /// <summary>
    /// Identifies the part of the designer at the specified location. 
    /// Used by various operation including drag-drop, cut-paste etc
    /// </summary>
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class HitTestInfo
    {
        private static HitTestInfo nowhere;
        private ActivityDesigner activityDesigner = null;
        private HitTestLocations location = HitTestLocations.None;
        private IDictionary userData;

        /// <summary>
        /// Returns Empty HitTestInfo
        /// </summary>
        public static HitTestInfo Nowhere
        {
            get
            {
                if (HitTestInfo.nowhere == null)
                    HitTestInfo.nowhere = new HitTestInfo();
                return HitTestInfo.nowhere;
            }
        }

        internal HitTestInfo()
        {
        }

        /// <summary>
        /// Constructs HitTestInfo with specified parameters
        /// </summary>
        /// <param name="designer">ActivityDesigner associated with HitTestInfo</param>
        /// <param name="flags">HitTestLocations indicating where the hit happened</param>
        public HitTestInfo(ActivityDesigner designer, HitTestLocations location)
        {
            if (designer == null)
                throw new ArgumentNullException("designer");

            this.activityDesigner = designer;
            this.location = location;
        }

        /// <summary>
        /// Returns designer associated
        /// </summary>
        [Browsable(false)]
        public ActivityDesigner AssociatedDesigner
        {
            get
            {
                return this.activityDesigner;
            }
        }

        /// <summary>
        /// Returns flags indicating where hit happened
        /// </summary>
        [Browsable(false)]
        public HitTestLocations HitLocation
        {
            get
            {
                return this.location;
            }
        }

        /// <summary>
        /// Returns enclosing bounds for the hit area
        /// </summary>
        [Browsable(false)]
        public virtual Rectangle Bounds
        {
            get
            {
                if (this.activityDesigner != null)
                    return this.activityDesigner.Bounds;
                else
                    return Rectangle.Empty;
            }
        }

        /// <summary>
        /// Returns selectable object associated with Hit Area
        /// </summary>
        [Browsable(false)]
        public virtual object SelectableObject
        {
            get
            {
                if (this.activityDesigner != null)
                    return this.activityDesigner.Activity;
                else
                    return null;
            }
        }

        /// <summary>
        /// Returns UserData to associated with HitLocation
        /// </summary>
        [Browsable(false)]
        public IDictionary UserData
        {
            get
            {
                if (this.userData == null)
                    this.userData = new HybridDictionary();
                return this.userData;
            }
        }

        /// <summary>
        /// Maps Hit area to index using which designers are to be inserted.
        /// </summary>
        /// <returns></returns>
        public virtual int MapToIndex()
        {
            CompositeActivity compositeActivity = this.activityDesigner.Activity as CompositeActivity;
            if (compositeActivity != null)
                return compositeActivity.Activities.Count;
            else
                return 0;
        }
    }
    #endregion

    #region Class ConnectorHitTestInfo
    /// <summary>
    /// Represents the hittest information for connectors within the designer, structured designers are expected to have connectors within them
    /// </summary>
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class ConnectorHitTestInfo : HitTestInfo
    {
        private int connector = 0;

        /// <summary>
        /// Constructor for ConnectorHitTestInfo
        /// </summary>
        /// <param name="structuredCompositeActivityDesigner">Designer associated with the HitTestInfo</param>
        /// <param name="flags">Flags where HitTest occured</param>
        /// <param name="connector">Index of the connector which was hit</param>
        public ConnectorHitTestInfo(CompositeActivityDesigner compositeActivityDesigner, HitTestLocations flags, int connector)
            : base(compositeActivityDesigner, flags)
        {
            if (this.connector < 0)
                throw new ArgumentException(SR.GetString(SR.Error_InvalidConnectorValue), "connector");

            this.connector = connector;
        }

        public override Rectangle Bounds
        {
            get
            {
                //
                SequentialActivityDesigner sequentialActivityDesigner = AssociatedDesigner as SequentialActivityDesigner;
                if (sequentialActivityDesigner != null && sequentialActivityDesigner.Expanded)
                {
                    Rectangle[] connectors = sequentialActivityDesigner.GetConnectors();
                    if (connectors.Length > 0)
                        return connectors[this.connector];
                }

                return Rectangle.Empty;
            }
        }

        public override object SelectableObject
        {
            get
            {
                return this;
            }
        }

        public override int MapToIndex()
        {
            return this.connector;
        }

        public override bool Equals(object obj)
        {
            ConnectorHitTestInfo destinationConnector = obj as ConnectorHitTestInfo;
            if (destinationConnector != null)
            {
                if (destinationConnector.AssociatedDesigner == AssociatedDesigner &&
                    destinationConnector.HitLocation == HitLocation &&
                    destinationConnector.MapToIndex() == MapToIndex())
                    return true;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() ^ ((AssociatedDesigner != null) ? AssociatedDesigner.GetHashCode() : 0) ^ MapToIndex().GetHashCode();
        }
    }
    #endregion

    #region Class DesignerAction
    //Public class as ActivityDesigners can provide their own DesignerActions
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class DesignerAction
    {
        private ActivityDesigner activityDesigner;
        private int actionId;
        private string text;
        private Image image;
        private IDictionary userData;
        private string propertyName = null;

        public DesignerAction(ActivityDesigner activityDesigner, int actionId, string text)
        {
            if (activityDesigner == null)
                throw new ArgumentNullException("activityDesigner");

            if (text == null || text.Length == 0)
                throw new ArgumentException(SR.GetString(SR.Error_NullOrEmptyValue), "text");

            this.activityDesigner = activityDesigner;
            this.actionId = actionId;
            this.text = text;
        }

        public DesignerAction(ActivityDesigner activityDesigner, int actionId, string text, Image image)
            : this(activityDesigner, actionId, text)
        {
            this.image = image;
        }

        public string PropertyName
        {
            get
            {
                return this.propertyName;
            }
            set
            {
                this.propertyName = value;
            }
        }

        public int ActionId
        {
            get
            {
                return this.actionId;
            }
        }

        public string Text
        {
            get
            {
                return this.text;
            }
        }

        public Image Image
        {
            get
            {
                return this.image;
            }
        }

        public IDictionary UserData
        {
            get
            {
                if (this.userData == null)
                    this.userData = new HybridDictionary();
                return this.userData;
            }
        }

        public void Invoke()
        {
            this.activityDesigner.OnExecuteDesignerAction(this);
        }
    }
    #endregion

    #region Enum DesignerVerbGroup
    /// <summary>
    /// Provides categories for grouping of similar verbs
    /// </summary>
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public enum DesignerVerbGroup
    {
        General = 0,
        View = 1,
        Edit = 2,
        Options = 3,
        Actions = 4,
        Misc = 5
    }
    #endregion

    #region Class ActivityDesignerVerb
    /// <summary>
    /// DesignerVerb class specific to ActivityDesigners. 
    /// Allows user to group similar types of DesignerVerbs togather.
    /// Provides user the ability to update the status of the verb.
    /// </summary>
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class ActivityDesignerVerb : DesignerVerb
    {
        private ActivityDesigner activityDesigner = null;
        private EventHandler invokeHandler = null;
        private EventHandler statusHandler = null;
        private DesignerVerbGroup verbGroup;
        private int id = 0;

        public ActivityDesignerVerb(ActivityDesigner activityDesigner, DesignerVerbGroup verbGroup, string text, EventHandler invokeHandler)
            : base(text, new EventHandler(OnExecuteDesignerVerb), new CommandID(WorkflowMenuCommands.MenuGuid, 0))
        {
            if (text == null || text.Length == 0)
                throw new ArgumentNullException("text");

            if (invokeHandler == null)
                throw new ArgumentNullException("invokeHandler");

            this.verbGroup = verbGroup;
            this.invokeHandler = invokeHandler;
            this.activityDesigner = activityDesigner;
        }

        public ActivityDesignerVerb(ActivityDesigner activityDesigner, DesignerVerbGroup verbGroup, string text, EventHandler invokeHandler, EventHandler statusHandler)
            : this(activityDesigner, verbGroup, text, invokeHandler)
        {
            this.statusHandler = statusHandler;
        }

        public override int OleStatus
        {
            get
            {
                if (statusHandler != null)
                {
                    try
                    {
                        statusHandler(this, EventArgs.Empty);
                    }
                    catch
                    {
                    }
                }

                return base.OleStatus;
            }
        }

        public override CommandID CommandID
        {
            get
            {
                return new CommandID(WorkflowMenuCommands.MenuGuid, this.id);
            }
        }

        public DesignerVerbGroup Group
        {
            get
            {
                return this.verbGroup;
            }
        }

        internal int Id
        {
            get
            {
                return this.id;
            }

            set
            {
                this.id = value;
            }
        }

        internal ActivityDesigner ActivityDesigner
        {
            get
            {
                return this.activityDesigner;
            }
        }

        private static void OnExecuteDesignerVerb(object sender, EventArgs e)
        {
            ActivityDesignerVerb activityDesignerVerb = sender as ActivityDesignerVerb;
            if (activityDesignerVerb != null)
            {
                if (activityDesignerVerb.invokeHandler != null)
                    activityDesignerVerb.invokeHandler(sender, e);

                int status = activityDesignerVerb.OleStatus;
                status = 0;

                if (activityDesignerVerb.activityDesigner != null)
                {
                    foreach (DesignerVerb verb in ((IDesigner)activityDesignerVerb.activityDesigner).Verbs)
                    {
                        if (verb is ActivityDesignerVerb)
                        {
                            //Update the status of the 
                            status = verb.OleStatus;
                            status = 0;
                        }
                    }
                }
            }
        }
    }
    #endregion

    #region Class ActivityDesignerVerbCollection
    /// <summary>
    /// Maintains collection of ActivityDesignerVerbs. 
    /// Groups verbs belonging to the same veb groups and ensures that their identifiers are consecutive.
    /// </summary>
    [PermissionSetAttribute(SecurityAction.InheritanceDemand, Name = "FullTrust")]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class ActivityDesignerVerbCollection : DesignerVerbCollection
    {
        public ActivityDesignerVerbCollection()
        {
        }

        public ActivityDesignerVerbCollection(IEnumerable<ActivityDesignerVerb> verbs)
        {
            if (verbs == null)
                throw new ArgumentNullException("verbs");

            foreach (ActivityDesignerVerb verb in verbs)
                Add(verb);
        }

        protected override void OnValidate(object value)
        {
            if (!(value is ActivityDesignerVerb))
                throw new InvalidOperationException(SR.GetString(SR.Error_InvalidDesignerVerbValue));
        }

        /// <summary>
        /// Returns a collection that is consumable by MenuCommandService. Verbs in the 
        /// safe collection have Ids in the range of VerbFirst.ID to VerbLast.ID. Also,
        /// the items are sorted, and the first item ID == VerbFirst.ID
        /// </summary>
        internal ActivityDesignerVerbCollection SafeCollection
        {
            get
            {
                if (this.Count == 0)
                    return this;

                Dictionary<DesignerVerbGroup, List<ActivityDesignerVerb>> activityDesignerVerbs = new Dictionary<DesignerVerbGroup, List<ActivityDesignerVerb>>();
                ArrayList safeCollection = new ArrayList(this);

                // set Ids
                foreach (ActivityDesignerVerb verb in safeCollection)
                {
                    List<ActivityDesignerVerb> verbList = null;
                    if (!activityDesignerVerbs.ContainsKey(verb.Group))
                    {
                        verbList = new List<ActivityDesignerVerb>();
                        activityDesignerVerbs.Add(verb.Group, verbList);
                    }
                    else
                    {
                        verbList = activityDesignerVerbs[verb.Group];
                    }

                    if (!verbList.Contains(verb))
                    {
                        verb.Id = ConvertGroupToId(verb.Group) + verbList.Count;
                        verbList.Add(verb);
                    }
                }

                //items should be sorted by verb id 
                safeCollection.Sort(new ActivityDesignerVerbComparer());

                // add first dummy verb if needed
                if (((ActivityDesignerVerb)safeCollection[0]).Id != MenuCommands.VerbFirst.ID)
                {
                    safeCollection.Insert(0, new ActivityDesignerVerb(null, DesignerVerbGroup.General, "Dummy", new EventHandler(OnDummyVerb)));
                    ((ActivityDesignerVerb)safeCollection[0]).Visible = false;
                }

                ActivityDesignerVerbCollection verbs = new ActivityDesignerVerbCollection();
                foreach (ActivityDesignerVerb verb in safeCollection)
                    verbs.Add(verb);

                return verbs;
            }
        }

        private void OnDummyVerb(object sender, EventArgs e)
        {
            // Should never be called
        }

        private int ConvertGroupToId(DesignerVerbGroup group)
        {
            if (group == DesignerVerbGroup.General)
                return WorkflowMenuCommands.VerbGroupGeneral;
            else if (group == DesignerVerbGroup.View)
                return WorkflowMenuCommands.VerbGroupView;
            else if (group == DesignerVerbGroup.Edit)
                return WorkflowMenuCommands.VerbGroupEdit;
            else if (group == DesignerVerbGroup.Options)
                return WorkflowMenuCommands.VerbGroupOptions;
            else if (group == DesignerVerbGroup.Actions)
                return WorkflowMenuCommands.VerbGroupActions;
            else
                return WorkflowMenuCommands.VerbGroupMisc;
        }
        #region class ActivityDesignerVerbComparer

        private class ActivityDesignerVerbComparer : IComparer
        {
            #region IComparer Members

            public int Compare(object x, object y)
            {
                ActivityDesignerVerb verb1 = x as ActivityDesignerVerb;
                ActivityDesignerVerb verb2 = y as ActivityDesignerVerb;

                if (verb1.Id == verb2.Id)
                    return 0;
                else if (verb1.Id > verb2.Id)
                    return 1;
                else
                    return -1;
            }
            #endregion
        }

        #endregion

    }
    #endregion

    #endregion

    #region ITypeFilterProvider Interface
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public interface ITypeFilterProvider
    {
        bool CanFilterType(Type type, bool throwOnError);
        string FilterDescription { get; }
    }
    #endregion

    #region TypeFilterProviderAttribute
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false)]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class TypeFilterProviderAttribute : Attribute
    {
        private string typeName = null;

        public TypeFilterProviderAttribute(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            this.typeName = type.AssemblyQualifiedName;
        }

        public TypeFilterProviderAttribute(string typeName)
        {
            this.typeName = typeName;
        }

        public string TypeFilterProviderTypeName
        {
            get
            {
                return this.typeName;
            }
        }
    }
    #endregion

    #region ITypeProviderCreator Interface
    [Guid("0E6DF9D7-B4B5-4af7-9647-FC335CCE393F")]
    [ComVisible(true)]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public interface ITypeProviderCreator
    {
        ITypeProvider GetTypeProvider(object obj);
        Assembly GetLocalAssembly(object obj);
        Assembly GetTransientAssembly(AssemblyName assemblyName);
        ITypeResolutionService GetTypeResolutionService(object obj);
    }
    #endregion
}
