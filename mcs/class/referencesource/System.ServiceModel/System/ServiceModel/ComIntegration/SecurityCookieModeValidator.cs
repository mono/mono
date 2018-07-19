//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ServiceModel;
    using System.ServiceModel.Security.Tokens;

    class SecurityCookieModeValidator : IServiceBehavior
    {
        void CheckForCookie(SecurityTokenParameters tokenParameters, ServiceEndpoint endpoint)
        {
            bool cookie = false;
            SecureConversationSecurityTokenParameters sc = tokenParameters as SecureConversationSecurityTokenParameters;
            if (sc != null && sc.RequireCancellation == false)
                cookie = true;
            SspiSecurityTokenParameters sspi = tokenParameters as SspiSecurityTokenParameters;
            if (sspi != null && sspi.RequireCancellation == false)
                cookie = true;
            SspiSecurityTokenParameters ssl = tokenParameters as SspiSecurityTokenParameters;
            if (ssl != null && ssl.RequireCancellation == false)
                cookie = true;
            if (cookie)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.RequireNonCookieMode, endpoint.Binding.Name, endpoint.Binding.Namespace)));

        }

        void IServiceBehavior.AddBindingParameters(ServiceDescription description, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection parameters)
        {
        }

        void IServiceBehavior.Validate(ServiceDescription service, ServiceHostBase serviceHostBase)
        {
        }

        void IServiceBehavior.ApplyDispatchBehavior(ServiceDescription service, ServiceHostBase serviceHostBase)
        {
            // The philosophy here is to respect settings obtained from the 
            // service surrogate class' attributes, as written by the user,
            // while rejecting those that contradict our requirements.
            // We never want to silently overwrite a user's attributes.
            // So we either accept overrides or reject them.
            //
            // If you're changing this code, you'll probably also want to change 
            // ComPlusServiceLoader.AddBehaviors

            foreach (ServiceEndpoint endpoint in service.Endpoints)
            {
                ICollection<BindingElement> bindingElements = endpoint.Binding.CreateBindingElements();
                foreach (BindingElement element in bindingElements)
                {
                    SymmetricSecurityBindingElement sbe = (element as SymmetricSecurityBindingElement);
                    if (sbe != null)
                    {
                        this.CheckForCookie(sbe.ProtectionTokenParameters, endpoint);
                        foreach (SecurityTokenParameters p in sbe.EndpointSupportingTokenParameters.Endorsing)
                            this.CheckForCookie(p, endpoint);
                        break;
                    }
                }
            }
        }
    }
}
