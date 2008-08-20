//
// ITransactedBindingElement.cs
//
// Author: Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2007 Novell, Inc (http://www.novell.com)
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
	public sealed class MsmqTransportBindingElement : MsmqBindingElementBase
	{
		int max_pool_size = 8;
		QueueTransferProtocol queue_tr_protocol;
		bool use_ad;

		public MsmqTransportBindingElement ()
		{
		}

		public int MaxPoolSize {
			get { return max_pool_size; }
			set { max_pool_size = value; }
		}

		public QueueTransferProtocol QueueTransferProtocol {
			get { return queue_tr_protocol; }
			set { queue_tr_protocol = value; }
		}

		public override string Scheme {
			get { return "net.msmq"; }
		}

		[MonoLimitation ("ActiveDirectory is windows-only solution")]
		public bool UseActiveDirectory {
			get { return use_ad; }
			set { use_ad = value; }
		}

		public override BindingElement Clone ()
		{
			return (MsmqTransportBindingElement) MemberwiseClone ();
		}

		public override bool CanBuildChannelFactory<TChannel> (BindingContext context)
		{
			if (context == null)
				throw new ArgumentNullException ("context");
			return  typeof (TChannel) == typeof (IOutputChannel) ||
				typeof (TChannel) == typeof (IOutputSessionChannel);
		}

		[MonoTODO]
		public override IChannelFactory<TChannel> BuildChannelFactory<TChannel> (BindingContext context)
		{
			if (context == null)
				throw new ArgumentNullException ("context");
			if (typeof (TChannel) == typeof (IOutputChannel))
				return (IChannelFactory<TChannel>) new MsmqChannelFactory<IOutputChannel> (this, context);
			if (typeof (TChannel) == typeof (IOutputSessionChannel))
				return (IChannelFactory<TChannel>) new MsmqChannelFactory<IOutputChannel> (this, context);
			return base.BuildChannelFactory<TChannel> (context);
		}

		public override bool CanBuildChannelListener<TChannel> (BindingContext context)
		{
			if (context == null)
				throw new ArgumentNullException ("context");
			return  typeof (TChannel) == typeof (IInputChannel) ||
				typeof (TChannel) == typeof (IInputSessionChannel);
		}

		[MonoTODO]
		public override IChannelListener<TChannel> BuildChannelListener<TChannel> (BindingContext context)
		{
			throw new NotImplementedException ();
		}
	}
}
