//
// RequestChannelBase.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2006 Novell, Inc.  http://www.novell.com
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
using System.IO;
using System.Net;
using System.Net.Security;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Security;

namespace System.ServiceModel.Channels
{
	internal abstract class RequestChannelBase : ChannelBase, IRequestChannel
	{
		ChannelFactoryBase channel_factory;
		EndpointAddress address;
		Uri via;

		public RequestChannelBase (ChannelFactoryBase factory, EndpointAddress address, Uri via)
			: base (factory)
		{
			this.channel_factory = factory;
			this.address = address;
			this.via = via;
		}

		protected internal override TimeSpan DefaultCloseTimeout {
			get { return channel_factory.DefaultCloseTimeout; }
		}

		protected internal override TimeSpan DefaultOpenTimeout {
			get { return channel_factory.DefaultOpenTimeout; }
		}

		public EndpointAddress RemoteAddress {
			get { return address; }
		}

		public Uri Via {
			get { return via ?? RemoteAddress.Uri; }
		}

		public override T GetProperty<T> ()
		{
			Console.Error.WriteLine (typeof (T));
			if (typeof (T) == typeof (IChannelFactory))
				return (T) (object) channel_factory;
			return base.GetProperty<T> ();
		}

		// Request

		public Message Request (Message message)
		{
			return Request (message, DefaultSendTimeout);
		}

		public abstract Message Request (Message message, TimeSpan timeout);

		public IAsyncResult BeginRequest (Message message, AsyncCallback callback, object state)
		{
			return BeginRequest (message, DefaultSendTimeout, callback, state);
		}

		Func<Message,TimeSpan,Message> request_delegate;

		public virtual IAsyncResult BeginRequest (Message message, TimeSpan timeout, AsyncCallback callback, object state)
		{
			if (request_delegate == null)
				request_delegate = new Func<Message,TimeSpan,Message> (Request);
			return request_delegate.BeginInvoke (message, timeout, callback, state);
		}

		public virtual Message EndRequest (IAsyncResult result)
		{
			return request_delegate.EndInvoke (result);
		}

		// Open and Close
		Action<TimeSpan> open_delegate;

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

		Action<TimeSpan> close_delegate;

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
	}
}
