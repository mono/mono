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
	public class XStreamingElementTest
	{
		[Test]
		public void ToString ()
		{
			var el = new XStreamingElement ("foo",
				new XAttribute ("bar", "baz"),
				"text");
			Assert.AreEqual ("<foo bar=\"baz\">text</foo>", el.ToString ());
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void ToStringAttributeAfterText ()
		{
			var el = new XStreamingElement ("foo",
				"text",
				new XAttribute ("bar", "baz"));
			el.ToString ();
		}

		[Test]
		public void WriteXStreamingElementChildren ()
		{
			var xml = "<?xml version='1.0' encoding='utf-8'?><root type='array'><item type='number'>0</item><item type='number'>2</item><item type='number'>5</item></root>".Replace ('\'', '"');
			
			var ms = new MemoryStream ();
			var xw = XmlWriter.Create (ms);
			int [] arr = new int [] {0, 2, 5};
			var xe = new XStreamingElement (XName.Get ("root"));
			xe.Add (new XAttribute (XName.Get ("type"), "array"));
			var at = new XAttribute (XName.Get ("type"), "number");
			foreach (var i in arr)
				xe.Add (new XStreamingElement (XName.Get ("item"), at, i));

			xe.WriteTo (xw);
			xw.Close ();
			Assert.AreEqual (xml, new StreamReader (new MemoryStream (ms.ToArray ())).ReadToEnd (), "#1");
		}
	}
}
