//
// caspol.cs: Code Access Security Policy Tool
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//

using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Permissions;
using System.Security.Policy;
using System.Text;

using Mono.Security.Cryptography;
#if !NET_2_0
using Mono.Xml;
#endif

[assembly: AssemblyTitle ("Mono CasPol")]
[assembly: AssemblyDescription ("Command line tool to modify Code Access Security policies.")]

namespace Mono.Tools {

	class CustomMembershipCondition : IMembershipCondition {

		SecurityElement _se;

		public CustomMembershipCondition (SecurityElement se)
		{
			_se = se;
		}

		public bool Check (Evidence evidence)
		{
			return true;
		}

		public IMembershipCondition Copy ()
		{
			return new CustomMembershipCondition (_se);
		}

		public void FromXml (SecurityElement e)
		{
			_se = e;
		}

		public SecurityElement ToXml ()
		{
			return _se;
		}

		public void FromXml (SecurityElement e, PolicyLevel level)
		{
			_se = e;
		}

		public SecurityElement ToXml (PolicyLevel level)
		{
			return _se;
		}
	}

	class CasPol {

		static ArrayList _levels;

		static private void Help () 
		{
			Console.WriteLine ("Usage: caspol [options] [arguments] ...{0}", Environment.NewLine);
		}

		// (to be) Stored Options
		static bool PolicyChangesConfirmation = true;

		static bool forcePolicyChanges = false;
		static bool policyLevelDefault = true;

		static void PrintGlobalInfo ()
		{
			Console.WriteLine ("Security: {0}", SecurityManager.SecurityEnabled);
			Console.WriteLine ("Execution check: {0}", SecurityManager.CheckExecutionRights);
			Console.WriteLine ("Policy changes confirmation: {0}", PolicyChangesConfirmation);
		}

		static bool Confirm ()
		{
			if (PolicyChangesConfirmation) {
				Console.WriteLine ("WARNING: This action will modify the specified security policy!");
				Console.WriteLine ("Do you want to change the policy ?");
				string answer = Console.ReadLine ();
				switch (answer.ToUpper ()) {
				case "YES":
				case "Y":
					return true;
				default:
					Console.WriteLine ("Change aborted!");
					return false;
				}
			}
			return true;
		}

		static string Policies (string prefix)
		{
			StringBuilder sb = new StringBuilder (prefix);
			PolicyLevel pl = null;
			for (int i = 0; i < Levels.Count - 1; i++) {
				pl = (PolicyLevel)Levels [i];
				sb.AppendFormat ("{0}, ", pl.Label);
			}
			pl = (PolicyLevel)Levels [Levels.Count - 1];
			sb.Append (pl.Label);

			sb.Append (" policy level");
			if (Levels.Count > 1)
				sb.Append ("s");

			return sb.ToString ();
		}

		// In Fx 1.0/1.1 there is not direct way to load a XML file 
		// into a SecurityElement so we use SecurityParser from 
		// Mono.Security.dll.
		static SecurityElement LoadXml (string filename)
		{
			if (!File.Exists (filename)) {
				Console.WriteLine ("Couldn't not find '{0}'.", filename);
				return null;
			}

			string xml = null;
			using (StreamReader sr = new StreamReader (filename)) {
				xml = sr.ReadToEnd ();
				sr.Close ();
			}
#if NET_2_0
			// actually this use the SecurityParser (on the Mono 
			// runtime) in corlib do to the job - but it remove 
			// the dependency on Mono.Security.dll
			SecurityElement se = SecurityElement.FromString (xml);
#else
			SecurityParser sp = new SecurityParser ();
			sp.LoadXml (xml);
			SecurityElement se = sp.ToXml ();
#endif
			return se;
		}

		static PermissionSet LoadPermissions (string filename)
		{
			SecurityElement se = LoadXml (filename);
			if (se == null)
				return null;

			PermissionSet ps = new PermissionSet (PermissionState.None);
			ps.FromXml (se);
			if (se.Attribute ("class").IndexOf ("System.Security.NamedPermissionSet") == -1)
				return ps;
			// now we know it's a NamedPermissionSet
			return (PermissionSet) new NamedPermissionSet (se.Attribute ("Name"), ps);
		}

