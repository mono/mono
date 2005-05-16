//
// System.Threading.Mutex.cs
//
// Author:
//   Dick Porter (dick@ximian.com)
//   Veronica De Santis (veron78@interfree.it)
//
// (C) Ximian, Inc.  http://www.ximian.com
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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

using System.Runtime.CompilerServices;
using System.Security.Permissions;

namespace System.Threading
{
	public sealed class Mutex : WaitHandle 
	{
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern IntPtr  CreateMutex_internal(
		                                         bool initiallyOwned,
		                                         string name,
							 out bool created);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern void ReleaseMutex_internal(IntPtr handle);

		public Mutex() {
			bool created;
			
			Handle=CreateMutex_internal(false, null, out created);
		}
		
		public Mutex(bool initiallyOwned) {
			bool created;
			
			Handle=CreateMutex_internal(initiallyOwned, null,
						    out created);
		}

		[SecurityPermission (SecurityAction.LinkDemand, UnmanagedCode = true)]
		public Mutex (bool initiallyOwned, string name)
		{
			bool created;
			Handle = CreateMutex_internal (initiallyOwned, name, out created);
		}

		[SecurityPermission (SecurityAction.LinkDemand, UnmanagedCode = true)]
		public Mutex (bool initiallyOwned, string name, out bool createdNew)
		{
			Handle = CreateMutex_internal (initiallyOwned, name, out createdNew);
		}
	
		public void ReleaseMutex() {
			ReleaseMutex_internal(Handle);
		}
	}
}
