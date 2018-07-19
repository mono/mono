//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery.VersionCD1
{
    using System.Runtime;
    using System.Collections.ObjectModel;

    sealed class ProbeRequestResponseCD1AsyncResult : ProbeRequestResponseAsyncResult<ProbeMessageCD1, ProbeMatchesMessageCD1>
    {
        internal ProbeRequestResponseCD1AsyncResult(ProbeMessageCD1 probeMessage,
            IDiscoveryServiceImplementation discoveryServiceImpl,
            AsyncCallback callback,
            object state)
            : base(probeMessage, discoveryServiceImpl, callback, state)
        {
        }

        public static ProbeMatchesMessageCD1 End(IAsyncResult result)
        {
            ProbeRequestResponseCD1AsyncResult thisPtr = AsyncResult.End<ProbeRequestResponseCD1AsyncResult>(result);
            return thisPtr.End();
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

        protected override ProbeMatchesMessageCD1 GetProbeResponse(
            DiscoveryMessageSequence discoveryMessageSequence,
            Collection<EndpointDiscoveryMetadata> matchingEndpoints)
        {
            return ProbeMatchesMessageCD1.Create(discoveryMessageSequence, matchingEndpoints);
        }
    }
}
