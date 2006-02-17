//
// System.Data.DataViewSetting
//
// Authors:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Tim Coleman (tim@timcoleman.com)
//   Atsushi Enomoto (atsushi@ximian.com)
//
// (C) Ximian, Inc. 2002
// Copyright (C) Tim Coleman, 2002
// Copyright (C) 2005 Novell Inc,
//

//
// Copyright (C) 2004-05 Novell, Inc (http://www.novell.com)
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

using System.ComponentModel;

namespace System.Data
{
	/// <summary>
	/// Represents the default settings for ApplyDefaultSort, DataViewManager, RowFilter, RowStateFilter, Sort, and Table for DataViews created from the DataViewManager.
	/// </summary>
	[TypeConverterAttribute (typeof (ExpandableObjectConverter))]
#if !NET_2_0
	[Serializable]
#endif
	public class DataViewSetting
	{
		#region Fields

		bool applyDefaultSort;
		DataViewManager dataViewManager;
		string rowFilter = String.Empty;
		DataViewRowState rowStateFilter = DataViewRowState.CurrentRows;
		string sort = String.Empty;
		DataTable dataTable;

		#endregion // Fields

		#region Constructors

		internal DataViewSetting (DataViewManager manager, DataTable table)
		{
			dataViewManager = manager;
			dataTable = table;
		}

		#endregion // Constructors

		#region Properties
		
		public bool ApplyDefaultSort {
			get { return applyDefaultSort; }
			set { applyDefaultSort = value; }
		}

		[Browsable (false)]
		public DataViewManager DataViewManager {
			get { return dataViewManager; }
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
			get { return sort; }
			set { sort = value; }
		}

		[Browsable (false)]
		public DataTable Table {
			get { return dataTable; }
		}

		#endregion // Properties
	}
}
