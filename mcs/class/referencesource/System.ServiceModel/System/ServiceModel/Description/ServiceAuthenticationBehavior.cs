//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------

namespace System.ServiceModel.Description
{
    using System;
    using System.Collections.ObjectModel;
    using System.Net;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Security;
    using System.ServiceModel.Security.Tokens;

    public sealed class ServiceAuthenticationBehavior : IServiceBehavior
    {
        internal ServiceAuthenticationManager defaultServiceAuthenticationManager;

        ServiceAuthenticationManager serviceAuthenticationManager;
        AuthenticationSchemes authenticationSchemes;
        bool isAuthenticationManagerSet;
        bool isAuthenticationSchemesSet;
        bool isReadOnly;
       
        public ServiceAuthenticationBehavior()
        {
            this.ServiceAuthenticationManager = defaultServiceAuthenticationManager;
            this.authenticationSchemes = AuthenticationSchemes.None;
        }

        ServiceAuthenticationBehavior(ServiceAuthenticationBehavior other)
        {
            this.serviceAuthenticationManager = other.ServiceAuthenticationManager;
            this.authenticationSchemes = other.authenticationSchemes;
            this.isReadOnly = other.isReadOnly;
            this.isAuthenticationManagerSet = other.isAuthenticationManagerSet;
            this.isAuthenticationSchemesSet = other.isAuthenticationSchemesSet;
        }

        public ServiceAuthenticationManager ServiceAuthenticationManager
        {
            get
            {
                return this.serviceAuthenticationManager;
            }
            set
            {
                ThrowIfImmutable();
                this.serviceAuthenticationManager = value;
                this.isAuthenticationManagerSet = value != null;
            }
        }

        public AuthenticationSchemes AuthenticationSchemes
        {
            get
            {
                return this.authenticationSchemes;
            }
            set
            {
                ThrowIfImmutable();
                this.authenticationSchemes = value;
                this.isAuthenticationSchemesSet = true;
            }
        }

        public bool ShouldSerializeServiceAuthenticationManager()
        {
            return this.isAuthenticationManagerSet;
        }

        public bool ShouldSerializeAuthenticationSchemes()
        {
            return this.isAuthenticationSchemesSet;
        }

        void IServiceBehavior.Validate(ServiceDescription description, ServiceHostBase serviceHostBase)
        {
        }

        void IServiceBehavior.AddBindingParameters(ServiceDescription description, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection parameters)
        {
            if (parameters == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("parameters");
            }

            if (this.serviceAuthenticationManager != null)
            {
                // throw if bindingParameters already has a AuthenticationManager
                ServiceAuthenticationManager otherAuthenticationManager = parameters.Find<ServiceAuthenticationManager>();
                if (otherAuthenticationManager != null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.MultipleAuthenticationManagersInServiceBindingParameters, otherAuthenticationManager)));
                }

                parameters.Add(this.serviceAuthenticationManager);
            }

            if (this.authenticationSchemes != AuthenticationSchemes.None)
            {
                // throw if bindingParameters already has an AuthenticationSchemes
                AuthenticationSchemesBindingParameter otherAuthenticationSchemesBindingParameter = parameters.Find<AuthenticationSchemesBindingParameter>();
                if (otherAuthenticationSchemesBindingParameter != null)
                {
                    if (otherAuthenticationSchemesBindingParameter.AuthenticationSchemes != authenticationSchemes)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.MultipleAuthenticationSchemesInServiceBindingParameters, otherAuthenticationSchemesBindingParameter.AuthenticationSchemes)));
                    }
                }
                else
                {
                    parameters.Add(new AuthenticationSchemesBindingParameter(this.authenticationSchemes));
                }
            }
        }

        void IServiceBehavior.ApplyDispatchBehavior(ServiceDescription description, ServiceHostBase serviceHostBase)
        {
            if (description == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("description"));
            if (serviceHostBase == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("serviceHostBase"));

            if (this.serviceAuthenticationManager == null)
            {
                return;
            }

            for (int i = 0; i < serviceHostBase.ChannelDispatchers.Count; i++)
            {
                ChannelDispatcher channelDispatcher = serviceHostBase.ChannelDispatchers[i] as ChannelDispatcher;
                if (channelDispatcher != null && !ServiceMetadataBehavior.IsHttpGetMetadataDispatcher(description, channelDispatcher))
                {
                    foreach (EndpointDispatcher endpointDispatcher in channelDispatcher.Endpoints)
                    {
                        DispatchRuntime behavior = endpointDispatcher.DispatchRuntime;
                        behavior.ServiceAuthenticationManager = this.serviceAuthenticationManager;

                        ServiceEndpoint endpoint = FindMatchingServiceEndpoint(description, endpointDispatcher);
                        if (endpoint != null)
                        {
                            bool isSecureConversationBinding = IsSecureConversationBinding(endpoint.Binding);

                            if (isSecureConversationBinding)
                            {
                                SecurityStandardsManager standardsManager = GetConfiguredSecurityStandardsManager(endpoint.Binding);
                                behavior.ServiceAuthenticationManager = new ServiceAuthenticationManagerWrapper(this.serviceAuthenticationManager, new string[] { standardsManager.SecureConversationDriver.CloseAction.Value });
                            }
                        }
                    }
                }
            }
        }

        internal ServiceAuthenticationBehavior Clone()
        {
            return new ServiceAuthenticationBehavior(this);
        }

        internal void MakeReadOnly()
        {
            this.isReadOnly = true;
        }

        void ThrowIfImmutable()
        {
            if (this.isReadOnly)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ObjectIsReadOnly)));
            }
        }

        ServiceEndpoint FindMatchingServiceEndpoint(ServiceDescription description, EndpointDispatcher endpointDispatcher)
        {
            foreach (ServiceEndpoint endpoint in description.Endpoints)
            {
                if (endpoint.Address.Equals(endpointDispatcher.EndpointAddress))
                {
                    return endpoint;
                }
            }

            return null;
        }

        bool IsSecureConversationBinding(Binding binding)
        {
            if (binding == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("binding");

            SecurityBindingElement securityBindingElement = binding.CreateBindingElements().Find<SecurityBindingElement>();
            if (securityBindingElement == null)
            {
                return false;
            }

            foreach (SecurityTokenParameters tokenParam in new SecurityTokenParametersEnumerable(securityBindingElement, true))
            {
                if (tokenParam is SecureConversationSecurityTokenParameters)
                {
                    return true;
                }
            }

            return false;
        }

        SecurityStandardsManager GetConfiguredSecurityStandardsManager(Binding binding)
        {
            if (binding == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("binding");

            SecurityBindingElement securityBindingElement = binding.CreateBindingElements().Find<SecurityBindingElement>();
            if (securityBindingElement == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("binding", SR.GetString(SR.NoSecurityBindingElementFound));
            }

            return new SecurityStandardsManager(securityBindingElement.MessageSecurityVersion, new WSSecurityTokenSerializer(securityBindingElement.MessageSecurityVersion.SecurityVersion));
        }

    }

}
