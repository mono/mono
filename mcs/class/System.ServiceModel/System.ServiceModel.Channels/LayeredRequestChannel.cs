//
// LayeredRequestChannel.cs
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
	internal abstract class LayeredRequestChannel : LayeredCommunicationObject, IRequestChannel
	{
		IRequestChannel inner;

		public LayeredRequestChannel (IRequestChannel source)
			: base (source)
		{
			inner = source;
		}

		public abstract ChannelFactoryBase Factory { get; }

		public override ChannelManagerBase ChannelManager {
			get { return Factory; }
		}

		// IRequestChannel
		public virtual EndpointAddress RemoteAddress {
			get { return inner.RemoteAddress; }
		}

		public IAsyncResult BeginRequest (Message message, AsyncCallback callback, object state)
		{
			// FIXME: send + receive?
			return BeginRequest (message, Factory.DefaultSendTimeout, callback, state);
		}

		public virtual IAsyncResult BeginRequest (Message message, TimeSpan timeout, AsyncCallback callback, object state)
		{
			ThrowIfNotOpen ();
			return OnBeginRequest (message, timeout, callback, state);
		}

		protected virtual IAsyncResult OnBeginRequest (Message message, TimeSpan timeout, AsyncCallback callback, object state)
		{
			return inner.BeginRequest (message, timeout, callback, state);
		}

		public Message EndRequest (IAsyncResult result)
		{
			return OnEndRequest (result);
		}

		protected virtual Message OnEndRequest (IAsyncResult result)
		{
			return inner.EndRequest (result);
		}

		public Message Request (Message message)
		{
			// FIXME: send + receive?
			return Request (message, Factory.DefaultSendTimeout);
		}

		public Message Request (Message message, TimeSpan timeout)
		{
			ThrowIfNotOpen ();
			return OnRequest (message, timeout);
		}

		protected virtual Message OnRequest (Message message, TimeSpan timeout)
		{
			return inner.Request (message, timeout);
		}

		public virtual Uri Via {
			get { return inner.Via; }
		}

		// IChannel

		public virtual T GetProperty<T> () where T : class
		{
			return inner.GetProperty<T> ();
		}
	}
}
