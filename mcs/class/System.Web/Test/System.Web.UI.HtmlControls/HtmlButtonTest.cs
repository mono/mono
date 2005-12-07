//
// HtmlButtonTest.cs
//	- Unit tests for System.Web.UI.HtmlControls.HtmlButton
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

using NUnit.Framework;

namespace MonoTests.System.Web.UI.HtmlControls {

	public class HtmlButtonPoker : HtmlButton {

		public HtmlButtonPoker ()
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
	}

	[TestFixture]
	public class HtmlButtonTest {

		[Test]
		public void Defaults ()
		{
			HtmlButtonPoker p = new HtmlButtonPoker ();

			Assert.IsTrue (p.CausesValidation, "A1");
#if NET_2_0
			Assert.AreEqual ("", p.ValidationGroup, "A2");
#endif
		}

		[Test]
		public void CleanProperties ()
		{
			HtmlButtonPoker p = new HtmlButtonPoker ();

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
			HtmlButtonPoker p = new HtmlButtonPoker ();
			p.CausesValidation = true;
#if NET_2_0
			p.ValidationGroup = "VG";
#endif
			object state = p.SaveState();

			HtmlButtonPoker copy = new HtmlButtonPoker ();
			copy.LoadState (state);
#if NET_2_0
			Assert.AreEqual ("VG", copy.ValidationGroup, "A1");
#endif
			Assert.IsTrue (copy.CausesValidation, "A2");
		}

		[Test]
		public void RenderAttributes ()
		{
			StringWriter sw = new StringWriter ();
			HtmlTextWriter tw = new HtmlTextWriter (sw);

			HtmlButtonPoker p = new HtmlButtonPoker ();

			Assert.AreEqual (p.Attributes.Count, 0, "A1");

			p.DoRenderAttributes (tw);
			Assert.AreEqual (sw.ToString (), String.Empty, "A2");

			p.ServerClick += new EventHandler (EmptyHandler);

			p.DoRenderAttributes (tw);
			// This is empty because the control doesn't have
			// its Page property initialized
			Assert.AreEqual (sw.ToString (), String.Empty, "A3");
		}

		private static void EmptyHandler (object sender, EventArgs e)
		{
		}
	}	
}

