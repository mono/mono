//
// System.Windows.Forms.MessageBox.cs
//
// Author:
//   stubbed out by Paul Osman (paul.osman@sympatico.ca)
//   Dennis Hayes (dennish@Raytek.com)
//   WINELib implementation started by John Sohn (jsohn@columbus.rr.com)
//
// (C) 2002 Ximian, Inc
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
		[MonoTODO]
		public override bool Equals(object o) 
		{
			throw new NotImplementedException ();
		}
                
		//public static bool Equals(object o1, object o2) 
		//{
		//	throw new NotImplementedException ();
		//}
                
		//Compact Framework
		[MonoTODO]
		public override int GetHashCode() 
		{
			//FIXME add our proprities
			return base.GetHashCode();
		}
                
		//Compact Framework
		//public Type GetType() {
		//	throw new NotImplementedException ();
		//}
                
		//Compact Framework
		public static DialogResult Show(string text) 
		{
			return (DialogResult) 
			    Win32.MessageBoxA ((IntPtr) 0, text, "", 
					       Win32.MB_OK);
		}
                
		public static DialogResult Show (IWin32Window w, string text) 
		{
			return (DialogResult) 
			    Win32.MessageBoxA (w.Handle, text, "", 
					       Win32.MB_OK);
		}
                
		//Compact Framework
		public static DialogResult Show (string text, string caption) 
		{
			return (DialogResult) 
			    Win32.MessageBoxA ((IntPtr) 0, text, caption, 
					       Win32.MB_OK);
		}
                
		public static DialogResult Show (IWin32Window w, string text, 
						 string caption) 
		{
			return (DialogResult) 
			    Win32.MessageBoxA (w.Handle, text, caption, 
					       Win32.MB_OK);
		}
                
		public static DialogResult Show (string text, string caption, 
						 MessageBoxButtons mb) 
		{
			return (DialogResult) 
			    Win32.MessageBoxA ((IntPtr) 0, text, caption, 
					       (uint) mb);
		}
                
		public static DialogResult Show (
			IWin32Window w, string text, 
			string caption, MessageBoxButtons mb) 
		{
			return (DialogResult) 
			    Win32.MessageBoxA (w.Handle, text, caption, 
					       (uint) mb);
		}
                
		public static DialogResult Show (
			string text, string caption, MessageBoxButtons mb, 
			MessageBoxIcon mi) 
		{
			return (DialogResult) 
			    Win32.MessageBoxA ((IntPtr) 0, text, caption, 
					       (uint) (mb | mi) );
		}
                
		public static DialogResult Show (
			IWin32Window w, string text, string caption, 
			MessageBoxButtons mb, MessageBoxIcon mi) 
		{
			return (DialogResult) 
			    Win32.MessageBoxA (w.Handle, text, caption, 
					       (uint) (mb | mi) );
		}
                
		//Compact Framework
		public static DialogResult Show (
			string text, string caption, MessageBoxButtons mb, 
			MessageBoxIcon mi, MessageBoxDefaultButton md) 
		{
			return (DialogResult) 
			    Win32.MessageBoxA ((IntPtr) 0, text, caption, 
					       (uint) (mb | mi | md) );

		}
                
		public static DialogResult Show (
			IWin32Window w, string text, string caption, 
			MessageBoxButtons mb, MessageBoxIcon mi, 
			MessageBoxDefaultButton md) 
		{
			return (DialogResult) 
			    Win32.MessageBoxA (w.Handle, text, caption, 
					       (uint) (mb | mi | md) );
		}
                
		public static DialogResult 
			Show (string text, string caption, 
			      MessageBoxButtons mb, MessageBoxIcon mi, 
			      MessageBoxDefaultButton md, MessageBoxOptions mo)
		{
			return (DialogResult) 
			    Win32.MessageBoxA ((IntPtr) 0, text, caption, 
					       (uint) (mb | mi | md | mo) );
		}
                
		public static DialogResult Show (
			IWin32Window w, string text, string caption, 
			MessageBoxButtons mb, MessageBoxIcon mi, 
			MessageBoxDefaultButton md, MessageBoxOptions mo) 
		{
			return (DialogResult) 
			    Win32.MessageBoxA (w.Handle, text, caption, 
					       (uint) (mb | mi | md | mo) );
		}

		//Compact Framework
		[MonoTODO]                
		public override string ToString () {
			throw new NotImplementedException ();
		}

		//Compact Framework
		[MonoTODO]                
		~MessageBox () {
			throw new NotImplementedException ();
		}

		//Compact Framework
		[MonoTODO]                
		protected object MemberWiseClone () {
			throw new NotImplementedException ();
		}
	}
}
