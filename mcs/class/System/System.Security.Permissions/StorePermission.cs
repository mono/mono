//
// System.Security.Permissions.StorePermission class
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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
	public sealed class StorePermission : CodeAccessPermission, IUnrestrictedPermission {

		private const int version = 1;

		private StorePermissionFlags _flags;


		public StorePermission (PermissionState state)
		{
			if (PermissionHelper.CheckPermissionState (state, true) == PermissionState.Unrestricted)
				_flags = StorePermissionFlags.AllFlags;
			else
				_flags = StorePermissionFlags.NoFlags;
		}

		public StorePermission (StorePermissionFlags flag) 
		{
			// reuse validation by the Flags property
			Flags = flag;
		}


		public StorePermissionFlags Flags {
			get { return _flags; }
			set {
				if ((value != 0) && (value & StorePermissionFlags.AllFlags) == 0) {
					string msg = String.Format (Locale.GetText ("Invalid enum {0}"), value);
					throw new ArgumentException (msg, "StorePermissionFlags");
				}
				_flags = value;
			}
		}

		public bool IsUnrestricted () 
		{
			return (_flags == StorePermissionFlags.AllFlags);
		}

		public override IPermission Copy () 
		{
			// buggy behaviour - affects other operations that use Copy
			// reported as FDBK40928
			if (_flags == StorePermissionFlags.NoFlags)
				return null;

			return new StorePermission (_flags);
		}

		public override IPermission Intersect (IPermission target) 
		{
			StorePermission dp = Cast (target);
			if (dp == null)
				return null;

			if (this.IsUnrestricted () && dp.IsUnrestricted ())
				return new StorePermission (PermissionState.Unrestricted);
			if (this.IsUnrestricted ())
				return dp.Copy ();
			if (dp.IsUnrestricted ())
				return this.Copy ();

			StorePermissionFlags spf = _flags & dp._flags;
			if (spf == StorePermissionFlags.NoFlags)
				return null;

			return new StorePermission (spf);
		}

		public override IPermission Union (IPermission target) 
		{
			StorePermission dp = Cast (target);
			if (dp == null)
				return this.Copy (); // will return null for NoFlags

			if (this.IsUnrestricted () || dp.IsUnrestricted ())
				return new StorePermission (PermissionState.Unrestricted);
			
			StorePermissionFlags spf = _flags | dp._flags;
			if (spf == StorePermissionFlags.NoFlags)
				return null;

			return new StorePermission (spf);
		}

		public override bool IsSubsetOf (IPermission target) 
		{
			StorePermission dp = Cast (target);
			if (dp == null) 
				return (_flags == StorePermissionFlags.NoFlags);

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

			string s = securityElement.Attribute ("Flags");
			if (s == null)
				_flags = StorePermissionFlags.NoFlags;
			else
				_flags = (StorePermissionFlags) Enum.Parse (typeof (StorePermissionFlags), s);
		}

		public override SecurityElement ToXml () 
		{
			SecurityElement e = PermissionHelper.Element (typeof (StorePermission), version);
			if (this.IsUnrestricted ()) {
				e.AddAttribute ("Unrestricted", Boolean.TrueString);
			} else {
				e.AddAttribute ("Flags", _flags.ToString ());
			}
			return e;
		}

		// helpers

		private StorePermission Cast (IPermission target)
		{
			if (target == null)
				return null;

			StorePermission dp = (target as StorePermission);
			if (dp == null) {
				PermissionHelper.ThrowInvalidPermission (target, typeof (StorePermission));
			}

			return dp;
		}
	}
}

