//
// Tests for System.Web.UI.WebControls.ThemeTest.cs
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

// Additional resources :
// PageWithStyleSheet.aspx; PageWithStyleSheet.aspx.cs;RunTimeSetTheme.aspx;
// RunTimeSetTheme.aspx.cs; PageWithTheme.aspx; PageWithTheme.aspx.cs; Theme1.skin

#if NET_2_0

using System;
using System.Drawing;
using System.IO;
using System.Configuration;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Threading;
using MyWebControl = System.Web.UI.WebControls;
using System.Reflection;
using NUnit.Framework;
using MonoTests.SystemWeb.Framework;

namespace MonoTests.System.Web.UI.WebControls
{
	[Serializable]
	[TestFixture]
	public class ThemeTest
	{	
		[TestFixtureSetUp]
		public void Set_Up ()
		{
#if VISUAL_STUDIO
			WebTest.CopyResource (GetType (), "MonoTests.System.Web.UI.WebControls.Resources.Theme1.skin", "App_Themes/Theme1/Theme1.skin");
			WebTest.CopyResource (GetType (), "MonoTests.System.Web.UI.WebControls.Resources.WizardTest.skin", "App_Themes/Theme1/WizardTest.skin");
			WebTest.CopyResource (GetType (), "MonoTests.System.Web.UI.WebControls.Resources.PageWithStyleSheet.aspx", "PageWithStyleSheet.aspx");
			WebTest.CopyResource (GetType (), "MonoTests.System.Web.UI.WebControls.Resources.PageWithTheme.aspx", "PageWithTheme.aspx");
			WebTest.CopyResource (GetType (), "MonoTests.System.Web.UI.WebControls.Resources.RunTimeSetTheme.aspx", "RunTimeSetTheme.aspx");
			WebTest.CopyResource (GetType (), "MonoTests.System.Web.UI.WebControls.Resources.UrlProperty.aspx", "UrlProperty.aspx");
			WebTest.CopyResource (GetType (), "MonoTests.System.Web.UI.WebControls.Resources.UrlProperty.ascx", "UrlProperty.ascx");
			WebTest.CopyResource (GetType (), "MonoTests.System.Web.UI.WebControls.Resources.UrlProperty.ascx.cs", "UrlProperty.ascx.cs");
			
#else
			WebTest.CopyResource (GetType (), "Theme1.skin", "App_Themes/Theme1/Theme1.skin");
			WebTest.CopyResource (GetType (), "WizardTest.skin", "App_Themes/Theme1/WizardTest.skin");
			WebTest.CopyResource (GetType (), "PageWithStyleSheet.aspx", "PageWithStyleSheet.aspx");
			WebTest.CopyResource (GetType (), "PageWithTheme.aspx", "PageWithTheme.aspx");
			WebTest.CopyResource (GetType (), "RunTimeSetTheme.aspx", "RunTimeSetTheme.aspx");
			WebTest.CopyResource (GetType (), "UrlProperty.aspx", "UrlProperty.aspx");
			WebTest.CopyResource (GetType (), "UrlProperty.ascx", "UrlProperty.ascx");
			WebTest.CopyResource (GetType (), "UrlProperty.ascx.cs", "UrlProperty.ascx.cs");
			
#endif
		}


		[SetUp]
		public void SetupTestCase ()
		{
			Thread.Sleep (100);
		}
		
		//Run on page with theme

		[Test]
		[Category ("NunitWeb")]
		public void Theme_TestLabelTheme ()
		{
			WebTest t = new WebTest ("PageWithTheme.aspx");
			t.Invoker = PageInvoker.CreateOnLoad (RenderLabelTest);
			t.Run ();
		}

		public static void RenderLabelTest (Page p)
		{
			Assert.AreEqual (Color.Black,((MyWebControl.Label) p.FindControl ("Label")).BackColor, "Default Theme#1");
			Assert.AreEqual (Color.Red, ((MyWebControl.Label) p.FindControl ("LabelRed")).BackColor, "Red Skin Theme#2");
			Assert.AreEqual (Color.Yellow, ((MyWebControl.Label) p.FindControl ("LabelYellow")).BackColor, "Yellow Skin Theme#3");
			Assert.AreEqual (Color.Black, ((MyWebControl.Label) p.FindControl ("LabelOverride")).BackColor, "Override Skin Theme#4");
		}

