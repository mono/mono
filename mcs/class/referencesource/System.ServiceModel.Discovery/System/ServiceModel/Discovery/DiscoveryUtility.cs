//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery
{
    using System.Collections.ObjectModel;
    using System.Runtime;
    using System.ServiceModel.Description;
    using System.ServiceModel.Discovery.Version11;
    using System.ServiceModel.Discovery.VersionApril2005;
    using System.ServiceModel.Discovery.VersionCD1;

    class DiscoveryUtility
    {
        public static Collection<EndpointDiscoveryMetadata> ToEndpointDiscoveryMetadataCollection(
            Collection<EndpointDiscoveryMetadataApril2005> endpointDiscoveryMetadataApril2005Collection)
        {
            Collection<EndpointDiscoveryMetadata> endpointDiscoveryMetadataCollection = new Collection<EndpointDiscoveryMetadata>();
            foreach (EndpointDiscoveryMetadataApril2005 endpointDiscoveryMetadataApril2005 in endpointDiscoveryMetadataApril2005Collection)
            {
                endpointDiscoveryMetadataCollection.Add(endpointDiscoveryMetadataApril2005.ToEndpointDiscoveryMetadata());
            }
            return endpointDiscoveryMetadataCollection;
        }

        public static Collection<EndpointDiscoveryMetadata> ToEndpointDiscoveryMetadataCollection(
            Collection<EndpointDiscoveryMetadataCD1> endpointDiscoveryMetadataCD1Collection)
        {
            Collection<EndpointDiscoveryMetadata> endpointDiscoveryMetadataCollection = new Collection<EndpointDiscoveryMetadata>();
            foreach (EndpointDiscoveryMetadataCD1 endpointDiscoveryMetadataCD1 in endpointDiscoveryMetadataCD1Collection)
            {
                endpointDiscoveryMetadataCollection.Add(endpointDiscoveryMetadataCD1.ToEndpointDiscoveryMetadata());
            }
            return endpointDiscoveryMetadataCollection;
        }

        public static Collection<EndpointDiscoveryMetadata> ToEndpointDiscoveryMetadataCollection(
            Collection<EndpointDiscoveryMetadata11> endpointDiscoveryMetadata11Collection)
        {
            Collection<EndpointDiscoveryMetadata> endpointDiscoveryMetadataCollection = new Collection<EndpointDiscoveryMetadata>();
            foreach (EndpointDiscoveryMetadata11 endpointDiscoveryMetadata11 in endpointDiscoveryMetadata11Collection)
            {
                endpointDiscoveryMetadataCollection.Add(endpointDiscoveryMetadata11.ToEndpointDiscoveryMetadata());
            }
            return endpointDiscoveryMetadataCollection;
        }

        public static ContractDescription GetContract(Type contractType)
        {
            Fx.Assert(contractType != null, "The discoveryContractType attribute must not be null.");

            ContractDescription discoveryContract = ContractDescription.GetContract(contractType);
            OperationBehaviorAttribute operationBehaviorAttribute;
            foreach (OperationDescription operationDescription in discoveryContract.Operations)
            {
                operationBehaviorAttribute = operationDescription.Behaviors.Find<OperationBehaviorAttribute>();
                if (operationBehaviorAttribute == null)
                {
                    operationBehaviorAttribute = new OperationBehaviorAttribute();
                    operationDescription.Behaviors.Add(operationBehaviorAttribute);
                }

                operationBehaviorAttribute.PreferAsyncInvocation = true;
            }

            return discoveryContract;
        }

        public static DiscoveryMessageSequence ToDiscoveryMessageSequenceOrNull(DiscoveryMessageSequenceApril2005 messageSequence)
        {
            if (messageSequence == null)
            {
                return null;
            }
            else
            {
                return messageSequence.ToDiscoveryMessageSequence();
            }
        }

        public static DiscoveryMessageSequence ToDiscoveryMessageSequenceOrNull(DiscoveryMessageSequenceCD1 messageSequence)
        {
            if (messageSequence == null)
            {
                return null;
            }
            else
            {
                return messageSequence.ToDiscoveryMessageSequence();
            }
        }

        public static DiscoveryMessageSequence ToDiscoveryMessageSequenceOrNull(DiscoveryMessageSequence11 messageSequence)
        {
            if (messageSequence == null)
            {
                return null;
            }
            else
            {
                return messageSequence.ToDiscoveryMessageSequence();
            }
        }

        public static bool IsCompatible(OperationContext context, IContextChannel channel)
        {
            return ((context != null) && 
                (context.InternalServiceChannel != null) && 
                (object.ReferenceEquals(context.InternalServiceChannel.Proxy, channel)));
        }
    }
}
