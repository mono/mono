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
	public class XNameTest
	{
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void GetNull ()
		{
			XName.Get (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void GetEmpty ()
		{
			XName.Get (String.Empty);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void GetBrokenFormat ()
		{
			XName.Get ("{");
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void GetBrokenFormat2 ()
		{
			XName.Get ("}");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void GetBrokenFormat3 ()
		{
			XName.Get ("{x_x}");
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void GetBrokenFormat4 ()
		{
			XName.Get (":");
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void GetBrokenFormat5 ()
		{
			XName.Get ("whoa!");
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void GetBrokenFormat6 ()
		{
			XName.Get ("x{y}");
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void GetBrokenFormat7 ()
		{
			XName.Get (" {x}y");
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void GetBrokenFormat8 ()
		{
			XName.Get ("{x}y ");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void GetBrokenFormat9 ()
		{
			XName.Get ("{xyz");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void GetBrokenFormat10 ()
		{
			XName.Get ("{}x");
		}

		[Test]
		public void Get1 ()
		{
			XName n = XName.Get ("{{}}x");
			Assert.AreEqual ("x", n.LocalName, "#1");
			// huh, looks like there is no URI format validation.
			Assert.AreEqual ("{}", n.NamespaceName, "#2");
		}


	}
}
