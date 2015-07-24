//
// Authors:
//   Atsushi Enomoto
//
// Copyright 2014 Xamarin Inc. (http://www.xamarin.com)
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
using System.Linq;

using NUnit.Framework;

namespace MonoTests.System.Xml.Linq
{
	[TestFixture]
	public class XCommentTest
	{
		[Test]
		public void EscapeSequentialDashes ()
		{
			XComment c;

			c = new XComment ("<--foo-->");
			Assert.AreEqual ("<--foo-->", c.Value, "#1");
			// bug #23318
			// Unlike XmlWriter.WriteComment(), XComment.ToString() seems to accept "--" in the value.
			Assert.AreEqual ("<!--<- -foo- ->-->", c.ToString (), "#2");
			// make sure if it can be read...
			XmlReader.Create (new StringReader (c.ToString ())).Read ();

			// The last '-' causes some glitch...
			c = new XComment ("--foo--");
			Assert.AreEqual ("--foo--", c.Value, "#3");
			Assert.AreEqual ("<!--- -foo- - -->", c.ToString (), "#4");
			XmlReader.Create (new StringReader (c.ToString ())).Read ();

			// What if <!-- appears in the value?
			c = new XComment ("<!--foo-->");
			Assert.AreEqual ("<!--foo-->", c.Value, "#5");
			Assert.AreEqual ("<!--<!- -foo- ->-->", c.ToString (), "#6");
			XmlReader.Create (new StringReader (c.ToString ())).Read ();
		}
	}
}
