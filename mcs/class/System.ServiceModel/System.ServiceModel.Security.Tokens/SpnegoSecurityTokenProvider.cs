//
// SpnegoSecurityTokenProvider.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2007 Novell, Inc.  http://www.novell.com
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
using System.Net;
using System.Security.Principal;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Security.Tokens;
using System.Xml;
using Mono.Security;

// mhm, why is this class not in S.SM.S.Tokens??
namespace System.ServiceModel.Security
{
	// Anyways we won't support SSPI until it becomes open.
	internal class SpnegoSecurityTokenProvider : CommunicationSecurityTokenProvider
	{
		ClientCredentialsSecurityTokenManager manager;
		SecurityTokenRequirement requirement;
		SpnegoCommunicationObject comm;

		public SpnegoSecurityTokenProvider (ClientCredentialsSecurityTokenManager manager, SecurityTokenRequirement requirement)
		{
			this.manager = manager;
			comm = new SpnegoCommunicationObject (this);
		}

		public ClientCredentialsSecurityTokenManager Manager {
			get { return manager; }
		}

		public override ProviderCommunicationObject Communication {
			get { return comm; }
		}

		public override SecurityToken GetOnlineToken (TimeSpan timeout)
		{
			return comm.GetToken (timeout);
		}
	}

	class SpnegoCommunicationObject : ProviderCommunicationObject
	{
		SpnegoSecurityTokenProvider owner;

		public SpnegoCommunicationObject (SpnegoSecurityTokenProvider owner)
		{
			this.owner = owner;
		}

		WSTrustSecurityTokenServiceProxy proxy;

		protected internal override TimeSpan DefaultCloseTimeout {
			get { throw new NotImplementedException (); }
		}

		protected internal override TimeSpan DefaultOpenTimeout {
			get { throw new NotImplementedException (); }
		}

		protected override void OnAbort ()
		{
			throw new NotImplementedException ();
		}

		protected override void OnOpen (TimeSpan timeout)
		{
			if (State == CommunicationState.Opened)
				throw new InvalidOperationException ("Already opened.");

			EnsureProperties ();

			proxy = new WSTrustSecurityTokenServiceProxy (
				IssuerBinding, IssuerAddress);
		}

		protected override IAsyncResult OnBeginOpen (TimeSpan timeout, AsyncCallback callback, object state)
		{
			throw new NotImplementedException ();
		}

		protected override void OnEndOpen (IAsyncResult result)
		{
			throw new NotImplementedException ();
		}

		protected override void OnClose (TimeSpan timeout)
		{
			if (proxy != null)
				proxy.Close ();
		}

		protected override IAsyncResult OnBeginClose (TimeSpan timeout, AsyncCallback callback, object state)
		{
			throw new NotImplementedException ();
		}

		protected override void OnEndClose (IAsyncResult result)
		{
			throw new NotImplementedException ();
		}

		public SecurityToken GetToken (TimeSpan timeout)
		{
			bool gss = (TargetAddress.Identity == null);
			SspiClientSession sspi = new SspiClientSession ();

			WstRequestSecurityToken rst =
				new WstRequestSecurityToken ();

			// send MessageType1
			rst.BinaryExchange = new WstBinaryExchange (Constants.WstBinaryExchangeValueGss);
			// When the TargetAddress does not contain the endpoint
			// identity, then .net seems to use Kerberos instead of
			// raw NTLM.
			if (gss)
				rst.BinaryExchange.Value = sspi.ProcessSpnegoInitialContextTokenRequest ();
			else
				rst.BinaryExchange.Value = sspi.ProcessMessageType1 ();

			Message request = Message.CreateMessage (IssuerBinding.MessageVersion, Constants.WstIssueAction, rst);
			request.Headers.MessageId = new UniqueId ();
			request.Headers.ReplyTo = new EndpointAddress (Constants.WsaAnonymousUri);
			request.Headers.To = TargetAddress.Uri;
			MessageBuffer buffer = request.CreateBufferedCopy (0x10000);
//			tlsctx.StoreMessage (buffer.CreateMessage ().GetReaderAtBodyContents ());

			// receive MessageType2
			Message response = proxy.Issue (buffer.CreateMessage ());
			buffer = response.CreateBufferedCopy (0x10000);
//			tlsctx.StoreMessage (buffer.CreateMessage ().GetReaderAtBodyContents ());

			WSTrustRequestSecurityTokenResponseReader reader =
				new WSTrustRequestSecurityTokenResponseReader (Constants.WstSpnegoProofTokenType, buffer.CreateMessage ().GetReaderAtBodyContents (), SecurityTokenSerializer, null);
			reader.Read ();

			byte [] raw = reader.Value.BinaryExchange.Value;
			if (gss)
				sspi.ProcessSpnegoInitialContextTokenResponse (raw);
			else
				sspi.ProcessMessageType2 (raw);

			// send MessageType3
			WstRequestSecurityTokenResponse rstr =
				new WstRequestSecurityTokenResponse (SecurityTokenSerializer);
			rstr.Context = reader.Value.Context;
			rstr.BinaryExchange = new WstBinaryExchange (Constants.WstBinaryExchangeValueGss);

			NetworkCredential cred = owner.Manager.ClientCredentials.Windows.ClientCredential;
			string user = string.IsNullOrEmpty (cred.UserName) ? Environment.UserName : cred.UserName;
			string pass = cred.Password ?? String.Empty;
			if (gss)
				rstr.BinaryExchange.Value = sspi.ProcessSpnegoProcessContextToken (user, pass);
			else
				rstr.BinaryExchange.Value = sspi.ProcessMessageType3 (user, pass);

			request = Message.CreateMessage (IssuerBinding.MessageVersion, Constants.WstIssueReplyAction, rstr);
			request.Headers.MessageId = new UniqueId ();
			request.Headers.ReplyTo = new EndpointAddress (Constants.WsaAnonymousUri);
			request.Headers.To = TargetAddress.Uri;

			buffer = request.CreateBufferedCopy (0x10000);
//			tlsctx.StoreMessage (buffer.CreateMessage ().GetReaderAtBodyContents ());

			proxy = new WSTrustSecurityTokenServiceProxy (
				IssuerBinding, IssuerAddress);
			response = proxy.IssueReply (buffer.CreateMessage ());
			// FIXME: use correct limitation
			buffer = response.CreateBufferedCopy (0x10000);
			// don't store this message for ckhash (it's not part
			// of exchange)
Console.WriteLine (buffer.CreateMessage ());

			throw new NotImplementedException ();
		}
	}
}
