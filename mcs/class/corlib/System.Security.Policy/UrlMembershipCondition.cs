//
// System.Security.Policy.UrlMembershipCondition.cs
//
// Author:
//   Duncan Mak (duncan@ximian.com)
//
// (C) 2003, Ximian Inc.
//

using System;

namespace System.Security.Policy {

        public sealed class UrlMembershipCondition
                : IMembershipCondition, ISecurityEncodable, ISecurityPolicyEncodable
        {
                string url;
                
                public UrlMembershipCondition (string url)
                {
                        if (url == null)
                                throw new ArgumentNullException (
                                        Locale.GetText ("The argument is null."));

                        this.url = url;
                }

                public string Url {
                        get { return url; }
                }

                [MonoTODO]
                public virtual bool Check (Evidence evidence)
                {
                }

                public IMembershipCondition Copy ()
                {
                        return new UrlMembershipCondition (url);
                }

                public override bool Equals (Object o)
                {
                        return (o is UrlMembershipCondition && ((UrlMembershipCondition) o).Url = url);
                }

                public void FromXml (SecurityElement element)
                {
                        return FromXml (element, null);
                }

                public void FromXml (SecurityElement element, PolicyLevel level)
                {
                        url = element.Attributes ["Url"];
                }

                public override int GetHashCode ()
                {
                        return url.GetHashCode ();
                }

                public override string ToString ()
                {
                        Console.WriteLine ("Url - " + url);
                }

                public override SecurityElement ToXml ()
                {
                        return ToXml (null);
                }

                public override SecurityElement ToXml (PolicyLevel level)
                {
                        SecurityElement element = new SecurityElement ("IMembershipCondition");
                        element.AddAttribute ("version", "1");

                        element.AddAttribute ("Url", url);

                        return element;
                }
        }
}
