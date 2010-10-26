//
// System.Threading.EventWaitHandle.cs
//
// Author:
// 	Dick Porter (dick@ximian.com)
//
// (C) Ximian, Inc.	(http://www.ximian.com)
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

#if NET_2_0

using System.IO;
using System.Runtime.InteropServices;
#if !NET_2_1 && !DISABLE_SECURITY
using System.Security.AccessControl;
#endif

namespace System.Threading
{
#if !DISABLE_SECURITY
	[ComVisible (true)]
#endif
	public class EventWaitHandle : WaitHandle
	{
		private EventWaitHandle (IntPtr handle)
		{
			Handle = handle;
		}
		
		private bool IsManualReset (EventResetMode mode)
		{
			if ((mode < EventResetMode.AutoReset) || (mode > EventResetMode.ManualReset))
				throw new ArgumentException ("mode");
			return (mode == EventResetMode.ManualReset);
		}
		
		public EventWaitHandle (bool initialState, EventResetMode mode)
		{
			bool created;
			bool manual = IsManualReset (mode);
			Handle = NativeEventCalls.CreateEvent_internal (manual, initialState, null, out created);
		}
		
		public EventWaitHandle (bool initialState, EventResetMode mode,
					string name)
		{
			bool created;
			bool manual = IsManualReset (mode);
			Handle = NativeEventCalls.CreateEvent_internal (manual, initialState, name, out created);
		}
		
		public EventWaitHandle (bool initialState, EventResetMode mode,
					string name, out bool createdNew)
		{
			bool manual = IsManualReset (mode);
			Handle = NativeEventCalls.CreateEvent_internal (manual, initialState, name, out createdNew);
		}
#if !NET_2_1 && !DISABLE_SECURITY
		[MonoTODO ("Implement access control")]
		public EventWaitHandle (bool initialState, EventResetMode mode,
					string name, out bool createdNew,
					EventWaitHandleSecurity eventSecurity)
		{
			bool manual = IsManualReset (mode);
			Handle = NativeEventCalls.CreateEvent_internal (manual, initialState, name, out createdNew);
		}
		
		[MonoTODO]
		public EventWaitHandleSecurity GetAccessControl ()
		{
			throw new NotImplementedException ();
		}
		
		public static EventWaitHandle OpenExisting (string name)
		{
			return(OpenExisting (name, EventWaitHandleRights.Synchronize | EventWaitHandleRights.Modify));
		}

		public static EventWaitHandle OpenExisting (string name, EventWaitHandleRights rights)
		{
			if (name == null) {
				throw new ArgumentNullException ("name");
			}
			if ((name.Length == 0) ||
			    (name.Length > 260)) {
				throw new ArgumentException ("name", Locale.GetText ("Invalid length [1-260]."));
			}
			
			MonoIOError error;
			IntPtr handle = NativeEventCalls.OpenEvent_internal (name, rights, out error);
			if (handle == (IntPtr)null) {
				if (error == MonoIOError.ERROR_FILE_NOT_FOUND) {
					throw new WaitHandleCannotBeOpenedException (Locale.GetText ("Named Event handle does not exist: ") + name);
				} else if (error == MonoIOError.ERROR_ACCESS_DENIED) {
					throw new UnauthorizedAccessException ();
				} else {
					throw new IOException (Locale.GetText ("Win32 IO error: ") + error.ToString ());
				}
			}
			
			return(new EventWaitHandle (handle));
		}
#endif
		public bool Reset ()
		{
			CheckDisposed ();
			
			return (NativeEventCalls.ResetEvent_internal (Handle));
		}
		
		public bool Set ()
		{
			CheckDisposed ();
			
			return (NativeEventCalls.SetEvent_internal (Handle));
		}
#if !NET_2_1 && !DISABLE_SECURITY
		[MonoTODO]
		public void SetAccessControl (EventWaitHandleSecurity eventSecurity)
		{
			throw new NotImplementedException ();
		}
#endif
	}
}

#endif
