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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Security.Policy;
using System.Text;

using Mono.Xml;

namespace System.Security {

	public sealed class SecurityManager {

		private static bool checkExecutionRights;
		private static bool securityEnabled;
		private static object _lockObject;
		private static ArrayList _hierarchy;
		private static PermissionSet _fullTrust; // for [AllowPartiallyTrustedCallers]
		private static Hashtable _declsecCache;

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
			// - calls IsSubsetOf even for non CAS permissions
			//   (i.e. it does call Demand so any code there won't be executed)
			return IsGranted (Assembly.GetCallingAssembly (), perm);
		}

		internal static bool IsGranted (Assembly a, IPermission perm)
		{
			CodeAccessPermission grant = null;

			if (a.GrantedPermissionSet != null)
				grant = (CodeAccessPermission) a.GrantedPermissionSet.GetPermission (perm.GetType ());

			if ((grant == null) || !perm.IsSubsetOf (grant)) {
				if (a.DeniedPermissionSet != null) {
					CodeAccessPermission refuse = (CodeAccessPermission) a.DeniedPermissionSet.GetPermission (perm.GetType ());
					if ((refuse != null) && perm.IsSubsetOf (refuse))
						return false;
				}
				return true;
			}
			return false;
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
				// double-lock pattern
				if (_hierarchy == null) {
					lock (_lockObject) {
						if (_hierarchy == null)
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

		internal static PermissionSet Decode (IntPtr permissions, int length)
		{
			// Permission sets from the runtime (declarative security) can be cached
			// for performance as they can never change (i.e. they are read-only).

			if (_declsecCache == null) {
				lock (_lockObject) {
					if (_declsecCache == null) {
						_declsecCache = new Hashtable ();
					}
				}
			}

			PermissionSet ps = null;
			lock (_lockObject) {
				object key = (object) (int) permissions;
				ps = (PermissionSet) _declsecCache [key];
				if (ps == null) {
					// create permissionset and add it to the cache
					byte[] data = new byte [length];
					Marshal.Copy (permissions, data, 0, length);
					ps = Decode (data);
					ps.DeclarativeSecurity = true;
					_declsecCache.Add (key, ps);
				}
			}
			return ps;
		}

		internal static PermissionSet Decode (byte[] encodedPermissions)
		{
			switch (encodedPermissions [0]) {
			case 60:
				// Fx 1.0/1.1 declarative security permissions metadata is in Unicode-encoded XML
				string xml = Encoding.Unicode.GetString (encodedPermissions);
				return new PermissionSet (xml);
			case 0x2E:
				// TODO: Fx 2.0
				throw new SecurityException ("Unsupported 2.0 metadata format.");
			default:
				throw new SecurityException ("Unknown metadata format.");
			}
		}

		private static PermissionSet Union (byte[] classPermissions, byte[] methodPermissions)
		{
			if (classPermissions != null) {
				PermissionSet ps = Decode (classPermissions);
				if (methodPermissions != null) {
					ps = ps.Union (Decode (methodPermissions));
				}
				return ps;
			}

			return Decode (methodPermissions);
		}

		// internal - get called at JIT time

		unsafe private static void LinkDemand (
			IntPtr casClassPermission, int casClassLength,
			IntPtr nonCasClassPermission, int nonCasClassLength,
			IntPtr casMethodPermission, int casMethodLength,
			IntPtr nonCasMethodPermission, int nonCasMethodLength,
			bool requiresFullTrust)
		{
			PermissionSet ps = null;

			if (casClassLength > 0) {
				ps = Decode (casClassPermission, casClassLength);
				ps.ImmediateCallerDemand ();
			}
			if (nonCasClassLength > 0) {
				ps = Decode (nonCasClassPermission, nonCasClassLength);
				ps.ImmediateCallerNonCasDemand ();
			}

			if (casMethodLength > 0) {
				ps = Decode (casMethodPermission, casMethodLength);
				ps.ImmediateCallerDemand ();
			}
			if (nonCasMethodLength > 0) {
				ps = Decode (nonCasMethodPermission, nonCasMethodLength);
				ps.ImmediateCallerNonCasDemand ();
			}

			if (requiresFullTrust) {
				// double-lock pattern
				if (_fullTrust == null) {
					lock (_lockObject) {
						if (_fullTrust == null)
							_fullTrust = new NamedPermissionSet ("FullTrust");
					}
				}
				// FIXME: to be optimized with a flag
				_fullTrust.ImmediateCallerDemand ();
			}
		}

		// internal - get called by the class loader

		// Called when
		// - class inheritance
		// - method overrides
		private static void InheritanceDemand (byte[] permissions, byte[] nonCasPermissions)
		{
			if (permissions != null) {
				PermissionSet ps = Decode (permissions);
				if (ps != null)
					ps.ImmediateCallerDemand ();
			}
			if (nonCasPermissions != null) {
				PermissionSet ps = Decode (nonCasPermissions);
				if (ps != null)
					ps.ImmediateCallerNonCasDemand ();
			}
		}

		// internal - get called by JIT generated code

		unsafe private static void InternalDemand (IntPtr permissions, int length)
		{
			PermissionSet ps = Decode (permissions, length);
			ps.Demand ();
		}

		unsafe private static void InternalDemandChoice (IntPtr permissions, int length)
		{
#if NET_2_0
			PermissionSet ps = Decode (permissions, length);
			// TODO
#else
			throw new SecurityException ("SecurityAction.DemandChoice is only possible in 2.0");
#endif
		}
	}
}
