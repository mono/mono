//
// Tests for System.Web.UI.WebControls.MenuTest.cs
//
// Author:
//	Yoni Klein (yonik@mainsoft.com)
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


#if NET_2_0


using NUnit.Framework;
using System;
using System.Threading;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using System.Drawing;
using MyWebControl = System.Web.UI.WebControls;
using System.Collections;
using NunitWeb;
using MonoTests.stand_alone.WebHarness;


namespace MonoTests.System.Web.UI.WebControls
{
	
	
	class PokerMenu:Menu
        {
		public PokerMenu ()
		{
		    TrackViewState();
		}
		public StateBag StateBag
		{
		    get { return base.ViewState; }
		}
		public HtmlTextWriterTag OnTagKey()
		{
			return  TagKey;
		}
		public void DoOnDataBind(EventArgs e)
		{
		        OnDataBinding(e);
		}
		public void DoOnDataBound (EventArgs e)
		{
			OnDataBound(e);
		}
		public void DoCreateChildControls ()
		{
			CreateChildControls ();
		}
		public void DoEnsureDataBound ()
		{
			EnsureDataBound ();
		}
		public void DoLoadViewState (object state)
		{
			LoadViewState (state);
		}
		public object DoSaveViewState ()
		{
			return SaveViewState ();
		}
		public void DoLoadControlState (object state)
		{
			LoadControlState (state);
		}
		public object DoSaveControlState ()
		{
		       return SaveControlState ();
		}
		public void DoOnMenuItemClick (MenuEventArgs e)
		{
			OnMenuItemClick(e);
		}
		public void DoOnInit(EventArgs e)
		{
			OnInit(e);
		}
		public void DoMenuItemDataBound (MenuEventArgs e)
		{
			OnMenuItemDataBound (e);	
		}
		public void DoOnPreRender (EventArgs e)
		{
			base.OnPreRender(e);
		}
		public bool DoOnBubbleEvent(EventArgs e)
		{
			return base.OnBubbleEvent(this,e);
		}
	}

	[Serializable]
	[TestFixture]
	public class MenuTest
	{	
		[Test]
		public void Menu_DefaultProperties ()
		{
			PokerMenu p = new PokerMenu ();
			Assert.AreEqual ("Click",PokerMenu.MenuItemClickCommandName,"Staic_MenuItemClickCommandName");
			Assert.AreEqual (0, p.Controls.Count,"ControlsCollection");
			Assert.AreEqual (0, p.DataBindings.Count,"DataBindings");
			Assert.AreEqual (500,p.DisappearAfter,"DisappearAfter");
			Assert.AreEqual (string.Empty, p.DynamicBottomSeparatorImageUrl, "DynamicBottomSeparatorImageUrl");
			Assert.IsTrue (p.DynamicEnableDefaultPopOutImage, "DynamicEnableDefaultPopOutImage");
			Assert.AreEqual (0, p.DynamicHorizontalOffset, "DynamicHorizontalOffset");
			Assert.IsNotNull (p.DynamicHoverStyle, "DynamicHoverStyle");
			Assert.AreEqual ("", p.DynamicItemFormatString, "DynamicItemFormatString");
			Assert.IsNull (p.DynamicItemTemplate, "DynamicItemTemplate");
			Assert.IsNotNull (p.DynamicMenuItemStyle, "DynamicMenuItemStyle");
			Assert.IsNotNull (p.DynamicMenuStyle, "DynamicMenuStyle");
			Assert.AreEqual ("Expand {0}", p.DynamicPopOutImageTextFormatString, "DynamicPopOutImageTextFormatString");
			Assert.AreEqual (string.Empty,p.DynamicPopOutImageUrl,"DynamicPopOutImageUrl");
			Assert.IsNotNull (p.DynamicSelectedStyle, "DynamicSelectedStyle");
			Assert.AreEqual (string.Empty, p.DynamicTopSeparatorImageUrl, "DynamicTopSeparatorImageUrl");
			Assert.AreEqual (0, p.DynamicVerticalOffset, "DynamicVerticalOffset");
			Assert.AreEqual (0, p.Items.Count, "Items");
			Assert.AreEqual (false, p.ItemWrap, "ItemWrap");
			Assert.IsNotNull (p.LevelSelectedStyles, "LevelSelectedStyles");
			Assert.IsNotNull (p.LevelSubMenuStyles, "LevelSubMenuStyles");
			Assert.AreEqual (3, p.MaximumDynamicDisplayLevels, "MaximumDynamicDisplayLevels");
			Assert.AreEqual (Orientation.Vertical, p.Orientation, "Orientation");
			Assert.AreEqual ("/", p.PathSeparator.ToString(), "PathSeparator");
			Assert.AreEqual (string.Empty, p.ScrollDownImageUrl, "ScrollDownImageUrl");
			Assert.AreEqual ("Scroll down", p.ScrollDownText, "ScrollDownText");
			Assert.AreEqual ("Scroll up", p.ScrollUpText, "ScrollUpText");
			Assert.AreEqual (null, p.SelectedItem, "p.SelectedItem");
			Assert.AreEqual (string.Empty, p.SelectedValue, "SelectedValue");
			Assert.AreEqual ("Skip Navigation Links", p.SkipLinkText, "SkipLinkText");
			Assert.AreEqual (string.Empty, p.StaticBottomSeparatorImageUrl, "StaticBottomSeparatorImageUrl");
			Assert.AreEqual (1, p.StaticDisplayLevels, "StaticDisplayLevels");
			Assert.AreEqual (true, p.StaticEnableDefaultPopOutImage, "StaticEnableDefaultPopOutImage");
			Assert.IsNotNull (p.StaticHoverStyle, "StaticHoverStyle");
			Assert.AreEqual ("", p.StaticItemFormatString, "StaticItemFormatString");
			Assert.AreEqual (null, p.StaticItemTemplate, "StaticItemTemplate");
			Assert.IsNotNull (p.StaticMenuItemStyle, "StaticMenuItemStyle");
			Assert.IsNotNull (p.StaticMenuStyle, "StaticMenuStyle");
			Assert.AreEqual ("Expand {0}", p.StaticPopOutImageTextFormatString, "StaticPopOutImageTextFormatString");
			Assert.AreEqual ("", p.StaticPopOutImageUrl, "StaticPopOutImageUrl");
			Assert.IsNotNull (p.StaticSelectedStyle, "StaticSelectedStyle");
			Assert.AreEqual (Unit.Pixel(16), p.StaticSubMenuIndent, "StaticSubMenuIndent");
			Assert.AreEqual ("", p.StaticTopSeparatorImageUrl, "StaticTopSeparatorImageUrl");
			Assert.AreEqual ("", p.Target, "Target");
			Assert.IsNotNull (p.OnTagKey (), "TagKey");

		}