		static StrongName GetStrongName (string filename)
		{
			try {
				AssemblyName an = AssemblyName.GetAssemblyName (filename);
				byte [] pk = an.GetPublicKey ();
				return new StrongName (new StrongNamePublicKeyBlob (pk), an.Name, an.Version);
			}
			catch (FileNotFoundException) {
				Console.WriteLine ("Couldn't find assembly '{0}'.", filename);
				return null;
			}
		}

		static Assembly GetAssembly (string filename)
		{
			try {
				AssemblyName an = AssemblyName.GetAssemblyName (filename);
				return Assembly.Load (an);
			}
			catch (FileNotFoundException) {
				Console.WriteLine ("Couldn't find assembly '{0}'.", filename);
				return null;
			}
		}

		static Evidence GetAssemblyEvidences (string filename)
		{
			return GetAssembly (filename).Evidence;
		}

		static bool OnOff (string value, ref bool on)
		{
			switch (value.ToUpper ()) {
			case "ON":
				on = true;
				break;
			case "OFF":
				on = false;
				break;
			default:
				return false;
			}
			return true;
		}

		static bool SaveSettings ()
		{
			Console.WriteLine ("TODO - where to save those settings ?");
			return false;
		}


		// Actions

		static void ShowCodeGroup (CodeGroup cg, string prefix) 
		{
			Console.WriteLine ("{0}. {1}: {2}", prefix, cg.MembershipCondition, cg.PermissionSetName);
			for (int i=0; i < cg.Children.Count; i++) {
				ShowCodeGroup ((CodeGroup)cg.Children [i], "  " + prefix + "." + (i + 1));
			}
		}

		// -lg
		// -listgroups
		static void ListCodeGroups ()
		{
			PrintGlobalInfo ();

			foreach (PolicyLevel pl in Levels) {
				Console.WriteLine ("{0}Level: {1}{0}", Environment.NewLine, pl.Label);

				Console.WriteLine ("Code Groups:{0}", Environment.NewLine);
				ShowCodeGroup (pl.RootCodeGroup, "1");
			}
		}

		static void ShowDescription (CodeGroup cg, string prefix)
		{
			Console.WriteLine ("{0}. {1}: {2}", prefix, cg.Name, cg.Description);
			for (int i = 0; i < cg.Children.Count; i++) {
				ShowDescription ((CodeGroup)cg.Children [i], "  " + prefix + "." + (i + 1));
			}
		}

		// -ld
		// -listdescription
		static void ListDescriptions ()
		{
			PrintGlobalInfo ();

			foreach (PolicyLevel pl in Levels) {
				Console.WriteLine ("{0}Level: {1}{0}", Environment.NewLine, pl.Label);

				Console.WriteLine ("Code Groups:{0}", Environment.NewLine);
				ShowDescription (pl.RootCodeGroup, "1");
			}
		}

		// -lp
		// -listpset
		static void ListPermissionSets ()
		{
			PrintGlobalInfo ();

			foreach (PolicyLevel pl in Levels) {
				Console.WriteLine ("{0}Level: {1}{0}", Environment.NewLine, pl.Label);

				Console.WriteLine ("Named Permission Sets:{0}", Environment.NewLine);
				int n=1;
				foreach (NamedPermissionSet nps in pl.NamedPermissionSets) {
					Console.WriteLine ("{0}. {1} ({2}) = {3}{4}", 
						n++, nps.Name, nps.Description, Environment.NewLine, nps);
				}
			}
		}

		// -lf
		// -listfulltrust
		static void ListFullTrust ()
		{
			PrintGlobalInfo ();

			foreach (PolicyLevel pl in Levels) {
				Console.WriteLine ("{0}Level: {1}{0}", Environment.NewLine, pl.Label);

				Console.WriteLine ("Full Trust Assemblies:{0}", Environment.NewLine);
				int n = 1;
				foreach (StrongNameMembershipCondition snmc in pl.FullTrustAssemblies) {
					Console.WriteLine ("{0}. {1} = {2}{3}",
						n++, snmc.Name, Environment.NewLine, snmc);
				}
			}
		}

		static void ShowResolveGroup (PolicyLevel pl, Evidence e)
		{
			Console.WriteLine ("{0}Level: {1}{0}", Environment.NewLine, pl.Label);
			CodeGroup cg = pl.ResolveMatchingCodeGroups (e);
			Console.WriteLine ("Code Groups:{0}", Environment.NewLine);
			ShowCodeGroup (cg, "1");
			Console.WriteLine ();
		}

