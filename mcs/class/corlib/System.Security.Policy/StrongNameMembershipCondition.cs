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
                : IMembershipCondition, ISecurityEncodable, ISecurityPolicyEncodable, IConstantMembershipCondition
        {
                StrongNamePublicKeyBlob blob;
                string name;
                Version version;
                
                public StrongNameMembershipCondition (StrongNamePublicKeyBlob blob, string name, Version version)
                {
                        if (blob == null)
                                throw new ArgumentNullException ("blob");

                        this.blob = blob;
                        this.name = name;
                        this.version = version;
                }

		// for PolicyLevel (to avoid validation duplication)
		internal StrongNameMembershipCondition (SecurityElement e)
		{
			FromXml (e);
		}

		// properties

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

		public bool Check (Evidence evidence)
		{
			if (evidence == null)
				return false;

			foreach (object o in evidence) {
				if (o is StrongName) {
					StrongName sn = (o as StrongName);
					if (sn.PublicKey.Equals (blob) && (sn.Name == name) && (sn.Version.Equals (version)))
						return true;
				}
			}
			return false;
		}

		public IMembershipCondition Copy ()
		{
			return new StrongNameMembershipCondition (blob, name, version);
		}

		public override bool Equals (object o)
		{	 
			if (o is StrongNameMembershipCondition == false)
				return false;
			else {
				StrongNameMembershipCondition snmc = (StrongNameMembershipCondition) o;
				return (snmc.Name == Name && snmc.Version == Version && snmc.PublicKey == PublicKey);
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

		public void FromXml (SecurityElement e, PolicyLevel level)
		{
			if (e == null)
				throw new ArgumentNullException ("e");

			if (e.Attribute ("class").IndexOf (GetType ().Name) < 0)
				throw new ArgumentException (Locale.GetText ("Invalid class"));

			if (e.Attribute ("version") != "1")
				throw new ArgumentException (Locale.GetText ("Invalid version"));

			blob = StrongNamePublicKeyBlob.FromString (e.Attribute ("PublicKeyBlob"));
			name = e.Attribute ("Name");
			string v = (string) e.Attribute ("AssemblyVersion");
			if (v == null)
				version = new Version ();
			else
				version = new Version (v);
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
			string v = version.ToString ();
			if (v != "0.0")
				element.AddAttribute ("AssemblyVersion", version.ToString ());

			return element;
                }
        }
}
