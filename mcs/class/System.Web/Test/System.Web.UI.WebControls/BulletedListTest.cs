//
// Tests for System.Web.UI.WebControls.BulletedList.cs
//
// Author:
//	Yoni Klein (yonik@mainsoft.com)
//	Merav Sudri(meravs@mainsoft.com)
//      Tal Klahr (talk@mainsoft.com)
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
#if NET_2_0
using NUnit.Framework;
using System;
using System.Collections;
using System.IO;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using MonoTests.SystemWeb.Framework;
using MonoTests.stand_alone.WebHarness;


namespace MonoTests.System.Web.UI.WebControls
{
	class PokerBulletedList : BulletedList
	{
		public PokerBulletedList ()
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
		public StateBag StateBag
		{
			get { return base.ViewState; }
		}

		public string Render ()
		{
			StringWriter sw = new StringWriter ();
			HtmlTextWriter tw = new HtmlTextWriter (sw);
			tw.NewLine = "\n";
			Render (tw);
			return sw.ToString ();
		}

		public void DoRenderContents (HtmlTextWriter tw)
		{
			RenderContents (tw);
		}

		public void DoRenderBulletText (ListItem item, int index, HtmlTextWriter tw)
		{
			RenderBulletText (item, index, tw);
		}

		public void DoOnClick (BulletedListEventArgs e)
		{
			OnClick (e);
		}


	}

    class VerifyMultiSelectBulletedList : BulletedList
    {
        public new virtual void VerifyMultiSelect()
        {
            base.VerifyMultiSelect();
        }
    }

	[TestFixture]
	public class BulletedListTest
	{
		[TestFixtureSetUp]
		public void SetUp ()
		{
			WebTest.CopyResource (GetType (), "NoEventValidation.aspx", "NoEventValidation.aspx");
		}

		[Test]
		public void BulletedList_DefaultProperties ()
		{

			PokerBulletedList b = new PokerBulletedList ();
			Assert.AreEqual (0, b.StateBag.Count, "ViewState.Count");
			Assert.AreEqual (string.Empty, b.BulletImageUrl, "BulletImageUrl");
			Assert.AreEqual (BulletStyle.NotSet, b.BulletStyle, "BulletStyle");
			EmptyControlCollection c = new EmptyControlCollection (b);
			EmptyControlCollection c2 = (EmptyControlCollection) b.Controls;
			Assert.AreEqual ("System.Web.UI.EmptyControlCollection", b.Controls.GetType ().ToString (), "Controls");
			Assert.AreEqual ("Text", b.DisplayMode.ToString (), "DisplayMode");
			Assert.AreEqual (1, b.FirstBulletNumber, "FirstBulletNumber");
			Assert.AreEqual (-1, b.SelectedIndex, "SelectedIndex");
			Assert.AreEqual (null, b.SelectedItem, "SelectedItem");
			Assert.AreEqual (string.Empty, b.Target, "Target");
		}

		[Test]
		public void BulletedList_DefaultPropertiesNotWorking ()
		{
			PokerBulletedList b = new PokerBulletedList ();
			Assert.AreEqual (string.Empty, b.SelectedValue, "SelectedValue"); // NotImplementedException on Mono
			Assert.AreEqual (string.Empty, b.Text, "Text");
		}

