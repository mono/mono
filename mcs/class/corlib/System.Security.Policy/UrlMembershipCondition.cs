//
// System.Security.Policy.UrlMembershipCondition.cs
//
// Authors:
//	Duncan Mak (duncan@ximian.com)
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003, Ximian Inc.
// (C) 2004 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.Globalization;

namespace System.Security.Policy {

	[Serializable]
        public sealed class UrlMembershipCondition
                : IMembershipCondition, ISecurityEncodable, ISecurityPolicyEncodable, IConstantMembershipCondition
        {
                string url;
                
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
			if (element == null)
				throw new ArgumentNullException ("element");
			
			if (element.Tag != "IMembershipCondition")
				throw new ArgumentException (
					Locale.GetText ("Invalid tag - expected IMembershipCondition"));

			if (element.Attribute ("class") != GetType ().AssemblyQualifiedName)
				throw new ArgumentException (
					Locale.GetText ("Invalid class attribute"));

			if (element.Attribute ("version") != "1")
				throw new ArgumentException (
					Locale.GetText ("Invalid version"));
			
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
                        SecurityElement element = new SecurityElement ("IMembershipCondition");
			element.AddAttribute ("class", this.GetType ().AssemblyQualifiedName);
			element.AddAttribute ("version", "1");
                        element.AddAttribute ("Url", url);
                        return element;
                }
        }
}
