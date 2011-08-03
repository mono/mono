//
// System.Web.SiteMapProviderTest.cs - Unit tests for System.Web.SiteMapProvider
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
	
	class SiteMapProviderPoker : SiteMapProvider
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

		public override SiteMapNode FindSiteMapNode (string rawUrl)
		{
			return new SiteMapNode (this, rawUrl, rawUrl);
		}

		public override SiteMapNodeCollection GetChildNodes (SiteMapNode node)
		{
			throw new NotImplementedException ();
		}

		public override SiteMapNode GetParentNode (SiteMapNode node)
		{
			throw new NotImplementedException ();
		}

		protected internal override SiteMapNode GetRootNodeCore ()
		{
			return new SiteMapNode (this, "rootKey", "rootUrl");
		}
	}

	[TestFixture]
	public class SiteMapProviderTest {

		[Test]
		[ExpectedException (typeof (NotImplementedException))]
		public void AddNode_null ()
		{
			SiteMapProviderPoker poker = new SiteMapProviderPoker ();
			poker.DoAddNode (null);
		}

		[Test]
		[ExpectedException (typeof (NotImplementedException))]
		public void AddNode ()
		{
			SiteMapProviderPoker poker = new SiteMapProviderPoker ();
			poker.DoAddNode (new SiteMapNode (poker, "key", "url"));
		}

		[Test]
		[ExpectedException (typeof (NotImplementedException))]
		public void AddNode2_nullNode ()
		{
			SiteMapProviderPoker poker = new SiteMapProviderPoker ();
			poker.DoAddNode (null, new SiteMapNode (poker, "parentKey", "parentUrl"));
		}

		[Test]
		[ExpectedException (typeof (NotImplementedException))]
		public void AddNode2_nullParent ()
		{
			SiteMapProviderPoker poker = new SiteMapProviderPoker ();
			poker.DoAddNode (new SiteMapNode (poker, "childKey", "childUrl"), null);
		}

		[Test]
		[ExpectedException (typeof (NotImplementedException))]
		public void AddNode2 ()
		{
			SiteMapProviderPoker poker = new SiteMapProviderPoker ();
			poker.DoAddNode (new SiteMapNode (poker, "childKey", "childUrl"),
					 new SiteMapNode (poker, "parentKey", "parentUrl"));
		}

		[Test]
		public void FindSiteMapNode_nullContext ()
		{
			SiteMapProviderPoker poker = new SiteMapProviderPoker ();
			SiteMapNode n = poker.FindSiteMapNode ((HttpContext)null);
			Assert.IsNull (n, "A1");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void FindSiteMapNodeFromKey_nullKey ()
		{
			SiteMapProviderPoker poker = new SiteMapProviderPoker ();
			SiteMapNode n = poker.FindSiteMapNodeFromKey (null);
		}

		[Test]
		public void FindSiteMapNodeFromKey ()
		{
			SiteMapProviderPoker poker = new SiteMapProviderPoker ();
			SiteMapNode n = poker.FindSiteMapNodeFromKey ("key");
			Assert.AreEqual ("key", n.Key, "A1");
			Assert.AreEqual ("key", n.Url, "A2");
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void GetCurrentNodeAndHintAncestorNodes_negativeUplevel ()
		{
			SiteMapProviderPoker poker = new SiteMapProviderPoker ();
			SiteMapNode n = poker.GetCurrentNodeAndHintAncestorNodes (-2);
			Assert.IsNull (n, "A1");
		}

		[Test]
		public void GetCurrentNodeAndHintAncestorNodes_negativeUplevel2 ()
		{
			SiteMapProviderPoker poker = new SiteMapProviderPoker ();
			SiteMapNode n = poker.GetCurrentNodeAndHintAncestorNodes (-1);
			Assert.IsNull (n, "A1");
		}

		[Test]
		public void GetCurrentNodeAndHintAncestorNodes ()
		{
			SiteMapProviderPoker poker = new SiteMapProviderPoker ();
			SiteMapNode n = poker.GetCurrentNodeAndHintAncestorNodes (5);
			Assert.IsNull (n, "A1");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void HintAncestorNodes_nullNode ()
		{
			SiteMapProviderPoker poker = new SiteMapProviderPoker ();
			poker.HintAncestorNodes (null, 0);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void HintAncestorNodes_negativeUplevel ()
		{
			SiteMapProviderPoker poker = new SiteMapProviderPoker ();
			poker.HintAncestorNodes (new SiteMapNode (poker, "key"), -2);
		}

		[Test]
		public void HintAncestorNodes_negativeUplevel2 ()
		{
			SiteMapProviderPoker poker = new SiteMapProviderPoker ();
			poker.HintAncestorNodes (new SiteMapNode (poker, "key"), -1);
		}

		[Test]
		[ExpectedException (typeof (NotImplementedException))]
		public void RemoveNode_nullNode ()
		{
			SiteMapProviderPoker poker = new SiteMapProviderPoker ();
			poker.DoRemoveNode (null);
		}

		[Test]
		[ExpectedException (typeof (NotImplementedException))]
		public void RemoveNode_nonexistantNode ()
		{
			SiteMapProviderPoker poker = new SiteMapProviderPoker ();
			poker.DoRemoveNode (new SiteMapNode (poker, "foo"));
		}

		[Test]
		public void DefaultProperties ()
		{
			SiteMapProviderPoker poker = new SiteMapProviderPoker ();

			Assert.IsFalse (poker.EnableLocalization, "A1");
			Assert.IsFalse (poker.SecurityTrimmingEnabled, "A2");
			Assert.IsNull (poker.ResourceKey, "A3");
		}

		[Test]
		public void Initialize ()
		{
			SiteMapProviderPoker poker = new SiteMapProviderPoker ();
			NameValueCollection attrs = new NameValueCollection ();

			attrs.Add ("securityTrimmingEnabled", "true");
			attrs.Add ("enableLocalization", "true");
			attrs.Add ("resourceKey", "hithere");

			poker.Initialize ("namehere", attrs);

			Assert.IsFalse (poker.EnableLocalization, "A1");
			Assert.IsTrue (poker.SecurityTrimmingEnabled, "A2");
			Assert.IsNull (poker.ResourceKey, "A3");
		}

	}
	
}

#endif
