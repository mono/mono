//
// System.Security.Permissions.ReflectionPermission.cs
//
// Authors:
//	Tim Coleman <tim@timcoleman.com>
//	Sebastien Pouliot (spouliot@motus.com)
//
// Copyright (C) 2002, Tim Coleman
// Portions (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;

namespace System.Security.Permissions {

	[Serializable]
	public sealed class ReflectionPermission : CodeAccessPermission, IUnrestrictedPermission {

		#region Fields

		ReflectionPermissionFlag flags;
		PermissionState state;

		#endregion // Fields

		#region Constructors

		public ReflectionPermission (PermissionState state)
		{
			switch (state) {
				case PermissionState.None:
					flags = ReflectionPermissionFlag.NoFlags;
					break;
				case PermissionState.Unrestricted:
					flags = ReflectionPermissionFlag.AllFlags;
					break;
				default:
					throw new ArgumentException ("Invalid PermissionState");
			}
		}

		public ReflectionPermission (ReflectionPermissionFlag flag)
		{
			flags = flag;
		}

		#endregion // Constructors

		#region Properties

		public ReflectionPermissionFlag Flags {
			get { return flags; }
			set { flags = value; }
		}

		#endregion // Properties

		#region Methods

		public override IPermission Copy ()
		{
			return new ReflectionPermission (flags);
		}

		public override void FromXml (SecurityElement esd)
		{
			if (esd == null)
				throw new ArgumentNullException ("esd");
			if (esd.Tag != "IPermission")
				throw new ArgumentException ("not IPermission");
			if (!(esd.Attributes ["class"] as string).StartsWith ("System.Security.Permissions.ReflectionPermission"))
				throw new ArgumentException ("not ReflectionPermission");
			if ((esd.Attributes ["version"] as string) != "1")
				throw new ArgumentException ("wrong version");

			if ((esd.Attributes ["Unrestricted"] as string) == "true")
				flags = ReflectionPermissionFlag.AllFlags;
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
			if (target == null)
				return null;
			if (! (target is ReflectionPermission))
				throw new ArgumentException ("wrong type");

			ReflectionPermission o = (ReflectionPermission) target;
			int n = 0;
			if (IsUnrestricted ()) {
				if (o.Flags == ReflectionPermissionFlag.NoFlags)
					return null;
				else
					return o.Copy ();
			}
			if (o.IsUnrestricted ()) {
				if (flags == ReflectionPermissionFlag.NoFlags)
					return null;
				else
					return Copy ();
			}

			ReflectionPermission p = (ReflectionPermission) o.Copy ();
			p.Flags &= flags;
			return ((p.Flags == ReflectionPermissionFlag.NoFlags) ? null : p);
		}

		public override bool IsSubsetOf (IPermission target)
		{
			if (target == null)
				return (flags == ReflectionPermissionFlag.NoFlags);

			if (! (target is ReflectionPermission))
				throw new ArgumentException ("wrong type");

			ReflectionPermission o = (ReflectionPermission) target;
			if (IsUnrestricted ())
				return o.IsUnrestricted ();
			else if (o.IsUnrestricted ())
				return true;

			return ((flags & o.Flags) == flags);
		}

		public bool IsUnrestricted ()
		{
			return (flags == ReflectionPermissionFlag.AllFlags);
		}

		public override SecurityElement ToXml ()
		{
			SecurityElement se = Element (this, 1);
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
			if (other == null)
				return Copy ();
			if (! (other is ReflectionPermission))
				throw new ArgumentException ("wrong type");

			ReflectionPermission o = (ReflectionPermission) other;
			if (IsUnrestricted () || o.IsUnrestricted ())
				return new ReflectionPermission (PermissionState.Unrestricted);

			ReflectionPermission p = (ReflectionPermission) o.Copy ();
			p.Flags |= flags;
			return p;
		}

		#endregion // Methods
	}
}
