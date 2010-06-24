//
// Tests for System.Web.UI.WebControls.SiteMapPath.cs
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
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Threading;
using System.Drawing;
using System.Configuration;
using MyWebControl = System.Web.UI.WebControls;
using MonoTests.SystemWeb.Framework;
using MonoTests.stand_alone.WebHarness;
using System.Collections;

namespace MonoTests.System.Web.UI.WebControls
{
	class PokerSiteMapPath : SiteMapPath
	{
		public PokerSiteMapPath ()
		{
			TrackViewState ();
		}
		public StateBag StateBag
		{
			get { return base.ViewState; }
		}
		public object SaveState ()
		{
			return SaveViewState ();
		}
		public void LoadState (object o)
		{
			LoadViewState (o);
		}
		public void InitilizeItems (SiteMapNodeItem I)
		{
			InitializeItem (I);
		}
		public void DoCreateControlHierarchy ()
		{
			CreateControlHierarchy ();
		}
		public void DoOnDataBinding (EventArgs e)
		{
			base.OnDataBinding (e);
		}
		public void DoOnItemDataBound (SiteMapNodeItemEventArgs e)
		{
			base.OnItemDataBound (e);
		}
		public void DoOnItemCteated (SiteMapNodeItemEventArgs e)
		{
			base.OnItemCreated (e);
		}
	}
	

	[Serializable]
	[TestFixture]
	public class SiteMapPathTest
	{
		[TestFixtureSetUp]
		public void Set_Up ()
		{
			WebTest.CopyResource (GetType (), "Web.sitemap", "Web.sitemap");
		}

		[SetUp]
		public void SetupTestCase ()
		{
			Thread.Sleep (100);
		}
			
		[Test]
		public void SiteMapPath_DefaultProperties ()
		{
			PokerSiteMapPath p = new PokerSiteMapPath ();
			Assert.IsTrue (p.CurrentNodeStyle.IsEmpty, "CurrentNodeStyle");
			Assert.IsNull (p.CurrentNodeTemplate, "CurrentNodeTemplate");
			Assert.IsTrue (p.NodeStyle.IsEmpty, "NodeStyle");
			Assert.IsNull (p.NodeTemplate, "NodeTemplate");
			Assert.AreEqual (-1, p.ParentLevelsDisplayed, "ParentLevelsDisplayed");
			Assert.AreEqual (PathDirection.RootToCurrent, p.PathDirection, "PathDirection");
			Assert.IsNull (p.PathSeparatorTemplate, "PathSeparatorTemplate");
			Assert.IsFalse (p.RenderCurrentNodeAsLink, "RenderCurrentNodeAsLink");
			Assert.IsTrue (p.RootNodeStyle.IsEmpty, "RootNodeStyle");
			Assert.IsNull (p.RootNodeTemplate, "RootNodeTemplate");
			Assert.IsTrue (p.ShowToolTips, "ShowToolTips");
			Assert.AreEqual ("", p.SiteMapProvider, "SiteMapProvider");
			Assert.AreEqual ("Skip Navigation Links", p.SkipLinkText, "Skip Navigation Links");
		}
		
		[Test]
		public void SiteMapPath_DefaultNotWorkingProperties()
		{
			PokerSiteMapPath p = new PokerSiteMapPath ();
			Assert.AreEqual (" > ", p.PathSeparator, "PathSeparator");
		}

