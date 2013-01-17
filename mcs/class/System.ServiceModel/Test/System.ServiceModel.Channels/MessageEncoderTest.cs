//
// MessageEncoderTest.cs
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
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using NUnit.Framework.SyntaxHelpers;

using TextElement = System.ServiceModel.Channels.TextMessageEncodingBindingElement;
using BinaryElement = System.ServiceModel.Channels.BinaryMessageEncodingBindingElement;
using MtomElement = System.ServiceModel.Channels.MtomMessageEncodingBindingElement;

namespace MonoTests.System.ServiceModel.Channels
{
	[TestFixture]
	public class MessageEncoderTest
	{
		class MyEncodingBindingElement : MessageEncodingBindingElement
		{
			public override BindingElement Clone ()
			{
				throw new Exception ();
			}

			public override T GetProperty<T> (BindingContext ctx)
			{
				throw new Exception ();
			}

			public override MessageEncoderFactory CreateMessageEncoderFactory ()
			{
				throw new Exception ();
			}

			public override MessageVersion MessageVersion {
				get { throw new Exception (); }
				set { throw new Exception (); }
			}
		}

		[Test]
		public void Properties ()
		{
			MessageEncoder t = new TextElement ().CreateMessageEncoderFactory ().Encoder;
			MessageEncoder b = new BinaryElement ().CreateMessageEncoderFactory ().Encoder;
			MessageEncoder m = new MtomElement ().CreateMessageEncoderFactory ().Encoder;

			// TextMessageEncodingBindingElement.WriteEncoding
			// default value is UTF8.
			ServiceAssert.AssertMessageEncoder (
				// Those curly double quotations are smelly, very implementation specific.
				"application/soap+xml; charset=utf-8", "application/soap+xml",
				MessageVersion.Default, t, "Text");
			ServiceAssert.AssertMessageEncoder (
				"application/soap+msbin1", "application/soap+msbin1",
				MessageVersion.Default, b, "Binary");
			ServiceAssert.AssertMessageEncoder (
				"multipart/related; type=\"application/xop+xml\"", "multipart/related",
				MessageVersion.Default, m, "Mtom");

			MessageEncoder t2 = new TextElement (
				MessageVersion.Soap11WSAddressing10,
				Encoding.UTF8)
				.CreateMessageEncoderFactory ().Encoder;

			ServiceAssert.AssertMessageEncoder (
				// Those curly double quotations are smelly, very implementation specific.
				"text/xml; charset=utf-8", "text/xml",
				MessageVersion.Soap11WSAddressing10, t2, "Text2");
		}

		[Test]
		[ExpectedException (typeof (ProtocolException))]
		public void MessageVersionMismatch ()
		{
			Message msg = Message.CreateMessage (
				// ... while BasicHttpBinding expects Soap11.
				MessageVersion.Soap11WSAddressing10,
				"http://tempuri.org/IFoo/Echo", "TEST");
			MessageEncoderFactory f = new TextMessageEncodingBindingElement (MessageVersion.Soap11, Encoding.UTF8).CreateMessageEncoderFactory ();
			f.Encoder.WriteMessage (msg, new MemoryStream ());
		}

		[Test]
		public void CreateSessionEncoder ()
		{
			Assert.AreEqual ("application/soap+xml", new TextMessageEncodingBindingElement ().CreateMessageEncoderFactory ().CreateSessionEncoder ().MediaType, "#1");
			Assert.AreEqual ("application/soap+msbinsession1", new BinaryMessageEncodingBindingElement ().CreateMessageEncoderFactory ().CreateSessionEncoder ().MediaType, "#2"); // different from application/soap+msbin1
			Assert.AreEqual ("multipart/related", new MtomMessageEncodingBindingElement ().CreateMessageEncoderFactory ().CreateSessionEncoder ().MediaType, "#3");
		}

		[Test]
		public void TestContentType ()
		{
			var element = new TextMessageEncodingBindingElement ();
			element.WriteEncoding = Encoding.UTF8;
			element.MessageVersion = MessageVersion.Soap11;
			var factory = element.CreateMessageEncoderFactory ();
			var encoder = factory.CreateSessionEncoder ();
			
			Assert.That (encoder.IsContentTypeSupported ("text/xmL;chaRset=uTf-8"), Is.True, "#1");
			Assert.That (encoder.IsContentTypeSupported ("text/xMl"), Is.True, "#2");
			Assert.That (encoder.IsContentTypeSupported ("teXt/xml;foo=bar;charset=utf-8"), Is.True, "#3");
			Assert.That (encoder.IsContentTypeSupported ("teXt/xml;charset=ascii"), Is.False, "#4");
		}

	}
}
