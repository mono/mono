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
	public abstract class ChannelFactoryBase<TChannel>
		: ChannelFactoryBase, IChannelFactory<TChannel>
	{
		[MonoTODO]
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
			EndpointAddress remoteAddress)
		{
			return CreateChannel (remoteAddress, null);
		}

		public TChannel CreateChannel (
			EndpointAddress remoteAddress, Uri via)
		{
			ValidateCreateChannel ();
			return OnCreateChannel (remoteAddress, via);
		}

		protected abstract TChannel OnCreateChannel (
			EndpointAddress remoteAddress, Uri via);

		[MonoTODO ("find out what to do here.")]
		protected override void OnAbort ()
		{
			base.OnAbort ();
		}

		[MonoTODO ("find out what to do here.")]
		protected override void OnClose (TimeSpan timeout)
		{
			base.OnClose (timeout);
		}

		[MonoTODO ("find out what to do here.")]
		protected override IAsyncResult OnBeginClose (TimeSpan timeout, AsyncCallback callback, object state)
		{
			return base.OnBeginClose (timeout, callback, state);
		}

		[MonoTODO ("find out what to do here.")]
		protected override void OnEndClose (IAsyncResult result)
		{
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

		[MonoTODO]
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

		[MonoTODO ("find out what to do here.")]
		protected override void OnAbort ()
		{
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

		[MonoTODO]
		protected override void OnClose (TimeSpan timeout)
		{
		}
	}
}
