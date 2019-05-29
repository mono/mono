//
// UrlSyndicationContentTest.cs
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
#if !MOBILE
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
	public class UrlSyndicationContentTest
	{
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorNullUrl ()
		{
			new UrlSyndicationContent (null, "text/plain");
		}

		[Test]
		public void Constructor ()
		{
			Uri uri = new Uri ("http://example.com");
			UrlSyndicationContent t = new UrlSyndicationContent (uri, null);
			t = new UrlSyndicationContent (uri, "text/plain");
			Assert.AreEqual (uri, t.Url, "#1");
			Assert.AreEqual ("text/plain", t.Type, "#2");
		}

		[Test]
		public void Clone ()
		{
			Uri uri = new Uri ("http://example.com");
			UrlSyndicationContent t = new UrlSyndicationContent (uri, "text/plain");
			t = t.Clone () as UrlSyndicationContent;
			Assert.AreEqual (uri, t.Url, "#1");
			Assert.AreEqual ("text/plain", t.Type, "#2");
		}

		[Test]
		[Category("NotWorking")]
		public void WriteTo ()
		{
			Uri uri = new Uri ("http://example.com/");
			UrlSyndicationContent t = new UrlSyndicationContent (uri, null);
			StringWriter sw = new StringWriter ();
			using (XmlWriter w = CreateWriter (sw))
				t.WriteTo (w, "root", String.Empty);
			Assert.AreEqual ("<root type=\"\" src=\"http://example.com/\" />", sw.ToString ());

			t = new UrlSyndicationContent (uri, "application/xml+svg");
			sw = new StringWriter ();
			using (XmlWriter w = CreateWriter (sw))
				t.WriteTo (w, "root", String.Empty);
			Assert.AreEqual ("<root type=\"application/xml+svg\" src=\"http://example.com/\" />", sw.ToString ());
		}

		XmlWriter CreateWriter (StringWriter sw)
		{
			XmlWriterSettings s = new XmlWriterSettings ();
			s.OmitXmlDeclaration = true;
			return XmlWriter.Create (sw, s);
		}
	}
}
#endif