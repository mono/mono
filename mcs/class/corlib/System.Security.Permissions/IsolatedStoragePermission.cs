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

		// Constructor
		public IsolatedStoragePermission (SecurityAction action) : base (action) {}

		// Properties
		[MonoTODO]
		public IsolatedStorageContainment UsageAllowed
		{
			get { return 0; }
			set {}
		}

		[MonoTODO]
		public long UserQuota
		{
			get { return 0; }
			set {}
		}
	}
}
			 

			 
			 
				    
