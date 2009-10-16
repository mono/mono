//
// ChannelListenerBase.cs
//
// Author: Atsushi Enomoto (atsushi@ximian.com)
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

namespace System.ServiceModel.Channels
{
	[MonoTODO]
	public abstract class ChannelListenerBase : ChannelManagerBase, 
		IChannelListener, ICommunicationObject,
		IDefaultCommunicationTimeouts
	{
		IDefaultCommunicationTimeouts timeouts;
		KeyedByTypeCollection<object> properties;

		protected ChannelListenerBase ()
			: this (DefaultCommunicationTimeouts.Instance)
		{
		}

		protected ChannelListenerBase (
			IDefaultCommunicationTimeouts timeouts)
		{
			this.timeouts = timeouts;
		}

		public abstract Uri Uri { get; }

		protected internal override TimeSpan DefaultCloseTimeout {
			get { return timeouts.CloseTimeout; }
		}

		protected internal override TimeSpan DefaultOpenTimeout {
			get { return timeouts.OpenTimeout; }
		}

		protected internal override TimeSpan DefaultReceiveTimeout {
			get { return timeouts.ReceiveTimeout; }
		}

		protected internal override TimeSpan DefaultSendTimeout {
			get { return timeouts.SendTimeout; }
		}

		TimeSpan IDefaultCommunicationTimeouts.CloseTimeout {
			get { return timeouts.CloseTimeout; }
		}

		TimeSpan IDefaultCommunicationTimeouts.OpenTimeout {
			get { return timeouts.OpenTimeout; }
		}

		TimeSpan IDefaultCommunicationTimeouts.ReceiveTimeout {
			get { return timeouts.ReceiveTimeout; }
		}

		TimeSpan IDefaultCommunicationTimeouts.SendTimeout {
			get { return timeouts.SendTimeout; }
		}

		internal KeyedByTypeCollection<object> Properties {
			get {
				if (properties == null)
					properties = new KeyedByTypeCollection<object> ();
				return properties;
			}
		}

		public virtual T GetProperty<T> () where T : class
		{
			return properties != null ? properties.Find<T> () : null;
		}

		public IAsyncResult BeginWaitForChannel (
			TimeSpan timeout, AsyncCallback callback, object state)
		{
			return OnBeginWaitForChannel (timeout, callback, state);
		}

		public bool EndWaitForChannel (IAsyncResult result)
		{
			return OnEndWaitForChannel (result);
		}

		public bool WaitForChannel (TimeSpan timeout)
		{
			return OnWaitForChannel (timeout);
		}

		protected abstract IAsyncResult OnBeginWaitForChannel (
			TimeSpan timeout, AsyncCallback callback, object state);

		protected abstract bool OnEndWaitForChannel (IAsyncResult result);

		protected abstract bool OnWaitForChannel (TimeSpan timeout);
	}
}
