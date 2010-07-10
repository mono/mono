//
// HtmlInputButtonTest.cs
//	- Unit tests for System.Web.UI.HtmlControls.HtmlInputButton
//
// Author:
//	Jackson Harper	(jackson@ximian.com)
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

using System;
using System.IO;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using MonoTests.stand_alone.WebHarness;
using NUnit.Framework;

namespace MonoTests.System.Web.UI.HtmlControls {

	public class HtmlInputButtonPoker : HtmlInputButton {

		public HtmlInputButtonPoker ()
		{
			TrackViewState ();
		}

		public HtmlInputButtonPoker (string type) : base (type)
		{
			TrackViewState ();
		}

		public object SaveState ()
		{
			return SaveViewState ();
		}

		public void LoadState (object state)
		{
			LoadViewState (state);
		}

		public void DoRenderAttributes (HtmlTextWriter writer)
		{
			RenderAttributes (writer);
		}

		public string RenderToString ()
		{
			StringWriter sr = new StringWriter ();
			RenderAttributes (new HtmlTextWriter (sr));
			return sr.ToString ();
		}
	}

	[TestFixture]
	public class HtmlInputButtonTest {

		[Test]
		public void Defaults ()
		{
			HtmlInputButtonPoker p = new HtmlInputButtonPoker ();

			Assert.IsTrue (p.CausesValidation, "A1");
		}

		[Test]
		public void CleanProperties ()
		{
			HtmlInputButtonPoker p = new HtmlInputButtonPoker ();

			p.CausesValidation = false;
			Assert.IsFalse (p.CausesValidation, "A1");

			p.CausesValidation = true;
			Assert.IsTrue (p.CausesValidation, "A2");

			p.CausesValidation = false;
			Assert.IsFalse (p.CausesValidation, "A3");
		}

		[Test]
		public void ViewState ()
		{
			HtmlInputButtonPoker p = new HtmlInputButtonPoker ();
#if NET_2_0
			p.CausesValidation = false;
			p.ValidationGroup = "VG";
#endif
			object s = p.SaveState();
			HtmlInputButtonPoker copy = new HtmlInputButtonPoker ();
			copy.LoadState (s);

#if NET_2_0
			Assert.IsFalse (copy.CausesValidation, "A1");
			Assert.AreEqual ("VG", p.ValidationGroup, "A2");
#endif
		}

		[Test]
		public void RenderAttributes ()
		{
			StringWriter sw = new StringWriter ();
			HtmlTextWriter tw = new HtmlTextWriter (sw);

			HtmlInputButtonPoker p = new HtmlInputButtonPoker ();
			
			p.Page = new Page ();

			p.CausesValidation = false;
#if NET_2_0
			p.ValidationGroup = "VG";

			Assert.AreEqual (3, p.Attributes.Count, "A1");
#else
			Assert.AreEqual (2, p.Attributes.Count, "A1");
#endif

			tw.WriteBeginTag ("dummy");
			p.DoRenderAttributes (tw);
			tw.Write ('>');
#if NET_2_0
			HtmlDiff.AssertAreEqual ("<dummy name type=\"button\" ValidationGroup=\"VG\" />", sw.ToString (), "A2");
#else
			HtmlDiff.AssertAreEqual ("<dummy name type=\"button\" />", sw.ToString (), "A2");
#endif
		}

		[Test]
		public void OnClickAttribute ()
		{
			StringWriter sw = new StringWriter ();
			HtmlTextWriter tw = new HtmlTextWriter (sw);

			HtmlInputButtonPoker p = new HtmlInputButtonPoker ();
			p.Page = new Page ();
			p.DoRenderAttributes (tw);
			string str = sw.ToString ();
			int found = str.IndexOf ("onclick");
			Assert.AreEqual (-1, found, "#01");
			p.ServerClick += new EventHandler (EmptyHandler);
			sw = new StringWriter ();
			tw = new HtmlTextWriter (sw);
			p.DoRenderAttributes (tw);
			str = sw.ToString ();
			found = str.IndexOf ("onclick");
			Assert.IsTrue (found >= 0, "#02");
		}

