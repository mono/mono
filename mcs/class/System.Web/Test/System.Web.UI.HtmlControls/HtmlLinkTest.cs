//
// HtmlLinkTest.cs - unit tests for System.Web.UI.HtmlControls.HtmlLink
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

using System;
using System.IO;
using System.Web.UI;
using System.Web.UI.HtmlControls;

namespace MonoTests.System.Web.UI.HtmlControls {

	public class HtmlLinkPoker : HtmlLink {
		public string Render ()
		{
			HtmlTextWriter writer = new HtmlTextWriter (new StringWriter ());
			base.Render (writer);
			return writer.InnerWriter.ToString ();
		}
	}

	[TestFixture]
	public class HtmlLinkTest {
		
		[Test]
		public void Defaults ()
		{
			HtmlLinkPoker link = new HtmlLinkPoker ();

			Assert.AreEqual ("", link.Href, "A1");
			Assert.AreEqual (0, link.Attributes.Count, "A2");
		}

		[Test]
		public void Setters ()
		{
			HtmlLinkPoker link = new HtmlLinkPoker ();
			link.Href = "http://www.ximian.com/";
			Assert.AreEqual ("http://www.ximian.com/", link.Href, "A1");
			Assert.AreEqual (1, link.Attributes.Count, "A2");
		}

		[Test]
		public void Render ()
		{
			HtmlLinkPoker link = new HtmlLinkPoker ();
			link.Href = "http://www.ximian.com/";
			Assert.AreEqual ("<link href=\"http://www.ximian.com/\" />", link.Render(), "A1");
		}

		[Test]
		public void Render_EmptyHref ()
		{
			HtmlLinkPoker link = new HtmlLinkPoker ();
			Assert.AreEqual ("<link />", link.Render(), "A1");
		}
	}
}

#endif
