//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Description
{
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Channels;

    public class TransactedBatchingBehavior : IEndpointBehavior
    {
        int maxBatchSize;

        public TransactedBatchingBehavior(int maxBatchSize)
        {
            if (maxBatchSize < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("maxBatchSize", maxBatchSize,
                                                    SR.GetString(SR.ValueMustBeNonNegative)));

            }
            this.maxBatchSize = maxBatchSize;
        }

        public int MaxBatchSize
        {
            get { return this.maxBatchSize; }
            set
            {
                if (value < 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value,
                                                    SR.GetString(SR.ValueMustBeNonNegative)));
                }
                this.maxBatchSize = value;
            }
        }

        void IEndpointBehavior.Validate(ServiceEndpoint serviceEndpoint)
        {
            BindingElementCollection bindingElements = serviceEndpoint.Binding.CreateBindingElements();
            bool transactedElementFound = false;

            foreach (BindingElement bindingElement in bindingElements)
            {
                ITransactedBindingElement txElement = bindingElement as ITransactedBindingElement;
                if (null != txElement && txElement.TransactedReceiveEnabled)
                {
                    transactedElementFound = true;
                    break;
                }
            }
            
            if (! transactedElementFound)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SfxTransactedBindingNeeded)));
        }

        void IEndpointBehavior.AddBindingParameters(ServiceEndpoint serviceEndpoint, BindingParameterCollection bindingParameters)
        {
        }

        void IEndpointBehavior.ApplyDispatchBehavior(ServiceEndpoint serviceEndpoint, EndpointDispatcher endpointDispatcher)
        {
            if (endpointDispatcher.DispatchRuntime.ReleaseServiceInstanceOnTransactionComplete)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxNoBatchingForReleaseOnComplete)));
            if (serviceEndpoint.Contract.SessionMode == SessionMode.Required)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxNoBatchingForSession)));
        }

        void IEndpointBehavior.ApplyClientBehavior(ServiceEndpoint serviceEndpoint, ClientRuntime behavior)
        {
            if (serviceEndpoint.Contract.SessionMode == SessionMode.Required)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxNoBatchingForSession)));
            behavior.CallbackDispatchRuntime.ChannelDispatcher.MaxTransactedBatchSize = this.MaxBatchSize;
        }
    }
}
