//
// XmlSyndicationContentTest.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2007 Novell, Inc (http://www.novell.com)
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
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.ServiceModel.Syndication;
using NUnit.Framework;

using QName = System.Xml.XmlQualifiedName;

namespace MonoTests.System.ServiceModel.Syndication
{
	[TestFixture]
	public class XmlSyndicationContentTest
	{
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorNullReader ()
		{
			new XmlSyndicationContent ((XmlReader) null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorNullExtensionObject ()
		{
			new XmlSyndicationContent ("text/plain", null, (XmlObjectSerializer) null);
		}

		[Test]
		public void ConstructorNullSerializer ()
		{
			// allowed
			new XmlSyndicationContent ("text/plain", 5, (XmlObjectSerializer) null);
			new XmlSyndicationContent ("text/plain", 5, (XmlSerializer) null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorNullExtension ()
		{
			new XmlSyndicationContent ("text/plain", null);
		}

		[Test]
		public void ConstructorNullType ()
		{
			// allowed
			new XmlSyndicationContent (null, 5, (XmlObjectSerializer) null);
		}

		[Test]
		public void Type ()
		{
			XmlSyndicationContent t = new XmlSyndicationContent (null, 3, (XmlObjectSerializer) null);
			Assert.AreEqual ("text/xml", t.Type, "#1");
			t = new XmlSyndicationContent ("text/plain", 3, (XmlObjectSerializer) null);
			Assert.AreEqual ("text/plain", t.Type, "#2");
		}

		[Test]
		public void Clone ()
		{
			XmlSyndicationContent t = new XmlSyndicationContent ("text/plain", 3, (XmlObjectSerializer) null);
			t = t.Clone () as XmlSyndicationContent;
			Assert.AreEqual ("text/plain", t.Type, "#1");
		}

		[Test]
		public void WriteTo ()
		{
			XmlSyndicationContent t = new XmlSyndicationContent ("text/plain", 6, (XmlObjectSerializer) null);
			StringWriter sw = new StringWriter ();
			using (XmlWriter w = CreateWriter (sw))
				t.WriteTo (w, "root", String.Empty);
			Assert.AreEqual ("<root type=\"text/plain\"><int xmlns=\"http://schemas.microsoft.com/2003/10/Serialization/\">6</int></root>", sw.ToString ());
		}

		XmlWriter CreateWriter (StringWriter sw)
		{
			XmlWriterSettings s = new XmlWriterSettings ();
			s.OmitXmlDeclaration = true;
			return XmlWriter.Create (sw, s);
		}

		[Test]
		public void GetReaderAtContent ()
		{
			var x = new SyndicationElementExtension (6);
			// premise.
			Assert.AreEqual ("<int xmlns=\"http://schemas.microsoft.com/2003/10/Serialization/\">6</int>", x.GetReader ().ReadOuterXml (), "#1");

			var t = new XmlSyndicationContent ("text/xml", 6, (XmlObjectSerializer) null);
			Assert.AreEqual ("<content type=\"text/xml\" xmlns=\"http://www.w3.org/2005/Atom\"><int xmlns=\"http://schemas.microsoft.com/2003/10/Serialization/\">6</int></content>", t.GetReaderAtContent ().ReadOuterXml (), "#2");
		}
		
		[Test]
		public void GetReaderAtContent2 ()
		{
			var inxml = "<xcontent type=\"text/xhtml\" xmlns=\"XXX-http://www.w3.org/2005/Atom\"><int xmlns=\"http://schemas.microsoft.com/2003/10/Serialization/\">6</int></xcontent>";
			var ms = new MemoryStream ();
			using (var xw = XmlWriter.Create (ms))
				new XmlSyndicationContent (XmlReader.Create (new StringReader (inxml))).WriteTo (xw, "contentsss", "urn:x");
			ms.Position = 0;
			var expected = "<?xml version='1.0' encoding='utf-8'?><contentsss type='text/xml' xmlns='urn:x'><int xmlns='http://schemas.microsoft.com/2003/10/Serialization/'>6</int></contentsss>".Replace ('\'', '"');
			Assert.AreEqual (expected, new StreamReader (ms).ReadToEnd (), "#1");
		}
	}
}
