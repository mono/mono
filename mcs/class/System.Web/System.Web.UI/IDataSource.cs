//
// System.Web.UI.IDataSource
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
	public interface IDataSource {
		event EventHandler DataSourceChanged;
		DataSourceView GetView (string viewName);
		ICollection GetViewNames ();
	}
}
#endif

