//
// System.Windows.Forms.MessageBox.cs
//
// Author:
//   stubbed out by Paul Osman (paul.osman@sympatico.ca)
//   Dennis Hayes (dennish@Raytek.com)
//   WINELib implementation started by John Sohn (jsohn@columbus.rr.com)
//
// (C) 2002/3 Ximian, Inc
//

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

using System;
using System.Reflection;
using System.Globalization;
//using System.Windows.Forms.AccessibleObject.IAccessible;
using System.Drawing;

namespace System.Windows.Forms {

	/// <summary>
	/// Most items complete
        /// </summary>
	public class MessageBox {

		private MessageBox(){//For signiture compatablity. Prevents the auto creation of public constructor
		}

		//
		// -- Public Methods
		//
                
		//Compact Framework
		public static DialogResult Show(string text) 
		{
			return (DialogResult) 
			    Win32.MessageBoxA ((IntPtr) 0, text, "", (uint)(WB_MessageBox_Types.MB_OK | WB_MessageBox_Types.MB_TASKMODAL));
		}
                
		public static DialogResult Show (IWin32Window owner, string text) 
		{
			return (DialogResult) 
			    Win32.MessageBoxA (owner.Handle, text, "", (uint)WB_MessageBox_Types.MB_OK);
		}
                
		//Compact Framework
		public static DialogResult Show (string text, string caption) 
		{
			return (DialogResult) 
			    Win32.MessageBoxA ((IntPtr) 0, text, caption, (uint)WB_MessageBox_Types.MB_OK);
		}
                
		public static DialogResult Show (IWin32Window owner, string text, string caption)
		{
			return (DialogResult) 
			    Win32.MessageBoxA (owner.Handle, text, caption, (uint)WB_MessageBox_Types.MB_OK);
		}
                
		public static DialogResult Show (string text, string caption, 
						 MessageBoxButtons buttons) 
		{
			return (DialogResult) 
			    Win32.MessageBoxA ((IntPtr) 0, text, caption, (uint) buttons);
		}
                
		public static DialogResult Show (
			IWin32Window owner, string text, 
			string caption, MessageBoxButtons buttons) 
		{
			return (DialogResult) 
			    Win32.MessageBoxA (owner.Handle, text, caption, (uint) buttons);
		}
                
		public static DialogResult Show (
			string text, string caption, MessageBoxButtons buttons, 
			MessageBoxIcon icon) 
		{
			return (DialogResult) 
			    Win32.MessageBoxA ((IntPtr) 0, text, caption, 
					       (uint) ((uint)buttons | (uint)icon) );
		}
                
		public static DialogResult Show (
			IWin32Window owner, string text, string caption, 
			MessageBoxButtons buttons, MessageBoxIcon icon) 
		{
			return (DialogResult) 
			    Win32.MessageBoxA (owner.Handle, text, caption, 
					       (uint) ((uint)buttons |(uint) icon) );
		}
                
		//Compact Framework
		public static DialogResult Show (
			string text, string caption, MessageBoxButtons buttons, 
			MessageBoxIcon icon, MessageBoxDefaultButton defaultButton) 
		{
			return (DialogResult) 
			    Win32.MessageBoxA ((IntPtr) 0, text, caption, 
					       (uint) ((uint)buttons | (uint)icon | (uint)defaultButton) );

		}
                
		public static DialogResult Show (
			IWin32Window owner, string text, string caption, 
			MessageBoxButtons buttons, MessageBoxIcon icon, 
			MessageBoxDefaultButton defaultButton) 
		{
			return (DialogResult) 
			    Win32.MessageBoxA (owner.Handle, text, caption, 
					       (uint) ((uint)buttons | (uint)icon | (uint)defaultButton) );
		}
                
		public static DialogResult 
			Show (string text, string caption, 
			      MessageBoxButtons buttons, MessageBoxIcon icon, 
			      MessageBoxDefaultButton defaultButton, MessageBoxOptions options)
		{
			return (DialogResult) 
			    Win32.MessageBoxA ((IntPtr) 0, text, caption, 
					       (uint) ((uint)buttons | (uint)icon | (uint)defaultButton |(uint) options) );
		}
                
		public static DialogResult Show (
			IWin32Window owner, string text, string caption, 
			MessageBoxButtons buttons, MessageBoxIcon icon, 
			MessageBoxDefaultButton defaultButton, MessageBoxOptions options) 
		{
			return (DialogResult) 
			    Win32.MessageBoxA (owner.Handle, text, caption, 
					       (uint) ((uint)buttons | (uint)icon | (uint)defaultButton | (uint)options) );
		}
	}
}
