//
// System.Web.SessionState.StateRuntime.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Andreas Nahr
//

namespace System.Web.SessionState
{
	public sealed class StateRuntime : IStateRuntime
	{
		[MonoTODO]
		public void ProcessRequest (IntPtr tracker, int verb,
			string uri, int exclusive, int timeout,
			int lockCookieExists, int lockCookie,
			int contentLength, IntPtr content)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void StopProcessing ()
		{
			throw new NotImplementedException ();
		}
	}
}