		[Test]
		public void OnClickAttributeWithSpecials ()
		{
#if NET_4_0
			string origHtml = "alert(&#39;&lt;&amp;&#39;);";
			string origHtml2 = "alert('<&');";
#else
			string origHtml = "alert('&lt;&amp;');";
			string origHtml2 = "alert('<&');";
#endif

			StringWriter sw = new StringWriter ();
			HtmlTextWriter tw = new HtmlTextWriter (sw);

			HtmlInputButtonPoker p = new HtmlInputButtonPoker ();
			p.Page = new Page ();
			p.Attributes["onclick"] = "alert('<&');";
			p.DoRenderAttributes (tw);
			string str = sw.ToString ();
			int found = str.IndexOf (origHtml);
			Assert.IsTrue (found >= 0, "#01");
			p.ServerClick += new EventHandler (EmptyHandler);
			sw = new StringWriter ();
			tw = new HtmlTextWriter (sw);
			p.DoRenderAttributes (tw);
			str = sw.ToString ();
			found = str.IndexOf (origHtml2);
			Assert.IsTrue (found >= 0, "#02" + str);
		}

		private static void EmptyHandler (object sender, EventArgs e)
		{
		}

		[Test]
		public void RenderOnclick1 ()
		{
			HtmlInputButtonPoker it = new HtmlInputButtonPoker ("button");
			it.ID = "id1";
			it.ServerClick += new EventHandler (EmptyHandler);
			string rendered = it.RenderToString ();
			Assert.IsTrue (rendered.IndexOf ("onclick") == -1, "#01");
		}

		[Test]
		public void RenderOnclick2 ()
		{
			Page page = new Page ();
#if NET_2_0
			page.EnableEventValidation = false;
#endif
			HtmlInputButtonPoker it = new HtmlInputButtonPoker ("button");
			page.Controls.Add (it);
			it.ID = "id1";
			it.ServerClick += new EventHandler (EmptyHandler);
			string rendered = it.RenderToString ();
			Assert.IsTrue (rendered.IndexOf ("onclick") != -1, "#01");
		}

		[Test]
		public void RenderOnclick3 ()
		{
			HtmlInputButtonPoker it = new HtmlInputButtonPoker ("submit");
			it.ID = "id1";
			it.ServerClick += new EventHandler (EmptyHandler);
			string rendered = it.RenderToString ();
			Assert.IsTrue (rendered.IndexOf ("onclick") == -1, "#01");
		}

		[Test]
		[Category ("NotWorking")]
		public void RenderOnclick4 ()
		{
			Page page = new Page ();
#if NET_2_0
			page.EnableEventValidation = false;
#endif
			HtmlInputButtonPoker it = new HtmlInputButtonPoker ("submit");
			page.Controls.Add (it);
			it.ID = "id1";
			it.ServerClick += new EventHandler (EmptyHandler);
			string rendered = it.RenderToString ();
			Assert.IsTrue (rendered.IndexOf ("onclick") != -1, "#01");
			Assert.IsTrue (rendered.IndexOf ("__doPostBack") != -1, "#02");
			Assert.IsTrue (rendered.IndexOf ("type=\"submit\"") != -1, "#03");
		}

		[Test]
		public void RenderOnclick5 ()
		{
			Page page = new Page ();
#if NET_2_0
			page.EnableEventValidation = false;
#endif
			RequiredFieldValidator val = new RequiredFieldValidator ();
			val.ControlToValidate = "id1";
			page.Validators.Add (val);
			HtmlInputButtonPoker it = new HtmlInputButtonPoker ("submit");
			page.Controls.Add (it);
			it.ID = "id1";
			it.ServerClick += new EventHandler (EmptyHandler);
			string rendered = it.RenderToString ();
			Assert.IsTrue (rendered.IndexOf ("onclick") != -1, "#01");
		}
	}	
}

