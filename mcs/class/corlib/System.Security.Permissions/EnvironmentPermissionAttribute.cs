//
// System.Security.Permissions.EnvironmentPermissionAttribute.cs
//
// Duncan Mak <duncan@ximian.com>
//
// (C) 2002 Ximian, Inc. http://www.ximian.com
//

using System;
using System.Security.Permissions;

namespace System.Security.Permissions
{
	[AttributeUsage (AttributeTargets.Assembly | AttributeTargets.Class |
			 AttributeTargets.Struct | AttributeTargets.Constructor |
			 AttributeTargets.Method)]
	[Serializable]
	public sealed class EnvironmentPermissionAttribute : CodeAccessSecurityAttribute
	{
		// Constructor
		public EnvironmentPermissionAttribute (SecurityAction action) : base (action) {}
		
		// Properties
		[MonoTODO]
		public string All
		{
				set {}
		}

		[MonoTODO]
		public string Read
		{
			get { return null; }
			set {}
		}

		[MonoTODO]
		public string Write
		{
			get { return null; }
			set {}
		}

		// Methods
		[MonoTODO]
		public override IPermission CreatePermission ()
		{
			return null;
		}
	}
}
