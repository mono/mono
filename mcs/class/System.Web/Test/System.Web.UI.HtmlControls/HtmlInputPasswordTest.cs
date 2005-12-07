//
// HtmlInputPasswordTest.cs
//	- Unit tests for System.Web.UI.HtmlControls.HtmlInputPassword
//
// Author:
//	Chris Toshok  <toshok@ximian.com>
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

using System;
using System.Collections.Specialized;
using System.IO;
using System.Web.UI;
using System.Web.UI.HtmlControls;

using NUnit.Framework;

namespace MonoTests.System.Web.UI.HtmlControls {

	public class TestHtmlInputPassword : HtmlInputPassword {

		bool value_changed; // true if the "value" is changed in RenderAttributes
		string new_value; // "value" in ViewState if value_changed is true.
		bool attr_value_changed; // same but for attributes (instead of viewstate)
		string attr_new_value;


		public TestHtmlInputPassword ()
			: base ()
		{
		}

		public string RenderAttributes ()
		{
			HtmlTextWriter writer = new HtmlTextWriter (new StringWriter ());
			string val = (string) ViewState ["value"];
			string att = Attributes ["value"];
			base.RenderAttributes (writer);
			if (val != (string) ViewState ["value"]) {
				value_changed = true;
				new_value = (string) ViewState ["value"];
			}
			if (att != Attributes ["value"]) {
				attr_value_changed = true;
				attr_new_value = Attributes ["value"];
			}
			return writer.InnerWriter.ToString ();
		}

		public bool ViewStateValueChanged {
			get { return value_changed; }
		}

		public string ViewStateNewValue {
			get { return new_value; }
		}

		public bool AttributeValueChanged {
			get { return attr_value_changed; }
		}

		public string AttributeNewValue {
			get { return attr_new_value; }
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
	public class HtmlInputPasswordTest {

		private const int defaultAttributesCount = 1;

		[Test]
		public void DefaultProperties ()
		{
			HtmlInputPassword it = new HtmlInputPassword ();
			Assert.AreEqual (defaultAttributesCount, it.Attributes.Count, "Attributes.Count");

			Assert.AreEqual ("password", it.Type, "Type");
			Assert.AreEqual (String.Empty, it.Value, "Value");

			Assert.AreEqual ("input", it.TagName, "TagName");
			Assert.AreEqual (defaultAttributesCount, it.Attributes.Count, "Attributes.Count-2");
		}

		[Test]
		public void RenderAttributes ()
		{
			TestHtmlInputPassword it = new TestHtmlInputPassword ();
			it.MaxLength = 4;
			it.Size = 2;
			it.Name = "mono";
			it.Value = "value";
			Assert.AreEqual (" name type=\"password\" maxlength=\"4\" size=\"2\" /", it.RenderAttributes ());
			Assert.IsTrue (it.ViewStateValueChanged, "ViewStateValueChanged");
			Assert.IsTrue (it.AttributeValueChanged, "AttributeValueChanged");
		}

		private bool serverChange;
		private void ServerChange (object sender, EventArgs e)
		{
			serverChange = true;
		}

		[Test]
		public void IPostBackDataHandler_RaisePostBackEvent ()
		{
			TestHtmlInputPassword it = new TestHtmlInputPassword ();
			it.ServerChange += new EventHandler (ServerChange);
			IPostBackDataHandler pbdh = (it as IPostBackDataHandler);
			serverChange = false;
			pbdh.RaisePostDataChangedEvent ();
			Assert.IsTrue (serverChange, "ServerChange");
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void IPostBackDataHandler_LoadPostData_NullCollection ()
		{
			TestHtmlInputPassword it = new TestHtmlInputPassword ();
			IPostBackDataHandler pbdh = (it as IPostBackDataHandler);
			pbdh.LoadPostData ("id1", null);
		}

		[Test]
		public void IPostBackDataHandler_LoadPostData ()
		{
			TestHtmlInputPassword it = new TestHtmlInputPassword ();
			it.ID = "id1";
			IPostBackDataHandler pbdh = (it as IPostBackDataHandler);
			NameValueCollection nvc = new NameValueCollection ();
			nvc.Add ("id1", "mono");
			Assert.IsTrue (pbdh.LoadPostData ("id1", nvc), "LoadPostData");
			Assert.AreEqual ("mono", it.Value, "Value");
		}

		[Test]
		public void RaisePostBackEvent ()
		{
			TestHtmlInputPassword it = new TestHtmlInputPassword ();
			it.ServerChange += new EventHandler (ServerChange);
			serverChange = false;
			it.Raise ();
			Assert.IsTrue (serverChange, "ServerClick");
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void LoadPostData_NullCollection ()
		{
			TestHtmlInputPassword it = new TestHtmlInputPassword ();
			it.LoadPost ("id1", null);
		}

		[Test]
		public void LoadPostData ()
		{
			TestHtmlInputPassword it = new TestHtmlInputPassword ();
			it.ID = "id1";
			NameValueCollection nvc = new NameValueCollection ();
			nvc.Add ("id1", "mono");
			Assert.IsTrue (it.LoadPost ("id1", nvc), "LoadPostData");
			Assert.AreEqual ("mono", it.Value, "Value");
		}
	}
}

#endif
