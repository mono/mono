//
// System.Web.AspNetHostingPermission.cs
//
// Authors:
//	Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
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

using System.Security;
using System.Security.Permissions;

namespace System.Web {

	[Serializable]
	public sealed class AspNetHostingPermission : CodeAccessPermission, IUnrestrictedPermission {

		private const int version = 1;

		private AspNetHostingPermissionLevel _level;

		public AspNetHostingPermission (AspNetHostingPermissionLevel level)
		{
			// use the property to get the enum validation
			Level = level;
		}

		public AspNetHostingPermission (PermissionState state)
		{
			if (PermissionHelper.CheckPermissionState (state, true) == PermissionState.Unrestricted)
				_level = AspNetHostingPermissionLevel.Unrestricted;
			else
				_level = AspNetHostingPermissionLevel.None;
		}

		public AspNetHostingPermissionLevel Level {
			get { return _level; }
			set {
				if ((value < AspNetHostingPermissionLevel.None) || (value > AspNetHostingPermissionLevel.Unrestricted)) {
					string msg = Locale.GetText ("Invalid enum {0}.");
					throw new ArgumentException (String.Format (msg, value), "Level");
				}
				_level = value;
			}
		}

		public bool IsUnrestricted ()
		{
			return (_level == AspNetHostingPermissionLevel.Unrestricted);
		}

		public override IPermission Copy ()
		{
			// note: no need to handle unrestricted here
			return new AspNetHostingPermission (_level);
		}

		public override void FromXml (SecurityElement securityElement)
		{
			PermissionHelper.CheckSecurityElement (securityElement, "securityElement", version, version);
			if (securityElement.Tag != "IPermission") {
				string msg = Locale.GetText ("Invalid tag '{0}' for permission.");
				throw new ArgumentException (String.Format (msg, securityElement.Tag), "securityElement");
			}
			if (securityElement.Attribute ("version") == null) {
				string msg = Locale.GetText ("Missing version attribute.");
				throw new ArgumentException (msg, "securityElement");
			}

			if (PermissionHelper.IsUnrestricted (securityElement)) {
				// in case it's get fixed later...
				_level = AspNetHostingPermissionLevel.Unrestricted;
			}
			else {
				string level = securityElement.Attribute ("Level");
				if (level != null) {
					_level = (AspNetHostingPermissionLevel) Enum.Parse (
						typeof (AspNetHostingPermissionLevel), level);
				}
				else
					_level = AspNetHostingPermissionLevel.None;
			}
		}

		public override SecurityElement ToXml ()
		{
			SecurityElement se = PermissionHelper.Element (typeof (AspNetHostingPermission), version);
			if (IsUnrestricted ())
				se.AddAttribute ("Unrestricted", "true"); // FDBK15156 fixed in 2.0 RC
			se.AddAttribute ("Level", _level.ToString ());
			return se;
		}

		public override IPermission Intersect (IPermission target)
		{
			AspNetHostingPermission anhp = Cast (target);
			if (anhp == null)
				return null;

			return new AspNetHostingPermission ((_level <= anhp.Level) ? _level : anhp.Level);
		}

		public override bool IsSubsetOf (IPermission target)
		{
			AspNetHostingPermission anhp = Cast (target);
			if (anhp == null)
				return IsEmpty ();
			return (_level <= anhp._level);
		}

		public override IPermission Union (IPermission target)
		{
			AspNetHostingPermission anhp = Cast (target);
			if (anhp == null)
				return Copy ();
			return new AspNetHostingPermission ((_level > anhp.Level) ? _level : anhp.Level);
		}

		// Internal helpers methods

		private bool IsEmpty ()
		{
			return (_level == AspNetHostingPermissionLevel.None);
		}

		private AspNetHostingPermission Cast (IPermission target)
		{
			if (target == null)
				return null;

			AspNetHostingPermission anhp = (target as AspNetHostingPermission);
			if (anhp == null) {
				PermissionHelper.ThrowInvalidPermission (target, typeof (AspNetHostingPermission));
			}

			return anhp;
		}
	}
}
