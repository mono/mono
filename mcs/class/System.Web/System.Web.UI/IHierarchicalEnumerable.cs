//
// System.Web.UI.IHierarchicalEnumerable
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//
// (C) 2003 Ben Maurer
//

#if NET_1_2
using System.Collections;

namespace System.Web.UI {
	public interface IHierarchicalEnumerable : IEnumerable {
		IHierarchyData GetHierarchyData (object enumeratedItem);
	}
}
#endif

