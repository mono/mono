//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Description
{
    using System;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;
    using System.Collections.Generic;

    public class ClientViaBehavior : IEndpointBehavior
    {
        Uri uri;
        
        public ClientViaBehavior(Uri uri)
        {
            if (uri == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("uri");
                
            this.uri = uri;
        }
        
        public Uri Uri
        {
            get { return this.uri; }
            set
            {
                if (value == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                    
                this.uri = value;
            }
        }
        
        void IEndpointBehavior.Validate(ServiceEndpoint serviceEndpoint)
        {
        }

        void IEndpointBehavior.AddBindingParameters(ServiceEndpoint serviceEndpoint, BindingParameterCollection bindingParameters)
        {
        }

        void IEndpointBehavior.ApplyDispatchBehavior(ServiceEndpoint serviceEndpoint, EndpointDispatcher endpointDispatcher)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                SR.GetString(SR.SFXEndpointBehaviorUsedOnWrongSide, typeof(ClientViaBehavior).Name)));
        }

        void IEndpointBehavior.ApplyClientBehavior(ServiceEndpoint serviceEndpoint, ClientRuntime behavior)
        {
            if (behavior == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("behavior");
            }

            behavior.Via = this.Uri;
        }
    }
}
