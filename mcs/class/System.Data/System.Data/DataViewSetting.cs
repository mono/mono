//
// System.Data.DataViewSetting
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//
// (C) Ximian, Inc. 2002
//

namespace System.Data
{
	/// <summary>
	/// Represents the default settings for ApplyDefaultSort, DataViewManager, RowFilter, RowStateFilter, Sort, and Table for DataViews created from the DataViewManager.
	/// </summary>
	public class DataViewSetting
	{
		private bool defaultSort;
		private DataViewManager viewManager;
		private string rowFilter;
		private DataViewRowState rowStateFilter;
		private string sortString;
		
		public bool ApplyDefaultSort {
			get {
				return defaultSort;
			}
			set {
				defaultSort = value;
			}
		}

		public DataViewManager DataViewManager {
			get {
				return viewManager;
			}
		}

		public string RowFilter {
			get {
				return rowFilter;
			}
			set {
				rowFilter = value;
			}
		}

		public DataViewRowState RowStateFilter {
			get {
				return rowStateFilter;
			}
			set {
				rowStateFilter = value;
			}
		}

		public string Sort {
			get {
				return sortString;
			}
			set {
				sortString = value;
			}
		}

		[MonoTODO]
		public DataTable Table {
			get {
				throw new NotImplementedException ();
			}
		}
	}
}
