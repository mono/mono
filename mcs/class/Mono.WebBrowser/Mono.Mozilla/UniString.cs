//Permission is hereby granted, free of charge, to any person obtaining
//a copy of this software and associated documentation files (the
//"Software"), to deal in the Software without restriction, including
//without limitation the rights to use, copy, modify, merge, publish,
//distribute, sublicense, and/or sell copies of the Software, and to
//permit persons to whom the Software is furnished to do so, subject to
//the following conditions:
//
//The above copyright notice and this permission notice shall be
//included in all copies or substantial portions of the Software.
//
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
//EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
//NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
//LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
//OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
//WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
//Copyright (c) 2008 Novell, Inc.
//
//Authors:
//	Andreia Gaita (avidigal@novell.com)
//

using System;
using System.Runtime.InteropServices;

namespace Mono.Mozilla
{
	internal class UniString : IDisposable
	{
		[StructLayout(LayoutKind.Sequential)]
		class nsStringContainer {
			IntPtr v;
			IntPtr d1;
			uint d2;
			IntPtr d3;
		}
		private bool disposed = false;
		private nsStringContainer unmanagedContainer;
		private HandleRef handle;
		private string str = String.Empty;
		private bool dirty;
		
		public UniString(string value)
		{
			unmanagedContainer = new nsStringContainer ();
			IntPtr p = Marshal.AllocHGlobal (Marshal.SizeOf (typeof (nsStringContainer)));
			Marshal.StructureToPtr (unmanagedContainer, p, false);
			handle = new HandleRef (typeof (nsStringContainer), p);
			Base.gluezilla_StringContainerInit (handle);
			String = value;
		}
		
		~UniString ()
		{
			Dispose (false);
		}		
		
		#region IDisposable Members

		protected virtual void Dispose (bool disposing)
		{
			if (!disposed) {
				if (disposing) {
					 Base.gluezilla_StringContainerFinish (handle);
					 Marshal.FreeHGlobal (handle.Handle);
				}
				disposed = true;
			}
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		#endregion
		
		
		public HandleRef Handle {
			get {
				dirty = true;
				return handle; 
			}
		}
		
		public string String { 
			get {
				if (dirty) {
					IntPtr buf;
					bool term;
					Base.gluezilla_StringGetData (handle, out buf, out term);
					str = Marshal.PtrToStringUni (buf);
					dirty = false;
				}
				return str;
			}
			set {
				if (str != value) {
					str = value;
					Base.gluezilla_StringSetData (handle, str, (uint)str.Length);
				}				
			}
		}
		
		public int Length {
			get { return String.Length; }
		}
		
		public override string ToString ()
		{
			return String;
		}
		
	}
}
