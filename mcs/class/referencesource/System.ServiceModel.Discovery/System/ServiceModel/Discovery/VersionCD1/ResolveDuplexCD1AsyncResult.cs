//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery.VersionCD1
{
    using System.Runtime;

    sealed class ResolveDuplexCD1AsyncResult : ResolveDuplexAsyncResult<ResolveMessageCD1, IDiscoveryResponseContractCD1>
    {
        internal ResolveDuplexCD1AsyncResult(ResolveMessageCD1 resolveMessage,
            IDiscoveryServiceImplementation discoveryServiceImpl,
            IMulticastSuppressionImplementation multicastSuppressionImpl,
            AsyncCallback callback,
            object state)
            : base(resolveMessage, discoveryServiceImpl, multicastSuppressionImpl, callback, state)
        {
        }

        public static void End(IAsyncResult result)
        {
            AsyncResult.End<ResolveDuplexCD1AsyncResult>(result);
        }

        protected override bool ValidateContent(ResolveMessageCD1 resolveMessage)
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

        protected override ResolveCriteria GetResolveCriteria(ResolveMessageCD1 resolveMessage)
        {
            return resolveMessage.Resolve.ToResolveCriteria();
        }

        protected override IAsyncResult BeginSendResolveResponse(
            IDiscoveryResponseContractCD1 responseChannel,
            DiscoveryMessageSequence discoveryMessageSequence,
            EndpointDiscoveryMetadata matchingEndpoint,
            AsyncCallback callback,
            object state)
        {
            return responseChannel.BeginResolveMatchOperation(
                ResolveMatchesMessageCD1.Create(
                discoveryMessageSequence,
                matchingEndpoint),
                callback,
                state);
        }

        protected override void EndSendResolveResponse(IDiscoveryResponseContractCD1 responseChannel, IAsyncResult result)
        {
            responseChannel.EndResolveMatchOperation(result);
        }

        protected override IAsyncResult BeginSendProxyAnnouncement(
            IDiscoveryResponseContractCD1 responseChannel,
            DiscoveryMessageSequence discoveryMessageSequence,
            EndpointDiscoveryMetadata proxyEndpointDiscoveryMetadata,
            AsyncCallback callback,
            object state)
        {
            return responseChannel.BeginHelloOperation(
                HelloMessageCD1.Create(
                discoveryMessageSequence,
                proxyEndpointDiscoveryMetadata),
                callback,
                state);
        }

        protected override void EndSendProxyAnnouncement(IDiscoveryResponseContractCD1 responseChannel, IAsyncResult result)
        {
            responseChannel.EndHelloOperation(result);
        }
    }
}
