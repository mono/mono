//
// System.Web.UI.HierarchicalDataSourceView
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

namespace System.Web.UI {
	public abstract class HierarchicalDataSourceView : DataSourceView {
		protected HierarchicalDataSourceView ()
		{
		}
		
		public abstract IHierarchicalEnumerable GetHierarchicalList ();
		public override IEnumerable Select ()
		{
			return GetHierarchicalList ();
		}
	 
	}
}
#endif

