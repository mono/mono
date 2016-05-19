//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Routing
{
    using System.ServiceModel.Description;

    sealed class RoutingEndpointTrait
    {
        public RoutingEndpointTrait(Type routerContract, ServiceEndpoint endpoint, OperationContext operationContext)
        {
            if (routerContract == typeof(IDuplexSessionRouter))
            {
                IDuplexRouterCallback callbackSession = operationContext.GetCallbackChannel<IDuplexRouterCallback>();
                this.CallbackInstance = callbackSession;
            }
            this.Endpoint = endpoint;
            this.RouterContract = routerContract;
        }

        public IDuplexRouterCallback CallbackInstance
        {
            get;
            private set;
        }

        public ServiceEndpoint Endpoint
        {
            get;
            private set;
        }

        public Type RouterContract
        {
            get;
            private set;
        }

        public override bool Equals(object obj)
        {
            RoutingEndpointTrait other = obj as RoutingEndpointTrait;
            if (other == null)
            {
                return false;
            }
            if (!object.ReferenceEquals(this.Endpoint, other.Endpoint))
            {
                return false;
            }
            if (this.RouterContract != other.RouterContract)
            {
                return false;
            }
            if (!object.ReferenceEquals(this.CallbackInstance, other.CallbackInstance))
            {
                return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            int num = 0;
            num ^= this.Endpoint.GetHashCode();
            num ^= this.RouterContract.GetHashCode();
            if (this.CallbackInstance != null)
            {
                num ^= this.CallbackInstance.GetHashCode();
            }
            return num;
        }

        public override string ToString()
        {
            return this.Endpoint.Name + ";" + this.Endpoint.Binding.Name + ";" + this.Endpoint.Address.Uri.ToString();
        }
    }
}
