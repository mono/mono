//
// System.Windows.Forms.DataGridParentRowsLabelStyle
//
// Author:
//   Jaak Simm (jaaksimm@firm.ee)
//   Dennis Hayes (dennish@raytek.com)
// (C) Ximian, Inc.  http://www.ximian.com
//

using System;

namespace System.Windows.Forms
{

	/// <summary>
  /// Specifies how the parent row labels of a DataGrid control are displayed.
	/// </summary>
	[Serializable]
	public enum DataGridParentRowsLabelStyle
	{
		Both = 1,
		ColumnName = 2,
		None = 0,
		TableName = 3
	}
}