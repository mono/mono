//
// System.Data.DataColumnChangeEventArgs.cs
//
// Author:
//   Christopher Podurgiel (cpodurgiel@msn.com)
//
// (C) Chris Podurgiel
//

using System;

namespace System.Data
{
	/// <summary>
	/// Provides data for the ColumnChanging event.
	/// </summary>
	public class DataColumnChangeEventArgs : EventArgs
	{
		
		private DataColumn _column = null;
		private DataRow _row = null;
		private object _proposedValue = null;

		/// <summary>
		/// Initializes a new instance of the DataColumnChangeEventArgs class.
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <param name="value"></param>
		public DataColumnChangeEventArgs(DataRow row, DataColumn column, object value)
		{
			_column = column;
			_row = row;
			_proposedValue = value;
		}

		/// <summary>
		/// Gets the DataColumn with a changing value.
		/// </summary>
		public DataColumn Column 
		{
			get
			{
				return _column;
			}
		}


		/// <summary>
		/// Gets or sets the proposed new value for the column.
		/// </summary>
		public object ProposedValue 
		{
			get
			{
				return _proposedValue;
			}
			set
			{
				_proposedValue = value;
			}
		}


		/// <summary>
		/// Gets the DataRow of the column with a changing value.
		/// </summary>
		public DataRow Row 
		{
			get
			{
				return _row;
			}
		}




	}
}
