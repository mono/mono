//
// SecurityMessageProperty.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005-2006 Novell, Inc.  http://www.novell.com
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
using System.Collections.ObjectModel;
using System.IdentityModel.Policy;
using System.IdentityModel.Tokens;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Security.Tokens;

namespace System.ServiceModel.Security
{
	public class SecurityMessageProperty : IMessageProperty, IDisposable
	{
		SecurityTokenSpecification initiator_token, protection_token,
			recipient_token, transport_token;
		Collection<SupportingTokenSpecification> incoming_supp_tokens =
			new Collection<SupportingTokenSpecification> ();
		ReadOnlyCollection<IAuthorizationPolicy> policies;
		string sender_id_prefix = "_";
		ServiceSecurityContext context = ServiceSecurityContext.Anonymous;

		// internal
		internal Collection<string> ConfirmedSignatures = new Collection<string> ();
		internal byte [] EncryptionKey;

		public SecurityMessageProperty ()
		{
		}

		public bool HasIncomingSupportingTokens {
			get { return incoming_supp_tokens != null && incoming_supp_tokens.Count > 0; }
		}

		public ReadOnlyCollection<IAuthorizationPolicy>
			ExternalAuthorizationPolicies {
			get { return policies; }
			set { policies = value; }
		}

		public Collection<SupportingTokenSpecification>
			IncomingSupportingTokens {
			get { return incoming_supp_tokens; }
		}

		public SecurityTokenSpecification InitiatorToken {
			get { return initiator_token; }
			set { initiator_token = value; }
		}

		public SecurityTokenSpecification ProtectionToken {
			get { return protection_token; }
			set { protection_token = value; }
		}

		public SecurityTokenSpecification RecipientToken {
			get { return recipient_token; }
			set { recipient_token = value; }
		}

		public SecurityTokenSpecification TransportToken {
			get { return transport_token; }
			set { transport_token = value; }
		}

		public string SenderIdPrefix {
			get { return sender_id_prefix; }
			set { sender_id_prefix = value; }
		}

		public ServiceSecurityContext ServiceSecurityContext {
			get { return context; }
			set { context = value; }
		}

		public IMessageProperty CreateCopy ()
		{
			return (SecurityMessageProperty) MemberwiseClone ();
		}

		public void Dispose ()
		{
			context = null;
			policies = null;
		}

		public static SecurityMessageProperty GetOrCreate (Message message)
		{
			SecurityMessageProperty s = message.Properties.Security;
			if (s == null) {
				s = new SecurityMessageProperty ();
				message.Properties.Security = s;
			}
			return s;
		}
	}
}
