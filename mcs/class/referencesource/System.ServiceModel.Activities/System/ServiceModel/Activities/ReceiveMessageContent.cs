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
    public sealed class ReceiveMessageContent : ReceiveContent
    {
        public ReceiveMessageContent()
            : base()
        {
        }

        public ReceiveMessageContent(OutArgument message)
            : this()
        {
            this.Message = message;
        }

        public ReceiveMessageContent(OutArgument message, Type declaredMessageType)
            : this(message)
        {
            this.DeclaredMessageType = declaredMessageType;
        }

        // The value that is received as the body of the message.
        // The type is derived from Message.Expression.ResultType. If the optional
        // DeclaredMessageType property is also specified, it is validated against
        // Message.Expression.ResultType.
        [DefaultValue(null)]
        public OutArgument Message
        {
            get;
            set;
        }

        // Allows the type of the variable specified for Message to be a derived type from the type
        // on the message contract. This type specifies what the type is one the message contract.
        // If DeclaredMessageType is not specified, the type from the Message OutArgument is used. 
        // The DeclaredMessageType must either be the same as the type of the OutArgument Message, 
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
            MessagingActivityHelper.FixMessageArgument(this.Message, ArgumentDirection.Out, metadata);

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

        internal override void ConfigureInternalReceive(InternalReceiveMessage internalReceiveMessage, out FromRequest requestFormatter)
        {
            if (this.InternalDeclaredMessageType == MessageDescription.TypeOfUntypedMessage)
            {
                internalReceiveMessage.Message = new OutArgument<Message>(context => ((OutArgument<Message>)this.Message).Get(context));
                requestFormatter = null;
            }
            else
            {
                requestFormatter = new FromRequest();
                if (this.Message != null)
                {
                    requestFormatter.Parameters.Add(OutArgument.CreateReference(this.Message, "Message"));
                }
            }
        }

        internal override void ConfigureInternalReceiveReply(InternalReceiveMessage internalReceiveMessage, out FromReply responseFormatter)
        {
            if (this.InternalDeclaredMessageType == MessageDescription.TypeOfUntypedMessage)
            {
                // do not use the formatter in this case
                internalReceiveMessage.Message = new OutArgument<Message>(context => ((OutArgument<Message>)this.Message).Get(context));
                responseFormatter = null;
            }
            else
            {
                responseFormatter = new FromReply();

                // WCF rule dictates that MessageContract must be bound to ReturnValue, not Parameters
                if (MessageBuilder.IsMessageContract(this.InternalDeclaredMessageType))
                {
                    responseFormatter.Result = OutArgument.CreateReference(this.Message, "Message");
                }
                else if (this.Message != null)
                {
                    responseFormatter.Parameters.Add(OutArgument.CreateReference(this.Message, "Message"));
                }
            }
        }

        internal override void InferMessageDescription(OperationDescription operation, object owner, MessageDirection direction)
        {
            ContractInferenceHelper.CheckForDisposableParameters(operation, this.InternalDeclaredMessageType);

            string overridingAction = null;
            SerializerOption serializerOption = SerializerOption.DataContractSerializer;
            Receive receive = owner as Receive;
            if (receive != null)
            {
                overridingAction = receive.Action;
                serializerOption = receive.SerializerOption;
            }
            else
            {
                ReceiveReply receiveReply = owner as ReceiveReply;
                Fx.Assert(receiveReply != null, "The owner of ReceiveMessageContent can only be Receive or ReceiveReply!");
                overridingAction = receiveReply.Action;
                serializerOption = receiveReply.Request.SerializerOption;
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

                Receive receive = owner as Receive;
                Fx.Assert(receive != null, "The parent of a ReceiveMessageContent with in-message can only be Receive!");

                overridingAction = receive.Action;
                serializerOption = receive.SerializerOption;

                isResponse = false;
            }
            else
            {
                Fx.Assert(targetOperation.Messages.Count == 2, "There must be exactly two MessageDescription objects for a two-way operation!");
                targetMessage = targetOperation.Messages[1];

                ReceiveReply receiveReply = owner as ReceiveReply;
                Fx.Assert(receiveReply != null, "The parent of a ReceiveMessageContent with out-message can only be ReceiveReply!");
                Fx.Assert(receiveReply.Request != null, "ReceiveReply.Request should not be null by now!");
                overridingAction = receiveReply.Action;
                serializerOption = receiveReply.Request.SerializerOption;

                isResponse = true;
            }

            ContractValidationHelper.ValidateAction(context, targetMessage, overridingAction, targetOperation, isResponse);
            if (ContractValidationHelper.IsReceiveParameterContent(targetOperation))
            {
                Constraint.AddValidationError(context, new ValidationError(SR.MisuseOfMessageContent(targetOperation.Name, targetOperation.DeclaringContract.Name))); 
            }
            else
            {
                ContractValidationHelper.ValidateMessageContent(context, targetMessage, this.InternalDeclaredMessageType,
                   serializerOption, targetOperation, isResponse);
            }
        }

    }
}
