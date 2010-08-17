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
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace System.ServiceModel.Discovery
{
	internal class DiscoveryChannelDispatcher : ChannelDispatcherBase
	{
		ServiceHostBase host;
		AnnouncementClient client;
		bool online;

		// FIXME: use online flag (to differentiate Hello and Bye dispatchers)
		public DiscoveryChannelDispatcher (AnnouncementEndpoint endpoint, bool online)
		{
			this.client = new AnnouncementClient (endpoint);
			this.online = online;
		}

		public ICommunicationObject Communication {
			get { return client; }
		}

		public override ServiceHostBase Host {
			get { return host; }
		}

		public override IChannelListener Listener {
			get { return null; }
		}

		// might be different value
		protected override TimeSpan DefaultOpenTimeout {
			get { return TimeSpan.FromMinutes (1); }
		}

		// might be different value
		protected override TimeSpan DefaultCloseTimeout {
			get { return TimeSpan.FromMinutes (1); }
		}

		protected override void Attach (ServiceHostBase host)
		{
			base.Attach (host);
			this.host = host;
		}

		protected override void Detach (ServiceHostBase host)
		{
			base.Detach (host);
			this.host = null;
		}

		protected override void OnOpen (TimeSpan timeout)
		{
			Communication.Open (timeout);
		}

		protected override IAsyncResult OnBeginOpen (TimeSpan timeout, AsyncCallback callback, object state)
		{
			return Communication.BeginOpen (timeout, callback, state);
		}

		protected override void OnEndOpen (IAsyncResult result)
		{
			Communication.EndOpen (result);
		}

		protected override void OnClose (TimeSpan timeout)
		{
			Communication.Close (timeout);
		}

		protected override IAsyncResult OnBeginClose (TimeSpan timeout, AsyncCallback callback, object state)
		{
			return Communication.BeginClose (timeout, callback, state);
		}

		protected override void OnEndClose (IAsyncResult result)
		{
			Communication.EndClose (result);
		}
		
		protected override void OnAbort ()
		{
			Communication.Abort ();
		}
	}
}
