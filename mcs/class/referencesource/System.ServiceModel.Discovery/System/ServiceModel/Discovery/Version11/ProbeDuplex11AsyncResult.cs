//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery.Version11
{
    using System.Runtime;

    sealed class ProbeDuplex11AsyncResult : ProbeDuplexAsyncResult<ProbeMessage11, IDiscoveryResponseContract11>
    {
        internal ProbeDuplex11AsyncResult(ProbeMessage11 probeMessage,
            IDiscoveryServiceImplementation discoveryServiceImpl,
            IMulticastSuppressionImplementation multicastSuppressionImpl,
            AsyncCallback callback,
            object state)
            : base(probeMessage, discoveryServiceImpl, multicastSuppressionImpl, callback, state)
        {
        }

        public static void End(IAsyncResult result)
        {
            AsyncResult.End<ProbeDuplex11AsyncResult>(result);
        }

        protected override bool ValidateContent(ProbeMessage11 probeMessage)
        {
            if ((probeMessage == null) || (probeMessage.Probe == null))
            {
                if (TD.DiscoveryMessageWithNoContentIsEnabled())
                {
                    TD.DiscoveryMessageWithNoContent(null, ProtocolStrings.TracingStrings.Probe);
                }

                return false;
            }
            return true;
        }

        protected override FindCriteria GetFindCriteria(ProbeMessage11 probeMessage)
        {
            return probeMessage.Probe.ToFindCriteria();
        }

        protected override IAsyncResult BeginSendFindResponse(
            IDiscoveryResponseContract11 responseChannel,
            DiscoveryMessageSequence discoveryMessageSequence,
            EndpointDiscoveryMetadata matchingEndpoint,
            AsyncCallback callback,
            object state)
        {
            return responseChannel.BeginProbeMatchOperation(
                ProbeMatchesMessage11.Create(
                discoveryMessageSequence,
                matchingEndpoint), 
                callback, 
                state);
        }

        protected override void EndSendFindResponse(IDiscoveryResponseContract11 responseChannel, IAsyncResult result)
        {
            responseChannel.EndProbeMatchOperation(result);
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
