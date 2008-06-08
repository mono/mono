//
// HtmlAnchorTest.cs
//	- Unit tests for System.Web.UI.HtmlControls.HtmlAnchor
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

using NUnit.Framework;

namespace MonoTests.System.Web.UI.HtmlControls {

	public class TestHtmlAnchor : HtmlAnchor {

		public HtmlTextWriter GetWriter ()
		{
			StringWriter text = new StringWriter ();
			HtmlTextWriter writer = new HtmlTextWriter (text);
			base.RenderAttributes (writer);
			return writer;
		}
#if NET_2_0
		public void Raise ()
		{
			base.RaisePostBackEvent ("2.0");
		}
#endif

		public void TrakState () {
			TrackViewState();
		}

		public object SaveState () {
			return SaveViewState ();
		}

		public void LoadState (object state) {
			LoadViewState (state);
		}
	}

	[TestFixture]
	public class HtmlAnchorTest {

		[Test]
		public void DefaultProperties ()
		{
			HtmlAnchor a = new HtmlAnchor ();
			Assert.AreEqual (0, a.Attributes.Count, "Attributes.Count");

			Assert.AreEqual (String.Empty, a.HRef, "HRef");
			Assert.AreEqual (String.Empty, a.Name, "Name");
			Assert.AreEqual (String.Empty, a.Target, "Target");
			Assert.AreEqual (String.Empty, a.Title, "Title");

			Assert.AreEqual ("a", a.TagName, "TagName");
		}

		[Test]
		public void NullProperties ()
		{
			HtmlAnchor a = new HtmlAnchor ();
			a.HRef = null;
			Assert.AreEqual (String.Empty, a.HRef, "HRef");
			a.Name = null;
			Assert.AreEqual (String.Empty, a.Name, "Name");
			a.Target = null;
			Assert.AreEqual (String.Empty, a.Target, "Target");
			a.Title = null;
			Assert.AreEqual (String.Empty, a.Title, "Title");

			Assert.AreEqual (0, a.Attributes.Count, "Attributes.Count");
		}

		[Test]
		public void Target ()
		{
			HtmlAnchor a = new HtmlAnchor ();

			// specials (various casing)
			a.Target = "_blank";
			Assert.AreEqual ("_blank", a.Target, "_blank");
			a.Target = "_parent";
			Assert.AreEqual ("_parent", a.Target, "_parent");
			a.Target = "_SELF";
			Assert.AreEqual ("_SELF", a.Target, "_SELF");
			a.Target = "_ToP";
			Assert.AreEqual ("_ToP", a.Target, "_ToP");

			// alpha
			a.Target = "a";
			Assert.AreEqual ("a", a.Target, "a");
			a.Target = "blank";
			Assert.AreEqual ("blank", a.Target, "blank");
			a.Target = "Z";
			Assert.AreEqual ("Z", a.Target, "Z");
		}

		[Test]
		public void Target_Invalid ()
		{
			HtmlAnchor a = new HtmlAnchor ();

			// non alpha
			a.Target = "9a";
			Assert.AreEqual ("9a", a.Target, "9a");

			// non special
			a.Target = "_mono";
			Assert.AreEqual ("_mono", a.Target, "_mono");
		}

		[Test]
		public void HRef ()
		{
			TestHtmlAnchor a = new TestHtmlAnchor ();
			a.HRef = "~/otherfile.txt";
			Assert.AreEqual ("~/otherfile.txt", a.HRef, "HRef");
			// resolve doesn't apply on the property
		}

		[Test]
		public void RenderAttributes ()
		{
			TestHtmlAnchor a = new TestHtmlAnchor ();
			a.HRef = "*1*";
			a.Name = "*2*";
			a.Target = "*3*";
			a.Title = "*4*";
			Assert.AreEqual (4, a.Attributes.Count, "Attributes.Count/4");

			HtmlTextWriter writer = a.GetWriter ();
			Assert.AreEqual (" href=\"*1*\" name=\"*2*\" target=\"*3*\" title=\"*4*\"", writer.InnerWriter.ToString (), "attributes");

			// HRef is missing, from the attributes collection, after rendering
			Assert.AreEqual (3, a.Attributes.Count, "Attributes.Count/3");
			Assert.AreEqual (String.Empty, a.HRef, "HRef");
			// but href is still rendered
			Assert.AreEqual (" href=\"*1*\" name=\"*2*\" target=\"*3*\" title=\"*4*\"", writer.InnerWriter.ToString (), "HRef is back");
		}

		[Test]
		public void RenderAttributes_HRef_Empty ()
		{
			TestHtmlAnchor a = new TestHtmlAnchor ();
			Assert.AreEqual (0, a.Attributes.Count, "#1");
			a.HRef = string.Empty;
			a.Name = "*2*";
			a.Target = "*3*";
			a.Title = "*4*";
			Assert.AreEqual (3, a.Attributes.Count, "#2");

			HtmlTextWriter writer = a.GetWriter ();
			Assert.AreEqual (" name=\"*2*\" target=\"*3*\" title=\"*4*\"", writer.InnerWriter.ToString (), "#3");
		}

		[Test]
		public void RenderAttributes_HRef_Null ()
		{
			TestHtmlAnchor a = new TestHtmlAnchor ();
			Assert.AreEqual (0, a.Attributes.Count, "#1");
			a.HRef = null;
			a.Name = "*2*";
			a.Target = "*3*";
			a.Title = "*4*";
			Assert.AreEqual (3, a.Attributes.Count, "#2");

			HtmlTextWriter writer = a.GetWriter ();
			Assert.AreEqual (" name=\"*2*\" target=\"*3*\" title=\"*4*\"", writer.InnerWriter.ToString (), "#3");
		}