		[Test]
		[Category ("NunitWeb")]
		public void Theme_TestImageTheme ()
		{
			WebTest t = new WebTest ("PageWithTheme.aspx");
			t.Invoker = PageInvoker.CreateOnLoad (RenderImageTest);
			t.Run ();
		}

		public static void RenderImageTest (Page p)
		{
			Assert.IsTrue (((MyWebControl.Image) p.FindControl ("Image")).ImageUrl.IndexOf ("myimageurl") >= 0, "Default Theme#1");
			Assert.IsTrue (((MyWebControl.Image) p.FindControl ("ImageRed")).ImageUrl.IndexOf ("myredimageurl") >= 0, "RedImage Theme#2");
			Assert.IsTrue (((MyWebControl.Image) p.FindControl ("ImageYellow")).ImageUrl.IndexOf ("myyellowimageurl") >= 0, "YellowImage Theme#3");
			Assert.IsTrue (((MyWebControl.Image) p.FindControl ("ImageOverride")).ImageUrl.IndexOf ("myimageurl") >= 0, "OverrideImage Theme#3");
		}

		// Run on page with StyleSheet

		[Test]
		[Category ("NunitWeb")]
		public void Theme_TestLabelStyleSheet ()
		{
			WebTest t = new WebTest ("PageWithStyleSheet.aspx");
			t.Invoker = PageInvoker.CreateOnLoad (StyleSheetRenderLabelTest);
			t.Run ();
		}

		public static void StyleSheetRenderLabelTest (Page p)
		{
			Assert.AreEqual (Color.Black, ((MyWebControl.Label) p.FindControl ("Label")).BackColor, "Default Theme#1");
			Assert.AreEqual (Color.Red, ((MyWebControl.Label) p.FindControl ("LabelRed")).BackColor, "Red Skin Theme#2");
			Assert.AreEqual (Color.Yellow, ((MyWebControl.Label) p.FindControl ("LabelYellow")).BackColor, "Yellow Skin Theme#3");
			Assert.AreEqual (Color.White, ((MyWebControl.Label) p.FindControl ("LabelOverride")).BackColor, "Override Skin Theme#4");
		}

		[Test]
		[Category ("NunitWeb")]
		public void Theme_TestImageStyleSheet ()
		{
			WebTest t = new WebTest ("PageWithStyleSheet.aspx");
			t.Invoker = PageInvoker.CreateOnLoad (StyleSheetRenderImageTest);
			t.Run ();
		}

		public static void StyleSheetRenderImageTest (Page p)
		{
			Assert.IsTrue (((MyWebControl.Image) p.FindControl ("Image")).ImageUrl.IndexOf ("myimageurl") >= 0, "Default Theme#1");
			Assert.IsTrue (((MyWebControl.Image) p.FindControl ("ImageRed")).ImageUrl.IndexOf ("myredimageurl") >= 0, "RedImage Theme#2");
			Assert.IsTrue (((MyWebControl.Image) p.FindControl ("ImageYellow")).ImageUrl.IndexOf ("myyellowimageurl") >= 0, "YellowImage Theme#3");
			Assert.IsTrue (((MyWebControl.Image) p.FindControl ("ImageOverride")).ImageUrl.IndexOf ("overridedurl") >= 0, "OverrideImage Theme#3");
		}

		[Test]
		[Category ("NunitWeb")]
		public void Theme_TestRuntimeSetTheme ()
		{
			PageDelegates p = new PageDelegates ();
			p.PreInit = RuntimeSetThemePreInit;
			p.Load = RuntimeSetThemeLoad;
			WebTest t = new WebTest ("RunTimeSetTheme.aspx");
			t.Invoker = new PageInvoker (p);
			t.Run ();
		}

		public static void RuntimeSetThemePreInit (Page p)
		{
			p.Theme = "Theme1";
		}

