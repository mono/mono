//
// System.Security.Permissions.ZoneIdentityPermission
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
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
	public sealed class ZoneIdentityPermission : CodeAccessPermission, IBuiltInPermission {

		private const int version = 1;

		private SecurityZone zone;

		public ZoneIdentityPermission (PermissionState state)
		{
			// false == do not allow Unrestricted for Identity Permissions
			CheckPermissionState (state, false);
			// default values
			zone = SecurityZone.NoZone;
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
			ZoneIdentityPermission zip = Cast (target);
			if (zip == null)
				return (zone == SecurityZone.NoZone);

			return ((zone == SecurityZone.NoZone) || (zone == zip.zone));
		}

		public override IPermission Union (IPermission target)
		{
			ZoneIdentityPermission zip = Cast (target);
			if (zip == null)
				return (zone == SecurityZone.NoZone) ? null : Copy ();

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
			ZoneIdentityPermission zip = Cast (target);
			if (zip == null || zone == SecurityZone.NoZone)
				return null;

			if (zone == zip.zone)
				return Copy ();

			return null;
		}

		public override void FromXml (SecurityElement esd)
		{
			// General validation in CodeAccessPermission
			CheckSecurityElement (esd, "esd", version, version);
			// Note: we do not (yet) care about the return value 
			// as we only accept version 1 (min/max values)

			string zoneName = esd.Attribute ("Zone");
			if (zoneName == null)
				zone = SecurityZone.NoZone;
			else
				zone = (SecurityZone) Enum.Parse (typeof (SecurityZone), zoneName);
		}

		public override SecurityElement ToXml ()
		{
			SecurityElement se = Element (version);
			if (zone != SecurityZone.NoZone)
				se.AddAttribute ("Zone", zone.ToString ());
			return se;
		}

		public SecurityZone SecurityZone {
			get { return zone; }
			set {
				if (!Enum.IsDefined (typeof (SecurityZone), value)) {
					string msg = String.Format (Locale.GetText ("Invalid enum {0}"), value);
					throw new ArgumentException (msg, "SecurityZone");
				}
				zone = value;
			}
		}

		// IBuiltInPermission
		int IBuiltInPermission.GetTokenIndex ()
		{
			return (int) BuiltInToken.ZoneIdentity;
		}

		// helpers

		private ZoneIdentityPermission Cast (IPermission target)
		{
			if (target == null)
				return null;

			ZoneIdentityPermission zip = (target as ZoneIdentityPermission);
			if (zip == null) {
				ThrowInvalidPermission (target, typeof (ZoneIdentityPermission));
			}

			return zip;
		}
	}
}

