//
// System.Web.UI.Design.DesignTimeData
//
// Authors:
//      Gert Driesen (drieseng@users.sourceforge.net)
//
// (C) 2004 Novell
//

using System.Collections;
using System.ComponentModel;
using System.Data;

namespace System.Web.UI.Design
{
	public sealed class DesignTimeData
	{
		private DesignTimeData ()
		{
		}

		[MonoTODO]
		public static DataTable CreateDummyDataTable ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static DataTable CreateSampleDataTable (IEnumerable referenceData)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static PropertyDescriptorCollection GetDataFields (IEnumerable dataSource)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static IEnumerable GetDataMember (IListSource dataSource, string dataMember)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static string[] GetDataMembers (object dataSource)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static IEnumerable GetDesignTimeDataSource (DataTable dataTable, int minimumRows)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static object GetSelectedDataSource (IComponent component, string dataSource)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static IEnumerable GetSelectedDataSource (IComponent component, string dataSource, string dataMember)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		private static void OnDataBind (object sender, EventArgs e)
		{
			throw new NotImplementedException ();
		}


		public static readonly EventHandler DataBindingHandler =
			new EventHandler (DesignTimeData.OnDataBind);
	}
}
