//
// System.Security.Principal.WindowsImpersonationContext
//
// Authors:
//      Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//

using System;

namespace System.Security.Principal
{
	[MonoTODO]
	public class WindowsImpersonationContext
	{
		private IntPtr _token;

		internal WindowsImpersonationContext (IntPtr token)
		{
			_token = token;
			throw new NotImplementedException ();
		}

		~WindowsImpersonationContext ()
		{
			_token = (IntPtr) 0;
		}

		[MonoTODO]
		public void Undo ()
		{
			throw new NotImplementedException ();
		}
	}
}

