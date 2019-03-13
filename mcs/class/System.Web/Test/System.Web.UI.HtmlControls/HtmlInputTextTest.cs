//
// HtmlInputTextTest.cs
//	- Unit tests for System.Web.UI.HtmlControls.HtmlInputText
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
using MonoTests.stand_alone.WebHarness;
using NUnit.Framework;

namespace MonoTests.System.Web.UI.HtmlControls {

	public class TestHtmlInputText : HtmlInputText {

		bool value_changed; // true if the "value" is changed in RenderAttributes
		string new_value; // "value" in ViewState if value_changed is true.
		bool attr_value_changed; // same but for attributes (instead of viewstate)
		string attr_new_value;


		public TestHtmlInputText ()
			: base ()
		{
		}

		public TestHtmlInputText (string type)
			: base (type)
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
			return base.LoadPostData(key, nvc);
		}

		public void Raise ()
		{
			base.RaisePostDataChangedEvent ();
		}
	}

	[TestFixture]
	public class HtmlInputTextTest {

		private const int defaultAttributesCount = 1;

		[Test]
		public void ConstructorType ()
		{
			HtmlInputText it = new HtmlInputText ("mono");
			Assert.AreEqual ("mono", it.Type, "Type");
		}

		[Test]
		public void DefaultProperties ()
		{
			HtmlInputText it = new HtmlInputText ();
			Assert.AreEqual (defaultAttributesCount, it.Attributes.Count, "Attributes.Count");

			Assert.AreEqual (-1, it.MaxLength, "MaxLength");
			Assert.IsNull (it.Name, "Name");
			Assert.AreEqual (-1, it.Size, "Size");
			Assert.AreEqual ("text", it.Type, "Type");
			Assert.AreEqual (String.Empty, it.Value, "Value");

			Assert.AreEqual ("input", it.TagName, "TagName");
			Assert.AreEqual (defaultAttributesCount, it.Attributes.Count, "Attributes.Count-2");
		}

		[Test]
		public void NullProperties ()
		{
			HtmlInputText it = new HtmlInputText ();
			it.MaxLength = -1;
			Assert.AreEqual (-1, it.MaxLength, "MaxLength");
			it.Name = null;
			Assert.IsNull (it.Name, "Name");
			it.Size = -1;
			Assert.AreEqual (-1, it.Size, "Size");
			it.Value = null;
			Assert.AreEqual (String.Empty, it.Value, "Value");

			Assert.AreEqual (defaultAttributesCount, it.Attributes.Count, "Attributes.Count");
		}

		[Test]
		public void CleanProperties ()
		{
			HtmlInputText it = new HtmlInputText ();
			it.MaxLength = 1;
			Assert.AreEqual (1, it.MaxLength, "MaxLength");
			it.Name = "name";
			Assert.IsNull (it.Name, "Name");
			it.Size = 2;
			Assert.AreEqual (2, it.Size, "Size");
			it.Value = "value";
			Assert.AreEqual ("value", it.Value, "Value");
			Assert.AreEqual (defaultAttributesCount + 3, it.Attributes.Count, "1");

			it.MaxLength = -1;
			Assert.AreEqual (-1, it.MaxLength, "-MaxLength");
			it.Name = null;
			Assert.IsNull (it.Name, "-Name");
			it.Size = -1;
			Assert.AreEqual (-1, it.Size, "Size");
			it.Value = null;
			Assert.AreEqual (String.Empty, it.Value, "-Value");
			Assert.AreEqual (defaultAttributesCount, it.Attributes.Count, "0");
		}

		[Test]
		public void Password ()
		{
			TestHtmlInputText it = new TestHtmlInputText ("password");
			it.Value = "s3kr3t";
			it.ID = "passwd";
			Assert.AreEqual ("s3kr3t", it.Value, "Value");
		}

		[Test]
		public void RenderAttributes ()
		{
			TestHtmlInputText it = new TestHtmlInputText ();
			it.MaxLength = 4;
			it.Size = 2;
			it.Name = "mono";
			it.Value = "value";
			Assert.AreEqual (" name type=\"text\" maxlength=\"4\" size=\"2\" value=\"value\" /", it.RenderAttributes ());
		}

		[Test]
		public void RenderAttributes_Password ()
		{
			TestHtmlInputText it = new TestHtmlInputText ("password");
			it.MaxLength = 2;
			it.Size = 4;
			it.ID = "mono";
			it.Value = "s3kr3t";
			// value is there, maybe because a new HtmlInputPassword class exists ?
			HtmlDiff.AssertAreEqual (" name=\"mono\" type=\"password\" id=\"mono\" maxlength=\"2\" size=\"4\" value=\"s3kr3t\" /", it.RenderAttributes (),"Render failed");
			Assert.IsFalse (it.ViewStateValueChanged, "ViewStateValueChanged");
			Assert.IsFalse (it.AttributeValueChanged, "AttributeValueChanged");
			Assert.IsNull (it.ViewStateNewValue, "ViewStateNewValue");
			Assert.IsNull (it.AttributeNewValue, "AttributeNewValue");
		}

		private bool serverChange;
		private void ServerChange (object sender, EventArgs e)
		{
			serverChange = true;
		}

		[Test]
		public void IPostBackDataHandler_RaisePostBackEvent ()
		{
			TestHtmlInputText it = new TestHtmlInputText ("password");
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
			TestHtmlInputText it = new TestHtmlInputText ("password");
			IPostBackDataHandler pbdh = (it as IPostBackDataHandler);
			pbdh.LoadPostData ("id1", null);
		}

		[Test]
		[Category ("NotDotNet")] // MS throws a NullReferenceException here
		[Ignore("Fails on .NET too.")]
		public void IPostBackDataHandler_LoadPostData_IdNull ()
		{
			TestHtmlInputText it = new TestHtmlInputText ("password");
			it.ID = "id1";
			IPostBackDataHandler pbdh = (it as IPostBackDataHandler);
			NameValueCollection nvc = new NameValueCollection ();
			nvc.Add ("id1", "mono");
			Assert.IsTrue (pbdh.LoadPostData (null, nvc), "LoadPostData");
			Assert.AreEqual (String.Empty, it.Value, "Value");
		}

		[Test]
		[Category ("NotDotNet")] // MS throws a NullReferenceException here
		[Ignore("Fails on .NET too.")]
		public void IPostBackDataHandler_LoadPostData_WrongId ()
		{
			TestHtmlInputText it = new TestHtmlInputText ("password");
			it.ID = "id1";
			IPostBackDataHandler pbdh = (it as IPostBackDataHandler);
			NameValueCollection nvc = new NameValueCollection ();
			nvc.Add ("id1", "mono");
			Assert.IsTrue (pbdh.LoadPostData ("id2", nvc), "LoadPostData");
			Assert.AreEqual (String.Empty, it.Value, "Value");
		}

		[Test]
		public void IPostBackDataHandler_LoadPostData ()
		{
			TestHtmlInputText it = new TestHtmlInputText ("password");
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
			TestHtmlInputText it = new TestHtmlInputText ("password");
			it.ServerChange += new EventHandler (ServerChange);
			serverChange = false;
			it.Raise ();
			Assert.IsTrue (serverChange, "ServerClick");
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void LoadPostData_NullCollection ()
		{
			TestHtmlInputText it = new TestHtmlInputText ("password");
			it.LoadPost ("id1", null);
		}

		[Test]
		[Category ("NotDotNet")] // MS throws a NullReferenceException here
		[Ignore("Fails on .NET too.")]
		public void LoadPostData_IdNull ()
		{
			TestHtmlInputText it = new TestHtmlInputText ("password");
			it.ID = "id1";
			NameValueCollection nvc = new NameValueCollection ();
			nvc.Add ("id1", "mono");
			Assert.IsTrue (it.LoadPost (null, nvc), "LoadPostData");
			Assert.AreEqual (String.Empty, it.Value, "Value");
		}

		[Test]
		[Category ("NotDotNet")] // MS throws a NullReferenceException here
		[Ignore("Fails on .NET too.")]
		public void LoadPostData_WrongId ()
		{
			TestHtmlInputText it = new TestHtmlInputText ("password");
			it.ID = "id1";
			NameValueCollection nvc = new NameValueCollection ();
			nvc.Add ("id1", "mono");
			Assert.IsTrue (it.LoadPost ("id2", nvc), "LoadPostData");
			Assert.AreEqual (String.Empty, it.Value, "Value");
		}

		[Test]
		public void LoadPostData ()
		{
			TestHtmlInputText it = new TestHtmlInputText ("password");
			it.ID = "id1";
			NameValueCollection nvc = new NameValueCollection ();
			nvc.Add ("id1", "mono");
			Assert.IsTrue (it.LoadPost ("id1", nvc), "LoadPostData");
			Assert.AreEqual ("mono", it.Value, "Value");
		}
	}
}
