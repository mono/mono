//
// System.Security.Permissions.SecurityPermission.cs
//
// Authors:
//	Dan Lewis (dihlewis@yahoo.co.uk)
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002
// Portions (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

//
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

using System;
using System.Globalization;

namespace System.Security.Permissions {
	
	[Serializable]
	public sealed class SecurityPermission :
		CodeAccessPermission, IUnrestrictedPermission, IBuiltInPermission {

		private SecurityPermissionFlag flags;

		// constructors

		public SecurityPermission (PermissionState state)
		{
			if (state == PermissionState.Unrestricted)
				flags = SecurityPermissionFlag.AllFlags;
			else
				flags = SecurityPermissionFlag.NoFlags;
		}

		public SecurityPermission (SecurityPermissionFlag flags) 
		{
			this.flags = flags;
		}

		public SecurityPermissionFlag Flags {
			get { return flags; }
			set { flags = value; }
		}

		public bool IsUnrestricted () 
		{
			return (flags == SecurityPermissionFlag.AllFlags);
		}

		public override IPermission Copy () 
		{
			return new SecurityPermission (flags);
		}

		internal SecurityPermission Cast (IPermission target) 
		{
			SecurityPermission perm = (target as SecurityPermission);
			if (perm == null)
				throw new ArgumentException ("wrong type for target");
			return perm;
		}

		public override IPermission Intersect (IPermission target) 
		{
			if (target == null)
				return null;

			SecurityPermission perm = Cast (target);
			if (this.IsUnrestricted () && perm.IsUnrestricted ())
				return new SecurityPermission (PermissionState.Unrestricted);
			if (this.IsUnrestricted ())
				return perm.Copy ();
			if (perm.IsUnrestricted ())
				return this.Copy ();
			return new SecurityPermission (flags & perm.flags);
		}

		public override IPermission Union (IPermission target) 
		{
			if (target == null)
				return this.Copy ();

			SecurityPermission perm = Cast (target);
			if (this.IsUnrestricted () || perm.IsUnrestricted ())
				return new SecurityPermission (PermissionState.Unrestricted);
			
			return new SecurityPermission (flags | perm.flags);
		}

		public override bool IsSubsetOf (IPermission target) 
		{
			if (target == null) 
				return (flags == SecurityPermissionFlag.NoFlags);

			SecurityPermission perm = Cast (target);
			if (perm.IsUnrestricted ())
				return true;
			if (this.IsUnrestricted ())
				return false;

			return ((flags & ~perm.flags) == 0);
		}

		public override void FromXml (SecurityElement e) 
		{
			if (e == null)
				throw new ArgumentNullException (
					Locale.GetText ("The argument is null."));
			
			if (e.Attribute ("class") != GetType ().AssemblyQualifiedName)
				throw new ArgumentException (
					Locale.GetText ("The argument is not valid"));

			if (e.Attribute ("version") != "1")
				throw new ArgumentException (
					Locale.GetText ("The argument is not valid"));

			flags = (SecurityPermissionFlag) Enum.Parse (
				typeof (SecurityPermissionFlag), e.Attribute ("Flags"));
		}

		public override SecurityElement ToXml () 
		{
			SecurityElement e = new SecurityElement ("IPermission");
			e.AddAttribute ("class", GetType ().AssemblyQualifiedName);
			e.AddAttribute ("version", "1");

			e.AddAttribute ("Flags", flags.ToString ());

			return e;
		}

		// IBuiltInPermission
		int IBuiltInPermission.GetTokenIndex ()
		{
			return 6;
		}
	}
}
