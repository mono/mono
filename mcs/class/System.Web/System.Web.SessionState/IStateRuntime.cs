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
			[In] IntPtr tracker, [In] int verb,
			[In] string uri, [In] int exclusive, [In] int timeout,
			[In] int lockCookieExists, [In] int lockCookie,
			[In] int contentLength, [In] IntPtr content);

		void StopProcessing ();
	}
}
