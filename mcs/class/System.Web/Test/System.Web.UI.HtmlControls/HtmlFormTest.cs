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
using System.Web.UI;
using System.Web.UI.HtmlControls;

using NUnit.Framework;

namespace MonoTests.System.Web.UI.HtmlControls {

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

#if NET_2_0
		public ControlCollection GetControlCollection ()
		{
			return CreateControlCollection();
		}
#endif
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
			Assert.IsNull (a.GetAttribute ("name"), "A5");

#if NET_2_0
			form.SubmitDisabledControls = true;
			Assert.IsNull (a.GetAttribute ("submitdisabledcontrols"), "A6");
#endif

			form.Target = "target";
			Assert.AreEqual ("target", a.GetAttribute ("target"), "A7");
		}

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
