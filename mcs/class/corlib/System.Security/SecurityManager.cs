//
// System.Security.SecurityManager.cs
//
// Author:
//	Nick Drochak(ndrochak@gol.com)
//
// (C) Nick Drochak
//

using System.Security.Policy;
using System.Collections;

namespace System.Security {

	public sealed class SecurityManager {

		private static bool checkExecutionRights;
		private static bool securityEnabled;

		private SecurityManager () {}

		public static bool CheckExecutionRights {
			get { return checkExecutionRights; }
			set { checkExecutionRights = value; }
		}

		public static bool SecurityEnabled {
			get { return securityEnabled; }
			set { securityEnabled = value; }
		}

		[MonoTODO("Incomplete")]
		public static bool IsGranted (IPermission perm)
		{
			if (perm == null)
				return false;
			if (!securityEnabled)
				return true;
			return false;
		}

		[MonoTODO()]
		public static PolicyLevel LoadPolicyLevelFromFile (string path, PolicyLevelType type)
		{
			return null;
		}

		[MonoTODO()]
		public static PolicyLevel LoadPolicyLevelFromString (string str, PolicyLevelType type)
		{
			if (null == str)
				throw new ArgumentNullException("str");
			return null;
		}

		[MonoTODO()]
		public static IEnumerator PolicyHierarchy ()
		{
			return null;
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

		[MonoTODO()]
		public static void SavePolicy () {}

		[MonoTODO()]
		public static void SavePolicyLevel (PolicyLevel level) {}
	}
}
