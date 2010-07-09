//
// SecurityRequestContext.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005-2007 Novell, Inc.  http://www.novell.com
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
using System.IO;
using System.Net.Security;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using System.ServiceModel.Security.Tokens;
using System.Xml;
using System.Xml.XPath;

namespace System.ServiceModel.Channels
{
	internal class SecurityRequestContext : RequestContext
	{
		RecipientMessageSecurityBindingSupport security;
		SecurityReplyChannel channel;
		RequestContext source;
		Message msg;
		MessageBuffer source_request;

		public SecurityRequestContext (SecurityReplyChannel channel, RequestContext source)
		{
			this.source = source;
			this.channel = channel;

			security = channel.Source.SecuritySupport;
		}

		public override Message RequestMessage {
			get {
				if (msg == null) {
					msg = source.RequestMessage;
					switch (msg.Headers.Action) {
					case Constants.WstIssueAction:
					case Constants.WstIssueReplyAction:
					case Constants.WstRenewAction:
					case Constants.WstCancelAction:
					case Constants.WstValidateAction:
						break;
					default:
						msg = new RecipientSecureMessageDecryptor (msg, security).DecryptMessage ();
						break;
					}
				}
				return msg;
			}
		}

		public override void Abort ()
		{
			source.Abort ();
		}

		public override IAsyncResult BeginReply (Message message, AsyncCallback callback, object state)
		{
			return BeginReply (message, channel.Listener.DefaultSendTimeout, callback, state);
		}

		public override IAsyncResult BeginReply (Message message, TimeSpan timeout, AsyncCallback callback, object state)
		{
			// FIXME: implement
			throw new NotImplementedException ();
		}

		public override void Close ()
		{
			Close (channel.Listener.DefaultCloseTimeout);
		}

		public override void Close (TimeSpan timeout)
		{
			source.Close (timeout);
		}

		public override void EndReply (IAsyncResult result)
		{
			// FIXME: implement
			throw new NotImplementedException ();
		}

		public override void Reply (Message message)
		{
			Reply (message, channel.Listener.DefaultSendTimeout);
		}

		public override void Reply (Message message, TimeSpan timeout)
		{
			try {
				if (!message.IsFault && message.Headers.Action != Constants.WstIssueReplyAction)
					message = SecureMessage (message);
				source.Reply (message, timeout);
			} catch (Exception ex) {
				FaultConverter fc = FaultConverter.GetDefaultFaultConverter (msg.Version);
				Message fault;
				if (fc.TryCreateFaultMessage (ex, out fault))
					source.Reply (fault, timeout);
				else
					throw;
			}
		}

		Message SecureMessage (Message input)
		{
			return new RecipientMessageSecurityGenerator (input, this, security).SecureMessage ();
		}
	}
}
