//
// System.Data.DataColumnChangeEventArgs.cs
//
// Author:
//   Christopher Podurgiel (cpodurgiel@msn.com)
//
// (C) Chris Podurgiel
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;

namespace System.Data
{
	/// <summary>
	/// Provides data for the ColumnChanging event.
	/// </summary>
	public class DataColumnChangeEventArgs : EventArgs
	{
		#region Fields

		private DataColumn _column = null;
		private DataRow _row = null;
		private object _proposedValue = null;

		#endregion // Fields

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the DataColumnChangeEventArgs class.
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <param name="value"></param>
		public DataColumnChangeEventArgs(DataRow row, DataColumn column, object value)
		{
			Initialize(row, column, value);
		}

		internal DataColumnChangeEventArgs()
		{
		}

		#endregion // Constructors

		#region Properties

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

		#endregion // Properties

		#region Methods

		internal void Initialize(DataRow row, DataColumn column, object value)
		{
			_column = column;
			_row = row;
			_proposedValue = value;
		}
	
		#endregion // Methods
	}
}
