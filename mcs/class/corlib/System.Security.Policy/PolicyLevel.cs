//
// System.Security.Policy.PolicyLevel.cs
//
// Authors:
//      Nick Drochak (ndrochak@gol.com)
//      Duncan Mak (duncan@ximian.com)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2001 Nick Drochak
// (C) 2003 Duncan Mak, Ximian Inc.
// Portions (C) 2004 Motus Technologies Inc. (http://www.motus.com)
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

using System.Collections; // for IList
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Security;
using System.Security.Permissions;

using Mono.Xml;

namespace System.Security.Policy {

	[Serializable]
	public sealed class PolicyLevel {

                string label;
                CodeGroup root_code_group;
		private ArrayList full_trust_assemblies;
		private ArrayList named_permission_sets;
		private string _location;
		private PolicyLevelType _type;

		internal PolicyLevel (string label, PolicyLevelType type)
                {
                        this.label = label;
			_type = type;
                        full_trust_assemblies = new ArrayList ();
                        named_permission_sets = new ArrayList ();
                }

		internal PolicyLevel (string label, PolicyLevelType type, string filename)
			: this (label, type)
                {
			LoadFromFile (filename);
		}

		internal void LoadFromFile (string filename) 
		{
			bool loaded = false;
			try {
				// check for policy file
				if (!File.Exists (filename)) {
					// if it doesn't exist use the default configuration (like Fx 2.0)
					// ref: http://blogs.msdn.com/shawnfa/archive/2004/04/21/117833.aspx
					string defcfg = filename + ".default";
					if (File.Exists (defcfg)) {
						// create policy from default file
						File.Copy (defcfg, filename);
					}
				}
				// load security policy configuration
				if (File.Exists (filename)) {
					using (StreamReader sr = File.OpenText (filename)) {
						LoadFromString (sr.ReadToEnd ());
					}
					loaded = true;
				}
				else {
					CreateFromHardcodedDefault (_type);
					loaded = true;
					Save ();
				}
			}
			catch (Exception) {
				// this can fail in many ways include
				// * can't lookup policy (path discovery);
				// * can't copy default file to policy
				// * can't read policy file;
				// * can't save hardcoded policy to filename
				// * can't decode policy file
				if (!loaded)
					CreateFromHardcodedDefault (_type);
			}
			finally {
				_location = filename;
			}
		}

		internal void LoadFromString (string xml) 
		{
			SecurityParser parser = new SecurityParser ();
			parser.LoadXml (xml);
			// configuration / mscorlib / security / policy / PolicyLevel
			SecurityElement configuration = parser.ToXml ();
			if (configuration.Tag != "configuration")
				throw new ArgumentException (Locale.GetText ("missing <configuration> root element"));
			SecurityElement mscorlib = (SecurityElement) configuration.Children [0];
			if (mscorlib.Tag != "mscorlib")
				throw new ArgumentException (Locale.GetText ("missing <mscorlib> tag"));
			SecurityElement security = (SecurityElement) mscorlib.Children [0];
			if (security.Tag != "security")
				throw new ArgumentException (Locale.GetText ("missing <security> tag"));
			SecurityElement policy = (SecurityElement) security.Children [0];
			if (policy.Tag != "policy")
				throw new ArgumentException (Locale.GetText ("missing <policy> tag"));
			SecurityElement policyLevel = (SecurityElement) policy.Children [0];
			FromXml (policyLevel);
		}

		// properties

		public IList FullTrustAssemblies
		{
			get { return full_trust_assemblies; }
		}

		public string Label {
			get { return label; }
		}

		public IList NamedPermissionSets {
			get { return named_permission_sets; }
		}

		public CodeGroup RootCodeGroup {
                        get { return root_code_group; }
			set { 
				if (value == null)
					throw new ArgumentNullException ("value");
				root_code_group = value; 
			}
		}

