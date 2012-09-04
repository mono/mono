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
using System.Xml.XPath;

using NUnit.Framework;

namespace MonoTests.System.Xml.Linq
{
	[TestFixture]
	public class XNodeNavigatorTest
	{
		[Test]
		public void MoveToNext ()
		{
			XElement a = new XElement ("root",
			new XElement ("a"),
			new XElement ("B"));
			XPathNavigator nav = a.CreateNavigator ();
			Assert.IsTrue (nav.MoveToFirstChild (), "#1");
			Assert.IsTrue (nav.MoveToNext (), "#2");
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void MoveToId () // ID is not supported here.
		{
			string xml = @"
<!DOCTYPE root [
<!ELEMENT foo EMPTY>
<!ELEMENT bar EMPTY>
<!ATTLIST foo id ID #IMPLIED>
<!ATTLIST bar id ID #IMPLIED>
]>
<root><foo id='foo' /><bar id='bar' /></root>";
			XDocument doc = XDocument.Parse (xml, LoadOptions.SetLineInfo);
			XPathNavigator nav = doc.CreateNavigator ();
			nav.MoveToId ("foo");
		}

		[Test]
		public void Bug594877 ()
		{
			string data = "<rt> <objsur t=\"o\" guid=\"06974d9a-ff86-4e1c-a3e5-7ce8c961dcb9\" /> </rt>";
			XElement xeOldOwner = XElement.Parse(data);
			string xpathOld = String.Format(".//objsur[@t='o']");
			XElement xeOldRef = xeOldOwner.XPathSelectElement(xpathOld);
			Assert.AreEqual ("<objsur t='o' guid='06974d9a-ff86-4e1c-a3e5-7ce8c961dcb9' />".Replace ('\'', '"'), xeOldRef.ToString (), "#1");
		}

		[Test]
		public void Bug4739 ()
		{
			string data = "<root><parent>A<child>B</child>C</parent></root>";
			XElement doc = XElement.Parse (data);
			var iterator = doc.CreateNavigator ().Select ("//parent/child");
			iterator.MoveNext ();
			var element = iterator.Current;
			Assert.AreEqual ("B", element.InnerXml);
		}

		[Test]
		public void MoveToRoot_Bug4690 ()
		{
			string data = "<root><parent><child/></parent></root>";
			XElement doc = XElement.Parse (data);
			var iterator = doc.CreateNavigator ().Select ("//child");
			iterator.MoveNext ();
			var element = iterator.Current;
			element.MoveToRoot ();
			Assert.AreEqual ("root", element.Name);
		}
	}
}
