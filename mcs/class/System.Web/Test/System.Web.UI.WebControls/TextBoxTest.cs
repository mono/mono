//
// Tests for System.Web.UI.WebControls.TextBox.cs 
//
// Author:
//     Ben Maurer (bmaurer@novell.com)
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

namespace MonoTests.System.Web.UI.WebControls {
	
	[TestFixture]	
	public class TextBoxTest {
		class Poker : TextBox {
			public new void AddParsedSubObject (object o)
			{
				base.AddParsedSubObject (o);
			}

			public void TrackState () 
			{
				TrackViewState ();
			}
			
			public object SaveState ()
			{
				foreach (string s in ViewState.Keys)
					Console.WriteLine ("{0}: {1}", s, ViewState[s]);

				return SaveViewState ();
			}
			
			public void LoadState (object o)
			{
				LoadViewState (o);
			}
			
			public string Render ()
			{
				StringWriter sw = new StringWriter ();
				sw.NewLine = "\n";
				HtmlTextWriter writer = new HtmlTextWriter (sw);
				base.Render (writer);
				return writer.InnerWriter.ToString ();
			}			
		}

		[Test]
		public void MultilineRenderEscape ()
		{
			Poker t = new Poker ();
			t.TextMode = TextBoxMode.MultiLine;
			t.Text = "</textarea>";
#if NET_2_0
			string exp = "<textarea rows=\"2\" cols=\"20\">&lt;/textarea&gt;</textarea>";
#else
			string exp = "<textarea name>&lt;/textarea&gt;</textarea>";
#endif

			Assert.AreEqual (exp, t.Render ());
		}


#if NET_2_0
		[Test]
		public void ValidationProperties ()
		{
			Poker t = new Poker ();

			// initial values
			Assert.AreEqual (false, t.CausesValidation, "A1");
			Assert.AreEqual ("", t.ValidationGroup, "A2");

			t.ValidationGroup = "VG";
			Assert.AreEqual ("VG", t.ValidationGroup, "A3");

			t.CausesValidation = true;
			Assert.IsTrue (t.CausesValidation, "A4");
		}

		[Test]
		public void ViewState ()
		{
			Poker t = new Poker ();

			t.TrackState();

			t.ValidationGroup = "VG";
			t.CausesValidation = true;

			object s = t.SaveState ();
			Console.WriteLine ("state = {0}", s == null ? "null" : "not-null");

			Poker copy = new Poker ();

			copy.LoadState (s);

			Assert.AreEqual ("VG", copy.ValidationGroup, "A1");
			Assert.IsTrue (copy.CausesValidation, "A2");
		}

		[Test]
		public void ValidationRender ()
		{
			/* test to show that the validation settings
			 * have no effect on downlevel rendering */
			Poker t = new Poker ();

			t.TrackState();

			t.ValidationGroup = "VG";
			t.CausesValidation = true;
			t.TextMode = TextBoxMode.MultiLine;

			string exp = "<textarea rows=\"2\" cols=\"20\"></textarea>";
			Assert.AreEqual (exp, t.Render ());
		}
#endif
	}
}

