//
// System.Security.Permissions.GacIdentityPermission
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
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

#if NET_2_0

using System;
using System.Globalization;
using System.Security;

namespace System.Security.Permissions {

	[Serializable]
	public sealed class GacIdentityPermission : CodeAccessPermission, IBuiltInPermission {

		public GacIdentityPermission ()
		{
		}

		public GacIdentityPermission (PermissionState state)
		{
			switch (state) {
				case PermissionState.None:
					break;
				case PermissionState.Unrestricted:
					throw new ArgumentException (Locale.GetText (
						"unrestricted not allowed"));
				default:
					throw new ArgumentException (Locale.GetText (
						"invalid state"));
			}
		}

		public override IPermission Copy ()
		{
			return (IPermission) new GacIdentityPermission ();
		}

		public override IPermission Intersect (IPermission target)
		{
			if (target == null)
				return null;
			if (!(target is GacIdentityPermission)) {
				throw new ArgumentException (Locale.GetText (
					"Invalid permission"));
			}
			return Copy ();
		}

		public override bool IsSubsetOf (IPermission target)
		{
			if (target == null)
				return false;

			if (!(target is GacIdentityPermission)) {
				throw new ArgumentException (Locale.GetText (
					"Invalid permission"));
			}

			return true;
		}

		public override IPermission Union (IPermission target)
		{
			if (target == null)
				return Copy ();

			if (!(target is GacIdentityPermission)) {
				throw new ArgumentException (Locale.GetText (
					"Invalid permission"));
			}

			return Copy ();
		}

		public override void FromXml (SecurityElement esd)
		{
			if (esd == null)
				throw new ArgumentException ("esd");

			if (esd.Attribute ("version") != "1") {
				throw new ArgumentException (Locale.GetText (
					"version attributte is wrong"));
			}

			// ??? check class name ???
		}

		public override SecurityElement ToXml ()
		{
			SecurityElement se = new SecurityElement ("IPermission");
			Type t = GetType ();
			se.AddAttribute ("class", t.FullName + ", " + t.Module.Assembly.FullName);
			se.AddAttribute ("version", "1");
			return se;
		}

		// IBuildInPermission

		int IBuiltInPermission.GetTokenIndex ()
		{
			return -1; // TODO
		}
	}
}

#endif
