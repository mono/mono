//
// TextSyndicationContentTest.cs
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
	public class TextSyndicationContentTest
	{
		[Test]
		public void Constructor ()
		{
			TextSyndicationContent t = new TextSyndicationContent (null); // hmm, null is allowed...
			Assert.IsNull (t.Text, "#0");

			t = new TextSyndicationContent ("test");
			Assert.AreEqual ("test", t.Text, "#1");
			Assert.AreEqual ("text", t.Type, "#2");

			t = new TextSyndicationContent ("test", TextSyndicationContentKind.Html);
			Assert.AreEqual ("html", t.Type, "#3");

			t = new TextSyndicationContent ("test", TextSyndicationContentKind.XHtml);
			Assert.AreEqual ("xhtml", t.Type, "#4");
		}

		[Test]
		public void Clone ()
		{
			TextSyndicationContent t = new TextSyndicationContent ("test");
			t = t.Clone () as TextSyndicationContent;
			Assert.AreEqual ("test", t.Text, "#1");
			Assert.AreEqual ("text", t.Type, "#2");

			t = new TextSyndicationContent ("test", TextSyndicationContentKind.Html);
			t = t.Clone () as TextSyndicationContent;
			Assert.AreEqual ("html", t.Type, "#3");
		}

		[Test]
		public void WriteTo ()
		{
			TextSyndicationContent t = new TextSyndicationContent (null);
			StringWriter sw = new StringWriter ();
			using (XmlWriter w = CreateWriter (sw))
				t.WriteTo (w, "root", String.Empty);
			Assert.AreEqual ("<root type=\"text\"></root>", sw.ToString ());

			t = new TextSyndicationContent ("broken<b>html", TextSyndicationContentKind.Html);
			sw = new StringWriter ();
			using (XmlWriter w = CreateWriter (sw))
				t.WriteTo (w, "root", String.Empty);
			Assert.AreEqual ("<root type=\"html\">broken&lt;b&gt;html</root>", sw.ToString ());
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