		[Test]
		public void BulletedList_AssignToDefaultProperties ()
		{
			PokerBulletedList b = new PokerBulletedList ();
			Assert.AreEqual (0, b.StateBag.Count, "ViewState.Count");
			b.BulletImageUrl = "Images/edit.gif";
			Assert.AreEqual ("Images/edit.gif", b.BulletImageUrl, "BulletImageUrl");
			b.BulletStyle = BulletStyle.Numbered;
			Assert.AreEqual (BulletStyle.Numbered, b.BulletStyle, "BulletStyle");
			b.BulletStyle = BulletStyle.LowerAlpha;
			Assert.AreEqual (BulletStyle.LowerAlpha, b.BulletStyle, "BulletStyle");
			b.BulletStyle = BulletStyle.CustomImage;
			Assert.AreEqual (BulletStyle.CustomImage, b.BulletStyle, "BulletStyle");
			b.BulletStyle = BulletStyle.Square;
			Assert.AreEqual (BulletStyle.Square, b.BulletStyle, "BulletStyle");
			b.DisplayMode = BulletedListDisplayMode.HyperLink;
			Assert.AreEqual (BulletedListDisplayMode.HyperLink, b.DisplayMode, "DisplayMode");
			b.FirstBulletNumber = 4;
			Assert.AreEqual (4, b.FirstBulletNumber, "FirstBulletNumber");
			b.Target = "_search";
			Assert.AreEqual ("_search", b.Target, "Target_search");
			b.Target = "_top";
			Assert.AreEqual ("_top", b.Target, "Target_top");
			b.Target = "_parent";
			Assert.AreEqual ("_parent", b.Target, "Target_parent");
			b.Target = "_blank";
			Assert.AreEqual ("_blank", b.Target, "Target_blank");
			b.Target = "_self";
			Assert.AreEqual ("_self", b.Target, "Target_self");

		}

		[Test]
		public void BulletedList_NullProperties ()
		{
			PokerBulletedList b = new PokerBulletedList ();
			Assert.AreEqual (0, b.StateBag.Count, "ViewState.Count");
			b.BulletImageUrl = null;
			Assert.AreEqual (string.Empty, b.BulletImageUrl, "BulletImageUrl");
			b.Target = null;
			Assert.AreEqual (string.Empty, b.Target, "Target");

		}

		[Test]
		public void BulletedList_BulletStyle_Render ()
		{
			PokerBulletedList b = new PokerBulletedList ();
			AddListItems (b);
			Assert.AreEqual (b.Render (), "<ul>\n\t<li>Item1</li><li>Item2</li><li>Item3</li><li>Item4</li><li>Item5</li><li>Item6</li><li>Item7</li><li>Item8</li><li>Item9</li><li>Item10</li>\n</ul>", "Render1");
			b.BulletStyle = BulletStyle.Square;
			Assert.AreEqual (b.Render (), "<ul style=\"list-style-type:square;\">\n\t<li>Item1</li><li>Item2</li><li>Item3</li><li>Item4</li><li>Item5</li><li>Item6</li><li>Item7</li><li>Item8</li><li>Item9</li><li>Item10</li>\n</ul>", "BulletStyle.Square");
			b.BulletStyle = BulletStyle.LowerRoman;
			Assert.AreEqual (b.Render (), "<ol style=\"list-style-type:lower-roman;\">\n\t<li>Item1</li><li>Item2</li><li>Item3</li><li>Item4</li><li>Item5</li><li>Item6</li><li>Item7</li><li>Item8</li><li>Item9</li><li>Item10</li>\n</ol>", "BulletStyle.LowerRoman");
			b.BulletStyle = BulletStyle.Circle;
			Assert.AreEqual (b.Render (), "<ul style=\"list-style-type:circle;\">\n\t<li>Item1</li><li>Item2</li><li>Item3</li><li>Item4</li><li>Item5</li><li>Item6</li><li>Item7</li><li>Item8</li><li>Item9</li><li>Item10</li>\n</ul>", "BulletStyle.Circle");
			b.BulletStyle = BulletStyle.Disc;
			Assert.AreEqual (b.Render (), "<ul style=\"list-style-type:disc;\">\n\t<li>Item1</li><li>Item2</li><li>Item3</li><li>Item4</li><li>Item5</li><li>Item6</li><li>Item7</li><li>Item8</li><li>Item9</li><li>Item10</li>\n</ul>", "BulletStyle.Disc");
			b.BulletStyle = BulletStyle.LowerAlpha;
			Assert.AreEqual (b.Render (), "<ol style=\"list-style-type:lower-alpha;\">\n\t<li>Item1</li><li>Item2</li><li>Item3</li><li>Item4</li><li>Item5</li><li>Item6</li><li>Item7</li><li>Item8</li><li>Item9</li><li>Item10</li>\n</ol>", "BulletStyle.LowerAlpha");
			b.BulletStyle = BulletStyle.Numbered;
			Assert.AreEqual (b.Render (), "<ol style=\"list-style-type:decimal;\">\n\t<li>Item1</li><li>Item2</li><li>Item3</li><li>Item4</li><li>Item5</li><li>Item6</li><li>Item7</li><li>Item8</li><li>Item9</li><li>Item10</li>\n</ol>", "BulletStyle.Numbered");
			b.BulletStyle = BulletStyle.UpperAlpha;
			Assert.AreEqual (b.Render (), "<ol style=\"list-style-type:upper-alpha;\">\n\t<li>Item1</li><li>Item2</li><li>Item3</li><li>Item4</li><li>Item5</li><li>Item6</li><li>Item7</li><li>Item8</li><li>Item9</li><li>Item10</li>\n</ol>", "BulletStyle.UpperAlpha");
			b.BulletStyle = BulletStyle.UpperRoman;
			Assert.AreEqual (b.Render (), "<ol style=\"list-style-type:upper-roman;\">\n\t<li>Item1</li><li>Item2</li><li>Item3</li><li>Item4</li><li>Item5</li><li>Item6</li><li>Item7</li><li>Item8</li><li>Item9</li><li>Item10</li>\n</ol>", "BulletStyle.UpperRoman");
			b.BulletStyle = BulletStyle.NotSet;
			Assert.AreEqual (b.Render (), "<ul>\n\t<li>Item1</li><li>Item2</li><li>Item3</li><li>Item4</li><li>Item5</li><li>Item6</li><li>Item7</li><li>Item8</li><li>Item9</li><li>Item10</li>\n</ul>", "BulletStyle.NotSet");
			b.BulletStyle = BulletStyle.CustomImage;
			b.BulletImageUrl = "Images/edit.gif";
			Assert.AreEqual (b.Render (), "<ul style=\"list-style-image:url(Images/edit.gif);\">\n\t<li>Item1</li><li>Item2</li><li>Item3</li><li>Item4</li><li>Item5</li><li>Item6</li><li>Item7</li><li>Item8</li><li>Item9</li><li>Item10</li>\n</ul>", "BulletStyle.CustomImage");
		}

