//
// System.Security.Permissions.GacIdentityPermission.cs
//
// Author:
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

using System.Globalization;
using System.Runtime.InteropServices;

namespace System.Security.Permissions {

	[ComVisible (true)]
	[Serializable]
	public sealed class GacIdentityPermission : CodeAccessPermission, IBuiltInPermission {

		private const int version = 1;

		public GacIdentityPermission ()
		{
		}

		public GacIdentityPermission (PermissionState state)
		{
			// false == do not allow Unrestricted for Identity Permissions
			CheckPermissionState (state, false);
		}

		public override IPermission Copy ()
		{
			return (IPermission) new GacIdentityPermission ();
		}

		public override IPermission Intersect (IPermission target)
		{
			GacIdentityPermission gip = Cast (target);
			if (gip == null)
				return null;

			return Copy ();
		}

		public override bool IsSubsetOf (IPermission target)
		{
			GacIdentityPermission gip = Cast (target);
			return (gip != null);
		}

		public override IPermission Union (IPermission target)
		{
			Cast (target);
			return Copy ();
		}

		public override void FromXml (SecurityElement securityElement)
		{
			// General validation in CodeAccessPermission
			CheckSecurityElement (securityElement, "securityElement", version, version);
			// Note: we do not (yet) care about the return value 
			// as we only accept version 1 (min/max values)
		}

		public override SecurityElement ToXml ()
		{
			SecurityElement se = Element (version);
			return se;
		}

		// IBuildInPermission

		int IBuiltInPermission.GetTokenIndex ()
		{
			return (int) BuiltInToken.GacIdentity;
		}

		// helpers

		private GacIdentityPermission Cast (IPermission target)
		{
			if (target == null)
				return null;

			GacIdentityPermission uip = (target as GacIdentityPermission);
			if (uip == null) {
				ThrowInvalidPermission (target, typeof (GacIdentityPermission));
			}

			return uip;
		}
	}
}

