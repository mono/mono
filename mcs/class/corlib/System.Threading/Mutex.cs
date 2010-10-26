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
#if !DISABLE_SECURITY
using System.Security.Permissions;
#endif
#if NET_2_0 && !DISABLE_SECURITY
using System.Runtime.ConstrainedExecution;
using System.IO;
using System.Runtime.InteropServices;
#if !NET_2_1 && !DISABLE_SECURITY
using System.Security.AccessControl;
#endif
#endif

namespace System.Threading
{
#if NET_2_0 && !DISABLE_SECURITY
	[ComVisible (true)]
#endif
	public sealed class Mutex : WaitHandle 
	{
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern IntPtr  CreateMutex_internal(
		                                         bool initiallyOwned,
		                                         string name,
							 out bool created);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern bool ReleaseMutex_internal(IntPtr handle);

#if NET_2_0 && !NET_2_1 && !DISABLE_SECURITY
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private static extern IntPtr OpenMutex_internal (string name, MutexRights rights, out MonoIOError error);
		
		private Mutex (IntPtr handle)
		{
			Handle = handle;
		}
#endif
		
#if NET_2_0 && !DISABLE_SECURITY
		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, Cer.MayFail)]
#endif
		public Mutex() {
			bool created;
			
			Handle=CreateMutex_internal(false, null, out created);
		}
		
#if NET_2_0 && !DISABLE_SECURITY
		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, Cer.MayFail)]
#endif
		public Mutex(bool initiallyOwned) {
			bool created;
			
			Handle=CreateMutex_internal(initiallyOwned, null,
						    out created);
		}

#if NET_2_0 && !DISABLE_SECURITY
		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, Cer.MayFail)]
#endif
#if !DISABLE_SECURITY
		[SecurityPermission (SecurityAction.LinkDemand, UnmanagedCode = true)]
#endif
		public Mutex (bool initiallyOwned, string name)
		{
			bool created;
			Handle = CreateMutex_internal (initiallyOwned, name, out created);
		}

#if NET_2_0 && !DISABLE_SECURITY
		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, Cer.MayFail)]
#endif
#if !DISABLE_SECURITY
		[SecurityPermission (SecurityAction.LinkDemand, UnmanagedCode = true)]
#endif
		public Mutex (bool initiallyOwned, string name, out bool createdNew)
		{
			Handle = CreateMutex_internal (initiallyOwned, name, out createdNew);
		}

#if NET_2_0 && !NET_2_1 && !DISABLE_SECURITY
		[MonoTODO ("Implement MutexSecurity")]
		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, Cer.MayFail)]
		public Mutex (bool initiallyOwned, string name, out bool createdNew, MutexSecurity mutexSecurity)
		{
			Handle = CreateMutex_internal (initiallyOwned, name, out createdNew);
		}

		public MutexSecurity GetAccessControl ()
		{
			throw new NotImplementedException ();
		}

		public static Mutex OpenExisting (string name)
		{
			return(OpenExisting (name, MutexRights.Synchronize |
					     MutexRights.Modify));
		}
		
		public static Mutex OpenExisting (string name,
						  MutexRights rights)
		{
			if (name == null) {
				throw new ArgumentNullException ("name");
			}
			if ((name.Length == 0) ||
			    (name.Length > 260)) {
				throw new ArgumentException ("name", Locale.GetText ("Invalid length [1-260]."));
			}
			
			MonoIOError error;
			IntPtr handle = OpenMutex_internal (name, rights,
							    out error);
			if (handle == (IntPtr)null) {
				if (error == MonoIOError.ERROR_FILE_NOT_FOUND) {
					throw new WaitHandleCannotBeOpenedException (Locale.GetText ("Named Mutex handle does not exist: ") + name);
				} else if (error == MonoIOError.ERROR_ACCESS_DENIED) {
					throw new UnauthorizedAccessException ();
				} else {
					throw new IOException (Locale.GetText ("Win32 IO error: ") +  error.ToString ());
				}
			}
			
			return(new Mutex (handle));
		}
#endif

#if NET_2_0 && !DISABLE_SECURITY
		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, Cer.MayFail)]
#endif	
		public void ReleaseMutex() {
			bool success = ReleaseMutex_internal(Handle);
			if (!success) {
				throw new ApplicationException ("Mutex is not owned");
			}
		}

#if NET_2_0 && !NET_2_1 && !DISABLE_SECURITY
		public void SetAccessControl (MutexSecurity mutexSecurity)
		{
			throw new NotImplementedException ();
		}
#endif
	}
}
