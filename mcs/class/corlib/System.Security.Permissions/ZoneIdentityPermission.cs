//
// System.Security.Permissions.ZoneIdentityPermission
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
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
using System.Security;

namespace System.Security.Permissions {

	[Serializable]
	public sealed class ZoneIdentityPermission : CodeAccessPermission, IBuiltInPermission {

		private SecurityZone zone;

		public ZoneIdentityPermission (PermissionState state)
		{
			switch (state) {
				case PermissionState.None:
					zone = SecurityZone.NoZone;
					break;
				case PermissionState.Unrestricted:
					throw new ArgumentException (Locale.GetText (
						"unrestricted not allowed"));
				default:
					throw new ArgumentException (Locale.GetText (
						"invalid state"));
			}
		}

		public ZoneIdentityPermission (SecurityZone zone)
		{
			// also needs the validations
			SecurityZone = zone;
		}

		public override IPermission Copy ()
		{
			return new ZoneIdentityPermission (zone);
		}

		public override bool IsSubsetOf (IPermission target)
		{
			if (target == null)
				return zone == SecurityZone.NoZone;

			ZoneIdentityPermission zip = (target as ZoneIdentityPermission);
			if (zip == null) {
				throw new ArgumentException (Locale.GetText (
					"Invalid permission"));
			}

			return (zone == zip.zone);
		}

		public override IPermission Union (IPermission target)
		{
			if (target == null)
				return (zone == SecurityZone.NoZone) ? null : Copy ();

			ZoneIdentityPermission zip = (target as ZoneIdentityPermission);
			if (zip == null) {
				throw new ArgumentException (Locale.GetText (
					"Invalid permission"));
			}

			if (zone == zip.zone || zip.zone == SecurityZone.NoZone)
				return Copy ();

			if (zone == SecurityZone.NoZone)
				return zip.Copy ();
#if NET_2_0
			throw new ArgumentException (Locale.GetText (
				"Union impossible"));
#else
			return null;
#endif
		}

		public override IPermission Intersect (IPermission target)
		{
			if (target == null || zone == SecurityZone.NoZone)
				return null;

			ZoneIdentityPermission zip = (target as ZoneIdentityPermission);
			if (zip == null) {
				throw new ArgumentException (Locale.GetText (
					"Invalid permission"));
			}

			if (zone == zip.zone)
				return Copy ();

			return null;
		}

		public override void FromXml (SecurityElement esd)
		{
			if (esd == null)
				throw new ArgumentException ("esd");

			if (esd.Attribute ("version") != "1") {
				throw new ArgumentException (Locale.GetText (
					"version attributte is wrong"));
			}
				
			string zoneName = esd.Attribute ("Zone");
			zone = (SecurityZone) Enum.Parse (typeof (SecurityZone), zoneName);
		}

		public override SecurityElement ToXml ()
		{
			SecurityElement se = new SecurityElement ("IPermission");
			Type t = GetType ();
			se.AddAttribute ("class", t.FullName + ", " + t.Module.Assembly.FullName);
			se.AddAttribute ("version", "1");
			se.AddAttribute ("Zone", zone.ToString ());

			return se;
		}

		public SecurityZone SecurityZone {
			get { return zone; }
			set {
				if (!Enum.IsDefined (typeof (SecurityZone), value)) {
					throw new ArgumentException (Locale.GetText (
						"invalid zone"));
				}
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