		[Test]
		public void SiteMapPath_ChangeProperties ()
		{
			PokerSiteMapPath p = new PokerSiteMapPath ();
			p.ShowToolTips = false;
			Assert.IsFalse (p.ShowToolTips, "ShowToolTips");

			Style currentNodeStyle = new Style ();
			p.CurrentNodeStyle.ForeColor = Color.AliceBlue;
			Assert.AreEqual (Color.AliceBlue, p.CurrentNodeStyle.ForeColor, "CurrentNodeStyle");

			Style NodeStyle = new Style ();
			p.NodeStyle.BackColor = Color.Aqua;
			Assert.AreEqual (Color.Aqua, p.NodeStyle.BackColor, "NodeStyle");

			p.PathDirection = PathDirection.CurrentToRoot;
			Assert.AreEqual (PathDirection.CurrentToRoot, p.PathDirection, "PathDirection");

			p.PathSeparator = " - ";
			Assert.AreEqual (" - ", p.PathSeparator, "PathSeparator");

			Style RootNodeStyle = new Style ();
			p.RootNodeStyle.BackColor = Color.Red;
			Assert.IsFalse (p.RootNodeStyle.IsEmpty, "RootNodeStyle#1");
			Assert.AreEqual (Color.Red, p.RootNodeStyle.BackColor, "RootNodeStyle#2");

			p.ParentLevelsDisplayed = 2;
			Assert.AreEqual (2, p.ParentLevelsDisplayed, "ParentLevelsDisplayed");

			p.RenderCurrentNodeAsLink = true;
			Assert.IsTrue (p.RenderCurrentNodeAsLink, "RenderCurrentNodeAsLink");

			p.SiteMapProvider = "test";
			Assert.AreEqual ("test", p.SiteMapProvider, "SiteMapProvider");

			p.SkipLinkText = "test";
			Assert.AreEqual ("test", p.SkipLinkText, "Skip Navigation Links");

			//programmatically create template
			MyWebControl.Image myImage = new MyWebControl.Image ();
			myImage.ImageUrl = "myimage.jpg";
			ImageTemplate rootNodeImageTemplate = new ImageTemplate ();
			rootNodeImageTemplate.MyImage = myImage;
			// end create template image
			p.RootNodeTemplate = rootNodeImageTemplate;
			Assert.IsNotNull (p.RootNodeTemplate, "RootNodeTemplate");
			Assert.AreEqual (rootNodeImageTemplate, p.RootNodeTemplate, "RootNodeTemplate");

			p.NodeTemplate = rootNodeImageTemplate;
			Assert.IsNotNull (p.NodeTemplate, "NodeTemplate");
			Assert.AreEqual (rootNodeImageTemplate, p.NodeTemplate, "NodeTemplate");

			p.CurrentNodeTemplate = rootNodeImageTemplate;
			Assert.IsNotNull (p.CurrentNodeTemplate, "RootNodeTemplate");
			Assert.AreEqual (rootNodeImageTemplate, p.CurrentNodeTemplate, "RootNodeTemplate");
		}
		[Test]
		public void SiteMapPath_NullProperties ()
		{
			PokerSiteMapPath p = new PokerSiteMapPath ();
			p.ShowToolTips = false;
			Assert.IsFalse (p.ShowToolTips, "ShowToolTips");
			Assert.AreEqual (1, p.StateBag.Count, "NullProperties#1");
			p.PathDirection = PathDirection.CurrentToRoot;
			Assert.AreEqual (PathDirection.CurrentToRoot, p.PathDirection, "PathDirection");
			Assert.AreEqual (2, p.StateBag.Count, "NullProperties#2");
			p.PathSeparator = " - ";
			Assert.AreEqual (3, p.StateBag.Count, "NullProperties#3");
			p.SiteMapProvider = "test";
			Assert.AreEqual (4, p.StateBag.Count, "NullProperties#4");
			p.SkipLinkText = "test";
			Assert.AreEqual (5, p.StateBag.Count, "NullProperties#5");
			p.SkipLinkText = null;
		}

		// Rendering tests

