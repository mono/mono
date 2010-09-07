//
// Author: Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2009,2010 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Threading;

namespace System.ServiceModel.Discovery.Version11
{
	internal class DiscoveryTargetClient11 : DuplexClientBase<IDiscoveryTargetContract11>, DiscoveryClient.IDiscoveryCommon
	{
		public DiscoveryTargetClient11 (ServiceEndpoint endpoint)
			: this (new DiscoveryTargetCallback11 (), endpoint)
		{
		}

		DiscoveryTargetClient11 (DiscoveryTargetCallback11 instance, ServiceEndpoint endpoint)
			: base (instance, endpoint)
		{
			instance.ReplyFindCompleted += delegate (MessageContracts11.FindResponse response) {
				find_completed = delegate { return response; };
				reply_find_handle.Set ();
			};
			instance.ReplyResolveCompleted += delegate (MessageContracts11.ResolveResponse response) {
				resolve_completed = delegate { return response; };
				reply_resolve_handle.Set ();
			};
		}

		// Find

		Func<FindCriteria,FindResponse> find_delegate;
		Func<MessageContracts11.FindResponse> find_completed;
		ManualResetEvent reply_find_handle = new ManualResetEvent (false);

		public IAsyncResult BeginFind (FindCriteria criteria, AsyncCallback callback, object state)
		{
			if (find_delegate == null)
				find_delegate = new Func<FindCriteria,FindResponse> (Find);
			return find_delegate.BeginInvoke (criteria, callback, state);
		}
		
		public FindResponse EndFind (IAsyncResult result)
		{
			return find_delegate.EndInvoke (result);
		}
		
		FindResponse Find (FindCriteria criteria)
		{
			var req = new MessageContracts11.FindRequest () { Body = new FindCriteria11 (criteria) };
			Channel.BeginFind (req, delegate (IAsyncResult result) {
				Channel.EndFind (result);
			}, null);
			
			if (!reply_find_handle.WaitOne (InnerChannel.OperationTimeout))
				throw new EndpointNotFoundException ("The discovery client could not receive Find operation response within the operation timeout.");
			try {
				var ir = find_completed ();
				var ret = new FindResponse ();
				foreach (var fr in ir.Body)
					ret.Endpoints.Add (fr.ToEndpointDiscoveryMetadata ());
				return ret;
			} finally {
				find_completed = null;
			}
		}

		// Resolve

		Func<ResolveCriteria,ResolveResponse> resolve_delegate;
		Func<MessageContracts11.ResolveResponse> resolve_completed;
		ManualResetEvent reply_resolve_handle = new ManualResetEvent (false);

		public IAsyncResult BeginResolve (ResolveCriteria criteria, AsyncCallback callback, object state)
		{
			if (resolve_delegate == null)
				resolve_delegate = new Func<ResolveCriteria,ResolveResponse> (Resolve);
			return resolve_delegate.BeginInvoke (criteria, callback, state);
		}
		
		public ResolveResponse EndResolve (IAsyncResult result)
		{
			return resolve_delegate.EndInvoke (result);
		}
		
		public ResolveResponse Resolve (ResolveCriteria criteria)
		{
			var req = new MessageContracts11.ResolveRequest () { Body = new ResolveCriteria11 (criteria) };
			Channel.BeginResolve (req, delegate (IAsyncResult result) {
				Channel.EndResolve (result);
			}, null);

			if (!reply_resolve_handle.WaitOne (InnerChannel.OperationTimeout))
				throw new TimeoutException ();
			try {
				var ir = resolve_completed ();
				var metadata = ir.Body.ToEndpointDiscoveryMetadata ();
				var sequence = ir.MessageSequence.ToDiscoveryMessageSequence ();
				return new ResolveResponse (metadata, sequence);
			} finally {
				resolve_completed = null;
			}
		}
	
		internal class DiscoveryTargetCallback11 : IDiscoveryTargetCallbackContract11
		{
			public event Action<MessageContracts11.FindResponse> ReplyFindCompleted;
			public event Action<MessageContracts11.ResolveResponse> ReplyResolveCompleted;

			public void ReplyFind (MessageContracts11.FindResponse message)
			{
				if (ReplyFindCompleted != null)
					ReplyFindCompleted (message);
			}

			public void ReplyResolve (MessageContracts11.ResolveResponse message)
			{
				if (ReplyResolveCompleted != null)
					ReplyResolveCompleted (message);
			}
		}
	}
}
