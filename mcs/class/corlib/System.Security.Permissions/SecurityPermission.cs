//
// System.Security.Permissions.SecurityPermission.cs
//
// Authors:
//	Dan Lewis (dihlewis@yahoo.co.uk)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2002
// Portions (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

using System.Globalization;

namespace System.Security.Permissions {
	
	[Serializable]
	public sealed class SecurityPermission :
		CodeAccessPermission, IUnrestrictedPermission, IBuiltInPermission {

		private const int version = 1;

		private SecurityPermissionFlag flags;

		// constructors

		public SecurityPermission (PermissionState state)
		{
			if (CheckPermissionState (state, true) == PermissionState.Unrestricted)
				flags = SecurityPermissionFlag.AllFlags;
			else
				flags = SecurityPermissionFlag.NoFlags;
		}

		public SecurityPermission (SecurityPermissionFlag flags) 
		{
			// reuse validation by the Flags property
			Flags = flags;
		}

		public SecurityPermissionFlag Flags {
			get { return flags; }
			set {
				if ((value & SecurityPermissionFlag.AllFlags) != value) {
					string msg = String.Format (Locale.GetText ("Invalid flags {0}"), value);
					throw new ArgumentException (msg, "SecurityPermissionFlag");
				}
				flags = value;
			}
		}

		// IUnrestrictedPermission
		public bool IsUnrestricted () 
		{
			return (flags == SecurityPermissionFlag.AllFlags);
		}

		public override IPermission Copy () 
		{
			return new SecurityPermission (flags);
		}

		public override IPermission Intersect (IPermission target) 
		{
			SecurityPermission sp = Cast (target);
			if (sp == null)
				return null;
			if (IsEmpty () || sp.IsEmpty ())
				return null;

			if (this.IsUnrestricted () && sp.IsUnrestricted ())
				return new SecurityPermission (PermissionState.Unrestricted);
			if (this.IsUnrestricted ())
				return sp.Copy ();
			if (sp.IsUnrestricted ())
				return this.Copy ();

			SecurityPermissionFlag f = flags & sp.flags;
			if (f == SecurityPermissionFlag.NoFlags)
				return null;
			else
				return new SecurityPermission (f);
		}

		public override IPermission Union (IPermission target) 
		{
			SecurityPermission sp = Cast (target);
			if (sp == null)
				return this.Copy ();

			if (this.IsUnrestricted () || sp.IsUnrestricted ())
				return new SecurityPermission (PermissionState.Unrestricted);
			
			return new SecurityPermission (flags | sp.flags);
		}

		public override bool IsSubsetOf (IPermission target) 
		{
			SecurityPermission sp = Cast (target);
			if (sp == null) 
				return IsEmpty ();

			if (sp.IsUnrestricted ())
				return true;
			if (this.IsUnrestricted ())
				return false;

			return ((flags & ~sp.flags) == 0);
		}

		public override void FromXml (SecurityElement e) 
		{
			// General validation in CodeAccessPermission
			CheckSecurityElement (e, "e", version, version);
			// Note: we do not (yet) care about the return value 
			// as we only accept version 1 (min/max values)

			if (IsUnrestricted (e)) {
				flags = SecurityPermissionFlag.AllFlags;
			}
			else {
				string f = e.Attribute ("Flags");
				if (f == null) {
					flags = SecurityPermissionFlag.NoFlags;
				}
				else {
					flags = (SecurityPermissionFlag) Enum.Parse (
						typeof (SecurityPermissionFlag), f);
				}
			}
		}

		public override SecurityElement ToXml () 
		{
			SecurityElement e = Element (version);
			if (IsUnrestricted ())
				e.AddAttribute ("Unrestricted", "true");
			else
				e.AddAttribute ("Flags", flags.ToString ());
			return e;
		}

		// IBuiltInPermission
		int IBuiltInPermission.GetTokenIndex ()
		{
			return (int) BuiltInToken.Security;
		}

		// helpers

		private bool IsEmpty ()
		{
			return (flags == SecurityPermissionFlag.NoFlags);
		}

		private SecurityPermission Cast (IPermission target)
		{
			if (target == null)
				return null;

			SecurityPermission sp = (target as SecurityPermission);
			if (sp == null) {
				ThrowInvalidPermission (target, typeof (SecurityPermission));
			}

			return sp;
		}
	}
}