		[Test]
		[Category ("NunitWeb")]
		public void SiteMapPath_RenderProperty ()
		{
			string RenderedPageHtml = new WebTest (PageInvoker.CreateOnLoad (DoTestPropertyRender)).Run ();
			string RenderedControlHtml = HtmlDiff.GetControlFromPageHtml (RenderedPageHtml);
			string OriginControlHtml = "<span style=\"display:inline-block;color:Red;background-color:Red;border-color:Red;border-width:3px;border-style:Dashed;\"><a href=\"#ctl01_SkipLink\"><img alt=\"Skip Navigation Links\" height=\"0\" width=\"0\" src=\"/NunitWeb/WebResource.axd?d=4RHYfeNnynkXiM59uthjZg2&amp;t=633802729995006876\" style=\"border-width:0px;\" /></a><span>node1</span><span>-</span><span><a title=\"test\" href=\"/NunitWeb/MyPageWithMaster.aspx\">root</a></span><a id=\"ctl01_SkipLink\"></a></span>";
			HtmlDiff.AssertAreEqual(OriginControlHtml,RenderedControlHtml,"RenderProperty");
		}
		[Test]
		[Category ("NunitWeb")]
		public void SiteMapPath_RenderStyles ()
		{
			string RenderedPageHtml = new WebTest (PageInvoker.CreateOnLoad (DoTestStylesRender)).Run ();
			string RenderedControlHtml = HtmlDiff.GetControlFromPageHtml (RenderedPageHtml);
			string OriginControlHtml = "<span style=\"background-color:Red;\"><a href=\"#ctl01_SkipLink\"><img alt=\"Skip Navigation Links\" height=\"0\" width=\"0\" src=\"/NunitWeb/WebResource.axd?d=4RHYfeNnynkXiM59uthjZg2&amp;t=633802729995006876\" style=\"border-width:0px;\" /></a><span><a title=\"test\" href=\"/NunitWeb/MyPageWithMaster.aspx\" style=\"background-color:Beige;border-color:Purple;\">root</a></span><span style=\"background-color:RoyalBlue;\"> &gt; </span><span style=\"background-color:Pink;border-color:Purple;\">node1</span><a id=\"ctl01_SkipLink\"></a></span>";
			HtmlDiff.AssertAreEqual (OriginControlHtml, RenderedControlHtml,"RenderStyles");
		}
		[Test]
		[Category ("NunitWeb")]
		public void SiteMapPath_DefaultRender()
		{
			string RenderedPageHtml = new WebTest (PageInvoker.CreateOnLoad (DoTestDefaultRender)).Run ();
			string RenderedControlHtml = HtmlDiff.GetControlFromPageHtml (RenderedPageHtml);
			string OriginControlHtml = "<span><a href=\"#ctl01_SkipLink\"><img alt=\"Skip Navigation Links\" height=\"0\" width=\"0\" src=\"/NunitWeb/WebResource.axd?d=4RHYfeNnynkXiM59uthjZg2&amp;t=633802729995006876\" style=\"border-width:0px;\" /></a><span><a title=\"test\" href=\"/NunitWeb/MyPageWithMaster.aspx\">root</a></span><span> &gt; </span><span>node1</span><a id=\"ctl01_SkipLink\"></a></span>";
			HtmlDiff.AssertAreEqual (OriginControlHtml, RenderedControlHtml,"RenderDefault");
		}

		/// <summary>
		/// All this methods are delegates for running tests in host assembly. 
		/// </summary>
		