		// -rsg assemblyname
		// -resolvegroup assemblyname
		static bool ResolveGroup (string assemblyname)
		{
			Evidence ev = GetAssemblyEvidences (assemblyname);
			if (ev == null)
				return false;

			if (policyLevelDefault) {
				// different "default" here
				IEnumerator e = SecurityManager.PolicyHierarchy ();
				while (e.MoveNext ()) {
					PolicyLevel pl = (PolicyLevel)e.Current;
					ShowResolveGroup (pl, ev);
				}
			} else {
				// use the user specified levels
				foreach (PolicyLevel pl in Levels) {
					ShowResolveGroup (pl, ev);
				}
			}
			return true;
		}

		// -rsp assemblyname
		// -resolveperm assemblyname
		static bool ResolvePermissions (string assemblyname)
		{
			Evidence ev = GetAssemblyEvidences (assemblyname);
			if (ev == null)
				return false;

			PermissionSet ps = null;
			Console.WriteLine ();
			if (policyLevelDefault)	{
				// different "default" here
				IEnumerator e = SecurityManager.PolicyHierarchy ();
				while (e.MoveNext ()) {
					PolicyLevel pl = (PolicyLevel)e.Current;
					Console.WriteLine ("Resolving {0} level", pl.Label);
					if (ps == null)
						ps = pl.Resolve (ev).PermissionSet;
					else
						ps = ps.Intersect (pl.Resolve (ev).PermissionSet);
				}
			} else {
				// use the user specified levels
				foreach (PolicyLevel pl in Levels) {
					Console.WriteLine ("Resolving {0} level", pl.Label);
					if (ps == null)
						ps = pl.Resolve (ev).PermissionSet;
					else
						ps = ps.Intersect (pl.Resolve (ev).PermissionSet);
				}
			}
			if (ps == null)
				return false;

			IEnumerator ee = ev.GetHostEnumerator ();
			while (ee.MoveNext ()) {
				IIdentityPermissionFactory ipf = (ee.Current as IIdentityPermissionFactory);
				if (ipf != null) {
					IPermission p = ipf.CreateIdentityPermission (ev);
					ps.AddPermission (p);
				}
			}

			Console.WriteLine ("{0}Grant:{0}{1}", Environment.NewLine, ps.ToXml ().ToString ());
			return true;
		}

		// -ap namedxmlfile
		// -addpset namedxmlfile
		// -ap xmlfile name
		// -addpset xmlfile name
		static bool AddPermissionSet (string [] args, ref int i)
		{
			// two syntax - so we first load the XML file and
			// if it's not a named XML file, then we use the next 
			// parameter as it's name
			string xmlfile = args [++i];
			PermissionSet ps = LoadPermissions (xmlfile);
			if ((ps == null) || !Confirm ())
				return false;

			NamedPermissionSet nps = null;
			if (ps is NamedPermissionSet) {
				nps = (NamedPermissionSet)ps;
			} else {
				nps = new NamedPermissionSet (args [++i], ps);
			}

			foreach (PolicyLevel pl in Levels) {
				pl.AddNamedPermissionSet (nps);
				SecurityManager.SavePolicyLevel (pl);
			}
			return true;
		}

		// -cp xmlfile psetname
		// -chgpset xmlfile psetname
		static bool ChangePermissionSet (string[] args, ref int i)
		{
			string xmlfile = args [++i];
			PermissionSet ps = LoadPermissions (xmlfile);
			if (ps == null)
				return false;

			bool confirmed = false;
			string psname = args [++i];

			foreach (PolicyLevel pl in Levels) {
				if (pl.GetNamedPermissionSet (psname) == null) {
					Console.WriteLine ("Couldn't find '{0}' permission set in policy.", psname);
					return false;
				} else if (confirmed || Confirm ()) {
					confirmed = true; // only ask once
					pl.ChangeNamedPermissionSet (psname, ps);
					SecurityManager.SavePolicyLevel (pl);
				} else
					return false;
			}
			return true;
		}

