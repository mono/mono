//
// System.Data.MergeFailedEventArgs.cs
//
// Author:
//   Miguel de Icaza <miguel@ximian.com>
//
// (C) Ximian, Inc 2002
//

using System;

namespace System.Data
{
	public class MergeFailedEventArgs : EventArgs {
		DataTable data_table;
		string conflict;
		Exception errors;
		bool f_continue;

		public FillErrorEventArgs (DataTable dataTable, string conflict)
		{
			this.data_table = dataTable;
			this.conflict = conflict;
		}

		public DataTable DataTable {
			get {
				return data_table;
			}
		}

		public string Conflict {
			get {
				return conflict;
			}
		}
	}
}
