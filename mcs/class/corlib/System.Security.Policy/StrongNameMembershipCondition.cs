//
// System.Security.Policy.StrongNameMembershipCondition.cs
//
// Author:
//      Duncan Mak (duncan@ximian.com)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2003 Duncan Mak, Ximian Inc.
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
using System.Security.Permissions;
using System.Runtime.InteropServices;
using System.Text;

namespace System.Security.Policy {

	[Serializable]
#if NET_2_0
	[ComVisible (true)]
#endif
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
			if (version != null)
	                        assemblyVersion = (Version) version.Clone ();
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
                                        throw new ArgumentNullException ("PublicKey");
				blob = value;
			}
		}

		public bool Check (Evidence evidence)
		{
			if (evidence == null)
				return false;

			IEnumerator e = evidence.GetHostEnumerator ();
			while (e.MoveNext ()) {
				StrongName sn = (e.Current as StrongName);
				if (sn != null) {
					if (!sn.PublicKey.Equals (blob))
						return false;
					if ((name != null) && (name != sn.Name))
						return false;
					if ((assemblyVersion != null) && !assemblyVersion.Equals (sn.Version))
						return false;
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
			StrongNameMembershipCondition snmc = (o as StrongNameMembershipCondition);
			if (snmc == null)
				return false;
			if (!snmc.PublicKey.Equals (PublicKey))
				return false;
			if (name != snmc.Name)
				return false;
			if (assemblyVersion != null)
			 	return assemblyVersion.Equals (snmc.Version);
			return (snmc.Version == null);
		}

		public override int GetHashCode ()
		{
			// name and version aren't part of the calculation
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
				assemblyVersion = null;
			else
				assemblyVersion = new Version (v);
		}

                public override string ToString ()
                {
			StringBuilder sb = new StringBuilder ("StrongName - ");
			sb.Append (blob);
			if (name != null)
				sb.AppendFormat (" name = {0}", name);
			if (assemblyVersion != null)
				sb.AppendFormat (" version = {0}", assemblyVersion);
			return sb.ToString ();
                }

                public SecurityElement ToXml ()
                {
                        return ToXml (null);
                }

                public SecurityElement ToXml (PolicyLevel level)
                {
			// PolicyLevel isn't used as there's no need to resolve NamedPermissionSet references
			SecurityElement se = MembershipConditionHelper.Element (typeof (StrongNameMembershipCondition), version);

			if (blob != null)
	                        se.AddAttribute ("PublicKeyBlob", blob.ToString ());
			if (name != null)
	                        se.AddAttribute ("Name", name);
			if (assemblyVersion != null) {
				string v = assemblyVersion.ToString ();
				if (v != "0.0")
					se.AddAttribute ("AssemblyVersion", v);
			}
			return se;
                }
        }
}
