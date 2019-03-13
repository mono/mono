//
// HtmlTextAreaTest.cs
//	- Unit tests for System.Web.UI.HtmlControls.HtmlTextArea
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

using System;
using System.Collections.Specialized;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using MonoTests.stand_alone.WebHarness;
using NUnit.Framework;

namespace MonoTests.System.Web.UI.HtmlControls {

	public class TestHtmlTextArea : HtmlTextArea {

		public StateBag StateBag {
			get { return base.ViewState; }
		}

		public string RenderAttributes ()
		{
			HtmlTextWriter writer = new HtmlTextWriter (new StringWriter ());
			writer.Write ("<dummy");
			base.RenderAttributes (writer);
			writer.Write (" />");
			return writer.InnerWriter.ToString ();
		}

		public string Render ()
		{
			HtmlTextWriter writer = new HtmlTextWriter (new StringWriter ());
			base.Render (writer);
			return writer.InnerWriter.ToString ();
		}

		public void PublicAddParsedSubObject (object o)
		{
			base.AddParsedSubObject (o);
		}
		public bool LoadPost (string key, NameValueCollection nvc)
		{
			return base.LoadPostData (key, nvc);
		}

		public void Raise ()
		{
			base.RaisePostDataChangedEvent ();
		}
	}

	[TestFixture]
	public class HtmlTextAreaTest {

		[Test]
		public void DefaultProperties ()
		{
			TestHtmlTextArea ta = new TestHtmlTextArea ();
			Assert.AreEqual (0, ta.Attributes.Count, "Attributes.Count");
			Assert.AreEqual (0, ta.StateBag.Count, "StateBag.Count");

			Assert.AreEqual (-1, ta.Cols, "Cols");
			Assert.IsNull (ta.Name, "Name");
			Assert.AreEqual (-1, ta.Rows, "Rows");
			Assert.AreEqual (String.Empty, ta.Value, "Value");

			Assert.AreEqual ("textarea", ta.TagName, "TagName");
			Assert.AreEqual (0, ta.Attributes.Count, "Attributes.Count-2");
			Assert.AreEqual (0, ta.StateBag.Count, "StateBag.Count-2");
		}

		[Test]
		public void NullProperties ()
		{
			TestHtmlTextArea ta = new TestHtmlTextArea ();
			ta.Cols = -1;
			Assert.AreEqual (-1, ta.Cols, "Cols");
			ta.Name = null;
			Assert.IsNull (ta.Name, "Name");
			ta.Rows = -1;
			Assert.AreEqual (-1, ta.Rows, "Rows");
			ta.Value = null;
			Assert.AreEqual (String.Empty, ta.Value, "Value");

			Assert.AreEqual (0, ta.Attributes.Count, "Attributes.Count");
			Assert.AreEqual (0, ta.StateBag.Count, "StateBag.Count");
		}

		[Test]
		public void CleanProperties ()
		{
			TestHtmlTextArea ta = new TestHtmlTextArea ();
			ta.Cols = 1;
			Assert.AreEqual (1, ta.Cols, "Cols");
			ta.Name = "name";
			Assert.IsNull (ta.Name, "Name");
			ta.Rows = 2;
			Assert.AreEqual (2, ta.Rows, "Rows");
			ta.Value = "value";
			Assert.AreEqual ("value", ta.Value, "Value");
			Assert.AreEqual (3, ta.Attributes.Count, "3");
			Assert.AreEqual (3, ta.StateBag.Count, "StateBag.Count=3");

			ta.Cols = -1;
			Assert.AreEqual (-1, ta.Cols, "-Cols");
			ta.Name = null;
			Assert.IsNull (ta.Name, "-Name");
			ta.Rows = -1;
			Assert.AreEqual (-1, ta.Rows, "Rows");
			ta.Value = null;
			Assert.AreEqual (String.Empty, ta.Value, "-Value");
			Assert.AreEqual (0, ta.Attributes.Count, "0");
			Assert.AreEqual (0, ta.StateBag.Count, "StateBag.Count=0");
		}

		[Test]
		public void Name ()
		{
			HtmlTextArea ta = new HtmlTextArea ();
			Assert.IsNull (ta.ID, "ID");
			ta.Name = "name";
			Assert.IsNull (ta.Name, "Name");

			ta.ID = "id";
			Assert.AreEqual ("id", ta.ID, "ID-2");
			Assert.AreEqual ("id", ta.Name, "Name-ID");

			ta.Name = "name";
			Assert.AreEqual ("id", ta.Name, "Name-ID-2");

			ta.ID = null;
			Assert.IsNull (ta.ID, "ID-3");
			Assert.IsNull (ta.Name, "Name-2");
		}

