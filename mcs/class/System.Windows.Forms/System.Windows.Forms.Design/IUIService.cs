//
// System.Windows.Forms.Design.IUIService.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//

using System.Runtime.InteropServices;
using System.Collections;

namespace System.Windows.Forms.Design
{
	[Guid ("06a9c74b-5e32-4561-be73-381b37869f4f")]
	public interface IUIService
	{
		IDictionary Styles {get;}

		bool CanShowComponentEditor (object component);
		IWin32Window GetDialogOwnerWindow ();
		void SetUIDirty ();
		bool ShowComponentEditor (object component, IWin32Window parent);
		void ShowError (Exception ex);
		void ShowError (string message);
		void ShowError (Exception ex, string message);
		DialogResult ShowDialog (Form form);
		void ShowMessage (string message);
		void ShowMessage (string message, string caption);
		DialogResult ShowMessage (string message, string caption, MessageBoxButtons buttons);
		bool ShowToolWindow (Guid toolWindow);
	}
}
