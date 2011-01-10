//
// System.Web.StaticSiteMapProviderTest.cs - Unit tests for System.Web.StaticSiteMapProvider
//
// Author:
//	Chris Toshok <toshok@ximian.com>
//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
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

using System;
using System.Text;
using System.Web;
using System.Collections.Specialized;
using NUnit.Framework;
using System.Diagnostics;
using MonoTests.SystemWeb.Framework;
using System.Web.UI;

using Tests;

namespace MonoTests.System.Web {
	
	class StaticPoker : StaticSiteMapProvider
	{
		public void DoAddNode (SiteMapNode node)
		{
			base.AddNode (node);
		}

		public void DoAddNode (SiteMapNode node, SiteMapNode parentNode)
		{
			base.AddNode (node, parentNode);
		}

		public void DoRemoveNode (SiteMapNode node)
		{
			base.RemoveNode (node);
		}


		//

		SiteMapNode root;

		public override SiteMapNode BuildSiteMap ()
		{
			if (root != null)
				return root;
				
			root = new SiteMapNode (this, "rootKey", "rootUrl");

			return root;
		}

		protected internal override SiteMapNode GetRootNodeCore ()
		{
			return BuildSiteMap ();
		}
	}

	[TestFixture]
	public class StaticSiteMapProviderTest
	{
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void AddNode_null ()
		{
			StaticPoker poker = new StaticPoker ();
			poker.DoAddNode (null);
		}

		[Test]
		[Category("NunitWeb")]
		public void AddNode ()
		{
			new WebTest (new HandlerInvoker (AddNode_delegate)).Run ();
		}

		static public void AddNode_delegate ()
		{
			StaticPoker poker = new StaticPoker();

			SiteMapNode n = new SiteMapNode (poker, "key", "url");

			poker.DoAddNode (n,poker.RootNode);
			Assert.AreEqual (1, poker.GetChildNodes (poker.RootNode).Count, "A1");

			poker.DoRemoveNode(n);
			Assert.AreEqual (0, poker.GetChildNodes (poker.RootNode).Count, "A2");
		}
		

		[Test]
		[Category ("NunitWeb")]
		public void AddNode_duplicate_url () {
			new WebTest (new HandlerInvoker (AddNode_delegate2)).Run ();
		}

		static public void AddNode_delegate2 () {
			StaticPoker poker = new StaticPoker ();

			SiteMapNode n1 = new SiteMapNode (poker, "key1", "");
			SiteMapNode n2 = new SiteMapNode (poker, "key2", "");

			poker.DoAddNode (n1, poker.RootNode);
			poker.DoAddNode (n2, poker.RootNode);
			Assert.AreEqual (2, poker.GetChildNodes (poker.RootNode).Count, "A1");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void AddNode2_nullNode ()
		{
			StaticPoker poker = new StaticPoker ();
			poker.DoAddNode (null, new SiteMapNode (poker, "parentKey", "parentUrl"));
		}

		[Test]
		[Category("NunitWeb")]
		public void AddNode2_nullParent ()
		{
			new WebTest (new HandlerInvoker (AddNode2_nullParent_delegate)).Run ();
		}
		
		static public void AddNode2_nullParent_delegate ()
		{
			StaticPoker poker = new StaticPoker ();
			poker.DoAddNode (new SiteMapNode (poker, "childKey", "childUrl"), null);
		}

		[Test]
		[Category("NunitWeb")]
		public void AddNode2 ()
		{
			new WebTest (new HandlerInvoker (AddNode2_delegate)).Run ();
		}

		static public void AddNode2_delegate ()
		{
			StaticPoker poker = new StaticPoker ();
			poker.DoAddNode (new SiteMapNode (poker, "childKey", "childUrl"),
					 new SiteMapNode (poker, "parentKey", "parentUrl"));
		}

		[Test]
		[Category ("NunitWeb")]
		public void IsAccessibleFrom1 ()
		{
			new WebTest (PageInvoker.CreateOnLoad (IsAccessibleFrom1_delegate)).Run ();
		}

		static public void IsAccessibleFrom1_delegate (Page page)
		{
			StaticPoker p = new StaticPoker ();
			SiteMapNode n = new SiteMapNode (p, "childKey", "http://childUrl/");
			n.Roles = null;
			bool b = p.IsAccessibleToUser (HttpContext.Current, n);
			Assert.IsTrue (b, "#1");
		}

		[Test]
		public void FindSiteMapNode_01 ()
		{
			new WebTest (PageInvoker.CreateOnLoad (FindSiteMapNode_01_OnLoad)).Run ();
		}

		public static void FindSiteMapNode_01_OnLoad (Page p)
		{
			var provider = new TestSiteMapProvider ();
			Assert.IsNotNull (provider.RootNode, "#A1");
			Assert.AreEqual ("default.aspx", provider.RootNode.Url, "#A1-1");

			SiteMapNode node = provider.FindSiteMapNode ("default.aspx");
			Assert.IsNull (node, "#A2");

			node = provider.FindSiteMapNode ("/NunitWeb/default.aspx");
			Assert.IsNotNull (node, "#A3");
			Assert.AreEqual ("default.aspx", node.Url, "#A3-1");
			Assert.AreEqual ("Test", node.Title, "#A3-2");

			node = provider.FindSiteMapNode ("~/default.aspx");
			Assert.IsNotNull (node, "#A4");
			Assert.AreEqual ("default.aspx", node.Url, "#A4-1");
			Assert.AreEqual ("Test", node.Title, "#A4-2");
		}
		
		[TestFixtureTearDown]
		public void TearDown ()
		{
			WebTest.Unload ();
		}
	}
}

#endif
