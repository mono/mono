//
// System.Security.Permissions.UrlIdentityPermission.cs
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
	public sealed class UrlIdentityPermission : CodeAccessPermission, IBuiltInPermission {

		private const int version = 1;

		private string url;

		public UrlIdentityPermission (PermissionState state)
		{
			// false == do not allow Unrestricted for Identity Permissions
			CheckPermissionState (state, false);
#if NET_2_0
			url = String.Empty;
#endif
		}

		public UrlIdentityPermission (string site)
		{
			if (site == null)
				throw new ArgumentNullException ("site");
			url = site;
		}

		public string Url { 
			get { 
#if !NET_2_0
				if (url == null)
					throw new NullReferenceException ("Url");
#endif
				return url; 
			}
			set {
				if (value == null)
					throw new ArgumentNullException ("Url");
				url = value;
			}
		}

		public override IPermission Copy () 
		{
			if (url == null) {
#if NET_2_0
				return new UrlIdentityPermission (PermissionState.None);
#else
				throw new NullReferenceException ("Url");
#endif
			}
			else
				return new UrlIdentityPermission (url);
		}

		public override void FromXml (SecurityElement esd)
		{
			// General validation in CodeAccessPermission
			CheckSecurityElement (esd, "esd", 1, 1);
			// Note: we do not (yet) care about the return value 
			// as we only accept version 1 (min/max values)

			string u = esd.Attribute ("Url");
			if (u == null)
				url = String.Empty;
			else
				Url = u;
		}

		public override IPermission Intersect (IPermission target) 
		{
			// if one permission is null (object or url) then there's no intersection
			// if both are null then intersection is null
			UrlIdentityPermission uip = Cast (target);
			if ((uip == null) || (IsEmpty ()))
				return null;
			if (Match (uip.url)) {
				// longest form is the intersection
				if (url.Length > uip.url.Length)
					return Copy ();
				else
					return uip.Copy ();
			}
			return null;
		}

		public override bool IsSubsetOf (IPermission target) 
		{
			UrlIdentityPermission uip = Cast (target);
			if (uip == null)
				return IsEmpty ();
			if (IsEmpty ())
				return true;
			if (uip.url == null)
				return false;

			// here Match wouldn't work as it is bidirectional
			int wildcard = uip.url.LastIndexOf ('*');
			if (wildcard == -1)
				wildcard = uip.url.Length;	// exact match

			return (String.Compare (url, 0, uip.url, 0, wildcard, true, CultureInfo.InvariantCulture) == 0);
		}

		public override SecurityElement ToXml () 
		{
			SecurityElement se = Element (version);
			if (!IsEmpty ())
				se.AddAttribute ("Url", url);
			return se;
		}

		public override IPermission Union (IPermission target) 
		{
			UrlIdentityPermission uip = Cast (target);
			if (uip == null)
				return Copy ();
			if (IsEmpty () && uip.IsEmpty ())
				return null;
			if (uip.IsEmpty ())
				return Copy ();
			if (IsEmpty ())
				return uip.Copy ();
			if (Match (uip.url)) {
				// shortest form is the union
				if (url.Length < uip.url.Length)
					return Copy ();
				else
					return uip.Copy ();
			}
#if NET_2_0
			throw new ArgumentException (Locale.GetText (
				"Cannot union two different urls."), "target");
#else
			return null;
#endif
		}

		// IBuiltInPermission
		int IBuiltInPermission.GetTokenIndex ()
		{
			return (int) BuiltInToken.UrlIdentity;
		}

		// helpers

		private bool IsEmpty ()
		{
			return ((url == null) || (url.Length == 0));
		}

		private UrlIdentityPermission Cast (IPermission target)
		{
			if (target == null)
				return null;

			UrlIdentityPermission uip = (target as UrlIdentityPermission);
			if (uip == null) {
				ThrowInvalidPermission (target, typeof (UrlIdentityPermission));
			}

			return uip;
		}

		private bool Match (string target) 
		{
			if ((url == null) || (target == null))
				return false;

			int wcu = url.LastIndexOf ('*');
			int wct = target.LastIndexOf ('*');
			int length = Int32.MaxValue;

			if ((wcu == -1) && (wct == -1)) {
				// no wildcard, this is an exact match
				length = Math.Max (url.Length, target.Length);
			}
			else if (wcu == -1) {
				// only "this" has a wildcard, use it
				length = wct;
			}
			else if (wct == -1) {
				// only "target" has a wildcard, use it
				length = wcu;
			}
			else {
				// both have wildcards, partial match with the smallest
				length = Math.Min (wcu, wct);
			}

			return (String.Compare (url, 0, target, 0, length, true, CultureInfo.InvariantCulture) == 0);
		}
	}
}
