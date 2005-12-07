//
// Tests for System.Web.UI.WebControls.ImageButton.cs
//
// Author:
//	Jordi Mas i Hernandez (jordi@ximian.com)
//

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

using NUnit.Framework;
using System;
using System.IO;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace MonoTests.System.Web.UI.WebControls
{
	class PokerImageButton : ImageButton {
		public PokerImageButton ()
		{
			TrackViewState ();
		}

		public object SaveState ()
		{
			return SaveViewState ();
		}

		public void LoadState (object o)
		{
			LoadViewState (o);
		}
	}

	[TestFixture]
	public class ImageButtonTest {
		
		[Test]
		public void ImageButton_DefaultValues ()
		{
			ImageButton b = new ImageButton ();
			Assert.AreEqual (true, b.CausesValidation, "CausesValidation");
			Assert.AreEqual (string.Empty, b.CommandArgument, "CommandArgument");
			Assert.AreEqual (string.Empty, b.CommandName, "CommandName");
#if NET_2_0
			Assert.AreEqual (string.Empty, b.ValidationGroup, "ValidationGroup");
#endif
		}

		
		[Test]
		public void ImageButton_Render ()
		{
			StringWriter sw = new StringWriter ();
			HtmlTextWriter tw = new HtmlTextWriter (sw);

			ImageButton b = new ImageButton ();			
			b.RenderControl (tw);				
			
			Assert.AreEqual (true, sw.ToString().IndexOf ("<input") != -1, "A1");
			Assert.AreEqual (true, sw.ToString().IndexOf ("type=\"image\"") != -1, "A2");
		}

		[Test]
		public void ImageButton_ViewState ()
		{
			PokerImageButton p = new PokerImageButton ();

			p.CommandArgument = "arg";
			Assert.AreEqual (p.CommandArgument, "arg", "A1");
			p.CommandName = "cmd";
			Assert.AreEqual (p.CommandName, "cmd", "A2");
#if NET_2_0
			p.ValidationGroup = "VG1";
			Assert.AreEqual (p.ValidationGroup, "VG1", "A3");
#endif

			object state = p.SaveState ();

			PokerImageButton copy = new PokerImageButton ();
			copy.LoadState (state);

			Assert.AreEqual (copy.CommandArgument, "arg", "A4");
			Assert.AreEqual (copy.CommandName, "cmd", "A5");
#if NET_2_0
			Assert.AreEqual (copy.ValidationGroup, "VG1", "A6");
#endif

		}

		[Test]
		public void RenderName ()
		{
			StringWriter sw = new StringWriter ();
			HtmlTextWriter tw = new HtmlTextWriter (sw);

			Page page = new Page ();
			ImageButton b = new ImageButton ();			
			page.Controls.Add (b);
			page.RenderControl (tw);
			Assert.AreEqual (true, sw.ToString().IndexOf ("<input") != -1, "A1");
			Assert.AreEqual (true, sw.ToString().IndexOf ("type=\"image\"") != -1, "A2");
			Assert.AreEqual (true, sw.ToString().IndexOf ("name=\"") != -1, "A3");
		}

	}
}


