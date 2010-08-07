using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace System.ServiceModel.Discovery
{
	public abstract class DiscoveryService
	{
		protected DiscoveryService ()
			: this (new DiscoveryMessageSequenceGenerator ())
		{
		}

		protected DiscoveryService (DiscoveryMessageSequenceGenerator discoveryMessageSequenceGenerator)
			: this (discoveryMessageSequenceGenerator, 0)
		{
		}

		protected DiscoveryService (DiscoveryMessageSequenceGenerator discoveryMessageSequenceGenerator, int duplicateMessageHistoryLength)
		{
			DiscoveryMessageSequenceGenerator = discoveryMessageSequenceGenerator;
			DuplicateMessageHistoryLength = duplicateMessageHistoryLength;
		}

		internal DiscoveryMessageSequenceGenerator DiscoveryMessageSequenceGenerator { get; private set; }

		internal int DuplicateMessageHistoryLength { get; private set; }

		protected abstract IAsyncResult OnBeginFind (FindRequestContext findRequestContext, AsyncCallback callback, Object state);

		protected abstract IAsyncResult OnBeginResolve (ResolveCriteria resolveCriteria, AsyncCallback callback, Object state);

		protected abstract void OnEndFind (IAsyncResult result);

		protected abstract EndpointDiscoveryMetadata OnEndResolve (IAsyncResult result);
	}

	internal class DefaultDiscoveryService : DiscoveryService
	{
		Action<FindRequestContext> find_delegate;
		Func<ResolveCriteria,EndpointDiscoveryMetadata> resolve_delegate;

		protected override IAsyncResult OnBeginFind (FindRequestContext findRequestContext, AsyncCallback callback, Object state)
		{
			if (find_delegate == null)
				find_delegate = new Action<FindRequestContext> (Find);
			return find_delegate.BeginInvoke (findRequestContext, callback, state);
		}

		protected override void OnEndFind (IAsyncResult result)
		{
			find_delegate.EndInvoke (result);
		}

		protected override IAsyncResult OnBeginResolve (ResolveCriteria resolveCriteria, AsyncCallback callback, Object state)
		{
			if (resolve_delegate == null)
				resolve_delegate = new Func<ResolveCriteria,EndpointDiscoveryMetadata> (Resolve);
			return resolve_delegate.BeginInvoke (resolveCriteria, callback, state);
		}

		protected override EndpointDiscoveryMetadata OnEndResolve (IAsyncResult result)
		{
			return resolve_delegate.EndInvoke (result);
		}

		void Find (FindRequestContext context)
		{
			throw new NotImplementedException ();
		}

		EndpointDiscoveryMetadata Resolve (ResolveCriteria criteria)
		{
			throw new NotImplementedException ();
		}
	}
}
