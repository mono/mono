//
// System.Threading.ManualResetEvent.cs
//
// Author:
//   Dick Porter (dick@ximian.com)
//   Veronica De Santis (veron78@interfree.it)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Runtime.CompilerServices;

namespace System.Threading 
{

 	public sealed class ManualResetEvent : WaitHandle 
	{
		// Constructor
		public ManualResetEvent(bool initialState) {
			os_handle = NativeEventCalls.CreateEvent_internal(true,initialState,null);
		}

		// Methods

		public bool Set() {
			return(NativeEventCalls.SetEvent_internal(os_handle));
		}

		public bool Reset() {
			return(NativeEventCalls.ResetEvent_internal(os_handle));
		}

	}
}
