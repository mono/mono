//
// ServiceCredentials.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005 Novell, Inc.  http://www.novell.com
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
using System.IdentityModel.Selectors;
using System.ServiceModel.Channels;
using System.ServiceModel.Security;
using System.ServiceModel.Security.Tokens;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace System.ServiceModel.Description
{
	public class ServiceCredentials
		: SecurityCredentialsManager, IServiceBehavior
	{
		public ServiceCredentials ()
		{
		}

		protected ServiceCredentials (ServiceCredentials other)
		{
			initiator = other.initiator.Clone ();
			peer = other.peer.Clone ();
			recipient = other.recipient.Clone ();
			userpass = other.userpass.Clone ();
			windows = other.windows.Clone ();
			issued_token = other.issued_token.Clone ();
			secure_conversation = other.secure_conversation.Clone ();
		}

		X509CertificateInitiatorServiceCredential initiator
			= new X509CertificateInitiatorServiceCredential ();
		PeerCredential peer = new PeerCredential ();
		X509CertificateRecipientServiceCredential recipient
			= new X509CertificateRecipientServiceCredential ();
		UserNamePasswordServiceCredential userpass
			= new UserNamePasswordServiceCredential ();
		WindowsServiceCredential windows
			= new WindowsServiceCredential ();
		IssuedTokenServiceCredential issued_token =
			new IssuedTokenServiceCredential ();
		SecureConversationServiceCredential secure_conversation =
			new SecureConversationServiceCredential ();

		public X509CertificateInitiatorServiceCredential ClientCertificate {
			get { return initiator; }
		}

		public IssuedTokenServiceCredential IssuedTokenAuthentication {
			get { return issued_token; }
		}

		public PeerCredential Peer {
			get { return peer; }
		}

		public SecureConversationServiceCredential SecureConversationAuthentication {
			get { return secure_conversation; }
		}

		public X509CertificateRecipientServiceCredential ServiceCertificate {
			get { return recipient; }
		}

		public UserNamePasswordServiceCredential UserNameAuthentication {
			get { return userpass; }
		}

		public WindowsServiceCredential WindowsAuthentication {
			get { return windows; }
		}

		public ServiceCredentials Clone ()
		{
			ServiceCredentials ret = CloneCore ();
			if (ret.GetType () != GetType ())
				throw new NotImplementedException ("CloneCore() must be implemented to return an instance of the same type in this custom ServiceCredentials type.");
			return ret;
		}

		protected virtual ServiceCredentials CloneCore ()
		{
			return new ServiceCredentials (this);
		}

		public override SecurityTokenManager CreateSecurityTokenManager ()
		{
			return new ServiceCredentialsSecurityTokenManager (this);
		}

		void IServiceBehavior.AddBindingParameters (
			ServiceDescription description,
			ServiceHostBase serviceHostBase,
			Collection<ServiceEndpoint> endpoints,
			BindingParameterCollection parameters)
		{
			parameters.Add (this);
		}

		void IServiceBehavior.ApplyDispatchBehavior (
			ServiceDescription description,
			ServiceHostBase serviceHostBase)
		{
			// do nothing
		}

		[MonoTODO]
		void IServiceBehavior.Validate (
			ServiceDescription description,
			ServiceHostBase serviceHostBase)
		{
			// unlike MSDN description, it does not throw NIE.
		}
	}
}
