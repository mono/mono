//
// System.Threading.AutoResetEvent.cs
//
// Author:
//   Dick Porter (dick@ximian.com)
//   Veronica De Santis (veron78@interfree.it)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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
using System.Runtime.CompilerServices;
using Microsoft.Win32.SafeHandles;

#if !MOBILE && !NETCORE
using System.Security.AccessControl;
using System.IO;
#endif

namespace System.Threading
{
 	internal static class NativeEventCalls
	{
		public unsafe static IntPtr CreateEvent_internal (bool manual, bool initial, string name, out int errorCode)
		{
			// FIXME check for embedded nuls in name
			fixed (char *fixed_name = name)
				return CreateEvent_icall (manual, initial, fixed_name, name?.Length ?? 0, out errorCode);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private unsafe static extern IntPtr CreateEvent_icall (bool manual, bool initial, char *name, int name_length, out int errorCode);

		public static bool SetEvent (SafeWaitHandle handle)
		{
			bool release = false;
			try {
				handle.DangerousAddRef (ref release);
				return SetEvent_internal (handle.DangerousGetHandle ());
			} finally {
				if (release)
					handle.DangerousRelease ();
			}
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		static extern bool SetEvent_internal(IntPtr handle);

		public static bool ResetEvent (SafeWaitHandle handle)
		{
			bool release = false;
			try {
				handle.DangerousAddRef (ref release);
				return ResetEvent_internal (handle.DangerousGetHandle ());
			} finally {
				if (release)
					handle.DangerousRelease ();
			}
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		static extern bool ResetEvent_internal(IntPtr handle);
	
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public static extern void CloseEvent_internal (IntPtr handle);

#if !MOBILE && !NETCORE
		public unsafe static IntPtr OpenEvent_internal (string name, EventWaitHandleRights rights, out int errorCode)
		{
			// FIXME check for embedded nuls in name
			fixed (char *fixed_name = name)
				return OpenEvent_icall (fixed_name, name?.Length ?? 0, rights, out errorCode);
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private unsafe static extern IntPtr OpenEvent_icall (char *name, int name_length, EventWaitHandleRights rights, out int errorCode);
#endif
	}
}
