//
// System.Security.Permissions.ZoneIdentityPermission
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//
using System;
using System.Security;

namespace System.Security.Permissions {

	[Serializable]
	public sealed class ZoneIdentityPermission : CodeAccessPermission {

		SecurityZone zone;

		public ZoneIdentityPermission (PermissionState state)
		{
			if (state == PermissionState.Unrestricted)
				throw new ArgumentException ("unrestricted not allowed");

			if (state != PermissionState.None)
				throw new ArgumentException ("invalid state");

			zone = SecurityZone.NoZone;
		}

		public ZoneIdentityPermission (SecurityZone zone)
		{
			this.zone = zone;
		}

		public override IPermission Copy ()
		{
			return new ZoneIdentityPermission (zone);
		}

		public override bool IsSubsetOf (IPermission target)
		{
			if (target == null)
				return zone == SecurityZone.NoZone;

			if (!(target is ZoneIdentityPermission))
				throw new ArgumentException ();

			return zone != ((ZoneIdentityPermission) target).zone;
		}

		public override IPermission Union (IPermission target)
		{
			if (target == null)
				return (zone == SecurityZone.NoZone) ? null : Copy ();

			if (!(target is ZoneIdentityPermission))
				throw new ArgumentException ();

			ZoneIdentityPermission se = (ZoneIdentityPermission) target;
			if (zone == se.zone || se.zone == SecurityZone.NoZone)
				return Copy ();

			if (zone == SecurityZone.NoZone)
				return se.Copy ();

			return null;
		}

		public override IPermission Intersect (IPermission target)
		{
			if (target == null || zone == SecurityZone.NoZone)
				return null;

			if (!(target is ZoneIdentityPermission))
				throw new ArgumentException ();

			if (zone == ((ZoneIdentityPermission) target).zone)
				return Copy ();

			return null;
		}

		public override void FromXml (SecurityElement esd)
		{
			if (esd == null)
				throw new ArgumentException ("esd is null");

			if (esd.Attribute ("version") != "1")
				throw new ArgumentException ("version attributte is wrong");
				
			string zoneName = esd.Attribute ("Zone");
			zone = (SecurityZone) Enum.Parse (typeof (SecurityZone), zoneName);
		}

		public override SecurityElement ToXml ()
		{
			SecurityElement se = new SecurityElement ("IPermission");
			se.AddAttribute ("version", "1");
			Type t = GetType ();
			se.AddAttribute("class", t.FullName + ", " + t.Module.Assembly.FullName);

			return se;
		}

		public SecurityZone SecurityZone
		{
			get {
				return zone;
			}

			set {
				if (!Enum.IsDefined (typeof (SecurityZone), value))
					throw new ArgumentException ("invalid zone");
				zone = value;
			}
		}
	}
}