		[Test]
		public void Menu_ChangeDefaultProperties ()
		{
			PokerMenu p = new PokerMenu ();
			Button B = new Button ();
			p.Controls.Add (B);
			Assert.AreEqual (1,p.Controls.Count, "ControlsCollection");

			MenuItemBinding M = new MenuItemBinding ();
			M.DataMember = "test";
			M.Depth = 0;
			M.TextField = "title"; 
                        M.NavigateUrl="url";
			Object C = p.DataBindings;
			Assert.AreEqual (0, p.DataBindings.Count, "DataBindings#1");
			((MenuItemBindingCollection)C).Add (M);
			Assert.AreEqual (1,p.DataBindings.Count,"DataBindings#2");

			p.DisappearAfter = 100;
			Assert.AreEqual (100, p.DisappearAfter, "DisappearAfter");

			p.DynamicBottomSeparatorImageUrl = "test.aspx";
			Assert.AreEqual ("test.aspx", p.DynamicBottomSeparatorImageUrl, "DynamicBottomSeparatorImageUrl");

			p.DynamicEnableDefaultPopOutImage = false;
			Assert.AreEqual (false,p.DynamicEnableDefaultPopOutImage, "DynamicEnableDefaultPopOutImage");

			p.DynamicHorizontalOffset = 10;
			Assert.AreEqual (10, p.DynamicHorizontalOffset, "DynamicHorizontalOffset");

			p.DynamicHoverStyle.BackColor = Color.Red;
			Assert.AreEqual (Color.Red ,p.DynamicHoverStyle.BackColor, "DynamicHoverStyle");

			p.DynamicItemFormatString = "Mytest";
			Assert.AreEqual ("Mytest", p.DynamicItemFormatString, "DynamicItemFormatString");

			MyWebControl.Image myImage = new MyWebControl.Image ();
			myImage.ImageUrl = "myimage.jpg";
			ImageTemplate Template = new ImageTemplate ();
			Template.MyImage = myImage;
			// end create template image
			p.DynamicItemTemplate = Template;
			Assert.IsNotNull (p.DynamicItemTemplate, "RootNodeTemplate");
			Assert.AreEqual (typeof (ImageTemplate), p.DynamicItemTemplate.GetType (), "RootNodeTemplate#1");

			p.DynamicMenuItemStyle.BackColor = Color.Red;			
			Assert.AreEqual (Color.Red, p.DynamicMenuItemStyle.BackColor, "DynamicMenuItemStyle");

			p.DynamicMenuStyle.BackColor = Color.Red;
			Assert.AreEqual (Color.Red,p.DynamicMenuStyle.BackColor, "DynamicMenuStyle");

			p.DynamicPopOutImageTextFormatString = "test";
			Assert.AreEqual ("test", p.DynamicPopOutImageTextFormatString, "DynamicPopOutImageTextFormatString");

			p.DynamicPopOutImageUrl = "test";
			Assert.AreEqual ("test", p.DynamicPopOutImageUrl, "DynamicPopOutImageUrl");

			p.DynamicSelectedStyle.BackColor = Color.Red;
			Assert.AreEqual (Color.Red,p.DynamicSelectedStyle.BackColor, "DynamicSelectedStyle");

			p.DynamicTopSeparatorImageUrl = "test";
			Assert.AreEqual ("test", p.DynamicTopSeparatorImageUrl, "DynamicTopSeparatorImageUrl");

			p.DynamicVerticalOffset = 10;
			Assert.AreEqual (10, p.DynamicVerticalOffset, "DynamicVerticalOffset");

			MenuItem I = new MenuItem ();
			I.NavigateUrl = "default.aspx";
			I.Text = "MyText";
			I.ToolTip = "Test";
			p.Items.Add (I);
			Assert.AreEqual (1, p.Items.Count, "Items");

			p.ItemWrap = true;
			Assert.AreEqual (true, p.ItemWrap, "ItemWrap");

			MenuItemStyle S = new MenuItemStyle ();
			S.BackColor = Color.Red;
			p.LevelSelectedStyles.Add (S);
			Assert.AreEqual (1,p.LevelSelectedStyles.Count , "LevelSelectedStyles#1");
			Assert.AreEqual (true, p.LevelSelectedStyles.Contains (S), "LevelSelectedStyles#2");

			SubMenuStyle SM = new SubMenuStyle ();
			SM.BackColor = Color.Red;
			p.LevelSubMenuStyles.Add (SM);
			Assert.AreEqual(1, p.LevelSubMenuStyles.Count, "LevelSubMenuStyles#1");
			Assert.AreEqual (true, p.LevelSubMenuStyles.Contains (SM), "LevelSubMenuStyles#2");

			p.MaximumDynamicDisplayLevels = 5; 
			Assert.AreEqual (5, p.MaximumDynamicDisplayLevels, "MaximumDynamicDisplayLevels");

			p.Orientation = Orientation.Horizontal;
			Assert.AreEqual (Orientation.Horizontal, p.Orientation, "Orientation");

			p.PathSeparator = 'A';
			Assert.AreEqual ('A', p.PathSeparator, "PathSeparator");

			p.ScrollDownImageUrl = "test";
			Assert.AreEqual ("test", p.ScrollDownImageUrl, "ScrollDownImageUrl");

			p.ScrollDownText = "test";
			Assert.AreEqual ("test", p.ScrollDownText, "ScrollDownText");

			p.ScrollUpText = "test";
			Assert.AreEqual ("test", p.ScrollUpText, "ScrollUpText");

			// This properties will be checked in events part of tests
			// Assert.AreEqual (0, p.SelectedItem, "p.SelectedItem");
			// Assert.AreEqual (string.Empty, p.SelectedValue, "SelectedValue");

			p.SkipLinkText = "test";
			Assert.AreEqual ("test", p.SkipLinkText, "SkipLinkText");

			p.StaticBottomSeparatorImageUrl = "test";
			Assert.AreEqual ("test", p.StaticBottomSeparatorImageUrl, "StaticBottomSeparatorImageUrl");

			p.StaticDisplayLevels = 2;
			Assert.AreEqual (2, p.StaticDisplayLevels, "StaticDisplayLevels");

			p.StaticEnableDefaultPopOutImage = false;
			Assert.AreEqual (false, p.StaticEnableDefaultPopOutImage, "StaticEnableDefaultPopOutImage");

			p.StaticHoverStyle.BackColor = Color.Red;
			Assert.AreEqual(Color.Red, p.StaticHoverStyle.BackColor, "StaticHoverStyle");

			p.StaticItemFormatString = "test";
			Assert.AreEqual ("test", p.StaticItemFormatString, "StaticItemFormatString");

			
			p.StaticItemTemplate = Template;
			Assert.IsNotNull (p.StaticItemTemplate, "StaticItemTemplate");

			p.StaticMenuItemStyle.BackColor = Color.Red;
			Assert.AreEqual(Color.Red,p.StaticMenuItemStyle.BackColor, "StaticMenuItemStyle");

			p.StaticMenuStyle.BackColor = Color.Red;
			Assert.AreEqual (Color.Red,p.StaticMenuStyle.BackColor, "StaticMenuStyle");

			p.StaticPopOutImageTextFormatString = "test";
			Assert.AreEqual ("test", p.StaticPopOutImageTextFormatString, "StaticPopOutImageTextFormatString");

			p.StaticPopOutImageUrl = "test";
			Assert.AreEqual ("test", p.StaticPopOutImageUrl, "StaticPopOutImageUrl");

			p.StaticSelectedStyle.BackColor = Color.Red;
			Assert.AreEqual(Color.Red,p.StaticSelectedStyle.BackColor, "StaticSelectedStyle");

			p.StaticSubMenuIndent = 20;
			Assert.AreEqual (Unit.Pixel (20), p.StaticSubMenuIndent, "StaticSubMenuIndent");

			p.StaticTopSeparatorImageUrl = "test";
			Assert.AreEqual ("test", p.StaticTopSeparatorImageUrl, "StaticTopSeparatorImageUrl");

			p.Target = "test";
			Assert.AreEqual ("test", p.Target, "Target");
		}
		[Test]
		public void Menu_StateBag ()
		{
		        PokerMenu p = new PokerMenu ();
		        PokerMenu c = new PokerMenu (); //Only for default property 
			
		        p.DisappearAfter = 100;
		        Assert.AreEqual (1, p.StateBag.Count, "DisappearAfter");
		        Assert.AreEqual (100, p.DisappearAfter, "DisappearAfter");
		        p.DisappearAfter = c.DisappearAfter;
		        Assert.AreEqual (1, p.StateBag.Count, "DisappearAfter"); //Set back to default do not change Statebag state

		        p.DynamicBottomSeparatorImageUrl = "test.aspx";
		        Assert.AreEqual ("test.aspx", p.DynamicBottomSeparatorImageUrl, "DynamicBottomSeparatorImageUrl");
		        Assert.AreEqual (2, p.StateBag.Count, "DynamicBottomSeparatorImageUrl");
		        p.DynamicBottomSeparatorImageUrl = null;
		        Assert.AreEqual (2, p.StateBag.Count, "DynamicBottomSeparatorImageUrl"); //Set back to default do not change Statebag state

		        p.DynamicEnableDefaultPopOutImage = false;
		        Assert.AreEqual (3, p.StateBag.Count, "DynamicEnableDefaultPopOutImage");
		        Assert.AreEqual (false, p.DynamicEnableDefaultPopOutImage, "DynamicEnableDefaultPopOutImage");

		        p.DynamicHorizontalOffset = 10;
		        Assert.AreEqual (4, p.StateBag.Count, "DynamicHorizontalOffset");
		        Assert.AreEqual (10, p.DynamicHorizontalOffset, "DynamicHorizontalOffset");

		        p.DynamicHoverStyle.BackColor = Color.Red;
		        Assert.AreEqual (Color.Red, p.DynamicHoverStyle.BackColor, "DynamicHoverStyle");
		        Assert.AreEqual (4, p.StateBag.Count, "DynamicHoverStyle"); //This property change do not change Statebag

		        p.DynamicItemFormatString = "Mytest";
		        Assert.AreEqual (5, p.StateBag.Count, "DynamicItemFormatString");
		        Assert.AreEqual ("Mytest", p.DynamicItemFormatString, "DynamicItemFormatString");
		        p.DynamicItemFormatString = null;
		        Assert.AreEqual (5, p.StateBag.Count, "DynamicItemFormatString");

		        p.DynamicPopOutImageTextFormatString = "test";
		        Assert.AreEqual (6, p.StateBag.Count, "DynamicPopOutImageTextFormatString");
		        Assert.AreEqual ("test", p.DynamicPopOutImageTextFormatString, "DynamicPopOutImageTextFormatString");
		        p.DynamicPopOutImageTextFormatString = null;
		        p.DynamicPopOutImageTextFormatString = c.DynamicPopOutImageTextFormatString;
		        Assert.AreEqual (6, p.StateBag.Count, "DynamicPopOutImageTextFormatString");

		        p.DynamicPopOutImageUrl = "test";
		        Assert.AreEqual (7, p.StateBag.Count, "DynamicPopOutImageUrl");
		        Assert.AreEqual ("test", p.DynamicPopOutImageUrl, "DynamicPopOutImageUrl");
		        p.DynamicPopOutImageUrl = null;
		        p.DynamicPopOutImageUrl = c.DynamicPopOutImageUrl;
		        Assert.AreEqual (7, p.StateBag.Count, "DynamicPopOutImageUrl");

		        p.DynamicTopSeparatorImageUrl = "test";
		        Assert.AreEqual (8, p.StateBag.Count, "DynamicTopSeparatorImageUrl");
		        Assert.AreEqual ("test", p.DynamicTopSeparatorImageUrl, "DynamicTopSeparatorImageUrl");
		        p.DynamicTopSeparatorImageUrl = null;
		        p.DynamicTopSeparatorImageUrl = c.DynamicPopOutImageUrl;
		        Assert.AreEqual (8, p.StateBag.Count, "DynamicTopSeparatorImageUrl");

		        p.DynamicVerticalOffset = 10;
		        Assert.AreEqual (9, p.StateBag.Count, "DynamicVerticalOffset#1");
		        Assert.AreEqual (10, p.DynamicVerticalOffset, "DynamicVerticalOffset#2");
		        p.DynamicVerticalOffset = c.DynamicVerticalOffset;
		        Assert.AreEqual (9, p.StateBag.Count, "DynamicVerticalOffset#3");

		        p.ItemWrap = true;
		        Assert.AreEqual (true, p.ItemWrap, "ItemWrap#1");
		        Assert.AreEqual (10, p.StateBag.Count, "ItemWrap#2");
		        p.ItemWrap = c.ItemWrap;
		        Assert.AreEqual (10, p.StateBag.Count, "ItemWrap#3");
			
		        p.MaximumDynamicDisplayLevels = 5;
		        Assert.AreEqual (5, p.MaximumDynamicDisplayLevels, "MaximumDynamicDisplayLevels");
		        Assert.AreEqual (11, p.StateBag.Count, "MaximumDynamicDisplayLevels#1");
		        p.MaximumDynamicDisplayLevels = c.MaximumDynamicDisplayLevels;
		        Assert.AreEqual (11, p.StateBag.Count, "MaximumDynamicDisplayLevels#2");

		        p.Orientation = Orientation.Horizontal;
		        Assert.AreEqual (12, p.StateBag.Count, "Orientation#1");
		        Assert.AreEqual (Orientation.Horizontal, p.Orientation, "Orientation");
		        p.Orientation = c.Orientation;
		        Assert.AreEqual (12, p.StateBag.Count, "Orientation#2");

		        p.PathSeparator = 'A';
		        Assert.AreEqual ('A', p.PathSeparator, "PathSeparator");
		        Assert.AreEqual (13, p.StateBag.Count, "PathSeparator#1");
		        p.PathSeparator = c.PathSeparator;
		        Assert.AreEqual (13, p.StateBag.Count, "PathSeparator#2");

		        p.ScrollDownImageUrl = "test";
		        Assert.AreEqual ("test", p.ScrollDownImageUrl, "ScrollDownImageUrl");
		        Assert.AreEqual (14, p.StateBag.Count, "ScrollDownImageUrl#1");
		        p.ScrollDownImageUrl = c.ScrollDownImageUrl;
		        Assert.AreEqual (14, p.StateBag.Count, "ScrollDownImageUrl#3");


		        p.ScrollDownText = "test";
		        Assert.AreEqual ("test", p.ScrollDownText, "ScrollDownText");
		        Assert.AreEqual (15, p.StateBag.Count, "ScrollDownText#1");
		        p.ScrollDownText = c.ScrollDownImageUrl;
		        Assert.AreEqual (15, p.StateBag.Count, "ScrollDownText#2");


		        p.ScrollUpText = "test";
		        Assert.AreEqual ("test", p.ScrollUpText, "ScrollUpText");
		        Assert.AreEqual (16, p.StateBag.Count, "ScrollUpText#1");
		        p.ScrollUpText = c.ScrollDownText;
		        Assert.AreEqual (16, p.StateBag.Count, "ScrollUpText#1");

		        p.SkipLinkText = "test";
		        Assert.AreEqual ("test", p.SkipLinkText, "SkipLinkText");
		        Assert.AreEqual (17, p.StateBag.Count, "SkipLinkText#1");
		        p.SkipLinkText = c.SkipLinkText;
		        Assert.AreEqual (17, p.StateBag.Count, "SkipLinkText#2");
		}
		[Test]
		public void Menu_CreateChildControl ()
		{
		        PokerMenu p = new PokerMenu ();
		        Button B = new Button ();
		        p.Controls.Add (B);
		        Assert.AreEqual (1, p.Controls.Count, "CreateChildControl#1");
		        p.DoCreateChildControls ();
		        Assert.AreEqual (0, p.Controls.Count, "CreateChildControl#2");
		}

