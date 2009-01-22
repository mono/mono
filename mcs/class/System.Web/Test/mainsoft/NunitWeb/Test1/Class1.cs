using System;
using System.Text;
using NUnit.Framework;
using MonoTests.SystemWeb.Framework;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Drawing;
using System.Runtime.Serialization;
using System.IO;
using System.Reflection;
using System.Diagnostics;

namespace Test1
{
	[TestFixture]
	public class Class1
	{
		public void TearDownFixture ()
		{
			WebTest.Unload ();
		}

#if NET_2_0
		[Test]
		public void RenderSiteMapPath ()
		{
			PageInvoker pi = PageInvoker.CreateOnLoad (_RenderSiteMapPath);
			string res = new WebTest (pi).Run ();
			Console.WriteLine (res);
			Assert.IsFalse (string.IsNullOrEmpty (res));
		}

		public static void _RenderSiteMapPath (Page p)
		{
			SiteMapPath smp = new SiteMapPath ();
			p.Controls.Add (smp);
		}

		[Test]
		public void RenderSiteMapPathProp ()
		{
			PageInvoker pi = PageInvoker.CreateOnLoad (_RenderSiteMapPathProp);
			string res = new WebTest (pi).Run ();
			Console.WriteLine (res);
			Assert.IsFalse (string.IsNullOrEmpty (res));
		}

		public static void _RenderSiteMapPathProp (Page p)
		{
			SiteMapPath smp = new SiteMapPath ();
			smp.BackColor = Color.Red;
			p.Controls.Add (smp);
		}

		[Test]
		public void TestMasterPage ()
		{
			PageInvoker pi = PageInvoker.CreateOnLoad (_TestMasterPage);
			WebTest t = new WebTest (pi);
			t.Request.Url = StandardUrl.PAGE_WITH_MASTER;
			string res = t.Run ();
			Console.WriteLine (res);
			Assert.IsFalse (string.IsNullOrEmpty (res));
		}

		public static void _TestMasterPage (Page p)
		{
			MasterPage mp = p.Master;
			Assert.IsNotNull (mp);
		}
#endif
		[Test]
		public void TestStyle ()
		{
			string res = new WebTest (PageInvoker.CreateOnLoad (
				new PageDelegate(_TestStyle))).Run ();
			Assert.IsNotNull (res);
			Assert.IsTrue (res != string.Empty);
		}

		public static void _TestStyle (Page p)
		{
			Button b = new Button ();
			b.BackColor = Color.Red;
			b.ID = "Yoni";
			p.Controls.Add (b);
		}

		[Test]
		public void TestDefaultRender ()
		{
			string str = new WebTest (PageInvoker.CreateOnLoad (
				new PageDelegate(_TestDefaultRender))).Run ();
			Assert.IsTrue (str!=null && str!=string.Empty);
		}

		public static void _TestDefaultRender (Page p)
		{
			LiteralControl lcb = new LiteralControl ("aaa");
			LiteralControl lce = new LiteralControl ("bbb");
			p.Controls.Add (lcb);
#if NET_2_0
			Menu menu = new Menu ();
			p.Controls.Add (menu);
#endif
			p.Controls.Add (lce);
		}
#if NET_2_0
		[Test]
		public void TestSkin ()
		{
			Assembly SampleAssembly;
			// Instantiate a target object.
			Int32 Integer1 = new Int32 ();
			Type Type1;
			// Set the Type instance to the target class type.
			Type1 = Integer1.GetType ();
			// Instantiate an Assembly class to the assembly housing the Integer type.  
			SampleAssembly = Assembly.GetAssembly (Integer1.GetType ());
			// Display the physical location of the assembly containing the manifest.
			Console.WriteLine ("Location=" + SampleAssembly.Location);



			WebTest.CopyResource (GetType (), "Test1.Resources.Default.skin", "App_Themes/Black/Default.skin");
			WebTest.CopyResource (GetType (), "Test1.Resources.MyPageWithTheme.aspx", "MyPageWithTheme.aspx");
			string res = new WebTest ("MyPageWithTheme.aspx").Run ();
			Debug.WriteLine (res);			
		}
#endif
		[Test]
		public void UnloadTest ()
		{
			new WebTest (new PageInvoker (new PageDelegates ())).Run ();
			WebTest.Unload ();
			new WebTest (new PageInvoker (new PageDelegates ())).Run ();
		}

		[Test]
		public void PostBack ()
		{
			WebTest.CopyResource (GetType (), "Test1.Resources.Postback.aspx", "Postback.aspx");
			WebTest t = new WebTest ("Postback.aspx");
			string res1 = t.Run ();
			FormRequest fr = new FormRequest (t.Response, "form1");
			fr.Controls.Add ("txt1");
			fr.Controls ["txt1"].Value = "value";
			t.Request = fr;
			string res2 = t.Run ();
			WebTest t1 = new WebTest (fr);
		}

		[Test]
		public void PostRequest ()
		{
			WebTest t = new WebTest (PageInvoker.CreateOnLoad (
				new PageDelegate (CheckPostRequest)));
			PostableRequest pr = new PostableRequest ();
			pr.IsPost = true;
			t.Request = pr;
			t.Run ();
		}

		static public void CheckPostRequest (Page p)
		{
			Assert.AreEqual ("POST", HttpContext.Current.Request.RequestType);
		}
	}
}