		// -rp psetname
		// -rempset psetname
		static bool RemovePermissionSet (string psname)
		{
			bool confirmed = false;

			foreach (PolicyLevel pl in Levels) {
				PermissionSet ps = pl.GetNamedPermissionSet (psname);
				if (ps == null) {
					Console.WriteLine ("Couldn't find '{0}' permission set in policy.", psname);
					return false;
				} else if (confirmed || Confirm ()) {
					confirmed = true; // only ask once
					pl.RemoveNamedPermissionSet (psname);
					SecurityManager.SavePolicyLevel (pl);
					Console.WriteLine ("Permission set '{0}' removed from policy.", psname);
				} else
					return false;
			}
			return true;
		}

		// -af assemblyname
		// -addfulltrust assemblyname
		static bool AddFullTrust (string aname)
		{
			StrongName sn = GetStrongName (aname);
			if ((sn == null) || !Confirm ())
				return false;

			foreach (PolicyLevel pl in Levels) {
				pl.AddFullTrustAssembly (sn);
			}
			return true;
		}

		// -rf assemblyname
		// -remfulltrust assemblyname
		static bool RemoveFullTrust (string aname)
		{
			StrongName sn = GetStrongName (aname);
			if ((sn == null) || !Confirm ())
				return false;

			foreach (PolicyLevel pl in Levels) {
				pl.RemoveFullTrustAssembly (sn);
			}
			return true;
		}


		static CodeGroup FindCodeGroupByName (string name, ref CodeGroup parent)
		{
			for (int i = 0; i < parent.Children.Count; i++)	{
				CodeGroup child = (CodeGroup)parent.Children [i];
				if (child.Name == name) {
					return child;
				} else {
					CodeGroup cg = FindCodeGroupByName (name, ref child);
					if (cg != null)
						return cg;
				}
			}
			return null;
		}

		static CodeGroup FindCodeGroupByLabel (string label, string current, ref CodeGroup parent)
		{
			for (int i=0; i < parent.Children.Count; i++) {
				CodeGroup child = (CodeGroup)parent.Children [i];
				string temp = String.Concat (current, ".", (i + 1).ToString ());
				if ((label == temp) || (label == temp + ".")) {
					return child;
				} else if (label.StartsWith (temp)) {
					CodeGroup cg = FindCodeGroupByLabel (label, temp, ref child);
					if (cg != null)
						return cg;
				}
			}
			return null;
		}

		static CodeGroup FindCodeGroup (string name, ref CodeGroup parent, ref PolicyLevel pl)
		{
			if (name.Length < 1)
				return null;
			
			// Notes:
			// - labels starts with numbers (e.g. 1.2.1)
			// - names cannot start with numbers (A-Z, 0-9 and _)
			bool label = Char.IsDigit (name, 0);

			// More notes
			// - we can't remove the root code group
			// - we remove only one group (e.g. name)
			for (int i=0; i < Levels.Count; i++) {
				pl = (PolicyLevel) Levels [i];
				parent = pl.RootCodeGroup;
				CodeGroup cg = null;
				if (label)
					cg = FindCodeGroupByLabel (name, "1", ref parent);
				else
					cg = FindCodeGroupByName (name, ref parent);
				
				if (cg != null)
					return cg;
			}
			Console.WriteLine ("CodeGroup with {0} '{1}' was not found!",
				label ? "label" : "name", name);
			return null;
		}

		// -custom xmlfile
		static IMembershipCondition ProcessCustomMembership (string filename)
		{
			SecurityElement se = LoadXml (filename);
			if (se == null)
				return null;
			return new CustomMembershipCondition (se);
		}

		// -hash algo -hex hash
		// -hash algo -file assemblyname
		static IMembershipCondition ProcessHashMembership (string[] args, ref int i)
		{
			HashAlgorithm ha = HashAlgorithm.Create (args [++i]);
			byte [] value = null;
			switch (args [++i]) {
				case "-hex":
					value = CryptoConvert.FromHex (args [++i]);
					break;
				case "-file":
					Hash hash = new Hash (GetAssembly (args [++i]));
					value = hash.GenerateHash (ha);
					break;
				default:
					return null;
			}
			return new HashMembershipCondition (ha, value);
		}

		// -pub -cert certificate
		// -pub -file signedfile
		// -pub -hex rawdata
		static IMembershipCondition ProcessPublisherMembership (string[] args, ref int i)
		{
			X509Certificate cert = null;
			switch (args [++i]) {
				case "-cert":
					cert = X509Certificate.CreateFromCertFile (args [++i]);
					break;
				case "-file":
					cert = X509Certificate.CreateFromSignedFile (args [++i]);
					break;
				case "-hex":
					byte[] raw = CryptoConvert.FromHex (args [++i]);
					cert = new X509Certificate (raw);
					break;
				default:
					return null;
			}
			return new PublisherMembershipCondition (cert);
		}

