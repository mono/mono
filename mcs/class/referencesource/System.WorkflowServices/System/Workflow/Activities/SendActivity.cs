//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

#pragma warning disable 1634, 1691
namespace System.Workflow.Activities
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.ComponentModel.Design;
    using System.Workflow.Runtime.Hosting;

    [SR2Description(SR2DescriptionAttribute.SendActivityDescription)]
    [SR2Category(SR2CategoryAttribute.Standard)]
    [DesignerAttribute(typeof(SendActivityDesigner), typeof(IDesigner))]
    [ToolboxBitmap(typeof(SendActivity), "Design.Resources.SendActivity.png")]
    [ActivityValidator(typeof(SendActivityValidator))]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class SendActivity : Activity
    {
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly DependencyProperty AfterResponseEvent =
            DependencyProperty.Register("AfterResponse",
            typeof(EventHandler<SendActivityEventArgs>),
            typeof(SendActivity));

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly DependencyProperty BeforeSendEvent =
            DependencyProperty.Register("BeforeSend",
            typeof(EventHandler<SendActivityEventArgs>),
            typeof(SendActivity));

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly DependencyProperty CustomAddressProperty =
            DependencyProperty.Register("CustomAddress",
            typeof(string),
            typeof(SendActivity),
            new PropertyMetadata(null));

        public const string ReturnValuePropertyName = "(ReturnValue)";

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        internal static readonly DependencyProperty ChannelTokenProperty =
            DependencyProperty.Register("ChannelToken",
            typeof(ChannelToken),
            typeof(SendActivity),
            new PropertyMetadata(null, DependencyPropertyOptions.Metadata));

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        internal static readonly DependencyProperty ParameterBindingsProperty =
            DependencyProperty.Register("ParameterBindings",
            typeof(WorkflowParameterBindingCollection),
            typeof(SendActivity),
            new PropertyMetadata(DependencyPropertyOptions.Metadata | DependencyPropertyOptions.ReadOnly));

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        internal static readonly DependencyProperty ServiceOperationInfoProperty =
            DependencyProperty.Register("ServiceOperationInfo",
            typeof(TypedOperationInfo),
            typeof(SendActivity),
            new PropertyMetadata(DependencyPropertyOptions.Metadata));

        [NonSerialized]
        private SendOperationInfoHelper operationHelper;

        public SendActivity()
        {
            base.SetReadOnlyPropertyValue(SendActivity.ParameterBindingsProperty,
                new WorkflowParameterBindingCollection(this));
        }

        public SendActivity(String name)
            : base(name)
        {
            base.SetReadOnlyPropertyValue(SendActivity.ParameterBindingsProperty,
                new WorkflowParameterBindingCollection(this));
        }

        [SuppressMessage("Microsoft.Naming", "CA1713:EventsShouldNotHaveBeforeOrAfterPrefix")]
        [Browsable(true)]
        [SR2Category(SR2CategoryAttribute.Handlers)]
        [SR2Description(SR2DescriptionAttribute.Send_AfterResponse_Description)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public event EventHandler<SendActivityEventArgs> AfterResponse
        {
            add
            {
                base.AddHandler(SendActivity.AfterResponseEvent, value);
            }
            remove
            {
                base.RemoveHandler(SendActivity.AfterResponseEvent, value);
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA1713:EventsShouldNotHaveBeforeOrAfterPrefix")]
        [Browsable(true)]
        [SR2Category(SR2CategoryAttribute.Handlers)]
        [SR2Description(SR2DescriptionAttribute.Send_BeforeSend_Description)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public event EventHandler<SendActivityEventArgs> BeforeSend
        {
            add
            {
                base.AddHandler(SendActivity.BeforeSendEvent, value);
            }
            remove
            {
                base.RemoveHandler(SendActivity.BeforeSendEvent, value);
            }
        }

        [DefaultValue(null)]
        [MergableProperty(false)]
        [RefreshProperties(RefreshProperties.All)]
        [SR2Category(SR2CategoryAttribute.Activity)]
        [SR2Description(SR2DescriptionAttribute.Send_ChannelToken_Description)]
        public ChannelToken ChannelToken
        {
            get
            {
                return (ChannelToken) this.GetValue(SendActivity.ChannelTokenProperty);
            }
            set
            {
                this.SetValue(SendActivity.ChannelTokenProperty, value);
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IDictionary<string, string> Context
        {
            get
            {
                if (this.ServiceOperationInfo == null)
                {
#pragma warning suppress 56503
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new InvalidOperationException(SR2.GetString(SR2.Error_ServiceOperationInfoNotSpecified, this.Name)));
                }

                return SendActivity.GetContext(this, this.ChannelToken, this.ServiceOperationInfo.ContractType);
            }
            set
            {
                if (this.ServiceOperationInfo == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new InvalidOperationException(SR2.GetString(SR2.Error_ServiceOperationInfoNotSpecified, this.Name)));
                }

                SendActivity.SetContext(this, this.ChannelToken, this.ServiceOperationInfo.ContractType, value);
            }
        }

        [DefaultValue(null)]
        [SR2Category(SR2CategoryAttribute.Activity)]
        [SR2Description(SR2DescriptionAttribute.Send_CustomAddress_Description)]
        public string CustomAddress
        {
            get
            {
                return (string) GetValue(CustomAddressProperty);
            }

            set
            {
                SetValue(CustomAddressProperty, value);
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public WorkflowParameterBindingCollection ParameterBindings
        {
            get
            {
                return ((WorkflowParameterBindingCollection)(base.GetValue(SendActivity.ParameterBindingsProperty)));
            }
        }

        [Browsable(true)]
        [SR2Category(SR2CategoryAttribute.Activity)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [SR2Description(SR2DescriptionAttribute.Send_OperationInfo_Description)]
        public TypedOperationInfo ServiceOperationInfo
        {
            get
            {
                return ((TypedOperationInfo)(base.GetValue(SendActivity.ServiceOperationInfoProperty)));
            }
            set
            {
                OperationInfoBase currentValue = ((OperationInfoBase)(base.GetValue(SendActivity.ServiceOperationInfoProperty)));
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

                if (this.DesignMode)
                {
                    Activity rootActivity = this.RootActivity;
                    rootActivity.RemoveProperty(DynamicContractTypeBuilder.DynamicContractTypesProperty);
                }

                base.SetValue(SendActivity.ServiceOperationInfoProperty, value);
            }
        }

        SendOperationInfoHelper OperationHelper
        {
            get
            {
                if (this.operationHelper == null)
                {
                    if (this.UserData.Contains(typeof(SendOperationInfoHelper)))
                    {
                        this.operationHelper = this.UserData[typeof(SendOperationInfoHelper)] as SendOperationInfoHelper;
                    }
                }

                if (this.operationHelper == null)
                {
                    this.operationHelper = new SendOperationInfoHelper(this.Site, this);
                    this.UserData[typeof(SendOperationInfoHelper)] = this.operationHelper;
                }

                return this.operationHelper;
            }
        }

        public static IDictionary<string, string> GetContext(Activity activity,
            ChannelToken endpoint,
            Type contractType)
        {
            if (activity == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("activity");
            }
            if (endpoint == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpoint");
            }
            if (contractType == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("contractType");
            }

            return GetContext(activity, endpoint.Name, endpoint.OwnerActivityName, contractType);
        }

        public static IDictionary<string, string> GetContext(Activity activity,
            string endpointName,
            string ownerActivityName,
            Type contractType)
        {
            if (activity == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("activity");
            }
            if (string.IsNullOrEmpty(endpointName))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("endpointName",
                    SR2.GetString(SR2.Error_ArgumentValueNullOrEmptyString));
            }
            if (contractType == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("contractType");
            }

            LogicalChannel logicalChannel = ChannelToken.GetLogicalChannel(activity, endpointName, ownerActivityName, contractType);
            if (logicalChannel != null)
            {
                return logicalChannel.Context;
            }

            return null;
        }

        public static void SetContext(Activity activity,
            ChannelToken endpoint,
            Type contractType,
            IDictionary<string, string> context)
        {
            if (activity == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("activity");
            }
            if (endpoint == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpoint");
            }
            if (contractType == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("contractType");
            }

            SetContext(activity, endpoint.Name, endpoint.OwnerActivityName, contractType, context);
        }

        public static void SetContext(Activity activity,
            string endpointName,
            string ownerActivityName,
            Type contractType,
            IDictionary<string, string> context)
        {
            if (activity == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("activity");
            }
            if (string.IsNullOrEmpty(endpointName))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("endpointName",
                    SR2.GetString(SR2.Error_ArgumentValueNullOrEmptyString));
            }
            if (contractType == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("contractType");
            }

            LogicalChannel logicalChannel = ChannelToken.GetLogicalChannel(activity, endpointName, ownerActivityName, contractType);
            if (logicalChannel != null)
            {
                if (context != null)
                {
                    logicalChannel.Context = context;
                }
                else
                {
                    logicalChannel.Context = ContextMessageProperty.Empty.Context;
                }
            }
        }

        protected internal override ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {
            TypedOperationInfo serviceOperationInfo = this.ServiceOperationInfo;
            if (serviceOperationInfo == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new InvalidOperationException(SR2.GetString(SR2.Error_ServiceOperationInfoNotSpecified, this.Name)));
            }

            MethodInfo methodInfo = serviceOperationInfo.GetMethodInfo(executionContext);
            if (methodInfo == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new InvalidOperationException(SR2.GetString(SR2.Error_MethodInfoNotAvailable, this.Name)));
            }

            ChannelToken channelToken = this.ChannelToken;

            LogicalChannel logicalChannel = ChannelToken.Register(this, channelToken, serviceOperationInfo.ContractType);
            if (!logicalChannel.Initialized)
            {
                logicalChannel.Initialize(channelToken.EndpointName, this.CustomAddress);
            }

            using (ChannelManagerService.ChannelTicket leasedChannel = ChannelManagerService.Take(executionContext, this.WorkflowInstanceId, logicalChannel))
            {
                using (OperationContextScope scope = new OperationContextScope((IContextChannel) leasedChannel.Channel))
                {
                    EventHandler<SendActivityEventArgs>[] invocationList = this.GetInvocationList<EventHandler<SendActivityEventArgs>>(SendActivity.BeforeSendEvent);
                    if (invocationList != null && invocationList.Length > 0)
                    {
                        base.RaiseGenericEvent(SendActivity.BeforeSendEvent, this, new SendActivityEventArgs(this));
                    }

                    SendOperationInfoHelper helper = this.OperationHelper;
                    WorkflowParameterBindingCollection bindings = this.ParameterBindings;

                    object[] parameters = helper.GetInputs(this, bindings);
                    object returnValue = null;

                    bool isSessionless = ChannelManagerHelpers.IsSessionlessContract(logicalChannel.ContractType);
                    bool hasContext = (logicalChannel.Context != null && logicalChannel.Context.Count > 0);
                    bool fatalException = false;

                    if (!isSessionless && hasContext)
                    {
                        ChannelManagerService.ApplyLogicalChannelContext(logicalChannel);
                    }

                    try
                    {
                        returnValue = this.InvokeOperation(methodInfo, leasedChannel.Channel, parameters);
                    }
                    catch (Exception exception)
                    {
                        if (Fx.IsFatal(exception))
                        {
                            fatalException = true;
                        }
                        throw;
                    }
                    finally
                    {
                        if (!fatalException &&
                            !hasContext && !isSessionless && !helper.IsOneWay)
                        {
                            ChannelManagerService.UpdateLogicalChannelContext(logicalChannel);
                        }
                    }

                    helper.PopulateOutputs(this, bindings, parameters, returnValue);

                    invocationList = this.GetInvocationList<EventHandler<SendActivityEventArgs>>(SendActivity.AfterResponseEvent);
                    if (invocationList != null && invocationList.Length > 0)
                    {
                        base.RaiseGenericEvent(SendActivity.AfterResponseEvent, this, new SendActivityEventArgs(this));
                    }
                }
            }

            return ActivityExecutionStatus.Closed;
        }

        internal void GetParameterPropertyDescriptors(IDictionary properties)
        {
            if (properties == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("properties");
            }

            if (((IComponent) this).Site == null)
            {
                return;
            }

            TypedOperationInfo serviceOperationInfo = this.ServiceOperationInfo;
            if (serviceOperationInfo != null)
            {
                MethodInfo methodInfo = serviceOperationInfo.GetMethodInfo(((IComponent) this).Site);
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

        protected override void InitializeProperties()
        {
            TypedOperationInfo serviceOperationInfo = this.ServiceOperationInfo;

            if (serviceOperationInfo == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new InvalidOperationException(SR2.GetString(SR2.Error_ServiceOperationInfoNotSpecified, this.Name)));
            }

            OperationParameterInfoCollection parameters = null;

            Activity definitionRoot = base.RootActivity.GetValue(Activity.WorkflowDefinitionProperty) as Activity;
            if (definitionRoot != null)
            {
                SendActivity definition = definitionRoot.GetActivityByName(this.QualifiedName, true) as SendActivity;
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

            WorkflowParameterBindingCollection parameterBindings = this.ParameterBindings;

            foreach (OperationParameterInfo parameterInfo in parameters)
            {
                if (!parameterBindings.Contains(parameterInfo.Name))
                {
                    parameterBindings.Add(new WorkflowParameterBinding(parameterInfo.Name));
                }
            }

            base.InitializeProperties();
        }

        object InvokeOperation(MethodInfo operation, IChannel channel, object[] parameters)
        {
            Guid workflowInstanceId = this.WorkflowInstanceId;
            string qualifiedName = this.QualifiedName;

            System.Workflow.Runtime.WorkflowTrace.Runtime.TraceEvent(TraceEventType.Verbose, 0,
                "Workflow Instance {0}, send activity {1} - invoking operation",
                workflowInstanceId, qualifiedName);

            try
            {
                object retVal = operation.ReflectedType.InvokeMember(operation.Name,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.InvokeMethod,
                    null,
                    channel,
                    parameters,
                    CultureInfo.InvariantCulture);

                System.Workflow.Runtime.WorkflowTrace.Runtime.TraceEvent(TraceEventType.Verbose, 0,
                    "Workflow Instance {0}, send activity {1} - operation invoke succeeded",
                    workflowInstanceId, qualifiedName);

                return retVal;

            }
            catch (TargetInvocationException e)
            {
                if (e.InnerException != null)
                {
                    System.Workflow.Runtime.WorkflowTrace.Runtime.TraceEvent(TraceEventType.Error, 0,
                        "Workflow Instance {0}, send activity {1} - operation invoke failed with error: {2}",
                        workflowInstanceId, qualifiedName, e.InnerException.Message);

                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(e.InnerException);
                }
                else
                {
                    System.Workflow.Runtime.WorkflowTrace.Runtime.TraceEvent(TraceEventType.Error, 0,
                        "Workflow Instance {0}, send activity {1} - operation invoke failed with error: {2}",
                        workflowInstanceId, qualifiedName, e.Message);

                    throw;
                }
            }
        }

        [Serializable]
        class SendOperationInfoHelper
        {
            bool hasReturnValue = false;
            IList<KeyValuePair<int, string>> inputParameters;
            bool isOneWay = false;
            string operationName;
            IList<KeyValuePair<int, string>> outputParameters;
            int parameterCount = 0;

            public SendOperationInfoHelper(IServiceProvider serviceProvider, SendActivity activity)
            {
                outputParameters = new List<KeyValuePair<int, string>>();
                inputParameters = new List<KeyValuePair<int, string>>();
                hasReturnValue = false;

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
                }

                foreach (ParameterInfo parameter in methodInfo.GetParameters())
                {
                    if (parameter.ParameterType.IsByRef ||
                        parameter.IsOut || (parameter.IsIn && parameter.IsOut))
                    {
                        outputParameters.Add(new KeyValuePair<int, string>(parameter.Position, parameter.Name));
                    }

                    if (!parameter.IsOut || (parameter.IsIn && parameter.IsOut))
                    {
                        inputParameters.Add(new KeyValuePair<int, string>(parameter.Position, parameter.Name));
                    }
                }

                this.parameterCount = methodInfo.GetParameters().Length;

                this.operationName = serviceOperationInfo.Name;

                object[] operationContractAttribs = methodInfo.GetCustomAttributes(typeof(OperationContractAttribute), true);

                if (operationContractAttribs != null && operationContractAttribs.Length > 0)
                {
                    if (operationContractAttribs[0] is OperationContractAttribute)
                    {
                        this.isOneWay = ((OperationContractAttribute) operationContractAttribs[0]).IsOneWay;
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

            public object[] GetInputs(SendActivity activity, WorkflowParameterBindingCollection bindings)
            {
                if (activity == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("activity");
                }

                object[] inputs = new object[this.parameterCount];

                if (inputParameters.Count > 0)
                {
                    for (int index = 0; index < inputParameters.Count; index++)
                    {
                        KeyValuePair<int, string> parameterInfo = inputParameters[index];

                        if (bindings.Contains(parameterInfo.Value))
                        {
                            inputs[parameterInfo.Key] = bindings[parameterInfo.Value].Value;
                        }
                        else
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                                new InvalidOperationException(SR2.GetString(SR2.Error_ParameterBindingMissing,
                                parameterInfo.Value,
                                this.operationName,
                                activity.Name)));
                        }
                    }
                }

                return inputs;
            }

            public void PopulateOutputs(SendActivity activity, WorkflowParameterBindingCollection bindings, object[] outputs, object returnValue)
            {
                if (activity == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("activity");
                }

                if (outputs == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("outputs");
                }

                if (this.outputParameters.Count > 0 && outputs.Length > 0)
                {
                    for (int index = 0; index < outputParameters.Count; ++index)
                    {
                        KeyValuePair<int, string> parameterInfo = outputParameters[index];
                        if (bindings.Contains(parameterInfo.Value))
                        {
                            bindings[parameterInfo.Value].Value = outputs[parameterInfo.Key];
                        }
                        else
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                                new InvalidOperationException(SR2.GetString(SR2.Error_ParameterBindingMissing,
                                parameterInfo.Value, this.operationName, activity.Name)));
                        }
                    }
                }

                if (hasReturnValue)
                {
                    if (bindings.Contains(SendActivity.ReturnValuePropertyName))
                    {
                        bindings[SendActivity.ReturnValuePropertyName].Value = returnValue;
                    }
                    else
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                            new InvalidOperationException(SR2.GetString(SR2.Error_ParameterBindingMissing,
                            SendActivity.ReturnValuePropertyName,
                            this.operationName,
                            activity.Name)));
                    }
                }
            }
        }
    }
}
