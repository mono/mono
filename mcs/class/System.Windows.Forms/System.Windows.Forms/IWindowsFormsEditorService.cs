//
// System.Windows.Forms.IWindowsFormsEditorService
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
//

namespace System.Windows.Forms
{
	public interface IWindowsFormsEditorService
	{
		void CloseDropDown();
		void DropDownControl (Control control);
		DialogResult ShowDialog (Form dialog);
	}
}
