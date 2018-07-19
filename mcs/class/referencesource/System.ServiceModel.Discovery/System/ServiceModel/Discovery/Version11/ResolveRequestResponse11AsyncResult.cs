//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery.Version11
{
    using System.Runtime;

    sealed class ResolveRequestResponse11AsyncResult : ResolveRequestResponseAsyncResult<ResolveMessage11, ResolveMatchesMessage11>
    {
        internal ResolveRequestResponse11AsyncResult(ResolveMessage11 resolveMessage,
            IDiscoveryServiceImplementation discoveryServiceImpl,
            AsyncCallback callback,
            object state)
            : base(resolveMessage, discoveryServiceImpl, callback, state)
        {
        }

        public static ResolveMatchesMessage11 End(IAsyncResult result)
        {
            ResolveRequestResponse11AsyncResult thisPtr = AsyncResult.End<ResolveRequestResponse11AsyncResult>(result);
            return thisPtr.End();
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

        protected override ResolveMatchesMessage11 GetResolveResponse(
            DiscoveryMessageSequence discoveryMessageSequence, 
            EndpointDiscoveryMetadata matchingEndpoint)
        {
            return ResolveMatchesMessage11.Create(discoveryMessageSequence, matchingEndpoint);
        }
    }
}
