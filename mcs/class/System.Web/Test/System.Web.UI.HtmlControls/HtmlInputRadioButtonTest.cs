//
// HtmlInputRadioButtonTest.cs
//	- Unit tests for System.Web.UI.HtmlControls.HtmlInputRadioButton
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
using System.Web.UI;
using System.Web.UI.HtmlControls;

using NUnit.Framework;

namespace MonoTests.System.Web.UI.HtmlControls {

	public class TestHtmlInputRadioButton : HtmlInputRadioButton {

		public string RenderAttributes ()
		{
			HtmlTextWriter writer = new HtmlTextWriter (new StringWriter ());
			base.RenderAttributes (writer);
			return writer.InnerWriter.ToString ();
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
	public class HtmlInputRadioButtonTest {

		private const int defaultAttributesCount = 1;

		[Test]
		public void DefaultProperties ()
		{
			HtmlInputRadioButton rb = new HtmlInputRadioButton ();
			Assert.AreEqual (defaultAttributesCount, rb.Attributes.Count, "Attributes.Count");

			Assert.IsFalse (rb.Checked, "Checked");
			Assert.AreEqual (String.Empty, rb.Name, "Name");
			Assert.IsNull (rb.Value, "Value");

			Assert.AreEqual ("input", rb.TagName, "TagName");
			Assert.AreEqual (defaultAttributesCount, rb.Attributes.Count, "Attributes.Count-2");
		}

		[Test]
		public void NullProperties ()
		{
			HtmlInputRadioButton rb = new HtmlInputRadioButton ();
			rb.Name = null;
			Assert.AreEqual (String.Empty, rb.Name, "Name");
			rb.Value = null;
			Assert.IsNull (rb.Value, "Value");

			Assert.AreEqual (defaultAttributesCount, rb.Attributes.Count, "Attributes.Count");
		}

		[Test]
		public void CleanProperties ()
		{
			HtmlInputRadioButton rb = new HtmlInputRadioButton ();
			rb.Checked = true;
			Assert.IsTrue (rb.Checked, "Checked");
			rb.Name = "name";
			Assert.AreEqual ("name", rb.Name, "Name");
			rb.Value = "value";
			Assert.AreEqual ("value", rb.Value, "Value");
			Assert.AreEqual (defaultAttributesCount + 3, rb.Attributes.Count, "1");

			rb.Checked = false;
			Assert.IsFalse (rb.Checked, "-Checked");
			rb.Name = null;
			Assert.AreEqual (String.Empty, rb.Name, "-Name");
			rb.Value = null;
			Assert.IsNull (rb.Value, "-Value");
			Assert.AreEqual (defaultAttributesCount, rb.Attributes.Count, "0");
		}

		[Test]
		public void Value_Existing ()
		{
			TestHtmlInputRadioButton rb = new TestHtmlInputRadioButton ();
			rb.Value = "a";
			Assert.AreEqual ("a", rb.Value, "Value before");
			rb.ID = "id1";
			Assert.AreEqual ("id1", rb.ID, "ID");
			Assert.AreEqual ("a", rb.Value, "Value after");
		}

		[Test]
		public void Value_Resetting ()
		{
			TestHtmlInputRadioButton rb = new TestHtmlInputRadioButton ();
			rb.ID = "id1";
			Assert.AreEqual ("id1", rb.Value, "Value before");
			rb.Value = "a";
			Assert.AreEqual ("id1", rb.ID, "ID");
			Assert.AreEqual ("a", rb.Value, "Value after");
		}

		[Test]
		public void Value_ResetNull ()
		{
			TestHtmlInputRadioButton rb = new TestHtmlInputRadioButton ();
			rb.Value = "a";
			rb.ID = "id1";
			rb.Value = null;
			Assert.AreEqual ("id1", rb.ID, "ID");
			Assert.AreEqual ("id1", rb.Value, "Value");
		}

		[Test]
		// note: this behaviour isn't present in HtmlInputControl
		public void IDversusValue ()
		{
			TestHtmlInputRadioButton rb = new TestHtmlInputRadioButton ();
			Assert.IsNull (rb.Value, "Value before");
			rb.ID = "id1";
			Assert.AreEqual ("id1", rb.ID, "ID");
			Assert.AreEqual ("id1", rb.Value, "Value after");
		}

		[Test]
		[Ignore ("throws NullReferenceException")]
		public void RenderAttributes ()
		{
			TestHtmlInputRadioButton rb = new TestHtmlInputRadioButton ();
			rb.Checked = true;
			rb.Name = "mono";
			rb.Value = "value";
			rb.RenderAttributes ();
		}

		private bool serverChange;
		private void ServerChange (object sender, EventArgs e)
		{
			serverChange = true;
		}

		[Test]
		public void IPostBackDataHandler_RaisePostBackEvent ()
		{
			TestHtmlInputRadioButton rb = new TestHtmlInputRadioButton ();
			rb.ServerChange += new EventHandler (ServerChange);
			IPostBackDataHandler pbdh = (rb as IPostBackDataHandler);
			serverChange = false;
			pbdh.RaisePostDataChangedEvent ();
			Assert.IsTrue (serverChange, "ServerChange");
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void IPostBackDataHandler_LoadPostData_NullCollection ()
		{
			TestHtmlInputRadioButton rb = new TestHtmlInputRadioButton ();
			IPostBackDataHandler pbdh = (rb as IPostBackDataHandler);
			pbdh.LoadPostData ("id1", null);
		}

		[Test]
		[Category ("NotDotNet")] // MS throws a NullReferenceException here
		public void IPostBackDataHandler_LoadPostData_IdNull ()
		{
			TestHtmlInputRadioButton rb = new TestHtmlInputRadioButton ();
			rb.ID = "id1";
			IPostBackDataHandler pbdh = (rb as IPostBackDataHandler);
			NameValueCollection nvc = new NameValueCollection ();
			nvc.Add ("id1", "mono");
			Assert.IsFalse (pbdh.LoadPostData (null, nvc), "LoadPostData");
			Assert.AreEqual ("id1", rb.Value, "Value");
		}

		[Test]
		public void IPostBackDataHandler_LoadPostData_WrongId ()
		{
			TestHtmlInputRadioButton rb = new TestHtmlInputRadioButton ();
			rb.ID = "id1";
			IPostBackDataHandler pbdh = (rb as IPostBackDataHandler);
			NameValueCollection nvc = new NameValueCollection ();
			nvc.Add ("id1", "mono");
			Assert.IsFalse (pbdh.LoadPostData ("id2", nvc), "LoadPostData");
			Assert.AreEqual ("id1", rb.Value, "Value");
		}

		[Test]
		public void IPostBackDataHandler_LoadPostData ()
		{
			TestHtmlInputRadioButton rb = new TestHtmlInputRadioButton ();
			rb.ID = "id1";
			IPostBackDataHandler pbdh = (rb as IPostBackDataHandler);
			NameValueCollection nvc = new NameValueCollection ();
			nvc.Add ("id1", "id1");
			// we didn't change the state of the control
			Assert.IsFalse (pbdh.LoadPostData ("id1", nvc), "LoadPostData");
			Assert.AreEqual ("id1", rb.Value, "Value");
		}

		[Test]
		public void RenderValue1 ()
		{
			TestHtmlInputRadioButton rb = new TestHtmlInputRadioButton ();
			rb.ID = "id";
			string attrs = rb.RenderAttributes ();
			Assert.IsTrue (attrs.IndexOf ("value=\"id\"") >= 0);
			rb.Value = "hola<&";
			attrs = rb.RenderAttributes ();
			Assert.IsTrue (attrs.IndexOf ("value=\"hola<&\"") >= 0);
		}

		[Test]
		public void RaisePostBackEvent ()
		{
			TestHtmlInputRadioButton rb = new TestHtmlInputRadioButton ();
			rb.ServerChange += new EventHandler (ServerChange);
			serverChange = false;
			rb.Raise ();
			Assert.IsTrue (serverChange, "ServerClick");
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void LoadPostData_NullCollection ()
		{
			TestHtmlInputRadioButton rb = new TestHtmlInputRadioButton ();
			rb.LoadPost ("id1", null);
		}

		[Test]
		[Category ("NotDotNet")] // MS throws a NullReferenceException here
		public void LoadPostData_IdNull ()
		{
			TestHtmlInputRadioButton rb = new TestHtmlInputRadioButton ();
			rb.ID = "id1";
			NameValueCollection nvc = new NameValueCollection ();
			nvc.Add ("id1", "mono");
			Assert.IsFalse (rb.LoadPost (null, nvc), "LoadPostData");
			Assert.AreEqual ("id1", rb.Value, "Value");
		}

		[Test]
		public void LoadPostData_WrongId ()
		{
			TestHtmlInputRadioButton rb = new TestHtmlInputRadioButton ();
			rb.ID = "id1";
			NameValueCollection nvc = new NameValueCollection ();
			nvc.Add ("id1", "mono");
			Assert.IsFalse (rb.LoadPost ("id2", nvc), "LoadPostData");
			Assert.AreEqual ("id1", rb.Value, "Value");
		}

		[Test]
		public void LoadPostData ()
		{
			TestHtmlInputRadioButton rb = new TestHtmlInputRadioButton ();
			rb.ID = "id1";
			NameValueCollection nvc = new NameValueCollection ();
			nvc.Add ("id1", "mono");
			Assert.IsFalse (rb.LoadPost ("id1", nvc), "LoadPostData");
			Assert.AreEqual ("id1", rb.Value, "Value");
		}
	}
}