	        [Test]
		public void Menu_ControlState()
		{
		        PokerMenu p = new PokerMenu ();
		        MenuItem I1 = new MenuItem ();
		        MenuItem I2 = new MenuItem ();
		        p.Items.Add (I1);
		        p.Items.Add (I2);
		        MenuEventArgs e = new MenuEventArgs (I1);
		        p.DoOnMenuItemClick (e);
		        object state = p.DoSaveControlState ();
		        p.DoLoadControlState (state);
		        e = new MenuEventArgs (I2);
		        p.DoOnMenuItemClick (e);
		        Console.WriteLine();
		}

		[Test]
		public void Menu_FindItem ()
		{
		        PokerMenu p = new PokerMenu ();
		        MenuItem I = new MenuItem ();
		        string path = I.ValuePath;  
		        p.Items.Add (I);
		        MenuItem V = new MenuItem ();
		        I.ChildItems.Add (V);
		        MenuItem copy = p.FindItem (path);
		        Assert.AreEqual (I, copy, "FindItem#1");
		        path = V.ValuePath;
		        Assert.AreEqual (V, p.FindItem (path), "FindItem#2");
		}

		
		 // Set & Get DesignModeState dosn't tested 
		 // Can't test on Page Load event 
		 

	       	[Test]
		[Category ("NunitWeb")]
		[Category ("NotWorking")]  //Must be running after hosting bug resolve
		//[Category ("StucksOnMono")]
		public void Menu_RenderBeginTag ()
		{
			//Thread.Sleep (1000);
		        Helper.Instance.RunInPage(DoTestBeginTagRender, null);
		}
		public static void DoTestBeginTagRender(HttpContext c, Page p, object param)
		{
		        PokerMenu pm = new PokerMenu ();
		        p.Form.Controls.Add (pm);
		        StringWriter sw = new StringWriter ();
		        HtmlTextWriter tw = new HtmlTextWriter (sw);
		        pm.RenderBeginTag (tw);
		        string RenderedControlHtml = sw.ToString();
		        string OriginControlHtml = @"<a href=""#ctl01_SkipLink"">
		                                     <img alt=""Skip Navigation Links"" src=""/NunitWeb/WebResource.axd?d=gZrz8lvSQfolS1pG07HX9g2&amp;t=632784640484505569"" 
		                                      width=""0"" height=""0"" style=""border-width:0px;"" />
		                                     </a><table id=""ctl01"" cellpadding=""0"" cellspacing=""0"" border=""0"">";



		        Assert.AreEqual (true, WebTest.HtmlComparer (OriginControlHtml, RenderedControlHtml), "RenderBeginTag");
			Helper.Unload ();
		}

