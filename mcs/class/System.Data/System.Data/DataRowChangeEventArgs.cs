//
// System.Data.DataRowChangeEventArgs.cs
//
// Author:
//   Daniel Morgan <danmorg@sc.rr.com>
//   Ville Palo <vi64pa@koti.soon.fi>
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

	        private DataRow row;
		private DataRowAction action; 
	
		public DataRowChangeEventArgs(DataRow row,
			DataRowAction action) {
			
			this.row = row;
			this.action = action;
		}

		public DataRowAction Action {
			get {
				return action;
			}
		}

		public DataRow Row {
			get {
				return row;
			}
		}
	}
}
