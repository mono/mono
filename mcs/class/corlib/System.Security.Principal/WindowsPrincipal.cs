//
// WindowsPrincipal.cs: Windows IPrincipal implementation
//
// Author:
//	Sebastien Pouliot (sebastien@ximian.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Claims;

namespace System.Security.Principal {

	[Serializable]
	[ComVisible (true)]
	public class WindowsPrincipal :
		ClaimsPrincipal
	{
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
		override
		public IIdentity Identity {
			get { return _identity; }
		}

		// methods

		public virtual bool IsInRole (int rid) 
		{
			if (Environment.IsUnix) {
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

		override
		public bool IsInRole (string role)
		{
			if (role == null)
				return false;	// ArgumentNullException

			if (Environment.IsUnix) {
				// note: Posix is always case-sensitive
				using (var rolePtr = new Mono.SafeStringMarshal (role))
					return IsMemberOfGroupName (Token, rolePtr.Value);
			}
			else {
				// Windows specific code that
				// (a) build the role cache like the MS framework (for compatibility)
				// (b) case sensitive (for Fx 1.0) and case insensitive (later Fx)
				if (m_roles == null) {
					m_roles = WindowsIdentity._GetRoles (Token);
				}
				
				role = role.ToUpperInvariant ();
				foreach (string check in m_roles) {
					if ((check != null) && (role == check.ToUpperInvariant ()))
						return true;
				}
				return false;
			}
		}

		public virtual bool IsInRole (WindowsBuiltInRole role)
		{
			if (Environment.IsUnix) {
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
		[MonoTODO ("not implemented")]
		[ComVisible (false)]
		public virtual bool IsInRole (SecurityIdentifier sid)
		{
			throw new NotImplementedException ();
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
		private extern static bool IsMemberOfGroupName (IntPtr user, IntPtr group);
	}
}
