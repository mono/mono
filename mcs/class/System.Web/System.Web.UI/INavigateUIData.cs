//
// System.Web.UI.INavigateUIData
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//
// (C) 2003 Ben Maurer
//

#if NET_1_2
namespace System.Web.UI {
	public interface INavigateUIData {
		string Name { get; }
		string NavigateUrl { get; }
		string Value { get; }	 
	}
}
#endif

