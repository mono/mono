//
// System.Security.Permissions.PrinciplePermissionAttribute.cs
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
	public sealed class PrinciplePermissionAttribute : CodeAccessSecurityAttribute
	{
		// Constructor
		public PrinciplePermissionAttribute (SecurityAction action) : base (action) {}

		// Properties
		[MonoTODO]
		public bool Authenticated
		{
			get { return false; }
			set {}
		}
			 			 
		[MonoTODO]
		public string Name
		{
			get { return null; }
			set {}
		}
			 
		[MonoTODO]
		public string Role
		{
			get { return null; }
			set {}
		}

		// Method
		[MonoTODO]
		public override IPermission CreatePermission ()
		{
			return null;
		}
	}
}
