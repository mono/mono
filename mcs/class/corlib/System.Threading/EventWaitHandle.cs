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

using System.Security.AccessControl;

namespace System.Threading
{
	public class EventWaitHandle : WaitHandle
	{
		public EventWaitHandle (bool initialState, EventResetMode mode)
		{
			Handle = NativeEventCalls.CreateEvent_internal ((mode == EventResetMode.ManualReset), initialState, null);
		}
		
		public EventWaitHandle (bool initialState, EventResetMode mode,
					string name)
		{
			Handle = NativeEventCalls.CreateEvent_internal ((mode == EventResetMode.ManualReset), initialState, name);
		}
		
		[MonoTODO ("Implement createdNew")]
		public EventWaitHandle (bool initialState, EventResetMode mode,
					string name, out bool createdNew)
		{
			Handle = NativeEventCalls.CreateEvent_internal ((mode == EventResetMode.ManualReset), initialState, name);
			createdNew = false;
		}
		
		[MonoTODO ("Implement createdNew and access control")]
		public EventWaitHandle (bool initialState, EventResetMode mode,
					string name, out bool createdNew,
					EventWaitHandleSecurity eventSecurity)
		{
			Handle = NativeEventCalls.CreateEvent_internal ((mode == EventResetMode.ManualReset), initialState, name);
			createdNew = false;
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

		[MonoTODO]
		public static EventWaitHandle OpenExisting (string name, EventWaitHandleRights rights)
		{
			throw new NotImplementedException ();
		}
		
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

		[MonoTODO]
		public void SetAccessControl (EventWaitHandleSecurity eventSecurity)
		{
			throw new NotImplementedException ();
		}
	}
}

#endif
