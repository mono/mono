//
// System.Security.Permissions.DataProtectionPermission class
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

namespace System.Security.Permissions {
	
	[Serializable]
	public sealed class DataProtectionPermission : CodeAccessPermission, IUnrestrictedPermission {

		private const int version = 1;

		private DataProtectionPermissionFlags _flags;


		public DataProtectionPermission (PermissionState state)
		{
			if (PermissionHelper.CheckPermissionState (state, true) == PermissionState.Unrestricted)
				_flags = DataProtectionPermissionFlags.AllFlags;
		}

		public DataProtectionPermission (DataProtectionPermissionFlags flag) 
		{
			// reuse validation by the Flags property
			Flags = flag;
		}


		public DataProtectionPermissionFlags Flags {
			get { return _flags; }
			set {
				if ((value & ~DataProtectionPermissionFlags.AllFlags) != 0) {
					string msg = String.Format (Locale.GetText ("Invalid enum {0}"), value);
					throw new ArgumentException (msg, "DataProtectionPermissionFlags");
				}
				_flags = value;
			}
		}

		public bool IsUnrestricted () 
		{
			return (_flags == DataProtectionPermissionFlags.AllFlags);
		}

		public override IPermission Copy () 
		{
			return new DataProtectionPermission (_flags);
		}

		public override IPermission Intersect (IPermission target) 
		{
			DataProtectionPermission dp = Cast (target);
			if (dp == null)
				return null;

			if (this.IsUnrestricted () && dp.IsUnrestricted ())
				return new DataProtectionPermission (PermissionState.Unrestricted);
			if (this.IsUnrestricted ())
				return dp.Copy ();
			if (dp.IsUnrestricted ())
				return this.Copy ();
			return new DataProtectionPermission (_flags & dp._flags);
		}

		public override IPermission Union (IPermission target) 
		{
			DataProtectionPermission dp = Cast (target);
			if (dp == null)
				return this.Copy ();

			if (this.IsUnrestricted () || dp.IsUnrestricted ())
				return new SecurityPermission (PermissionState.Unrestricted);
			
			return new DataProtectionPermission (_flags | dp._flags);
		}

		public override bool IsSubsetOf (IPermission target) 
		{
			DataProtectionPermission dp = Cast (target);
			if (dp == null) 
				return (_flags == DataProtectionPermissionFlags.NoFlags);

			if (dp.IsUnrestricted ())
				return true;
			if (this.IsUnrestricted ())
				return false;

			return ((_flags & ~dp._flags) == 0);
		}

		public override void FromXml (SecurityElement securityElement) 
		{
			// General validation in CodeAccessPermission
			PermissionHelper.CheckSecurityElement (securityElement, "securityElement", version, version);
			// Note: we do not (yet) care about the return value 
			// as we only accept version 1 (min/max values)

			_flags = (DataProtectionPermissionFlags) Enum.Parse (
				typeof (DataProtectionPermissionFlags), securityElement.Attribute ("Flags"));
		}

		public override SecurityElement ToXml () 
		{
			SecurityElement e = PermissionHelper.Element (typeof (DataProtectionPermission), version);
			e.AddAttribute ("Flags", _flags.ToString ());
			return e;
		}

		// helpers

		private DataProtectionPermission Cast (IPermission target)
		{
			if (target == null)
				return null;

			DataProtectionPermission dp = (target as DataProtectionPermission);
			if (dp == null) {
				PermissionHelper.ThrowInvalidPermission (target, typeof (DataProtectionPermission));
			}

			return dp;
		}
	}
}

