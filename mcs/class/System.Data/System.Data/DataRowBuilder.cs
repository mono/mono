//
// System.Data.DataRowBuilder
//
// Author:
//   Tim Coleman <tim@timcoleman.com>
//
// Copyright (C) 2002 Tim Coleman
//

using System;

namespace System.Data
{
	/// <summary>
	/// A supporting class that exists solely to support
	/// DataRow and DataTable
	/// Implementation of something meaningful will follow.
	/// Presumably, what that is will become apparent when
	/// constructing DataTable and DataRow.
	/// </summary>

	public class DataRowBuilder 
	{
		#region Fields
		
		private DataTable table;
		internal int _rowId;

		#endregion

		#region Constructors

		// DataRowBuilder on .NET takes 3 arguments, a
		// DataTable and two Int32.  For consistency, this
		// class will also take those arguments.

		internal DataRowBuilder (DataTable table, Int32 rowID, Int32 y)
		{
			this.table = table;
			this._rowId = rowID;
		}

		#endregion

		#region Properties

		protected internal DataTable Table {
			get { return table; }
		}

		#endregion

	}
}
