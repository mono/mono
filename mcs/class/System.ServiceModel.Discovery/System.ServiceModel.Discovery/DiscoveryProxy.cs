using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace System.ServiceModel.Discovery
{
	[MonoTODO]
	public abstract class DiscoveryProxy
	{
		protected DiscoveryProxy ()
		{
		}

		protected DiscoveryProxy (DiscoveryMessageSequenceGenerator messageSequenceGenerator)
		{
		}

		protected DiscoveryProxy (DiscoveryMessageSequenceGenerator messageSequenceGenerator, int duplicateMessageHistoryLength)
		{
		}

		protected virtual IAsyncResult BeginShouldRedirectFind (FindCriteria resolveCriteria, AsyncCallback callback, Object state)
		{
			throw new NotImplementedException ();
		}

		protected virtual IAsyncResult BeginShouldRedirectResolve (ResolveCriteria findCriteria, AsyncCallback callback, Object state)
		{
			throw new NotImplementedException ();
		}

		protected virtual bool EndShouldRedirectFind (IAsyncResult result, out Collection<EndpointDiscoveryMetadata> redirectionEndpoints)
		{
			throw new NotImplementedException ();
		}

		protected virtual bool EndShouldRedirectResolve (IAsyncResult result, out Collection<EndpointDiscoveryMetadata> redirectionEndpoints)
		{
			throw new NotImplementedException ();
		}

		protected abstract IAsyncResult OnBeginFind (FindRequestContext findRequestContext, AsyncCallback callback, Object state);
		protected abstract IAsyncResult OnBeginOfflineAnnouncement (DiscoveryMessageSequence messageSequence, EndpointDiscoveryMetadata endpointDiscoveryMetadata, AsyncCallback callback, Object state);
		protected abstract IAsyncResult OnBeginOnlineAnnouncement (DiscoveryMessageSequence messageSequence, EndpointDiscoveryMetadata endpointDiscoveryMetadata, AsyncCallback callback, Object state);
		protected abstract IAsyncResult OnBeginResolve (ResolveCriteria resolveCriteria, AsyncCallback callback, Object state);
		protected abstract void OnEndFind (IAsyncResult result);
		protected abstract void OnEndOfflineAnnouncement (IAsyncResult result);
		protected abstract void OnEndOnlineAnnouncement (IAsyncResult result);
		protected abstract EndpointDiscoveryMetadata OnEndResolve (IAsyncResult result);

	}
}
