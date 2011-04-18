// CS0647: Error during emitting `System.Security.Permissions.SecurityPermissionAttribute' attribute. The reason is `SecurityAction `Demand' is not valid for this declaration'
// Line : 10

using System;
using System.Security;
using System.Security.Permissions;

[assembly: SecurityPermission (SecurityAction.Demand, SkipVerification=true)]

class Test
{
	static void Main () {}
}
