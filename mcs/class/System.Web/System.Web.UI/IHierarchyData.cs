//
// System.Web.UI.IHierarchyData
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//
// (C) 2003 Ben Maurer
//

#if NET_1_2
namespace System.Web.UI {
	public interface IHierarchyData {
		IHierarchicalEnumerable GetChildren ();
		IHierarchicalEnumerable GetParent ();
		bool HasChildren { get; }
		object Item { get; }
		string Path { get; }
		string Type { get; }
	}
}
#endif

