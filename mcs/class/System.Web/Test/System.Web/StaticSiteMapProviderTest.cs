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

		protected override SiteMapNode GetRootNodeCore ()
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
		public void AddNode ()
		{
			StaticPoker poker = new StaticPoker();

			SiteMapNode n = new SiteMapNode (poker, "key", "url");

			poker.DoAddNode (n);
			Assert.AreEqual (1, poker.GetChildNodes (poker.RootNode).Count, "A1");

			poker.DoRemoveNode (n);
			Assert.AreEqual (0, poker.GetChildNodes (poker.RootNode).Count, "A2");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void AddNode2_nullNode ()
		{
			StaticPoker poker = new StaticPoker ();
			poker.DoAddNode (null, new SiteMapNode (poker, "parentKey", "parentUrl"));
		}

		[Test]
		public void AddNode2_nullParent ()
		{
			StaticPoker poker = new StaticPoker ();
			poker.DoAddNode (new SiteMapNode (poker, "childKey", "childUrl"), null);
		}

		[Test]
		public void AddNode2 ()
		{
			StaticPoker poker = new StaticPoker ();
			poker.DoAddNode (new SiteMapNode (poker, "childKey", "childUrl"),
					 new SiteMapNode (poker, "parentKey", "parentUrl"));
		}
	}
}

#endif
