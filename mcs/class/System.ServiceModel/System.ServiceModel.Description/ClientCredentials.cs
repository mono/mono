//
// ClientCredentials.cs
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
using System.Collections.Generic;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Security;
#if !MOBILE
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.ServiceModel.Security.Tokens;
#endif

namespace System.ServiceModel.Description
{
	public class ClientCredentials
#if MOBILE || XAMMAC_4_5
		: IEndpointBehavior
#else
		: SecurityCredentialsManager, IEndpointBehavior
#endif
	{
		public ClientCredentials ()
		{
		}

		[MonoTODO]
		protected ClientCredentials (ClientCredentials other)
		{
			userpass = other.userpass.Clone ();
			digest = other.digest.Clone ();
			initiator = other.initiator.Clone ();
			recipient = other.recipient.Clone ();
			windows = other.windows.Clone ();
#if !MOBILE
			issued_token = other.issued_token.Clone ();
			peer = other.peer.Clone ();
			support_interactive = other.support_interactive;
#endif
		}

		UserNamePasswordClientCredential userpass =
			new UserNamePasswordClientCredential ();

		HttpDigestClientCredential digest =
			new HttpDigestClientCredential ();
		X509CertificateInitiatorClientCredential initiator =
			new X509CertificateInitiatorClientCredential ();
		X509CertificateRecipientClientCredential recipient =
			new X509CertificateRecipientClientCredential ();
		WindowsClientCredential windows =
			new WindowsClientCredential ();

		public X509CertificateInitiatorClientCredential ClientCertificate {
			get { return initiator; }
		}

		public HttpDigestClientCredential HttpDigest {
			get { return digest; }
		}

		public X509CertificateRecipientClientCredential ServiceCertificate {
			get { return recipient; }
		}

		public WindowsClientCredential Windows {
			get { return windows; }
		}

#if !MOBILE
		IssuedTokenClientCredential issued_token =
			new IssuedTokenClientCredential ();
		PeerCredential peer = new PeerCredential ();
		bool support_interactive = true;

		public IssuedTokenClientCredential IssuedToken {
			get { return issued_token; }
		}

		public PeerCredential Peer {
			get { return peer; }
		}

		public bool SupportInteractive {
			get { return support_interactive; }
			set { support_interactive = value; }
		}
#endif

		public UserNamePasswordClientCredential UserName {
			get { return userpass; }
		}

		public ClientCredentials Clone ()
		{
			ClientCredentials ret = CloneCore ();
			if (ret.GetType () != GetType ())
				throw new NotImplementedException ("CloneCore() must be implemented to return an instance of the same type in this custom ClientCredentials type.");
			return ret;
		}

		protected virtual ClientCredentials CloneCore ()
		{
			return new ClientCredentials (this);
		}

#if !MOBILE && !XAMMAC_4_5
		public override SecurityTokenManager CreateSecurityTokenManager ()
		{
			return new ClientCredentialsSecurityTokenManager (this);
		}

		[MonoTODO]
		protected virtual SecurityToken GetInfoCardSecurityToken (
			bool requiresInfoCard, CardSpacePolicyElement [] chain,
			SecurityTokenSerializer tokenSerializer)
		{
			throw new NotImplementedException ();
		}
#endif

		void IEndpointBehavior.ApplyDispatchBehavior (ServiceEndpoint endpoint,
			EndpointDispatcher dispatcher)
		{
			// documented as to have no effect.
		}

		void IEndpointBehavior.AddBindingParameters (ServiceEndpoint endpoint,
			BindingParameterCollection parameters)
		{
			parameters.Add (this);
		}

		[MonoTODO]
		public virtual void ApplyClientBehavior (
			ServiceEndpoint serviceEndpoint, ClientRuntime behavior)
		{
			if (serviceEndpoint == null)
				throw new ArgumentNullException ("serviceEndpoint");
			if (behavior == null)
				throw new ArgumentNullException ("behavior");

			// FIXME: apply to endpoint when there is not security binding element.
		}

		void IEndpointBehavior.Validate (ServiceEndpoint endpoint)
		{
			// documented as reserved for future use.
		}
	}
}
