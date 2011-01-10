//
// HtmlFormTest.cs
//	- Unit tests for System.Web.UI.HtmlControls.HtmlForm
//
// Author:
//	Dick Porter  <dick@ximian.com>
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
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using MonoTests.stand_alone.WebHarness;
using NUnit.Framework;

namespace MonoTests.System.Web.UI.HtmlControls {

	class TestPage : SystemWebTestShim.Page {

		private HttpContext ctx;

		// don't call base class (so _context is never set to a non-null value)
		protected internal override HttpContext Context {
			get {
				if (ctx == null) {
					ctx = new HttpContext (
						new HttpRequest ("default.aspx", "http://mono-project.com/", "q=1&q2=2"),
						new HttpResponse (new StringWriter ())
						);
				}
				return ctx;
			}
		}

#if NET_2_0 && !TARGET_JVM
		public void SetContext ()
		{            
			SetContext (Context);
		}
#endif
	}
	
	public class FormPoker : HtmlForm {
		public FormPoker () {
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

		/* Not called at all when running the current tests (2005/09/29)
		protected override void OnInit (EventArgs e)
		{
			Console.WriteLine (Environment.StackTrace);
			base.OnInit (e);
		}
		*/
		public string RenderChildren ()
		{
			StringWriter sw = new StringWriter();
			HtmlTextWriter w = new HtmlTextWriter (sw);
			
			RenderChildren (w);

			return sw.ToString();
		}

		public string RenderAttributes ()
		{
			StringWriter sw = new StringWriter();
			HtmlTextWriter w = new HtmlTextWriter (sw);
			
			RenderAttributes (w);

			return sw.ToString ();
		}
#if NET_2_0
		public ControlCollection GetControlCollection ()
		{
			return CreateControlCollection();
		}
#endif
	}

	class FUControl : UserControl {
	}


	[TestFixture]
	public class HtmlFormTest {
		[Test]
		public void DefaultProperties ()
		{
			HtmlForm form = new HtmlForm ();
			Assert.AreEqual (0, form.Attributes.Count, "Attributes.Count");

			Assert.AreEqual (String.Empty, form.Enctype, "Enctype");
			Assert.AreEqual ("post", form.Method, "Method");
			Assert.AreEqual (form.UniqueID, form.Name, "Name");
			Assert.AreEqual (String.Empty, form.Target, "Target");

			Assert.AreEqual ("form", form.TagName, "TagName");

#if NET_2_0
			Assert.IsFalse (form.SubmitDisabledControls, "TagName");
#endif
		}

		[Test]
		public void NullProperties ()
		{
			HtmlForm form = new HtmlForm ();

			form.Enctype = null;
			Assert.AreEqual (String.Empty, form.Enctype, "Enctype");
			form.Method = null;
			Assert.AreEqual ("post", form.Method, "Method");
			form.Name = null;
			Assert.AreEqual (form.UniqueID, form.Name, "Name");
			form.Target = null;
			Assert.AreEqual (String.Empty, form.Target, "Target");

#if NET_2_0
			form.DefaultButton = null;
			Assert.AreEqual (String.Empty, form.DefaultButton, "DefaultButton");

			form.DefaultFocus = null;
			Assert.AreEqual (String.Empty, form.DefaultFocus, "DefaultFocus");
#endif
			Assert.AreEqual (0, form.Attributes.Count, "Attributes.Count");

		}

