// CS7050: SecurityAction value `System.Security.Permissions.SecurityAction' is invalid for security attributes applied to an assembly
// Line: 10

using System;
using System.Security;
using System.Security.Permissions;

[assembly: SecurityPermission (SecurityAction.Demand, SkipVerification=true)]

class Test
{
	static void Main () {}
}