		// -strong -file filename [name | -noname] [version | -noversion]
		static IMembershipCondition ProcessStrongNameMembership (string[] args, ref int i)
		{
			if (args [++i] != "-file") {
				Console.WriteLine ("Missing -file parameter.");
				return null;
			}
			
			StrongName sn = GetStrongName (args [++i]);

			string name = args [++i];
			if (name == "-noname")
				name = null;

			Version v = null;
			string version = args [++i];
			if (version != "-noversion")
				v = new Version (version);

			return new StrongNameMembershipCondition (sn.PublicKey, name, v);
		}

		static bool ProcessCodeGroup (CodeGroup cg, string[] args, ref int i)
		{
			IMembershipCondition mship = null;
			for (; i < args.Length; i++) {
				switch (args [++i]) {
				case "-all":
					cg.MembershipCondition = new AllMembershipCondition ();
					break;
				case "-appdir":
					cg.MembershipCondition = new ApplicationDirectoryMembershipCondition ();
					break;
				case "-custom":
					mship = ProcessCustomMembership (args [++i]);
					if (mship == null)
						return false;
					cg.MembershipCondition = mship;
					break;
				case "-hash":
					mship = ProcessHashMembership (args, ref i);
					if (mship == null)
						return false;
					cg.MembershipCondition = mship;
					break;
				case "-pub":
					mship = ProcessPublisherMembership (args, ref i);
					if (mship == null)
						return false;
					cg.MembershipCondition = mship;
					break;
				case "-site":
					cg.MembershipCondition = new SiteMembershipCondition (args [++i]);
					break;
				case "-strong":
					mship = ProcessStrongNameMembership (args, ref i);
					if (mship == null)
						return false;
					cg.MembershipCondition = mship;
					break;
				case "-url":
					cg.MembershipCondition = new UrlMembershipCondition (args [++i]);
					break;
				case "-zone":
					SecurityZone zone = (SecurityZone) Enum.Parse (typeof (SecurityZone), args [++i]);
					cg.MembershipCondition = new ZoneMembershipCondition (zone);
					break;

				case "-d":
				case "-description":
					cg.Description = args [++i];
					break;
				case "-exclusive":
					bool exclusive = false;
					if (OnOff (args [++i], ref exclusive)) {
						if (exclusive)
							cg.PolicyStatement.Attributes |= PolicyStatementAttribute.Exclusive;
					}
					else
						return false;
					break;
				case "-levelfinal":
					bool final = false;
					if (OnOff (args [++i], ref final)) {
						if (final)
							cg.PolicyStatement.Attributes |= PolicyStatementAttribute.LevelFinal;
					}
					else
						return false;
					break;
				case "-n":
				case "-name":
					cg.Name = args [++i];
					break;
				default:
					i--;
					break;
				}
			}
			return true;
		}

		// -ag label|name membership psetname flag
		// -addgroup label|name membership psetname flag
		static bool AddCodeGroup (string[] args, ref int i)
		{
			string name = args [++i];

			PolicyLevel pl = null;
			CodeGroup parent = null;
			CodeGroup cg = FindCodeGroup (name, ref parent, ref pl);
			if ((pl == null) || (parent == null) || (cg == null))
				return false;

			UnionCodeGroup child = new UnionCodeGroup (
				new AllMembershipCondition (), 
				new PolicyStatement (new PermissionSet (PermissionState.Unrestricted)));
			if (!ProcessCodeGroup (child, args, ref i))
				return false;

			cg.AddChild (child);
			SecurityManager.SavePolicyLevel (pl);
			Console.WriteLine ("CodeGroup '{0}' added in {1} policy level.",
				cg.Name, pl.Label);
			return true;
		}

		// -cg label|name membership|psetname|flag
		// -chggroup label|name membership|psetname|flag
		static bool ChangeCodeGroup (string[] args, ref int i)
		{
			string name = args [++i];

			PolicyLevel pl = null;
			CodeGroup parent = null;
			CodeGroup cg = FindCodeGroup (name, ref parent, ref pl);
			if ((pl == null) || (parent == null) || (cg == null))
				return false;

			if (!ProcessCodeGroup (cg, args, ref i))
				return false;

			SecurityManager.SavePolicyLevel (pl);
			Console.WriteLine ("CodeGroup '{0}' modified in {1} policy level.",
				cg.Name, pl.Label);
			return true;
		}

