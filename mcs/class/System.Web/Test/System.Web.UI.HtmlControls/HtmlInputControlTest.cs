//
// HtmlInputControlTest.cs
//	- Unit tests for System.Web.UI.HtmlControls.HtmlInputControl
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
using System.IO;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using MonoTests.stand_alone.WebHarness;
using NUnit.Framework;

namespace MonoTests.System.Web.UI.HtmlControls {

	public class TestHtmlInputControl : HtmlInputControl {
		bool name_called;

		public TestHtmlInputControl ()
			: base ("mono")
		{
		}

		public TestHtmlInputControl (string type)
			: base (type)
		{
		}

		public string RenderAttributes ()
		{
			HtmlTextWriter writer = new HtmlTextWriter (new StringWriter ());
			writer.Write ("<dummy");
			base.RenderAttributes (writer);
			writer.Write (">");
			return writer.InnerWriter.ToString ();
		}

		public override string Name {
			get {
				name_called = true;
				return base.Name;
			}
		}

		public bool NameCalled {
			get { return name_called; }
			set { name_called = value; }
		}
	}

	public class UControl : UserControl {
	}

	[TestFixture]
	public class HtmlInputControlTest {

		private const int defaultAttributesCount = 1;

		[Test]
		public void DefaultProperties ()
		{
			TestHtmlInputControl ic = new TestHtmlInputControl ();
			Assert.AreEqual (defaultAttributesCount, ic.Attributes.Count, "Attributes.Count");
			Assert.IsNull (ic.Name, "Name");
			Assert.AreEqual ("mono", ic.Type, "Type");
			Assert.AreEqual (String.Empty, ic.Value, "Value");

			Assert.AreEqual ("input", ic.TagName, "TagName");
			Assert.AreEqual (defaultAttributesCount, ic.Attributes.Count, "Attributes.Count-2");
		}

		[Test]
		public void NullProperties ()
		{
			TestHtmlInputControl ic = new TestHtmlInputControl ();
			ic.Name = null;
			Assert.IsNull (ic.Name, "Name");
			ic.Value = null;
			Assert.AreEqual (String.Empty, ic.Value, "Value");

			Assert.AreEqual (defaultAttributesCount, ic.Attributes.Count, "Attributes.Count");
		}

		[Test]
		public void CleanProperties ()
		{
			TestHtmlInputControl ic = new TestHtmlInputControl ();
			ic.Name = "name";
			Assert.IsNull (ic.Name, "Name");
			Assert.AreEqual (defaultAttributesCount, ic.Attributes.Count, "always null");

			ic.Value = "value";
			Assert.AreEqual ("value", ic.Value, "Value");
			Assert.AreEqual (defaultAttributesCount + 1, ic.Attributes.Count, "1");

			ic.Name = null;
			Assert.IsNull (ic.Name, "-Name");
			ic.Value = null;
			Assert.AreEqual (String.Empty, ic.Value, "-Value");
			Assert.AreEqual (defaultAttributesCount, ic.Attributes.Count, "0");
		}

		[Test]
		public void Name ()
		{
			TestHtmlInputControl ic = new TestHtmlInputControl ();
			Assert.IsNull (ic.UniqueID, "UniqueID");
			Assert.IsNull (ic.ID, "ID");
			ic.Name = "name";
			Assert.IsNull (ic.Name, "Name");

			ic.ID = "id";
			Assert.AreEqual ("id", ic.ID, "ID-2");
			Assert.AreEqual ("id", ic.UniqueID, "UniqueID");
			Assert.AreEqual ("id", ic.Name, "Name-ID");

			ic.Name = "name";
			Assert.AreEqual ("id", ic.Name, "Name-ID-2");
			Assert.AreEqual ("id", ic.UniqueID, "UniqueID-2");

			ic.ID = null;
			Assert.IsNull (ic.ID, "ID-3");
			Assert.IsNull (ic.UniqueID, "UniqueID-3");
			Assert.IsNull (ic.Name, "Name-2");
		}

		[Test]
		public void Name_InsideNaming ()
		{
			Control ctrl = new UControl ();
			ctrl.ID = "parent";
			TestHtmlInputControl ic = new TestHtmlInputControl ();
			ctrl.Controls.Add (ic);
			Assert.IsNull (ic.ID, "ID");
			Assert.AreEqual (false, ic.NameCalled);
			ic.Name = "name";
			Assert.AreEqual (ic.Name, ic.UniqueID, "name and unique id");
			Assert.AreEqual (true, ic.NameCalled, "name called");

			ic.ID = "id";
			Assert.AreEqual ("id", ic.ID, "ID-2");
			Assert.AreEqual (ic.UniqueID, ic.Name, "Name-ID");

			ic.Name = "name";
			Assert.AreEqual (ic.Name, ic.UniqueID, "UniqueID-2");

			ic.ID = null;
			Assert.IsNull (ic.ID, "ID-3");
			Assert.IsNotNull (ic.UniqueID, "UniqueID-3");
			Assert.IsNotNull (ic.Name, "Name-2");
		}

		[Test]
		public void IDversusValue ()
		{
			TestHtmlInputControl ic = new TestHtmlInputControl ();
			Assert.AreEqual (String.Empty, ic.Value, "Value before");
			ic.ID = "id1";
			Assert.AreEqual ("id1", ic.ID, "ID");
			Assert.AreEqual (String.Empty, ic.Value, "Value after");
			// HtmlInputRadioButton has a different behaviour
		}

		[Test]
		public void RenderAttributes ()
		{
			TestHtmlInputControl ic = new TestHtmlInputControl ("test");
			ic.Name = "mono";
			ic.Value = "value";

			HtmlDiff.AssertAreEqual ("<dummy name type=\"test\" value=\"value\" />", ic.RenderAttributes (), "RenderAttributes failed #1");

			ic.ID = "toto";
			HtmlDiff.AssertAreEqual ("<dummy name=\"toto\" id=\"toto\" type=\"test\" value=\"value\" />", ic.RenderAttributes (), "RenderAttributes failed #2");
		}

		[Test]
		public void Constructor_Null ()
		{
			TestHtmlInputControl ic = new TestHtmlInputControl (null);
			Assert.AreEqual (String.Empty, ic.Type, "Type");
		}

		[Test]
		public void Password ()
		{
			TestHtmlInputControl ic = new TestHtmlInputControl ("password");
			ic.Name = "mono";
			ic.Value = "s3kr3t";

			// logic to hide password isn't in HtmlInputControl
			HtmlDiff.AssertAreEqual ("<dummy name type=\"password\" value=\"s3kr3t\" />", ic.RenderAttributes (), "Password failed");
		}
	}
}
