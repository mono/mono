//
// System.Security.Permissions.SiteIdentityPermission.cs
//
// Author
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2003 Motus Technologies. http://www.motus.com
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

using System.Globalization;

namespace System.Security.Permissions {

	[Serializable]
	public sealed class SiteIdentityPermission : CodeAccessPermission, IBuiltInPermission {

		private const int version = 1;

		private string _site;

		// Constructors

		public SiteIdentityPermission (PermissionState state) 
		{
			// false == do not allow Unrestricted for Identity Permissions
			CheckPermissionState (state, false);
		}

		public SiteIdentityPermission (string site) 
		{
			Site = site;
		}

		// Properties

		public string Site {
			get { 
				if (IsEmpty ())
					throw new NullReferenceException ("Site");
				return _site; 
			}
			set {
				if (!IsValid (value))
					throw new ArgumentException ("Site");
				_site = value;
			}
		}

		// Methods

		public override IPermission Copy () 
		{
			if (IsEmpty ())
				return new SiteIdentityPermission (PermissionState.None);
			else
				return new SiteIdentityPermission (_site);
		}

		public override void FromXml (SecurityElement esd) 
		{
			// General validation in CodeAccessPermission
			CheckSecurityElement (esd, "esd", version, version);
			// Note: we do not (yet) care about the return value 
			// as we only accept version 1 (min/max values)

			string s = esd.Attribute ("Site");
			if (s != null)
				Site = s;
		}

		[MonoTODO ("do not support wildcard")]
		public override IPermission Intersect (IPermission target)
		{
			SiteIdentityPermission sip = Cast (target);
			if ((sip == null) || (IsEmpty ()))
				return null;
			if (_site == sip._site)
				return Copy ();
			return null;
		}

		[MonoTODO ("do not support wildcard")]
		public override bool IsSubsetOf (IPermission target) 
		{
			SiteIdentityPermission sip = Cast (target);
			if (sip == null)
				return IsEmpty ();
			return (_site == sip._site);
		}

		public override SecurityElement ToXml ()
		{
			SecurityElement e = Element (version);
			if (_site != null)
				e.AddAttribute ("Site", _site);
                        return e;
		}

		[MonoTODO ("do not support wildcard")]
		public override IPermission Union (IPermission target) 
		{
			SiteIdentityPermission sip = Cast (target);
			if ((sip == null) || (_site == sip._site) || sip.IsEmpty ())
				return Copy ();
			if (IsEmpty ())
				return sip.Copy ();

			throw new ArgumentException (Locale.GetText (
				"Cannot union two different sites."), "target");
		}

		// IBuiltInPermission
		int IBuiltInPermission.GetTokenIndex ()
		{
			return (int) BuiltInToken.SiteIdentity;
		}

		// helpers

		private bool IsEmpty ()
		{
			return (_site == null);
		}

		private SiteIdentityPermission Cast (IPermission target)
		{
			if (target == null)
				return null;

			SiteIdentityPermission sip = (target as SiteIdentityPermission);
			if (sip == null) {
				ThrowInvalidPermission (target, typeof (SiteIdentityPermission));
			}

			return sip;
		}

		private static bool[] valid = new bool [94] {
			/*  33 */ true,  false, true,  true,  true,  true,  true,  true,  true,  true,
			/*  43 */ false, false, true,  true,  false, true,  true,  true,  true,  true,
			/*  53 */ true,  true,  true,  true,  false, false, false, false, false, false,
			/*  63 */ false, true,  true,  true,  true,  true,  true,  true,  true,  true,
			/*  73 */ true,  true,  true,  true,  true,  true,  true,  true,  true,  true,
			/*  83 */ true,  true,  true,  true,  true,  true,  true,  true,  false, false,
			/*  93 */ false, true,  true,  false, true,  true,  true,  true,  true,  true,
			/* 103 */ true,  true,  true,  true,  true,  true,  true,  true,  true,  true,
			/* 113 */ true,  true,  true,  true,  true,  true,  true,  true,  true,  true,
			/* 123 */ true,  false, true,  true
		};

		private bool IsValid (string s)
		{
			if (s == null)
				return false;

			for (int i = 0; i < s.Length; i++) {
				ushort x = (ushort) s [i];
				if ((x < 33) || (x > 126)) {
					return false;
				}
				if (x == 42) {
					// special case for wildcards (*)
					// must be alone or followed by a dot
					if ((s.Length > 1) && (s [i + 1] != '.'))
						return false;
				}
				if (!valid [x - 33]) {
					return false;
				}
			}

			// a lone dot isn't valid
			if (s.Length == 1)
				return (s [0] != '.');
			return true;
		}

		private bool HasWildcard ()
		{
			if (_site == null)
				return false;
			return (_site.IndexOf ('*') >= 0);
		}
	}
}
