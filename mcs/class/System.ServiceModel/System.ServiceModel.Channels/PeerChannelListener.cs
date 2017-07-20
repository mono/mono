//
// PeerChannelListener.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2009 Novell, Inc.  http://www.novell.com
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
using System.Net;
using System.Net.Security;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using System.Text;
using System.Threading;

namespace System.ServiceModel.Channels
{
	internal class PeerChannelListener<TChannel> : InternalChannelListenerBase<TChannel>, IPeerChannelManager
		where TChannel : class, IChannel
	{
		PeerTransportBindingElement source;
		BindingContext context;
		TChannel channel;
		AutoResetEvent accept_handle = new AutoResetEvent (false);

		public PeerChannelListener (PeerTransportBindingElement source,
			BindingContext context)
			: base (context)
		{
			this.source = source;
			foreach (BindingElement be in context.Binding.Elements) {
				MessageEncodingBindingElement mbe = be as MessageEncodingBindingElement;
				if (mbe != null) {
					MessageEncoder = CreateEncoder<TChannel> (mbe);
					break;
				}
			}
			if (MessageEncoder == null)
				MessageEncoder = new BinaryMessageEncoder ();
		}

		public PeerResolver Resolver { get; set; }

		public PeerTransportBindingElement Source {
			get { return source; }
		}

		protected override TChannel OnAcceptChannel (TimeSpan timeout)
		{
			DateTime start = DateTime.UtcNow;
			if (channel != null)
				if (!accept_handle.WaitOne (timeout))
					throw new TimeoutException ();
			channel = PopulateChannel (timeout - (DateTime.UtcNow - start));
			((CommunicationObject) (object) channel).Closed += delegate {
				this.channel = null;
				accept_handle.Set ();
				};
			return channel;
		}

		TChannel PopulateChannel (TimeSpan timeout)
		{
			if (typeof (TChannel) == typeof (IInputChannel))
				return (TChannel) (object) new PeerDuplexChannel (this);
			// FIXME: handle timeout somehow.
			if (typeof (TChannel) == typeof (IDuplexChannel))
				return (TChannel) (object) new PeerDuplexChannel (this);

			throw new InvalidOperationException (String.Format ("Not supported channel '{0}' (mono bug; it is incorrectly allowed at construction time)", typeof (TChannel)));
		}

		protected override bool OnWaitForChannel (TimeSpan timeout)
		{
			throw new NotImplementedException ();
		}

		protected override void OnOpen (TimeSpan timeout)
		{
		}

		protected override void OnClose (TimeSpan timeout)
		{
			if (channel != null)
				channel.Close (timeout);
		}

		protected override void OnAbort ()
		{
			if (channel != null)
				channel.Abort ();
		}
	}
}
