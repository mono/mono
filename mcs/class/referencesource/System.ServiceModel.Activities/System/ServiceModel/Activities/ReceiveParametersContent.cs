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
    public sealed class ReceiveParametersContent : ReceiveContent
    {
        string[] argumentNames;
        Type[] argumentTypes;

        public ReceiveParametersContent()
            : base()
        {
            this.Parameters = new OrderedDictionary<string, OutArgument>();
        }

        public ReceiveParametersContent(IDictionary<string, OutArgument> parameters)
            : base()
        {
            if (parameters == null)
            {
                throw FxTrace.Exception.ArgumentNull("parameters");
            }

            this.Parameters = new OrderedDictionary<string, OutArgument>(parameters);
        }

        public IDictionary<string, OutArgument> Parameters
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

        void ShredParameters()
        {
            // Turn Dictionary into ordered Argument arrays
            int argumentCount = this.Parameters.Count;
            this.argumentNames = new string[argumentCount];
            this.argumentTypes = new Type[argumentCount];

            int index = 0;
            foreach (KeyValuePair<string, OutArgument> pair in this.Parameters)
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
                    metadata.AddValidationError(SR.ReceiveParametersContentDoesNotSupportMessage(owner.DisplayName, argumentNames[index]));
                }
                index++;
            }

            if (!metadata.HasViolations)
            {
                foreach (KeyValuePair<string, OutArgument> pair in this.Parameters)
                {
                    RuntimeArgument newRuntimeArgument = new RuntimeArgument(pair.Key, pair.Value.ArgumentType, ArgumentDirection.Out);
                    metadata.Bind(pair.Value, newRuntimeArgument);
                    metadata.AddArgument(newRuntimeArgument);
                }
            }
        }

        internal override void ConfigureInternalReceive(InternalReceiveMessage internalReceiveMessage, out FromRequest requestFormatter)
        {
            requestFormatter = new FromRequest();
            foreach (KeyValuePair<string, OutArgument> parameter in this.Parameters)
            {
                requestFormatter.Parameters.Add(OutArgument.CreateReference(parameter.Value, parameter.Key));
            }
        }
        
        internal override void ConfigureInternalReceiveReply(InternalReceiveMessage internalReceiveMessage, out FromReply responseFormatter)
        {
            responseFormatter = new FromReply();
            foreach (KeyValuePair<string, OutArgument> parameter in this.Parameters)
            {
                responseFormatter.Parameters.Add(OutArgument.CreateReference(parameter.Value, parameter.Key));
            }
        }

        internal override void InferMessageDescription(OperationDescription operation, object owner, MessageDirection direction)
        {
            ContractInferenceHelper.CheckForDisposableParameters(operation, this.ArgumentTypes);

            string overridingAction = owner is Receive ? ((Receive)owner).Action : ((ReceiveReply)owner).Action;

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

                Fx.Assert(owner is Receive, "The parent of a ReceiveParametersContent with in-message can only be Receive!");
                overridingAction = ((Receive)owner).Action;

                isResponse = false;
            }
            else
            {
                Fx.Assert(targetOperation.Messages.Count == 2, "There must be exactly two MessageDescription objects for a two-way operation!");
                targetMessage = targetOperation.Messages[1];

                Fx.Assert(owner is ReceiveReply, "The parent of a ReceiveParametersContent with out-message can only be ReceiveReply!");
                overridingAction = ((ReceiveReply)owner).Action;

                isResponse = true;
            }

            ContractValidationHelper.ValidateAction(context, targetMessage, overridingAction, targetOperation, isResponse);
            if (ContractValidationHelper.IsReceiveParameterContent(targetOperation))
            {
                ContractValidationHelper.ValidateParametersContent(context, targetMessage, (IDictionary)this.Parameters, targetOperation, isResponse);
            }
            else
            {
                Constraint.AddValidationError(context, new ValidationError(SR.MisuseOfParameterContent(targetOperation.Name, targetOperation.DeclaringContract.Name))); 
            }
        }
    }
}
