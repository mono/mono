//
// System.Security.SecurityManager.cs
//
// Authors:
//	Nick Drochak(ndrochak@gol.com)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) Nick Drochak
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

using System.Collections;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Security.Permissions;
using System.Security.Policy;

using Mono.Xml;

namespace System.Security {

	// Note: Using [SecurityPermissionAttribute] would be cool but triggers an error
	// as you can't reference a custom security attribute from it's own assembly (CS0647)

	public sealed class SecurityManager {

		private static bool checkExecutionRights;
		private static bool securityEnabled;
		private static object _lockObject;
		private static ArrayList _hierarchy;

		static SecurityManager () 
		{
			// lock(this) is bad
			// http://msdn.microsoft.com/library/en-us/dnaskdr/html/askgui06032003.asp?frame=true
			_lockObject = new object ();
			securityEnabled = true;
//			checkExecutionRights = true;
		}

		private SecurityManager ()
		{
		}

		// properties

		public static bool CheckExecutionRights {
			get { return checkExecutionRights; }

			[SecurityPermission (SecurityAction.Demand, Flags=SecurityPermissionFlag.ControlPolicy)]
			set {
				// throw a SecurityException if we don't have ControlPolicy permission
				checkExecutionRights = value; 
			}
		}

		public static bool SecurityEnabled {
			get { return securityEnabled; }

			[SecurityPermission (SecurityAction.Demand, Flags=SecurityPermissionFlag.ControlPolicy)]
			set {
				// throw a SecurityException if we don't have ControlPolicy permission
				securityEnabled = value; 
			}
		}

		// methods

#if NET_2_0
		[MonoTODO]
		[StrongNameIdentityPermission (SecurityAction.LinkDemand, PublicKey = "0x00000000000000000400000000000000")]
		public static void GetZoneAndOrigin (out ArrayList zone, out ArrayList origin) 
		{
			zone = null;
			origin = null;
		}
#endif

		public static bool IsGranted (IPermission perm)
		{
			if (perm == null)
				return true;
			if (!securityEnabled)
				return true;

			// - Policy driven
			// - Only check the caller (no stack walk required)
			// - Not affected by overrides (like Assert, Deny and PermitOnly)
			return Assembly.GetCallingAssembly ().Demand (perm);
		}

		[SecurityPermission (SecurityAction.Demand, Flags=SecurityPermissionFlag.ControlPolicy)]
		public static PolicyLevel LoadPolicyLevelFromFile (string path, PolicyLevelType type)
		{
			// throw a SecurityException if we don't have ControlPolicy permission
			if (path == null)
				throw new ArgumentNullException ("path");

			PolicyLevel pl = null;
			try {
				pl = new PolicyLevel (type.ToString (), PolicyLevelType.AppDomain);
				pl.LoadFromFile (path);
			}
			catch (Exception e) {
				throw new ArgumentException (Locale.GetText ("Invalid policy XML"), e);
			}
			return pl;
		}

		[SecurityPermission (SecurityAction.Demand, Flags=SecurityPermissionFlag.ControlPolicy)]
		public static PolicyLevel LoadPolicyLevelFromString (string str, PolicyLevelType type)
		{
			// throw a SecurityException if we don't have ControlPolicy permission
			if (null == str)
				throw new ArgumentNullException ("str");

			PolicyLevel pl = null;
			try {
				pl = new PolicyLevel (type.ToString (), PolicyLevelType.AppDomain);
				pl.LoadFromString (str);
			}
			catch (Exception e) {
				throw new ArgumentException (Locale.GetText ("Invalid policy XML"), e);
			}
			return pl;
		}

		[SecurityPermission (SecurityAction.Demand, Flags=SecurityPermissionFlag.ControlPolicy)]
		public static IEnumerator PolicyHierarchy ()
		{
			// throw a SecurityException if we don't have ControlPolicy permission
			return Hierarchy;
		}

