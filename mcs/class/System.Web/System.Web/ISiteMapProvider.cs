//
// System.Web.ISiteMapProvider
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//
// (C) 2003 Ben Maurer
//

#if NET_1_2
namespace System.Web {
	public interface ISiteMapProvider {
		SiteMapNode FindSiteMapNode (string rawUrl);
		SiteMapNodeCollection GetChildNodes (SiteMapNode node);
		SiteMapNode GetParentNode (SiteMapNode node);
		SiteMapNode CurrentNode { get; }
		ISiteMapProvider ParentProvider { get; set; }
		SiteMapNode RootNode { get; }
		ISiteMapProvider RootProvider { get; }
	}
}
#endif

