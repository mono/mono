//
// System.Net.DnsPermissionAttribute.cs
//
// Author:
//   Lawrence Pit (loz@cable.a2000.nl)
//

using System;
using System.Security;
using System.Security.Permissions;

namespace System.Net
{
	[AttributeUsage (AttributeTargets.Assembly 
	               | AttributeTargets.Class 
	               | AttributeTargets.Struct 
	               | AttributeTargets.Constructor 
	               | AttributeTargets.Method, AllowMultiple = true, Inherited = false)
	]	
	[Serializable]
	public sealed class DnsPermissionAttribute : CodeAccessSecurityAttribute
	{
		
		// Constructors
		public DnsPermissionAttribute (SecurityAction action) : base (action)
		{
		}

		// Methods
		
		public override IPermission CreatePermission () {
			return new DnsPermission (
				this.Unrestricted ?
				PermissionState.Unrestricted :
				PermissionState.None);
		}		
	}
}
