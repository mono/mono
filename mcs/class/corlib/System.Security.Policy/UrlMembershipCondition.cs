//
// System.Security.Policy.UrlMembershipCondition.cs
//
// Authors:
//	Duncan Mak (duncan@ximian.com)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2003, Ximian Inc.
// (C) 2004 Motus Technologies Inc. (http://www.motus.com)
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

using System.Collections;
using System.Globalization;

namespace System.Security.Policy {

	[Serializable]
        public sealed class UrlMembershipCondition : IMembershipCondition, IConstantMembershipCondition {

		private readonly int version = 1;

		private Url url;
                
                public UrlMembershipCondition (string url)
                {
                        this.url = new Url (url);
                }

		internal UrlMembershipCondition (Url url)
		{
			// as the Url object has already been validated there's no
			// need to restart the whole process by converting to string
			this.url = (Url) url.Copy ();
		}

		// properties

                public string Url {
                        get { return url.Value; }
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
                        return new UrlMembershipCondition (url);
                }

		public override bool Equals (object o)
		{
			if (o is UrlMembershipCondition) {
				string u = url.Value;
				int wildcard = u.LastIndexOf ("*");	// partial match with a wildcard at the end
				if (wildcard == -1)
					wildcard = u.Length;		// exact match

				return (String.Compare (u, 0, (o as UrlMembershipCondition).Url,
					0, wildcard, true, CultureInfo.InvariantCulture) == 0);
			}
			return false;
		}

                public void FromXml (SecurityElement element)
                {
                        FromXml (element, null);
                }

		public void FromXml (SecurityElement element, PolicyLevel level)
		{
			MembershipConditionHelper.CheckSecurityElement (element, "element", version, version);
			
			string u = element.Attribute ("Url");
			url = (u == null) ? null : new Url (u);
		}

                public override int GetHashCode ()
                {
                        return url.GetHashCode ();
                }

                public override string ToString ()
                {
                        return "Url - " + url.Value;
                }

                public SecurityElement ToXml ()
                {
                        return ToXml (null);
                }

                public SecurityElement ToXml (PolicyLevel level)
                {
			// PolicyLevel isn't used as there's no need to resolve NamedPermissionSet references
			SecurityElement se = MembershipConditionHelper.Element (typeof (UrlMembershipCondition), version);
                        se.AddAttribute ("Url", url.Value);
                        return se;
                }
        }
}
