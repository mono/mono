//
// Tests for System.Web.UI.WebControls.View.cs
//
// Author:
//	Yoni Klein (yonik@mainsoft.com)
//
// (C) 2005 Mainsoft Corporation (http://www.mainsoft.com)
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

using NUnit.Framework;
using System;
using System.IO;
using System.Globalization;
using System.Configuration;
using System.Collections;
using System.Collections.Specialized;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Security.Permissions;


namespace MonoTests.System.Web.UI.WebControls
{
	class PokerSiteMapDataSource : SiteMapDataSource
	{
		public PokerSiteMapDataSource ()
		{
			TrackViewState ();
		}
		public object SaveState ()
		{
			return SaveViewState ();
		}
		public void LoadState (object o)
		{
			LoadViewState (o);
		}
		public StateBag StateBag
		{
			get { return base.ViewState; }
		}
		public HierarchicalDataSourceView DoHierarchicalDataSourceView (string str)
		{
			return GetHierarchicalView (str);
		}
	}

	[TestFixture]
	public class SiteMapDataSourceTest
	{

		[Test]
		public void SiteMapDataSource_DefaultProperties ()
		{

			PokerSiteMapDataSource p = new PokerSiteMapDataSource ();
			Assert.AreEqual (true, p.ShowStartingNode, "ShowStartingNode");
			Assert.AreEqual (string.Empty, p.SiteMapProvider, "SiteMapProvider");
			Assert.AreEqual (false, p.StartFromCurrentNode, "StartFromCurrentNode");
			Assert.AreEqual (0, p.StartingNodeOffset, "StartingNodeOffset");
			Assert.AreEqual (string.Empty, p.StartingNodeUrl, "StartingNodeUrl");
			
		}

		[Test]
		public void SiteMapDataSource_ContainsListCollection ()
		{	
			PokerSiteMapDataSource p = new PokerSiteMapDataSource ();
			Assert.AreEqual (true, p.ContainsListCollection, "ContainsListCollection");
		}
		
		[Test]
		public void SiteMapDataSource_ChangeProperties ()
		{
			PokerSiteMapDataSource p = new PokerSiteMapDataSource ();
			p.ShowStartingNode = false;
			Assert.AreEqual (false, p.ShowStartingNode, "ShowStartingNode");
			Assert.AreEqual (1, p.StateBag.Count, "ShowStartingNode#1");

			p.SiteMapProvider = "test";
			Assert.AreEqual ("test", p.SiteMapProvider, "SiteMapProvider");
			Assert.AreEqual (2, p.StateBag.Count, "SiteMapProvider#1");
			// null properties doe's not affects on state bag count
			p.SiteMapProvider = null;
			Assert.AreEqual (2, p.StateBag.Count, "SiteMapProvider#2");

			p.StartFromCurrentNode = true;
			Assert.AreEqual (true, p.StartFromCurrentNode, "StartFromCurrentNode");
			Assert.AreEqual (3, p.StateBag.Count, "StartFromCurrentNode#1");

			p.StartingNodeOffset = 1;
			Assert.AreEqual (1, p.StartingNodeOffset, "StartingNodeOffset");
			Assert.AreEqual (4, p.StateBag.Count, "StartingNodeOffset#2");

			p.StartingNodeUrl = "default.aspx";
			Assert.AreEqual ("default.aspx", p.StartingNodeUrl, "StartingNodeUrl");
			Assert.AreEqual (5, p.StateBag.Count, "StartingNodeUrl#1");
			// null properties doe's not affects on state bag count
			p.StartingNodeUrl = null;
			Assert.AreEqual (5, p.StateBag.Count, "StartingNodeUrl#2");
		}

		
		[Test]
		public void SiteMapDataSource_DataSourceChanged ()
		{
			PokerSiteMapDataSource p = new PokerSiteMapDataSource ();
			((IDataSource) p).DataSourceChanged += new EventHandler (SiteMapDataSourceTest_DataSourceChanged);
			
			eventChecker = false;
			p.ShowStartingNode = false;
			Assert.IsTrue (eventChecker, "DataSourceChanged#1");

			eventChecker = false;
			p.SiteMapProvider = "test";
			Assert.IsTrue (eventChecker, "DataSourceChanged#2");

			eventChecker = false;
			p.StartFromCurrentNode = true;
			Assert.IsTrue (eventChecker, "DataSourceChanged#3");

			eventChecker = false;
			p.StartingNodeOffset = 1;
			Assert.IsTrue (eventChecker, "DataSourceChanged#4");

			eventChecker = false;
			p.StartingNodeUrl = "default.aspx";
			Assert.IsTrue (eventChecker, "DataSourceChanged#5");
		}