		public string StoreLocation {
			get { return _location; }
		}

#if NET_2_0
		public PolicyLevelType Type {
			get { return _type; }
		}
#endif

		// methods

                public void AddFullTrustAssembly (StrongName sn)
                {
			if (sn == null)
				throw new ArgumentNullException ("sn");

			StrongNameMembershipCondition snMC = new StrongNameMembershipCondition(
                                sn.PublicKey, sn.Name, sn.Version);

                        AddFullTrustAssembly (snMC);
                }

                public void AddFullTrustAssembly (StrongNameMembershipCondition snMC)
                {
                        if (snMC == null)
                                throw new ArgumentNullException ("snMC");
                        
			foreach (StrongNameMembershipCondition sn in full_trust_assemblies) {
				if (sn.Equals (snMC)) {
					throw new ArgumentException (Locale.GetText ("sn already has full trust."));
				}
			}
                        full_trust_assemblies.Add (snMC);
                }

                public void AddNamedPermissionSet (NamedPermissionSet permSet)
                {
                        if (permSet == null)
                                throw new ArgumentNullException ("permSet");

			foreach (NamedPermissionSet n in named_permission_sets) {
				if (permSet.Name == n.Name) {
					throw new ArgumentException (
						Locale.GetText ("This NamedPermissionSet is the same an existing NamedPermissionSet."));
				}
			}
                        named_permission_sets.Add (permSet);
                }

		public NamedPermissionSet ChangeNamedPermissionSet (string name, PermissionSet pSet)
		{
			if (name == null)
				throw new ArgumentNullException ("name");
			if (pSet == null)
				throw new ArgumentNullException ("pSet");
			if (IsReserved (name))
				throw new ArgumentException (Locale.GetText ("Reserved name"));

			foreach (NamedPermissionSet n in named_permission_sets) {
				if (name == n.Name) {
					named_permission_sets.Remove (n);
					AddNamedPermissionSet (new NamedPermissionSet (name, pSet));
					return n;
				}
			}
			throw new ArgumentException (Locale.GetText ("PermissionSet not found"));
		}

                public static PolicyLevel CreateAppDomainLevel ()
                {
			NamedPermissionSet fullTrust = new NamedPermissionSet ("FullTrust", PermissionState.Unrestricted);
			UnionCodeGroup cg = new UnionCodeGroup (new AllMembershipCondition (), new PolicyStatement (fullTrust));
			cg.Name = "All_Code";
			PolicyLevel pl = new PolicyLevel ("AppDomain", PolicyLevelType.AppDomain);
			pl.RootCodeGroup = cg;
                        return pl;
                }

                public void FromXml (SecurityElement e)
                {
                        if (e == null)
                                throw new ArgumentNullException ("e");
// MS doesn't throw an exception for this case
//			if (e.Tag != "PolicyLevel")
//				throw new ArgumentException (Locale.GetText ("Invalid XML"));

			Hashtable fullNames = null;
			SecurityElement sc = e.SearchForChildByTag ("SecurityClasses");
			if ((sc != null) && (sc.Children != null) && (sc.Children.Count > 0)) {
				fullNames = new Hashtable (sc.Children.Count);
				foreach (SecurityElement se in sc.Children) {
					fullNames.Add (se.Attributes ["Name"], se.Attributes ["Description"]);
				}
			}

			SecurityElement nps = e.SearchForChildByTag ("NamedPermissionSets");
			if ((nps != null) && (nps.Children != null) && (nps.Children.Count > 0)) {
				named_permission_sets.Clear ();
				foreach (SecurityElement se in nps.Children) {
					NamedPermissionSet n = new NamedPermissionSet ();
					n.FromXml (se);
					named_permission_sets.Add (n);
				}
			}

			SecurityElement cg = e.SearchForChildByTag ("CodeGroup");
			if ((cg != null) && (cg.Children != null) && (cg.Children.Count > 0)) {
				root_code_group = CodeGroup.CreateFromXml (cg, this);
			}
			else
				throw new ArgumentException (Locale.GetText ("Missing Root CodeGroup"));

			SecurityElement fta = e.SearchForChildByTag ("FullTrustAssemblies");
			if ((fta != null) && (fta.Children != null) && (fta.Children.Count > 0)) {
				full_trust_assemblies.Clear ();
				foreach (SecurityElement se in fta.Children) {
					if (se.Tag != "IMembershipCondition")
						throw new ArgumentException (Locale.GetText ("Invalid XML"));
					string className = (string) se.Attributes ["class"];
					if (className.IndexOf ("StrongNameMembershipCondition") < 0)
						throw new ArgumentException (Locale.GetText ("Invalid XML - must be StrongNameMembershipCondition"));
					// we directly use StrongNameMembershipCondition
					full_trust_assemblies.Add (new StrongNameMembershipCondition (se));
				}
			}
		}

