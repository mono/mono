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
using System.IO;
using System.Xml;
using System.Xml.Linq;

using NUnit.Framework;

namespace MonoTests.System.Xml.Linq
{
	[TestFixture]
	public class XNamespaceTest
	{
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void GetNull ()
		{
			XNamespace.Get (null);
		}

		[Test]
		public void GetEmpty ()
		{
			XNamespace n = XNamespace.Get (String.Empty);
			Assert.AreEqual (String.Empty, n.NamespaceName);
		}

		[Test]
		//[ExpectedException (typeof (ArgumentException))]
		public void GetBrokenFormat ()
		{
			XNamespace n = XNamespace.Get ("{");
			Assert.AreEqual ("{", n.NamespaceName, "#1");
		}

		[Test]
		//[ExpectedException (typeof (XmlException))]
		public void GetBrokenFormat2 ()
		{
			XNamespace n = XNamespace.Get ("}");
			Assert.AreEqual ("}", n.NamespaceName, "#1");
		}

		[Test]
		//[ExpectedException (typeof (ArgumentException))]
		public void GetBrokenFormat3 ()
		{
			XNamespace n = XNamespace.Get ("{{}}x");
			Assert.AreEqual ("{{}}x", n.NamespaceName, "#1");
		}

		[Test]
		public void GetBrokenFormat4 ()
		{
			XNamespace n = XNamespace.Get ("{}x}");
			Assert.AreEqual ("{}x}", n.NamespaceName, "#1");
		}

		[Test]
		public void Get1 ()
		{
			XNamespace n = XNamespace.Get ("{x_x}");
			Assert.AreEqual ("{x_x}", n.NamespaceName, "#1");

			n = XNamespace.Get ("x_x"); // looks like this is the ordinal use.
			Assert.AreEqual ("x_x", n.NamespaceName, "#2");
		}

		[Test]
		public void Get2 ()
		{
			XNamespace n = XNamespace.Get (String.Empty);
			Assert.IsTrue (Object.ReferenceEquals (XNamespace.None, n), "#1");
			n = XNamespace.Get ("http://www.w3.org/2000/xmlns/");
			Assert.IsTrue (Object.ReferenceEquals (XNamespace.Xmlns, n), "#2");
			Assert.IsTrue (Object.ReferenceEquals (XNamespace.Get ("urn:foo"), XNamespace.Get ("urn:foo")), "#3");
		}

		[Test]
		public void GetName ()
		{
			XNamespace n = XNamespace.Get ("urn:foo");
			Assert.IsTrue (Object.ReferenceEquals (n.GetName ("foo"), n.GetName ("foo")), "#1");
			Assert.IsTrue (n.GetName ("foo") == n.GetName ("foo"), "#2");
			Assert.IsFalse (n.GetName ("foo") == n.GetName ("bar"), "#3");
		}

		[Test]
		public void Predefined ()
		{
			Assert.AreEqual ("http://www.w3.org/XML/1998/namespace", XNamespace.Xml.NamespaceName, "#1");
			Assert.AreEqual ("http://www.w3.org/2000/xmlns/", XNamespace.Xmlns.NamespaceName, "#2");
		}

		[Test]
		public void Addition ()
		{
			XNamespace ns = "http://www.novell.com";
			XName d = ns + "hello";

			Assert.AreEqual ("hello", d.LocalName, "localname");
			Assert.AreEqual (ns, d.Namespace, "namespace");
			Assert.AreEqual ("http://www.novell.com", d.NamespaceName, "nsname");
		}
			
		[Test]
		public void Equals ()
		{
			Assert.IsTrue (XNamespace.None.Equals (XNamespace.Get ("")), "#1");
			Assert.IsTrue (XNamespace.None == XNamespace.Get (""), "#2");
			Assert.IsFalse (XNamespace.None.Equals (XNamespace.Get (" ")), "#3");
			Assert.IsFalse (XNamespace.None == XNamespace.Get (" "), "#4");
		}

		[Test]
		public void TestXmlNoNs ()
		{
			var ns = XNamespace.Get ("urn:foo");
			var element = new XElement ("Demo", new XAttribute (ns + "nil", true));
			Assert.AreEqual ("<Demo p1:nil=\"true\" xmlns:p1=\"urn:foo\" />", element.ToString ());
		}

		[Test]
		public void TestXmlWithNs ()
		{
			var ns = XNamespace.Get ("urn:foo");
			var element = new XElement ("Demo", new XAttribute (ns + "nil", true), new XAttribute (XNamespace.Xmlns + "xsi", ns));
			Assert.AreEqual ("<Demo xsi:nil=\"true\" xmlns:xsi=\"urn:foo\" />", element.ToString ());
		}
	}
}
