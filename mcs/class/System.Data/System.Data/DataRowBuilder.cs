//
// System.Data.DataRowBuilder
//
// Author:
//   Tim Coleman <tim@timcoleman.com>
//
// Copyright (C) 2002 Tim Coleman
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
	/// A supporting class that exists solely to support
	/// DataRow and DataTable
	/// Implementation of something meaningful will follow.
	/// Presumably, what that is will become apparent when
	/// constructing DataTable and DataRow.
	/// </summary>

	public
#if NET_2_0
	sealed
#endif
	class DataRowBuilder 
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

		internal DataTable Table {
			get { return table; }
		}

		#endregion

	}
}
