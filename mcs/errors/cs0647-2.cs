// cs0647.cs : Error emitting 'System.Security.Permissions.SecurityPermissionAttribute' attribute -- 'SecurityAction RequestMinimum is not valid on this declaration
// Line : 10

using System;
using System.Security;
using System.Security.Permissions;

[assembly: SecurityPermission (SecurityAction.Demand, SkipVerification=true)]

class Test
{
	static void Main () {}
}
