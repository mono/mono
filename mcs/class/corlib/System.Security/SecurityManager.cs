//
// System.Security.SecurityManager.cs
//
// Author:
//   Nick Drochak(ndrochak@gol.com)
//
// (C) Nick Drochak
//

using System.Security.Policy;
using System.Collections;

namespace System.Security {

	public sealed class SecurityManager  {
		private static bool checkExecutionRights;
		private static bool securityEnabled;

		public static bool CheckExecutionRights {
			get{
				return checkExecutionRights;
			}
			set{
				checkExecutionRights = value;
			}
		}

		public static bool SecurityEnabled {
			get{
				return securityEnabled;
			}
			set{
				securityEnabled = value;
			}
		}

		public static bool IsGranted(IPermission perm){
			return false;
		}

		public static PolicyLevel LoadPolicyLevelFromFile(
			string path, 
			PolicyLevelType type)
		{
			return null;
		}

		public static PolicyLevel LoadPolicyLevelFromString(
			string str, 
			PolicyLevelType type)
		{
			if (null == str){    
				throw new ArgumentNullException("str");
			}
			return null;
		}

		public static IEnumerator PolicyHierarchy(){
			return null;
		}

		public static PermissionSet ResolvePolicy(Evidence evidence){
			return null;
		}

		public static PermissionSet ResolvePolicy(
			Evidence evidence,
			PermissionSet reqdPset,
			PermissionSet optPset,
			PermissionSet denyPset,
			out PermissionSet denied)
		{
			denied = null;
			return null;
		}

		public static IEnumerator ResolvePolicyGroups(Evidence evidence){
			return null;
		}

		public static void SavePolicy(){}

		public static void SavePolicyLevel(PolicyLevel level){}

	}
}