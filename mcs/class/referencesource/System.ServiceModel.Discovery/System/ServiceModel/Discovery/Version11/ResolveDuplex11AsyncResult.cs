//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery.Version11
{
    using System.Runtime;

    sealed class ResolveDuplex11AsyncResult : ResolveDuplexAsyncResult<ResolveMessage11, IDiscoveryResponseContract11>
    {
        internal ResolveDuplex11AsyncResult(ResolveMessage11 resolveMessage,
            IDiscoveryServiceImplementation discoveryServiceImpl,
            IMulticastSuppressionImplementation multicastSuppressionImpl,
            AsyncCallback callback,
            object state)
            : base(resolveMessage, discoveryServiceImpl, multicastSuppressionImpl, callback, state)
        {
        }

        public static void End(IAsyncResult result)
        {
            AsyncResult.End<ResolveDuplex11AsyncResult>(result);
        }

        protected override bool ValidateContent(ResolveMessage11 resolveMessage)
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

        protected override ResolveCriteria GetResolveCriteria(ResolveMessage11 resolveMessage)
        {
            return resolveMessage.Resolve.ToResolveCriteria();
        }

        protected override IAsyncResult BeginSendResolveResponse(
            IDiscoveryResponseContract11 responseChannel, 
            DiscoveryMessageSequence discoveryMessageSequence, 
            EndpointDiscoveryMetadata matchingEndpoint, 
            AsyncCallback callback, 
            object state)
        {
            return responseChannel.BeginResolveMatchOperation(
                ResolveMatchesMessage11.Create(
                discoveryMessageSequence,
                matchingEndpoint),
                callback, 
                state);
        }

        protected override void EndSendResolveResponse(IDiscoveryResponseContract11 responseChannel, IAsyncResult result)
        {
            responseChannel.EndResolveMatchOperation(result);
        }

        protected override IAsyncResult BeginSendProxyAnnouncement(
            IDiscoveryResponseContract11 responseChannel,
            DiscoveryMessageSequence discoveryMessageSequence,
            EndpointDiscoveryMetadata proxyEndpointDiscoveryMetadata,
            AsyncCallback callback,
            object state)
        {
            return responseChannel.BeginHelloOperation(
                HelloMessage11.Create(
                discoveryMessageSequence,
                proxyEndpointDiscoveryMetadata), 
                callback, 
                state);
        }

        protected override void EndSendProxyAnnouncement(IDiscoveryResponseContract11 responseChannel, IAsyncResult result)
        {
            responseChannel.EndHelloOperation(result);
        }
    }
}
