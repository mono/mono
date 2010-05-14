//
// MsmqChannelListener.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2007 Novell, Inc.  http://www.novell.com
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
using System.Messaging;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using System.Text;

namespace System.ServiceModel.Channels
{
	internal class MsmqChannelListener<TChannel> : ChannelListenerBase<TChannel>
		where TChannel : class, IChannel
	{
		MsmqTransportBindingElement source;
		BindingContext context;
		Uri listen_uri;
		MessageQueue queue;
		List<IChannel> channels = new List<IChannel> ();
		MessageEncoder encoder;

		public MsmqChannelListener (MsmqTransportBindingElement source,
			BindingContext context)
			: base (context.Binding)
		{
			if (context.ListenUriMode == ListenUriMode.Explicit)
				listen_uri = new Uri (context.ListenUriBaseAddress, context.ListenUriRelativeAddress);
			else
				// FIXME: consider ListenUriMode.Unique
				throw new NotImplementedException ();

			foreach (BindingElement be in context.Binding.Elements) {
				MessageEncodingBindingElement mbe = be as MessageEncodingBindingElement;
				if (mbe != null) {
					encoder = CreateEncoder<TChannel> (mbe);
					break;
				}
			}
			if (encoder == null)
				encoder = new BinaryMessageEncoder ();
		}

		public MessageQueue Queue {
			get { return queue; }
		}

		public MessageEncoder MessageEncoder {
			get { return encoder; }
		}

		public override Uri Uri {
			get { return listen_uri; }
		}

		protected override TChannel OnAcceptChannel (TimeSpan timeout)
		{
			TChannel ch = PopulateChannel (timeout);
			channels.Add (ch);
			return ch;
		}

		TChannel PopulateChannel (TimeSpan timeout)
		{
			if (typeof (TChannel) == typeof (IInputChannel)) {
				return (TChannel) (object) new MsmqInputChannel (
					(MsmqChannelListener<IInputChannel>) (object) this, timeout);
			}

			// FIXME: implement more

			throw new NotImplementedException ();
		}

		protected override IAsyncResult OnBeginAcceptChannel (
			TimeSpan timeout, AsyncCallback callback,
			object asyncState)
		{
			throw new NotImplementedException ();
		}

		protected override TChannel OnEndAcceptChannel (IAsyncResult result)
		{
			throw new NotImplementedException ();
		}

		protected override IAsyncResult OnBeginWaitForChannel (
			TimeSpan timeout, AsyncCallback callback, object state)
		{
			throw new NotImplementedException ();
		}

		protected override bool OnEndWaitForChannel (IAsyncResult result)
		{
			throw new NotImplementedException ();
		}

		protected override bool OnWaitForChannel (TimeSpan timeout)
		{
			throw new NotImplementedException ();
		}
		
		void StartListening (TimeSpan timeout)
		{
			if (queue != null)
				throw new InvalidOperationException ("This listener is already waiting for connection.");

			queue = new MessageQueue (listen_uri.GetLeftPart (UriPartial.Scheme));
		}

		protected override void OnOpen (TimeSpan timeout)
		{
			StartListening (timeout);
		}

		protected override IAsyncResult OnBeginOpen (TimeSpan timeout,
			AsyncCallback callback, object state)
		{
			throw new NotImplementedException ();
		}

		protected override void OnEndOpen (IAsyncResult result)
		{
			throw new NotImplementedException ();
		}

		protected override void OnClose (TimeSpan timeout)
		{
			// FIXME: somewhere to use timeout?
			if (queue == null)
				return;
			queue.Dispose ();
		}

		[MonoTODO]
		protected override IAsyncResult OnBeginClose (TimeSpan timeout,
			AsyncCallback callback, object state)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void OnEndClose (IAsyncResult result)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("find out what to do here.")]
		protected override void OnAbort ()
		{
		}
	}
}
