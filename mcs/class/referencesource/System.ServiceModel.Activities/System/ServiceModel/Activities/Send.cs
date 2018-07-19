//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Activities
{
    using System.Activities;
    using System.Activities.Expressions;
    using System.Activities.Statements;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Net.Security;
    using System.Runtime;
    using System.Security.Principal;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.XamlIntegration;
    using System.Windows.Markup;
    using System.Xml.Linq;

    [ContentProperty("Content")]
    public sealed class Send : Activity
    {
        ToRequest requestFormatter;
        InternalSendMessage internalSend;
        Collection<Type> knownTypes;

        Collection<CorrelationInitializer> correlationInitializers;

        bool isOneWay;
        IClientMessageFormatter lazyFormatter;
        IList<CorrelationQuery> lazyCorrelationQueries;

        bool? channelCacheEnabled;

        public Send()
        {
            this.isOneWay = true; // ReceiveReply if defined by user, will set this to false
            this.TokenImpersonationLevel = TokenImpersonationLevel.Identification;
            base.Implementation = () =>
            {
                // if CacheMetadata isn't called, bail early
                if (this.internalSend == null)
                {
                    return null;
                }

                if (this.requestFormatter == null) 
                {
                    return this.internalSend;
                }
                else
                {
                    Variable<Message> request = new Variable<Message> { Name = "RequestMessage" };
                    this.requestFormatter.Message = new OutArgument<Message>(request);
                    this.requestFormatter.Send = this;
                    this.internalSend.Message = new InArgument<Message>(request);
                    this.internalSend.MessageOut = new OutArgument<Message>(request);

                    return new MessagingNoPersistScope
                    {
                        Body = new Sequence
                        {
                            Variables = { request },
                            Activities = 
                            { 
                                this.requestFormatter, 
                                this.internalSend, 
                            }
                        }
                    };
                }
            };
        }

        // the content to send (either message or parameters-based) declared by the user
        [DefaultValue(null)]
        public SendContent Content
        {
            get;
            set;
        }

        // Internally, we should always use InternalContent since this property may have default content that we added
        internal SendContent InternalContent
        {
            get
            {
                return this.Content ?? SendContent.DefaultSendContent;
            }
        }

        // Additional correlations allow situations where a "session" involves multiple
        // messages between two workflow instances.
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

        [DefaultValue(null)]
        public InArgument<CorrelationHandle> CorrelatesWith
        {
            get;
            set;
        }

        // Specifies the endpoint of the service we are sending to. If not specified, it must come
        // from the configuration file.
        [DefaultValue(null)]
        public Endpoint Endpoint
        {
            get;
            set;
        }

        // This is used to load the client Endpoint from configuration. This should be mutually exclusive
        // with the Endpoint property.
        // optional defaults to ServiceContractName.LocalName
        [DefaultValue(null)]
        public string EndpointConfigurationName
        {
            get;
            set;
        }

        // Allows the address portion of the endpoint to be overridden at runtime by the
        // workflow instance.
        [DefaultValue(null)]
        public InArgument<Uri> EndpointAddress
        {
            get;
            set;
        }

        // Do we need this?
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

        // this will be used to construct the default value for Action if the Action property is
        // not specifically set.
        // There is validation to make sure either this or Action has a value.
        [DefaultValue(null)]
        public string OperationName
        {
            get;
            set;
        }

        // The protection level for the message.
        // If ValueType or Value.Expression.Type is MessageContract, the MessageContract definition may have additional settings.
        // Default if this is not specified is Sign.
        [DefaultValue(null)]
        public ProtectionLevel? ProtectionLevel
        {
            get;
            set;
        }

        // Maybe an enum to limit possibilities
        // I am still a little fuzzy on this property.
        [DefaultValue(SerializerOption.DataContractSerializer)]
        public SerializerOption SerializerOption
        {
            get;
            set;
        }

        // The service contract name. This allows the same workflow instance to implement multiple
        // servce "contracts". If not specified and this is the first Receive* activity in the
        // workflow, contract inference uses this activity's Name as the service contract name.
        [DefaultValue(null)]
        [TypeConverter(typeof(ServiceXNameTypeConverter))]
        public XName ServiceContractName
        {
            get;
            set;
        }
        // The token impersonation level that is allowed for the receiver of the message.
        [DefaultValue(TokenImpersonationLevel.Identification)]
        public TokenImpersonationLevel TokenImpersonationLevel
        {
            get;
            set;
        }

        internal bool ChannelCacheEnabled
        {
            get
            {
                Fx.Assert(this.channelCacheEnabled.HasValue, "The value of channelCacheEnabled must be initialized!");
                return this.channelCacheEnabled.Value;
            }
        }

        internal bool OperationUsesMessageContract
        {
            get;
            set;
        }

        internal OperationDescription OperationDescription
        {
            get;
            set;
        }

        protected override void CacheMetadata(ActivityMetadata metadata)
        {
            if (string.IsNullOrEmpty(this.OperationName))
            {
                metadata.AddValidationError(SR.MissingOperationName(this.DisplayName));
            }
            if (this.ServiceContractName == null)
            {
                string errorOperationName = ContractValidationHelper.GetErrorMessageOperationName(this.OperationName);
                metadata.AddValidationError(SR.MissingServiceContractName(this.DisplayName, errorOperationName));
            }

            if (this.Endpoint == null)
            {
                if (string.IsNullOrEmpty(this.EndpointConfigurationName))
                {
                    metadata.AddValidationError(SR.EndpointNotSet(this.DisplayName, this.OperationName));
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(this.EndpointConfigurationName))
                {
                    metadata.AddValidationError(SR.EndpointIncorrectlySet(this.DisplayName, this.OperationName));
                }
                if (this.Endpoint.Binding == null)
                {
                    metadata.AddValidationError(SR.MissingBindingInEndpoint(this.Endpoint.Name, this.ServiceContractName));
                }
            }

            // validate Correlation Initializers
            MessagingActivityHelper.ValidateCorrelationInitializer(metadata, this.correlationInitializers, false, this.DisplayName, this.OperationName);
            
            // Add runtime arguments
            MessagingActivityHelper.AddRuntimeArgument(this.CorrelatesWith, "CorrelatesWith", Constants.CorrelationHandleType, ArgumentDirection.In, metadata);
            MessagingActivityHelper.AddRuntimeArgument(this.EndpointAddress, "EndpointAddress", Constants.UriType, ArgumentDirection.In, metadata);

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
                if (this.InternalContent is SendMessageContent
                    && MessageBuilder.IsMessageContract(((SendMessageContent)this.InternalContent).InternalDeclaredMessageType))
                {
                    this.OperationUsesMessageContract = true;
                }

                this.internalSend = CreateInternalSend();
                this.InternalContent.ConfigureInternalSend(this.internalSend, out this.requestFormatter);

                if (this.requestFormatter != null && this.lazyFormatter != null)
                {
                    this.requestFormatter.Formatter = this.lazyFormatter;
                }
            }
            else
            {
                this.internalSend = null;
                this.requestFormatter = null;
            }
        }

        InternalSendMessage CreateInternalSend()
        {
            InternalSendMessage result = new InternalSendMessage
            {
                OwnerDisplayName = this.DisplayName,
                OperationName = this.OperationName,
                CorrelatesWith = new InArgument<CorrelationHandle>(new ArgumentValue<CorrelationHandle> { ArgumentName = "CorrelatesWith" }),
                Endpoint = this.Endpoint,
                EndpointConfigurationName = this.EndpointConfigurationName,
                IsOneWay = this.isOneWay,
                IsSendReply = false,
                TokenImpersonationLevel = this.TokenImpersonationLevel,
                ServiceContractName = this.ServiceContractName,
                Action = this.Action,
                Parent = this
            };

            if (this.correlationInitializers != null)
            {
                foreach (CorrelationInitializer correlation in this.correlationInitializers)
                {
                    result.CorrelationInitializers.Add(correlation.Clone());
                }

                Collection<CorrelationQuery> internalCorrelationQueryCollection = ContractInferenceHelper.CreateClientCorrelationQueries(null, this.correlationInitializers,
                        this.Action, this.ServiceContractName, this.OperationName, false);
                Fx.Assert(internalCorrelationQueryCollection.Count <= 1, "Querycollection for send cannot have more than one correlation query");
                if (internalCorrelationQueryCollection.Count == 1)
                {
                    result.CorrelationQuery = internalCorrelationQueryCollection[0];
                }
            }

            if (this.EndpointAddress != null)
            {
                result.EndpointAddress = new InArgument<Uri>(context => ((InArgument<Uri>)this.EndpointAddress).Get(context));
            }

            if (this.lazyCorrelationQueries != null)
            {
                foreach (CorrelationQuery correlationQuery in this.lazyCorrelationQueries)
                {
                    result.ReplyCorrelationQueries.Add(correlationQuery);
                }
            }

            return result;
        }

        // Acccessed by ReceiveReply.CreateBody to set OneWay to false
        internal void SetIsOneWay(bool value)
        {
            this.isOneWay = value;
            if (this.internalSend != null)
            {
                this.internalSend.IsOneWay = this.isOneWay;
            }
        }

        internal MessageVersion GetMessageVersion()
        {
            return this.internalSend.GetMessageVersion();
        }

        internal void SetFormatter(IClientMessageFormatter formatter)
        {
            if (this.requestFormatter != null)
            {
                this.requestFormatter.Formatter = formatter;
            }
            else
            {
                // we save the formatter and set the requestFormatter.Formatter later
                this.lazyFormatter = formatter;
            }
        }

        internal void SetReplyCorrelationQuery(CorrelationQuery replyQuery)
        {
            Fx.Assert(replyQuery != null, "replyQuery cannot be null!");

            if (this.internalSend != null && !this.internalSend.ReplyCorrelationQueries.Contains(replyQuery))
            {
                this.internalSend.ReplyCorrelationQueries.Add(replyQuery);
            }
            else
            {
                // we save the CorrelationQuery and add it to InternalSendMessage later
                if (this.lazyCorrelationQueries == null)
                {
                    this.lazyCorrelationQueries = new List<CorrelationQuery>();
                }
                this.lazyCorrelationQueries.Add(replyQuery);
            }
        }

        internal void InitializeChannelCacheEnabledSetting(ActivityContext context)
        {
            if (!this.channelCacheEnabled.HasValue)
            {
                SendMessageChannelCache channelCacheExtension = context.GetExtension<SendMessageChannelCache>();
                Fx.Assert(channelCacheExtension != null, "channelCacheExtension must exist!");

                InitializeChannelCacheEnabledSetting(channelCacheExtension);
            }
        }

        internal void InitializeChannelCacheEnabledSetting(SendMessageChannelCache channelCacheExtension)
        {
            Fx.Assert(channelCacheExtension != null, "channelCacheExtension cannot be null!");

            ChannelCacheSettings factorySettings = channelCacheExtension.FactorySettings;
            Fx.Assert(factorySettings != null, "FactorySettings cannot be null!");

            bool enabled;

            if (factorySettings.IdleTimeout == TimeSpan.Zero || factorySettings.LeaseTimeout == TimeSpan.Zero || factorySettings.MaxItemsInCache == 0)
            {
                enabled = false;
            }
            else
            {
                enabled = true;
            }

            if (!this.channelCacheEnabled.HasValue)
            {
                this.channelCacheEnabled = enabled;
            }
            else
            {
                Fx.Assert(this.channelCacheEnabled.Value == enabled, "Once ChannelCacheEnabled is set, it cannot be changed!");
            }
        }
    }
}
