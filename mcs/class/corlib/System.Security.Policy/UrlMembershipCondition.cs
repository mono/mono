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

using System.Globalization;

namespace System.Security.Policy {

	[Serializable]
        public sealed class UrlMembershipCondition : IMembershipCondition, IConstantMembershipCondition {

		private readonly int version = 1;

		private string url;
                
                public UrlMembershipCondition (string url)
                {
                        this.url = System.Security.Policy.Url.Prepare (url);
                }

                public string Url {
                        get { return url; }
			set { url = System.Security.Policy.Url.Prepare (value); }
                }

                public bool Check (Evidence evidence)
                {
			if (evidence == null)
				return false;

			foreach (object o in evidence) {
				Url u = (o as Url);
				if (u != null) {
					// note: there shouldn't be more than one Url evidence
					if (System.Security.Policy.Url.Compare (url, u.Value))
						return true;
				}
			}
                        return false;
                }

                public IMembershipCondition Copy ()
                {
                        return new UrlMembershipCondition (url);
                }

                public override bool Equals (Object o)
                {
			if (o is UrlMembershipCondition) {
				return System.Security.Policy.Url.Compare (url, ((UrlMembershipCondition) o).Url);
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
			
			url = element.Attribute ("Url");
		}

                public override int GetHashCode ()
                {
                        return url.GetHashCode ();
                }

                public override string ToString ()
                {
                        return "Url - " + url;
                }

                public SecurityElement ToXml ()
                {
                        return ToXml (null);
                }

                public SecurityElement ToXml (PolicyLevel level)
                {
			// PolicyLevel isn't used as there's no need to resolve NamedPermissionSet references
			SecurityElement se = MembershipConditionHelper.Element (typeof (UrlMembershipCondition), version);
                        se.AddAttribute ("Url", url);
                        return se;
                }
        }
}
