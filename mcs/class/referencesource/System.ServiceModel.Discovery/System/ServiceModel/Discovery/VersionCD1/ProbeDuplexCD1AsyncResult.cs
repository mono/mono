//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery.VersionCD1
{
    using System.Runtime;

    sealed class ProbeDuplexCD1AsyncResult : ProbeDuplexAsyncResult<ProbeMessageCD1, IDiscoveryResponseContractCD1>
    {
        internal ProbeDuplexCD1AsyncResult(ProbeMessageCD1 probeMessage,
            IDiscoveryServiceImplementation discoveryServiceImpl,
            IMulticastSuppressionImplementation multicastSuppressionImpl,
            AsyncCallback callback,
            object state)
            : base(probeMessage, discoveryServiceImpl, multicastSuppressionImpl, callback, state)
        {
        }

        public static void End(IAsyncResult result)
        {
            AsyncResult.End<ProbeDuplexCD1AsyncResult>(result);
        }

        protected override bool ValidateContent(ProbeMessageCD1 probeMessage)
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

        protected override FindCriteria GetFindCriteria(ProbeMessageCD1 probeMessage)
        {
            return probeMessage.Probe.ToFindCriteria();
        }

        protected override IAsyncResult BeginSendFindResponse(
            IDiscoveryResponseContractCD1 responseChannel,
            DiscoveryMessageSequence discoveryMessageSequence,
            EndpointDiscoveryMetadata matchingEndpoint,
            AsyncCallback callback,
            object state)
        {
            return responseChannel.BeginProbeMatchOperation(
                ProbeMatchesMessageCD1.Create(
                discoveryMessageSequence,
                matchingEndpoint),
                callback,
                state);
        }

        protected override void EndSendFindResponse(IDiscoveryResponseContractCD1 responseChannel, IAsyncResult result)
        {
            responseChannel.EndProbeMatchOperation(result);
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