		// -rg label|name
		// -remgroup label|name
		static bool RemoveCodeGroup (string name)
		{
			PolicyLevel pl = null;
			CodeGroup parent = null;
			CodeGroup cg = FindCodeGroup (name, ref parent, ref pl);
			if ((pl == null) || (parent == null) || (cg == null))
				return false;

			if (!Confirm ())
				return false;

			parent.RemoveChild (cg);
			SecurityManager.SavePolicyLevel (pl);
			Console.WriteLine ("CodeGroup '{0}' removed from {1} policy level.",
				cg.Name, pl.Label);
			return true;
		}

		// -r
		// -recover
		static void Recover ()
		{
			// no confirmation required to recover
			foreach (PolicyLevel pl in Levels) {
				pl.Recover ();
				SecurityManager.SavePolicyLevel (pl);
			}
		}

		// -rs
		// -reset
		static bool Reset ()
		{
			Console.WriteLine (Policies ("Resetting "));
			if (Confirm ()) {
				foreach (PolicyLevel pl in Levels) {
					pl.Reset ();
					SecurityManager.SavePolicyLevel (pl);
				}
				return true;
			}
			return false;
		}

		// -s on|off
		// -security on|off
		static bool Security (string value)
		{
			bool on = true;
			if (!OnOff (value, ref on))
				return false;
			SecurityManager.SecurityEnabled = on;
			return SaveSettings ();
		}

		// -e on|off
		// -execution on|off
		static bool Execution (string value)
		{
			bool on = true;
			if (!OnOff (value, ref on))
				return false;
			SecurityManager.CheckExecutionRights = on;
			return SaveSettings ();
		}

		// -b
		// -buildcache
		static bool BuildCache ()
		{
			// TODO
			return false;
		}

		// -pp on|off
		// -polchgprompt on|off
		static bool PolicyChangePrompt (string value)
		{
			bool on = true;
			if (!OnOff (value, ref on))
				return false;
			PolicyChangesConfirmation = on;
			return SaveSettings ();
		}


		// Policy Levels Internal Management

		static PolicyLevel levelEnterprise;
		static PolicyLevel levelMachine;
		static PolicyLevel levelUser;

		static void BuildLevels ()
		{
			IEnumerator e = SecurityManager.PolicyHierarchy ();
			if (e.MoveNext ())
				levelEnterprise = (PolicyLevel) e.Current;
			if (e.MoveNext ())
				levelMachine = (PolicyLevel) e.Current;
			if (e.MoveNext ())
				levelUser = (PolicyLevel) e.Current;
		}

		static PolicyLevel Enterprise {
			get {
				if (levelEnterprise == null)
					BuildLevels ();
				return levelEnterprise;
			}
		}

		static PolicyLevel Machine {
			get {
				if (levelMachine == null)
					BuildLevels ();
				return levelMachine;
			}
		}

		static PolicyLevel User {
			get {
				if (levelUser == null)
					BuildLevels ();
				return levelUser;
			}
		}

		static ArrayList Levels {
			get {
				if (_levels == null)
					_levels = new ArrayList (3);
				return _levels;
			}
		}