		public static void DoTestDefaultRender (Page p)
		{
			LiteralControl lcb = new LiteralControl (HtmlDiff.BEGIN_TAG);
			LiteralControl lce = new LiteralControl (HtmlDiff.END_TAG);
			SiteMapPath smp = new SiteMapPath ();
			p.Form.Controls.Add (lcb);
			p.Form.Controls.Add (smp);
			p.Form.Controls.Add (lce);
		}
		public static void DoTestPropertyRender (Page p)
		{
			SiteMapPath smp = new SiteMapPath ();
			smp.BackColor = Color.Red;
			smp.BorderColor = Color.Red;
			smp.BorderStyle = BorderStyle.Dashed;
			smp.BorderWidth = 3;
			smp.ForeColor = Color.Red;
			smp.PathDirection = PathDirection.CurrentToRoot;
			smp.PathSeparator = "-";

			LiteralControl lcb = new LiteralControl (HtmlDiff.BEGIN_TAG);
			LiteralControl lce = new LiteralControl (HtmlDiff.END_TAG);
			p.Form.Controls.Add (lcb);
			p.Form.Controls.Add (smp);
			p.Form.Controls.Add (lce);
		}
		public static void DoTestStylesRender (Page p)
		{
			PokerSiteMapPath smp = new PokerSiteMapPath ();
			smp.ControlStyle.BackColor = Color.Red;
			smp.CurrentNodeStyle.BackColor = Color.Pink;
			smp.NodeStyle.BorderColor = Color.Purple;
			smp.PathSeparatorStyle.BackColor = Color.RoyalBlue;
			smp.RootNodeStyle.BackColor = Color.Beige;
			LiteralControl lcb = new LiteralControl (HtmlDiff.BEGIN_TAG);
			LiteralControl lce = new LiteralControl (HtmlDiff.END_TAG);
			p.Form.Controls.Add (lcb);
			p.Form.Controls.Add (smp);
			p.Form.Controls.Add (lce);
		}
		[Test]
		public void SiteMapPath_ViewState ()
		{
			PokerSiteMapPath p = new PokerSiteMapPath ();
			p.ShowToolTips = false;
			Style currentNodeStyle = new Style ();
			p.CurrentNodeStyle.ForeColor = Color.AliceBlue;
			Style NodeStyle = new Style ();
			p.NodeStyle.BackColor = Color.Aqua;
			p.PathDirection = PathDirection.CurrentToRoot;
			Assert.AreEqual (PathDirection.CurrentToRoot, p.PathDirection, "PathDirection");
			p.PathSeparator = " - ";
			Style RootNodeStyle = new Style ();
			p.RootNodeStyle.BackColor = Color.Red;
			p.ParentLevelsDisplayed = 2;
			p.RenderCurrentNodeAsLink = true;
			p.SiteMapProvider = "test";
			p.SkipLinkText = "test";

			object state = p.SaveState ();
			PokerSiteMapPath copy = new PokerSiteMapPath ();
			copy.LoadState (state);
			Assert.IsFalse (copy.ShowToolTips, "ShowToolTips");
			Assert.AreEqual (Color.AliceBlue, copy.CurrentNodeStyle.ForeColor, "CurrentNodeStyle");
			Assert.AreEqual (Color.Aqua, p.NodeStyle.BackColor, "NodeStyle");
			Assert.AreEqual (" - ", p.PathSeparator, "PathSeparator");
			Assert.IsFalse (p.RootNodeStyle.IsEmpty, "RootNodeStyle#1");
			Assert.AreEqual (Color.Red, p.RootNodeStyle.BackColor, "RootNodeStyle#2");
			Assert.AreEqual (2, p.ParentLevelsDisplayed, "ParentLevelsDisplayed");
			Assert.IsTrue (p.RenderCurrentNodeAsLink, "RenderCurrentNodeAsLink");
			Assert.AreEqual ("test", p.SiteMapProvider, "SiteMapProvider");
			Assert.AreEqual ("test", p.SkipLinkText, "Skip Navigation Links");
		}
		
		[Test]
		[Category ("NunitWeb")]
		public void SiteMapPath_InitializeItemCurrent ()
		{
			new WebTest (PageInvoker.CreateOnLoad (InitializeItemCurrent)).Run ();
		}

