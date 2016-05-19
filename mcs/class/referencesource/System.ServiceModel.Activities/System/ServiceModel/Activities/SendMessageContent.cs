//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Activities
{
    using System.Activities;
    using System.Activities.Validation;
    using System.ComponentModel;
    using System.Runtime;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.Windows.Markup;

    [ContentProperty("Message")]
    public sealed class SendMessageContent : SendContent
    {
        public SendMessageContent()
            : base()
        {
        }

        public SendMessageContent(InArgument message)
            : this()
        {
            this.Message = message;
        }

        public SendMessageContent(InArgument message, Type declaredMessageType)
            : this(message)
        {
            this.DeclaredMessageType = declaredMessageType;
        }

        // The value that is sent as the body of the message.
        // The type is derived from Message.Expression.ResultType. If the optional
        // DeclaredMessageType property is also specified, it is validated against
        // Message.Expression.ResultType.
        [DefaultValue(null)]
        public InArgument Message
        {
            get;
            set;
        }

        // Allows the type of the variable specified for Message to be a derived type from the type
        // on the message contract. This type specifies what the type is one the message contract.
        // If DeclaredMessageType is not specified, the type from the Message InArgument is used. 
        // The DeclaredMessageType must either be the same as the type of the InArgument Message, 
        // or it must be a base type of Message.
        [DefaultValue(null)]
        public Type DeclaredMessageType
        {
            get;
            set;
        }

        internal Type InternalDeclaredMessageType
        {
            get
            {
                if (this.DeclaredMessageType == null && this.Message != null)
                {
                    return this.Message.ArgumentType;
                }
                else
                {
                    return this.DeclaredMessageType;
                }
            }
        }

        internal override bool IsFault
        {
            get 
            {
                if (this.InternalDeclaredMessageType != null)
                {
                    return ContractInferenceHelper.ExceptionType.IsAssignableFrom(this.InternalDeclaredMessageType);
                }
                else
                {
                    return false;
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeDeclaredMessageType()
        {
            // don't serialize null
            if (this.DeclaredMessageType == null)
            {
                return false;
            }

            // if the expression type of Message matches the declared message type, no need to serialize duplicative information
            if (this.Message != null && this.DeclaredMessageType == this.Message.ArgumentType)
            {
                return false;
            }

            return true;
        }

        internal override void CacheMetadata(ActivityMetadata metadata, Activity owner, string operationName)
        {
            MessagingActivityHelper.FixMessageArgument(this.Message, ArgumentDirection.In, metadata);

            if (this.DeclaredMessageType != null)
            {
                if (this.Message == null && this.DeclaredMessageType != TypeHelper.VoidType)
                {
                    string errorOperationName = ContractValidationHelper.GetErrorMessageOperationName(operationName);
                    metadata.AddValidationError(SR.ValueCannotBeNull(owner.DisplayName, errorOperationName));
                }
                else if (this.Message != null && !this.DeclaredMessageType.IsAssignableFrom(this.Message.ArgumentType))
                {
                    string errorOperationName = ContractValidationHelper.GetErrorMessageOperationName(operationName);
                    metadata.AddValidationError(SR.ValueArgumentTypeNotDerivedFromValueType(owner.DisplayName, errorOperationName));
                }
            }
        }

        internal override void ConfigureInternalSend(InternalSendMessage internalSendMessage, out ToRequest requestFormatter)
        {
            if (this.InternalDeclaredMessageType == MessageDescription.TypeOfUntypedMessage)
            {
                // Value is a Message, do not use the formatter but directly pass it to InternalSendMessage
                internalSendMessage.Message = new InArgument<Message>(context => ((InArgument<Message>)this.Message).Get(context));
                requestFormatter = null;
            }
            else
            {
                requestFormatter = new ToRequest();
                if (this.Message != null)
                {
                    requestFormatter.Parameters.Add(InArgument.CreateReference(this.Message, "Message"));
                }
            }
        }

        internal override void ConfigureInternalSendReply(InternalSendMessage internalSendMessage, out ToReply responseFormatter)
        {
            if (this.InternalDeclaredMessageType == MessageDescription.TypeOfUntypedMessage)
            {
                internalSendMessage.Message = new InArgument<Message>(context => ((InArgument<Message>)this.Message).Get(context));
                responseFormatter = null;
            }
            else
            {
                responseFormatter = new ToReply();

                // WCF rule dictates that MessageContract must be bound to ReturnValue, not Parameters
                if (MessageBuilder.IsMessageContract(this.InternalDeclaredMessageType))
                {
                    responseFormatter.Result = InArgument.CreateReference(this.Message, "Message");
                }
                else if (this.Message != null)
                {
                    responseFormatter.Parameters.Add(InArgument.CreateReference(this.Message, "Message"));
                }
            }
        }

        internal override void InferMessageDescription(OperationDescription operation, object owner, MessageDirection direction)
        {
            ContractInferenceHelper.CheckForDisposableParameters(operation, this.InternalDeclaredMessageType);

            string overridingAction = null;
            SerializerOption serializerOption = SerializerOption.DataContractSerializer;
            Send send = owner as Send;
            if (send != null)
            {
                overridingAction = send.Action;
                serializerOption = send.SerializerOption;
            }
            else
            {
                SendReply sendReply = owner as SendReply;
                Fx.Assert(sendReply != null, "The owner of SendMessageContent can only be Send or SendReply!");
                overridingAction = sendReply.Action;
                serializerOption = sendReply.Request.SerializerOption;
            }

            if (direction == MessageDirection.Input)
            {
                ContractInferenceHelper.AddInputMessage(operation, overridingAction, this.InternalDeclaredMessageType, serializerOption);
            }
            else
            {
                ContractInferenceHelper.AddOutputMessage(operation, overridingAction, this.InternalDeclaredMessageType, serializerOption);
            }
        }
        
        internal override void ValidateContract(NativeActivityContext context, OperationDescription targetOperation, object owner, MessageDirection direction)
        {
            MessageDescription targetMessage;
            string overridingAction;
            bool isResponse;
            SerializerOption serializerOption;

            if (direction == MessageDirection.Input)
            {
                Fx.Assert(targetOperation.Messages.Count >= 1, "There must be at least one MessageDescription in an OperationDescription!");
                targetMessage = targetOperation.Messages[0];

                Send send = owner as Send;
                Fx.Assert(send != null, "The parent of a SendMessageContent with in-message can only be Send!");

                overridingAction = send.Action;
                serializerOption = send.SerializerOption;

                isResponse = false;
            }
            else
            {
                Fx.Assert(targetOperation.Messages.Count == 2, "There must be exactly two MessageDescription objects for a two-way operation!");
                targetMessage = targetOperation.Messages[1];

                SendReply sendReply = owner as SendReply;
                Fx.Assert(sendReply != null, "The parent of a SendMessageContent with out-message can only be SendReply!");
                Fx.Assert(sendReply.Request != null, "SendReply.Request should not be null by now!");
                overridingAction = sendReply.Action;
                serializerOption = sendReply.Request.SerializerOption;

                isResponse = true;
            }

            if (!this.IsFault)
            {
                ContractValidationHelper.ValidateAction(context, targetMessage, overridingAction, targetOperation, isResponse);
                if (ContractValidationHelper.IsSendParameterContent(targetOperation))
                {
                    Constraint.AddValidationError(context, new ValidationError(SR.MisuseOfMessageContent(targetOperation.Name, targetOperation.DeclaringContract.Name))); 
                }
                else
                {
                    ContractValidationHelper.ValidateMessageContent(context, targetMessage, this.InternalDeclaredMessageType,
                        serializerOption, targetOperation, isResponse);
                }
            }
            else
            {
                Fx.Assert(this.InternalDeclaredMessageType != null, "IsFault returns true only when argument is of exception type!");
                Type argumentType = this.InternalDeclaredMessageType;

                if (argumentType.IsGenericType && argumentType.GetGenericTypeDefinition() == ContractInferenceHelper.FaultExceptionType)
                {
                    Type faultType = argumentType.GetGenericArguments()[0];
                    ContractValidationHelper.ValidateFault(context, targetOperation, overridingAction, faultType);
                }
            }
        }

    }
}
