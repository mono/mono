// CS0647: Error during emitting `System.Security.Permissions.SecurityPermissionAttribute' attribute. The reason is `SecurityAction `RequestMinimum' is not valid for this declaration'
// Line : 10

using System;
using System.Security;
using System.Security.Permissions;

public class Program {

	[SecurityPermission (SecurityAction.RequestMinimum, ControlPrincipal=true, Flags=SecurityPermissionFlag.ControlPrincipal)]
	static public void Main (string[] args)
	{
	}
}
