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
//* Strange, but this causes compiler error at DiscoveryClientBindingElement.

	internal class DiscoveryChannel<TChannel> : DiscoveryChannelBase, IRequestSessionChannel, IDuplexSessionChannel
	{
		DiscoveryChannelFactory<TChannel> factory;
		TChannel inner;

		public DiscoveryChannel (DiscoveryChannelFactory<TChannel> factory, EndpointAddress address, Uri via)
			: base (factory)
		{
			this.factory = factory;
			RemoteAddress = address;
			Via = via;
		}

		public EndpointAddress RemoteAddress { get; private set; }
		public Uri Via { get; private set; }
		public EndpointAddress LocalAddress {
			get { return ((IDuplexSessionChannel) inner).LocalAddress; }
		}

		IDuplexSession ISessionChannel<IDuplexSession>.Session {
			get { return ((IDuplexSessionChannel) inner).Session; }
		}

		IOutputSession ISessionChannel<IOutputSession>.Session {
			get { return ((IOutputSessionChannel) inner).Session; }
		}

		Action<TimeSpan> open_delegate, close_delegate;

		protected override IAsyncResult OnBeginClose (TimeSpan timeout, AsyncCallback callback, object state)
		{
			if (close_delegate == null)
				close_delegate = new Action<TimeSpan> (OnClose);
			return close_delegate.BeginInvoke (timeout, callback, state);
		}

		protected override void OnEndClose (IAsyncResult result)
		{
			close_delegate.EndInvoke (result);
		}

		protected override void OnAbort ()
		{
			if (inner != null) {
				((IChannel) inner).Abort ();
				inner = default (TChannel);
			}
		}

		protected override void OnClose (TimeSpan timeout)
		{
			if (inner != null) {
				((IChannel) inner).Close (timeout);
				inner = default (TChannel);
			}
		}

		protected override IAsyncResult OnBeginOpen (TimeSpan timeout, AsyncCallback callback, object state)
		{
			if (open_delegate == null)
				open_delegate = new Action<TimeSpan> (OnOpen);
			return open_delegate.BeginInvoke (timeout, callback, state);
		}

		protected override void OnEndOpen (IAsyncResult result)
		{
			open_delegate.EndInvoke (result);
		}

		protected override void OnOpen (TimeSpan timeout)
		{
			// FIXME: use timeout
			DateTime start = DateTime.UtcNow;
			inner = CreateDiscoveryInnerChannel<TChannel> (factory);
			((IChannel) inner).Open (timeout - (DateTime.UtcNow - start));
		}

		public Message Request (Message msg)
		{
			return Request (msg, DefaultSendTimeout + DefaultReceiveTimeout);
		}

		public Message Request (Message msg, TimeSpan timeout)
		{
			return ((IRequestChannel) inner).Request (msg, timeout);
		}

		public IAsyncResult BeginRequest (Message msg, AsyncCallback callback, object state)
		{
			return BeginRequest (msg, DefaultSendTimeout + DefaultReceiveTimeout, callback, state);
		}

		public IAsyncResult BeginRequest (Message msg, TimeSpan timeout, AsyncCallback callback, object state)
		{
			return ((IRequestChannel) inner).BeginRequest (msg, timeout, callback, state);
		}

		public Message EndRequest (IAsyncResult result)
		{
			return ((IRequestChannel) inner).EndRequest (result);
		}

		public Message Receive ()
		{
			return Receive (DefaultReceiveTimeout);
		}

		public Message Receive (TimeSpan timeout)
		{
			return ((IInputChannel) inner).Receive (timeout);
		}

		public IAsyncResult BeginReceive (AsyncCallback callback, object state)
		{
			return BeginReceive (DefaultReceiveTimeout, callback, state);
		}

		public IAsyncResult BeginReceive (TimeSpan timeout, AsyncCallback callback, object state)
		{
			return ((IInputChannel) inner).BeginReceive (timeout, callback, state);
		}

		public Message EndReceive (IAsyncResult result)
		{
			return ((IInputChannel) inner).EndReceive (result);
		}

		public bool TryReceive (out Message msg)
		{
			return TryReceive (DefaultReceiveTimeout, out msg);
		}

		public bool TryReceive (TimeSpan timeout, out Message msg)
		{
			return ((IInputChannel) inner).TryReceive (timeout, out msg);
		}

		public IAsyncResult BeginTryReceive (TimeSpan timeout, AsyncCallback callback, object state)
		{
			return ((IInputChannel) inner).BeginTryReceive (timeout, callback, state);
		}

		public bool EndTryReceive (IAsyncResult result, out Message msg)
		{
			return ((IInputChannel) inner).EndTryReceive (result, out msg);
		}

		public bool WaitForMessage (TimeSpan timeout)
		{
			return ((IInputChannel) inner).WaitForMessage (timeout);
		}

		public IAsyncResult BeginWaitForMessage (TimeSpan timeout, AsyncCallback callback, object state)
		{
			return ((IInputChannel) inner).BeginWaitForMessage (timeout, callback, state);
		}

		public bool EndWaitForMessage (IAsyncResult result)
		{
			return ((IInputChannel) inner).EndWaitForMessage (result);
		}

		public void Send (Message msg)
		{
			Send (msg, DefaultSendTimeout);
		}

		public void Send (Message msg, TimeSpan timeout)
		{
			((IOutputChannel) inner).Send (msg, timeout);
		}

		public IAsyncResult BeginSend (Message msg, AsyncCallback callback, object state)
		{
			return BeginSend (msg, DefaultSendTimeout, callback, state);
		}

		public IAsyncResult BeginSend (Message msg, TimeSpan timeout, AsyncCallback callback, object state)
		{
			return ((IOutputChannel) inner).BeginSend (msg, timeout, callback, state);
		}
		
		public void EndSend (IAsyncResult result)
		{
			((IOutputChannel) inner).EndSend (result);
		}
	}
//*/

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
			inner = CreateDiscoveryInnerChannel<IRequestChannel> (factory);
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
