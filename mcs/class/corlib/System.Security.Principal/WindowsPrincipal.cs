//
// WindowsPrincipal.cs: Windows IPrincipal implementation
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;

namespace System.Security.Principal {

	[Serializable]
	public class WindowsPrincipal : IPrincipal {

		private WindowsIdentity _identity;

		public WindowsPrincipal (WindowsIdentity ntIdentity)
		{
			if (ntIdentity == null)
				throw new ArgumentNullException ("ntIdentity");

			_identity = ntIdentity;
		}

		// properties

		public virtual IIdentity Identity {
			get { return _identity; }
		}

		// methods

		[MonoTODO]
		public virtual bool IsInRole (int rid) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual bool IsInRole (string role)
		{
			if (role == null)
				return false;	// ArgumentNullException
#if NET_1_0
			// case sensitive (for 1.0)
#else
			// case insensitive (for 1.1 and later)
#endif
			throw new NotImplementedException ();
		}

		public virtual bool IsInRole (WindowsBuiltInRole role)
		{
			return IsInRole ((int)role);
		}
	}
}
