//
// System.Web.UI.Design.IDataSourceProvider
//
// Authors:
//      Gert Driesen (drieseng@users.sourceforge.net)
//
// (C) 2004 Novell
//

using System.Collections;

namespace System.Web.UI.Design
{
	public interface IDataSourceProvider
	{
		IEnumerable GetResolvedSelectedDataSource ();
		object GetSelectedDataSource ();
	}
} 