		public static void InitializeItemCurrent (Page p)
		{
			PokerSiteMapPath smp = new PokerSiteMapPath ();
			SiteMapNodeItem I = new SiteMapNodeItem (1, SiteMapNodeItemType.Current);
			smp.CurrentNodeStyle.BackColor = Color.Red;
			smp.NodeStyle.BorderColor = Color.Red;
			smp.DoCreateControlHierarchy ();
			I.SiteMapNode = smp.Provider.CurrentNode;
			smp.InitilizeItems (I);
			
			Assert.AreEqual (1, I.Controls.Count, "InitializeItem#1");
			Assert.AreEqual (typeof (Literal), I.Controls[0].GetType (), "InitializeItem#2");
		        Assert.AreEqual (Color.Red, I.BackColor , "InitializeItem#3");
			Assert.AreEqual (Color.Red, I.BorderColor, "InitializeItem#4");

			I.Controls.Clear ();
			smp.RenderCurrentNodeAsLink = true;
			smp.InitilizeItems (I);
			
			Assert.AreEqual (1, I.Controls.Count, "InitializeItem#5");
			Assert.AreEqual (typeof (HyperLink), I.Controls[0].GetType (), "InitializeItem#6");
			Assert.AreEqual (Color.Red, I.BackColor, "InitializeItem#7");
			Assert.AreEqual (Color.Red, I.BorderColor, "InitializeItem#8");
		}

		[Test]
		[Category ("NunitWeb")]
		public void SiteMapPath_InitializeItemRoot ()
		{
			new WebTest (PageInvoker.CreateOnLoad (InitializeItemRoot)).Run ();
		}

		public static void InitializeItemRoot (Page p)
		{
			PokerSiteMapPath smp = new PokerSiteMapPath ();
			SiteMapNodeItem I = new SiteMapNodeItem (0, SiteMapNodeItemType.Root);
			smp.RootNodeStyle.BackColor = Color.Red;
			smp.NodeStyle.BorderColor = Color.Red;
			smp.DoCreateControlHierarchy ();
			I.SiteMapNode = smp.Provider.RootNode;
			smp.InitilizeItems (I);

			Assert.AreEqual (1, I.Controls.Count, "InitializeItem#1");
			Assert.AreEqual (typeof (HyperLink), I.Controls[0].GetType (), "InitializeItem#2");
			Assert.AreEqual (Color.Red, ((HyperLink)I.Controls[0]).ControlStyle.BackColor, "InitializeItem#3");
			Assert.AreEqual (Color.Red, ((HyperLink)I.Controls[0]).ControlStyle.BorderColor, "InitializeItem#4");
		}

		[Test]
		[Category ("NunitWeb")]
		public void SiteMapPath_InitializeItemParent ()
		{
			new WebTest (PageInvoker.CreateOnLoad (InitializeItemParent)).Run ();
		}

		public static void InitializeItemParent (Page p)
		{
			PokerSiteMapPath smp = new PokerSiteMapPath ();
			SiteMapNodeItem I = new SiteMapNodeItem (0, SiteMapNodeItemType.Parent);
			smp.NodeStyle.BorderColor = Color.Red;
			smp.DoCreateControlHierarchy ();
			I.SiteMapNode = smp.Provider.RootNode;
			smp.InitilizeItems (I);

			Assert.AreEqual (1, I.Controls.Count, "InitializeItem#1");
			Assert.AreEqual (typeof (HyperLink), I.Controls[0].GetType (), "InitializeItem#2");
			Assert.AreEqual (Color.Red, ((HyperLink) I.Controls[0]).ControlStyle.BorderColor, "InitializeItem#4");
		}

		[Test]
		[Category ("NunitWeb")]
		public void SiteMapPath_InitializeItemPathSeparator ()
		{
			new WebTest (PageInvoker.CreateOnLoad (InitializeItemPathSeparator)).Run ();
		}

		public static void InitializeItemPathSeparator (Page p)
		{
			PokerSiteMapPath smp = new PokerSiteMapPath ();
			SiteMapNodeItem I = new SiteMapNodeItem (0, SiteMapNodeItemType.PathSeparator);
			smp.PathSeparatorStyle.BorderColor = Color.Red;
			smp.InitilizeItems (I);
			Assert.AreEqual (1, I.Controls.Count, "InitializeItem#1");
			Assert.AreEqual (typeof (Literal), I.Controls[0].GetType (), "InitializeItem#2");
			Assert.AreEqual (Color.Red, I.BorderColor, "InitializeItem#3");
		}