		[Test]
		public void Value ()
		{
			TestHtmlTextArea ta = new TestHtmlTextArea ();
			Assert.AreEqual (0, ta.Attributes.Count, "0");
			Assert.AreEqual (0, ta.StateBag.Count, "StateBag.Count=0");

			ta.Value = "value";
			Assert.AreEqual ("value", ta.Value, "Value");
			Assert.AreEqual (1, ta.Attributes.Count, "1");
			Assert.AreEqual (1, ta.StateBag.Count, "StateBag.Count=1");

			// however it's not in attributes
			Assert.IsNull (ta.Attributes["value"], "Attributes");
			// but in InnerText and InnerHtml
			Assert.AreEqual ("value", ta.InnerText, "InnerText");
			Assert.AreEqual ("value", ta.InnerHtml, "InnerHtml");
			// the later is kept in the attributes
			Assert.IsNull (ta.Attributes["innertext"], "Attributes-InnerText");
			Assert.AreEqual ("value", ta.Attributes["innerhtml"], "Attributes-InnerHtml");
		}

		[Test]
		public void RenderAttributes ()
		{
			TestHtmlTextArea ta = new TestHtmlTextArea ();
			ta.Cols = 4;
			ta.Rows = 2;
			ta.Name = "mono";
			ta.Value = "value";
			// value is out
			HtmlDiff.AssertAreEqual ("<dummy name cols=\"4\" rows=\"2\" />", ta.RenderAttributes (), "RenderAttributes failed #1");

			ta.ID = "go";
			HtmlDiff.AssertAreEqual ("<dummy name=\"go\" id=\"go\" cols=\"4\" rows=\"2\" />", ta.RenderAttributes (), "RenderAttributes failed #2");
		}

		[Test]
		[Category ("NotDotNet")] // Implementation details changes : Control name will diffrent.
		public void RenderName1 ()
		{
			UserControl ctrl = new UserControl ();
			ctrl.ID = "UC";
			Page page = new Page ();
			page.EnableEventValidation = false;
			TestHtmlTextArea ta = new TestHtmlTextArea ();
			page.Controls.Add (ctrl);
			ctrl.Controls.Add (ta);
			ta.Name = "mono";
			ta.ID = "go";
			string expected = "<dummy name=\"UC$go\" id=\"UC_go\" />";
			Assert.AreEqual (expected, ta.RenderAttributes ());
		}

