//
// System.Security.Permissions.ReflectionPermissionAttribute.cs
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
	public sealed class ReflectionPermissionAttribute : CodeAccessSecurityAttribute
	{
		//Constructor
		public ReflectionPermissionAttribute (SecurityAction action) : base (action) {}
		
		// Properties
		[MonoTODO]
		public ReflectionPermissionFlag Flags
		{
			get { return 0; }
			set {}
		}
		
		[MonoTODO]
		public bool MemberAccess
		{
			get { return false; }
			set {}
		}
		
		[MonoTODO]				    
		public bool ReflectionEmit
		{
			get { return false; }
			set {}
		}  

		[MonoTODO]
		public bool TypeInformation
		{
			get { return false; }
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