		public static PermissionSet ResolvePolicy (Evidence evidence)
		{
			// no evidence, no permission
			if (evidence == null)
				return new PermissionSet (PermissionState.None);

			PermissionSet ps = null;
			// Note: can't call PolicyHierarchy since ControlPolicy isn't required to resolve policies
			IEnumerator ple = Hierarchy;
			while (ple.MoveNext ()) {
				PolicyLevel pl = (PolicyLevel) ple.Current;
				PolicyStatement pst = pl.Resolve (evidence);
				if (pst != null) {
					if (ps == null)
						ps = pst.PermissionSet;	// for first time only
					else
						ps = ps.Intersect (pst.PermissionSet);

					// some permissions returns null, other returns an empty set
					// sadly we must adjust for every variations :(
					if (ps == null)
						ps = new PermissionSet (PermissionState.None);

					if ((pst.Attributes & PolicyStatementAttribute.LevelFinal) == PolicyStatementAttribute.LevelFinal)
						break;
				}
			}

			// Only host evidence are used for policy resolution
			IEnumerator ee = evidence.GetHostEnumerator ();
			while (ee.MoveNext ()) {
				IIdentityPermissionFactory ipf = (ee.Current as IIdentityPermissionFactory);
				if (ipf != null) {
					IPermission p = ipf.CreateIdentityPermission (evidence);
					ps.AddPermission (p);
				}
			}

			return ps;
		}

#if NET_2_0
		public static PermissionSet ResolvePolicy (Evidence[] evidences)
		{
			if (evidences == null)
				return new PermissionSet (PermissionState.None);

			// probably not optimal
			PermissionSet ps = null;
			foreach (Evidence evidence in evidences) {
				if (ps == null)
					ps = ResolvePolicy (evidence);
				else
					ps = ps.Intersect (ResolvePolicy (evidence));
			}
			return ps;
		}
#endif

		static private SecurityPermission _execution = new SecurityPermission (SecurityPermissionFlag.Execution);

		[MonoTODO()]
		public static PermissionSet ResolvePolicy (Evidence evidence, PermissionSet reqdPset, PermissionSet optPset, PermissionSet denyPset, out PermissionSet denied)
		{
			PermissionSet resolved = ResolvePolicy (evidence);
			// do we have the minimal permission requested by the assembly ?
			if ((reqdPset != null) && !reqdPset.IsSubsetOf (resolved)) {
				throw new PolicyException (Locale.GetText (
					"Policy doesn't grant the minimal permissions required to execute the assembly."));
			}
			// do we have the right to execute ?
			if (checkExecutionRights) {
				// unless we have "Full Trust"...
				if (!resolved.IsUnrestricted ()) {
					// ... we need to find a SecurityPermission
					IPermission security = resolved.GetPermission (typeof (SecurityPermission));
					if (!_execution.IsSubsetOf (security)) {
						throw new PolicyException (Locale.GetText (
							"Policy doesn't grant the right to execute to the assembly."));
					}
				}
			}

			denied = denyPset;
			return resolved;
		}

		public static IEnumerator ResolvePolicyGroups (Evidence evidence)
		{
			if (evidence == null)
				throw new ArgumentNullException ("evidence");

			ArrayList al = new ArrayList ();
			// Note: can't call PolicyHierarchy since ControlPolicy isn't required to resolve policies
			IEnumerator ple = Hierarchy;
			while (ple.MoveNext ()) {
				PolicyLevel pl = (PolicyLevel) ple.Current;
				CodeGroup cg = pl.ResolveMatchingCodeGroups (evidence);
				al.Add (cg);
			}
			return al.GetEnumerator ();
		}

		[SecurityPermission (SecurityAction.Demand, Flags=SecurityPermissionFlag.ControlPolicy)]
		public static void SavePolicy () 
		{
			// throw a SecurityException if we don't have ControlPolicy permission
			IEnumerator e = Hierarchy;
			while (e.MoveNext ()) {
				PolicyLevel level = (e.Current as PolicyLevel);
				level.Save ();
			}
		}

		[SecurityPermission (SecurityAction.Demand, Flags=SecurityPermissionFlag.ControlPolicy)]
		public static void SavePolicyLevel (PolicyLevel level) 
		{
			// Yes this will throw a NullReferenceException, just like MS (see FDBK13121)
			level.Save ();
		}

		// private/internal stuff

		private static IEnumerator Hierarchy {
			get {
				if (_hierarchy == null) {
					lock (_lockObject) {
						InitializePolicyHierarchy ();
					}
				}
				return _hierarchy.GetEnumerator ();
			}
		}

		private static void InitializePolicyHierarchy ()
		{
			string machinePolicyPath = Path.GetDirectoryName (Environment.GetMachineConfigPath ());
			string userPolicyPath = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData), "mono");

			ArrayList al = new ArrayList ();
			al.Add (new PolicyLevel ("Enterprise", PolicyLevelType.Enterprise,
				Path.Combine (machinePolicyPath, "enterprisesec.config")));

			al.Add (new PolicyLevel ("Machine", PolicyLevelType.Machine,
				Path.Combine (machinePolicyPath, "security.config")));

			al.Add (new PolicyLevel ("User", PolicyLevelType.User,
				Path.Combine (userPolicyPath, "security.config")));

			_hierarchy = ArrayList.Synchronized (al);
		}
	}
}
