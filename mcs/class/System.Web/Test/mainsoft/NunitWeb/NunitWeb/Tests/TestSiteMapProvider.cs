using System;
using System.Collections.Specialized;
using System.Web;

#if NET_2_0
namespace Tests {
	public class TestSiteMapProvider : StaticSiteMapProvider {
		object rootNodeLock = new object();
		SiteMapNode rootNode = null;

		public override SiteMapNode RootNode {
			get { return BuildSiteMap(); }
		}

		protected internal override SiteMapNode GetRootNodeCore() {
			return BuildSiteMap();
		}

		protected override void Clear() {
			lock (rootNodeLock) {
				rootNode = null;
				base.Clear();
			}
		}
		
		public override SiteMapNode BuildSiteMap () {
			lock (rootNodeLock) {
				if(rootNode == null) {
					rootNode = new SiteMapNode(this, "Test", "default.aspx", "Test");
					AddNode(rootNode);
				}
				return rootNode;
			}
		}
	}
}
#endif
