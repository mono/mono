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
/* It does not compile probably due to bug #359733.
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
		public void MoveToId () // Not supported
		{
			string xml = @"
<!DOCTYPE root [
<!ELEMENT foo EMPTY>
<!ELEMENT bar EMPTY>
<!ATTLIST foo id ID #IMPLIED>
<!ATTLIST bar id ID #IMPLIED>
]>
<root><foo id='foo' /><bar id='bar' /></root>",
			XDocument doc = XDocument.Parse (xml, LoadOptions.SetLineInfo);
			XPathNavigator nav = doc.CreateNavigator ();
			nav.MoveToId ("foo");
		}
*/
	}
}
