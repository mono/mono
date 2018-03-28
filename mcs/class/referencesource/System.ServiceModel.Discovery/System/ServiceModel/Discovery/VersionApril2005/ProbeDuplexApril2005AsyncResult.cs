//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery.VersionApril2005
{
    using System.Runtime;

    sealed class ProbeDuplexApril2005AsyncResult : ProbeDuplexAsyncResult<ProbeMessageApril2005, IDiscoveryResponseContractApril2005>
    {
        internal ProbeDuplexApril2005AsyncResult(ProbeMessageApril2005 probeMessage,
            IDiscoveryServiceImplementation discoveryServiceImpl,
            IMulticastSuppressionImplementation multicastSuppressionImpl,
            AsyncCallback callback,
            object state)
            : base(probeMessage, discoveryServiceImpl, multicastSuppressionImpl, callback, state)
        {
        }

        public static void End(IAsyncResult result)
        {
            AsyncResult.End<ProbeDuplexApril2005AsyncResult>(result);
        }

        protected override bool ValidateContent(ProbeMessageApril2005 probeMessage)
        {
            if ((probeMessage == null) || (probeMessage.Probe == null))
            {
                if (TD.DiscoveryMessageWithNoContentIsEnabled())
                {
                    TD.DiscoveryMessageWithNoContent(this.Context.EventTraceActivity, ProtocolStrings.TracingStrings.Probe);
                }

                return false;
            }
            return true;
        }

        protected override FindCriteria GetFindCriteria(ProbeMessageApril2005 probeMessage)
        {
            return probeMessage.Probe.ToFindCriteria();
        }

        protected override IAsyncResult BeginSendFindResponse(
            IDiscoveryResponseContractApril2005 responseChannel, 
            DiscoveryMessageSequence discoveryMessageSequence, 
            EndpointDiscoveryMetadata matchingEndpoint, 
            AsyncCallback callback, 
            object state)
        {
            return responseChannel.BeginProbeMatchOperation(
                ProbeMatchesMessageApril2005.Create(
                discoveryMessageSequence,
                matchingEndpoint),
                callback, 
                state);
        }

        protected override void EndSendFindResponse(IDiscoveryResponseContractApril2005 responseChannel, IAsyncResult result)
        {
            responseChannel.EndProbeMatchOperation(result);
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
