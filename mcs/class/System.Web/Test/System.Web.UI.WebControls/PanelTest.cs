//
// Tests for System.Web.UI.WebControls.Panel.cs 
//
// Author:
//     Ben Maurer <bmaurer@novell.com>
//     Yoni Klain <yonik@mainsoft.com>
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
using System.Drawing;
using MonoTests.stand_alone.WebHarness;
using MonoTests.SystemWeb.Framework;

namespace MonoTests.System.Web.UI.WebControls {
	[TestFixture]
	public class PanelTest
	{
		#region helpclasses
		class Poker : Panel {
			
			public string Render ()
			{
				StringWriter sw = new StringWriter ();
				sw.NewLine = "\n";
				HtmlTextWriter writer = new HtmlTextWriter (sw);
				base.Render (writer);
				return writer.InnerWriter.ToString ();
			}
		}
	
		class PokerS : Panel
		{
			public string Render ()
			{
				StringWriter sw = new StringWriter ();
				sw.NewLine = "\n";
				HtmlTextWriter writer = new HtmlTextWriter (sw);
				base.Render (writer);
				return writer.InnerWriter.ToString ();
			}
			protected override Style CreateControlStyle ()
			{
				Style s = new Style (new StateBag ());
				s.BackColor = Color.Red;
				s.BorderColor = Color.Red;
				return s;
			}

			public override void RenderBeginTag (HtmlTextWriter writer)
			{
				base.RenderBeginTag (writer);
			}

			public override void RenderEndTag (HtmlTextWriter writer)
			{
				base.RenderEndTag (writer);
			}

			public  string RenderBeginTag ()
			{
				StringWriter sw = new StringWriter ();
				sw.NewLine = "\n";
				HtmlTextWriter writer = new HtmlTextWriter (sw);
				base.RenderBeginTag (writer);
				return writer.InnerWriter.ToString ();
			}

			public  string RenderEndTag ()
			{
				StringWriter sw = new StringWriter ();
				sw.NewLine = "\n";
				HtmlTextWriter writer = new HtmlTextWriter (sw);
				base.RenderBeginTag (writer);
				base.RenderEndTag (writer);
				return writer.InnerWriter.ToString ();
			}
		}

		class PokerR : Panel
		{
			public string Render ()
			{
				StringWriter sw = new StringWriter ();
				sw.NewLine = "\n";
				HtmlTextWriter writer = new HtmlTextWriter (sw);
				sw.Write ("Render");
				base.Render (writer);
				return writer.InnerWriter.ToString ();
			}

			public override void RenderBeginTag (HtmlTextWriter writer)
			{
				writer.Write ("RenderBeginTag");
			}

			public override void RenderEndTag (HtmlTextWriter writer)
			{
				writer.Write ("RenderEndTag");
			}
		}
		#endregion

		[TestFixtureSetUp]
		public void SetUp ()
		{
			WebTest.CopyResource (GetType (), "NoEventValidation.aspx", "NoEventValidation.aspx");
		}

		[Test]
		public void Defaults ()
		{
			Poker p = new Poker ();

			Assert.AreEqual (ContentDirection.NotSet, p.Direction, "Direction"); 
			Assert.AreEqual (string.Empty, p.GroupingText, "GroupingText");
			Assert.AreEqual (ScrollBars.None, p.ScrollBars, "ScrollBars"); 
			Assert.AreEqual (string.Empty, p.DefaultButton, "DefaultButton");
		}
		
		[Test]
		public void NoWrap ()
		{
			Poker p = new Poker ();
			p.Wrap = false;
			p.Controls.Add (new LiteralControl ("TEXT"));
			const string html ="<div style=\"white-space:nowrap;\">\n\tTEXT\n</div>";
			Assert.AreEqual (html, p.Render ());
		}

		[Test]
		public void CreateControlStyle ()
		{
			PokerS p = new PokerS ();
			Style s = p.ControlStyle;
			Assert.AreEqual (Color.Red, s.BackColor, "CreateControlStyle#1");
			Assert.AreEqual (Color.Red, s.BorderColor, "CreateControlStyle#2");
			p.ApplyStyle (s);
			string html = p.Render ();
			string origHtml = "<div style=\"background-color:Red;border-color:Red;\">\n\n</div>";
			HtmlDiff.AssertAreEqual (origHtml, html, "CreateControlStyle");
		}

