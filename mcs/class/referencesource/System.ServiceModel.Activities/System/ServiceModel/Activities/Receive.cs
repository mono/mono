//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Activities
{
    using System.Activities;
    using System.Activities.DynamicUpdate;
    using System.Activities.Expressions;
    using System.Activities.Statements;
    using System.Activities.Validation;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Net.Security;
    using System.Runtime;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.XamlIntegration;
    using System.Windows.Markup;
    using System.Xaml;
    using System.Xml.Linq;

    [ContentProperty("Content")]
    public sealed class Receive : Activity
    {
        MessageQuerySet correlatesOn;
        Collection<CorrelationInitializer> correlationInitializers;
        Collection<Type> knownTypes;

        // Used by contract-inference to build the ServiceDescription
        IList<SendReply> followingReplies;
        IList<SendReply> followingFaults;

        // cached by base.Implementation invocation
        InternalReceiveMessage internalReceive;
        FromRequest requestFormatter;

        public Receive()
            : base()
        {
            base.Implementation = () =>
            {
                // if CacheMetadata isn't called, bail early
                if (this.internalReceive == null)
                {
                    return null;
                }

                // requestFormatter is null if we have an untyped message situation
                if (this.requestFormatter == null)
                {
                    return this.internalReceive;
                }
                else
                {
                    Variable<Message> request = new Variable<Message> { Name = "RequestMessage" };
                    Variable<NoPersistHandle> noPersistHandle = new Variable<NoPersistHandle> { Name = "ReceiveNoPersistHandle" };
                    this.internalReceive.Message = new OutArgument<Message>(request);
                    this.requestFormatter.Message = new InOutArgument<Message>(request);
                    this.internalReceive.NoPersistHandle = new InArgument<NoPersistHandle>(noPersistHandle);
                    this.requestFormatter.NoPersistHandle = new InArgument<NoPersistHandle>(noPersistHandle);

                    return new Sequence
                    {
                        Variables = { request, noPersistHandle },
                        Activities = { this.internalReceive, this.requestFormatter }
                    };
                }
            };
        }

        [SuppressMessage(FxCop.Category.Usage, FxCop.Rule.CollectionPropertiesShouldBeReadOnly,
            Justification = "MessageQuerySet is a stand-alone class. We want to allow users to create their own.")]
        [SuppressMessage(FxCop.Category.Xaml, FxCop.Rule.PropertyExternalTypesMustBeKnown,
            Justification = "MessageQuerySet is a known XAML-serializable type in this assembly.")]
        public MessageQuerySet CorrelatesOn
        {
            get
            {
                if (this.correlatesOn == null)
                {
                    this.correlatesOn = new MessageQuerySet();
                }
                return this.correlatesOn;
            }
            set
            {
                this.correlatesOn = value;
            }
        }

        // the content to receive (either message or parameters-based) declared by the user
        [DefaultValue(null)]
        public ReceiveContent Content
        {
            get;
            set;
        }

        // Internally, we should always use InternalContent since this property may have default content that we added
        internal ReceiveContent InternalContent
        {
            get
            {
                return this.Content ?? ReceiveContent.DefaultReceiveContent;
            }
        }

        // For initializing additional correlations beyond the CorrelatesWith property for following
        // activities.
        public Collection<CorrelationInitializer> CorrelationInitializers
        {
            get
            {
                if (this.correlationInitializers == null)
                {
                    this.correlationInitializers = new Collection<CorrelationInitializer>();
                }
                return this.correlationInitializers;
            }
        }

        // Allows the action for the message to be specified by the user/designer.
        // If not set, the value is constructed from the Name of the activity.
        // If specified on the Send side of a message, the Receive side needs to have the same value in order
        // for the message to be delivered correctly.
        [DefaultValue(null)]
        public string Action
        {
            get;
            set;
        }

        // If true, a new workflow instance is created to process the message. If false, an existing workflow
        // instance is determined based on correlations.
        [DefaultValue(false)]
        public bool CanCreateInstance
        {
            get;
            set;
        }

        [DefaultValue(null)]
        public InArgument<CorrelationHandle> CorrelatesWith
        {
            get;
            set;
        }

        // This will be used to construct the default value for Action if the Action property is
        // not specifically set. It is also used in conjunction with CorrelatesWith and IsReply
        // to determine which Send and Receive activities make up a Request/Reply combination.
        // There is validation to make sure either this or Action has a value.
        // Used by IServiceDescriptionBuilder2 as well
        [DefaultValue(null)]
        public string OperationName
        {
            get;
            set;
        }

        // The protection level for the message.
        // If ValueType or Value.Expression.ExpressionType is MessageContract, the MessageContract definition may have additional settings.
        // Default if this is not specified is Sign.
        [DefaultValue(null)]
        public ProtectionLevel? ProtectionLevel
        {
            get;
            set;
        }

        [DefaultValue(SerializerOption.DataContractSerializer)]
        public SerializerOption SerializerOption
        {
            get;
            set;
        }

        // The service contract name. This allows the same workflow instance to implement multiple
        // servce "contracts". If not specified and this is the first Receive activity in the
        // workflow, contract inference uses this activity's Name as the service contract name.
        [DefaultValue(null)]
        [TypeConverter(typeof(ServiceXNameTypeConverter))]
        public XName ServiceContractName
        {
            get;
            set;
        }

        public Collection<Type> KnownTypes
        {
            get
            {
                if (this.knownTypes == null)
                {
                    this.knownTypes = new Collection<Type>();
                }
                return this.knownTypes;
            }
        }

        internal string OperationBookmarkName
        {
            get
            {
                Fx.Assert(this.internalReceive != null, "CacheMetadata must be called before this!");
                return this.internalReceive.OperationBookmarkName;
            }
        }

        internal Collection<Type> InternalKnownTypes
        {
            get
            {
                // avoid the allocation if internal
                return this.knownTypes;
            }
        }

        internal bool HasCorrelatesOn
        {
            get
            {
                return this.correlatesOn != null && this.correlatesOn.Count > 0;
            }
        }

        internal bool HasCorrelationInitializers
        {
            get
            {
                return this.correlationInitializers != null && this.correlationInitializers.Count > 0;
            }
        }

        internal IList<SendReply> FollowingReplies
        {
            get
            {
                if (this.followingReplies == null)
                {
                    this.followingReplies = new List<SendReply>();
                }
                return this.followingReplies;
            }
        }

        internal IList<SendReply> FollowingFaults
        {
            get
            {
                if (this.followingFaults == null)
                {
                    this.followingFaults = new List<SendReply>();
                }
                return this.followingFaults;
            }
        }

        internal bool HasReply
        {
            get
            {
                return this.followingReplies != null && this.followingReplies.Count > 0;
            }
        }

        internal bool HasFault
        {
            get
            {
                return this.followingFaults != null && this.followingFaults.Count > 0;
            }
        }
              
        internal InternalReceiveMessage InternalReceive
        {
            get 
            {
                return this.internalReceive;
            }
        }

        bool IsInvalidContentChangeFrom(ReceiveContent originalContent)
        {
            ReceiveMessageContent newMsgContent = this.InternalContent as ReceiveMessageContent;
            ReceiveMessageContent originalMsgContent = originalContent as ReceiveMessageContent;

            if (newMsgContent != null && newMsgContent.InternalDeclaredMessageType == MessageDescription.TypeOfUntypedMessage)
            {
                if (originalMsgContent == null || originalMsgContent.InternalDeclaredMessageType != MessageDescription.TypeOfUntypedMessage)
                {
                    return true;
                }
            }
            else if (originalMsgContent != null && originalMsgContent.InternalDeclaredMessageType == MessageDescription.TypeOfUntypedMessage)
            {
                return true;
            }
            
            return false;
        }

        bool HasCorrelationsChanged(Collection<CorrelationInitializer> originalCorrelationInitializers)
        {
            if (this.CorrelationInitializers.Count != originalCorrelationInitializers.Count)
            {
                return true;
            }

            for (int i = 0; i < this.CorrelationInitializers.Count; i++)
            {
                CorrelationInitializer newCorr = this.CorrelationInitializers[i];
                CorrelationInitializer oldCorr = originalCorrelationInitializers[i];

                if (newCorr.ArgumentName != oldCorr.ArgumentName)
                {
                    return true;
                }
            }

            return false;
        }

        protected override void OnCreateDynamicUpdateMap(UpdateMapMetadata metadata, Activity originalActivity)
        {
            if (this.IsInvalidContentChangeFrom(((Receive)originalActivity).InternalContent))
            {
                // Due to technical limitation, we don't currently support changing from untyped MessageContent to typed MessageContent or ParametersContent and vice versa.
                metadata.DisallowUpdateInsideThisActivity(SR.ReceiveContentChanged);
            }
            else if (this.HasCorrelationsChanged(((Receive)originalActivity).CorrelationInitializers))
            {
                // we don't currently support changing CorrelationInitializers collection of Receive due to technical limitation.
                // This change could be detected and blocked for update by the runtime, but we check this early
                // so that we can provide more meaningful error message.
                metadata.DisallowUpdateInsideThisActivity(SR.ReceiveCorrelationInitializiersChanged);
            }
        }

        protected override void CacheMetadata(ActivityMetadata metadata)
        {
            if (string.IsNullOrEmpty(this.OperationName))
            {
                metadata.AddValidationError(SR.MissingOperationName(this.DisplayName));
            }

            // validate Correlation Initializers
            MessagingActivityHelper.ValidateCorrelationInitializer(metadata, this.correlationInitializers, false, this.DisplayName, this.OperationName);

            // Add runtime arguments
            MessagingActivityHelper.AddRuntimeArgument(this.CorrelatesWith, "CorrelatesWith", Constants.CorrelationHandleType, ArgumentDirection.In, metadata);
            
            // Validate Content
            this.InternalContent.CacheMetadata(metadata, this, this.OperationName);

            if (this.correlationInitializers != null)
            {
                for (int i = 0; i < this.correlationInitializers.Count; i++)
                {
                    CorrelationInitializer initializer = this.correlationInitializers[i];
                    initializer.ArgumentName = Constants.Parameter + i;
                    RuntimeArgument initializerArgument = new RuntimeArgument(initializer.ArgumentName, Constants.CorrelationHandleType, ArgumentDirection.In);
                    metadata.Bind(initializer.CorrelationHandle, initializerArgument);
                    metadata.AddArgument(initializerArgument);
                }
            }

            if (!metadata.HasViolations)
            {
                this.internalReceive = CreateInternalReceive();
                this.InternalContent.ConfigureInternalReceive(this.internalReceive, out this.requestFormatter);
            }
            else
            {
                this.internalReceive = null;
                this.requestFormatter = null;
            }
        }
        
        InternalReceiveMessage CreateInternalReceive()
        {
            InternalReceiveMessage result = new InternalReceiveMessage
            {
                Action = this.Action,
                OperationName = this.OperationName,
                OwnerDisplayName = this.DisplayName,
                ServiceContractName = this.ServiceContractName,
                CorrelatesWith = new InArgument<CorrelationHandle>(new ArgumentValue<CorrelationHandle> { ArgumentName = "CorrelatesWith" }),
                IsOneWay = true  // This will be updated by contract inference logic,
            };

            if (this.correlationInitializers != null)
            {
                foreach (CorrelationInitializer correlation in this.correlationInitializers)
                {
                    result.CorrelationInitializers.Add(correlation.Clone());
                }
            }

            return result;
        }

        internal void SetIsOneWay(bool flag)
        {
            Fx.Assert(this.internalReceive != null, "InternalReceiveMessage cannot be null!");

            this.internalReceive.IsOneWay = flag;
            
            // perf optimization if the receive is two way, null out the NoPersistHandle, 
            // this optimization allows us not to access AEC in InternalReceiveMessage->Execute to null out the 
            // NoPersistHandle in case of two-way. With this optimization, we just assert in InternalReceiveMessage
            if (!this.internalReceive.IsOneWay)
            {
                this.internalReceive.NoPersistHandle = null;
                if (this.requestFormatter != null)
                {
                    this.requestFormatter.NoPersistHandle = null;
                }
            }
            else
            {
                // For oneway operations the FromRequest should close the message
                if (this.requestFormatter != null)
                {
                    this.requestFormatter.CloseMessage = true;
                }
            }
        }

        internal void SetDefaultFormatters(OperationDescription operationDescription)
        {
            this.SetFormatter(this.GetDefaultMessageFormatter(operationDescription), this.GetDefaultFaultFormatter(), includeExceptionDetailInFaults: false);
        }

        internal void SetFormatter(IDispatchMessageFormatter formatter, IDispatchFaultFormatter faultFormatter, bool includeExceptionDetailInFaults)
        {
            if (this.requestFormatter != null)
            {
                this.requestFormatter.Formatter = formatter;
            }

            if (this.followingReplies != null) 
            {
                for (int i = 0; i < this.followingReplies.Count; i++)
                {
                    this.followingReplies[i].SetFormatter(formatter);
                }
            }

            if (this.followingFaults != null)
            {
                for (int i = 0; i < this.followingFaults.Count; i++)
                {
                    this.followingFaults[i].SetFaultFormatter(faultFormatter, includeExceptionDetailInFaults);
                }
            }
        }

        IDispatchMessageFormatter GetDefaultMessageFormatter(OperationDescription operationDescription)
        {
            return ServiceOperationFormatterProvider.GetDispatcherFormatterFromRuntime(operationDescription);
        }

        IDispatchFaultFormatter GetDefaultFaultFormatter()
        {
            return new FaultFormatter(this.KnownTypes.ToArray());
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeCorrelatesOn()
        {
            // don't serialize null nor default MessageQuerySet
            if (this.correlatesOn == null || (this.correlatesOn.Name == null && this.correlatesOn.Count == 0))
            {
                return false;
            }
            return true;
        }

        public static Receive FromOperationDescription(OperationDescription operation)
        {
            if (operation == null)
            {
                throw FxTrace.Exception.ArgumentNull("operation", "OperationDescription should not be null");
            }

            MessageDescription message;
            Receive receiveActivity = new Receive();
            receiveActivity.ServiceContractName = XName.Get(operation.DeclaringContract.Name, operation.DeclaringContract.Namespace);
            receiveActivity.OperationName = operation.Name;
            receiveActivity.DisplayName = operation.Name + "Receive";
            receiveActivity.ProtectionLevel = operation.ProtectionLevel;

            if (operation.Messages != null && operation.Messages.Count > 0)
            {
                receiveActivity.Action = operation.Messages[0].Action;
            }

            Collection<Type> knownTypes = operation.KnownTypes;
            if (knownTypes != null)
            {
                foreach (Type knownType in knownTypes)
                {
                    receiveActivity.KnownTypes.Add(knownType);
                }
            }

            // Set SerializerOption
            if (operation.Behaviors.Contains(typeof(XmlSerializerOperationBehavior)))
            {
                receiveActivity.SerializerOption = SerializerOption.XmlSerializer;
            }
            else
            {
                receiveActivity.SerializerOption = SerializerOption.DataContractSerializer;
            }

            bool contentIsParameter = false;
            bool noReceiveMessageContent = false;

            message = operation.Messages[0];

            // MessageType is null indicating it is not typed message contract
            if (message.MessageType == null)
            {
                if (message.Body.Parts != null)
                {
                    if (message.Body.Parts.Count != 0)
                    {
                        foreach (MessagePartDescription messagePart in message.Body.Parts)
                        {
                            if (messagePart.Index > 0)
                            {
                                contentIsParameter = true;
                                break;
                            }
                            // Indicating it is a untyped message contract
                            if (!messagePart.Type.IsAssignableFrom(typeof(System.ServiceModel.Channels.Message)))
                            {
                                contentIsParameter = true;
                            }
                        }
                    }
                    else
                    {
                        noReceiveMessageContent = true;
                    }
                }
                else
                {
                    noReceiveMessageContent = true;
                }
            }

            if (!noReceiveMessageContent)
            {
                if (contentIsParameter)
                {
                    ReceiveParametersContent content = new ReceiveParametersContent();
                    if (message.Direction == MessageDirection.Input && message.Body.Parts != null)
                    {
                        foreach (MessagePartDescription messagePart in message.Body.Parts)
                        {
                            Argument outArgument = OutArgument.Create(messagePart.Type, ArgumentDirection.Out);
                            content.Parameters.Add(messagePart.Name, (OutArgument)(outArgument));
                        }
                    }

                    receiveActivity.Content = content;
                }
                else
                {
                    ReceiveMessageContent content = new ReceiveMessageContent();
                    if (message.Direction == MessageDirection.Input)
                    {
                        if (message.MessageType != null)
                        {
                            content.DeclaredMessageType = message.MessageType;
                        }
                        else
                        {
                            content.DeclaredMessageType = message.Body.Parts[0].Type;
                        }

                        Argument outArgument = OutArgument.Create(content.DeclaredMessageType, ArgumentDirection.Out);
                        content.Message = (OutArgument)outArgument;
                    }

                    receiveActivity.Content = content;
                }
            }
            else
            {
                if ((message.Body.ReturnValue != null && message.Body.ReturnValue.Type.IsDefined(typeof(MessageContractAttribute), false))
                    || (message.Body.ReturnValue != null && message.Body.ReturnValue.Type.IsAssignableFrom(typeof(System.ServiceModel.Channels.Message))))
                {
                    receiveActivity.Content = new ReceiveMessageContent();
                }
                else if (operation.Messages.Count > 1)
                {
                    if (operation.Messages[1].MessageType != null || operation.Messages[1].Body.ReturnValue.Type.IsAssignableFrom(typeof(System.ServiceModel.Channels.Message)))
                    {
                        receiveActivity.Content = new ReceiveMessageContent();
                    }
                    else
                    {
                        receiveActivity.Content = new ReceiveParametersContent();
                    }
                }
                else
                {
                    receiveActivity.Content = new ReceiveParametersContent();
                }
            }

            return receiveActivity;
        }
    }
}
