#if !MOBILE && !XAMMAC_4_5
using System;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using NUnit.Framework;

namespace MonoTests.System.ServiceModel
{
	[TestFixture]
	public class WebMessageEncodingBindingElementTest
	{
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructor ()
		{
			new WebMessageEncodingBindingElement (null);
		}

		[Test]
		public void DefaultPropertyValues ()
		{
			WebMessageEncodingBindingElement be = new WebMessageEncodingBindingElement ();
			Assert.AreEqual (Encoding.UTF8, be.WriteEncoding, "#1");
		}

		MessageEncoder CreateEncoder ()
		{
			WebMessageEncodingBindingElement m = new WebMessageEncodingBindingElement ();
			return m.CreateMessageEncoderFactory ().Encoder;
		}

		[Test]
		public void MessageEncoder ()
		{
			var e = CreateEncoder ();
			Assert.AreEqual ("application/xml", e.MediaType, "#1");
			Assert.AreEqual ("application/xml; charset=utf-8", e.ContentType, "#2");
		}

		[Test]
		[ExpectedException (typeof (ProtocolException))]
		public void MessageEncoderWriteMessageSoap12 ()
		{
			var e = CreateEncoder ();
			e.WriteMessage (Message.CreateMessage (MessageVersion.Soap12, "urn:foo"), Stream.Null);
		}

		[Test]
		public void MessageEncoderWriteMessageXml ()
		{
			var e = CreateEncoder ();
			MemoryStream ms = new MemoryStream ();
			// FIXME: nothing -> "" (right now it outputs <anyType xsi:nil='true' ... />)
			//e.WriteMessage (Message.CreateMessage (MessageVersion.None, "urn:foo"), ms);
			//Assert.AreEqual ("", Encoding.UTF8.GetString (ms.ToArray ()));

			e.WriteMessage (Message.CreateMessage (MessageVersion.None, "urn:foo", 135), ms);
			Assert.AreEqual ("<int xmlns=\"http://schemas.microsoft.com/2003/10/Serialization/\">135</int>", Encoding.UTF8.GetString (ms.ToArray ()));
		}

		BindingContext CreateBindingContext ()
		{
			return new BindingContext (new CustomBinding (new HttpTransportBindingElement () { AllowCookies = true }), new BindingParameterCollection ());
		}

		BindingContext CreateBindingContext2 ()
		{
			return new BindingContext (new CustomBinding (new TcpTransportBindingElement ()), new BindingParameterCollection ());
		}

		[Test]
		public void CanBuildChannelFactory ()
		{
			// with HttpTransport
			var m = new WebMessageEncodingBindingElement ();
			Assert.IsTrue (m.CanBuildChannelFactory<IRequestChannel> (CreateBindingContext ()), "#1");
			Assert.IsFalse (m.CanBuildChannelFactory<IReplyChannel> (CreateBindingContext ()), "#2");
			Assert.IsFalse (m.CanBuildChannelFactory<IRequestSessionChannel> (CreateBindingContext ()), "#3");
			Assert.IsFalse (m.CanBuildChannelFactory<IDuplexChannel> (CreateBindingContext ()), "#4");

			// actually they are from transport
			var h = new HttpTransportBindingElement ();
			Assert.IsTrue (h.CanBuildChannelFactory<IRequestChannel> (CreateBindingContext ()), "#5");
			Assert.IsFalse (h.CanBuildChannelFactory<IReplyChannel> (CreateBindingContext ()), "#6");
			Assert.IsFalse (h.CanBuildChannelFactory<IRequestSessionChannel> (CreateBindingContext ()), "#7");
			Assert.IsFalse (h.CanBuildChannelFactory<IDuplexChannel> (CreateBindingContext ()), "#8");

			// with TcpTransport
			Assert.IsFalse (m.CanBuildChannelFactory<IRequestChannel> (CreateBindingContext2 ()), "#9");
			Assert.IsFalse (m.CanBuildChannelFactory<IReplyChannel> (CreateBindingContext2 ()), "#10");
			Assert.IsFalse (m.CanBuildChannelFactory<IRequestSessionChannel> (CreateBindingContext2 ()), "#11");
			Assert.IsFalse (m.CanBuildChannelFactory<IDuplexChannel> (CreateBindingContext2 ()), "#12");

			// ... yes, actually they are from transport
			var t = new TcpTransportBindingElement ();
			Assert.IsFalse (t.CanBuildChannelFactory<IRequestChannel> (CreateBindingContext2 ()), "#13");
			Assert.IsFalse (t.CanBuildChannelFactory<IReplyChannel> (CreateBindingContext2 ()), "#14");
			Assert.IsFalse (t.CanBuildChannelFactory<IRequestSessionChannel> (CreateBindingContext2 ()), "#15");
			Assert.IsFalse (t.CanBuildChannelFactory<IDuplexChannel> (CreateBindingContext2 ()), "#16");
		}

