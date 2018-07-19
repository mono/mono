//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery.Version11
{
    using System.Collections.ObjectModel;
    using System.Runtime;

    sealed class ProbeRequestResponse11AsyncResult : ProbeRequestResponseAsyncResult<ProbeMessage11, ProbeMatchesMessage11>
    {
        internal ProbeRequestResponse11AsyncResult(ProbeMessage11 probeMessage,
            IDiscoveryServiceImplementation discoveryServiceImpl,
            AsyncCallback callback,
            object state)
            : base(probeMessage, discoveryServiceImpl, callback, state)
        {
        }

        public static ProbeMatchesMessage11 End(IAsyncResult result)
        {
            ProbeRequestResponse11AsyncResult thisPtr = AsyncResult.End<ProbeRequestResponse11AsyncResult>(result);
            return thisPtr.End();
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

        protected override ProbeMatchesMessage11 GetProbeResponse(
            DiscoveryMessageSequence discoveryMessageSequence, 
            Collection<EndpointDiscoveryMetadata> matchingEndpoints)
        {
            return ProbeMatchesMessage11.Create(discoveryMessageSequence, matchingEndpoints);
        }
    }
}