		[Test]
		public void Render ()
		{
			TestHtmlTextArea ta = new TestHtmlTextArea ();
			ta.Cols = 4;
			ta.Rows = 2;
			ta.Name = "mono";
			ta.Value = "value";
			// value is out
			HtmlDiff.AssertAreEqual ("<textarea name cols=\"4\" rows=\"2\">value</textarea>", ta.Render (),"Render #1");

			ta.ID = "go";
			HtmlDiff.AssertAreEqual ("<textarea name=\"go\" id=\"go\" cols=\"4\" rows=\"2\">value</textarea>", ta.Render (),"Render #2");
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		[NUnit.Framework.Category ("NotWorking")] // Mono throw HttpException
		public void AddParsedSubObject_Null ()
		{
			TestHtmlTextArea ta = new TestHtmlTextArea ();
			ta.PublicAddParsedSubObject (null);
		}

		[Test]
		[ExpectedException (typeof (HttpException))]
		public void AddParsedSubObject_WrongType ()
		{
			TestHtmlTextArea ta = new TestHtmlTextArea ();
			ta.PublicAddParsedSubObject (this);
		}

		[Test]
		public void AddParsedSubObject_LiteralControl ()
		{
			TestHtmlTextArea ta = new TestHtmlTextArea ();
			ta.PublicAddParsedSubObject (new LiteralControl ());
		}

		[Test]
		public void AddParsedSubObject_DataBoundLiteralControl ()
		{
			TestHtmlTextArea ta = new TestHtmlTextArea ();
			ta.PublicAddParsedSubObject (new DataBoundLiteralControl (1,1));
		}

		private bool serverChange;
		private void ServerChange (object sender, EventArgs e)
		{
			serverChange = true;
		}

		[Test]
		public void IPostBackDataHandler_RaisePostBackEvent ()
		{
			TestHtmlTextArea ta = new TestHtmlTextArea ();
			ta.ServerChange += new EventHandler (ServerChange);
			IPostBackDataHandler pbdh = (ta as IPostBackDataHandler);
			serverChange = false;
			pbdh.RaisePostDataChangedEvent ();
			Assert.IsTrue (serverChange, "ServerChange");
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void IPostBackDataHandler_LoadPostData_NullCollection ()
		{
			TestHtmlTextArea ta = new TestHtmlTextArea ();
			IPostBackDataHandler pbdh = (ta as IPostBackDataHandler);
			pbdh.LoadPostData ("id1", null);
		}

		[Test]
		[Category ("NotDotNet")] // MS throws a NullReferenceException here
		[Ignore ("Fails on .NET too.")]
		public void IPostBackDataHandler_LoadPostData_IdNull ()
		{
			TestHtmlTextArea ta = new TestHtmlTextArea ();
			ta.ID = "id1";
			IPostBackDataHandler pbdh = (ta as IPostBackDataHandler);
			NameValueCollection nvc = new NameValueCollection ();
			nvc.Add ("id1", "mono");
			Assert.IsFalse (pbdh.LoadPostData (null, new NameValueCollection ()));
			Assert.AreEqual (String.Empty, ta.Value, "Value");
		}

		[Test]
		[Category ("NotDotNet")] // MS throws a NullReferenceException here
		[Ignore ("Fails on .NET too.")]
		public void IPostBackDataHandler_LoadPostData_WrongId ()
		{
			TestHtmlTextArea ta = new TestHtmlTextArea ();
			ta.ID = "id1";
			IPostBackDataHandler pbdh = (ta as IPostBackDataHandler);
			NameValueCollection nvc = new NameValueCollection ();
			nvc.Add ("id1", "mono");
			Assert.IsFalse (pbdh.LoadPostData ("id2", nvc), "LoadPostData");
			Assert.AreEqual (String.Empty, ta.Value, "Value");
		}

		[Test]
		public void IPostBackDataHandler_LoadPostData ()
		{
			TestHtmlTextArea ta = new TestHtmlTextArea ();
			ta.ID = "id1";
			IPostBackDataHandler pbdh = (ta as IPostBackDataHandler);
			NameValueCollection nvc = new NameValueCollection ();
			nvc.Add ("id1", "mono");
			Assert.IsTrue (pbdh.LoadPostData ("id1", nvc), "LoadPostData");
			Assert.AreEqual ("mono", ta.Value, "Value");
		}
		[Test]
		public void RaisePostBackEvent ()
		{
			TestHtmlTextArea ta = new TestHtmlTextArea ();
			ta.ServerChange += new EventHandler (ServerChange);
			serverChange = false;
			ta.Raise ();
			Assert.IsTrue (serverChange, "ServerClick");
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void LoadPostData_NullCollection ()
		{
			TestHtmlTextArea ta = new TestHtmlTextArea ();
			ta.LoadPost ("id1", null);
		}

		[Test]
		[Category ("NotDotNet")] // MS throws a NullReferenceException here
		[Ignore ("Fails on .NET too.")]
		public void LoadPostData_IdNull ()
		{
			TestHtmlTextArea ta = new TestHtmlTextArea ();
			ta.ID = "id1";
			NameValueCollection nvc = new NameValueCollection ();
			nvc.Add ("id1", "mono");
			Assert.IsFalse (ta.LoadPost (null, nvc), "LoadPostData");
			Assert.AreEqual (String.Empty, ta.Value, "Value");
		}

		[Test]
		[Category ("NotDotNet")] // MS throws a NullReferenceException here
		[Ignore ("Fails on .NET too.")]
		public void LoadPostData_WrongId ()
		{
			TestHtmlTextArea ta = new TestHtmlTextArea ();
			ta.ID = "id1";
			NameValueCollection nvc = new NameValueCollection ();
			nvc.Add ("id1", "mono");
			Assert.IsFalse (ta.LoadPost ("id2", nvc), "LoadPostData");
			Assert.AreEqual (String.Empty, ta.Value, "Value");
		}

		[Test]
		public void LoadPostData ()
		{
			TestHtmlTextArea ta = new TestHtmlTextArea ();
			ta.ID = "id1";
			NameValueCollection nvc = new NameValueCollection ();
			nvc.Add ("id1", "mono");
			Assert.IsTrue (ta.LoadPost ("id1", nvc), "LoadPostData");
			Assert.AreEqual ("mono", ta.Value, "Value");
		}
	}
}
