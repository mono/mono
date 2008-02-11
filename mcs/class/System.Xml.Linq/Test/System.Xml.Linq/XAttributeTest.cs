//
// Authors:
//   Atsushi Enomoto
//
// Copyright 2007 Novell (http://www.novell.com)
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
using System.Xml;
using System.Xml.Linq;

using NUnit.Framework;

namespace MonoTests.System.Xml.Linq
{
	[TestFixture]
	public class XAttributeTest
	{

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorNameNull ()
		{
			XAttribute a = new XAttribute (null, "v");
			
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorValueNull ()
		{
			XAttribute a = new XAttribute (XName.Get ("a"), null);
			
		}

		[Test]
		public void IsNamespaceDeclaration ()
		{
			string xml = "<root a='v' xmlns='urn:foo' xmlns:x='urn:x' x:a='v' xmlns:xml='http://www.w3.org/XML/1998/namespace' />";
			XElement el = XElement.Parse (xml);
			List<XAttribute> l = new List<XAttribute> (el.Attributes ());
			Assert.IsFalse (l [0].IsNamespaceDeclaration, "#1");
			Assert.IsTrue (l [1].IsNamespaceDeclaration, "#2");
			Assert.IsTrue (l [2].IsNamespaceDeclaration, "#3");
			Assert.IsFalse (l [3].IsNamespaceDeclaration, "#4");
			Assert.IsTrue (l [4].IsNamespaceDeclaration, "#5");

			Assert.AreEqual ("a", l [0].Name.LocalName, "#2-1");
			Assert.AreEqual ("xmlns", l [1].Name.LocalName, "#2-2");
			Assert.AreEqual ("x", l [2].Name.LocalName, "#2-3");
			Assert.AreEqual ("a", l [3].Name.LocalName, "#2-4");
			Assert.AreEqual ("xml", l [4].Name.LocalName, "#2-5");

			Assert.AreEqual ("", l [0].Name.NamespaceName, "#3-1");
			// not sure how current Orcas behavior makes sense here though ...
			Assert.AreEqual ("", l [1].Name.NamespaceName, "#3-2");
			Assert.AreEqual ("http://www.w3.org/2000/xmlns/", l [2].Name.NamespaceName, "#3-3");
			Assert.AreEqual ("urn:x", l [3].Name.NamespaceName, "#3-4");
			Assert.AreEqual ("http://www.w3.org/2000/xmlns/", l [4].Name.NamespaceName, "#3-5");
		}

		[Test]
		public void Document ()
		{
			XDocument doc = XDocument.Parse ("<root a='v' />");
			Assert.AreEqual (doc, doc.Root.Document, "#1");
			foreach (XAttribute a in doc.Root.Attributes ())
				Assert.AreEqual (doc, a.Document, "#2");
			Assert.AreEqual (doc, doc.Document, "#3");
		}

		[Test]
		public void SetValue ()
		{
			XAttribute a = new XAttribute (XName.Get ("a"), "v");
			a.SetValue (new XDeclaration ("1.0", null, null));
			// value object is converted to a string.
			Assert.AreEqual ("<?xml version=\"1.0\"?>", a.Value, "#1");
		}

		[Test]
		public void ToString ()
		{
			XAttribute a = new XAttribute (XName.Get ("a"), "v");
			Assert.AreEqual ("a=\"v\"", a.ToString ());

			a = new XAttribute (XName.Get ("a"), " >_< ");
			Assert.AreEqual ("a=\" &gt;_&lt; \"", a.ToString ());
		}

		[Test]
		public void NullCasts ()
		{
			XAttribute a = null;

			Assert.AreEqual (null, (bool?) a, "bool?");
			Assert.AreEqual (null, (DateTime?) a, "DateTime?");
			Assert.AreEqual (null, (decimal?) a, "decimal?");
			Assert.AreEqual (null, (double?) a, "double?");
			Assert.AreEqual (null, (float?) a, "float?");
			Assert.AreEqual (null, (Guid?) a, "Guid?");
			Assert.AreEqual (null, (int?) a, "int?");
			Assert.AreEqual (null, (long?) a, "long?");
			Assert.AreEqual (null, (uint?) a, "uint?");
			Assert.AreEqual (null, (ulong?) a, "ulong?");
			Assert.AreEqual (null, (TimeSpan?) a, "TimeSpan?");
			Assert.AreEqual (null, (string) a, "string");
		}
	}
}
