//
// (C) 2005 Mainsoft Corporation (http://www.mainsoft.com)
//
// Authors:
//	Andrew Skiba <andrews@mainsoft.com>
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
using System;
using NUnit.Framework;
using System.Threading;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;

namespace MonoTests.System.Web.UI.WebControls
{
	[TestFixture]
	public class FileUploadTest
	{
		class PokerFileUpload : FileUpload
		{
			public void DoRender (HtmlTextWriter writer)
			{
				base.Render (writer);
			}
		}

		[Test]
		public void RenderWithoutPageTest ()
		{
			PokerFileUpload fu = new PokerFileUpload ();
			using (StringWriter sw = new StringWriter ()) {
				using (HtmlTextWriter htw = new HtmlTextWriter (sw)) {
					fu.DoRender (htw);
				}
				sw.Close ();
				string res = sw.ToString ();
				Assert.AreEqual ("<input type=\"file\" />", res);
			}
		}

		[Test]
		public void RenderWithPageTest ()
		{
			PokerFileUpload fu = new PokerFileUpload ();
			fu.Page = new Page ();
			fu.Page.Controls.Add (fu);
			
			using (StringWriter sw = new StringWriter ()) {
				using (HtmlTextWriter htw = new HtmlTextWriter (sw)) {
					fu.DoRender (htw);
				}
				sw.Close ();
			}
		}

		[Test]
		public void RenderControlTest ()
		{
			FileUpload fu = new FileUpload ();
			fu.Page = new Page ();
			string res;
			using (StringWriter sw = new StringWriter ()) {
				using (HtmlTextWriter htw = new HtmlTextWriter (sw)) {
					fu.RenderControl (htw);
				}
				sw.Close ();
				res = sw.ToString ();
			}
			Assert.AreEqual ("<input type=\"file\" />", res);
		}
		[Test]
		public void RenderBeginTagTest ()
		{
			FileUpload fu = new FileUpload ();
			fu.Page = new Page ();
			string res;
			using (StringWriter sw = new StringWriter ()) {
				using (HtmlTextWriter htw = new HtmlTextWriter (sw)) {
					fu.RenderBeginTag (htw);
				}
				sw.Close ();
				res = sw.ToString ();
			}
			Assert.AreEqual ("<input type=\"file\" />", res);
		}
		[Test]
		public void FileContentTest ()
		{
			FileUpload fu = new FileUpload ();
			Stream s = fu.FileContent;
			Assert.AreSame (s, Stream.Null);
		}

	}
}
#endif
