#pragma warning disable 1634, 1691

namespace System.Workflow.ComponentModel
{
    #region Imports

    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Xml;
    using System.Text;
    using System.IO;
    using System.Reflection;
    using System.Collections;
    using System.Runtime.Serialization;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.ComponentModel.Design.Serialization;
    using System.Drawing.Design;
    using System.Workflow.ComponentModel.Design;
    using System.Workflow.ComponentModel.Compiler;
    using System.Resources;
    using System.Globalization;
    using System.Diagnostics;
    using System.Collections.Specialized;
    using System.Collections.ObjectModel;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Workflow.ComponentModel.Serialization;

    #endregion

    #region Classes ActivityResolveEventArgs and WorkflowChangeActionsResolveEventArgs

    internal delegate Activity ActivityResolveEventHandler(object sender, ActivityResolveEventArgs e);
    internal delegate ArrayList WorkflowChangeActionsResolveEventHandler(object sender, WorkflowChangeActionsResolveEventArgs e);

    internal sealed class ActivityResolveEventArgs : EventArgs
    {
        private Type activityType = null;
        private string activityDefinition = null;
        private string rulesDefinition = null;
        private bool createNew = false;
        private bool initForRuntime = true;
        private IServiceProvider serviceProvider = null;

        internal ActivityResolveEventArgs(Type activityType, string workflowMarkup, string rulesMarkup, bool createNew, bool initForRuntime, IServiceProvider serviceProvider)
        {
            if (!(string.IsNullOrEmpty(workflowMarkup) ^ activityType == null))
                throw new ArgumentException(SR.GetString(SR.Error_WrongParamForActivityResolveEventArgs));

            this.activityType = activityType;
            this.activityDefinition = workflowMarkup;
            this.rulesDefinition = rulesMarkup;
            this.createNew = createNew;
            this.initForRuntime = initForRuntime;
            this.serviceProvider = serviceProvider;
        }

        public Type Type
        {
            get
            {
                return this.activityType;
            }
        }
        public string WorkflowMarkup
        {
            get
            {
                return this.activityDefinition;
            }
        }
        public string RulesMarkup
        {
            get
            {
                return this.rulesDefinition;
            }
        }
        public bool CreateNewDefinition
        {
            get
            {
                return this.createNew;
            }
        }
        public bool InitializeForRuntime
        {
            get
            {
                return this.initForRuntime;
            }
        }
        public IServiceProvider ServiceProvider
        {
            get
            {
                return this.serviceProvider;
            }
        }
    }

    internal sealed class WorkflowChangeActionsResolveEventArgs : EventArgs
    {
        private string workflowChangesMarkup;

        public WorkflowChangeActionsResolveEventArgs(string workflowChangesMarkup)
        {
            this.workflowChangesMarkup = workflowChangesMarkup;
        }

        public string WorkflowChangesMarkup
        {
            get
            {
                return this.workflowChangesMarkup;
            }
        }
    }

    #endregion

    #region Class Activity