		static bool ProcessInstruction (string[] args, ref int i)
		{
			for (; i < args.Length; i++) {
				switch (args [i]) {
				case "-q":
				case "-quiet":
					PolicyChangesConfirmation = false;
					break;
				case "-f":
				case "-force":
					forcePolicyChanges = true;
					break;
				case "-?":
				case "/?":
				case "-h":
				case "-help":
					Help ();
					break;

				case "-a":
				case "-all":
					policyLevelDefault = false;
					Levels.Clear ();
					Levels.Add (Enterprise);
					Levels.Add (Machine);
					Levels.Add (User);
					break;
				case "-ca":
				case "-customall":
					policyLevelDefault = false;
					Levels.Clear ();
					Levels.Add (Enterprise);
					Levels.Add (Machine);
					Levels.Add (SecurityManager.LoadPolicyLevelFromFile (args [++i], PolicyLevelType.User));
					break;
				case "-cu":
				case "-customuser":
					policyLevelDefault = false;
					Levels.Clear ();
					Levels.Add (SecurityManager.LoadPolicyLevelFromFile (args [++i], PolicyLevelType.User));
					break;
				case "-en":
				case "-entreprise":
					policyLevelDefault = false;
					Levels.Clear ();
					Levels.Add (Enterprise);
					break;
				case "-m":
				case "-machine":
					policyLevelDefault = false;
					Levels.Clear ();
					Levels.Add (Machine);
					break;
				case "-u":
				case "-user":
					policyLevelDefault = false;
					Levels.Clear ();
					Levels.Add (User);
					break;

				case "-lg":
				case "-listgroups":
					ListCodeGroups ();
					break;
				case "-ld":
				case "-listdescription":
					ListDescriptions ();
					break;
				case "-lp":
				case "-listpset":
					ListPermissionSets ();
					break;
				case "-lf":
				case "-listfulltrust":
					ListFullTrust ();
					break;
				case "-l":
				case "-list":
					ListCodeGroups ();
					Console.WriteLine ();
					ListPermissionSets ();
					Console.WriteLine ();
					ListFullTrust ();
					break;

				case "-rsg":
				case "-resolvegroup":
					if (!ResolveGroup (args [++i]))
						return false;
					break;
				case "-rsp":
				case "-resolveperm":
					if (!ResolvePermissions (args [++i]))
						return false;
					break;

				case "-ap":
				case "-addpset":
					if (!AddPermissionSet (args, ref i))
						return false;
					break;
				case "-cp":
				case "-chgpset":
					if (!ChangePermissionSet (args, ref i))
						return false;
					break;
				case "-rp":
				case "-rempset":
					if (!RemovePermissionSet (args [++i]))
						return false;
					break;

				case "-af":
				case "-addfulltrust":
					if (!AddFullTrust (args [++i]))
						return false;
					break;
				case "-rf":
				case "-remfulltrust":
					if (!RemoveFullTrust (args [++i]))
						return false;
					break;

				case "-ag":
				case "-addgroup":
					if (!AddCodeGroup (args, ref i))
						return false;
					break;
				case "-cg":
				case "-chggroup":
					if (!ChangeCodeGroup (args, ref i))
						return false;
					break;
				case "-rg":
				case "-remgroup":
					if (!RemoveCodeGroup (args [++i]))
						return false;
					break;

				case "-r":
				case "-recover":
					Recover ();
					break;
				case "-rs":
				case "-reset":
					if (!Reset ())
						return false;
					break;

				case "-s":
				case "-security":
					if (!Security (args [++i]))
						return false;
					break;
				case "-e":
				case "-execution":
					if (!Execution (args [++i]))
						return false;
					break;
				case "-b":
				case "-buildcache":
					if (!BuildCache ())
						return false;
					break;
				case "-pp":
				case "-polchgprompt":
					if (!PolicyChangePrompt (args [++i]))
						return false;
					break;

				default:
					Console.WriteLine ("*** unknown argument {0} ***", args [i]);
					return false;
				}
				Console.WriteLine ();
			}
			return true;
		}

		static void SetDefaultPolicyLevel ()
		{
			// default is User for normal users and Machine for 
			// administrators. Here we define an administrator as
			// someone who can write to the Machine policy files
			try {
				using (FileStream fs = File.OpenWrite (Machine.StoreLocation)) {
					fs.Close ();
				}
				Levels.Add (Machine);
			}
			catch {
				Levels.Add (User);
			}
			// some actions, like resolves, use a different default (all)
			policyLevelDefault = true;
		}

		[STAThread]
		static int Main (string[] args) 
		{
			Console.WriteLine (new AssemblyInfo ().ToString ());
			if (args.Length == 0) {
				Help ();
				return 0;
			}

			try {
				// set default level (when none is specified 
				// by command line options)
				SetDefaultPolicyLevel ();

				// process instructions (i.e. multiple 
				// instructions can be chained)
				for (int i=0; i < args.Length; i++) {
					if (!ProcessInstruction (args, ref i))
						return 1;
				}
			}
			catch (Exception e) {
				Console.WriteLine ("Error: " + e.ToString ());
				Help ();
				return 2;
			}
			Console.WriteLine ("Success");
			return 0;
		}
	}
}
