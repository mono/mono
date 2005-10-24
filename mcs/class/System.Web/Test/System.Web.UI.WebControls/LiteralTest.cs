//
// Tests for System.Web.UI.WebControls.Literal.cs 
//
// Author:
//	Jackson Harper (jackson@ximian.com)
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
	class LiteralPoker : Literal {

		private bool use_poker_col;

		public LiteralPoker () : this (false)
		{
		}

		public LiteralPoker (bool use_poker_col)
		{
			TrackViewState ();
			this.use_poker_col = use_poker_col;
		}
		
		public object SaveState ()
		{
			return SaveViewState ();
		}

		public void LoadState (object o)
		{
			LoadViewState (o);
		}

		public void ParsedSubObject (object o)
		{
			AddParsedSubObject (o);
		}

		public ControlCollection ControlCollection ()
		{
			if (use_poker_col)
				return ControlCollection ();
			return CreateControlCollection ();
		}
	}
	
	[TestFixture]	
	public class LiteralTest {

		[Test]
		public void TextProperty ()
		{
			LiteralPoker p = new LiteralPoker ();

			Assert.AreEqual (p.Text, String.Empty, "A1");

			p.Text = "foo";
			Assert.AreEqual (p.Text, "foo", "A2");

			p.Text = null;
			Assert.AreEqual (p.Text, String.Empty, "A3");
		}

		[Test]
		public void ViewState ()
		{
			LiteralPoker p = new LiteralPoker ();
			p.Text = "foo";

			LiteralPoker copy = new LiteralPoker ();
			copy.LoadState (p.SaveState ());
			Assert.AreEqual (copy.Text, "foo", "A1");
		}

		[Test]
		public void Render ()
		{
			StringWriter sw = new StringWriter ();
			HtmlTextWriter tw = new HtmlTextWriter (sw);

			Literal l = new Literal ();
			l.Text = "foobar";
			l.RenderControl (tw);
			Assert.AreEqual (sw.ToString (), "foobar", "A1");
		}

		[Test]
		public void ControlsCollection ()
		{
			LiteralPoker p = new LiteralPoker ();

			Assert.AreEqual (p.ControlCollection ().GetType (),
					typeof (EmptyControlCollection), "A1");
		}

		[Test]
		[ExpectedException (typeof (HttpException))]
		public void AddParsedSubObject1 ()
		{
			LiteralPoker p = new LiteralPoker (true);
			p.ParsedSubObject (this);
		}

		[Test]
		[ExpectedException (typeof (HttpException))]
		public void AddParsedSubObject2 ()
		{
			LiteralPoker p = new LiteralPoker (true);
			p.Text = "hey";
			p.ParsedSubObject (p);
		}

		[Test]
		public void AddParsedSubObject3 ()
		{
			LiteralPoker p = new LiteralPoker (true);
			p.ParsedSubObject (new LiteralControl ("Hey!"));
			Assert.AreEqual (0, p.Controls.Count, "#01");
			Assert.AreEqual ("Hey!", p.Text, "#02");
		}
	}
}