		[Test]
		public void RenderAttributes_Name_Empty ()
		{
			TestHtmlAnchor a = new TestHtmlAnchor ();
			Assert.AreEqual (0, a.Attributes.Count, "#1");
			a.HRef = "*1*";
			a.Name = string.Empty;
			a.Target = "*3*";
			a.Title = "*4*";
			Assert.AreEqual (3, a.Attributes.Count, "#2");

			HtmlTextWriter writer = a.GetWriter ();
			Assert.AreEqual (" href=\"*1*\" target=\"*3*\" title=\"*4*\"", writer.InnerWriter.ToString (), "#3");
		}

		[Test]
		public void RenderAttributes_Name_Null ()
		{
			TestHtmlAnchor a = new TestHtmlAnchor ();
			Assert.AreEqual (0, a.Attributes.Count, "#1");
			a.HRef = "*1*";
			a.Name = null;
			a.Target = "*3*";
			a.Title = "*4*";
			Assert.AreEqual (3, a.Attributes.Count, "#2");

			HtmlTextWriter writer = a.GetWriter ();
			Assert.AreEqual (" href=\"*1*\" target=\"*3*\" title=\"*4*\"", writer.InnerWriter.ToString (), "#3");
		}

		[Test]
		public void RenderAttributes_Target_Empty ()
		{
			TestHtmlAnchor a = new TestHtmlAnchor ();
			Assert.AreEqual (0, a.Attributes.Count, "#1");
			a.HRef = "*1*";
			a.Name = "*2*";
			a.Target = string.Empty;
			a.Title = "*4*";
			Assert.AreEqual (3, a.Attributes.Count, "#2");

			HtmlTextWriter writer = a.GetWriter ();
			Assert.AreEqual (" href=\"*1*\" name=\"*2*\" title=\"*4*\"", writer.InnerWriter.ToString (), "#3");
		}

		[Test]
		public void RenderAttributes_Target_Null ()
		{
			TestHtmlAnchor a = new TestHtmlAnchor ();
			Assert.AreEqual (0, a.Attributes.Count, "#1");
			a.HRef = "*1*";
			a.Name = "*2*";
			a.Target = null;
			a.Title = "*4*";
			Assert.AreEqual (3, a.Attributes.Count, "#2");

			HtmlTextWriter writer = a.GetWriter ();
			Assert.AreEqual (" href=\"*1*\" name=\"*2*\" title=\"*4*\"", writer.InnerWriter.ToString (), "#3");
		}

		[Test]
		public void RenderAttributes_Title_Empty ()
		{
			TestHtmlAnchor a = new TestHtmlAnchor ();
			Assert.AreEqual (0, a.Attributes.Count, "#1");
			a.HRef = "*1*";
			a.Name = "*2*";
			a.Target = "*3*";
			a.Title = string.Empty;
			Assert.AreEqual (3, a.Attributes.Count, "#2");

			HtmlTextWriter writer = a.GetWriter ();
			Assert.AreEqual (" href=\"*1*\" name=\"*2*\" target=\"*3*\"", writer.InnerWriter.ToString (), "#3");
		}

		[Test]
		public void RenderAttributes_Title_Null ()
		{
			TestHtmlAnchor a = new TestHtmlAnchor ();
			Assert.AreEqual (0, a.Attributes.Count, "#1");
			a.HRef = "*1*";
			a.Name = "*2*";
			a.Target = "*3*";
			a.Title = null;
			Assert.AreEqual (3, a.Attributes.Count, "#2");

			HtmlTextWriter writer = a.GetWriter ();
			Assert.AreEqual (" href=\"*1*\" name=\"*2*\" target=\"*3*\"", writer.InnerWriter.ToString (), "#3");
		}

		private bool serverClick;
		private void ServerClick (object sender, EventArgs e)
		{
			serverClick = true;
		}

		[Test]
#if NET_2_0
		[ExpectedException (typeof (NullReferenceException))] // since 2.0 RC :-(
#endif
		public void IPostBackEventHandler_RaisePostBackEvent ()
		{
			TestHtmlAnchor a = new TestHtmlAnchor ();
			a.ServerClick += new EventHandler (ServerClick);
			IPostBackEventHandler pbeh = (a as IPostBackEventHandler);
			serverClick = false;
			pbeh.RaisePostBackEvent ("mono");
			Assert.IsTrue (serverClick, "ServerClick");
		}

		[Test]
		public void AbsoluteHRef ()
		{
			TestHtmlAnchor a = new TestHtmlAnchor ();
			a.HRef = "http://127.0.0.1/";
			HtmlTextWriter writer = a.GetWriter ();
			Assert.AreEqual (" href=\"http://127.0.0.1/\"", writer.InnerWriter.ToString (), "#01");
		}
#if NET_2_0
		[Test]
		[ExpectedException (typeof (NullReferenceException))] // since 2.0 RC :-(
		public void RaisePostBackEvent ()
		{
			TestHtmlAnchor a = new TestHtmlAnchor ();
			a.ServerClick += new EventHandler (ServerClick);
			serverClick = false;
			a.Raise ();
			Assert.IsTrue (serverClick, "ServerClick");
		}

		[Test]
		public void ViewState2 () {
			TestHtmlAnchor a = new TestHtmlAnchor ();
			a.TrakState ();
			a.CausesValidation = false;
			a.ValidationGroup = "VG";
			
			object s = a.SaveState ();
			
			TestHtmlAnchor copy = new TestHtmlAnchor ();
			
			copy.LoadState (s);

			Assert.AreEqual (false, copy.CausesValidation, "A1");
			Assert.AreEqual ("VG", copy.ValidationGroup, "A2");
		}
#endif
	}
}
