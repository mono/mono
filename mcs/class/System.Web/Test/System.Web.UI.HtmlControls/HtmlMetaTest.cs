//
// HtmlMetaTest.cs - unit tests for System.Web.UI.HtmlControls.HtmlMeta
//
// Author:
//	Chris Toshok <toshok@ximian.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

#if NET_2_0

using NUnit.Framework;
using MonoTests.stand_alone.WebHarness;
using System;
using System.IO;
using System.Web.UI;
using System.Web.UI.HtmlControls;

namespace MonoTests.System.Web.UI.HtmlControls {

	public class HtmlMetaPoker : HtmlMeta {
		public string Render ()
		{
			HtmlTextWriter writer = new HtmlTextWriter (new StringWriter ());
			base.Render (writer);
			return writer.InnerWriter.ToString ();
		}
	}

	[TestFixture]
	public class HtmlMetaTest {
		
		[Test]
		public void Defaults ()
		{
			HtmlMetaPoker meta = new HtmlMetaPoker ();

			Assert.AreEqual ("", meta.Content, "A1");
			Assert.AreEqual ("", meta.HttpEquiv, "A2");
			Assert.AreEqual ("", meta.Name, "A3");
			Assert.AreEqual ("", meta.Scheme, "A4");
			Assert.AreEqual (0, meta.Attributes.Count, "A5");
		}

		[Test]
		public void Setters ()
		{
			HtmlMetaPoker meta = new HtmlMetaPoker ();

			meta.Name = "Name";
			meta.Content = "Content";
			meta.HttpEquiv = "Equiv";
			meta.Scheme = "Scheme";

			Assert.AreEqual ("Name", meta.Name, "A1");
			Assert.AreEqual ("Content", meta.Content, "A2");
			Assert.AreEqual ("Equiv", meta.HttpEquiv, "A3");
			Assert.AreEqual ("Scheme", meta.Scheme, "A4");

			Assert.AreEqual (4, meta.Attributes.Count, "A2");
		}

		[Test]
		public void Render ()
		{
			HtmlMetaPoker meta = new HtmlMetaPoker ();
			meta.Name = "Name";
			meta.Content = "Content";
			meta.HttpEquiv = "Equiv";
			meta.Scheme = "Scheme";
			HtmlDiff.AssertAreEqual ("<meta name=\"Name\" content=\"Content\" http-equiv=\"Equiv\" scheme=\"Scheme\" />", meta.Render(), "A1");
		}

		[Test]
		[Category ("NotWorking")]
		public void Render_Empty ()
		{
			HtmlMetaPoker meta = new HtmlMetaPoker ();
			Assert.AreEqual ("<meta />", meta.Render(), "A1");
		}
	}
}

#endif
