//
// System.Security.Permissions.IsolatedStoragePermissionAttributes.cs
//
// Author:
//   Dan Lewis (dihlewis@yahoo.co.uk)
//
// (C) 2002
//

using System;
using System.Security.Permissions;

namespace System.Security.Permissions {
	
	[AttributeUsage ( AttributeTargets.Assembly | AttributeTargets.Class |
		AttributeTargets.Struct | AttributeTargets.Constructor |
		AttributeTargets.Method)]
	[Serializable]
	public abstract class IsolatedStoragePermissionAttribute : CodeAccessSecurityAttribute {
		
		public IsolatedStoragePermissionAttribute (SecurityAction action) : base (action) {
		}

		public IsolatedStorageContainment UsageAllowed {
			get { return usage_allowed; }
			set { usage_allowed = value; }
		}

		public long UserQuota {
			get { return user_quota; }
			set { user_quota = value; }
		}

		// private

		private IsolatedStorageContainment usage_allowed;
		private long user_quota;
	}
}
