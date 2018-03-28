//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel
{
    using System.Runtime;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;

    sealed class ServiceEndpointTrait<TChannel> : EndpointTrait<TChannel>
        where TChannel : class
    {
        InstanceContext callbackInstance;
        ServiceEndpoint endpoint;

        public ServiceEndpointTrait(ServiceEndpoint endpoint,
            InstanceContext callbackInstance)
        {
            this.endpoint = endpoint;
            this.callbackInstance = callbackInstance;
        }

        public override bool Equals(object obj)
        {
            ServiceEndpointTrait<TChannel> trait1 = obj as ServiceEndpointTrait<TChannel>;
            if (trait1 == null) return false;

            if (!object.ReferenceEquals(this.callbackInstance, trait1.callbackInstance))
                return false;

            if (!object.ReferenceEquals(this.endpoint, trait1.endpoint))
                return false;

            return true;
        }

        public override int GetHashCode()
        {
            int hashCode = 0;

            if (this.callbackInstance != null)
            {
                hashCode ^= this.callbackInstance.GetHashCode();
            }

            Fx.Assert(this.endpoint != null, "endpoint should not be null.");
            hashCode ^= this.endpoint.GetHashCode();

            return hashCode;
        }

        public override ChannelFactory<TChannel> CreateChannelFactory()
        {
            if (this.callbackInstance != null)
                return CreateDuplexFactory();

            return CreateSimplexFactory();
        }

        DuplexChannelFactory<TChannel> CreateDuplexFactory()
        {
            Fx.Assert(this.endpoint != null, "endpoint should not be null.");
            return new DuplexChannelFactory<TChannel>(this.callbackInstance, this.endpoint);
        }

        ChannelFactory<TChannel> CreateSimplexFactory()
        {
            Fx.Assert(this.endpoint != null, "endpoint should not be null.");
            return new ChannelFactory<TChannel>(this.endpoint);
        }
    }
}
