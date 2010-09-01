//
// TypedMessageConverterTest.cs
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
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Xml;
using NUnit.Framework;

namespace MonoTests.System.ServiceModel.Description
{
	[TestFixture]
	public class TypedMessageConverterTest
	{
		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void InvalidArgumentType ()
		{
			TypedMessageConverter.Create (
				typeof (int), "http://tempuri.org/MyTest");
		}

		[Test]
		// It is imported from samples/typed-message-converter.
		public void StandardToMessage ()
		{
			TypedMessageConverter c = TypedMessageConverter.Create (
				typeof (Test1), "http://tempuri.org/MyTest");
			Message msg = c.ToMessage (new Test1 ());

			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (msg.ToString ());

			XmlNamespaceManager nss = new XmlNamespaceManager (doc.NameTable);
			nss.AddNamespace ("s", "http://www.w3.org/2003/05/soap-envelope");
			nss.AddNamespace ("t", "http://tempuri.org/");
			nss.AddNamespace ("v", "space");
			nss.AddNamespace ("w", "yy1");
			XmlElement el = doc.SelectSingleNode ("/s:Envelope/s:Body/v:MyName", nss) as XmlElement;
			Assert.IsNotNull (el, "#1");
			XmlNode part = el.SelectSingleNode ("t:body2", nss);
			Assert.IsNotNull (part, "#2");
			Assert.AreEqual ("TEST body", part.InnerText, "#3");
			Assert.IsNotNull (el.SelectSingleNode ("w:xx1", nss), "#4");
			part = el.SelectSingleNode ("w:xx1/v:msg", nss);
			Assert.IsNotNull (part, "#5");
			Assert.AreEqual ("default", part.InnerText, "#6");
		}

		[Test]
		public void StandardRoundtrip ()
		{
			TypedMessageConverter c = TypedMessageConverter.Create (
				typeof (Test1), "http://tempuri.org/MyTest");
			Test1 t1 = new Test1 ();
			t1.echo.msg = "test";
			t1.body2 = "testtest";
			Message msg = c.ToMessage (t1);
			Test1 t2 = (Test1) c.FromMessage (msg);
			Assert.AreEqual ("test", t2.echo.msg, "#01");
			Assert.AreEqual ("testtest", t2.body2, "#02");
		}

		[Test]
		public void StandardRoundtrip2 ()
		{
			TypedMessageConverter c = TypedMessageConverter.Create (
				typeof (Test2), "http://tempuri.org/MyTest");
			var t1 = new Test2 ();
			t1.Body = new Echo () { msg = "test" };
			var uid = Guid.NewGuid ();
			t1.Id = uid;
			Message msg = c.ToMessage (t1);
			var t2 = (Test2) c.FromMessage (msg);
			Assert.AreEqual ("test", t2.Body.msg, "#01");
			// FIXME: enable it (not working yet)
			//Assert.AreEqual (uid, t2.Id, "#02");
		}

		[Test]
		public void XmlSerializerdRoundtrip ()
		{
			TypedMessageConverter c = TypedMessageConverter.Create (
				typeof (Test1), "http://tempuri.org/MyTest", new XmlSerializerFormatAttribute ());
			Test1 t1 = new Test1 ();
			t1.echo.msg = "test";
			t1.body2 = "testtest";
			Message msg = c.ToMessage (t1);
			Test1 t2 = (Test1) c.FromMessage (msg);
			Assert.AreEqual ("test", t2.echo.msg, "#01");
			Assert.AreEqual ("testtest", t2.body2, "#02");
		}
	}

	[MessageContract (WrapperNamespace = "space", WrapperName = "MyName")]
	public class Test1
	{
		[MessageBodyMember (Name = "xx1", Namespace = "yy1")]
		public Echo echo = new Echo ();

		[MessageBodyMember]
		public string body2 = "TEST body";
	}

	[DataContract (Namespace = "space")]
	public class Echo
	{
		[DataMember]
		public string msg = "default";
	}

	[MessageContract (IsWrapped = false)]
	public class Test2
	{
		[MessageHeader (Name = "head", Namespace = "urn:foo")]
		public Guid Id { get; set; }
		[MessageBodyMember (Name = "body", Namespace = "urn:foo")]
		public Echo Body { get; set; }
	}
}
