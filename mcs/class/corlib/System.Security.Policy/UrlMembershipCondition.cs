//
// System.Security.Policy.UrlMembershipCondition.cs
//
// Author:
//   Duncan Mak (duncan@ximian.com)
//
// (C) 2003, Ximian Inc.
//

using System;
using System.Globalization;

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
                public bool Check (Evidence evidence)
                {
                        return false;
                }

                public IMembershipCondition Copy ()
                {
                        return new UrlMembershipCondition (url);
                }

                public override bool Equals (Object o)
                {
                        if (o is UrlMembershipCondition == false)
                                return false;

                        else
                                return ((UrlMembershipCondition) o).Url == url;
                }

                public void FromXml (SecurityElement element)
                {
                        FromXml (element, null);
                }

                public void FromXml (SecurityElement element, PolicyLevel level)
                {
			if (element == null)
				throw new ArgumentNullException (
                                        Locale.GetText ("The argument is null."));
                        
                        string value = element.Attribute ("Url") as String;

                        if (value == null)
                                return;

                        else url = value;
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
                        element.AddAttribute ("version", "1");

                        element.AddAttribute ("Url", url);

                        return element;
                }
        }
}
