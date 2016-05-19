//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.Runtime;
    using System.Security;
    using System.ServiceModel;
    using System.ServiceModel.Description;

    public abstract partial class StandardEndpointElement : ConfigurationElement, IConfigurationContextProviderInternal
    {
        [Fx.Tag.SecurityNote(Critical = "Stores information used in a security decision.")]
        [SecurityCritical]
        EvaluationContextHelper contextHelper;

        protected StandardEndpointElement()
            : base()
        {
        }

        protected internal abstract Type EndpointType
        {
            get;
        }

        [ConfigurationProperty(ConfigurationStrings.Name, Options = ConfigurationPropertyOptions.IsKey)]
        [StringValidator(MinLength = 0)]
        public string Name
        {
            get { return (string)base[ConfigurationStrings.Name]; }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    value = String.Empty;
                }
                base[ConfigurationStrings.Name] = value;
            }
        }

        public void InitializeAndValidate(ChannelEndpointElement channelEndpointElement)
        {
            if (null == channelEndpointElement)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("channelEndpointElement");
            }

            // The properties channelEndpointElement.Name and this.Name are actually two different things:
            //     - channelEndpointElement.Name corresponds to the service endpoint name
            //     - this.Name is a token used as a key in the endpoint collection to identify
            //       a specific bucket of configuration settings.
            // Thus, the Name property is skipped here.

            this.OnInitializeAndValidate(channelEndpointElement);
        }

        public void InitializeAndValidate(ServiceEndpointElement serviceEndpointElement)
        {
            if (null == serviceEndpointElement)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("serviceEndpointElement");
            }

            // The properties serviceEndpointElement.Name and this.Name are actually two different things:
            //     - serviceEndpointElement.Name corresponds to the service endpoint name 
            //     - this.Name is a token used as a key in the endpoint collection to identify
            //       a specific bucket of configuration settings.
            // Thus, the Name property is skipped here.

            this.OnInitializeAndValidate(serviceEndpointElement);
        }
        
        public void ApplyConfiguration(ServiceEndpoint endpoint, ChannelEndpointElement channelEndpointElement)
        {
            if (null == endpoint)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpoint");
            }

            if (null == channelEndpointElement)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("channelEndpointElement");
            }

            if (endpoint.GetType() != this.EndpointType)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.ConfigInvalidTypeForEndpoint,
                    this.EndpointType.AssemblyQualifiedName,
                    endpoint.GetType().AssemblyQualifiedName));
            }

            // The properties endpoint.Name and this.Name are actually two different things:
            //     - endpoint.Name corresponds to the service endpoint name and is surfaced through
            //       serviceEndpointElement.Name
            //     - this.Name is a token used as a key in the endpoint collection to identify
            //       a specific bucket of configuration settings.
            // Thus, the Name property is skipped here.

            this.OnApplyConfiguration(endpoint, channelEndpointElement);
        }

        public void ApplyConfiguration(ServiceEndpoint endpoint, ServiceEndpointElement serviceEndpointElement)
        {
            if (null == endpoint)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpoint");
            }

            if (null == serviceEndpointElement)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("serviceEndpointElement");
            }

            if (endpoint.GetType() != this.EndpointType)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.ConfigInvalidTypeForEndpoint,
                    (this.EndpointType == null) ? string.Empty : this.EndpointType.AssemblyQualifiedName,
                    endpoint.GetType().AssemblyQualifiedName));
            }

            // The properties endpoint.Name and this.Name are actually two different things:
            //     - endpoint.Name corresponds to the service endpoint name and is surfaced through
            //       serviceEndpointElement.Name
            //     - this.Name is a token used as a key in the endpoint collection to identify
            //       a specific bucket of configuration settings.
            // Thus, the Name property is skipped here.

            this.OnApplyConfiguration(endpoint, serviceEndpointElement);
        }

        protected virtual internal void InitializeFrom(ServiceEndpoint endpoint)
        {
            if (null == endpoint)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpoint");
            }
            if (endpoint.GetType() != this.EndpointType)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.ConfigInvalidTypeForEndpoint,
                    (this.EndpointType == null) ? string.Empty : this.EndpointType.AssemblyQualifiedName,
                    endpoint.GetType().AssemblyQualifiedName));
            }

            // The properties endpoint.Name and this.Name are actually two different things:
            //     - endpoint.Name corresponds to the service endpoint name and is surfaced through
            //       serviceEndpointElement.Name
            //     - this.Name is a token used as a key in the endpoint collection to identify
            //       a specific bucket of configuration settings.
            // Thus, the Name property is skipped here.

        }

        protected internal abstract ServiceEndpoint CreateServiceEndpoint(ContractDescription contractDescription);
        protected abstract void OnApplyConfiguration(ServiceEndpoint endpoint, ChannelEndpointElement channelEndpointElement);
        protected abstract void OnApplyConfiguration(ServiceEndpoint endpoint, ServiceEndpointElement serviceEndpointElement);
        protected abstract void OnInitializeAndValidate(ChannelEndpointElement channelEndpointElement);
        protected abstract void OnInitializeAndValidate(ServiceEndpointElement serviceEndpointElement);

        [Fx.Tag.SecurityNote(Critical = "Accesses critical field contextHelper.")]
        [SecurityCritical]
        protected override void Reset(ConfigurationElement parentElement)
        {
            this.contextHelper.OnReset(parentElement);

            base.Reset(parentElement);
        }

        ContextInformation IConfigurationContextProviderInternal.GetEvaluationContext()
        {
            return this.EvaluationContext;
        }

        [Fx.Tag.SecurityNote(Critical = "Accesses critical field contextHelper.",
            Miscellaneous = "RequiresReview -- the return value will be used for a security decision -- see comment in interface definition.")]
        [SecurityCritical]
        ContextInformation IConfigurationContextProviderInternal.GetOriginalEvaluationContext()
        {
            return this.contextHelper.GetOriginalContext(this);
        }
    }
}
