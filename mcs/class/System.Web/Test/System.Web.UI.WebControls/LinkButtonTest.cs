//
// Tests for System.Web.UI.WebControls.LinkButton 
//
// Author:
//	Miguel de Icaza (miguel@novell.com) [copied alot of this from his Label test]
//      Ben Maurer <bmaurer@novell.com>
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
	[TestFixture]
	public class LinkButtonTest {	
		class Poker : LinkButton {
			
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
		public void ViewState ()
		{
			Poker p = new Poker ();
			p.TrackState ();

			Assert.AreEqual (p.Text, "", "A1");
			p.Text = "Hello";
			Assert.AreEqual (p.Text, "Hello", "A2");

			object state = p.SaveState ();

			Poker copy = new Poker ();
			copy.TrackState ();
			copy.LoadState (state);
			Assert.AreEqual (copy.Text, "Hello", "A3");
		}

		[Test]
		public void Render ()
		{
			Poker l = new Poker ();
			l.Text = "Hello";
			Assert.AreEqual ("<a>Hello</a>", l.Render (), "R1");
		}

		Poker MakeNested ()
		{
			Poker p = new Poker ();
			LinkButton ll = new LinkButton ();
			ll.Text = ", World";
			p.AddParsedSubObject (new LiteralControl ("Hello"));
			p.AddParsedSubObject (ll);
			return p;
		}
		
		
		[Test]
		public void ChildControl ()
		{
			Poker l = MakeNested ();
			Assert.AreEqual ("<a>Hello<a>, World</a></a>", l.Render ());
			Assert.AreEqual ("", l.Text);
			l.Text = "Hello";
			Assert.AreEqual ("<a>Hello</a>", l.Render ());
			Assert.AreEqual ("Hello", l.Text);
			Assert.IsFalse (l.HasControls ());
		}

		[Test]
		public void ChildControlViewstate ()
		{
			Poker l = MakeNested ();
			l.TrackState ();
			l.Text = "Hello";

			object o = l.SaveState ();
			l = MakeNested ();
			l.TrackState ();
			l.LoadState (o);
			
			Assert.AreEqual ("<a>Hello</a>", l.Render ());
			Assert.AreEqual ("Hello", l.Text);
			Assert.IsFalse (l.HasControls ());
		}

		class BubbleNet : Control {
			public EventHandler Bubble;
			protected override bool OnBubbleEvent (object s, EventArgs e)
			{
				if (Bubble != null)
					Bubble (s, e);
				return false;
			}	
		}

		//
		// I (heart) anonymous methods
		//
		[Test]
		public void TestEvents ()
		{
			BubbleNet p = new BubbleNet ();
			LinkButton l = new LinkButton ();
			l.CommandName = "N";
			l.CommandArgument = "A";
			l.CausesValidation = false; // avoid an NRE on msft
			p.Controls.Add (l);
			
			bool got_command = false;
			bool got_click = false;
			bool got_bubble = false;

			l.Click += delegate {
				Assert.IsFalse (got_click, "#1");
				Assert.IsFalse (got_command, "#2");
				Assert.IsFalse (got_bubble, "#3");
				got_click = true;
			};
			
			l.Command += delegate (object sender, CommandEventArgs e) {
				Assert.IsTrue (got_click, "#4");
				Assert.IsFalse (got_command, "#5");
				Assert.IsFalse (got_bubble, "#6");
				Assert.AreEqual ("N", e.CommandName, "#7");
				Assert.AreEqual ("A", e.CommandArgument, "#8");
				got_command = true;
			};
			p.Bubble += delegate (object sender, EventArgs e) {
				Assert.IsTrue (got_click, "#9");
				Assert.IsTrue (got_command, "#10");
				Assert.IsFalse (got_bubble, "#11");
				Assert.AreEqual ("N", ((CommandEventArgs) e).CommandName, "#12");
				Assert.AreEqual ("A", ((CommandEventArgs) e).CommandArgument, "#13");
				got_bubble = true;
			};
			
			((IPostBackEventHandler) l).RaisePostBackEvent ("");
			Assert.IsTrue (got_click, "#14");
			Assert.IsTrue (got_command, "#15");
			Assert.IsTrue (got_bubble, "#16");
		}

#if NET_2_0
		[Test]
		public void TestValidationGroup ()
		{
			Poker p = new Poker ();
			p.TrackState ();

			Assert.AreEqual (p.ValidationGroup, "", "V1");
			p.ValidationGroup = "VG1";
			Assert.AreEqual (p.ValidationGroup, "VG1", "V2");

			object state = p.SaveState ();

			Poker copy = new Poker ();
			copy.TrackState ();
			copy.LoadState (state);
			Assert.AreEqual (copy.ValidationGroup, "VG1", "A3");
		}
#endif
		
	}
}

		