		[Test]
		public void BulletedList_HyperLinkDisplayMode_Render ()
		{
			PokerBulletedList b = new PokerBulletedList ();
			ListItem item1 = new ListItem ("HyperLink1", "TextFile1.txt");
			ListItem item2 = new ListItem ("HyperLink2", "TextFile2.txt");
			b.Items.Add (item1);
			b.Items.Add (item2);
			b.DisplayMode = BulletedListDisplayMode.HyperLink;
			Assert.AreEqual (b.Render (), "<ul>\n\t<li><a href=\"TextFile1.txt\">HyperLink1</a></li><li><a href=\"TextFile2.txt\">HyperLink2</a></li>\n</ul>", "BulletedListDisplayMode.HyperLink");			
		}

		[Test]
		public void BulletedList_ButtonLinkDisplayMode_Render ()
		{
			PokerBulletedList b = new PokerBulletedList ();
			AddListItems (b);
			b.DisplayMode = BulletedListDisplayMode.LinkButton;
			b.ID = "BulletedListTest";
			Page p = new Page ();
			p.Controls.Add (b);
			p.EnableEventValidation = false;
			string html = b.Render ();
			MonoTests.stand_alone.WebHarness.HtmlDiff.AssertAreEqual (b.Render (), "<ul id=\"BulletedListTest\">\n\t<li><a href=\"javascript:__doPostBack('BulletedListTest','0')\">Item1</a></li><li><a href=\"javascript:__doPostBack('BulletedListTest','1')\">Item2</a></li><li><a href=\"javascript:__doPostBack('BulletedListTest','2')\">Item3</a></li><li><a href=\"javascript:__doPostBack('BulletedListTest','3')\">Item4</a></li><li><a href=\"javascript:__doPostBack('BulletedListTest','4')\">Item5</a></li><li><a href=\"javascript:__doPostBack('BulletedListTest','5')\">Item6</a></li><li><a href=\"javascript:__doPostBack('BulletedListTest','6')\">Item7</a></li><li><a href=\"javascript:__doPostBack('BulletedListTest','7')\">Item8</a></li><li><a href=\"javascript:__doPostBack('BulletedListTest','8')\">Item9</a></li><li><a href=\"javascript:__doPostBack('BulletedListTest','9')\">Item10</a></li>\n</ul>", "BulletedListDisplayMode.LinkButton");

		}


