//
// System.Windows.Forms.IDataGridColumnStyleEditingNotificationService.cs
//
// Author:
// William Lamb (wdlamb@notwires.com)
// Dennis Hayes (dennish@raytek.com)
//
// (C) 2002 Ximian, Inc. http://www.ximian.com
//

namespace System.Windows.Forms {

	public interface IDataGridColumnStyleEditingNotificationService {

		void ColumnStartedEditing(Control editingControl);
	}
}
