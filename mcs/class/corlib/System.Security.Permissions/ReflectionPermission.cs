//
// System.Security.Permissions.ReflectionPermission.cs
//
// Authors:
//	Tim Coleman <tim@timcoleman.com>
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2002, Tim Coleman
// Portions (C) 2003 Motus Technologies Inc. (http://www.motus.com)
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

using System.Globalization;
using System.Runtime.InteropServices;

namespace System.Security.Permissions {

#if NET_2_0
	[ComVisible (true)]
#endif
	[Serializable]
	public sealed class ReflectionPermission : CodeAccessPermission, IUnrestrictedPermission, IBuiltInPermission {

		private const int version = 1;

		ReflectionPermissionFlag flags;


		public ReflectionPermission (PermissionState state)
		{
			if (CheckPermissionState (state, true) == PermissionState.Unrestricted)
				flags = ReflectionPermissionFlag.AllFlags;
			else
				flags = ReflectionPermissionFlag.NoFlags;
		}

		public ReflectionPermission (ReflectionPermissionFlag flag)
		{
			// reuse validation by the Flags property
			Flags = flag;
		}


		public ReflectionPermissionFlag Flags {
			get { return flags; }
			set {
#if NET_2_0
				const ReflectionPermissionFlag all_flags = ReflectionPermissionFlag.AllFlags | ReflectionPermissionFlag.RestrictedMemberAccess;
#else				
				const ReflectionPermissionFlag all_flags = ReflectionPermissionFlag.AllFlags;
#endif

				if ((value & all_flags) != value) {
					string msg = String.Format (Locale.GetText ("Invalid flags {0}"), value);
					throw new ArgumentException (msg, "ReflectionPermissionFlag");
				}

				flags = value;
			}
		}


		public override IPermission Copy ()
		{
			return new ReflectionPermission (flags);
		}

		public override void FromXml (SecurityElement esd)
		{
			// General validation in CodeAccessPermission
			CheckSecurityElement (esd, "esd", version, version);
			// Note: we do not (yet) care about the return value 
			// as we only accept version 1 (min/max values)

			if (IsUnrestricted (esd)) {
				flags = ReflectionPermissionFlag.AllFlags;
			}
			else {
				flags = ReflectionPermissionFlag.NoFlags;
				string xmlFlags = (esd.Attributes ["Flags"] as string);
				if (xmlFlags.IndexOf ("MemberAccess") >= 0)
					flags |= ReflectionPermissionFlag.MemberAccess;
				if (xmlFlags.IndexOf ("ReflectionEmit") >= 0)
					flags |= ReflectionPermissionFlag.ReflectionEmit;
				if (xmlFlags.IndexOf ("TypeInformation") >= 0)
					flags |= ReflectionPermissionFlag.TypeInformation;
			}
		}

		public override IPermission Intersect (IPermission target)
		{
			ReflectionPermission rp = Cast (target);
			if (rp == null)
				return null;

			if (IsUnrestricted ()) {
				if (rp.Flags == ReflectionPermissionFlag.NoFlags)
					return null;
				else
					return rp.Copy ();
			}
			if (rp.IsUnrestricted ()) {
				if (flags == ReflectionPermissionFlag.NoFlags)
					return null;
				else
					return Copy ();
			}

			ReflectionPermission p = (ReflectionPermission) rp.Copy ();
			p.Flags &= flags;
			return ((p.Flags == ReflectionPermissionFlag.NoFlags) ? null : p);
		}

		public override bool IsSubsetOf (IPermission target)
		{
			ReflectionPermission rp = Cast (target);
			if (rp == null)
				return (flags == ReflectionPermissionFlag.NoFlags);

			if (IsUnrestricted ())
				return rp.IsUnrestricted ();
			else if (rp.IsUnrestricted ())
				return true;

			return ((flags & rp.Flags) == flags);
		}

		public bool IsUnrestricted ()
		{
			return (flags == ReflectionPermissionFlag.AllFlags);
		}

		public override SecurityElement ToXml ()
		{
			SecurityElement se = Element (version);
			if (IsUnrestricted ()) {
				se.AddAttribute ("Unrestricted", "true");
			}
			else {
				if (flags == ReflectionPermissionFlag.NoFlags)
					se.AddAttribute ("Flags", "NoFlags");
				else if ((flags & ReflectionPermissionFlag.AllFlags) == ReflectionPermissionFlag.AllFlags)
					se.AddAttribute ("Flags", "AllFlags");
				else {
					string xmlFlags = "";
					if ((flags & ReflectionPermissionFlag.MemberAccess) == ReflectionPermissionFlag.MemberAccess)
						xmlFlags = "MemberAccess";
					if ((flags & ReflectionPermissionFlag.ReflectionEmit) == ReflectionPermissionFlag.ReflectionEmit) {
						if (xmlFlags.Length > 0)
							xmlFlags += ", ";
						xmlFlags += "ReflectionEmit";
					}
					if ((flags & ReflectionPermissionFlag.TypeInformation) == ReflectionPermissionFlag.TypeInformation) {
						if (xmlFlags.Length > 0)
							xmlFlags += ", ";
						xmlFlags += "TypeInformation";
					}
					se.AddAttribute ("Flags", xmlFlags);
				}
			}
			return se;
		}

		public override IPermission Union (IPermission other)
		{
			ReflectionPermission rp = Cast (other);
			if (other == null)
				return Copy ();

			if (IsUnrestricted () || rp.IsUnrestricted ())
				return new ReflectionPermission (PermissionState.Unrestricted);

			ReflectionPermission p = (ReflectionPermission) rp.Copy ();
			p.Flags |= flags;
			return p;
		}

		// IBuiltInPermission
		int IBuiltInPermission.GetTokenIndex ()
		{
			return (int) BuiltInToken.Reflection;
		}

		// helpers

		private ReflectionPermission Cast (IPermission target)
		{
			if (target == null)
				return null;

			ReflectionPermission rp = (target as ReflectionPermission);
			if (rp == null) {
				ThrowInvalidPermission (target, typeof (ReflectionPermission));
			}

			return rp;
		}
	}
}