                public NamedPermissionSet GetNamedPermissionSet (string name)
                {
                        if (name == null)
                                throw new ArgumentNullException ("name");

                        foreach (NamedPermissionSet n in named_permission_sets)
                                if (n.Name == name)
                                        return n;

                        return null;
                }

                [MonoTODO]
                public void Recover ()
                {
                        throw new NotImplementedException ();
                }

                public void RemoveFullTrustAssembly (StrongName sn)
                {
			if (sn == null)
				throw new ArgumentNullException ("sn");

			StrongNameMembershipCondition s = new StrongNameMembershipCondition (sn.PublicKey, sn.Name, sn.Version);
                        RemoveFullTrustAssembly (s);
                }

                public void RemoveFullTrustAssembly (StrongNameMembershipCondition snMC)
                {
                        if (snMC == null)
                                throw new ArgumentNullException ("snMC");

                        if (((IList) full_trust_assemblies).Contains (snMC))
                                ((IList) full_trust_assemblies).Remove (snMC);

                        else
                                throw new ArgumentException (
                                        Locale.GetText ("sn does not have full trust."));
                }

                public NamedPermissionSet RemoveNamedPermissionSet (NamedPermissionSet permSet)
                {
                        if (permSet == null)
                                throw new ArgumentNullException (
                                        Locale.GetText ("The Argument is null."));

                        if (! ((IList )named_permission_sets).Contains (permSet))
                                throw new ArgumentException (
                                        Locale.GetText ("permSet cannot be found."));

                        ((IList) named_permission_sets).Remove (permSet);

                        return permSet;
                }

                [MonoTODO ("Check for reserved names")]
                public NamedPermissionSet RemoveNamedPermissionSet (string name)
                {
                        if (name == null)
                                throw new ArgumentNullException ("name");

                        int idx = -1;
                        for (int i = 0; i < named_permission_sets.Count; i++) {
                                NamedPermissionSet current = (NamedPermissionSet) named_permission_sets [i];

                                if (current.Name == name)
                                        idx = i;
                                i ++;
                        }                       

                        if (idx == -1)
                                throw new ArgumentException (
                                        Locale.GetText ("Name cannot be found."));

                        NamedPermissionSet retval = (NamedPermissionSet) named_permission_sets [idx];
                        named_permission_sets.RemoveAt (idx);

                        return retval;
                }

                public void Reset ()
                {
                        full_trust_assemblies.Clear ();
                        named_permission_sets.Clear ();

			if ((_location != null) && (File.Exists (_location))) {
				try {
					File.Delete (_location);
				}
				catch {}
			}
			// because the policy doesn't exist LoadFromFile will try to
			// 1. use the .default file if existing (like Fx 2.0 does); or
			// 2. use the hard-coded default values
			// and recreate a policy file
			LoadFromFile (_location);
                }

                public PolicyStatement Resolve (Evidence evidence)
                {
                        if (evidence == null)
                                throw new ArgumentNullException ("evidence");

			PolicyStatement ps = root_code_group.Resolve (evidence);
			return ((ps != null) ? ps : PolicyStatement.Empty ());
                }

