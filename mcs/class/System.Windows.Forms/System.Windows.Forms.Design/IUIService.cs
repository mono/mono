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
		void ShowError(string str);
		void ShowError(Exception ex, string str);
		DialogResult ShowDialog(Form form);		
		void ShowMessage(string str);
		void ShowMessage(string str1, string str2);
		DialogResult ShowMessage(string str1, string str2, MessageBoxButtons btn);
		bool ShowToolWindow( Guid toolWindow);
	}
}
