//
// System.Security.Permissions.IsolatedStoragePermission.cs
//
// Duncan Mak <duncan@ximian.com>
//
// (C) 2002 Ximian, Inc.			http://www.ximian.com
//

using System;
using System.Security.Permissions;

namespace System.Security.Permissions
{
	[AttributeUsage (AttributeTargets.Assembly | AttributeTargets.Class |
			 AttributeTargets.Struct | AttributeTargets.Constructor |
			 AttributeTargets.Method)]
        [Serializable]
	public abstract class IsolatedStoragePermission : CodeAccessSecurityAttribute
	{
		// Fields
		private IsolatedStorageContainment containment;
		private long quota;
		
		// Constructor
		public IsolatedStoragePermission (SecurityAction action)
			: base (action)
		{
		}

		// Properties
		public IsolatedStorageContainment UsageAllowed
		{
			get { return containment; }
			set { containment = value; }
		}

		public long UserQuota
		{
			get { return quota; }
			set { quota = value; }
		}
	}
}
			 

			 
			 
				    