		[Test]
		[Category ("NunitWeb")]
	        [Category ("NotWorking")]  //Must be running after hosting bug resolve
		//[Category ("StucksOnMono")]
		public void Menu_RenderEndTag ()
		{
			//Thread.Sleep (1000);
		        Helper.Instance.RunInPage (DoTestEndTagRender, null);
		}
		public static void DoTestEndTagRender (HttpContext c, Page p, object param)
		{
		        PokerMenu pm = new PokerMenu ();
		        p.Form.Controls.Add (pm);
		        StringWriter sw = new StringWriter ();
		        HtmlTextWriter tw = new HtmlTextWriter (sw);
		        pm.RenderBeginTag (tw);
		        pm.RenderEndTag (tw);
		        string RenderedControlHtml = sw.ToString ();
		        string OriginControlHtml = @"<a href=""#ctl01_SkipLink"">
		                                     <img alt=""Skip Navigation Links"" src=""/NunitWeb/WebResource.axd?d=gZrz8lvSQfolS1pG07HX9g2&amp;t=632784640484505569""
		                                      width=""0"" height=""0"" style=""border-width:0px;"" />
		                                     </a><table id=""ctl01"" cellpadding=""0"" cellspacing=""0"" border=""0"">
		                                     </table><a id=""ctl01_SkipLink""></a>";



		        Assert.AreEqual (true, WebTest.HtmlComparer (OriginControlHtml, RenderedControlHtml), "RenderEndTag");
			
		}

