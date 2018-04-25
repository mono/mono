//
// NetTcpBinding.cs
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
using System.Collections.Generic;
using System.Net.Security;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using System.ServiceModel.Security.Tokens;
using System.ServiceModel.Configuration;
using System.Text;
using System.Xml;

namespace System.ServiceModel
{
	public class NetTcpBinding : Binding, IBindingRuntimePreferences
	{
		int max_conn;
		OptionalReliableSession reliable_session;
		NetTcpSecurity security;
		XmlDictionaryReaderQuotas reader_quotas
			= new XmlDictionaryReaderQuotas ();
		bool transaction_flow;
#if !MOBILE && !XAMMAC_4_5
		TransactionProtocol transaction_protocol;
#endif
		TcpTransportBindingElement transport;

		public NetTcpBinding ()
			: this (SecurityMode.Transport)
		{
		}

		public NetTcpBinding (SecurityMode securityMode)
			: this (securityMode, false)
		{
		}

		public NetTcpBinding (SecurityMode securityMode,
			bool reliableSessionEnabled)
		{
			security = new NetTcpSecurity (securityMode);
			transport = new TcpTransportBindingElement ();
		}

		public NetTcpBinding (string configurationName)
			: this ()
		{
#if !MOBILE && !XAMMAC_4_5
			var bindingsSection = ConfigUtil.BindingsSection;
			var el = bindingsSection.NetTcpBinding.Bindings [configurationName];
			el.ApplyConfiguration (this);
#else
			throw new NotImplementedException ();
#endif
		}

		internal NetTcpBinding (TcpTransportBindingElement transport,
		                        NetTcpSecurity security,
		                        bool reliableSessionEnabled)
		{
			this.transport = transport;
			this.security = security;
		}

		public HostNameComparisonMode HostNameComparisonMode {
			get { return transport.HostNameComparisonMode; }
			set { transport.HostNameComparisonMode = value; }
		}

		public int ListenBacklog {
			get { return transport.ListenBacklog; }
			set { transport.ListenBacklog = value; }
		}

		public long MaxBufferPoolSize {
			get { return transport.MaxBufferPoolSize; }
			set { transport.MaxBufferPoolSize = value; }
		}

		public int MaxBufferSize {
			get { return transport.MaxBufferSize; }
			set { transport.MaxBufferSize = value; }
		}

		[MonoTODO]
		public int MaxConnections {
			get { return max_conn; }
			set { max_conn = value; }
		}

		public long MaxReceivedMessageSize {
			get { return transport.MaxReceivedMessageSize; }
			set { transport.MaxReceivedMessageSize = value; }
		}

		public bool PortSharingEnabled {
			get { return transport.PortSharingEnabled; }
			set { transport.PortSharingEnabled = value; }
		}

		[MonoTODO]
		public OptionalReliableSession ReliableSession {
			get { return reliable_session; }
		}

		public XmlDictionaryReaderQuotas ReaderQuotas {
			get { return reader_quotas; }
			set { reader_quotas = value; }
		}

		public NetTcpSecurity Security {
			get { return security; }
			set { security = value; }
		}

		public EnvelopeVersion EnvelopeVersion {
			get { return EnvelopeVersion.Soap12; }
		}

		public TransferMode TransferMode {
			get { return transport.TransferMode; }
			set { transport.TransferMode = value; }
		}

		public bool TransactionFlow {
			get { return transaction_flow; }
			set { transaction_flow = value; }
		}

#if !MOBILE && !XAMMAC_4_5
		public TransactionProtocol TransactionProtocol {
			get { return transaction_protocol; }
			set { transaction_protocol = value; }
		}
#endif

		// overrides

		public override string Scheme {
			get { return "net.tcp"; }
		}

