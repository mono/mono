using System;
using System.Collections;

namespace System.Windows.Forms.Design
{
	/// <summary>
	/// Summary description for IUIService.
	/// </summary>
	public interface IUIService
	{
		IDictionary Styles {get;}
		
		bool CanShowComponentEditor( object component);		
		IWin32Window GetDialogOwnerWindow();
		void SetUIDirty();
		bool ShowComponentEditor( object component, IWin32Window parent);		
		void ShowError(Exception ex);
		void ShowError(string message);
		void ShowError(Exception ex, string message);
		DialogResult ShowDialog(Form form);		
		void ShowMessage(string message);
		void ShowMessage(string message, string caption);
		DialogResult ShowMessage(string message, string caption, MessageBoxButtons buttons);
		bool ShowToolWindow( Guid toolWindow);
	}
}