		[Test]
		public void Menu_ViewState()
		{
		        PokerMenu b = new PokerMenu ();
		        PokerMenu copy = new PokerMenu ();
		        b.ToolTip = "mytest1";
		        b.Target = "mytest2";
		        b.BackColor = Color.Red;
		        b.BorderColor = Color.Red;
		        b.BorderStyle = BorderStyle.Dotted;
		        b.BorderWidth = 1;
		        b.Font.Size = 10;
		        b.ForeColor = Color.Red;
		        b.Height = 100;
		        b.MaximumDynamicDisplayLevels = 2;
		        b.Orientation = Orientation.Vertical;
		        b.PathSeparator = '-';
		        b.ScrollDownImageUrl = "test";
		        b.ScrollDownText = "test";
		        b.ScrollUpImageUrl = "test";
		        b.ScrollUpText = "test";
		        b.SkipLinkText = "test";
		        b.Visible = false;
		        b.Width = 100;
		        b.TabIndex = 1;
			
		        object state = b.DoSaveViewState ();
		        copy.DoLoadViewState (state);
		        Assert.AreEqual ("mytest1", copy.ToolTip, "ViewState#1");
		        Assert.AreEqual ("mytest2", copy.Target, "ViewState#2");
		        Assert.AreEqual (Color.Red, copy.BackColor, "ViewState#3");
		        Assert.AreEqual (Color.Red, copy.BorderColor , "ViewState#4");
		        Assert.AreEqual (BorderStyle.Dotted, copy.BorderStyle, "ViewState#5");
		        Assert.AreEqual (Unit.Pixel(1), copy.BorderWidth, "ViewState#6");
		        Assert.AreEqual ("10pt", copy.Font.Size.ToString() , "ViewState#7");
		        Assert.AreEqual (Color.Red, copy.ForeColor, "ViewState#8");
		        Assert.AreEqual (Unit.Pixel(100), copy.Height, "ViewState#9");
		        Assert.AreEqual (2, copy.MaximumDynamicDisplayLevels, "ViewState#10");
		        Assert.AreEqual (Orientation.Vertical, copy.Orientation, "ViewState#11");
		        Assert.AreEqual ('-', copy.PathSeparator, "ViewState#12");
		        Assert.AreEqual ("test", copy.ScrollDownImageUrl, "ViewState#13");
		        Assert.AreEqual ("test", copy.ScrollDownText, "ViewState#14");
		        Assert.AreEqual ("test", copy.ScrollUpImageUrl, "ViewState#15");
		        Assert.AreEqual ("test", copy.ScrollUpText, "ViewState#16");
		        Assert.AreEqual ("test", copy.SkipLinkText, "ViewState#17");
		        Assert.AreEqual (1, copy.TabIndex, "ViewState#18");
		        Assert.AreEqual (false, copy.Visible, "ViewState#19");
		        Assert.AreEqual (Unit.Pixel (100), copy.Width, "ViewState#20");

		}
		 
