//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Activities.Description
{
    using System.Collections.ObjectModel;
    using System.Runtime;
    using System.ServiceModel.Activities;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;

    class WorkflowFormatterBehavior : IOperationBehavior
    {
        IDispatchMessageFormatter formatter;
        IDispatchFaultFormatter faultFormatter;
        Collection<Receive> receives;

        public Collection<Receive> Receives
        {
            get
            {
                if (this.receives == null)
                {
                    this.receives = new Collection<Receive>();
                }
                return this.receives;
            }
        }

        public void ApplyClientBehavior(OperationDescription operationDescription, System.ServiceModel.Dispatcher.ClientOperation clientOperation)
        {
            throw FxTrace.Exception.AsError(new NotImplementedException());
        }

        public void ApplyDispatchBehavior(OperationDescription operationDescription, DispatchOperation dispatchOperation)
        {
            Fx.Assert(operationDescription != null, "OperationDescription cannot be null!");
            Fx.Assert(dispatchOperation != null, "DispatchOperation cannot be null!");

            if (dispatchOperation.Formatter == null)
            {
                return;
            }

            this.formatter = dispatchOperation.Formatter;
            this.faultFormatter = dispatchOperation.FaultFormatter;
            if (this.receives != null)
            {
                foreach (Receive receive in this.receives)
                {
                    receive.SetFormatter(this.formatter, this.faultFormatter, dispatchOperation.IncludeExceptionDetailInFaults);
                }
            }

            // Remove operation formatter from dispatch runtime
            dispatchOperation.Formatter = null;
            dispatchOperation.DeserializeRequest = false;
            dispatchOperation.SerializeReply = false;
        }

        public void AddBindingParameters(OperationDescription operationDescription, BindingParameterCollection bindingParameters)
        {
        }

        public void Validate(OperationDescription operationDescription)
        {
        }
    }
}