		[Test]
		public void Attributes ()
		{
			HtmlForm form = new HtmlForm ();
			IAttributeAccessor a = (IAttributeAccessor)form;

#if NET_2_0
			/* not stored in Attributes */
			form.DefaultButton = "defaultbutton";
			Assert.IsNull (a.GetAttribute ("defaultbutton"), "A1");

			/* not stored in Attributes */
			form.DefaultFocus = "defaultfocus";
			Assert.IsNull (a.GetAttribute ("defaultfocus"), "A2");
#endif
			form.Enctype = "enctype";
			Assert.AreEqual ("enctype", a.GetAttribute ("enctype"), "A3");

			form.Method = "method";
			Assert.AreEqual ("method", a.GetAttribute ("method"), "A4");

			/* not stored in Attributes */
			form.Name = "name";
			Assert.AreEqual (form.UniqueID, form.Name, "A5");
			Assert.IsNull (form.Name, "A6");
			Assert.IsNull (form.UniqueID, "A7");
			Assert.IsNull (a.GetAttribute ("name"), "A8");
			form.ID = "hithere";
			Assert.AreEqual ("hithere", form.Name, "A9");

#if NET_2_0
			form.SubmitDisabledControls = true;
			Assert.IsNull (a.GetAttribute ("submitdisabledcontrols"), "A10");
#endif

			form.Target = "target";
			Assert.AreEqual ("target", a.GetAttribute ("target"), "A11");
		}

#if NET_2_0
#if !TARGET_DOTNET && !TARGET_JVM
		[Test]
		public void ActionStringWithQuery ()
		{
			TestPage p = new TestPage ();
			p.SetContext ();
			FormPoker form = new FormPoker ();
			form.Page = p;
			string attrs = form.RenderAttributes ();

			// Indirect test for HttpRequest.QueryStringRaw, see
			// https://bugzilla.novell.com/show_bug.cgi?id=376352
			Assert.AreEqual (" method=\"post\" action=\"?q=1&amp;q2=2\"", attrs, "A1");
		}
#endif

		[Test]
		public void Undocumented_ActionProperty ()
		{
			TestPage p = new TestPage ();
			p.SetContext ();
			FormPoker form = new FormPoker ();
			form.Page = p;
			form.Action = "someactionfile.aspx";
			string attrs = form.RenderAttributes ();

			Assert.AreEqual (" method=\"post\" action=\"someactionfile.aspx\"", attrs, "A1");
		}
#endif
		
		[Test]
		public void ViewState ()
		{
			FormPoker form = new FormPoker();
			FormPoker copy = new FormPoker();

#if NET_2_0
			form.DefaultButton = "defaultbutton";
			form.DefaultFocus = "defaultfocus";
#endif

			object state = form.SaveState();
			copy.LoadState (state);

#if NET_2_0
			Assert.AreEqual ("", copy.DefaultButton, "A1");
			Assert.AreEqual ("defaultfocus", form.DefaultFocus, "A2");
#endif

		}

		[Test]
		public void Name_InsideNaming ()
		{
			Control ctrl = new FUControl ();
			ctrl.ID = "parent";
			FormPoker form = new FormPoker ();
			ctrl.Controls.Add (form);
			Assert.IsNull (form.ID, "ID");
			form.Name = "name";
			Assert.AreEqual (form.Name, form.UniqueID, "name and unique id");

			form.ID = "id";
			Assert.AreEqual ("id", form.ID, "ID-2");
			Assert.AreEqual (form.UniqueID, form.Name, "Name-ID");

			form.Name = "name";
			Assert.AreEqual (form.Name, form.UniqueID, "UniqueID-2");

			form.ID = null;
			Assert.IsNull (form.ID, "ID-3");
			Assert.IsNotNull (form.UniqueID, "UniqueID-3");
			Assert.IsNotNull (form.Name, "Name-2");
		}

		[Test]
		[Category ("NotWorking")]
		public void RenderChildren ()
		{
			Page p = new Page();
			FormPoker form = new FormPoker ();
			form.Page = p;
#if NET_2_0
			HtmlDiff.AssertAreEqual ("<div>\r\n<input type=\"hidden\" name=\"__VIEWSTATE\" id=\"\r\n__VIEWSTATE\" value=\"\" />\r\n</div>", form.RenderChildren ().Trim (), "A1");
#else
			HtmlDiff.AssertAreEqual ("<input type=\"hidden\" name=\"__VIEWSTATE\" value=\"\" />", form.RenderChildren ().Trim (), "A1");
#endif
		}

#if NET_2_0
		[Test]
		public void ControlCollection ()
		{
			FormPoker poker = new FormPoker();
			ControlCollection col = poker.GetControlCollection();
			Assert.AreEqual (col.GetType(), typeof (ControlCollection), "A1");
			Assert.IsFalse (col.IsReadOnly, "A2");
			Assert.AreEqual (0, col.Count, "A3");
		}
#endif
	}
}
