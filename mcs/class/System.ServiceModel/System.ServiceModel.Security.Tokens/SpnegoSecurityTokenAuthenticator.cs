//
// SpnegoSecurityTokenAuthenticator.cs
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
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Security;
using System.IdentityModel.Policy;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using System.ServiceModel.Security.Tokens;
using System.Text;
using System.Xml;
using Mono.Security;

using ReqType = System.ServiceModel.Security.Tokens.ServiceModelSecurityTokenRequirement;

namespace System.ServiceModel.Security.Tokens
{
	// FIXME: implement all
	class SpnegoSecurityTokenAuthenticator : CommunicationSecurityTokenAuthenticator
	{
		ServiceCredentialsSecurityTokenManager manager;
		SpnegoAuthenticatorCommunicationObject comm;

		public SpnegoSecurityTokenAuthenticator (
			ServiceCredentialsSecurityTokenManager manager, 
			SecurityTokenRequirement r)
		{
			this.manager = manager;
			comm = new SpnegoAuthenticatorCommunicationObject (this);
		}

		public ServiceCredentialsSecurityTokenManager Manager {
			get { return manager; }
		}

		public override AuthenticatorCommunicationObject Communication {
			get { return comm; }
		}

		protected override bool CanValidateTokenCore (SecurityToken token)
		{
			throw new NotImplementedException ();
		}

		protected override ReadOnlyCollection<IAuthorizationPolicy>
			ValidateTokenCore (SecurityToken token)
		{
			throw new NotImplementedException ();
		}
	}

	class SpnegoAuthenticatorCommunicationObject : AuthenticatorCommunicationObject
	{
		SpnegoSecurityTokenAuthenticator owner;

		public SpnegoAuthenticatorCommunicationObject (SpnegoSecurityTokenAuthenticator owner)
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

		public override Message ProcessNegotiation (Message request, TimeSpan timeout)
		{
			if (request.Headers.Action == Constants.WstIssueAction)
				return ProcessMessageType1 (request, timeout);
			else
				return ProcessMessageType3 (request, timeout);
		}

		class TlsServerSessionInfo
		{
			public TlsServerSessionInfo (string context, TlsServerSession tls)
			{
				ContextId = context;
				Tls = tls;
			}

			public string ContextId;
			public TlsServerSession Tls;
			public MemoryStream Messages = new MemoryStream ();
		}

		Dictionary<string,SspiServerSession> sessions =
			new Dictionary<string,SspiServerSession> ();

		void AppendNegotiationMessageXml (XmlReader reader, TlsServerSessionInfo tlsInfo)
		{
			XmlDsigExcC14NTransform t = new XmlDsigExcC14NTransform ();
			XmlDocument doc = new XmlDocument ();
			doc.PreserveWhitespace = true;
			reader.MoveToContent ();
			doc.AppendChild (doc.ReadNode (reader));
			t.LoadInput (doc);
			MemoryStream stream = (MemoryStream) t.GetOutput ();
			byte [] bytes = stream.ToArray ();
			tlsInfo.Messages.Write (bytes, 0, bytes.Length);
		}

		// FIXME: use timeout
		Message ProcessMessageType1 (Message request, TimeSpan timeout)
		{
			// FIXME: use correct buffer size
			MessageBuffer buffer = request.CreateBufferedCopy (0x10000);
			WSTrustRequestSecurityTokenReader reader =
				new WSTrustRequestSecurityTokenReader (buffer.CreateMessage ().GetReaderAtBodyContents (), SecurityTokenSerializer);
			reader.Read ();

			if (sessions.ContainsKey (reader.Value.Context))
				throw new SecurityNegotiationException (String.Format ("The context '{0}' already exists in this SSL negotiation manager", reader.Value.Context));

Console.WriteLine (buffer.CreateMessage ());

			SspiServerSession sspi = new SspiServerSession ();
//			AppendNegotiationMessageXml (buffer.CreateMessage ().GetReaderAtBodyContents (), tlsInfo);

			// FIXME: when an explicit endpoint identity is
			// specified in the target EndpointAddress at client,
			// it sends some other kind of binary octets that
			// include NTLM octet, instead of raw NTLM octet itself.

			byte [] raw = reader.Value.BinaryExchange.Value;

			bool gss = "NTLMSSP" != Encoding.ASCII.GetString (raw, 0, 7);

			if (gss)
				sspi.ProcessSpnegoInitialContextTokenRequest (raw);
			else
				sspi.ProcessMessageType1 (raw);

			WstRequestSecurityTokenResponse rstr =
				new WstRequestSecurityTokenResponse (SecurityTokenSerializer);
			rstr.Context = reader.Value.Context;
			rstr.BinaryExchange = new WstBinaryExchange (Constants.WstBinaryExchangeValueGss);

			if (gss)
				rstr.BinaryExchange.Value = sspi.ProcessSpnegoInitialContextTokenResponse ();
			else
				rstr.BinaryExchange.Value = sspi.ProcessMessageType2 ();

			Message reply = Message.CreateMessage (request.Version, Constants.WstIssueReplyAction, rstr);
			reply.Headers.RelatesTo = request.Headers.MessageId;

			// FIXME: use correct buffer size
			buffer = reply.CreateBufferedCopy (0x10000);
//			AppendNegotiationMessageXml (buffer.CreateMessage ().GetReaderAtBodyContents (), tlsInfo);

			sessions [reader.Value.Context] = sspi;

			return buffer.CreateMessage ();
		}

