//
// System.Threading.Mutex.cs
//
// Author:
//
//   Dick Porter (dick@ximian.com)
//   Veronica De Santis (veron78@interfree.it)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Runtime.CompilerServices;

namespace System.Threading
{
	public sealed class Mutex : WaitHandle 
	{
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern IntPtr  CreateMutex_internal(
		                                         bool initiallyOwned,
		                                         string name);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern void ReleaseMutex_internal(IntPtr handle);

		public Mutex() {
			Handle=CreateMutex_internal(false,null);
		}
		
		public Mutex(bool initiallyOwned) {
			Handle=CreateMutex_internal(initiallyOwned,null);
		}

		public Mutex(bool initiallyOwned, string name) {				
			Handle=CreateMutex_internal(initiallyOwned,name);	
		}
	

		public Mutex(bool initiallyOwned, string name, out bool gotOwnership) {
			Handle=CreateMutex_internal(initiallyOwned,name);
			gotOwnership=false;
		}
	
		public void ReleaseMutex() {
			ReleaseMutex_internal(Handle);
		}
	}
}
