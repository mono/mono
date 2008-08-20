//
// CustomBindingTest.cs
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
using System.Net.Security;
using System.IdentityModel.Tokens;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using System.ServiceModel.Security.Tokens;
using System.Text;
using System.Xml;
using NUnit.Framework;

namespace MonoTests.System.ServiceModel.Channels
{
	[TestFixture]
	public class CustomBindingTest
	{
		[Test]
		public void DefaultCtor ()
		{
			CustomBinding cb = new CustomBinding ();
			
			Assert.AreEqual (0, cb.Elements.Count, "#1");
			Assert.AreEqual ("CustomBinding", cb.Name, "#3");
			Assert.AreEqual ("http://tempuri.org/", cb.Namespace, "#4");
			Assert.AreEqual (TimeSpan.FromMinutes (1), cb.OpenTimeout, "#5");
			Assert.AreEqual (TimeSpan.FromMinutes (1), cb.CloseTimeout, "#6");
			Assert.AreEqual (TimeSpan.FromMinutes (1), cb.SendTimeout, "#7");
			Assert.AreEqual (TimeSpan.FromMinutes (10), cb.ReceiveTimeout, "#8");
			Assert.AreEqual (0, cb.CreateBindingElements ().Count, "#9");
		}

		class MyBinding : Binding
		{
		        public override string Scheme { get { return "hoge"; } }

		        public override BindingElementCollection CreateBindingElements ()
		        {
		                throw new ApplicationException ("HEHE");
		        }
		}

		[Test]
		public void CtorFromAnotherBinding ()
		{
			CustomBinding cb =
				new CustomBinding (new WSHttpBinding ());
			// Its properties become mostly copy of the original one
			Assert.AreEqual (4, cb.Elements.Count, "#1");
			Assert.AreEqual ("http", cb.Scheme, "#2");
			Assert.AreEqual ("WSHttpBinding", cb.Name, "#3");
			Assert.AreEqual ("http://tempuri.org/", cb.Namespace, "#4");

			Assert.AreEqual (4, cb.CreateBindingElements ().Count, "#9");
		}

		[Test]
		[ExpectedException (typeof (ApplicationException))]
		public void CtorFromAnotherBindingCallsCreateBindingElement ()
		{
			new CustomBinding (new MyBinding ());
		}

		Message reqmsg, resmsg;