		// FIXME: use timeout
		Message ProcessMessageType3 (Message request, TimeSpan timeout)
		{
			// FIXME: use correct buffer size
			MessageBuffer buffer = request.CreateBufferedCopy (0x10000);
Console.WriteLine (buffer.CreateMessage ());
			WSTrustRequestSecurityTokenResponseReader reader =
				new WSTrustRequestSecurityTokenResponseReader (Constants.WstSpnegoProofTokenType, buffer.CreateMessage ().GetReaderAtBodyContents (), SecurityTokenSerializer, null);
			reader.Read ();

			byte [] raw = reader.Value.BinaryExchange.Value;

			bool gss = "NTLMSSP" != Encoding.ASCII.GetString (raw, 0, 7);

foreach (byte b in raw) Console.Write ("{0:X02} ", b); Console.WriteLine ();

			SspiServerSession sspi;
			if (!sessions.TryGetValue (reader.Value.Context, out sspi))
				throw new SecurityNegotiationException (String.Format ("The context '{0}' does not exist in this SSL negotiation manager", reader.Value.Context));

			if (gss)
				sspi.ProcessSpnegoProcessContextToken (raw);
			else
				sspi.ProcessMessageType3 (raw);

			throw new NotImplementedException ();
/*
			AppendNegotiationMessageXml (buffer.CreateMessage ().GetReaderAtBodyContents (), tlsInfo);
//Console.WriteLine (System.Text.Encoding.UTF8.GetString (tlsInfo.Messages.ToArray ()));

			tls.ProcessClientKeyExchange (reader.Value.BinaryExchange.Value);

			byte [] serverFinished = tls.ProcessServerFinished ();

			// The shared key is computed as recommended in WS-Trust:
			// P_SHA1(encrypted_key,SHA1(exc14n(RST..RSTRs))+"CK-HASH")
			byte [] hash = SHA1.Create ().ComputeHash (tlsInfo.Messages.ToArray ());
			byte [] key = tls.CreateHash (tls.MasterSecret, hash, "CK-HASH");
foreach (byte b in hash) Console.Write ("{0:X02} ", b); Console.WriteLine ();
foreach (byte b in key) Console.Write ("{0:X02} ", b); Console.WriteLine ();

			WstRequestSecurityTokenResponseCollection col =
				new WstRequestSecurityTokenResponseCollection ();
			WstRequestSecurityTokenResponse rstr =
				new WstRequestSecurityTokenResponse (SecurityTokenSerializer);
			rstr.Context = reader.Value.Context;
			rstr.TokenType = Constants.WsscContextToken;
			DateTime from = DateTime.Now;
			// FIXME: not sure if arbitrary key is used here.
			SecurityContextSecurityToken sct = SecurityContextSecurityToken.CreateCookieSecurityContextToken (
				// Create a new context.
				// (do not use sslnego context here.)
				new UniqueId (),
				"uuid-" + Guid.NewGuid (),
				key,
				from,
				// FIXME: use LocalServiceSecuritySettings.NegotiationTimeout
				from.AddHours (8),
				null,
				owner.Manager.ServiceCredentials.SecureConversationAuthentication.SecurityStateEncoder);
			rstr.RequestedSecurityToken = sct;
			rstr.RequestedProofToken = tls.ProcessApplicationData (key);
			rstr.RequestedAttachedReference = new LocalIdKeyIdentifierClause (sct.Id);
			rstr.RequestedUnattachedReference = new SecurityContextKeyIdentifierClause (sct.ContextId, null);
			WstLifetime lt = new WstLifetime ();
			lt.Created = from;
			// FIXME: use LocalServiceSecuritySettings.NegotiationTimeout
			lt.Expires = from.AddHours (8);
			rstr.Lifetime = lt;
			rstr.BinaryExchange = new WstBinaryExchange (Constants.WstBinaryExchangeValueGss);
			rstr.BinaryExchange.Value = serverFinished;

			col.Responses.Add (rstr);

			// Authenticator is mandatory for MS sslnego.
			rstr = new WstRequestSecurityTokenResponse (SecurityTokenSerializer);
			rstr.Context = reader.Value.Context;
			rstr.Authenticator = tls.CreateHash (key, hash, "AUTH-HASH");
			col.Responses.Add (rstr);

			sessions.Remove (reader.Value.Context);

			return Message.CreateMessage (request.Version, Constants.WstIssueReplyAction, col);
*/
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
	}
}
