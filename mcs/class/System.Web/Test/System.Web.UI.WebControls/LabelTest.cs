//
// Tests for System.Web.UI.WebControls.Label.cs 
//
// Author:
//	Miguel de Icaza (miguel@novell.com)
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
	public class LabelTest {	
		class Poker : Label {
			
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
		public void Label_ViewState ()
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
		public void Label_Render ()
		{
			Poker l = new Poker ();
			l.Text = "Hello";
			Assert.AreEqual ("<span>Hello</span>", l.Render (), "R1");
		}

		Poker MakeNested ()
		{
			Poker p = new Poker ();
			Label ll = new Label ();
			ll.Text = ", World";
			p.AddParsedSubObject (new LiteralControl ("Hello"));
			p.AddParsedSubObject (ll);
			return p;
		}
		
		
		[Test]
		public void ChildControl ()
		{
			Poker l = MakeNested ();
			Assert.AreEqual ("<span>Hello<span>, World</span></span>", l.Render ());
			Assert.AreEqual ("", l.Text);
			l.Text = "Hello";
			Assert.AreEqual ("<span>Hello</span>", l.Render ());
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
			
			Assert.AreEqual ("<span>Hello</span>", l.Render ());
			Assert.AreEqual ("Hello", l.Text);
			Assert.IsFalse (l.HasControls ());
		}

		[Test]
		public void AssocControlId ()
		{
			Page p = new Page ();
			Poker l = new Poker ();
			TextBox t = new TextBox ();
			t.ID = "mytxtbox";

			p.Controls.Add (l);
			p.Controls.Add (t);
			
			l.Text = "Hello";
			l.AssociatedControlID = "mytxtbox";
			Assert.AreEqual (@"<label for=""mytxtbox"">Hello</label>", l.Render ());			
		}		
	}
}

		
