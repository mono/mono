using System;

namespace System.Windows.Forms.Design
{
	public interface IMenuEditorService
	{
		Menu GetMenu();
		bool IsActive();
		bool MessageFilter(ref Message m);
		void SetMenu(Menu menu);
		void SetSelection(MenuItem item);
	}
}
