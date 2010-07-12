//
// SecurityReplyChannel.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2006,2010 Novell, Inc.  http://www.novell.com
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Security;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using System.ServiceModel.Security.Tokens;

namespace System.ServiceModel.Channels.Security
{
	internal class SecurityReplyChannel : InternalReplyChannelBase
	{
		IReplyChannel inner;

		public SecurityReplyChannel (
			SecurityChannelListener<IReplyChannel> source,
			IReplyChannel innerChannel)
			: base (source)
		{
			this.inner = innerChannel;
		}

		public SecurityChannelListener<IReplyChannel> Source {
			get { return (SecurityChannelListener<IReplyChannel>) Listener; }
		}

		// IReplyChannel

		protected override void OnOpen (TimeSpan timeout)
		{
			inner.Open (timeout);
		}

		protected override void OnClose (TimeSpan timeout)
		{
			inner.Close (timeout);
		}

		public override RequestContext ReceiveRequest (TimeSpan timeout)
		{
			RequestContext ctx;
			if (TryReceiveRequest (timeout, out ctx))
				return ctx;
			throw new TimeoutException ("Failed to receive request context");
		}

		public override bool TryReceiveRequest (TimeSpan timeout, out RequestContext context)
		{
			DateTime start = DateTime.Now;

			if (!inner.TryReceiveRequest (timeout, out context))
				return false;

			var msg = context.RequestMessage;
			switch (msg.Headers.Action) {
			case Constants.WstIssueAction:
			case Constants.WstIssueReplyAction:
			case Constants.WstRenewAction:
			case Constants.WstCancelAction:
			case Constants.WstValidateAction:
				var support = Source.SecuritySupport;
				var commAuth = support.TokenAuthenticator as CommunicationSecurityTokenAuthenticator;
				if (commAuth!= null)
					msg = commAuth.Communication.ProcessNegotiation (msg, timeout - (DateTime.Now - start));
				context.Reply (msg, timeout - (DateTime.Now - start));
				context.Close (timeout - (DateTime.Now - start));
				// wait for another incoming message
				return TryReceiveRequest (timeout - (DateTime.Now - start), out context);

				break;
			}

			context = new SecurityRequestContext (this, context);
			return true;
		}

		[MonoTODO]
		public override bool WaitForRequest (TimeSpan timeout)
		{
			throw new NotImplementedException ();
		}

		// IChannel

		public override T GetProperty<T> ()
		{
			// FIXME: implement
			return inner.GetProperty<T> ();
		}
	}
}
