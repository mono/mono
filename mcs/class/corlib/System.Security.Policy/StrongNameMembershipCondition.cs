//
// System.Security.Policy.StrongNameMembershipCondition.cs
//
// Author:
//      Duncan Mak (duncan@ximian.com)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2003 Duncan Mak, Ximian Inc.
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
using System.Security.Permissions;

namespace System.Security.Policy {

	[Serializable]
        public sealed class StrongNameMembershipCondition : IMembershipCondition, IConstantMembershipCondition {

		private readonly int version = 1;

		private StrongNamePublicKeyBlob blob;
		private string name;
		private Version assemblyVersion;
                
                public StrongNameMembershipCondition (StrongNamePublicKeyBlob blob, string name, Version version)
                {
                        if (blob == null)
                                throw new ArgumentNullException ("blob");

                        this.blob = blob;
                        this.name = name;
                        assemblyVersion = version;
                }

		// for PolicyLevel (to avoid validation duplication)
		internal StrongNameMembershipCondition (SecurityElement e)
		{
			FromXml (e);
		}

		// so System.Activator.CreateInstance can create an instance...
		internal StrongNameMembershipCondition ()
		{
		}

		// properties

                public string Name {
                        get { return name; }
                        set { name = value; }
                }

                public Version Version {
                        get { return assemblyVersion; }
                        set { assemblyVersion = value; }
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
					/* ??? partial match ??? */
					if (sn.PublicKey.Equals (blob) && (sn.Name == name) && (sn.Version.Equals (assemblyVersion)))
						return true;
				}
			}
			return false;
		}

		public IMembershipCondition Copy ()
		{
			return new StrongNameMembershipCondition (blob, name, assemblyVersion);
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
			MembershipConditionHelper.CheckSecurityElement (e, "e", version, version);

			blob = StrongNamePublicKeyBlob.FromString (e.Attribute ("PublicKeyBlob"));
			name = e.Attribute ("Name");
			string v = (string) e.Attribute ("AssemblyVersion");
			if (v == null)
				assemblyVersion = new Version ();
			else
				assemblyVersion = new Version (v);
		}

                public override string ToString ()
                {
			// ??? missing informations ???
                        return String.Format ( "Strong Name - {0} name = {1} version {2}",
                                        blob, name, assemblyVersion);
                }

                public SecurityElement ToXml ()
                {
                        return ToXml (null);
                }

                public SecurityElement ToXml (PolicyLevel level)
                {
			// PolicyLevel isn't used as there's no need to resolve NamedPermissionSet references
			SecurityElement se = MembershipConditionHelper.Element (typeof (StrongNameMembershipCondition), version);

                        se.AddAttribute ("PublicKeyBlob", blob.ToString ());
                        se.AddAttribute ("Name", name);
			string v = assemblyVersion.ToString ();
			if (v != "0.0")
				se.AddAttribute ("AssemblyVersion", assemblyVersion.ToString ());

			return se;
                }
        }
}
