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
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace System.ServiceModel.Discovery.Version11
{
	internal class DiscoveryProxyClient11 : ClientBase<IDiscoveryProxyContract11>, DiscoveryClient.IDiscoveryCommon
	{
		public IAsyncResult BeginFind (FindCriteria criteria, AsyncCallback callback, object state)
		{
			var req = new MessageContracts11.FindRequest () { Body = new FindCriteria11 (criteria) };
			return Channel.BeginFind (req, callback, state);
		}
		
		public FindResponse EndFind (IAsyncResult result)
		{
			var ir = Channel.EndFind (result);
			var ret = new FindResponse ();
			foreach (var fr in ir.Body)
				ret.Endpoints.Add (fr.ToEndpointDiscoveryMetadata ());
			return ret;
		}

		public IAsyncResult BeginResolve (ResolveCriteria criteria, AsyncCallback callback, object state)
		{
			var req = new MessageContracts11.ResolveRequest () { Body = new ResolveCriteria11 (criteria) };
			return Channel.BeginResolve (req, callback, state);
		}

		public ResolveResponse EndResolve (IAsyncResult result)
		{
			var ir = Channel.EndResolve (result);
			var metadata = ir.Body.ToEndpointDiscoveryMetadata ();
			var sequence = ir.MessageSequence.ToDiscoveryMessageSequence ();
			return new ResolveResponse (metadata, sequence);
		}
	}
}
