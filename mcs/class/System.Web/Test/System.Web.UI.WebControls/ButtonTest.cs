//
// Tests for System.Web.UI.WebControls.Button.cs
//
// Author:
//	Jordi Mas i Hernandez (jordi@ximian.com)
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
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
	class PokerButton : Button {
		public PokerButton ()
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
	public class ButtonTest {
		
		[Test]
		public void Button_DefaultValues ()
		{
			Button b = new Button ();
			Assert.AreEqual (true, b.CausesValidation, "CausesValidation");
			Assert.AreEqual (string.Empty, b.CommandArgument, "CommandArgument");
			Assert.AreEqual (string.Empty, b.CommandName, "CommandName");			
#if NET_2_0
			Assert.AreEqual (string.Empty, b.ValidationGroup, "ValidationGroup");			
#endif
		}

		[Test]
		public void Button_ViewState ()
		{
			PokerButton p = new PokerButton ();

			Assert.AreEqual (p.Text, "", "A1");
			p.Text = "Hello";
			Assert.AreEqual (p.Text, "Hello", "A2");

#if NET_2_0
			p.ValidationGroup = "VG1";
			Assert.AreEqual (p.ValidationGroup, "VG1", "A3");
#endif

			object state = p.SaveState ();

			PokerButton copy = new PokerButton ();
			copy.LoadState (state);
			Assert.AreEqual (copy.Text, "Hello", "A4");

#if NET_2_0
			Assert.AreEqual (copy.ValidationGroup, "VG1", "A5");
#endif
		}

		[Test]
		public void Button_Render ()
		{
			StringWriter sw = new StringWriter ();
			HtmlTextWriter tw = new HtmlTextWriter (sw);

			Button b = new Button ();
			b.Text = "Hello";
			b.RenderControl (tw);
			
			Assert.AreEqual (true, sw.ToString().IndexOf ("value=\"Hello\"") != -1, "A4");
			Assert.AreEqual (true, sw.ToString().IndexOf ("<input") != -1, "A5");
			Assert.AreEqual (true, sw.ToString().IndexOf ("type=\"submit\"") != -1, "A6");
		}

		[Test]
		public void IgnoresChildren ()
		{
			Button b = new  Button ();
			b.Controls.Add (new LiteralControl ("hola"));
			Assert.AreEqual (1, b.Controls.Count, "controls");
			StringWriter sw = new StringWriter ();
			HtmlTextWriter tw = new HtmlTextWriter (sw);
			b.RenderControl (tw);
			string str = tw.ToString ();
			Assert.AreEqual (-1, str.IndexOf ("hola"), "hola");
		}
	}
}


