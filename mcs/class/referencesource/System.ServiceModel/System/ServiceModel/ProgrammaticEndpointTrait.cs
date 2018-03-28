//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel
{
    using System.Runtime;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;

    sealed class ProgrammaticEndpointTrait<TChannel> : EndpointTrait<TChannel>
        where TChannel : class
    {
        EndpointAddress remoteAddress;
        Binding binding;
        InstanceContext callbackInstance;

        public ProgrammaticEndpointTrait(Binding binding,
            EndpointAddress remoteAddress,
            InstanceContext callbackInstance)
            : base()
        {
            this.binding = binding;
            this.remoteAddress = remoteAddress;
            this.callbackInstance = callbackInstance;
        }

        public override bool Equals(object obj)
        {
            ProgrammaticEndpointTrait<TChannel> trait1 = obj as ProgrammaticEndpointTrait<TChannel>;
            if (trait1 == null) return false;

            if (!object.ReferenceEquals(this.callbackInstance, trait1.callbackInstance))
                return false;

            // EndpointAddress.Equals is used.
            if (this.remoteAddress != trait1.remoteAddress)
                return false;

            if (!object.ReferenceEquals(this.binding, trait1.binding))
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

            Fx.Assert(this.remoteAddress != null, "remoteAddress should not be null.");
            hashCode ^= this.remoteAddress.GetHashCode();


            Fx.Assert(this.binding != null, "binding should not be null.");
            hashCode ^= this.binding.GetHashCode();

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
            Fx.Assert(this.remoteAddress != null, "remoteAddress should not be null.");
            Fx.Assert(this.binding != null, "binding should not be null.");

            return new DuplexChannelFactory<TChannel>(this.callbackInstance, this.binding, this.remoteAddress);
        }

        ChannelFactory<TChannel> CreateSimplexFactory()
        {
            Fx.Assert(this.remoteAddress != null, "remoteAddress should not be null.");
            Fx.Assert(this.binding != null, "binding should not be null.");

            return new ChannelFactory<TChannel>(this.binding, this.remoteAddress);
        }
    }
}