		 // Rendering Menu controll with some possible options, styles and items
		 

		[Test]
		[Category ("NunitWeb")]
		[Category ("NotWorking")]  //Must be running after hosting bug resolve
		public void Menu_DefaultRender ()
		{
		        string RenderedPageHtml = Helper.Instance.RunInPage (DoTestDefaultRender, null);
		        string RenderedControlHtml = WebTest.GetControlFromPageHtml (RenderedPageHtml);
		        string OriginControlHtml = "";
		        Assert.AreEqual (true, WebTest.HtmlComparer (OriginControlHtml, RenderedControlHtml), "RenderDefault");
			
		}
	
		 // All this methods are delegates for running tests in host assembly. 
		 
		public static void DoTestDefaultRender (HttpContext c, Page p, object param)
		{
		        LiteralControl lcb = new LiteralControl (WebTest.BEGIN_TAG);
		        LiteralControl lce = new LiteralControl (WebTest.END_TAG);
		        Menu menu = new Menu ();
		        p.Form.Controls.Add (lcb);
		        p.Form.Controls.Add (menu);
		        p.Form.Controls.Add (lce);
		}
	  	[Test]
		[Category ("NunitWeb")]
		[Category ("NotWorking")]  //Must be running after hosting bug resolve
		//[Category ("StucksOnMono")]
		public void Menu_ItemsRender ()
		{
			//Thread.Sleep (1000);
		        string RenderedPageHtml = Helper.Instance.RunInPage (DoTestItemsRender, null);
		        string RenderedControlHtml = WebTest.GetControlFromPageHtml (RenderedPageHtml);
		        string OriginControlHtml = @"<a href=""#ctl01_SkipLink""><img alt=""Skip Navigation Links"" src=""/NunitWeb/WebResource.axd?d=gZrz8lvSQfolS1pG07HX9g2&amp;t=632784640484505569"" width=""0"" height=""0"" border=""0"" />
		                                     </a><table id=""ctl01"" cellpadding=""0"" cellspacing=""0"" border=""0"">
		                                     <tr onmouseover=""Menu_HoverStatic(this)"" onmouseout=""Menu_Unhover(this)"" onkeyup=""Menu_Key(event)"" id=""ctl01n0"">
		                                     <td><table cellpadding=""0"" cellspacing=""0"" border=""0"" width=""100%"">
		                                     <tr>
		                                     <td nowrap=""nowrap"" width=""100%""><a href=""javascript:__doPostBack('ctl01','value1')"">root</a></td><td width=""0"">
		                                     <img src=""/NunitWeb/WebResource.axd?d=jEQEPhExqNH3fus0nmWZ3pFNw-rGIVoBqrGqFcOqB1U1&amp;t=632784640484505569"" alt=""Expand root"" valign=""middle"" /></td>
		                                     </tr>
		                                     </table></td>
		                                     </tr>
		                                     </table><div id=""ctl01n0Items"" style=""display:none;"">
		                                     <table border=""0"" cellpadding=""0"" cellspacing=""0"">
		                                     <tr onmouseover=""Menu_HoverDynamic(this)"" onmouseout=""Menu_Unhover(this)"" onkeyup=""Menu_Key(event)"" id=""ctl01n1"">
		                                     <td><table cellpadding=""0"" cellspacing=""0"" border=""0"" width=""100%"">
		                                     <tr>
		                                     <td nowrap=""nowrap"" width=""100%""><a href=""javascript:__doPostBack('ctl01','value1\\value2')"">node1</a></td>
		                                     </tr>
		                                     </table></td>
		                                     </tr><tr onmouseover=""Menu_HoverDynamic(this)"" onmouseout=""Menu_Unhover(this)"" onkeyup=""Menu_Key(event)"" id=""ctl01n2"">
		                                     <td><table cellpadding=""0"" cellspacing=""0"" border=""0"" width=""100%"">
		                                     <tr>
		                                     <td nowrap=""nowrap"" width=""100%""><a href=""javascript:__doPostBack('ctl01','value1\\value3')"">node2</a></td>
		                                     </tr>
		                                     </table></td>
		                                     </tr>
		                                     </table><div id=""ctl01n0ItemsUp"" onmouseover=""PopOut_Up(this)"" onmouseout=""PopOut_Stop(this)"" align=""center"" style=""display:none;"">
		                                     <img src=""/NunitWeb/WebResource.axd?d=Kql4shtTcfCiKn_s1ZX6W6WIJmS2VsB7hDFw8oD-9I01&amp;t=632784640484505569"" alt=""Scroll up"" />
		                                     </div><div id=""ctl01n0ItemsDn"" onmouseover=""PopOut_Down(this)"" onmouseout=""PopOut_Stop(this)"" align=""center"" style=""display:none;"">
		                                     <img src=""/NunitWeb/WebResource.axd?d=QxI-WSWnY8jfAZsv_BcOLFGj_CTJTI_bGi0dPzQPCtI1&amp;t=632784640484505569"" alt=""Scroll down"" />
		                                     </div>
		                                     </div><a id=""ctl01_SkipLink""></a>";

		        Assert.AreEqual (true, WebTest.HtmlComparer (OriginControlHtml, RenderedControlHtml), "Render Items");
		}

