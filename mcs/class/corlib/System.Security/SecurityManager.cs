//
// System.Security.SecurityManager.cs
//
// Authors:
//	Nick Drochak(ndrochak@gol.com)
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) Nick Drochak
// Portions (C) 2004 Motus Technologies Inc. (http://www.motus.com)
//

using System.Collections;
using System.IO;
using System.Security.Permissions;
using System.Security.Policy;

namespace System.Security {

	public sealed class SecurityManager {

		private static bool checkExecutionRights;
		private static bool securityEnabled;
		static private object _lockObject;
		private static ArrayList _hierarchy;

		static SecurityManager () 
		{
			// lock(this) is bad
			// http://msdn.microsoft.com/library/en-us/dnaskdr/html/askgui06032003.asp?frame=true
			_lockObject = new object ();
		}

		private SecurityManager () {}

		// properties

		public static bool CheckExecutionRights {
			get { return checkExecutionRights; }
			set { 
				// throw a SecurityException if we don't have ControlPolicy permission
				new SecurityPermission (SecurityPermissionFlag.ControlPolicy).Demand ();
				checkExecutionRights = value; 
			}
		}

		public static bool SecurityEnabled {
			get { return securityEnabled; }
			set { 
				// throw a SecurityException if we don't have ControlPolicy permission
				new SecurityPermission (SecurityPermissionFlag.ControlPolicy).Demand ();
				securityEnabled = value; 
			}
		}

		// methods

		[MonoTODO("Incomplete")]
		public static bool IsGranted (IPermission perm)
		{
			if (perm == null)
				return true;
			if (!securityEnabled)
				return true;
			return false;
		}

		[MonoTODO()]
		public static PolicyLevel LoadPolicyLevelFromFile (string path, PolicyLevelType type)
		{
			if (path == null)
				throw new ArgumentNullException ("path");
			if (!File.Exists (path))
				throw new ArgumentException ("file do not exist");
			// throw a SecurityException if we don't have ControlPolicy permission
			new SecurityPermission (SecurityPermissionFlag.ControlPolicy).Demand ();
			// throw a SecurityException if we don't have Read, Write and PathDiscovery permissions
			FileIOPermissionAccess access = FileIOPermissionAccess.Read | FileIOPermissionAccess.Write | FileIOPermissionAccess.PathDiscovery;
			new FileIOPermission (access, path).Demand ();

			// TODO
			return null;
		}

		[MonoTODO()]
		public static PolicyLevel LoadPolicyLevelFromString (string str, PolicyLevelType type)
		{
			if (null == str)
				throw new ArgumentNullException("str");
			// throw a SecurityException if we don't have ControlPolicy permission
			new SecurityPermission (SecurityPermissionFlag.ControlPolicy).Demand ();

			// TODO
			return null;
		}

		[MonoTODO("InitializePolicyHierarchy not implemented")]
		public static IEnumerator PolicyHierarchy ()
		{
			// throw a SecurityException if we don't have ControlPolicy permission
			new SecurityPermission (SecurityPermissionFlag.ControlPolicy).Demand ();
			if (_hierarchy == null) {
				lock (_lockObject) {
					InitializePolicyHierarchy ();
				}
			}
			return _hierarchy.GetEnumerator ();
		}

		[MonoTODO()]
		public static PermissionSet ResolvePolicy (Evidence evidence)
		{
			return null;
		}

		[MonoTODO()]
		public static PermissionSet ResolvePolicy (Evidence evidence, PermissionSet reqdPset, PermissionSet optPset, PermissionSet denyPset, out PermissionSet denied)
		{
			denied = null;
			return null;
		}

		[MonoTODO()]
		public static IEnumerator ResolvePolicyGroups (Evidence evidence)
		{
			return null;
		}

		[MonoTODO ("InternalSavePolicyLevel isn't complete")]
		public static void SavePolicy () 
		{
			// throw a SecurityException if we don't have ControlPolicy permission
			// done by using PolicyHierarchy (no need to duplicate)
			IEnumerator e = PolicyHierarchy ();
			while (e.MoveNext ()) {
				PolicyLevel level = (e.Current as PolicyLevel);
				InternalSavePolicyLevel (level);
			}
		}

		[MonoTODO ("InternalSavePolicyLevel isn't complete")]
		public static void SavePolicyLevel (PolicyLevel level) 
		{
			// throw a SecurityException if we don't have ControlPolicy permission
			new SecurityPermission (SecurityPermissionFlag.ControlPolicy).Demand ();

			InternalSavePolicyLevel (level);
		}

		// internal stuff

		internal static void InternalSavePolicyLevel (PolicyLevel level) 
		{
			// without the security checks (to avoid checks in loops)
		}

		[MonoTODO ("Incomplete")]
		internal static void InitializePolicyHierarchy ()
		{
			ArrayList al = new ArrayList ();
			// minimum: Machine, Enterprise and User
			// FIXME: Incomplete
			al.Add (new PolicyLevel ("Enterprise"));
			al.Add (new PolicyLevel ("Machine"));
			al.Add (new PolicyLevel ("User"));
			_hierarchy = ArrayList.Synchronized (al);
		}
	}
}