		[Test]
		[Category ("NunitWeb")]
		public void SiteMapPath_InitializeItemTemplates ()
		{
			new WebTest (PageInvoker.CreateOnLoad (InitializeItemTemplates)).Run ();
		}

		public static void InitializeItemTemplates (Page p)
		{
			PokerSiteMapPath smp = new PokerSiteMapPath ();
			SiteMapNodeItem I = new SiteMapNodeItem (1, SiteMapNodeItemType.Current);
			smp.CurrentNodeTemplate = new CompiledTemplateBuilder (templatebuilder);
			smp.CurrentNodeStyle.BackColor = Color.Red;
			smp.DoCreateControlHierarchy ();
			I.SiteMapNode = smp.Provider.CurrentNode;
			smp.InitilizeItems (I);

			Assert.AreEqual (1, I.Controls.Count, "InitializeItem#1");
			Assert.AreEqual (typeof (TextBox), I.Controls[0].GetType (), "InitializeItem#2");
			Assert.AreEqual (Color.Red, I.BackColor, "InitializeItem#3");
		}

		private static void templatebuilder (Control container)
		{
			TextBox ctrl;
			ctrl = new TextBox ();
			ctrl.ID = "TextBox1";
			container.Controls.Add (ctrl);
		}
		

		[Test]
		[Category ("NunitWeb")]
		public void SiteMapPath_SiteMapRootNode ()
		{
			new WebTest (PageInvoker.CreateOnLoad (SiteMapRootNode)).Run ();
		}

		[Test]
		[Category ("NunitWeb")]
		public void SiteMapPath_SiteMapChildNode ()
		{
			new WebTest (PageInvoker.CreateOnLoad (InitializeItemPathSeparator)).Run ();
		}
		public static void SiteMapRootNode (Page p)
		{
			PokerSiteMapPath smp = new PokerSiteMapPath ();
			Assert.AreEqual ("root", smp.Provider.RootNode.Title, "RootNode");
		}

		public static void SiteMapChildNode (Page p)
		{
			PokerSiteMapPath smp = new PokerSiteMapPath ();
			SiteMapNodeCollection myCol = smp.Provider.GetChildNodes (smp.Provider.RootNode);
			Assert.AreEqual (1, myCol.Count, "SiteMapChildNode#1");
		}


		[Test]
		[Category ("NunitWeb")]
		[Category ("NotWorking")]
		public void SiteMapPath_FireEvents()
		{
			WebTest t = new WebTest (PageInvoker.CreateOnInit(FireEvents_Init));
			t.Run ();
			ArrayList eventlist = t.UserData as ArrayList;
			if (eventlist == null)
				Assert.Fail ("User data does not been created fail");

			Assert.AreEqual ("ItemCreated", eventlist[0], "#1");
			Assert.AreEqual ("ItemDataBound", eventlist[1], "#2");
			Assert.AreEqual ("ItemCreated", eventlist[2], "#3");
			Assert.AreEqual ("ItemDataBound", eventlist[3], "#4");
			Assert.AreEqual ("ItemCreated", eventlist[4], "#5");
			Assert.AreEqual ("ItemDataBound", eventlist[5], "#6");
		}

		#region FireEvents
		public static void FireEvents_Init (Page p)
		{
			SiteMapPath s = new SiteMapPath ();
			s.ItemCreated += new SiteMapNodeItemEventHandler (s_ItemCreated);
			s.ItemDataBound += new SiteMapNodeItemEventHandler (s_ItemDataBound);
			p.Form.Controls.Add (s);
		}

