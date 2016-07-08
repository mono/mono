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

using System.Collections; // for IList
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Permissions;

using Mono.Xml;

namespace System.Security.Policy {

	[Serializable]
	[ComVisible (true)]
	public sealed class PolicyLevel {

                string label;
                CodeGroup root_code_group;
		private ArrayList full_trust_assemblies;
		private ArrayList named_permission_sets;
		private string _location;
		private PolicyLevelType _type;
		private Hashtable fullNames;
		private SecurityElement xml;

		internal PolicyLevel (string label, PolicyLevelType type)
                {
                        this.label = label;
			_type = type;
                        full_trust_assemblies = new ArrayList ();
                        named_permission_sets = new ArrayList ();
                }

		internal void LoadFromFile (string filename)
		{
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
						xml = FromString (sr.ReadToEnd ());
					}
					try {
						SecurityManager.ResolvingPolicyLevel = this;
						FromXml (xml);
					}
					finally {
						SecurityManager.ResolvingPolicyLevel = this;
					}
				} else {
					CreateDefaultFullTrustAssemblies ();
					CreateDefaultNamedPermissionSets ();
					CreateDefaultLevel (_type);
					Save ();
				}
			}
			catch {
				// this can fail in many ways including...
				// * can't lookup policy (path discovery);
				// * can't copy default file to policy
				// * can't read policy file;
				// * can't decode policy file
				// * can't save hardcoded policy to filename
			}
			finally {
				_location = filename;
			}
		}

		internal void LoadFromString (string xml) 
		{
			FromXml (FromString (xml));
		}

		private SecurityElement FromString (string xml) 
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
			return policyLevel;
		}

		// properties

		[Obsolete ("All GACed assemblies are now fully trusted and all permissions now succeed on fully trusted code.")]
		public IList FullTrustAssemblies {
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

		[ComVisible (false)]
		public PolicyLevelType Type {
			get { return _type; }
		}

		// methods

		[Obsolete ("All GACed assemblies are now fully trusted and all permissions now succeed on fully trusted code.")]
                public void AddFullTrustAssembly (StrongName sn)
                {
			if (sn == null)
				throw new ArgumentNullException ("sn");

			StrongNameMembershipCondition snMC = new StrongNameMembershipCondition(
                                sn.PublicKey, sn.Name, sn.Version);

                        AddFullTrustAssembly (snMC);
                }

		[Obsolete ("All GACed assemblies are now fully trusted and all permissions now succeed on fully trusted code.")]
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
                        named_permission_sets.Add (permSet.Copy ());
                }

		public NamedPermissionSet ChangeNamedPermissionSet (string name, PermissionSet pSet)
		{
			if (name == null)
				throw new ArgumentNullException ("name");
			if (pSet == null)
				throw new ArgumentNullException ("pSet");
			if (DefaultPolicies.ReservedNames.IsReserved (name))
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
			UnionCodeGroup cg = new UnionCodeGroup (new AllMembershipCondition (), new PolicyStatement (DefaultPolicies.FullTrust));
			cg.Name = "All_Code";
			PolicyLevel pl = new PolicyLevel ("AppDomain", PolicyLevelType.AppDomain);
			pl.RootCodeGroup = cg;
			pl.Reset ();
                        return pl;
                }


		public void FromXml (SecurityElement e)
		{
			if (e == null)
				throw new ArgumentNullException ("e");
// MS doesn't throw an exception for this case
//			if (e.Tag != "PolicyLevel")
//				throw new ArgumentException (Locale.GetText ("Invalid XML"));

			SecurityElement sc = e.SearchForChildByTag ("SecurityClasses");
			if ((sc != null) && (sc.Children != null) && (sc.Children.Count > 0)) {
				fullNames = new Hashtable (sc.Children.Count);
				foreach (SecurityElement se in sc.Children) {
					fullNames.Add (se.Attributes ["Name"], se.Attributes ["Description"]);
				}
			}

			SecurityElement fta = e.SearchForChildByTag ("FullTrustAssemblies");
			if ((fta != null) && (fta.Children != null) && (fta.Children.Count > 0)) {
				full_trust_assemblies.Clear ();
				foreach (SecurityElement se in fta.Children) {
					if (se.Tag != "IMembershipCondition")
						throw new ArgumentException (Locale.GetText ("Invalid XML"));
					string className = se.Attribute ("class");
					if (className.IndexOf ("StrongNameMembershipCondition") < 0)
						throw new ArgumentException (Locale.GetText ("Invalid XML - must be StrongNameMembershipCondition"));
					// we directly use StrongNameMembershipCondition
					full_trust_assemblies.Add (new StrongNameMembershipCondition (se));
				}
			}

			SecurityElement cg = e.SearchForChildByTag ("CodeGroup");
			if ((cg != null) && (cg.Children != null) && (cg.Children.Count > 0)) {
				root_code_group = CodeGroup.CreateFromXml (cg, this);
			} else {
				throw new ArgumentException (Locale.GetText ("Missing Root CodeGroup"));
			}

			SecurityElement nps = e.SearchForChildByTag ("NamedPermissionSets");
			if ((nps != null) && (nps.Children != null) && (nps.Children.Count > 0)) {
				named_permission_sets.Clear ();
				foreach (SecurityElement se in nps.Children) {
					NamedPermissionSet n = new NamedPermissionSet ();
					n.Resolver = this;
					n.FromXml (se);
					named_permission_sets.Add (n);
				}
			}
		}

                public NamedPermissionSet GetNamedPermissionSet (string name)
                {
                        if (name == null)
                                throw new ArgumentNullException ("name");

                        foreach (NamedPermissionSet n in named_permission_sets) {
                                if (n.Name == name)
                                        return (NamedPermissionSet) n.Copy ();
			}
                        return null;
                }

                public void Recover ()
                {
			if (_location == null) {
				string msg = Locale.GetText ("Only file based policies may be recovered.");
				throw new PolicyException (msg);
			}

			string backup = _location + ".backup";
			if (!File.Exists (backup)) {
				string msg = Locale.GetText ("No policy backup exists.");
				throw new PolicyException (msg);
			}

			try {
				File.Copy (backup, _location, true);
			}
			catch (Exception e) {
				string msg = Locale.GetText ("Couldn't replace the policy file with it's backup.");
				throw new PolicyException (msg, e);
			}
                }

		[Obsolete ("All GACed assemblies are now fully trusted and all permissions now succeed on fully trusted code.")]
                public void RemoveFullTrustAssembly (StrongName sn)
                {
			if (sn == null)
				throw new ArgumentNullException ("sn");

			StrongNameMembershipCondition s = new StrongNameMembershipCondition (sn.PublicKey, sn.Name, sn.Version);
                        RemoveFullTrustAssembly (s);
                }

		[Obsolete ("All GACed assemblies are now fully trusted and all permissions now succeed on fully trusted code.")]
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
                                throw new ArgumentNullException ("permSet");

			return RemoveNamedPermissionSet (permSet.Name);
                }

		public NamedPermissionSet RemoveNamedPermissionSet (string name)
		{
			if (name == null)
				throw new ArgumentNullException ("name");
			if (DefaultPolicies.ReservedNames.IsReserved (name))
				throw new ArgumentException (Locale.GetText ("Reserved name"));

			foreach (NamedPermissionSet nps in named_permission_sets) {
				if (name == nps.Name) {
					named_permission_sets.Remove (nps);
					return nps;
				}
			}
			string msg = String.Format (Locale.GetText ("Name '{0}' cannot be found."), name);
			throw new ArgumentException (msg, "name");
                }

                public void Reset ()
                {
			if (fullNames != null)
				fullNames.Clear ();

			if (_type != PolicyLevelType.AppDomain) {
	                        full_trust_assemblies.Clear ();
	                        named_permission_sets.Clear ();

				// because the policy doesn't exist LoadFromFile will try to
				// 1. use the .default file if existing (like Fx 2.0 does); or
				// 2. use the hard-coded default values
				// and recreate a policy file
				if ((_location != null) && (File.Exists (_location))) {
					try {
						File.Delete (_location);
					}
					catch {}
				}
				LoadFromFile (_location);
			} else {
				CreateDefaultFullTrustAssemblies ();
				CreateDefaultNamedPermissionSets ();
			}
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

		// NOTE: Callers are expected to check for ControlPolicy
		internal void Save ()
		{
			if (_type == PolicyLevelType.AppDomain) {
				throw new PolicyException (Locale.GetText (
					"Can't save AppDomain PolicyLevel"));
			}

			if (_location != null) {
				try {
					if (File.Exists (_location)) {
						File.Copy (_location, _location + ".backup", true);
					}
				}
				catch (Exception) {
				}
				finally {
					using (StreamWriter sw = new StreamWriter (_location)) {
						sw.Write (ToXml ().ToString ());
						sw.Close ();
					}
				}
			}
		}

		// Hardcode defaults in case 
		// (a) the specified policy file doesn't exists; and
		// (b) no corresponding default policy file exists
		internal void CreateDefaultLevel (PolicyLevelType type) 
		{
			PolicyStatement psu = new PolicyStatement (DefaultPolicies.FullTrust);

			switch (type) {
			case PolicyLevelType.Machine:
				// by default all stuff is in the machine policy...
				PolicyStatement psn = new PolicyStatement (DefaultPolicies.Nothing);
				root_code_group = new UnionCodeGroup (new AllMembershipCondition (), psn);
				root_code_group.Name = "All_Code";

				UnionCodeGroup myComputerZone = new UnionCodeGroup (new ZoneMembershipCondition (SecurityZone.MyComputer), psu);
				myComputerZone.Name = "My_Computer_Zone";
				// TODO: strongname code group for ECMA and MS keys
				root_code_group.AddChild (myComputerZone);

				UnionCodeGroup localIntranetZone = new UnionCodeGroup (new ZoneMembershipCondition (SecurityZone.Intranet), 
					new PolicyStatement (DefaultPolicies.LocalIntranet));
				localIntranetZone.Name = "LocalIntranet_Zone";
				// TODO: same site / same directory
				root_code_group.AddChild (localIntranetZone);

				PolicyStatement psi = new PolicyStatement (DefaultPolicies.Internet);
				UnionCodeGroup internetZone = new UnionCodeGroup (new ZoneMembershipCondition (SecurityZone.Internet), psi);
				internetZone.Name = "Internet_Zone";
				// TODO: same site
				root_code_group.AddChild (internetZone);

				UnionCodeGroup restrictedZone = new UnionCodeGroup (new ZoneMembershipCondition (SecurityZone.Untrusted), psn);
				restrictedZone.Name = "Restricted_Zone";
				root_code_group.AddChild (restrictedZone);

				UnionCodeGroup trustedZone = new UnionCodeGroup (new ZoneMembershipCondition (SecurityZone.Trusted), psi);
				trustedZone.Name = "Trusted_Zone";
				// TODO: same site
				root_code_group.AddChild (trustedZone);
				break;
			case PolicyLevelType.User:
			case PolicyLevelType.Enterprise:
			case PolicyLevelType.AppDomain:
				// while the other policies don't restrict anything
				root_code_group = new UnionCodeGroup (new AllMembershipCondition (), psu); 
				root_code_group.Name = "All_Code";
				break;
			}
		}

		internal void CreateDefaultFullTrustAssemblies () 
		{
			// (default) assemblies that are fully trusted during policy resolution
			full_trust_assemblies.Clear ();
			full_trust_assemblies.Add (DefaultPolicies.FullTrustMembership ("mscorlib", DefaultPolicies.Key.Ecma));
			full_trust_assemblies.Add (DefaultPolicies.FullTrustMembership ("System", DefaultPolicies.Key.Ecma));
			full_trust_assemblies.Add (DefaultPolicies.FullTrustMembership ("System.Data", DefaultPolicies.Key.Ecma));
			full_trust_assemblies.Add (DefaultPolicies.FullTrustMembership ("System.DirectoryServices", DefaultPolicies.Key.MsFinal));
			full_trust_assemblies.Add (DefaultPolicies.FullTrustMembership ("System.Drawing", DefaultPolicies.Key.MsFinal));
			full_trust_assemblies.Add (DefaultPolicies.FullTrustMembership ("System.Messaging", DefaultPolicies.Key.MsFinal));
			full_trust_assemblies.Add (DefaultPolicies.FullTrustMembership ("System.ServiceProcess", DefaultPolicies.Key.MsFinal));
		}

		internal void CreateDefaultNamedPermissionSets () 
		{
			named_permission_sets.Clear ();
			try {
				SecurityManager.ResolvingPolicyLevel = this;
				named_permission_sets.Add (DefaultPolicies.LocalIntranet);
				named_permission_sets.Add (DefaultPolicies.Internet);
				named_permission_sets.Add (DefaultPolicies.SkipVerification);
				named_permission_sets.Add (DefaultPolicies.Execution);
				named_permission_sets.Add (DefaultPolicies.Nothing);
				named_permission_sets.Add (DefaultPolicies.Everything);
				named_permission_sets.Add (DefaultPolicies.FullTrust);
			}
			finally {
				SecurityManager.ResolvingPolicyLevel = null;
			}
		}

		internal string ResolveClassName (string className)
		{
			if (fullNames != null) {
				object name = fullNames [className];
				if (name != null)
					return (string) name;
			}
			return className;
		}

		internal bool IsFullTrustAssembly (Assembly a)
		{
			AssemblyName an = a.GetName ();
			StrongNamePublicKeyBlob snpkb = new StrongNamePublicKeyBlob (an.GetPublicKey ());
			StrongNameMembershipCondition snMC = new StrongNameMembershipCondition (snpkb, an.Name, an.Version);
			foreach (StrongNameMembershipCondition sn in full_trust_assemblies) {
				if (sn.Equals (snMC)) {
					return true;
				}
			}
			return false;
		}
        }
}
