//
// System.Web.UI.WebControls.SiteMapDataSourceView
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//
// (C) 2003 Ben Maurer
//

#if NET_1_2
using System.Collections;
using System.Collections.Specialized;
using System.Text;

namespace System.Web.UI.WebControls {
	public class SiteMapDataSourceView : HierarchicalDataSourceView {
		public SiteMapDataSourceView (SiteMapNode node) : this (new SiteMapNodeCollection (node)) {}
		public SiteMapDataSourceView (SiteMapNodeCollection collection)
		{
			this.collection = collection;
		}
		public override IHierarchicalEnumerable GetHierarchicalList ()
		{
			return collection;
		}
		
		SiteMapNodeCollection collection;
	}
}
#endif

