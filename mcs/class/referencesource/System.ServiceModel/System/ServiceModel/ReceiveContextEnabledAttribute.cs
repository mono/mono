//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.ServiceModel
{
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Description;

    [AttributeUsage(ServiceModelAttributeTargets.OperationBehavior)]
    public sealed class ReceiveContextEnabledAttribute : Attribute, IOperationBehavior
    {
        public ReceiveContextEnabledAttribute()
        {

        }

        public bool ManualControl
        {
            get;
            set;
        }

        public void Validate(OperationDescription operationDescription)
        {

        }

        public void ApplyDispatchBehavior(OperationDescription operationDescription, DispatchOperation dispatchOperation)
        {
            if (operationDescription == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("operationDescription");
            }

            if (dispatchOperation == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("dispatchOperation");
            }

            if (this.ManualControl)
            {
                dispatchOperation.ReceiveContextAcknowledgementMode = ReceiveContextAcknowledgementMode.ManualAcknowledgement;
            }
            else
            {
                dispatchOperation.ReceiveContextAcknowledgementMode = ReceiveContextAcknowledgementMode.AutoAcknowledgeOnRPCComplete;
            }
        }

        public void ApplyClientBehavior(OperationDescription operationDescription, ClientOperation clientOperation)
        {

        }

        public void AddBindingParameters(OperationDescription operationDescription, BindingParameterCollection bindingParameters)
        {

        }
    }
}
