//
// System.Security.Policy.UrlMembershipCondition.cs
//
// Authors:
//	Duncan Mak (duncan@ximian.com)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2003, Ximian Inc.
// (C) 2004 Motus Technologies Inc. (http://www.motus.com)
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

using System.Collections;
using System.Globalization;
using System.Runtime.InteropServices;

using Mono.Security;

namespace System.Security.Policy {

	[Serializable]
#if NET_2_0
	[ComVisible (true)]
#endif
	public sealed class UrlMembershipCondition : IMembershipCondition, IConstantMembershipCondition {

		private readonly int version = 1;

		private Url url;
		private string userUrl;
                
                public UrlMembershipCondition (string url)
                {
			if (url == null)
				throw new ArgumentNullException ("url");
#if NET_2_0
			CheckUrl (url);
			userUrl = url;
                        this.url = new Url (url);
#else
                        this.url = new Url (url);
			userUrl = this.url.Value;
#endif
                }

		internal UrlMembershipCondition (Url url, string userUrl)
		{
			// as the Url object has already been validated there's no
			// need to restart the whole process by converting to string
			this.url = (Url) url.Copy ();
			this.userUrl = userUrl;
		}

		// properties

                public string Url {
                        get {
				if (userUrl == null)
					userUrl = url.Value;
				return userUrl;
			}
			set { url = new Url (value); }
                }

		// methods

                public bool Check (Evidence evidence)
                {
			if (evidence == null)
				return false;

			string u = url.Value;
			int wildcard = u.LastIndexOf ("*");	// partial match with a wildcard at the end
			if (wildcard == -1)
				wildcard = u.Length;		// exact match

			IEnumerator e = evidence.GetHostEnumerator ();
			while (e.MoveNext ()) {
				if (e.Current is Url) {
					// note: there shouldn't be more than one Url evidence
					if (String.Compare (u, 0, (e.Current as Url).Value, 0, wildcard,
						true, CultureInfo.InvariantCulture) == 0) {
						return true;
					}
					// but we must check for all of them!
				}
			}
                        return false;
                }

                public IMembershipCondition Copy ()
                {
                        return new UrlMembershipCondition (url, userUrl);
                }

		public override bool Equals (object o)
		{
			UrlMembershipCondition umc = (o as UrlMembershipCondition);
			if (o == null)
				return false;

			string u = url.Value;
			int length = u.Length; // exact match

			// partial match with a wildcard at the end
			if (u [length - 1] == '*') {
				length--;
				// in this case the last / could be ommited
				if (u [length - 1] == '/')
					length--;
			}

			return (String.Compare (u, 0, umc.Url, 0, length, true, CultureInfo.InvariantCulture) == 0);
		}

                public void FromXml (SecurityElement e)
                {
                        FromXml (e, null);
                }

		public void FromXml (SecurityElement e, PolicyLevel level)
		{
			MembershipConditionHelper.CheckSecurityElement (e, "e", version, version);
			
			string u = e.Attribute ("Url");
#if NET_2_0
			if (u != null) {
				CheckUrl (u);
				url = new Url (u);
			} else {
				url = null;
			}
#else
			url = (u == null) ? null : new Url (u);
#endif
			userUrl = u;
		}

                public override int GetHashCode ()
                {
                        return url.GetHashCode ();
                }

                public override string ToString ()
                {
                        return "Url - " + Url;
                }

                public SecurityElement ToXml ()
                {
                        return ToXml (null);
                }

                public SecurityElement ToXml (PolicyLevel level)
                {
			// PolicyLevel isn't used as there's no need to resolve NamedPermissionSet references
			SecurityElement se = MembershipConditionHelper.Element (typeof (UrlMembershipCondition), version);
                        se.AddAttribute ("Url", userUrl);
                        return se;
                }

		// internal stuff

#if NET_2_0
		internal void CheckUrl (string url)
		{
			// In .NET 1.x Url class checked the validity of the 
			// URL but that's no more the case in 2.x - but we 
			// still need the check done here
			int protocolPos = url.IndexOf (Uri.SchemeDelimiter);
			string u = (protocolPos < 0) ? "file://" + url : url;

			Uri uri = new Uri (u, false, false);
			// no * except for the "lone star" case
			if (uri.Host.IndexOf ('*') >= 1) {
				string msg = Locale.GetText ("Invalid * character in url");
				throw new ArgumentException (msg, "name");
			}
		}
#endif
        }
}
