//
// ReplyChannelBase.cs
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
#if !MOBILE && !XAMMAC_4_5
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Security;

namespace MonoTests.System.ServiceModel.Channels
{
	public abstract class ReplyChannelBase : ChannelBase, IReplyChannel
	{
		ChannelListenerBase channel_listener;

		public ReplyChannelBase (ChannelListenerBase listener)
			: base (listener)
		{
			this.channel_listener = listener;
		}

		protected override TimeSpan DefaultCloseTimeout {
			get { return TimeSpan.FromSeconds (5); }
		}

		protected override TimeSpan DefaultOpenTimeout {
			get { return TimeSpan.FromSeconds (5); }
		}

		public abstract EndpointAddress LocalAddress { get; }

		public virtual bool TryReceiveRequest ()
		{
			RequestContext dummy;
			return TryReceiveRequest (DefaultReceiveTimeout, out dummy);
		}

		public abstract bool TryReceiveRequest (TimeSpan timeout, out RequestContext context);

		public abstract IAsyncResult BeginTryReceiveRequest (TimeSpan timeout, AsyncCallback callback, object state);

		public virtual bool EndTryReceiveRequest (IAsyncResult result)
		{
			RequestContext dummy;
			return EndTryReceiveRequest (result, out dummy);
		}

		public abstract bool EndTryReceiveRequest (IAsyncResult result, out RequestContext context);

		public virtual bool WaitForRequest ()
		{
			return WaitForRequest (DefaultReceiveTimeout);
		}

		public abstract bool WaitForRequest (TimeSpan timeout);

		public abstract IAsyncResult BeginWaitForRequest (TimeSpan timeout, AsyncCallback callback, object state);

		public abstract bool EndWaitForRequest (IAsyncResult result);

		public virtual RequestContext ReceiveRequest ()
		{
			return ReceiveRequest (DefaultReceiveTimeout);
		}

		public abstract RequestContext ReceiveRequest (TimeSpan timeout);

		public virtual IAsyncResult BeginReceiveRequest (AsyncCallback callback, object state)
		{
			return BeginReceiveRequest (DefaultReceiveTimeout, callback, state);
		}

		public abstract IAsyncResult BeginReceiveRequest (TimeSpan timeout, AsyncCallback callback, object state);

		public abstract RequestContext EndReceiveRequest (IAsyncResult result);

		protected override IAsyncResult OnBeginOpen (TimeSpan timeout,
			AsyncCallback callback, object state)
		{
			throw new NotImplementedException ();
		}

		protected override void OnEndOpen (IAsyncResult result)
		{
			throw new NotImplementedException ();
		}

		protected override IAsyncResult OnBeginClose (TimeSpan timeout,
			AsyncCallback callback, object state)
		{
			throw new NotImplementedException ();
		}

		protected override void OnEndClose (IAsyncResult result)
		{
			throw new NotImplementedException ();
		}
	}
}
#endif
