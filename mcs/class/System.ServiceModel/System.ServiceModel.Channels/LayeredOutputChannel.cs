//
// LayeredOutputChannel.cs
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
	internal abstract class LayeredOutputChannel : LayeredCommunicationObject, IOutputChannel
	{
		IOutputChannel inner;

		public LayeredOutputChannel (IOutputChannel source)
			: base (source)
		{
			inner = source;
		}

		public abstract ChannelFactoryBase Factory { get; }

		public override ChannelManagerBase ChannelManager {
			get { return Factory; }
		}

		// IOutputChannel
		public virtual EndpointAddress RemoteAddress {
			get { return inner.RemoteAddress; }
		}

		public IAsyncResult BeginSend (Message message, AsyncCallback callback, object state)
		{
			// FIXME: send + receive?
			return BeginSend (message, Factory.DefaultSendTimeout, callback, state);
		}

		public virtual IAsyncResult BeginSend (Message message, TimeSpan timeout, AsyncCallback callback, object state)
		{
			ThrowIfNotOpen ();
			return OnBeginSend (message, timeout, callback, state);
		}

		protected virtual IAsyncResult OnBeginSend (Message message, TimeSpan timeout, AsyncCallback callback, object state)
		{
			return inner.BeginSend (message, timeout, callback, state);
		}

		public void EndSend (IAsyncResult result)
		{
			OnEndSend (result);
		}

		protected virtual void OnEndSend (IAsyncResult result)
		{
			inner.EndSend (result);
		}

		public void Send (Message message)
		{
			// FIXME: send + receive?
			Send (message, Factory.DefaultSendTimeout);
		}

		public void Send (Message message, TimeSpan timeout)
		{
			ThrowIfNotOpen ();
			OnSend (message, timeout);
		}

		protected virtual void OnSend (Message message, TimeSpan timeout)
		{
			inner.Send (message, timeout);
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
