//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Activities
{
    using System.Activities;
    using System.Activities.Statements;
    using System.Activities.Validation;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;
    using System.Windows.Markup;
    using System.Xaml;
    using System.Xml.Linq;

    // Used to send the reply in a request/reply messaging pattern. Must be paired with
    // a corresponding Receive activity.
    [ContentProperty("Content")]
    public sealed class SendReply : Activity
    {
        Collection<CorrelationInitializer> correlationInitializers;
        ToReply responseFormatter;
        InternalSendMessage internalSend;

        public SendReply()
            : base()
        {
            base.Implementation = () =>
            {
                // if CacheMetadata isn't called, bail early
                if (this.internalSend == null)
                {
                    return null;
                }

                if (this.responseFormatter == null) // untyped message
                {
                    return this.internalSend;
                }
                else
                {
                    Variable<Message> response = new Variable<Message> { Name = "ResponseMessage" };
                    this.responseFormatter.Message = new OutArgument<Message>(response);
                    this.internalSend.Message = new InArgument<Message>(response);

                    // This is used to clear out the response variable
                    this.internalSend.MessageOut = new OutArgument<Message>(response);

                    return new Sequence
                    {
                        Variables = { response },
                        Activities = 
                        { 
                            this.responseFormatter,
                            this.internalSend
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

        // Reference to the Receive activity that is responsible for receiving the Request part of the
        // request/reply pattern. This cannot be null.
        [DefaultValue(null)]
        public Receive Request
        {
            get;
            set;
        }

        [DefaultValue(null)]
        public string Action
        {
            get;
            set;
        }


        // Additional correlations allow situations where a "session" involves multiple
        // messages between two workflow instances.
        [DefaultValue(null)]
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

        // used to ensure that the other side gets ALO behavior
        [DefaultValue(false)]
        public bool PersistBeforeSend
        {
            get;
            set;
        }

        protected override void CacheMetadata(ActivityMetadata metadata)
        {
            if (this.Request == null)
            {
                metadata.AddValidationError(SR.SendReplyRequestCannotBeNull(this.DisplayName));
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
                this.internalSend = CreateInternalSend();
                this.InternalContent.ConfigureInternalSendReply(this.internalSend, out this.responseFormatter);

                InArgument<CorrelationHandle> requestReplyHandleFromReceive = GetReplyHandleFromReceive();
                if (requestReplyHandleFromReceive != null)
                {
                    InArgument<CorrelationHandle> internalSendCorrelatesWith = MessagingActivityHelper.CreateReplyCorrelatesWith(requestReplyHandleFromReceive);
                    
                    RuntimeArgument internalSendCorrelatesWithArgument = new RuntimeArgument("InternalSendCorrelatesWith", Constants.CorrelationHandleType, ArgumentDirection.In);
                    metadata.Bind(internalSendCorrelatesWith, internalSendCorrelatesWithArgument);
                    metadata.AddArgument(internalSendCorrelatesWithArgument);

                    this.internalSend.CorrelatesWith = (InArgument<CorrelationHandle>)InArgument.CreateReference(internalSendCorrelatesWith, "InternalSendCorrelatesWith");

                    if (this.responseFormatter != null)
                    {
                        InArgument<CorrelationHandle> responseFormatterCorrelatesWith = MessagingActivityHelper.CreateReplyCorrelatesWith(requestReplyHandleFromReceive);

                        RuntimeArgument responseFormatterCorrelatesWithArgument = new RuntimeArgument("ResponseFormatterCorrelatesWith", Constants.CorrelationHandleType, ArgumentDirection.In);
                        metadata.Bind(responseFormatterCorrelatesWith, responseFormatterCorrelatesWithArgument);
                        metadata.AddArgument(responseFormatterCorrelatesWithArgument);

                        responseFormatter.CorrelatesWith = (InArgument<CorrelationHandle>)InArgument.CreateReference(responseFormatterCorrelatesWith, "ResponseFormatterCorrelatesWith");
                    }
                }
            }
            else
            {
                this.internalSend = null;
                this.responseFormatter = null;
            }

            // We don't have any imported children despite referencing the Request
            metadata.SetImportedChildrenCollection(new Collection<Activity>());
        }

        // responseFormatter is null if we have an untyped message situation
        InternalSendMessage CreateInternalSend()
        {
            InternalSendMessage result = new InternalSendMessage
            {
                IsSendReply = true, //indicates that we are sending a reply(server-side)
                ShouldPersistBeforeSend = this.PersistBeforeSend,
                OperationName = this.Request.OperationName, //need this for displaying error messages
                OwnerDisplayName = this.DisplayName
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

        internal void SetFormatter(IDispatchMessageFormatter formatter)
        {
            if (this.responseFormatter != null)
            {
                this.responseFormatter.Formatter = formatter;
            }
        }

        internal void SetFaultFormatter(IDispatchFaultFormatter faultFormatter, bool includeExceptionDetailInFaults)
        {
            Fx.Assert(this.responseFormatter != null, "ToReply cannot be null!");

            this.responseFormatter.FaultFormatter = faultFormatter;
            this.responseFormatter.IncludeExceptionDetailInFaults = includeExceptionDetailInFaults;
        }

        internal void SetContractName(XName contractName)
        {
            Fx.Assert(this.internalSend != null, "InternalSend cannot be null!");

            this.internalSend.ServiceContractName = contractName;
        }

        InArgument<CorrelationHandle> GetReplyHandleFromReceive()
        {
            if (this.Request != null)
            {
                //if the user has set AdditionalCorrelations, then we need to first look for requestReply Handle there
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

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.AvoidOutParameters,
            Justification = "This is the design for this public interface, we want to distinguish between SendReply for faults and SendReply for Return parameter therefore need the second output")]
        public static SendReply FromOperationDescription(OperationDescription operation, out IEnumerable<SendReply> faultReplies)
        {
            if (operation == null)
            {
                throw FxTrace.Exception.ArgumentNull("operation", "OperationDescription cannot be null");
            }

            bool contentIsParameter = false;
            bool contentIsMessage = false; 
            bool isSendContentEmpty = false;
            
            MessageDescription message;

            faultReplies = null; 
            List<SendReply> faultRepliesList = new List<SendReply>();
            SendReply reply = null;
            
            if (operation.IsOneWay)
            {
                return null;
            }

            if (operation.Messages.Count > 1)
            {
                reply = new SendReply();
                reply.Action = operation.Messages[1].Action;
                reply.DisplayName = operation.Name + "SendReply";

                message = operation.Messages[1];

                contentIsParameter = false;

                if (message.MessageType == null)
                {
                    if (message.Body.ReturnValue != null && message.Body.ReturnValue.Type != typeof(void))
                    {
                        if (!message.Body.ReturnValue.Type.IsAssignableFrom(typeof(System.ServiceModel.Channels.Message)))
                        {
                            contentIsParameter = true;
                        }

                        isSendContentEmpty = true;
                    }
                }

                if (message.MessageType == null)
                {
                    if (message.Body.Parts != null)
                    {
                        if (message.Body.Parts.Count > 0)
                        {
                            MessagePartDescriptionCollection parts = message.Body.Parts;
                            foreach (MessagePartDescription messagePart in parts)
                            {
                                if (messagePart.Index >= 0)
                                {
                                    contentIsParameter = true;
                                    break;
                                }
                                if (!messagePart.Type.IsAssignableFrom(typeof(System.ServiceModel.Channels.Message)))
                                {
                                    contentIsParameter = true;
                                }
                            }
                            isSendContentEmpty = true;
                        }
                    }
                }

                if (isSendContentEmpty)
                {
                    if (contentIsParameter)
                    {
                        SendParametersContent content = new SendParametersContent();
                        if (message.Direction == MessageDirection.Output
                            && message.Body.ReturnValue != null
                            && message.Body.ReturnValue.Type != typeof(void))
                        {
                            Argument returnArgument = InArgument.Create(message.Body.ReturnValue.Type, ArgumentDirection.In);
                            content.Parameters.Add(message.Body.ReturnValue.Name, (InArgument)returnArgument);
                        }

                        if (message.Direction == MessageDirection.Output && message.Body.Parts != null)
                        {
                            foreach (MessagePartDescription messagePart in message.Body.Parts)
                            {
                                Argument inArgument = InArgument.Create(messagePart.Type, ArgumentDirection.In);
                                content.Parameters.Add(messagePart.Name, (InArgument)(inArgument));
                            }
                        }
                        contentIsMessage = false;
                        reply.Content = content;
                    }
                    else
                    {
                        // We must have an untyped message contract
                        // 
                        SendMessageContent content = new SendMessageContent();
                        if (message.Direction == MessageDirection.Output)
                        {
                            content.DeclaredMessageType = message.Body.ReturnValue.Type;
                            Argument inArgument = InArgument.Create(content.DeclaredMessageType, ArgumentDirection.In);
                            content.Message = (InArgument)(inArgument);
                        }
                        contentIsMessage = true; 
                        reply.Content = content;
                    }
                }
                else
                {
                    if (message.MessageType != null && message.MessageType.IsDefined(typeof(MessageContractAttribute), false))
                    {
                        SendMessageContent sendMessageContent;
                        sendMessageContent = new SendMessageContent();
                        sendMessageContent.DeclaredMessageType = message.MessageType;
                        Argument inArgument = InArgument.Create(sendMessageContent.DeclaredMessageType, ArgumentDirection.In);
                        sendMessageContent.Message = (InArgument)(inArgument);
                        reply.Content = sendMessageContent;
                        contentIsMessage = true; 
                    }
                    else if (operation.Messages[0].MessageType != null)
                    {
                        reply.Content = new SendMessageContent();
                        contentIsMessage = true; 
                    }
                    else if (operation.Messages[0].Body.Parts != null
                        && operation.Messages[0].Body.Parts.Count == 1
                        && operation.Messages[0].Body.Parts[0].Type.IsAssignableFrom(typeof(System.ServiceModel.Channels.Message)))
                    {
                        reply.Content = new SendMessageContent();
                        contentIsMessage = true; 
                    }
                    else
                    {
                        reply.Content = new SendParametersContent();
                        contentIsMessage = false; 
                    }
                }
            }

            if (operation.Faults != null)
            {
                foreach (FaultDescription faultDescription in operation.Faults)
                {
                    faultRepliesList.Add(BuildFaultReplies(faultDescription, contentIsMessage));
                }
            }

            faultReplies = faultRepliesList;

            return reply;
        }

        static SendReply BuildFaultReplies(FaultDescription faultDescription, bool isMessageContract)
        {
            Fx.Assert(faultDescription != null, "fault Description cannot be null");
            if (faultDescription.DetailType == TypeHelper.VoidType || faultDescription.DetailType == null)
            {
                throw FxTrace.Exception.ArgumentNullOrEmpty("FaultDescription.DetailType");
            }

            SendReply faultReply = new SendReply()
            {
                DisplayName = faultDescription.Name + "SendFaultReply",
                Action = faultDescription.Action,
            };

            Type[] substitute = { faultDescription.DetailType };
            Type faultType = typeof(FaultException<>).MakeGenericType(substitute);
            if (isMessageContract)
            {
                faultReply.Content = new SendMessageContent()
                {
                    Message = (InArgument)(InArgument.Create(faultType, ArgumentDirection.In)),
                };
            }
            else
            {
                InArgument argument = (InArgument)(InArgument.Create(faultType, ArgumentDirection.In));
                SendParametersContent faultReplyParameterContent = new SendParametersContent();
                faultReplyParameterContent.Parameters.Add(faultDescription.Name, argument);
                faultReply.Content = faultReplyParameterContent;
            }

            return faultReply;
        }
    }
}
