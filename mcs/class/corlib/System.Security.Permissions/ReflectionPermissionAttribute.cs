//
// System.Security.Permissions.ReflectionPermissionAttribute.cs
//
// Authors
//	Duncan Mak <duncan@ximian.com>
//	Sebastien Pouliot <spouliot@motus.com>
//
// (C) 2002 Ximian, Inc. http://www.ximian.com
// Portions Copyright (C) 2003 Motus Technologies (http://www.motus.com)
//

using System;
using System.Security.Permissions;

namespace System.Security.Permissions {

	[AttributeUsage (AttributeTargets.Assembly | AttributeTargets.Class |
			 AttributeTargets.Struct | AttributeTargets.Constructor |
			 AttributeTargets.Method)]
	[Serializable]
	public sealed class ReflectionPermissionAttribute : CodeAccessSecurityAttribute {

		// Fields
		private ReflectionPermissionFlag flags;
		private bool memberAccess;
		private bool reflectionEmit;
		private bool typeInfo;
		
		//Constructor
		public ReflectionPermissionAttribute (SecurityAction action) : base (action) {}
		
		// Properties
		public ReflectionPermissionFlag Flags
		{
			get { return flags; }
			set { 
				flags = value; 
				memberAccess = ((flags & ReflectionPermissionFlag.MemberAccess) == ReflectionPermissionFlag.MemberAccess);
				reflectionEmit = ((flags & ReflectionPermissionFlag.ReflectionEmit) == ReflectionPermissionFlag.ReflectionEmit);
				typeInfo = ((flags & ReflectionPermissionFlag.TypeInformation) == ReflectionPermissionFlag.TypeInformation);
			}
		}
		
		public bool MemberAccess
		{
			get { return memberAccess; }
			set { 
				if (value)
					flags |= ReflectionPermissionFlag.MemberAccess;
				else
					flags -= ReflectionPermissionFlag.MemberAccess;
				memberAccess = value; 
			}
		}
		
		public bool ReflectionEmit
		{
			get { return reflectionEmit; }
			set { 
				if (value)
					flags |= ReflectionPermissionFlag.ReflectionEmit;
				else
					flags -= ReflectionPermissionFlag.ReflectionEmit;
				reflectionEmit = value; 
			}
		}  

		public bool TypeInformation
		{
			get { return typeInfo; }
			set { 
				if (value)
					flags |= ReflectionPermissionFlag.TypeInformation;
				else
					flags -= ReflectionPermissionFlag.TypeInformation;
				typeInfo = value; 
			}
		}
		
		// Methods
		public override IPermission CreatePermission ()
		{
			return new ReflectionPermission (flags);
		}
	}
}