		[Test]
		public void BulletedList_FirstBulletNumber_Render ()
		{
			PokerBulletedList b = new PokerBulletedList ();
			AddListItems (b);
			b.BulletStyle = BulletStyle.Numbered;
			b.FirstBulletNumber = 3;
			Assert.AreEqual (b.Render (), "<ol start=\"3\" style=\"list-style-type:decimal;\">\n\t<li>Item1</li><li>Item2</li><li>Item3</li><li>Item4</li><li>Item5</li><li>Item6</li><li>Item7</li><li>Item8</li><li>Item9</li><li>Item10</li>\n</ol>", "FirstBulletNumber1");
			b.FirstBulletNumber = 6;
			Assert.AreEqual (b.Render (), "<ol start=\"6\" style=\"list-style-type:decimal;\">\n\t<li>Item1</li><li>Item2</li><li>Item3</li><li>Item4</li><li>Item5</li><li>Item6</li><li>Item7</li><li>Item8</li><li>Item9</li><li>Item10</li>\n</ol>", "FirstBulletNumber2");
			b.FirstBulletNumber = -2;
			Assert.AreEqual (b.Render (), "<ol start=\"-2\" style=\"list-style-type:decimal;\">\n\t<li>Item1</li><li>Item2</li><li>Item3</li><li>Item4</li><li>Item5</li><li>Item6</li><li>Item7</li><li>Item8</li><li>Item9</li><li>Item10</li>\n</ol>", "FirstBulletNumber3");

		}

		[Test]
		public void BulletedList_ViewState ()
		{
			PokerBulletedList b = new PokerBulletedList ();
			b.BulletImageUrl = "Images/edit.gif";
			Assert.AreEqual ("Images/edit.gif", b.BulletImageUrl, "ViewState1");
			b.BulletStyle = BulletStyle.Numbered;
			Assert.AreEqual (BulletStyle.Numbered, b.BulletStyle, "ViewState2");
			b.Target = "_search";
			Assert.AreEqual ("_search", b.Target, "ViewState3");
			b.DisplayMode = BulletedListDisplayMode.HyperLink;
			Assert.AreEqual (BulletedListDisplayMode.HyperLink, b.DisplayMode, "ViewState4");
			b.FirstBulletNumber = 5;
			Assert.AreEqual (5, b.FirstBulletNumber, "ViewState5");
			object state = b.SaveState ();
			PokerBulletedList copy = new PokerBulletedList ();
			copy.LoadState (state);
			Assert.AreEqual ("Images/edit.gif", b.BulletImageUrl, "ViewState6");
			Assert.AreEqual (BulletStyle.Numbered, b.BulletStyle, "ViewState7");
			Assert.AreEqual ("_search", b.Target, "ViewState8");
			Assert.AreEqual (BulletedListDisplayMode.HyperLink, b.DisplayMode, "ViewState9");
			Assert.AreEqual (5, b.FirstBulletNumber, "ViewState10");
		}

		//Protected Methods

		[Test]
		public void BulletedList_RenderContents ()
		{
			PokerBulletedList p = new PokerBulletedList ();
			StringWriter sw = new StringWriter ();
			HtmlTextWriter tw = new HtmlTextWriter (sw);
			AddListItems (p);
			p.DoRenderContents (tw);
			Assert.AreEqual (sw.ToString (), "<li>Item1</li><li>Item2</li><li>Item3</li><li>Item4</li><li>Item5</li><li>Item6</li><li>Item7</li><li>Item8</li><li>Item9</li><li>Item10</li>", "BulletedList_RenderContents");
		}

