//
// System.Threading.AutoResetEvent.cs
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
 	internal sealed class NativeEventCalls
	{
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public static extern IntPtr CreateEvent_internal(bool manual,bool initial,string name);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public static extern bool SetEvent_internal(IntPtr handle);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public static extern bool ResetEvent_internal(IntPtr handle);
	
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public static extern void CloseEvent_internal (IntPtr handle);
	}
}
