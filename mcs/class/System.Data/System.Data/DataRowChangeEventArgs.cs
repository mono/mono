//
// System.Data.DataRowChangeEventArgs.cs
//
// Author:
//   Daniel Morgan <danmorg@sc.rr.com>
//
// (C) Ximian, Inc 2002
//

namespace System.Data
{
	/// <summary>
	/// argument data for events RowChanged, RowChanging, 
	/// OnRowDeleting, and OnRowDeleted
	/// </summary>
	public class DataRowChangeEventArgs : EventArgs {

		[MonoTODO]
		public DataRowChangeEventArgs(DataRow row,
			DataRowAction action) {

			throw new NotImplementedException ();
		}

		public DataRowAction Action {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
		}

		public DataRow Row {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
		}

	}

}