                public CodeGroup ResolveMatchingCodeGroups (Evidence evidence)
                {
                        if (evidence == null)
				throw new ArgumentNullException ("evidence");

			CodeGroup cg = root_code_group.ResolveMatchingCodeGroups (evidence);
			// TODO
			return ((cg != null) ? cg : null);

                }

                public SecurityElement ToXml ()
                {
			Hashtable fullNames = new Hashtable ();
			// only StrongNameMembershipCondition so no need to loop
			if (full_trust_assemblies.Count > 0) {
				if (!fullNames.Contains ("StrongNameMembershipCondition")) {
					fullNames.Add ("StrongNameMembershipCondition", typeof (StrongNameMembershipCondition).FullName);
				}
			}
                        
                        SecurityElement namedPSs = new SecurityElement ("NamedPermissionSets");
			foreach (NamedPermissionSet nps in named_permission_sets) {
				SecurityElement se = nps.ToXml ();
				object objectClass = se.Attributes ["class"];
				if (!fullNames.Contains (objectClass)) {
					fullNames.Add (objectClass, nps.GetType ().FullName);
				}
				namedPSs.AddChild (se);
			}

                        SecurityElement fta = new SecurityElement ("FullTrustAssemblies");
			foreach (StrongNameMembershipCondition snmc in full_trust_assemblies) {
				fta.AddChild (snmc.ToXml (this));
			}

			SecurityElement security_classes = new SecurityElement ("SecurityClasses");
			if (fullNames.Count > 0) {
				foreach (DictionaryEntry de in fullNames) {
					SecurityElement sc = new SecurityElement ("SecurityClass");
					sc.AddAttribute ("Name", (string)de.Key);
					sc.AddAttribute ("Description", (string)de.Value);
					security_classes.AddChild (sc);
				}
			}

			SecurityElement element = new SecurityElement (typeof (System.Security.Policy.PolicyLevel).Name);
			element.AddAttribute ("version", "1");
			element.AddChild (security_classes);
			element.AddChild (namedPSs);
			if (root_code_group != null) {
				element.AddChild (root_code_group.ToXml (this));
			}
			element.AddChild (fta);

                        return element;
                }

		// internal stuff

		internal bool IsReserved (string name) 
		{
			switch (name) {
				case "FullTrust":
				case "LocalIntranet":
				case "Internet":
				case "SkipVerification":
				case "Execution":
				case "Nothing":
				case "Everything":
					// FIXME: Are there others ?
					return true;
				default:
					return false;
			}
		}

		// NOTE: Callers are expected to check for ControlPolicy
		internal void Save ()
		{
			if (_type == PolicyLevelType.AppDomain) {
				throw new PolicyException (Locale.GetText (
					"Can't save AppDomain PolicyLevel"));
			}

			if (_location != null) {
				using (StreamWriter sw = new StreamWriter (_location)) {
					sw.Write (ToXml ().ToString ());
					sw.Close ();
				}
			}
		}

		// TODO : hardcode defaults in case 
		// (a) the specified policy file doesn't exists; and
		// (b) no corresponding default policy file exists
		internal void CreateFromHardcodedDefault (PolicyLevelType type) 
		{
			PolicyStatement psu = new PolicyStatement (new PermissionSet (PermissionState.Unrestricted));

			switch (type) {
			case PolicyLevelType.Machine:
				// by default all stuff is in the machine policy...
				root_code_group = new UnionCodeGroup (new ZoneMembershipCondition (SecurityZone.MyComputer), psu);
				break;
			case PolicyLevelType.User:
			case PolicyLevelType.Enterprise:
			case PolicyLevelType.AppDomain:
				// while the other policies don't restrict anything
				root_code_group = new UnionCodeGroup (new AllMembershipCondition (), psu); 
				break;
			}
		}
        }
}
