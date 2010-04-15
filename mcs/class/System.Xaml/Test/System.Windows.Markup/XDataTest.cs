//
// Copyright (C) 2010 Novell Inc. http://novell.com
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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Markup;
using System.Xaml;
using System.Xaml.Schema;
using System.Xml;
using NUnit.Framework;

using Category = NUnit.Framework.CategoryAttribute;

namespace MonoTests.System.Windows.Markup
{
	[TestFixture]
	public class XDataTest
	{
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void GetXmlReaderWithNullText ()
		{
			var x = new XData ();
			Assert.IsNull (x.Text, "#1");
			Assert.IsNull (x.XmlReader, "#2");
		}

		[Test]
		public void TextSetsXmlReader ()
		{
			var x = new XData ();
			x.Text = "foobar";
			Assert.IsNotNull (x.Text, "#3");
			var r = x.XmlReader as XmlReader;
			Assert.IsNotNull (r, "#4");
			Assert.AreEqual (XmlNodeType.None, r.NodeType, "#5");
			try {
				r.Read (); // invalid xml
				Assert.Fail ("#6");
			} catch (XmlException) {
			}
		}

		[Test]
		public void SetTextNull ()
		{
			var x = new XData ();
			// allowed.
			x.Text = null;
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void SetNonXmlReader ()
		{
			var x = new XData ();
			XmlReader r;
			x.XmlReader = "<foo/>"; // not allowed. It does *not* raise an error, but the value becomes null.
			r = x.XmlReader as XmlReader; // and thus it causes ANE.
		}

		[Test]
		public void SetXmlReader ()
		{
			var x = new XData ();
			x.XmlReader = XmlReader.Create (new StringReader ("<root/>"));
			Assert.IsNull (x.Text, "#1");
			Assert.IsNotNull (x.XmlReader, "#2");
			x.XmlReader = null;
		}
	}
}
