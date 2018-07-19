//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel
{
    using System.Runtime;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;


    sealed class ConfigurationEndpointTrait<TChannel> : EndpointTrait<TChannel>
        where TChannel : class
    {
        string endpointConfigurationName;
        EndpointAddress remoteAddress;
        InstanceContext callbackInstance;

        public ConfigurationEndpointTrait(string endpointConfigurationName,
            EndpointAddress remoteAddress,
            InstanceContext callbackInstance)
        {
            this.endpointConfigurationName = endpointConfigurationName;
            this.remoteAddress = remoteAddress;
            this.callbackInstance = callbackInstance;
        }

        public override bool Equals(object obj)
        {
            ConfigurationEndpointTrait<TChannel> trait1 = obj as ConfigurationEndpointTrait<TChannel>;
            if (trait1 == null) return false;

            if (!object.ReferenceEquals(this.callbackInstance, trait1.callbackInstance))
                return false;

            if (string.CompareOrdinal(this.endpointConfigurationName, trait1.endpointConfigurationName) != 0)
            {
                return false;
            }

            // EndpointAddress.Equals is used.
            if (this.remoteAddress != trait1.remoteAddress)
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

            Fx.Assert(this.endpointConfigurationName != null, "endpointConfigurationName should not be null.");
            hashCode ^= this.endpointConfigurationName.GetHashCode();

            if (this.remoteAddress != null)
            {
                hashCode ^= this.remoteAddress.GetHashCode();
            }

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
            if (this.remoteAddress != null)
            {
                return new DuplexChannelFactory<TChannel>(this.callbackInstance, this.endpointConfigurationName, this.remoteAddress);
            }

            return new DuplexChannelFactory<TChannel>(this.callbackInstance, this.endpointConfigurationName);
        }

        ChannelFactory<TChannel> CreateSimplexFactory()
        {
            if (this.remoteAddress != null)
            {
                return new ChannelFactory<TChannel>(this.endpointConfigurationName, this.remoteAddress);
            }

            return new ChannelFactory<TChannel>(this.endpointConfigurationName);
        }
    }

}
