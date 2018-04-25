//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery.VersionCD1
{
    using System.Runtime;

    sealed class ResolveRequestResponseCD1AsyncResult : ResolveRequestResponseAsyncResult<ResolveMessageCD1, ResolveMatchesMessageCD1>
    {
        internal ResolveRequestResponseCD1AsyncResult(ResolveMessageCD1 resolveMessage,
            IDiscoveryServiceImplementation discoveryServiceImpl,
            AsyncCallback callback,
            object state)
            : base(resolveMessage, discoveryServiceImpl, callback, state)
        {
        }

        public static ResolveMatchesMessageCD1 End(IAsyncResult result)
        {
            ResolveRequestResponseCD1AsyncResult thisPtr = AsyncResult.End<ResolveRequestResponseCD1AsyncResult>(result);
            return thisPtr.End();
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

        protected override ResolveMatchesMessageCD1 GetResolveResponse(
            DiscoveryMessageSequence discoveryMessageSequence,
            EndpointDiscoveryMetadata matchingEndpoint)
        {
            return ResolveMatchesMessageCD1.Create(discoveryMessageSequence, matchingEndpoint);
        }
    }
}