		[Test]
		public void CanBuildChannelListener ()
		{
			// with HttpTransport
			var m = new WebMessageEncodingBindingElement ();
			Assert.IsFalse (m.CanBuildChannelListener<IRequestChannel> (CreateBindingContext ()), "#1");
			Assert.IsTrue (m.CanBuildChannelListener<IReplyChannel> (CreateBindingContext ()), "#2");
			Assert.IsFalse (m.CanBuildChannelListener<IRequestSessionChannel> (CreateBindingContext ()), "#3");
			Assert.IsFalse (m.CanBuildChannelListener<IDuplexChannel> (CreateBindingContext ()), "#4");

			// actually they are from transport
			var h = new HttpTransportBindingElement ();
			Assert.IsFalse (h.CanBuildChannelListener<IRequestChannel> (CreateBindingContext ()), "#5");
			Assert.IsTrue  (h.CanBuildChannelListener<IReplyChannel> (CreateBindingContext ()), "#6");
			Assert.IsFalse (h.CanBuildChannelListener<IRequestSessionChannel> (CreateBindingContext ()), "#7");
			Assert.IsFalse (h.CanBuildChannelListener<IDuplexChannel> (CreateBindingContext ()), "#8");

			// with TcpTransport
			Assert.IsFalse (m.CanBuildChannelListener<IRequestChannel> (CreateBindingContext2 ()), "#9");
			Assert.IsFalse (m.CanBuildChannelListener<IReplyChannel> (CreateBindingContext2 ()), "#10");
			Assert.IsFalse (m.CanBuildChannelListener<IRequestSessionChannel> (CreateBindingContext2 ()), "#11");
			Assert.IsFalse (m.CanBuildChannelListener<IDuplexChannel> (CreateBindingContext2 ()), "#12");

			// ... yes, actually they are from transport
			var t = new TcpTransportBindingElement ();
			Assert.IsFalse (t.CanBuildChannelListener<IRequestChannel> (CreateBindingContext2 ()), "#13");
			Assert.IsFalse (t.CanBuildChannelListener<IReplyChannel> (CreateBindingContext2 ()), "#14");
			Assert.IsFalse (t.CanBuildChannelListener<IRequestSessionChannel> (CreateBindingContext2 ()), "#15");
			Assert.IsFalse (t.CanBuildChannelListener<IDuplexChannel> (CreateBindingContext2 ()), "#16");
		}

		[Test]
		public void BuildChannelFactory ()
		{
			var m = new WebMessageEncodingBindingElement ();
			var f = m.BuildChannelFactory<IRequestChannel> (CreateBindingContext ());
			Assert.AreEqual (f.GetType (), new HttpTransportBindingElement ().BuildChannelFactory<IRequestChannel> (CreateBindingContext ()).GetType (), "#1");
		}

		[Test]
		public void GetPropertyMessageVersion ()
		{
			var m = new WebMessageEncodingBindingElement ();
			var bc = new BindingContext (new CustomBinding (), new BindingParameterCollection ());
			Assert.AreEqual (MessageVersion.None, m.GetProperty<MessageVersion> (bc), "#1");
		}

		[Test]
		public void ReadMessageNullContentType ()
		{
			var e = CreateEncoder ();
			e.ReadMessage (new MemoryStream (), 10, null);
		}

		[Test]
		public void MessageEncoderIsContentTypeSupported ()
		{
			var enc = new WebMessageEncodingBindingElement ().CreateMessageEncoderFactory ().Encoder;
			Assert.IsTrue (enc.IsContentTypeSupported ("application/xml"), "#1");
			Assert.IsTrue (enc.IsContentTypeSupported ("text/xml"), "#2");
			Assert.IsTrue (enc.IsContentTypeSupported ("application/soap+xml"), "#3");
			Assert.IsTrue (enc.IsContentTypeSupported ("application/foobar+xml"), "#4"); // wow.
			Assert.IsTrue (enc.IsContentTypeSupported ("application"), "#5"); // wow.
			Assert.IsTrue (enc.IsContentTypeSupported (String.Empty), "#6"); // wow.
		}
	}
}
#endif