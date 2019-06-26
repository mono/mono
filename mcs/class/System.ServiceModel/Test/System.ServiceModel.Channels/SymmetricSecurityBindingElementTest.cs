//
// SymmetricSecurityBindingElementTest.cs
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
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using System.ServiceModel.Security.Tokens;
using System.Xml;
using NUnit.Framework;

using MonoTests.Helpers;

namespace MonoTests.System.ServiceModel.Channels
{
	[TestFixture]
	public class SymmetricSecurityBindingElementTest
	{
		[Test]
		public void DefaultValues ()
		{
			SymmetricSecurityBindingElement be =
				new SymmetricSecurityBindingElement ();

			SecurityAssert.AssertSymmetricSecurityBindingElement (
				SecurityAlgorithmSuite.Default,
				true, // IncludeTimestamp
				SecurityKeyEntropyMode.CombinedEntropy,
				MessageProtectionOrder.SignBeforeEncryptAndEncryptSignature,
				MessageSecurityVersion.Default,
				false, // RequireSignatureConfirmation
				SecurityHeaderLayout.Strict,
				// EndpointSupportingTokenParameters: endorsing, signed, signedEncrypted, signedEndorsing (by count)
				0, 0, 0, 0,
				// ProtectionTokenParameters
				false,
				default (SecurityTokenInclusionMode),
				default (SecurityTokenReferenceStyle),
				default (bool),
				// LocalClientSettings
				true, 60, true,

				be, "");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void BuildChannelWithoutProtectionTokenParameters ()
		{
			CustomBinding b = new CustomBinding (
				new SymmetricSecurityBindingElement (),
				new TextMessageEncodingBindingElement (),
				new HttpTransportBindingElement ());
			b.BuildChannelFactory<IRequestChannel> (new BindingParameterCollection ());
		}

		CustomBinding CreateBinding ()
		{
			RequestSender handler = delegate (Message input) {
				throw new Exception ();
			};
			return CreateBinding (handler);
		}

		CustomBinding CreateBinding (RequestSender sender)
		{
			return CreateBinding (sender, new X509SecurityTokenParameters ());
		}

		CustomBinding CreateBinding (RequestSender sender, bool isOneWay)
		{
			return CreateBinding (sender, new X509SecurityTokenParameters (), isOneWay);
		}

		CustomBinding CreateBinding (SecurityTokenParameters protectionTokenParameters)
		{
			RequestSender handler = delegate (Message input) {
				throw new Exception ();
			};
			return CreateBinding (handler, protectionTokenParameters);
		}

		CustomBinding CreateBinding (RequestSender sender,
			SecurityTokenParameters protectionTokenParameters)
		{
			return CreateBinding (sender, protectionTokenParameters, false);
		}

		CustomBinding CreateBinding (RequestSender sender,
			SecurityTokenParameters protectionTokenParameters,
			bool isOneWay)
		{
			SymmetricSecurityBindingElement sbe =
				new SymmetricSecurityBindingElement ();
			sbe.ProtectionTokenParameters = protectionTokenParameters;
			List<BindingElement> l = new List<BindingElement> ();
			l.Add (sbe);
			l.Add (new TextMessageEncodingBindingElement ());
			if (isOneWay)
				l.Add (new OneWayBindingElement ());
			l.Add (new HandlerTransportBindingElement (sender));
			CustomBinding b = new CustomBinding (l);
			return b;
		}

		CustomBinding CreateBinding (ReplyHandler replier, RequestReceiver receiver)
		{
			SymmetricSecurityBindingElement sbe =
				new SymmetricSecurityBindingElement ();
			sbe.ProtectionTokenParameters =
				new X509SecurityTokenParameters ();
			CustomBinding b = new CustomBinding (
				sbe,
				new TextMessageEncodingBindingElement (),
				new HandlerTransportBindingElement (replier, receiver));
			return b;
		}

		EndpointAddress CreateX509EndpointAddress (string uri)
		{
			EndpointIdentity identity =
				new X509CertificateEndpointIdentity (new X509Certificate2 (TestResourceHelper.GetFullPathOfResource ("Test/Resources/test.pfx"), "mono"));
			return new EndpointAddress (new Uri (uri), identity);
		}

		IChannelListener<IReplyChannel> CreateListener (ReplyHandler handler, RequestReceiver receiver)
		{
			CustomBinding rb = CreateBinding (handler, receiver);
			BindingParameterCollection bpl =
				new BindingParameterCollection ();
			ServiceCredentials cred = new ServiceCredentials ();
			cred.ServiceCertificate.Certificate =
				new X509Certificate2 (TestResourceHelper.GetFullPathOfResource ("Test/Resources/test.pfx"), "mono");
			IServiceBehavior sb = cred;
			sb.AddBindingParameters (null, null, null, bpl);
			IChannelListener<IReplyChannel> listener = rb.BuildChannelListener<IReplyChannel> (bpl);
			return listener;
		}

		[Test]
		public void OpenChannelFactory ()
		{
			CustomBinding b = CreateBinding ();

			IChannelFactory<IRequestChannel> f =
				b.BuildChannelFactory<IRequestChannel> (new BindingParameterCollection ());
			f.Open ();
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void BuildChannelWithoutOpen ()
		{
			CustomBinding b = CreateBinding ();

			IChannelFactory<IRequestChannel> f =
				b.BuildChannelFactory<IRequestChannel> (new BindingParameterCollection ());
			f.CreateChannel (CreateX509EndpointAddress ("stream:dummy"));
		}

		[Test]
		public void OpenRequestNonAuthenticatable ()
		{
			SymmetricSecurityBindingElement sbe = 
				new SymmetricSecurityBindingElement ();
			sbe.ProtectionTokenParameters =
				new UserNameSecurityTokenParameters ();
			Binding binding = new CustomBinding (sbe, new HandlerTransportBindingElement (null));
			BindingParameterCollection pl =
				new BindingParameterCollection ();
			ClientCredentials cred = new ClientCredentials ();
			cred.UserName.UserName = "mono";
			pl.Add (cred);
			IChannelFactory<IRequestChannel> f =
				binding.BuildChannelFactory<IRequestChannel> (pl);
			f.Open ();
			IRequestChannel ch = f.CreateChannel (new EndpointAddress ("stream:dummy"));
			try {
				ch.Open ();
				Assert.Fail ("NotSupportedException is expected.");
			} catch (NotSupportedException) {
			}
		}

		// The service certificate is not provided for target
		// 'stream:dummy'. Specify a service certificate in 
		// ClientCredentials.
		[Test]
		public void OpenRequestWithoutServiceCertificate ()
		{
			CustomBinding b = CreateBinding ();

			IChannelFactory<IRequestChannel> f =
				b.BuildChannelFactory<IRequestChannel> (new BindingParameterCollection ());
			f.Open ();
			// This EndpointAddress does not contain X509 identity
			IRequestChannel ch = f.CreateChannel (new EndpointAddress ("stream:dummy"));
			try {
				ch.Open ();
				Assert.Fail ("expected InvalidOperationException here.");
			} catch (InvalidOperationException) {
			}
		}

		IChannelFactory<IRequestChannel> CreateDefaultServiceCertFactory ()
		{
			CustomBinding b = CreateBinding (delegate (Message req) {
				return null;
				});
			ClientCredentials cred = new ClientCredentials ();
			cred.ServiceCertificate.DefaultCertificate = new X509Certificate2 (TestResourceHelper.GetFullPathOfResource ("Test/Resources/test.pfx"), "mono");
			BindingParameterCollection parameters =
				new BindingParameterCollection ();
			parameters.Add (cred);
			ChannelProtectionRequirements cp =
				new ChannelProtectionRequirements ();
			cp.IncomingSignatureParts.AddParts (
				new MessagePartSpecification (true),
				"http://tempuri.org/MyAction");
			cp.IncomingEncryptionParts.AddParts (
				new MessagePartSpecification (true),
				"http://tempuri.org/MyAction");
			parameters.Add (cp);

			return b.BuildChannelFactory<IRequestChannel> (parameters);
		}

		[Test]
		public void OpenRequestWithDefaultServiceCertificate ()
		{
			IChannelFactory<IRequestChannel> f =
				CreateDefaultServiceCertFactory ();
			f.Open ();
			// This EndpointAddress does not contain X509 identity
			IRequestChannel ch = f.CreateChannel (new EndpointAddress ("stream:dummy"));
			ch.Open ();
			// stop here.
		}

		[Test]
		[ExpectedException (typeof (MessageSecurityException))]
		[Category ("NotWorking")]
		// from WinFX:
		// MessageSecurityException : Identity check failed for outgoing
		// message. The expected DNS identity of the remote endpoint was
		// '' but the remote endpoint provided DNS claim 'Poupou's-
		// Software-Factory'. If this is a legitimate remote endpoint,
		// you can fix the problem by explicitly specifying DNS identity
		// 'Poupou's-Software-Factory' as the Identity property of
		// EndpointAddress when creating channel proxy.
		public void RequestWithDefaultServiceCertificateWithoutDns ()
		{
			IChannelFactory<IRequestChannel> f =
				CreateDefaultServiceCertFactory ();
			f.Open ();
			// This EndpointAddress does not contain X509 identity
			IRequestChannel ch = f.CreateChannel (new EndpointAddress ("stream:dummy"));
			ch.Open ();
			// -> MessageSecurityException (IdentityVerifier complains DNS claim)
			ch.Request (Message.CreateMessage (MessageVersion.Default, "http://tempuri.org/MyAction"));
		}

		[Test]
		[Category ("NotWorking")]
		public void RequestWithDefaultServiceCertificateWithDns ()
		{
			IChannelFactory<IRequestChannel> f =
				CreateDefaultServiceCertFactory ();
			f.Open ();
			// This EndpointAddress does not contain X509 identity
			IRequestChannel ch = f.CreateChannel (new EndpointAddress (new Uri ("stream:dummy"), new DnsEndpointIdentity ("Poupou's-Software-Factory")));
			ch.Open ();
			// -> MessageSecurityException (IdentityVerifier complains DNS claim)
			ch.Request (Message.CreateMessage (MessageVersion.Default, "http://tempuri.org/MyAction"));
		}

		[Test]
		[Category ("NotWorking")] // it depends on Kerberos
		public void OpenRequestWithoutServiceCertificateForNonX509 ()
		{
			CustomBinding b = CreateBinding (new MyOwnSecurityTokenParameters ());

			IChannelFactory<IRequestChannel> f =
				b.BuildChannelFactory<IRequestChannel> (new BindingParameterCollection ());
			f.Open ();
			// This EndpointAddress does not contain X509 identity
			IRequestChannel ch = f.CreateChannel (new EndpointAddress ("stream:dummy"));
			ch.Open ();
		}

		[Test]
		public void SendRequestWithoutOpen ()
		{
			CustomBinding b = CreateBinding ();

			IChannelFactory<IRequestChannel> f =
				b.BuildChannelFactory<IRequestChannel> (new BindingParameterCollection ());
			f.Open ();
			IRequestChannel ch = f.CreateChannel (CreateX509EndpointAddress ("stream:dummy"));
			try {
				ch.Request (Message.CreateMessage (MessageVersion.Default, "myAction"));
				Assert.Fail ("expected InvalidOperationException here.");
			} catch (InvalidOperationException) {
			}
		}

		[Test]
		[Category ("NotWorking")]
		public void SendRequestWithoutSignatureMessagePart ()
		{
			CustomBinding b = CreateBinding ();

			// without ChannelProtectionRequirements it won't be
			// signed and/or encrypted.
			IChannelFactory<IRequestChannel> f =
				b.BuildChannelFactory<IRequestChannel> (new BindingParameterCollection ());
			f.Open ();
			IRequestChannel ch = f.CreateChannel (CreateX509EndpointAddress ("stream:dummy"));

			ch.Open ();
			// MessageSecurityException : No signature message parts
			// were specified for messages with the 'myAction' 
			// action.
			try {
				ch.Request (Message.CreateMessage (b.MessageVersion, "myAction"));
				Assert.Fail ("MessageSecurityException is expected here.");
			} catch (MessageSecurityException) {
			}
		}


		[Test]
		[ExpectedException (typeof (Exception))]
		[Category ("NotWorking")]
		public void SendRequestWithSignatureMessagePart ()
		{
			CustomBinding b = CreateBinding ();
			ChannelProtectionRequirements cp =
				new ChannelProtectionRequirements ();
			cp.IncomingSignatureParts.AddParts (new MessagePartSpecification (true), "myAction");
			cp.IncomingEncryptionParts.AddParts (new MessagePartSpecification (true), "myAction");
			BindingParameterCollection parameters =
				new BindingParameterCollection ();
			parameters.Add (cp);

			IChannelFactory<IRequestChannel> f =
				b.BuildChannelFactory<IRequestChannel> (parameters);
			f.Open ();
			IRequestChannel ch = f.CreateChannel (CreateX509EndpointAddress ("stream:dummy"));

			ch.Open ();
			ch.Request (Message.CreateMessage (b.MessageVersion, "myAction"));
		}

		[Test]
		[Category ("NotWorking")] // it requires OneWay
		public void RequestBasedOnContract1 ()
		{
			CustomBinding b = CreateBinding (delegate (Message input) {
				return null;
			}, true);

			IFoo foo = ChannelFactory<IFoo>.CreateChannel (b, CreateX509EndpointAddress ("stream:dummy"));
			foo.Bar (Message.CreateMessage (b.MessageVersion, "http://tempuri.org/IFoo/Bar"));
		}

		[Test]
		[Category ("NotWorking")] // it requires OneWay
		public void RequestBasedOnContract2 ()
		{
			CustomBinding b = CreateBinding (delegate (Message input) {
				return null;
			}, true);

			IFoo foo = ChannelFactory<IFoo>.CreateChannel (b, CreateX509EndpointAddress ("stream:dummy"));
			foo.Baz ("TEST");
		}

		[Test]
		// it still does not produce secure message ...
		[Category ("NotWorking")]
		public void RequestBasedOnContract3 ()
		{
			CustomBinding b = CreateBinding (delegate (Message input) {
				// seems like security message property is not attached to the request.
				foreach (object o in input.Properties.Values)
					if (o is SecurityMessageProperty)
						Assert.Fail ("there should be a SecurityMessageProperty.");
				return null;
			}, true);

			IFoo foo = ChannelFactory<IFoo>.CreateChannel (b, CreateX509EndpointAddress ("stream:dummy"));
			foo.Bleh ("TEST");
		}

		// from WCF (beta2):
		// "MessageSecurityException : Security processor was unable
		// to find a security header in the message. This might be
		// because the message is an unsecured fault or because there
		// is a binding mismatch between the communicating parties.
		// This can occur if the service is configured for security
		// and the client is not using security."
		[Test]
		[ExpectedException (typeof (MessageSecurityException))]
		[Category ("NotWorking")]
		public void RequestUnsecuredReply ()
		{
			CustomBinding b = CreateBinding (delegate (Message input) {
				return input;
			});

			IFoo foo = ChannelFactory<IFoo>.CreateChannel (b, CreateX509EndpointAddress ("stream:dummy"));
			foo.Bar (Message.CreateMessage (b.MessageVersion, "http://tempuri.org/IFoo/Bar"));
		}

		[ServiceContract]
		interface IFoo
		{
			[OperationContract (IsOneWay = true)]
			void Bar (Message msg);

			[OperationContract (IsOneWay = true)]
			void Baz (string src);

			[OperationContract (ProtectionLevel = ProtectionLevel.Sign, IsOneWay = true)]
			void Bleh (string src);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void BuildListenerWithoutProtectionTokenParameters ()
		{
			CustomBinding b = new CustomBinding (
				new SymmetricSecurityBindingElement (),
				new TextMessageEncodingBindingElement (),
				new HttpTransportBindingElement ());
			b.BuildChannelListener<IReplyChannel> (new BindingParameterCollection ());
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void OpenListenerWithoutServiceCertificate ()
		{
			CustomBinding rb = CreateBinding ();
			IChannelListener<IReplyChannel> listener = rb.BuildChannelListener<IReplyChannel> (new BindingParameterCollection ());
			listener.Open ();
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void OpenListenerNoPrivateKeyInServiceCertificate ()
		{
			CustomBinding rb = CreateBinding ();
			BindingParameterCollection bpl =
				new BindingParameterCollection ();
			ServiceCredentials cred = new ServiceCredentials ();
			cred.ServiceCertificate.Certificate =
				new X509Certificate2 (TestResourceHelper.GetFullPathOfResource ("Test/Resources/test.cer"));
			IServiceBehavior sb = cred;
			sb.AddBindingParameters (null, null, null, bpl);
			IChannelListener<IReplyChannel> listener = rb.BuildChannelListener<IReplyChannel> (bpl);
			listener.Open ();
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void AcceptChannelWithoutOpenListener ()
		{
			IChannelListener<IReplyChannel> listener = CreateListener (null, null);
			listener.AcceptChannel ();
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		[Category ("NotWorking")]
		public void ReceiveRequestWithoutOpenChannel ()
		{
			IChannelListener<IReplyChannel> listener = CreateListener (null, null);
			listener.Open ();
			IReplyChannel reply = listener.AcceptChannel ();
			reply.ReceiveRequest ();
		}

		[Test]
		[Ignore ("It's not working")]
		[ExpectedException (typeof (ApplicationException))]
		public void ReceiveRequest ()
		{
			// Seems like this method is invoked to send a reply
			// with related to "already created" SOAP fault.
			//
			// It is still not understandable that this delegate
			// is invoked as an infinite loop ...
			ReplyHandler handler = delegate (Message input) {
Console.Error.WriteLine ("Processing a reply.");
				// a:InvalidSecurity
				// An error occurred when verifying security for the message.
				Assert.IsTrue (input.IsFault);
				throw new ApplicationException ();
			};
			Message msg = Message.CreateMessage (MessageVersion.Default, "myAction");
			RequestReceiver receiver = delegate () {
				return msg;
			};
			IChannelListener<IReplyChannel> listener = CreateListener (handler, receiver);
			listener.Open ();
			IReplyChannel reply = listener.AcceptChannel ();
			reply.Open ();
			RequestContext ctx = reply.EndReceiveRequest (reply.BeginReceiveRequest (null, null));
		}

		// Without SecurityBindingElement it works.
		// With it, it causes kind of infinite loop around 
		// RequestContext.get_RequestMessage() which somehow blocks
		// finishing HandlerTransportRequestChannel.Request() (and
		// it continues until the timeout).
		[Test]
		[Ignore ("It's not working")]
		[Category ("NotWorking")]
		public void FullRequest ()
		{
			EndpointIdentity identity =
				new X509CertificateEndpointIdentity (new X509Certificate2 (TestResourceHelper.GetFullPathOfResource ("Test/Resources/test.pfx"), "mono"));
			EndpointAddress address =
				new EndpointAddress (new Uri ("stream:dummy"), identity);

			Message mreq = Message.CreateMessage (MessageVersion.Default, "myAction");
			Message mreply = null;

XmlWriterSettings settings = new XmlWriterSettings ();
settings.Indent = true;

			// listener setup
			ReplyHandler replyHandler = delegate (Message rinput) {
				mreply = rinput;
			};
			RequestReceiver receiver = delegate () {
				return mreq;
			};
			IChannelListener<IReplyChannel> listener = CreateListener (replyHandler, receiver);
			listener.Open ();
			IReplyChannel reply = listener.AcceptChannel ();
			reply.Open ();

			RequestSender reqHandler = delegate (Message input) {
				try {
					// sync version somehow causes an infinite loop (!?)
					RequestContext ctx = reply.EndReceiveRequest (reply.BeginReceiveRequest (TimeSpan.FromSeconds (5), null, null));
//					RequestContext ctx = reply.ReceiveRequest (TimeSpan.FromSeconds (5));
					Console.Error.WriteLine ("Acquired RequestContext.");
					ctx.Reply (input);
				} catch (Exception ex) {
					Console.Error.WriteLine ("ERROR during processing a request in FullRequest()");
					Console.Error.WriteLine (ex);
					Console.Error.Flush ();
					throw;
				}
				return mreply;
			};
			CustomBinding b = CreateBinding (reqHandler);

			IRequestChannel ch = ChannelFactory<IRequestChannel>.CreateChannel (b, address);

			ch.Open ();
			Console.Error.WriteLine ("**** starting a request  ****");
			IAsyncResult async = ch.BeginRequest (mreq, null, null);
			Console.Error.WriteLine ("**** request started. ****");
			Message res = ch.EndRequest (async);
		}

		[Test]
		public void SetKeyDerivation ()
		{
			SymmetricSecurityBindingElement be;
			X509SecurityTokenParameters p;

			be = new SymmetricSecurityBindingElement ();
			p = new X509SecurityTokenParameters ();
			be.ProtectionTokenParameters = p;
			be.SetKeyDerivation (false);
			Assert.AreEqual (false, p.RequireDerivedKeys, "#1");

			be = new SymmetricSecurityBindingElement ();
			p = new X509SecurityTokenParameters ();
			be.SetKeyDerivation (false); // set in prior - makes no sense
			be.ProtectionTokenParameters = p;
			Assert.AreEqual (true, p.RequireDerivedKeys, "#2");
		}
	}

	class MyOwnSecurityTokenParameters : SecurityTokenParameters
	{
		public MyOwnSecurityTokenParameters ()
		{
		}

		protected MyOwnSecurityTokenParameters (MyOwnSecurityTokenParameters source)
		{
		}

		protected override bool HasAsymmetricKey {
			get { return false; }
		}

		protected override bool SupportsClientAuthentication {
			get { return true; }
		}

		protected override bool SupportsClientWindowsIdentity {
			get { return false; }
		}

		protected override bool SupportsServerAuthentication {
			get { return true; }
		}

		protected override SecurityTokenParameters CloneCore ()
		{
			return new MyOwnSecurityTokenParameters (this);
		}

		protected override SecurityKeyIdentifierClause CreateKeyIdentifierClause (
			SecurityToken token, SecurityTokenReferenceStyle referenceStyle)
		{
			throw new NotImplementedException ();
		}

		protected override void InitializeSecurityTokenRequirement (SecurityTokenRequirement requirement)
		{
			// If there were another token type that supports protection
			// and does not require X509, it should be used instead ...
			requirement.TokenType = SecurityTokenTypes.Kerberos;
		}
	}
}
#endif