		[Test]
		[Category ("NunitWeb")]
		public void DefaultButton ()
		{
			WebTest t = new WebTest ("NoEventValidation.aspx");
			t.Invoker = PageInvoker.CreateOnInit (DefaultButton__Init);
			t.Run ();
		}

		public static void DefaultButton__Init (Page p)
		{
			Poker pl = new Poker ();
			pl.DefaultButton = "MyButton";
			Button b = new Button ();
			b.ID = "MyButton";
			p.Form.Controls.Add (b);
			p.Form.Controls.Add (pl);
			string html = pl.Render ();
			if (html.IndexOf ("onkeypress") == -1)
				Assert.Fail ("Default button script not created #1");
			if (html.IndexOf ("MyButton") == -1)
				Assert.Fail ("Default button script not created #2");
		}
		
		[Test]
		[Category("NunitWeb")]
		[ExpectedException(typeof(InvalidOperationException))]
		public void DefaultButton_Exception ()
		{
			WebTest t = new WebTest ("NoEventValidation.aspx");
			t.Invoker = PageInvoker.CreateOnInit (DefaultButton_Init);
			t.Run ();

		}

		public static void DefaultButton_Init (Page p)
		{
			Poker pl = new Poker ();
			pl.DefaultButton = "test";
			p.Form.Controls.Add (pl);
			pl.Render ();

		}

		[Test]
		public void Direction ()
		{
			Poker p = new Poker ();
			p.Direction = ContentDirection.LeftToRight;
			string html = p.Render();
			string origHtml = "<div dir=\"ltr\">\n\n</div>";
			HtmlDiff.AssertAreEqual (origHtml, html, "Direction");
		}

		[Test]
		public void GroupingText ()
		{
			Poker p = new Poker ();
			p.GroupingText = "MyNameText";
			string html = p.Render ();
			string origHtml = "<div>\n\t<fieldset>\n\t\t<legend>\n\t\t\tMyNameText\n\t\t</legend>\n\t</fieldset>\n</div>";
			HtmlDiff.AssertAreEqual (origHtml, html, "GroupingText");
		}

		[Test]
		public void RenderBeginTag ()
		{
			PokerS p = new PokerS ();
			string html = p.RenderBeginTag ();
			HtmlDiff.AssertAreEqual ("<div>\n", html, "RenderBeginTag");
		}

		[Test]
		public void RenderEndTag ()
		{
			PokerS p = new PokerS ();
			string html = p.RenderEndTag ();
			HtmlDiff.AssertAreEqual ("<div>\n\n</div>", html, "RenderBeginTag");
		}

		[Test]
		public void RenderFlow ()
		{
			PokerR p = new PokerR ();
			string html = p.Render ();
			Assert.AreEqual ("RenderRenderBeginTagRenderEndTag", html, "RenderFlow");
		}

		[Test]
		public void Scroll_Bars ()
		{
			Poker p = new Poker ();
			p.ScrollBars = ScrollBars.Horizontal;
			string html = p.Render ();
			string origHtml1 = "<div style=\"overflow-x:scroll;\">\n\n</div>";
			HtmlDiff.AssertAreEqual (origHtml1, html, "ScrollBars.Horizontal");
			p.ScrollBars = ScrollBars.Vertical;
			html = p.Render ();
			string origHtml2 = "<div style=\"overflow-y:scroll;\">\n\n</div>";
			HtmlDiff.AssertAreEqual (origHtml2, html, "ScrollBars.Vertical");
			p.ScrollBars = ScrollBars.Both;
			html = p.Render ();
			string origHtml3 = "<div style=\"overflow:scroll;\">\n\n</div>";
			HtmlDiff.AssertAreEqual (origHtml3, html, "ScrollBars.Both");
		}

		[TestFixtureTearDown]
		public void TearDown ()
		{
			WebTest.Unload ();
		}
	}
}

		
