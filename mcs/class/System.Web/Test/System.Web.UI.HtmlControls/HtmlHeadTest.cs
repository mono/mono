//
// Author:
//      Marek Habersack <grendel@twistedcode.net>
//
// Copyright (C) 2011 Novell, Inc (http://novell.com)
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
using System.Web;
using System.Web.Compilation;
using System.Threading;

using NUnit.Framework;
using MonoTests.SystemWeb.Framework;
using MonoTests.stand_alone.WebHarness;

namespace MonoTests.System.Web.UI.HtmlControls
{
        [TestFixture]
	public class HtmlHeadTest
	{
		[TestFixtureSetUp]
		public void SetUp ()
		{
			WebTest.CopyResource (this.GetType (), "HtmlTitleCodeRender_Bug662918.aspx", "HtmlTitleCodeRender_Bug662918.aspx");
		}
		
		[Test (Description="Bug #662918")]
		public void HtmlTitleCodeRender_Bug662918 ()
		{
			string origHtml = "<head><title>\r\n\tTitle text\r\n</title></head>";
			string pageHtml = new WebTest ("HtmlTitleCodeRender_Bug662918.aspx").Run ();
			string renderedHtml = HtmlDiff.GetControlFromPageHtml (pageHtml);

			HtmlDiff.AssertAreEqual (origHtml, renderedHtml, "#A1");
		}
	}
}
