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
// Copyright (c) 2004 Novell, Inc.
//
// Authors:
//	Andreas Nahr	(ClassDevelopment@A-SoftTech.com)
//

// NOT COMPLETE

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
