//
// System.Security.Principal.WindowsImpersonationContext
//
// Authors:
//      Gonzalo Paniagua Javier (gonzalo@ximian.com)
//	Sebastien Pouliot  (sebastien@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
// (C) 2004 Novell (http://www.novell.com)
//

using System;
using System.Runtime.CompilerServices;
using System.Security;

namespace System.Security.Principal {

	public class WindowsImpersonationContext {

		private IntPtr _token;
		private bool undo;

		internal WindowsImpersonationContext (IntPtr token)
		{
			// we get a copy to control it's lifetime
			_token = DuplicateToken (token);
			if (!SetCurrentToken (token)) {
				throw new SecurityException ("Couldn't impersonate token.");
			}
			undo = false;
		}

		~WindowsImpersonationContext ()
		{
			if (!undo) {
				Undo ();
			}
		}

		public void Undo ()
		{
			if (!RevertToSelf ()) {
				CloseToken (_token);
				throw new SecurityException ("Couldn't switch back to original token.");
			}
			CloseToken (_token);
			undo = true;
			GC.SuppressFinalize (this);
		}

		// see mono/mono/metadata/security.c for implementation

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern static bool CloseToken (IntPtr token);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern static IntPtr DuplicateToken (IntPtr token);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern static bool SetCurrentToken (IntPtr token);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern static bool RevertToSelf ();
	}
}
