//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Activities
{
    using System.Activities;
    using System.Activities.Statements;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;
    using System.Windows.Markup;

    // Used to receive the reply in a request/reply messaging pattern. Must be paired with
    // a corresponding Send activity.
    [ContentProperty("Content")]
    public sealed class ReceiveReply : Activity
    {
        Collection<CorrelationInitializer> correlationInitializers;
        FromReply responseFormatter;
        InternalReceiveMessage internalReceive;

        public ReceiveReply()
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
                if (this.responseFormatter == null) 
                {
                    return this.internalReceive;
                }
                else
                {
                    Variable<Message> response = new Variable<Message> { Name = "ResponseMessage" };
                    this.internalReceive.Message = new OutArgument<Message>(response);
                    this.responseFormatter.Message = new InArgument<Message>(response);

                    return new MessagingNoPersistScope
                    {
                        Body = new Sequence
                        {
                            Variables = { response },
                            Activities =
                                {
                                    this.internalReceive,
                                    this.responseFormatter,
                                }
                        }
                    };
                }
            };
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

        [DefaultValue(null)]
        public string Action
        {
            get;
            set;
        }

        // Reference to the Send activity that is responsible for sending the Request part of the
        // request/reply pattern. This cannot be null.
        [DefaultValue(null)]
        public Send Request
        {
            get;
            set;
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

        protected override void CacheMetadata(ActivityMetadata metadata)
        {
            if (this.Request == null)
            {
                metadata.AddValidationError(SR.ReceiveReplyRequestCannotBeNull(this.DisplayName));
            }
            else
            {
                // Need to validate Send.ServiceContractName and Send.OperationName here so that we can proceed with contract inference
                if (this.Request.ServiceContractName == null)
                {
                    string errorOperationName = ContractValidationHelper.GetErrorMessageOperationName(this.Request.OperationName);
                    metadata.AddValidationError(SR.MissingServiceContractName(this.Request.DisplayName, errorOperationName));
                }
                if (string.IsNullOrEmpty(this.Request.OperationName))
                {
                    metadata.AddValidationError(SR.MissingOperationName(this.Request.DisplayName));
                }
            }

            // validate Correlation Initializers
            MessagingActivityHelper.ValidateCorrelationInitializer(metadata, this.correlationInitializers, true, this.DisplayName, (this.Request != null ? this.Request.OperationName : String.Empty));
            
            // Validate Content
            string operationName = this.Request != null ? this.Request.OperationName : null;
            this.InternalContent.CacheMetadata(metadata, this, operationName);

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

                InArgument<CorrelationHandle> requestReplyHandleFromSend = GetReplyHandleFromSend();
                if (requestReplyHandleFromSend != null)
                {
                    InArgument<CorrelationHandle> resultCorrelatesWith = MessagingActivityHelper.CreateReplyCorrelatesWith(requestReplyHandleFromSend);

                    RuntimeArgument resultCorrelatesWithArgument = new RuntimeArgument("ResultCorrelatesWith", Constants.CorrelationHandleType, ArgumentDirection.In);
                    metadata.Bind(resultCorrelatesWith, resultCorrelatesWithArgument);
                    metadata.AddArgument(resultCorrelatesWithArgument);

                    this.internalReceive.CorrelatesWith = (InArgument<CorrelationHandle>)InArgument.CreateReference(resultCorrelatesWith, "ResultCorrelatesWith");
                }

                this.InternalContent.ConfigureInternalReceiveReply(this.internalReceive, out this.responseFormatter);

                if (this.InternalContent is ReceiveMessageContent
                    && MessageBuilder.IsMessageContract(((ReceiveMessageContent)this.InternalContent).InternalDeclaredMessageType))
                {
                    this.Request.OperationUsesMessageContract = true;
                }

                OperationDescription operation = ContractInferenceHelper.CreateTwoWayOperationDescription(this.Request, this);
                this.Request.OperationDescription = operation;

                if (this.responseFormatter != null)
                {
                    IClientMessageFormatter formatter = ClientOperationFormatterProvider.GetFormatterFromRuntime(operation);

                    this.Request.SetFormatter(formatter);
                    this.responseFormatter.Formatter = formatter;

                    // 
                    int index = 0;
                    Type[] faultTypes = new Type[operation.KnownTypes.Count];
                    foreach (Type type in operation.KnownTypes)
                    {
                        faultTypes[index] = type;
                        index++;
                    }

                    this.responseFormatter.FaultFormatter = new FaultFormatter(faultTypes);
                }

                // Add CorrelationQuery to the Send->ReplyCorrelation, we validate that the same query is not added multiple times
                if (this.correlationInitializers != null && this.correlationInitializers.Count > 0)
                {
                    Collection<CorrelationQuery> internalCorrelationQueryCollection = ContractInferenceHelper.CreateClientCorrelationQueries(null, this.correlationInitializers,
                        this.Action, this.Request.ServiceContractName, this.Request.OperationName, true);

                    foreach (CorrelationQuery query in internalCorrelationQueryCollection)
                    {
                        this.Request.SetReplyCorrelationQuery(query);
                    }
                    
                }
            }
            else
            {
                this.internalReceive = null;
                this.responseFormatter = null;
            }

            // We don't have any imported children despite referencing the Request
            metadata.SetImportedChildrenCollection(new Collection<Activity>());
        }

        InternalReceiveMessage CreateInternalReceive()
        {
            InternalReceiveMessage result = new InternalReceiveMessage
            {
                IsOneWay = false,
                IsReceiveReply = true,
                OwnerDisplayName = this.DisplayName
            };

            if (this.correlationInitializers != null)
            {
                foreach (CorrelationInitializer correlation in this.correlationInitializers)
                {
                    result.CorrelationInitializers.Add(correlation.Clone());
                }
            }

            // Update the send->IsOneWay
            if (this.Request != null)
            {
                this.Request.SetIsOneWay(false);
            }

            return result;
        }

        InArgument<CorrelationHandle> GetReplyHandleFromSend()
        {
            if (this.Request != null)
            {
                // If user has set AdditionalCorrelations, then we need to first look for requestReply Handle there
                foreach (CorrelationInitializer correlation in this.Request.CorrelationInitializers)
                {
                    RequestReplyCorrelationInitializer requestReplyCorrelation = correlation as RequestReplyCorrelationInitializer;

                    if (requestReplyCorrelation != null && requestReplyCorrelation.CorrelationHandle != null)
                    {
                        return requestReplyCorrelation.CorrelationHandle;
                    }
                }
            }
            
            return null;
        }
    }
}
