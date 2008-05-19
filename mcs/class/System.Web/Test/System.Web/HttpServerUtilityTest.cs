//
// System.Web.HttpServerUtilityTest.cs 
//	- Unit tests for System.Web.HttpServerUtility
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
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

using System.Text;
using System.Web;

using NUnit.Framework;

namespace MonoTests.System.Web {

	[TestFixture]
	public class HttpServerUtilityTest {

		private HttpApplication _app;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			_app = new HttpApplication ();
		}

		public HttpServerUtility Server {
			get { return _app.Server; }
		}

		[Test]
		public void HtmlEncode_LtGt ()
		{
			Assert.AreEqual ("&lt;script&gt;", Server.HtmlEncode ("<script>"));
		}

		// Notes:
		// * this is to avoid a regression that would cause Mono to 
		//   fail item #3 of the XSS vulnerabilities listed at:
		//   http://it-project.ru/andir/docs/aspxvuln/aspxvuln.en.xml
		//   we didn't fall the first time so let's ensure we never will
		// * The author notes that Microsoft has decided not to fix 
		//   this issue (hence the NotDotNet category).

		[Test]
		[Category ("NotDotNet")]
#if TARGET_JVM
		[Ignore ("TD #6954")]
#endif
		public void HtmlEncode_XSS ()
		{
			string problem = "\xff1cscript\xff1e";  // unicode looks alike <script>
			byte[] utf8data = Encoding.UTF8.GetBytes (problem);
			Encoding win1251 = Encoding.GetEncoding ("windows-1251");
			byte[] windata = Encoding.Convert (Encoding.UTF8, win1251, utf8data);
			// now it's a real problem
			Assert.AreEqual ("<script>", Encoding.ASCII.GetString (windata), "<script>");

			string encoded = Server.HtmlEncode (problem);
			Assert.AreEqual ("&#65308;script&#65310;", encoded, "&#65308;script&#65310;");
			
			utf8data = Encoding.UTF8.GetBytes (encoded);
			windata = Encoding.Convert (Encoding.UTF8, win1251, utf8data);
			Assert.AreEqual ("&#65308;script&#65310;", Encoding.ASCII.GetString (windata), "ok");
		}

		[Test]
		public void UrlPathEncode2()
		{
			string s = "default.xxx?sdsd=sds";
			string s2 = Server.UrlPathEncode(s);
			Assert.AreEqual(s, s2, "UrlPathEncode " + s);
		}		

	}
}
