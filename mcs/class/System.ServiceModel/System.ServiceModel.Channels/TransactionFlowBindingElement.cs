//
// TransactionFlowBindingElement.cs
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
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using System.ServiceModel.Channels;
using System.Transactions;

namespace System.ServiceModel.Channels
{
	public class TransactionFlowBindingElement : BindingElement
	{
		TransactionProtocol protocol;

		// Funny, but since it uses OLE TX, Mono will never support this constructor.
		[MonoTODO]
		public TransactionFlowBindingElement ()
			: this (TransactionProtocol.Default)
		{
		}

		public TransactionFlowBindingElement (TransactionProtocol transactionProtocol)
		{
			this.protocol = transactionProtocol;
		}

		public TransactionProtocol TransactionProtocol {
			get { return protocol; }
		}

		public override BindingElement Clone ()
		{
			return new TransactionFlowBindingElement (protocol);
		}

		[MonoTODO]
		public override T GetProperty<T> (BindingContext context)
		{
			return context.GetInnerProperty<T> ();
		}

		public override bool CanBuildChannelFactory<TChannel> (BindingContext context)
		{
			return context.CanBuildInnerChannelFactory<TChannel> ();
		}

		[MonoTODO]
		public override bool CanBuildChannelListener<TChannel> (BindingContext context)
		{
			return context.CanBuildInnerChannelListener<TChannel> ();
		}

		public override IChannelFactory<TChannel> BuildChannelFactory<TChannel> (BindingContext context)
		{
			if (protocol == null)
				throw new InvalidOperationException ("Set transaction protocol in prior to build a channel factory.");
			if (protocol == TransactionProtocol.Default)
				throw new NotSupportedException ("Mono does not support DTC.");
			if (!CanBuildChannelFactory<TChannel> (context.Clone ()))
				throw new ArgumentException (String.Format ("The channel type '{0}' is not supported", typeof (TChannel)));
			return new TransactionChannelFactory<TChannel> (context.BuildInnerChannelFactory<TChannel> (), protocol);
		}

		public override IChannelListener<TChannel> BuildChannelListener<TChannel> (BindingContext context)
		{
			if (protocol == null)
				throw new InvalidOperationException ("Set transaction protocol in prior to build a channel listener.");
			if (protocol == TransactionProtocol.Default)
				throw new NotSupportedException ("Mono does not support DTC.");
			if (!CanBuildChannelListener<TChannel> (context.Clone ()))
				throw new ArgumentException (String.Format ("The channel type '{0}' is not supported", typeof (TChannel)));
			return new TransactionChannelListener<TChannel> (
				context.BuildInnerChannelListener<TChannel> (),
				protocol);
		}
	}

	internal class TransactionChannelFactory<TChannel> : ChannelFactoryBase<TChannel>
	{
		IChannelFactory<TChannel> inner_factory;
		TransactionScope txscope;
		TransactionProtocol protocol;

		public TransactionChannelFactory (IChannelFactory<TChannel> innerFactory, TransactionProtocol protocol)
		{
			this.inner_factory = innerFactory;
			this.protocol = protocol;
		}

		void ProcessOpen ()
		{
			CommittableTransaction tx = new CommittableTransaction ();
			txscope = new TransactionScope (tx);
		}

		protected override void OnOpen (TimeSpan timeout)
		{
			ProcessOpen ();
			inner_factory.Open (timeout);
		}

		protected override IAsyncResult OnBeginOpen (TimeSpan timeout, AsyncCallback callback, object state)
		{
			ProcessOpen ();
			return inner_factory.BeginOpen (timeout, callback, state);
		}

		protected override void OnEndOpen (IAsyncResult result)
		{
			inner_factory.EndOpen (result);
		}

		protected override TChannel OnCreateChannel (
			EndpointAddress remoteAddress, Uri via)
		{
			return inner_factory.CreateChannel (remoteAddress, via);
		}

		protected override void OnClose (TimeSpan timeout)
		{
			inner_factory.Close (timeout);
			txscope.Complete ();
		}
	}

	internal class TransactionChannelListener<TChannel> : ChannelListenerBase<TChannel> where TChannel : class, IChannel
	{
		IChannelListener<TChannel> inner_listener;
		TransactionScope txscope;
		TransactionProtocol protocol;

		public TransactionChannelListener (IChannelListener<TChannel> innerListener, TransactionProtocol protocol)
		{
			this.inner_listener = innerListener;
			this.protocol = protocol;
		}

		public override T GetProperty<T> ()
		{
			return inner_listener.GetProperty<T> () ?? base.GetProperty<T> ();
		}

		public override Uri Uri {
			get { return inner_listener.Uri; }
		}

		protected override void OnAbort ()
		{
			inner_listener.Abort ();
		}

		protected override void OnOpen (TimeSpan timeout)
		{
			CommittableTransaction tx = new CommittableTransaction ();
			txscope = new TransactionScope (tx);
			inner_listener.Open (timeout);
		}

		protected override IAsyncResult OnBeginOpen (TimeSpan timeout, AsyncCallback callback, object state)
		{
			return inner_listener.BeginOpen (timeout, callback, state);
		}

		protected override void OnEndOpen (IAsyncResult result)
		{
			inner_listener.EndOpen (result);
		}

		protected override void OnClose (TimeSpan timeout)
		{
			inner_listener.Close (timeout);
			txscope.Complete ();
		}

		protected override IAsyncResult OnBeginClose (TimeSpan timeout, AsyncCallback callback, object state)
		{
			return inner_listener.BeginClose (timeout, callback, state);
		}

		protected override void OnEndClose (IAsyncResult result)
		{
			inner_listener.EndClose (result);
		}

		protected override bool OnWaitForChannel (TimeSpan timeout)
		{
			return inner_listener.WaitForChannel (timeout);
		}

		protected override IAsyncResult OnBeginWaitForChannel (TimeSpan timeout, AsyncCallback callback, object state)
		{
			return inner_listener.BeginWaitForChannel (timeout, callback, state);
		}

		protected override bool OnEndWaitForChannel (IAsyncResult result)
		{
			return inner_listener.EndWaitForChannel (result);
		}

		protected override TChannel OnAcceptChannel (TimeSpan timeout)
		{
			return inner_listener.AcceptChannel (timeout);
		}

		protected override IAsyncResult OnBeginAcceptChannel (TimeSpan timeout,
			AsyncCallback callback, object asyncState)
		{
			return inner_listener.BeginAcceptChannel (timeout, callback, asyncState);
		}

		protected override TChannel OnEndAcceptChannel (IAsyncResult result)
		{
			return inner_listener.EndAcceptChannel (result);
		}
	}
}
