//
// System.Security.Policy.ZoneMembershipCondition.cs
//
// Author:
//   Duncan Mak (duncan@ximian.com)
//
// (C) 2003, Ximian Inc.
//

using System;
using System.Globalization;

namespace System.Security.Policy {

        public sealed class ZoneMembershipCondition
                : IMembershipCondition, ISecurityEncodable, ISecurityPolicyEncodable
        {
                SecurityZone zone;
                
                public ZoneMembershipCondition (SecurityZone zone)
                {
                        this.zone = zone;
                }

                public SecurityZone SecurityZone {
                        set { zone = value; }
                        get { return zone; }
                }

                [MonoTODO]
                public bool Check (Evidence evidence)
                {
                        return false;
                }

                public IMembershipCondition Copy ()
                {
                        return new ZoneMembershipCondition (zone);
                }

                public override bool Equals (Object o)
                {
                        if (o is ZoneMembershipCondition == false)
                                return false;

                        else
                                return ((ZoneMembershipCondition) o).SecurityZone == zone;
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

                        if (element.Attribute ("class") != GetType ().AssemblyQualifiedName)
                                throw new ArgumentException (
                                        Locale.GetText ("The argument is invalid."));

                        if (element.Attribute ("version") != "1")
                                throw new ArgumentException (
                                        Locale.GetText ("The argument is invalid."));

                        zone = (SecurityZone) Enum.Parse (
                                typeof (SecurityZone), element.Attribute ("Zone"));
                }

                public override int GetHashCode ()
                {
                        return zone.GetHashCode ();
                }

                public override string ToString ()
                {
                        return "Zone - " + zone;
                }

                public SecurityElement ToXml ()
                {
                        return ToXml (null);
                }

                public SecurityElement ToXml (PolicyLevel level)
                {
                        SecurityElement element = new SecurityElement ("IMembershipCondition");
                        element.AddAttribute ("version", "1");

                        element.AddAttribute ("Zone", zone.ToString ());

                        return element;
                }
        }
}
