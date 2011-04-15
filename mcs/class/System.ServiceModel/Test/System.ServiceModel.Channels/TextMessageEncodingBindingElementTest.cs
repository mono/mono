//
// TextMessageEncodingBindingElementTest.cs
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
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Text;
using System.Xml;
using NUnit.Framework;

using Element = System.ServiceModel.Channels.TextMessageEncodingBindingElement;

namespace MonoTests.System.ServiceModel.Channels
{
	[TestFixture]
	public class TextMessageEncodingBindingElementTest
	{
		[Test]
		public void DefaultValues ()
		{
			Element el = new Element ();
			Assert.AreEqual (64, el.MaxReadPoolSize, "#1");
			Assert.AreEqual (16, el.MaxWritePoolSize, "#2");
			Assert.AreEqual (MessageVersion.Default, el.MessageVersion, "#3");
			// FIXME: test ReaderQuotas

			Assert.AreEqual (Encoding.UTF8, el.WriteEncoding, "#4");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void BuildChannelListenerNullArg ()
		{
			new Element ().BuildChannelListener<IReplyChannel> (null);
		}

		[Test]
		public void CanBuildChannelFactory ()
		{
			CustomBinding cb = new CustomBinding (
				new HttpTransportBindingElement ());
			BindingContext ctx = new BindingContext (
				cb, new BindingParameterCollection ());
			Element el = new Element ();
			Assert.IsTrue (el.CanBuildChannelFactory<IRequestChannel> (ctx), "#1");
			Assert.IsFalse (el.CanBuildChannelFactory<IRequestSessionChannel> (ctx), "#2");
		}

		[Test]
		public void BuildChannelFactory ()
		{
			CustomBinding cb = new CustomBinding (
				new HttpTransportBindingElement ());
			BindingContext ctx = new BindingContext (
				cb, new BindingParameterCollection ());
			Element el = new Element ();
			IChannelFactory<IRequestChannel> cf =
				el.BuildChannelFactory<IRequestChannel> (ctx);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void BuildChannelListenerEmptyCustomBinding ()
		{
			CustomBinding cb = new CustomBinding ();
			BindingContext ctx = new BindingContext (
				cb, new BindingParameterCollection ());
			new Element ().BuildChannelListener<IReplyChannel> (ctx);
		}

		[Test]
		public void BuildChannelListenerWithTransport ()
		{
			CustomBinding cb = new CustomBinding (
				new HttpTransportBindingElement ());
			BindingContext ctx = new BindingContext (
				cb, new BindingParameterCollection (),
				new Uri ("http://localhost:8080"), String.Empty, ListenUriMode.Unique);
			new Element ().BuildChannelListener<IReplyChannel> (ctx);
		}

		[Test]
		public void MessageEncoderIsContentTypeSupported ()
		{
			var enc = new TextMessageEncodingBindingElement ().CreateMessageEncoderFactory ().Encoder;
			Assert.IsFalse (enc.IsContentTypeSupported ("application/xml"), "#1");
			Assert.IsFalse (enc.IsContentTypeSupported ("text/xml"), "#2");
			Assert.IsTrue (enc.IsContentTypeSupported ("application/soap+xml"), "#3");
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ReadNullStream ()
		{
			var enc = new TextMessageEncodingBindingElement ().CreateMessageEncoderFactory ().Encoder;
			enc.ReadMessage (null, 10, "text/xml");
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ReadNullBufferManager ()
		{
			var enc = new TextMessageEncodingBindingElement ().CreateMessageEncoderFactory ().Encoder;
			enc.ReadMessage (new ArraySegment<byte> (new byte [0]), null, "text/xml");
		}
		
		[Test]
		[ExpectedException (typeof (XmlException))] // (document is expected)
		public void ReadEmptyBuffer ()
		{
			var enc = new TextMessageEncodingBindingElement ().CreateMessageEncoderFactory ().Encoder;
			enc.ReadMessage (new ArraySegment<byte> (new byte [0]), BufferManager.CreateBufferManager (1000, 1000), "text/xml");
		}
	}
}
