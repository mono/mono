//
// System.Web.SessionState.IStateRuntime.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Andreas Nahr
//

using System;
using System.Runtime.InteropServices;

namespace System.Web.SessionState
{
	[Guid ("7297744b-e188-40bf-b7e9-56698d25cf44")]
	[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
	public interface IStateRuntime
	{
		void ProcessRequest (
			[In, MarshalAs(UnmanagedType.SysInt)] IntPtr tracker, 
			[In, MarshalAs(UnmanagedType.I4)] int verb,
			[In, MarshalAs(UnmanagedType.LPWStr)] string uri,
			[In, MarshalAs(UnmanagedType.I4)] int exclusive, 
			[In, MarshalAs(UnmanagedType.I4)] int timeout,
			[In, MarshalAs(UnmanagedType.I4)] int lockCookieExists,
			[In, MarshalAs(UnmanagedType.I4)] int lockCookie,
			[In, MarshalAs(UnmanagedType.I4)] int contentLength,
			[In, MarshalAs(UnmanagedType.SysInt)] IntPtr content);

		void StopProcessing ();
	}
}
