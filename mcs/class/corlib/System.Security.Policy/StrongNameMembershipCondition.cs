//
// System.Security.Policy.StrongNameMembershipCondition.cs
//
// Author:
//      Duncan Mak (duncan@ximian.com)
//
// (C) 2003 Duncan Mak, Ximian Inc.
//


using System;
using System.Globalization;
using System.Security.Permissions;

namespace System.Security.Policy {

        public sealed class StrongNameMembershipCondition
                : IMembershipCondition, ISecurityEncodable, ISecurityPolicyEncodable
        {

                StrongNamePublicKeyBlob blob;
                string name;
                Version version;
                
                public StrongNameMembershipCondition (
                        StrongNamePublicKeyBlob blob, string name, Version version)
                {

                        if (blob == null)
                                throw new ArgumentNullException (
                                        Locale.GetText ("The argument is null."));

                        this.blob = blob;
                        this.name = name;
                        this.version = version;
                }

                public string Name {

                        get { return name; }

                        set { name = value; }
                }

                public Version Version {

                        get { return version; }

                        set { version = value; }
                }

                public StrongNamePublicKeyBlob PublicKey {

                        get { return blob; }

                        set {
                                if (value == null)
                                        throw new ArgumentNullException (
                                                Locale.GetText ("The argument is null."));

                                blob = value;
                        }
                }

                [MonoTODO ("How do you check for StrongName from an Evidence?")]
                public bool Check (Evidence evidence)
                {
                        return false;
                }

                public IMembershipCondition Copy ()
                {
                        return new StrongNameMembershipCondition (blob, name, version);
                }

                public override bool Equals (object o)
                {        
                        if (o is StrongName == false)
                                return false;

                        else {
                                StrongName sn = (StrongName) o;
                                return (sn.Name == Name && sn.Version == Version && sn.PublicKey == PublicKey);
                        }
                }

                public override int GetHashCode ()
                {
                        return blob.GetHashCode ();
                }

                public void FromXml (SecurityElement e)
                {
                        FromXml (e, null);
                }

                [MonoTODO ("Check for parameter validity")]
                public void FromXml (SecurityElement e, PolicyLevel level)
                {
                        if (e == null)
                                throw new ArgumentNullException (
                                        Locale.GetText ("The argument is null."));

                        System.Collections.Hashtable attrs = e.Attributes;

                        blob = StrongNamePublicKeyBlob.FromString (attrs ["PublicKeyBlob"] as String);
                        name = attrs ["Name"] as String;;
                        version = new Version (attrs ["AssemblyVersion"] as String);
                }

                public override string ToString ()
                {
                        return String.Format ( "Strong Name - {0} name = {1} version {2}",
                                        blob, name, version);
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

                        element.AddAttribute ("PublicKeyBlob", blob.ToString ());
                        element.AddAttribute ("Name", name);
                        element.AddAttribute ("AssemblyVersion", version.ToString ());

                        return element;
                }
        }
}