		static void s_ItemDataBound (object sender, SiteMapNodeItemEventArgs e)
		{
			if (WebTest.CurrentTest.UserData == null) {
				ArrayList list = new ArrayList ();
				list.Add ("ItemDataBound");
				WebTest.CurrentTest.UserData = list;
			}
			else {
				ArrayList list = WebTest.CurrentTest.UserData as ArrayList;
				if (list == null)
					throw new NullReferenceException ();
				list.Add ("ItemDataBound");
				WebTest.CurrentTest.UserData = list;
			}
		}

		static void s_ItemCreated (object sender, SiteMapNodeItemEventArgs e)
		{
			if (WebTest.CurrentTest.UserData == null) {
				ArrayList list = new ArrayList ();
				list.Add ("ItemCreated");
				WebTest.CurrentTest.UserData = list;
			}
			else {
				ArrayList list = WebTest.CurrentTest.UserData as ArrayList;
				if (list == null)
					throw new NullReferenceException ();
				list.Add ("ItemCreated");
				WebTest.CurrentTest.UserData = list;
			}
		}
		#endregion

		// Events Stuff
		private bool DataBinding;
		private bool ItemDataBounding;
		private bool ItemCreated;

		private void DataBindingHandler (object sender, EventArgs e)
		{
			DataBinding = true;
		}
		private void ItemDataBoundHandler (object sender, SiteMapNodeItemEventArgs e)
		{
			ItemDataBounding = true;
		}
		private void ItemCreatedHandler (object sender, SiteMapNodeItemEventArgs e)
		{
			ItemCreated = true;
		}
		private void ResetEvents ()
		{
			DataBinding = false;
			ItemDataBounding = false;
			ItemCreated = false;
		}

		[Test]
		[Category ("NunitWeb")]
		public void SiteMapPath_Events ()
		{
			new WebTest (PageInvoker.CreateOnLoad (Events)).Run ();
		}

		public void Events (Page p)
		{
			PokerSiteMapPath smp = new PokerSiteMapPath ();
			ResetEvents ();
			smp.DataBinding += new EventHandler (DataBindingHandler);
			smp.ItemDataBound += new SiteMapNodeItemEventHandler (ItemDataBoundHandler);
			smp.ItemCreated += new SiteMapNodeItemEventHandler (ItemCreatedHandler);

			Assert.AreEqual (false, DataBinding, "BeforeDataBinding");
			smp.DoOnDataBinding (new EventArgs ());
			Assert.AreEqual (true, DataBinding, "AfterDataBinding");

			ResetEvents ();
			Assert.AreEqual (false, ItemDataBounding, "BeforeItemDataBound");
			SiteMapNodeItem i = new SiteMapNodeItem (0, SiteMapNodeItemType.Root);
			smp.DoOnItemDataBound (new SiteMapNodeItemEventArgs (i));
			Assert.AreEqual (true, ItemDataBounding, "AfterItemDataBound");

			ResetEvents ();
			SiteMapNodeItemEventArgs MyArgs = new SiteMapNodeItemEventArgs (new SiteMapNodeItem(0,SiteMapNodeItemType.Parent));
			Assert.AreEqual (false, ItemCreated, "BeforeItemCreated");
			smp.DoOnItemCteated (MyArgs);
			Assert.AreEqual (true, ItemCreated, "AfterItemCreated");
		}

		//[Test]
		//[Category ("NotWorking")]  //throws System.IndexOutOfRangeException : Array index is out of range
		//[ExpectedException (typeof (ConfigurationErrorsException))]
		//public void SiteMapPath_CreateControlHierarchy ()
		//{
		//    PokerSiteMapPath p = new PokerSiteMapPath ();
		//    p.DoCreateControlHierarchy ();
		//}
		//[Test]
		//[Category ("NotWorking")]  //throws System.IndexOutOfRangeException : Array index is out of range
		//[ExpectedException (typeof (ConfigurationErrorsException))]
		//public void SiteMapPath_DataBindExeption ()
		//{
		//    PokerSiteMapPath p = new PokerSiteMapPath ();
		//    p.DataBind ();
		//}


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
	}
}
#endif
