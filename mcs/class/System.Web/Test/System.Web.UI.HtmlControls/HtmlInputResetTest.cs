//
// HtmlInputResetTest.cs
//	- Unit tests for System.Web.UI.HtmlControls.HtmlInputReset
//
// Author:
//	Chris Toshok <toshok@ximian.com>
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

	public class HtmlInputResetPoker : HtmlInputReset {

		public HtmlInputResetPoker ()
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
	public class HtmlInputResetTest {

		[Test]
		public void OverrideProperties ()
		{
			HtmlInputResetPoker p = new HtmlInputResetPoker ();

			Assert.IsTrue (p.CausesValidation, "A1");
			Assert.IsTrue (((HtmlInputButton)p).CausesValidation, "A2");

			Assert.AreEqual ("", p.ValidationGroup, "A3");
			Assert.AreEqual ("", ((HtmlInputButton)p).ValidationGroup, "A4");
		}

		[Test]
		public void Defaults ()
		{
			HtmlInputResetPoker p = new HtmlInputResetPoker ();

			Assert.IsTrue (p.CausesValidation, "A1");
			Assert.AreEqual ("", p.ValidationGroup, "A2");
		}

		[Test]
		public void CleanProperties ()
		{
			HtmlInputResetPoker p = new HtmlInputResetPoker ();

			p.CausesValidation = false;
			Assert.IsFalse (p.CausesValidation, "A1");

			p.CausesValidation = true;
			Assert.IsTrue (p.CausesValidation, "A2");

			p.CausesValidation = false;
			Assert.IsFalse (p.CausesValidation, "A3");

			p.ValidationGroup = "hi";
			Assert.AreEqual ("hi", p.ValidationGroup, "A4");

			p.ValidationGroup = "";
			Assert.AreEqual ("", p.ValidationGroup, "A4");
		}

		[Test]
		public void RenderAttributes ()
		{
			StringWriter sw = new StringWriter ();
			HtmlTextWriter tw = new HtmlTextWriter (sw);

			HtmlInputResetPoker p = new HtmlInputResetPoker ();

			Assert.AreEqual (p.Attributes.Count, 1, "A1");

			p.DoRenderAttributes (tw);
			Assert.AreEqual (sw.ToString (), " name type=\"reset\" /", "A2");
		}
	}	
}

