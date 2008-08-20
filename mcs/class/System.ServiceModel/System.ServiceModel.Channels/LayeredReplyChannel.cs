//
// SecurityReplyChannel.cs
//
// Author: Atsushi Enomoto (atsushi@ximian.com)
//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
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
using System.ServiceModel;

namespace System.ServiceModel.Channels
{
	internal abstract class LayeredReplyChannel : LayeredCommunicationObject, IReplyChannel
	{
		IReplyChannel inner;

		public LayeredReplyChannel (IReplyChannel innerChannel)
			: base (innerChannel)
		{
			inner = innerChannel;
		}

		public abstract ChannelListenerBase Listener { get; }

		public override ChannelManagerBase ChannelManager {
			get { return Listener; }
		}

		// IReplyChannel

		public virtual EndpointAddress LocalAddress {
			get { return inner.LocalAddress; }
		}

		public virtual IAsyncResult BeginReceiveRequest (
			AsyncCallback callback, object state)
		{
			return BeginReceiveRequest (Listener.DefaultReceiveTimeout, callback, state);
		}

		public virtual IAsyncResult BeginReceiveRequest (
			TimeSpan timeout, AsyncCallback callback, object state)
		{
			return inner.BeginReceiveRequest (timeout, callback, state);
		}

		public virtual IAsyncResult BeginTryReceiveRequest (
			TimeSpan timeout, AsyncCallback callback, object state)
		{
			return inner.BeginTryReceiveRequest (timeout, callback, state);
		}

		public virtual IAsyncResult BeginWaitForRequest (
			TimeSpan timeout, AsyncCallback callback, object state)
		{
			return inner.BeginWaitForRequest (timeout, callback, state);
		}

		public virtual RequestContext EndReceiveRequest (IAsyncResult result)
		{
			return inner.EndReceiveRequest (result);
		}

		public virtual bool EndTryReceiveRequest (IAsyncResult result, out RequestContext context)
		{
			return inner.EndTryReceiveRequest (result, out context);
		}

		public virtual bool EndWaitForRequest (IAsyncResult result)
		{
			return inner.EndWaitForRequest (result);
		}

		public virtual RequestContext ReceiveRequest ()
		{
			return ReceiveRequest (Listener.DefaultReceiveTimeout);
		}

		public virtual RequestContext  ReceiveRequest (TimeSpan timeout)
		{
			return inner.ReceiveRequest (timeout);
		}

		public virtual bool TryReceiveRequest (TimeSpan timeout, out RequestContext context)
		{
			return inner.TryReceiveRequest (timeout, out context);
		}

		public virtual bool WaitForRequest (TimeSpan timeout)
		{
			return inner.WaitForRequest (timeout);
		}

		// IChannel

		public virtual T GetProperty<T> () where T : class
		{
			return inner.GetProperty<T> ();
		}
	}
}
