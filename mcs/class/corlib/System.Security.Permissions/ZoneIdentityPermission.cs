//
// System.Security.Permissions.ZoneIdentityPermission
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
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
using System.Security;

namespace System.Security.Permissions {

	[Serializable]
	public sealed class ZoneIdentityPermission : CodeAccessPermission, IBuiltInPermission {

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

		// IBuiltInPermission
		int IBuiltInPermission.GetTokenIndex ()
		{
			return 13;
		}
	}
}

