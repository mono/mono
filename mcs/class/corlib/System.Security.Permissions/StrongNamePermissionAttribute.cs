//
// System.Security.Permissions.StrongNameIdentityPermissionAttribute.cs
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
	public sealed class StrongNameIdentityPermissionAttribute : CodeAccessSecurityAttribute
	{
		// Constructor
		public StrongNameIdentityPermissionAttribute (SecurityAction action) : base (action) {}
		
		// Properties
		[MonoTODO]
		public string Name
		{
			get { return null; }
			set {}
		}

		[MonoTODO]
		public string PublicKey
		{
			get { return null; }
			set {}
		}

		[MonoTODO]
		public string Version
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
