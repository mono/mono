//
// System.Security.Policy.Zone
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//
using System;
using System.Security;
using System.Security.Permissions;

namespace System.Security.Policy {

	[Serializable]
	public sealed class Zone : IIdentityPermissionFactory, IBuiltInEvidence	{
		SecurityZone zone;
		
		public Zone (SecurityZone zone)
		{
			if (!Enum.IsDefined (typeof (SecurityZone), zone))
				throw new ArgumentException ("invalid zone");

			this.zone = zone;
		}

		public object Copy ()
		{
			return new Zone (zone);
		}

		public IPermission CreateIdentityPermission (Evidence evidence)
		{
			return new ZoneIdentityPermission (zone);
		}

		[MonoTODO("This depends on zone configuration in IE")]
		public static Zone CreateFromUrl (string url)
		{
			throw new NotImplementedException ();
		}

		public override bool Equals (object o)
		{
			if (!(o is Zone))
				return false;

			return (((Zone) o).zone == zone);
		}

		public override int GetHashCode ()
		{
			return (int) zone;
		}

		public override string ToString ()
		{
			SecurityElement se = new SecurityElement (GetType ().FullName);
			se.AddAttribute ("version", "1");
			se.AddChild (new SecurityElement ("Zone", zone.ToString ()));

			return se.ToString ();
		}

		int IBuiltInEvidence.GetRequiredSize (bool verbose)
		{
			return 3;
		}

		int IBuiltInEvidence.InitFromBuffer (char [] buffer, int position) {
			int new_zone = (int) buffer [position++];
			new_zone += buffer [position++];
			return position;
		}

		int IBuiltInEvidence.OutputToBuffer (char [] buffer, int position, bool verbose)
		{
			buffer [position++] = '\x0003';
			buffer [position++] = (char) (((int) zone) >> 16);
			buffer [position++] = (char) (((int) zone) & 0x0FFFF);
			return position;
		}

		public SecurityZone SecurityZone
		{
			get { return zone; }
		}
	}
}