		public static void DoTestItemsRender (HttpContext c, Page p, object param)
		{
		        LiteralControl lcb = new LiteralControl (WebTest.BEGIN_TAG);
		        LiteralControl lce = new LiteralControl (WebTest.END_TAG);
		        Menu menu = new Menu ();
		        MenuItem R = new MenuItem ("root", "value1");
		        MenuItem N1 = new MenuItem ("node1", "value2");
		        MenuItem N2 = new MenuItem ("node2", "value3");
		        R.ChildItems.Add (N1);
		        R.ChildItems.Add (N2);
		        menu.Items.Add (R);
		        p.Form.Controls.Add (lcb);
		        p.Form.Controls.Add (menu);
		        p.Form.Controls.Add (lce);
		}

		 //Events Stuff
		private bool OnDataBinding;
		private bool OnDataBound;
		private bool OnMenuItemClick;
		private bool OnInit;
		private bool OnMenuItemDataBound;
		private bool OnPreRender;
				
		private void OnMenuItemDataBoundHandler(object sender, MenuEventArgs e)
		{
			OnMenuItemDataBound = true;
		}
		private void OnInitHandler (object sender, EventArgs e)
		{
			OnInit = true;
		}
		private void OnDataBindingHandler (object sender, EventArgs e)
		{
			OnDataBinding = true;
		}
		private void OnDataDataBoundHandler (object sender, EventArgs e)
		{
			OnDataBound = true;
		}
		private void OnMenuItemClickHandler (object sender, MenuEventArgs e)
		{
			OnMenuItemClick = true;
		}
		private void OnPreRenderHandler (object sender, EventArgs e)
		{
			OnPreRender = true;
		}
		private void ResetEvents ()
		{
			OnMenuItemClick = false;
			OnDataBinding = false;
			OnDataBound = false;
			OnInit = false;
			OnPreRender = false;
		}
		[Test]
		public void Menu_Events ()
		{
		        Page myPage = new Page ();
		        PokerMenu p = new PokerMenu ();
		        MenuItem I = new MenuItem ();
		        p.Items.Add (I);
		        myPage.Controls.Add(p);

			
		        p.Init += new EventHandler(OnInitHandler); 
		        p.DataBinding += new EventHandler (OnDataBindingHandler);
		        p.DataBound  += new EventHandler(OnDataDataBoundHandler);
		        p.MenuItemClick += new MenuEventHandler(OnMenuItemClickHandler);
		        p.MenuItemDataBound += new MenuEventHandler (OnMenuItemDataBoundHandler);
		        Assert.AreEqual (false, OnDataBinding, "BeforeOnDataBinding");
		        p.DoOnDataBind (new EventArgs ());
		        Assert.AreEqual (true, OnDataBinding, "AfterOnDataBinding");
		        Assert.AreEqual (false, OnDataBound, "BeforeOnDataBound");
		        p.DoOnDataBound (new EventArgs ());
		        Assert.AreEqual (true, OnDataBound, "AfterOnDataBinding");
		        MenuEventArgs e = new MenuEventArgs (I);
		        Assert.AreEqual (false, OnMenuItemClick, "BeforeMenuItemClick");
		        p.DoOnMenuItemClick (e);
		        Assert.AreEqual (true, OnMenuItemClick, "AfterMenuItemClick");
		        Assert.AreEqual (false, OnInit, "BeforeOnInit");
		        p.DoOnInit (new EventArgs());
		        Assert.AreEqual (true, OnInit, "AfterOnInit");
		        Assert.AreEqual (false, OnMenuItemDataBound, "BeforeMenuItemDataBound");
		        p.DoMenuItemDataBound(e);
		        Assert.AreEqual (true, OnMenuItemDataBound, "AfterMenuItemDataBound");
		}
		[Test]
		public void Menu_BubbleEvent()
		{
		        PokerMenu pm = new PokerMenu ();
		        CommandEventArgs commandarg = new CommandEventArgs (Menu.MenuItemClickCommandName, null);
		        Assert.AreEqual (true, pm.DoOnBubbleEvent (commandarg), "Bubble Event#1");
		        Assert.AreEqual (false, pm.DoOnBubbleEvent (new EventArgs ()), "Bubble Event#2");
		}
		[Test]
		[Category ("NunitWeb")]
		// [Category ("NotWorking")]  //Must be running after hosting bug resolve
		public void Menu_PreRenderEvent ()
		{
		        Helper.Instance.RunInPage (PreRenderEvent, null);
		}
		public void PreRenderEvent (HttpContext c, Page p, object param)
		{
		        PokerMenu pm = new PokerMenu ();
		        p.Controls.Add (pm);
		        pm.PreRender += new EventHandler (OnPreRenderHandler);
		        Assert.AreEqual (false, OnPreRender, "BeforePreRender");
		        pm.DoOnPreRender (new EventArgs ());
		        Assert.AreEqual (true, OnPreRender, "AfterPreRender");
		}
		[TestFixtureTearDown]
		public void TearDown ()
		{
		        Helper.Unload ();
		}
	
		// A simple Template class to wrap an image.
		public class ImageTemplate : ITemplate
		{
			private MyWebControl.Image myImage;
			public MyWebControl.Image MyImage
			{
				get
				{
					return myImage;
				}
				set
				{
					myImage = value;
				}
			}
			public void InstantiateIn (Control container)
			{
				container.Controls.Add (MyImage);
			}
		}
	}
}
#endif
