//
// Tests for System.Web.UI.LiteralControl
//
// Authors:
//      Igor Zelmanovich    <igorz@mainsoft.com>
//
// Copyright (C) 2005 Mainsoft, Inc (http://www.mainsoft.com)
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
using System.Threading;
using System.Security.Principal;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Collections;

namespace MonoTests.System.Web.UI {

	[TestFixture]
	public class LiteralControlTest
	{
		class PokerLiteralControl : LiteralControl
		{
			public PokerLiteralControl () {
				TrackViewState ();
			}

			public PokerLiteralControl (string text)
				: base (text) {
				TrackViewState ();
			}

			public object SaveState () {
				return SaveViewState ();
			}

			public void LoadState (object state) {
				LoadViewState (state);
			}
		}

		[Test]
		public void ViewState () {
			PokerLiteralControl literal = new PokerLiteralControl ();
			literal.Text = "Text";

			PokerLiteralControl copy = new PokerLiteralControl ();
			object state = literal.SaveState ();
			copy.LoadState (state);

			Assert.AreEqual (null, copy.Text, "ViewState");
		}
		
		[Test]
		public void NullProperties () {
			PokerLiteralControl literal = new PokerLiteralControl ();
			Assert.AreEqual (null, literal.Text, "NullProperties #1");
			literal.Text = null;
			Assert.AreEqual (String.Empty, literal.Text, "NullProperties #1");
		}
		
		[Test]
		public void Constructors () {
			PokerLiteralControl literal = new PokerLiteralControl ();
			Assert.AreEqual (null, literal.Text, "Constructors #1");

			literal = new PokerLiteralControl (null);
			Assert.AreEqual (String.Empty, literal.Text, "Constructors #2");

			literal = new PokerLiteralControl ("Text");
			Assert.AreEqual ("Text", literal.Text, "Constructors #3");
		}
	}
}
