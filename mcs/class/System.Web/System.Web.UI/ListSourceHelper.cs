//
// System.Web.UI.ListSourceHelper
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
	public sealed class ListSourceHelper {
		private ListSourceHelper () {}
		
		[MonoTODO]
		public static bool ContainsListCollection (IDataSource dataSource)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public static IList GetList (IDataSource dataSource)
		{
			throw new NotImplementedException ();
		}
	}
	

}
#endif

