//
// System.Data.DataViewSetting
//
// Authors:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Tim Coleman (tim@timcoleman.com)
//
// (C) Ximian, Inc. 2002
// Copyright (C) Tim Coleman, 2002
//

using System.ComponentModel;

namespace System.Data
{
	/// <summary>
	/// Represents the default settings for ApplyDefaultSort, DataViewManager, RowFilter, RowStateFilter, Sort, and Table for DataViews created from the DataViewManager.
	/// </summary>
	[Serializable]
	public class DataViewSetting
	{
		#region Fields

		bool defaultSort;
		DataViewManager viewManager;
		string rowFilter;
		DataViewRowState rowStateFilter;
		string sortString;

		#endregion // Fields

		#region Constructors

		internal DataViewSetting ()
		{
		}

		#endregion // Constructors

		#region Properties
		
		public bool ApplyDefaultSort {
			get { return defaultSort; }
			set { defaultSort = value; }
		}

		[Browsable (false)]
		public DataViewManager DataViewManager {
			get { return viewManager; }
		}

		public string RowFilter {
			get { return rowFilter; }
			set { rowFilter = value; }
		}

		public DataViewRowState RowStateFilter {
			get { return rowStateFilter; }
			set { rowStateFilter = value; }
		}

		public string Sort {
			get { return sortString; }
			set { sortString = value; }
		}

		[MonoTODO]
		[Browsable (false)]
		public DataTable Table {
			get { throw new NotImplementedException (); }
		}

		#endregion // Properties
	}
}
