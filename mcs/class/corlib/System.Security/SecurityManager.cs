//
// System.Security.SecurityManager.cs
//
// Authors:
//	Nick Drochak(ndrochak@gol.com)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) Nick Drochak
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

#if !MOBILE

using System.Collections;
using System.Diagnostics;
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

	// Must match MonoDeclSecurityActions in /mono/metadata/reflection.h
	internal struct RuntimeDeclSecurityActions {
		public RuntimeDeclSecurityEntry cas;
		public RuntimeDeclSecurityEntry noncas;
		public RuntimeDeclSecurityEntry choice;
	}

	[ComVisible (true)]
	public static class SecurityManager {
		private static object _lockObject;
		private static ArrayList _hierarchy;
		private static IPermission _unmanagedCode;
		private static Hashtable _declsecCache;
		private static PolicyLevel _level;

		static SecurityManager () 
		{
			// lock(this) is bad
			// http://msdn.microsoft.com/library/en-us/dnaskdr/html/askgui06032003.asp?frame=true
			_lockObject = new object ();
		}

		// properties

		[Obsolete]
		public static bool CheckExecutionRights {
			get {
				return false;
			}
			set {
			}
		}

		[Obsolete ("The security manager cannot be turned off on MS runtime")]
		extern public static bool SecurityEnabled {
			[MethodImplAttribute (MethodImplOptions.InternalCall)]
			get;

			[MethodImplAttribute (MethodImplOptions.InternalCall)]
			[SecurityPermission (SecurityAction.Demand, ControlPolicy = true)]
			set;
		}

		internal static bool CheckElevatedPermissions ()
		{
			return true; // always true outside Moonlight
		}

		[Conditional ("ENABLE_SANDBOX")] //??
		internal static void EnsureElevatedPermissions ()
		{
			// do nothing outside of Moonlight
		}

		// methods

		// NOTE: This method doesn't show in the class library status page because
		// it cannot be "found" with the StrongNameIdentityPermission for ECMA key.
		// But it's there!
		// FIXME works for fulltrust (empty), documentation doesn't really make sense, type wise
		[MonoTODO ("CAS support is experimental (and unsupported). This method only works in FullTrust.")]
		[StrongNameIdentityPermission (SecurityAction.LinkDemand, PublicKey = "0x00000000000000000400000000000000")]
		public static void GetZoneAndOrigin (out ArrayList zone, out ArrayList origin) 
		{
			zone = new ArrayList ();
			origin = new ArrayList ();
		}

		[Obsolete]
		public static bool IsGranted (IPermission perm)
		{
			if (perm == null)
				return true;
			if (!SecurityEnabled)
				return true;

			// - Policy driven
			// - Only check the caller (no stack walk required)
			// - Not affected by overrides (like Assert, Deny and PermitOnly)
			// - calls IsSubsetOf even for non CAS permissions
			//   (i.e. it does call Demand so any code there won't be executed)
			// with 2.0 identity permission are unrestrictable
			return IsGranted (Assembly.GetCallingAssembly (), perm);
		}

		// note: in 2.0 *all* permissions (including identity permissions) support unrestricted
		internal static bool IsGranted (Assembly a, IPermission perm)
		{
			PermissionSet granted = a.GrantedPermissionSet;
			if ((granted != null) && !granted.IsUnrestricted ()) {
				CodeAccessPermission grant = (CodeAccessPermission) granted.GetPermission (perm.GetType ());
				if (!perm.IsSubsetOf (grant)) {
					return false;
				}
			}

			PermissionSet denied = a.DeniedPermissionSet;
			if ((denied != null) && !denied.IsEmpty ()) {
				if (denied.IsUnrestricted ())
					return false;
				CodeAccessPermission refuse = (CodeAccessPermission) a.DeniedPermissionSet.GetPermission (perm.GetType ());
				if ((refuse != null) && perm.IsSubsetOf (refuse))
					return false;
			}
			return true;
		}

		[Obsolete]
		[SecurityPermission (SecurityAction.Demand, ControlPolicy = true)]
		public static PolicyLevel LoadPolicyLevelFromFile (string path, PolicyLevelType type)
		{
			if (path == null)
				throw new ArgumentNullException ("path");

			PolicyLevel pl = null;
			try {
				pl = new PolicyLevel (type.ToString (), type);
				pl.LoadFromFile (path);
			}
			catch (Exception e) {
				throw new ArgumentException (Locale.GetText ("Invalid policy XML"), e);
			}
			return pl;
		}

		[Obsolete]
		[SecurityPermission (SecurityAction.Demand, ControlPolicy = true)]
		public static PolicyLevel LoadPolicyLevelFromString (string str, PolicyLevelType type)
		{
			if (null == str)
				throw new ArgumentNullException ("str");

			PolicyLevel pl = null;
			try {
				pl = new PolicyLevel (type.ToString (), type);
				pl.LoadFromString (str);
			}
			catch (Exception e) {
				throw new ArgumentException (Locale.GetText ("Invalid policy XML"), e);
			}
			return pl;
		}

		[Obsolete]
		[SecurityPermission (SecurityAction.Demand, ControlPolicy = true)]
		public static IEnumerator PolicyHierarchy ()
		{
			return Hierarchy;
		}

		[Obsolete]
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
				if (ResolvePolicyLevel (ref ps, pl, evidence)) {
					break;	// i.e. PolicyStatementAttribute.LevelFinal
				}
			}

			ResolveIdentityPermissions (ps, evidence);

			return ps;
		}

		[Obsolete]
		[MonoTODO ("(2.0) more tests are needed")]
		public static PermissionSet ResolvePolicy (Evidence[] evidences)
		{
			if ((evidences == null) || (evidences.Length == 0) ||
				((evidences.Length == 1) && (evidences [0].Count == 0))) {
				return new PermissionSet (PermissionState.None);
			}

			// probably not optimal
			PermissionSet ps = ResolvePolicy (evidences [0]);
			for (int i=1; i < evidences.Length; i++) {
				ps = ps.Intersect (ResolvePolicy (evidences [i]));
			}
			return ps;
		}

		[Obsolete]
		public static PermissionSet ResolveSystemPolicy (Evidence evidence)
		{
			// no evidence, no permission
			if (evidence == null)
				return new PermissionSet (PermissionState.None);

			// Note: can't call PolicyHierarchy since ControlPolicy isn't required to resolve policies
			PermissionSet ps = null;
			IEnumerator ple = Hierarchy;
			while (ple.MoveNext ()) {
				PolicyLevel pl = (PolicyLevel) ple.Current;
				if (pl.Type == PolicyLevelType.AppDomain)
					break;
				if (ResolvePolicyLevel (ref ps, pl, evidence))
					break;	// i.e. PolicyStatementAttribute.LevelFinal
			}

			ResolveIdentityPermissions (ps, evidence);
			return ps;
		}

		static private SecurityPermission _execution = new SecurityPermission (SecurityPermissionFlag.Execution);

		[Obsolete]
		public static PermissionSet ResolvePolicy (Evidence evidence, PermissionSet reqdPset, PermissionSet optPset, PermissionSet denyPset, out PermissionSet denied)
		{
			PermissionSet resolved = ResolvePolicy (evidence);
			// do we have the minimal permission requested by the assembly ?
			if ((reqdPset != null) && !reqdPset.IsSubsetOf (resolved)) {
				throw new PolicyException (Locale.GetText (
					"Policy doesn't grant the minimal permissions required to execute the assembly."));
			}

			// do we check for execution rights ?
			if (CheckExecutionRights) {
				bool execute = false;
				// an empty permissionset doesn't include Execution
				if (resolved != null) {
					// unless we have "Full Trust"...
					if (resolved.IsUnrestricted ()) {
						execute = true;
					} else {
						// ... we need to find a SecurityPermission
						IPermission security = resolved.GetPermission (typeof (SecurityPermission));
						execute = _execution.IsSubsetOf (security);
					}
				}

				if (!execute) {
					throw new PolicyException (Locale.GetText (
						"Policy doesn't grant the right to execute the assembly."));
				}
			}

			denied = denyPset;
			return resolved;
		}

		[Obsolete]
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

		[Obsolete]
		[SecurityPermission (SecurityAction.Demand, ControlPolicy = true)]
		public static void SavePolicy () 
		{
			IEnumerator e = Hierarchy;
			while (e.MoveNext ()) {
				PolicyLevel level = (e.Current as PolicyLevel);
				level.Save ();
			}
		}

		[Obsolete]
		[SecurityPermission (SecurityAction.Demand, ControlPolicy = true)]
		public static void SavePolicyLevel (PolicyLevel level) 
		{
			// Yes this will throw a NullReferenceException, just like MS (see FDBK13121)
			level.Save ();
		}

		// private/internal stuff

		private static IEnumerator Hierarchy {
			get {
				lock (_lockObject) {
					if (_hierarchy == null)
						InitializePolicyHierarchy ();
				}
				return _hierarchy.GetEnumerator ();
			}
		}

		private static void InitializePolicyHierarchy ()
		{
			string machinePolicyPath = Path.GetDirectoryName (Environment.GetMachineConfigPath ());
			// note: use InternalGetFolderPath to avoid recursive policy initialization
			string userPolicyPath = Path.Combine (Environment.UnixGetFolderPath (Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.Create), "mono");

			PolicyLevel enterprise = new PolicyLevel ("Enterprise", PolicyLevelType.Enterprise);
			_level = enterprise;
			enterprise.LoadFromFile (Path.Combine (machinePolicyPath, "enterprisesec.config"));

			PolicyLevel machine = new PolicyLevel ("Machine", PolicyLevelType.Machine);
			_level = machine;
			machine.LoadFromFile (Path.Combine (machinePolicyPath, "security.config"));

			PolicyLevel user = new PolicyLevel ("User", PolicyLevelType.User);
			_level = user;
			user.LoadFromFile (Path.Combine (userPolicyPath, "security.config"));

			ArrayList al = new ArrayList ();
			al.Add (enterprise);
			al.Add (machine);
			al.Add (user);

			_hierarchy = ArrayList.Synchronized (al);
			_level = null;
		}

		internal static bool ResolvePolicyLevel (ref PermissionSet ps, PolicyLevel pl, Evidence evidence)
		{
			PolicyStatement pst = pl.Resolve (evidence);
			if (pst != null) {
				if (ps == null) {
					// only for initial (first) policy level processed
					ps = pst.PermissionSet;
				} else {
					ps = ps.Intersect (pst.PermissionSet);
					if (ps == null) {
						// null is equals to None - exist that null can throw NullReferenceException ;-)
						ps = new PermissionSet (PermissionState.None);
					}
				}

				if ((pst.Attributes & PolicyStatementAttribute.LevelFinal) == PolicyStatementAttribute.LevelFinal)
					return true;
			}
			return false;
		}

		internal static void ResolveIdentityPermissions (PermissionSet ps, Evidence evidence)
		{
			// in 2.0 identity permissions can now be unrestricted
			if (ps.IsUnrestricted ())
				return;

			// Only host evidence are used for policy resolution
			IEnumerator ee = evidence.GetHostEnumerator ();
			while (ee.MoveNext ()) {
				IIdentityPermissionFactory ipf = (ee.Current as IIdentityPermissionFactory);
				if (ipf != null) {
					IPermission p = ipf.CreateIdentityPermission (evidence);
					ps.AddPermission (p);
				}
			}
		}

		internal static PolicyLevel ResolvingPolicyLevel {
			get { return _level; }
			set { _level = value; }
		}

		internal static PermissionSet Decode (IntPtr permissions, int length)
		{
			// Permission sets from the runtime (declarative security) can be cached
			// for performance as they can never change (i.e. they are read-only).
			PermissionSet ps = null;

			lock (_lockObject) {
				if (_declsecCache == null) {
					_declsecCache = new Hashtable ();
				}

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
			if ((encodedPermissions == null) || (encodedPermissions.Length < 1))
				throw new SecurityException ("Invalid metadata format.");

			switch (encodedPermissions [0]) {
			case 60:
				// Fx 1.0/1.1 declarative security permissions metadata is in Unicode-encoded XML
				string xml = Encoding.Unicode.GetString (encodedPermissions);
				return new PermissionSet (xml);
			case 0x2E:
				// Fx 2.0 are encoded "somewhat, but not enough, like" custom attributes
				// note: we still support the older format!
				return PermissionSet.CreateFromBinaryFormat (encodedPermissions);
			default:
				throw new SecurityException (Locale.GetText ("Unknown metadata format."));
			}
		}

		private static IPermission UnmanagedCode {
			get {
				lock (_lockObject) {
					if (_unmanagedCode == null)
						_unmanagedCode = new SecurityPermission (SecurityPermissionFlag.UnmanagedCode);
				}
				return _unmanagedCode;
			}
		}

		// called by the runtime when CoreCLR is enabled

		private static void ThrowException (Exception ex)
		{
			throw ex;
		}

#pragma warning restore 169

		public static PermissionSet GetStandardSandbox (Evidence evidence)
		{
			if (evidence == null)
				throw new ArgumentNullException ("evidence");

			throw new NotImplementedException ();
		}

		public static bool CurrentThreadRequiresSecurityContextCapture ()
		{
			throw new NotImplementedException ();
		}
	}
}

#endif

