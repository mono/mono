namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.IO;
    using System.Data;
    using System.Drawing;
    using System.Security;
    using System.Resources;
    using System.Reflection;
    using System.Diagnostics;
    using System.Collections;
    using System.Windows.Forms;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.Drawing.Imaging;
    using System.Drawing.Printing;
    using System.Drawing.Drawing2D;
    using System.Workflow.Interop;
    using System.Collections.Generic;
    using System.Windows.Forms.Design;
    using System.Security.Permissions;
    using System.ComponentModel.Design;
    using System.Runtime.InteropServices;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    /// What did I change in this file
    /// 1. Eliminated the layout manager and introduced classes for WorkflowLayout and PrintPreviewLayout
    /// 2. Eliminated the event syncing of PageSetupData change. We call performlayout on the current designer service whenever the pagesetupdata changes

    /// Designer Features:
    /// Selection on click and thru drag rectangle
    /// Reconfigurable background
    /// Scrolling
    /// Ensure Visible functionality
    /// Accessibility
    /// Zoom
    /// Rehostable
    /// Extensible
    /// Small memory footprint
    /// Ability to move around objects using drag drop
    /// Ability to drop the objects using drag drop
    /// Printing support
    /// Theme support
    /// Magnifier
    /// AutoScroll
    /// AutoExpand
    /// USE THIS FOR PERFORMANCE TEST: Debug.WriteLine("******Root drawing: " + Convert.ToString((DateTime.Now.Ticks - ticks) / 10000) + "ms");
    ///
    /// Here are some details about the coordinate system,
    /// 
    /// Screen CoOrdinate System: Starts at 0,0 of the screen
    /// Client CoOrdinate System: Starts at 0,0 of the control
    /// Logical CoOrdinate System: The workflowview supports zooming and scroll, we want to hide this
    /// complexity from the activity writter and hence whenever we get a coordinate we translate it based 
    /// scroll position, zoom level and layout. This helps us to sheild the activity designers from complexity 
    /// of zooming, scaling and layouting. The designer writters deal with one coordinate system which is unscaled and
    /// starts at 0,0
    /// 
    ///

    [ToolboxItem(false)]
    [ActivityDesignerTheme(typeof(AmbientTheme), Xml = WorkflowView.ThemeXml)]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class WorkflowView : UserControl, IServiceProvider, IMessageFilter
    {
        #region Theme Initializer XML
        internal const string ThemeXml =
                "<AmbientTheme xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/workflow\"" +
                     " ApplyTo=\"System.Workflow.ComponentModel.Design.WorkflowView\"" +
                     " ShowConfigErrors=\"True\"" +
                     " DrawShadow=\"False\"" +
                     " DrawGrayscale=\"False\"" +
                     " DropIndicatorColor=\"0xFF006400\"" +
                     " SelectionForeColor=\"0xFF0000FF\"" +
                     " SelectionPatternColor=\"0xFF606060\"" +
                     " ForeColor=\"0xFF808080\"" +
                     " BackColor=\"0xFFFFFFFF\"" +
                     " ShowGrid=\"False\"" +
                     " GridColor=\"0xFFC0C0C0\"" +
                     " TextQuality=\"Aliased\"" +
                     " DrawRounded=\"True\"" +
                     " ShowDesignerBorder=\"True\"" +
                 " />";
        #endregion

        #region Members Variables
        [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
        private enum TabButtonIds { MultiPage = 1, Zoom, Pan }

        //Designer Hookup
        private IServiceProvider serviceProvider = null;

        private ActivityDesigner rootDesigner = null;

        //Zoom
        private float zoomLevel = 1.0f;
        private int shadowDepth = WorkflowTheme.CurrentTheme.AmbientTheme.ShadowDepth;

        //MessageFilters
        private List<WorkflowDesignerMessageFilter> stockMessageFilters = new List<WorkflowDesignerMessageFilter>();
        private List<WorkflowDesignerMessageFilter> customMessageFilters = new List<WorkflowDesignerMessageFilter>();

        //

        private Bitmap viewPortBitmap = null;

        //Misc.
        private WorkflowToolTip workflowToolTip = null;

        private CommandSet commandSet = null;
        private DynamicAction fitAllAction = null;

        //print
        private int prePreviewZoom = 100;
        private Point prePreviewScroll = Point.Empty;
        private WorkflowPrintDocument printDocument = null;

        //Active layout
        private WorkflowLayout activeLayout = null;
        private WorkflowLayout defaultLayout = null;

        //One time callable delegates
        private EventHandler layoutEventHandler = null;
        private EventHandler ensureVisibleEventHandler = null;

        private Stack<HitTestInfo> messageHitTestContexts = new Stack<HitTestInfo>();

        private HScrollBar hScrollBar;
        private VScrollBar vScrollBar;

        private TabControl toolContainer;
        private EventHandler idleEventListeners;
        private EventHandler idleEventHandler;

        private bool dragDropInProgress;
        #endregion

        #region Events
        public event EventHandler ZoomChanged;
        public event EventHandler RootDesignerChanged;
        #endregion

        #region Constructor and Dispose
        public WorkflowView()
            : this(new DesignSurface())
        {
        }

        public WorkflowView(IServiceProvider serviceProvider)
        {
            Debug.Assert(serviceProvider != null);
            if (serviceProvider == null)
                throw new ArgumentNullException("serviceProvider");

            SuspendLayout();
            AllowDrop = true;
            AutoScroll = false;
            HScroll = false;
            VScroll = false;
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.Opaque | ControlStyles.AllPaintingInWmPaint | ControlStyles.Selectable | ControlStyles.EnableNotifyMessage, true);

            this.serviceProvider = serviceProvider;

            //*****Promote the services which are accessed from other components
            IServiceContainer serviceContainer = GetService(typeof(IServiceContainer)) as IServiceContainer;
            if (serviceContainer != null)
            {
                //Remove any existing designer service if there is any
                serviceContainer.RemoveService(typeof(WorkflowView));
                serviceContainer.AddService(typeof(WorkflowView), this);
            }

            //set the UI Service to be used by themes
            IUIService uiService = this.serviceProvider.GetService(typeof(IUIService)) as IUIService;
            if (uiService != null)
                WorkflowTheme.UIService = uiService;

            //Make sure that we add scrollbars
            EnsureScrollBars(new HScrollBar(), new VScrollBar());

            //Initialize the tooltip shown
            this.workflowToolTip = new WorkflowToolTip(this);

            //Sync the global theme change event, which is fired by the theme infrastructure for theme change
            WorkflowTheme.ThemeChanged += new EventHandler(OnThemeChange);

            //Create the core message filters
            PopulateMessageFilters(true);

            //Set the root designer, note that the dynamic action is dependent on the DynamicActionMessageFilter pushed
            //when the root is set.
            RootDesigner = ActivityDesigner.GetSafeRootDesigner(this);
            this.fitAllAction = CreateDynamicAction();

            //If the active layout is still null then we will set the default layout as active layout
            if (this.activeLayout == null || this.defaultLayout == null)
                ActiveLayout = DefaultLayout = new WorkflowRootLayout(this.serviceProvider);

            //Create the local command set and update all the commands once
            IMenuCommandService menuCommandService = GetService(typeof(IMenuCommandService)) as IMenuCommandService;
            if (menuCommandService != null)
            {
                this.commandSet = new CommandSet(this);
                this.commandSet.UpdatePanCommands(true);
            }

            //Subscribe to selection change
            ISelectionService selectionService = GetService(typeof(ISelectionService)) as ISelectionService;
            if (selectionService != null)
                selectionService.SelectionChanged += new EventHandler(OnSelectionChanged);

            //In case of non VS case we need to pumpin the Keyboard messages, the user control sets
            //focus to the child controls by default which is a problem so we need to trap the 
            //messages by adding application level message filter, in case of VS this is not required and
            //the message filter is never called.
            Application.AddMessageFilter(this);

            //We make sure that during the construction we dont do perform layouts on idle event
            ResumeLayout(true);
        }

        protected override void Dispose(bool disposing)
        {
            //Remove the proffered services
            if (disposing)
            {
                try
                {
                    SuspendLayout();

                    Application.RemoveMessageFilter(this);

                    if (this.layoutEventHandler != null)
                    {
                        Idle -= this.layoutEventHandler;
                        this.layoutEventHandler = null;
                    }

                    if (this.ensureVisibleEventHandler != null)
                    {
                        Idle -= this.ensureVisibleEventHandler;
                        this.ensureVisibleEventHandler = null;
                    }

                    if (this.idleEventHandler != null)
                    {
                        this.idleEventListeners = null;

                        Form host = TopLevelControl as Form;
                        if (!Application.MessageLoop || (host != null && host.Modal))
                            WorkflowTimer.Default.Unsubscribe(this.idleEventHandler);
                        else
                            Application.Idle -= this.idleEventHandler;
                        this.idleEventHandler = null;
                    }

                    ISelectionService selectionService = GetService(typeof(ISelectionService)) as ISelectionService;
                    if (selectionService != null)
                        selectionService.SelectionChanged -= new EventHandler(OnSelectionChanged);

                    //Unsubscribe the theme change
                    WorkflowTheme.ThemeChanged -= new EventHandler(OnThemeChange);

                    //Remove the dynamic action
                    if (this.fitAllAction != null)
                    {
                        this.fitAllAction.Dispose();
                        this.fitAllAction = null;
                    }

                    if (this.workflowToolTip != null)
                    {
                        ((IDisposable)this.workflowToolTip).Dispose();
                        this.workflowToolTip = null;
                    }

                    DisposeMessageFilters(false);
                    DisposeMessageFilters(true);

                    //Dispose the layouts
                    this.activeLayout = null;
                    if (this.defaultLayout != null)
                    {
                        this.defaultLayout.Dispose();
                        this.defaultLayout = null;
                    }

                    //Destroy other resources
                    if (this.viewPortBitmap != null)
                    {
                        this.viewPortBitmap.Dispose();
                        this.viewPortBitmap = null;
                    }

                    if (this.commandSet != null)
                    {
                        this.commandSet.Dispose();
                        this.commandSet = null;
                    }

                    HScrollBar.ValueChanged -= new EventHandler(OnScroll);
                    VScrollBar.ValueChanged -= new EventHandler(OnScroll);

                    if (this.toolContainer != null)
                    {
                        Controls.Remove(this.toolContainer);
                        this.toolContainer.TabStrip.Tabs.Clear();
                        this.toolContainer.Dispose();
                        this.toolContainer = null;
                    }

                    IServiceContainer serviceContainer = GetService(typeof(IServiceContainer)) as IServiceContainer;
                    if (serviceContainer != null)
                    {
                        serviceContainer.RemoveService(typeof(WorkflowView));
                    }
                }
                finally
                {
                    ResumeLayout(false);
                }
            }

            base.Dispose(disposing);
        }
        #endregion

        #region Public Properties
        public int Zoom
        {
            get
            {
                return Convert.ToInt32(this.zoomLevel * 100);
            }

            set
            {
                if (Zoom == value)
                    return;

                if (value < AmbientTheme.MinZoom || value > AmbientTheme.MaxZoom)
                    throw new NotSupportedException(DR.GetString(DR.ZoomLevelException2, AmbientTheme.MinZoom, AmbientTheme.MaxZoom));

                ScrollBar hScrollBar = HScrollBar;
                ScrollBar vScrollBar = VScrollBar;

                if (hScrollBar != null && vScrollBar != null)
                {
                    PointF oldRelativeCenter = Point.Empty;
                    Point oldCenter = new Point(ScrollPosition.X, ScrollPosition.Y);
                    oldRelativeCenter = new PointF((float)oldCenter.X / (float)hScrollBar.Maximum, (float)oldCenter.Y / (float)vScrollBar.Maximum);

                    //recalculate the zoom and scroll range
                    this.zoomLevel = (float)value / 100.0f;
                    UpdateScrollRange();

                    //center the view again
                    Point newCenter = new Point((int)((float)hScrollBar.Maximum * oldRelativeCenter.X), (int)((float)vScrollBar.Maximum * oldRelativeCenter.Y));
                    ScrollPosition = new Point(newCenter.X, newCenter.Y);

                    if (this.rootDesigner != null)
                        this.rootDesigner.Location = this.activeLayout.RootDesignerAlignment;

                    InvalidateClientRectangle(Rectangle.Empty);

                    //

                    this.activeLayout.Update(null, WorkflowLayout.LayoutUpdateReason.ZoomChanged);

                    //force command refresh
                    //this is to workarond VS not refreshing Zoom drop down when doing area zoom-in
                    IUIService uis = GetService(typeof(IUIService)) as IUIService;
                    if (uis != null)
                        uis.SetUIDirty();

                    //We need to update the zoom commands when the zoom is updated
                    if (this.commandSet != null)
                        this.commandSet.UpdateZoomCommands(true);

                    OnZoomChanged();
                }
            }
        }

        public ActivityDesigner RootDesigner
        {
            get
            {
                return this.rootDesigner;
            }

            set
            {
                if (this.rootDesigner == value)
                    return;

                DisposeMessageFilters(false);

                this.rootDesigner = value;

                if (this.rootDesigner != null)
                {
                    PopulateMessageFilters(false);
                    ActiveLayout = DefaultLayout = this.rootDesigner.SupportedLayout;
                }

                OnRootDesignerChanged();

                base.PerformLayout();
            }
        }

        public int ShadowDepth
        {
            get
            {
                return this.shadowDepth;
            }

            set
            {
                if (value < AmbientTheme.MinShadowDepth || value > AmbientTheme.MaxShadowDepth)
                    throw new NotSupportedException(DR.GetString(DR.ShadowDepthException, AmbientTheme.MinShadowDepth, AmbientTheme.MaxShadowDepth));

                if (this.shadowDepth == value)
                    return;

                this.shadowDepth = value;
                InvalidateClientRectangle(Rectangle.Empty);
            }
        }

        public Rectangle ViewPortRectangle
        {
            get
            {
                return new Rectangle(ScrollPosition, ViewPortSize);
            }
        }

        public Size ViewPortSize
        {
            get
            {
                Size viewPortSize = ClientSize;
                if (HScrollBar.Visible)
                    viewPortSize.Height = Math.Max(0, viewPortSize.Height - HScrollBar.Height);
                if (VScrollBar.Visible)
                    viewPortSize.Width = Math.Max(0, viewPortSize.Width - VScrollBar.Width);
                return viewPortSize;
            }
        }

        public Point ScrollPosition
        {
            get
            {
                return new Point(HScrollBar.Value, VScrollBar.Value);
            }

            set
            {
                ScrollBar hScrollBar = HScrollBar;
                if (hScrollBar != null)
                {
                    value.X = Math.Min(value.X, hScrollBar.Maximum - hScrollBar.LargeChange + 1);
                    value.X = Math.Max(value.X, hScrollBar.Minimum);
                    hScrollBar.Value = value.X;
                }

                ScrollBar vScrollBar = VScrollBar;
                if (vScrollBar != null)
                {
                    value.Y = Math.Min(value.Y, vScrollBar.Maximum - vScrollBar.LargeChange + 1);
                    value.Y = Math.Max(value.Y, vScrollBar.Minimum);
                    vScrollBar.Value = value.Y;
                }
            }
        }

        public bool PrintPreviewMode
        {
            get
            {
                return (this.activeLayout == ((WorkflowPrintDocument)PrintDocument).PrintPreviewLayout);
            }

            set
            {
                if (PrintPreviewMode == value)
                    return;

                if (value && PrinterSettings.InstalledPrinters.Count == 0)
                {
                    DesignerHelpers.ShowError(this, DR.GetString(DR.ThereIsNoPrinterInstalledErrorMessage));
                    value = false;
                }

                ActiveLayout = (value) ? ((WorkflowPrintDocument)PrintDocument).PrintPreviewLayout : DefaultLayout;

                if (this.commandSet != null)
                    this.commandSet.UpdatePageLayoutCommands(true);

                if (PrintPreviewMode)
                {
                    this.prePreviewZoom = Zoom;
                    this.prePreviewScroll = ScrollPosition;
                    Zoom = 40;
                }
                else
                {
                    Zoom = this.prePreviewZoom;
                    ScrollPosition = this.prePreviewScroll;
                }
            }
        }

        public PrintDocument PrintDocument
        {
            get
            {
                if (this.printDocument == null)
                    this.printDocument = new WorkflowPrintDocument(this);

                return this.printDocument;
            }
        }

        public event EventHandler Idle
        {
            add
            {
                //Add the listener to our list
                this.idleEventListeners += value;

                if (this.idleEventHandler == null)
                {
                    this.idleEventHandler = new EventHandler(OnWorkflowIdle);

                    Form host = TopLevelControl as Form;
                    if (!Application.MessageLoop || (host != null && host.Modal))
                        WorkflowTimer.Default.Subscribe(100, this.idleEventHandler);
                    else
                        Application.Idle += this.idleEventHandler;
                }
            }

            remove
            {
                this.idleEventListeners -= value;

                if (this.idleEventHandler != null && this.idleEventListeners == null)
                {
                    Form host = TopLevelControl as Form;
                    if (host != null && host.Modal)
                        WorkflowTimer.Default.Unsubscribe(this.idleEventHandler);
                    else
                        Application.Idle -= this.idleEventHandler;

                    this.idleEventHandler = null;
                }
            }
        }

        public HScrollBar HScrollBar
        {
            get
            {
                return this.hScrollBar;
            }
        }

        public VScrollBar VScrollBar
        {
            get
            {
                return this.vScrollBar;
            }
        }

        public bool EnableFitToScreen
        {
            get
            {
                return (this.fitAllAction != null);
            }

            set
            {
                if (EnableFitToScreen == value)
                    return;

                if (value)
                {
                    if (this.fitAllAction == null)
                        this.fitAllAction = CreateDynamicAction();
                }
                else
                {
                    if (this.fitAllAction != null)
                    {
                        this.fitAllAction.Dispose();
                        this.fitAllAction = null;
                    }
                }

                InvalidateClientRectangle(Rectangle.Empty);
            }
        }
        #endregion

        #region Protected Properties
        #endregion

        #region Private Properties
        internal bool DragDropInProgress
        {
            get
            {
                return this.dragDropInProgress;
            }
        }

        internal bool ShowToolContainer
        {
            get
            {
                return (this.toolContainer != null);
            }

            set
            {
                if (ShowToolContainer == value)
                    return;

                try
                {
                    SuspendLayout();

                    if (value)
                    {
                        this.toolContainer = new TabControl(DockStyle.Right, AnchorAlignment.Far);
                        Controls.Add(this.toolContainer);
                        EnsureScrollBars(this.hScrollBar, this.toolContainer.ScrollBar as VScrollBar);

                        string[,] tabButtonInfo = new string[/*Caption Resource ID*/, /*Bitmap Resource ID*/] { { "MultipageLayoutCaption", "MultipageLayout" }, { "ZoomCaption", "Zoom" }, { "PanCaption", "AutoPan" } };
                        for (int i = 0; i < tabButtonInfo.GetLength(0); i++)
                        {
                            Bitmap tabImage = DR.GetImage(tabButtonInfo[i, 1]) as Bitmap;
                            string buttonCaption = DR.GetString(tabButtonInfo[i, 0]);
                            this.toolContainer.TabStrip.Tabs.Add(new ItemInfo(i + 1, tabImage, buttonCaption));
                        }

                        this.toolContainer.TabStrip.TabChange += new SelectionChangeEventHandler<TabSelectionChangeEventArgs>(OnTabChange);
                        if (this.commandSet != null)
                        {
                            this.commandSet.UpdatePageLayoutCommands(true);
                            this.commandSet.UpdateZoomCommands(true);
                            this.commandSet.UpdatePanCommands(true);
                        }
                    }
                    else
                    {
                        this.toolContainer.TabStrip.TabChange -= new SelectionChangeEventHandler<TabSelectionChangeEventArgs>(OnTabChange);
                        this.toolContainer.TabStrip.Tabs.Clear();

                        Controls.Remove(this.toolContainer);
                        this.toolContainer.Dispose();
                        this.toolContainer = null;

                        EnsureScrollBars(this.hScrollBar, new VScrollBar());
                    }
                }
                finally
                {
                    ResumeLayout(true);
                }
            }
        }

        internal HitTestInfo MessageHitTestContext
        {
            get
            {
                return this.messageHitTestContexts.Peek();
            }
        }

        internal WorkflowLayout ActiveLayout
        {
            get
            {
                return this.activeLayout;
            }

            set
            {
                Debug.Assert(value != null);
                if (value == null)
                    throw new ArgumentNullException("Layout cannot be null!");

                Cursor cursor = Cursor.Current;
                try
                {
                    Cursor.Current = Cursors.WaitCursor;

                    this.activeLayout = value;
                    if (this.activeLayout != ((WorkflowPrintDocument)PrintDocument).PrintPreviewLayout)
                        DefaultLayout = this.activeLayout;

                    base.PerformLayout();
                    if (this.commandSet != null)
                        this.commandSet.UpdatePageLayoutCommands(true);
                }
                finally
                {
                    Cursor.Current = cursor;
                }
            }
        }

        private WorkflowLayout DefaultLayout
        {
            get
            {
                if (this.defaultLayout == null)
                    this.defaultLayout = new WorkflowRootLayout(this);
                return this.defaultLayout;
            }

            set
            {
                if (value == null)
                    throw new ArgumentNullException(DR.GetString(DR.Error_WorkflowLayoutNull));

                if (this.defaultLayout == value)
                    return;

                if (this.defaultLayout != null)
                    this.defaultLayout.Dispose();

                this.defaultLayout = value;
            }
        }

        private float ScaleZoomFactor
        {
            get
            {
                return (this.zoomLevel * this.activeLayout.Scaling);
            }
        }
        #endregion

        #region Public Methods
        public void AddDesignerMessageFilter(WorkflowDesignerMessageFilter designerMessageFilter)
        {
            if (designerMessageFilter == null)
                throw new ArgumentNullException("designerMessageFilter");

            if (Capture)
                Capture = false;

            this.customMessageFilters.Insert(0, designerMessageFilter);
            designerMessageFilter.SetParentView(this);
        }

        public void RemoveDesignerMessageFilter(WorkflowDesignerMessageFilter designerMessageFilter)
        {
            if (designerMessageFilter == null)
                throw new ArgumentNullException("designerMessageFilter");

            if (this.customMessageFilters.Contains(designerMessageFilter))
            {
                if (Capture)
                    Capture = false;

                this.customMessageFilters.Remove(designerMessageFilter);
                ((IDisposable)designerMessageFilter).Dispose();
            }
        }

        public void ShowInPlaceToolTip(string toolTipText, Rectangle toolTipRectangle)
        {
            if (toolTipText == null)
                throw new ArgumentNullException("toolTipText");

            if (toolTipRectangle.IsEmpty)
                throw new ArgumentException(SR.GetString(SR.Error_EmptyToolTipRectangle));

            this.workflowToolTip.SetText(toolTipText, toolTipRectangle);
        }

        public void ShowInfoTip(string text)
        {
            if (text == null)
                throw new ArgumentNullException("text");

            this.workflowToolTip.SetText(String.Empty, text);
        }

        public void ShowInfoTip(string title, string text)
        {
            if (title == null)
                throw new ArgumentNullException("title");

            if (text == null)
                throw new ArgumentNullException("text");

            this.workflowToolTip.SetText(title, text);
        }

        public void EnsureVisible(object selectableObject)
        {
            if (selectableObject == null)
                throw new ArgumentNullException("selectableObject");

            // make sure that all the parents are expanded
            Activity activity = selectableObject as Activity;
            while (activity != null)
            {
                ActivityDesigner activityDesigner = ActivityDesigner.GetDesigner(activity);
                CompositeActivityDesigner parentDesigner = activityDesigner.ParentDesigner;
                if (parentDesigner != null)
                {
                    if (activityDesigner != null)
                        parentDesigner.EnsureVisibleContainedDesigner(activityDesigner);
                    activity = parentDesigner.Activity;
                }
                else
                {
                    activity = null;
                }
            }

            //this is to handle the case when we call ensure visible of a scope which currently has 
            //activity from the secondary flow selected. instead we should always switch to the main flow
            activity = selectableObject as Activity;
            if (activity != null)
            {
                CompositeActivityDesigner compositeDesigner = ActivityDesigner.GetDesigner(activity) as CompositeActivityDesigner;
                if (compositeDesigner != null)
                    compositeDesigner.EnsureVisibleContainedDesigner(compositeDesigner);
            }

            PerformLayout(false);

            if (this.ensureVisibleEventHandler == null)
            {
                this.ensureVisibleEventHandler = new EventHandler(OnEnsureVisible);
                Idle += this.ensureVisibleEventHandler;
            }
        }

        public void PerformLayout(bool immediateUpdate)
        {
            if (immediateUpdate)
            {
                if (this.layoutEventHandler != null)
                {
                    Idle -= this.layoutEventHandler;
                    this.layoutEventHandler = null;
                }
                base.PerformLayout(); //invalidate rectangle really cares for the this.layoutEventHandler being null
            }
            else if (this.layoutEventHandler == null)
            {
                this.layoutEventHandler = new EventHandler(OnPerformLayout);
                Idle += this.layoutEventHandler;
            }
        }

        public void SaveViewState(Stream viewState)
        {
            if (viewState == null)
                throw new ArgumentNullException("viewState");

            IDesignerHost designerHost = (IDesignerHost)GetService(typeof(IDesignerHost));
            if (designerHost == null)
                throw new Exception(SR.GetString(SR.General_MissingService, typeof(IDesignerHost).FullName));

            BinaryWriter writer = new BinaryWriter(viewState);

            // write workflow properties
            writer.Write(this.PrintPreviewMode);
            writer.Write(this.Zoom);

            // write components
            DesignerHelpers.SerializeDesignerStates(designerHost, writer);

            // write scroll position
            writer.Write(this.ScrollPosition.X);
            writer.Write(this.ScrollPosition.Y);
        }

        public void LoadViewState(Stream viewState)
        {
            if (viewState == null)
                throw new ArgumentNullException("viewState");

            bool outdated = false;
            Point scrollPosition = new Point(0, 0);

            IDesignerHost designerHost = (IDesignerHost)GetService(typeof(IDesignerHost));
            if (designerHost == null)
                throw new Exception(SR.GetString(SR.General_MissingService, typeof(IDesignerHost).FullName));

            viewState.Position = 0;

            BinaryReader reader = new BinaryReader(viewState);

            // read workflow properties
            this.PrintPreviewMode = reader.ReadBoolean();
            this.Zoom = reader.ReadInt32();
            try
            {
                // get activities
                outdated = DesignerHelpers.DeserializeDesignerStates(designerHost, reader);

                // we will apply the scrolling only if if there is perfect match
                // between the components in the workflow and the persisted data.
                // It might be different if files were updated outside of VS, or if
                // VS crashes.
                if (!outdated)
                {
                    scrollPosition.X = reader.ReadInt32();
                    scrollPosition.Y = reader.ReadInt32();
                }
            }
            finally
            {
                // flush the layout to apply the new settings, this will set the scrollers extents
                base.PerformLayout();
                this.ScrollPosition = scrollPosition;
            }
        }

        /// <summary>
        /// Changes zoom level on the design surface such that the entire workflow is displayed in the view
        /// </summary>
        public void FitToScreenSize()
        {
            if (HScrollBar.Maximum > ViewPortSize.Width || VScrollBar.Maximum > ViewPortSize.Height)
            {
                int newZoom = (int)(100.0f / ActiveLayout.Scaling * Math.Min((float)ViewPortSize.Width / (float)ActiveLayout.Extent.Width, (float)ViewPortSize.Height / (float)ActiveLayout.Extent.Height));
                Zoom = Math.Min(Math.Max(newZoom, AmbientTheme.MinZoom), AmbientTheme.MaxZoom);
            }
        }

        /// <summary>
        /// Sets the zoom level to 100% so that the workflow size is restored to actial workflow size
        /// </summary>
        public void FitToWorkflowSize()
        {
            if (Zoom != 100)
                Zoom = 100;
        }

        /// <summary>
        /// Saves workflow as image to a file based on the format specified
        /// </summary>
        /// <param name="imageFile">Path to file where to save the image</param>
        /// <param name="imageFormat">Format in which to save the image</param>
        public void SaveWorkflowImage(string imageFile, ImageFormat imageFormat)
        {
            if (imageFile == null)
                throw new ArgumentNullException("imageFile");

            if (imageFormat == null)
                throw new ArgumentNullException("imageFormat");

            Bitmap workflowBitmap = TakeWorkflowSnapShot();
            if (workflowBitmap != null)
            {
                workflowBitmap.Save(imageFile, imageFormat);
                workflowBitmap.Dispose();
            }
        }

        /// <summary>
        /// Saves workflow as image to a stream based on encoding specified
        /// </summary>
        /// <param name="stream">Stream where to save the workflow</param>
        /// <param name="imageFormat">Format in which to save the image</param>
        public void SaveWorkflowImage(Stream stream, ImageFormat imageFormat)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            if (imageFormat == null)
                throw new ArgumentNullException("imageFormat");

            Bitmap workflowBitmap = TakeWorkflowSnapShot();
            if (workflowBitmap != null)
            {
                workflowBitmap.Save(stream, imageFormat);
                workflowBitmap.Dispose();
            }
        }

        /// <summary>
        /// Stores the workflow image to clipboard.
        /// </summary>
        public void SaveWorkflowImageToClipboard()
        {
            Bitmap workflowBitmap = TakeWorkflowSnapShot();
            if (workflowBitmap != null)
            {
                Clipboard.SetDataObject(workflowBitmap, true);
                workflowBitmap.Dispose();
            }
        }
        #endregion

        #region Protected Methods

        #region Overridden Methods handling UI events
        #region Drawing
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            //We set the highest quality interpolation so that we do not loose the image quality
            GraphicsContainer graphicsState = e.Graphics.BeginContainer();
            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            e.Graphics.CompositingQuality = CompositingQuality.HighQuality;

            bool takeWorkflowSnapShot = (this.viewPortBitmap == null || this.viewPortBitmap.Size != ViewPortSize);
            if (takeWorkflowSnapShot)
            {
                if (this.viewPortBitmap != null)
                    this.viewPortBitmap.Dispose();
                this.viewPortBitmap = new Bitmap(Math.Max(1, ViewPortSize.Width), Math.Max(1, ViewPortSize.Height), e.Graphics);
            }

            //Create viewport information and take the workflow snapshot before passing on the information to the active layout
            ViewPortData viewPortData = new ViewPortData();
            viewPortData.LogicalViewPort = ClientRectangleToLogical(new Rectangle(Point.Empty, ViewPortSize));
            viewPortData.MemoryBitmap = this.viewPortBitmap;
            viewPortData.Scaling = new SizeF(ScaleZoomFactor, ScaleZoomFactor);
            viewPortData.Translation = ScrollPosition;
            viewPortData.ShadowDepth = new Size(this.shadowDepth, this.shadowDepth);
            viewPortData.ViewPortSize = ViewPortSize;

            //capture the workflow onto in-memory bitmap
            if (this.layoutEventHandler == null || takeWorkflowSnapShot)
                WorkflowView.TakeWorkflowSnapShot(this, viewPortData);

            //copy workflow from the bitmap onto corresponding pages on the screen
            try
            {
                this.activeLayout.OnPaintWorkflow(e, viewPortData);
            }
            catch (Exception ex)
            {
                //If a layout throws an exception then we will not draw the layout
                //
                Debug.WriteLine(ex);
            }

            //If any of the message filters throws an exception we continue to draw
            using (WorkflowMessageDispatchData dispatchData = new WorkflowMessageDispatchData(this, EventArgs.Empty))
            {
                foreach (WorkflowDesignerMessageFilter filter in dispatchData.Filters)
                {
                    try
                    {
                        if (((IWorkflowDesignerMessageSink)filter).OnPaintWorkflowAdornments(e, ViewPortRectangle))
                            break;
                    }
                    catch (Exception ex)
                    {
                        //Ignore the filter throwing the exception and continue to function
                        Debug.WriteLine(ex);
                    }
                }
            }

            e.Graphics.EndContainer(graphicsState);

            e.Graphics.FillRectangle(SystemBrushes.Control, new Rectangle(Width - SystemInformation.VerticalScrollBarWidth, Height - SystemInformation.HorizontalScrollBarHeight, SystemInformation.VerticalScrollBarWidth, SystemInformation.HorizontalScrollBarHeight));
        }

        protected virtual void OnZoomChanged()
        {
            if (this.ZoomChanged != null)
                this.ZoomChanged(this, EventArgs.Empty);
        }

        protected virtual void OnRootDesignerChanged()
        {
            if (this.RootDesignerChanged != null)
                this.RootDesignerChanged(this, EventArgs.Empty);
        }
        #endregion

        #region Mouse Events
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            using (WorkflowMessageDispatchData dispatchData = new WorkflowMessageDispatchData(this, e))
            {
                foreach (WorkflowDesignerMessageFilter filter in dispatchData.Filters)
                {
                    if (((IWorkflowDesignerMessageSink)filter).OnMouseDown(e))
                        break;
                }
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            using (WorkflowMessageDispatchData dispatchData = new WorkflowMessageDispatchData(this, e))
            {
                foreach (WorkflowDesignerMessageFilter filter in dispatchData.Filters)
                {
                    if (((IWorkflowDesignerMessageSink)filter).OnMouseMove(e))
                        break;
                }
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            using (WorkflowMessageDispatchData dispatchData = new WorkflowMessageDispatchData(this, e))
            {
                foreach (WorkflowDesignerMessageFilter filter in dispatchData.Filters)
                {
                    if (((IWorkflowDesignerMessageSink)filter).OnMouseUp(e))
                        break;
                }
            }
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);

            using (WorkflowMessageDispatchData dispatchData = new WorkflowMessageDispatchData(this, e))
            {
                foreach (WorkflowDesignerMessageFilter filter in dispatchData.Filters)
                {
                    if (((IWorkflowDesignerMessageSink)filter).OnMouseDoubleClick(e))
                        break;
                }
            }
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);

            Point clientPoint = PointToClient(Control.MousePosition);
            MouseEventArgs eventArgs = new MouseEventArgs(Control.MouseButtons, 1, clientPoint.X, clientPoint.Y, 0);

            using (WorkflowMessageDispatchData dispatchData = new WorkflowMessageDispatchData(this, eventArgs))
            {
                foreach (WorkflowDesignerMessageFilter filter in dispatchData.Filters)
                {
                    if (((IWorkflowDesignerMessageSink)filter).OnMouseEnter(eventArgs))
                        break;
                }
            }
        }

        protected override void OnMouseHover(EventArgs e)
        {
            base.OnMouseHover(e);

            Point clientPoint = PointToClient(Control.MousePosition);
            MouseEventArgs eventArgs = new MouseEventArgs(Control.MouseButtons, 1, clientPoint.X, clientPoint.Y, 0);

            using (WorkflowMessageDispatchData dispatchData = new WorkflowMessageDispatchData(this, eventArgs))
            {
                foreach (WorkflowDesignerMessageFilter filter in dispatchData.Filters)
                {
                    if (((IWorkflowDesignerMessageSink)filter).OnMouseHover(eventArgs))
                        break;
                }
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            using (WorkflowMessageDispatchData dispatchData = new WorkflowMessageDispatchData(this, EventArgs.Empty))
            {
                foreach (WorkflowDesignerMessageFilter filter in dispatchData.Filters)
                {
                    if (((IWorkflowDesignerMessageSink)filter).OnMouseLeave())
                        break;
                }
            }
        }

        protected override void OnMouseCaptureChanged(EventArgs e)
        {
            base.OnMouseCaptureChanged(e);

            using (WorkflowMessageDispatchData dispatchData = new WorkflowMessageDispatchData(this, EventArgs.Empty))
            {
                foreach (WorkflowDesignerMessageFilter filter in dispatchData.Filters)
                {
                    if (((IWorkflowDesignerMessageSink)filter).OnMouseCaptureChanged())
                        break;
                }
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            using (WorkflowMessageDispatchData dispatchData = new WorkflowMessageDispatchData(this, e))
            {
                foreach (WorkflowDesignerMessageFilter filter in dispatchData.Filters)
                {
                    if (((IWorkflowDesignerMessageSink)filter).OnMouseWheel(e))
                        break;
                }
            }
        }
        #endregion

        #region Keyboard Events
        protected override void OnKeyDown(KeyEventArgs e)
        {
            using (WorkflowMessageDispatchData dispatchData = new WorkflowMessageDispatchData(this, e))
            {
                foreach (WorkflowDesignerMessageFilter filter in dispatchData.Filters)
                {
                    if (((IWorkflowDesignerMessageSink)filter).OnKeyDown(e))
                        break;
                }
            }

            if (!e.Handled)
                base.OnKeyDown(e);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            using (WorkflowMessageDispatchData dispatchData = new WorkflowMessageDispatchData(this, e))
            {
                foreach (WorkflowDesignerMessageFilter filter in dispatchData.Filters)
                {
                    if (((IWorkflowDesignerMessageSink)filter).OnKeyUp(e))
                        break;
                }
            }

            if (!e.Handled)
                base.OnKeyUp(e);
        }
        #endregion

        #region Layouting Events
        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);

            ScrollBar hScrollBar = HScrollBar;
            ScrollBar vScrollBar = VScrollBar;

            if (Controls.Contains(hScrollBar))
                hScrollBar.Bounds = new Rectangle(0, Math.Max(0, Height - SystemInformation.HorizontalScrollBarHeight), Math.Max(Width - ((vScrollBar.Visible) ? SystemInformation.VerticalScrollBarWidth : 0), 0), SystemInformation.HorizontalScrollBarHeight);

            if (Controls.Contains(vScrollBar))
                vScrollBar.Bounds = new Rectangle(Math.Max(0, Width - SystemInformation.VerticalScrollBarWidth), 0, SystemInformation.VerticalScrollBarWidth, Math.Max(Height - ((hScrollBar.Visible) ? SystemInformation.HorizontalScrollBarHeight : 0), 0));

            if (this.toolContainer != null)
            {
                this.toolContainer.Location = new Point(Width - this.toolContainer.Width, 0);
                this.toolContainer.Height = Height - ((hScrollBar.Visible) ? hScrollBar.Height : 0);
            }

            using (WorkflowMessageDispatchData dispatchData = new WorkflowMessageDispatchData(this, levent))
            {
                foreach (WorkflowDesignerMessageFilter filter in dispatchData.Filters)
                    ((IWorkflowDesignerMessageSink)filter).OnLayout(levent);
            }

            //Layout the designers
            using (Graphics graphics = CreateGraphics())
            {
                this.activeLayout.Update(graphics, WorkflowLayout.LayoutUpdateReason.LayoutChanged);

                if (this.rootDesigner != null)
                    this.rootDesigner.Location = this.activeLayout.RootDesignerAlignment;
            }

            //Update the scroll range and redraw
            UpdateScrollRange();
            InvalidateClientRectangle(Rectangle.Empty);
        }
        #endregion

        #region DragDrop Events
        protected override void OnDragEnter(DragEventArgs dragEventArgs)
        {
            base.OnDragEnter(dragEventArgs);

            this.dragDropInProgress = true;

            using (WorkflowMessageDispatchData dispatchData = new WorkflowMessageDispatchData(this, dragEventArgs))
            {
                foreach (WorkflowDesignerMessageFilter filter in dispatchData.Filters)
                {
                    if (((IWorkflowDesignerMessageSink)filter).OnDragEnter(dragEventArgs))
                        break;
                }
            }
        }

        protected override void OnDragOver(DragEventArgs dragEventArgs)
        {
            base.OnDragOver(dragEventArgs);

            using (WorkflowMessageDispatchData dispatchData = new WorkflowMessageDispatchData(this, dragEventArgs))
            {
                foreach (WorkflowDesignerMessageFilter filter in dispatchData.Filters)
                {
                    if (((IWorkflowDesignerMessageSink)filter).OnDragOver(dragEventArgs))
                        break;
                }
            }
        }

        protected override void OnDragLeave(EventArgs e)
        {
            base.OnDragLeave(e);

            using (WorkflowMessageDispatchData dispatchData = new WorkflowMessageDispatchData(this, EventArgs.Empty))
            {
                foreach (WorkflowDesignerMessageFilter filter in dispatchData.Filters)
                {
                    if (((IWorkflowDesignerMessageSink)filter).OnDragLeave())
                        break;
                }
            }

            this.dragDropInProgress = false;
        }

        protected override void OnDragDrop(DragEventArgs dragEventArgs)
        {
            base.OnDragDrop(dragEventArgs);

            using (WorkflowMessageDispatchData dispatchData = new WorkflowMessageDispatchData(this, dragEventArgs))
            {
                foreach (WorkflowDesignerMessageFilter filter in dispatchData.Filters)
                {
                    if (((IWorkflowDesignerMessageSink)filter).OnDragDrop(dragEventArgs))
                        break;
                }
            }

            this.dragDropInProgress = false;
        }

        protected override void OnGiveFeedback(GiveFeedbackEventArgs gfbevent)
        {
            base.OnGiveFeedback(gfbevent);

            using (WorkflowMessageDispatchData dispatchData = new WorkflowMessageDispatchData(this, gfbevent))
            {
                foreach (WorkflowDesignerMessageFilter filter in dispatchData.Filters)
                {
                    if (((IWorkflowDesignerMessageSink)filter).OnGiveFeedback(gfbevent))
                        break;
                }
            }
        }

        protected override void OnQueryContinueDrag(QueryContinueDragEventArgs qcdevent)
        {
            base.OnQueryContinueDrag(qcdevent);

            using (WorkflowMessageDispatchData dispatchData = new WorkflowMessageDispatchData(this, qcdevent))
            {
                foreach (WorkflowDesignerMessageFilter filter in dispatchData.Filters)
                {
                    if (((IWorkflowDesignerMessageSink)filter).OnQueryContinueDrag(qcdevent))
                        break;
                }
            }
        }
        #endregion

        #region General Events
        //Handle context menus here reason being it can come from mouse r button click 
        //or shift+F10, or there might be other keys too 
        //We need to handle the WndProc and not the OnNotifyMessage because we need to set
        //the m.Result to handled (IntPtr.Zero) and dont let the base class see the message at all
        //see WinOE #787 "The keyboard "key" to launch the context menu launches the menu at 0,0"
        [UIPermission(SecurityAction.Assert, Window = UIPermissionWindow.AllWindows)]
        [SuppressMessage("Microsoft.Security", "CA2106", Justification = "This is SecurityCritical, therefore not callable from partial trust code.")]
        protected override void WndProc(ref Message m)
        {
            using (WorkflowMessageDispatchData dispatchData = new WorkflowMessageDispatchData(this, EventArgs.Empty))
            {
                foreach (WorkflowDesignerMessageFilter filter in dispatchData.Filters)
                {
                    if (((IWorkflowDesignerMessageSink)filter).ProcessMessage(m))
                        break;
                }

                const int WM_CONTEXTMENU = 0x007B;
                if (m.Msg == WM_CONTEXTMENU)
                {
                    int LParam = (int)m.LParam;
                    Point location = (LParam != -1) ? new Point(LParam) : Control.MousePosition;

                    foreach (WorkflowDesignerMessageFilter filter in dispatchData.Filters)
                    {
                        if (((IWorkflowDesignerMessageSink)filter).OnShowContextMenu(location))
                            break;
                    }

                    //mark the message handled
                    m.Result = IntPtr.Zero;
                    //dont pass the message to the base but return immediatly
                    return;
                }
            }

            if (this.workflowToolTip != null && m.Msg == NativeMethods.WM_NOTIFY)
                this.workflowToolTip.RelayParentNotify(ref m);

            try
            {
                if (m.Result == IntPtr.Zero)
                    base.WndProc(ref m);
            }
            catch (Exception e)
            {
                if (e != CheckoutException.Canceled)
                    DesignerHelpers.ShowError(this, e);
            }
        }

        protected override void OnControlAdded(ControlEventArgs e)
        {
            if (e.Control != VScrollBar && e.Control != HScrollBar && e.Control != this.toolContainer)
                throw new InvalidOperationException(SR.GetString(SR.Error_InsertingChildControls));
        }

        protected override AccessibleObject CreateAccessibilityInstance()
        {
            return new WorkflowViewAccessibleObject(this);
        }
        #endregion

        #endregion

        #endregion

        #region Private Methods
        private void OnWorkflowIdle(object sender, EventArgs e)
        {
            if (this.idleEventListeners != null)
                this.idleEventListeners(this, e);
        }

        private void UpdateLayout()
        {
            if (this.layoutEventHandler != null)
            {
                PerformLayout(true);
                InvalidateClientRectangle(Rectangle.Empty);
            }
        }

        internal void OnCommandKey(KeyEventArgs e)
        {
            this.OnKeyDown(e);
            this.OnKeyUp(e);
        }

        private void OnSelectionChanged(object sender, EventArgs e)
        {
            if (this.commandSet != null)
                this.commandSet.UpdateCommandSet();

            //Make sure that the ensure visible also works when the component is selected
            //from property browser dropdown
            //Make sure that when there is a selection change using the property browser
            //drop down we make sure that the designer associated with component selected by the user in the dropdown
            //is made visible. 
            //To enable this functionality please note that selection change is not a good event as it will get
            //fired in multiple cases, instead we should add a event in extended ui service which will do this and move
            //the following code in the event handler of that event
            //Ref Bug#3925

            if (RootDesigner != null && RootDesigner.Activity != null)
            {
                ISelectionService selectionService = GetService(typeof(ISelectionService)) as ISelectionService;
                if (selectionService != null && selectionService.GetComponentSelected(RootDesigner.Activity))
                {
                    IHelpService helpService = GetService(typeof(IHelpService)) as IHelpService;
                    if (helpService != null)
                        helpService.AddContextAttribute("Keyword", RootDesigner.Activity.GetType().FullName, HelpKeywordType.F1Keyword);
                }
            }
        }

        private void OnPerformLayout(object sender, EventArgs e)
        {
            if (this.layoutEventHandler != null)
            {
                Idle -= this.layoutEventHandler;
                this.layoutEventHandler = null;

                base.PerformLayout();
            }
        }

        //Gets the snapshot of the entire workflow
        private Bitmap TakeWorkflowSnapShot()
        {
            Bitmap bitmap = null;
            ActivityDesigner rootDesigner = RootDesigner;
            if (rootDesigner != null)
            {
                using (Graphics graphics = CreateGraphics())
                {
                    ViewPortData viewPortData = new ViewPortData();
                    viewPortData.LogicalViewPort = new Rectangle(Point.Empty, new Size(rootDesigner.Bounds.Width + 2 * DefaultWorkflowLayout.Separator.Width, rootDesigner.Bounds.Height + 2 * DefaultWorkflowLayout.Separator.Height));
                    viewPortData.MemoryBitmap = new Bitmap(viewPortData.LogicalViewPort.Width, viewPortData.LogicalViewPort.Height, graphics);
                    viewPortData.Scaling = new SizeF(1, 1);
                    viewPortData.Translation = Point.Empty;
                    viewPortData.ShadowDepth = new Size(0, 0);
                    viewPortData.ViewPortSize = viewPortData.LogicalViewPort.Size;
                    TakeWorkflowSnapShot(this, viewPortData);
                    bitmap = viewPortData.MemoryBitmap;
                }
            }

            return bitmap;
        }

        //This function will give snapshot of what is drawn on the screen at any point of time
        //It will scale and translate the designers and drawing based on the viewport data
        //We need this function in OnPaint and taking snapshot of magnifier bitmap
        //At the end of this function; the ViewPortData.MemoryBitmap will contain the bitmap of the 
        //workflow to be drawn as per layout
        internal static void TakeWorkflowSnapShot(WorkflowView workflowView, ViewPortData viewPortData)
        {
            //Get the drawing canvas
            Bitmap memoryBitmap = viewPortData.MemoryBitmap;
            Debug.Assert(memoryBitmap != null);

            using (Graphics viewPortGraphics = Graphics.FromImage(memoryBitmap))
            {
                //We set the highest quality interpolation so that we do not loose the image quality
                viewPortGraphics.SmoothingMode = SmoothingMode.HighQuality;
                viewPortGraphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

                using (PaintEventArgs eventArgs = new PaintEventArgs(viewPortGraphics, viewPortData.LogicalViewPort))
                {
                    workflowView.ActiveLayout.OnPaint(eventArgs, viewPortData);
                }

                //Create the scaling matrix 
                Matrix transformationMatrix = new Matrix();
                transformationMatrix.Scale(viewPortData.Scaling.Width, viewPortData.Scaling.Height, MatrixOrder.Prepend);

                //When we draw on the viewport we draw in scaled and translated. 
                //So that we minimize the calls to DrawImage
                //Make sure that we scale down the logical view port origin in order to take care of scaling factor
                //Before we select the transform factor we make sure that logicalviewport origin is scaled down
                Point[] logicalViewPortOrigin = new Point[] { viewPortData.LogicalViewPort.Location };
                transformationMatrix.TransformPoints(logicalViewPortOrigin);

                //For performance improvement and to eliminate one extra DrawImage...we draw the designers on the viewport
                //bitmap with visual depth consideration
                transformationMatrix.Translate(-logicalViewPortOrigin[0].X + viewPortData.ShadowDepth.Width, -logicalViewPortOrigin[0].Y + viewPortData.ShadowDepth.Height, MatrixOrder.Append);

                //Select the transform into viewport graphics.
                //Viewport bitmap has the scaled and translated designers which we then map to
                //the actual graphics based on page layout
                viewPortGraphics.Transform = transformationMatrix;

                //Draw the designers on bitmap
                if (workflowView.RootDesigner != null)
                {
                    using (Region clipRegion = new Region())
                    using (GraphicsPath designerPath = ActivityDesignerPaint.GetDesignerPath(workflowView.RootDesigner, false))
                    {
                        Region oldRegion = viewPortGraphics.Clip;

                        //First draw the grid and rectangle with the designer clip region
                        clipRegion.MakeEmpty();
                        clipRegion.Union(designerPath);
                        viewPortGraphics.Clip = clipRegion;
                        AmbientTheme ambientTheme = WorkflowTheme.CurrentTheme.AmbientTheme;
                        viewPortGraphics.FillRectangle(ambientTheme.BackgroundBrush, workflowView.RootDesigner.Bounds);
                        if (ambientTheme.ShowGrid)
                            ActivityDesignerPaint.DrawGrid(viewPortGraphics, workflowView.RootDesigner.Bounds);
                        viewPortGraphics.Clip = oldRegion;

                        //Then draw the root with clip region extended
                        try
                        {
                            using (PaintEventArgs paintEventArgs = new PaintEventArgs(viewPortGraphics, viewPortData.LogicalViewPort))
                            {
                                ((IWorkflowDesignerMessageSink)workflowView.RootDesigner).OnPaint(paintEventArgs, viewPortData.LogicalViewPort);
                            }
                        }
                        catch (Exception e)
                        {
                            //Eat the exception thrown in draw
                            Debug.WriteLine(e);
                        }
                    }
                }

                //Draw all the filters


                using (PaintEventArgs paintArgs = new PaintEventArgs(viewPortGraphics, workflowView.RootDesigner.Bounds))
                {
                    using (WorkflowMessageDispatchData dispatchData = new WorkflowMessageDispatchData(workflowView, EventArgs.Empty))
                    {
                        foreach (WorkflowDesignerMessageFilter filter in dispatchData.Filters)
                        {
                            try
                            {
                                if (((IWorkflowDesignerMessageSink)filter).OnPaint(paintArgs, viewPortData.LogicalViewPort))
                                    break;
                            }
                            catch (Exception e)
                            {
                                Debug.WriteLine(e);
                            }
                        }
                    }
                }

                viewPortGraphics.Transform = new Matrix();

                //Now that we have a bitmap which is bit offseted based visual depth we need to take copy of it
                //This is done so as to avoid expensive DrawImage call, what I am assuming here is that time it 
                //will take to create a new bitmap from an existing one is less expensive in terms of speed than space
                //As you just need to copy bitmap bits in memory than to perform expesive Image Drawing operation
                if (!viewPortData.ShadowDepth.IsEmpty)
                {
                    Bitmap temporaryBitmap = new Bitmap(memoryBitmap);

                    //THEMETODO: WE JUST NEED TO GRAYSCALE THIS, RATHER THAN DRAWING A SHADOW
                    //Now that we have taken a copy we will draw over the existing bitmap so that we can make it as shadow bitmap
                    using (Brush shadowDepthBrush = new SolidBrush(Color.FromArgb(220, Color.White)))
                        viewPortGraphics.FillRectangle(shadowDepthBrush, new Rectangle(Point.Empty, new Size(memoryBitmap.Size.Width - viewPortData.ShadowDepth.Width - 1, memoryBitmap.Size.Height - viewPortData.ShadowDepth.Height - 1)));

                    //Now make sure that we draw the image from the temporary bitmap with white color set as transparent 
                    //so that we achive the 3D effect
                    //Make sure that we take into consideration the transparency key
                    ImageAttributes transparentColorKey = new ImageAttributes();
                    transparentColorKey.SetColorKey(viewPortData.TransparentColor, viewPortData.TransparentColor, ColorAdjustType.Default);
                    transparentColorKey.SetColorKey(viewPortData.TransparentColor, viewPortData.TransparentColor, ColorAdjustType.Bitmap);
                    viewPortGraphics.DrawImage(temporaryBitmap, new Rectangle(-viewPortData.ShadowDepth.Width, -viewPortData.ShadowDepth.Height, memoryBitmap.Width, memoryBitmap.Height), 0, 0, memoryBitmap.Width, memoryBitmap.Height, GraphicsUnit.Pixel, transparentColorKey);

                    //Now dispose the temporary bitmap
                    temporaryBitmap.Dispose();
                }
            }
        }

        internal void OnThemeChange(object sender, EventArgs e)
        {
            ShadowDepth = WorkflowTheme.CurrentTheme.AmbientTheme.ShadowDepth;

            using (WorkflowMessageDispatchData dispatchData = new WorkflowMessageDispatchData(this, EventArgs.Empty))
            {
                foreach (WorkflowDesignerMessageFilter filter in dispatchData.Filters)
                {
                    try
                    {
                        ((IWorkflowDesignerMessageSink)filter).OnThemeChange();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                    }
                }
            }

            base.PerformLayout();
        }

        private void OnEnsureVisible(object sender, EventArgs e)
        {
            if (this.ensureVisibleEventHandler != null)
            {
                Idle -= this.ensureVisibleEventHandler;
                this.ensureVisibleEventHandler = null;
            }

            ISelectionService selectionService = (ISelectionService)GetService(typeof(ISelectionService));
            if (selectionService != null && selectionService.SelectionCount > 0)
            {
                //We do not want to regenerate a layout event in ensure visible 
                ArrayList selectedComponents = new ArrayList(selectionService.GetSelectedComponents());
                for (int i = selectedComponents.Count - 1; i >= 0; i--)
                {
                    Rectangle rectangleToMakeVisible = Rectangle.Empty;
                    if (selectedComponents[i] is Activity)
                    {
                        ActivityDesigner activityDesigner = ActivityDesigner.GetDesigner(selectedComponents[i] as Activity);
                        if (activityDesigner != null)
                        {
                            rectangleToMakeVisible = activityDesigner.Bounds;
                            rectangleToMakeVisible.Inflate(WorkflowTheme.CurrentTheme.AmbientTheme.SelectionSize);
                            rectangleToMakeVisible.Inflate(WorkflowTheme.CurrentTheme.AmbientTheme.SelectionSize);
                        }
                    }
                    else if (selectedComponents[i] is HitTestInfo)
                    {
                        rectangleToMakeVisible = ((HitTestInfo)selectedComponents[i]).Bounds;
                    }

                    if (!rectangleToMakeVisible.IsEmpty)
                        EnsureVisible(rectangleToMakeVisible);
                }
            }
        }

        private void EnsureVisible(Rectangle rect)
        {
            Rectangle clientRectangle = ClientRectangleToLogical(new Rectangle(Point.Empty, ViewPortSize));

            if (!clientRectangle.Contains(rect.Location) || !clientRectangle.Contains(new Point(rect.Right, rect.Bottom)))
            {
                Size scrollDelta = new Size();
                if (!clientRectangle.Contains(new Point(rect.Left, clientRectangle.Top)) || !clientRectangle.Contains(new Point(rect.Right, clientRectangle.Top)))
                {
                    if (rect.Width > clientRectangle.Width)
                        scrollDelta.Width = (rect.Left + rect.Width / 2) - (clientRectangle.Left + clientRectangle.Width / 2);
                    else if (rect.Left < clientRectangle.Left)
                        scrollDelta.Width = (rect.Left - clientRectangle.Left);
                    else
                        scrollDelta.Width = (rect.Right - clientRectangle.Right);
                }

                if (!clientRectangle.Contains(new Point(clientRectangle.Left, rect.Top)) || !clientRectangle.Contains(new Point(clientRectangle.Left, rect.Bottom)))
                {
                    if ((rect.Top < clientRectangle.Top) || (rect.Height > clientRectangle.Height))
                        scrollDelta.Height = (rect.Top - clientRectangle.Top);
                    else
                        scrollDelta.Height = rect.Bottom - clientRectangle.Bottom;
                }

                scrollDelta = LogicalSizeToClient(scrollDelta);
                Point scrollPosition = ScrollPosition;
                ScrollPosition = new Point(scrollPosition.X + scrollDelta.Width, scrollPosition.Y + scrollDelta.Height);
            }
        }

        private void OnScroll(object sender, EventArgs e)
        {
            //Lets speedup the scrolling logic
            InvalidateClientRectangle(Rectangle.Empty);

            ScrollBar scrollBar = sender as ScrollBar;
            if (scrollBar != null)
            {
                using (WorkflowMessageDispatchData dispatchData = new WorkflowMessageDispatchData(this, e))
                {
                    foreach (WorkflowDesignerMessageFilter filter in dispatchData.Filters)
                    {
                        try
                        {
                            ((IWorkflowDesignerMessageSink)filter).OnScroll(scrollBar, scrollBar.Value);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex);
                        }
                    }
                }
            }
        }

        private void UpdateScrollRange()
        {
            if (ViewPortSize.Width < 0 || ViewPortSize.Height < 0)
                return;

            Size currentSize = ViewPortSize;
            Size maximumScrollSize = LogicalSizeToClient(this.activeLayout.Extent);
            Size largeChangeSize = new Size(Math.Min(maximumScrollSize.Width, currentSize.Width), Math.Min(maximumScrollSize.Height, currentSize.Height));

            if (hScrollBar.Maximum != maximumScrollSize.Width)
                hScrollBar.Maximum = maximumScrollSize.Width;
            if (vScrollBar.Maximum != maximumScrollSize.Height)
                vScrollBar.Maximum = maximumScrollSize.Height;

            if (hScrollBar.LargeChange != largeChangeSize.Width)
            {
                hScrollBar.SmallChange = largeChangeSize.Width / 15;
                hScrollBar.LargeChange = largeChangeSize.Width + 1;
            }
            if (vScrollBar.LargeChange != largeChangeSize.Height)
            {
                vScrollBar.SmallChange = largeChangeSize.Height / 15;
                vScrollBar.LargeChange = largeChangeSize.Height + 1;
            }

            int xMaxScrollPos = maximumScrollSize.Width - hScrollBar.LargeChange;
            xMaxScrollPos = (xMaxScrollPos < 0) ? 0 : xMaxScrollPos;
            if (hScrollBar.Value > xMaxScrollPos)
                hScrollBar.Value = xMaxScrollPos;

            int yMaxScrollPos = maximumScrollSize.Height - vScrollBar.LargeChange;
            yMaxScrollPos = (yMaxScrollPos < 0) ? 0 : yMaxScrollPos;
            if (vScrollBar.Value > yMaxScrollPos)
                vScrollBar.Value = yMaxScrollPos;

            RefreshDynamicAction();

            bool hScrollBarVisible = hScrollBar.Visible;
            if (Controls.Contains(hScrollBar))
                hScrollBar.Visible = (hScrollBar.Maximum > currentSize.Width);

            bool vScrollBarVisible = vScrollBar.Visible;
            if (Controls.Contains(vScrollBar))
                vScrollBar.Visible = (vScrollBar.Maximum > currentSize.Height);

            if (hScrollBarVisible != hScrollBar.Visible || vScrollBar.Visible != vScrollBarVisible)
            {
                base.PerformLayout();
                Refresh();
            }
        }

        private DynamicAction CreateDynamicAction()
        {
            DynamicAction fitAllAction = new DynamicAction();
            fitAllAction.ButtonSize = DynamicAction.ButtonSizes.Large;
            fitAllAction.DockAlignment = DesignerContentAlignment.BottomRight;
            fitAllAction.DockMargin = new Size(5, 5);

            ActionButton fitallButton = new ActionButton(new Image[] { DR.GetImage(DR.FitToScreen) as Bitmap });
            fitallButton.StateChanged += new EventHandler(OnFitToScreen);
            fitAllAction.Buttons.Add(fitallButton);

            return fitAllAction;
        }

        private void RefreshDynamicAction()
        {
            DynamicActionMessageFilter dynamicActionFilter = GetService(typeof(DynamicActionMessageFilter)) as DynamicActionMessageFilter;
            if (dynamicActionFilter == null || this.fitAllAction == null)
                return;

            if (HScrollBar.Maximum > ViewPortSize.Width || VScrollBar.Maximum > ViewPortSize.Height)
            {
                //This means we need to show the zoomin icon
                this.fitAllAction.Buttons[0].Description = DR.GetString(DR.FitToScreenDescription);
                this.fitAllAction.Buttons[0].StateImages = new Bitmap[] { DR.GetImage(DR.FitToScreen) as Bitmap };
                dynamicActionFilter.AddAction(this.fitAllAction);
            }
            else if (Zoom != 100)
            {
                //We need to show zoomout icon
                this.fitAllAction.Buttons[0].Description = DR.GetString(DR.FitToWorkflowDescription);
                this.fitAllAction.Buttons[0].StateImages = new Bitmap[] { DR.GetImage(DR.FitToWorkflow) as Bitmap };
                dynamicActionFilter.AddAction(this.fitAllAction);
            }
            else
            {
                //In neither case we remove the action
                dynamicActionFilter.RemoveAction(this.fitAllAction);
                this.fitAllAction.Buttons[0].State = ActionButton.States.Normal;
            }
        }

        private void OnFitToScreen(object sender, EventArgs e)
        {
            ActionButton fitallButton = sender as ActionButton;
            if (fitallButton == null || fitallButton.State != ActionButton.States.Pressed)
                return;

            if (HScrollBar.Maximum > ViewPortSize.Width || VScrollBar.Maximum > ViewPortSize.Height)
                FitToScreenSize();
            else if (Zoom != 100)
                FitToWorkflowSize();
        }

        private void OnTabChange(object sender, TabSelectionChangeEventArgs e)
        {
            if (e.CurrentItem.Identifier == (int)TabButtonIds.MultiPage ||
                    e.CurrentItem.Identifier == (int)TabButtonIds.Zoom ||
                    e.CurrentItem.Identifier == (int)TabButtonIds.Pan)
            {
                Rectangle buttonRect = e.SelectedTabBounds;
                CommandID menuID = null;

                if (e.CurrentItem.Identifier == (int)TabButtonIds.MultiPage)
                    menuID = WorkflowMenuCommands.PageLayoutMenu;
                else if (e.CurrentItem.Identifier == (int)TabButtonIds.Zoom)
                    menuID = WorkflowMenuCommands.ZoomMenu;
                else
                    menuID = WorkflowMenuCommands.PanMenu;

                IMenuCommandService menuCommandService = (IMenuCommandService)GetService(typeof(IMenuCommandService));
                if (menuCommandService != null)
                    menuCommandService.ShowContextMenu(menuID, buttonRect.Right, buttonRect.Top);
            }
        }

        private void EnsureScrollBars(HScrollBar newHorizScrollBar, VScrollBar newVertScrollBar)
        {
            try
            {
                SuspendLayout();

                if (this.hScrollBar != newHorizScrollBar)
                {
                    if (this.hScrollBar != null)
                    {
                        this.hScrollBar.ValueChanged -= new EventHandler(OnScroll);
                        if (Controls.Contains(this.hScrollBar))
                            Controls.Remove(this.hScrollBar);
                    }

                    this.hScrollBar = newHorizScrollBar;
                    if (this.hScrollBar.Parent == null)
                    {
                        this.hScrollBar.TabStop = false;
                        Controls.Add(this.hScrollBar);
                    }
                }

                if (this.vScrollBar != newVertScrollBar)
                {
                    if (this.vScrollBar != null)
                    {
                        this.vScrollBar.ValueChanged -= new EventHandler(OnScroll);
                        if (Controls.Contains(this.vScrollBar))
                            Controls.Remove(this.vScrollBar);
                    }

                    this.vScrollBar = newVertScrollBar;
                    if (this.vScrollBar.Parent == null)
                    {
                        this.vScrollBar.TabStop = false;
                        Controls.Add(this.vScrollBar);
                    }
                }

                this.hScrollBar.ValueChanged += new EventHandler(OnScroll);
                this.vScrollBar.ValueChanged += new EventHandler(OnScroll);
            }
            finally
            {
                ResumeLayout(true);
            }
        }

        private void PopulateMessageFilters(bool stockFilters)
        {
            IList<WorkflowDesignerMessageFilter> filters = (stockFilters) ? this.stockMessageFilters : this.customMessageFilters;
            Debug.Assert(filters.Count == 0);

            if (stockFilters)
            {
                filters.Add(new GlyphManager());
                filters.Add(new WindowManager());
            }
            else
            {
                Debug.Assert(this.rootDesigner != null);

                if (Capture)
                    Capture = false;

                IList customFilters = ((IWorkflowRootDesigner)this.rootDesigner).MessageFilters;
                foreach (WorkflowDesignerMessageFilter filter in customFilters)
                    filters.Add(filter);
            }

            foreach (WorkflowDesignerMessageFilter filter in filters)
                filter.SetParentView(this);
        }

        private void DisposeMessageFilters(bool stockFilters)
        {
            List<WorkflowDesignerMessageFilter> filters = (stockFilters) ? this.stockMessageFilters : this.customMessageFilters;

            //We dispose all the message filters, this is done by copying because some of the 
            //message filters might remove other dependent messagefilters
            ArrayList clonedFilterList = new ArrayList(filters.ToArray());
            foreach (WorkflowDesignerMessageFilter filter in clonedFilterList)
                ((IDisposable)filter).Dispose();
            filters.Clear();
        }
        #endregion

        #region Coordinate Transformation Functions
        public void InvalidateClientRectangle(Rectangle clientRectangle)
        {
            if (this.layoutEventHandler == null)
            {
                if (!clientRectangle.IsEmpty)
                {
                    //Inflate the invalidated rectangle. When zoom factor is less than 1; there is a loss of precision
                    clientRectangle.Inflate(1, 1);
                    base.Invalidate(clientRectangle);
                }
                else
                {
                    base.Invalidate();
                }
            }
        }

        public void InvalidateLogicalRectangle(Rectangle logicalRectangle)
        {
            InvalidateClientRectangle(LogicalRectangleToClient(logicalRectangle));
        }

        public Point LogicalPointToScreen(Point logicalPoint)
        {
            return PointToScreen(LogicalPointToClient(logicalPoint));
        }

        public Point ScreenPointToLogical(Point screenPoint)
        {
            return ClientPointToLogical(PointToClient(screenPoint));
        }

        public Point LogicalPointToClient(Point logicalPoint)
        {
            return LogicalPointToClient(logicalPoint, true);
        }

        public Point ClientPointToLogical(Point clientPoint)
        {
            return ClientPointToLogical(clientPoint, true);
        }

        public Size LogicalSizeToClient(Size logicalSize)
        {
            Point[] points = new Point[] { new Point(logicalSize) };

            //Scale the point
            Matrix scalingMatrix = new Matrix();
            scalingMatrix.Scale(ScaleZoomFactor, ScaleZoomFactor);
            scalingMatrix.TransformPoints(points);
            return new Size(points[0]);
        }

        public Size ClientSizeToLogical(Size clientSize)
        {
            //Scale the size, size scaling does not require translate
            Point[] points = new Point[] { new Point(clientSize) };
            Matrix scalingMatrix = new Matrix();
            scalingMatrix.Scale(ScaleZoomFactor, ScaleZoomFactor);
            scalingMatrix.Invert();
            scalingMatrix.TransformPoints(points);
            scalingMatrix.Invert();
            return new Size(points[0]);
        }

        public Rectangle LogicalRectangleToClient(Rectangle rectangle)
        {
            Debug.Assert(this.activeLayout != null, "active layout should not be null");
            Rectangle clientViewPort = (this.activeLayout != null) ? this.activeLayout.MapOutRectangleFromLayout(rectangle) : rectangle;
            //


            return new Rectangle(LogicalPointToClient(clientViewPort.Location, false), LogicalSizeToClient(clientViewPort.Size));
        }

        public Rectangle ClientRectangleToLogical(Rectangle rectangle)
        {
            //We translate the client viewport to logical view port. 
            //To do this we first get the view port rectangle scale it down
            //then translate it to area of page we would be viewing
            Rectangle scaledLogicalViewPort = new Rectangle(ClientPointToLogical(rectangle.Location, false), ClientSizeToLogical(rectangle.Size));
            return this.activeLayout.MapInRectangleToLayout(scaledLogicalViewPort);
        }

        internal bool IsClientPointInActiveLayout(Point clientPoint)
        {
            Point logicalPoint = ClientPointToLogical(clientPoint, false);
            return this.activeLayout.IsCoOrdInLayout(logicalPoint);
        }

        /*
        * Client scale is when we transform the coordinate based on zoom and translate it based on scrolling position and layout
        * Logical scale is when we transform the coordinate and map it to a flat coordinate system which goes from 0,0 to m,n
        * We also consider the ActiveLayout to transform the coordinates.
        */
        private Point LogicalPointToClient(Point point, bool mapToLayout)
        {
            if (mapToLayout)
                point = this.activeLayout.MapOutCoOrdFromLayout(point);

            //Scale the point
            Matrix scalingMatrix = new Matrix();
            scalingMatrix.Scale(ScaleZoomFactor, ScaleZoomFactor);
            Point[] points = new Point[] { point };
            scalingMatrix.TransformPoints(points);

            //Translate the point
            Matrix translateMatrix = new Matrix();
            translateMatrix.Translate(-ScrollPosition.X, -ScrollPosition.Y);
            translateMatrix.TransformPoints(points);
            return points[0];
        }

        private Point ClientPointToLogical(Point point, bool mapToLayout)
        {
            Point[] points = new Point[] { point };

            //Translate the point
            Matrix translateMatrix = new Matrix();
            translateMatrix.Translate(ScrollPosition.X, ScrollPosition.Y);
            translateMatrix.TransformPoints(points);

            //Scale down the point
            Matrix scalingMatrix = new Matrix();
            scalingMatrix.Scale(ScaleZoomFactor, ScaleZoomFactor);
            scalingMatrix.Invert();
            scalingMatrix.TransformPoints(points);
            scalingMatrix.Invert();
            if (!mapToLayout)
                return points[0];
            else
                return this.activeLayout.MapInCoOrdToLayout(points[0]);
        }
        #endregion

        #region IServiceProvider Implemetation
        object IServiceProvider.GetService(Type serviceType)
        {
            return GetService(serviceType);
        }

        protected override object GetService(Type serviceType)
        {
            object retVal = null;

            if (serviceType == typeof(CommandID))
                retVal = new CommandID(new Guid("5f1c3c8d-60f1-4b98-b85b-8679f97e8eac"), 0);
            else
                retVal = this.serviceProvider.GetService(serviceType);

            return retVal;
        }
        #endregion

        #region Class WorkflowMessageDispatchData
        private sealed class WorkflowMessageDispatchData : IDisposable
        {
            private WorkflowView workflowView;
            private HitTestInfo messageContext = null;

            public WorkflowMessageDispatchData(WorkflowView workflowView, EventArgs e)
            {
                this.workflowView = workflowView;

                if (this.workflowView.RootDesigner != null && this.workflowView.stockMessageFilters.Count > 0)
                {
                    Point clientPoint = Point.Empty;
                    if (e is MouseEventArgs || e is DragEventArgs)
                    {
                        if (e is MouseEventArgs)
                        {
                            clientPoint = new Point(((MouseEventArgs)e).X, ((MouseEventArgs)e).Y);
                        }
                        else if (e is DragEventArgs)
                        {
                            clientPoint = this.workflowView.PointToClient(new Point(((DragEventArgs)e).X, ((DragEventArgs)e).Y));
                            this.workflowView.UpdateLayout();
                        }

                        Point logicalPoint = this.workflowView.ClientPointToLogical(clientPoint);
                        HitTestInfo hitTestInfo = this.workflowView.RootDesigner.HitTest(logicalPoint);
                        this.messageContext = (hitTestInfo != null) ? hitTestInfo : HitTestInfo.Nowhere;
                        this.workflowView.messageHitTestContexts.Push(this.messageContext);
                    }
                }
            }

            void IDisposable.Dispose()
            {
                if (this.workflowView != null && this.messageContext != null)
                {
                    HitTestInfo hittestInfo = this.workflowView.messageHitTestContexts.Pop();
                    if (hittestInfo != this.messageContext)
                        Debug.Assert(false, "WorkflowView poped wrong message context");
                }
            }

            public ReadOnlyCollection<WorkflowDesignerMessageFilter> Filters
            {
                get
                {
                    //We recreate a new list everytime as in some of the messages dispatched, we there can
                    //be additional filters which might be added
                    List<WorkflowDesignerMessageFilter> mergedFilterList = new List<WorkflowDesignerMessageFilter>();
                    mergedFilterList.AddRange(this.workflowView.customMessageFilters);
                    mergedFilterList.AddRange(this.workflowView.stockMessageFilters);
                    return mergedFilterList.AsReadOnly();
                }
            }
        }
        #endregion

        #region IMessageFilter Implementation
        bool IMessageFilter.PreFilterMessage(ref Message m)
        {
            bool handled = false;
            if (m.Msg == NativeMethods.WM_KEYDOWN || m.Msg == NativeMethods.WM_SYSKEYDOWN ||
                m.Msg == NativeMethods.WM_KEYUP || m.Msg == NativeMethods.WM_SYSKEYUP)
            {
                Control control = Control.FromHandle(m.HWnd);
                if (control != null && (control == this || Controls.Contains(control)))
                {
                    KeyEventArgs eventArgs = new KeyEventArgs((Keys)(unchecked((int)(long)m.WParam)) | ModifierKeys);
                    if (m.Msg == NativeMethods.WM_KEYDOWN || m.Msg == NativeMethods.WM_SYSKEYDOWN)
                        OnKeyDown(eventArgs);
                    else
                        OnKeyUp(eventArgs);

                    handled = eventArgs.Handled;
                }
            }

            return handled;
        }
        #endregion
    }

    #region Class WorkflowViewAccessibleObject
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class WorkflowViewAccessibleObject : Control.ControlAccessibleObject
    {
        private WorkflowView workflowView;

        public WorkflowViewAccessibleObject(WorkflowView workflowView)
            : base(workflowView)
        {
            if (workflowView == null)
                throw new ArgumentNullException("workflowView");
            this.workflowView = workflowView;
        }

        public override Rectangle Bounds
        {
            get
            {
                return new Rectangle(this.workflowView.PointToScreen(Point.Empty), this.workflowView.ViewPortSize);
            }
        }

        public override string DefaultAction
        {
            get
            {
                return DR.GetString(DR.AccessibleAction);
            }
        }

        public override string Description
        {
            get
            {
                return DR.GetString(DR.WorkflowViewAccessibleDescription);
            }
        }

        public override string Help
        {
            get
            {
                return DR.GetString(DR.WorkflowViewAccessibleHelp);
            }
        }

        public override string Name
        {
            get
            {
                return DR.GetString(DR.WorkflowViewAccessibleName);
            }

            set
            {
            }
        }

        public override AccessibleRole Role
        {
            get
            {
                return AccessibleRole.Diagram;
            }
        }

        public override AccessibleObject GetChild(int index)
        {
            return (this.workflowView.RootDesigner != null && index == 0) ? this.workflowView.RootDesigner.AccessibilityObject : base.GetChild(index);
        }

        public override int GetChildCount()
        {
            return (this.workflowView.RootDesigner != null) ? 1 : -1;
        }

        public override AccessibleObject Navigate(AccessibleNavigation navdir)
        {
            if (navdir == AccessibleNavigation.FirstChild || navdir == AccessibleNavigation.LastChild)
                return GetChild(0);
            else
                return base.Navigate(navdir);
        }
    }
    #endregion

    #region WorkflowTimer
    internal sealed class WorkflowTimer : IDisposable
    {
        private static WorkflowTimer workflowTimer;

        private const int TimerInterval = 50;
        private Timer timer = null;
        private List<ElapsedEventUnit> elapsedEvents = new List<ElapsedEventUnit>();

        internal static WorkflowTimer Default
        {
            get
            {
                if (WorkflowTimer.workflowTimer == null)
                    WorkflowTimer.workflowTimer = new WorkflowTimer();
                return WorkflowTimer.workflowTimer;
            }
        }

        private WorkflowTimer()
        {
            this.timer = new Timer();
            this.timer.Interval = WorkflowTimer.TimerInterval;
            this.timer.Tick += new EventHandler(OnTimer);
            this.timer.Stop();
        }

        ~WorkflowTimer()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (this.timer != null)
            {
                if (this.timer.Enabled)
                    this.timer.Stop();

                this.timer.Dispose();
                this.timer = null;
            }
        }

        internal void Subscribe(int elapsedInterval, EventHandler elapsedEventHandler)
        {
            this.elapsedEvents.Add(new ElapsedEventUnit(elapsedInterval / WorkflowTimer.TimerInterval, elapsedEventHandler));
            if (!this.timer.Enabled)
                this.timer.Start();
        }

        internal void Unsubscribe(EventHandler elapsedEventHandler)
        {
            List<ElapsedEventUnit> removableElapsedEvents = new List<ElapsedEventUnit>();
            foreach (ElapsedEventUnit elapsedEvent in this.elapsedEvents)
            {
                if (elapsedEvent.elapsedEventHandler == elapsedEventHandler)
                    removableElapsedEvents.Add(elapsedEvent);
            }

            foreach (ElapsedEventUnit elapsedEvent in removableElapsedEvents)
                this.elapsedEvents.Remove(elapsedEvent);

            if (this.elapsedEvents.Count == 0 && this.timer.Enabled)
                this.timer.Stop();
        }

        private void OnTimer(object sender, EventArgs e)
        {
            List<ElapsedEventUnit> clonedList = new List<ElapsedEventUnit>(this.elapsedEvents);
            foreach (ElapsedEventUnit elapsedEvent in clonedList)
            {
                elapsedEvent.elapsedTime += 1;
                if (elapsedEvent.elapsedInterval <= elapsedEvent.elapsedTime)
                {
                    elapsedEvent.elapsedTime = 0;
                    elapsedEvent.elapsedEventHandler(this, EventArgs.Empty);
                }
            }
        }

        private sealed class ElapsedEventUnit
        {
            internal EventHandler elapsedEventHandler;
            internal int elapsedInterval;
            internal int elapsedTime;

            internal ElapsedEventUnit(int interval, EventHandler eventHandler)
            {
                this.elapsedInterval = interval;
                this.elapsedEventHandler = eventHandler;
            }
        }
    }
    #endregion
}