    [ActivityCodeGenerator(typeof(ActivityCodeGenerator))]
    [ActivityValidator(typeof(ActivityValidator))]
    [System.Drawing.ToolboxBitmap(typeof(Activity), "Design.Resources.Activity.png")]
    [ToolboxItemFilter("Microsoft.Workflow.VSDesigner", ToolboxItemFilterType.Require)]
    [ToolboxItemFilter("System.Workflow.ComponentModel.Design.ActivitySet", ToolboxItemFilterType.Allow)]
    [DesignerSerializer(typeof(ActivityMarkupSerializer), typeof(WorkflowMarkupSerializer))]
    [DesignerSerializer(typeof(ActivityCodeDomSerializer), typeof(CodeDomSerializer))]
    [DesignerSerializer(typeof(ActivityTypeCodeDomSerializer), typeof(TypeCodeDomSerializer))]
    [DesignerCategory("Component")]
    [ActivityExecutor(typeof(ActivityExecutor<Activity>))]
    [Designer(typeof(ActivityDesigner), typeof(IDesigner))]
    [Designer(typeof(ActivityDesigner), typeof(IRootDesigner))]
    [ToolboxItem(typeof(ActivityToolboxItem))]
    [RuntimeNameProperty("Name")]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class Activity : DependencyObject
    {
        private static DependencyProperty NameProperty = DependencyProperty.Register("Name", typeof(string), typeof(Activity), new PropertyMetadata("", DependencyPropertyOptions.Metadata, new ValidationOptionAttribute(ValidationOption.Required)));
        private static DependencyProperty DescriptionProperty = DependencyProperty.Register("Description", typeof(string), typeof(Activity), new PropertyMetadata("", DependencyPropertyOptions.Metadata));
        private static DependencyProperty EnabledProperty = DependencyProperty.Register("Enabled", typeof(bool), typeof(Activity), new PropertyMetadata(true, DependencyPropertyOptions.Metadata));
        private static DependencyProperty QualifiedNameProperty = DependencyProperty.Register("QualifiedName", typeof(string), typeof(Activity), new PropertyMetadata(DependencyPropertyOptions.Metadata | DependencyPropertyOptions.ReadOnly));
        private static DependencyProperty DottedPathProperty = DependencyProperty.Register("DottedPath", typeof(string), typeof(Activity), new PropertyMetadata(DependencyPropertyOptions.Metadata | DependencyPropertyOptions.ReadOnly));
        internal static readonly DependencyProperty WorkflowXamlMarkupProperty = DependencyProperty.Register("WorkflowXamlMarkup", typeof(string), typeof(Activity));
        internal static readonly DependencyProperty WorkflowRulesMarkupProperty = DependencyProperty.Register("WorkflowRulesMarkup", typeof(string), typeof(Activity));

        internal static readonly DependencyProperty SynchronizationHandlesProperty = DependencyProperty.Register("SynchronizationHandles", typeof(ICollection<String>), typeof(Activity), new PropertyMetadata(DependencyPropertyOptions.Metadata));

        internal static readonly DependencyProperty ActivityExecutionContextInfoProperty = DependencyProperty.RegisterAttached("ActivityExecutionContextInfo", typeof(ActivityExecutionContextInfo), typeof(Activity));
        public static readonly DependencyProperty ActivityContextGuidProperty = DependencyProperty.RegisterAttached("ActivityContextGuid", typeof(Guid), typeof(Activity), new PropertyMetadata(Guid.Empty));
        internal static readonly DependencyProperty CompletedExecutionContextsProperty = DependencyProperty.RegisterAttached("CompletedExecutionContexts", typeof(IList), typeof(Activity));
        internal static readonly DependencyProperty ActiveExecutionContextsProperty = DependencyProperty.RegisterAttached("ActiveExecutionContexts", typeof(IList), typeof(Activity));
        internal static readonly DependencyProperty CompletedOrderIdProperty = DependencyProperty.Register("CompletedOrderId", typeof(int), typeof(Activity), new PropertyMetadata(new Int32()));
        private static readonly DependencyProperty SerializedStreamLengthProperty = DependencyProperty.RegisterAttached("SerializedStreamLength", typeof(long), typeof(Activity), new PropertyMetadata(DependencyPropertyOptions.NonSerialized));

        // activity runtime state
        internal static readonly DependencyProperty ExecutionStatusProperty = DependencyProperty.RegisterAttached("ExecutionStatus", typeof(ActivityExecutionStatus), typeof(Activity), new PropertyMetadata(ActivityExecutionStatus.Initialized, new Attribute[] { new BrowsableAttribute(false), new DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden) }));
        internal static readonly DependencyProperty ExecutionResultProperty = DependencyProperty.RegisterAttached("ExecutionResult", typeof(ActivityExecutionResult), typeof(Activity), new PropertyMetadata(ActivityExecutionResult.None, new Attribute[] { new BrowsableAttribute(false), new DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden) }));
        internal static readonly DependencyProperty WasExecutingProperty = DependencyProperty.RegisterAttached("WasExecuting", typeof(bool), typeof(Activity), new PropertyMetadata(false, new Attribute[] { new BrowsableAttribute(false), new DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden) }));

        // lock count on status change property
        private static readonly DependencyProperty LockCountOnStatusChangeProperty = DependencyProperty.RegisterAttached("LockCountOnStatusChange", typeof(int), typeof(Activity), new PropertyMetadata(new Int32()));
        internal static readonly DependencyProperty HasPrimaryClosedProperty = DependencyProperty.RegisterAttached("HasPrimaryClosed", typeof(bool), typeof(Activity), new PropertyMetadata(false));

        // nested activity collection used at serialization time
        private static readonly DependencyProperty NestedActivitiesProperty = DependencyProperty.RegisterAttached("NestedActivities", typeof(IList<Activity>), typeof(Activity));

        // Workflow Definition property, should only be visible to runtime
        internal static readonly DependencyProperty WorkflowDefinitionProperty = DependencyProperty.RegisterAttached("WorkflowDefinition", typeof(Activity), typeof(Activity), new PropertyMetadata(DependencyPropertyOptions.NonSerialized));

        // Workflow Runtime property, should only be visible to runtime
        internal static readonly DependencyProperty WorkflowRuntimeProperty = DependencyProperty.RegisterAttached("WorkflowRuntime", typeof(IServiceProvider), typeof(Activity), new PropertyMetadata(DependencyPropertyOptions.NonSerialized));

        [ThreadStatic]
        internal static Hashtable ContextIdToActivityMap = null;
        [ThreadStatic]
        internal static Activity DefinitionActivity = null;
        [ThreadStatic]
        internal static ArrayList ActivityRoots = null;

        private static readonly BinaryFormatter binaryFormatter = null;
        private static ActivityResolveEventHandler activityDefinitionResolve = null;
        private static WorkflowChangeActionsResolveEventHandler workflowChangeActionsResolve = null;

        [NonSerialized]
        private string cachedDottedPath = null;

        [NonSerialized]
        private IWorkflowCoreRuntime workflowCoreRuntime = null;

        [NonSerialized]
        internal CompositeActivity parent = null;

        private static object staticSyncRoot = new object();

        internal static readonly DependencyProperty CustomActivityProperty = DependencyProperty.Register("CustomActivity", typeof(bool), typeof(Activity), new PropertyMetadata(DependencyPropertyOptions.Metadata));
        internal static Type ActivityType = null;

        static Activity()
        {
            binaryFormatter = new BinaryFormatter();
            binaryFormatter.SurrogateSelector = ActivitySurrogateSelector.Default;

            // register known properties
            DependencyProperty.RegisterAsKnown(ActivityExecutionContextInfoProperty, (byte)1, DependencyProperty.PropertyValidity.Reexecute);
            DependencyProperty.RegisterAsKnown(CompletedExecutionContextsProperty, (byte)2, DependencyProperty.PropertyValidity.Reexecute);
            DependencyProperty.RegisterAsKnown(ActiveExecutionContextsProperty, (byte)3, DependencyProperty.PropertyValidity.Uninitialize);
            DependencyProperty.RegisterAsKnown(CompletedOrderIdProperty, (byte)4, DependencyProperty.PropertyValidity.Uninitialize);
            DependencyProperty.RegisterAsKnown(ExecutionStatusProperty, (byte)5, DependencyProperty.PropertyValidity.Reexecute);
            DependencyProperty.RegisterAsKnown(ExecutionResultProperty, (byte)6, DependencyProperty.PropertyValidity.Reexecute);
            DependencyProperty.RegisterAsKnown(WasExecutingProperty, (byte)7, DependencyProperty.PropertyValidity.Uninitialize);
            DependencyProperty.RegisterAsKnown(LockCountOnStatusChangeProperty, (byte)8, DependencyProperty.PropertyValidity.Uninitialize);
            DependencyProperty.RegisterAsKnown(HasPrimaryClosedProperty, (byte)9, DependencyProperty.PropertyValidity.Uninitialize);
            DependencyProperty.RegisterAsKnown(NestedActivitiesProperty, (byte)10, DependencyProperty.PropertyValidity.Uninitialize);
            DependencyProperty.RegisterAsKnown(ActivityContextGuidProperty, (byte)11, DependencyProperty.PropertyValidity.Reexecute);
            DependencyProperty.RegisterAsKnown(WorkflowXamlMarkupProperty, (byte)12, DependencyProperty.PropertyValidity.Uninitialize);
            DependencyProperty.RegisterAsKnown(WorkflowRulesMarkupProperty, (byte)13, DependencyProperty.PropertyValidity.Uninitialize);

            // other classes
            DependencyProperty.RegisterAsKnown(ActivityExecutionContext.CurrentExceptionProperty, (byte)23, DependencyProperty.PropertyValidity.Reexecute);
            DependencyProperty.RegisterAsKnown(ActivityExecutionContext.GrantedLocksProperty, (byte)24, DependencyProperty.PropertyValidity.Uninitialize);
            DependencyProperty.RegisterAsKnown(ActivityExecutionContext.LockAcquiredCallbackProperty, (byte)25, DependencyProperty.PropertyValidity.Uninitialize);


            // events
            DependencyProperty.RegisterAsKnown(ExecutingEvent, (byte)31, DependencyProperty.PropertyValidity.Uninitialize);
            DependencyProperty.RegisterAsKnown(CancelingEvent, (byte)32, DependencyProperty.PropertyValidity.Uninitialize);
            DependencyProperty.RegisterAsKnown(ClosedEvent, (byte)33, DependencyProperty.PropertyValidity.Uninitialize);
            DependencyProperty.RegisterAsKnown(CompensatingEvent, (byte)34, DependencyProperty.PropertyValidity.Uninitialize);
            DependencyProperty.RegisterAsKnown(StatusChangedEvent, (byte)35, DependencyProperty.PropertyValidity.Uninitialize);
            DependencyProperty.RegisterAsKnown(StatusChangedLockedEvent, (byte)36, DependencyProperty.PropertyValidity.Uninitialize);
            DependencyProperty.RegisterAsKnown(LockCountOnStatusChangeChangedEvent, (byte)37, DependencyProperty.PropertyValidity.Uninitialize);
            DependencyProperty.RegisterAsKnown(FaultingEvent, (byte)38, DependencyProperty.PropertyValidity.Uninitialize);

            // misc. others
            DependencyProperty.RegisterAsKnown(FaultAndCancellationHandlingFilter.FaultProcessedProperty, (byte)41, DependencyProperty.PropertyValidity.Uninitialize);
            DependencyProperty.RegisterAsKnown(CompensationHandlingFilter.CompensateProcessedProperty, (byte)43, DependencyProperty.PropertyValidity.Uninitialize);
            DependencyProperty.RegisterAsKnown(CompensationHandlingFilter.LastCompensatedOrderIdProperty, (byte)44, DependencyProperty.PropertyValidity.Uninitialize);
        }

        public Activity()
        {
            SetValue(CustomActivityProperty, false);
            SetValue(NameProperty, GetType().Name);
        }

        public Activity(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            SetValue(CustomActivityProperty, false);
            SetValue(NameProperty, name);
        }

        #region Execution Signals

        protected internal virtual void Initialize(IServiceProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException("provider");

        }
        protected internal virtual ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {
            if (executionContext == null)
                throw new ArgumentNullException("executionContext");

            return ActivityExecutionStatus.Closed;
        }
        protected internal virtual ActivityExecutionStatus Cancel(ActivityExecutionContext executionContext)
        {
            if (executionContext == null)
                throw new ArgumentNullException("executionContext");

            return ActivityExecutionStatus.Closed;
        }
        protected internal virtual ActivityExecutionStatus HandleFault(ActivityExecutionContext executionContext, Exception exception)
        {
            if (executionContext == null)
                throw new ArgumentNullException("executionContext");

            return ActivityExecutionStatus.Closed;
        }

        /// <summary>
        /// Derived implementation may do necessary cleanup here such as removing serializable instance 
        /// dependency properties before the activity status is set to closed.
        /// </summary>
        protected virtual void OnClosed(IServiceProvider provider)
        {
        }

        /// <summary>
        /// Called Immediatly once activity is done with it life time
        /// i.e Simple Activity when they transition to close.
        ///     CompensatableActivity & any activity which is in compensation chain
        ///     when root activity close.
        /// </summary>
        /// <param name="serviceProvider"></param>
        protected internal virtual void Uninitialize(IServiceProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException("provider");

            ResetKnownDependencyProperties(false);
        }

        protected internal virtual void OnActivityExecutionContextLoad(IServiceProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException("provider");
        }

        protected internal virtual void OnActivityExecutionContextUnload(IServiceProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException("provider");
        }
        #endregion

        #region Activity Execution Helper Methods

        protected internal void RaiseGenericEvent<T>(DependencyProperty dependencyEvent, object sender, T e) where T : EventArgs
        {
            if (dependencyEvent == null)
                throw new ArgumentNullException("dependencyEvent");

            if (e == null)
                throw new ArgumentNullException("e");

            if (this.WorkflowCoreRuntime == null)
                throw new InvalidOperationException(SR.GetString(SR.Error_NoRuntimeAvailable));

            EventHandler<T>[] eventHandlers = ((IDependencyObjectAccessor)this).GetInvocationList<EventHandler<T>>(dependencyEvent);
            if (eventHandlers != null)
            {
                foreach (EventHandler<T> eventHandler in eventHandlers)
                {
                    this.WorkflowCoreRuntime.RaiseHandlerInvoking(eventHandler);
                    try
                    {
                        eventHandler(sender, e);
                    }
                    finally
                    {
                        this.WorkflowCoreRuntime.RaiseHandlerInvoked();
                    }
                }
            }
        }

        protected internal void RaiseEvent(DependencyProperty dependencyEvent, object sender, EventArgs e)
        {
            if (sender == null)
                throw new ArgumentNullException("sender");

            if (dependencyEvent == null)
                throw new ArgumentNullException("dependencyEvent");

            if (e == null)
                throw new ArgumentNullException("e");

            if (this.WorkflowCoreRuntime == null)
                throw new InvalidOperationException(SR.GetString(SR.Error_NoRuntimeAvailable));

            EventHandler[] eventHandlers = ((IDependencyObjectAccessor)this).GetInvocationList<EventHandler>(dependencyEvent);
            if (eventHandlers != null)
            {
                foreach (EventHandler eventHandler in eventHandlers)
                {
                    this.WorkflowCoreRuntime.RaiseHandlerInvoking(eventHandler);
                    try
                    {
                        eventHandler(sender, e);
                    }
                    finally
                    {
                        this.WorkflowCoreRuntime.RaiseHandlerInvoked();
                    }
                }
            }
        }

        protected void TrackData(object userData)
        {
            if (userData == null)
                throw new ArgumentNullException("userData");
            if (this.WorkflowCoreRuntime == null)
                throw new InvalidOperationException(SR.GetString(SR.Error_NoRuntimeAvailable));

            this.WorkflowCoreRuntime.Track(null, userData);
        }
        protected void TrackData(string userDataKey, object userData)
        {
            if (userData == null)
                throw new ArgumentNullException("userData");
            if (this.WorkflowCoreRuntime == null)
                throw new InvalidOperationException(SR.GetString(SR.Error_NoRuntimeAvailable));

            this.WorkflowCoreRuntime.Track(userDataKey, userData);
        }
        protected Guid WorkflowInstanceId
        {
            get
            {
                if (this.WorkflowCoreRuntime == null)
#pragma warning suppress 56503
                    throw new InvalidOperationException(SR.GetString(SR.Error_NoRuntimeAvailable));
                return this.WorkflowCoreRuntime.InstanceID;
            }
        }
        protected internal void Invoke<T>(EventHandler<T> handler, T e) where T : EventArgs
        {

            if (handler == null)
                throw new ArgumentNullException("handler");

            if (e == null)
                throw new ArgumentNullException("e");

            if (this.WorkflowCoreRuntime == null)
                throw new InvalidOperationException(SR.GetString(System.Globalization.CultureInfo.CurrentCulture, SR.Error_NoRuntimeAvailable));

            if (this.ExecutionStatus == ActivityExecutionStatus.Initialized || this.ExecutionStatus == ActivityExecutionStatus.Closed)
                throw new InvalidOperationException(SR.GetString(System.Globalization.CultureInfo.CurrentCulture, SR.Error_InvalidInvokingState));

            // create subscriber
            ActivityExecutorDelegateInfo<T> activityExecutorDelegate = null;
            using (this.WorkflowCoreRuntime.SetCurrentActivity(this))
                activityExecutorDelegate = new ActivityExecutorDelegateInfo<T>(handler, this.ContextActivity);

            activityExecutorDelegate.InvokeDelegate(this.WorkflowCoreRuntime.CurrentActivity.ContextActivity, e, false);
        }

        protected internal void Invoke<T>(IActivityEventListener<T> eventListener, T e) where T : EventArgs
        {
            if (eventListener == null)
                throw new ArgumentNullException("eventListener");

            if (e == null)
                throw new ArgumentNullException("e");

            if (this.WorkflowCoreRuntime == null)
                throw new InvalidOperationException(SR.GetString(System.Globalization.CultureInfo.CurrentCulture, SR.Error_NoRuntimeAvailable));

            if (this.ExecutionStatus == ActivityExecutionStatus.Initialized || this.ExecutionStatus == ActivityExecutionStatus.Closed)
                throw new InvalidOperationException(SR.GetString(System.Globalization.CultureInfo.CurrentCulture, SR.Error_InvalidInvokingState));

            // create subscriber
            ActivityExecutorDelegateInfo<T> activityExecutorDelegate = null;
            using (this.WorkflowCoreRuntime.SetCurrentActivity(this))
                activityExecutorDelegate = new ActivityExecutorDelegateInfo<T>(eventListener, this.ContextActivity);

            activityExecutorDelegate.InvokeDelegate(this.WorkflowCoreRuntime.CurrentActivity.ContextActivity, e, false);
        }
        #endregion

        #region Activity Meta Properties

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public CompositeActivity Parent
        {
            get
            {
                return this.parent;
            }
        }

        internal void SetParent(CompositeActivity compositeActivity)
        {
            this.parent = compositeActivity;
        }

        [Browsable(true)]
        [SRCategory(SR.Activity)]
        [ParenthesizePropertyName(true)]
        [SRDescription(SR.NameDescr)]
        [MergableProperty(false)]
        [DefaultValue("")]
        public string Name
        {
            get
            {
                return (string)GetValue(NameProperty);
            }
            set
            {
                SetValue(NameProperty, value);
            }
        }

        [Browsable(true)]
        [SRCategory(SR.Activity)]
        [SRDescription(SR.EnabledDescr)]
        [DefaultValue(true)]
        public bool Enabled
        {
            get
            {
                return (bool)GetValue(EnabledProperty);
            }
            set
            {
                SetValue(EnabledProperty, value);
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string QualifiedName
        {
            get
            {
                if (!this.DesignMode && !this.DynamicUpdateMode)
                {
                    string cachedQualifiedName = (string)GetValue(QualifiedNameProperty);
                    if (cachedQualifiedName != null)
                        return cachedQualifiedName;
                }

                string sbQId = null;
                if (Helpers.IsActivityLocked(this))
                    sbQId = InternalHelpers.GenerateQualifiedNameForLockedActivity(this, null);
                else
                    sbQId = (string)GetValue(NameProperty);
                return sbQId;
            }
        }

        [Browsable(true)]
        [SRCategory(SR.Activity)]
        [SRDescription(SR.DescriptionDescr)]
        [Editor(typeof(MultilineStringEditor), typeof(UITypeEditor))]
        [DefaultValue("")]
        public string Description
        {
            get
            {
                return (string)GetValue(DescriptionProperty);
            }
            set
            {
                SetValue(DescriptionProperty, value);
            }
        }
        #endregion

        #region Activity Instance Properties

        public static readonly DependencyProperty StatusChangedEvent = DependencyProperty.Register("StatusChanged", typeof(EventHandler<ActivityExecutionStatusChangedEventArgs>), typeof(Activity));
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public event EventHandler<ActivityExecutionStatusChangedEventArgs> StatusChanged
        {
            add
            {
                this.AddStatusChangeHandler(StatusChangedEvent, value);
            }
            remove
            {
                this.RemoveStatusChangeHandler(StatusChangedEvent, value);
            }
        }

        internal static readonly DependencyProperty LockCountOnStatusChangeChangedEvent = DependencyProperty.Register("LockCountOnStatusChangeChanged", typeof(EventHandler<ActivityExecutionStatusChangedEventArgs>), typeof(Activity));
        internal static readonly DependencyProperty StatusChangedLockedEvent = DependencyProperty.Register("StatusChangedLocked", typeof(EventHandler<ActivityExecutionStatusChangedEventArgs>), typeof(Activity));
        internal void HoldLockOnStatusChange(IActivityEventListener<ActivityExecutionStatusChangedEventArgs> eventListener)
        {
            this.RegisterForStatusChange(StatusChangedLockedEvent, eventListener);

            // increment count
            this.SetValue(LockCountOnStatusChangeProperty, this.LockCountOnStatusChange + 1);
        }
        internal void ReleaseLockOnStatusChange(IActivityEventListener<ActivityExecutionStatusChangedEventArgs> eventListener)
        {
            // remove it 
            this.UnregisterForStatusChange(StatusChangedLockedEvent, eventListener);

            // decrement count and fire event
            int lockCountOnStatusChange = this.LockCountOnStatusChange;
            Debug.Assert(lockCountOnStatusChange > 0, "lock count on status change should be > 0");
            this.SetValue(LockCountOnStatusChangeProperty, --lockCountOnStatusChange);
            if (lockCountOnStatusChange == 0)
            {
                // Work around: if primary activity was never closed, then we set it to Canceled outcome
                if (!this.HasPrimaryClosed)
                    this.SetValue(ExecutionResultProperty, ActivityExecutionResult.Canceled);

                try
                {
                    this.MarkClosed();
                }
                catch
                {
                    // roll back the lock count changes which we did
                    this.SetValue(LockCountOnStatusChangeProperty, ++lockCountOnStatusChange);
                    this.RegisterForStatusChange(StatusChangedLockedEvent, eventListener);
                    throw;
                }
            }
            else
            {
                FireStatusChangedEvents(Activity.LockCountOnStatusChangeChangedEvent, false);
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        internal int LockCountOnStatusChange
        {
            get
            {
                return (int)this.GetValue(LockCountOnStatusChangeProperty);
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        internal bool HasPrimaryClosed
        {
            get
            {
                return (bool)this.GetValue(HasPrimaryClosedProperty);
            }
        }

        public static readonly DependencyProperty CancelingEvent = DependencyProperty.Register("Canceling", typeof(EventHandler<ActivityExecutionStatusChangedEventArgs>), typeof(Activity));

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public event EventHandler<ActivityExecutionStatusChangedEventArgs> Canceling
        {
            add
            {
                this.AddStatusChangeHandler(CancelingEvent, value);
            }
            remove
            {
                this.RemoveStatusChangeHandler(CancelingEvent, value);
            }
        }

        public static readonly DependencyProperty FaultingEvent = DependencyProperty.Register("Faulting", typeof(EventHandler<ActivityExecutionStatusChangedEventArgs>), typeof(Activity));

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public event EventHandler<ActivityExecutionStatusChangedEventArgs> Faulting
        {
            add
            {
                this.AddStatusChangeHandler(FaultingEvent, value);
            }
            remove
            {
                this.RemoveStatusChangeHandler(FaultingEvent, value);
            }
        }


        public static readonly DependencyProperty ClosedEvent = DependencyProperty.Register("Closed", typeof(EventHandler<ActivityExecutionStatusChangedEventArgs>), typeof(Activity));

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public event EventHandler<ActivityExecutionStatusChangedEventArgs> Closed
        {
            add
            {
                this.AddStatusChangeHandler(ClosedEvent, value);
            }
            remove
            {
                this.RemoveStatusChangeHandler(ClosedEvent, value);
            }
        }

        public static readonly DependencyProperty ExecutingEvent = DependencyProperty.Register("Executing", typeof(EventHandler<ActivityExecutionStatusChangedEventArgs>), typeof(Activity));

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public event EventHandler<ActivityExecutionStatusChangedEventArgs> Executing
        {
            add
            {
                this.AddStatusChangeHandler(ExecutingEvent, value);
            }
            remove
            {
                this.RemoveStatusChangeHandler(ExecutingEvent, value);
            }
        }

        public static readonly DependencyProperty CompensatingEvent = DependencyProperty.Register("Compensating", typeof(EventHandler<ActivityExecutionStatusChangedEventArgs>), typeof(Activity));

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public event EventHandler<ActivityExecutionStatusChangedEventArgs> Compensating
        {
            add
            {
                this.AddStatusChangeHandler(CompensatingEvent, value);
            }
            remove
            {
                this.RemoveStatusChangeHandler(CompensatingEvent, value);
            }
        }
        private void AddStatusChangeHandler(DependencyProperty dependencyProp, EventHandler<ActivityExecutionStatusChangedEventArgs> delegateValue)
        {
            IList handlers = null;
            if (this.DependencyPropertyValues.ContainsKey(dependencyProp))
            {
                handlers = this.DependencyPropertyValues[dependencyProp] as IList;
            }
            else
            {
                handlers = new ArrayList();
                this.DependencyPropertyValues[dependencyProp] = handlers;
            }
            handlers.Add(new ActivityExecutorDelegateInfo<ActivityExecutionStatusChangedEventArgs>(true, delegateValue, this.ContextActivity ?? this.RootActivity));
        }
        private void RemoveStatusChangeHandler(DependencyProperty dependencyProp, EventHandler<ActivityExecutionStatusChangedEventArgs> delegateValue)
        {
            if (this.DependencyPropertyValues.ContainsKey(dependencyProp))
            {
                IList handlers = this.DependencyPropertyValues[dependencyProp] as IList;
                if (handlers != null)
                {
                    handlers.Remove(new ActivityExecutorDelegateInfo<ActivityExecutionStatusChangedEventArgs>(true, delegateValue, this.ContextActivity));
                    if (handlers.Count == 0)
                        this.DependencyPropertyValues.Remove(dependencyProp);
                }
            }
        }
        private IList GetStatusChangeHandlers(DependencyProperty dependencyProp)
        {
            IList handlers = null;
            if (this.DependencyPropertyValues.ContainsKey(dependencyProp))
                handlers = this.DependencyPropertyValues[dependencyProp] as IList;
            return handlers;
        }

        public void RegisterForStatusChange(DependencyProperty dependencyProp, IActivityEventListener<ActivityExecutionStatusChangedEventArgs> activityStatusChangeListener)
        {
            if (dependencyProp == null)
                throw new ArgumentNullException("dependencyProp");
            if (activityStatusChangeListener == null)
                throw new ArgumentNullException("activityStatusChangeListener");

            if (dependencyProp != Activity.ExecutingEvent &&
                dependencyProp != Activity.CancelingEvent &&
                dependencyProp != Activity.ClosedEvent &&
                dependencyProp != Activity.CompensatingEvent &&
                dependencyProp != Activity.FaultingEvent &&
                dependencyProp != Activity.StatusChangedEvent &&
                dependencyProp != Activity.StatusChangedLockedEvent &&
                dependencyProp != Activity.LockCountOnStatusChangeChangedEvent)
                throw new ArgumentException();

            IList handlers = null;
            if (this.DependencyPropertyValues.ContainsKey(dependencyProp))
            {
                handlers = this.DependencyPropertyValues[dependencyProp] as IList;
            }
            else
            {
                handlers = new ArrayList();
                this.DependencyPropertyValues[dependencyProp] = handlers;
            }
            handlers.Add(new ActivityExecutorDelegateInfo<ActivityExecutionStatusChangedEventArgs>(true, activityStatusChangeListener, this.ContextActivity));
        }
        public void UnregisterForStatusChange(DependencyProperty dependencyProp, IActivityEventListener<ActivityExecutionStatusChangedEventArgs> activityStatusChangeListener)
        {
            if (dependencyProp == null)
                throw new ArgumentNullException("dependencyProp");
            if (activityStatusChangeListener == null)
                throw new ArgumentNullException("activityStatusChangeListener");

            if (dependencyProp != Activity.ExecutingEvent &&
                dependencyProp != Activity.CancelingEvent &&
                dependencyProp != Activity.ClosedEvent &&
                dependencyProp != Activity.CompensatingEvent &&
                dependencyProp != Activity.FaultingEvent &&
                dependencyProp != Activity.StatusChangedEvent &&
                dependencyProp != Activity.StatusChangedLockedEvent &&
                dependencyProp != Activity.LockCountOnStatusChangeChangedEvent)
                throw new ArgumentException();

            if (this.DependencyPropertyValues.ContainsKey(dependencyProp))
            {
                IList handlers = this.DependencyPropertyValues[dependencyProp] as IList;
                if (handlers != null)
                {
                    handlers.Remove(new ActivityExecutorDelegateInfo<ActivityExecutionStatusChangedEventArgs>(true, activityStatusChangeListener, this.ContextActivity));
                    if (handlers.Count == 0)
                        this.DependencyPropertyValues.Remove(dependencyProp);
                }
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ActivityExecutionStatus ExecutionStatus
        {
            get
            {
                return (ActivityExecutionStatus)this.GetValue(ExecutionStatusProperty);
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ActivityExecutionResult ExecutionResult
        {
            get
            {
                return (ActivityExecutionResult)this.GetValue(ExecutionResultProperty);
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsDynamicActivity
        {
            get
            {
                if (this.DesignMode)
                    return false;
                else
                    return (this.ContextActivity != this.RootActivity);
            }
        }


        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        internal bool WasExecuting
        {
            get
            {
                return (bool)this.GetValue(WasExecutingProperty);
            }
        }
        public Activity GetActivityByName(string activityQualifiedName)
        {
            return GetActivityByName(activityQualifiedName, false);
        }
        public Activity GetActivityByName(string activityQualifiedName, bool withinThisActivityOnly)
        {
            if (activityQualifiedName == null)
                throw new ArgumentNullException("activityQualifiedName");

            if (this.QualifiedName == activityQualifiedName)
                return this;

            Activity resolvedActivity = null;

            // try with just the passed qualified id
            resolvedActivity = ResolveActivityByName(activityQualifiedName, withinThisActivityOnly);

            if (resolvedActivity == null)
            {
                // if custom then append its qualified id and then try it
                if (this is CompositeActivity && Helpers.IsCustomActivity(this as CompositeActivity))
                    resolvedActivity = ResolveActivityByName(this.QualifiedName + "." + activityQualifiedName, withinThisActivityOnly);
            }

            return resolvedActivity;
        }

        private Activity ResolveActivityByName(string activityQualifiedName, bool withinThisActivityOnly)
        {
            Activity resolvedActivity = null;
            if (!this.DesignMode && !this.DynamicUpdateMode)
            {
                Activity rootActivity = this.RootActivity;

                Hashtable lookupPaths = (Hashtable)rootActivity.UserData[UserDataKeys.LookupPaths];
                if (lookupPaths != null)
                {
                    string path = (string)lookupPaths[activityQualifiedName];
                    if (path != null)
                    {
                        if (path.Length != 0)
                        {
                            string thisPath = (string)lookupPaths[this.QualifiedName];
                            if (path.StartsWith(thisPath, StringComparison.Ordinal))
                            {
                                if (path.Length == thisPath.Length)
                                    resolvedActivity = this;
                                else if (thisPath.Length == 0 || path[thisPath.Length] == '.')
                                    resolvedActivity = this.TraverseDottedPath(path.Substring(thisPath.Length > 0 ? thisPath.Length + 1 : 0));
                            }
                            else if (!withinThisActivityOnly)
                            {
                                resolvedActivity = rootActivity.TraverseDottedPath(path);
                            }
                        }
                        else if (!withinThisActivityOnly)
                        {
                            resolvedActivity = rootActivity;
                        }
                    }
                }
            }
            else if (!this.DesignMode)
            {
                // WinOE Bug 20584: Fix this for dynamic updates only.  See bug description for details.
                CompositeActivity parent = (withinThisActivityOnly ? this : this.RootActivity) as CompositeActivity;
                if (parent != null)
                {
                    foreach (Activity childActivity in Helpers.GetNestedActivities(parent))
                    {
                        if (childActivity.QualifiedName == activityQualifiedName)
                        {
                            resolvedActivity = childActivity;
                            break;
                        }
                    }
                }
            }
            else
            {
                // 
                resolvedActivity = Helpers.ParseActivity(this, activityQualifiedName);

                if (resolvedActivity == null && !withinThisActivityOnly)
                    resolvedActivity = Helpers.ParseActivity(this.RootActivity, activityQualifiedName);
            }

            return resolvedActivity;
        }

        internal void ResetAllKnownDependencyProperties()
        {
            ResetKnownDependencyProperties(true);
        }

        private void ResetKnownDependencyProperties(bool forReexecute)
        {
            DependencyProperty[] propertyClone = new DependencyProperty[this.DependencyPropertyValues.Keys.Count];
            this.DependencyPropertyValues.Keys.CopyTo(propertyClone, 0);

            foreach (DependencyProperty property in propertyClone)
            {
                if (property.IsKnown && (property.Validity == DependencyProperty.PropertyValidity.Uninitialize || (forReexecute && property.Validity == DependencyProperty.PropertyValidity.Reexecute)))
                    this.RemoveProperty(property);
            }
        }

        internal virtual Activity TraverseDottedPath(string dottedPath)
        {
            return null;
        }
        internal Activity TraverseDottedPathFromRoot(string dottedPathFromRoot)
        {
            string thisActivityDottedPath = this.DottedPath;
            if (dottedPathFromRoot == thisActivityDottedPath)
                return this;

            // if it start with the smae dotted path then it's ok, otherwise return
            if (!dottedPathFromRoot.StartsWith(thisActivityDottedPath, StringComparison.Ordinal))
                return null;

            // calculate relative path
            string relativeDottedPath = dottedPathFromRoot;
            if (thisActivityDottedPath.Length > 0)
                relativeDottedPath = dottedPathFromRoot.Substring(thisActivityDottedPath.Length + 1);

            return this.TraverseDottedPath(relativeDottedPath);
        }
        internal string DottedPath
        {
            get
            {
                if (!this.DesignMode && !this.DynamicUpdateMode)
                {
                    string cachedDottedPath = (string)GetValue(DottedPathProperty);
                    if (cachedDottedPath != null)
                        return cachedDottedPath;
                }

                StringBuilder dottedPathBuilder = new StringBuilder();
                Activity thisActivity = this;
                while (thisActivity.parent != null)
                {
                    int thisActivityIndex = thisActivity.parent.Activities.IndexOf(thisActivity);
                    dottedPathBuilder.Insert(0, thisActivityIndex.ToString(CultureInfo.InvariantCulture)); //15
                    dottedPathBuilder.Insert(0, '.'); //.15
                    thisActivity = thisActivity.parent;
                }
                if (dottedPathBuilder.Length > 0)
                    dottedPathBuilder.Remove(0, 1); // remove the first dot
                return dottedPathBuilder.ToString();
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        internal IWorkflowCoreRuntime WorkflowCoreRuntime
        {
            get
            {
                return this.workflowCoreRuntime;
            }
        }
        internal bool DynamicUpdateMode
        {
            get
            {
                return (this.cachedDottedPath != null);
            }
            set
            {
                if (value)
                    this.cachedDottedPath = this.DottedPath;
                else
                    this.cachedDottedPath = null;
            }
        }
        internal string CachedDottedPath
        {
            get
            {
                return this.cachedDottedPath;
            }
        }

        #endregion

        #region Load/Save Static Methods

        public Activity Clone()
        {
            if (this.DesignMode)
                throw new InvalidOperationException(SR.GetString(SR.Error_NoRuntimeAvailable));

            long max = (long)this.GetValue(SerializedStreamLengthProperty);
            if (max == 0)
                max = 10240;
            MemoryStream memoryStream = new MemoryStream((int)max);
            Save(memoryStream);
            memoryStream.Position = 0;
            this.SetValue(SerializedStreamLengthProperty, memoryStream.Length > max ? memoryStream.Length : max);
            return Activity.Load(memoryStream, this);
        }

        public void Save(Stream stream)
        {
            this.Save(stream, binaryFormatter);
        }
        public void Save(Stream stream, IFormatter formatter)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");
            if (formatter == null)
                throw new ArgumentNullException("formatter");

            if (this.DesignMode)
                throw new InvalidOperationException(SR.GetString(SR.Error_NoRuntimeAvailable));

            // cache the old values so that this fucntion can be re-entrant
            Hashtable oldContextIdToActivityMap = ContextIdToActivityMap;

            ContextIdToActivityMap = new Hashtable();
            try
            {
                // fill context id to Activity map
                FillContextIdToActivityMap(this);

                // walk through all nested activity roots and set nested activities
                foreach (Activity activityRoot in ContextIdToActivityMap.Values)
                {
                    IList<Activity> nestedActivities = activityRoot.CollectNestedActivities();
                    if (nestedActivities != null && nestedActivities.Count > 0)
                        activityRoot.SetValue(Activity.NestedActivitiesProperty, nestedActivities);
                }

                // serialize the graph
                formatter.Serialize(stream, this);
            }
            finally
            {
                foreach (Activity activityRoot in ContextIdToActivityMap.Values)
                    activityRoot.RemoveProperty(Activity.NestedActivitiesProperty);
                ContextIdToActivityMap = oldContextIdToActivityMap;
                ActivityRoots = null;
            }
        }
        public static Activity Load(Stream stream, Activity outerActivity)
        {
            return Load(stream, outerActivity, binaryFormatter);
        }
        public static Activity Load(Stream stream, Activity outerActivity, IFormatter formatter)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");
            if (formatter == null)
                throw new ArgumentNullException("formatter");

            if (outerActivity != null && outerActivity.DesignMode)
                throw new InvalidOperationException(SR.GetString(SR.Error_NoRuntimeAvailable));

            Activity returnActivity = null;

            // cache the old values, so that this fucntion can be re-entrant
            Hashtable oldContextIdToActivityMap = ContextIdToActivityMap;
            Activity oldDefinitionActivity = DefinitionActivity;

            // initialize the thread static guys
            ContextIdToActivityMap = new Hashtable();
            DefinitionActivity = outerActivity;

            try
            {
                // fill in the context id to activity map for surrounding contexts
                if (outerActivity != null)
                    FillContextIdToActivityMap(outerActivity.RootActivity);

                // deserialize the stream
                returnActivity = (Activity)formatter.Deserialize(stream);

                // fix up parent child relation ships for root activity and al nested context activities
                Queue<Activity> deserializedActivityRootsQueue = new Queue<Activity>();
                deserializedActivityRootsQueue.Enqueue(returnActivity);
                while (deserializedActivityRootsQueue.Count > 0)
                {
                    Activity deserializedActivityRoot = deserializedActivityRootsQueue.Dequeue();

                    // determine the parent activity and definition activity
                    Activity definitionActivity = DefinitionActivity;
                    Activity parentActivity = outerActivity != null ? outerActivity.parent : null;

                    if (deserializedActivityRoot.IsContextActivity)
                    {
                        // get the corresponding definition activity
                        ActivityExecutionContextInfo contextInfo = (ActivityExecutionContextInfo)deserializedActivityRoot.GetValue(Activity.ActivityExecutionContextInfoProperty);
                        definitionActivity = definitionActivity.GetActivityByName(contextInfo.ActivityQualifiedName);

                        // get the corresponding parent activity
                        Activity parentContextActivity = (Activity)ContextIdToActivityMap[contextInfo.ParentContextId];
                        if (parentContextActivity != null)
                            parentActivity = parentContextActivity.GetActivityByName(contextInfo.ActivityQualifiedName).parent;

                        // fill up the cached context activities
                        ContextIdToActivityMap[deserializedActivityRoot.ContextId] = deserializedActivityRoot;

                        // get nested context activities and queue them for processing
                        IList<Activity> deserializedNestedActivityRoots = (IList<Activity>)deserializedActivityRoot.GetValue(Activity.ActiveExecutionContextsProperty);
                        if (deserializedNestedActivityRoots != null)
                        {
                            foreach (Activity deserializedNestedActivityRoot in deserializedNestedActivityRoots)
                                deserializedActivityRootsQueue.Enqueue(deserializedNestedActivityRoot);
                        }
                    }

                    // prepare hash of id to activity
                    Hashtable idToActivityMap = new Hashtable();
                    IList<Activity> nestedActivities = (IList<Activity>)deserializedActivityRoot.GetValue(Activity.NestedActivitiesProperty);
                    if (nestedActivities != null)
                    {
                        foreach (Activity nestedActivity in nestedActivities)
                            idToActivityMap.Add(nestedActivity.DottedPath, nestedActivity);
                    }

                    // fix up parent child relation ship for this activity
                    deserializedActivityRoot.FixUpParentChildRelationship(definitionActivity, parentActivity, idToActivityMap);
                    deserializedActivityRoot.FixUpMetaProperties(definitionActivity);
                    deserializedActivityRoot.RemoveProperty(Activity.NestedActivitiesProperty);
                }

                // set the Workflow Definition in case of root activity
                if (returnActivity.Parent == null)
                    returnActivity.SetValue(Activity.WorkflowDefinitionProperty, DefinitionActivity);
            }
            finally
            {
                ContextIdToActivityMap = oldContextIdToActivityMap;
                DefinitionActivity = oldDefinitionActivity;
                ActivityRoots = null;
            }
            return returnActivity;
        }
        private static void FillContextIdToActivityMap(Activity seedActivity)
        {
            Queue<Activity> activityRootsQueue = new Queue<Activity>();
            activityRootsQueue.Enqueue(seedActivity);
            while (activityRootsQueue.Count > 0)
            {
                Activity activityRoot = activityRootsQueue.Dequeue();
                if (activityRoot.IsContextActivity)
                {
                    ContextIdToActivityMap[activityRoot.ContextId] = activityRoot;
                    IList<Activity> activeActivityRoots = (IList<Activity>)activityRoot.GetValue(Activity.ActiveExecutionContextsProperty);
                    if (activeActivityRoots != null)
                    {
                        foreach (Activity activeActivityRoot in activeActivityRoots)
                            activityRootsQueue.Enqueue(activeActivityRoot);
                    }
                }
                else
                {
                    ContextIdToActivityMap[0] = activityRoot;
                }
            }
            ActivityRoots = new ArrayList(ContextIdToActivityMap.Values);
        }

        internal static event ActivityResolveEventHandler ActivityResolve
        {
            add
            {
                lock (staticSyncRoot)
                {
                    activityDefinitionResolve += value;
                }
            }
            remove
            {
                lock (staticSyncRoot)
                {
                    activityDefinitionResolve -= value;
                }
            }
        }

        internal static event WorkflowChangeActionsResolveEventHandler WorkflowChangeActionsResolve
        {
            add
            {
                lock (staticSyncRoot)
                {
                    workflowChangeActionsResolve += value;
                }
            }

            remove
            {
                lock (staticSyncRoot)
                {
                    workflowChangeActionsResolve -= value;
                }
            }
        }

        internal static Activity OnResolveActivityDefinition(Type type, string workflowMarkup, string rulesMarkup, bool createNew, bool initForRuntime, IServiceProvider serviceProvider)
        {
            // get invocation list
            Delegate[] invocationList = null;
            lock (staticSyncRoot)
            {
                if (activityDefinitionResolve != null)
                    invocationList = activityDefinitionResolve.GetInvocationList();
            }

            // call resovlers one by one
            Activity activityDefinition = null;
            if (invocationList != null)
            {
                foreach (ActivityResolveEventHandler activityDefinitionResolver in invocationList)
                {
                    activityDefinition = activityDefinitionResolver(null, new ActivityResolveEventArgs(type, workflowMarkup, rulesMarkup, createNew, initForRuntime, serviceProvider));
                    if (activityDefinition != null)
                        return activityDefinition;
                }
            }
            return null;
        }

        internal static ArrayList OnResolveWorkflowChangeActions(string workflowChangesMarkup, Activity root)
        {
            // get invocation list
            Delegate[] invocationList = null;
            lock (staticSyncRoot)
            {
                if (workflowChangeActionsResolve != null)
                    invocationList = workflowChangeActionsResolve.GetInvocationList();
            }

            // call resovlers one by one
            ArrayList changeActions = null;
            if (invocationList != null)
            {
                foreach (WorkflowChangeActionsResolveEventHandler workflowChangeActionsResolver in invocationList)
                {
                    changeActions = workflowChangeActionsResolver(root, new WorkflowChangeActionsResolveEventArgs(workflowChangesMarkup));
                    if (changeActions != null)
                        return changeActions;
                }
            }
            return null;
        }

        #endregion

        #region Context Activity properties

        internal bool IsContextActivity
        {
            get
            {
                return this.GetValue(Activity.ActivityExecutionContextInfoProperty) != null;
            }
        }
        internal int ContextId
        {
            get
            {
                return ((ActivityExecutionContextInfo)this.ContextActivity.GetValue(Activity.ActivityExecutionContextInfoProperty)).ContextId;
            }
        }
        internal Guid ContextGuid
        {
            get
            {
                return ((ActivityExecutionContextInfo)this.ContextActivity.GetValue(Activity.ActivityExecutionContextInfoProperty)).ContextGuid;
            }
        }
        internal Activity ContextActivity
        {
            get
            {
                Activity contextActivity = this;
                while (contextActivity != null && contextActivity.GetValue(Activity.ActivityExecutionContextInfoProperty) == null)
                    contextActivity = contextActivity.parent;
                return contextActivity;
            }
        }
        internal Activity ParentContextActivity
        {
            get
            {
                Activity contextActivity = this.ContextActivity;
                ActivityExecutionContextInfo executionContextInfo = (ActivityExecutionContextInfo)contextActivity.GetValue(Activity.ActivityExecutionContextInfoProperty);
                if (executionContextInfo.ParentContextId == -1)
                    return null;
                return this.WorkflowCoreRuntime.GetContextActivityForId(executionContextInfo.ParentContextId);
            }
        }
        internal Activity RootContextActivity
        {
            get
            {
                return this.WorkflowCoreRuntime.RootActivity;
            }
        }
        internal Activity RootActivity
        {
            get
            {
                Activity parent = this;
                while (parent.parent != null)
                    parent = parent.parent;
                return parent;
            }
        }

        #endregion

        #region Runtime Initialization

        internal override void OnInitializeDefinitionForRuntime()
        {
            // only if we are in design mode, execute this code, other wise ignore this call
            if (this.DesignMode)
            {
                // call base
                base.OnInitializeDefinitionForRuntime();

                this.UserData[UserDataKeys.CustomActivity] = this.GetValue(CustomActivityProperty);

                // Work around !! Supports Synchronization and atomic transaction isolation
                ICollection<String> handles = (ICollection<String>)GetValue(SynchronizationHandlesProperty);
                if (this.SupportsTransaction)
                {
                    if (handles == null)
                        handles = new List<string>();
                    handles.Add(TransactionScopeActivity.TransactionScopeActivityIsolationHandle);
                }
                if (handles != null)
                    this.SetValue(SynchronizationHandlesProperty, new ReadOnlyCollection<string>(new List<string>(handles)));

                // lookup paths for root activity
                if (this.Parent == null)
                {
                    Hashtable lookupPaths = new Hashtable();
                    this.UserData[UserDataKeys.LookupPaths] = lookupPaths;
                    lookupPaths.Add(this.QualifiedName, string.Empty);
                }

                // Initialize the cache at runtime
                SetReadOnlyPropertyValue(QualifiedNameProperty, this.QualifiedName);
                SetReadOnlyPropertyValue(DottedPathProperty, this.DottedPath);

                // cache attributes
                this.UserData[typeof(PersistOnCloseAttribute)] = (this.GetType().GetCustomAttributes(typeof(PersistOnCloseAttribute), true).Length > 0);
            }
        }
        internal override void OnInitializeInstanceForRuntime(IWorkflowCoreRuntime workflowCoreRuntime)
        {
            base.OnInitializeInstanceForRuntime(workflowCoreRuntime);
            this.workflowCoreRuntime = workflowCoreRuntime;

        }

        internal override void OnInitializeActivatingInstanceForRuntime(IWorkflowCoreRuntime workflowCoreRuntime)
        {
            //doing this will call OnRuntimeInitialized() which is a hook for the activity writers
            //to initialize/wire up their DPs correctly for the activating instance
            base.OnInitializeActivatingInstanceForRuntime(workflowCoreRuntime);
            this.workflowCoreRuntime = workflowCoreRuntime;
        }

        internal override void FixUpMetaProperties(DependencyObject originalObject)
        {
            if (originalObject == null)
                throw new ArgumentNullException();

            // call base class
            base.FixUpMetaProperties(originalObject);
        }
        internal virtual void FixUpParentChildRelationship(Activity definitionActivity, Activity parentActivity, Hashtable deserializedActivities)
        {
            // set parent for myself, root activity will have null parent
            if (parentActivity != null)
                this.SetParent((CompositeActivity)parentActivity);
        }
        internal virtual IList<Activity> CollectNestedActivities()
        {
            return null;
        }
        #endregion

        #region Activity Status Change Signals

        internal void SetStatus(ActivityExecutionStatus newStatus, bool transacted)
        {
            System.Workflow.Runtime.WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0, "Activity Status Change - Activity: {0} Old:{1}; New:{2}", this.QualifiedName, ActivityExecutionStatusEnumToString(this.ExecutionStatus), ActivityExecutionStatusEnumToString(newStatus));

            // Set Was Executing
            if (newStatus == ActivityExecutionStatus.Faulting &&
                    this.ExecutionStatus == ActivityExecutionStatus.Executing)
            {
                this.SetValue(WasExecutingProperty, true);
            }
            this.SetValue(ExecutionStatusProperty, newStatus);

            // fire status change events
            FireStatusChangedEvents(Activity.StatusChangedEvent, transacted);
            switch (newStatus)
            {
                case ActivityExecutionStatus.Closed:
                    FireStatusChangedEvents(Activity.ClosedEvent, transacted);
                    break;
                case ActivityExecutionStatus.Executing:
                    FireStatusChangedEvents(Activity.ExecutingEvent, transacted);
                    break;
                case ActivityExecutionStatus.Canceling:
                    FireStatusChangedEvents(Activity.CancelingEvent, transacted);
                    break;
                case ActivityExecutionStatus.Faulting:
                    FireStatusChangedEvents(Activity.FaultingEvent, transacted);
                    break;
                case ActivityExecutionStatus.Compensating:
                    FireStatusChangedEvents(Activity.CompensatingEvent, transacted);
                    break;
                default:
                    return;
            }

            // inform the workflow synchronously about this
            this.WorkflowCoreRuntime.ActivityStatusChanged(this, transacted, false);
            if (newStatus == ActivityExecutionStatus.Closed)
            {
                // remove these
                this.RemoveProperty(Activity.LockCountOnStatusChangeProperty);
                this.RemoveProperty(Activity.HasPrimaryClosedProperty);
                this.RemoveProperty(Activity.WasExecutingProperty);
            }
        }
        private void FireStatusChangedEvents(DependencyProperty dependencyProperty, bool transacted)
        {
            IList eventListeners = this.GetStatusChangeHandlers(dependencyProperty);
            if (eventListeners != null)
            {
                ActivityExecutionStatusChangedEventArgs statusChangeEventArgs = new ActivityExecutionStatusChangedEventArgs(this.ExecutionStatus, this.ExecutionResult, this);
                foreach (ActivityExecutorDelegateInfo<ActivityExecutionStatusChangedEventArgs> delegateInfo in eventListeners)
                    delegateInfo.InvokeDelegate(this.ContextActivity, statusChangeEventArgs, delegateInfo.ActivityQualifiedName == null, transacted);
            }
        }
        internal void MarkCanceled()
        {
            if (this.ExecutionStatus != ActivityExecutionStatus.Closed)
            {
                if (this.ExecutionStatus != ActivityExecutionStatus.Canceling)
                    throw new InvalidOperationException(SR.GetString(SR.Error_InvalidCancelActivityState)); //DCR01
                this.SetValue(ExecutionResultProperty, ActivityExecutionResult.Canceled);
                this.MarkClosed();
            }
        }
        internal void MarkCompleted()
        {
            this.SetValue(ExecutionResultProperty, ActivityExecutionResult.Succeeded);
            this.MarkClosed();
        }
        internal void MarkCompensated()
        {
            if (this.ExecutionStatus != ActivityExecutionStatus.Compensating)
                throw new InvalidOperationException(SR.GetString(SR.Error_InvalidCompensateActivityState)); //DCR01

            this.SetValue(ExecutionResultProperty, ActivityExecutionResult.Compensated);
            this.MarkClosed();
        }
        internal void MarkFaulted()
        {
            this.SetValue(ExecutionResultProperty, ActivityExecutionResult.Faulted);
            this.MarkClosed();
        }
        private void MarkClosed()
        {
            switch (this.ExecutionStatus)
            {
                case ActivityExecutionStatus.Executing:
                case ActivityExecutionStatus.Faulting:
                case ActivityExecutionStatus.Compensating:
                case ActivityExecutionStatus.Canceling:
                    break;
                default:
                    throw new InvalidOperationException(SR.GetString(SR.Error_InvalidCloseActivityState)); //DCR01
            }

            if (this is CompositeActivity)
            {
                foreach (Activity childActivity in ((CompositeActivity)this).Activities)
                    if (childActivity.Enabled && !(childActivity.ExecutionStatus == ActivityExecutionStatus.Initialized || childActivity.ExecutionStatus == ActivityExecutionStatus.Closed))
                        throw new InvalidOperationException(SR.GetString(System.Globalization.CultureInfo.CurrentCulture, SR.Error_ActiveChildExist));

                ActivityExecutionContext currentContext = new ActivityExecutionContext(this);

                foreach (ActivityExecutionContext childContext in currentContext.ExecutionContextManager.ExecutionContexts)
                {
                    if (this.GetActivityByName(childContext.Activity.QualifiedName, true) != null)
                        throw new InvalidOperationException(SR.GetString(System.Globalization.CultureInfo.CurrentCulture, SR.Error_ActiveChildContextExist));
                }
            }

            if (this.LockCountOnStatusChange > 0)
            {
                this.SetValue(HasPrimaryClosedProperty, true);
                this.FireStatusChangedEvents(Activity.StatusChangedLockedEvent, false);
            }
            else if (this.parent == null ||
                        (this.ExecutionResult == ActivityExecutionResult.Succeeded && (this is ICompensatableActivity || this.PersistOnClose))
                    )
            {
                ActivityExecutionStatus oldStatus = this.ExecutionStatus;
                ActivityExecutionResult oldOutcome = this.ExecutionResult;

                this.SetStatus(ActivityExecutionStatus.Closed, true);
                try
                {
                    // The activity may remove any instance specific dependency properties to reduce serialization size
                    this.OnClosed(this.RootActivity.WorkflowCoreRuntime);
                }
                catch (Exception e)
                {
                    this.SetValue(ExecutionResultProperty, ActivityExecutionResult.Faulted);
                    this.SetValueCommon(ActivityExecutionContext.CurrentExceptionProperty, e, ActivityExecutionContext.CurrentExceptionProperty.DefaultMetadata, false);
                }


                if (this.parent != null && (this is ICompensatableActivity))
                {
                    this.SetValue(CompletedOrderIdProperty, this.IncrementCompletedOrderId());
                }
                if (CanUninitializeNow)
                {
                    //

                    this.Uninitialize(this.RootActivity.WorkflowCoreRuntime);
                    this.SetValue(ExecutionResultProperty, ActivityExecutionResult.Uninitialized);
                }
                else if (this.parent == null) //Root Activity Closure
                {
                    UninitializeCompletedContext(this, new ActivityExecutionContext(this));
                }

                try
                {
                    Exception exception = (Exception)this.GetValue(ActivityExecutionContext.CurrentExceptionProperty);
                    if (exception != null && this.parent == null)
                    {
                        this.WorkflowCoreRuntime.ActivityStatusChanged(this, false, true);
                        // terminate the workflow instance
                        string errorString = "Uncaught exception escaped to the root of the workflow.\n"
                            + string.Format(CultureInfo.CurrentCulture, "    In instance {0} in activity {1}\n", new object[] { this.WorkflowInstanceId, string.Empty })
                            + string.Format(CultureInfo.CurrentCulture, "Inner exception: {0}", new object[] { exception });
                        System.Workflow.Runtime.WorkflowTrace.Runtime.TraceEvent(TraceEventType.Critical, 0, errorString);
                        this.WorkflowCoreRuntime.TerminateInstance(exception);
                    }
                    else if (exception != null && this.parent != null)
                    {
                        this.WorkflowCoreRuntime.RaiseException(exception, this.Parent, string.Empty);
                        this.RemoveProperty(ActivityExecutionContext.CurrentExceptionProperty);
                    }
                    else if (this.parent == null || this.PersistOnClose)
                    {
                        this.WorkflowCoreRuntime.PersistInstanceState(this);
                        this.WorkflowCoreRuntime.ActivityStatusChanged(this, false, true);

                        // throw exception to outer
                        if (exception != null)
                        {
                            this.WorkflowCoreRuntime.RaiseException(exception, this.Parent, string.Empty);
                            this.RemoveProperty(ActivityExecutionContext.CurrentExceptionProperty);
                        }
                    }

                    // remove the cached lock values
                    Activity parent = this.parent;
                    while (parent != null)
                    {
                        if (parent.SupportsSynchronization || parent.Parent == null)
                            parent.RemoveProperty(ActivityExecutionContext.CachedGrantedLocksProperty);

                        parent = parent.parent;
                    }

                }
                catch
                {
                    if (this.parent != null && (this is ICompensatableActivity))
                    {
                        this.RemoveProperty(CompletedOrderIdProperty);
                        this.DecrementCompletedOrderId();
                    }
                    this.SetValue(ExecutionResultProperty, oldOutcome);
                    this.SetStatus(oldStatus, true);

                    // copy back the old locks values
                    Activity parent = this.parent;
                    while (parent != null)
                    {
                        if (parent.SupportsSynchronization || parent.Parent == null)
                        {
                            object cachedGrantedLocks = parent.GetValue(ActivityExecutionContext.CachedGrantedLocksProperty);
                            if (cachedGrantedLocks != null)
                                parent.SetValue(ActivityExecutionContext.GrantedLocksProperty, cachedGrantedLocks);
                            parent.RemoveProperty(ActivityExecutionContext.CachedGrantedLocksProperty);
                        }
                        parent = parent.parent;
                    }
                    throw;
                }
            }
            else
            {
                // The activity may remove any instance specific dependency properties to reduce serialization size
                this.SetStatus(ActivityExecutionStatus.Closed, false);
                try
                {
                    this.OnClosed(this.RootActivity.WorkflowCoreRuntime);
                }
                catch (Exception e)
                {
                    this.SetValue(ExecutionResultProperty, ActivityExecutionResult.Faulted);
                    this.SetValueCommon(ActivityExecutionContext.CurrentExceptionProperty, e, ActivityExecutionContext.CurrentExceptionProperty.DefaultMetadata, false);
                }
                if (CanUninitializeNow)
                {
                    //

                    this.Uninitialize(this.RootActivity.WorkflowCoreRuntime);
                    this.SetValue(ExecutionResultProperty, ActivityExecutionResult.Uninitialized);
                }

                Exception exception = (Exception)this.GetValue(ActivityExecutionContext.CurrentExceptionProperty);
                if (exception != null)
                {
                    this.WorkflowCoreRuntime.RaiseException(exception, this.Parent, string.Empty);
                    this.RemoveProperty(ActivityExecutionContext.CurrentExceptionProperty);
                }
            }
        }

        internal bool CanUninitializeNow
        {
            get
            {
                //This check finds
                //1) Existence of Succeeded ISC in same context.
                //2) If activity is ContextActivity? Checks existence of completed child 
                //context which needs compensation            
                if (this.NeedsCompensation)
                    return false;

                //3) If this activity is not a context activity, check for completed child context
                //which needs compensation.
                Activity contextActivity = this.ContextActivity;

                if (contextActivity != this)
                {
                    IList<ActivityExecutionContextInfo> childsCompletedContexts = contextActivity.GetValue(Activity.CompletedExecutionContextsProperty) as IList<ActivityExecutionContextInfo>;

                    if (childsCompletedContexts != null && childsCompletedContexts.Count > 0)
                    {
                        foreach (ActivityExecutionContextInfo contextInfo in childsCompletedContexts)
                        {
                            if ((contextInfo.Flags & PersistFlags.NeedsCompensation) != 0 && this.GetActivityByName(contextInfo.ActivityQualifiedName, true) != null)
                            {
                                return false;
                            }
                        }
                    }
                }

                //Safe to Uninitialize this activity now.
                return true;
            }
        }

        static void UninitializeCompletedContext(Activity activity, ActivityExecutionContext executionContext)
        {
            //Uninitialize Compensatable Children which ran in Sub Contextee.
            IList<ActivityExecutionContextInfo> childsCompletedContexts = activity.GetValue(Activity.CompletedExecutionContextsProperty) as IList<ActivityExecutionContextInfo>;

            if (childsCompletedContexts != null && childsCompletedContexts.Count > 0)
            {
                IList<ActivityExecutionContextInfo> childsCompletedContextsClone = new List<ActivityExecutionContextInfo>(childsCompletedContexts);
                foreach (ActivityExecutionContextInfo contextInfo in childsCompletedContextsClone)
                {
                    if ((contextInfo.Flags & PersistFlags.NeedsCompensation) != 0 && activity.GetActivityByName(contextInfo.ActivityQualifiedName, true) != null)
                    {
                        ActivityExecutionContext resurrectedContext = executionContext.ExecutionContextManager.DiscardPersistedExecutionContext(contextInfo);
                        UninitializeCompletedContext(resurrectedContext.Activity, resurrectedContext);
                        executionContext.ExecutionContextManager.CompleteExecutionContext(resurrectedContext);
                    }
                }
            }

            //UnInitialize any compensatable children which ran in same context.
            CompositeActivity compositeActivity = activity as CompositeActivity;
            if (compositeActivity != null)
            {
                Activity[] compensatableChildren = CompensationUtils.GetCompensatableChildren(compositeActivity);

                for (int i = compensatableChildren.Length - 1; i >= 0; --i)
                {
                    Activity compensatableChild = (Activity)compensatableChildren.GetValue(i);
                    compensatableChild.Uninitialize(activity.RootActivity.WorkflowCoreRuntime);
                    compensatableChild.SetValue(ExecutionResultProperty, ActivityExecutionResult.Uninitialized);
                }
            }

            //UnInitialize Self
            activity.Uninitialize(activity.RootActivity.WorkflowCoreRuntime);
            activity.SetValue(ExecutionResultProperty, ActivityExecutionResult.Uninitialized);
        }

        internal bool NeedsCompensation
        {
            get
            {
                IList<ActivityExecutionContextInfo> childsCompletedContexts = this.GetValue(Activity.CompletedExecutionContextsProperty) as IList<ActivityExecutionContextInfo>;
                if (childsCompletedContexts != null && childsCompletedContexts.Count > 0)
                {
                    foreach (ActivityExecutionContextInfo completedActivityInfo in childsCompletedContexts)
                    {
                        if ((completedActivityInfo.Flags & PersistFlags.NeedsCompensation) != 0 && this.GetActivityByName(completedActivityInfo.ActivityQualifiedName, true) != null)
                            return true;
                    }
                }

                // walk through all compensatable children and compensate them
                Queue<Activity> completedActivities = new Queue<Activity>();
                completedActivities.Enqueue(this);
                while (completedActivities.Count > 0)
                {
                    Activity completedChild = completedActivities.Dequeue();
                    if (completedChild is ICompensatableActivity &&
                         completedChild.ExecutionStatus == ActivityExecutionStatus.Closed &&
                         completedChild.ExecutionResult == ActivityExecutionResult.Succeeded)
                        return true;

                    if (completedChild is CompositeActivity)
                    {
                        foreach (Activity completedChild2 in ((CompositeActivity)completedChild).Activities)
                        {
                            if (completedChild2.Enabled)
                                completedActivities.Enqueue(completedChild2);
                        }
                    }
                }
                return false;
            }
        }

        #endregion

        #region Behaviors Supports

        internal bool SupportsTransaction
        {
            get
            {
                return this is CompensatableTransactionScopeActivity || this is TransactionScopeActivity;
            }
        }
        internal bool SupportsSynchronization
        {
            get
            {
                return this is SynchronizationScopeActivity;
            }
        }
        internal bool PersistOnClose
        {
            get
            {
                if (this.UserData.Contains(typeof(PersistOnCloseAttribute)))
                    return (bool)this.UserData[typeof(PersistOnCloseAttribute)];

                object[] attributes = this.GetType().GetCustomAttributes(typeof(PersistOnCloseAttribute), true);
                return (attributes != null && attributes.Length > 0);
            }
        }

        internal int IncrementCompletedOrderId()
        {
            int completedOrderId = (int)this.RootActivity.GetValue(Activity.CompletedOrderIdProperty);
            this.RootActivity.SetValue(Activity.CompletedOrderIdProperty, completedOrderId + 1);
            return (completedOrderId + 1);
        }
        internal void DecrementCompletedOrderId()
        {
            int completedOrderId = (int)this.RootActivity.GetValue(Activity.CompletedOrderIdProperty);
            this.RootActivity.SetValue(Activity.CompletedOrderIdProperty, completedOrderId - 1);
        }

        #endregion

        #region EnumToString Converters

        internal static string ActivityExecutionStatusEnumToString(ActivityExecutionStatus status)
        {
            string retVal = string.Empty;
            switch (status)
            {
                case ActivityExecutionStatus.Initialized:
                    retVal = "Initialized";
                    break;
                case ActivityExecutionStatus.Executing:
                    retVal = "Executing";
                    break;
                case ActivityExecutionStatus.Canceling:
                    retVal = "Canceling";
                    break;
                case ActivityExecutionStatus.Faulting:
                    retVal = "Faulting";
                    break;
                case ActivityExecutionStatus.Compensating:
                    retVal = "Compensating";
                    break;
                case ActivityExecutionStatus.Closed:
                    retVal = "Closed";
                    break;
            }
            return retVal;
        }
        internal static string ActivityExecutionResultEnumToString(ActivityExecutionResult activityExecutionResult)
        {
            string retVal = string.Empty;
            switch (activityExecutionResult)
            {
                case ActivityExecutionResult.None:
                    retVal = "None";
                    break;
                case ActivityExecutionResult.Succeeded:
                    retVal = "Succeeded";
                    break;
                case ActivityExecutionResult.Canceled:
                    retVal = "Canceled";
                    break;
                case ActivityExecutionResult.Faulted:
                    retVal = "Faulted";
                    break;
                case ActivityExecutionResult.Compensated:
                    retVal = "Compensated";
                    break;
            }
            return retVal;
        }

        #endregion

        public override String ToString()
        {
            return this.QualifiedName + " [" + GetType().FullName + "]";
        }
    }
    #endregion

    #region Class CompositeActivity

    [ContentProperty("Activities")]
    [DesignerSerializer(typeof(CompositeActivityMarkupSerializer), typeof(WorkflowMarkupSerializer))]
    [ActivityCodeGenerator(typeof(CompositeActivityCodeGenerator))]
    [ActivityValidator(typeof(CompositeActivityValidator))]
    [ActivityExecutor(typeof(CompositeActivityExecutor<CompositeActivity>))]
    [TypeDescriptionProvider(typeof(CompositeActivityTypeDescriptorProvider))]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class CompositeActivity : Activity, ISupportAlternateFlow
    {
        private static DependencyProperty CanModifyActivitiesProperty = DependencyProperty.Register("CanModifyActivities", typeof(bool), typeof(CompositeActivity), new PropertyMetadata(DependencyPropertyOptions.Metadata, new Attribute[] { new DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden) }));

        [NonSerialized]
        private ActivityCollection activities = null;

        public CompositeActivity()
        {
            this.activities = new ActivityCollection(this);
            this.activities.ListChanging += new EventHandler<ActivityCollectionChangeEventArgs>(OnListChangingEventHandler);
            this.activities.ListChanged += new EventHandler<ActivityCollectionChangeEventArgs>(OnListChangedEventHandler);

            SetValue(CanModifyActivitiesProperty, false);
        }

        public CompositeActivity(IEnumerable<Activity> children)
            : this()
        {
            if (children == null)
                throw new ArgumentNullException("children");

            foreach (Activity child in children)
                this.activities.Add(child);
        }

        public CompositeActivity(string name)
            : base(name)
        {
            this.activities = new ActivityCollection(this);
            this.activities.ListChanging += new EventHandler<ActivityCollectionChangeEventArgs>(OnListChangingEventHandler);
            this.activities.ListChanged += new EventHandler<ActivityCollectionChangeEventArgs>(OnListChangedEventHandler);

            SetValue(CanModifyActivitiesProperty, false);
        }

        protected Activity[] GetDynamicActivities(Activity activity)
        {
            if (activity == null)
                throw new ArgumentNullException("activity");

            // Work around - make sure that we return the parent property value
            // otherwise .Parent property would have set the workflowCoreRuntime for parent
            if (activity.parent != this)
                throw new ArgumentException(SR.GetString(SR.GetDynamicActivities_InvalidActivity), "activity");

            // get context activity
            Activity contextActivity = this.ContextActivity;
            List<Activity> dynamicActivities = new List<Activity>();
            if (contextActivity != null)
            {
                IList<Activity> activeContextActivities = (IList<Activity>)contextActivity.GetValue(Activity.ActiveExecutionContextsProperty);
                if (activeContextActivities != null)
                {
                    foreach (Activity activeContextActivity in activeContextActivities)
                    {
                        if (activeContextActivity.MetaEquals(activity))
                            dynamicActivities.Add(activeContextActivity);
                    }
                }
            }
            return dynamicActivities.ToArray();
        }

        protected internal override void Initialize(IServiceProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException("provider");

            if (!(provider is ActivityExecutionContext))
                throw new ArgumentException(SR.GetString(SR.Error_InvalidServiceProvider, "provider"));

            foreach (Activity childActivity in Helpers.GetAllEnabledActivities(this))
            {
                ActivityExecutionContext executionContext = provider as ActivityExecutionContext;
                executionContext.InitializeActivity(childActivity);
            }
        }

        protected internal override void Uninitialize(IServiceProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException("provider");

            foreach (Activity childActivity in Helpers.GetAllEnabledActivities(this))
            {
                //Case for Template/Conditional Branch/Non Context based Composite which
                //exists in path between compensatable context.
                if (childActivity.ExecutionResult != ActivityExecutionResult.Uninitialized)
                {
                    childActivity.Uninitialize(provider);
                    childActivity.SetValue(ExecutionResultProperty, ActivityExecutionResult.Uninitialized);
                }
            }
            base.Uninitialize(provider);
        }
        protected internal override void OnActivityExecutionContextLoad(IServiceProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException("provider");

            base.OnActivityExecutionContextLoad(provider);

            foreach (Activity childActivity in Helpers.GetAllEnabledActivities(this))
            {
                childActivity.OnActivityExecutionContextLoad(provider);
            }
        }
        protected internal override void OnActivityExecutionContextUnload(IServiceProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException("provider");

            base.OnActivityExecutionContextUnload(provider);

            foreach (Activity childActivity in Helpers.GetAllEnabledActivities(this))
            {
                childActivity.OnActivityExecutionContextUnload(provider);
            }
        }

        protected internal override ActivityExecutionStatus HandleFault(ActivityExecutionContext executionContext, Exception exception)
        {
            if (executionContext == null)
                throw new ArgumentNullException("executionContext");

            if (exception == null)
                throw new ArgumentNullException("exception");

            ActivityExecutionStatus newStatus = this.Cancel(executionContext);
            if (newStatus == ActivityExecutionStatus.Canceling)
                return ActivityExecutionStatus.Faulting;

            return newStatus;
        }

        protected void ApplyWorkflowChanges(WorkflowChanges workflowChanges)
        {
            if (workflowChanges == null)
                throw new ArgumentNullException("workflowChanges");

            if (this.Parent != null)
                throw new InvalidOperationException(SR.GetString(SR.Error_InvalidActivityForWorkflowChanges));

            if (this.RootActivity == null)
                throw new InvalidOperationException(SR.GetString(SR.Error_MissingRootActivity));

            if (this.WorkflowCoreRuntime == null)
                throw new InvalidOperationException(SR.GetString(SR.Error_NoRuntimeAvailable));

            workflowChanges.ApplyTo(this);
        }


        #region Workflow Change Notification Methods
        protected internal virtual void OnActivityChangeAdd(ActivityExecutionContext executionContext, Activity addedActivity)
        {
            if (executionContext == null)
                throw new ArgumentNullException("executionContext");

            if (addedActivity == null)
                throw new ArgumentNullException("addedActivity");


        }

        protected internal virtual void OnActivityChangeRemove(ActivityExecutionContext executionContext, Activity removedActivity)
        {

        }

        protected internal virtual void OnWorkflowChangesCompleted(ActivityExecutionContext rootContext)
        {

        }
        #endregion

        #region CompositeActivity Meta Properties

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Browsable(false)]
        public ActivityCollection Activities
        {
            get
            {
                return this.activities;
            }
        }

        protected internal bool CanModifyActivities
        {
            get
            {
                return (bool)GetValue(CanModifyActivitiesProperty);
            }
            set
            {
                SetValue(CanModifyActivitiesProperty, value);
                if (this.Activities.Count > 0)
                    SetValue(CustomActivityProperty, true);
            }
        }

        #endregion

        #region CompositeActivity Instance Properties

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public ReadOnlyCollection<Activity> EnabledActivities
        {
            get
            {
                List<Activity> executableActivities = new List<Activity>();
                foreach (Activity activity in this.activities)
                {
                    // This check makes sure that only Enabled activities are returned 
                    // and the framwork provided activities are skipped.
                    if (activity.Enabled && !Helpers.IsFrameworkActivity(activity))
                        executableActivities.Add(activity);
                }
                return executableActivities.AsReadOnly();
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        IList<Activity> ISupportAlternateFlow.AlternateFlowActivities
        {
            get
            {
                List<Activity> secondaryFlowActivities = new List<Activity>();
                foreach (Activity activity in this.activities)
                {
                    if (activity.Enabled && Helpers.IsFrameworkActivity(activity))
                        secondaryFlowActivities.Add(activity);
                }
                return secondaryFlowActivities.AsReadOnly();
            }
        }

        internal override Activity TraverseDottedPath(string dottedPath)
        {
            string subPath = dottedPath;
            string remainingPath = string.Empty;
            int indexOfDot = dottedPath.IndexOf('.');
            if (indexOfDot != -1)
            {
                subPath = dottedPath.Substring(0, indexOfDot);
                remainingPath = dottedPath.Substring(indexOfDot + 1);
            }

            int index = Convert.ToInt32(subPath, CultureInfo.InvariantCulture);
            if (index >= this.activities.Count)
                return null;

            Activity nextActivity = this.activities[index];
            if (!string.IsNullOrEmpty(remainingPath))
                return nextActivity.TraverseDottedPath(remainingPath);
            return nextActivity;
        }

        #endregion

        #region Runtime Initialize

        internal override void OnInitializeDefinitionForRuntime()
        {
            // only if we are in design mode, execute this code, other wise ignore this call
            if (this.DesignMode)
            {
                // call base
                base.OnInitializeDefinitionForRuntime();

                // get the lookup path
                Activity rootActivity = this.RootActivity;
                Hashtable lookupPaths = (Hashtable)rootActivity.UserData[UserDataKeys.LookupPaths];
                string thisLookupPath = (string)lookupPaths[this.QualifiedName];

                // call initializeForRuntime for every activity
                foreach (Activity childActivity in (IList)this.activities)
                {
                    if (childActivity.Enabled)
                    {
                        // setup lookup path
                        string lookupPath = thisLookupPath;
                        if (!string.IsNullOrEmpty(thisLookupPath))
                            lookupPath += ".";
                        lookupPath += this.activities.IndexOf(childActivity).ToString(CultureInfo.InvariantCulture);
                        lookupPaths.Add(childActivity.QualifiedName, lookupPath);

                        // initialize for runtime
                        ((IDependencyObjectAccessor)childActivity).InitializeDefinitionForRuntime(null);
                    }
                    else
                    {
                        // There are sevrel properties that are required even for disabled activities.  They include
                        // DottedPath, QualifiedName and Readonly.  To initialize these properties, we call
                        // OnInitializeDefinitionForRuntime directly and skip the InitializeProperties call inside
                        // IDependencyObjectAccessor.InitializeDefinitionForRuntime because other properties are not
                        // validated and they may not initialize properly, which the runtime really doesn't care.
                        childActivity.OnInitializeDefinitionForRuntime();
                        childActivity.Readonly = true;
                    }
                }
            }
        }

        internal override void OnInitializeInstanceForRuntime(IWorkflowCoreRuntime workflowCoreRuntime)
        {
            base.OnInitializeInstanceForRuntime(workflowCoreRuntime);

            // call children to do initialize runtime instance
            foreach (Activity childActivity in this.activities)
            {
                if (childActivity.Enabled)
                    ((IDependencyObjectAccessor)childActivity).InitializeInstanceForRuntime(workflowCoreRuntime);
            }
        }

        internal override void OnInitializeActivatingInstanceForRuntime(IWorkflowCoreRuntime workflowCoreRuntime)
        {
            // call base: this will call activity
            base.OnInitializeActivatingInstanceForRuntime(workflowCoreRuntime);

            // call children to do initialize runtime instance
            foreach (Activity childActivity in this.activities)
            {
                if (childActivity.Enabled)
                    ((IDependencyObjectAccessor)childActivity).InitializeActivatingInstanceForRuntime(null, workflowCoreRuntime);
                else
                    this.Readonly = true;
            }
        }

        internal override void FixUpMetaProperties(DependencyObject originalObject)
        {
            if (originalObject == null)
                throw new ArgumentNullException();
            if (!(originalObject is CompositeActivity))
                throw new ArgumentException();

            // call base
            base.FixUpMetaProperties(originalObject);

            // ask each child to fixup 
            if (this.activities != null)
            {
                CompositeActivity originalCompositeActivity = originalObject as CompositeActivity;
                if (originalCompositeActivity != null)
                {
                    int index = 0;
                    foreach (Activity childActivity in this.activities)
                        childActivity.FixUpMetaProperties(originalCompositeActivity.activities[index++]);
                }
            }
        }
        internal override void FixUpParentChildRelationship(Activity definitionActivity, Activity parentActivity, Hashtable deserializedActivities)
        {
            CompositeActivity definitionCompositeActivity = definitionActivity as CompositeActivity;
            if (definitionCompositeActivity == null)
                throw new ArgumentException("definitionActivity");

            // call base
            base.FixUpParentChildRelationship(definitionActivity, parentActivity, deserializedActivities);

            // fix up the children collection
            this.activities = new ActivityCollection(this);
            this.activities.ListChanging += new EventHandler<ActivityCollectionChangeEventArgs>(OnListChangingEventHandler);
            this.activities.ListChanged += new EventHandler<ActivityCollectionChangeEventArgs>(OnListChangedEventHandler);

            // detect if there was a context
            string prefix = this.DottedPath;

            // fixup all the children, and then call them to fixup their relation ships
            int index = 0;
            foreach (Activity definitionChildActivity in definitionCompositeActivity.activities)
            {
                Activity childActivity = (Activity)deserializedActivities[prefix.Length == 0 ? index.ToString(CultureInfo.InvariantCulture) : prefix + "." + index.ToString(CultureInfo.InvariantCulture)];
                this.activities.InnerAdd(childActivity);

                // ask child to fix up its parent child relation ship
                childActivity.FixUpParentChildRelationship(definitionChildActivity, this, deserializedActivities);
                index++;
            }
        }
        internal override IList<Activity> CollectNestedActivities()
        {
            List<Activity> nestedActivities = new List<Activity>();
            Queue<Activity> activityQueue = new Queue<Activity>(this.activities);
            while (activityQueue.Count > 0)
            {
                Activity nestedActivity = activityQueue.Dequeue();
                nestedActivities.Add(nestedActivity);
                if (nestedActivity is CompositeActivity)
                {
                    foreach (Activity nestedChildActivity in ((CompositeActivity)nestedActivity).activities)
                        activityQueue.Enqueue(nestedChildActivity);
                }
            }
            return nestedActivities;
        }

        #endregion

        #region Collection's Event Handlers
        private void OnListChangingEventHandler(object sender, ActivityCollectionChangeEventArgs e)
        {
            if (!this.DesignMode && !this.DynamicUpdateMode)
                throw new InvalidOperationException(SR.GetString(SR.Error_CanNotChangeAtRuntime));

            if (!this.CanModifyActivities)
            {
                // Check the ActivityType only during design mode
                if (this.DesignMode && Activity.ActivityType != null && this.GetType() == Activity.ActivityType)
                    throw new InvalidOperationException(SR.GetString(SR.Error_Missing_CanModifyProperties_True, this.GetType().FullName));

                if (!IsDynamicMode(this) && CannotModifyChildren(this, false))
                    throw new InvalidOperationException(SR.GetString(SR.Error_CannotAddRemoveChildActivities));

                if (IsDynamicMode(this) && CannotModifyChildren(this, true))
                    throw new InvalidOperationException(SR.GetString(SR.Error_CannotAddRemoveChildActivities));
            }

            if (e.Action == ActivityCollectionChangeAction.Add && e.AddedItems != null)
            {
                Activity parent = this;
                while (parent != null)
                {
                    if (e.AddedItems.Contains(parent))
                        throw new InvalidOperationException(SR.GetString(SR.Error_ActivityCircularReference));

                    parent = parent.Parent;
                }
            }

            OnListChanging(e);
        }

        protected virtual void OnListChanging(ActivityCollectionChangeEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException("e");
            if (e.Action == ActivityCollectionChangeAction.Add && e.AddedItems != null)
            {
                foreach (Activity activity in e.AddedItems)
                {
                    if (activity.Parent != null)
                        throw new InvalidOperationException(SR.GetString(SR.Error_ActivityHasParent, activity.QualifiedName, activity.Parent.QualifiedName));
                    if (activity == this)
                        throw new InvalidOperationException(SR.GetString(SR.Error_Recursion, activity.QualifiedName));
                }
            }
            if (((IComponent)this).Site != null)
            {
                IComponentChangeService changeService = ((IComponent)this).Site.GetService(typeof(IComponentChangeService)) as IComponentChangeService;
                if (changeService != null)
                    changeService.OnComponentChanging(this, null);
            }
        }

        private void OnListChangedEventHandler(object sender, ActivityCollectionChangeEventArgs e)
        {
            OnListChanged(e);
        }

        protected virtual void OnListChanged(ActivityCollectionChangeEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException("e");
            // remove the parent
            if ((e.Action == ActivityCollectionChangeAction.Replace || e.Action == ActivityCollectionChangeAction.Remove) &&
                    e.RemovedItems != null)
            {
                foreach (Activity activity in e.RemovedItems)
                    activity.SetParent(null);
            }

            // set the parent on the activity
            if ((e.Action == ActivityCollectionChangeAction.Replace || e.Action == ActivityCollectionChangeAction.Add) &&
                    e.AddedItems != null)
            {
                foreach (Activity activity in e.AddedItems)
                    activity.SetParent(this);

                Queue<Activity> queue = new Queue<Activity>(e.AddedItems as IEnumerable<Activity>);
                while (queue.Count > 0)
                {
                    Activity activity = queue.Dequeue() as Activity;
                    if (activity != null && (activity.Name == null || activity.Name.Length == 0 || activity.Name == activity.GetType().Name))
                    {
                        Activity rootActivity = Helpers.GetRootActivity(activity);
                        string className = rootActivity.GetValue(WorkflowMarkupSerializer.XClassProperty) as string;
                        if (rootActivity.Parent == null || !string.IsNullOrEmpty(className))
                        {
                            ArrayList identifiers = new ArrayList();
                            identifiers.AddRange(Helpers.GetIdentifiersInCompositeActivity(rootActivity as CompositeActivity));
                            activity.Name = DesignerHelpers.GenerateUniqueIdentifier(((IComponent)this).Site, Helpers.GetBaseIdentifier(activity), (string[])identifiers.ToArray(typeof(string)));
                        }
                    }
                    if (activity is CompositeActivity)
                    {
                        foreach (Activity activity2 in ((CompositeActivity)activity).Activities)
                            queue.Enqueue(activity2);
                    }
                }
            }

            // 
            if (((IComponent)this).Site != null)
            {
                IComponentChangeService changeService = ((IComponent)this).Site.GetService(typeof(IComponentChangeService)) as IComponentChangeService;
                if (changeService != null)
                    changeService.OnComponentChanged(this, null, e, null);
            }
        }

        private static bool IsDynamicMode(CompositeActivity compositeActivity)
        {
            if (compositeActivity == null)
                throw new ArgumentNullException("compositeActivity");

            while (compositeActivity.Parent != null)
            {
                if (compositeActivity.DynamicUpdateMode)
                    return true;
                compositeActivity = compositeActivity.Parent;
            }
            return compositeActivity.DynamicUpdateMode;
        }

        private static bool CannotModifyChildren(CompositeActivity compositeActivity, bool parent)
        {
            if (compositeActivity == null)
                throw new ArgumentNullException("compositeActivity");

            if (parent && compositeActivity.Parent == null)
                return false;

            if ((bool)compositeActivity.GetValue(CustomActivityProperty) == true)
                return true;

            if (compositeActivity.Parent != null)
                return CannotModifyChildren(compositeActivity.Parent, parent);

            return false;
        }

        #endregion

        #region IDisposable
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (Activity activity in this.Activities)
                    activity.Dispose();
            }
            base.Dispose(disposing);
        }
        #endregion
    }
    #endregion
}
