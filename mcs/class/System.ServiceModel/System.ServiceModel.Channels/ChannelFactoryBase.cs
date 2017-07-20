//
// ChannelFactoryBase.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
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
using System.ServiceModel.Dispatcher;

namespace System.ServiceModel.Channels
{
	internal interface IHasMessageEncoder
	{
		MessageEncoder MessageEncoder { get; }
	}

	internal abstract class TransportChannelFactoryBase<TChannel> : ChannelFactoryBase<TChannel>, IHasMessageEncoder
	{
		protected TransportChannelFactoryBase (TransportBindingElement source, BindingContext ctx)
		{
			Transport = source;
		}

		public TransportBindingElement Transport { get; private set; }

		public MessageEncoder MessageEncoder { get; internal set; }

		Action<TimeSpan> open_delegate;

		protected override IAsyncResult OnBeginOpen (TimeSpan timeout,
			AsyncCallback callback, object state)
		{
			if (open_delegate == null)
				open_delegate = new Action<TimeSpan> (OnOpen);
			return open_delegate.BeginInvoke (timeout, callback, state);
		}

		protected override void OnEndOpen (IAsyncResult result)
		{
			if (open_delegate == null)
				throw new InvalidOperationException ("Async open operation has not started");
			open_delegate.EndInvoke (result);
		}

		protected override void OnOpen (TimeSpan timeout)
		{
		}

		/* commented out as it is in doubt.
		public override T GetProperty<T> ()
		{
			if (typeof (T) == typeof (MessageVersion))
				return (T) (object) MessageEncoder.MessageVersion;
			return base.GetProperty<T> ();
		}
		*/
	}

	public abstract class ChannelFactoryBase<TChannel>
		: ChannelFactoryBase, IChannelFactory<TChannel>
	{
		List<TChannel> channels = new List<TChannel> ();

		protected ChannelFactoryBase ()
			: this (DefaultCommunicationTimeouts.Instance)
		{
		}

		protected ChannelFactoryBase (
			IDefaultCommunicationTimeouts timeouts)
			: base (timeouts)
		{
		}

		public TChannel CreateChannel (
			EndpointAddress address)
		{
			if (address == null)
				throw new ArgumentNullException ("address");
			return CreateChannel (address, address.Uri);
		}

		public TChannel CreateChannel (
			EndpointAddress address, Uri via)
		{
			if (address == null)
				throw new ArgumentNullException ("address");
			if (via == null)
				throw new ArgumentNullException ("via");

			ValidateCreateChannel ();
			var ch = OnCreateChannel (address, via);
			channels.Add (ch);
			return ch;
		}

		protected abstract TChannel OnCreateChannel (
			EndpointAddress address, Uri via);

		protected override void OnAbort ()
		{
			// this implicitly premises: TChannel is IChannel
			foreach (IChannel ch in channels)
				ch.Abort ();
			base.OnAbort ();
		}

		protected override void OnClose (TimeSpan timeout)
		{
			DateTime start = DateTime.UtcNow;
			// this implicitly premises: TChannel is IChannel
			foreach (IChannel ch in channels)
				ch.Close (timeout - (DateTime.UtcNow - start));
			base.OnClose (timeout - (DateTime.UtcNow - start));
		}

		protected override IAsyncResult OnBeginClose (TimeSpan timeout, AsyncCallback callback, object state)
		{
			// base impl. will call this.OnClose()
			// FIXME: use async BeginClose/EndClose on the channels.
			return base.OnBeginClose (timeout, callback, state);
		}

		protected override void OnEndClose (IAsyncResult result)
		{
			// base impl. will call this.OnClose()
			base.OnEndClose (result);
		}

		protected void ValidateCreateChannel ()
		{
			ThrowIfDisposedOrNotOpen ();
			if (State == CommunicationState.Faulted)
				throw new CommunicationObjectFaultedException ();
		}
	}

	public abstract class ChannelFactoryBase
		: ChannelManagerBase, IChannelFactory, ICommunicationObject
	{
		TimeSpan open_timeout, close_timeout, receive_timeout, send_timeout;

		protected ChannelFactoryBase ()
			: this (DefaultCommunicationTimeouts.Instance)
		{
		}

		protected ChannelFactoryBase (
			IDefaultCommunicationTimeouts timeouts)
		{
			open_timeout = timeouts.OpenTimeout;
			close_timeout = timeouts.CloseTimeout;
			send_timeout = timeouts.SendTimeout;
			receive_timeout = timeouts.ReceiveTimeout;
		}

		protected internal override TimeSpan DefaultCloseTimeout {
			get { return close_timeout; }
		}

		protected internal override TimeSpan DefaultOpenTimeout {
			get { return open_timeout; }
		}

		protected internal override TimeSpan DefaultReceiveTimeout {
			get { return receive_timeout; }
		}

		protected internal override TimeSpan DefaultSendTimeout {
			get { return send_timeout; }
		}

		public virtual T GetProperty<T> () where T : class
		{
			return null;
		}

		protected override void OnAbort ()
		{
			// what should we do here?
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
			if (close_delegate == null)
				throw new InvalidOperationException ("Async close operation has not started");
			close_delegate.EndInvoke (result);
		}

		protected override void OnClose (TimeSpan timeout)
		{
			// what should we do here?
		}
	}
}