		public static void RuntimeSetThemeLoad (Page p)
		{
			Assert.AreEqual (Color.Black, ((MyWebControl.Label) p.FindControl ("Label")).BackColor, "Default Theme#1");
			Assert.AreEqual (Color.Red, ((MyWebControl.Label) p.FindControl ("LabelRed")).BackColor, "Red Skin Theme#2");
			Assert.AreEqual (Color.Yellow, ((MyWebControl.Label) p.FindControl ("LabelYellow")).BackColor, "Yellow Skin Theme#3");
			Assert.AreEqual (Color.Black, ((MyWebControl.Label) p.FindControl ("LabelOverride")).BackColor, "Override Skin Theme#4");
			Assert.IsTrue (((MyWebControl.Image) p.FindControl ("Image")).ImageUrl.IndexOf ("myimageurl") >= 0, "Default Theme#1");
			Assert.IsTrue (((MyWebControl.Image) p.FindControl ("ImageRed")).ImageUrl.IndexOf ("myredimageurl") >= 0, "RedImage Theme#2");
			Assert.IsTrue (((MyWebControl.Image) p.FindControl ("ImageYellow")).ImageUrl.IndexOf ("myyellowimageurl") >= 0, "YellowImage Theme#3");
			Assert.IsTrue (((MyWebControl.Image) p.FindControl ("ImageOverride")).ImageUrl.IndexOf ("myimageurl") >= 0, "OverrideImage Theme#3");
		}

		[Test]
		[Category ("NunitWeb")]
		[ExpectedException (typeof (HttpException))]
		public void Theme_SetThemeException ()
		{
			string page=new WebTest (PageInvoker.CreateOnPreInit (SetThemeExeption)).Run ();
			Assert.IsTrue (page.IndexOf("System.Web.HttpException") >= 0, "System.Web.HttpException was expected, actual result: "+page);
		}

		//// Delegate running on Page Load , only before PreInit possible set Theme on running time !
		//[Test]
		//[Category ("NunitWeb")]
		////Use Assert.Fail to print the actual result
		////[ExpectedException (typeof (InvalidOperationException))]
		//[Category ("NotWorking")]
		//public void Theme_SetThemeException ()
		//{
		//        try {
		//                string res=Helper.Instance.RunInPagePreInit (SetThemeExeption);
		//                Assert.Fail ("InvalidOperationException was expected. Result: "+res); 
		//        }
		//        catch (InvalidOperationException e) {
		//                //swallow the expected exception
		//        }
		//}

		public static void SetThemeExeption (Page p)
		{
			p.Theme = "InvalidTheme1";
		}

		[Test]
		[Category ("NunitWeb")]
		public void Theme_EnableTheming ()
		{
			
			PageDelegates pd = new PageDelegates ();
			pd.PreInit = new PageDelegate (SetTheme1);
			pd.Load = new PageDelegate (Theme1Load);
			PageInvoker pi = new PageInvoker (pd);

			string page = new WebTest (pi).Run ();

			Assert.IsTrue (page.IndexOf ("testing") < 0, "Theme_EnableTheming");
		}
		public static void Theme1Load (Page p)
		{
			Table t = new Table ();
			TableRow tr = new TableRow ();
			TableCell cell = new TableCell ();

			cell.Controls.Add (new Button ());
			tr.Cells.Add (cell);
			t.Rows.Add (tr);

			t.EnableTheming = false;
			p.Form.Controls.Add (t);
		}
		public static void SetTheme1 (Page p)
		{
			p.Theme = "Theme1";
		}

		[Test]
		[Category ("NunitWeb")]
		public void Theme_EnableThemingChild ()
		{

			PageDelegates pd = new PageDelegates ();
			pd.PreInit = new PageDelegate (SetTheme1);
			pd.Load = new PageDelegate (Theme1ChildLoad);
			PageInvoker pi = new PageInvoker (pd);

			string page = new WebTest (pi).Run ();

			Assert.IsTrue (page.IndexOf ("testing") > 0, "Theme_EnableThemingChild");
		}
		public static void Theme1ChildLoad (Page p)
		{
			Table t = new Table ();
			TableRow tr = new TableRow ();
			TableCell cell = new TableCell ();

			cell.Controls.Add (new Button ());
			tr.Cells.Add (cell);
			t.Rows.Add (tr);

			t.EnableTheming = false;
			cell.EnableTheming = true;
			p.Form.Controls.Add (t);
		}

		[TestFixtureTearDown]
		public void TearDown ()
		{
			Thread.Sleep (100);
			WebTest.Unload ();
			Thread.Sleep (100);
		}
		
		[Test]
		[Category("NunitWeb")]
		public void UrlPropertyTest ()
		{
			string res = new WebTest ("UrlProperty.aspx").Run ();

			Assert.IsTrue (res.IndexOf ("Property1 = testProp1") != -1,
				"Property1 should be assigned as is, actual result: "+res);
			Assert.IsTrue (res.IndexOf ("UrlProperty2 = ~/App_Themes/Theme1/testProp2") != -1,
				"UrlProperty2 should be assigned including theme subfolder, actual result: "+res);
		}
	}
}
#endif
