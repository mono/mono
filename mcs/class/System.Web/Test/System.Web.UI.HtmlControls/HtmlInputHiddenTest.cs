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
//
// System.Web.UI.HtmlControls.HtmlInputHidden.cs
//
// Authors:
//	Jackson Harper (jackson@ximian.com)
//
// (C) 2005 Novell, Inc.


using System;
using System.IO;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Collections.Specialized;

using NUnit.Framework;

namespace MonoTests.System.Web.UI.HtmlControls {

	public class HtmlInputHiddenPoker : HtmlInputHidden {

		public string Render ()
		{
			StringWriter sw = new StringWriter ();
			HtmlTextWriter tw = new HtmlTextWriter (sw);
			base.Render (tw);

			return sw.ToString ();
		}
	}

	[TestFixture]
	public class HtmlInputHiddenTest {

		[Test]
		public void Defaults ()
		{
			HtmlInputHidden h = new HtmlInputHidden ();

			Assert.AreEqual (h.Type, "hidden", "A1");
			Assert.AreEqual (h.Value, String.Empty, "A2");
		}

		[Test]
		public void PropertiesNull ()
		{
			HtmlInputHidden h = new HtmlInputHidden ();

			h.Value = null;
			Assert.AreEqual (h.Value, String.Empty, "A1");
		}

		[Test]
		public void Postback ()
		{
			HtmlInputHidden h = new HtmlInputHidden ();
			IPostBackDataHandler p = (IPostBackDataHandler) h;
			NameValueCollection collection = new NameValueCollection ();
			string key = "key";
			string value = "Hi i am a value";
			
			collection [key] = value;
			p.LoadPostData (key, collection);

			Assert.AreEqual (h.Value, value, "A1");
		}

		[Test]
		public void TestPostbackHandling ()
		{
			HtmlInputHidden h = new HtmlInputHidden ();
			IPostBackDataHandler p = (IPostBackDataHandler) h;
			NameValueCollection collection = new NameValueCollection ();
			string key = "key";
			string value = "Hi i am a value";
			
			collection [key] = value;
			Assert.IsTrue(p.LoadPostData (key, collection));
			Assert.IsFalse (p.LoadPostData (key, collection));
			Assert.AreEqual (h.Value, value);
		}
		
		[Test]
		public void Render ()
		{
			HtmlInputHiddenPoker p = new HtmlInputHiddenPoker ();

			Assert.AreEqual (p.Render (), "<input name type=\"hidden\" />");

			p.Value = "foobar";
			Assert.AreEqual (p.Render (), "<input name type=\"hidden\" " +
					"value=\"foobar\" />");
		}
	}
}

