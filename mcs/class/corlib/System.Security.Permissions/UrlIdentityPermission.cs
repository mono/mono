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
			if (url == null)
				return new UrlIdentityPermission (PermissionState.None);
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

		[MonoTODO ("do not support wildcard")]
		public override IPermission Intersect (IPermission target) 
		{
			// if one permission is null (object or url) then there's no intersection
			// if both are null then intersection is null
			UrlIdentityPermission uip = Cast (target);
			if ((uip == null) || (IsEmpty ()))
				return null;
			if (url == uip.url)
				return Copy ();
			return null;
		}

		[MonoTODO ("do not support wildcard")]
		public override bool IsSubsetOf (IPermission target) 
		{
			UrlIdentityPermission uip = Cast (target);
			if (uip == null)
				return IsEmpty ();
			return (url == uip.url);
		}

		public override SecurityElement ToXml () 
		{
			SecurityElement se = Element (version);
			if (!IsEmpty ())
				se.AddAttribute ("Url", url);
			return se;
		}

		[MonoTODO ("do not support wildcard")]
		public override IPermission Union (IPermission target) 
		{
			UrlIdentityPermission uip = Cast (target);
			if (uip == null)
				return Copy ();
			if (IsEmpty () && uip.IsEmpty ())
				return null;
			if (uip.IsEmpty () || (url == uip.url))
				return Copy ();
			if (IsEmpty ())
				return uip.Copy ();
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
	}
}
