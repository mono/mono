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
