//
// System.Web.UI.IHierarchicalDataSource
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
	public interface IHierarchicalDataSource {
		event EventHandler DataSourceChanged;
		HierarchicalDataSourceView GetHierarchicalView (string viewPath);
	}
}
#endif

