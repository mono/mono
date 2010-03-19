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
	public abstract class DiscoveryService
	{
		protected DiscoveryService ()
		{
		}

		protected DiscoveryService (DiscoveryMessageSequenceGenerator discoveryMessageSequenceGenerator)
		{
		}

		protected DiscoveryService (DiscoveryMessageSequenceGenerator discoveryMessageSequenceGenerator, int duplicateMessageHistoryLength)
		{
		}

		protected abstract IAsyncResult OnBeginFind (FindRequestContext findRequestContext, AsyncCallback callback, Object state);

		protected abstract IAsyncResult OnBeginResolve (ResolveCriteria resolveCriteria, AsyncCallback callback, Object state);

		protected abstract void OnEndFind (IAsyncResult result);

		protected abstract EndpointDiscoveryMetadata OnEndResolve (IAsyncResult result);
	}
}
