//
// WindowsPrincipal.cs: Windows IPrincipal implementation
//
// Author:
//	Sebastien Pouliot (sebastien@ximian.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// (C) 2004 Novell (http://www.novell.com)
//

using System;
using System.Collections;
using System.Runtime.CompilerServices;

namespace System.Security.Principal {

	[Serializable]
	public class WindowsPrincipal : IPrincipal {

		private WindowsIdentity _identity;
		// http://groups.google.ca/groups?q=WindowsPrincipal+m_roles&hl=en&lr=&ie=UTF-8&oe=UTF-8&selm=OghXf4OgCHA.4228%40tkmsftngp08&rnum=4
		private string [] m_roles;

		// case sensitivity versus number of groups
		// http://groups.google.ca/groups?q=WindowsPrincipal+m_roles&hl=en&lr=&ie=UTF-8&oe=UTF-8&selm=%23JEMHsMQCHA.1916%40tkmsftngp13&rnum=5

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

		public virtual bool IsInRole (int rid) 
		{
			if (IsPosix) {
				return IsMemberOfGroupId (Token, (IntPtr) rid);
			}
			else {
				string role = null;
				switch (rid) {
					case 544: // Administrator
						role = "BUILTIN\\Administrators";
						break;
					case 545: // User
						role = "BUILTIN\\Users";
						break;
					case 546: // Guest
						role = "BUILTIN\\Guests";
						break;
					case 547: // PowerUser
						role = "BUILTIN\\Power Users";
						break;
					case 548: // AccountOperator
						role = "BUILTIN\\Account Operators";
						break;
					case 549: // SystemOperator
						role = "BUILTIN\\System Operators";
						break;
					case 550: // PrintOperator
						role = "BUILTIN\\Print Operators";
						break;
					case 551: // BackupOperator
						role = "BUILTIN\\Backup Operators";
						break;
					case 552: // Replicator
						role = "BUILTIN\\Replicator";
						break;
					default:
						return false;
				}
				return IsInRole (role);
			}
		}

		public virtual bool IsInRole (string role)
		{
			if (role == null)
				return false;	// ArgumentNullException

			if (IsPosix) {
				// note: Posix is always case-sensitive
				return IsMemberOfGroupName (Token, role);
			}
			else {
				// Windows specific code that
				// (a) build the role cache like the MS framework (for compatibility)
				// (b) case sensitive (for Fx 1.0) and case insensitive (later Fx)
				if (m_roles == null) {
					m_roles = WindowsIdentity._GetRoles (Token);
				}
#if !NET_1_0
				role = role.ToUpper ();
#endif
				foreach (string check in m_roles) {
#if NET_1_0
					if (role == check)
						return true;
#else
					Console.WriteLine ("> {0}", check);
					if ((check != null) && (role == check.ToUpper ()))
						return true;
#endif
				}
				return false;
			}
		}

		public virtual bool IsInRole (WindowsBuiltInRole role)
		{
			if (IsPosix) {
				// right now we only map Administrator == root
				string group = null;
				switch (role) {
					case WindowsBuiltInRole.Administrator:
						group = "root";
						break;
					default:
						return false;
				}
				return IsInRole (group);
			}
			else {
				return IsInRole ((int) role);
			}
		}

		private static bool IsPosix {
			get { return ((int) Environment.Platform == 128); }
		}

		private IntPtr Token {
			get { return (_identity as WindowsIdentity).Token; }
		}

		// see mono/mono/metadata/security.c for implementation

		// note: never called by Win32 code (i.e. always return false)
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern static bool IsMemberOfGroupId (IntPtr user, IntPtr group);

		// note: never called by Win32 code (i.e. always return false)
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern static bool IsMemberOfGroupName (IntPtr user, string group);
	}
}
