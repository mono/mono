//
// System.Data.MergeFailedEventArgs.cs
//
// Author:
//   Miguel de Icaza <miguel@ximian.com>
//   Tim Coleman (tim@timcoleman.com)
//
// (C) Ximian, Inc 2002
// Copyright (C) Tim Coleman, 2002
//

using System;

namespace System.Data {
	public class MergeFailedEventArgs : EventArgs 
	{
		#region Fields

		DataTable data_table;
		string conflict;

		#endregion // Fields

		#region Constructors

		public MergeFailedEventArgs (DataTable dataTable, string conflict)
		{
			this.data_table = dataTable;
			this.conflict = conflict;
		}

		#endregion // Constructors

		#region Properties

		public DataTable Table {
			get { return data_table; }
		}

		public string Conflict {
			get { return conflict; }
		}

		#endregion // Properties
	}
}
