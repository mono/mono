//
// System.Web.SessionState.IStateRuntime.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Andreas Nahr
//

using System;

namespace System.Web.SessionState
{
	public interface IStateRuntime
	{
		void ProcessRequest (IntPtr tracker, int verb,
			string uri, int exclusive, int timeout,
			int lockCookieExists, int lockCookie,
			int contentLength, IntPtr content);

		void StopProcessing ();
	}
}
