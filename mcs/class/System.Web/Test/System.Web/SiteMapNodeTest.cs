//
// System.Web.SiteMapNodeTest.cs - Unit tests for System.Web.SiteMapNode
//
// Author:
//	Andrew Skiba <andrews@mainsoft.com>
//
// (C) 2005 Mainsoft Corporation (http://www.mainsoft.com)
//
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
using System.Collections.Generic;
using System.Web;
using System.Collections.Specialized;
using NUnit.Framework;

namespace MonoTests.System.Web
{

	class DummyProvider : SiteMapProvider
	{
		public override SiteMapNode FindSiteMapNode (string rawUrl)
		{
			throw new Exception ("The method or operation is not implemented.");
		}

		public override SiteMapNodeCollection GetChildNodes (SiteMapNode node)
		{
			throw new Exception ("The method or operation is not implemented.");
		}

		public override SiteMapNode GetParentNode (SiteMapNode node)
		{
			throw new Exception ("The method or operation is not implemented.");
		}

		protected internal override SiteMapNode GetRootNodeCore ()
		{
			throw new Exception ("The method or operation is not implemented.");
		}
	}

	[TestFixture]
	public class SiteMapNodeTest
	{
		[Test]
        [Category ("NotWorking")]
		public void Node_Null_Attrib_equals ()
		{
            // Note: dot.net implementation dosn't compare attributes
			SiteMapNode node = new SiteMapNode (new DummyProvider (), "", "", "", null, null, null, null, null);
			SiteMapNode node1 = new SiteMapNode (new DummyProvider (), "", "", "", null, null, null, null, null);
			SiteMapNode node2 = new SiteMapNode (new DummyProvider (), "", "", "", null, null, new NameValueCollection (), null, null);
			Assert.IsTrue (node.Equals (node1), "both nodes have attrib=null");
			Assert.IsTrue (node.Equals (node2), "one node has attrib=null");
		}

		[Test]
		public void Node_equals ()
		{
			SiteMapNode node = new SiteMapNode (new DummyProvider (), "Node", "1", "", null, null, null, null, null);
			SiteMapNode node1 = new SiteMapNode (new DummyProvider (), "Node", "1", "", null, null, null, null, null);
			SiteMapNode node2 = new SiteMapNode (new DummyProvider (), "Node", "2", "", null, null, new NameValueCollection (), null, null);
			Assert.IsTrue (node.Equals (node1), "both nodes have attrib=null");
			Assert.IsFalse (node.Equals (node2), "one node has attrib=null");
		}

		[Test]
		[Category ("NotWorking")]
		public void Node_Null_Roles_equals ()
		{
			SiteMapNode node = new SiteMapNode (new DummyProvider (), "", "", "", null, null, null, null, null);
			SiteMapNode node1 = new SiteMapNode (new DummyProvider (), "", "", "", null, null, null, null, null);
			SiteMapNode node2 = new SiteMapNode (new DummyProvider (), "", "", "", null, new int[] { }, null, null, null);
			Assert.IsTrue (node.Equals (node1));
			Assert.IsTrue (node.Equals (node2));
		}

		[Test]
		public void Node_Null_Attrib_clone ()
		{
			SiteMapNode node = new SiteMapNode (new DummyProvider (), "", "", "", null, null, null, null, null);
			SiteMapNode copy = node.Clone ();
			Assert.IsNotNull (copy, "Node not created");
			Assert.AreEqual (copy, node, "Cloning failed");
		}
	}
}
#endif
