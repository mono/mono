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
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace System.ServiceModel.Discovery
{
	internal class DiscoveryRequestChannel : RequestChannelBase
	{
		public DiscoveryRequestChannel (DiscoveryChannelFactory<IRequestChannel> factory, EndpointAddress address, Uri via)
			: base (factory, address, via)
		{
			this.factory = factory;
		}
		
		DiscoveryChannelFactory<IRequestChannel> factory;
		IRequestChannel inner;
		DiscoveryClient client;

		protected override void OnOpen (TimeSpan timeout)
		{
			// FIXME: use timeout
			client = new DiscoveryClient (factory.Source.DiscoveryEndpointProvider.GetDiscoveryEndpoint ());
			var res = client.Find (factory.Source.FindCriteria);

			foreach (var edm in res.Endpoints) {
				try {
					// FIXME: find scheme-matching ListenUri
					inner = factory.InnerFactory.CreateChannel (edm.Address, edm.ListenUris.FirstOrDefault (u => true) ?? edm.Address.Uri);
					return;
				} catch (Exception) {
				}
			}
			throw new EndpointNotFoundException (String.Format ("Could not find usable endpoint in {0} endpoints returned by the discovery service.", res.Endpoints.Count));
		}

		protected override void OnClose (TimeSpan timeout)
		{
			if (inner != null) {
				inner.Close (timeout);
				inner = null;
			}
		}

		protected override void OnAbort ()
		{
			if (inner != null) {
				inner.Abort ();
				inner = null;
			}
		}

		public override Message Request (Message input, TimeSpan timeout)
		{
			ThrowIfDisposedOrNotOpen ();
			return inner.Request (input, timeout);
		}
	}
}
