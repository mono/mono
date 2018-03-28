//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Description
{
    using System.Collections.Generic;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;

    public sealed class ServiceMetadataContractBehavior : IContractBehavior
    {
        bool metadataGenerationDisabled = false;

        public ServiceMetadataContractBehavior()
        {
        }

        public ServiceMetadataContractBehavior(bool metadataGenerationDisabled)
            : this()
        {
            this.metadataGenerationDisabled = metadataGenerationDisabled;
        }

        public bool MetadataGenerationDisabled
        {
            get { return this.metadataGenerationDisabled; }
            set { this.metadataGenerationDisabled = value; }
        }

        #region IContractBehavior Members

        void IContractBehavior.Validate(ContractDescription description, ServiceEndpoint endpoint)
        {
        }

        void IContractBehavior.ApplyDispatchBehavior(ContractDescription description, ServiceEndpoint endpoint, DispatchRuntime dispatch)
        {
        }

        void IContractBehavior.AddBindingParameters(ContractDescription description, ServiceEndpoint endpoint, BindingParameterCollection parameters)
        {
        }

        void IContractBehavior.ApplyClientBehavior(ContractDescription description, ServiceEndpoint endpoint, ClientRuntime proxy)
        {
        }

        #endregion
    }
}
