//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Activities
{
    using System.Activities;
    using System.Activities.Validation;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Runtime.Collections;
    using System.ServiceModel.Description;
    using System.Windows.Markup;

    [ContentProperty("Parameters")]
    public sealed class SendParametersContent : SendContent
    {
        string[] argumentNames;
        Type[] argumentTypes;

        public SendParametersContent()
            : base()
        {
            this.Parameters = new OrderedDictionary<string, InArgument>();
        }

        public SendParametersContent(IDictionary<string, InArgument> parameters)
            : base()
        {
            if (parameters == null)
            {
                throw FxTrace.Exception.ArgumentNull("parameters");
            }
         
            this.Parameters = new OrderedDictionary<string, InArgument>(parameters);
        }

        public IDictionary<string, InArgument> Parameters
        {
            get;
            private set;
        }

        internal string[] ArgumentNames
        {
            get
            {
                if (this.argumentNames == null)
                {
                    ShredParameters();
                }
                return this.argumentNames;
            }
        }

        internal Type[] ArgumentTypes
        {
            get
            {
                if (this.argumentTypes == null)
                {
                    ShredParameters();
                }
                return this.argumentTypes;
            }
        }


        internal override bool IsFault
        {
            get 
            {
                if (this.ArgumentTypes.Length == 1)
                {
                    return ContractInferenceHelper.ExceptionType.IsAssignableFrom(this.ArgumentTypes[0]);
                }
                else
                {
                    return false;
                }
            }
        }

        void ShredParameters()
        {
            // Turn Dictionary into ordered Argument arrays
            int argumentCount = this.Parameters.Count;
            this.argumentNames = new string[argumentCount];
            this.argumentTypes = new Type[argumentCount];

            int index = 0;
            foreach (KeyValuePair<string, InArgument> pair in this.Parameters)
            {
                this.argumentNames[index] = pair.Key;
                if (pair.Value != null)
                {
                    this.argumentTypes[index] = pair.Value.ArgumentType;
                }
                index++;
            }
        }

        internal override void CacheMetadata(ActivityMetadata metadata, Activity owner, string operationName)
        {
            // force a shred for every CacheMetadata call
            ShredParameters();

            int index = 0;
            foreach (Type argumentType in this.argumentTypes)
            {
                if (argumentType == null || argumentType == TypeHelper.VoidType)
                {
                    metadata.AddValidationError(SR.ArgumentCannotHaveNullOrVoidType(owner.DisplayName, argumentNames[index]));
                }
                if (argumentType == MessageDescription.TypeOfUntypedMessage || MessageBuilder.IsMessageContract(argumentType))
                {
                    metadata.AddValidationError(SR.SendParametersContentDoesNotSupportMessage(owner.DisplayName, argumentNames[index]));
                }
                index++;
            }

            if (!metadata.HasViolations)
            {
                foreach (KeyValuePair<string, InArgument> pair in this.Parameters)
                {
                    RuntimeArgument newRuntimeArgument = new RuntimeArgument(pair.Key, pair.Value.ArgumentType, ArgumentDirection.In);
                    metadata.Bind(pair.Value, newRuntimeArgument);
                    metadata.AddArgument(newRuntimeArgument);
                }
            }
        }

        internal override void ConfigureInternalSend(InternalSendMessage internalSendMessage, out ToRequest requestFormatter)
        {
            //Zero or more arguments
            requestFormatter = new ToRequest();

            foreach (KeyValuePair<string, InArgument> parameter in this.Parameters)
            {
                requestFormatter.Parameters.Add(InArgument.CreateReference(parameter.Value, parameter.Key));
            }
        }

        internal override void ConfigureInternalSendReply(InternalSendMessage internalSendMessage, out ToReply responseFormatter)
        {
            responseFormatter = new ToReply();

            foreach (KeyValuePair<string, InArgument> parameter in this.Parameters)
            {
                responseFormatter.Parameters.Add(InArgument.CreateReference(parameter.Value, parameter.Key));
            }
        }

        internal override void InferMessageDescription(OperationDescription operation, object owner, MessageDirection direction)
        {
            ContractInferenceHelper.CheckForDisposableParameters(operation, this.ArgumentTypes);

            string overridingAction = owner is Send ? ((Send)owner).Action : ((SendReply)owner).Action;

            if (direction == MessageDirection.Input)
            {
                ContractInferenceHelper.AddInputMessage(operation, overridingAction, this.ArgumentNames, this.ArgumentTypes);
            }
            else
            {
                ContractInferenceHelper.AddOutputMessage(operation, overridingAction, this.ArgumentNames, this.ArgumentTypes);
            }
        }

        internal override void ValidateContract(NativeActivityContext context, OperationDescription targetOperation, object owner, MessageDirection direction)
        {
            MessageDescription targetMessage;
            string overridingAction;
            bool isResponse;

            if (direction == MessageDirection.Input)
            {
                Fx.Assert(targetOperation.Messages.Count >= 1, "There must be at least one MessageDescription in an OperationDescription!");
                targetMessage = targetOperation.Messages[0];

                Fx.Assert(owner is Send, "The parent of a SendParametersContent with in-message can only be Send!");
                overridingAction = ((Send)owner).Action;

                isResponse = false;
            }
            else
            {
                Fx.Assert(targetOperation.Messages.Count == 2, "There must be exactly two MessageDescription objects for a two-way operation!");
                targetMessage = targetOperation.Messages[1];

                Fx.Assert(owner is SendReply, "The parent of a SendParametersContent with out-message can only be SendReply!");
                overridingAction = ((SendReply)owner).Action;

                isResponse = true;
            }

            if (!this.IsFault)
            {
                ContractValidationHelper.ValidateAction(context, targetMessage, overridingAction, targetOperation, isResponse);
                if (ContractValidationHelper.IsSendParameterContent(targetOperation))
                {
                    ContractValidationHelper.ValidateParametersContent(context, targetMessage, (IDictionary)this.Parameters, targetOperation, isResponse);
                }
                else
                {
                    Constraint.AddValidationError(context, new ValidationError(SR.MisuseOfParameterContent(targetOperation.Name, targetOperation.DeclaringContract.Name))); 
                }
            }
            else
            {
                Fx.Assert(this.argumentTypes != null && this.argumentTypes.Length == 1, "Exception should be the only parameter in SendFault!");
                Type argumentType = this.argumentTypes[0];

                if (argumentType.IsGenericType && argumentType.GetGenericTypeDefinition() == ContractInferenceHelper.FaultExceptionType)
                {
                    Type faultType = argumentType.GetGenericArguments()[0];
                    ContractValidationHelper.ValidateFault(context, targetOperation, overridingAction, faultType);
                }
            }
        }
    }
}