		public override BindingElementCollection CreateBindingElements ()
		{
#if !MOBILE && !XAMMAC_4_5
			BindingElement tx = new TransactionFlowBindingElement (TransactionProtocol.WSAtomicTransactionOctober2004);
			SecurityBindingElement sec = CreateMessageSecurity ();
#endif
			var msg = new BinaryMessageEncodingBindingElement ();
			if (ReaderQuotas != null)
				ReaderQuotas.CopyTo (msg.ReaderQuotas);
			var trsec = CreateTransportSecurity ();
			BindingElement tr = GetTransport ();
			List<BindingElement> list = new List<BindingElement> ();
#if !MOBILE && !XAMMAC_4_5
			if (tx != null)
				list.Add (tx);
			if (sec != null)
				list.Add (sec);
#endif
			list.Add (msg);
			if (trsec != null)
				list.Add (trsec);
			list.Add (tr);
			return new BindingElementCollection (list.ToArray ());
		}

		BindingElement GetTransport ()
		{
			return transport.Clone ();
		}

#if !MOBILE && !XAMMAC_4_5
		// It is problematic, but there is no option to disable establishing security context in this binding unlike WSHttpBinding...
		SecurityBindingElement CreateMessageSecurity ()
		{
			if (Security.Mode == SecurityMode.Transport ||
			    Security.Mode == SecurityMode.None)
				return null;

			// FIXME: this is wrong. Could be Asymmetric, depends on Security.Message.AlgorithmSuite value.
			SymmetricSecurityBindingElement element =
				new SymmetricSecurityBindingElement ();

			element.MessageSecurityVersion = MessageSecurityVersion.Default;

			element.SetKeyDerivation (false);

			switch (Security.Message.ClientCredentialType) {
			case MessageCredentialType.Certificate:
				element.EndpointSupportingTokenParameters.Endorsing.Add (
					new X509SecurityTokenParameters ());
				goto default;
			case MessageCredentialType.IssuedToken:
				IssuedSecurityTokenParameters istp =
					new IssuedSecurityTokenParameters ();
				// FIXME: issuer binding must be secure.
				istp.IssuerBinding = new CustomBinding (
					new TextMessageEncodingBindingElement (),
					GetTransport ());
				element.EndpointSupportingTokenParameters.Endorsing.Add (istp);
				goto default;
			case MessageCredentialType.UserName:
				element.EndpointSupportingTokenParameters.SignedEncrypted.Add (
					new UserNameSecurityTokenParameters ());
				goto default;
			case MessageCredentialType.Windows:
				element.ProtectionTokenParameters =
					new KerberosSecurityTokenParameters ();
				break;
			default: // including .None
				X509SecurityTokenParameters p =
					new X509SecurityTokenParameters ();
				p.X509ReferenceStyle = X509KeyIdentifierClauseType.Thumbprint;
				element.ProtectionTokenParameters = p;
				break;
			}

			// SecureConversation enabled

			ChannelProtectionRequirements reqs =
				new ChannelProtectionRequirements ();
			// FIXME: fill the reqs

			return SecurityBindingElement.CreateSecureConversationBindingElement (
				// FIXME: requireCancellation
				element, true, reqs);
		}
#endif

		BindingElement CreateTransportSecurity ()
		{
			switch (Security.Mode) {
			case SecurityMode.Transport:
				return new WindowsStreamSecurityBindingElement () {
					ProtectionLevel = Security.Transport.ProtectionLevel };

			case SecurityMode.TransportWithMessageCredential:
				return new SslStreamSecurityBindingElement ();

			default:
				return null;
			}

			// FIXME: consider Security.Transport.ExtendedProtectionPolicy.

			switch (Security.Transport.ClientCredentialType) {
			case TcpClientCredentialType.Windows:
				return new WindowsStreamSecurityBindingElement () { ProtectionLevel = Security.Transport.ProtectionLevel };
			case TcpClientCredentialType.Certificate:
				// FIXME: set RequireClientCertificate and IdentityVerifier depending on other properties, if applicable.
				return new SslStreamSecurityBindingElement ();
			default: // includes None
				return null;
			}
		}

		bool IBindingRuntimePreferences.ReceiveSynchronously {
			get { throw new NotImplementedException (); }
		}
	}
}
