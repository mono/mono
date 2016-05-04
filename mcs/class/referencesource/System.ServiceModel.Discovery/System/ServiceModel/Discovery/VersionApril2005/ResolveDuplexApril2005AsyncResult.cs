//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery.VersionApril2005
{
    using System.Runtime;

    sealed class ResolveDuplexApril2005AsyncResult : ResolveDuplexAsyncResult<ResolveMessageApril2005, IDiscoveryResponseContractApril2005>
    {
        internal ResolveDuplexApril2005AsyncResult(ResolveMessageApril2005 resolveMessage,
            IDiscoveryServiceImplementation discoveryServiceImpl,
            IMulticastSuppressionImplementation multicastSuppressionImpl,
            AsyncCallback callback,
            object state)
            : base(resolveMessage, discoveryServiceImpl, multicastSuppressionImpl, callback, state)
        {
        }

        public static void End(IAsyncResult result)
        {
            AsyncResult.End<ResolveDuplexApril2005AsyncResult>(result);
        }

        protected override bool ValidateContent(ResolveMessageApril2005 resolveMessage)
        {
            if ((resolveMessage == null) || (resolveMessage.Resolve == null))
            {
                if (TD.DiscoveryMessageWithNoContentIsEnabled())
                {
                    TD.DiscoveryMessageWithNoContent(this.Context.EventTraceActivity, ProtocolStrings.TracingStrings.Resolve);
                }

                return false;
            }
            return true;
        }

        protected override ResolveCriteria GetResolveCriteria(ResolveMessageApril2005 resolveMessage)
        {
            return resolveMessage.Resolve.ToResolveCriteria();
        }

        protected override IAsyncResult BeginSendResolveResponse(
            IDiscoveryResponseContractApril2005 responseChannel,
            DiscoveryMessageSequence discoveryMessageSequence,
            EndpointDiscoveryMetadata matchingEndpoint,
            AsyncCallback callback,
            object state)
        {
            return responseChannel.BeginResolveMatchOperation(
                ResolveMatchesMessageApril2005.Create(
                discoveryMessageSequence,
                matchingEndpoint),
                callback,
                state);
        }

        protected override void EndSendResolveResponse(IDiscoveryResponseContractApril2005 responseChannel, IAsyncResult result)
        {
            responseChannel.EndResolveMatchOperation(result);
        }

        protected override IAsyncResult BeginSendProxyAnnouncement(
            IDiscoveryResponseContractApril2005 responseChannel,
            DiscoveryMessageSequence discoveryMessageSequence,
            EndpointDiscoveryMetadata proxyEndpointDiscoveryMetadata,
            AsyncCallback callback,
            object state)
        {
            return responseChannel.BeginHelloOperation(
                HelloMessageApril2005.Create(
                discoveryMessageSequence,
                proxyEndpointDiscoveryMetadata),
                callback,
                state);
        }

        protected override void EndSendProxyAnnouncement(IDiscoveryResponseContractApril2005 responseChannel, IAsyncResult result)
        {
            responseChannel.EndHelloOperation(result);
        }
    }
}