		[Test]
		public void CustomTransportDoesNotRequireMessageEncoding ()
		{
			ReplyHandler replier = delegate (Message msg) {
				resmsg = msg;
			};

			RequestReceiver receiver = delegate () {
				return reqmsg;
			};

			RequestSender sender = delegate (Message msg) {
				reqmsg = msg;

				CustomBinding br = new CustomBinding (
					new HandlerTransportBindingElement (replier, receiver));
				IChannelListener<IReplyChannel> l =
					br.BuildChannelListener<IReplyChannel> (
						new BindingParameterCollection ());
				l.Open ();
				IReplyChannel rch = l.AcceptChannel ();
				rch.Open ();
				Message res = Message.CreateMessage (MessageVersion.Default, "urn:succeeded");
				rch.ReceiveRequest ().Reply (res);
				rch.Close ();
				l.Close ();

				return resmsg;
			};

			CustomBinding bs = new CustomBinding (
				new HandlerTransportBindingElement (sender));

			IChannelFactory<IRequestChannel> f =
				bs.BuildChannelFactory<IRequestChannel> (
					new BindingParameterCollection ());
			f.Open ();
			IRequestChannel ch = f.CreateChannel (new EndpointAddress ("urn:dummy"));
			ch.Open ();
			Message result = ch.Request (Message.CreateMessage (MessageVersion.Default, "urn:request"));
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		//  Envelope Version 'EnvelopeNone (http://schemas.microsoft.com/ws/2005/05/envelope/none)'
		// does not support adding Message Headers.
		public void MessageSecurityPOX ()
		{
			SymmetricSecurityBindingElement sbe =
				new SymmetricSecurityBindingElement ();
			sbe.ProtectionTokenParameters =
				new X509SecurityTokenParameters ();
			RequestSender sender = delegate (Message input) {
				MessageBuffer buf = input.CreateBufferedCopy (0x10000);
				using (XmlWriter w = XmlWriter.Create (Console.Error)) {
					buf.CreateMessage ().WriteMessage (w);
				}
				return buf.CreateMessage ();
			};

			CustomBinding binding = new CustomBinding (
				sbe,
				new TextMessageEncodingBindingElement (
					MessageVersion.None, Encoding.UTF8),
				new HandlerTransportBindingElement (sender));

			EndpointAddress address = new EndpointAddress (
				new Uri ("http://localhost:8080"),
				new X509CertificateEndpointIdentity (new X509Certificate2 ("Test/Resources/test.pfx", "mono")));

			ChannelFactory<IRequestChannel> cf =
				new ChannelFactory<IRequestChannel> (binding, address);
			IRequestChannel ch = cf.CreateChannel ();
/*
			// neither of Endpoint, Contract nor its Operation seems
			// to have applicable behaviors (except for
			// ClientCredentials)
			Assert.AreEqual (1, cf.Endpoint.Behaviors.Count, "EndpointBehavior");
			Assert.AreEqual (0, cf.Endpoint.Contract.Behaviors.Count, "ContractBehavior");
			Assert.AreEqual (1, cf.Endpoint.Contract.Operations.Count, "Operations");
			OperationDescription od = cf.Endpoint.Contract.Operations [0];
			Assert.AreEqual (0, od.Behaviors.Count, "OperationBehavior");
*/

			ch.Open ();
			try {
				ch.Request (Message.CreateMessage (MessageVersion.None, "urn:myaction"));
			} finally {
				ch.Close ();
			}
		}

		[Test]
		[Ignore ("it's underway")]
		[Category ("NotWorking")]
		public void MessageSecurityManualProtection ()
		{
			SymmetricSecurityBindingElement sbe =
				new SymmetricSecurityBindingElement ();
			sbe.ProtectionTokenParameters =
				new X509SecurityTokenParameters ();
			RequestSender sender = delegate (Message input) {
				MessageBuffer buf = input.CreateBufferedCopy (0x10000);
				using (XmlWriter w = XmlWriter.Create (Console.Error)) {
					buf.CreateMessage ().WriteMessage (w);
				}
				return buf.CreateMessage ();
			};

			CustomBinding binding = new CustomBinding (
				sbe,
				new TextMessageEncodingBindingElement (),
				new HandlerTransportBindingElement (sender));

			EndpointAddress address = new EndpointAddress (
				new Uri ("http://localhost:8080"),
				new X509CertificateEndpointIdentity (new X509Certificate2 ("Test/Resources/test.pfx", "mono")));

			ChannelProtectionRequirements reqs =
				new ChannelProtectionRequirements ();
			reqs.OutgoingSignatureParts.AddParts (
				new MessagePartSpecification (new XmlQualifiedName ("SampleValue", "urn:foo")), "urn:myaction");
			BindingParameterCollection parameters =
				new BindingParameterCollection ();
			parameters.Add (reqs);
/*
			SymmetricSecurityBindingElement innersbe =
				new SymmetricSecurityBindingElement ();
			innersbe.ProtectionTokenParameters =
				new X509SecurityTokenParameters ();
			sbe.ProtectionTokenParameters =
				new SecureConversationSecurityTokenParameters (
					innersbe, false, reqs);
*/

			IChannelFactory<IRequestChannel> cf =
				binding.BuildChannelFactory<IRequestChannel> (parameters);
			cf.Open ();
			IRequestChannel ch = cf.CreateChannel (address);

			ch.Open ();
			try {
				ch.Request (Message.CreateMessage (MessageVersion.None, "urn:myaction", new SampleValue ()));
			} finally {
				ch.Close ();
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		[Category ("NotWorking")]
		public void BuildChannelListenerNoTransport ()
		{
			CustomBinding cb = new CustomBinding (
				new TextMessageEncodingBindingElement (),
				new CompositeDuplexBindingElement ());
			BindingContext ctx = new BindingContext (
				cb, new BindingParameterCollection ());
			new TextMessageEncodingBindingElement ().BuildChannelListener<IReplyChannel> (ctx);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		[Category ("NotWorking")]
		public void BuildChannelListenerWithDuplicateElement ()
		{
			CustomBinding cb = new CustomBinding (
				new TextMessageEncodingBindingElement (),
				new HttpTransportBindingElement ());
			BindingContext ctx = new BindingContext (
				cb, new BindingParameterCollection (),
				new Uri ("http://localhost:8080"), String.Empty, ListenUriMode.Unique);
			new TextMessageEncodingBindingElement ().BuildChannelListener<IReplyChannel> (ctx);
		}
	}

	[DataContract (Namespace = "urn:foo")]
	class SampleValue
	{
	}
}
