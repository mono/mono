//
// System.Windows.Forms.DataGridParentRowsLabelStyle.cs
//
// Author:
//   Jaak Simm (jaaksimm@firm.ee)
//   Dennis Hayes (dennish@raytek.com)
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

using System;

namespace System.Windows.Forms {


	/// <summary>
  /// Specifies how the parent row labels of a DataGrid control are displayed.
	/// </summary>
	public enum DataGridParentRowsLabelStyle {

		//Values were verified with enumcheck.
		None = 0,
		TableName = 1,
		ColumnName = 2,
		Both = 3,
	}
}
