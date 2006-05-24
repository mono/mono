using System;
using System.Text;
using NUnit.Framework;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using NunitWeb;
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
			NunitWeb.Helper.Unload ();
		}

		[Test]
		public void RenderSiteMapPath ()
		{
			string res = Helper.Instance.RunInPage (_RenderSiteMapPath);
			Console.WriteLine (res);
			Assert.IsFalse (string.IsNullOrEmpty (res));
		}

		public static void _RenderSiteMapPath (HttpContext c, Page p, object o)
		{
			SiteMapPath smp = new SiteMapPath ();
			p.Controls.Add (smp);
		}

		[Test]
		public void RenderSiteMapPathProp ()
		{
			string res = Helper.Instance.RunInPage (_RenderSiteMapPathProp);
			Console.WriteLine (res);
			Assert.IsFalse (string.IsNullOrEmpty (res));
		}

		public static void _RenderSiteMapPathProp (HttpContext c, Page p, object o)
		{
			SiteMapPath smp = new SiteMapPath ();
			smp.BackColor = Color.Red;
			p.Controls.Add (smp);
		}

		[Test]
		public void TestMasterPage ()
		{
			string res = Helper.Instance.RunInPageWithMaster (_TestMasterPage);
			Console.WriteLine (res);
			Assert.IsFalse (string.IsNullOrEmpty (res));
		}

		public static void _TestMasterPage (HttpContext c, Page p, object o)
		{
			MasterPage mp = p.Master;
			Assert.IsNotNull (mp);
		}

		[Test]
		public void TestStyle ()
		{
			string res = Helper.Instance.RunInPage(_TestStyle);
			Console.WriteLine (res);
			Assert.IsFalse (string.IsNullOrEmpty (res));
		}

		public static void _TestStyle (HttpContext c, Page p, object param)
		{
			Button b = new Button ();
			b.BackColor = Color.Red;
			b.ID = "Yoni";
			p.Form.Controls.Add (b);
		}

		[Test]
		public void TestDefaultRender ()
		{
			string str = Helper.Instance.RunInPage (_TestDefaultRender, null);
			Assert.IsFalse (string.IsNullOrEmpty (str));
		}

		public static void _TestDefaultRender (HttpContext c, Page p, object param)
		{
			LiteralControl lcb = new LiteralControl ("aaa");
			LiteralControl lce = new LiteralControl ("bbb");
			Menu menu = new Menu ();
			p.Form.Controls.Add (lcb);
			p.Form.Controls.Add (menu);
			p.Form.Controls.Add (lce);
		}

		[Test]
		public void TestSkin ()
		{
			NunitWeb.Helper.Instance.CopyResource (Assembly.GetExecutingAssembly (), "Test1.Resources.Default.skin", "App_Themes/Black/Default.skin");
			NunitWeb.Helper.Instance.CopyResource (Assembly.GetExecutingAssembly (), "Test1.Resources.MyPageWithTheme.aspx", "MyPageWithTheme.aspx");
			NunitWeb.Helper.Instance.CopyResource (Assembly.GetExecutingAssembly (), "Test1.Resources.MyPageWithTheme.aspx.cs", "MyPageWithTheme.aspx.cs");
			string res = NunitWeb.Helper.Instance.RunUrl ("MyPageWithTheme.aspx", null, null);
			Debug.WriteLine (res);			
		}

	}
}
