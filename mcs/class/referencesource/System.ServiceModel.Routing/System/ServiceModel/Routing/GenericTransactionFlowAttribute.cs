//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Routing
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;

    sealed class GenericTransactionFlowAttribute : Attribute, IOperationBehavior
    {
        TransactionFlowAttribute transactionFlowAttribute;

        public GenericTransactionFlowAttribute(TransactionFlowOption flowOption)
        {
            this.transactionFlowAttribute = new TransactionFlowAttribute(flowOption);
        }

        void IOperationBehavior.AddBindingParameters(OperationDescription operationDescription, BindingParameterCollection bindingParameters)
        {
            ((IOperationBehavior)this.transactionFlowAttribute).AddBindingParameters(operationDescription, bindingParameters);
        }

        void IOperationBehavior.ApplyClientBehavior(OperationDescription operationDescription, ClientOperation clientOperation)
        {
            ((IOperationBehavior)this.transactionFlowAttribute).ApplyClientBehavior(operationDescription, clientOperation);
        }

        void IOperationBehavior.ApplyDispatchBehavior(OperationDescription operationDescription, DispatchOperation dispatchOperation)
        {
            ((IOperationBehavior)this.transactionFlowAttribute).ApplyDispatchBehavior(operationDescription, dispatchOperation);
        }

        void IOperationBehavior.Validate(OperationDescription operationDescription)
        {
            ((IOperationBehavior)this.transactionFlowAttribute).Validate(operationDescription);
        }
    }
}
