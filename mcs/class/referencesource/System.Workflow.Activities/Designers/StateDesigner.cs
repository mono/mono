namespace System.Workflow.Activities
{
    using System;
    using System.Text;
    using System.Reflection;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.CodeDom;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing.Design;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Diagnostics;
    using System.IO;
    using System.Windows.Forms;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;
    using System.Runtime.Serialization;
    using Microsoft.Win32;
    using System.Runtime.InteropServices;
    using System.Workflow.ComponentModel.Serialization;
    using System.Globalization;
    using System.ComponentModel.Design.Serialization;
    using System.Resources;

    #region StateDesigner Class
    [DesignerSerializer(typeof(StateDesignerLayoutSerializer), typeof(WorkflowMarkupSerializer))]
    [ActivityDesignerTheme(typeof(StateDesignerTheme))]
    [System.Runtime.InteropServices.ComVisible(false)]
    internal partial class StateDesigner : FreeformActivityDesigner
    {
        #region Fields

        internal static readonly Image CompletedState = DR.GetImage(DR.CompletedState);
        internal static readonly Image InitialState = DR.GetImage(DR.InitialState);
        private const string ActiveDesignerNamePropertyName = "ActiveDesignerName";

        // 30 is the margin around the designer
        // this value comes from DefaultWorkflowLayout.Separator, but this
        // class is internal
        internal static Size Separator = new Size(30, 30);
        private const int DefaultStateDesignerAutoLayoutDistance = 16;

        private ActivityDesigner _activeDesigner;

        private bool _dragDropActive;
        internal bool _ensuringVisible;
        private Layout _rootDesignerLayout;
        private DesignerLinkLayout _designerLinkLayout;
        private StateDesigner _rootStateDesigner;
        private Size _stateSize;
        private Point _stateLocation;
        private Size _stateMinimumSize;
        private Size _minimumSize = Size.Empty;
        private bool _performingLayout = false;

        private EventHandlersLayout _eventHandlersLayout;
        private EventDrivenLayout _eventDrivenLayout;
        private Dictionary<Activity, DesignerLayout> _designerLayouts;
        private StatesLayout _statesLayout;
        private TitleBarLayout _titleBarLayout;
        private ContainedDesignersParser _designersParser;
        private ISelectionService _selectionService;
        private ActivityDesignerVerbCollection _verbs;
        private string _helpText;
        private bool _needsAutoLayout = false;

        // 







        private bool _addingSetState = true;
        private bool _removingSetState = true;

        #endregion

        #region Constructor
        /// <summary>
        /// Default constructor for the StateDesigner
        /// </summary>
        public StateDesigner()
        {
        }

        protected override void Initialize(Activity activity)
        {
            base.Initialize(activity);
            EnsureDesignerExtender();
            _titleBarLayout = new TitleBarLayout(this);
            _eventDrivenLayout = new EventDrivenLayout(this, _titleBarLayout);
            _eventHandlersLayout = new EventHandlersLayout(this);
            _statesLayout = new StatesLayout(this, _titleBarLayout, _eventHandlersLayout);
            _designerLinkLayout = new DesignerLinkLayout(this);
            _designerLinkLayout.MouseDown += new MouseEventHandler(this.StateDesignerLinkMouseDown);
            this.AutoSizeMargin = new Size(16, 24);
            this.AutoSize = true;
        }

        private void EnsureDesignerExtender()
        {
            bool addExtender = true;
            IExtenderListService extenderListService = GetService(typeof(IExtenderListService)) as IExtenderListService;
            if (extenderListService != null)
            {
                foreach (IExtenderProvider extenderProvider in extenderListService.GetExtenderProviders())
                {
                    if (extenderProvider.GetType() == typeof(StateDesignerPropertyExtender))
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
                    extenderProviderService.AddExtenderProvider(new StateDesignerPropertyExtender());
                    TypeDescriptor.Refresh(Activity);
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _designerLinkLayout.MouseDown -= new MouseEventHandler(this.StateDesignerLinkMouseDown);
            }

            base.Dispose(disposing);
        }

        #endregion

        #region Properties

        #region Public Properties

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
                if (!IsVisible)
                    return null;

                if (this.HasActiveDesigner)
                    return this.ActiveDesigner.Activity;

                if (this.DesignersParser.Ordered.Count > 0)
                    return this.DesignersParser.Ordered[0].Activity;

                return null;
            }
        }

        public override object LastSelectableObject
        {
            get
            {
                if (!IsVisible)
                    return null;

                if (this.HasActiveDesigner)
                    return this.ActiveDesigner.Activity;

                if (this.DesignersParser.Ordered.Count > 0)
                    return this.DesignersParser.Ordered[this.DesignersParser.Ordered.Count - 1].Activity;

                return null;
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
                if (base.Location == value)
                    return;

                if (this.HasActiveDesigner && !this.PerformingLayout && !this.IsRootStateDesigner)
                {
                    this._stateLocation = value;
                }
                else
                {
                    if (this.IsRootStateDesigner)
                    {
                        bool previousPerformingLayout = this.PerformingLayout;
                        this.PerformingLayout = true;
                        try
                        {
                            base.Location = value;
                        }
                        finally
                        {
                            this.PerformingLayout = previousPerformingLayout;
                        }
                    }
                    else
                    {
                        base.Location = value;
                    }

                    // note that we must use base.Location instead of
                    // value in the line below, because the 
                    // base implementation of the Location property may
                    // auto adjust the value depending on auto layouting
                    // characteristics
                    this.RootDesignerLayout.MoveLayout(base.Location);
                    Invalidate();
                }
            }
        }

        public override Size Size
        {
            get
            {
                return base.Size;
            }
            set
            {
                if (this.HasActiveDesigner && !this.PerformingLayout && !this.IsRootStateDesigner)
                {
                    this._stateSize = value;
                }
                else
                {
                    if (this.IsRootStateDesigner)
                    {
                        bool previousPerformingLayout = this.PerformingLayout;
                        this.PerformingLayout = true;
                        try
                        {
                            base.Size = value;
                        }
                        finally
                        {
                            this.PerformingLayout = previousPerformingLayout;
                        }
                    }
                    else
                    {
                        base.Size = value;
                    }

                    Size newSize = base.Size;
                    this.RootDesignerLayout.ResizeLayout(newSize);
                }
            }
        }

        public override string Text
        {
            get
            {
                string text = base.Text;
                if (String.IsNullOrEmpty(text))
                {
                    text = this.Activity.GetType().Name;
                }
                return text;
            }
        }

        #endregion

        #region Protected Properties

        public override Image Image
        {
            get
            {
                StateActivity state = this.Activity as StateActivity;
                if (state != null)
                {
                    if (StateMachineHelpers.IsLeafState(state))
                    {
                        if (StateMachineHelpers.IsInitialState(state))
                        {
                            if (!StateMachineHelpers.IsCompletedState(state))
                                return GetInitialStateDesignerImage(this);
                        }
                        else
                        {
                            if (StateMachineHelpers.IsCompletedState(state))
                                return GetCompletedStateDesignerImage(this);
                        }
                    }
                }
                return base.Image;
            }
            protected set
            {
                base.Image = value;
            }
        }

        protected override Rectangle ExpandButtonRectangle
        {
            get
            {
                return Rectangle.Empty;
            }
        }

        /// <summary>
        /// Gets the array of glyphs with which to adorn the designer.
        /// </summary>
        protected override ActivityDesignerGlyphCollection Glyphs
        {
            get
            {
                ActivityDesignerGlyphCollection glyphs = new ActivityDesignerGlyphCollection();

                glyphs.AddRange(base.Glyphs);

                if (!this.HasActiveDesigner)
                {
                    foreach (EventDrivenDesigner eventDrivenDesigner in this.DesignersParser.EventDrivenDesigners)
                    {
                        Layout selectedLayout = this.RootDesignerLayout.GetLayout(eventDrivenDesigner);
                        if (selectedLayout != null)
                        {
                            if (eventDrivenDesigner.IsSelected)
                            {
                                LayoutSelectionGlyph glyph = new LayoutSelectionGlyph(selectedLayout);
                                glyphs.Add(glyph);
                            }
                            if (!eventDrivenDesigner.Activity.Enabled)
                            {
                                CommentLayoutGlyph glyph = new CommentLayoutGlyph(selectedLayout);
                                glyphs.Add(glyph);
                            }
                        }
                    }

                    foreach (StateInitializationDesigner stateInitializationDesigner in this.DesignersParser.StateInitializationDesigners)
                    {
                        Layout selectedLayout = this.RootDesignerLayout.GetLayout(stateInitializationDesigner);
                        if (selectedLayout != null)
                        {
                            if (stateInitializationDesigner.IsSelected)
                            {
                                LayoutSelectionGlyph glyph = new LayoutSelectionGlyph(selectedLayout);
                                glyphs.Add(glyph);
                            }
                            if (!stateInitializationDesigner.Activity.Enabled)
                            {
                                CommentLayoutGlyph glyph = new CommentLayoutGlyph(selectedLayout);
                                glyphs.Add(glyph);
                            }
                        }
                    }

                    foreach (StateFinalizationDesigner stateFinalizationDesigner in this.DesignersParser.StateFinalizationDesigners)
                    {
                        Layout selectedLayout = this.RootDesignerLayout.GetLayout(stateFinalizationDesigner);
                        if (selectedLayout != null)
                        {
                            if (stateFinalizationDesigner.IsSelected)
                            {
                                LayoutSelectionGlyph glyph = new LayoutSelectionGlyph(selectedLayout);
                                glyphs.Add(glyph);
                            }
                            if (!stateFinalizationDesigner.Activity.Enabled)
                            {
                                CommentLayoutGlyph glyph = new CommentLayoutGlyph(selectedLayout);
                                glyphs.Add(glyph);
                            }
                        }
                    }
                }

                return glyphs;
            }
        }
        protected override Rectangle ImageRectangle
        {
            get
            {
                if (this.HasActiveDesigner && !this.IsRootStateDesigner)
                    return new Rectangle(-1, -1, 1, 1); // Create a rectangle outside the window to hide the icon

                return _titleBarLayout.ImageLayout.Bounds;
            }
        }

        protected override Rectangle TextRectangle
        {
            get
            {
                if (this.HasActiveDesigner && !this.IsRootStateDesigner)
                    return Rectangle.Empty;
                else
                    return _titleBarLayout.TextLayout.Bounds;
            }
        }

        protected override ActivityDesignerVerbCollection Verbs
        {
            get
            {
                ActivityDesignerVerbCollection verbs = new ActivityDesignerVerbCollection();

                verbs.AddRange(base.Verbs);

                if (_verbs == null)
                {
                    _verbs = new ActivityDesignerVerbCollection();

                    ActivityDesignerVerb stateMachineView = new ActivityDesignerVerb(this,
                        DesignerVerbGroup.General,
                        DR.GetString(DR.StateMachineView),
                        new EventHandler(OnStateMachineView),
                        new EventHandler(OnStatusStateMachineView));
                    _verbs.Add(stateMachineView);

                    ActivityDesignerVerb setAsInitialState = new ActivityDesignerVerb(this,
                        DesignerVerbGroup.General,
                        DR.GetString(DR.SetAsInitialState),
                        new EventHandler(OnSetAsInitialState),
                        new EventHandler(OnStatusSetAsInitialState));
                    _verbs.Add(setAsInitialState);

                    ActivityDesignerVerb setAsCompletedState = new ActivityDesignerVerb(this,
                        DesignerVerbGroup.General,
                        DR.GetString(DR.SetAsCompletedState),
                        new EventHandler(OnSetAsCompletedState),
                        new EventHandler(OnStatusSetAsCompletedState));
                    _verbs.Add(setAsCompletedState);

                    ActivityDesignerVerb addState = new ActivityDesignerVerb(this,
                        DesignerVerbGroup.General,
                        DR.GetString(DR.AddState),
                        new EventHandler(OnAddState),
                        new EventHandler(OnStatusAddState));
                    _verbs.Add(addState);

                    ActivityDesignerVerb addEventDrivenVerb = new ActivityDesignerVerb(this,
                        DesignerVerbGroup.General,
                        DR.GetString(DR.AddEventDriven),
                        new EventHandler(OnAddEventDriven),
                        new EventHandler(OnStatusAddEventDriven));
                    _verbs.Add(addEventDrivenVerb);

                    ActivityDesignerVerb addStateInitialization = new ActivityDesignerVerb(this,
                        DesignerVerbGroup.General,
                        DR.GetString(DR.AddStateInitialization),
                        new EventHandler(OnAddStateInitialization),
                        new EventHandler(OnStatusAddStateInitialization));
                    _verbs.Add(addStateInitialization);

                    ActivityDesignerVerb addStateFinalization = new ActivityDesignerVerb(this,
                        DesignerVerbGroup.General,
                        DR.GetString(DR.AddStateFinalization),
                        new EventHandler(OnAddStateFinalization),
                        new EventHandler(OnStatusAddStateFinalization));
                    _verbs.Add(addStateFinalization);
                }

                verbs.AddRange(this._verbs);

                return verbs;
            }
        }

        protected override bool ShowConnectorsInForeground
        {
            get
            {
                return true;
            }
        }

        #endregion Protected Properties

        #region Private Properties

        internal ActivityDesigner ActiveDesigner
        {
            get
            {
                return _activeDesigner;
            }
            set
            {
                if (_activeDesigner == value)
                    return;

                // if we're setting to a new active designer then we need to make sure
                // that we don't have one active yet.
                Debug.Assert((value == null) || (value != null && _activeDesigner == null));

                _activeDesigner = value;

                // Don't use AutoSize in the EventDriven view
                this.AutoSize = (value == null);

                RefreshDesignerVerbs();

                if (IsRootStateDesigner)
                {
                    PerformLayout();
                }
                else
                {
                    StateDesigner parentStateDesigner = ParentDesigner as StateDesigner;
                    if (value == null)
                        SetActiveDesignerHelper(parentStateDesigner, null);
                    else
                        SetActiveDesignerHelper(parentStateDesigner, this);
                }

                // When switching back and forth between
                // the EventDriven view or State Machine View
                // we preserve the location of the StateDesigner
                if (_activeDesigner == null)
                {
                    // Important: _minimumSize needs to be restored before
                    // this.Size, because base.Size honors _minimumSize
                    _minimumSize = _stateMinimumSize;
                    this.Location = _stateLocation;
                    this.Size = _stateSize;
                }
                else
                {
                    _stateLocation = this.Location;
                    _stateSize = this.Size;
                    _stateMinimumSize = _minimumSize;
                    _minimumSize = Size.Empty;
                }
            }
        }

        // This property is used to signal to the OnConnectorAdded method
        // it is necessary to add a SetState activity or not. For example,
        // when the user manually draws a connector, we need to add a corresponding
        // SetState activity, but when the user adds a SetState activity directly
        // to the event driven, we have code that will automatically create a 
        // corresponding connector. When this happens OnConnectorAdded gets called
        // but we cannot add another SetState, otherwise we will end up with 
        // duplicate SetState activities.
        internal bool AddingSetState
        {
            get
            {
                return _addingSetState;
            }
            set
            {
                _addingSetState = value;
            }
        }

        internal bool RemovingSetState
        {
            get
            {
                return _removingSetState;
            }
            set
            {
                _removingSetState = value;
            }
        }

        internal bool PerformingLayout
        {
            get
            {
                return this.RootStateDesigner._performingLayout;
            }
            set
            {
                Debug.Assert(this.IsRootStateDesigner);
                this._performingLayout = value;
            }
        }

        internal Dictionary<Activity, DesignerLayout> DesignerLayouts
        {
            get
            {
                if (_designerLayouts == null)
                {
                    _designerLayouts = new Dictionary<Activity, DesignerLayout>();
                }
                return _designerLayouts;
            }
        }

        private ContainedDesignersParser DesignersParser
        {
            get
            {
                if (_designersParser == null)
                {
                    _designersParser = new ContainedDesignersParser(this.ContainedDesigners);
                }
                return _designersParser;
            }
            set
            {
                _designersParser = value;
            }
        }

        internal bool HasActiveDesigner
        {
            get
            {
                return (this.ActiveDesigner != null);
            }
        }

        private bool IsStateCustomActivity
        {
            get
            {
                StateActivity state = (StateActivity)this.Activity;
                if (StateMachineHelpers.IsStateMachine(state) ||
                    state.Parent != null)
                    return false;
                else
                    return true;
            }
        }

        internal virtual string HelpText
        {
            get
            {
                if (_helpText == null)
                {
                    _helpText = DR.GetString(DR.StateHelpText);
                }
                return _helpText;
            }
        }

        private bool DragDropActive
        {
            get
            {
                return _dragDropActive;
            }
            set
            {
                if (value == _dragDropActive)
                    return;
                _dragDropActive = value;
                Invalidate();
            }
        }

        private DesignerLinkLayout InlineLayout
        {
            get
            {
                return _designerLinkLayout;
            }
        }

        internal bool IsRootStateDesigner
        {
            get
            {
                // if the site is null, then it means
                // that the designer was created buy not
                // added to the WorkflowView yet. In 
                // this case we cannot assume that just because
                // the activity is the root just because
                // it doesn't have a parent
                return (this.Activity.Site != null) &&
                    StateMachineHelpers.IsRootState((StateActivity)this.Activity);
            }
        }

        internal ReadOnlyCollection<Rectangle> EventHandlersBounds
        {
            get
            {
                List<Rectangle> excluded = new List<Rectangle>();
                foreach (DesignerLayout layout in this.DesignerLayouts.Values)
                {
                    Rectangle bounds = layout.Bounds;
                    bounds.Inflate(0, 4);
                    excluded.Add(bounds);
                }
                return excluded.AsReadOnly();
            }
        }

        internal StateDesigner RootStateDesigner
        {
            get
            {
                if (_rootStateDesigner == null)
                {
                    StateActivity rootState = StateMachineHelpers.GetRootState((StateActivity)this.Activity);
                    _rootStateDesigner = GetDesigner(rootState) as StateDesigner;
                }
                return _rootStateDesigner;
            }
        }

        public override Size MinimumSize
        {
            get
            {
                return _minimumSize;
            }
        }

        private Layout RootDesignerLayout
        {
            get
            {
                if (_rootDesignerLayout == null)
                {
                    RefreshRootDesignerLayout();
                }
                return _rootDesignerLayout;
            }
            set
            {
                _rootDesignerLayout = value;
            }
        }

        internal ISelectionService SelectionService
        {
            get
            {
                if (_selectionService == null)
                {
                    _selectionService = (ISelectionService)this.GetService(typeof(ISelectionService));
                    _selectionService.SelectionChanged += new EventHandler(SelectionChanged);
                }
                return _selectionService;
            }
        }

        internal virtual ReadOnlyCollection<Type> ValidChildTypes
        {
            get
            {
                List<Type> validChildTypes = new List<Type>();
                validChildTypes.Add(typeof(StateActivity));
                validChildTypes.Add(typeof(EventDrivenActivity));
                StateActivity state = (StateActivity)this.Activity;
                if (StateMachineHelpers.IsLeafState(state))
                {
                    if (this.DesignersParser.StateInitializationDesigners.Count == 0)
                        validChildTypes.Add(typeof(StateInitializationActivity));
                    if (this.DesignersParser.StateFinalizationDesigners.Count == 0)
                        validChildTypes.Add(typeof(StateFinalizationActivity));
                }
                return validChildTypes.AsReadOnly();
            }
        }

        internal Cursor Cursor
        {
            get
            {
                return this.ParentView.Cursor;
            }
            set
            {
                this.ParentView.Cursor = value;
            }
        }

        private Point TopConnectionPoint
        {
            get
            {
                Rectangle bounds = this.Bounds;
                int midHorz = bounds.X + (bounds.Width / 2);
                Point point = new Point(midHorz, bounds.Top);
                return point;
            }
        }

        private Point BottomConnectionPoint
        {
            get
            {
                Rectangle bounds = this.Bounds;
                int midHorz = bounds.X + (bounds.Width / 2);
                Point point = new Point(midHorz, bounds.Bottom);
                return point;
            }
        }

        private bool NeedsAutoLayout
        {
            get
            {
                return _needsAutoLayout;
            }
            set
            {
                _needsAutoLayout = value;
            }
        }

        #endregion Private Properties

        #endregion

        #region Methods

        #region Public Methods

        public override bool CanInsertActivities(HitTestInfo insertLocation, ReadOnlyCollection<Activity> activitiesToInsert)
        {
            if (this.HasActiveDesigner ||
                this.IsStateCustomActivity)
                return false;

            StateActivity state = (StateActivity)this.Activity;
            if (StateMachineHelpers.IsLeafState(state) &&
                StateMachineHelpers.IsCompletedState(state))
                return false;

            ReadOnlyCollection<Type> validChildTypes = ValidChildTypes;
            foreach (Activity activity in activitiesToInsert)
            {
                bool contains = false;
                foreach (Type type in validChildTypes)
                {
                    if (type.IsInstanceOfType(activity))
                    {
                        contains = true;
                        break;
                    }
                }
                if (!contains)
                    return false;
            }
            return base.CanInsertActivities(insertLocation, activitiesToInsert);
        }

        public override bool CanBeParentedTo(CompositeActivityDesigner parentActivityDesigner)
        {
            if (parentActivityDesigner == null)
                throw new ArgumentNullException("parentActivityDesigner");

            CompositeActivity parentActivity = parentActivityDesigner.Activity as CompositeActivity;
            if (parentActivity == null)
                return false;

            if (!(parentActivity is StateActivity))
                return false;

            return base.CanBeParentedTo(parentActivityDesigner);
        }

        public override void EnsureVisibleContainedDesigner(ActivityDesigner containedDesigner)
        {
            if (containedDesigner == null)
                throw new ArgumentNullException("containedDesigner");

            // call base
            base.EnsureVisibleContainedDesigner(containedDesigner);

            // 



            if (!_ensuringVisible)
            {
                if (containedDesigner is StateDesigner)
                {
                    SetActiveDesigner(null);
                }
                else
                {
                    SetActiveDesigner(containedDesigner);
                }
                SetParentTreeEnsuringVisible(true);
            }
            _ensuringVisible = false;
        }

        public override object GetNextSelectableObject(object current, DesignerNavigationDirection direction)
        {
            Activity activity = current as Activity;
            if (activity == null)
                return null;

            // Workaround: there is a special case code in
            // WorkflowView.EnsureVisible that calls EnsureVisible
            // in the wrong order, causing the EnsureVisible flag
            // to be in the wrong state
            SetParentTreeEnsuringVisible(false);

            ActivityDesigner designer = GetDesigner(activity);

            List<ActivityDesigner> ordered = this.DesignersParser.Ordered;
            int indexOf = ordered.IndexOf(designer);
            if (indexOf < 0)
                return null;

            if (current is EventDrivenActivity ||
                current is StateInitializationActivity ||
                current is StateFinalizationActivity)
            {
                if (direction == DesignerNavigationDirection.Left || direction == DesignerNavigationDirection.Right)
                    return null;

                if (direction == DesignerNavigationDirection.Down)
                {
                    if (indexOf < (ordered.Count - 1))
                        return ordered[indexOf + 1].Activity;
                    else
                        return null;
                }
                else
                {
                    if (indexOf > 0)
                        return ordered[indexOf - 1].Activity;
                    else
                        return null;
                }
            }
            else
            {
                StateActivity currentState = current as StateActivity;
                Debug.Assert(currentState != null);
                if (StateMachineHelpers.IsLeafState(currentState))
                {
                    if (direction == DesignerNavigationDirection.Right)
                    {
                        if (this.DesignersParser.StateDesigners.Count > 0)
                            return this.DesignersParser.StateDesigners[0].Activity;
                    }
                    else if (direction == DesignerNavigationDirection.Up)
                    {
                        if (indexOf > 0)
                            return ordered[indexOf - 1].Activity;
                    }
                    else if (direction == DesignerNavigationDirection.Down)
                    {
                        if (indexOf < (ordered.Count - 1))
                            return ordered[indexOf + 1].Activity;
                    }
                }
                else
                {
                    if (direction == DesignerNavigationDirection.Left || direction == DesignerNavigationDirection.Up)
                    {
                        if (indexOf > 0)
                            return ordered[indexOf - 1].Activity;
                    }
                    else
                    {
                        if (indexOf < (ordered.Count - 1))
                            return ordered[indexOf + 1].Activity;
                    }
                }
            }

            return null;
        }

        public override HitTestInfo HitTest(Point point)
        {
            // get the base class hit test
            HitTestInfo hitTestInfo = base.HitTest(point);

            // first we check if one of our layouts got the hit
            HitTestInfo hitInfo = this.RootDesignerLayout.HitTest(point);
            if (!hitInfo.Equals(HitTestInfo.Nowhere))
                return hitInfo;

            return hitTestInfo;
        }

        public override bool IsContainedDesignerVisible(ActivityDesigner containedDesigner)
        {
            if (this.HasActiveDesigner)
            {
                if (containedDesigner == this.ActiveDesigner)
                    return true;
            }
            else
            {
                if (containedDesigner is StateDesigner)
                    return true;
            }

            return false;
        }

        #endregion

        #region Protected Methods

        protected override bool CanResizeContainedDesigner(ActivityDesigner containedDesigner)
        {
            if (this.HasActiveDesigner)
                return false;

            return base.CanResizeContainedDesigner(containedDesigner);
        }

        protected override Connector CreateConnector(ConnectionPoint source, ConnectionPoint target)
        {
            return new StateDesignerConnector(source, target);
        }

        protected override bool CanConnect(ConnectionPoint source, ConnectionPoint target)
        {
            DesignerLayoutConnectionPoint sourceDesignerLayoutConnectionPoint = source as DesignerLayoutConnectionPoint;
            DesignerLayoutConnectionPoint targetDesignerLayoutConnectionPoint = target as DesignerLayoutConnectionPoint;

            if (sourceDesignerLayoutConnectionPoint == null)
            {
                if (!IsValidTargetConnectionPoint(source))
                    return false;
            }
            else
            {
                if (sourceDesignerLayoutConnectionPoint.DesignerLayout.ActivityDesigner is StateFinalizationDesigner)
                    return false;
            }

            if (targetDesignerLayoutConnectionPoint == null)
            {
                if (!IsValidTargetConnectionPoint(target))
                    return false;
            }
            else
            {
                if (targetDesignerLayoutConnectionPoint.DesignerLayout.ActivityDesigner is StateFinalizationDesigner)
                    return false;
            }

            bool canConnect =
                (sourceDesignerLayoutConnectionPoint == null && targetDesignerLayoutConnectionPoint != null) ||
                (sourceDesignerLayoutConnectionPoint != null && targetDesignerLayoutConnectionPoint == null);

            return canConnect;
        }

        private bool IsValidTargetConnectionPoint(ConnectionPoint target)
        {
            StateDesigner stateDesigner = target.AssociatedDesigner as StateDesigner;
            if (stateDesigner == null)
                return false;
            StateActivity state = (StateActivity)stateDesigner.Activity;
            return StateMachineHelpers.IsLeafState(state);
        }

        public override ReadOnlyCollection<ConnectionPoint> GetConnectionPoints(DesignerEdges edges)
        {
            List<ConnectionPoint> connectionPoints = new List<ConnectionPoint>();
            // In the EventDriven view we don't allow free form connectors
            if (!this.HasActiveDesigner)
            {
                if (!this.IsRootStateDesigner)
                {
                    if ((edges & DesignerEdges.Top) > 0)
                        connectionPoints.Add(new ConnectionPoint(this, DesignerEdges.Top, 0));

                    if ((edges & DesignerEdges.Bottom) > 0)
                        connectionPoints.Add(new ConnectionPoint(this, DesignerEdges.Bottom, 0));
                }

                int leftConnectionIndex = 0, rightConnectionIndex = 0;
                foreach (DesignerLayout layout in this.DesignerLayouts.Values)
                {
                    if (!this.IsRootStateDesigner)
                    {
                        if ((edges & DesignerEdges.Left) > 0 && layout.LeftConnectionPoint != Point.Empty)
                        {
                            connectionPoints.Add(new DesignerLayoutConnectionPoint(this, leftConnectionIndex, (CompositeActivity)layout.ActivityDesigner.Activity, DesignerEdges.Left));
                            leftConnectionIndex += 1;
                        }
                    }

                    if ((edges & DesignerEdges.Right) > 0 && layout.RightConnectionPoint != Point.Empty)
                    {
                        connectionPoints.Add(new DesignerLayoutConnectionPoint(this, rightConnectionIndex, (CompositeActivity)layout.ActivityDesigner.Activity, DesignerEdges.Right));
                        rightConnectionIndex += 1;
                    }
                }
            }

            return connectionPoints.AsReadOnly();
        }

        protected override ReadOnlyCollection<Point> GetConnections(DesignerEdges edges)
        {
            List<Point> connections = new List<Point>();
            if ((edges & DesignerEdges.Top) > 0)
                connections.Add(this.TopConnectionPoint);

            if ((edges & DesignerEdges.Bottom) > 0)
                connections.Add(this.BottomConnectionPoint);

            foreach (DesignerLayout layout in this.DesignerLayouts.Values)
            {
                if (!this.IsRootStateDesigner)
                {
                    if ((edges & DesignerEdges.Left) > 0 && layout.LeftConnectionPoint != Point.Empty)
                        connections.Add(layout.LeftConnectionPoint);
                }
                if ((edges & DesignerEdges.Right) > 0 && layout.RightConnectionPoint != Point.Empty)
                    connections.Add(layout.RightConnectionPoint);
            }

            return connections.AsReadOnly();
        }

        protected override void OnConnectorAdded(ConnectorEventArgs e)
        {
            base.OnConnectorAdded(e);

            StateDesignerConnector connector = e.Connector as StateDesignerConnector;
            if (connector == null)
                return;

            // We need to make sure that the source connection point
            // is always the event handler
            DesignerLayoutConnectionPoint sourceDesignerLayoutConnectionPoint = connector.Source as DesignerLayoutConnectionPoint;
            DesignerLayoutConnectionPoint targetDesignerLayoutConnectionPoint = connector.Target as DesignerLayoutConnectionPoint;
            if (sourceDesignerLayoutConnectionPoint == null)
            {
                Debug.Assert(targetDesignerLayoutConnectionPoint != null);
                ConnectionPoint source = connector.Source;
                connector.Source = connector.Target;
                connector.Target = source;
            }
            else
            {
                Debug.Assert(targetDesignerLayoutConnectionPoint == null);
            }

            ConnectionPoint target = connector.Target;
            sourceDesignerLayoutConnectionPoint = (DesignerLayoutConnectionPoint)connector.Source;
            if (this.RootStateDesigner.AddingSetState)
            {
                SetStateActivity setState = new SetStateActivity();
                setState.TargetStateName = target.AssociatedDesigner.Activity.QualifiedName;
                CompositeActivityDesigner compositeDesigner = (CompositeActivityDesigner)StateDesigner.GetDesigner(sourceDesignerLayoutConnectionPoint.EventHandler);
                List<Activity> activitiesToInsert = new List<Activity>();
                activitiesToInsert.Add(setState);
                compositeDesigner.InsertActivities(new HitTestInfo(compositeDesigner, HitTestLocations.Designer), activitiesToInsert.AsReadOnly());
                connector.SetStateName = setState.QualifiedName;
            }
            connector.TargetStateName = target.AssociatedDesigner.Activity.QualifiedName;
            connector.SourceStateName = sourceDesignerLayoutConnectionPoint.EventHandler.Parent.QualifiedName;
            connector.EventHandlerName = sourceDesignerLayoutConnectionPoint.EventHandler.QualifiedName;
        }

        protected override void OnConnectorRemoved(ConnectorEventArgs e)
        {
            base.OnConnectorRemoved(e);

            StateDesignerConnector connector = e.Connector as StateDesignerConnector;
            if (connector == null || string.IsNullOrEmpty(connector.SetStateName) || !this.RootStateDesigner.RemovingSetState)
                return;

            DesignerLayoutConnectionPoint sourceDesignerLayoutConnectionPoint = connector.Source as DesignerLayoutConnectionPoint;
            if (sourceDesignerLayoutConnectionPoint != null)
            {
                CompositeActivityDesigner compositeDesigner = StateDesigner.GetDesigner(sourceDesignerLayoutConnectionPoint.EventHandler) as CompositeActivityDesigner;
                if (compositeDesigner != null && sourceDesignerLayoutConnectionPoint.EventHandler != null)
                {
                    Activity setStateActivity = StateDesigner.FindActivityByQualifiedName(sourceDesignerLayoutConnectionPoint.EventHandler, connector.SetStateName);
                    if (setStateActivity != null)
                    {
                        List<Activity> activitiesToRemove = new List<Activity>();
                        activitiesToRemove.Add(setStateActivity);
                        CompositeActivityDesigner setStateParentDesigner = StateDesigner.GetDesigner(setStateActivity.Parent) as CompositeActivityDesigner;
                        if (setStateParentDesigner != null)
                            setStateParentDesigner.RemoveActivities(activitiesToRemove.AsReadOnly());
                    }
                }
            }
        }

        protected override void OnConnectorChanged(ConnectorEventArgs e)
        {
            base.OnConnectorChanged(e);

            Connector connector = e.Connector;

            StateDesignerConnector stateDesignerConnector = connector as StateDesignerConnector;
            if (stateDesignerConnector == null)
                return;

            if (!stateDesignerConnector.Target.AssociatedDesigner.Activity.QualifiedName.Equals(stateDesignerConnector.TargetStateName))
            {
                StateActivity rootState = (StateActivity)this.RootStateDesigner.Activity;
                // target state has changed
                SetStateActivity setState = FindActivityByQualifiedName(rootState, stateDesignerConnector.SetStateName) as SetStateActivity;
                if (setState != null)
                {
                    StateActivity targetState = (StateActivity)stateDesignerConnector.Target.AssociatedDesigner.Activity;
                    PropertyDescriptor property = GetPropertyDescriptor(setState, SetStateActivity.TargetStateNamePropertyName);
                    property.SetValue(setState, targetState.QualifiedName);
                    stateDesignerConnector.TargetStateName = targetState.QualifiedName;
                }
            }

            StateDesigner.DesignerLayoutConnectionPoint sourceConnectionPoint = (StateDesigner.DesignerLayoutConnectionPoint)stateDesignerConnector.Source;
            if (!sourceConnectionPoint.EventHandler.QualifiedName.Equals(stateDesignerConnector.EventHandlerName))
            {
                StateActivity rootState = (StateActivity)this.RootStateDesigner.Activity;
                // source state has changed
                SetStateActivity setState = FindActivityByQualifiedName(rootState, stateDesignerConnector.SetStateName) as SetStateActivity;
                if (setState != null)
                {
                    IDesignerHost designerHost = GetService(typeof(IDesignerHost)) as IDesignerHost;
                    DesignerTransaction transaction = null;
                    if (designerHost != null)
                        transaction = designerHost.CreateTransaction(SR.GetMoveSetState());

                    try
                    {
                        CompositeActivityDesigner previousSetStateParentDesigner = (CompositeActivityDesigner)StateDesigner.GetDesigner(setState.Parent);
                        List<Activity> activitiesToRemove = new List<Activity>();
                        activitiesToRemove.Add(setState);
                        previousSetStateParentDesigner.RemoveActivities(activitiesToRemove.AsReadOnly());

                        DesignerLayoutConnectionPoint source = (DesignerLayoutConnectionPoint)stateDesignerConnector.Source;
                        CompositeActivityDesigner newSetStateParentDesigner = (CompositeActivityDesigner)StateDesigner.GetDesigner(source.EventHandler);
                        List<Activity> activitiesToInsert = new List<Activity>();
                        activitiesToInsert.Add(setState);
                        newSetStateParentDesigner.InsertActivities(new HitTestInfo(newSetStateParentDesigner, HitTestLocations.Designer), activitiesToInsert.AsReadOnly());

                        stateDesignerConnector.EventHandlerName = source.EventHandler.QualifiedName;
                        stateDesignerConnector.SourceStateName = source.EventHandler.Parent.QualifiedName;

                        if (transaction != null)
                            transaction.Commit();
                    }
                    catch
                    {
                        if (transaction != null)
                            transaction.Cancel();

                        throw;
                    }
                }
            }
        }

        #region Drag & Drop

        protected override void OnDragEnter(ActivityDragEventArgs e)
        {
            base.OnDragEnter(e);

            this.DragDropActive = CanDrop(e);
            e.Effect = CheckDragEffect(e);
            e.DragImageSnapPoint = GetDragImageSnapPoint(e);
        }

        protected override void OnDragOver(ActivityDragEventArgs e)
        {
            base.OnDragOver(e);

            this.DragDropActive = CanDrop(e);
            e.Effect = CheckDragEffect(e);
            e.DragImageSnapPoint = GetDragImageSnapPoint(e);
        }

        protected override void OnDragLeave()
        {
            base.OnDragLeave();
            this.DragDropActive = false;
        }

        protected override void OnDragDrop(ActivityDragEventArgs e)
        {
            if (this.DragDropActive)
            {
                base.OnDragDrop(e);
                this.DragDropActive = false;
            }
        }

        #endregion

        #region Mouse events handlers

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            this.RootDesignerLayout.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            this.RootDesignerLayout.OnMouseUp(e);
        }

        protected override void OnMouseLeave()
        {
            base.OnMouseLeave();
            this.RootDesignerLayout.OnMouseLeave();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            this.RootDesignerLayout.OnMouseMove(e);
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            this.RootDesignerLayout.OnMouseDoubleClick(e);
        }

        #endregion Mouse event handlers

        protected override void OnContainedActivitiesChanged(ActivityCollectionChangeEventArgs listChangeArgs)
        {
            base.OnContainedActivitiesChanged(listChangeArgs);

            if (this.ActiveDesigner != null &&
                listChangeArgs.RemovedItems.Contains(this.ActiveDesigner.Activity))
            {
                SetActiveDesigner(null);
            }

            this.DesignersParser = null;
        }

        protected override void OnLayoutPosition(ActivityDesignerLayoutEventArgs e)
        {
            try
            {
                if (this.IsRootStateDesigner)
                {
                    if (this.ActiveDesigner == null)
                    {
                        // UpdateConnectors depends on having the 
                        // RootDesignerLayout refreshed at least once
                        this.UpdateConnectors();
                    }
                }

                Graphics graphics = e.Graphics;
                ActivityDesignerTheme designerTheme = e.DesignerTheme;
                AmbientTheme ambientTheme = e.AmbientTheme;
                this.RootDesignerLayout.Location = this.Location;
                this.RootDesignerLayout.OnLayoutPosition(graphics, designerTheme, ambientTheme);

                if (!this.HasActiveDesigner)
                    RelocateStates();

                base.OnLayoutPosition(e);

                if (!this.HasActiveDesigner && this.NeedsAutoLayout)
                    RepositionStates();

                if (IsRootDesigner && InvokingDesigner == null)
                    RecalculateRootDesignerSize();

            }
#if DEBUG
            catch (Exception exception)
            {
                Trace.WriteLine(String.Format(
                    System.Globalization.CultureInfo.InvariantCulture,
                    "Unhandled exception in {0}.OnLayoutPosition: {1}",
                    typeof(StateDesigner), exception));
                throw;
            }
#endif
            finally
            {
                if (this.IsRootStateDesigner)
                    this.PerformingLayout = false;
            }
        }

        private void RelocateStates()
        {
            // make sure that if we add event handlers, the states
            // are moved down so they're still visible
            int minimumY = _eventHandlersLayout.Bounds.Bottom + DefaultStateDesignerAutoLayoutDistance;
            int deltaY = 0;
            Rectangle moveBounds = Rectangle.Empty;
            int freeSpaceHeight = int.MaxValue;
            foreach (ActivityDesigner designer in this.ContainedDesigners)
            {
                if (IsContainedDesignerVisible(designer))
                {
                    StateDesigner stateDesigner = designer as StateDesigner;
                    if (stateDesigner != null)
                    {
                        if (stateDesigner.Location.Y < minimumY)
                        {
                            deltaY = Math.Max(deltaY, minimumY - stateDesigner.Location.Y);
                            if (moveBounds.IsEmpty)
                                moveBounds = stateDesigner.Bounds;
                            else
                                moveBounds = Rectangle.Union(moveBounds, stateDesigner.Bounds);
                        }
                        else
                        {
                            freeSpaceHeight = Math.Min(freeSpaceHeight, stateDesigner.Location.Y - minimumY);
                        }
                    }
                }
            }
            if (freeSpaceHeight == int.MaxValue)
                freeSpaceHeight = 0;

            if (deltaY > 0)
            {
                int maximumY = int.MinValue;
                foreach (ActivityDesigner designer in this.ContainedDesigners)
                {
                    if (IsContainedDesignerVisible(designer))
                    {
                        StateDesigner stateDesigner = designer as StateDesigner;
                        if (stateDesigner != null)
                        {
                            Point location = stateDesigner.Location;
                            if (stateDesigner.Location.Y < minimumY)
                                stateDesigner.Location = new Point(location.X, location.Y + deltaY);
                            else
                                stateDesigner.Location = new Point(location.X, location.Y + moveBounds.Height + DefaultStateDesignerAutoLayoutDistance - freeSpaceHeight);
                            maximumY = Math.Max(maximumY, stateDesigner.Bounds.Bottom);
                        }
                    }
                }
                if (maximumY > this.Bounds.Bottom)
                {
                    Size newSize = new Size(this.Size.Width, this.Size.Height + ((maximumY + DefaultStateDesignerAutoLayoutDistance) - this.Bounds.Bottom));
                    this.Size = newSize;
                }
            }
        }

        private void RepositionStates()
        {
            Debug.Assert(!this.HasActiveDesigner && this.NeedsAutoLayout);

            int maximumY = _eventHandlersLayout.Bounds.Bottom + DefaultStateDesignerAutoLayoutDistance;
            foreach (ActivityDesigner designer in this.ContainedDesigners)
            {
                if (IsContainedDesignerVisible(designer))
                {
                    StateDesigner stateDesigner = designer as StateDesigner;
                    if (stateDesigner == null)
                        continue;

                    int x = this.Location.X + DefaultStateDesignerAutoLayoutDistance;
                    int y = maximumY;
                    stateDesigner.Location = new Point(x, y);
                    maximumY = stateDesigner.Bounds.Bottom + DefaultStateDesignerAutoLayoutDistance;
                }
            }
            this.NeedsAutoLayout = false;
        }

        private void RecalculateRootDesignerSize()
        {
            Debug.Assert(IsRootDesigner && InvokingDesigner == null);

            // Make sure that the root designer has enough
            // space to show all contained designers
            Size newSize = Size.Empty;
            foreach (ActivityDesigner designer in this.ContainedDesigners)
            {
                if (IsContainedDesignerVisible(designer))
                {
                    Rectangle bounds = designer.Bounds;
                    bounds.Offset(Separator.Width - this.Location.X, Separator.Height - this.Location.Y);
                    newSize.Width = Math.Max(newSize.Width, bounds.Right);
                    newSize.Height = Math.Max(newSize.Height, bounds.Bottom);
                }
            }
            newSize.Width = Math.Max(newSize.Width, this.MinimumSize.Width);
            newSize.Height = Math.Max(newSize.Height, this.MinimumSize.Height);
            this.Size = newSize;
        }

        protected override Size OnLayoutSize(ActivityDesignerLayoutEventArgs e)
        {
#if DEBUG
            try
            {
#endif
            if (this.IsRootStateDesigner)
                this.PerformingLayout = true;

            if (this.HasActiveDesigner)
            {
                // If we are in the event driven view, 
                // then we need to make sure that the size
                // of this designer will be as small as possible
                _minimumSize = Size.Empty;
                this.Size = Size.Empty;
            }
            else
            {
                this.NeedsAutoLayout = this.Size.IsEmpty;
            }

            Size newSize = base.OnLayoutSize(e);

            Graphics graphics = e.Graphics;
            ActivityDesignerTheme designerTheme = e.DesignerTheme;
            AmbientTheme ambientTheme = e.AmbientTheme;

            RefreshRootDesignerLayout();

            this.RootDesignerLayout.OnLayoutSize(graphics, designerTheme, ambientTheme, newSize);
            _minimumSize = this.RootDesignerLayout.MinimumSize;

            return this.RootDesignerLayout.Size;
#if DEBUG
            }
            catch (Exception exception)
            {
                Trace.WriteLine(String.Format(
                    System.Globalization.CultureInfo.InvariantCulture,
                    "Unhandled exception in {0}.OnLayoutSize: {1}",
                    typeof(StateDesigner), exception));
                throw;
            }
#endif
        }

        protected override void OnPaint(ActivityDesignerPaintEventArgs e)
        {
            Graphics graphics = e.Graphics;
            ActivityDesignerTheme designerTheme = e.DesignerTheme;
            AmbientTheme ambientTheme = e.AmbientTheme;

#if DEBUG
            try
            {
#endif
            this.RootDesignerLayout.OnPaint(graphics, designerTheme, ambientTheme);
            this.PaintContainedDesigners(e);
#if DEBUG
            }
            catch (Exception exception)
            {
                Trace.WriteLine(String.Format(
                    System.Globalization.CultureInfo.InvariantCulture,
                    "Unhandled exception in {0}.OnPaint: {1}",
                    typeof(StateDesigner), exception));
            }
#endif
        }

        protected override void OnThemeChange(ActivityDesignerTheme newTheme)
        {
            base.OnThemeChange(newTheme);
            this.Image = GetDesignerImage(this);
        }

        #endregion

        #region Private Methods

        #region Drag & Drop

        private bool CanDrop(ActivityDragEventArgs e)
        {
            if (e.Activities.Count == 0)
                return false;

            if (this.HasActiveDesigner)
                return false;

            if (!CanInsertActivities(new HitTestInfo(this, HitTestLocations.Designer), e.Activities))
                return false;

            bool ctrlKeyPressed = ((e.KeyState & 8) == 8);
            if (!ctrlKeyPressed && (e.AllowedEffect & DragDropEffects.Move) == DragDropEffects.Move)
            {
                HitTestInfo moveLocation = new HitTestInfo(this, HitTestLocations.Designer);
                foreach (Activity activity in e.Activities)
                {
                    if (activity.Site != null)
                    {
                        ActivityDesigner activityDesigner = StateDesigner.GetDesigner(activity);
                        if (activityDesigner == null || activityDesigner.ParentDesigner == null || !activityDesigner.ParentDesigner.CanMoveActivities(moveLocation, new List<Activity>(new Activity[] { activity }).AsReadOnly()))
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        private DragDropEffects CheckDragEffect(ActivityDragEventArgs e)
        {
            if (e.Activities.Count == 0 || (!this.DragDropActive))
            {
                return DragDropEffects.None;
            }
            else
            {
                bool ctrlKeyPressed = ((e.KeyState & 8) == 8);
                if (ctrlKeyPressed && (e.AllowedEffect & DragDropEffects.Copy) == DragDropEffects.Copy)
                    return DragDropEffects.Copy;
                else if ((e.AllowedEffect & DragDropEffects.Move) == DragDropEffects.Move)
                    return DragDropEffects.Move;
            }

            return e.Effect;
        }

        private Point GetDragImageSnapPoint(ActivityDragEventArgs e)
        {
            Point snapPoint = new Point(e.Y, e.Y);
            if (!this.HasActiveDesigner)
            {
                int eventHandlersLayoutBottom = this._statesLayout.EventHandlersLayout.Bounds.Bottom;
                if (snapPoint.Y <= eventHandlersLayoutBottom)
                    snapPoint.Y = eventHandlersLayoutBottom + 1;
            }

            return snapPoint;
        }

        #endregion

        private void UpdateConnectors()
        {
            try
            {
                Debug.Assert(this.IsRootStateDesigner);
                Debug.Assert(this.ActiveDesigner == null);

                this.RootStateDesigner.RemovingSetState = false;

                StateActivity rootState = (StateActivity)this.Activity;
                ReadOnlyCollection<TransitionInfo> transitions = TransitionInfo.ParseStateMachine(rootState);
                Connector[] connectors = new Connector[this.Connectors.Count];
                this.Connectors.CopyTo(connectors, 0);
                foreach (Connector connector in connectors)
                {
                    StateDesignerConnector stateDesignerConnector = connector as StateDesignerConnector;
                    if (stateDesignerConnector == null)
                    {
                        RemoveConnector(connector);
                        continue;
                    }

                    bool foundMatchingTransition = false;
                    foreach (TransitionInfo transitionInfo in transitions)
                    {
                        if (transitionInfo.Matches(stateDesignerConnector))
                        {
                            transitionInfo.Connector = stateDesignerConnector;
                            foundMatchingTransition = true;
                            break;
                        }
                    }

                    if (!foundMatchingTransition)
                        RemoveConnector(connector);
                }

                foreach (TransitionInfo transitionInfo in transitions)
                {
                    if (transitionInfo.Connector == null && transitionInfo.TargetState != null)
                    {
                        DesignerLayoutConnectionPoint source = GetEventHandlerConnectionPoint(transitionInfo.EventHandler);
                        ConnectionPoint target = GetTargetStateConnectionPoint(transitionInfo.TargetState);

                        if (source != null && target != null)
                        {
                            this.RootStateDesigner.AddingSetState = false;
                            try
                            {
                                StateDesignerConnector stateDesignerConnector = (StateDesignerConnector)this.AddConnector(source, target);
                                stateDesignerConnector.SetStateName = transitionInfo.SetState.QualifiedName;
                                stateDesignerConnector.TargetStateName = transitionInfo.SetState.TargetStateName;
                                if (transitionInfo.EventHandler != null)
                                    stateDesignerConnector.EventHandlerName = transitionInfo.EventHandler.QualifiedName;
                            }
                            finally
                            {
                                this.RootStateDesigner.AddingSetState = true;
                            }
                        }
                    }
                }

            }
#if DEBUG
            catch (Exception exception)
            {
                Trace.WriteLine(String.Format(
                    System.Globalization.CultureInfo.InvariantCulture,
                    "Unhandled exception in {0}.UpdateConnectors: {1}",
                    typeof(StateDesigner), exception));
                throw;
            }
#endif
            finally
            {
                this.RemovingSetState = true;
            }
        }

        internal StateDesignerConnector FindConnector(TransitionInfo transitionInfo)
        {
            foreach (Connector connector in this.Connectors)
            {
                StateDesignerConnector stateDesignerConnector = connector as StateDesignerConnector;
                if (stateDesignerConnector != null)
                {
                    if (transitionInfo.Matches(stateDesignerConnector))
                        return stateDesignerConnector;
                }
            }
            return null;
        }

        private DesignerLayoutConnectionPoint GetEventHandlerConnectionPoint(CompositeActivity eventHandler)
        {
            Debug.Assert(eventHandler != null);
            StateDesigner sourceStateDesigner = (StateDesigner)GetDesigner(eventHandler.Parent);
            DesignerLayout eventHandlerLayout;
            if (!sourceStateDesigner.DesignerLayouts.TryGetValue(eventHandler, out eventHandlerLayout))
                return null;

            int connectionIndex = 0;
            foreach (DesignerLayout layout in sourceStateDesigner.DesignerLayouts.Values)
            {
                if (layout == eventHandlerLayout)
                {
                    // we add one so we connect to the Right side by default
                    break;
                }

                connectionIndex++;
            }

            return new DesignerLayoutConnectionPoint(sourceStateDesigner, connectionIndex, eventHandler, DesignerEdges.Right);
        }

        private ConnectionPoint GetTargetStateConnectionPoint(StateActivity targetState)
        {
            //
            Debug.Assert(targetState != null);
            StateDesigner targetStateDesigner = (StateDesigner)GetDesigner(targetState);
            return new ConnectionPoint(targetStateDesigner, DesignerEdges.Top, 0);
        }

        internal void StateDesignerLinkMouseDown(object sender, MouseEventArgs e)
        {
            IDesignerHost designerHost = GetService(typeof(IDesignerHost)) as IDesignerHost;
            DesignerTransaction transaction = null;
            if (designerHost != null)
                transaction = designerHost.CreateTransaction(SR.GetString(SR.UndoSwitchViews));

            try
            {
                ISelectionService selectionService = this.GetService(typeof(ISelectionService)) as ISelectionService;
                if (selectionService != null)
                    selectionService.SetSelectedComponents(new object[] { this.Activity }, SelectionTypes.Primary);

                SetLeafActiveDesigner(this, null);

                if (transaction != null)
                    transaction.Commit();
            }
            catch
            {
                if (transaction != null)
                    transaction.Cancel();

                throw;
            }
        }

        private void RefreshRootDesignerLayout()
        {
            if (!this.HasActiveDesigner)
            {
                _eventHandlersLayout.Layouts.Clear();
                this.DesignerLayouts.Clear();
                _designersParser = new ContainedDesignersParser(this.ContainedDesigners);

                foreach (StateInitializationDesigner stateInitializationDesigner in this.DesignersParser.StateInitializationDesigners)
                {
                    DesignerLayout layout = new DesignerLayout(stateInitializationDesigner);
                    this.DesignerLayouts[stateInitializationDesigner.Activity] = layout;
                    _eventHandlersLayout.Layouts.Add(layout);
                }

                // we now add the EventDrivenDesigners
                foreach (EventDrivenDesigner eventDrivenDesigner in this.DesignersParser.EventDrivenDesigners)
                {
                    DesignerLayout layout = new DesignerLayout(eventDrivenDesigner);
                    this.DesignerLayouts[eventDrivenDesigner.Activity] = layout;
                    _eventHandlersLayout.Layouts.Add(layout);
                }

                foreach (StateFinalizationDesigner stateFinalizationDesigner in this.DesignersParser.StateFinalizationDesigners)
                {
                    DesignerLayout layout = new DesignerLayout(stateFinalizationDesigner);
                    this.DesignerLayouts[stateFinalizationDesigner.Activity] = layout;
                    _eventHandlersLayout.Layouts.Add(layout);
                }

                this.RootDesignerLayout = _statesLayout;
            }
            else
                this.RootDesignerLayout = _eventDrivenLayout;
        }

        internal void OnStateMachineView(object sender, EventArgs e)
        {
            SetLeafActiveDesigner(this, null);
        }

        internal void OnSetAsInitialState(object sender, EventArgs e)
        {
            IDesignerHost designerHost = GetService(typeof(IDesignerHost)) as IDesignerHost;
            DesignerTransaction transaction = null;
            StateActivity state = (StateActivity)this.Activity;
            if (designerHost != null)
                transaction = designerHost.CreateTransaction(SR.GetUndoSetAsInitialState(state.Name));

            try
            {
                StateActivity rootState = StateMachineHelpers.GetRootState(state);

                PropertyDescriptor initialStateProperty = GetPropertyDescriptor(rootState, StateMachineWorkflowActivity.InitialStateNamePropertyName);
                initialStateProperty.SetValue(rootState, state.Name);

                string completedStateName = StateMachineHelpers.GetCompletedStateName(rootState);
                if (completedStateName == state.Name)
                {
                    PropertyDescriptor completedStateProperty = GetPropertyDescriptor(rootState, StateMachineWorkflowActivity.CompletedStateNamePropertyName);
                    completedStateProperty.SetValue(rootState, "");
                }

                if (transaction != null)
                    transaction.Commit();
            }
            catch
            {
                if (transaction != null)
                    transaction.Cancel();

                throw;
            }
        }

        internal void OnSetAsCompletedState(object sender, EventArgs e)
        {
            IDesignerHost designerHost = GetService(typeof(IDesignerHost)) as IDesignerHost;
            DesignerTransaction transaction = null;
            StateActivity state = (StateActivity)this.Activity;
            if (designerHost != null)
                transaction = designerHost.CreateTransaction(SR.GetUndoSetAsCompletedState(state.Name));

            try
            {
                StateActivity rootState = StateMachineHelpers.GetRootState(state);

                PropertyDescriptor completedStateProperty = GetPropertyDescriptor(rootState, StateMachineWorkflowActivity.CompletedStateNamePropertyName);
                completedStateProperty.SetValue(rootState, state.Name);

                string initialStateName = StateMachineHelpers.GetInitialStateName(rootState);
                if (initialStateName == state.Name)
                {
                    PropertyDescriptor initialStateProperty = GetPropertyDescriptor(rootState, StateMachineWorkflowActivity.InitialStateNamePropertyName);
                    initialStateProperty.SetValue(rootState, "");
                }

                if (transaction != null)
                    transaction.Commit();
            }
            catch
            {
                if (transaction != null)
                    transaction.Cancel();

                throw;
            }
        }

        private static PropertyDescriptor GetPropertyDescriptor(Activity activity, string propertyName)
        {
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(activity);
            PropertyDescriptor property = properties.Find(propertyName, false);
            return property;
        }

        internal void OnAddEventDriven(object sender, EventArgs e)
        {
            EventDrivenActivity eventDriven = new EventDrivenActivity();
            AddChild(eventDriven);
        }

        internal void OnAddState(object sender, EventArgs e)
        {
            StateActivity state = new StateActivity();
            AddChild(state);
        }

        internal void OnAddStateInitialization(object sender, EventArgs e)
        {
            StateInitializationActivity stateInitialization = new StateInitializationActivity();
            AddChild(stateInitialization);
        }

        internal void OnAddStateFinalization(object sender, EventArgs e)
        {
            StateFinalizationActivity stateFinalization = new StateFinalizationActivity();
            AddChild(stateFinalization);
        }

        internal void AddChild(Activity child)
        {
            CompositeActivity compositeActivity = this.Activity as CompositeActivity;
            if (compositeActivity != null && child != null)
            {
                // Record the current number of child activities
                int designerCount = ContainedDesigners.Count;

                HitTestInfo hitTestInfo = new HitTestInfo(this, HitTestLocations.Designer);

                CompositeActivityDesigner.InsertActivities(
                    this,
                    hitTestInfo,
                    new List<Activity>(new Activity[] { child }).AsReadOnly(),
                    string.Format(System.Globalization.CultureInfo.InvariantCulture,
                    DR.GetString(DR.AddingChild),
                    child.GetType().Name));

                // If the number of child activities has increased, the branch add was successful, so
                // make sure the highest indexed branch is visible
                if (ContainedDesigners.Count > designerCount && ContainedDesigners.Count > 0)
                    ContainedDesigners[ContainedDesigners.Count - 1].EnsureVisible();

                this.SelectionService.SetSelectedComponents(new object[] { child }, SelectionTypes.Primary);
            }
        }

        // 





        private bool GetIsEditable()
        {
            return true;
        }

        internal void OnStatusSetAsInitialState(object sender, EventArgs e)
        {
            ActivityDesignerVerb verb = sender as ActivityDesignerVerb;
            if (verb != null)
            {
                bool enabled = false;
                if (!this.HasActiveDesigner)
                {
                    StateActivity state = (StateActivity)this.Activity;
                    StateActivity rootState = StateMachineHelpers.GetRootState(state);
                    enabled = StateMachineHelpers.IsLeafState(state) &&
                        StateMachineHelpers.IsStateMachine(rootState) &&
                        !StateMachineHelpers.IsInitialState(state);
                }
                verb.Visible = enabled;
                verb.Enabled = enabled;
            }
        }

        internal void OnStatusSetAsCompletedState(object sender, EventArgs e)
        {
            ActivityDesignerVerb verb = sender as ActivityDesignerVerb;
            if (verb != null)
            {
                bool enabled = false;
                if (!this.HasActiveDesigner)
                {
                    StateActivity state = (StateActivity)this.Activity;
                    StateActivity rootState = StateMachineHelpers.GetRootState(state);
                    enabled =
                        StateMachineHelpers.IsLeafState(state) &&
                        StateMachineHelpers.IsStateMachine(rootState) &&
                        !StateMachineHelpers.IsCompletedState(state) &&
                        (state.Activities.Count == 0);
                }
                verb.Visible = enabled;
                verb.Enabled = enabled;
            }
        }
        internal void OnStatusAddState(object sender, EventArgs e)
        {
            ActivityDesignerVerb verb = sender as ActivityDesignerVerb;
            if (verb != null)
            {
                bool enabled;
                StateActivity state = (StateActivity)this.Activity;
                bool isInitialState = false;
                bool isCompletedState = false;
                if (StateMachineHelpers.IsLeafState(state))
                {
                    isInitialState = StateMachineHelpers.IsInitialState(state);
                    isCompletedState = StateMachineHelpers.IsCompletedState(state);
                }

                enabled = GetIsEditable() && (!this.HasActiveDesigner) && !isInitialState && !isCompletedState && !IsLocked && !IsStateCustomActivity;
                verb.Visible = enabled;
                verb.Enabled = enabled;
            }
        }

        internal void OnStatusAddEventDriven(object sender, EventArgs e)
        {
            ActivityDesignerVerb verb = sender as ActivityDesignerVerb;
            if (verb != null)
            {
                bool enabled;
                StateActivity state = (StateActivity)this.Activity;
                bool isCompletedState = (StateMachineHelpers.IsLeafState(state) && StateMachineHelpers.IsCompletedState(state));
                enabled = GetIsEditable() && (!this.HasActiveDesigner) && !isCompletedState && !IsLocked && !IsStateCustomActivity;
                verb.Visible = enabled;
                verb.Enabled = enabled;
            }
        }

        internal void OnStatusAddStateInitialization(object sender, EventArgs e)
        {
            ActivityDesignerVerb verb = sender as ActivityDesignerVerb;
            if (verb != null)
            {
                bool enabled;
                StateActivity state = (StateActivity)this.Activity;
                bool isLeafState = (StateMachineHelpers.IsLeafState(state));
                bool isCompletedState = (isLeafState && StateMachineHelpers.IsCompletedState(state));
                bool hasStateInitialization = this.DesignersParser.StateInitializationDesigners.Count > 0;
                enabled = GetIsEditable() && (!this.HasActiveDesigner) && isLeafState && !isCompletedState && !hasStateInitialization && !IsLocked && !IsStateCustomActivity;
                verb.Visible = enabled;
                verb.Enabled = enabled;
            }
        }

        internal void OnStatusAddStateFinalization(object sender, EventArgs e)
        {
            ActivityDesignerVerb verb = sender as ActivityDesignerVerb;
            if (verb != null)
            {
                bool enabled;
                StateActivity state = (StateActivity)this.Activity;
                bool isLeafState = (StateMachineHelpers.IsLeafState(state));
                bool isCompletedState = (isLeafState && StateMachineHelpers.IsCompletedState(state));
                bool hasStateFinalization = this.DesignersParser.StateFinalizationDesigners.Count > 0;
                enabled = GetIsEditable() && (!this.HasActiveDesigner) && isLeafState && !isCompletedState && !hasStateFinalization && !IsLocked && !IsStateCustomActivity;
                verb.Visible = enabled;
                verb.Enabled = enabled;
            }
        }

        internal void OnStatusStateMachineView(object sender, EventArgs e)
        {
            ActivityDesignerVerb verb = sender as ActivityDesignerVerb;
            if (verb != null)
            {
                bool enabled = this.HasActiveDesigner;
                verb.Visible = enabled;
                verb.Enabled = enabled;
            }
        }

        private void SelectionChanged(object sender, EventArgs e)
        {
            if (this.HasActiveDesigner)
            {
                StateActivity state = (StateActivity)this.Activity;
                Activity selection = this.SelectionService.PrimarySelection as Activity;
                if (selection != null &&
                    state.Activities.Contains(selection) &&
                    this.ActiveDesigner.Activity != selection)
                {
                    ActivityDesigner activityDesigner = GetDesigner(selection);
                    if (!(activityDesigner is StateDesigner))
                        SetActiveDesigner(activityDesigner);
                }
            }
            else
            {
                if (this.Activity == this.SelectionService.PrimarySelection)
                    RefreshDesignerVerbs();
            }
        }

        private void SetActiveDesigner(ActivityDesigner designer)
        {
            string activeDesignerName = null;
            if (designer == null)
            {
                if (!this.HasActiveDesigner)
                    return; // Nothing to do
            }
            else
            {
                activeDesignerName = designer.Activity.QualifiedName;
                if (this.HasActiveDesigner && (this.ActiveDesigner.Activity.QualifiedName == activeDesignerName))
                    return; // Nothing to do. 
            }

            IDesignerHost designerHost = GetService(typeof(IDesignerHost)) as IDesignerHost;
            DesignerTransaction transaction = null;
            if (designerHost != null)
                transaction = designerHost.CreateTransaction(SR.GetString(SR.UndoSwitchViews));

            try
            {
                StateDesigner rootDesigner = this.RootStateDesigner;
                SetLeafActiveDesigner(rootDesigner, null);
                SetActiveDesignerHelper(this, designer);

                if (transaction != null)
                    transaction.Commit();
            }
            catch
            {
                if (transaction != null)
                    transaction.Cancel();

                throw;
            }
        }

        private void SetLeafActiveDesigner(StateDesigner parentDesigner, ActivityDesigner activityDesigner)
        {
            StateDesigner stateDesigner = parentDesigner;
            while (true)
            {
                StateDesigner childStateDesigner = stateDesigner.ActiveDesigner as StateDesigner;
                if (childStateDesigner == null)
                    break;
                stateDesigner = childStateDesigner;
            }
            SetActiveDesignerHelper(stateDesigner, activityDesigner);
        }

        private void SetParentTreeEnsuringVisible(bool value)
        {
            _ensuringVisible = value;
            StateDesigner stateDesigner = this.ParentDesigner as StateDesigner;
            while (stateDesigner != null)
            {
                stateDesigner._ensuringVisible = value;
                stateDesigner = stateDesigner.ParentDesigner as StateDesigner;
            }
        }

        private void SetActiveDesignerByName(string activeDesignerName)
        {
            ActivityDesigner activeDesigner = null;
            if (!String.IsNullOrEmpty(activeDesignerName))
            {
                foreach (ActivityDesigner activityDesigner in this.ContainedDesigners)
                {
                    if (activityDesigner.Activity.QualifiedName == activeDesignerName)
                    {
                        activeDesigner = activityDesigner;
                        break;
                    }
                }
            }
            this.ActiveDesigner = activeDesigner;
        }

        private void SetActiveDesignerHelper(StateDesigner stateDesigner, ActivityDesigner activeDesigner)
        {
            WorkflowDesignerLoader workflowDesignerLoader = GetService(typeof(WorkflowDesignerLoader)) as WorkflowDesignerLoader;
            if (workflowDesignerLoader != null && workflowDesignerLoader.InDebugMode)
            {
                stateDesigner.ActiveDesigner = activeDesigner;
            }
            else
            {
                PropertyDescriptor activeDesignerProperty = GetPropertyDescriptor(stateDesigner.Activity, ActiveDesignerNamePropertyName);
                if (activeDesigner == null)
                    activeDesignerProperty.SetValue(stateDesigner.Activity, null);
                else
                    activeDesignerProperty.SetValue(stateDesigner.Activity, activeDesigner.Activity.QualifiedName);
            }
        }

        #endregion Private Methods

        #region Static Private Methods
        private static object GetService(ActivityDesigner designer, Type serviceType)
        {
            if (designer == null)
                throw new ArgumentNullException("designer");

            if (serviceType == null)
                throw new ArgumentNullException("serviceType");

            Activity activity = designer.Activity;

            object service = null;
            if (activity != null && activity.Site != null)
            {
                service = activity.Site.GetService(serviceType);
            }

            return service;
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

        internal static Image GetDesignerImage(ActivityDesigner designer)
        {
            Image image = null;
            if (designer.DesignerTheme != null && designer.DesignerTheme.DesignerImage != null)
                image = designer.DesignerTheme.DesignerImage;
            else
                if (designer.Image == null)
                    image = ActivityToolboxItem.GetToolboxImage(designer.Activity.GetType());
                else
                    image = designer.Image;

            return image;
        }

        internal static Image GetInitialStateDesignerImage(StateDesigner stateDesigner)
        {
            StateMachineTheme stateDesignerTheme = stateDesigner.DesignerTheme as StateMachineTheme;
            if (stateDesignerTheme != null && stateDesignerTheme.InitialStateDesignerImage != null)
                return stateDesignerTheme.InitialStateDesignerImage;
            else
                return StateDesigner.InitialState;
        }

        internal static Image GetCompletedStateDesignerImage(StateDesigner stateDesigner)
        {
            StateMachineTheme stateDesignerTheme = stateDesigner.DesignerTheme as StateMachineTheme;
            if (stateDesignerTheme != null && stateDesignerTheme.CompletedStateDesignerImage != null)
                return stateDesignerTheme.CompletedStateDesignerImage;
            else
                return StateDesigner.CompletedState;
        }

        internal object OnGetPropertyValue(ExtendedPropertyInfo extendedProperty, object extendee)
        {
            object value = null;
            if (extendedProperty.Name.Equals("Location", StringComparison.Ordinal))
                value = (ActiveDesigner == null) ? Location : this._stateLocation;
            else if (extendedProperty.Name.Equals("Size", StringComparison.Ordinal))
                value = (ActiveDesigner == null) ? Size : this._stateSize;
            return value;
        }

        // We have this method here, instead of using Activity.GetActivityByName
        // because the Activity method ignores activities like FaultHandlers.
        // When in designer mode, we need to search the entire activity tree.
        private static Activity FindActivityByQualifiedName(Activity activity, string qualifiedName)
        {
            Queue<Activity> activities = new Queue<Activity>();
            activities.Enqueue(activity);
            while (activities.Count > 0)
            {
                activity = activities.Dequeue();
                if (activity.QualifiedName.Equals(qualifiedName))
                    return activity;

                CompositeActivity compositeActivity = activity as CompositeActivity;
                if (compositeActivity != null)
                    foreach (Activity childActivity in compositeActivity.Activities)
                        activities.Enqueue(childActivity);
            }
            return null;
        }

        #endregion

        #endregion Methods

        #region Class StateDesignerPropertyExtender
        [ProvideProperty("ActiveDesignerName", typeof(Activity))]
        private sealed class StateDesignerPropertyExtender : IExtenderProvider
        {
            #region Properties
            [DesignOnly(true)]
            [MergableProperty(false)]
            [Browsable(false)]
            [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public string GetActiveDesignerName(Activity activity)
            {
                string activeDesignerName = null;
                StateDesigner designer = (StateDesigner)StateDesigner.GetDesigner(activity);
                if (designer != null)
                {
                    ActivityDesigner activeDesigner = designer.ActiveDesigner;
                    if (activeDesigner != null)
                    {
                        activeDesignerName = activeDesigner.Activity.QualifiedName;
                    }
                }
                return activeDesignerName;
            }

            public void SetActiveDesignerName(Activity activity, string activeDesignerName)
            {
                StateDesigner designer = (StateDesigner)StateDesigner.GetDesigner(activity);
                if (designer != null)
                {
                    designer.SetActiveDesignerByName(activeDesignerName);
                }
            }


            #endregion
            bool IExtenderProvider.CanExtend(object extendee)
            {
                bool canExtend = false;

                StateActivity activity = extendee as StateActivity;
                if (activity != null)
                {
                    StateDesigner designer = StateDesigner.GetDesigner(activity) as StateDesigner;
                    if (designer != null)
                    {
                        canExtend = true;
                    }
                }
                return canExtend;
            }
        }
        #endregion
    }
    #endregion StateDesigner Class

    #region Class StateDesignerLayoutSerializer
    internal class StateDesignerLayoutSerializer : FreeformActivityDesignerLayoutSerializer
    {
        protected override PropertyInfo[] GetProperties(WorkflowMarkupSerializationManager serializationManager, object obj)
        {
            List<PropertyInfo> properties = new List<PropertyInfo>();
            StateDesigner stateDesigner = obj as StateDesigner;
            if (stateDesigner != null)
            {
                foreach (PropertyInfo property in base.GetProperties(serializationManager, obj))
                {
                    if (property.Name.Equals("Location", StringComparison.Ordinal) ||
                        property.Name.Equals("Size", StringComparison.Ordinal))
                    {
                        properties.Add(new ExtendedPropertyInfo(property, new GetValueHandler(stateDesigner.OnGetPropertyValue)));
                    }
                    else
                    {
                        properties.Add(property);
                    }
                }
            }
            else
            {
                properties.AddRange(base.GetProperties(serializationManager, obj));
            }

            return properties.ToArray();
        }
    }
    #endregion

    #region Class ExtendedPropertyInfo
    //
    internal delegate object GetValueHandler(ExtendedPropertyInfo extendedProperty, object extendee);

    internal sealed class ExtendedPropertyInfo : PropertyInfo
    {
        #region Members and Constructors
        private PropertyInfo realPropertyInfo = null;
        private GetValueHandler OnGetValue;

        internal ExtendedPropertyInfo(PropertyInfo propertyInfo, GetValueHandler getValueHandler)
        {
            this.realPropertyInfo = propertyInfo;
            this.OnGetValue = getValueHandler;
        }
        #endregion

        #region Property Info overrides
        public override string Name
        {
            get
            {
                return this.realPropertyInfo.Name;
            }
        }

        public override Type DeclaringType
        {
            get
            {
                return this.realPropertyInfo.DeclaringType;
            }
        }

        public override Type ReflectedType
        {
            get
            {
                return this.realPropertyInfo.ReflectedType;
            }
        }

        public override Type PropertyType
        {
            get
            {
                return this.realPropertyInfo.PropertyType;
            }
        }

        public override MethodInfo[] GetAccessors(bool nonPublic)
        {
            return this.realPropertyInfo.GetAccessors(nonPublic);
        }

        public override MethodInfo GetGetMethod(bool nonPublic)
        {
            return this.realPropertyInfo.GetGetMethod(nonPublic);
        }

        public override MethodInfo GetSetMethod(bool nonPublic)
        {
            return this.realPropertyInfo.GetSetMethod(nonPublic);
        }

        public override object GetValue(object obj, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
        {
            if (OnGetValue != null)
                return OnGetValue(this, obj);
            else
                return null;
        }

        public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
        {
            this.realPropertyInfo.SetValue(obj, value, invokeAttr, binder, index, culture);
        }

        public override ParameterInfo[] GetIndexParameters()
        {
            return this.realPropertyInfo.GetIndexParameters();
        }

        public override PropertyAttributes Attributes
        {
            get
            {
                return this.realPropertyInfo.Attributes;
            }
        }

        public override bool CanRead
        {
            get
            {
                return this.realPropertyInfo.CanRead;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return this.realPropertyInfo.CanWrite;
            }
        }
        #endregion

        #region MemberInfo Overrides
        public override object[] GetCustomAttributes(bool inherit)
        {
            return this.realPropertyInfo.GetCustomAttributes(inherit);
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            return this.realPropertyInfo.GetCustomAttributes(attributeType, inherit);
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return this.realPropertyInfo.IsDefined(attributeType, inherit);
        }
        #endregion


    }
    #endregion

    #region Class ImageBrowserEditor
    internal sealed class ImageBrowserEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.AddExtension = true;
            fileDialog.DefaultExt = StateMachineTheme.DefaultThemeFileExtension;
            fileDialog.CheckFileExists = true;
            fileDialog.Filter = DR.GetString(DR.ImageFileFilter);
            if (fileDialog.ShowDialog() == DialogResult.OK)
                return fileDialog.FileName;
            else
                return value;
        }
    }
    #endregion

    #region StateMachineTheme
    internal class StateMachineTheme : CompositeDesignerTheme
    {
        internal const string DefaultThemeFileExtension = "*.wtm";

        private Color _connectorColor = Color.Black;
        private Pen _connectorPen;
        private Size _connectorSize = new Size(20, 20);
        private string _initialStateDesignerImagePath;
        private string _completedStateDesignerImagePath;
        private Image _initialStateDesignerImage;
        private Image _completedStateDesignerImage;

        public StateMachineTheme(WorkflowTheme theme)
            : base(theme)
        {
        }

        public override Size ConnectorSize
        {
            get
            {
                return _connectorSize;
            }
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (this._connectorPen != null)
                {
                    this._connectorPen.Dispose();
                    this._connectorPen = null;
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        [SRDescription(SR.ConnectorColorDescription)]
        [SRCategory(SR.ForegroundCategory)]
        public Color ConnectorColor
        {
            get
            {
                return _connectorColor;
            }
            set
            {
                _connectorColor = value;
            }
        }

        [SRDescription(SR.InitialStateImagePathDescription)]
        [SRCategory(SR.ForegroundCategory)]
        [Editor(typeof(ImageBrowserEditor), typeof(UITypeEditor))]
        public virtual string InitialStateDesignerImagePath
        {
            get
            {
                return _initialStateDesignerImagePath;
            }
            set
            {
                if (ReadOnly)
                    throw new InvalidOperationException(DR.GetString(DR.ThemePropertyReadOnly));

                if (value != null && value.Length > 0 && value.Contains(Path.DirectorySeparatorChar.ToString()) && Path.IsPathRooted(value))
                {
                    value = GetRelativePath(ContainingTheme.ContainingFileDirectory, value);

                    if (!IsValidImageResource(this, ContainingTheme.ContainingFileDirectory, value))
                        throw new InvalidOperationException(DR.GetString(DR.Error_InvalidImageResource));
                }

                this._initialStateDesignerImagePath = value;
                if (this._initialStateDesignerImage != null)
                {
                    this._initialStateDesignerImage.Dispose();
                    this._initialStateDesignerImage = null;
                }
            }
        }

        [SRDescription(SR.CompletedStateImagePathDescription)]
        [SRCategory(SR.ForegroundCategory)]
        [Editor(typeof(ImageBrowserEditor), typeof(UITypeEditor))]
        public virtual string CompletedStateDesignerImagePath
        {
            get
            {
                return _completedStateDesignerImagePath;
            }
            set
            {
                if (ReadOnly)
                    throw new InvalidOperationException(DR.GetString(DR.ThemePropertyReadOnly));

                if (value != null && value.Length > 0 && value.Contains(Path.DirectorySeparatorChar.ToString()) && Path.IsPathRooted(value))
                {
                    value = GetRelativePath(ContainingTheme.ContainingFileDirectory, value);

                    if (!IsValidImageResource(this, ContainingTheme.ContainingFileDirectory, value))
                        throw new InvalidOperationException(DR.GetString(DR.Error_InvalidImageResource));
                }

                this._completedStateDesignerImagePath = value;
                if (this._completedStateDesignerImage != null)
                {
                    this._completedStateDesignerImage.Dispose();
                    this._completedStateDesignerImage = null;
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public Pen ConnectorPen
        {
            get
            {
                if (this._connectorPen == null)
                    this._connectorPen = new Pen(this._connectorColor, BorderWidth);
                return this._connectorPen;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public Image InitialStateDesignerImage
        {
            get
            {
                if (_initialStateDesignerImage == null && !String.IsNullOrEmpty(_initialStateDesignerImagePath))
                    _initialStateDesignerImage = GetImageFromPath(this, ContainingTheme.ContainingFileDirectory, _initialStateDesignerImagePath);
                return _initialStateDesignerImage;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public Image CompletedStateDesignerImage
        {
            get
            {
                if (_completedStateDesignerImage == null && !String.IsNullOrEmpty(_completedStateDesignerImagePath))
                    _completedStateDesignerImage = GetImageFromPath(this, ContainingTheme.ContainingFileDirectory, _completedStateDesignerImagePath);
                return _completedStateDesignerImage;
            }
        }

        internal static bool IsValidImageResource(DesignerTheme designerTheme, string directory, string path)
        {
            Image image = GetImageFromPath(designerTheme, directory, path);
            bool validImage = (image != null);
            if (image != null)
                image.Dispose();
            return validImage;
        }

        internal static string GetRelativePath(string pathFrom, string pathTo)
        {
            Uri uri = new Uri(pathFrom);
            string relativePath = Uri.UnescapeDataString(uri.MakeRelativeUri(new Uri(pathTo)).ToString());
            relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            if (!relativePath.Contains(Path.DirectorySeparatorChar.ToString()))
                relativePath = "." + Path.DirectorySeparatorChar + relativePath;
            return relativePath;
        }

        internal static Image GetImageFromPath(DesignerTheme designerTheme, string directory, string path)
        {
            Bitmap image = null;
            if (path.Contains(Path.DirectorySeparatorChar.ToString()) && directory.Length > 0)
            {
                string imageFilePath = System.Web.HttpUtility.UrlDecode((new Uri(new Uri(directory), path).LocalPath));
                if (File.Exists(imageFilePath))
                {
                    try
                    {
                        image = new Bitmap(imageFilePath);
                    }
                    catch
                    {
                    }
                }
            }
            else if (designerTheme.DesignerType != null)
            {
                int index = path.LastIndexOf('.');
                if (index > 0)
                {
                    string nameSpace = path.Substring(0, index);
                    string name = path.Substring(index + 1);
                    if (nameSpace != null && nameSpace.Length > 0 &&
                        name != null && name.Length > 0)
                    {
                        try
                        {
                            ResourceManager resourceManager = new ResourceManager(nameSpace, designerTheme.DesignerType.Assembly);
                            image = resourceManager.GetObject(name) as Bitmap;
                        }
                        catch
                        {
                        }
                    }
                }
            }

            if (image != null)
                image.MakeTransparent(DR.TransparentColor);

            return image;
        }

    }
    #endregion

    #region StateDesignerTheme
    internal sealed class StateDesignerTheme : StateMachineTheme
    {
        public StateDesignerTheme(WorkflowTheme theme)
            : base(theme)
        {
            this.ShowDropShadow = false;
            this.ConnectorStartCap = LineAnchor.DiamondAnchor;
            this.ConnectorEndCap = LineAnchor.ArrowAnchor;
            this.ForeColor = Color.FromArgb(0xFF, 0x10, 0x10, 0x10);
            this.BorderColor = Color.FromArgb(0xFF, 0x49, 0x77, 0xb4);
            this.BorderStyle = DashStyle.Solid;
            this.BackColorStart = Color.FromArgb(0xd0, 0xff, 0xff, 0xff);
            this.BackColorEnd = Color.FromArgb(0xd0, 0xff, 0xff, 0xff);
        }
    }
    #endregion
}
