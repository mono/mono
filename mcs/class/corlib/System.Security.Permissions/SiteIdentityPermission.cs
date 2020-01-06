//
// System.Security.Permissions.SiteIdentityPermission.cs
//
// Author
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2003 Motus Technologies. http://www.motus.com
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
					throw new NullReferenceException ("No site.");
				return _site; 
			}
			set {
				if (!IsValid (value))
					throw new ArgumentException ("Invalid site.");
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

		public override IPermission Intersect (IPermission target)
		{
			SiteIdentityPermission sip = Cast (target);
			if ((sip == null) || (IsEmpty ()))
				return null;

			if (Match (sip._site)) {
				string s = ((_site.Length > sip._site.Length) ? _site : sip._site);
				return new SiteIdentityPermission (s);
			}
			return null;
		}

		public override bool IsSubsetOf (IPermission target) 
		{
			SiteIdentityPermission sip = Cast (target);
			if (sip == null)
				return IsEmpty ();
			if ((_site == null) && (sip._site == null))
				return true;
			if ((_site == null) || (sip._site == null))
				return false;

			int wildcard = sip._site.IndexOf ('*');
			if (wildcard == -1) {
				// exact match
				return (_site == sip._site);
			}
			return _site.EndsWith (sip._site.Substring (wildcard + 1));
		}

		public override SecurityElement ToXml ()
		{
			SecurityElement e = Element (version);
			if (_site != null)
				e.AddAttribute ("Site", _site);
                        return e;
		}

		public override IPermission Union (IPermission target) 
		{
			SiteIdentityPermission sip = Cast (target);
			if ((sip == null) || sip.IsEmpty ())
				return Copy ();
			if (IsEmpty ())
				return sip.Copy ();

			if (Match (sip._site)) {
				string s = ((_site.Length < sip._site.Length) ? _site : sip._site);
				return new SiteIdentityPermission (s);
			}
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
			if ((s == null) || (s.Length == 0))
				return false;

			for (int i = 0; i < s.Length; i++) {
				ushort x = (ushort) s [i];
				if ((x < 33) || (x > 126)) {
					return false;
				}
				if (x == 42) {
					// special case for wildcards (*)
					// must be alone or first and followed by a dot
					if ((s.Length > 1) && ((i > 0) || (s [i + 1] != '.')))
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

		private bool Match (string target) 
		{
			if ((_site == null) || (target == null))
				return false;

			int wcs = _site.IndexOf ('*');
			int wct = target.IndexOf ('*');

			if ((wcs == -1) && (wct == -1)) {
				// no wildcard, this is an exact match
				return (_site == target);
			}
			else if (wcs == -1) {
				// only "target" has a wildcard, use it
				return _site.EndsWith (target.Substring (wct + 1));
			}
			else if (wct == -1) {
				// only "this" has a wildcard, use it
				return target.EndsWith (_site.Substring (wcs + 1));
			}
			else {
				// both have wildcards, partial match with the smallest
				string s = _site.Substring (wcs + 1);
				target = target.Substring (wct + 1);
				if (s.Length > target.Length)
					return s.EndsWith (target);
				else
					return target.EndsWith (s);
			}
		}
	}
}