		bool eventChecker;
		void SiteMapDataSourceTest_DataSourceChanged (object sender, EventArgs e)
		{
			eventChecker = true;
		}


		[Test]
		public void SiteMapDataSource_GetList ()
		{
			PokerSiteMapDataSource p = new PokerSiteMapDataSource ();
			Assert.IsNotNull (p.GetList (), "GetList");
			Assert.IsTrue (p.ContainsListCollection, "ContainsListCollection");
		}

		[Test]
		public void SiteMapDataSource_GetViewNames () {
			PokerSiteMapDataSource p = new PokerSiteMapDataSource ();
			Assert.AreEqual (1, p.GetViewNames ().Count, "GetViewNames().Count");
			Assert.AreEqual ("DefaultView", ((string []) p.GetViewNames ()) [0], "GetViewNames () [0]");
		}
		
		[Test]
		public void SiteMapDataSource_GetView ()
		{
			PokerSiteMapDataSource p = new PokerSiteMapDataSource ();
			p.Provider = new mySiteMapProvider ();
			DataSourceView V  = p.GetView("");
			Assert.IsNotNull (V, "GetView");
		}

		[Test]
		public void SiteMapDataSource_ViewState ()
		{
			PokerSiteMapDataSource p = new PokerSiteMapDataSource ();
			p.SiteMapProvider = "test";

			p.StartFromCurrentNode = false;
			p.StartingNodeOffset = 1;
			p.StartingNodeUrl = "default.aspx";

			object state = p.SaveState ();
			PokerSiteMapDataSource copy = new PokerSiteMapDataSource ();
			copy.LoadState (state);

			Assert.AreEqual ("test", copy.SiteMapProvider, "SiteMapProvider");
			Assert.AreEqual (false, copy.StartFromCurrentNode, "StartFromCurrentNode");
			Assert.AreEqual (1, copy.StartingNodeOffset, "StartingNodeOffset");
			Assert.AreEqual ("default.aspx", copy.StartingNodeUrl, "StartingNodeUrl");
		}

		[Test]
		public void SiteMapDataSource_HierarchicalDataSourceView ()
		{
			PokerSiteMapDataSource p = new PokerSiteMapDataSource ();
			p.Provider = new mySiteMapProvider ();
			HierarchicalDataSourceView h = p.DoHierarchicalDataSourceView ("1");
			Assert.IsNotNull (h, "HierarchicalDataSourceView");
		}

	}

   
	// Helper Class
	public class mySiteMapProvider : StaticSiteMapProvider
	{
		private SiteMapNode rootNode = null;
		// Implement a default constructor.
		public mySiteMapProvider ()
		{
		}
		// Some basic state to help track the initialization state of the provider.
		private bool initialized = false;
		public virtual bool IsInitialized
		{
			get { return initialized; }
		}

		// Return the root node of the current site map.
		public override SiteMapNode RootNode
		{
			get
			{
				SiteMapNode temp = null;
				temp = BuildSiteMap ();
				return temp;
			}
		}
		protected internal override SiteMapNode GetRootNodeCore ()
		{
			return RootNode;
		}
		// Initialize is used to initialize the properties and any state that the
		// AccessProvider holds, but is not used to build the site map.
		// The site map is built when the BuildSiteMap method is called.
		public override void Initialize (string name, NameValueCollection attributes)
		{
			if (IsInitialized)
				return;

			base.Initialize (name, attributes);
			initialized = true;
		}

		///
		/// SiteMapProvider and StaticSiteMapProvider methods that this derived class must override.
		///
		// Clean up any collections or other state that an instance of this may hold.
		protected override void Clear ()
		{
			lock (this) {
				rootNode = null;
				base.Clear ();
			}
		}

		// Build an in-memory representation from persistent
		// storage, and return the root node of the site map.
		public override SiteMapNode BuildSiteMap ()
		{
			// Since the SiteMap class is static, make sure that it is
			// not modified while the site map is built.
			lock (this) {

				// If there is no root node, then there is no site map.
				if (null == rootNode) {
					// Start with a clean slate
					Clear ();

					// Select the root node of the site map .
					rootNode = new SiteMapNode (this, "1", "default.aspx", "Default");

				}

				else return null;

				SiteMapNode childNode = null;
				childNode = new SiteMapNode (this, "2", "catalog.aspx", "catalog");
				AddNode (childNode, rootNode);
				childNode = new SiteMapNode (this, "3", "aboutus.aspx", "about us");
				AddNode (childNode, rootNode);

				return rootNode;
			}
		}
	}
}
#endif