		[Test]
		public void BulletedList_RenderBulletText ()
		{
			PokerBulletedList p = new PokerBulletedList ();
			StringWriter sw = new StringWriter ();
			HtmlTextWriter tw = new HtmlTextWriter (sw);
			AddListItems (p);
			p.DoRenderBulletText (p.Items[0], 0, tw);
			Assert.AreEqual (sw.ToString (), "Item1", "BulletedList_RenderBulletText1");
			p.DoRenderBulletText (p.Items[5], 5, tw);
			Assert.AreEqual (sw.ToString (), "Item1Item6", "BulletedList_RenderBulletText2");
		}

		//Events

		private bool clicked = false;

		private void BulletedListClickHandler (object sender, BulletedListEventArgs e)
		{
			clicked = true;
		}

		private void ResetEvents ()
		{
			clicked = false;
		}

		[Test]
		public void BulletedList_Events ()
		{
			PokerBulletedList pb = new PokerBulletedList ();
			AddListItems (pb);
			ResetEvents ();
			pb.Click += new BulletedListEventHandler (BulletedListClickHandler);
			Assert.AreEqual (false, clicked, "BeforeClick");
			pb.DoOnClick (new BulletedListEventArgs (0));
			Assert.AreEqual (true, clicked, "BeforeClick");
		}

		//PostBack raise event
		[Test]
		[Category ("NunitWeb")]
		public void BulletedList_PostBackEvent ()
		{
			WebTest t = new WebTest ("NoEventValidation.aspx");
			t.Invoker = PageInvoker.CreateOnInit (new PageDelegate (_PostBackEvent));
			string html = t.Run ();
			if (html.IndexOf ("Test_Item") < 0)
				Assert.Fail ("BulletedList not created");

			FormRequest fr = new FormRequest (t.Response, "form1");
			fr.Controls.Add ("__EVENTTARGET");
			fr.Controls.Add ("__EVENTARGUMENT");
			fr.Controls["__EVENTTARGET"].Value = "BL";
			fr.Controls["__EVENTARGUMENT"].Value = "0";
			t.Request = fr;
			html = t.Run ();
			if (t.UserData == null || (string) t.UserData != "list_Click Fired_0")
				Assert.Fail ("list_Click Not Fired");
		}

		#region _PostBackEvent_helper
		public static void _PostBackEvent (Page p)
		{
			BulletedList list = new BulletedList ();
			list.ID = "BL";
			list.DisplayMode = BulletedListDisplayMode.LinkButton;
			list.Items.Add (new ListItem ("Test_Item", "Test_Value", true));
			list.Click += new BulletedListEventHandler (list_Click);
			p.Controls.Add (list);
		}

		static void list_Click (object sender, BulletedListEventArgs e)
		{
			WebTest.CurrentTest.UserData = "list_Click Fired_" + e.Index.ToString();
		}
		#endregion

		[Test]
		[ExpectedException (typeof (HttpException))]
		public void VerifyMultiSelectTest ()
		{
			VerifyMultiSelectBulletedList list = new VerifyMultiSelectBulletedList ();
			list.VerifyMultiSelect ();
		}
       

		[TestFixtureTearDown]
		public void TearDown ()
		{
			WebTest.Unload ();
		}

		// Help class for DS creation
		private void AddListItems (PokerBulletedList b)
		{
			ListItem item1 = new ListItem ("Item1", "Item1");
			ListItem item2 = new ListItem ("Item2", "Item2");
			ListItem item3 = new ListItem ("Item3", "Item3");
			ListItem item4 = new ListItem ("Item4", "Item4");
			ListItem item5 = new ListItem ("Item5", "Item5");
			ListItem item6 = new ListItem ("Item6", "Item6");
			ListItem item7 = new ListItem ("Item7", "Item7");
			ListItem item8 = new ListItem ("Item8", "Item8");
			ListItem item9 = new ListItem ("Item9", "Item9");
			ListItem item10 = new ListItem ("Item10", "Item10");
			b.Items.Add (item1);
			b.Items.Add (item2);
			b.Items.Add (item3);
			b.Items.Add (item4);
			b.Items.Add (item5);
			b.Items.Add (item6);
			b.Items.Add (item7);
			b.Items.Add (item8);
			b.Items.Add (item9);
			b.Items.Add (item10);
		}
	}
}

#endif
