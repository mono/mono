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
		// Fields
		private ReflectionPermissionFlag flags;
		privat bool memberAccess;
		private bool reflectionEmit;
		
		
		//Constructor
		public ReflectionPermissionAttribute (SecurityAction action) : base (action) {}
		
		// Properties
		public ReflectionPermissionFlag Flags
		{
			get { return flags; }
			set { flags = value; }
		}
		
		public bool MemberAccess
		{
			get { return memberAccess; }
			set { memberAccess = value; }
		}
		
		public bool ReflectionEmit
		{
			get { return reflectionEmit; }
			set {  reflectionEmit = value; }
		}  

		public bool TypeInformation
		{
			get { return typeInfo; }
			set { typeInfo = value; }
		}
			 
		// Methods
		[MonoTODO]
		public override IPermission CreatePermission ()
		{
			return null;
		}
	}
}
