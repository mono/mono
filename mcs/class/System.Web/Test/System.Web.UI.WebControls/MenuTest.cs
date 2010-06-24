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
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using System.Drawing;
using System.Threading;
using MyWebControl = System.Web.UI.WebControls;
using System.Collections;
using MonoTests.SystemWeb.Framework;
using MonoTests.stand_alone.WebHarness;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Xml;

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

		[TestFixtureSetUp]
		public void SetUp ()
		{
			WebTest.CopyResource (GetType (), "menuclass.aspx", "menuclass.aspx");
			WebTest.CopyResource (GetType (), "PostBackMenuTest.aspx", "PostBackMenuTest.aspx");
		}

		[Test]
		public void Menu_DefaultProperties ()
		{
			PokerMenu p = new PokerMenu ();
			Assert.AreEqual ("Click",PokerMenu.MenuItemClickCommandName,"Staic_MenuItemClickCommandName");
			Assert.AreEqual (0,p.Controls.Count,"ControlsCollection");
			Assert.AreEqual (0,p.DataBindings.Count,"DataBindings");
			Assert.AreEqual (500,p.DisappearAfter,"DisappearAfter");
			Assert.AreEqual (string.Empty, p.DynamicBottomSeparatorImageUrl, "DynamicBottomSeparatorImageUrl");
			Assert.IsTrue (p.DynamicEnableDefaultPopOutImage, "DynamicEnableDefaultPopOutImage");
			Assert.AreEqual (0, p.DynamicHorizontalOffset, "DynamicHorizontalOffset");
			Assert.IsNotNull (p.DynamicHoverStyle, "DynamicHoverStyle");
			Assert.AreEqual ("", p.DynamicItemFormatString, "DynamicItemFormatString");
			Assert.IsNull (p.DynamicItemTemplate, "DynamicItemTemplate");
			Assert.IsNotNull (p.DynamicMenuItemStyle, "DynamicMenuItemStyle");
			Assert.IsNotNull (p.DynamicMenuStyle, "DynamicMenuStyle");
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
			Assert.AreEqual (null, p.SelectedItem, "p.SelectedItem");
			Assert.AreEqual (string.Empty, p.StaticBottomSeparatorImageUrl, "StaticBottomSeparatorImageUrl");
			Assert.AreEqual (1, p.StaticDisplayLevels, "StaticDisplayLevels");
			Assert.AreEqual (true, p.StaticEnableDefaultPopOutImage, "StaticEnableDefaultPopOutImage");
			Assert.IsNotNull (p.StaticHoverStyle, "StaticHoverStyle");
			Assert.AreEqual ("", p.StaticItemFormatString, "StaticItemFormatString");
			Assert.AreEqual (null, p.StaticItemTemplate, "StaticItemTemplate");
			Assert.IsNotNull (p.StaticMenuItemStyle, "StaticMenuItemStyle");
			Assert.IsNotNull (p.StaticMenuStyle, "StaticMenuStyle");
			Assert.AreEqual ("", p.StaticPopOutImageUrl, "StaticPopOutImageUrl");
			Assert.IsNotNull (p.StaticSelectedStyle, "StaticSelectedStyle");
			Assert.AreEqual (Unit.Pixel(16), p.StaticSubMenuIndent, "StaticSubMenuIndent");
			Assert.AreEqual ("", p.StaticTopSeparatorImageUrl, "StaticTopSeparatorImageUrl");
			Assert.AreEqual ("", p.Target, "Target");
			Assert.IsNotNull (p.OnTagKey (), "TagKey");

		}

		[Test]
		public void Menu_DefaultProperties_2 ()
		{
			PokerMenu p = new PokerMenu ();
			Assert.AreEqual ("Skip Navigation Links", p.SkipLinkText, "SkipLinkText");
			Assert.AreEqual (string.Empty, p.SelectedValue, "SelectedValue");
			Assert.AreEqual ("Scroll up", p.ScrollUpText, "ScrollUpText");
			Assert.AreEqual ("Expand {0}", p.StaticPopOutImageTextFormatString, "StaticPopOutImageTextFormatString"); //not implemented
			Assert.AreEqual ("Scroll down", p.ScrollDownText, "ScrollDownText");
			Assert.AreEqual ("Expand {0}", p.DynamicPopOutImageTextFormatString, "DynamicPopOutImageTextFormatString"); //not implemented 
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
		public void Menu_ChangeDefaultProperties_2 ()
		{
			PokerMenu p = new PokerMenu ();
			p.ScrollUpText = "test";
			Assert.AreEqual ("test", p.ScrollUpText, "ScrollUpText");
			p.DynamicPopOutImageTextFormatString = "test";
			Assert.AreEqual ("test", p.DynamicPopOutImageTextFormatString, "DynamicPopOutImageTextFormatString");
			p.StaticPopOutImageTextFormatString = "test";
			Assert.AreEqual ("test", p.StaticPopOutImageTextFormatString, "StaticPopOutImageTextFormatString");
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
		public void Menu_RenderBeginTag ()
		{
			new WebTest (PageInvoker.CreateOnLoad (_BeginTagRender)).Run ();
		}

		public static void _BeginTagRender(Page p)
		{
			PokerMenu pm = new PokerMenu ();
			p.Form.Controls.Add (pm);
			StringWriter sw = new StringWriter ();
			HtmlTextWriter tw = new HtmlTextWriter (sw);
			pm.RenderBeginTag (tw);
			string RenderedControlHtml = sw.ToString();
			string OriginControlHtml = "<a href=\"#ctl01_SkipLink\"><img alt=\"Skip Navigation Links\" src=\"/NunitWeb/WebResource.axd?d=4RHYfeNnynkXiM59uthjZg2&amp;t=633802729995006876\" width=\"0\" height=\"0\" style=\"border-width:0px;\" /></a><table id=\"ctl01\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\">\r\n";

			HtmlDiff.AssertAreEqual(OriginControlHtml,RenderedControlHtml,"RenderBeginTag");
		}

		[Test]
		[Category ("NunitWeb")]
		public void Menu_RenderEndTag ()
		{
			new WebTest (PageInvoker.CreateOnLoad (_EndTagRender)).Run ();
		}
		public static void _EndTagRender (Page p)
		{
			PokerMenu pm = new PokerMenu ();
			p.Form.Controls.Add (pm);
			StringWriter sw = new StringWriter ();
			HtmlTextWriter tw = new HtmlTextWriter (sw);
			pm.RenderBeginTag (tw);
			pm.RenderEndTag (tw);
			string RenderedControlHtml = sw.ToString ();
			string OriginControlHtml = "<a href=\"#ctl01_SkipLink\"><img alt=\"Skip Navigation Links\" src=\"/NunitWeb/WebResource.axd?d=4RHYfeNnynkXiM59uthjZg2&amp;t=633802729995006876\" width=\"0\" height=\"0\" style=\"border-width:0px;\" /></a><table id=\"ctl01\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\">\r\n\r\n</table><a id=\"ctl01_SkipLink\"></a>";

			HtmlDiff.AssertAreEqual(OriginControlHtml, RenderedControlHtml,"RenderEndTag");	
		}

		[Test]
		public void Menu_DataBind () {
			Page p = new Page ();
			Menu m = CreateMenu ();
			m.DataBinding += new EventHandler (m_DataBinding);
			m.DataBound += new EventHandler (m_DataBound);
			p.Controls.Add (m);

			ResetTemplateBoundFlags ();
			m.DataBind ();
		}

		static void m_DataBinding (object sender, EventArgs e) {
			Assert.AreEqual (true, _StaticTemplateCreated, "StaticTemplateCreated");
			Assert.AreEqual (true, _DynamicTemplateCreated, "DynamicTemplateCreated");
		}
		
		static void m_DataBound (object sender, EventArgs e) {
			Assert.AreEqual (true, _StaticTemplateBound, "StaticTemplateBound");
			Assert.AreEqual (true, _DynamicTemplateBound, "DynamicTemplateBound");
		}

		private static void ResetTemplateBoundFlags() {
			_StaticTemplateBound = false;
			_DynamicTemplateBound = false;
			_StaticTemplateCreated = false;
			_DynamicTemplateCreated = false;
		}

		static Menu CreateMenu () {
			Menu m = new Menu ();
			MenuItem rootItem = new MenuItem ("RootItem-Text", "RootItem-Value");
			m.Items.Add (rootItem);
			rootItem.ChildItems.Add (new MenuItem ("Node1-Text", "Node1-Value"));
			rootItem.ChildItems.Add (new MenuItem ("Node2-Text", "Node2-Value"));
			m.StaticItemTemplate = new CompiledTemplateBuilder (_StaticItemTemplate);
			m.DynamicItemTemplate = new CompiledTemplateBuilder (_DynamicItemTemplate);
			return m;
		}

		[Test]
		[Category ("NunitWeb")]
		public void Menu_DataBindByDataSourceID () {
			PageDelegates pd = new PageDelegates ();
			pd.Init = Menu_DataBindByDataSourceID_PageInit;
			pd.PreRenderComplete = Menu_DataBindByDataSourceID_PagePreRenderComplete;
			PageInvoker pi = new PageInvoker (pd);
			new WebTest (pi).Run ();
		}

		public static void Menu_DataBindByDataSourceID_PageInit (Page p) {
			XmlDataSource xmlDs = new XmlDataSource ();
			xmlDs.ID = "XmlDataSource";
			xmlDs.Data = "<root><node /><node /><node><subnode /><subnode /></node></root>";
			p.Form.Controls.Add (xmlDs);

			Menu m = CreateMenu ();
			m.DataSourceID = "XmlDataSource";
			m.MenuItemDataBound += new MenuEventHandler (m_MenuItemDataBound);
			p.Form.Controls.Add (m);

			ResetTemplateBoundFlags ();
			_MenuItemBoundCount = 0;
			_MenuItemCreatedCount = 0;
		}

		public static void m_MenuItemDataBound (object sender, MenuEventArgs e) {
			_MenuItemBoundCount++;
		}

		[Test]
		[Category ("NunitWeb")]
		public void Menu_Templates () {
			PageDelegates pd = new PageDelegates ();
			pd.Init = Menu_Templates_PageInit;
			pd.PreRenderComplete = Menu_Templates_PagePreRenderComplete;
			PageInvoker pi = new PageInvoker (pd);
			new WebTest (pi).Run ();
		}

		public static void Menu_Templates_PageInit (Page p) {
			Menu m = CreateMenu ();
			p.Form.Controls.Add (m);

			ResetTemplateBoundFlags ();
		}

		static bool _StaticTemplateBound;
		static bool _DynamicTemplateBound;

		static bool _StaticTemplateCreated;
		static bool _DynamicTemplateCreated;

		static int _MenuItemBoundCount;
		static int _MenuItemCreatedCount;

		private static void CheckTemplateBoundFlags () {
			Assert.AreEqual (true, _StaticTemplateCreated, "StaticTemplateCreated");
			Assert.AreEqual (true, _DynamicTemplateCreated, "DynamicTemplateCreated");
			Assert.AreEqual (true, _StaticTemplateBound, "StaticTemplateBound");
			Assert.AreEqual (true, _DynamicTemplateBound, "DynamicTemplateBound");
		}

		public static void Menu_Templates_PagePreRenderComplete (Page p) {
			CheckTemplateBoundFlags ();
		}

		public static void Menu_DataBindByDataSourceID_PagePreRenderComplete (Page p) {
			CheckTemplateBoundFlags ();
			Assert.AreEqual (6, _MenuItemBoundCount, "MenuItemBoundCount");
			Assert.AreEqual (6, _MenuItemCreatedCount, "MenuItemBoundCount");
		}

		private static void _StaticItemTemplate (Control container) {
			_StaticTemplateCreated = true;
			_MenuItemCreatedCount++;
			Literal l = new Literal ();
			container.Controls.Add (l);
			container.DataBinding += new EventHandler (StaticTemplate_DataBinding);
		}

		static void StaticTemplate_DataBinding (object sender, EventArgs e) {
			_StaticTemplateBound = true;
		}

		private static void _DynamicItemTemplate (Control container) {
			_DynamicTemplateCreated = true;
			_MenuItemCreatedCount++;
			Literal l = new Literal ();
			container.Controls.Add (l);
			container.DataBinding += new EventHandler (DynamicTemplate_DataBinding);
		}

		static void DynamicTemplate_DataBinding (object sender, EventArgs e) {
			_DynamicTemplateBound = true;
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

		[Test]
		public void Menu_ViewStateNotWorking()
		{
			PokerMenu b = new PokerMenu ();
			PokerMenu copy = new PokerMenu ();
			b.Font.Size = 10;
			object state = b.DoSaveViewState ();
			copy.DoLoadViewState (state);
			Assert.AreEqual ("10pt", copy.Font.Size.ToString() , "ViewState#7");			
		}

		[Test]
		public void Menu_ViewStateItems () {
			PokerMenu b = new PokerMenu ();
			MenuItem R = new MenuItem ("root", "value-root");
			MenuItem N1 = new MenuItem ("node1", "value-node1");
			MenuItem N2 = new MenuItem ("node2", "value-node2");
			R.ChildItems.Add (N1);
			R.ChildItems.Add (N2);
			b.Items.Add (R);
			PokerMenu copy = new PokerMenu ();
			object state = b.DoSaveViewState ();
			copy.DoLoadViewState (state);
			Assert.AreEqual (1, copy.Items.Count, "ViewStateItems#1");
			Assert.AreEqual (2, copy.Items [0].ChildItems.Count, "ViewStateItems#2");
			Assert.AreEqual (0, copy.Items [0].ChildItems [0].ChildItems.Count, "ViewStateItems#3");
			Assert.AreEqual ("node1", copy.Items [0].ChildItems [0].Text, "ViewStateItems#4");
			Assert.AreEqual ("value-node1", copy.Items [0].ChildItems [0].Value, "ViewStateItems#5");
			Assert.AreEqual (false, copy.Items [0].ChildItems [0].DataBound, "ViewStateItems#6");
			Assert.AreEqual ("", copy.Items [0].ChildItems [0].DataPath, "ViewStateItems#7");
		}


		[Test]
		public void Menu_ViewStateDataBoundItems () {
			PokerMenu b = new PokerMenu ();
			SetDataBindings (b);
			b.DataSource = CreateXmlDataSource ();
			b.DataBind ();
			PokerMenu copy = new PokerMenu ();
			object state = b.DoSaveViewState ();
			copy.DoLoadViewState (state);
			CheckMenuItems (copy);
		}

		private static void CheckMenuItems (Menu m) {
			Assert.AreEqual (1, m.Items.Count, "CheckMenuItems#1");
			Assert.AreEqual (10, m.Items [0].ChildItems.Count, "CheckMenuItems#2");
			Assert.AreEqual (0, m.Items [0].ChildItems [0].ChildItems.Count, "CheckMenuItems#3");
			Assert.AreEqual (true, m.Items [0].ChildItems [0].DataBound, "CheckMenuItems#4");
			Assert.AreEqual ("/*[position()=1]/*[position()=1]", m.Items [0].ChildItems [0].DataPath, "CheckMenuItems#5");

			Assert.AreEqual (false, m.Items [0].Enabled, "CheckMenuItems_Enabled#1");
			Assert.AreEqual (true, m.Items [0].ChildItems [0].Enabled, "CheckMenuItems_Enabled#2");
			Assert.AreEqual (false, m.Items [0].ChildItems [1].Enabled, "CheckMenuItems_Enabled#3");
			Assert.AreEqual (false, m.Items [0].ChildItems [2].Enabled, "CheckMenuItems_Enabled#4");
			Assert.AreEqual (true, m.Items [0].ChildItems [2].ChildItems [0].Enabled, "CheckMenuItems_Enabled#5");

			Assert.AreEqual ("img#root", m.Items [0].ImageUrl, "CheckMenuItems_ImageUrl#1");
			Assert.AreEqual ("img#1", m.Items [0].ChildItems [0].ImageUrl, "CheckMenuItems_ImageUrl#2");
			Assert.AreEqual ("img#2", m.Items [0].ChildItems [1].ImageUrl, "CheckMenuItems_ImageUrl#3");
			Assert.AreEqual ("img#default", m.Items [0].ChildItems [2].ImageUrl, "CheckMenuItems_ImageUrl#4");
			Assert.AreEqual ("", m.Items [0].ChildItems [2].ChildItems [0].ImageUrl, "CheckMenuItems_ImageUrl#5");

			Assert.AreEqual ("url#root", m.Items [0].NavigateUrl, "CheckMenuItems_NavigateUrl#1");
			Assert.AreEqual ("url#1", m.Items [0].ChildItems [0].NavigateUrl, "CheckMenuItems_NavigateUrl#2");
			Assert.AreEqual ("url#2", m.Items [0].ChildItems [1].NavigateUrl, "CheckMenuItems_NavigateUrl#3");
			Assert.AreEqual ("url#default", m.Items [0].ChildItems [2].NavigateUrl, "CheckMenuItems_NavigateUrl#4");
			Assert.AreEqual ("", m.Items [0].ChildItems [2].ChildItems [0].NavigateUrl, "CheckMenuItems_NavigateUrl#5");

			Assert.AreEqual ("popoutimg#root", m.Items [0].PopOutImageUrl, "CheckMenuItems_PopOutImageUrl#1");
			Assert.AreEqual ("popoutimg#1", m.Items [0].ChildItems [0].PopOutImageUrl, "CheckMenuItems_PopOutImageUrl#2");
			Assert.AreEqual ("popoutimg#2", m.Items [0].ChildItems [1].PopOutImageUrl, "CheckMenuItems_PopOutImageUrl#3");
			Assert.AreEqual ("popoutimg#default", m.Items [0].ChildItems [2].PopOutImageUrl, "CheckMenuItems_PopOutImageUrl#4");
			Assert.AreEqual ("", m.Items [0].ChildItems [2].ChildItems [0].PopOutImageUrl, "CheckMenuItems_PopOutImageUrl#5");

			Assert.AreEqual (false, m.Items [0].Selectable, "CheckMenuItems_Selectable#1");
			Assert.AreEqual (true, m.Items [0].ChildItems [0].Selectable, "CheckMenuItems_Selectable#2");
			Assert.AreEqual (false, m.Items [0].ChildItems [1].Selectable, "CheckMenuItems_Selectable#3");
			Assert.AreEqual (false, m.Items [0].ChildItems [2].Selectable, "CheckMenuItems_Selectable#4");
			Assert.AreEqual (true, m.Items [0].ChildItems [2].ChildItems [0].Selectable, "CheckMenuItems_Selectable#5");

			Assert.AreEqual ("separatorimg#root", m.Items [0].SeparatorImageUrl, "CheckMenuItems_SeparatorImageUrl#1");
			Assert.AreEqual ("separatorimg#1", m.Items [0].ChildItems [0].SeparatorImageUrl, "CheckMenuItems_SeparatorImageUrl#2");
			Assert.AreEqual ("separatorimg#2", m.Items [0].ChildItems [1].SeparatorImageUrl, "CheckMenuItems_SeparatorImageUrl#3");
			Assert.AreEqual ("separatorimg#default", m.Items [0].ChildItems [2].SeparatorImageUrl, "CheckMenuItems_SeparatorImageUrl#4");
			Assert.AreEqual ("", m.Items [0].ChildItems [2].ChildItems [0].SeparatorImageUrl, "CheckMenuItems_SeparatorImageUrl#5");

			Assert.AreEqual ("target#root", m.Items [0].Target, "CheckMenuItems_Target#1");
			Assert.AreEqual ("target#1", m.Items [0].ChildItems [0].Target, "CheckMenuItems_Target#2");
			Assert.AreEqual ("target#2", m.Items [0].ChildItems [1].Target, "CheckMenuItems_Target#3");
			Assert.AreEqual ("target#default", m.Items [0].ChildItems [2].Target, "CheckMenuItems_Target#4");
			Assert.AreEqual ("", m.Items [0].ChildItems [2].ChildItems [0].Target, "CheckMenuItems_Target#5");

			Assert.AreEqual ("text#root", m.Items [0].Text, "CheckMenuItems_Text#1");
			Assert.AreEqual ("text#1", m.Items [0].ChildItems [0].Text, "CheckMenuItems_Text#2");
			Assert.AreEqual ("text#2", m.Items [0].ChildItems [1].Text, "CheckMenuItems_Text#3");
			Assert.AreEqual ("text#", m.Items [0].ChildItems [2].Text, "CheckMenuItems_Text#4");
			Assert.AreEqual ("subnode", m.Items [0].ChildItems [2].ChildItems [0].Text, "CheckMenuItems_Text#5");

			Assert.AreEqual ("tooltip#root", m.Items [0].ToolTip, "CheckMenuItems_ToolTip#1");
			Assert.AreEqual ("tooltip#1", m.Items [0].ChildItems [0].ToolTip, "CheckMenuItems_ToolTip#2");
			Assert.AreEqual ("tooltip#2", m.Items [0].ChildItems [1].ToolTip, "CheckMenuItems_ToolTip#3");
			Assert.AreEqual ("tooltip#default", m.Items [0].ChildItems [2].ToolTip, "CheckMenuItems_ToolTip#4");
			Assert.AreEqual ("", m.Items [0].ChildItems [2].ChildItems [0].ToolTip, "CheckMenuItems_ToolTip#5");

			Assert.AreEqual ("value#root", m.Items [0].Value, "CheckMenuItems_Value#1");
			Assert.AreEqual ("value#1", m.Items [0].ChildItems [0].Value, "CheckMenuItems_Value#2");
			Assert.AreEqual ("value#2", m.Items [0].ChildItems [1].Value, "CheckMenuItems_Value#3");
			Assert.AreEqual ("value#default", m.Items [0].ChildItems [2].Value, "CheckMenuItems_Value#4");
			Assert.AreEqual ("subnode", m.Items [0].ChildItems [2].ChildItems [0].Value, "CheckMenuItems_Value#5");

			Assert.AreEqual ("text#extra1", m.Items [0].ChildItems [3].Text, "CheckMenuItems_Extra1#1");
			Assert.AreEqual ("text#extra1", m.Items [0].ChildItems [3].Value, "CheckMenuItems_Extra1#2");

			Assert.AreEqual ("value#extra2", m.Items [0].ChildItems [4].Text, "CheckMenuItems_Extra2#1");
			Assert.AreEqual ("value#extra2", m.Items [0].ChildItems [4].Value, "CheckMenuItems_Extra2#2");

			Assert.AreEqual ("text#extra3", m.Items [0].ChildItems [5].Text, "CheckMenuItems_Extra3#1");
			Assert.AreEqual ("", m.Items [0].ChildItems [5].Value, "CheckMenuItems_Extra3#2");
			Assert.AreEqual ("", m.Items [0].ChildItems [6].Text, "CheckMenuItems_Extra3#3");
			Assert.AreEqual ("value#extra3", m.Items [0].ChildItems [6].Value, "CheckMenuItems_Extra3#4");
			Assert.AreEqual ("", m.Items [0].ChildItems [7].Text, "CheckMenuItems_Extra3#5");
			Assert.AreEqual ("", m.Items [0].ChildItems [7].Value, "CheckMenuItems_Extra3#6");

			Assert.AreEqual ("text#extra4", m.Items [0].ChildItems [8].Text, "CheckMenuItems_Extra4#1");
			Assert.AreEqual ("text#default", m.Items [0].ChildItems [8].Value, "CheckMenuItems_Extra4#2");

			Assert.AreEqual ("value#default", m.Items [0].ChildItems [9].Text, "CheckMenuItems_Extra5#1");
			Assert.AreEqual ("value#extra5", m.Items [0].ChildItems [9].Value, "CheckMenuItems_Extra5#2");
		}

		void SetDataBindings (Menu menu) {
			MenuItemBinding b = new MenuItemBinding ();
			b.DataMember = "node";
			b.EnabledField = "enabled";
			b.Enabled = false;
			b.ImageUrlField = "img";
			b.ImageUrl = "img#default";
			b.NavigateUrlField = "url";
			b.NavigateUrl = "url#default";
			b.PopOutImageUrlField = "popoutimg";
			b.PopOutImageUrl = "popoutimg#default";
			b.SelectableField = "selectable";
			b.Selectable = false;
			b.SeparatorImageUrlField = "separatorimg";
			b.SeparatorImageUrl = "separatorimg#default";
			b.TargetField = "target";
			b.Target = "target#default";
			b.FormatString = "text#{0}";
			b.TextField = "text";
			b.Text = "text#default";
			b.ToolTipField = "tooltip";
			b.ToolTip = "tooltip#default";
			b.ValueField = "value";
			b.Value = "value#default";
			menu.DataBindings.Add (b);

			b = new MenuItemBinding ();
			b.DataMember = "root";
			b.Enabled = false;
			b.ImageUrl = "img#root";
			b.NavigateUrl = "url#root";
			b.PopOutImageUrl = "popoutimg#root";
			b.Selectable = false;
			b.SeparatorImageUrl = "separatorimg#root";
			b.Target = "target#root";
			b.Text = "text#root";
			b.ToolTip = "tooltip#root";
			b.Value = "value#root";
			menu.DataBindings.Add (b);

			b = new MenuItemBinding ();
			b.DataMember = "extra1";
			b.Text = "text#extra1";
			menu.DataBindings.Add (b);

			b = new MenuItemBinding ();
			b.DataMember = "extra2";
			b.Value = "value#extra2";
			menu.DataBindings.Add (b);

			b = new MenuItemBinding ();
			b.DataMember = "extra3";
			b.TextField = "text";
			b.ValueField = "value";
			menu.DataBindings.Add (b);

			b = new MenuItemBinding ();
			b.DataMember = "extra4";
			b.TextField = "text";
			b.Text = "text#default";
			b.ValueField = "value";
			menu.DataBindings.Add (b);

			b = new MenuItemBinding ();
			b.DataMember = "extra5";
			b.TextField = "text";
			b.Value = "value#default";
			b.ValueField = "value";
			menu.DataBindings.Add (b);
		}

		XmlDataSource CreateXmlDataSource () {
			XmlDataSource xmlDs = new XmlDataSource ();
			xmlDs.ID = "XmlDataSource";
			xmlDs.Data = "<root>"+
				"<node url=\"url#1\" img=\"img#1\" enabled=\"true\" selectable=\"true\" popoutimg=\"popoutimg#1\" separatorimg=\"separatorimg#1\" target=\"target#1\" text=\"1\" tooltip=\"tooltip#1\" value=\"value#1\" />" +
				"<node url=\"url#2\" img=\"img#2\" enabled=\"false\" selectable=\"false\" popoutimg=\"popoutimg#2\" separatorimg=\"separatorimg#2\" target=\"target#2\" text=\"2\" tooltip=\"tooltip#2\" value=\"value#2\" />" +
				"<node url=\"\" img=\"\" enabled=\"\" selectable=\"\" popoutimg=\"\" separatorimg=\"\" target=\"\" text=\"\" tooltip=\"\" value=\"\">" +
				"<subnode url=\"url#unreachable\" img=\"img#unreachable\" enabled=\"false\" selectable=\"false\" popoutimg=\"popoutimg#unreachable\" separatorimg=\"separatorimg#unreachable\" target=\"target#unreachable\" text=\"text#unreachable\" tooltip=\"tooltip#unreachable\" value=\"value#unreachable\" />" +
				"<subnode /></node>"+
				"<extra1 /><extra2 />"+
				"<extra3 text=\"text#extra3\" value=\"\" />" +
				"<extra3 text=\"\" value=\"value#extra3\" />" +
				"<extra3 text=\"\" value=\"\" />" +
				"<extra4 text=\"text#extra4\" value=\"\" />" +
				"<extra5 text=\"\" value=\"value#extra5\" />" +
				"</root>";
			return xmlDs;
		}

		[Test]
		public void Menu_DataBindings () {
			Menu m = new Menu ();
			SetDataBindings (m);
			m.DataSource = CreateXmlDataSource ();
			m.DataBind ();
			CheckMenuItems (m);
		}

		// Rendering Menu controll with some possible options, styles and items
		 

		[Test]
		[Category ("NunitWeb")]
		[Ignore ("NUNIT 2.4 issue - temporarily disabled")]
		public void Menu_DefaultRender ()
		{
			string RenderedPageHtml = new WebTest (PageInvoker.CreateOnLoad (_DefaultRender)).Run ();
			string RenderedControlHtml = HtmlDiff.GetControlFromPageHtml (RenderedPageHtml);
			string OriginControlHtml = "";
			HtmlDiff.AssertAreEqual(OriginControlHtml, RenderedControlHtml,"RenderDefault");
		}
	
		 // All this methods are delegates for running tests in host assembly. 
		 
		public static void _DefaultRender (Page p)
		{
			LiteralControl lcb = new LiteralControl (HtmlDiff.BEGIN_TAG);
			LiteralControl lce = new LiteralControl (HtmlDiff.END_TAG);
			Menu menu = new Menu ();
			p.Form.Controls.Add (lcb);
			p.Form.Controls.Add (menu);
			p.Form.Controls.Add (lce);
		}

		[Test]
		[Category ("NunitWeb")]
		public void Menu_RenderStaticItems () {
			string RenderedPageHtml, RenderedControlHtml, OriginControlHtml;
			#region OriginControlHtml
			OriginControlHtml = @"<a href=""#Menu_SkipLink""><img alt=""Skip Navigation Links"" src=""/NunitWeb/WebResource.axd?d=QxfUEifeQdL5PTiZOF8HlA2&amp;t=632900536532114160"" width=""0"" height=""0"" style=""border-width:0px;"" /></a><table id=""Menu"" class=""Menu_2"" cellpadding=""0"" cellspacing=""0"" border=""0"">
	<tr onmouseover=""Menu_HoverRoot(this)"" onmouseout=""Menu_Unhover(this)"" onkeyup=""Menu_Key(this)"" id=""Menun0"">
		<td><table cellpadding=""0"" cellspacing=""0"" border=""0"" width=""100%"">
			<tr>
				<td style=""white-space:nowrap;width:100%;""><a class=""Menu_1"" href=""javascript:__doPostBack('Menu','one-black-value')"">one-black</a></td>
			</tr>
		</table></td>
	</tr><tr>
		<td><table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"">
			<tr onmouseover=""Menu_HoverRoot(this)"" onmouseout=""Menu_Unhover(this)"" onkeyup=""Menu_Key(this)"" id=""Menun1"">
				<td><table cellpadding=""0"" cellspacing=""0"" border=""0"" width=""100%"">
					<tr>
						<td style=""white-space:nowrap;width:100%;""><a class=""Menu_1"" href=""javascript:__doPostBack('Menu','one-black-value\\two-black-1-value')"" style=""margin-left:16px;"">two-black-1</a></td>
					</tr>
				</table></td>
			</tr><tr>
				<td><table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"">
					<tr onmouseover=""Menu_HoverRoot(this)"" onmouseout=""Menu_Unhover(this)"" onkeyup=""Menu_Key(this)"" id=""Menun2"">
						<td><table cellpadding=""0"" cellspacing=""0"" border=""0"" width=""100%"">
							<tr>
								<td style=""white-space:nowrap;width:100%;""><a class=""Menu_1"" href=""javascript:__doPostBack('Menu','one-black-value\\two-black-1-value\\three-black-1-value')"" style=""margin-left:32px;"">three-black-1</a></td>
							</tr>
						</table></td>
					</tr><tr>
						<td><table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"">
							<tr onmouseover=""Menu_HoverStatic(this)"" onmouseout=""Menu_Unhover(this)"" onkeyup=""Menu_Key(this)"" id=""Menun3"">
								<td><table cellpadding=""0"" cellspacing=""0"" border=""0"" width=""100%"">
									<tr>
										<td style=""white-space:nowrap;width:100%;""><a class=""Menu_1"" href=""javascript:__doPostBack('Menu','one-black-value\\two-black-1-value\\three-black-1-value\\four-black-1-value')"" style=""margin-left:48px;"">four-black-1</a></td>
									</tr>
								</table></td>
							</tr><tr onmouseover=""Menu_HoverStatic(this)"" onmouseout=""Menu_Unhover(this)"" onkeyup=""Menu_Key(this)"" id=""Menun4"">
								<td><table cellpadding=""0"" cellspacing=""0"" border=""0"" width=""100%"">
									<tr>
										<td style=""white-space:nowrap;width:100%;""><a class=""Menu_1"" href=""javascript:__doPostBack('Menu','one-black-value\\two-black-1-value\\three-black-1-value\\four-black-2-value')"" style=""margin-left:48px;"">four-black-2</a></td>
									</tr>
								</table></td>
							</tr>
						</table></td>
					</tr><tr onmouseover=""Menu_HoverRoot(this)"" onmouseout=""Menu_Unhover(this)"" onkeyup=""Menu_Key(this)"" id=""Menun5"">
						<td><table cellpadding=""0"" cellspacing=""0"" border=""0"" width=""100%"">
							<tr>
								<td style=""white-space:nowrap;width:100%;""><a class=""Menu_1"" href=""javascript:__doPostBack('Menu','one-black-value\\two-black-1-value\\three-black-2-value')"" style=""margin-left:32px;"">three-black-2</a></td>
							</tr>
						</table></td>
					</tr><tr>
						<td><table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"">
							<tr onmouseover=""Menu_HoverStatic(this)"" onmouseout=""Menu_Unhover(this)"" onkeyup=""Menu_Key(this)"" id=""Menun6"">
								<td><table cellpadding=""0"" cellspacing=""0"" border=""0"" width=""100%"">
									<tr>
										<td style=""white-space:nowrap;width:100%;""><a class=""Menu_1"" href=""javascript:__doPostBack('Menu','one-black-value\\two-black-1-value\\three-black-2-value\\four-black-3-value')"" style=""margin-left:48px;"">four-black-3</a></td>
									</tr>
								</table></td>
							</tr><tr onmouseover=""Menu_HoverStatic(this)"" onmouseout=""Menu_Unhover(this)"" onkeyup=""Menu_Key(this)"" id=""Menun7"">
								<td><table cellpadding=""0"" cellspacing=""0"" border=""0"" width=""100%"">
									<tr>
										<td style=""white-space:nowrap;width:100%;""><a class=""Menu_1"" href=""javascript:__doPostBack('Menu','one-black-value\\two-black-1-value\\three-black-2-value\\four-black-4-value')"" style=""margin-left:48px;"">four-black-4</a></td>
									</tr>
								</table></td>
							</tr>
						</table></td>
					</tr>
				</table></td>
			</tr><tr onmouseover=""Menu_HoverRoot(this)"" onmouseout=""Menu_Unhover(this)"" onkeyup=""Menu_Key(this)"" id=""Menun8"">
				<td><table cellpadding=""0"" cellspacing=""0"" border=""0"" width=""100%"">
					<tr>
						<td style=""white-space:nowrap;width:100%;""><a class=""Menu_1"" href=""javascript:__doPostBack('Menu','one-black-value\\two-black-2-value')"" style=""margin-left:16px;"">two-black-2</a></td>
					</tr>
				</table></td>
			</tr><tr>
				<td><table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"">
					<tr onmouseover=""Menu_HoverRoot(this)"" onmouseout=""Menu_Unhover(this)"" onkeyup=""Menu_Key(this)"" id=""Menun9"">
						<td><table cellpadding=""0"" cellspacing=""0"" border=""0"" width=""100%"">
							<tr>
								<td style=""white-space:nowrap;width:100%;""><a class=""Menu_1"" href=""javascript:__doPostBack('Menu','one-black-value\\two-black-2-value\\three-black-3-value')"" style=""margin-left:32px;"">three-black-3</a></td>
							</tr>
						</table></td>
					</tr><tr>
						<td><table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"">
							<tr onmouseover=""Menu_HoverStatic(this)"" onmouseout=""Menu_Unhover(this)"" onkeyup=""Menu_Key(this)"" id=""Menun10"">
								<td><table cellpadding=""0"" cellspacing=""0"" border=""0"" width=""100%"">
									<tr>
										<td style=""white-space:nowrap;width:100%;""><a class=""Menu_1"" href=""javascript:__doPostBack('Menu','one-black-value\\two-black-2-value\\three-black-3-value\\four-black-5-value')"" style=""margin-left:48px;"">four-black-5</a></td>
									</tr>
								</table></td>
							</tr><tr onmouseover=""Menu_HoverStatic(this)"" onmouseout=""Menu_Unhover(this)"" onkeyup=""Menu_Key(this)"" id=""Menun11"">
								<td><table cellpadding=""0"" cellspacing=""0"" border=""0"" width=""100%"">
									<tr>
										<td style=""white-space:nowrap;width:100%;""><a class=""Menu_1"" href=""javascript:__doPostBack('Menu','one-black-value\\two-black-2-value\\three-black-3-value\\four-black-6-value')"" style=""margin-left:48px;"">four-black-6</a></td>
									</tr>
								</table></td>
							</tr>
						</table></td>
					</tr><tr onmouseover=""Menu_HoverRoot(this)"" onmouseout=""Menu_Unhover(this)"" onkeyup=""Menu_Key(this)"" id=""Menun12"">
						<td><table cellpadding=""0"" cellspacing=""0"" border=""0"" width=""100%"">
							<tr>
								<td style=""white-space:nowrap;width:100%;""><a class=""Menu_1"" href=""javascript:__doPostBack('Menu','one-black-value\\two-black-2-value\\three-black-4-value')"" style=""margin-left:32px;"">three-black-4</a></td>
							</tr>
						</table></td>
					</tr><tr>
						<td><table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"">
							<tr onmouseover=""Menu_HoverStatic(this)"" onmouseout=""Menu_Unhover(this)"" onkeyup=""Menu_Key(this)"" id=""Menun13"">
								<td><table cellpadding=""0"" cellspacing=""0"" border=""0"" width=""100%"">
									<tr>
										<td style=""white-space:nowrap;width:100%;""><a class=""Menu_1"" href=""javascript:__doPostBack('Menu','one-black-value\\two-black-2-value\\three-black-4-value\\four-black-7-value')"" style=""margin-left:48px;"">four-black-7</a></td>
									</tr>
								</table></td>
							</tr><tr onmouseover=""Menu_HoverStatic(this)"" onmouseout=""Menu_Unhover(this)"" onkeyup=""Menu_Key(this)"" id=""Menun14"">
								<td><table cellpadding=""0"" cellspacing=""0"" border=""0"" width=""100%"">
									<tr>
										<td style=""white-space:nowrap;width:100%;""><a class=""Menu_1"" href=""javascript:__doPostBack('Menu','one-black-value\\two-black-2-value\\three-black-4-value\\four-black-8-value')"" style=""margin-left:48px;"">four-black-8</a></td>
									</tr>
								</table></td>
							</tr>
						</table></td>
					</tr>
				</table></td>
			</tr>
		</table></td>
	</tr>
</table><a id=""Menu_SkipLink""></a>";
			#endregion
			RenderedPageHtml = new WebTest (PageInvoker.CreateOnLoad (Menu_RenderStaticItems_Vertical)).Run ();
			RenderedControlHtml = HtmlDiff.GetControlFromPageHtml (RenderedPageHtml);
			HtmlDiff.AssertAreEqual (OriginControlHtml, RenderedControlHtml, "Menu_RenderStaticItems_Vertical");
			RenderedPageHtml = new WebTest (PageInvoker.CreateOnLoad (Menu_RenderStaticItemsWithBaseAdapter_Vertical)).Run ();
			RenderedControlHtml = HtmlDiff.GetControlFromPageHtml (RenderedPageHtml);
			HtmlDiff.AssertAreEqual (OriginControlHtml, RenderedControlHtml, "Menu_RenderStaticItemsWithDefaultAdapter_Vertical");

			#region OriginControlHtml
			OriginControlHtml = @"<a href=""#Menu_SkipLink""><img alt=""Skip Navigation Links"" src=""/NunitWeb/WebResource.axd?d=QxfUEifeQdL5PTiZOF8HlA2&amp;t=632900536532114160"" width=""0"" height=""0"" style=""border-width:0px;"" /></a><table id=""Menu"" class=""Menu_2"" cellpadding=""0"" cellspacing=""0"" border=""0"">
	<tr>
		<td onmouseover=""Menu_HoverRoot(this)"" onmouseout=""Menu_Unhover(this)"" onkeyup=""Menu_Key(this)"" id=""Menun0""><table cellpadding=""0"" cellspacing=""0"" border=""0"" width=""100%"">
			<tr>
				<td style=""white-space:nowrap;""><a class=""Menu_1"" href=""javascript:__doPostBack('Menu','one-black-value')"">one-black</a></td>
			</tr>
		</table></td><td style=""width:3px;""></td><td><table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"">
			<tr>
				<td onmouseover=""Menu_HoverRoot(this)"" onmouseout=""Menu_Unhover(this)"" onkeyup=""Menu_Key(this)"" id=""Menun1""><table cellpadding=""0"" cellspacing=""0"" border=""0"" width=""100%"">
					<tr>
						<td style=""white-space:nowrap;""><a class=""Menu_1"" href=""javascript:__doPostBack('Menu','one-black-value\\two-black-1-value')"" style=""margin-left:16px;"">two-black-1</a></td>
					</tr>
				</table></td><td style=""width:3px;""></td><td><table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"">
					<tr>
						<td onmouseover=""Menu_HoverRoot(this)"" onmouseout=""Menu_Unhover(this)"" onkeyup=""Menu_Key(this)"" id=""Menun2""><table cellpadding=""0"" cellspacing=""0"" border=""0"" width=""100%"">
							<tr>
								<td style=""white-space:nowrap;""><a class=""Menu_1"" href=""javascript:__doPostBack('Menu','one-black-value\\two-black-1-value\\three-black-1-value')"" style=""margin-left:32px;"">three-black-1</a></td>
							</tr>
						</table></td><td style=""width:3px;""></td><td><table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"">
							<tr>
								<td onmouseover=""Menu_HoverStatic(this)"" onmouseout=""Menu_Unhover(this)"" onkeyup=""Menu_Key(this)"" id=""Menun3""><table cellpadding=""0"" cellspacing=""0"" border=""0"" width=""100%"">
									<tr>
										<td style=""white-space:nowrap;""><a class=""Menu_1"" href=""javascript:__doPostBack('Menu','one-black-value\\two-black-1-value\\three-black-1-value\\four-black-1-value')"" style=""margin-left:48px;"">four-black-1</a></td>
									</tr>
								</table></td><td style=""width:3px;""></td><td onmouseover=""Menu_HoverStatic(this)"" onmouseout=""Menu_Unhover(this)"" onkeyup=""Menu_Key(this)"" id=""Menun4""><table cellpadding=""0"" cellspacing=""0"" border=""0"" width=""100%"">
									<tr>
										<td style=""white-space:nowrap;""><a class=""Menu_1"" href=""javascript:__doPostBack('Menu','one-black-value\\two-black-1-value\\three-black-1-value\\four-black-2-value')"" style=""margin-left:48px;"">four-black-2</a></td>
									</tr>
								</table></td><td style=""width:3px;""></td>
							</tr>
						</table></td><td onmouseover=""Menu_HoverRoot(this)"" onmouseout=""Menu_Unhover(this)"" onkeyup=""Menu_Key(this)"" id=""Menun5""><table cellpadding=""0"" cellspacing=""0"" border=""0"" width=""100%"">
							<tr>
								<td style=""white-space:nowrap;""><a class=""Menu_1"" href=""javascript:__doPostBack('Menu','one-black-value\\two-black-1-value\\three-black-2-value')"" style=""margin-left:32px;"">three-black-2</a></td>
							</tr>
						</table></td><td style=""width:3px;""></td><td><table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"">
							<tr>
								<td onmouseover=""Menu_HoverStatic(this)"" onmouseout=""Menu_Unhover(this)"" onkeyup=""Menu_Key(this)"" id=""Menun6""><table cellpadding=""0"" cellspacing=""0"" border=""0"" width=""100%"">
									<tr>
										<td style=""white-space:nowrap;""><a class=""Menu_1"" href=""javascript:__doPostBack('Menu','one-black-value\\two-black-1-value\\three-black-2-value\\four-black-3-value')"" style=""margin-left:48px;"">four-black-3</a></td>
									</tr>
								</table></td><td style=""width:3px;""></td><td onmouseover=""Menu_HoverStatic(this)"" onmouseout=""Menu_Unhover(this)"" onkeyup=""Menu_Key(this)"" id=""Menun7""><table cellpadding=""0"" cellspacing=""0"" border=""0"" width=""100%"">
									<tr>
										<td style=""white-space:nowrap;""><a class=""Menu_1"" href=""javascript:__doPostBack('Menu','one-black-value\\two-black-1-value\\three-black-2-value\\four-black-4-value')"" style=""margin-left:48px;"">four-black-4</a></td>
									</tr>
								</table></td><td style=""width:3px;""></td>
							</tr>
						</table></td>
					</tr>
				</table></td><td onmouseover=""Menu_HoverRoot(this)"" onmouseout=""Menu_Unhover(this)"" onkeyup=""Menu_Key(this)"" id=""Menun8""><table cellpadding=""0"" cellspacing=""0"" border=""0"" width=""100%"">
					<tr>
						<td style=""white-space:nowrap;""><a class=""Menu_1"" href=""javascript:__doPostBack('Menu','one-black-value\\two-black-2-value')"" style=""margin-left:16px;"">two-black-2</a></td>
					</tr>
				</table></td><td style=""width:3px;""></td><td><table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"">
					<tr>
						<td onmouseover=""Menu_HoverRoot(this)"" onmouseout=""Menu_Unhover(this)"" onkeyup=""Menu_Key(this)"" id=""Menun9""><table cellpadding=""0"" cellspacing=""0"" border=""0"" width=""100%"">
							<tr>
								<td style=""white-space:nowrap;""><a class=""Menu_1"" href=""javascript:__doPostBack('Menu','one-black-value\\two-black-2-value\\three-black-3-value')"" style=""margin-left:32px;"">three-black-3</a></td>
							</tr>
						</table></td><td style=""width:3px;""></td><td><table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"">
							<tr>
								<td onmouseover=""Menu_HoverStatic(this)"" onmouseout=""Menu_Unhover(this)"" onkeyup=""Menu_Key(this)"" id=""Menun10""><table cellpadding=""0"" cellspacing=""0"" border=""0"" width=""100%"">
									<tr>
										<td style=""white-space:nowrap;""><a class=""Menu_1"" href=""javascript:__doPostBack('Menu','one-black-value\\two-black-2-value\\three-black-3-value\\four-black-5-value')"" style=""margin-left:48px;"">four-black-5</a></td>
									</tr>
								</table></td><td style=""width:3px;""></td><td onmouseover=""Menu_HoverStatic(this)"" onmouseout=""Menu_Unhover(this)"" onkeyup=""Menu_Key(this)"" id=""Menun11""><table cellpadding=""0"" cellspacing=""0"" border=""0"" width=""100%"">
									<tr>
										<td style=""white-space:nowrap;""><a class=""Menu_1"" href=""javascript:__doPostBack('Menu','one-black-value\\two-black-2-value\\three-black-3-value\\four-black-6-value')"" style=""margin-left:48px;"">four-black-6</a></td>
									</tr>
								</table></td><td style=""width:3px;""></td>
							</tr>
						</table></td><td onmouseover=""Menu_HoverRoot(this)"" onmouseout=""Menu_Unhover(this)"" onkeyup=""Menu_Key(this)"" id=""Menun12""><table cellpadding=""0"" cellspacing=""0"" border=""0"" width=""100%"">
							<tr>
								<td style=""white-space:nowrap;""><a class=""Menu_1"" href=""javascript:__doPostBack('Menu','one-black-value\\two-black-2-value\\three-black-4-value')"" style=""margin-left:32px;"">three-black-4</a></td>
							</tr>
						</table></td><td style=""width:3px;""></td><td><table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"">
							<tr>
								<td onmouseover=""Menu_HoverStatic(this)"" onmouseout=""Menu_Unhover(this)"" onkeyup=""Menu_Key(this)"" id=""Menun13""><table cellpadding=""0"" cellspacing=""0"" border=""0"" width=""100%"">
									<tr>
										<td style=""white-space:nowrap;""><a class=""Menu_1"" href=""javascript:__doPostBack('Menu','one-black-value\\two-black-2-value\\three-black-4-value\\four-black-7-value')"" style=""margin-left:48px;"">four-black-7</a></td>
									</tr>
								</table></td><td style=""width:3px;""></td><td onmouseover=""Menu_HoverStatic(this)"" onmouseout=""Menu_Unhover(this)"" onkeyup=""Menu_Key(this)"" id=""Menun14""><table cellpadding=""0"" cellspacing=""0"" border=""0"" width=""100%"">
									<tr>
										<td style=""white-space:nowrap;""><a class=""Menu_1"" href=""javascript:__doPostBack('Menu','one-black-value\\two-black-2-value\\three-black-4-value\\four-black-8-value')"" style=""margin-left:48px;"">four-black-8</a></td>
									</tr>
								</table></td>
							</tr>
						</table></td>
					</tr>
				</table></td>
			</tr>
		</table></td>
	</tr>
</table><a id=""Menu_SkipLink""></a>";
			#endregion
			RenderedPageHtml = new WebTest (PageInvoker.CreateOnLoad (Menu_RenderStaticItems_Horizontal)).Run ();
			RenderedControlHtml = HtmlDiff.GetControlFromPageHtml (RenderedPageHtml);
			HtmlDiff.AssertAreEqual (OriginControlHtml, RenderedControlHtml, "Menu_RenderStaticItems_Horizontal");
			RenderedPageHtml = new WebTest (PageInvoker.CreateOnLoad (Menu_RenderStaticItemsWithBaseAdapter_Horizontal)).Run ();
			RenderedControlHtml = HtmlDiff.GetControlFromPageHtml (RenderedPageHtml);
			HtmlDiff.AssertAreEqual (OriginControlHtml, RenderedControlHtml, "Menu_RenderStaticItemsWithBaseAdapter_Horizontal");
		}

		class MyMenuAdapter : global::System.Web.UI.WebControls.Adapters.MenuAdapter
		{
			protected override void RenderItem (HtmlTextWriter writer, 
								    MenuItem item,
								    int position)
			{
				writer.Write ("{");
				base.RenderItem (writer, item, position);
				writer.Write ("}");				
			}			
		}

		[Test]
		[Category ("NunitWeb")]
		public void Menu_RenderStaticItemsWithAdapter () {
			string RenderedPageHtml, RenderedControlHtml, OriginControlHtml;
			RenderedPageHtml = new WebTest (PageInvoker.CreateOnLoad (Menu_RenderStaticItemsWithAdapter_Vertical)).Run ();
			RenderedControlHtml = HtmlDiff.GetControlFromPageHtml (RenderedPageHtml);
			#region OriginControlHtml
			OriginControlHtml = @"<a href=""#Menu_SkipLink""><img alt=""Skip Navigation Links"" src=""/NunitWeb/WebResource.axd?d=QxfUEifeQdL5PTiZOF8HlA2&amp;t=632900536532114160"" width=""0"" height=""0"" style=""border-width:0px;"" /></a><table id=""Menu"" class=""Menu_2"" cellpadding=""0"" cellspacing=""0"" border=""0"">
	{<tr onmouseover=""Menu_HoverRoot(this)"" onmouseout=""Menu_Unhover(this)"" onkeyup=""Menu_Key(this)"" id=""Menun0"">
		<td><table cellpadding=""0"" cellspacing=""0"" border=""0"" width=""100%"">
			<tr>
				<td style=""white-space:nowrap;width:100%;""><a class=""Menu_1"" href=""javascript:__doPostBack('Menu','one-black-value')"">one-black</a></td>
			</tr>
		</table></td>
	</tr><tr>
		<td><table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"">
			{<tr onmouseover=""Menu_HoverRoot(this)"" onmouseout=""Menu_Unhover(this)"" onkeyup=""Menu_Key(this)"" id=""Menun1"">
				<td><table cellpadding=""0"" cellspacing=""0"" border=""0"" width=""100%"">
					<tr>
						<td style=""white-space:nowrap;width:100%;""><a class=""Menu_1"" href=""javascript:__doPostBack('Menu','one-black-value\\two-black-1-value')"" style=""margin-left:16px;"">two-black-1</a></td>
					</tr>
				</table></td>
			</tr><tr>
				<td><table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"">
					{<tr onmouseover=""Menu_HoverRoot(this)"" onmouseout=""Menu_Unhover(this)"" onkeyup=""Menu_Key(this)"" id=""Menun2"">
						<td><table cellpadding=""0"" cellspacing=""0"" border=""0"" width=""100%"">
							<tr>
								<td style=""white-space:nowrap;width:100%;""><a class=""Menu_1"" href=""javascript:__doPostBack('Menu','one-black-value\\two-black-1-value\\three-black-1-value')"" style=""margin-left:32px;"">three-black-1</a></td>
							</tr>
						</table></td>
					</tr><tr>
						<td><table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"">
							{<tr onmouseover=""Menu_HoverStatic(this)"" onmouseout=""Menu_Unhover(this)"" onkeyup=""Menu_Key(this)"" id=""Menun3"">
								<td><table cellpadding=""0"" cellspacing=""0"" border=""0"" width=""100%"">
									<tr>
										<td style=""white-space:nowrap;width:100%;""><a class=""Menu_1"" href=""javascript:__doPostBack('Menu','one-black-value\\two-black-1-value\\three-black-1-value\\four-black-1-value')"" style=""margin-left:48px;"">four-black-1</a></td>
									</tr>
								</table></td>
							</tr>}{<tr onmouseover=""Menu_HoverStatic(this)"" onmouseout=""Menu_Unhover(this)"" onkeyup=""Menu_Key(this)"" id=""Menun4"">
								<td><table cellpadding=""0"" cellspacing=""0"" border=""0"" width=""100%"">
									<tr>
										<td style=""white-space:nowrap;width:100%;""><a class=""Menu_1"" href=""javascript:__doPostBack('Menu','one-black-value\\two-black-1-value\\three-black-1-value\\four-black-2-value')"" style=""margin-left:48px;"">four-black-2</a></td>
									</tr>
								</table></td>
							</tr>}
						</table></td>
					</tr>}{<tr onmouseover=""Menu_HoverRoot(this)"" onmouseout=""Menu_Unhover(this)"" onkeyup=""Menu_Key(this)"" id=""Menun5"">
						<td><table cellpadding=""0"" cellspacing=""0"" border=""0"" width=""100%"">
							<tr>
								<td style=""white-space:nowrap;width:100%;""><a class=""Menu_1"" href=""javascript:__doPostBack('Menu','one-black-value\\two-black-1-value\\three-black-2-value')"" style=""margin-left:32px;"">three-black-2</a></td>
							</tr>
						</table></td>
					</tr><tr>
						<td><table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"">
							{<tr onmouseover=""Menu_HoverStatic(this)"" onmouseout=""Menu_Unhover(this)"" onkeyup=""Menu_Key(this)"" id=""Menun6"">
								<td><table cellpadding=""0"" cellspacing=""0"" border=""0"" width=""100%"">
									<tr>
										<td style=""white-space:nowrap;width:100%;""><a class=""Menu_1"" href=""javascript:__doPostBack('Menu','one-black-value\\two-black-1-value\\three-black-2-value\\four-black-3-value')"" style=""margin-left:48px;"">four-black-3</a></td>
									</tr>
								</table></td>
							</tr>}{<tr onmouseover=""Menu_HoverStatic(this)"" onmouseout=""Menu_Unhover(this)"" onkeyup=""Menu_Key(this)"" id=""Menun7"">
								<td><table cellpadding=""0"" cellspacing=""0"" border=""0"" width=""100%"">
									<tr>
										<td style=""white-space:nowrap;width:100%;""><a class=""Menu_1"" href=""javascript:__doPostBack('Menu','one-black-value\\two-black-1-value\\three-black-2-value\\four-black-4-value')"" style=""margin-left:48px;"">four-black-4</a></td>
									</tr>
								</table></td>
							</tr>}
						</table></td>
					</tr>}
				</table></td>
			</tr>}{<tr onmouseover=""Menu_HoverRoot(this)"" onmouseout=""Menu_Unhover(this)"" onkeyup=""Menu_Key(this)"" id=""Menun8"">
				<td><table cellpadding=""0"" cellspacing=""0"" border=""0"" width=""100%"">
					<tr>
						<td style=""white-space:nowrap;width:100%;""><a class=""Menu_1"" href=""javascript:__doPostBack('Menu','one-black-value\\two-black-2-value')"" style=""margin-left:16px;"">two-black-2</a></td>
					</tr>
				</table></td>
			</tr><tr>
				<td><table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"">
					{<tr onmouseover=""Menu_HoverRoot(this)"" onmouseout=""Menu_Unhover(this)"" onkeyup=""Menu_Key(this)"" id=""Menun9"">
						<td><table cellpadding=""0"" cellspacing=""0"" border=""0"" width=""100%"">
							<tr>
								<td style=""white-space:nowrap;width:100%;""><a class=""Menu_1"" href=""javascript:__doPostBack('Menu','one-black-value\\two-black-2-value\\three-black-3-value')"" style=""margin-left:32px;"">three-black-3</a></td>
							</tr>
						</table></td>
					</tr><tr>
						<td><table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"">
							{<tr onmouseover=""Menu_HoverStatic(this)"" onmouseout=""Menu_Unhover(this)"" onkeyup=""Menu_Key(this)"" id=""Menun10"">
								<td><table cellpadding=""0"" cellspacing=""0"" border=""0"" width=""100%"">
									<tr>
										<td style=""white-space:nowrap;width:100%;""><a class=""Menu_1"" href=""javascript:__doPostBack('Menu','one-black-value\\two-black-2-value\\three-black-3-value\\four-black-5-value')"" style=""margin-left:48px;"">four-black-5</a></td>
									</tr>
								</table></td>
							</tr>}{<tr onmouseover=""Menu_HoverStatic(this)"" onmouseout=""Menu_Unhover(this)"" onkeyup=""Menu_Key(this)"" id=""Menun11"">
								<td><table cellpadding=""0"" cellspacing=""0"" border=""0"" width=""100%"">
									<tr>
										<td style=""white-space:nowrap;width:100%;""><a class=""Menu_1"" href=""javascript:__doPostBack('Menu','one-black-value\\two-black-2-value\\three-black-3-value\\four-black-6-value')"" style=""margin-left:48px;"">four-black-6</a></td>
									</tr>
								</table></td>
							</tr>}
						</table></td>
					</tr>}{<tr onmouseover=""Menu_HoverRoot(this)"" onmouseout=""Menu_Unhover(this)"" onkeyup=""Menu_Key(this)"" id=""Menun12"">
						<td><table cellpadding=""0"" cellspacing=""0"" border=""0"" width=""100%"">
							<tr>
								<td style=""white-space:nowrap;width:100%;""><a class=""Menu_1"" href=""javascript:__doPostBack('Menu','one-black-value\\two-black-2-value\\three-black-4-value')"" style=""margin-left:32px;"">three-black-4</a></td>
							</tr>
						</table></td>
					</tr><tr>
						<td><table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"">
							{<tr onmouseover=""Menu_HoverStatic(this)"" onmouseout=""Menu_Unhover(this)"" onkeyup=""Menu_Key(this)"" id=""Menun13"">
								<td><table cellpadding=""0"" cellspacing=""0"" border=""0"" width=""100%"">
									<tr>
										<td style=""white-space:nowrap;width:100%;""><a class=""Menu_1"" href=""javascript:__doPostBack('Menu','one-black-value\\two-black-2-value\\three-black-4-value\\four-black-7-value')"" style=""margin-left:48px;"">four-black-7</a></td>
									</tr>
								</table></td>
							</tr>}{<tr onmouseover=""Menu_HoverStatic(this)"" onmouseout=""Menu_Unhover(this)"" onkeyup=""Menu_Key(this)"" id=""Menun14"">
								<td><table cellpadding=""0"" cellspacing=""0"" border=""0"" width=""100%"">
									<tr>
										<td style=""white-space:nowrap;width:100%;""><a class=""Menu_1"" href=""javascript:__doPostBack('Menu','one-black-value\\two-black-2-value\\three-black-4-value\\four-black-8-value')"" style=""margin-left:48px;"">four-black-8</a></td>
									</tr>
								</table></td>
							</tr>}
						</table></td>
					</tr>}
				</table></td>
			</tr>}
		</table></td>
	</tr>}
</table><a id=""Menu_SkipLink""></a>";
			#endregion
			HtmlDiff.AssertAreEqual (OriginControlHtml, RenderedControlHtml, "Menu_RenderStaticItemsWithAdapter_Vertical");
			RenderedPageHtml = new WebTest (PageInvoker.CreateOnLoad (Menu_RenderStaticItemsWithAdapter_Horizontal)).Run ();
			RenderedControlHtml = HtmlDiff.GetControlFromPageHtml (RenderedPageHtml);
			#region OriginControlHtml
			OriginControlHtml = @"<a href=""#Menu_SkipLink""><img alt=""Skip Navigation Links"" src=""/NunitWeb/WebResource.axd?d=QxfUEifeQdL5PTiZOF8HlA2&amp;t=632900536532114160"" width=""0"" height=""0"" style=""border-width:0px;"" /></a><table id=""Menu"" class=""Menu_2"" cellpadding=""0"" cellspacing=""0"" border=""0"">
	<tr>
		{<td onmouseover=""Menu_HoverRoot(this)"" onmouseout=""Menu_Unhover(this)"" onkeyup=""Menu_Key(this)"" id=""Menun0""><table cellpadding=""0"" cellspacing=""0"" border=""0"" width=""100%"">
			<tr>
				<td style=""white-space:nowrap;""><a class=""Menu_1"" href=""javascript:__doPostBack('Menu','one-black-value')"">one-black</a></td>
			</tr>
		</table></td><td style=""width:3px;""></td><td><table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"">
			<tr>
				{<td onmouseover=""Menu_HoverRoot(this)"" onmouseout=""Menu_Unhover(this)"" onkeyup=""Menu_Key(this)"" id=""Menun1""><table cellpadding=""0"" cellspacing=""0"" border=""0"" width=""100%"">
					<tr>
						<td style=""white-space:nowrap;""><a class=""Menu_1"" href=""javascript:__doPostBack('Menu','one-black-value\\two-black-1-value')"" style=""margin-left:16px;"">two-black-1</a></td>
					</tr>
				</table></td><td style=""width:3px;""></td><td><table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"">
					<tr>
						{<td onmouseover=""Menu_HoverRoot(this)"" onmouseout=""Menu_Unhover(this)"" onkeyup=""Menu_Key(this)"" id=""Menun2""><table cellpadding=""0"" cellspacing=""0"" border=""0"" width=""100%"">
							<tr>
								<td style=""white-space:nowrap;""><a class=""Menu_1"" href=""javascript:__doPostBack('Menu','one-black-value\\two-black-1-value\\three-black-1-value')"" style=""margin-left:32px;"">three-black-1</a></td>
							</tr>
						</table></td><td style=""width:3px;""></td><td><table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"">
							<tr>
								{<td onmouseover=""Menu_HoverStatic(this)"" onmouseout=""Menu_Unhover(this)"" onkeyup=""Menu_Key(this)"" id=""Menun3""><table cellpadding=""0"" cellspacing=""0"" border=""0"" width=""100%"">
									<tr>
										<td style=""white-space:nowrap;""><a class=""Menu_1"" href=""javascript:__doPostBack('Menu','one-black-value\\two-black-1-value\\three-black-1-value\\four-black-1-value')"" style=""margin-left:48px;"">four-black-1</a></td>
									</tr>
								</table></td><td style=""width:3px;""></td>}{<td onmouseover=""Menu_HoverStatic(this)"" onmouseout=""Menu_Unhover(this)"" onkeyup=""Menu_Key(this)"" id=""Menun4""><table cellpadding=""0"" cellspacing=""0"" border=""0"" width=""100%"">
									<tr>
										<td style=""white-space:nowrap;""><a class=""Menu_1"" href=""javascript:__doPostBack('Menu','one-black-value\\two-black-1-value\\three-black-1-value\\four-black-2-value')"" style=""margin-left:48px;"">four-black-2</a></td>
									</tr>
								</table></td><td style=""width:3px;""></td>}
							</tr>
						</table></td>}{<td onmouseover=""Menu_HoverRoot(this)"" onmouseout=""Menu_Unhover(this)"" onkeyup=""Menu_Key(this)"" id=""Menun5""><table cellpadding=""0"" cellspacing=""0"" border=""0"" width=""100%"">
							<tr>
								<td style=""white-space:nowrap;""><a class=""Menu_1"" href=""javascript:__doPostBack('Menu','one-black-value\\two-black-1-value\\three-black-2-value')"" style=""margin-left:32px;"">three-black-2</a></td>
							</tr>
						</table></td><td style=""width:3px;""></td><td><table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"">
							<tr>
								{<td onmouseover=""Menu_HoverStatic(this)"" onmouseout=""Menu_Unhover(this)"" onkeyup=""Menu_Key(this)"" id=""Menun6""><table cellpadding=""0"" cellspacing=""0"" border=""0"" width=""100%"">
									<tr>
										<td style=""white-space:nowrap;""><a class=""Menu_1"" href=""javascript:__doPostBack('Menu','one-black-value\\two-black-1-value\\three-black-2-value\\four-black-3-value')"" style=""margin-left:48px;"">four-black-3</a></td>
									</tr>
								</table></td><td style=""width:3px;""></td>}{<td onmouseover=""Menu_HoverStatic(this)"" onmouseout=""Menu_Unhover(this)"" onkeyup=""Menu_Key(this)"" id=""Menun7""><table cellpadding=""0"" cellspacing=""0"" border=""0"" width=""100%"">
									<tr>
										<td style=""white-space:nowrap;""><a class=""Menu_1"" href=""javascript:__doPostBack('Menu','one-black-value\\two-black-1-value\\three-black-2-value\\four-black-4-value')"" style=""margin-left:48px;"">four-black-4</a></td>
									</tr>
								</table></td><td style=""width:3px;""></td>}
							</tr>
						</table></td>}
					</tr>
				</table></td>}{<td onmouseover=""Menu_HoverRoot(this)"" onmouseout=""Menu_Unhover(this)"" onkeyup=""Menu_Key(this)"" id=""Menun8""><table cellpadding=""0"" cellspacing=""0"" border=""0"" width=""100%"">
					<tr>
						<td style=""white-space:nowrap;""><a class=""Menu_1"" href=""javascript:__doPostBack('Menu','one-black-value\\two-black-2-value')"" style=""margin-left:16px;"">two-black-2</a></td>
					</tr>
				</table></td><td style=""width:3px;""></td><td><table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"">
					<tr>
						{<td onmouseover=""Menu_HoverRoot(this)"" onmouseout=""Menu_Unhover(this)"" onkeyup=""Menu_Key(this)"" id=""Menun9""><table cellpadding=""0"" cellspacing=""0"" border=""0"" width=""100%"">
							<tr>
								<td style=""white-space:nowrap;""><a class=""Menu_1"" href=""javascript:__doPostBack('Menu','one-black-value\\two-black-2-value\\three-black-3-value')"" style=""margin-left:32px;"">three-black-3</a></td>
							</tr>
						</table></td><td style=""width:3px;""></td><td><table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"">
							<tr>
								{<td onmouseover=""Menu_HoverStatic(this)"" onmouseout=""Menu_Unhover(this)"" onkeyup=""Menu_Key(this)"" id=""Menun10""><table cellpadding=""0"" cellspacing=""0"" border=""0"" width=""100%"">
									<tr>
										<td style=""white-space:nowrap;""><a class=""Menu_1"" href=""javascript:__doPostBack('Menu','one-black-value\\two-black-2-value\\three-black-3-value\\four-black-5-value')"" style=""margin-left:48px;"">four-black-5</a></td>
									</tr>
								</table></td><td style=""width:3px;""></td>}{<td onmouseover=""Menu_HoverStatic(this)"" onmouseout=""Menu_Unhover(this)"" onkeyup=""Menu_Key(this)"" id=""Menun11""><table cellpadding=""0"" cellspacing=""0"" border=""0"" width=""100%"">
									<tr>
										<td style=""white-space:nowrap;""><a class=""Menu_1"" href=""javascript:__doPostBack('Menu','one-black-value\\two-black-2-value\\three-black-3-value\\four-black-6-value')"" style=""margin-left:48px;"">four-black-6</a></td>
									</tr>
								</table></td><td style=""width:3px;""></td>}
							</tr>
						</table></td>}{<td onmouseover=""Menu_HoverRoot(this)"" onmouseout=""Menu_Unhover(this)"" onkeyup=""Menu_Key(this)"" id=""Menun12""><table cellpadding=""0"" cellspacing=""0"" border=""0"" width=""100%"">
							<tr>
								<td style=""white-space:nowrap;""><a class=""Menu_1"" href=""javascript:__doPostBack('Menu','one-black-value\\two-black-2-value\\three-black-4-value')"" style=""margin-left:32px;"">three-black-4</a></td>
							</tr>
						</table></td><td style=""width:3px;""></td><td><table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"">
							<tr>
								{<td onmouseover=""Menu_HoverStatic(this)"" onmouseout=""Menu_Unhover(this)"" onkeyup=""Menu_Key(this)"" id=""Menun13""><table cellpadding=""0"" cellspacing=""0"" border=""0"" width=""100%"">
									<tr>
										<td style=""white-space:nowrap;""><a class=""Menu_1"" href=""javascript:__doPostBack('Menu','one-black-value\\two-black-2-value\\three-black-4-value\\four-black-7-value')"" style=""margin-left:48px;"">four-black-7</a></td>
									</tr>
								</table></td><td style=""width:3px;""></td>}{<td onmouseover=""Menu_HoverStatic(this)"" onmouseout=""Menu_Unhover(this)"" onkeyup=""Menu_Key(this)"" id=""Menun14""><table cellpadding=""0"" cellspacing=""0"" border=""0"" width=""100%"">
									<tr>
										<td style=""white-space:nowrap;""><a class=""Menu_1"" href=""javascript:__doPostBack('Menu','one-black-value\\two-black-2-value\\three-black-4-value\\four-black-8-value')"" style=""margin-left:48px;"">four-black-8</a></td>
									</tr>
								</table></td>}
							</tr>
						</table></td>}
					</tr>
				</table></td>}
			</tr>
		</table></td>}
	</tr>
</table><a id=""Menu_SkipLink""></a>";
			#endregion
			HtmlDiff.AssertAreEqual (OriginControlHtml, RenderedControlHtml, "Menu_RenderStaticItemsWithAdapter_Horizontal");
		}

		public static void Menu_RenderStaticItems_Vertical (Page p) {
			Menu m = CreateMenuForRenderTests (null);
			m.StaticDisplayLevels = 4;
			AddMenuToPage (p, m);
		}

		public static void Menu_RenderStaticItems_Horizontal (Page p) {
			Menu m = CreateMenuForRenderTests (null);
			m.Orientation = Orientation.Horizontal;
			m.StaticDisplayLevels = 4;
			AddMenuToPage (p, m);
		}

		public static void Menu_RenderStaticItemsWithBaseAdapter_Vertical (Page p) {
			Menu m = CreateMenuForRenderTests (new MyWebControl.Adapters.MenuAdapter());
			m.StaticDisplayLevels = 4;
			AddMenuToPage (p, m);
		}

		public static void Menu_RenderStaticItemsWithBaseAdapter_Horizontal (Page p) {
			Menu m = CreateMenuForRenderTests (new MyWebControl.Adapters.MenuAdapter());
			m.Orientation = Orientation.Horizontal;
			m.StaticDisplayLevels = 4;
			AddMenuToPage (p, m);
		}

		public static void Menu_RenderStaticItemsWithAdapter_Vertical (Page p) {
			Menu m = CreateMenuForRenderTests (new MyMenuAdapter());
			m.StaticDisplayLevels = 4;
			AddMenuToPage (p, m);
		}

		public static void Menu_RenderStaticItemsWithAdapter_Horizontal (Page p) {
			Menu m = CreateMenuForRenderTests (new MyMenuAdapter());
			m.Orientation = Orientation.Horizontal;
			m.StaticDisplayLevels = 4;
			AddMenuToPage (p, m);
		}

		private static void AddMenuToPage (Page p, Menu m) {
			LiteralControl lcb = new LiteralControl (HtmlDiff.BEGIN_TAG);
			LiteralControl lce = new LiteralControl (HtmlDiff.END_TAG);
			p.Form.Controls.Add (lcb);
			p.Form.Controls.Add (m);
			p.Form.Controls.Add (lce);
		}
		
		class MyMenu : Menu
		{
			internal MyMenu (MyWebControl.Adapters.MenuAdapter adapter) : base ()
			{
				menu_adapter = adapter;
			}

			MyWebControl.Adapters.MenuAdapter menu_adapter;
			protected override global::System.Web.UI.Adapters.ControlAdapter ResolveAdapter ()
			{
				return menu_adapter;
			}			
		}

		private static Menu CreateMenuForRenderTests (MyWebControl.Adapters.MenuAdapter adapter) {
			Menu menu = new MyMenu (adapter);
			menu.ID = "Menu";
			MenuItem R, N1, N2, SN1, SN2, SN3, SN4;
			R = new MenuItem ("one-black", "one-black-value");
			N1 = new MenuItem ("two-black-1", "two-black-1-value");
			N2 = new MenuItem ("two-black-2", "two-black-2-value");
			SN1 = new MenuItem ("three-black-1", "three-black-1-value");
			SN2 = new MenuItem ("three-black-2", "three-black-2-value");
			SN3 = new MenuItem ("three-black-3", "three-black-3-value");
			SN4 = new MenuItem ("three-black-4", "three-black-4-value");
			SN1.ChildItems.Add (new MenuItem ("four-black-1", "four-black-1-value"));
			SN1.ChildItems.Add (new MenuItem ("four-black-2", "four-black-2-value"));
			SN2.ChildItems.Add (new MenuItem ("four-black-3", "four-black-3-value"));
			SN2.ChildItems.Add (new MenuItem ("four-black-4", "four-black-4-value"));
			SN3.ChildItems.Add (new MenuItem ("four-black-5", "four-black-5-value"));
			SN3.ChildItems.Add (new MenuItem ("four-black-6", "four-black-6-value"));
			SN4.ChildItems.Add (new MenuItem ("four-black-7", "four-black-7-value"));
			SN4.ChildItems.Add (new MenuItem ("four-black-8", "four-black-8-value"));
			N1.ChildItems.Add (SN1);
			N1.ChildItems.Add (SN2);
			N2.ChildItems.Add (SN3);
			N2.ChildItems.Add (SN4);
			R.ChildItems.Add (N1);
			R.ChildItems.Add (N2);
			menu.Items.Add (R);
			return menu;
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
		public void Menu_BubbleEvent () {
			PokerMenu pm = new PokerMenu ();
			MenuItem item = new MenuItem ("Root");
			pm.Items.Add (item);
			pm.MenuItemClick += new MenuEventHandler (pm_MenuItemClick);
			_MenuItemClick = false;
			MenuEventArgs clickCommandArg = new MenuEventArgs (item, null, new CommandEventArgs (Menu.MenuItemClickCommandName, null));
			CommandEventArgs notClickCommandArg = new CommandEventArgs (Menu.MenuItemClickCommandName + "No", null);
			Assert.AreEqual (true, pm.DoOnBubbleEvent (notClickCommandArg), "Bubble Event#1");
			Assert.AreEqual (false, _MenuItemClick, "MenuItemClick Bubbled");
			Assert.AreEqual (true, pm.DoOnBubbleEvent (clickCommandArg), "Bubble Event#2");
			Assert.AreEqual (true, _MenuItemClick, "MenuItemClick Bubbled");
			Assert.AreEqual (false, pm.DoOnBubbleEvent (new EventArgs ()), "Bubble Event#3");
		}

		bool _MenuItemClick;

		void pm_MenuItemClick (object sender, MenuEventArgs e) {
			_MenuItemClick = true;
		}

		[Test]
		[Category ("NunitWeb")]
		public void Menu_PreRenderEvent ()
		{
			new WebTest (PageInvoker.CreateOnLoad (PreRenderEvent)).Run ();
		}
		public void PreRenderEvent (Page p)
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
			WebTest.Unload ();
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
		[Test]
		[Category ("NunitWeb")]
		public void MenuClass ()
		{
			string res = new WebTest ("menuclass.aspx").Run ();
			string menua_pattern="<table[^>]*class=\"[^\"]*menua[^\"]*\"[^>]*>";
			Assert.IsTrue (Regex.IsMatch (res, ".*"+menua_pattern+".*",
				RegexOptions.IgnoreCase|RegexOptions.Singleline),
				"check that <table class=\"menua\"> is found");
			Assert.IsFalse (Regex.IsMatch (res, ".*"+menua_pattern+".*"+menua_pattern+".*",
				RegexOptions.IgnoreCase|RegexOptions.Singleline),
				"check that <table class=\"menua\"> is found only once");
		}

		[Test]
		[Category ("NunitWeb")]
		[Category ("NotDotNet")] // implementation specific
		public void Menu_PostBack ()
		{
			WebTest t = new WebTest ("PostBackMenuTest.aspx");
			string str = t.Run ();
			FormRequest fr = new FormRequest (t.Response, "form1");
			fr.Controls.Add ("__EVENTTARGET");
			fr.Controls.Add ("__EVENTARGUMENT");
			fr.Controls["__EVENTTARGET"].Value = "Menu1";
			fr.Controls ["__EVENTARGUMENT"].Value = "0_1";
			t.Request = fr;
			PageDelegates pd = new PageDelegates ();
			pd.PreRender = _MenuItemsPost;
			t.Invoker = new PageInvoker (pd);
			t.Run ();
		}

		public static void _MenuItemsPost (Page p)
		{
			foreach (Control c in p.Form.Controls) {
				Menu m = c as Menu;
				if (m != null) {
					Assert.AreEqual ("node2", m.SelectedValue, "MenuItemsPostBack");
				}
			}
		}

		[Test]
		[Category ("NunitWeb")]
		public void Menu_PostBackFireEvents_1 ()
		{
			WebTest t = new WebTest ("PostBackMenuTest.aspx");
			string str = t.Run ();
			FormRequest fr = new FormRequest (t.Response, "form1");
			fr.Controls.Add ("__EVENTTARGET");
			fr.Controls.Add ("__EVENTARGUMENT");
			fr.Controls["__EVENTTARGET"].Value = "Menu1";
#if DOT_NET
			fr.Controls["__EVENTARGUMENT"].Value = "root";  // "0_1";
#else
			fr.Controls ["__EVENTARGUMENT"].Value = "0";  // "0_1";
#endif
			t.Request = fr;
			str = t.Run ();
			Assert.AreEqual ("MenuItemClick", t.UserData.ToString (), "PostBackEvent");
		}

		[Test]
		[Category ("NunitWeb")]
		public void Menu_PostBackFireEvents_2 ()
		{
			WebTest t = new WebTest ("PostBackMenuTest.aspx");
			PageDelegates pd = new PageDelegates ();
			pd.Init = PostBackFireEvents_Init ;
			t.Invoker = new PageInvoker (pd);
			t.Run ();
			Assert.AreEqual ("MenuItemDataBound", t.UserData.ToString (), "MenuItemDataBound");
		}

		public static void PostBackFireEvents_Init (Page p)
		{
			Menu m = new Menu ();
			m.MenuItemDataBound += new MenuEventHandler (MenuItemDataBound_Event);
			m.DataSource = LoadXml ();
			p.Controls.Add (m);
			m.DataBind ();
		}

		public static XmlDataSource LoadXml ()
		{
			XmlDataSource ds = new XmlDataSource ();
			ds.EnableCaching = false;
			#region xml_doc
			String xml_text = @"<siteMapNode url=""1"" title=""root""  description="""">
						<siteMapNode url=""~/MyPage.aspx"" title=""node1""  description="""" />
					    </siteMapNode>";
			#endregion
			ds.Data = xml_text;
			return ds;
		}

		static void MenuItemDataBound_Event (object sender, MenuEventArgs e)
		{
			WebTest.CurrentTest.UserData = "MenuItemDataBound"; 
		}

		[Test]
		public void MenuItemCollection1 ()
		{
			Menu m = new Menu ();
			fillMenu (m);

			((IStateManager) m.Items).TrackViewState ();
			m.Items [0].Text = "root";
			m.Items [0].ChildItems [0].Text = "node";
			m.Items [0].ChildItems [0].ChildItems [0].Text = "subnode";
			object state = ((IStateManager) m.Items).SaveViewState ();

			Menu copy = new Menu ();
			fillMenu (copy);
			((IStateManager) copy.Items).TrackViewState ();
			((IStateManager) copy.Items).LoadViewState (state);

			Assert.AreEqual (1, copy.Items.Count);
			Assert.AreEqual (2, copy.Items [0].ChildItems.Count);
			Assert.AreEqual (1, copy.Items [0].ChildItems [0].ChildItems.Count);

			Assert.AreEqual ("root", copy.Items [0].Text);
			Assert.AreEqual ("node", copy.Items [0].ChildItems [0].Text);
			Assert.AreEqual ("subnode", copy.Items [0].ChildItems [0].ChildItems [0].Text);
		}
		
		[Test]
		public void MenuItemCollection2 ()
		{
			Menu m = new Menu ();
			fillMenu (m);

			((IStateManager) m.Items).TrackViewState ();
			m.Items [0].Text = "root";
			m.Items [0].ChildItems [0].Text = "node";
			m.Items [0].ChildItems [0].ChildItems [0].Text = "subnode";
			m.Items.Add (new MenuItem ("root 2"));
			object state = ((IStateManager) m.Items).SaveViewState ();

			Menu copy = new Menu ();
			fillMenu (copy);
			((IStateManager) copy.Items).TrackViewState ();
			((IStateManager) copy.Items).LoadViewState (state);

			Assert.AreEqual (2, copy.Items.Count);
			Assert.AreEqual (2, copy.Items [0].ChildItems.Count);
			Assert.AreEqual (1, copy.Items [0].ChildItems [0].ChildItems.Count);

			Assert.AreEqual ("root", copy.Items [0].Text);
			Assert.AreEqual ("node", copy.Items [0].ChildItems [0].Text);
			Assert.AreEqual ("subnode", copy.Items [0].ChildItems [0].ChildItems [0].Text);
			Assert.AreEqual ("root 2", copy.Items [1].Text);
		}

		[Test]
		public void MenuItemCollection3 ()
		{
			Menu m = new Menu ();
			fillMenu (m);
			m.Items.Add (new MenuItem ("root 2"));

			((IStateManager) m.Items).TrackViewState ();
			m.Items [0].Text = "root";
			m.Items [0].ChildItems [0].Text = "node";
			m.Items [0].ChildItems [0].ChildItems [0].Text = "subnode";
			m.Items.RemoveAt (1);
			object state = ((IStateManager) m.Items).SaveViewState ();

			Menu copy = new Menu ();
			fillMenu (copy);
			copy.Items.Add (new MenuItem ("root 2"));
			((IStateManager) copy.Items).TrackViewState ();
			((IStateManager) copy.Items).LoadViewState (state);

			Assert.AreEqual (1, copy.Items.Count);
			Assert.AreEqual (2, copy.Items [0].ChildItems.Count);
			Assert.AreEqual (1, copy.Items [0].ChildItems [0].ChildItems.Count);

			Assert.AreEqual ("root", copy.Items [0].Text);
			Assert.AreEqual ("node", copy.Items [0].ChildItems [0].Text);
			Assert.AreEqual ("subnode", copy.Items [0].ChildItems [0].ChildItems [0].Text);
		}
		
		[Test]
		public void MenuItemCollection4 ()
		{
			Menu m = new Menu ();
			fillMenu (m);
			m.Items.Add (new MenuItem ("root 2"));
			m.Items [0].ChildItems.RemoveAt (1);

			((IStateManager) m.Items).TrackViewState ();
			m.Items [0].Text = "root";
			m.Items [0].ChildItems [0].Text = "node";
			m.Items [0].ChildItems [0].ChildItems [0].Text = "subnode";
			object state = ((IStateManager) m.Items).SaveViewState ();

			Menu copy = new Menu ();
			fillMenu (copy);
			copy.Items.Add (new MenuItem ("root 2"));
			copy.Items [0].ChildItems.RemoveAt (1);
			((IStateManager) copy.Items).TrackViewState ();
			((IStateManager) copy.Items).LoadViewState (state);

			Assert.AreEqual (2, copy.Items.Count);
			Assert.AreEqual (1, copy.Items [0].ChildItems.Count);
			Assert.AreEqual (1, copy.Items [0].ChildItems [0].ChildItems.Count);

			Assert.AreEqual ("root", copy.Items [0].Text);
			Assert.AreEqual ("node", copy.Items [0].ChildItems [0].Text);
			Assert.AreEqual ("subnode", copy.Items [0].ChildItems [0].ChildItems [0].Text);
		}

		[Test]
		public void MenuItemCollection5 ()
		{
			Menu m = new Menu ();
			((IStateManager) m.Items).TrackViewState ();
			fillMenu (m);

			object state = ((IStateManager) m.Items).SaveViewState ();

			Menu copy = new Menu ();
			((IStateManager) copy.Items).TrackViewState ();
			((IStateManager) copy.Items).LoadViewState (state);

			Assert.AreEqual (1, copy.Items.Count);
			Assert.AreEqual (2, copy.Items [0].ChildItems.Count);
			Assert.AreEqual (1, copy.Items [0].ChildItems [0].ChildItems.Count);
		}

		private static void fillMenu (Menu m) {
			m.Items.Clear ();
			m.Items.Add (new MenuItem ());
			m.Items [0].ChildItems.Add (new MenuItem ());
			m.Items [0].ChildItems.Add (new MenuItem ());
			m.Items [0].ChildItems [0].ChildItems.Add (new MenuItem ());
		}

		[Test]
		public void MenuItem_TextValue1 ()
		{
			MenuItem item = new MenuItem ();
			item.Text = "TTT";
			Assert.AreEqual ("TTT", item.Value, "MenuItem_TextValue1#1");
			item.Value = "";
			Assert.AreEqual ("", item.Value, "MenuItem_TextValue1#2");
			item.Value = null;
			Assert.AreEqual ("TTT", item.Value, "MenuItem_TextValue1#3");
		}

		[Test]
		public void MenuItem_TextValue2 ()
		{
			MenuItem item = new MenuItem ();
			item.Value = "VVV";
			Assert.AreEqual ("VVV", item.Text, "MenuItem_TextValue2#1");
			item.Text = "";
			Assert.AreEqual ("", item.Text, "MenuItem_TextValue2#2");
			item.Text = null;
			Assert.AreEqual ("VVV", item.Text, "MenuItem_TextValue2#3");
		}
	}
}
#endif
