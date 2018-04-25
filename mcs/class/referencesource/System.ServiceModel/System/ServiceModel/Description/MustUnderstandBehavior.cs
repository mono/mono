//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Description
{
    using System;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;
    using System.Collections.Generic;

    public class MustUnderstandBehavior : IEndpointBehavior
    {
        bool validateMustUnderstand;

        public MustUnderstandBehavior(bool validate)
        {
            this.validateMustUnderstand = validate;
        }

        public bool ValidateMustUnderstand
        {
            get { return this.validateMustUnderstand; }
            set { this.validateMustUnderstand = value; }
        }


        void IEndpointBehavior.Validate(ServiceEndpoint serviceEndpoint)
        {
        }

        void IEndpointBehavior.AddBindingParameters(ServiceEndpoint serviceEndpoint, BindingParameterCollection bindingParameters)
        {
        }

        void IEndpointBehavior.ApplyDispatchBehavior(ServiceEndpoint serviceEndpoint, EndpointDispatcher endpointDispatcher)
        {
            if (endpointDispatcher == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("endpointDispatcher"));

            endpointDispatcher.DispatchRuntime.ValidateMustUnderstand = this.ValidateMustUnderstand;
        }

        void IEndpointBehavior.ApplyClientBehavior(ServiceEndpoint serviceEndpoint, ClientRuntime behavior)
        {
            if (behavior == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("behavior"));

            behavior.ValidateMustUnderstand = this.ValidateMustUnderstand;
        }
    }
}
