//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Workflow.Activities
{
    using System;
    using System.Reflection;
    using System.Collections;
    using System.Collections.ObjectModel;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Globalization;
    using System.IdentityModel.Claims;
    using System.Security.Permissions;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;
    using System.Workflow.Activities;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.ComponentModel.Design;
    using System.Workflow.ComponentModel.Serialization;
    using System.Workflow.Runtime;
    using System.Xml;

    [SR2Description(SR2DescriptionAttribute.ReceiveActivityDescription)]
    [SR2Category(SR2CategoryAttribute.Standard)]
    [Designer(typeof(ReceiveActivityDesigner), typeof(IDesigner))]
    [ToolboxBitmap(typeof(ReceiveActivity), "Design.Resources.ReceiveActivity.png")]
    [ActivityValidator(typeof(ReceiveActivityValidator))]
    [Serializable]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class ReceiveActivity : SequenceActivity,
        IEventActivity,
        IActivityEventListener<QueueEventArgs>,
        IServiceDescriptionBuilder
    {
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly DependencyProperty FaultMessageProperty =
            DependencyProperty.Register("FaultMessage",
            typeof(FaultException),
            typeof(ReceiveActivity),
            new PropertyMetadata(null));

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly DependencyProperty OperationValidationEvent =
            DependencyProperty.Register("OperationValidation",
            typeof(EventHandler<OperationValidationEventArgs>),
            typeof(ReceiveActivity));

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly DependencyProperty WorkflowServiceAttributesProperty =
            DependencyProperty.RegisterAttached("WorkflowServiceAttributes",
            typeof(WorkflowServiceAttributes), typeof(ReceiveActivity),
            new PropertyMetadata(null, DependencyPropertyOptions.Metadata,
            ReceiveActivity.GetWorkflowServiceAttributesValueOverride, null),
            typeof(WorkflowServiceAttributesDynamicPropertyValidator));

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        internal static readonly DependencyProperty CanCreateInstanceProperty =
            DependencyProperty.Register("CanCreateInstance",
            typeof(bool),
            typeof(ReceiveActivity),
            new PropertyMetadata(false, DependencyPropertyOptions.Metadata));

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        internal static readonly DependencyProperty ContextTokenProperty =
            DependencyProperty.Register("ContextToken",
            typeof(ContextToken),
            typeof(ReceiveActivity),
            new PropertyMetadata(null, DependencyPropertyOptions.Metadata));

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        internal static readonly DependencyProperty ParameterBindingsProperty =
            DependencyProperty.Register("ParameterBindings",
            typeof(WorkflowParameterBindingCollection),
            typeof(ReceiveActivity),
            new PropertyMetadata(DependencyPropertyOptions.Metadata | DependencyPropertyOptions.ReadOnly));

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        internal static readonly DependencyProperty ServiceOperationInfoProperty =
            DependencyProperty.Register("ServiceOperationInfo",
            typeof(OperationInfoBase),
            typeof(ReceiveActivity),
            new PropertyMetadata(DependencyPropertyOptions.Metadata));

        private static readonly DependencyProperty QueueNameProperty =
            DependencyProperty.Register("QueueName",
            typeof(string),
            typeof(ReceiveActivity));

        private static readonly DependencyProperty RequestContextProperty =
            DependencyProperty.Register("RequestContext",
            typeof(WorkflowRequestContext),
            typeof(ReceiveActivity));

        static DependencyProperty QueueInitializationModeProperty =
            DependencyProperty.Register("QueueInitializationMode",
            typeof(QueueInitializationMode),
            typeof(ReceiveActivity),
            new PropertyMetadata(QueueInitializationMode.Standalone));

        [NonSerialized]
        private ReceiveOperationInfoHelper operationHelper;

        private IActivityEventListener<QueueEventArgs> securityShim;
        private IActivityEventListener<QueueEventArgs> validationShim;

        [NonSerialized]
        private static Hashtable requestContextsCache = Hashtable.Synchronized(new Hashtable());
        private bool isContextCached;

        public ReceiveActivity()
        {
            base.SetReadOnlyPropertyValue(ReceiveActivity.ParameterBindingsProperty,
                new WorkflowParameterBindingCollection(this));
        }

        public ReceiveActivity(string name)
            : base(name)
        {
            base.SetReadOnlyPropertyValue(ReceiveActivity.ParameterBindingsProperty,
                new WorkflowParameterBindingCollection(this));
        }

        [SRCategory(SR2CategoryAttribute.Handlers)]
        [SR2Description(SR2DescriptionAttribute.Receive_OperationValidation_Description)]
        [MergableProperty(false)]
        public event EventHandler<OperationValidationEventArgs> OperationValidation
        {
            add
            {
                base.AddHandler(OperationValidationEvent, value);
            }
            remove
            {
                base.RemoveHandler(OperationValidationEvent, value);
            }
        }

        [Browsable(true)]
        [DefaultValue(false)]
        [SR2Category(SR2CategoryAttribute.Activity)]
        [SR2Description(SR2DescriptionAttribute.Receive_CanCreateInstance_Description)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public bool CanCreateInstance
        {
            get
            {
                return ((bool)(base.GetValue(ReceiveActivity.CanCreateInstanceProperty)));
            }
            set
            {
                base.SetValue(ReceiveActivity.CanCreateInstanceProperty, value);
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IDictionary<string, string> Context
        {
            get
            {
                if (this.ContextToken == null)
                {
                    return ReceiveActivity.GetRootContext(this);
                }
                return ReceiveActivity.GetContext(this, this.ContextToken);
            }
        }

        [DefaultValue(null)]
        [MergableProperty(false)]
        [RefreshProperties(RefreshProperties.All)]
        [SR2Category(SR2CategoryAttribute.Activity)]
        [SR2Description(SR2DescriptionAttribute.Receive_ContextToken_Description)]
        [TypeConverter(typeof(ContextTokenTypeConverter))]
        public ContextToken ContextToken
        {
            get
            {
                return base.GetValue(ContextTokenProperty) as ContextToken;
            }
            set
            {
                base.SetValue(ContextTokenProperty, value);
            }
        }

        [Browsable(true)]
        [DefaultValue(null)]
        [SR2Category(SR2CategoryAttribute.Activity)]
        [SR2Description(SR2DescriptionAttribute.Receive_FaultMessage_Description)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public FaultException FaultMessage
        {
            get
            {
                return ((FaultException)base.GetValue(ReceiveActivity.FaultMessageProperty));
            }

            set
            {
                base.SetValue(ReceiveActivity.FaultMessageProperty, value);
            }
        }


        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public WorkflowParameterBindingCollection ParameterBindings
        {
            get
            {
                return ((WorkflowParameterBindingCollection)(base.GetValue(ReceiveActivity.ParameterBindingsProperty)));
            }
        }


        [Browsable(true)]
        [SR2Category(SR2CategoryAttribute.Activity)]
        [SR2Description(SR2DescriptionAttribute.Receive_OperationInfo_Description)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public OperationInfoBase ServiceOperationInfo
        {
            get
            {
                return ((OperationInfoBase)(base.GetValue(ReceiveActivity.ServiceOperationInfoProperty)));
            }
            set
            {
                OperationInfoBase currentValue = ((OperationInfoBase)(base.GetValue(ReceiveActivity.ServiceOperationInfoProperty)));
                if (value != null && currentValue != value)
                {
                    DependencyProperty ParentDependencyObjectProperty =
                        DependencyProperty.FromName("ParentDependencyObject", typeof(DependencyObject));

                    Activity currentParent = value.GetValue(ParentDependencyObjectProperty) as Activity;

                    if (currentParent != null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(
                            "value",
                            SR2.GetString(SR2.Error_OperationIsAlreadyAssociatedWithActivity,
                            value,
                            currentParent.QualifiedName));
                    }

                    if (currentValue != null)
                    {
                        currentValue.SetValue(ParentDependencyObjectProperty, null);
                    }

                    value.SetValue(ParentDependencyObjectProperty, this);
                }

                if (this.DesignMode && value is OperationInfo)
                {
                    Activity rootActivity = this.RootActivity;
                    rootActivity.RemoveProperty(DynamicContractTypeBuilder.DynamicContractTypesProperty);
                }

                base.SetValue(ReceiveActivity.ServiceOperationInfoProperty, value);
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        internal WorkflowRequestContext RequestContext
        {
            get
            {
                return ((WorkflowRequestContext)(base.GetValue(ReceiveActivity.RequestContextProperty)));
            }
            set
            {
                base.SetValue(ReceiveActivity.RequestContextProperty, value);
            }
        }

        IComparable IEventActivity.QueueName
        {
            get { return this.GetValue(ReceiveActivity.QueueNameProperty) as string; }
        }

        ReceiveOperationInfoHelper OperationHelper
        {
            get
            {
                if (this.operationHelper == null)
                {
                    if (this.UserData.Contains(typeof(ReceiveOperationInfoHelper)))
                    {
                        this.operationHelper = this.UserData[typeof(ReceiveOperationInfoHelper)] as ReceiveOperationInfoHelper;
                    }
                }

                if (this.operationHelper == null)
                {
                    this.operationHelper = new ReceiveOperationInfoHelper(this.Site, this);
                    this.UserData[typeof(ReceiveOperationInfoHelper)] = this.operationHelper;
                }

                return this.operationHelper;
            }
        }

        private QueueInitializationMode QueueInitializationMode
        {
            get
            {
                return (QueueInitializationMode)base.GetValue(ReceiveActivity.QueueInitializationModeProperty);
            }
            set
            {
                base.SetValue(ReceiveActivity.QueueInitializationModeProperty, value);
            }
        }

        public static IDictionary<string, string> GetContext(Activity activity,
            ContextToken contextToken)
        {
            if (activity == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("activity");
            }
            if (contextToken == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("contextToken");
            }

            if (contextToken.IsRootContext)
            {
                return GetRootContext(activity);
            }

            return GetContext(activity, contextToken.Name, contextToken.OwnerActivityName);
        }

        public static IDictionary<string, string> GetContext(Activity activity,
            string contextName,
            string ownerActivityName)
        {
            if (activity == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("activity");
            }
            if (string.IsNullOrEmpty(contextName))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("contextName",
                    SR2.GetString(SR2.Error_ArgumentValueNullOrEmptyString));
            }

            ReceiveContext receiveContext = ContextToken.GetReceiveContext(activity, contextName, ownerActivityName);
            if (receiveContext == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new InvalidOperationException(SR2.GetString(SR2.Error_CannotFindReceiveContext, contextName)));
            }

            return receiveContext.Properties;
        }

        public static IDictionary<string, string> GetRootContext(Activity activity)
        {
            if (activity == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("activity");
            }

            ReceiveContext receiveContext = ContextToken.GetRootReceiveContext(activity);
            if (receiveContext == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new InvalidOperationException(SR2.GetString(SR2.Error_CannotFindReceiveContext, ContextToken.RootContextName)));
            }

            return receiveContext.Properties;
        }

        public static object GetWorkflowServiceAttributes(object dependencyObject)
        {
            if (dependencyObject == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("dependencyObject");
            }
            if (!(dependencyObject is Activity))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(
                    "dependencyObject",
                    SR2.GetString(SR2.Error_UnexpectedArgumentType, typeof(Activity).FullName));
            }

            return (dependencyObject as DependencyObject).GetValue(ReceiveActivity.WorkflowServiceAttributesProperty);
        }

        public static void SetWorkflowServiceAttributes(object dependencyObject,
            object value)
        {
            if (dependencyObject == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("dependencyObject");
            }
            if (!(dependencyObject is Activity))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(
                    "dependencyObject",
                    SR2.GetString(SR2.Error_UnexpectedArgumentType, typeof(Activity).FullName));
            }

            (dependencyObject as DependencyObject).SetValue(ReceiveActivity.WorkflowServiceAttributesProperty, value);
        }

        void IActivityEventListener<QueueEventArgs>.OnEvent(object sender,
            QueueEventArgs e)
        {
            if (sender == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("sender");
            }

            if (e == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("e");
            }

            ActivityExecutionContext executionContext = sender as ActivityExecutionContext;
            if (executionContext == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new ArgumentException(SR2.GetString(SR2.Error_ArgumentTypeInvalid,
                    "sender",
                    typeof(ActivityExecutionContext))));
            }

            WorkflowQueuingService queuingService = executionContext.GetService<WorkflowQueuingService>();
            if (queuingService == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new InvalidOperationException(SR2.GetString(SR2.General_MissingService,
                    typeof(WorkflowQueuingService))));
            }

            WorkflowQueue workflowQueue = queuingService.GetWorkflowQueue(e.QueueName);
            if (workflowQueue == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new InvalidOperationException(SR2.GetString(SR2.Error_QueueNotFound, e.QueueName)));
            }

            if (workflowQueue.Count != 0)
            {
                WorkflowRequestContext requestContext = workflowQueue.Peek() as WorkflowRequestContext;
                if (requestContext == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new InvalidOperationException(SR2.GetString(SR2.Error_RequestContextUnavailable,
                        this.QualifiedName)));
                }
                if (ValidationShim.EvaluateSecurityConstraints(executionContext, this, requestContext))
                {
                    workflowQueue.UnregisterForQueueItemAvailable(this);
                    this.RequestContext = workflowQueue.Dequeue() as WorkflowRequestContext;
                    CacheRequestContext(this.RequestContext);

                    if (this.QueueInitializationMode == QueueInitializationMode.Standalone)
                    {
                        if (this.securityShim != null)
                        {
                            workflowQueue.UnregisterForQueueItemArrived(this.securityShim);
                            this.securityShim = null;
                        }
                        workflowQueue.Enabled = false;
                    }

                    if (this.RequestContext == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                            new InvalidOperationException(SR2.GetString(SR2.Error_RequestContextUnavailable, this.QualifiedName)));
                    }

                    if (ExecuteActivity(this.RequestContext, executionContext) == ActivityExecutionStatus.Closed)
                    {
                        try
                        {
                            executionContext.CloseActivity();
                        }
                        finally
                        {
                            RemoveRequestContext();
                        }
                    }
                }
                else
                {
                    workflowQueue.Dequeue();

                    try
                    {
                        System.Workflow.Runtime.WorkflowTrace.Runtime.TraceEvent(TraceEventType.Verbose, 0,
                            "Workflow Instance {0}, receive activity {1} - message validation failed. Message will be discarded.",
                            this.WorkflowInstanceId, this.QualifiedName);

                        requestContext.SendFault(new FaultException(SR2.GetString(SR2.SecurityCheckFailed)), null);
                    }
                    catch (CommunicationException cex)
                    {
                        System.Workflow.Runtime.WorkflowTrace.Runtime.TraceEvent(TraceEventType.Error, 0,
                            "Workflow Instance {0}, receive activity {1} - failed to send fault for rejected message. Error: {2}",
                            this.WorkflowInstanceId, this.QualifiedName, cex);
                    }
                    catch (TimeoutException tex)
                    {
                        System.Workflow.Runtime.WorkflowTrace.Runtime.TraceEvent(TraceEventType.Error, 0,
                            "Workflow Instance {0}, receive activity {1} - failed to send fault for rejected message. Error: {2}",
                            this.WorkflowInstanceId, this.QualifiedName, tex);
                    }

                    if (requestContext.ContextProperties == null ||
                        !(requestContext.ContextProperties.Keys.Contains(WellKnownContextProperties.InstanceId)))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                            new FaultException(SR2.GetString(SR2.Error_FailedToValidateActivatingMessage, this.WorkflowInstanceId)));
                    }
                }
            }
        }

        void IEventActivity.Subscribe(ActivityExecutionContext parentContext,
            IActivityEventListener<QueueEventArgs> parentEventHandler)
        {
            if (parentContext == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("parentContext");
            }

            if (parentEventHandler == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("parentEventHandler");
            }

            if (this.QueueInitializationMode == QueueInitializationMode.Standalone)
            {
                this.QueueInitializationMode = QueueInitializationMode.EventDriven;
            }

            // make sure that we are getting the proper queue
            // even if that means creating a new queue
            // given our conversation context and execution context.
            //
            WorkflowQueue workflowQueue = GetWorkflowQueue(parentContext);
            if (workflowQueue != null)
            {
                if (this.QueueInitializationMode == QueueInitializationMode.EventDriven)
                {
                    workflowQueue.Enabled = true;
                }

                if (this.validationShim == null)
                {
                    this.validationShim = new ValidationShim(this, parentEventHandler);
                }
                if (this.securityShim == null)
                {
                    this.securityShim = new SecurityShim(this);
                }

                workflowQueue.RegisterForQueueItemArrived(this.securityShim);
                workflowQueue.RegisterForQueueItemAvailable(this.validationShim, this.QualifiedName);
            }

            return;
        }

        void IEventActivity.Unsubscribe(ActivityExecutionContext parentContext,
            IActivityEventListener<QueueEventArgs> parentEventHandler)
        {
            if (parentContext == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("parentContext");
            }

            if (parentEventHandler == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("parentEventHandler");
            }

            WorkflowQueuingService queuingService = parentContext.GetService<WorkflowQueuingService>();
            if (queuingService == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new InvalidOperationException(SR2.GetString(SR2.General_MissingService,
                    typeof(WorkflowQueuingService))));
            }

            // get the queue using the queue name
            // at this point the conversation context should have been re-initialized if necessary
            //            
            WorkflowQueue workflowQueue = queuingService.GetWorkflowQueue(((IEventActivity)this).QueueName);
            if (workflowQueue == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new InvalidOperationException(SR2.GetString(SR2.Error_QueueNotFound,
                    ((IEventActivity)this).QueueName)));
            }

            if (this.securityShim != null)
            {
                workflowQueue.UnregisterForQueueItemArrived(this.securityShim);
                this.securityShim = null;
            }
            if (this.validationShim != null)
            {
                workflowQueue.UnregisterForQueueItemAvailable(this.validationShim);
            }

            if (this.QueueInitializationMode == QueueInitializationMode.EventDriven)
            {
                workflowQueue.Enabled = false;
            }

            return;
        }

        [SuppressMessage("Microsoft.Security", "CA2103")] // Review imperative security, because constructing PrincipalPermission
        void IServiceDescriptionBuilder.BuildServiceDescription(ServiceDescriptionContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }

            if (!this.Enabled)
            {
                return;
            }

            OperationInfoBase serviceOperationInfo = this.ServiceOperationInfo;
            if (serviceOperationInfo == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new InvalidOperationException(SR2.GetString(SR2.Error_ServiceOperationInfoNotSpecified, this.Name)));
            }

            // set the workflow service behavior
            //
            IServiceDescriptionBuilder serviceAttributes =
                GetWorkflowServiceAttributes(this.RootActivity) as IServiceDescriptionBuilder;

            if (serviceAttributes != null)
            {
                serviceAttributes.BuildServiceDescription(context);
            }

            if (context.ReflectedContracts == null || context.WorkflowOperationBehaviors == null)
            {
                return;
            }

            // add contract types and configure the operation behavior
            //
            Type contractType = serviceOperationInfo.GetContractType(((IComponent)this).Site);

            List<Type> interfaces = ServiceOperationHelpers.GetContracts(contractType);
            for (int i = 0; i < interfaces.Count; i++)
            {
                Type interfaceType = interfaces[i];
                ContractDescription contractDescription = null;

                if (!context.ReflectedContracts.Contains(interfaceType))
                {
                    contractDescription = ContractDescription.GetContract(interfaceType);
                    ServiceOperationHelpers.SetWorkflowOperationBehavior(contractDescription, context);

                    context.Contracts.Add(contractDescription.ConfigurationName, contractDescription);
                    context.ReflectedContracts.Add(contractDescription.ContractType);
                }
                else
                {
                    contractDescription = context.Contracts[ContractDescription.GetContract(interfaceType).ConfigurationName];
                }

                Collection<ContractDescription> inheritedContractDescriptions = contractDescription.GetInheritedContracts();
                for (int j = 0; j < inheritedContractDescriptions.Count; j++)
                {
                    ContractDescription inheritedContractDescription = inheritedContractDescriptions[j];
                    if (!context.ReflectedContracts.Contains(inheritedContractDescription.ContractType))
                    {
                        ServiceOperationHelpers.SetWorkflowOperationBehavior(inheritedContractDescription, context);

                        context.Contracts.Add(inheritedContractDescription.ConfigurationName, inheritedContractDescription);
                        context.ReflectedContracts.Add(inheritedContractDescription.ContractType);
                    }
                }
            }

            Type operationDeclaringType = null;
            MethodInfo methodInfo = serviceOperationInfo.GetMethodInfo(((IComponent)this).Site);
            if (methodInfo != null)
            {
                operationDeclaringType = methodInfo.DeclaringType;
            }

            if (operationDeclaringType != null)
            {
                WorkflowOperationBehavior behavior = null;
                KeyValuePair<Type, string> operationKey =
                    new KeyValuePair<Type, string>(operationDeclaringType, serviceOperationInfo.Name);

                if (context.WorkflowOperationBehaviors.TryGetValue(operationKey, out behavior) && behavior != null)
                {
                    if (!behavior.CanCreateInstance && this.CanCreateInstance)
                    {
                        behavior.CanCreateInstance = true;
                    }

                    if (!string.IsNullOrEmpty(serviceOperationInfo.PrincipalPermissionRole)
                        || !string.IsNullOrEmpty(serviceOperationInfo.PrincipalPermissionName))
                    {
                        if (behavior.ServiceAuthorizationManager == null)
                        {
                            PrincipalPermission permission =
                                new PrincipalPermission(serviceOperationInfo.PrincipalPermissionName,
                                serviceOperationInfo.PrincipalPermissionRole, true);

                            PrincipalPermissionServiceAuthorizationManager authManager
                                = new PrincipalPermissionServiceAuthorizationManager(permission);

                            behavior.ServiceAuthorizationManager = authManager;
                        }
                    }
                }
            }
        }

        protected internal override void Initialize(IServiceProvider provider)
        {
            if (provider == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("provider");
            }

            ContextToken.Register(this, this.WorkflowInstanceId);

            SetQueueInitializationMode();

            // make sure that we are getting the proper queue
            // even if that means creating a new queue
            // given our conversation context and execution context.
            //
            WorkflowQueue workflowQueue = GetWorkflowQueue(provider);
            if (workflowQueue == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new InvalidOperationException(SR2.GetString(SR2.Error_QueueNotFound,
                    ((IEventActivity)this).QueueName)));
            }

            if (this.QueueInitializationMode == QueueInitializationMode.StateMachine)
            {
                workflowQueue.Enabled = true;
            }

            base.Initialize(provider);
        }

        internal void GetParameterPropertyDescriptors(IDictionary properties)
        {
            if (properties == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("properties");
            }

            if (((IComponent)this).Site == null)
            {
                return;
            }

            OperationInfoBase serviceOperationInfo = this.ServiceOperationInfo;
            if (serviceOperationInfo != null)
            {
                MethodInfo methodInfo = serviceOperationInfo.GetMethodInfo(((IComponent)this).Site);
                if (methodInfo != null)
                {
                    ArrayList paramInfo = new ArrayList(methodInfo.GetParameters());
                    if (!(methodInfo.ReturnType == typeof(void)))
                    {
                        paramInfo.Add(methodInfo.ReturnParameter);
                    }

                    foreach (ParameterInfo param in paramInfo)
                    {
                        if (param.ParameterType != null)
                        {
                            PropertyDescriptor prop =
                                new ParameterInfoBasedPropertyDescriptor(typeof(ReceiveActivity),
                                param, true, DesignOnlyAttribute.Yes);

                            properties[prop.Name] = prop;
                        }
                    }
                }
            }
        }

        protected override ActivityExecutionStatus Cancel(ActivityExecutionContext executionContext)
        {
            try
            {
                if (executionContext == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("executionContext");
                }

                WorkflowQueuingService queuingService = executionContext.GetService<WorkflowQueuingService>();
                if (queuingService == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new InvalidOperationException(SR2.GetString(SR2.General_MissingService,
                        typeof(WorkflowQueuingService))));
                }

                WorkflowQueue workflowQueue = GetWorkflowQueue(executionContext);
                if (workflowQueue == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new InvalidOperationException(SR2.GetString(SR2.Error_QueueNotFound,
                        ((IEventActivity)this).QueueName)));
                }

                workflowQueue.UnregisterForQueueItemAvailable(this);

                if (this.QueueInitializationMode == QueueInitializationMode.Standalone)
                {
                    if (this.securityShim != null)
                    {
                        workflowQueue.UnregisterForQueueItemArrived(this.securityShim);
                        this.securityShim = null;
                    }
                    workflowQueue.Enabled = false;
                }
            }
            finally
            {
                RemoveRequestContext();
            }

            return base.Cancel(executionContext);
        }

        protected override ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {
            if (executionContext == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("executionContext");
            }

            WorkflowQueuingService queuingService = executionContext.GetService<WorkflowQueuingService>();
            if (queuingService == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new InvalidOperationException(SR2.GetString(SR2.General_MissingService,
                    typeof(WorkflowQueuingService))));
            }

            // make sure that we are getting the proper queue
            // even if that means creating a new queue
            // given our conversation context and execution context.
            //
            WorkflowQueue workflowQueue = GetWorkflowQueue(executionContext);
            if (workflowQueue == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new InvalidOperationException(SR2.GetString(SR2.Error_QueueNotFound,
                    ((IEventActivity)this).QueueName)));
            }

            if (this.QueueInitializationMode == QueueInitializationMode.Standalone && workflowQueue.Count == 0)
            {
                workflowQueue.Enabled = true;

                if (this.securityShim == null)
                {
                    this.securityShim = new SecurityShim(this);
                }
                workflowQueue.RegisterForQueueItemArrived(this.securityShim);
                workflowQueue.RegisterForQueueItemAvailable(this, this.QualifiedName);
                return ActivityExecutionStatus.Executing;
            }
            else if (workflowQueue.Count == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new InvalidOperationException(SR2.GetString(SR2.Error_RequestContextUnavailable,
                    this.QualifiedName)));
            }

            WorkflowRequestContext requestContext = workflowQueue.Dequeue() as WorkflowRequestContext;

            if (requestContext == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new InvalidOperationException(SR2.GetString(SR2.Error_RequestContextUnavailable,
                    this.QualifiedName)));
            }
            else
            {
                if (this.validationShim != null)
                {
                    this.RequestContext = requestContext;
                    CacheRequestContext(requestContext);
                }
                else if (ValidationShim.EvaluateSecurityConstraints(executionContext, this, requestContext) == true)
                {
                    this.RequestContext = requestContext;
                    CacheRequestContext(requestContext);

                    if (this.QueueInitializationMode == QueueInitializationMode.Standalone)
                    {
                        workflowQueue.Enabled = false;
                    }
                }
                else
                {
                    try
                    {
                        System.Workflow.Runtime.WorkflowTrace.Runtime.TraceEvent(TraceEventType.Verbose, 0,
                            "Workflow Instance {0}, receive activity {1} - message validation failed. Message will be discarded.",
                            this.WorkflowInstanceId, this.QualifiedName);

                        requestContext.SendFault(new FaultException(SR2.GetString(SR2.SecurityCheckFailed)), null);
                    }
                    catch (CommunicationException cex)
                    {
                        System.Workflow.Runtime.WorkflowTrace.Runtime.TraceEvent(TraceEventType.Error, 0,
                            "Workflow Instance {0}, receive activity {1} - failed to send fault for rejected message. Error: {2}",
                            this.WorkflowInstanceId, this.QualifiedName, cex);
                    }
                    catch (TimeoutException tex)
                    {
                        System.Workflow.Runtime.WorkflowTrace.Runtime.TraceEvent(TraceEventType.Error, 0,
                            "Workflow Instance {0}, receive activity {1} - failed to send fault for rejected message. Error: {2}",
                            this.WorkflowInstanceId, this.QualifiedName, tex);
                    }

                    if (requestContext.ContextProperties == null ||
                        !(requestContext.ContextProperties.Keys.Contains(WellKnownContextProperties.InstanceId)))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                            new FaultException(SR2.GetString(SR2.Error_FailedToValidateActivatingMessage, this.WorkflowInstanceId)));
                    }

                    if (this.QueueInitializationMode == QueueInitializationMode.Standalone)
                    {
                        if (this.securityShim == null)
                        {
                            this.securityShim = new SecurityShim(this);
                        }
                        workflowQueue.RegisterForQueueItemArrived(this.securityShim);
                    }

                    workflowQueue.RegisterForQueueItemAvailable(this, this.QualifiedName);
                    return ActivityExecutionStatus.Executing;
                }
            }

            return ExecuteActivity(this.RequestContext, executionContext);
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
            Justification = "Supress any exceptions thrown by SendFault to avoid calling HandleFault infinitely.")]
        protected override ActivityExecutionStatus HandleFault(ActivityExecutionContext executionContext,
            Exception exception)
        {
            try
            {
                if (executionContext == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("executionContext");
                }

                if (exception == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("exception");
                }

                if (executionContext == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("executionContext");
                }

                WorkflowQueuingService queuingService = executionContext.GetService<WorkflowQueuingService>();
                if (queuingService == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new InvalidOperationException(SR2.GetString(SR2.General_MissingService,
                        typeof(WorkflowQueuingService))));
                }

                WorkflowQueue workflowQueue = GetWorkflowQueue(executionContext);
                if (workflowQueue == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new InvalidOperationException(SR2.GetString(SR2.Error_QueueNotFound,
                        ((IEventActivity)this).QueueName)));
                }

                workflowQueue.UnregisterForQueueItemAvailable(this);

                if (this.QueueInitializationMode == QueueInitializationMode.Standalone)
                {
                    if (this.securityShim != null)
                    {
                        workflowQueue.UnregisterForQueueItemArrived(this.securityShim);
                        this.securityShim = null;
                    }
                    workflowQueue.Enabled = false;
                }

                RestoreRequestContext();

                if (this.RequestContext != null)
                {
                    System.Workflow.Runtime.WorkflowTrace.Runtime.TraceEvent(TraceEventType.Verbose, 0,
                         "Workflow Instance {0}, receive activity {1} - sending fault response message",
                         this.WorkflowInstanceId, this.QualifiedName);

                    if (this.FaultMessage != null)
                    {
                        try
                        {
                            this.RequestContext.SendFault(this.FaultMessage, null);
                        }
                        catch
                        {
                            System.Workflow.Runtime.WorkflowTrace.Runtime.TraceEvent(TraceEventType.Error, 0,
                                "Workflow Instance {0}, receive activity {1} - failed to send response fault message.",
                                this.WorkflowInstanceId, this.QualifiedName);
                        }
                    }
                    else
                    {
                        try
                        {
                            this.RequestContext.SendFault(exception, null);
                        }
                        catch
                        {
                            System.Workflow.Runtime.WorkflowTrace.Runtime.TraceEvent(TraceEventType.Error, 0,
                                "Workflow Instance {0}, receive activity {1} - failed to send response fault message.",
                                this.WorkflowInstanceId, this.QualifiedName);
                        }
                    }
                }
            }
            finally
            {
                RemoveRequestContext();
            }

            return base.HandleFault(executionContext, exception);
        }

        protected override void InitializeProperties()
        {
            OperationInfoBase serviceOperationInfo = this.ServiceOperationInfo;

            if (serviceOperationInfo == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new InvalidOperationException(SR2.GetString(SR2.Error_ServiceOperationInfoNotSpecified, this.Name)));
            }

            OperationParameterInfoCollection parameters = null;

            Activity definitionRoot = base.RootActivity.GetValue(Activity.WorkflowDefinitionProperty) as Activity;
            if (definitionRoot != null)
            {
                ReceiveActivity definition = definitionRoot.GetActivityByName(this.QualifiedName, true) as ReceiveActivity;
                if ((definition != null) && definition.UserData.Contains(typeof(OperationParameterInfoCollection)))
                {
                    parameters = definition.UserData[typeof(OperationParameterInfoCollection)] as OperationParameterInfoCollection;
                }
            }

            if (parameters == null)
            {
                parameters = serviceOperationInfo.GetParameters(this.Site);
                this.UserData[typeof(OperationParameterInfoCollection)] = parameters;
            }

            WorkflowParameterBindingCollection bindings = this.ParameterBindings;

            foreach (OperationParameterInfo parameter in parameters)
            {
                if (!bindings.Contains(parameter.Name))
                {
                    bindings.Add(new WorkflowParameterBinding(parameter.Name));
                }
            }

            base.InitializeProperties();
        }

        protected override void OnSequenceComplete(ActivityExecutionContext executionContext)
        {
            if (executionContext == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("executionContext");
            }

            try
            {
                RestoreRequestContext();
                if (this.RequestContext == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new InvalidOperationException(SR2.GetString(SR2.Error_RequestContextUnavailable,
                        this.QualifiedName)));
                }

                object returnValue;
                object[] outputValues;

                if (this.FaultMessage != null)
                {
                    System.Workflow.Runtime.WorkflowTrace.Runtime.TraceEvent(TraceEventType.Verbose, 0,
                        "Workflow Instance {0}, receive activity {1} - sending fault response message",
                        this.WorkflowInstanceId, this.QualifiedName);

                    try
                    {
                        this.RequestContext.SendFault(this.FaultMessage, null);
                    }
                    catch (CommunicationException cex)
                    {
                        System.Workflow.Runtime.WorkflowTrace.Runtime.TraceEvent(TraceEventType.Error, 0,
                            "Workflow Instance {0}, receive activity {1} - failed to send fault response message. Error: {2}",
                            this.WorkflowInstanceId, this.QualifiedName, cex);
                        throw;
                    }
                    catch (TimeoutException tex)
                    {
                        System.Workflow.Runtime.WorkflowTrace.Runtime.TraceEvent(TraceEventType.Error, 0,
                            "Workflow Instance {0}, receive activity {1} - failed to send fault response message. Error: {2}",
                            this.WorkflowInstanceId, this.QualifiedName, tex);
                        throw;
                    }
                }
                else if (!this.OperationHelper.IsOneWay)
                {
                    returnValue = this.OperationHelper.GetOutputs(this, out outputValues);

                    System.Workflow.Runtime.WorkflowTrace.Runtime.TraceEvent(TraceEventType.Verbose, 0,
                        "Workflow Instance {0}, receive activity {1} - sending response message",
                        this.WorkflowInstanceId, this.QualifiedName);

                    try
                    {
                        this.RequestContext.SendReply(returnValue, outputValues, null);
                    }
                    catch (CommunicationException cex)
                    {
                        System.Workflow.Runtime.WorkflowTrace.Runtime.TraceEvent(TraceEventType.Error, 0,
                            "Workflow Instance {0}, receive activity {1} - failed to send response message. Error: {2}",
                            this.WorkflowInstanceId, this.QualifiedName, cex);
                        throw;
                    }
                    catch (TimeoutException tex)
                    {
                        System.Workflow.Runtime.WorkflowTrace.Runtime.TraceEvent(TraceEventType.Error, 0,
                            "Workflow Instance {0}, receive activity {1} - failed to send response message. Error: {2}",
                            this.WorkflowInstanceId, this.QualifiedName, tex);
                        throw;
                    }
                }
                else
                {
                    System.Workflow.Runtime.WorkflowTrace.Runtime.TraceEvent(TraceEventType.Verbose, 0,
                        "Workflow Instance {0}, receive activity {1} - completing one way operation",
                        this.WorkflowInstanceId, this.QualifiedName);

                    this.RequestContext.SetOperationCompleted();
                }

                base.OnSequenceComplete(executionContext);

                // Null out the request context to reduce serialization size of the activity.
                this.RequestContext = null;
            }
            finally
            {
                RemoveRequestContext();
            }
        }

        static object GetWorkflowServiceAttributesValueOverride(object dependencyObject)
        {
            if (dependencyObject == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("dependencyObject");
            }

            if (!(dependencyObject is Activity))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(
                    "dependencyObject",
                    SR2.GetString(SR2.Error_UnexpectedArgumentType, typeof(Activity).FullName));
            }

            Activity activity = dependencyObject as Activity;

            if (activity.GetValueBase(ReceiveActivity.WorkflowServiceAttributesProperty) == null)
            {
                if (activity.DesignMode)
                {
                    WorkflowServiceAttributes workflowServiceAttribsValue = new WorkflowServiceAttributes();
                    Activity rootActivity = activity.RootActivity;
                    if (rootActivity != null)
                    {
                        string fullClassName = (String)rootActivity.GetValue(WorkflowMarkupSerializer.XClassProperty);
                        if (!String.IsNullOrEmpty(fullClassName))
                        {
                            string namespaceName;
                            string className;
                            Helpers.GetNamespaceAndClassName(fullClassName, out namespaceName, out className);
                            workflowServiceAttribsValue.ConfigurationName = fullClassName;
                            workflowServiceAttribsValue.Name = className;
                        }
                    }
                    activity.SetValue(ReceiveActivity.WorkflowServiceAttributesProperty, workflowServiceAttribsValue);
                    return workflowServiceAttribsValue;
                }
            }
            return activity.GetValueBase(ReceiveActivity.WorkflowServiceAttributesProperty);
        }

        private ActivityExecutionStatus ExecuteActivity(WorkflowRequestContext requestContext,
            ActivityExecutionContext executionContext)
        {
            if (requestContext == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("requestContext");
            }

            if (executionContext == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("executionContext");
            }

            System.Workflow.Runtime.WorkflowTrace.Runtime.TraceEvent(TraceEventType.Verbose, 0,
                "Workflow Instance {0}, receive activity {1} - received message",
                this.WorkflowInstanceId, this.QualifiedName);

            this.OperationHelper.PopulateInputs(this, requestContext.Inputs);

            return base.Execute(executionContext);
        }

        private WorkflowQueue GetWorkflowQueue(IServiceProvider provider)
        {
            if (provider == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("provider");
            }

            if (this.ServiceOperationInfo == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new InvalidOperationException(SR2.GetString(SR2.Error_ServiceOperationInfoNotSpecified, this.Name)));
            }

            WorkflowQueuingService queuingService =
                provider.GetService(typeof(WorkflowQueuingService)) as WorkflowQueuingService;

            if (queuingService == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new InvalidOperationException(SR2.GetString(SR2.General_MissingService,
                    typeof(WorkflowQueuingService))));
            }

            WorkflowQueue workflowQueue = null;
            string queueName = this.OperationHelper.GetWorkflowQueueName(this.Context);
            this.SetValue(QueueNameProperty, queueName);

            if (!queuingService.Exists(queueName))
            {
                workflowQueue = queuingService.CreateWorkflowQueue(queueName, false);
                workflowQueue.Enabled = false;
            }
            else
            {
                workflowQueue = queuingService.GetWorkflowQueue(queueName);
            }

            return workflowQueue;
        }

        private void SetQueueInitializationMode()
        {
            if (this.parent != null && this.parent is EventDrivenActivity)
            {
                if (this.parent.parent != null && this.parent.parent is StateActivity)
                {
                    this.QueueInitializationMode = QueueInitializationMode.StateMachine;
                }
            }
        }

        private void CacheRequestContext(WorkflowRequestContext requestContext)
        {
            string keyValue = WorkflowEnvironment.WorkflowInstanceId.ToString() + ":" +
                this.GetValue(ReceiveActivity.QueueNameProperty) as string;
            requestContextsCache[keyValue] = requestContext;
            this.isContextCached = true;
        }

        private void RestoreRequestContext()
        {
            string keyValue = WorkflowEnvironment.WorkflowInstanceId.ToString() + ":"
                + this.GetValue(ReceiveActivity.QueueNameProperty) as string;
            if (requestContextsCache.ContainsKey(keyValue))
            {
                this.RequestContext = requestContextsCache[keyValue] as WorkflowRequestContext;
            }
        }

        private void RemoveRequestContext()
        {
            requestContextsCache.Remove(WorkflowEnvironment.WorkflowInstanceId.ToString() + ":" +
                this.GetValue(ReceiveActivity.QueueNameProperty) as string);
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing && this.isContextCached)
                {
                    RemoveRequestContext();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        [Serializable]
        class ReceiveOperationInfoHelper
        {
            string baseQueueName;
            bool hasReturnValue = false;
            IList<KeyValuePair<int, string>> inputParameters;
            bool isOneWay = false;
            IDictionary<int, Type> notNullableParameters;
            bool nullableReturnValue = true;
            string operationName;
            IList<KeyValuePair<int, string>> outputParameters;
            string returnTypeName;

            public ReceiveOperationInfoHelper(IServiceProvider serviceProvider, ReceiveActivity activity)
            {
                outputParameters = new List<KeyValuePair<int, string>>();
                inputParameters = new List<KeyValuePair<int, string>>();
                notNullableParameters = new Dictionary<int, Type>();
                hasReturnValue = false;
                nullableReturnValue = true;

                if (activity == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("activity");
                }

                OperationInfoBase serviceOperationInfo = activity.ServiceOperationInfo;
                if (serviceOperationInfo == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new InvalidOperationException(SR2.GetString(SR2.Error_ServiceOperationInfoNotSpecified, activity.Name)));
                }

                MethodInfo methodInfo = serviceOperationInfo.GetMethodInfo(serviceProvider);
                if (methodInfo == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new InvalidOperationException(SR2.GetString(SR2.Error_MethodInfoNotAvailable, activity.Name)));
                }

                if (methodInfo.ReturnType != null && methodInfo.ReturnType != typeof(void))
                {
                    hasReturnValue = true;
                    this.returnTypeName = methodInfo.ReturnType.FullName;
                    nullableReturnValue =
                        !((methodInfo.ReturnType.IsPrimitive || methodInfo.ReturnType.IsEnum || methodInfo.ReturnType.IsValueType) && !ServiceOperationHelpers.IsNullableType(methodInfo.ReturnType));
                }

                foreach (ParameterInfo parameter in methodInfo.GetParameters())
                {
                    if (parameter.ParameterType.IsByRef ||
                        parameter.IsOut || (parameter.IsIn && parameter.IsOut))
                    {
                        outputParameters.Add(new KeyValuePair<int, string>(parameter.Position, parameter.Name));

                        if (parameter.ParameterType.IsByRef &&
                            parameter.ParameterType.GetElementType().IsValueType &&
                            !ServiceOperationHelpers.IsNullableType(parameter.ParameterType))
                        {
                            notNullableParameters.Add(parameter.Position, parameter.ParameterType);
                        }
                    }

                    if (!parameter.IsOut || (parameter.IsIn && parameter.IsOut))
                    {
                        inputParameters.Add(new KeyValuePair<int, string>(parameter.Position, parameter.Name));
                    }
                }

                this.operationName = serviceOperationInfo.Name;

                this.baseQueueName = QueueNameHelper.Create(methodInfo.DeclaringType, this.operationName);

                object[] operationContractAttribs = methodInfo.GetCustomAttributes(typeof(OperationContractAttribute), true);

                if (operationContractAttribs != null && operationContractAttribs.Length > 0)
                {
                    if (operationContractAttribs[0] is OperationContractAttribute)
                    {
                        this.isOneWay = ((OperationContractAttribute)operationContractAttribs[0]).IsOneWay;
                    }
                }

            }

            public bool IsOneWay
            {
                get
                {
                    return this.isOneWay;
                }
            }

            public object GetOutputs(ReceiveActivity activity, out object[] outputs)
            {
                if (activity == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("activity");
                }

                outputs = new object[outputParameters.Count];
                object returnValue = null;

                WorkflowParameterBindingCollection bindings = activity.ParameterBindings;

                for (int index = 0; index < outputParameters.Count; ++index)
                {
                    KeyValuePair<int, string> parameterInfo = outputParameters[index];
                    if (bindings[parameterInfo.Value].Value == null &&
                        this.notNullableParameters.Keys.Contains(parameterInfo.Key))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                            new InvalidOperationException(SR2.GetString(SR2.Error_ReceiveActivityInvalidParameterValue,
                            activity.Name, parameterInfo.Value, this.notNullableParameters[parameterInfo.Key])));
                    }

                    outputs[index] = bindings[parameterInfo.Value].Value;
                }

                if (hasReturnValue)
                {
                    if (bindings["(ReturnValue)"].Value == null &&
                        !this.nullableReturnValue)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                            new InvalidOperationException(SR2.GetString(SR2.Error_ReceiveActivityInvalidReturnValue,
                            activity.Name, this.returnTypeName)));
                    }

                    returnValue = bindings["(ReturnValue)"].Value;
                }

                return returnValue;
            }

            public string GetWorkflowQueueName(IDictionary<string, string> context)
            {
                return QueueNameHelper.Create(this.baseQueueName, context);
            }

            public void PopulateInputs(ReceiveActivity activity, ReadOnlyCollection<object> inputs)
            {
                if (activity == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("activity");
                }

                if (inputs == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("inputs");
                }

                WorkflowParameterBindingCollection bindings = activity.ParameterBindings;

                for (int index = 0; index < inputParameters.Count; index++)
                {
                    KeyValuePair<int, string> parameterInfo = inputParameters[index];

                    if (!bindings.Contains(parameterInfo.Value))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                            new InvalidOperationException(SR2.GetString(SR2.Error_ParameterBindingMissing,
                            parameterInfo.Value,
                            this.operationName,
                            activity.Name)));
                    }
                    if (index >= inputs.Count)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                            new InvalidOperationException(SR2.GetString(SR2.Error_InputValueUnavailable,
                            parameterInfo.Value,
                            this.operationName,
                            activity.Name)));
                    }

                    WorkflowParameterBinding parameterBinding = bindings[parameterInfo.Value];
                    parameterBinding.Value = inputs[index];
                }
            }
        }

        [Serializable]
        class SecurityShim : IActivityEventListener<QueueEventArgs>, IDisposable
        {
            ReceiveActivity receiveActivity;

            internal SecurityShim(ReceiveActivity receiveActivity)
            {
                if (receiveActivity == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("receiveActivity");
                }

                this.receiveActivity = receiveActivity;
            }

            void IActivityEventListener<QueueEventArgs>.OnEvent(object sender, QueueEventArgs queueEventArgs)
            {
                if (sender == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("sender");
                }

                if (queueEventArgs == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("queueEventArgs");
                }

                WorkflowQueue workflowQueue = sender as WorkflowQueue;
                if (workflowQueue == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new ArgumentException(SR2.GetString(SR2.Error_ArgumentTypeInvalid, "sender", typeof(WorkflowQueue))));
                }

                WorkflowRequestContext requestContext = workflowQueue.Peek() as WorkflowRequestContext;
                if (requestContext != null)
                {
                    IDependencyObjectAccessor doa = (IDependencyObjectAccessor)receiveActivity;
                    EventHandler<OperationValidationEventArgs>[] eventHandlers =
                        doa.GetInvocationList<EventHandler<OperationValidationEventArgs>>(
                        ReceiveActivity.OperationValidationEvent);

                    if (eventHandlers != null && eventHandlers.Length > 0)
                    {
                        requestContext.PopulateAuthorizationState();
                    }
                }
            }

            void IDisposable.Dispose()
            {
                this.receiveActivity.Dispose();
                GC.SuppressFinalize(this);
            }
        }

        [Serializable]
        class ValidationShim : IActivityEventListener<QueueEventArgs>, IDisposable
        {
            IActivityEventListener<QueueEventArgs> activityEventListener;
            ReceiveActivity receiveActivity;

            internal ValidationShim(ReceiveActivity receiveActivity, IActivityEventListener<QueueEventArgs> activityEventListener)
            {
                if (receiveActivity == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("receiveActivity");
                }

                if (activityEventListener == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("activityEventListener");
                }

                this.receiveActivity = receiveActivity;
                this.activityEventListener = activityEventListener;
            }

            void IActivityEventListener<QueueEventArgs>.OnEvent(object sender, QueueEventArgs queueEventArgs)
            {
                if (sender == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("sender");
                }

                if (queueEventArgs == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("queueEventArgs");
                }

                ActivityExecutionContext executionContext = sender as ActivityExecutionContext;
                if (executionContext == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new ArgumentException(SR2.GetString(SR2.Error_ArgumentTypeInvalid, "sender", typeof(ActivityExecutionContext))));
                }

                WorkflowQueuingService queuingService = executionContext.GetService<WorkflowQueuingService>();
                if (queuingService == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new InvalidOperationException(SR2.GetString(SR2.General_MissingService, typeof(WorkflowQueuingService))));
                }

                WorkflowQueue workflowQueue = queuingService.GetWorkflowQueue(queueEventArgs.QueueName);
                if (workflowQueue == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new InvalidOperationException(SR2.GetString(SR2.Error_QueueNotFound, queueEventArgs.QueueName)));
                }

                WorkflowRequestContext requestContext = workflowQueue.Peek() as WorkflowRequestContext;
                if (requestContext != null)
                {
                    if (EvaluateSecurityConstraints(executionContext, this.receiveActivity, requestContext) == true)
                    {
                        this.activityEventListener.OnEvent(sender, queueEventArgs);
                    }
                    else
                    {
                        System.Workflow.Runtime.WorkflowTrace.Runtime.TraceEvent(TraceEventType.Verbose, 0,
                            "Workflow Instance {0}, receive activity {1} - message validation failed. Message will be discarded.",
                            this.receiveActivity.WorkflowInstanceId, this.receiveActivity.QualifiedName);

                        workflowQueue.Dequeue();
                        try
                        {
                            requestContext.SendFault(new FaultException(SR2.GetString(SR2.SecurityCheckFailed)), null);
                        }
                        catch (CommunicationException cex)
                        {
                            System.Workflow.Runtime.WorkflowTrace.Runtime.TraceEvent(TraceEventType.Error, 0,
                                "Workflow Instance {0}, receive activity {1} - failed to send fault for rejected message. Error: {2}",
                                this.receiveActivity.WorkflowInstanceId, this.receiveActivity.QualifiedName, cex);
                        }
                        catch (TimeoutException tex)
                        {
                            System.Workflow.Runtime.WorkflowTrace.Runtime.TraceEvent(TraceEventType.Error, 0,
                                "Workflow Instance {0}, receive activity {1} - failed to send fault for rejected message. Error: {2}",
                                this.receiveActivity.WorkflowInstanceId, this.receiveActivity.QualifiedName, tex);
                        }

                        if (requestContext.ContextProperties == null ||
                            !(requestContext.ContextProperties.Keys.Contains(WellKnownContextProperties.InstanceId)))
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                                new FaultException(SR2.GetString(SR2.Error_FailedToValidateActivatingMessage, this.receiveActivity.WorkflowInstanceId)));
                        }
                    }
                }
                else
                {
                    this.activityEventListener.OnEvent(sender, queueEventArgs);
                }
            }

            void IDisposable.Dispose()
            {
                this.receiveActivity.Dispose();
                GC.SuppressFinalize(this);
            }

            internal static bool EvaluateSecurityConstraints(IServiceProvider serviceProvider, ReceiveActivity receiveActivity, WorkflowRequestContext requestContext)
            {
                bool retVal = true;

                if (serviceProvider == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("serviceProvider");
                }

                if (receiveActivity == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("receiveActivity");
                }

                if (requestContext == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("requestContext");
                }

                IDependencyObjectAccessor doa = (IDependencyObjectAccessor)receiveActivity;
                EventHandler<OperationValidationEventArgs>[] eventHandlers =
                    doa.GetInvocationList<EventHandler<OperationValidationEventArgs>>(
                    ReceiveActivity.OperationValidationEvent);

                if (eventHandlers != null && eventHandlers.Length > 0)
                {
                    ReadOnlyCollection<ClaimSet> claims = requestContext.AuthorizationContext == null ?
                        new ReadOnlyCollection<ClaimSet>(new List<ClaimSet>()) :
                        requestContext.AuthorizationContext.ClaimSets;
                    OperationValidationEventArgs e = new OperationValidationEventArgs(claims);
                    receiveActivity.RaiseGenericEvent(ReceiveActivity.OperationValidationEvent,
                        receiveActivity, e);
                    retVal = e.IsValid;
                }

                return retVal;
            }
        }
    }
}
