//
// System.Security.Permissions.HostProtectionPermission.cs
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

using System.Globalization;

namespace System.Security.Permissions {
	
	[Serializable]
	internal sealed class HostProtectionPermission : CodeAccessPermission, IUnrestrictedPermission, IBuiltInPermission {

		private const int version = 1;

		private HostProtectionResource _resources;

		// constructors

		public HostProtectionPermission (PermissionState state)
		{
			if (CheckPermissionState (state, true) == PermissionState.Unrestricted)
				_resources = HostProtectionResource.All;
			else
				_resources = HostProtectionResource.None;
		}

		public HostProtectionPermission (HostProtectionResource resources) 
		{
			// reuse validation by the Flags property
			Resources = _resources;
		}

		public HostProtectionResource Resources {
			get { return _resources; }
			set {
				if (!Enum.IsDefined (typeof (HostProtectionResource), value)) {
					string msg = String.Format (Locale.GetText ("Invalid enum {0}"), value);
					throw new ArgumentException (msg, "HostProtectionResource");
				}
				_resources = value;
			}
		}

		public override IPermission Copy () 
		{
			return new HostProtectionPermission (_resources);
		}

		public override IPermission Intersect (IPermission target) 
		{
			HostProtectionPermission hpp = Cast (target);
			if (hpp == null)
				return null;

			if (this.IsUnrestricted () && hpp.IsUnrestricted ())
				return new HostProtectionPermission (PermissionState.Unrestricted);
			if (this.IsUnrestricted ())
				return hpp.Copy ();
			if (hpp.IsUnrestricted ())
				return this.Copy ();
			return new HostProtectionPermission (_resources & hpp._resources);
		}

		public override IPermission Union (IPermission target) 
		{
			HostProtectionPermission hpp = Cast (target);
			if (hpp == null)
				return this.Copy ();

			if (this.IsUnrestricted () || hpp.IsUnrestricted ())
				return new HostProtectionPermission (PermissionState.Unrestricted);
			
			return new HostProtectionPermission (_resources | hpp._resources);
		}

		public override bool IsSubsetOf (IPermission target) 
		{
			HostProtectionPermission hpp = Cast (target);
			if (hpp == null) 
				return (_resources == HostProtectionResource.None);

			if (hpp.IsUnrestricted ())
				return true;
			if (this.IsUnrestricted ())
				return false;

			return ((_resources & ~hpp._resources) == 0);
		}

		public override void FromXml (SecurityElement e) 
		{
			// General validation in CodeAccessPermission
			CheckSecurityElement (e, "e", version, version);
			// Note: we do not (yet) care about the return value 
			// as we only accept version 1 (min/max values)

			_resources = (HostProtectionResource) Enum.Parse (
				typeof (HostProtectionResource), e.Attribute ("Resources"));
		}

		public override SecurityElement ToXml () 
		{
			SecurityElement e = Element (version);
			e.AddAttribute ("Resources", _resources.ToString ());
			return e;
		}

		// IUnrestrictedPermission
		public bool IsUnrestricted () 
		{
			return (_resources == HostProtectionResource.All);
		}

		// IBuiltInPermission
		int IBuiltInPermission.GetTokenIndex ()
		{
			return (int) BuiltInToken.HostProtection;
		}

		// helpers

		private HostProtectionPermission Cast (IPermission target)
		{
			if (target == null)
				return null;

			HostProtectionPermission hpp = (target as HostProtectionPermission);
			if (hpp == null) {
				ThrowInvalidPermission (target, typeof (HostProtectionPermission));
			}

			return hpp;
		}
	}
}

#endif
